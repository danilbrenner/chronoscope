using Chronoscope.Application.Abstractions.Data;
using Chronoscope.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Chronoscope.Data.SourceAuth;

public sealed class SourceAuthStateRepository(IDbContextFactory<ChronoscopeDbContext> dbContextFactory) : ISourceAuthStateRepository
{
    private const string OneDriveSourceType = "onedrive";

    public async Task<string?> GetProtectedStateAsync(CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var source = await dbContext.Sources
            .AsNoTracking()
            .Where(item => item.Type == OneDriveSourceType)
            .OrderBy(item => item.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        return source?.AuthState;
    }

    public async Task SaveProtectedStateAsync(string protectedState, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(protectedState);

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var source = await dbContext.Sources
            .Where(item => item.Type == OneDriveSourceType)
            .OrderBy(item => item.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (source is null)
        {
            source = new Source
            {
                Id = Guid.NewGuid(),
                Type = OneDriveSourceType,
                FolderPath = string.Empty,
                AuthState = protectedState,
                CreatedAtUtc = DateTimeOffset.UtcNow
            };

            dbContext.Sources.Add(source);
        }
        else
        {
            source.AuthState = protectedState;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var source = await dbContext.Sources
            .Where(item => item.Type == OneDriveSourceType)
            .OrderBy(item => item.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (source is null)
        {
            return;
        }

        source.AuthState = null;
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

