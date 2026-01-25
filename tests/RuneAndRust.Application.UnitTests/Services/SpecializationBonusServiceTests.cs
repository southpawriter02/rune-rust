using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for <see cref="SpecializationBonusService"/>.
/// </summary>
/// <remarks>
/// <para>
/// Tests cover all three Wasteland Survival specializations and their abilities:
/// </para>
/// <list type="bullet">
///   <item><description><b>Veioimaor:</b> Beast Tracker (+2d10), Predator's Eye, Hunting Grounds (+2d10)</description></item>
///   <item><description><b>Myr-Stalker:</b> Swamp Navigator (+1d10), Toxin Resistance (advantage), Mire Knowledge</description></item>
///   <item><description><b>Gantry-Runner:</b> Urban Navigator (+1d10), Rooftop Routes, Scrap Familiar (+1d10)</description></item>
/// </list>
/// <para>
/// Each test follows the Arrange-Act-Assert pattern and uses FluentAssertions for
/// readable and maintainable assertions.
/// </para>
/// </remarks>
[TestFixture]
public class SpecializationBonusServiceTests
{
    // =========================================================================
    // FIELDS
    // =========================================================================

    private ILogger<SpecializationBonusService> _logger = null!;
    private SpecializationBonusService _sut = null!;
    private const string TestCharacterId = "test-character";
    private const string TestAreaId = "test-area-001";
    private const string TestAreaName = "The Rusted Valley";

    // Empty context for tests that don't need specific context data
    private static readonly IReadOnlyDictionary<string, string> EmptyContext =
        new Dictionary<string, string>();

    // =========================================================================
    // SETUP
    // =========================================================================

    [SetUp]
    public void SetUp()
    {
        _logger = Substitute.For<ILogger<SpecializationBonusService>>();
        _sut = new SpecializationBonusService(_logger);
    }

    // =========================================================================
    // CHARACTER REGISTRATION TESTS
    // =========================================================================

    #region Character Registration Tests

    [Test]
    public void GetCharacterSpecialization_WithoutRegistration_ReturnsNone()
    {
        // Act
        var result = _sut.GetCharacterSpecialization(TestCharacterId);

        // Assert
        result.Should().Be(WastelandSurvivalSpecializationType.None);
    }

    [Test]
    public void RegisterCharacterSpecialization_WithValidSpecialization_RegistersSuccessfully()
    {
        // Arrange & Act
        _sut.RegisterCharacterSpecialization(TestCharacterId, WastelandSurvivalSpecializationType.Veioimaor);
        var result = _sut.GetCharacterSpecialization(TestCharacterId);

        // Assert
        result.Should().Be(WastelandSurvivalSpecializationType.Veioimaor);
    }

    [Test]
    public void RegisterCharacterSpecialization_WithDifferentSpecialization_OverwritesPrevious()
    {
        // Arrange
        _sut.RegisterCharacterSpecialization(TestCharacterId, WastelandSurvivalSpecializationType.Veioimaor);

        // Act
        _sut.RegisterCharacterSpecialization(TestCharacterId, WastelandSurvivalSpecializationType.MyrStalker);
        var result = _sut.GetCharacterSpecialization(TestCharacterId);

        // Assert
        result.Should().Be(WastelandSurvivalSpecializationType.MyrStalker);
    }

    [Test]
    public void UnregisterCharacter_WithRegisteredCharacter_RemovesSpecialization()
    {
        // Arrange
        _sut.RegisterCharacterSpecialization(TestCharacterId, WastelandSurvivalSpecializationType.GantryRunner);

        // Act
        _sut.UnregisterCharacter(TestCharacterId);
        var result = _sut.GetCharacterSpecialization(TestCharacterId);

        // Assert
        result.Should().Be(WastelandSurvivalSpecializationType.None);
    }

    #endregion

    // =========================================================================
    // VEIOIMAOR ABILITY TESTS
    // =========================================================================

    #region Veioimaor: Beast Tracker Tests

    [Test]
    public void GetBonusesForCheck_VeioimaorTrackingLivingCreature_ReturnsBeastTrackerBonus()
    {
        // Arrange
        _sut.RegisterCharacterSpecialization(TestCharacterId, WastelandSurvivalSpecializationType.Veioimaor);

        // Act
        var bonuses = _sut.GetBonusesForCheck(
            TestCharacterId,
            WastelandSurvivalCheckType.Tracking,
            NavigationTerrainType.OpenWasteland,
            targetType: TargetType.LivingCreature);

        // Assert
        bonuses.Should().HaveCount(1);
        bonuses[0].AbilityId.Should().Be("beast-tracker");
        bonuses[0].BonusDice.Should().Be(2);
        bonuses[0].Type.Should().Be(SpecializationBonusType.DicePool);
    }

    [Test]
    public void GetBonusesForCheck_VeioimaorTrackingNonLivingCreature_ReturnsNoBonus()
    {
        // Arrange
        _sut.RegisterCharacterSpecialization(TestCharacterId, WastelandSurvivalSpecializationType.Veioimaor);

        // Act
        var bonuses = _sut.GetBonusesForCheck(
            TestCharacterId,
            WastelandSurvivalCheckType.Tracking,
            NavigationTerrainType.OpenWasteland,
            targetType: TargetType.Construct);

        // Assert
        bonuses.Should().BeEmpty();
    }

    [Test]
    public void GetBonusesForCheck_VeioimaorNonTrackingCheck_ReturnsNoBonus()
    {
        // Arrange
        _sut.RegisterCharacterSpecialization(TestCharacterId, WastelandSurvivalSpecializationType.Veioimaor);

        // Act
        var bonuses = _sut.GetBonusesForCheck(
            TestCharacterId,
            WastelandSurvivalCheckType.Foraging,
            NavigationTerrainType.OpenWasteland,
            targetType: TargetType.LivingCreature);

        // Assert
        bonuses.Should().BeEmpty();
    }

    #endregion

    #region Veioimaor: Predator's Eye Tests

    [Test]
    public void HasAbility_VeioimaorPredatorsEye_ReturnsTrue()
    {
        // Arrange
        _sut.RegisterCharacterSpecialization(TestCharacterId, WastelandSurvivalSpecializationType.Veioimaor);

        // Act
        var result = _sut.HasAbility(TestCharacterId, "predators-eye");

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public void ActivateAbility_PredatorsEye_ReturnsPostCheckActivation()
    {
        // Arrange
        _sut.RegisterCharacterSpecialization(TestCharacterId, WastelandSurvivalSpecializationType.Veioimaor);

        // Act
        var activation = _sut.ActivateAbility(TestCharacterId, "predators-eye", EmptyContext);

        // Assert
        activation.Should().NotBeNull();
        activation!.Value.IsValid.Should().BeTrue();
        activation.Value.Type.Should().Be(AbilityActivationType.PostCheck);
        activation.Value.AbilityId.Should().Be("predators-eye");
        activation.Value.HasData("weakness").Should().BeTrue();
        activation.Value.HasData("behavior").Should().BeTrue();
    }

    [Test]
    public void ActivateAbility_PredatorsEyeWithoutAbility_ReturnsNull()
    {
        // Arrange
        _sut.RegisterCharacterSpecialization(TestCharacterId, WastelandSurvivalSpecializationType.MyrStalker);

        // Act
        var activation = _sut.ActivateAbility(TestCharacterId, "predators-eye", EmptyContext);

        // Assert
        activation.Should().BeNull();
    }

    #endregion

    #region Veioimaor: Hunting Grounds Tests

    [Test]
    public void MarkHuntingGrounds_ValidInput_CreatesMarker()
    {
        // Arrange
        _sut.RegisterCharacterSpecialization(TestCharacterId, WastelandSurvivalSpecializationType.Veioimaor);

        // Act
        var marker = _sut.MarkHuntingGrounds(TestCharacterId, TestAreaId, TestAreaName);

        // Assert
        marker.Should().NotBeNull();
        marker!.Value.IsValid.Should().BeTrue();
        marker.Value.IsActive.Should().BeTrue();
        marker.Value.PlayerId.Should().Be(TestCharacterId);
        marker.Value.AreaId.Should().Be(TestAreaId);
        marker.Value.AreaName.Should().Be(TestAreaName);
    }

    [Test]
    public void MarkHuntingGrounds_WithoutAbility_ReturnsNull()
    {
        // Arrange
        _sut.RegisterCharacterSpecialization(TestCharacterId, WastelandSurvivalSpecializationType.MyrStalker);

        // Act
        var marker = _sut.MarkHuntingGrounds(TestCharacterId, TestAreaId, TestAreaName);

        // Assert
        marker.Should().BeNull();
    }

    [Test]
    public void GetActiveHuntingGrounds_WithMarkedArea_ReturnsMarker()
    {
        // Arrange
        _sut.RegisterCharacterSpecialization(TestCharacterId, WastelandSurvivalSpecializationType.Veioimaor);
        _sut.MarkHuntingGrounds(TestCharacterId, TestAreaId, TestAreaName);

        // Act
        var marker = _sut.GetActiveHuntingGrounds(TestCharacterId);

        // Assert
        marker.Should().NotBeNull();
        marker!.Value.IsValid.Should().BeTrue();
        marker.Value.AreaId.Should().Be(TestAreaId);
    }

    [Test]
    public void GetActiveHuntingGrounds_WithoutMarkedArea_ReturnsNull()
    {
        // Arrange
        _sut.RegisterCharacterSpecialization(TestCharacterId, WastelandSurvivalSpecializationType.Veioimaor);

        // Act
        var marker = _sut.GetActiveHuntingGrounds(TestCharacterId);

        // Assert
        marker.Should().BeNull();
    }

    [Test]
    public void ClearHuntingGrounds_WithMarkedArea_RemovesMarker()
    {
        // Arrange
        _sut.RegisterCharacterSpecialization(TestCharacterId, WastelandSurvivalSpecializationType.Veioimaor);
        _sut.MarkHuntingGrounds(TestCharacterId, TestAreaId, TestAreaName);

        // Act
        _sut.ClearHuntingGrounds(TestCharacterId);
        var marker = _sut.GetActiveHuntingGrounds(TestCharacterId);

        // Assert
        marker.Should().BeNull();
    }

    [Test]
    public void GetBonusesForCheck_VeioimaorInHuntingGrounds_ReturnsBonusDice()
    {
        // Arrange
        _sut.RegisterCharacterSpecialization(TestCharacterId, WastelandSurvivalSpecializationType.Veioimaor);
        _sut.MarkHuntingGrounds(TestCharacterId, TestAreaId, TestAreaName);

        // Act
        var bonuses = _sut.GetBonusesForCheck(
            TestCharacterId,
            WastelandSurvivalCheckType.General,
            NavigationTerrainType.OpenWasteland,
            areaId: TestAreaId);

        // Assert
        bonuses.Should().Contain(b => b.AbilityId == "hunting-grounds");
        var huntingGroundsBonus = bonuses.First(b => b.AbilityId == "hunting-grounds");
        huntingGroundsBonus.BonusDice.Should().Be(2);
    }

    [Test]
    public void GetBonusesForCheck_VeioimaorOutsideHuntingGrounds_ReturnsNoHuntingGroundsBonus()
    {
        // Arrange
        _sut.RegisterCharacterSpecialization(TestCharacterId, WastelandSurvivalSpecializationType.Veioimaor);
        _sut.MarkHuntingGrounds(TestCharacterId, TestAreaId, TestAreaName);

        // Act
        var bonuses = _sut.GetBonusesForCheck(
            TestCharacterId,
            WastelandSurvivalCheckType.General,
            NavigationTerrainType.OpenWasteland,
            areaId: "different-area");

        // Assert
        bonuses.Should().NotContain(b => b.AbilityId == "hunting-grounds");
    }

    #endregion

    // =========================================================================
    // MYR-STALKER ABILITY TESTS
    // =========================================================================

    #region Myr-Stalker: Swamp Navigator Tests

    [Test]
    [TestCase(NavigationTerrainType.OpenWasteland, false, Description = "Non-swamp terrain returns no bonus")]
    [TestCase(NavigationTerrainType.ModerateRuins, false, Description = "Ruins terrain returns no bonus")]
    public void GetBonusesForCheck_MyrStalkerTerrainCheck_ReturnsCorrectBonus(
        NavigationTerrainType terrain,
        bool expectBonus)
    {
        // Arrange
        _sut.RegisterCharacterSpecialization(TestCharacterId, WastelandSurvivalSpecializationType.MyrStalker);

        // Act
        var bonuses = _sut.GetBonusesForCheck(
            TestCharacterId,
            WastelandSurvivalCheckType.Navigation,
            terrain);

        // Assert
        if (expectBonus)
        {
            bonuses.Should().Contain(b => b.AbilityId == "swamp-navigator");
            var swampBonus = bonuses.First(b => b.AbilityId == "swamp-navigator");
            swampBonus.BonusDice.Should().Be(1);
        }
        else
        {
            bonuses.Should().NotContain(b => b.AbilityId == "swamp-navigator");
        }
    }

    #endregion

    #region Myr-Stalker: Toxin Resistance Tests

    [Test]
    public void HasAdvantage_MyrStalkerPoisonGasHazard_ReturnsTrue()
    {
        // Arrange
        _sut.RegisterCharacterSpecialization(TestCharacterId, WastelandSurvivalSpecializationType.MyrStalker);

        // Act
        var hasAdvantage = _sut.HasAdvantage(TestCharacterId, HazardType.PoisonGas);

        // Assert
        hasAdvantage.Should().BeTrue();
    }

    [Test]
    public void HasAdvantage_MyrStalkerNonPoisonHazard_ReturnsFalse()
    {
        // Arrange
        _sut.RegisterCharacterSpecialization(TestCharacterId, WastelandSurvivalSpecializationType.MyrStalker);

        // Act
        var hasAdvantage = _sut.HasAdvantage(TestCharacterId, HazardType.Fire);

        // Assert
        hasAdvantage.Should().BeFalse();
    }

    [Test]
    public void HasAdvantage_NonMyrStalkerPoisonGasHazard_ReturnsFalse()
    {
        // Arrange
        _sut.RegisterCharacterSpecialization(TestCharacterId, WastelandSurvivalSpecializationType.Veioimaor);

        // Act
        var hasAdvantage = _sut.HasAdvantage(TestCharacterId, HazardType.PoisonGas);

        // Assert
        hasAdvantage.Should().BeFalse();
    }

    #endregion

    #region Myr-Stalker: Mire Knowledge Tests

    [Test]
    public void HasAbility_MyrStalkerMireKnowledge_ReturnsTrue()
    {
        // Arrange
        _sut.RegisterCharacterSpecialization(TestCharacterId, WastelandSurvivalSpecializationType.MyrStalker);

        // Act
        var result = _sut.HasAbility(TestCharacterId, "mire-knowledge");

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public void ActivateAbility_MireKnowledge_ReturnsPathRevealActivation()
    {
        // Arrange
        _sut.RegisterCharacterSpecialization(TestCharacterId, WastelandSurvivalSpecializationType.MyrStalker);

        // Act
        var activation = _sut.ActivateAbility(TestCharacterId, "mire-knowledge", EmptyContext);

        // Assert
        activation.Should().NotBeNull();
        activation!.Value.IsValid.Should().BeTrue();
        activation.Value.Type.Should().Be(AbilityActivationType.PathReveal);
        activation.Value.AbilityId.Should().Be("mire-knowledge");
    }

    #endregion

    // =========================================================================
    // GANTRY-RUNNER ABILITY TESTS
    // =========================================================================

    #region Gantry-Runner: Urban Navigator Tests

    [Test]
    [TestCase(NavigationTerrainType.ModerateRuins, true, Description = "Moderate ruins returns bonus")]
    [TestCase(NavigationTerrainType.DenseRuins, true, Description = "Dense ruins returns bonus")]
    [TestCase(NavigationTerrainType.OpenWasteland, false, Description = "Non-ruins terrain returns no bonus")]
    public void GetBonusesForCheck_GantryRunnerTerrainCheck_ReturnsCorrectBonus(
        NavigationTerrainType terrain,
        bool expectBonus)
    {
        // Arrange
        _sut.RegisterCharacterSpecialization(TestCharacterId, WastelandSurvivalSpecializationType.GantryRunner);

        // Act
        var bonuses = _sut.GetBonusesForCheck(
            TestCharacterId,
            WastelandSurvivalCheckType.Navigation,
            terrain);

        // Assert
        if (expectBonus)
        {
            bonuses.Should().Contain(b => b.AbilityId == "urban-navigator");
            var urbanBonus = bonuses.First(b => b.AbilityId == "urban-navigator");
            urbanBonus.BonusDice.Should().Be(1);
        }
        else
        {
            bonuses.Should().NotContain(b => b.AbilityId == "urban-navigator");
        }
    }

    #endregion

    #region Gantry-Runner: Rooftop Routes Tests

    [Test]
    public void HasAbility_GantryRunnerRooftopRoutes_ReturnsTrue()
    {
        // Arrange
        _sut.RegisterCharacterSpecialization(TestCharacterId, WastelandSurvivalSpecializationType.GantryRunner);

        // Act
        var result = _sut.HasAbility(TestCharacterId, "rooftop-routes");

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public void ActivateAbility_RooftopRoutes_ReturnsPathRevealActivation()
    {
        // Arrange
        _sut.RegisterCharacterSpecialization(TestCharacterId, WastelandSurvivalSpecializationType.GantryRunner);

        // Act
        var activation = _sut.ActivateAbility(TestCharacterId, "rooftop-routes", EmptyContext);

        // Assert
        activation.Should().NotBeNull();
        activation!.Value.IsValid.Should().BeTrue();
        activation.Value.Type.Should().Be(AbilityActivationType.PathReveal);
        activation.Value.AbilityId.Should().Be("rooftop-routes");
    }

    #endregion

    #region Gantry-Runner: Scrap Familiar Tests

    [Test]
    public void GetBonusesForCheck_GantryRunnerForagingInRuins_ReturnsScrapFamiliarBonus()
    {
        // Arrange
        _sut.RegisterCharacterSpecialization(TestCharacterId, WastelandSurvivalSpecializationType.GantryRunner);

        // Act
        var bonuses = _sut.GetBonusesForCheck(
            TestCharacterId,
            WastelandSurvivalCheckType.Foraging,
            NavigationTerrainType.ModerateRuins);

        // Assert
        bonuses.Should().HaveCountGreaterOrEqualTo(1);
        bonuses.Should().Contain(b => b.AbilityId == "scrap-familiar");
        var scrapBonus = bonuses.First(b => b.AbilityId == "scrap-familiar");
        scrapBonus.BonusDice.Should().Be(1);
    }

    [Test]
    public void GetBonusesForCheck_GantryRunnerForagingOutsideRuins_ReturnsNoScrapFamiliarBonus()
    {
        // Arrange
        _sut.RegisterCharacterSpecialization(TestCharacterId, WastelandSurvivalSpecializationType.GantryRunner);

        // Act
        var bonuses = _sut.GetBonusesForCheck(
            TestCharacterId,
            WastelandSurvivalCheckType.Foraging,
            NavigationTerrainType.OpenWasteland);

        // Assert
        bonuses.Should().NotContain(b => b.AbilityId == "scrap-familiar");
    }

    [Test]
    public void GetBonusesForCheck_GantryRunnerNonForagingInRuins_ReturnsNoScrapFamiliarBonus()
    {
        // Arrange
        _sut.RegisterCharacterSpecialization(TestCharacterId, WastelandSurvivalSpecializationType.GantryRunner);

        // Act
        var bonuses = _sut.GetBonusesForCheck(
            TestCharacterId,
            WastelandSurvivalCheckType.Tracking,
            NavigationTerrainType.ModerateRuins);

        // Assert
        bonuses.Should().NotContain(b => b.AbilityId == "scrap-familiar");
    }

    #endregion

    // =========================================================================
    // TOTAL BONUS DICE CALCULATION TESTS
    // =========================================================================

    #region GetTotalBonusDice Tests

    [Test]
    public void GetTotalBonusDice_WithMultipleBonuses_SumsCorrectly()
    {
        // Arrange
        _sut.RegisterCharacterSpecialization(TestCharacterId, WastelandSurvivalSpecializationType.GantryRunner);

        // Act - Foraging in ruins should get both Urban Navigator (+1) and Scrap Familiar (+1)
        var totalDice = _sut.GetTotalBonusDice(
            TestCharacterId,
            WastelandSurvivalCheckType.Foraging,
            NavigationTerrainType.ModerateRuins);

        // Assert
        totalDice.Should().Be(2); // Urban Navigator + Scrap Familiar
    }

    [Test]
    public void GetTotalBonusDice_WithNoBonuses_ReturnsZero()
    {
        // Arrange
        _sut.RegisterCharacterSpecialization(TestCharacterId, WastelandSurvivalSpecializationType.Veioimaor);

        // Act
        var totalDice = _sut.GetTotalBonusDice(
            TestCharacterId,
            WastelandSurvivalCheckType.Navigation,
            NavigationTerrainType.OpenWasteland);

        // Assert
        totalDice.Should().Be(0);
    }

    [Test]
    public void GetTotalBonusDice_VeioimaorTrackingWithHuntingGrounds_ReturnsStackedBonus()
    {
        // Arrange
        _sut.RegisterCharacterSpecialization(TestCharacterId, WastelandSurvivalSpecializationType.Veioimaor);
        _sut.MarkHuntingGrounds(TestCharacterId, TestAreaId, TestAreaName);

        // Act - Tracking living creature in hunting grounds should get Beast Tracker (+2) + Hunting Grounds (+2)
        var totalDice = _sut.GetTotalBonusDice(
            TestCharacterId,
            WastelandSurvivalCheckType.Tracking,
            NavigationTerrainType.OpenWasteland,
            targetType: TargetType.LivingCreature,
            areaId: TestAreaId);

        // Assert
        totalDice.Should().Be(4); // Beast Tracker + Hunting Grounds
    }

    #endregion

    // =========================================================================
    // TERRAIN HELPER TESTS
    // =========================================================================

    #region Terrain Helper Tests

    [Test]
    [TestCase(NavigationTerrainType.ModerateRuins, true)]
    [TestCase(NavigationTerrainType.DenseRuins, true)]
    [TestCase(NavigationTerrainType.OpenWasteland, false)]
    [TestCase(NavigationTerrainType.Labyrinthine, false)]
    public void IsRuinsTerrain_ReturnsCorrectResult(NavigationTerrainType terrain, bool expected)
    {
        // Act
        var result = _sut.IsRuinsTerrain(terrain);

        // Assert
        result.Should().Be(expected);
    }

    [Test]
    public void IsSwampTerrain_CurrentlyReturnsFalse()
    {
        // Note: Swamp terrain types are not yet added to NavigationTerrainType.
        // This test documents the current behavior and will need updating when
        // swamp terrain types are added.

        // Act
        var result = _sut.IsSwampTerrain(NavigationTerrainType.OpenWasteland);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    // =========================================================================
    // EDGE CASE TESTS
    // =========================================================================

    #region Edge Case Tests

    [Test]
    public void GetBonusesForCheck_WithNullCharacterId_ReturnsEmptyList()
    {
        // Act
        var bonuses = _sut.GetBonusesForCheck(
            null!,
            WastelandSurvivalCheckType.General,
            NavigationTerrainType.OpenWasteland);

        // Assert
        bonuses.Should().BeEmpty();
    }

    [Test]
    public void GetBonusesForCheck_WithEmptyCharacterId_ReturnsEmptyList()
    {
        // Act
        var bonuses = _sut.GetBonusesForCheck(
            string.Empty,
            WastelandSurvivalCheckType.General,
            NavigationTerrainType.OpenWasteland);

        // Assert
        bonuses.Should().BeEmpty();
    }

    [Test]
    public void HasAbility_WithNullAbilityId_ReturnsFalse()
    {
        // Arrange
        _sut.RegisterCharacterSpecialization(TestCharacterId, WastelandSurvivalSpecializationType.Veioimaor);

        // Act
        var result = _sut.HasAbility(TestCharacterId, null!);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void ActivateAbility_WithUnknownAbility_ReturnsNull()
    {
        // Arrange
        _sut.RegisterCharacterSpecialization(TestCharacterId, WastelandSurvivalSpecializationType.Veioimaor);

        // Act
        var activation = _sut.ActivateAbility(TestCharacterId, "unknown-ability", EmptyContext);

        // Assert
        activation.Should().BeNull();
    }

    [Test]
    public void MarkHuntingGrounds_ReplacesExistingMarker()
    {
        // Arrange
        _sut.RegisterCharacterSpecialization(TestCharacterId, WastelandSurvivalSpecializationType.Veioimaor);
        _sut.MarkHuntingGrounds(TestCharacterId, "area-1", "First Area");

        // Act
        _sut.MarkHuntingGrounds(TestCharacterId, "area-2", "Second Area");
        var marker = _sut.GetActiveHuntingGrounds(TestCharacterId);

        // Assert
        marker.Should().NotBeNull();
        marker!.Value.AreaId.Should().Be("area-2");
        marker.Value.AreaName.Should().Be("Second Area");
    }

    #endregion

    // =========================================================================
    // VALUE OBJECT TESTS
    // =========================================================================

    #region SpecializationBonus Value Object Tests

    [Test]
    public void SpecializationBonus_DiceBonus_CreatesCorrectBonus()
    {
        // Act
        var bonus = SpecializationBonus.DiceBonus("test-ability", 2, "Test bonus");

        // Assert
        bonus.IsValid.Should().BeTrue();
        bonus.AddsDice.Should().BeTrue();
        bonus.BonusDice.Should().Be(2);
        bonus.Type.Should().Be(SpecializationBonusType.DicePool);
        bonus.Description.Should().Be("Test bonus");
    }

    [Test]
    public void SpecializationBonus_Advantage_CreatesCorrectBonus()
    {
        // Act
        var bonus = SpecializationBonus.Advantage("test-ability", "Advantage test");

        // Assert
        bonus.IsValid.Should().BeTrue();
        bonus.GrantsAdvantage.Should().BeTrue();
        bonus.AddsDice.Should().BeFalse();
        bonus.Type.Should().Be(SpecializationBonusType.Advantage);
    }

    [Test]
    public void SpecializationBonus_Empty_CreatesInvalidBonus()
    {
        // Act
        var bonus = SpecializationBonus.Empty();

        // Assert
        bonus.IsValid.Should().BeFalse();
    }

    [Test]
    public void SpecializationBonus_ToDisplayString_FormatsCorrectly()
    {
        // Arrange
        var diceBonus = SpecializationBonus.DiceBonus("test", 2, "Beast Tracker");
        var advantageBonus = SpecializationBonus.Advantage("test", "Toxin Resistance");

        // Act & Assert
        diceBonus.ToDisplayString().Should().Be("+2d10 (Beast Tracker)");
        advantageBonus.ToDisplayString().Should().Be("Advantage (Toxin Resistance)");
    }

    #endregion

    #region HuntingGroundsMarker Value Object Tests

    [Test]
    public void HuntingGroundsMarker_Create_CreatesActiveMarker()
    {
        // Act
        var marker = HuntingGroundsMarker.Create(TestCharacterId, TestAreaId, TestAreaName);

        // Assert
        marker.IsValid.Should().BeTrue();
        marker.IsActive.Should().BeTrue();
        marker.IsExpired.Should().BeFalse();
        marker.ExpiresAt.Should().BeNull();
    }

    [Test]
    public void HuntingGroundsMarker_CreateWithDuration_CreatesMarkerWithExpiration()
    {
        // Act
        var marker = HuntingGroundsMarker.CreateWithDuration(
            TestCharacterId,
            TestAreaId,
            TestAreaName,
            TimeSpan.FromHours(1));

        // Assert
        marker.IsValid.Should().BeTrue();
        marker.IsActive.Should().BeTrue();
        marker.ExpiresAt.Should().NotBeNull();
        marker.RemainingTime.Should().NotBeNull();
    }

    [Test]
    public void HuntingGroundsMarker_GetBonus_ReturnsCorrectBonus()
    {
        // Arrange
        var marker = HuntingGroundsMarker.Create(TestCharacterId, TestAreaId, TestAreaName);

        // Act
        var bonus = marker.GetBonus();

        // Assert
        bonus.IsValid.Should().BeTrue();
        bonus.BonusDice.Should().Be(2);
        bonus.AbilityId.Should().Be("hunting-grounds");
    }

    [Test]
    public void HuntingGroundsMarker_AppliesToArea_MatchesCorrectArea()
    {
        // Arrange
        var marker = HuntingGroundsMarker.Create(TestCharacterId, TestAreaId, TestAreaName);

        // Act & Assert
        marker.AppliesToArea(TestAreaId).Should().BeTrue();
        marker.AppliesToArea("different-area").Should().BeFalse();
        marker.AppliesToArea(TestAreaId.ToUpper()).Should().BeTrue(); // Case-insensitive
    }

    [Test]
    public void HuntingGroundsMarker_Empty_CreatesInvalidMarker()
    {
        // Act
        var marker = HuntingGroundsMarker.Empty();

        // Assert
        marker.IsValid.Should().BeFalse();
        marker.GetBonus().IsValid.Should().BeFalse();
    }

    #endregion

    #region AbilityActivation Value Object Tests

    [Test]
    public void AbilityActivation_PredatorsEye_CreatesCorrectActivation()
    {
        // Act
        var activation = AbilityActivation.PredatorsEye("fire", "pack-hunter");

        // Assert
        activation.IsValid.Should().BeTrue();
        activation.IsPostCheck.Should().BeTrue();
        activation.Type.Should().Be(AbilityActivationType.PostCheck);
        activation.GetData("weakness").Should().Be("fire");
        activation.GetData("behavior").Should().Be("pack-hunter");
    }

    [Test]
    public void AbilityActivation_HuntingGrounds_CreatesCorrectActivation()
    {
        // Act
        var activation = AbilityActivation.HuntingGrounds(TestAreaId, TestAreaName);

        // Assert
        activation.IsValid.Should().BeTrue();
        activation.MarksZone.Should().BeTrue();
        activation.Type.Should().Be(AbilityActivationType.ZoneMarker);
        activation.GetData("areaId").Should().Be(TestAreaId);
        activation.GetData("areaName").Should().Be(TestAreaName);
    }

    [Test]
    public void AbilityActivation_MireKnowledge_CreatesCorrectActivation()
    {
        // Act
        var activation = AbilityActivation.MireKnowledge(
            "Follow the moss-covered stones",
            "toxic gas pocket, quicksand");

        // Assert
        activation.IsValid.Should().BeTrue();
        activation.RevealsPath.Should().BeTrue();
        activation.Type.Should().Be(AbilityActivationType.PathReveal);
        activation.GetData("path").Should().Contain("moss-covered stones");
        activation.GetData("hazardsAvoided").Should().Contain("quicksand");
    }

    [Test]
    public void AbilityActivation_RooftopRoutes_CreatesCorrectActivation()
    {
        // Act
        var activation = AbilityActivation.RooftopRoutes(
            "Jump to the fire escape, cross via the gantry",
            "The abandoned market district");

        // Assert
        activation.IsValid.Should().BeTrue();
        activation.RevealsPath.Should().BeTrue();
        activation.Type.Should().Be(AbilityActivationType.PathReveal);
        activation.GetData("route").Should().Contain("fire escape");
        activation.GetData("destination").Should().Be("The abandoned market district");
    }

    [Test]
    public void AbilityActivation_Empty_CreatesInvalidActivation()
    {
        // Act
        var activation = AbilityActivation.Empty();

        // Assert
        activation.IsValid.Should().BeFalse();
        activation.HasAdditionalData.Should().BeFalse();
    }

    #endregion
}
