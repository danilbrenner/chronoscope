using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace Chronoscope.Infrastructure;

public static class InfrastructureSetup
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        return services;
    }
}
