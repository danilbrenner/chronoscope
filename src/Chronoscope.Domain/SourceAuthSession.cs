namespace Chronoscope.Application.SourceAuth;

public sealed record SourceAuthSession(
    string DisplayName,
    string? EmailAddress,
    string DriveId);
