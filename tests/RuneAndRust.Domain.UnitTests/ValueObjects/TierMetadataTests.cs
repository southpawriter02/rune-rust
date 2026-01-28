using FluentAssertions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="TierMetadata"/> value object.
/// </summary>
[TestFixture]
public class TierMetadataTests
{
    private TierColorDefinition _testColor;

    [SetUp]
    public void SetUp()
    {
        _testColor = TierColorDefinition.FromHex("#00FF00", ConsoleColor.Green);
    }

    /// <summary>
    /// Verifies that Create returns a valid TierMetadata with all properties set.
    /// </summary>
    [Test]
    public void Create_WithValidParameters_ReturnsTierMetadata()
    {
        // Act
        var metadata = TierMetadata.Create(
            tier: QualityTier.ClanForged,
            displayName: "Clan-Forged",
            displayNamePlural: "Clan-Forged items",
            description: "Quality craftsmanship from regional clan smiths.",
            colorDefinition: _testColor,
            iconGlyph: '◆',
            dropWeightMultiplier: 0.25m);

        // Assert
        metadata.Tier.Should().Be(QualityTier.ClanForged);
        metadata.DisplayName.Should().Be("Clan-Forged");
        metadata.DisplayNamePlural.Should().Be("Clan-Forged items");
        metadata.Description.Should().Be("Quality craftsmanship from regional clan smiths.");
        metadata.ColorDefinition.Should().Be(_testColor);
        metadata.IconGlyph.Should().Be('◆');
        metadata.DropWeightMultiplier.Should().Be(0.25m);
    }

    /// <summary>
    /// Verifies that Create throws when displayName is null or empty.
    /// </summary>
    [Test]
    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void Create_WithNullOrEmptyDisplayName_ThrowsArgumentException(string? invalidName)
    {
        // Act
        var act = () => TierMetadata.Create(
            tier: QualityTier.Scavenged,
            displayName: invalidName!,
            displayNamePlural: "Items",
            description: "Valid description text.",
            colorDefinition: _testColor,
            iconGlyph: '●',
            dropWeightMultiplier: 0.5m);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Verifies that Create throws when displayNamePlural is null or empty.
    /// </summary>
    [Test]
    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void Create_WithNullOrEmptyDisplayNamePlural_ThrowsArgumentException(string? invalidName)
    {
        // Act
        var act = () => TierMetadata.Create(
            tier: QualityTier.Scavenged,
            displayName: "Scavenged",
            displayNamePlural: invalidName!,
            description: "Valid description text.",
            colorDefinition: _testColor,
            iconGlyph: '●',
            dropWeightMultiplier: 0.5m);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Verifies that Create throws when description is null or empty.
    /// </summary>
    [Test]
    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void Create_WithNullOrEmptyDescription_ThrowsArgumentException(string? invalidDescription)
    {
        // Act
        var act = () => TierMetadata.Create(
            tier: QualityTier.Scavenged,
            displayName: "Scavenged",
            displayNamePlural: "Scavenged items",
            description: invalidDescription!,
            colorDefinition: _testColor,
            iconGlyph: '●',
            dropWeightMultiplier: 0.5m);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Verifies that Create throws when dropWeightMultiplier is negative.
    /// </summary>
    [Test]
    public void Create_WithNegativeDropWeight_ThrowsArgumentOutOfRangeException()
    {
        // Act
        var act = () => TierMetadata.Create(
            tier: QualityTier.Scavenged,
            displayName: "Scavenged",
            displayNamePlural: "Scavenged items",
            description: "Valid description text.",
            colorDefinition: _testColor,
            iconGlyph: '●',
            dropWeightMultiplier: -0.5m);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// Verifies that Create accepts zero as a valid drop weight.
    /// </summary>
    [Test]
    public void Create_WithZeroDropWeight_Succeeds()
    {
        // Act
        var metadata = TierMetadata.Create(
            tier: QualityTier.JuryRigged,
            displayName: "Jury-Rigged",
            displayNamePlural: "Jury-Rigged items",
            description: "Makeshift equipment.",
            colorDefinition: _testColor,
            iconGlyph: '○',
            dropWeightMultiplier: 0m);

        // Assert
        metadata.DropWeightMultiplier.Should().Be(0m);
    }

    /// <summary>
    /// Verifies that FormatItemName returns the expected format.
    /// </summary>
    [Test]
    public void FormatItemName_WithValidName_ReturnsFormattedString()
    {
        // Arrange
        var metadata = TierMetadata.Create(
            tier: QualityTier.ClanForged,
            displayName: "Clan-Forged",
            displayNamePlural: "Clan-Forged items",
            description: "Quality craftsmanship.",
            colorDefinition: _testColor,
            iconGlyph: '◆',
            dropWeightMultiplier: 0.25m);

        // Act
        var result = metadata.FormatItemName("Iron Longsword");

        // Assert
        result.Should().Be("[Clan-Forged] Iron Longsword");
    }

    /// <summary>
    /// Verifies that FormatItemName throws for null or empty item names.
    /// </summary>
    [Test]
    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void FormatItemName_WithInvalidName_ThrowsArgumentException(string? invalidName)
    {
        // Arrange
        var metadata = TierMetadata.Create(
            tier: QualityTier.Scavenged,
            displayName: "Scavenged",
            displayNamePlural: "Scavenged items",
            description: "Standard equipment.",
            colorDefinition: _testColor,
            iconGlyph: '●',
            dropWeightMultiplier: 0.5m);

        // Act
        var act = () => metadata.FormatItemName(invalidName!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Verifies that TierValue returns the correct integer.
    /// </summary>
    [Test]
    [TestCase(QualityTier.JuryRigged, 0)]
    [TestCase(QualityTier.Scavenged, 1)]
    [TestCase(QualityTier.ClanForged, 2)]
    [TestCase(QualityTier.Optimized, 3)]
    [TestCase(QualityTier.MythForged, 4)]
    public void TierValue_ReturnsCorrectInteger(QualityTier tier, int expectedValue)
    {
        // Arrange
        var metadata = TierMetadata.Create(
            tier: tier,
            displayName: "Test",
            displayNamePlural: "Tests",
            description: "Test description text.",
            colorDefinition: _testColor,
            iconGlyph: '○',
            dropWeightMultiplier: 1.0m);

        // Assert
        metadata.TierValue.Should().Be(expectedValue);
    }

    /// <summary>
    /// Verifies that IsLowestTier returns true only for JuryRigged.
    /// </summary>
    [Test]
    [TestCase(QualityTier.JuryRigged, true)]
    [TestCase(QualityTier.Scavenged, false)]
    [TestCase(QualityTier.ClanForged, false)]
    [TestCase(QualityTier.Optimized, false)]
    [TestCase(QualityTier.MythForged, false)]
    public void IsLowestTier_ReturnsCorrectValue(QualityTier tier, bool expected)
    {
        // Arrange
        var metadata = TierMetadata.Create(
            tier: tier,
            displayName: "Test",
            displayNamePlural: "Tests",
            description: "Test description text.",
            colorDefinition: _testColor,
            iconGlyph: '○',
            dropWeightMultiplier: 1.0m);

        // Assert
        metadata.IsLowestTier.Should().Be(expected);
    }

    /// <summary>
    /// Verifies that IsHighestTier returns true only for MythForged.
    /// </summary>
    [Test]
    [TestCase(QualityTier.JuryRigged, false)]
    [TestCase(QualityTier.Scavenged, false)]
    [TestCase(QualityTier.ClanForged, false)]
    [TestCase(QualityTier.Optimized, false)]
    [TestCase(QualityTier.MythForged, true)]
    public void IsHighestTier_ReturnsCorrectValue(QualityTier tier, bool expected)
    {
        // Arrange
        var metadata = TierMetadata.Create(
            tier: tier,
            displayName: "Test",
            displayNamePlural: "Tests",
            description: "Test description text.",
            colorDefinition: _testColor,
            iconGlyph: '○',
            dropWeightMultiplier: 1.0m);

        // Assert
        metadata.IsHighestTier.Should().Be(expected);
    }

    /// <summary>
    /// Verifies that IsLegendary returns true only for MythForged.
    /// </summary>
    [Test]
    [TestCase(QualityTier.JuryRigged, false)]
    [TestCase(QualityTier.Scavenged, false)]
    [TestCase(QualityTier.ClanForged, false)]
    [TestCase(QualityTier.Optimized, false)]
    [TestCase(QualityTier.MythForged, true)]
    public void IsLegendary_ReturnsCorrectValue(QualityTier tier, bool expected)
    {
        // Arrange
        var metadata = TierMetadata.Create(
            tier: tier,
            displayName: "Test",
            displayNamePlural: "Tests",
            description: "Test description text.",
            colorDefinition: _testColor,
            iconGlyph: '○',
            dropWeightMultiplier: 1.0m);

        // Assert
        metadata.IsLegendary.Should().Be(expected);
    }

    /// <summary>
    /// Verifies that ToString returns a useful representation.
    /// </summary>
    [Test]
    public void ToString_ReturnsFormattedString()
    {
        // Arrange
        var metadata = TierMetadata.Create(
            tier: QualityTier.Optimized,
            displayName: "Optimized",
            displayNamePlural: "Optimized items",
            description: "Exceptional gear.",
            colorDefinition: _testColor,
            iconGlyph: '★',
            dropWeightMultiplier: 0.1m);

        // Act
        var result = metadata.ToString();

        // Assert
        result.Should().Contain("Optimized");
        result.Should().Contain("Tier 3");
        result.Should().Contain("0.10");
    }
}
