using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RuneAndRust.Domain.Entities;

namespace RuneAndRust.Infrastructure.Persistence.Configurations;

public class PerceptionDescriptorConfiguration : IEntityTypeConfiguration<PerceptionDescriptor>
{
    public void Configure(EntityTypeBuilder<PerceptionDescriptor> builder)
    {
        builder.ToTable("PerceptionDescriptors");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.PerceptionType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.SuccessLevel)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.RequiredDC)
            .IsRequired();

        builder.Property(x => x.BiomeAffinity)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(x => x.DescriptorText)
            .IsRequired();

        builder.Property(x => x.Weight)
            .HasDefaultValue(1);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        // Index for efficient queries
        builder.HasIndex(x => new { x.PerceptionType, x.SuccessLevel })
            .HasDatabaseName("IX_PercDesc_Type");
    }
}
