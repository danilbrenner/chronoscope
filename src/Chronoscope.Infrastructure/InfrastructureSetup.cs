using Chronoscope.Application.Abstractions.SourceAuth;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace Chronoscope.Infrastructure;

public static class InfrastructureSetup
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var authConfiguration = configuration.GetSection(Auth.OneDriveAuthOptions.SectionName);
        _ = authConfiguration.Get<Auth.OneDriveAuthOptions>()
            ?? throw new InvalidOperationException($"Failed to bind {Auth.OneDriveAuthOptions.SectionName} configuration section.");
        var dataProtectionOptions = configuration.GetSection(Auth.DataProtectionOptions.SectionName).Get<Auth.DataProtectionOptions>()
            ?? throw new InvalidOperationException($"Failed to bind {Auth.DataProtectionOptions.SectionName} configuration section.");

        var keysDirectory = Directory.CreateDirectory(dataProtectionOptions.KeysPath);

        services.AddDataProtection()
            .PersistKeysToFileSystem(keysDirectory);

        services.AddAuthentication();

        services.AddAuthorization();
        services.AddHttpClient(Auth.MicrosoftGraphSourceAuthProvider.HttpClientName, httpClient =>
        {
            httpClient.BaseAddress = new Uri("https://graph.microsoft.com/v1.0/");
        });

        services.AddOptions<Auth.OneDriveAuthOptions>()
            .Bind(authConfiguration);
        services.AddOptions<Auth.DataProtectionOptions>()
            .Bind(configuration.GetSection(Auth.DataProtectionOptions.SectionName));

        services.AddScoped<ISourceAuthProvider, Auth.MicrosoftGraphSourceAuthProvider>();

        return services;
    }
}
