using Chronoscope.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chronoscope.Data.Configurations;

public sealed class SourceEntityConfiguration : IEntityTypeConfiguration<Source>
{
    public void Configure(EntityTypeBuilder<Source> builder)
    {
        builder.ToTable("sources");

        builder.HasKey(source => source.Id);

        builder.Property(source => source.Type)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(source => source.FolderPath)
            .IsRequired()
            .HasMaxLength(1024);

        builder.Property(source => source.DeltaToken)
            .HasMaxLength(2048);

        builder.Property(source => source.AuthState)
            .HasColumnType("text");

        builder.Property(source => source.CreatedAtUtc)
            .IsRequired();
    }
}
