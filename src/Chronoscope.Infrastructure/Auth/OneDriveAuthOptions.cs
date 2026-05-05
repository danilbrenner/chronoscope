namespace Chronoscope.Infrastructure.Auth;

public sealed class OneDriveAuthOptions
{
    public const string SectionName = "OneDrive:Auth";
    public static readonly string[] RequiredScopes = ["Files.Read", "offline_access", "User.Read"];

    public required string Instance { get; init; }
    public required string TenantId { get; init; }
    public required string ClientId { get; init; }
    public required string ClientSecret { get; init; }
    public required string CallbackPath { get; init; }
    public required string SignedOutCallbackPath { get; init; }
    public required string AppRegistrationType { get; init; }
    public required string SupportedAccountTypes { get; init; }
    public required string[] Scopes { get; init; }
}
