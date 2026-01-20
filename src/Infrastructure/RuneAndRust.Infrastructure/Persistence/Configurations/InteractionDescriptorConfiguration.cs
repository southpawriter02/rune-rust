using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RuneAndRust.Domain.Entities;

namespace RuneAndRust.Infrastructure.Persistence.Configurations;

public class InteractionDescriptorConfiguration : IEntityTypeConfiguration<InteractionDescriptor>
{
    public void Configure(EntityTypeBuilder<InteractionDescriptor> builder)
    {
        builder.ToTable("InteractionDescriptors");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.Category)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.SubCategory)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.State)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.DescriptorText)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.BiomeAffinity)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(x => x.Weight)
            .HasDefaultValue(1);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        // Indexes for efficient queries
        builder.HasIndex(x => new { x.Category, x.SubCategory, x.State })
            .HasDatabaseName("IX_InteractionDesc_Category_Sub_State");

        builder.HasIndex(x => x.Category)
            .HasDatabaseName("IX_InteractionDesc_Category");
    }
}
