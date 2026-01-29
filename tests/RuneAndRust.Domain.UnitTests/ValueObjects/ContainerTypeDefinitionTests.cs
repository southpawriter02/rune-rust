using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="ContainerTypeDefinition"/> value object.
/// </summary>
/// <remarks>
/// Tests cover factory method validation, computed properties, and edge cases
/// for container type specifications.
/// </remarks>
[TestFixture]
public class ContainerTypeDefinitionTests
{
    #region Create Factory Method Tests

    /// <summary>
    /// Verifies that Create with valid parameters creates a correctly configured definition.
    /// </summary>
    [Test]
    public void Create_WithValidParameters_CreatesDefinition()
    {
        // Arrange & Act
        var definition = ContainerTypeDefinition.Create(
            type: ContainerType.SmallChest,
            minItems: 1,
            maxItems: 2,
            minTier: 0,
            maxTier: 1,
            minCurrency: 10,
            maxCurrency: 30);

        // Assert
        definition.Type.Should().Be(ContainerType.SmallChest);
        definition.MinItems.Should().Be(1);
        definition.MaxItems.Should().Be(2);
        definition.MinTier.Should().Be(0);
        definition.MaxTier.Should().Be(1);
        definition.MinCurrency.Should().Be(10);
        definition.MaxCurrency.Should().Be(30);
        definition.AwardsCurrency.Should().BeTrue();
        definition.CanBeEmpty.Should().BeFalse();
        definition.HasCategoryFilter.Should().BeFalse();
        definition.RequiresDiscovery.Should().BeFalse();
        definition.HasMythForgedChance.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that MaxItems less than MinItems throws ArgumentOutOfRangeException.
    /// </summary>
    [Test]
    public void Create_WithMaxItemsLessThanMinItems_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => ContainerTypeDefinition.Create(
            type: ContainerType.SmallChest,
            minItems: 5,
            maxItems: 2,
            minTier: 0,
            maxTier: 1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("maxItems");
    }

    /// <summary>
    /// Verifies that negative MinItems throws ArgumentOutOfRangeException.
    /// </summary>
    [Test]
    public void Create_WithNegativeMinItems_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => ContainerTypeDefinition.Create(
            type: ContainerType.Corpse,
            minItems: -1,
            maxItems: 2,
            minTier: 0,
            maxTier: 1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("minItems");
    }

    /// <summary>
    /// Verifies that tier greater than 4 throws ArgumentOutOfRangeException.
    /// </summary>
    [Test]
    public void Create_WithMaxTierGreaterThan4_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => ContainerTypeDefinition.Create(
            type: ContainerType.BossChest,
            minItems: 4,
            maxItems: 6,
            minTier: 3,
            maxTier: 5);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("maxTier");
    }

    /// <summary>
    /// Verifies that MaxCurrency less than MinCurrency throws ArgumentOutOfRangeException.
    /// </summary>
    [Test]
    public void Create_WithMaxCurrencyLessThanMinCurrency_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => ContainerTypeDefinition.Create(
            type: ContainerType.LargeChest,
            minItems: 3,
            maxItems: 5,
            minTier: 2,
            maxTier: 3,
            minCurrency: 100,
            maxCurrency: 50);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("maxCurrency")
            .WithMessage("*MaxCurrency (50) must be >= MinCurrency (100)*");
    }

    /// <summary>
    /// Verifies that DiscoveryDC less than 1 throws ArgumentOutOfRangeException.
    /// </summary>
    [Test]
    public void Create_WithInvalidDiscoveryDC_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => ContainerTypeDefinition.Create(
            type: ContainerType.HiddenCache,
            minItems: 1,
            maxItems: 3,
            minTier: 1,
            maxTier: 3,
            discoveryDC: 0);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("discoveryDC");
    }

    #endregion

    #region Computed Properties Tests

    /// <summary>
    /// Verifies CanBeEmpty returns true when MinItems is 0.
    /// </summary>
    [Test]
    public void CanBeEmpty_WhenMinItemsIsZero_ReturnsTrue()
    {
        // Arrange
        var definition = ContainerTypeDefinition.Create(
            type: ContainerType.Corpse,
            minItems: 0,
            maxItems: 2,
            minTier: 0,
            maxTier: 1);

        // Act & Assert
        definition.CanBeEmpty.Should().BeTrue();
    }

    /// <summary>
    /// Verifies CanBeEmpty returns true when MayBeEmpty flag is set.
    /// </summary>
    [Test]
    public void CanBeEmpty_WhenMayBeEmptyFlagSet_ReturnsTrue()
    {
        // Arrange
        var definition = ContainerTypeDefinition.Create(
            type: ContainerType.Corpse,
            minItems: 1,
            maxItems: 2,
            minTier: 0,
            maxTier: 1,
            flags: ContainerFlags.MayBeEmpty);

        // Act & Assert
        definition.CanBeEmpty.Should().BeTrue();
    }

    /// <summary>
    /// Verifies HasMythForgedChance returns true when flag is set.
    /// </summary>
    [Test]
    public void HasMythForgedChance_WhenFlagSet_ReturnsTrue()
    {
        // Arrange
        var definition = ContainerTypeDefinition.Create(
            type: ContainerType.BossChest,
            minItems: 4,
            maxItems: 6,
            minTier: 3,
            maxTier: 4,
            flags: ContainerFlags.MythForgedChance);

        // Act & Assert
        definition.HasMythForgedChance.Should().BeTrue();
    }

    /// <summary>
    /// Verifies RequiresDiscovery returns true when DiscoveryDC is set.
    /// </summary>
    [Test]
    public void RequiresDiscovery_WhenDiscoveryDCSet_ReturnsTrue()
    {
        // Arrange
        var definition = ContainerTypeDefinition.Create(
            type: ContainerType.HiddenCache,
            minItems: 1,
            maxItems: 3,
            minTier: 1,
            maxTier: 3,
            discoveryDC: 14);

        // Act & Assert
        definition.RequiresDiscovery.Should().BeTrue();
    }

    /// <summary>
    /// Verifies HasCategoryFilter returns true when filter is set.
    /// </summary>
    [Test]
    public void HasCategoryFilter_WhenFilterSet_ReturnsTrue()
    {
        // Arrange
        var definition = ContainerTypeDefinition.Create(
            type: ContainerType.WeaponRack,
            minItems: 1,
            maxItems: 2,
            minTier: 1,
            maxTier: 3,
            itemCategoryFilter: "weapons");

        // Act & Assert
        definition.HasCategoryFilter.Should().BeTrue();
        definition.ItemCategoryFilter.Should().Be("weapons");
    }

    /// <summary>
    /// Verifies ItemCategoryFilter is normalized to lowercase.
    /// </summary>
    [Test]
    public void Create_WithMixedCaseFilter_NormalizesToLowercase()
    {
        // Arrange & Act
        var definition = ContainerTypeDefinition.Create(
            type: ContainerType.ArmorStand,
            minItems: 1,
            maxItems: 1,
            minTier: 1,
            maxTier: 3,
            itemCategoryFilter: "ARMOR");

        // Assert
        definition.ItemCategoryFilter.Should().Be("armor");
    }

    /// <summary>
    /// Verifies AwardsCurrency returns false when currency values are null.
    /// </summary>
    [Test]
    public void AwardsCurrency_WhenCurrencyNull_ReturnsFalse()
    {
        // Arrange
        var definition = ContainerTypeDefinition.Create(
            type: ContainerType.WeaponRack,
            minItems: 1,
            maxItems: 2,
            minTier: 1,
            maxTier: 3);

        // Act & Assert
        definition.AwardsCurrency.Should().BeFalse();
        definition.MinCurrency.Should().BeNull();
        definition.MaxCurrency.Should().BeNull();
    }

    #endregion

    #region Validation Method Tests

    /// <summary>
    /// Verifies IsValidItemCount returns true for values within range.
    /// </summary>
    [Test]
    public void IsValidItemCount_WithValueInRange_ReturnsTrue()
    {
        // Arrange
        var definition = ContainerTypeDefinition.Create(
            type: ContainerType.MediumChest,
            minItems: 2,
            maxItems: 4,
            minTier: 1,
            maxTier: 2);

        // Act & Assert
        definition.IsValidItemCount(2).Should().BeTrue();
        definition.IsValidItemCount(3).Should().BeTrue();
        definition.IsValidItemCount(4).Should().BeTrue();
    }

    /// <summary>
    /// Verifies IsValidItemCount returns false for values outside range.
    /// </summary>
    [Test]
    public void IsValidItemCount_WithValueOutOfRange_ReturnsFalse()
    {
        // Arrange
        var definition = ContainerTypeDefinition.Create(
            type: ContainerType.MediumChest,
            minItems: 2,
            maxItems: 4,
            minTier: 1,
            maxTier: 2);

        // Act & Assert
        definition.IsValidItemCount(1).Should().BeFalse();
        definition.IsValidItemCount(5).Should().BeFalse();
    }

    /// <summary>
    /// Verifies IsValidTier returns correct results for boundary values.
    /// </summary>
    [Test]
    public void IsValidTier_WithBoundaryValues_ReturnsCorrectResult()
    {
        // Arrange
        var definition = ContainerTypeDefinition.Create(
            type: ContainerType.LargeChest,
            minItems: 3,
            maxItems: 5,
            minTier: 2,
            maxTier: 3);

        // Act & Assert
        definition.IsValidTier(1).Should().BeFalse();
        definition.IsValidTier(2).Should().BeTrue();
        definition.IsValidTier(3).Should().BeTrue();
        definition.IsValidTier(4).Should().BeFalse();
    }

    /// <summary>
    /// Verifies IsValidCurrency returns true for values within range.
    /// </summary>
    [Test]
    public void IsValidCurrency_WithValueInRange_ReturnsTrue()
    {
        // Arrange
        var definition = ContainerTypeDefinition.Create(
            type: ContainerType.SmallChest,
            minItems: 1,
            maxItems: 2,
            minTier: 0,
            maxTier: 1,
            minCurrency: 10,
            maxCurrency: 30);

        // Act & Assert
        definition.IsValidCurrency(10).Should().BeTrue();
        definition.IsValidCurrency(20).Should().BeTrue();
        definition.IsValidCurrency(30).Should().BeTrue();
    }

    /// <summary>
    /// Verifies IsValidCurrency returns false for values outside range.
    /// </summary>
    [Test]
    public void IsValidCurrency_WithValueOutOfRange_ReturnsFalse()
    {
        // Arrange
        var definition = ContainerTypeDefinition.Create(
            type: ContainerType.SmallChest,
            minItems: 1,
            maxItems: 2,
            minTier: 0,
            maxTier: 1,
            minCurrency: 10,
            maxCurrency: 30);

        // Act & Assert
        definition.IsValidCurrency(5).Should().BeFalse();
        definition.IsValidCurrency(35).Should().BeFalse();
    }

    /// <summary>
    /// Verifies IsValidCurrency for containers with no currency.
    /// </summary>
    [Test]
    public void IsValidCurrency_WhenNoCurrency_ReturnsCorrectResult()
    {
        // Arrange
        var definition = ContainerTypeDefinition.Create(
            type: ContainerType.WeaponRack,
            minItems: 1,
            maxItems: 2,
            minTier: 1,
            maxTier: 3);

        // Act & Assert
        definition.IsValidCurrency(0).Should().BeTrue();
        definition.IsValidCurrency(10).Should().BeFalse();
    }

    #endregion

    #region ToString Tests

    /// <summary>
    /// Verifies ToString produces expected output format.
    /// </summary>
    [Test]
    public void ToString_WithAllProperties_ReturnsFormattedString()
    {
        // Arrange
        var definition = ContainerTypeDefinition.Create(
            type: ContainerType.HiddenCache,
            minItems: 1,
            maxItems: 3,
            minTier: 1,
            maxTier: 3,
            minCurrency: 50,
            maxCurrency: 200,
            discoveryDC: 14,
            flags: ContainerFlags.RequiresDiscovery);

        // Act
        var result = definition.ToString();

        // Assert
        result.Should().Contain("HiddenCache");
        result.Should().Contain("Items=1-3");
        result.Should().Contain("Tier=1-3");
        result.Should().Contain("Currency=50-200");
        result.Should().Contain("DC=14");
        result.Should().Contain("Flags=RequiresDiscovery");
    }

    /// <summary>
    /// Verifies ToString with minimal properties.
    /// </summary>
    [Test]
    public void ToString_WithMinimalProperties_ReturnsBasicFormat()
    {
        // Arrange
        var definition = ContainerTypeDefinition.Create(
            type: ContainerType.ArmorStand,
            minItems: 1,
            maxItems: 1,
            minTier: 1,
            maxTier: 3,
            itemCategoryFilter: "armor");

        // Act
        var result = definition.ToString();

        // Assert
        result.Should().Be("ArmorStand: Items=1-1, Tier=1-3, Filter=armor");
    }

    #endregion

    #region CreateSimple Factory Method Tests

    /// <summary>
    /// Verifies CreateSimple factory method creates definition correctly.
    /// </summary>
    [Test]
    public void CreateSimple_WithValidRanges_CreatesDefinition()
    {
        // Arrange & Act
        var definition = ContainerTypeDefinition.CreateSimple(
            type: ContainerType.Locker,
            itemRange: (1, 3),
            tierRange: (0, 2));

        // Assert
        definition.Type.Should().Be(ContainerType.Locker);
        definition.MinItems.Should().Be(1);
        definition.MaxItems.Should().Be(3);
        definition.MinTier.Should().Be(0);
        definition.MaxTier.Should().Be(2);
        definition.AwardsCurrency.Should().BeFalse();
        definition.HasCategoryFilter.Should().BeFalse();
    }

    #endregion

    #region Computed Average Tests

    /// <summary>
    /// Verifies AverageItemCount calculation.
    /// </summary>
    [Test]
    public void AverageItemCount_WithValidRange_ReturnsCorrectAverage()
    {
        // Arrange
        var definition = ContainerTypeDefinition.Create(
            type: ContainerType.MediumChest,
            minItems: 2,
            maxItems: 4,
            minTier: 1,
            maxTier: 2);

        // Act & Assert
        definition.AverageItemCount.Should().Be(3.0);
    }

    /// <summary>
    /// Verifies AverageCurrency calculation.
    /// </summary>
    [Test]
    public void AverageCurrency_WithCurrency_ReturnsCorrectAverage()
    {
        // Arrange
        var definition = ContainerTypeDefinition.Create(
            type: ContainerType.SmallChest,
            minItems: 1,
            maxItems: 2,
            minTier: 0,
            maxTier: 1,
            minCurrency: 10,
            maxCurrency: 30);

        // Act & Assert
        definition.AverageCurrency.Should().Be(20.0);
    }

    /// <summary>
    /// Verifies AverageCurrency returns 0 when no currency.
    /// </summary>
    [Test]
    public void AverageCurrency_WithoutCurrency_ReturnsZero()
    {
        // Arrange
        var definition = ContainerTypeDefinition.Create(
            type: ContainerType.WeaponRack,
            minItems: 1,
            maxItems: 2,
            minTier: 1,
            maxTier: 3);

        // Act & Assert
        definition.AverageCurrency.Should().Be(0);
    }

    #endregion
}
