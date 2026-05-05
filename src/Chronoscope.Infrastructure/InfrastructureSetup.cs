using Chronoscope.Application.Abstractions.SourceAuth;
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

        services
            .AddAuthentication(Microsoft.AspNetCore.Authentication.OpenIdConnect.OpenIdConnectDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApp(authConfiguration)
            .EnableTokenAcquisitionToCallDownstreamApi(authOptions.Scopes)
            .AddInMemoryTokenCaches();

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
