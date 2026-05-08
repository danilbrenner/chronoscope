using Chronoscope.Application.Abstractions.SourceAuth;
using System.IO;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Web;

namespace Chronoscope.Infrastructure;

public static class InfrastructureSetup
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var authConfiguration = configuration.GetSection(Auth.OneDriveAuthOptions.SectionName);
        var authOptions = authConfiguration.Get<Auth.OneDriveAuthOptions>() 
                          ?? throw new InvalidOperationException($"Failed to bind {Auth.OneDriveAuthOptions.SectionName} configuration section.");
        var tokenCacheOptions = configuration.GetSection(Auth.OneDriveTokenCacheOptions.SectionName).Get<Auth.OneDriveTokenCacheOptions>()
            ?? throw new InvalidOperationException($"Failed to bind {Auth.OneDriveTokenCacheOptions.SectionName} configuration section.");
        var dataProtectionOptions = configuration.GetSection(Auth.DataProtectionOptions.SectionName).Get<Auth.DataProtectionOptions>()
            ?? throw new InvalidOperationException($"Failed to bind {Auth.DataProtectionOptions.SectionName} configuration section.");
        var connectionString = configuration.GetConnectionString("Chronoscope");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'Chronoscope' was not found.");
        }

        var keysDirectory = Directory.CreateDirectory(dataProtectionOptions.KeysPath);

        services.AddDataProtection()
            .PersistKeysToFileSystem(keysDirectory);

        services.AddDistributedPostgresCache(options =>
        {
            options.ConnectionString = connectionString;
            options.SchemaName = tokenCacheOptions.SchemaName;
            options.TableName = tokenCacheOptions.TableName;
            options.CreateIfNotExists = tokenCacheOptions.CreateIfNotExists;
        });

        services
            .AddAuthentication(Microsoft.AspNetCore.Authentication.OpenIdConnect.OpenIdConnectDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApp(authConfiguration)
            .EnableTokenAcquisitionToCallDownstreamApi(authOptions.Scopes)
            .AddDistributedTokenCaches();

        services.AddAuthorization();
        services.AddHttpContextAccessor();
        services.AddHttpClient(Auth.MicrosoftGraphSourceAuthService.HttpClientName, httpClient =>
        {
            httpClient.BaseAddress = new Uri("https://graph.microsoft.com/v1.0/");
        });

        services.AddOptions<Auth.OneDriveAuthOptions>()
            .Bind(authConfiguration)
            .Validate(options => HasRequiredScopes(options.Scopes),
                $"OneDrive auth scopes must include: {string.Join(", ", Auth.OneDriveAuthOptions.RequiredScopes)}.");
        services.AddOptions<Auth.OneDriveTokenCacheOptions>()
            .Bind(configuration.GetSection(Auth.OneDriveTokenCacheOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.SchemaName),
                "OneDrive token cache schema name is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.TableName),
                "OneDrive token cache table name is required.");
        services.AddOptions<Auth.DataProtectionOptions>()
            .Bind(configuration.GetSection(Auth.DataProtectionOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.KeysPath),
                "Data protection keys path is required.");

        services.AddScoped<ISourceAuthService, Auth.MicrosoftGraphSourceAuthService>();

        return services;
    }

    private static bool HasRequiredScopes(IReadOnlyCollection<string>? scopes)
    {
        if (scopes is null)
        {
            return false;
        }

        var configuredScopes = new HashSet<string>(scopes, StringComparer.OrdinalIgnoreCase);
        return Auth.OneDriveAuthOptions.RequiredScopes.All(configuredScopes.Contains);
    }
}
