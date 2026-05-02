namespace Chronoscope.Domain.Entities;

public sealed class Source
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string FolderPath { get; set; } = string.Empty;
    public string? DeltaToken { get; set; }
    public string? AuthState { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    public ICollection<Photo> Photos { get; set; } = new List<Photo>();
}
