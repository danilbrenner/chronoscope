using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using Chronoscope.Application.Abstractions.Data;
using Chronoscope.Application.Abstractions.SourceAuth;
using Chronoscope.Application.SourceAuth;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace Chronoscope.Infrastructure.Auth;

public sealed class MicrosoftGraphSourceAuthProvider(
    IHttpClientFactory httpClientFactory,
    IDataProtectionProvider dataProtectionProvider,
    ISourceAuthStateRepository sourceAuthStateRepository,
    IOptions<OneDriveAuthOptions> authOptions) : ISourceAuthProvider
{
    public const string HttpClientName = "MicrosoftGraph";
    private const string AuthStateProtectorName = "Chronoscope.Infrastructure.Auth.OneDriveAuthState.v1";
    private const string AuthRequestStateProtectorName = "Chronoscope.Infrastructure.Auth.OneDriveAuthRequestState.v1";
    private static readonly TimeSpan AuthRequestStateLifetime = TimeSpan.FromMinutes(10);
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public string CreateLinkUriAsync(string callbackUri, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;

        ArgumentException.ThrowIfNullOrWhiteSpace(callbackUri);
        var scope = string.Join(' ', authOptions.Value.Scopes);
        var state = CreateProtectedAuthRequestState(callbackUri);

        var authorizationEndpoint = BuildAuthorityPath("oauth2/v2.0/authorize");
        var parameters = new Dictionary<string, string?>
        {
            ["client_id"] = authOptions.Value.ClientId,
            ["response_type"] = "code",
            ["redirect_uri"] = callbackUri,
            ["response_mode"] = "query",
            ["scope"] = scope,
            ["state"] = state,
            ["prompt"] = "select_account"
        };

        return QueryHelpers.AddQueryString(authorizationEndpoint, parameters);
    }

    public async Task CompleteLinkAsync(
        string authorizationCode,
        string state,
        string callbackUri,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(authorizationCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(state);
        ArgumentException.ThrowIfNullOrWhiteSpace(callbackUri);
        ValidateAuthRequestState(state, callbackUri);

        var tokenResponse = await RequestTokenAsync(new Dictionary<string, string?>
        {
            ["grant_type"] = "authorization_code",
            ["code"] = authorizationCode,
            ["redirect_uri"] = callbackUri
        }, cancellationToken);

        var session = await GetSessionAsync(tokenResponse.AccessToken, cancellationToken);

        var persistedState = new PersistedSourceAuthState(
            AccessToken: tokenResponse.AccessToken,
            RefreshToken: tokenResponse.RefreshToken,
            AccessTokenExpiresAtUtc: DateTimeOffset.UtcNow.AddSeconds(tokenResponse.ExpiresIn),
            DisplayName: session.DisplayName,
            EmailAddress: session.EmailAddress,
            DriveId: session.DriveId);

        await sourceAuthStateRepository.SaveProtectedStateAsync(ProtectAuthState(persistedState), cancellationToken);
    }

    public async Task<SourceAuthSession> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        var persistedState = await GetPersistedStateAsync(cancellationToken);

        if (persistedState == null) throw new InvalidOperationException("No OneDrive Source is linked.");

        if (IsExpiring(persistedState.AccessTokenExpiresAtUtc))
        {
            persistedState = await RefreshAccessTokenAsync(persistedState, cancellationToken);
        }

        var session = await GetSessionAsync(persistedState.AccessToken, cancellationToken);

        if (string.Equals(session.DisplayName, persistedState.DisplayName, StringComparison.Ordinal)
            && string.Equals(session.EmailAddress, persistedState.EmailAddress, StringComparison.Ordinal)
            && string.Equals(session.DriveId, persistedState.DriveId, StringComparison.Ordinal))
            return session;

        var updatedState = persistedState with
        {
            DisplayName = session.DisplayName,
            EmailAddress = session.EmailAddress,
            DriveId = session.DriveId
        };

        await sourceAuthStateRepository.SaveProtectedStateAsync(ProtectAuthState(updatedState), cancellationToken);

        return session;
    }

    public async Task RefreshTokensIfNeededAsync(CancellationToken cancellationToken = default)
    {
        var persistedState = await GetPersistedStateAsync(cancellationToken);
        if (persistedState is null)
        {
            return;
        }
        
        await RefreshAccessTokenAsync(persistedState, cancellationToken);
    }

    public Task UnlinkCurrentAsync(CancellationToken cancellationToken = default)
        => sourceAuthStateRepository.ClearAsync(cancellationToken);


    private async Task<PersistedSourceAuthState?> GetPersistedStateAsync(CancellationToken cancellationToken)
    {
        var protectedState = await sourceAuthStateRepository.GetProtectedStateAsync(cancellationToken);
        return string.IsNullOrWhiteSpace(protectedState) ? null : UnprotectAuthState(protectedState);
    }

    private async Task<PersistedSourceAuthState> RefreshAccessTokenAsync(
        PersistedSourceAuthState persistedState,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(persistedState.RefreshToken))
        {
            throw new InvalidOperationException(
                "Cannot refresh OneDrive Source token because no refresh token is stored.");
        }

        var tokenResponse = await RequestTokenAsync(new Dictionary<string, string?>
        {
            ["grant_type"] = "refresh_token",
            ["refresh_token"] = persistedState.RefreshToken
        }, cancellationToken);

        var refreshedState = persistedState with
        {
            AccessToken = tokenResponse.AccessToken,
            RefreshToken = string.IsNullOrWhiteSpace(tokenResponse.RefreshToken)
                ? persistedState.RefreshToken
                : tokenResponse.RefreshToken,
            AccessTokenExpiresAtUtc = DateTimeOffset.UtcNow.AddSeconds(tokenResponse.ExpiresIn)
        };

        await sourceAuthStateRepository.SaveProtectedStateAsync(ProtectAuthState(refreshedState), cancellationToken);
        return refreshedState;
    }

    private async Task<SourceAuthSession> GetSessionAsync(string accessToken, CancellationToken cancellationToken)
    {
        using var httpClient = httpClientFactory.CreateClient(HttpClientName);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var userResponse = await httpClient.GetAsync("me", cancellationToken);
        userResponse.EnsureSuccessStatusCode();

        using var driveResponse = await httpClient.GetAsync("me/drive", cancellationToken);
        driveResponse.EnsureSuccessStatusCode();

        using var userDocument = await JsonDocument.ParseAsync(
            await userResponse.Content.ReadAsStreamAsync(cancellationToken),
            cancellationToken: cancellationToken);

        using var driveDocument = await JsonDocument.ParseAsync(
            await driveResponse.Content.ReadAsStreamAsync(cancellationToken),
            cancellationToken: cancellationToken);

        var userElement = userDocument.RootElement;
        var driveElement = driveDocument.RootElement;

        return new SourceAuthSession(
            DisplayName: GetRequiredString(userElement, "displayName"),
            EmailAddress: GetOptionalString(userElement, "mail") ?? GetOptionalString(userElement, "userPrincipalName"),
            DriveId: GetRequiredString(driveElement, "id"));
    }

    private async Task<TokenResponse> RequestTokenAsync(
        IReadOnlyDictionary<string, string?> grantParameters,
        CancellationToken cancellationToken)
    {
        using var httpClient = httpClientFactory.CreateClient(HttpClientName);

        var parameters = new Dictionary<string, string?>(grantParameters)
        {
            ["client_id"] = authOptions.Value.ClientId,
            ["client_secret"] = authOptions.Value.ClientSecret,
            ["scope"] = string.Join(' ', authOptions.Value.Scopes)
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, BuildAuthorityPath("oauth2/v2.0/token"))
        {
            Content = new FormUrlEncodedContent(parameters.Where(pair => !string.IsNullOrWhiteSpace(pair.Value))!)
        };

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"Microsoft token endpoint call failed with status {(int)response.StatusCode}: {errorBody}");
        }

        var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>(JsonOptions, cancellationToken);
        if (tokenResponse is null || string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
        {
            throw new InvalidOperationException("Microsoft token endpoint did not return a valid access token.");
        }

        return tokenResponse;
    }

    private string ProtectAuthState(PersistedSourceAuthState state)
    {
        var json = JsonSerializer.Serialize(state, JsonOptions);
        return dataProtectionProvider.CreateProtector(AuthStateProtectorName).Protect(json);
    }

    private string CreateProtectedAuthRequestState(string callbackUri)
    {
        var requestState = new AuthRequestState(
            CallbackUri: callbackUri,
            Nonce: Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)),
            IssuedAtUtc: DateTimeOffset.UtcNow);
        var json = JsonSerializer.Serialize(requestState, JsonOptions);
        return dataProtectionProvider.CreateProtector(AuthRequestStateProtectorName).Protect(json);
    }

    private void ValidateAuthRequestState(string protectedRequestState, string callbackUri)
    {
        AuthRequestState requestState;
        try
        {
            var json = dataProtectionProvider.CreateProtector(AuthRequestStateProtectorName).Unprotect(protectedRequestState);
            requestState = JsonSerializer.Deserialize<AuthRequestState>(json, JsonOptions)
                           ?? throw new InvalidOperationException("OneDrive callback state is invalid.");
        }
        catch (Exception exception) when (exception is CryptographicException || exception is JsonException ||
                                          exception is FormatException || exception is InvalidOperationException)
        {
            throw new InvalidOperationException("OneDrive callback state is invalid.", exception);
        }

        if (!string.Equals(requestState.CallbackUri, callbackUri, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("OneDrive callback state does not match callback URI.");
        }

        if (DateTimeOffset.UtcNow - requestState.IssuedAtUtc > AuthRequestStateLifetime)
        {
            throw new InvalidOperationException("OneDrive callback state has expired.");
        }
    }

    private PersistedSourceAuthState UnprotectAuthState(string protectedState)
    {
        try
        {
            var json = dataProtectionProvider.CreateProtector(AuthStateProtectorName).Unprotect(protectedState);
            var state = JsonSerializer.Deserialize<PersistedSourceAuthState>(json, JsonOptions);
            return state ?? throw new InvalidOperationException("Stored OneDrive Source auth state is invalid.");
        }
        catch (Exception exception) when (exception is JsonException || exception is FormatException ||
                                          exception is InvalidOperationException)
        {
            throw new InvalidOperationException("Stored OneDrive Source auth state is corrupted.", exception);
        }
    }

    private string BuildAuthorityPath(string endpoint)
    {
        var authority =
            $"{authOptions.Value.Instance.TrimEnd('/')}/{authOptions.Value.TenantId}/{endpoint.TrimStart('/')}";
        return authority;
    }

    private static bool IsExpiring(DateTimeOffset expiresAtUtc)
    {
        return expiresAtUtc <= DateTimeOffset.UtcNow.AddMinutes(1);
    }

    private static string GetRequiredString(JsonElement element, string propertyName)
    {
        var value = GetOptionalString(element, propertyName);
        return !string.IsNullOrWhiteSpace(value)
            ? value
            : throw new InvalidOperationException($"Microsoft Graph response did not include '{propertyName}'.");
    }

    private static string? GetOptionalString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) ? property.GetString() : null;
    }

    private sealed record PersistedSourceAuthState(
        string AccessToken,
        string RefreshToken,
        DateTimeOffset AccessTokenExpiresAtUtc,
        string DisplayName,
        string? EmailAddress,
        string DriveId);

    private sealed record AuthRequestState(
        string CallbackUri,
        string Nonce,
        DateTimeOffset IssuedAtUtc);

    private sealed record TokenResponse(
        [property: System.Text.Json.Serialization.JsonPropertyName("access_token")]
        string AccessToken,
        [property: System.Text.Json.Serialization.JsonPropertyName("expires_in")]
        int ExpiresIn,
        [property: System.Text.Json.Serialization.JsonPropertyName("refresh_token")]
        string RefreshToken);
}
