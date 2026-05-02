using Chronoscope.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chronoscope.Data.Configurations;

public sealed class PhotoEntityConfiguration : IEntityTypeConfiguration<Photo>
{
    public void Configure(EntityTypeBuilder<Photo> builder)
    {
        builder.ToTable("photos");

        builder.HasKey(photo => photo.InternalId);

        builder.Property(photo => photo.ExternalId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(photo => photo.Filename)
            .IsRequired()
            .HasMaxLength(1024);

        builder.Property(photo => photo.TakenAt)
            .IsRequired();

        builder.Property(photo => photo.SizeBytes)
            .IsRequired();

        builder.Property(photo => photo.Thumbnail);

        builder.Property(photo => photo.ProcessingStatus)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(64);

        builder.HasIndex(photo => new { photo.SourceId, photo.ExternalId })
            .IsUnique();

        builder.HasOne(photo => photo.Source)
            .WithMany(source => source.Photos)
            .HasForeignKey(photo => photo.SourceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
