using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RuneAndRust.Domain.Entities;

namespace RuneAndRust.Infrastructure.Persistence.Configurations;

public class ExaminationDescriptorConfiguration : IEntityTypeConfiguration<ExaminationDescriptor>
{
    public void Configure(EntityTypeBuilder<ExaminationDescriptor> builder)
    {
        builder.ToTable("ExaminationDescriptors");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.ObjectCategory)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.ObjectType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Layer)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.RequiredDC)
            .HasDefaultValue(0);

        builder.Property(x => x.BiomeAffinity)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(x => x.DescriptorText)
            .IsRequired();

        builder.Property(x => x.RevealsHint)
            .HasDefaultValue(false);

        builder.Property(x => x.RevealsSolutionId)
            .HasMaxLength(100);

        builder.Property(x => x.Weight)
            .HasDefaultValue(1);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        // Indexes for efficient queries
        builder.HasIndex(x => new { x.ObjectCategory, x.ObjectType })
            .HasDatabaseName("IX_ExamDesc_Category");

        builder.HasIndex(x => new { x.Layer, x.RequiredDC })
            .HasDatabaseName("IX_ExamDesc_Layer");
    }
}
