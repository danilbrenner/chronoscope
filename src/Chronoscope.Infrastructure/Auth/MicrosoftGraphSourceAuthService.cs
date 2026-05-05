using System.Net.Http.Headers;
using System.Text.Json;
using Chronoscope.Application.Abstractions.SourceAuth;
using Chronoscope.Application.SourceAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;

namespace Chronoscope.Infrastructure.Auth;

public sealed class MicrosoftGraphSourceAuthService(
    IHttpClientFactory httpClientFactory,
    ITokenAcquisition tokenAcquisition,
    IHttpContextAccessor httpContextAccessor,
    IOptions<OneDriveAuthOptions> authOptions) : ISourceAuthService
{
    public const string HttpClientName = "MicrosoftGraph";

    public async Task<SourceAuthSession> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        var accessToken = await tokenAcquisition.GetAccessTokenForUserAsync(
            authOptions.Value.Scopes,
            user: httpContextAccessor.HttpContext?.User);

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

    private static string GetRequiredString(JsonElement element, string propertyName)
    {
        var value = GetOptionalString(element, propertyName);
        if (!string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        throw new InvalidOperationException($"Microsoft Graph response did not include '{propertyName}'.");
    }

    private static string? GetOptionalString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) ? property.GetString() : null;
    }
}
