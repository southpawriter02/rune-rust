using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="ContainerContents"/> value object.
/// </summary>
/// <remarks>
/// Tests cover factory method validation, static Empty property, computed properties,
/// and convenience factory methods for the container contents value object.
/// </remarks>
[TestFixture]
public class ContainerContentsTests
{
    #region Create Factory Method Tests

    /// <summary>
    /// Verifies that Create with valid parameters creates correctly configured contents.
    /// </summary>
    [Test]
    public void Create_WithValidParameters_CreatesContents()
    {
        // Arrange
        var itemIds = new List<string> { "sword-iron", "potion-health" };

        // Act
        var contents = ContainerContents.Create(itemIds, 50, 2);

        // Assert
        contents.ItemIds.Should().BeEquivalentTo(itemIds);
        contents.CurrencyAmount.Should().Be(50);
        contents.AppliedTier.Should().Be(2);
        contents.HasItems.Should().BeTrue();
        contents.HasCurrency.Should().BeTrue();
        contents.HasContents.Should().BeTrue();
        contents.ItemCount.Should().Be(2);
    }

    /// <summary>
    /// Verifies that Create with null itemIds throws ArgumentNullException.
    /// </summary>
    [Test]
    public void Create_WithNullItemIds_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => ContainerContents.Create(null!, 50, 2);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("itemIds");
    }

    /// <summary>
    /// Verifies that Create with negative currency throws ArgumentOutOfRangeException.
    /// </summary>
    [Test]
    public void Create_WithNegativeCurrency_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => ContainerContents.Create(
            new List<string> { "sword-iron" },
            -10,
            2);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("currencyAmount");
    }

    /// <summary>
    /// Verifies that Create with negative tier throws ArgumentOutOfRangeException.
    /// </summary>
    [Test]
    public void Create_WithNegativeTier_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => ContainerContents.Create(
            new List<string> { "sword-iron" },
            50,
            -1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("appliedTier");
    }

    /// <summary>
    /// Verifies that Create with tier greater than 4 throws ArgumentOutOfRangeException.
    /// </summary>
    [Test]
    public void Create_WithTierGreaterThan4_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => ContainerContents.Create(
            new List<string> { "sword-iron" },
            50,
            5);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("appliedTier");
    }

    #endregion

    #region Empty Property Tests

    /// <summary>
    /// Verifies that Empty returns correctly configured empty contents.
    /// </summary>
    [Test]
    public void Empty_ReturnsEmptyContents()
    {
        // Arrange & Act
        var empty = ContainerContents.Empty;

        // Assert
        empty.HasItems.Should().BeFalse();
        empty.HasCurrency.Should().BeFalse();
        empty.HasContents.Should().BeFalse();
        empty.ItemCount.Should().Be(0);
        empty.CurrencyAmount.Should().Be(0);
        empty.AppliedTier.Should().Be(0);
    }

    /// <summary>
    /// Verifies that Empty returns the same instance (singleton pattern).
    /// </summary>
    [Test]
    public void Empty_ReturnsSameInstance()
    {
        // Arrange & Act
        var empty1 = ContainerContents.Empty;
        var empty2 = ContainerContents.Empty;

        // Assert
        empty1.Should().Be(empty2);
    }

    #endregion

    #region Computed Properties Tests

    /// <summary>
    /// Verifies HasItems returns false when ItemIds is empty.
    /// </summary>
    [Test]
    public void HasItems_WhenNoItems_ReturnsFalse()
    {
        // Arrange
        var contents = ContainerContents.Create(
            Array.Empty<string>(),
            100,
            2);

        // Act & Assert
        contents.HasItems.Should().BeFalse();
        contents.HasContents.Should().BeTrue(); // Still has currency
    }

    /// <summary>
    /// Verifies HasCurrency returns false when CurrencyAmount is 0.
    /// </summary>
    [Test]
    public void HasCurrency_WhenNoCurrency_ReturnsFalse()
    {
        // Arrange
        var contents = ContainerContents.Create(
            new List<string> { "sword-iron" },
            0,
            2);

        // Act & Assert
        contents.HasCurrency.Should().BeFalse();
        contents.HasContents.Should().BeTrue(); // Still has items
    }

    /// <summary>
    /// Verifies HasContents returns false when both items and currency are empty.
    /// </summary>
    [Test]
    public void HasContents_WhenBothEmpty_ReturnsFalse()
    {
        // Arrange
        var contents = ContainerContents.Create(
            Array.Empty<string>(),
            0,
            0);

        // Act & Assert
        contents.HasContents.Should().BeFalse();
    }

    #endregion

    #region Convenience Factory Method Tests

    /// <summary>
    /// Verifies CurrencyOnly creates contents with only currency.
    /// </summary>
    [Test]
    public void CurrencyOnly_CreatesContentsWithOnlyCurrency()
    {
        // Arrange & Act
        var contents = ContainerContents.CurrencyOnly(100);

        // Assert
        contents.CurrencyAmount.Should().Be(100);
        contents.HasCurrency.Should().BeTrue();
        contents.HasItems.Should().BeFalse();
        contents.ItemCount.Should().Be(0);
    }

    /// <summary>
    /// Verifies ItemsOnly creates contents with only items.
    /// </summary>
    [Test]
    public void ItemsOnly_CreatesContentsWithOnlyItems()
    {
        // Arrange
        var itemIds = new List<string> { "armor-plate", "helmet-steel" };

        // Act
        var contents = ContainerContents.ItemsOnly(itemIds, 3);

        // Assert
        contents.ItemIds.Should().BeEquivalentTo(itemIds);
        contents.AppliedTier.Should().Be(3);
        contents.HasItems.Should().BeTrue();
        contents.HasCurrency.Should().BeFalse();
        contents.CurrencyAmount.Should().Be(0);
    }

    #endregion

    #region ToString Tests

    /// <summary>
    /// Verifies ToString produces expected output format.
    /// </summary>
    [Test]
    public void ToString_WithContents_ReturnsFormattedString()
    {
        // Arrange
        var contents = ContainerContents.Create(
            new List<string> { "sword-iron", "shield-wood" },
            75,
            2);

        // Act
        var result = contents.ToString();

        // Assert
        result.Should().Be("Contents: 2 item(s), 75 gold, Tier 2");
    }

    /// <summary>
    /// Verifies ToString for empty contents.
    /// </summary>
    [Test]
    public void ToString_WhenEmpty_ReturnsZeroValues()
    {
        // Arrange & Act
        var result = ContainerContents.Empty.ToString();

        // Assert
        result.Should().Be("Contents: 0 item(s), 0 gold, Tier 0");
    }

    #endregion

    #region Boundary Value Tests

    /// <summary>
    /// Verifies tier boundary at 0 (minimum valid tier).
    /// </summary>
    [Test]
    public void Create_WithTierZero_Succeeds()
    {
        // Arrange & Act
        var contents = ContainerContents.Create(
            new List<string> { "item" },
            0,
            0);

        // Assert
        contents.AppliedTier.Should().Be(0);
    }

    /// <summary>
    /// Verifies tier boundary at 4 (maximum valid tier - MythForged).
    /// </summary>
    [Test]
    public void Create_WithTierFour_Succeeds()
    {
        // Arrange & Act
        var contents = ContainerContents.Create(
            new List<string> { "legendary-sword" },
            500,
            4);

        // Assert
        contents.AppliedTier.Should().Be(4);
    }

    #endregion
}
