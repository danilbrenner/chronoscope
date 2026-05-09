using Chronoscope.Application.Abstractions.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Chronoscope.Application.Abstractions.SourceAuth;

namespace Chronoscope.Data;

public static class DataSetup
{
    public static IServiceCollection AddData(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Chronoscope");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'Chronoscope' was not found.");
        }

        services.AddDbContextFactory<ChronoscopeDbContext>(options =>
            options.UseNpgsql(connectionString, npgsqlOptions => npgsqlOptions.UseNetTopologySuite())
                .UseSnakeCaseNamingConvention());

        services.AddScoped<ISourceAuthStateRepository, SourceAuth.SourceAuthStateRepository>();

        return services;
    }
}
