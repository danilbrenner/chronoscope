using Chronoscope.Application.SourceAuth;

namespace Chronoscope.Application.Abstractions.SourceAuth;

public interface ISourceAuthService
{
    Task<SourceAuthSession> GetCurrentAsync(CancellationToken cancellationToken = default);
}
