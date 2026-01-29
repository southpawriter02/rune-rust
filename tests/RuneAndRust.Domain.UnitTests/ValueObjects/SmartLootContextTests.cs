using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for <see cref="SmartLootContext"/> value object.
/// </summary>
/// <remarks>
/// Tests validate factory methods, computed properties, and validation behavior.
/// </remarks>
[TestFixture]
public class SmartLootContextTests
{
    #region Test Data

    private static readonly List<LootEntry> TestItems =
    [
        LootEntry.Create("iron-sword", "swords"),
        LootEntry.Create("steel-dagger", "daggers"),
        LootEntry.Create("health-potion", null)  // Non-equipment item
    ];

    #endregion

    #region Create Factory Method Tests

    /// <summary>
    /// Verifies that Create with valid parameters produces a correctly configured context.
    /// </summary>
    [Test]
    public void Create_WithValidParameters_ReturnsCorrectContext()
    {
        // Arrange
        const string archetypeId = "warrior";
        const QualityTier tier = QualityTier.ClanForged;
        const int bias = 70;

        // Act
        var context = SmartLootContext.Create(
            archetypeId,
            tier,
            TestItems,
            bias);

        // Assert
        context.PlayerArchetypeId.Should().Be(archetypeId);
        context.QualityTier.Should().Be(tier);
        context.AvailableItems.Should().HaveCount(3);
        context.BiasPercentage.Should().Be(bias);
        context.HasPlayerArchetype.Should().BeTrue();
        context.HasAvailableItems.Should().BeTrue();
        context.NormalizedArchetypeId.Should().Be("warrior");
        context.AvailableItemCount.Should().Be(3);
    }

    /// <summary>
    /// Verifies that Create with null archetype produces context for random-only selection.
    /// </summary>
    [Test]
    public void Create_WithNullArchetype_HasPlayerArchetypeIsFalse()
    {
        // Arrange & Act
        var context = SmartLootContext.Create(
            playerArchetypeId: null,
            QualityTier.Scavenged,
            TestItems);

        // Assert
        context.HasPlayerArchetype.Should().BeFalse();
        context.NormalizedArchetypeId.Should().BeNull();
    }

    /// <summary>
    /// Verifies that Create normalizes archetype ID to lowercase.
    /// </summary>
    [Test]
    public void Create_WithUppercaseArchetype_NormalizesToLowercase()
    {
        // Arrange & Act
        var context = SmartLootContext.Create(
            "WARRIOR",
            QualityTier.Scavenged,
            TestItems);

        // Assert
        context.NormalizedArchetypeId.Should().Be("warrior");
    }

    /// <summary>
    /// Verifies that Create uses default bias when not specified.
    /// </summary>
    [Test]
    public void Create_WithDefaultBias_UsesSixtyPercent()
    {
        // Arrange & Act
        var context = SmartLootContext.Create(
            "warrior",
            QualityTier.Scavenged,
            TestItems);

        // Assert
        context.BiasPercentage.Should().Be(SmartLootContext.DefaultBiasPercentage);
        context.BiasPercentage.Should().Be(60);
    }

    /// <summary>
    /// Verifies that Create throws when availableItems is null.
    /// </summary>
    [Test]
    public void Create_WithNullItems_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => SmartLootContext.Create(
            "warrior",
            QualityTier.Scavenged,
            null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("availableItems");
    }

    /// <summary>
    /// Verifies that Create throws when bias percentage is negative.
    /// </summary>
    [Test]
    public void Create_WithNegativeBias_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => SmartLootContext.Create(
            "warrior",
            QualityTier.Scavenged,
            TestItems,
            biasPercentage: -1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("biasPercentage");
    }

    /// <summary>
    /// Verifies that Create throws when bias percentage exceeds 100.
    /// </summary>
    [Test]
    public void Create_WithBiasOver100_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => SmartLootContext.Create(
            "warrior",
            QualityTier.Scavenged,
            TestItems,
            biasPercentage: 101);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("biasPercentage");
    }

    #endregion

    #region CreateRandomOnly Factory Method Tests

    /// <summary>
    /// Verifies that CreateRandomOnly sets zero bias for pure random selection.
    /// </summary>
    [Test]
    public void CreateRandomOnly_SetsZeroBias()
    {
        // Arrange & Act
        var context = SmartLootContext.CreateRandomOnly(
            QualityTier.Scavenged,
            TestItems);

        // Assert
        context.BiasPercentage.Should().Be(0);
        context.PlayerArchetypeId.Should().BeNull();
        context.HasPlayerArchetype.Should().BeFalse();
    }

    #endregion

    #region CreateForTesting Factory Method Tests

    /// <summary>
    /// Verifies that CreateForTesting includes random seed for deterministic behavior.
    /// </summary>
    [Test]
    public void CreateForTesting_IncludesRandomSeed()
    {
        // Arrange
        const int seed = 42;

        // Act
        var context = SmartLootContext.CreateForTesting(
            "warrior",
            QualityTier.Scavenged,
            TestItems,
            seed);

        // Assert
        context.RandomSeed.Should().Be(seed);
    }

    #endregion

    #region HasAvailableItems Tests

    /// <summary>
    /// Verifies that HasAvailableItems returns false for empty list.
    /// </summary>
    [Test]
    public void HasAvailableItems_WithEmptyList_ReturnsFalse()
    {
        // Arrange & Act
        var context = SmartLootContext.Create(
            "warrior",
            QualityTier.Scavenged,
            new List<LootEntry>());

        // Assert
        context.HasAvailableItems.Should().BeFalse();
        context.AvailableItemCount.Should().Be(0);
    }

    #endregion

    #region ToString Tests

    /// <summary>
    /// Verifies ToString produces readable output for logging.
    /// </summary>
    [Test]
    public void ToString_ReturnsReadableFormat()
    {
        // Arrange
        var context = SmartLootContext.Create(
            "warrior",
            QualityTier.ClanForged,
            TestItems);

        // Act
        var result = context.ToString();

        // Assert
        result.Should().Contain("Archetype=warrior");
        result.Should().Contain("Tier=ClanForged");
        result.Should().Contain("Items=3");
        result.Should().Contain("Bias=60%");
    }

    #endregion
}
