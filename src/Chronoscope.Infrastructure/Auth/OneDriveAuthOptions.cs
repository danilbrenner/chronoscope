namespace Chronoscope.Infrastructure.Auth;

public sealed class OneDriveAuthOptions
{
    public const string SectionName = "OneDrive:Auth";

    public required string Instance { get; init; }
    public required string TenantId { get; init; }
    public required string ClientId { get; init; }
    public required string ClientSecret { get; init; }
    public required string[] Scopes { get; init; }
}
