namespace Chronoscope.Infrastructure.Auth;

public sealed class OneDriveTokenCacheOptions
{
    public const string SectionName = "OneDrive:TokenCache";

    public required string SchemaName { get; init; }
    public required string TableName { get; init; }
    public bool CreateIfNotExists { get; init; } = true;
}
