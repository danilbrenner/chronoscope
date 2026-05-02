using Microsoft.Extensions.DependencyInjection;

namespace Chronoscope.Application;

public static class ApplicationSetup
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services;
    }
}
