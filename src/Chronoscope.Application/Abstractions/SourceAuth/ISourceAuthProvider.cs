using Chronoscope.Application.SourceAuth;

namespace Chronoscope.Application.Abstractions.SourceAuth;

public interface ISourceAuthProvider
{
    string CreateLinkUriAsync(string callbackUri, CancellationToken cancellationToken = default);
    Task CompleteLinkAsync(string authorizationCode, string state, string callbackUri, CancellationToken cancellationToken = default);
    Task<SourceAuthSession> GetCurrentAsync(CancellationToken cancellationToken = default);
    Task RefreshTokensIfNeededAsync(CancellationToken cancellationToken = default);
    Task UnlinkCurrentAsync(CancellationToken cancellationToken = default);
}
