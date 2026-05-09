namespace Chronoscope.Application.Abstractions.Data;

public interface ISourceAuthStateRepository
{
    Task<string?> GetProtectedStateAsync(CancellationToken cancellationToken = default);
    Task SaveProtectedStateAsync(string protectedState, CancellationToken cancellationToken = default);
    Task ClearAsync(CancellationToken cancellationToken = default);
}

