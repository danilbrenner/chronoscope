using Chronoscope.Application.Abstractions.SourceAuth;

namespace Chronoscope.Host.Services;

public sealed class RefreshTokenService(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<RefreshTokenService> logger) : BackgroundService
{
    private static readonly TimeSpan RefreshInterval = TimeSpan.FromHours(24);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(RefreshInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            await RefreshTokensIfNeededAsync(stoppingToken);
            await timer.WaitForNextTickAsync(stoppingToken);
        }
    }

    private async Task RefreshTokensIfNeededAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var sourceAuthProvider = scope.ServiceProvider.GetRequiredService<ISourceAuthProvider>();

        try
        {
            await sourceAuthProvider.RefreshTokensIfNeededAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Failed to refresh source authentication tokens");
        }
    }
}
