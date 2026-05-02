namespace Chronoscope.Domain.Entities;

public sealed class Photo
{
    public Guid InternalId { get; set; }
    public Guid SourceId { get; set; }
    public Source Source { get; set; } = null!;
    public string ExternalId { get; set; } = string.Empty;
    public string Filename { get; set; } = string.Empty;
    public DateTimeOffset TakenAt { get; set; }
    public long SizeBytes { get; set; }
    public byte[]? Thumbnail { get; set; }
    public ProcessingStatus ProcessingStatus { get; set; } = ProcessingStatus.Discovered;
}
