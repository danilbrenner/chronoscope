namespace Chronoscope.Infrastructure.Auth;

public sealed class DataProtectionOptions
{
    public const string SectionName = "DataProtection";

    public required string KeysPath { get; init; }
}
