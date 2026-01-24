// ------------------------------------------------------------------------------
// <copyright file="BypassSpecializationAbilityServiceTests.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Unit tests for the BypassSpecializationAbilityService, covering passive
// ability modifiers, triggered ability activation, unique action execution,
// trap detection, and masterwork recipe access.
// Part of v0.15.4i Specialization Integration implementation.
// </summary>
// ------------------------------------------------------------------------------

using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for the <see cref="BypassSpecializationAbilityService"/> service.
/// </summary>
/// <remarks>
/// <para>
/// Tests cover the following areas:
/// <list type="bullet">
///   <item><description>Character specialization registration and queries</description></item>
///   <item><description>Passive ability modifier application ([Fast Pick], [Pattern Recognition], [Bypass Under Fire])</description></item>
///   <item><description>Triggered ability activation ([Deep Access])</description></item>
///   <item><description>Unique action execution ([Relock], [Trap Artist])</description></item>
///   <item><description>Trap detection via [Sixth Sense]</description></item>
///   <item><description>Masterwork recipe access via [Master Craftsman]</description></item>
/// </list>
/// </para>
/// </remarks>
[TestFixture]
public class BypassSpecializationAbilityServiceTests
{
    // =========================================================================
    // TEST DEPENDENCIES
    // =========================================================================

    private IDiceService _diceService = null!;
    private ILogger<BypassSpecializationAbilityService> _logger = null!;
    private BypassSpecializationAbilityService _service = null!;

    // =========================================================================
    // CONSTANTS
    // =========================================================================

    private const string TestCharacterId = "test-character-001";
    private const string TestLockId = "lock-001";
    private const string TestTrapId = "trap-001";
    private const string TestTerminalId = "terminal-001";

    // =========================================================================
    // SETUP AND TEARDOWN
    // =========================================================================

    /// <summary>
    /// Sets up the test environment before each test.
    /// </summary>
    [SetUp]
    public void Setup()
    {
        _logger = Substitute.For<ILogger<BypassSpecializationAbilityService>>();
        _diceService = Substitute.For<IDiceService>();

        // Create service under test
        _service = new BypassSpecializationAbilityService(_diceService, _logger);
    }

    // =========================================================================
    // MOCK HELPER METHODS
    // =========================================================================

    /// <summary>
    /// Configures the dice service mock to return a result with specified net successes.
    /// </summary>
    /// <param name="netSuccesses">Net successes to return.</param>
    private void SetupDiceRoll(int netSuccesses)
    {
        var successes = Math.Max(0, netSuccesses);
        var botches = netSuccesses < 0 ? Math.Abs(netSuccesses) : 0;
        var rolls = CreateRollsForNetSuccesses(successes, botches);
        var pool = DicePool.D10(rolls.Count);

        var result = new DiceRollResult(pool, rolls);
        _diceService.Roll(Arg.Any<DicePool>(), Arg.Any<AdvantageType>()).Returns(result);
    }

    /// <summary>
    /// Creates a list of rolls that would produce the specified successes and botches.
    /// </summary>
    private static List<int> CreateRollsForNetSuccesses(int successes, int botches)
    {
        var rolls = new List<int>();

        // Add successes (8, 9, or 10)
        for (var i = 0; i < successes; i++)
        {
            rolls.Add(8 + (i % 3));
        }

        // Add botches (1)
        for (var i = 0; i < botches; i++)
        {
            rolls.Add(1);
        }

        // Pad with neutral values if needed
        if (rolls.Count == 0)
        {
            rolls.Add(5);
        }

        return rolls;
    }

    // =========================================================================
    // SPECIALIZATION REGISTRATION TESTS
    // =========================================================================

    /// <summary>
    /// Verifies that unregistered characters have no specialization.
    /// </summary>
    [Test]
    public void GetCharacterSpecialization_UnregisteredCharacter_ReturnsNone()
    {
        // Act
        var result = _service.GetCharacterSpecialization(TestCharacterId);

        // Assert
        result.Should().Be(BypassSpecialization.None);
    }

    /// <summary>
    /// Verifies that registered characters return their specialization.
    /// </summary>
    [TestCase(BypassSpecialization.ScrapTinker)]
    [TestCase(BypassSpecialization.RuinStalker)]
    [TestCase(BypassSpecialization.JotunReader)]
    [TestCase(BypassSpecialization.GantryRunner)]
    public void GetCharacterSpecialization_RegisteredCharacter_ReturnsSpecialization(
        BypassSpecialization specialization)
    {
        // Arrange
        _service.RegisterCharacterSpecialization(TestCharacterId, specialization);

        // Act
        var result = _service.GetCharacterSpecialization(TestCharacterId);

        // Assert
        result.Should().Be(specialization);
    }

    /// <summary>
    /// Verifies that unregistering removes the specialization.
    /// </summary>
    [Test]
    public void UnregisterCharacter_RegisteredCharacter_RemovesSpecialization()
    {
        // Arrange
        _service.RegisterCharacterSpecialization(TestCharacterId, BypassSpecialization.GantryRunner);

        // Act
        _service.UnregisterCharacter(TestCharacterId);
        var result = _service.GetCharacterSpecialization(TestCharacterId);

        // Assert
        result.Should().Be(BypassSpecialization.None);
    }

    // =========================================================================
    // ABILITY QUERY TESTS
    // =========================================================================

    /// <summary>
    /// Verifies that each specialization returns exactly 2 abilities.
    /// </summary>
    [TestCase(BypassSpecialization.ScrapTinker, "master-craftsman", "relock")]
    [TestCase(BypassSpecialization.RuinStalker, "trap-artist", "sixth-sense")]
    [TestCase(BypassSpecialization.JotunReader, "deep-access", "pattern-recognition")]
    [TestCase(BypassSpecialization.GantryRunner, "fast-pick", "bypass-under-fire")]
    public void GetCharacterAbilities_RegisteredSpecialization_ReturnsTwoAbilities(
        BypassSpecialization specialization,
        string expectedAbility1,
        string expectedAbility2)
    {
        // Arrange
        _service.RegisterCharacterSpecialization(TestCharacterId, specialization);

        // Act
        var abilities = _service.GetCharacterAbilities(TestCharacterId);

        // Assert
        abilities.Should().HaveCount(2);
        abilities.Should().Contain(a => a.AbilityId == expectedAbility1);
        abilities.Should().Contain(a => a.AbilityId == expectedAbility2);
    }

    /// <summary>
    /// Verifies that HasAbility correctly identifies abilities.
    /// </summary>
    [Test]
    public void HasAbility_CharacterHasAbility_ReturnsTrue()
    {
        // Arrange
        _service.RegisterCharacterSpecialization(TestCharacterId, BypassSpecialization.GantryRunner);

        // Act & Assert
        _service.HasAbility(TestCharacterId, "fast-pick").Should().BeTrue();
        _service.HasAbility(TestCharacterId, "bypass-under-fire").Should().BeTrue();
        _service.HasAbility(TestCharacterId, "relock").Should().BeFalse(); // Wrong specialization
    }

    // =========================================================================
    // PASSIVE MODIFIER TESTS
    // =========================================================================

    /// <summary>
    /// Verifies that [Fast Pick] reduces bypass time by 1 round.
    /// </summary>
    [Test]
    public void GetPassiveModifiers_GantryRunner_AppliesFastPick()
    {
        // Arrange
        _service.RegisterCharacterSpecialization(TestCharacterId, BypassSpecialization.GantryRunner);

        // Act
        var context = _service.GetPassiveModifiers(
            TestCharacterId,
            BypassType.Lockpicking,
            isInDanger: false,
            isTargetGlitched: false);

        // Assert
        context.TimeReductionRounds.Should().Be(1);
        context.AppliedAbilities.Should().Contain("fast-pick");
    }

    /// <summary>
    /// Verifies that [Bypass Under Fire] negates combat penalties when in danger.
    /// </summary>
    [Test]
    public void GetPassiveModifiers_GantryRunnerInDanger_AppliesBypassUnderFire()
    {
        // Arrange
        _service.RegisterCharacterSpecialization(TestCharacterId, BypassSpecialization.GantryRunner);

        // Act
        var context = _service.GetPassiveModifiers(
            TestCharacterId,
            BypassType.TerminalHacking,
            isInDanger: true,
            isTargetGlitched: false);

        // Assert
        context.NegateCombatPenalties.Should().BeTrue();
        context.AppliedAbilities.Should().Contain("bypass-under-fire");
        context.AppliedAbilities.Should().Contain("fast-pick"); // Also applies
    }

    /// <summary>
    /// Verifies that [Pattern Recognition] reduces DC when target is glitched.
    /// </summary>
    [Test]
    public void GetPassiveModifiers_JotunReaderGlitchedTarget_AppliesPatternRecognition()
    {
        // Arrange
        _service.RegisterCharacterSpecialization(TestCharacterId, BypassSpecialization.JotunReader);

        // Act
        var context = _service.GetPassiveModifiers(
            TestCharacterId,
            BypassType.GlitchExploitation,
            isInDanger: false,
            isTargetGlitched: true);

        // Assert
        context.DcReduction.Should().Be(2);
        context.AppliedAbilities.Should().Contain("pattern-recognition");
    }

    /// <summary>
    /// Verifies that characters without specialization get no modifiers.
    /// </summary>
    [Test]
    public void GetPassiveModifiers_NoSpecialization_ReturnsEmptyContext()
    {
        // Act
        var context = _service.GetPassiveModifiers(
            TestCharacterId,
            BypassType.Lockpicking,
            isInDanger: true,
            isTargetGlitched: true);

        // Assert
        context.HasModifiers.Should().BeFalse();
        context.AppliedAbilities.Should().BeEmpty();
    }

    // =========================================================================
    // TRAP DETECTION TESTS ([SIXTH SENSE])
    // =========================================================================

    /// <summary>
    /// Verifies that [Sixth Sense] detects traps within 10 feet.
    /// </summary>
    [Test]
    public void CheckTrapDetection_RuinStalkerWithNearbyTraps_DetectsTraps()
    {
        // Arrange
        _service.RegisterCharacterSpecialization(TestCharacterId, BypassSpecialization.RuinStalker);

        var traps = new[]
        {
            new TrapInfo("trap-001", "Pressure Plate", 5, 5, IsArmed: true, IsHidden: true),
            new TrapInfo("trap-002", "Tripwire", 15, 15, IsArmed: true, IsHidden: true) // Too far
        };

        // Act (character at 5, 10 - trap-001 is 5 units away, trap-002 is ~14 units away)
        var result = _service.CheckTrapDetection(TestCharacterId, 5, 10, traps);

        // Assert
        result.HasDetectedTraps.Should().BeTrue();
        result.TrapCount.Should().Be(1);
        result.DetectedTrapIds.Should().Contain("trap-001");
        result.DetectedTrapIds.Should().NotContain("trap-002");
    }

    /// <summary>
    /// Verifies that non-Ruin-Stalkers don't detect traps via [Sixth Sense].
    /// </summary>
    [Test]
    public void CheckTrapDetection_NonRuinStalker_ReturnsAreaClear()
    {
        // Arrange
        _service.RegisterCharacterSpecialization(TestCharacterId, BypassSpecialization.GantryRunner);

        var traps = new[]
        {
            new TrapInfo("trap-001", "Pressure Plate", 5, 5, IsArmed: true, IsHidden: true)
        };

        // Act
        var result = _service.CheckTrapDetection(TestCharacterId, 5, 5, traps);

        // Assert
        result.HasDetectedTraps.Should().BeFalse();
        result.IsAreaClear.Should().BeTrue();
    }

    // =========================================================================
    // TRIGGERED ABILITY TESTS ([DEEP ACCESS])
    // =========================================================================

    /// <summary>
    /// Verifies that [Deep Access] triggers on terminal hack success.
    /// </summary>
    [Test]
    public void TryTriggerAbility_JotunReaderTerminalSuccess_TriggersDeepAccess()
    {
        // Arrange
        _service.RegisterCharacterSpecialization(TestCharacterId, BypassSpecialization.JotunReader);

        // Act
        var result = _service.TryTriggerAbility(
            TestCharacterId,
            BypassType.TerminalHacking,
            wasSuccessful: true,
            TestTerminalId);

        // Assert
        result.Should().NotBeNull();
        result!.Value.Success.Should().BeTrue();
        result.Value.AbilityId.Should().Be("deep-access");
        result.Value.EffectType.Should().Be(BypassEffectType.UpgradeResult);
    }

    /// <summary>
    /// Verifies that [Deep Access] doesn't trigger on failure.
    /// </summary>
    [Test]
    public void TryTriggerAbility_JotunReaderTerminalFailure_NoTrigger()
    {
        // Arrange
        _service.RegisterCharacterSpecialization(TestCharacterId, BypassSpecialization.JotunReader);

        // Act
        var result = _service.TryTriggerAbility(
            TestCharacterId,
            BypassType.TerminalHacking,
            wasSuccessful: false,
            TestTerminalId);

        // Assert
        result.Should().BeNull();
    }

    // =========================================================================
    // UNIQUE ACTION TESTS ([RELOCK])
    // =========================================================================

    /// <summary>
    /// Verifies that [Relock] succeeds when check passes.
    /// </summary>
    [Test]
    public void ExecuteRelock_ScrapTinkerPassesCheck_Succeeds()
    {
        // Arrange
        _service.RegisterCharacterSpecialization(TestCharacterId, BypassSpecialization.ScrapTinker);
        _service.MarkLockAsPicked(TestLockId, TestCharacterId);
        SetupDiceRoll(15); // Passes DC 12

        // Act
        var result = _service.ExecuteRelock(TestCharacterId, TestLockId, witsScore: 3);

        // Assert
        result.Success.Should().BeTrue();
        result.AbilityId.Should().Be("relock");
    }

    /// <summary>
    /// Verifies that [Relock] fails when check fails.
    /// </summary>
    [Test]
    public void ExecuteRelock_ScrapTinkerFailsCheck_Fails()
    {
        // Arrange
        _service.RegisterCharacterSpecialization(TestCharacterId, BypassSpecialization.ScrapTinker);
        _service.MarkLockAsPicked(TestLockId, TestCharacterId);
        SetupDiceRoll(5); // Fails DC 12

        // Act
        var result = _service.ExecuteRelock(TestCharacterId, TestLockId, witsScore: 2);

        // Assert
        result.Success.Should().BeFalse();
        result.HasFailureReason.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that [Relock] fails without proper specialization.
    /// </summary>
    [Test]
    public void ExecuteRelock_WrongSpecialization_Fails()
    {
        // Arrange
        _service.RegisterCharacterSpecialization(TestCharacterId, BypassSpecialization.GantryRunner);
        _service.MarkLockAsPicked(TestLockId, TestCharacterId);

        // Act
        var result = _service.ExecuteRelock(TestCharacterId, TestLockId, witsScore: 5);

        // Assert
        result.Success.Should().BeFalse();
        result.FailureReason.Should().Contain("Scrap");
    }

    // =========================================================================
    // UNIQUE ACTION TESTS ([TRAP ARTIST])
    // =========================================================================

    /// <summary>
    /// Verifies that [Trap Artist] succeeds when check passes.
    /// </summary>
    [Test]
    public void ExecuteTrapArtist_RuinStalkerPassesCheck_Succeeds()
    {
        // Arrange
        _service.RegisterCharacterSpecialization(TestCharacterId, BypassSpecialization.RuinStalker);
        _service.MarkTrapAsDisabled(TestTrapId, TestCharacterId);
        SetupDiceRoll(16); // Passes DC 14

        // Act
        var result = _service.ExecuteTrapArtist(TestCharacterId, TestTrapId, witsScore: 4);

        // Assert
        result.Success.Should().BeTrue();
        result.AbilityId.Should().Be("trap-artist");
    }

    /// <summary>
    /// Verifies that [Trap Artist] fails without disabled trap.
    /// </summary>
    [Test]
    public void ExecuteTrapArtist_TrapNotDisabled_Fails()
    {
        // Arrange
        _service.RegisterCharacterSpecialization(TestCharacterId, BypassSpecialization.RuinStalker);
        // Note: NOT marking trap as disabled

        // Act
        var result = _service.ExecuteTrapArtist(TestCharacterId, TestTrapId, witsScore: 5);

        // Assert
        result.Success.Should().BeFalse();
        result.FailureReason.Should().Contain("disabled first");
    }

    // =========================================================================
    // MASTERWORK RECIPE TESTS ([MASTER CRAFTSMAN])
    // =========================================================================

    /// <summary>
    /// Verifies that Scrap-Tinkers have access to masterwork recipes.
    /// </summary>
    [Test]
    public void GetAvailableMasterworkRecipes_ScrapTinker_ReturnsRecipes()
    {
        // Arrange
        _service.RegisterCharacterSpecialization(TestCharacterId, BypassSpecialization.ScrapTinker);

        // Act
        var recipes = _service.GetAvailableMasterworkRecipes(TestCharacterId);

        // Assert
        recipes.Should().NotBeEmpty();
        recipes.Should().Contain(r => r.RecipeId == "masterwork-shim-picks");
        recipes.Should().Contain(r => r.RecipeId == "masterwork-wire-probe");
    }

    /// <summary>
    /// Verifies that non-Scrap-Tinkers don't have access to masterwork recipes.
    /// </summary>
    [Test]
    public void GetAvailableMasterworkRecipes_NonScrapTinker_ReturnsEmpty()
    {
        // Arrange
        _service.RegisterCharacterSpecialization(TestCharacterId, BypassSpecialization.JotunReader);

        // Act
        var recipes = _service.GetAvailableMasterworkRecipes(TestCharacterId);

        // Assert
        recipes.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies that CanCraftMasterwork checks components.
    /// </summary>
    [Test]
    public void CanCraftMasterwork_HasComponents_ReturnsTrue()
    {
        // Arrange
        _service.RegisterCharacterSpecialization(TestCharacterId, BypassSpecialization.ScrapTinker);
        var components = new[] { "Fine Wire", "Precision Springs", "Grip Material" };

        // Act
        var canCraft = _service.CanCraftMasterwork(
            TestCharacterId,
            "masterwork-shim-picks",
            components);

        // Assert
        canCraft.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that CanCraftMasterwork fails without components.
    /// </summary>
    [Test]
    public void CanCraftMasterwork_MissingComponents_ReturnsFalse()
    {
        // Arrange
        _service.RegisterCharacterSpecialization(TestCharacterId, BypassSpecialization.ScrapTinker);
        var components = new[] { "Fine Wire" }; // Missing other components

        // Act
        var canCraft = _service.CanCraftMasterwork(
            TestCharacterId,
            "masterwork-shim-picks",
            components);

        // Assert
        canCraft.Should().BeFalse();
    }

    // =========================================================================
    // VALUE OBJECT TESTS
    // =========================================================================

    /// <summary>
    /// Verifies that BypassSpecializationAbility static factories create correct abilities.
    /// </summary>
    [Test]
    public void BypassSpecializationAbility_StaticFactories_CreateCorrectAbilities()
    {
        // Act & Assert
        var fastPick = BypassSpecializationAbility.FastPick();
        fastPick.AbilityId.Should().Be("fast-pick");
        fastPick.Specialization.Should().Be(BypassSpecialization.GantryRunner);
        fastPick.AbilityType.Should().Be(BypassAbilityType.Passive);
        fastPick.EffectType.Should().Be(BypassEffectType.TimeReduction);
        fastPick.EffectMagnitude.Should().Be(1);

        var sixthSense = BypassSpecializationAbility.SixthSense();
        sixthSense.AbilityId.Should().Be("sixth-sense");
        sixthSense.Specialization.Should().Be(BypassSpecialization.RuinStalker);
        sixthSense.EffectType.Should().Be(BypassEffectType.AutoDetection);
        sixthSense.EffectMagnitude.Should().Be(10); // 10 ft radius

        var relock = BypassSpecializationAbility.Relock();
        relock.AbilityType.Should().Be(BypassAbilityType.UniqueAction);
        relock.RequiresCheck.Should().BeTrue();
        relock.CheckDc.Should().Be(12);
        relock.CheckAttribute.Should().Be("WITS");
    }

    /// <summary>
    /// Verifies that GetAbilitiesFor returns correct abilities for each specialization.
    /// </summary>
    [Test]
    public void BypassSpecializationAbility_GetAbilitiesFor_ReturnsCorrectAbilities()
    {
        // Act & Assert
        BypassSpecializationAbility.GetAbilitiesFor(BypassSpecialization.None)
            .Should().BeEmpty();

        BypassSpecializationAbility.GetAbilitiesFor(BypassSpecialization.ScrapTinker)
            .Should().HaveCount(2)
            .And.Contain(a => a.AbilityId == "master-craftsman")
            .And.Contain(a => a.AbilityId == "relock");

        BypassSpecializationAbility.GetAll()
            .Should().HaveCount(8); // 4 specializations x 2 abilities each
    }

    /// <summary>
    /// Verifies that MasterworkRecipe static factories create correct recipes.
    /// </summary>
    [Test]
    public void MasterworkRecipe_StaticFactories_CreateCorrectRecipes()
    {
        // Act & Assert
        var shimPicks = MasterworkRecipe.ShimPicks();
        shimPicks.RecipeId.Should().Be("masterwork-shim-picks");
        shimPicks.BypassType.Should().Be(BypassType.Lockpicking);
        shimPicks.CraftingDc.Should().Be(14);
        shimPicks.BonusDice.Should().Be(1);
        shimPicks.RequiredComponents.Should().HaveCount(3);

        MasterworkRecipe.GetAll().Should().HaveCount(4);
    }

    /// <summary>
    /// Verifies that TrapDetectionResult correctly identifies detected traps.
    /// </summary>
    [Test]
    public void TrapDetectionResult_TrapsDetected_HasCorrectProperties()
    {
        // Arrange
        var detectedTraps = new[]
        {
            TrapDetectionResult.CreateDetectedTrap("trap-001", "Spike Pit", 5, 5, 5, 10)
        };

        // Act
        var result = TrapDetectionResult.TrapsDetected(TestCharacterId, 5, 10, detectedTraps);

        // Assert
        result.HasDetectedTraps.Should().BeTrue();
        result.TrapCount.Should().Be(1);
        result.DetectionRadiusFeet.Should().Be(10);
        result.IsAreaClear.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that BypassAbilityContext correctly reports modifiers.
    /// </summary>
    [Test]
    public void BypassAbilityContext_WithModifiers_HasCorrectProperties()
    {
        // Arrange
        var context = new BypassAbilityContext(
            TimeReductionRounds: 1,
            DcReduction: 2,
            NegateCombatPenalties: true,
            AppliedAbilities: new[] { "fast-pick", "pattern-recognition" });

        // Assert
        context.HasModifiers.Should().BeTrue();
        context.TimeReductionRounds.Should().Be(1);
        context.DcReduction.Should().Be(2);
        context.NegateCombatPenalties.Should().BeTrue();
    }
}
