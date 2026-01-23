using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Extensions;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for <see cref="SpecializationAbilityService"/>.
/// </summary>
/// <remarks>
/// Tests cover:
/// <list type="bullet">
///   <item><description>Gantry-Runner abilities: stage reduction, distance bonus, reroll, auto-success</description></item>
///   <item><description>Myrk-gengr abilities: hidden entry, maintain hidden, party bonus, zone auto-hidden</description></item>
///   <item><description>Use tracking: daily and encounter limits</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class SpecializationAbilityServiceTests
{
    private ILogger<SpecializationAbilityService> _logger = null!;
    private SpecializationAbilityService _sut = null!;
    private const string TestCharacterId = "test-character";

    [SetUp]
    public void SetUp()
    {
        _logger = Substitute.For<ILogger<SpecializationAbilityService>>();
        _sut = new SpecializationAbilityService(_logger);
    }

    #region Ability Check Tests

    [Test]
    public void HasAbility_WithoutAbility_ReturnsFalse()
    {
        // Act
        var result = _sut.HasAbility(TestCharacterId, GantryRunnerAbility.RoofRunner);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void HasAbility_WithGrantedAbility_ReturnsTrue()
    {
        // Arrange
        _sut.GrantAbility(TestCharacterId, GantryRunnerAbility.RoofRunner);

        // Act
        var result = _sut.HasAbility(TestCharacterId, GantryRunnerAbility.RoofRunner);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Gantry-Runner: Roof-Runner Tests

    [Test]
    public void ApplyRoofRunner_ReducesClimbingStagesByOne()
    {
        // Arrange
        _sut.GrantAbility(TestCharacterId, GantryRunnerAbility.RoofRunner);

        // Act
        var result = _sut.ApplyRoofRunner(TestCharacterId, originalStages: 3);

        // Assert
        result.Success.Should().BeTrue();
        result.Modifier.Should().Be(-1);
        result.EffectDescription.Should().Contain("reduced from 3 to 2");
    }

    [Test]
    public void ApplyRoofRunner_MinimumOneStage()
    {
        // Arrange
        _sut.GrantAbility(TestCharacterId, GantryRunnerAbility.RoofRunner);

        // Act
        var result = _sut.ApplyRoofRunner(TestCharacterId, originalStages: 1);

        // Assert
        result.Success.Should().BeTrue();
        result.EffectDescription.Should().Contain("reduced from 1 to 1");
    }

    [Test]
    public void ApplyRoofRunner_WithoutAbility_Fails()
    {
        // Act
        var result = _sut.ApplyRoofRunner(TestCharacterId, originalStages: 3);

        // Assert
        result.Success.Should().BeFalse();
        result.FailureReason.Should().Contain("does not have");
    }

    #endregion

    #region Gantry-Runner: Death-Defying Leap Tests

    [Test]
    public void ApplyDeathDefyingLeap_AddsTenFeetToMaxDistance()
    {
        // Arrange
        _sut.GrantAbility(TestCharacterId, GantryRunnerAbility.DeathDefyingLeap);

        // Act
        var result = _sut.ApplyDeathDefyingLeap(TestCharacterId, baseMaxDistance: 25);

        // Assert
        result.Success.Should().BeTrue();
        result.Modifier.Should().Be(10);
        result.EffectDescription.Should().Contain("increased by 10 ft");
    }

    #endregion

    #region Gantry-Runner: Wall-Run Tests

    [Test]
    public void ActivateWallRun_ReturnsSuccess()
    {
        // Arrange
        _sut.GrantAbility(TestCharacterId, GantryRunnerAbility.WallRun);

        // Act
        var result = _sut.ActivateWallRun(TestCharacterId, heightFeet: 20, dicePool: 6);

        // Assert
        result.Success.Should().BeTrue();
        result.EffectDescription.Should().Contain("20 ft vertical");
        result.EffectDescription.Should().Contain("DC 3");
    }

    #endregion

    #region Gantry-Runner: Double Jump Tests

    [Test]
    public void ActivateDoubleJump_GrantsRerollWithBonus()
    {
        // Arrange
        _sut.GrantAbility(TestCharacterId, GantryRunnerAbility.DoubleJump);

        // Act
        var result = _sut.ActivateDoubleJump(TestCharacterId);

        // Assert
        result.Success.Should().BeTrue();
        result.DiceBonus.Should().Be(1);
        result.UsesRemaining.Should().Be(0);
    }

    [Test]
    public void ActivateDoubleJump_SecondUse_Fails()
    {
        // Arrange
        _sut.GrantAbility(TestCharacterId, GantryRunnerAbility.DoubleJump);
        _sut.ActivateDoubleJump(TestCharacterId); // Use first

        // Act
        var result = _sut.ActivateDoubleJump(TestCharacterId);

        // Assert
        result.Success.Should().BeFalse();
        result.FailureReason.Should().Contain("used maximum times");
    }

    [Test]
    public void ActivateDoubleJump_AfterReset_WorksAgain()
    {
        // Arrange
        _sut.GrantAbility(TestCharacterId, GantryRunnerAbility.DoubleJump);
        _sut.ActivateDoubleJump(TestCharacterId); // Use
        _sut.ResetDailyUses(TestCharacterId);

        // Act
        var result = _sut.ActivateDoubleJump(TestCharacterId);

        // Assert
        result.Success.Should().BeTrue();
    }

    #endregion

    #region Gantry-Runner: Featherfall Tests

    [Test]
    public void CheckFeatherfall_DcThreeOrLess_AutoSucceeds()
    {
        // Arrange
        _sut.GrantAbility(TestCharacterId, GantryRunnerAbility.Featherfall);

        // Act
        var result = _sut.CheckFeatherfall(TestCharacterId, crashLandingDc: 3);

        // Assert
        result.Success.Should().BeTrue();
        result.EffectDescription.Should().Contain("Auto-succeeded");
    }

    [Test]
    public void CheckFeatherfall_DcGreaterThanThree_FailsThreshold()
    {
        // Arrange
        _sut.GrantAbility(TestCharacterId, GantryRunnerAbility.Featherfall);

        // Act
        var result = _sut.CheckFeatherfall(TestCharacterId, crashLandingDc: 4);

        // Assert
        result.Success.Should().BeFalse();
        result.FailureReason.Should().Contain("DC ≤ 3");
    }

    #endregion

    #region Myrk-gengr: Slip into Shadow Tests

    [Test]
    public void ActivateSlipIntoShadow_InShadows_EntersHiddenWithoutAction()
    {
        // Arrange
        _sut.GrantAbility(TestCharacterId, MyrkengrAbility.SlipIntoShadow);

        // Act
        var result = _sut.ActivateSlipIntoShadow(TestCharacterId, isInShadows: true, isObserved: false);

        // Assert
        result.Success.Should().BeTrue();
        result.EffectDescription.Should().Contain("without using an action");
    }

    [Test]
    public void ActivateSlipIntoShadow_NotInShadows_Fails()
    {
        // Arrange
        _sut.GrantAbility(TestCharacterId, MyrkengrAbility.SlipIntoShadow);

        // Act
        var result = _sut.ActivateSlipIntoShadow(TestCharacterId, isInShadows: false, isObserved: false);

        // Assert
        result.Success.Should().BeFalse();
        result.FailureReason.Should().Contain("Not in shadows");
    }

    [Test]
    public void ActivateSlipIntoShadow_WhenObserved_Fails()
    {
        // Arrange
        _sut.GrantAbility(TestCharacterId, MyrkengrAbility.SlipIntoShadow);

        // Act
        var result = _sut.ActivateSlipIntoShadow(TestCharacterId, isInShadows: true, isObserved: true);

        // Assert
        result.Success.Should().BeFalse();
        result.FailureReason.Should().Contain("observed");
    }

    #endregion

    #region Myrk-gengr: Ghostly Form Tests

    [Test]
    public void ActivateGhostlyForm_MaintainsHiddenAfterAttack()
    {
        // Arrange
        _sut.GrantAbility(TestCharacterId, MyrkengrAbility.GhostlyForm);

        // Act
        var result = _sut.ActivateGhostlyForm(TestCharacterId);

        // Assert
        result.Success.Should().BeTrue();
        result.EffectDescription.Should().Contain("Remained [Hidden]");
        result.UsesRemaining.Should().Be(0);
    }

    [Test]
    public void ActivateGhostlyForm_SecondUseInEncounter_Fails()
    {
        // Arrange
        _sut.GrantAbility(TestCharacterId, MyrkengrAbility.GhostlyForm);
        _sut.ActivateGhostlyForm(TestCharacterId); // Use first

        // Act
        var result = _sut.ActivateGhostlyForm(TestCharacterId);

        // Assert
        result.Success.Should().BeFalse();
    }

    [Test]
    public void ActivateGhostlyForm_AfterEncounterReset_WorksAgain()
    {
        // Arrange
        _sut.GrantAbility(TestCharacterId, MyrkengrAbility.GhostlyForm);
        _sut.ActivateGhostlyForm(TestCharacterId); // Use
        _sut.ResetEncounterUses(TestCharacterId);

        // Act
        var result = _sut.ActivateGhostlyForm(TestCharacterId);

        // Assert
        result.Success.Should().BeTrue();
    }

    #endregion

    #region Myrk-gengr: Cloak the Party Tests

    [Test]
    public void ActivateCloakTheParty_GrantsPartyDiceBonus()
    {
        // Arrange
        _sut.GrantAbility(TestCharacterId, MyrkengrAbility.CloakTheParty);
        var partyMembers = new List<string> { "ally-1", "ally-2", "ally-3" };

        // Act
        var result = _sut.ActivateCloakTheParty(TestCharacterId, partyMembers);

        // Assert
        result.Success.Should().BeTrue();
        result.DiceBonus.Should().Be(2);
        result.EffectDescription.Should().Contain("3 party members");
    }

    #endregion

    #region Myrk-gengr: One with the Static Tests

    [Test]
    public void CheckOneWithTheStatic_InPsychicResonance_AutoEntersHidden()
    {
        // Arrange
        _sut.GrantAbility(TestCharacterId, MyrkengrAbility.OneWithTheStatic);

        // Act
        var result = _sut.CheckOneWithTheStatic(TestCharacterId, isInPsychicResonance: true);

        // Assert
        result.Success.Should().BeTrue();
        result.EffectDescription.Should().Contain("without using an action");
    }

    [Test]
    public void CheckOneWithTheStatic_NotInZone_Fails()
    {
        // Arrange
        _sut.GrantAbility(TestCharacterId, MyrkengrAbility.OneWithTheStatic);

        // Act
        var result = _sut.CheckOneWithTheStatic(TestCharacterId, isInPsychicResonance: false);

        // Assert
        result.Success.Should().BeFalse();
        result.FailureReason.Should().Contain("Psychic Resonance");
    }

    #endregion

    #region Extension Method Tests

    [Test]
    public void GantryRunnerAbilityExtensions_GetDisplayName_ReturnsCorrectFormat()
    {
        // Act & Assert
        GantryRunnerAbility.RoofRunner.GetDisplayName().Should().Be("[Roof-Runner]");
        GantryRunnerAbility.DeathDefyingLeap.GetDisplayName().Should().Be("[Death-Defying Leap]");
        GantryRunnerAbility.WallRun.GetDisplayName().Should().Be("[Wall-Run]");
        GantryRunnerAbility.DoubleJump.GetDisplayName().Should().Be("[Double Jump]");
        GantryRunnerAbility.Featherfall.GetDisplayName().Should().Be("[Featherfall]");
    }

    [Test]
    public void GantryRunnerAbilityExtensions_GetAbilityType_ReturnsCorrectType()
    {
        // Act & Assert
        GantryRunnerAbility.RoofRunner.GetAbilityType().Should().Be(SpecializationAbilityType.Passive);
        GantryRunnerAbility.WallRun.GetAbilityType().Should().Be(SpecializationAbilityType.Active);
        GantryRunnerAbility.DoubleJump.GetAbilityType().Should().Be(SpecializationAbilityType.Reactive);
    }

    [Test]
    public void MyrkengrAbilityExtensions_GetDisplayName_ReturnsCorrectFormat()
    {
        // Act & Assert
        MyrkengrAbility.SlipIntoShadow.GetDisplayName().Should().Be("[Slip into Shadow]");
        MyrkengrAbility.GhostlyForm.GetDisplayName().Should().Be("[Ghostly Form]");
        MyrkengrAbility.CloakTheParty.GetDisplayName().Should().Be("[Cloak the Party]");
        MyrkengrAbility.OneWithTheStatic.GetDisplayName().Should().Be("[One with the Static]");
    }

    [Test]
    public void MyrkengrAbilityExtensions_GetAbilityType_ReturnsCorrectType()
    {
        // Act & Assert
        MyrkengrAbility.SlipIntoShadow.GetAbilityType().Should().Be(SpecializationAbilityType.Triggered);
        MyrkengrAbility.GhostlyForm.GetAbilityType().Should().Be(SpecializationAbilityType.Reactive);
        MyrkengrAbility.CloakTheParty.GetAbilityType().Should().Be(SpecializationAbilityType.Active);
        MyrkengrAbility.OneWithTheStatic.GetAbilityType().Should().Be(SpecializationAbilityType.Passive);
    }

    #endregion

    #region Value Object Tests

    [Test]
    public void SpecializationAbilityDefinition_FromGantryRunner_CreatesCorrectDefinition()
    {
        // Act
        var definition = SpecializationAbilityDefinition.FromGantryRunner(GantryRunnerAbility.RoofRunner);

        // Assert
        definition.DisplayName.Should().Be("[Roof-Runner]");
        definition.SpecializationId.Should().Be("gantry-runner");
        definition.AbilityType.Should().Be(SpecializationAbilityType.Passive);
        definition.IsPassive.Should().BeTrue();
    }

    [Test]
    public void SpecializationAbilityDefinition_FromMyrkengr_CreatesCorrectDefinition()
    {
        // Act
        var definition = SpecializationAbilityDefinition.FromMyrkengr(MyrkengrAbility.GhostlyForm);

        // Assert
        definition.DisplayName.Should().Be("[Ghostly Form]");
        definition.SpecializationId.Should().Be("myrk-gengr");
        definition.AbilityType.Should().Be(SpecializationAbilityType.Reactive);
        definition.HasEncounterLimit.Should().BeTrue();
        definition.EncounterUses.Should().Be(1);
    }

    [Test]
    public void AbilityActivationContext_ForClimbing_SetsCorrectTrigger()
    {
        // Act
        var context = AbilityActivationContext.ForClimbing("char-1", "roof-runner", height: 30);

        // Assert
        context.TriggerType.Should().Be(AbilityTriggerType.Climbing);
        context.CurrentHeight.Should().Be(30);
    }

    [Test]
    public void AbilityActivationContext_ForParty_IncludesPartyMembers()
    {
        // Arrange
        var partyIds = new List<string> { "ally-1", "ally-2" };

        // Act
        var context = AbilityActivationContext.ForParty("char-1", "cloak-the-party", partyIds);

        // Assert
        context.HasPartyTargets.Should().BeTrue();
        context.PartyMembersInRange.Should().Be(2);
    }

    [Test]
    public void AbilityActivationResult_Success_HasCorrectProperties()
    {
        // Act
        var result = AbilityActivationResult.StageReduction("char-1", "roof-runner", 1, 3, 2);

        // Assert
        result.Success.Should().BeTrue();
        result.HasModifier.Should().BeTrue();
        result.Modifier.Should().Be(-1);
    }

    [Test]
    public void AbilityActivationResult_Failure_HasFailureReason()
    {
        // Act
        var result = AbilityActivationResult.NotAvailable("char-1", "unknown");

        // Assert
        result.Success.Should().BeFalse();
        result.FailureReason.Should().NotBeNullOrEmpty();
    }

    #endregion
}
