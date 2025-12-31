using System;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using Xunit;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Events;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models.Magic;
using RuneAndRust.Engine.Services;

namespace RuneAndRust.Tests.Services;

public class ResonanceServiceTests
{
    private readonly Mock<ILogger<ResonanceService>> _loggerMock;
    private readonly Mock<IEventBus> _eventBusMock;
    private readonly ResonanceService _sut;

    public ResonanceServiceTests()
    {
        _loggerMock = new Mock<ILogger<ResonanceService>>();
        _eventBusMock = new Mock<IEventBus>();
        _sut = new ResonanceService(_loggerMock.Object, _eventBusMock.Object);
    }

    private static Character CreateMystic(string name = "TestMystic", int resonance = 0)
    {
        return new Character
        {
            Id = Guid.NewGuid(),
            Name = name,
            Archetype = ArchetypeType.Mystic,
            ResonanceState = new ResonanceState { CurrentValue = resonance }
        };
    }

    private static Character CreateWarrior(string name = "TestWarrior")
    {
        return new Character
        {
            Id = Guid.NewGuid(),
            Name = name,
            Archetype = ArchetypeType.Warrior
        };
    }

    #region ModifyResonance Tests

    [Fact]
    public void ModifyResonance_WithPositiveAmount_IncreasesResonance()
    {
        // Arrange
        var character = CreateMystic(resonance: 20);

        // Act
        var result = _sut.ModifyResonance(character, 15, "Spell cast");

        // Assert
        result.NewValue.Should().Be(35);
        result.ActualAmount.Should().Be(15);
        result.IsIncrease.Should().BeTrue();
        character.ResonanceState!.CurrentValue.Should().Be(35);
    }

    [Fact]
    public void ModifyResonance_WithNegativeAmount_DecreasesResonance()
    {
        // Arrange
        var character = CreateMystic(resonance: 50);

        // Act
        var result = _sut.ModifyResonance(character, -20, "Decay");

        // Assert
        result.NewValue.Should().Be(30);
        result.ActualAmount.Should().Be(-20);
        result.IsDecrease.Should().BeTrue();
    }

    [Fact]
    public void ModifyResonance_ExceedingMax_ClampsTo100()
    {
        // Arrange
        var character = CreateMystic(resonance: 90);

        // Act
        var result = _sut.ModifyResonance(character, 20, "Power surge");

        // Assert
        result.NewValue.Should().Be(100);
        result.ActualAmount.Should().Be(10);
        result.WasClamped.Should().BeTrue();
        result.OverflowTriggered.Should().BeTrue();
    }

    [Fact]
    public void ModifyResonance_BelowMin_ClampsTo0()
    {
        // Arrange
        var character = CreateMystic(resonance: 15);

        // Act
        var result = _sut.ModifyResonance(character, -30, "Purge");

        // Assert
        result.NewValue.Should().Be(0);
        result.ActualAmount.Should().Be(-15);
        result.WasClamped.Should().BeTrue();
        result.FullyDissipated.Should().BeTrue();
    }

    [Fact]
    public void ModifyResonance_ForNonMystic_ReturnsNoChange()
    {
        // Arrange
        var character = CreateWarrior();

        // Act
        var result = _sut.ModifyResonance(character, 10, "Test");

        // Assert
        result.Should().BeEquivalentTo(ResonanceResult.NoChange("Test"));
    }

    [Fact]
    public void ModifyResonance_WithNullResonanceState_InitializesState()
    {
        // Arrange
        var character = CreateMystic();
        character.ResonanceState = null;

        // Act
        var result = _sut.ModifyResonance(character, 25, "First cast");

        // Assert
        character.ResonanceState.Should().NotBeNull();
        result.NewValue.Should().Be(25);
    }

    [Fact]
    public void ModifyResonance_WithZeroAmount_ReturnsCurrentState()
    {
        // Arrange
        var character = CreateMystic(resonance: 40);

        // Act
        var result = _sut.ModifyResonance(character, 0, "No change");

        // Assert
        result.NewValue.Should().Be(40);
        result.ActualAmount.Should().Be(0);
        result.ThresholdChanged.Should().BeFalse();
    }

    [Fact]
    public void ModifyResonance_RecordsSource()
    {
        // Arrange
        var character = CreateMystic(resonance: 30);
        var source = "Fireball cast";

        // Act
        var result = _sut.ModifyResonance(character, 10, source);

        // Assert
        result.Source.Should().Be(source);
    }

    #endregion

    #region Threshold Detection Tests

    [Theory]
    [InlineData(0, ResonanceThreshold.Dim)]
    [InlineData(24, ResonanceThreshold.Dim)]
    [InlineData(25, ResonanceThreshold.Steady)]
    [InlineData(49, ResonanceThreshold.Steady)]
    [InlineData(50, ResonanceThreshold.Bright)]
    [InlineData(74, ResonanceThreshold.Bright)]
    [InlineData(75, ResonanceThreshold.Blazing)]
    [InlineData(99, ResonanceThreshold.Blazing)]
    [InlineData(100, ResonanceThreshold.Overflow)]
    public void GetThreshold_ReturnsCorrectThreshold(int resonance, ResonanceThreshold expected)
    {
        // Arrange
        var character = CreateMystic(resonance: resonance);

        // Act
        var result = _sut.GetThreshold(character);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void ModifyResonance_CrossingThresholdUp_SetsThresholdChanged()
    {
        // Arrange
        var character = CreateMystic(resonance: 24); // Dim

        // Act
        var result = _sut.ModifyResonance(character, 1, "Test"); // -> 25 (Steady)

        // Assert
        result.ThresholdChanged.Should().BeTrue();
        result.PreviousThreshold.Should().Be(ResonanceThreshold.Dim);
        result.NewThreshold.Should().Be(ResonanceThreshold.Steady);
    }

    [Fact]
    public void ModifyResonance_CrossingThresholdDown_SetsThresholdChanged()
    {
        // Arrange
        var character = CreateMystic(resonance: 50); // Bright

        // Act
        var result = _sut.ModifyResonance(character, -1, "Test"); // -> 49 (Steady)

        // Assert
        result.ThresholdChanged.Should().BeTrue();
        result.PreviousThreshold.Should().Be(ResonanceThreshold.Bright);
        result.NewThreshold.Should().Be(ResonanceThreshold.Steady);
    }

    [Fact]
    public void ModifyResonance_WithinSameThreshold_ThresholdUnchanged()
    {
        // Arrange
        var character = CreateMystic(resonance: 30); // Steady

        // Act
        var result = _sut.ModifyResonance(character, 10, "Test"); // -> 40 (still Steady)

        // Assert
        result.ThresholdChanged.Should().BeFalse();
        result.PreviousThreshold.Should().Be(ResonanceThreshold.Steady);
        result.NewThreshold.Should().Be(ResonanceThreshold.Steady);
    }

    [Fact]
    public void ModifyResonance_CrossingMultipleThresholds_ReportsCorrectThresholds()
    {
        // Arrange
        var character = CreateMystic(resonance: 20); // Dim

        // Act
        var result = _sut.ModifyResonance(character, 60, "Power surge"); // -> 80 (Blazing)

        // Assert
        result.ThresholdChanged.Should().BeTrue();
        result.PreviousThreshold.Should().Be(ResonanceThreshold.Dim);
        result.NewThreshold.Should().Be(ResonanceThreshold.Blazing);
    }

    [Fact]
    public void GetThreshold_ForNonMystic_ReturnsDim()
    {
        // Arrange
        var character = CreateWarrior();

        // Act
        var result = _sut.GetThreshold(character);

        // Assert
        result.Should().Be(ResonanceThreshold.Dim);
    }

    #endregion

    #region Potency Calculation Tests

    [Theory]
    [InlineData(0, 0.90)]
    [InlineData(24, 0.90)]
    [InlineData(25, 1.00)]
    [InlineData(49, 1.00)]
    [InlineData(50, 1.15)]
    [InlineData(74, 1.15)]
    [InlineData(75, 1.30)]
    [InlineData(99, 1.30)]
    [InlineData(100, 1.50)]
    public void GetPotencyModifier_ReturnsCorrectMultiplier(int resonance, decimal expected)
    {
        // Arrange
        var character = CreateMystic(resonance: resonance);

        // Act
        var result = _sut.GetPotencyModifier(character);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void GetPotencyModifier_ForNonMystic_Returns090()
    {
        // Arrange
        var character = CreateWarrior();

        // Act
        var result = _sut.GetPotencyModifier(character);

        // Assert
        result.Should().Be(0.90m); // Dim threshold default
    }

    [Fact]
    public void GetPotencyModifier_AtDimThreshold_AppliesPenalty()
    {
        // Arrange
        var character = CreateMystic(resonance: 10);

        // Act
        var modifier = _sut.GetPotencyModifier(character);
        var baseDamage = 100;
        var modifiedDamage = (int)(baseDamage * modifier);

        // Assert
        modifier.Should().Be(0.90m);
        modifiedDamage.Should().Be(90);
    }

    [Fact]
    public void GetPotencyModifier_AtOverflow_GrantsMaxBonus()
    {
        // Arrange
        var character = CreateMystic(resonance: 100);

        // Act
        var modifier = _sut.GetPotencyModifier(character);
        var baseDamage = 100;
        var modifiedDamage = (int)(baseDamage * modifier);

        // Assert
        modifier.Should().Be(1.50m);
        modifiedDamage.Should().Be(150);
    }

    [Fact]
    public void ResonanceState_PotencyModifier_MatchesServiceCalculation()
    {
        // Arrange
        var character = CreateMystic(resonance: 60);

        // Act
        var serviceModifier = _sut.GetPotencyModifier(character);
        var stateModifier = character.ResonanceState!.PotencyModifier;

        // Assert
        serviceModifier.Should().Be(stateModifier);
        serviceModifier.Should().Be(1.15m);
    }

    #endregion

    #region Casting Mode Tests

    [Fact]
    public void ApplyCastingModeModifiers_Quick_ReturnsCorrectValues()
    {
        // Act
        var result = _sut.ApplyCastingModeModifiers(CastingMode.Quick);

        // Assert
        result.Mode.Should().Be(CastingMode.Quick);
        result.ResonanceGain.Should().Be(15);
        result.CastTimeModifier.Should().Be(0);
        result.FluxModifier.Should().Be(5);
        result.IsBonusAction.Should().BeTrue();
    }

    [Fact]
    public void ApplyCastingModeModifiers_Standard_ReturnsCorrectValues()
    {
        // Act
        var result = _sut.ApplyCastingModeModifiers(CastingMode.Standard);

        // Assert
        result.Mode.Should().Be(CastingMode.Standard);
        result.ResonanceGain.Should().Be(10);
        result.CastTimeModifier.Should().Be(1);
        result.FluxModifier.Should().Be(0);
        result.IsBonusAction.Should().BeFalse();
    }

    [Fact]
    public void ApplyCastingModeModifiers_Channeled_ReturnsCorrectValues()
    {
        // Act
        var result = _sut.ApplyCastingModeModifiers(CastingMode.Channeled);

        // Assert
        result.Mode.Should().Be(CastingMode.Channeled);
        result.ResonanceGain.Should().Be(5);
        result.CastTimeModifier.Should().Be(2);
        result.FluxModifier.Should().Be(-5);
        result.IsExtendedCast.Should().BeTrue();
    }

    [Fact]
    public void ApplyCastingModeModifiers_Ritual_ReturnsCorrectValues()
    {
        // Act
        var result = _sut.ApplyCastingModeModifiers(CastingMode.Ritual);

        // Assert
        result.Mode.Should().Be(CastingMode.Ritual);
        result.ResonanceGain.Should().Be(0);
        result.CastTimeModifier.Should().Be(-1);
        result.FluxModifier.Should().Be(-10);
        result.IsOutOfCombatOnly.Should().BeTrue();
    }

    [Fact]
    public void CastingModeResult_Description_ReturnsCorrectText()
    {
        // Arrange & Act
        var quick = _sut.ApplyCastingModeModifiers(CastingMode.Quick);
        var ritual = _sut.ApplyCastingModeModifiers(CastingMode.Ritual);

        // Assert
        quick.Description.Should().Contain("bonus action");
        ritual.Description.Should().Contain("out of combat");
    }

    [Fact]
    public void ApplyCastingModeModifiers_Quick_AddsExtraFlux()
    {
        // Act
        var result = _sut.ApplyCastingModeModifiers(CastingMode.Quick);

        // Assert
        result.FluxModifier.Should().BePositive();
        result.FluxModifier.Should().Be(5);
    }

    [Fact]
    public void ApplyCastingModeModifiers_Channeled_ReducesFlux()
    {
        // Act
        var result = _sut.ApplyCastingModeModifiers(CastingMode.Channeled);

        // Assert
        result.FluxModifier.Should().BeNegative();
        result.FluxModifier.Should().Be(-5);
    }

    [Fact]
    public void ApplyCastingModeModifiers_Ritual_HasZeroResonanceGain()
    {
        // Act
        var result = _sut.ApplyCastingModeModifiers(CastingMode.Ritual);

        // Assert
        result.ResonanceGain.Should().Be(0);
    }

    #endregion

    #region Overflow & Decay Tests

    [Fact]
    public void TriggerOverflow_AtMax_ReturnsOverflowResult()
    {
        // Arrange
        var character = CreateMystic(resonance: 100);

        // Act
        var result = _sut.TriggerOverflow(character, "Test overflow");

        // Assert
        result.PotencyBonus.Should().Be(1.50m);
        result.DurationTurns.Should().Be(1);
        result.DischargeAmount.Should().Be(50);
        character.ResonanceState!.IsOverflowActive.Should().BeTrue();
        character.ResonanceState.OverflowCount.Should().Be(1);
    }

    [Fact]
    public void TriggerOverflow_BelowMax_ReturnsNone()
    {
        // Arrange
        var character = CreateMystic(resonance: 99);

        // Act
        var result = _sut.TriggerOverflow(character, "Test");

        // Assert
        result.Should().Be(OverflowResult.None);
    }

    [Fact]
    public void TriggerOverflow_ThirdTime_SetsSoulFractureRisk()
    {
        // Arrange
        var character = CreateMystic(resonance: 100);
        character.ResonanceState!.OverflowCount = 2; // Already triggered twice

        // Act
        var result = _sut.TriggerOverflow(character, "Risk test");

        // Assert
        result.SoulFractureRisk.Should().BeTrue();
        result.TotalOverflowCount.Should().Be(3);
    }

    [Fact]
    public void ProcessOverflowDischarge_ReducesResonanceBy50()
    {
        // Arrange
        var character = CreateMystic(resonance: 100);
        character.ResonanceState!.IsOverflowActive = true;

        // Act
        var discharged = _sut.ProcessOverflowDischarge(character, "Discharge");

        // Assert
        discharged.Should().Be(-50); // Negative because it's a reduction
        character.ResonanceState.CurrentValue.Should().Be(50);
        character.ResonanceState.IsOverflowActive.Should().BeFalse();
    }

    [Fact]
    public void ProcessResonanceDecay_ReducesByDecayRate()
    {
        // Arrange
        var character = CreateMystic(resonance: 50);

        // Act
        var decayed = _sut.ProcessResonanceDecay(character, "Rest");

        // Assert
        decayed.Should().Be(10); // Default decay rate
        character.ResonanceState!.CurrentValue.Should().Be(40);
    }

    [Fact]
    public void ProcessResonanceDecay_AtLowResonance_DecaysToZero()
    {
        // Arrange
        var character = CreateMystic(resonance: 5);

        // Act
        var decayed = _sut.ProcessResonanceDecay(character, "Rest");

        // Assert
        decayed.Should().Be(5); // Can only decay what's there
        character.ResonanceState!.CurrentValue.Should().Be(0);
    }

    #endregion

    #region Event Publishing Tests

    [Fact]
    public void ModifyResonance_OnThresholdChange_PublishesEvent()
    {
        // Arrange
        var character = CreateMystic(resonance: 24);

        // Act
        _sut.ModifyResonance(character, 1, "Test");

        // Assert
        _eventBusMock.Verify(
            e => e.Publish(It.Is<ResonanceChangedEvent>(evt =>
                evt.ThresholdChanged == true &&
                evt.OldThreshold == ResonanceThreshold.Dim &&
                evt.NewThreshold == ResonanceThreshold.Steady)),
            Times.Once);
    }

    [Fact]
    public void ModifyResonance_OnLargeChange_PublishesEvent()
    {
        // Arrange
        var character = CreateMystic(resonance: 30);

        // Act
        _sut.ModifyResonance(character, 15, "Power surge"); // >= 15 is significant

        // Assert
        _eventBusMock.Verify(
            e => e.Publish(It.IsAny<ResonanceChangedEvent>()),
            Times.Once);
    }

    [Fact]
    public void ModifyResonance_OnSmallChangeWithinThreshold_DoesNotPublishEvent()
    {
        // Arrange
        var character = CreateMystic(resonance: 30);

        // Act
        _sut.ModifyResonance(character, 5, "Small change"); // < 15, within threshold

        // Assert
        _eventBusMock.Verify(
            e => e.Publish(It.IsAny<ResonanceChangedEvent>()),
            Times.Never);
    }

    [Fact]
    public void TriggerOverflow_PublishesOverflowEvent()
    {
        // Arrange
        var character = CreateMystic(resonance: 100);

        // Act
        _sut.TriggerOverflow(character, "Event test");

        // Assert
        _eventBusMock.Verify(
            e => e.Publish(It.Is<OverflowTriggeredEvent>(evt =>
                evt.CharacterId == character.Id &&
                evt.OverflowCount == 1)),
            Times.Once);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void FullCastingCycle_Quick_AppliesCorrectResonance()
    {
        // Arrange
        var character = CreateMystic(resonance: 20);
        var modeResult = _sut.ApplyCastingModeModifiers(CastingMode.Quick);

        // Act
        var resonanceResult = _sut.ModifyResonance(
            character,
            modeResult.ResonanceGain,
            "Quick cast: Fireball");

        // Assert
        resonanceResult.NewValue.Should().Be(35); // 20 + 15
        resonanceResult.PreviousThreshold.Should().Be(ResonanceThreshold.Dim);
        resonanceResult.NewThreshold.Should().Be(ResonanceThreshold.Steady);
    }

    [Fact]
    public void FullCastingCycle_Channeled_AppliesReducedResonance()
    {
        // Arrange
        var character = CreateMystic(resonance: 40);
        var modeResult = _sut.ApplyCastingModeModifiers(CastingMode.Channeled);

        // Act
        var resonanceResult = _sut.ModifyResonance(
            character,
            modeResult.ResonanceGain,
            "Channeled cast: Heal");

        // Assert
        resonanceResult.NewValue.Should().Be(45); // 40 + 5
        resonanceResult.ThresholdChanged.Should().BeFalse();
    }

    [Fact]
    public void PotencyCalculation_IntegratesWithResonance()
    {
        // Arrange
        var character = CreateMystic(resonance: 60); // Bright threshold
        var baseDamage = 100;

        // Act
        var modifier = _sut.GetPotencyModifier(character);
        var finalDamage = (int)(baseDamage * modifier);

        // Assert
        modifier.Should().Be(1.15m);
        finalDamage.Should().Be(115);
    }

    [Fact]
    public void OverflowCycle_TriggerAndDischarge_CompletesCorrectly()
    {
        // Arrange
        var character = CreateMystic(resonance: 95);

        // Act - Push to overflow
        _sut.ModifyResonance(character, 5, "Final cast");
        var overflowResult = _sut.TriggerOverflow(character, "Trigger");

        // Assert overflow state
        overflowResult.PotencyBonus.Should().Be(1.50m);
        character.ResonanceState!.IsOverflowActive.Should().BeTrue();

        // Act - Discharge
        var discharged = _sut.ProcessOverflowDischarge(character, "Discharge");

        // Assert post-discharge
        character.ResonanceState.CurrentValue.Should().Be(50);
        character.ResonanceState.IsOverflowActive.Should().BeFalse();
    }

    #endregion
}
