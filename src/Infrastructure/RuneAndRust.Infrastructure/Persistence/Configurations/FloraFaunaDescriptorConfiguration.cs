using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RuneAndRust.Domain.Entities;

namespace RuneAndRust.Infrastructure.Persistence.Configurations;

public class FloraFaunaDescriptorConfiguration : IEntityTypeConfiguration<FloraFaunaDescriptor>
{
    public void Configure(EntityTypeBuilder<FloraFaunaDescriptor> builder)
    {
        builder.ToTable("FloraFaunaDescriptors");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.Category)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.SpeciesName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.ScientificName)
            .HasMaxLength(100);

        builder.Property(x => x.Layer)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.RequiredDC)
            .HasDefaultValue(0);

        builder.Property(x => x.Biome)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.DescriptorText)
            .IsRequired();

        builder.Property(x => x.AlchemicalUse)
            .HasMaxLength(500);

        builder.Property(x => x.HarvestDC);

        builder.Property(x => x.HarvestRisk)
            .HasMaxLength(200);

        builder.Property(x => x.Weight)
            .HasDefaultValue(1);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        // Index for biome-based queries
        builder.HasIndex(x => new { x.Biome, x.Category })
            .HasDatabaseName("IX_FloraFauna_Biome");

        // Index for species lookups
        builder.HasIndex(x => x.SpeciesName)
            .HasDatabaseName("IX_FloraFauna_Species");
    }
}
