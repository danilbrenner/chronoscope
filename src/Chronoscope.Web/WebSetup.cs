using Microsoft.Extensions.DependencyInjection;

namespace Chronoscope.Web;

public static class WebSetup
{
    public static IServiceCollection AddWeb(this IServiceCollection services)
    {
        services.AddControllersWithViews()
            .AddApplicationPart(typeof(WebSetup).Assembly);

        return services;
    }
}
