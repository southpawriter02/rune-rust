using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for <see cref="ForagingService"/>.
/// </summary>
/// <remarks>
/// Tests cover:
/// <list type="bullet">
///   <item><description>Duration bonus calculation</description></item>
///   <item><description>Yield calculation based on successes</description></item>
///   <item><description>Cache discovery mechanics</description></item>
///   <item><description>Biome loot table retrieval</description></item>
///   <item><description>Target DC retrieval</description></item>
///   <item><description>ForagingContext and ForagingResult value objects</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class ForagingServiceTests
{
    private SkillCheckService _skillCheckService = null!;
    private DiceService _diceService = null!;
    private ILogger<ForagingService> _logger = null!;
    private ForagingService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _skillCheckService = CreateMockSkillCheckService();
        _diceService = CreateDiceService();
        _logger = Substitute.For<ILogger<ForagingService>>();
        _sut = new ForagingService(_skillCheckService, _diceService, _logger);
    }

    #region Duration Bonus Tests

    [Test]
    public void GetDurationBonus_ReturnsCorrectBonusForEachDuration()
    {
        // Assert - Quick: +0, Thorough: +2, Complete: +4
        _sut.GetDurationBonus(SearchDuration.Quick).Should().Be(0);
        _sut.GetDurationBonus(SearchDuration.Thorough).Should().Be(2);
        _sut.GetDurationBonus(SearchDuration.Complete).Should().Be(4);
    }

    [Test]
    public void GetDurationTimeMinutes_ReturnsCorrectTimeForEachDuration()
    {
        // Assert - Quick: 10 min, Thorough: 60 min, Complete: 240 min
        _sut.GetDurationTimeMinutes(SearchDuration.Quick).Should().Be(10);
        _sut.GetDurationTimeMinutes(SearchDuration.Thorough).Should().Be(60);
        _sut.GetDurationTimeMinutes(SearchDuration.Complete).Should().Be(240);
    }

    [Test]
    public void GetDurationDescription_ReturnsNameAndDescriptionForEachDuration()
    {
        // Act
        var (quickName, quickDesc) = _sut.GetDurationDescription(SearchDuration.Quick);
        var (thoroughName, thoroughDesc) = _sut.GetDurationDescription(SearchDuration.Thorough);
        var (completeName, completeDesc) = _sut.GetDurationDescription(SearchDuration.Complete);

        // Assert
        quickName.Should().Be("Quick Search");
        quickDesc.Should().Contain("10-minute");

        thoroughName.Should().Be("Thorough Search");
        thoroughDesc.Should().Contain("1-hour");

        completeName.Should().Be("Complete Search");
        completeDesc.Should().Contain("4-hour");
    }

    #endregion

    #region Yield Calculation Tests

    [Test]
    public void CalculateYields_WithZeroSuccesses_ReturnsNothing()
    {
        // Act
        var (scrap, rations, components) = _sut.CalculateYields(0, "wasteland");

        // Assert
        scrap.Should().Be(0);
        rations.Should().Be(0);
        components.Should().Be(0);
    }

    [Test]
    public void CalculateYields_WithOneSuccess_ReturnsNothing()
    {
        // Act
        var (scrap, rations, components) = _sut.CalculateYields(1, "wasteland");

        // Assert
        scrap.Should().Be(0);
        rations.Should().Be(0);
        components.Should().Be(0);
    }

    [Test]
    public void CalculateYields_WithTwoSuccesses_ReturnsOnlyScrap()
    {
        // Act - 2 successes yields 2d10 scrap
        var (scrap, rations, components) = _sut.CalculateYields(2, "wasteland");

        // Assert - Should have scrap (2d10 = 2-20), no rations or components
        scrap.Should().BeGreaterThan(0);
        rations.Should().Be(0);
        components.Should().Be(0);
    }

    [Test]
    public void CalculateYields_WithFourSuccesses_ReturnsScrapAndRations()
    {
        // Act - 4 successes yields 3d10 scrap + 1d6 rations
        var (scrap, rations, components) = _sut.CalculateYields(4, "wasteland");

        // Assert - Should have scrap (3d10 = 3-30) and rations (1d6 = 1-6), no components
        scrap.Should().BeGreaterThan(0);
        rations.Should().BeGreaterThan(0);
        components.Should().Be(0);
    }

    [Test]
    public void CalculateYields_WithSixSuccesses_ReturnsScrapRationsAndOneComponent()
    {
        // Act - 6 successes yields 4d10 scrap + 1d6 rations + 1 component
        var (scrap, rations, components) = _sut.CalculateYields(6, "wasteland");

        // Assert
        scrap.Should().BeGreaterThan(0);
        rations.Should().BeGreaterThan(0);
        components.Should().Be(1); // Fixed component count at this tier
    }

    [Test]
    public void CalculateYields_WithEightOrMoreSuccesses_ReturnsMaximumYields()
    {
        // Act - 8+ successes yields 5d10 scrap + 2d6 rations + 1d4 components
        var (scrap, rations, components) = _sut.CalculateYields(8, "wasteland");

        // Assert
        scrap.Should().BeGreaterThan(0);
        rations.Should().BeGreaterThan(0);
        components.Should().BeGreaterThan(0); // Rolled 1d4, at least 1
    }

    [Test]
    public void CalculateYields_WithVeryHighSuccesses_ClampsToMaximumTier()
    {
        // Act - Even 20 successes should use the 8+ tier
        var (scrap, rations, components) = _sut.CalculateYields(20, "wasteland");

        // Assert - Should still return valid yields (not crash)
        scrap.Should().BeGreaterThan(0);
        rations.Should().BeGreaterThan(0);
        components.Should().BeGreaterThan(0);
    }

    #endregion

    #region Cache Discovery Tests

    [Test]
    public void CheckForCache_WithTenInRolls_ReturnsTrue()
    {
        // Arrange
        var rollsWithTen = new[] { 3, 7, 10, 2, 5 };

        // Act
        var result = _sut.CheckForCache(rollsWithTen);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public void CheckForCache_WithNoTen_ReturnsFalse()
    {
        // Arrange
        var rollsWithoutTen = new[] { 3, 7, 9, 2, 5 };

        // Act
        var result = _sut.CheckForCache(rollsWithoutTen);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void CheckForCache_WithMultipleTens_ReturnsTrue()
    {
        // Arrange
        var rollsWithMultipleTens = new[] { 10, 3, 10, 10, 5 };

        // Act
        var result = _sut.CheckForCache(rollsWithMultipleTens);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public void CheckForCache_WithEmptyRolls_ReturnsFalse()
    {
        // Arrange
        var emptyRolls = Array.Empty<int>();

        // Act
        var result = _sut.CheckForCache(emptyRolls);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void GenerateCacheContents_AlwaysReturnsMarks()
    {
        // Act
        var (marks, _) = _sut.GenerateCacheContents("wasteland");

        // Assert - Marks should be 1d100 = 1-100
        marks.Should().BeGreaterThan(0);
        marks.Should().BeLessThanOrEqualTo(100);
    }

    #endregion

    #region Target DC Tests

    [Test]
    public void GetTargetBaseDc_ReturnsCorrectDcForEachTarget()
    {
        // Assert - CommonSalvage: 10, UsefulSupplies: 14, ValuableComponents: 18, HiddenCache: 22
        _sut.GetTargetBaseDc(ForageTarget.CommonSalvage).Should().Be(10);
        _sut.GetTargetBaseDc(ForageTarget.UsefulSupplies).Should().Be(14);
        _sut.GetTargetBaseDc(ForageTarget.ValuableComponents).Should().Be(18);
        _sut.GetTargetBaseDc(ForageTarget.HiddenCache).Should().Be(22);
    }

    [Test]
    public void GetTargetDescription_ReturnsNameAndDescriptionForEachTarget()
    {
        // Act
        var (salvageName, salvageDesc) = _sut.GetTargetDescription(ForageTarget.CommonSalvage);
        var (suppliesName, _) = _sut.GetTargetDescription(ForageTarget.UsefulSupplies);
        var (componentsName, _) = _sut.GetTargetDescription(ForageTarget.ValuableComponents);
        var (cacheName, _) = _sut.GetTargetDescription(ForageTarget.HiddenCache);

        // Assert
        salvageName.Should().Be("Common Salvage");
        salvageDesc.Should().Contain("DC 10");

        suppliesName.Should().Be("Useful Supplies");
        componentsName.Should().Be("Valuable Components");
        cacheName.Should().Be("Hidden Cache");
    }

    #endregion

    #region Biome Loot Table Tests

    [Test]
    public void GetBiomeLootTable_ReturnsCorrectTableForKnownBiome()
    {
        // Act
        var industrialTable = _sut.GetBiomeLootTable("industrial-ruins");
        var swampTable = _sut.GetBiomeLootTable("swamp");

        // Assert
        industrialTable.Should().NotBeEmpty();
        industrialTable.Should().Contain("Corroded Medkit");

        swampTable.Should().NotBeEmpty();
        swampTable.Should().Contain("Medicinal Moss");
    }

    [Test]
    public void GetBiomeLootTable_ReturnsDefaultForUnknownBiome()
    {
        // Act
        var table = _sut.GetBiomeLootTable("unknown-biome");

        // Assert
        table.Should().NotBeEmpty();
        table.Should().Contain("Mysterious Object");
    }

    [Test]
    public void GetBiomeLootTable_IsCaseInsensitive()
    {
        // Act
        var lowerCase = _sut.GetBiomeLootTable("industrial-ruins");
        var upperCase = _sut.GetBiomeLootTable("INDUSTRIAL-RUINS");
        var mixedCase = _sut.GetBiomeLootTable("Industrial-Ruins");

        // Assert
        lowerCase.Should().BeEquivalentTo(upperCase);
        lowerCase.Should().BeEquivalentTo(mixedCase);
    }

    [Test]
    public void GetRandomBiomeItem_ReturnsItemFromBiome()
    {
        // Act
        var item = _sut.GetRandomBiomeItem("industrial-ruins");

        // Assert
        item.Should().NotBeNullOrEmpty();
        _sut.GetBiomeLootTable("industrial-ruins").Should().Contain(item!);
    }

    #endregion

    #region Economy Tests

    [Test]
    public void CalculateEstimatedValue_CalculatesCorrectly()
    {
        // Arrange - 10 scrap (×1) + 5 rations (×5) + 2 components (×20)
        // Expected: 10 + 25 + 40 = 75

        // Act
        var value = _sut.CalculateEstimatedValue(10, 5, 2);

        // Assert
        value.Should().Be(75);
    }

    [Test]
    public void CalculateEstimatedValue_WithZeros_ReturnsZero()
    {
        // Act
        var value = _sut.CalculateEstimatedValue(0, 0, 0);

        // Assert
        value.Should().Be(0);
    }

    [Test]
    public void GetExpectedValuePerHour_ReturnsCorrectEstimates()
    {
        // Assert - Quick: ~48/hr, Thorough: ~20/hr, Complete: ~15/hr
        _sut.GetExpectedValuePerHour(SearchDuration.Quick).Should().BeApproximately(48, 1);
        _sut.GetExpectedValuePerHour(SearchDuration.Thorough).Should().BeApproximately(20, 1);
        _sut.GetExpectedValuePerHour(SearchDuration.Complete).Should().BeApproximately(15, 1);
    }

    #endregion

    #region ForagingContext Value Object Tests

    [Test]
    public void ForagingContext_CalculatesCorrectTimeMinutes()
    {
        // Arrange
        var quickContext = ForagingContext.CreateQuick("player-1", "wasteland");
        var thoroughContext = ForagingContext.CreateThorough("player-1", "wasteland");
        var completeContext = ForagingContext.CreateComplete("player-1", "wasteland");

        // Assert
        quickContext.TimeMinutes.Should().Be(10);
        thoroughContext.TimeMinutes.Should().Be(60);
        completeContext.TimeMinutes.Should().Be(240);
    }

    [Test]
    public void ForagingContext_CalculatesCorrectDurationBonusDice()
    {
        // Arrange
        var quickContext = ForagingContext.CreateQuick("player-1", "wasteland");
        var thoroughContext = ForagingContext.CreateThorough("player-1", "wasteland");
        var completeContext = ForagingContext.CreateComplete("player-1", "wasteland");

        // Assert
        quickContext.DurationBonusDice.Should().Be(0);
        thoroughContext.DurationBonusDice.Should().Be(2);
        completeContext.DurationBonusDice.Should().Be(4);
    }

    [Test]
    public void ForagingContext_CalculatesCorrectBaseDc()
    {
        // Arrange
        var salvageContext = new ForagingContext("player-1", "wasteland", SearchDuration.Quick, ForageTarget.CommonSalvage);
        var suppliesContext = new ForagingContext("player-1", "wasteland", SearchDuration.Quick, ForageTarget.UsefulSupplies);
        var componentsContext = new ForagingContext("player-1", "wasteland", SearchDuration.Quick, ForageTarget.ValuableComponents);
        var cacheContext = new ForagingContext("player-1", "wasteland", SearchDuration.Quick, ForageTarget.HiddenCache);

        // Assert
        salvageContext.BaseDc.Should().Be(10);
        suppliesContext.BaseDc.Should().Be(14);
        componentsContext.BaseDc.Should().Be(18);
        cacheContext.BaseDc.Should().Be(22);
    }

    [Test]
    public void ForagingContext_CalculatesTotalDiceModifier()
    {
        // Arrange - Thorough (+2) + Equipment (+1) - Exhaustion (-2) = +1
        var context = new ForagingContext(
            CharacterId: "player-1",
            BiomeId: "wasteland",
            SearchDuration: SearchDuration.Thorough,
            ForageTarget: ForageTarget.CommonSalvage,
            EquipmentBonus: 1,
            PreviousSearches: 2);

        // Assert
        context.TotalDiceModifier.Should().Be(1);
    }

    [Test]
    public void ForagingContext_ExhaustionPenaltyCapsAtThree()
    {
        // Arrange - 5 previous searches should cap at -3
        var context = new ForagingContext(
            CharacterId: "player-1",
            BiomeId: "wasteland",
            SearchDuration: SearchDuration.Quick,
            ForageTarget: ForageTarget.CommonSalvage,
            EquipmentBonus: 0,
            PreviousSearches: 5);

        // Assert
        context.ExhaustionPenalty.Should().Be(3);
    }

    [Test]
    public void ForagingContext_ToSkillContext_CreatesValidContext()
    {
        // Arrange
        var context = ForagingContext.CreateThorough("player-1", "industrial-ruins", ForageTarget.UsefulSupplies, 1);

        // Act
        var skillContext = context.ToSkillContext();

        // Assert
        skillContext.Should().NotBeNull();
        // The skill context should have situational modifiers for duration and equipment
        skillContext.SituationalModifiers.Should().NotBeEmpty();
    }

    #endregion

    #region ForagingResult Value Object Tests

    [Test]
    public void ForagingResult_FoundAnything_ReturnsTrueWhenResourcesFound()
    {
        // Arrange
        var resultWithScrap = new ForagingResult(2, 10, 0, 0, false, 0, null, Array.Empty<string>(), TimeSpan.FromMinutes(10), "");
        var resultWithCache = new ForagingResult(1, 0, 0, 0, true, 50, null, Array.Empty<string>(), TimeSpan.FromMinutes(10), "");
        var emptyResult = new ForagingResult(0, 0, 0, 0, false, 0, null, Array.Empty<string>(), TimeSpan.FromMinutes(10), "");

        // Assert
        resultWithScrap.FoundAnything.Should().BeTrue();
        resultWithCache.FoundAnything.Should().BeTrue();
        emptyResult.FoundAnything.Should().BeFalse();
    }

    [Test]
    public void ForagingResult_SuccessTier_ReturnsCorrectDescription()
    {
        // Arrange
        var tier0 = new ForagingResult(0, 0, 0, 0, false, 0, null, Array.Empty<string>(), TimeSpan.FromMinutes(10), "");
        var tier2 = new ForagingResult(2, 10, 0, 0, false, 0, null, Array.Empty<string>(), TimeSpan.FromMinutes(10), "");
        var tier4 = new ForagingResult(4, 15, 3, 0, false, 0, null, Array.Empty<string>(), TimeSpan.FromMinutes(60), "");
        var tier8 = new ForagingResult(8, 25, 7, 3, false, 0, null, Array.Empty<string>(), TimeSpan.FromMinutes(240), "");

        // Assert
        tier0.SuccessTier.Should().Be("Nothing Found");
        tier2.SuccessTier.Should().Be("Meager Haul");
        tier4.SuccessTier.Should().Be("Decent Finds");
        tier8.SuccessTier.Should().Be("Excellent Haul");
    }

    [Test]
    public void ForagingResult_EstimatedValue_CalculatesCorrectly()
    {
        // Arrange - 10 scrap (×1) + 5 rations (×5) + 2 components (×20) + 50 cache marks
        // Expected: 10 + 25 + 40 + 50 = 125
        var result = new ForagingResult(5, 10, 5, 2, true, 50, "Test Item", Array.Empty<string>(), TimeSpan.FromMinutes(60), "");

        // Assert
        result.EstimatedValue.Should().Be(125);
    }

    [Test]
    public void ForagingResult_IsCriticalSuccess_WhenNetSuccessesAtLeastFive()
    {
        // Arrange
        var criticalResult = new ForagingResult(5, 20, 5, 1, false, 0, null, new[] { "Bonus Item" }, TimeSpan.FromMinutes(60), "");
        var normalResult = new ForagingResult(4, 15, 3, 0, false, 0, null, Array.Empty<string>(), TimeSpan.FromMinutes(60), "");

        // Assert
        criticalResult.IsCriticalSuccess.Should().BeTrue();
        normalResult.IsCriticalSuccess.Should().BeFalse();
    }

    [Test]
    public void ForagingResult_Empty_CreatesEmptyResult()
    {
        // Act
        var result = ForagingResult.Empty(TimeSpan.FromMinutes(10), "Failed roll");

        // Assert
        result.SuccessLevel.Should().Be(0);
        result.ScrapYield.Should().Be(0);
        result.RationsYield.Should().Be(0);
        result.ComponentsYield.Should().Be(0);
        result.CacheFound.Should().BeFalse();
        result.FoundAnything.Should().BeFalse();
    }

    [Test]
    public void ForagingResult_ToDisplayString_FormatsCorrectly()
    {
        // Arrange
        var result = new ForagingResult(4, 15, 3, 1, true, 50, "Test Item", Array.Empty<string>(), TimeSpan.FromMinutes(60), "");

        // Act
        var display = result.ToDisplayString();

        // Assert
        display.Should().Contain("15 scrap");
        display.Should().Contain("3 rations");
        display.Should().Contain("1 component");
        display.Should().Contain("CACHE");
        display.Should().Contain("50 Marks");
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Creates a mock SkillCheckService for testing.
    /// </summary>
    private static SkillCheckService CreateMockSkillCheckService()
    {
        var seededRandom = new Random(42);
        var diceLogger = Substitute.For<ILogger<DiceService>>();
#pragma warning disable CS0618 // Type or member is obsolete
        var diceService = new DiceService(diceLogger, seededRandom);
#pragma warning restore CS0618
        var configProvider = Substitute.For<IGameConfigurationProvider>();
        var logger = Substitute.For<ILogger<SkillCheckService>>();

        return new SkillCheckService(diceService, configProvider, logger);
    }

    /// <summary>
    /// Creates a DiceService with deterministic random for testing.
    /// </summary>
    private static DiceService CreateDiceService()
    {
        var seededRandom = new Random(42);
        var diceLogger = Substitute.For<ILogger<DiceService>>();
#pragma warning disable CS0618 // Type or member is obsolete
        return new DiceService(diceLogger, seededRandom);
#pragma warning restore CS0618
    }

    #endregion
}
