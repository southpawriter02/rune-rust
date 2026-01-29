using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for <see cref="SmartLootResult"/> value object.
/// </summary>
/// <remarks>
/// Tests validate factory methods, computed properties, and the empty result.
/// </remarks>
[TestFixture]
public class SmartLootResultTests
{
    #region Test Data

    private static readonly LootEntry TestSword = LootEntry.Create("iron-sword", "swords");
    private static readonly LootEntry TestDagger = LootEntry.Create("steel-dagger", "daggers");

    #endregion

    #region Empty Result Tests

    /// <summary>
    /// Verifies that Empty result has no selection and appropriate metadata.
    /// </summary>
    [Test]
    public void Empty_HasNoSelection()
    {
        // Arrange & Act
        var result = SmartLootResult.Empty;

        // Assert
        result.HasSelection.Should().BeFalse();
        result.SelectedItem.Should().BeNull();
        result.WasClassAppropriate.Should().BeFalse();
        result.BiasRoll.Should().Be(-1);
        result.FilteredPoolSize.Should().Be(0);
        result.TotalPoolSize.Should().Be(0);
        result.SelectionReason.Should().Be("No items available");
    }

    #endregion

    #region CreateClassAppropriate Tests

    /// <summary>
    /// Verifies that CreateClassAppropriate sets correct flags and metadata.
    /// </summary>
    [Test]
    public void CreateClassAppropriate_SetsCorrectFlags()
    {
        // Arrange
        const int biasRoll = 45;      // Within class-appropriate range
        const int filteredSize = 5;
        const int totalSize = 10;

        // Act
        var result = SmartLootResult.CreateClassAppropriate(
            TestSword,
            biasRoll,
            filteredSize,
            totalSize);

        // Assert
        result.HasSelection.Should().BeTrue();
        result.SelectedItem.Should().Be(TestSword);
        result.WasClassAppropriate.Should().BeTrue();
        result.BiasRoll.Should().Be(biasRoll);
        result.FilteredPoolSize.Should().Be(filteredSize);
        result.TotalPoolSize.Should().Be(totalSize);
        result.SelectionReason.Should().Contain("Class-appropriate");
    }

    /// <summary>
    /// Verifies that BiasRollFavoredClass is true when roll is below 60.
    /// </summary>
    [Test]
    public void CreateClassAppropriate_BiasRollFavoredClass_IsTrue()
    {
        // Arrange & Act
        var result = SmartLootResult.CreateClassAppropriate(
            TestSword,
            biasRoll: 30,  // Below 60
            filteredPoolSize: 5,
            totalPoolSize: 10);

        // Assert
        result.BiasRollFavoredClass.Should().BeTrue();
    }

    #endregion

    #region CreateRandom Tests

    /// <summary>
    /// Verifies that CreateRandom sets WasClassAppropriate to false.
    /// </summary>
    [Test]
    public void CreateRandom_SetsWasClassAppropriateToFalse()
    {
        // Arrange & Act
        var result = SmartLootResult.CreateRandom(
            TestDagger,
            biasRoll: 75,  // Above 60, random path
            filteredPoolSize: 3,
            totalPoolSize: 10);

        // Assert
        result.WasClassAppropriate.Should().BeFalse();
        result.SelectionReason.Should().Contain("Random");
        result.BiasRollFavoredClass.Should().BeFalse();
    }

    #endregion

    #region CreateFallback Tests

    /// <summary>
    /// Verifies that CreateFallback indicates bias succeeded but no class items.
    /// </summary>
    [Test]
    public void CreateFallback_HasZeroFilteredPoolSize()
    {
        // Arrange & Act
        var result = SmartLootResult.CreateFallback(
            TestSword,
            biasRoll: 25,  // Would have been class-appropriate
            totalPoolSize: 10);

        // Assert
        result.FilteredPoolSize.Should().Be(0);
        result.SelectionReason.Should().Contain("Fallback");
        result.SelectionReason.Should().Contain("no class-appropriate");
        result.WasClassAppropriate.Should().BeFalse();
        result.BiasRollFavoredClass.Should().BeTrue();  // Roll favored class, but no items
    }

    #endregion

    #region CreateRandomOnly Tests

    /// <summary>
    /// Verifies that CreateRandomOnly is used when no archetype is specified.
    /// </summary>
    [Test]
    public void CreateRandomOnly_HasNegativeBiasRoll()
    {
        // Arrange & Act
        var result = SmartLootResult.CreateRandomOnly(
            TestDagger,
            totalPoolSize: 10);

        // Assert
        result.BiasRoll.Should().Be(-1);
        result.SelectionReason.Should().Contain("no archetype");
        result.WasClassAppropriate.Should().BeFalse();
    }

    #endregion

    #region FilteredPoolPercentage Tests

    /// <summary>
    /// Verifies FilteredPoolPercentage calculation.
    /// </summary>
    [Test]
    public void FilteredPoolPercentage_CalculatesCorrectly()
    {
        // Arrange
        var result = SmartLootResult.CreateClassAppropriate(
            TestSword,
            biasRoll: 30,
            filteredPoolSize: 5,
            totalPoolSize: 10);

        // Act & Assert
        result.FilteredPoolPercentage.Should().BeApproximately(50.0, 0.01);
    }

    /// <summary>
    /// Verifies FilteredPoolPercentage returns 0 when total is 0.
    /// </summary>
    [Test]
    public void FilteredPoolPercentage_WithZeroTotal_ReturnsZero()
    {
        // Arrange & Act
        var result = SmartLootResult.Empty;

        // Assert
        result.FilteredPoolPercentage.Should().Be(0);
    }

    #endregion

    #region ToString Tests

    /// <summary>
    /// Verifies ToString produces readable output for logging.
    /// </summary>
    [Test]
    public void ToString_WithSelection_ReturnsReadableFormat()
    {
        // Arrange
        var result = SmartLootResult.CreateClassAppropriate(
            TestSword,
            biasRoll: 30,
            filteredPoolSize: 5,
            totalPoolSize: 10);

        // Act
        var str = result.ToString();

        // Assert
        str.Should().Contain("iron-sword");
        str.Should().Contain("Roll=30");
        str.Should().Contain("Filtered=5/10");
    }

    /// <summary>
    /// Verifies ToString for empty result.
    /// </summary>
    [Test]
    public void ToString_Empty_IndicatesNoSelection()
    {
        // Arrange & Act
        var str = SmartLootResult.Empty.ToString();

        // Assert
        str.Should().Contain("Empty");
    }

    #endregion
}
