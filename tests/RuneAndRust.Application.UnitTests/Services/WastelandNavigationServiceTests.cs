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
/// Unit tests for <see cref="WastelandNavigationService"/>.
/// </summary>
/// <remarks>
/// Tests cover:
/// <list type="bullet">
///   <item><description>Terrain DC calculation</description></item>
///   <item><description>Dice modifier calculation (compass, familiar territory, weather, night)</description></item>
///   <item><description>Compass effectiveness in glitched terrain</description></item>
///   <item><description>Navigation outcomes and time multipliers</description></item>
///   <item><description>Dangerous area type rolling</description></item>
///   <item><description>NavigationContext and NavigationResult value objects</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class WastelandNavigationServiceTests
{
    private SkillCheckService _skillCheckService = null!;
    private DiceService _diceService = null!;
    private IFumbleConsequenceService _fumbleConsequenceService = null!;
    private ILogger<WastelandNavigationService> _logger = null!;
    private WastelandNavigationService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _skillCheckService = CreateMockSkillCheckService();
        _diceService = CreateDiceService();
        _fumbleConsequenceService = Substitute.For<IFumbleConsequenceService>();
        _logger = Substitute.For<ILogger<WastelandNavigationService>>();
        _sut = new WastelandNavigationService(_skillCheckService, _diceService, _fumbleConsequenceService, _logger);
    }

    #region Terrain DC Tests

    [Test]
    [TestCase(NavigationTerrainType.OpenWasteland, 8)]
    [TestCase(NavigationTerrainType.ModerateRuins, 12)]
    [TestCase(NavigationTerrainType.DenseRuins, 16)]
    [TestCase(NavigationTerrainType.Labyrinthine, 20)]
    [TestCase(NavigationTerrainType.GlitchedLabyrinth, 24)]
    public void CalculateBaseDc_ReturnsCorrectDcForEachTerrainType(NavigationTerrainType terrainType, int expectedDc)
    {
        // Act
        var dc = _sut.CalculateBaseDc(terrainType);

        // Assert
        dc.Should().Be(expectedDc);
    }

    [Test]
    public void GetTerrainDescription_ReturnsDescriptionForEachTerrain()
    {
        // Act
        var (openName, openDesc) = _sut.GetTerrainDescription(NavigationTerrainType.OpenWasteland);
        var (denseName, denseDesc) = _sut.GetTerrainDescription(NavigationTerrainType.DenseRuins);
        var (glitchedName, glitchedDesc) = _sut.GetTerrainDescription(NavigationTerrainType.GlitchedLabyrinth);

        // Assert
        openName.Should().Be("Open Wasteland");
        openDesc.Should().Contain("DC 8");

        denseName.Should().Be("Dense Ruins");
        denseDesc.Should().Contain("DC 16");

        glitchedName.Should().Be("Glitched Labyrinth");
        glitchedDesc.Should().Contain("DC 24");
        glitchedDesc.Should().Contain("Compasses fail");
    }

    [Test]
    [TestCase(NavigationTerrainType.OpenWasteland, 1.0f)]
    [TestCase(NavigationTerrainType.ModerateRuins, 0.8f)]
    [TestCase(NavigationTerrainType.DenseRuins, 0.6f)]
    [TestCase(NavigationTerrainType.Labyrinthine, 0.4f)]
    [TestCase(NavigationTerrainType.GlitchedLabyrinth, 0.3f)]
    public void GetTerrainSpeedMultiplier_ReturnsCorrectMultiplier(NavigationTerrainType terrainType, float expectedMultiplier)
    {
        // Act
        var multiplier = _sut.GetTerrainSpeedMultiplier(terrainType);

        // Assert
        multiplier.Should().BeApproximately(expectedMultiplier, 0.01f);
    }

    #endregion

    #region Compass Effectiveness Tests

    [Test]
    [TestCase(NavigationTerrainType.OpenWasteland, true)]
    [TestCase(NavigationTerrainType.ModerateRuins, true)]
    [TestCase(NavigationTerrainType.DenseRuins, true)]
    [TestCase(NavigationTerrainType.Labyrinthine, true)]
    [TestCase(NavigationTerrainType.GlitchedLabyrinth, false)]
    public void IsCompassEffective_ReturnsFalseOnlyInGlitchedLabyrinth(NavigationTerrainType terrainType, bool expectedEffective)
    {
        // Act
        var effective = _sut.IsCompassEffective(terrainType);

        // Assert
        effective.Should().Be(expectedEffective);
    }

    [Test]
    public void CalculateDiceModifiers_WithCompassInNormalTerrain_IncludesBonus()
    {
        // Arrange
        var context = NavigationContext.CreateWithBonuses(
            destination: "Sector 7",
            terrainType: NavigationTerrainType.ModerateRuins,
            hasCompass: true,
            familiarTerritory: false);

        // Act
        var modifiers = _sut.CalculateDiceModifiers(context);

        // Assert - Compass (+1) + Clear weather (+1) = +2
        modifiers.Should().Be(2);
    }

    [Test]
    public void CalculateDiceModifiers_WithCompassInGlitchedTerrain_NoCompassBonus()
    {
        // Arrange
        var context = NavigationContext.CreateWithBonuses(
            destination: "Glitched Zone",
            terrainType: NavigationTerrainType.GlitchedLabyrinth,
            hasCompass: true,
            familiarTerritory: false);

        // Act
        var modifiers = _sut.CalculateDiceModifiers(context);

        // Assert - Compass ineffective (0) + Clear weather (+1) = +1
        modifiers.Should().Be(1);
    }

    #endregion

    #region Dice Modifier Tests

    [Test]
    public void CalculateDiceModifiers_WithFamiliarTerritory_IncludesBonus()
    {
        // Arrange
        var context = NavigationContext.CreateWithBonuses(
            destination: "Home Base",
            terrainType: NavigationTerrainType.ModerateRuins,
            hasCompass: false,
            familiarTerritory: true);

        // Act
        var modifiers = _sut.CalculateDiceModifiers(context);

        // Assert - Familiar (+2) + Clear weather (+1) = +3
        modifiers.Should().Be(3);
    }

    [Test]
    public void CalculateDiceModifiers_WithAllBonuses_StacksCorrectly()
    {
        // Arrange - Compass (+1) + Familiar (+2) + Clear (+1) = +4
        var context = NavigationContext.CreateFull(
            destination: "Safe Haven",
            terrainType: NavigationTerrainType.OpenWasteland,
            hasCompass: true,
            familiarTerritory: true,
            weatherConditions: WeatherType.Clear,
            isNight: false);

        // Act
        var modifiers = _sut.CalculateDiceModifiers(context);

        // Assert
        modifiers.Should().Be(4);
    }

    [Test]
    [TestCase(WeatherType.Clear, 1)]
    [TestCase(WeatherType.Cloudy, 0)]
    [TestCase(WeatherType.LightRain, -1)]
    [TestCase(WeatherType.HeavyRain, -2)]
    [TestCase(WeatherType.Fog, -3)]
    [TestCase(WeatherType.Storm, -4)]
    public void GetWeatherModifier_ReturnsCorrectModifierForEachWeather(WeatherType weather, int expectedModifier)
    {
        // Act
        var modifier = _sut.GetWeatherModifier(weather);

        // Assert
        modifier.Should().Be(expectedModifier);
    }

    [Test]
    public void CalculateDiceModifiers_WithNightPenalty_AppliesPenalty()
    {
        // Arrange
        var context = NavigationContext.CreateFull(
            destination: "Night Destination",
            terrainType: NavigationTerrainType.ModerateRuins,
            hasCompass: false,
            familiarTerritory: false,
            weatherConditions: WeatherType.Clear,
            isNight: true);

        // Act
        var modifiers = _sut.CalculateDiceModifiers(context);

        // Assert - Clear (+1) + Night (-2) = -1
        modifiers.Should().Be(-1);
    }

    [Test]
    public void CalculateDiceModifiers_WorstCaseScenario_AppliesAllPenalties()
    {
        // Arrange - Storm (-4) + Night (-2) = -6
        var context = NavigationContext.CreateFull(
            destination: "Worst Case",
            terrainType: NavigationTerrainType.GlitchedLabyrinth,
            hasCompass: true, // Ineffective anyway
            familiarTerritory: false,
            weatherConditions: WeatherType.Storm,
            isNight: true);

        // Act
        var modifiers = _sut.CalculateDiceModifiers(context);

        // Assert - Compass ineffective (0) + Storm (-4) + Night (-2) = -6
        modifiers.Should().Be(-6);
    }

    #endregion

    #region Time Multiplier Tests

    [Test]
    [TestCase(NavigationOutcome.Success, 1.0f)]
    [TestCase(NavigationOutcome.PartialSuccess, 1.25f)]
    [TestCase(NavigationOutcome.Failure, 1.5f)]
    [TestCase(NavigationOutcome.Fumble, 1.0f)]
    public void GetOutcomeTimeMultiplier_ReturnsCorrectMultiplier(NavigationOutcome outcome, float expectedMultiplier)
    {
        // Act
        var multiplier = _sut.GetOutcomeTimeMultiplier(outcome);

        // Assert
        multiplier.Should().BeApproximately(expectedMultiplier, 0.01f);
    }

    [Test]
    public void CalculateActualTravelTime_AppliesMultiplierCorrectly()
    {
        // Arrange
        var successResult = NavigationResult.Success(10, 8);
        var partialResult = NavigationResult.PartialSuccess(6, 8);
        var failureResult = NavigationResult.Failure(4, 8);

        // Act
        var successTime = _sut.CalculateActualTravelTime(60, successResult);
        var partialTime = _sut.CalculateActualTravelTime(60, partialResult);
        var failureTime = _sut.CalculateActualTravelTime(60, failureResult);

        // Assert
        successTime.Should().Be(60);  // 60 × 1.0
        partialTime.Should().Be(75);  // 60 × 1.25
        failureTime.Should().Be(90);  // 60 × 1.5
    }

    #endregion

    #region Dangerous Area Tests

    [Test]
    public void RollDangerousAreaType_ReturnsValidAreaType()
    {
        // Act - Roll multiple times to check all possible outcomes
        var results = new List<DangerousAreaType>();
        for (int i = 0; i < 100; i++)
        {
            results.Add(_sut.RollDangerousAreaType());
        }

        // Assert - All results should be valid enum values
        results.Should().OnlyContain(r =>
            r == DangerousAreaType.HazardZone ||
            r == DangerousAreaType.HostileTerritory ||
            r == DangerousAreaType.GlitchPocket);
    }

    [Test]
    public void GetDangerousAreaDescription_ReturnsDescriptionForEachType()
    {
        // Act
        var (hazardName, hazardDesc) = _sut.GetDangerousAreaDescription(DangerousAreaType.HazardZone);
        var (hostileName, hostileDesc) = _sut.GetDangerousAreaDescription(DangerousAreaType.HostileTerritory);
        var (glitchName, glitchDesc) = _sut.GetDangerousAreaDescription(DangerousAreaType.GlitchPocket);

        // Assert
        hazardName.Should().Be("Hazard Zone");
        hazardDesc.Should().Contain("environmental");

        hostileName.Should().Be("Hostile Territory");
        hostileDesc.Should().Contain("hostile");

        glitchName.Should().Be("Glitch Pocket");
        glitchDesc.Should().Contain("reality distortion");
    }

    #endregion

    #region Navigation Prerequisites Tests

    [Test]
    public void CanNavigate_WithValidDestination_ReturnsTrue()
    {
        // Arrange
        var player = CreateTestPlayer();
        var context = NavigationContext.CreateSimple("Valid Destination", NavigationTerrainType.OpenWasteland);

        // Act
        var canNavigate = _sut.CanNavigate(player, context);

        // Assert
        canNavigate.Should().BeTrue();
    }

    [Test]
    public void CanNavigate_WithEmptyDestination_ReturnsFalse()
    {
        // Arrange
        var player = CreateTestPlayer();
        var context = NavigationContext.CreateSimple("", NavigationTerrainType.OpenWasteland);

        // Act
        var canNavigate = _sut.CanNavigate(player, context);

        // Assert
        canNavigate.Should().BeFalse();
    }

    [Test]
    public void GetNavigationBlockedReason_WithEmptyDestination_ReturnsReason()
    {
        // Arrange
        var player = CreateTestPlayer();
        var context = NavigationContext.CreateSimple("", NavigationTerrainType.OpenWasteland);

        // Act
        var reason = _sut.GetNavigationBlockedReason(player, context);

        // Assert
        reason.Should().NotBeNull();
        reason.Should().Contain("destination");
    }

    [Test]
    public void GetNavigationBlockedReason_WithValidDestination_ReturnsNull()
    {
        // Arrange
        var player = CreateTestPlayer();
        var context = NavigationContext.CreateSimple("Valid Destination", NavigationTerrainType.OpenWasteland);

        // Act
        var reason = _sut.GetNavigationBlockedReason(player, context);

        // Assert
        reason.Should().BeNull();
    }

    #endregion

    #region NavigationContext Value Object Tests

    [Test]
    public void NavigationContext_CalculatesCorrectBaseDc()
    {
        // Arrange & Act
        var openContext = NavigationContext.CreateSimple("Dest", NavigationTerrainType.OpenWasteland);
        var denseContext = NavigationContext.CreateSimple("Dest", NavigationTerrainType.DenseRuins);
        var glitchedContext = NavigationContext.CreateSimple("Dest", NavigationTerrainType.GlitchedLabyrinth);

        // Assert
        openContext.BaseDc.Should().Be(8);
        denseContext.BaseDc.Should().Be(16);
        glitchedContext.BaseDc.Should().Be(24);
    }

    [Test]
    public void NavigationContext_CompassEffective_FalseInGlitchedTerrain()
    {
        // Arrange
        var normalContext = NavigationContext.CreateWithBonuses("Dest", NavigationTerrainType.DenseRuins, hasCompass: true);
        var glitchedContext = NavigationContext.CreateWithBonuses("Dest", NavigationTerrainType.GlitchedLabyrinth, hasCompass: true);

        // Assert
        normalContext.CompassEffective.Should().BeTrue();
        glitchedContext.CompassEffective.Should().BeFalse();
    }

    [Test]
    public void NavigationContext_TotalDiceModifier_CalculatesCorrectly()
    {
        // Arrange - Compass (+1) + Familiar (+2) + HeavyRain (-2) + Night (-2) = -1
        var context = NavigationContext.CreateFull(
            destination: "Test",
            terrainType: NavigationTerrainType.ModerateRuins,
            hasCompass: true,
            familiarTerritory: true,
            weatherConditions: WeatherType.HeavyRain,
            isNight: true);

        // Assert
        context.TotalDiceModifier.Should().Be(-1);
    }

    [Test]
    public void NavigationContext_ToSkillContext_CreatesValidContext()
    {
        // Arrange
        var context = NavigationContext.CreateFull(
            destination: "Test",
            terrainType: NavigationTerrainType.DenseRuins,
            hasCompass: true,
            familiarTerritory: true,
            weatherConditions: WeatherType.Fog,
            isNight: true);

        // Act
        var skillContext = context.ToSkillContext();

        // Assert
        skillContext.Should().NotBeNull();
        skillContext.EquipmentModifiers.Should().NotBeEmpty(); // Has compass
        skillContext.SituationalModifiers.Should().NotBeEmpty(); // Has familiar territory and night
        skillContext.EnvironmentModifiers.Should().NotBeEmpty(); // Has weather
    }

    [Test]
    public void NavigationContext_FactoryMethods_CreateCorrectContexts()
    {
        // Act
        var simple = NavigationContext.CreateSimple("Dest", NavigationTerrainType.OpenWasteland);
        var withBonuses = NavigationContext.CreateWithBonuses("Dest", NavigationTerrainType.DenseRuins, true, true);
        var full = NavigationContext.CreateFull("Dest", NavigationTerrainType.Labyrinthine, true, true, WeatherType.Storm, true);

        // Assert
        simple.HasCompass.Should().BeFalse();
        simple.FamiliarTerritory.Should().BeFalse();

        withBonuses.HasCompass.Should().BeTrue();
        withBonuses.FamiliarTerritory.Should().BeTrue();

        full.WeatherConditions.Should().Be(WeatherType.Storm);
        full.IsNightWithoutLight.Should().BeTrue();
    }

    #endregion

    #region NavigationResult Value Object Tests

    [Test]
    public void NavigationResult_Success_HasCorrectProperties()
    {
        // Act
        var result = NavigationResult.Success(10, 8, "Test roll");

        // Assert
        result.Outcome.Should().Be(NavigationOutcome.Success);
        result.TimeModifier.Should().Be(1.0f);
        result.ReachedDestination.Should().BeTrue();
        result.GotLost.Should().BeFalse();
        result.EnteredDangerousArea.Should().BeFalse();
        result.Margin.Should().Be(2); // 10 - 8
    }

    [Test]
    public void NavigationResult_PartialSuccess_HasCorrectProperties()
    {
        // Act
        var result = NavigationResult.PartialSuccess(6, 8, "Test roll");

        // Assert
        result.Outcome.Should().Be(NavigationOutcome.PartialSuccess);
        result.TimeModifier.Should().Be(1.25f);
        result.ReachedDestination.Should().BeTrue();
        result.GotLost.Should().BeFalse();
        result.TimeModifierPercent.Should().Be(25);
    }

    [Test]
    public void NavigationResult_Failure_HasCorrectProperties()
    {
        // Act
        var result = NavigationResult.Failure(4, 8, "Test roll");

        // Assert
        result.Outcome.Should().Be(NavigationOutcome.Failure);
        result.TimeModifier.Should().Be(1.5f);
        result.ReachedDestination.Should().BeFalse();
        result.GotLost.Should().BeTrue();
        result.Failed.Should().BeTrue();
    }

    [Test]
    public void NavigationResult_Fumble_HasCorrectProperties()
    {
        // Act
        var result = NavigationResult.Fumble(0, 12, DangerousAreaType.HostileTerritory, "Test roll");

        // Assert
        result.Outcome.Should().Be(NavigationOutcome.Fumble);
        result.HazardEncountered.Should().BeTrue();
        result.DangerousAreaType.Should().Be(DangerousAreaType.HostileTerritory);
        result.EnteredDangerousArea.Should().BeTrue();
        result.Failed.Should().BeTrue();
    }

    [Test]
    public void NavigationResult_Empty_HasCorrectProperties()
    {
        // Act
        var result = NavigationResult.Empty();

        // Assert
        result.NetSuccesses.Should().Be(0);
        result.TargetDc.Should().Be(0);
        result.ReachedDestination.Should().BeFalse();
    }

    [Test]
    public void NavigationResult_ToDisplayString_FormatsCorrectly()
    {
        // Arrange
        var successResult = NavigationResult.Success(10, 8);
        var fumbleResult = NavigationResult.Fumble(0, 12, DangerousAreaType.GlitchPocket);

        // Act
        var successDisplay = successResult.ToDisplayString();
        var fumbleDisplay = fumbleResult.ToDisplayString();

        // Assert
        successDisplay.Should().Contain("successful");
        successDisplay.Should().Contain("10 successes");
        successDisplay.Should().Contain("DC 8");

        fumbleDisplay.Should().Contain("Critically lost");
        fumbleDisplay.Should().Contain("Glitch Pocket");
    }

    #endregion

    #region NavigationOutcome Enum Extension Tests

    [Test]
    public void NavigationOutcome_GetTimeMultiplier_ReturnsCorrectValues()
    {
        // Assert
        NavigationOutcome.Success.GetTimeMultiplier().Should().Be(1.0f);
        NavigationOutcome.PartialSuccess.GetTimeMultiplier().Should().Be(1.25f);
        NavigationOutcome.Failure.GetTimeMultiplier().Should().Be(1.5f);
        NavigationOutcome.Fumble.GetTimeMultiplier().Should().Be(1.0f);
    }

    [Test]
    public void NavigationOutcome_ReachedDestination_ReturnsTrueForSuccessAndPartial()
    {
        // Assert
        NavigationOutcome.Success.ReachedDestination().Should().BeTrue();
        NavigationOutcome.PartialSuccess.ReachedDestination().Should().BeTrue();
        NavigationOutcome.Failure.ReachedDestination().Should().BeFalse();
        NavigationOutcome.Fumble.ReachedDestination().Should().BeFalse();
    }

    [Test]
    public void NavigationOutcome_GotLost_ReturnsTrueForFailureAndFumble()
    {
        // Assert
        NavigationOutcome.Success.GotLost().Should().BeFalse();
        NavigationOutcome.PartialSuccess.GotLost().Should().BeFalse();
        NavigationOutcome.Failure.GotLost().Should().BeTrue();
        NavigationOutcome.Fumble.GotLost().Should().BeTrue();
    }

    #endregion

    #region DangerousAreaType Enum Extension Tests

    [Test]
    public void DangerousAreaType_FromRoll_ReturnsCorrectTypes()
    {
        // Assert - 1-2: HazardZone, 3-4: HostileTerritory, 5-6: GlitchPocket
        DangerousAreaTypeExtensions.FromRoll(1).Should().Be(DangerousAreaType.HazardZone);
        DangerousAreaTypeExtensions.FromRoll(2).Should().Be(DangerousAreaType.HazardZone);
        DangerousAreaTypeExtensions.FromRoll(3).Should().Be(DangerousAreaType.HostileTerritory);
        DangerousAreaTypeExtensions.FromRoll(4).Should().Be(DangerousAreaType.HostileTerritory);
        DangerousAreaTypeExtensions.FromRoll(5).Should().Be(DangerousAreaType.GlitchPocket);
        DangerousAreaTypeExtensions.FromRoll(6).Should().Be(DangerousAreaType.GlitchPocket);
    }

    [Test]
    public void DangerousAreaType_HasProperties_ReturnsCorrectValues()
    {
        // Assert
        DangerousAreaType.HazardZone.HasEnvironmentalHazards().Should().BeTrue();
        DangerousAreaType.HazardZone.MayTriggerCombat().Should().BeFalse();
        DangerousAreaType.HazardZone.HasGlitchEffects().Should().BeFalse();

        DangerousAreaType.HostileTerritory.HasEnvironmentalHazards().Should().BeFalse();
        DangerousAreaType.HostileTerritory.MayTriggerCombat().Should().BeTrue();
        DangerousAreaType.HostileTerritory.HasGlitchEffects().Should().BeFalse();

        DangerousAreaType.GlitchPocket.HasEnvironmentalHazards().Should().BeTrue();
        DangerousAreaType.GlitchPocket.MayTriggerCombat().Should().BeFalse();
        DangerousAreaType.GlitchPocket.HasGlitchEffects().Should().BeTrue();
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

    /// <summary>
    /// Creates a test player for navigation tests.
    /// </summary>
    private static Domain.Entities.Player CreateTestPlayer()
    {
        return new Domain.Entities.Player("Test Navigator");
    }

    #endregion
}
