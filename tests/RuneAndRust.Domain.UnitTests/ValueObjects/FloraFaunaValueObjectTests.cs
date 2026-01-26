using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for <see cref="HarvestableFlora"/> value object.
/// </summary>
[TestFixture]
public class HarvestableFloraTests
{
    [Test]
    public void Constructor_WithAllParameters_SetsProperties()
    {
        // Arrange & Act
        var flora = new HarvestableFlora(
            SpeciesId: "flora-roots-luminous-01",
            SpeciesName: "Luminous Shelf Fungus",
            HarvestDc: 8,
            HarvestRisk: null,
            AlchemicalUse: "Light-producing elixirs");

        // Assert
        flora.SpeciesId.Should().Be("flora-roots-luminous-01");
        flora.SpeciesName.Should().Be("Luminous Shelf Fungus");
        flora.HarvestDc.Should().Be(8);
        flora.HarvestRisk.Should().BeNull();
        flora.AlchemicalUse.Should().Be("Light-producing elixirs");
    }

    [Test]
    public void IsRisky_WithNoRisk_ReturnsFalse()
    {
        // Arrange
        var flora = new HarvestableFlora("id", "Safe Plant", 8, null, null);

        // Assert
        flora.IsRisky.Should().BeFalse();
    }

    [Test]
    public void IsRisky_WithRisk_ReturnsTrue()
    {
        // Arrange
        var flora = new HarvestableFlora("id", "Rust-Eater Moss", 12, "Acidic residue", null);

        // Assert
        flora.IsRisky.Should().BeTrue();
    }

    [Test]
    public void HasAlchemicalUse_WithUse_ReturnsTrue()
    {
        // Arrange
        var flora = new HarvestableFlora("id", "Fungus", 8, null, "Light elixirs");

        // Assert
        flora.HasAlchemicalUse.Should().BeTrue();
    }

    [Test]
    public void ToDisplayString_WithoutRisk_ReturnsBasicFormat()
    {
        // Arrange
        var flora = new HarvestableFlora("id", "Luminous Shelf Fungus", 8, null, null);

        // Act
        var result = flora.ToDisplayString();

        // Assert
        result.Should().Be("Luminous Shelf Fungus (DC 8)");
    }

    [Test]
    public void ToDisplayString_WithRisk_IncludesRiskWarning()
    {
        // Arrange
        var flora = new HarvestableFlora("id", "Rust-Eater Moss", 12, "Acidic residue", null);

        // Act
        var result = flora.ToDisplayString();

        // Assert
        result.Should().Be("Rust-Eater Moss (DC 12) [Risk: Acidic residue]");
    }
}

/// <summary>
/// Unit tests for <see cref="SpeciesExaminationResult"/> value object.
/// </summary>
[TestFixture]
public class SpeciesExaminationResultTests
{
    [Test]
    public void Constructor_WithAllParameters_SetsProperties()
    {
        // Arrange & Act
        var layers = new List<string> { "Layer 1 text", "Layer 2 text" };
        var result = new SpeciesExaminationResult(
            SpeciesId: "flora-roots-luminous-01",
            SpeciesName: "Luminous Shelf Fungus",
            ScientificName: "Fungus luminaris",
            Category: FloraFaunaCategory.Flora,
            LayersRevealed: layers,
            HighestLayerReached: 2,
            SuccessCount: 3,
            AlchemicalUseRevealed: null,
            HarvestInfoRevealed: null);

        // Assert
        result.SpeciesId.Should().Be("flora-roots-luminous-01");
        result.SpeciesName.Should().Be("Luminous Shelf Fungus");
        result.ScientificName.Should().Be("Fungus luminaris");
        result.Category.Should().Be(FloraFaunaCategory.Flora);
        result.LayersRevealed.Should().HaveCount(2);
        result.HighestLayerReached.Should().Be(2);
        result.SuccessCount.Should().Be(3);
    }

    [Test]
    public void ReachedExpertLevel_AtLayer3_ReturnsTrue()
    {
        // Arrange
        var result = CreateResult(highestLayer: 3);

        // Assert
        result.ReachedExpertLevel.Should().BeTrue();
    }

    [Test]
    public void ReachedExpertLevel_AtLayer2_ReturnsFalse()
    {
        // Arrange
        var result = CreateResult(highestLayer: 2);

        // Assert
        result.ReachedExpertLevel.Should().BeFalse();
    }

    [Test]
    public void GetDisplayHeader_AtExpertLevel_IncludesScientificName()
    {
        // Arrange
        var result = new SpeciesExaminationResult(
            "id", "Cave Rat", "Rattus subterranus", FloraFaunaCategory.Fauna,
            new List<string>(), 3, 4, null, null);

        // Act
        var header = result.GetDisplayHeader();

        // Assert
        header.Should().Be("Cave Rat (Rattus subterranus)");
    }

    [Test]
    public void GetDisplayHeader_BelowExpertLevel_ReturnsOnlyCommonName()
    {
        // Arrange
        var result = new SpeciesExaminationResult(
            "id", "Cave Rat", "Rattus subterranus", FloraFaunaCategory.Fauna,
            new List<string>(), 2, 2, null, null);

        // Act
        var header = result.GetDisplayHeader();

        // Assert
        header.Should().Be("Cave Rat");
    }

    [Test]
    public void IsFlora_ForFloraCategory_ReturnsTrue()
    {
        // Arrange
        var result = CreateResult(category: FloraFaunaCategory.Flora);

        // Assert
        result.IsFlora.Should().BeTrue();
        result.IsFauna.Should().BeFalse();
    }

    [Test]
    public void IsFauna_ForFaunaCategory_ReturnsTrue()
    {
        // Arrange
        var result = CreateResult(category: FloraFaunaCategory.Fauna);

        // Assert
        result.IsFauna.Should().BeTrue();
        result.IsFlora.Should().BeFalse();
    }

    private static SpeciesExaminationResult CreateResult(
        int highestLayer = 1,
        FloraFaunaCategory category = FloraFaunaCategory.Flora) =>
        new("id", "Test Species", null, category, new List<string>(), highestLayer, highestLayer, null, null);
}
