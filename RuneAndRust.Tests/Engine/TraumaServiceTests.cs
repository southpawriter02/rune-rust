using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models.Combat;
using RuneAndRust.Engine.Services;
using Xunit;
using CharacterAttribute = RuneAndRust.Core.Enums.Attribute;
using CharacterEntity = RuneAndRust.Core.Entities.Character;

namespace RuneAndRust.Tests.Engine;

/// <summary>
/// Tests for the TraumaService class.
/// Validates psychic stress mechanics including WILL-based resolve checks,
/// stress status tiers, and defense penalty calculations.
/// </summary>
public class TraumaServiceTests
{
    private readonly Mock<IDiceService> _mockDice;
    private readonly Mock<ILogger<TraumaService>> _mockLogger;
    private readonly TraumaService _sut;

    public TraumaServiceTests()
    {
        _mockDice = new Mock<IDiceService>();
        _mockLogger = new Mock<ILogger<TraumaService>>();
        _sut = new TraumaService(_mockDice.Object, _mockLogger.Object);
    }

    #region Helper Methods

    private static Combatant CreateCombatant(int will = 5, int currentStress = 0)
    {
        var character = new CharacterEntity
        {
            Name = "Test Combatant",
            Will = will,
            Might = 5,
            Finesse = 5,
            Sturdiness = 5,
            Wits = 5
        };
        var combatant = Combatant.FromCharacter(character);
        combatant.CurrentStress = currentStress;
        return combatant;
    }

    private static Combatant CreateEnemyCombatant(int will = 3, int currentStress = 0)
    {
        var enemy = new Enemy
        {
            Name = "Test Enemy",
            Attributes = new Dictionary<CharacterAttribute, int>
            {
                { CharacterAttribute.Will, will },
                { CharacterAttribute.Might, 5 },
                { CharacterAttribute.Finesse, 5 },
                { CharacterAttribute.Sturdiness, 5 },
                { CharacterAttribute.Wits, 3 }
            },
            MaxHp = 50,
            CurrentHp = 50
        };
        var combatant = Combatant.FromEnemy(enemy);
        combatant.CurrentStress = currentStress;
        return combatant;
    }

    #endregion

    #region GetStressStatus Tests

    [Theory]
    [InlineData(0, StressStatus.Stable)]
    [InlineData(10, StressStatus.Stable)]
    [InlineData(19, StressStatus.Stable)]
    [InlineData(20, StressStatus.Unsettled)]
    [InlineData(30, StressStatus.Unsettled)]
    [InlineData(39, StressStatus.Unsettled)]
    [InlineData(40, StressStatus.Shaken)]
    [InlineData(50, StressStatus.Shaken)]
    [InlineData(59, StressStatus.Shaken)]
    [InlineData(60, StressStatus.Distressed)]
    [InlineData(70, StressStatus.Distressed)]
    [InlineData(79, StressStatus.Distressed)]
    [InlineData(80, StressStatus.Fractured)]
    [InlineData(90, StressStatus.Fractured)]
    [InlineData(99, StressStatus.Fractured)]
    [InlineData(100, StressStatus.Breaking)]
    public void GetStressStatus_ReturnsCorrectTier(int stressValue, StressStatus expectedStatus)
    {
        // Act
        var status = _sut.GetStressStatus(stressValue);

        // Assert
        status.Should().Be(expectedStatus);
    }

    #endregion

    #region GetDefensePenalty Tests

    [Theory]
    [InlineData(0, 0)]
    [InlineData(10, 0)]
    [InlineData(19, 0)]
    [InlineData(20, 1)]
    [InlineData(39, 1)]
    [InlineData(40, 2)]
    [InlineData(59, 2)]
    [InlineData(60, 3)]
    [InlineData(79, 3)]
    [InlineData(80, 4)]
    [InlineData(99, 4)]
    [InlineData(100, 5)]
    [InlineData(120, 5)]  // Should cap at 5
    public void GetDefensePenalty_ReturnsCorrectValue(int stressValue, int expectedPenalty)
    {
        // Act
        var penalty = _sut.GetDefensePenalty(stressValue);

        // Assert
        penalty.Should().Be(expectedPenalty);
    }

    [Fact]
    public void GetDefensePenalty_CapsAtFive()
    {
        // Arrange - stress way over 100
        var stressValue = 200;

        // Act
        var penalty = _sut.GetDefensePenalty(stressValue);

        // Assert
        penalty.Should().Be(5);
    }

    #endregion

    #region InflictStress Tests

    [Fact]
    public void InflictStress_ReducesByWillSuccesses()
    {
        // Arrange
        var combatant = CreateCombatant(will: 5, currentStress: 0);

        // Mock: Roll 3 successes on WILL check (mitigates 3 stress)
        _mockDice.Setup(d => d.Roll(5, It.IsAny<string>()))
            .Returns(new DiceResult(3, 0, new[] { 8, 9, 10, 4, 2 }));

        // Act - inflict 10 stress
        var result = _sut.InflictStress(combatant, 10, "Test Source");

        // Assert
        result.RawStress.Should().Be(10);
        result.MitigatedAmount.Should().Be(3);
        result.NetStressApplied.Should().Be(7); // 10 - 3 = 7
        result.CurrentTotal.Should().Be(7);
        result.ResolveSuccesses.Should().Be(3);
        combatant.CurrentStress.Should().Be(7);
    }

    [Fact]
    public void InflictStress_ZeroNetOnHighResistance()
    {
        // Arrange
        var combatant = CreateCombatant(will: 8, currentStress: 0);

        // Mock: Roll 5 successes (more than stress amount)
        _mockDice.Setup(d => d.Roll(8, It.IsAny<string>()))
            .Returns(new DiceResult(5, 0, new[] { 8, 9, 10, 8, 9, 4, 3, 2 }));

        // Act - inflict only 3 stress
        var result = _sut.InflictStress(combatant, 3, "Weak Source");

        // Assert
        result.RawStress.Should().Be(3);
        result.MitigatedAmount.Should().Be(5);
        result.NetStressApplied.Should().Be(0); // Cannot go negative
        result.CurrentTotal.Should().Be(0);
        combatant.CurrentStress.Should().Be(0);
    }

    [Fact]
    public void InflictStress_ClampsAt100AndTriggersBreakingPoint()
    {
        // Arrange - combatant already at 90 stress
        var combatant = CreateCombatant(will: 1, currentStress: 90);

        // Mock: Roll 0 successes (no mitigation) - this causes Trauma outcome (stress reset to 50)
        _mockDice.Setup(d => d.Roll(1, It.IsAny<string>()))
            .Returns(new DiceResult(0, 0, new[] { 3 }));

        // Act - inflict 50 stress (would go to 140)
        var result = _sut.InflictStress(combatant, 50, "Massive Trauma");

        // Assert - Breaking Point triggered, Trauma outcome (0 successes, no botches) resets to 50
        // (v0.3.0c: HandleBreakingPoint now resolves the breaking point)
        result.IsBreakingPoint.Should().BeTrue();
        combatant.CurrentStress.Should().Be(50); // Trauma outcome resets to 50
    }

    [Fact]
    public void InflictStress_TriggersBreakingPointAt100()
    {
        // Arrange - combatant at 85 stress
        var combatant = CreateCombatant(will: 1, currentStress: 85);

        // Mock: Roll 0 successes - causes Trauma outcome (stress reset to 50)
        _mockDice.Setup(d => d.Roll(1, It.IsAny<string>()))
            .Returns(new DiceResult(0, 0, new[] { 4 }));

        // Act - inflict 20 stress (pushes to 100+)
        var result = _sut.InflictStress(combatant, 20, "Final Straw");

        // Assert - Breaking Point triggered and resolved (v0.3.0c)
        result.IsBreakingPoint.Should().BeTrue();
        // Trauma outcome (0 successes, no botches) resets stress to 50
        combatant.CurrentStress.Should().Be(50);
    }

    [Fact]
    public void InflictStress_NoBreakingPointIfAlreadyAt100()
    {
        // Arrange - combatant already at 100 stress
        var combatant = CreateCombatant(will: 1, currentStress: 100);

        // Mock: Roll 0 successes
        _mockDice.Setup(d => d.Roll(1, It.IsAny<string>()))
            .Returns(new DiceResult(0, 0, new[] { 3 }));

        // Act - inflict more stress
        var result = _sut.InflictStress(combatant, 10, "Additional Stress");

        // Assert - not a new breaking point
        result.IsBreakingPoint.Should().BeFalse();
        result.CurrentTotal.Should().Be(100);
    }

    [Fact]
    public void InflictStress_TracksStatusTransition()
    {
        // Arrange - combatant at 15 stress (Stable)
        var combatant = CreateCombatant(will: 1, currentStress: 15);

        // Mock: Roll 0 successes
        _mockDice.Setup(d => d.Roll(1, It.IsAny<string>()))
            .Returns(new DiceResult(0, 0, new[] { 3 }));

        // Act - inflict 10 stress (pushes to 25 = Unsettled)
        var result = _sut.InflictStress(combatant, 10, "Creeping Dread");

        // Assert
        result.PreviousStatus.Should().Be(StressStatus.Stable);
        result.NewStatus.Should().Be(StressStatus.Unsettled);
    }

    [Fact]
    public void InflictStress_StoresSourceInResult()
    {
        // Arrange
        var combatant = CreateCombatant(will: 3, currentStress: 0);
        _mockDice.Setup(d => d.Roll(3, It.IsAny<string>()))
            .Returns(new DiceResult(0, 0, new[] { 1, 2, 3 }));

        // Act
        var result = _sut.InflictStress(combatant, 5, "Witnessing Corruption");

        // Assert
        result.Source.Should().Be("Witnessing Corruption");
    }

    #endregion

    #region RecoverStress Tests

    [Fact]
    public void RecoverStress_ReducesStress()
    {
        // Arrange - combatant at 50 stress
        var combatant = CreateCombatant(will: 5, currentStress: 50);

        // Act - recover 20 stress
        var result = _sut.RecoverStress(combatant, 20, "Rest");

        // Assert
        result.CurrentTotal.Should().Be(30);
        combatant.CurrentStress.Should().Be(30);
    }

    [Fact]
    public void RecoverStress_ClampsAtZero()
    {
        // Arrange - combatant at 10 stress
        var combatant = CreateCombatant(will: 5, currentStress: 10);

        // Act - recover 50 stress (more than current)
        var result = _sut.RecoverStress(combatant, 50, "Full Rest");

        // Assert
        result.CurrentTotal.Should().Be(0);
        result.NetStressApplied.Should().Be(-10); // Only recovered 10 (actual amount)
        combatant.CurrentStress.Should().Be(0);
    }

    [Fact]
    public void RecoverStress_TracksStatusTransition()
    {
        // Arrange - combatant at 45 stress (Shaken)
        var combatant = CreateCombatant(will: 5, currentStress: 45);

        // Act - recover 10 stress (drops to 35 = Unsettled)
        var result = _sut.RecoverStress(combatant, 10, "Calm Moment");

        // Assert
        result.PreviousStatus.Should().Be(StressStatus.Shaken);
        result.NewStatus.Should().Be(StressStatus.Unsettled);
    }

    [Fact]
    public void RecoverStress_NeverTriggersBreakingPoint()
    {
        // Arrange - combatant at 100 stress
        var combatant = CreateCombatant(will: 5, currentStress: 100);

        // Act
        var result = _sut.RecoverStress(combatant, 5, "Minor Relief");

        // Assert
        result.IsBreakingPoint.Should().BeFalse();
    }

    [Fact]
    public void RecoverStress_ReturnsNegativeRawStress()
    {
        // Arrange
        var combatant = CreateCombatant(will: 5, currentStress: 50);

        // Act
        var result = _sut.RecoverStress(combatant, 15, "Meditation");

        // Assert
        result.RawStress.Should().Be(-15);
        result.ResolveSuccesses.Should().Be(0); // No resolve roll for recovery
        result.MitigatedAmount.Should().Be(0);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void InflictStress_UsesCorrectWillAttribute()
    {
        // Arrange - combatant with WILL 7
        var combatant = CreateCombatant(will: 7, currentStress: 0);

        _mockDice.Setup(d => d.Roll(7, It.IsAny<string>()))
            .Returns(new DiceResult(4, 0, new[] { 8, 9, 10, 8, 3, 2, 1 }));

        // Act
        _sut.InflictStress(combatant, 10, "Test");

        // Assert - verify WILL 7 was used for the roll
        _mockDice.Verify(d => d.Roll(7, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void InflictStress_WorksWithEnemyCombatant()
    {
        // Arrange - enemy with WILL 3
        var enemy = CreateEnemyCombatant(will: 3, currentStress: 0);

        _mockDice.Setup(d => d.Roll(3, It.IsAny<string>()))
            .Returns(new DiceResult(1, 0, new[] { 8, 4, 2 }));

        // Act - inflict 15 stress
        var result = _sut.InflictStress(enemy, 15, "Player Intimidation");

        // Assert
        result.NetStressApplied.Should().Be(14); // 15 - 1 mitigation
        enemy.CurrentStress.Should().Be(14);
    }

    #endregion

    #region Corruption Tests (v0.3.0b)

    [Fact]
    public void AddCorruption_AccumulatesDirectly_NoMitigation()
    {
        // Arrange
        var character = CreateCharacter();
        character.Corruption = 0;

        // Act - add 25 corruption
        var result = _sut.AddCorruption(character, 25, "Blight Exposure");

        // Assert - NO mitigation from WILL
        result.RawCorruption.Should().Be(25);
        result.NetCorruptionApplied.Should().Be(25);
        result.CurrentTotal.Should().Be(25);
        character.Corruption.Should().Be(25);

        // Verify NO dice roll occurred
        _mockDice.Verify(d => d.Roll(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void AddCorruption_ClampsAt100()
    {
        // Arrange
        var character = CreateCharacter();
        character.Corruption = 90;

        // Act - add 50 corruption (would exceed 100)
        var result = _sut.AddCorruption(character, 50, "Massive Corruption");

        // Assert
        result.CurrentTotal.Should().Be(100);
        result.NetCorruptionApplied.Should().Be(10); // Only 10 could be applied
        character.Corruption.Should().Be(100);
    }

    [Fact]
    public void AddCorruption_TriggersTerminalAt100()
    {
        // Arrange
        var character = CreateCharacter();
        character.Corruption = 85;

        // Act - push to 100+
        var result = _sut.AddCorruption(character, 20, "Fatal Exposure");

        // Assert
        result.IsTerminal.Should().BeTrue();
        result.CurrentTotal.Should().Be(100);
        result.NewTier.Should().Be(CorruptionTier.Terminal);
    }

    [Fact]
    public void AddCorruption_NoTerminalIfAlreadyAt100()
    {
        // Arrange
        var character = CreateCharacter();
        character.Corruption = 100;

        // Act - already at 100
        var result = _sut.AddCorruption(character, 10, "Additional Corruption");

        // Assert - not a new terminal event
        result.IsTerminal.Should().BeFalse();
        result.NetCorruptionApplied.Should().Be(0);
    }

    [Fact]
    public void AddCorruption_TracksTierTransition()
    {
        // Arrange - at 35 (Tainted)
        var character = CreateCharacter();
        character.Corruption = 35;

        // Act - add 10 (pushes to 45 = Corrupted)
        var result = _sut.AddCorruption(character, 10, "Artifact Contact");

        // Assert
        result.TierChanged.Should().BeTrue();
        result.PreviousTier.Should().Be(CorruptionTier.Tainted);
        result.NewTier.Should().Be(CorruptionTier.Corrupted);
    }

    [Fact]
    public void AddCorruption_IgnoresNonPositiveAmounts()
    {
        // Arrange
        var character = CreateCharacter();
        character.Corruption = 25;

        // Act
        var result = _sut.AddCorruption(character, 0, "Zero Source");

        // Assert
        result.NetCorruptionApplied.Should().Be(0);
        result.TierChanged.Should().BeFalse();
        character.Corruption.Should().Be(25);
    }

    [Fact]
    public void AddCorruption_Combatant_UpdatesCharacterSource()
    {
        // Arrange
        var combatant = CreateCombatant(will: 5, currentStress: 0);
        combatant.CurrentCorruption = 10;
        combatant.CharacterSource!.Corruption = 10;

        // Act
        var result = _sut.AddCorruption(combatant, 15, "Combat Corruption");

        // Assert - both updated
        result.CurrentTotal.Should().Be(25);
        combatant.CurrentCorruption.Should().Be(25);
        combatant.CharacterSource.Corruption.Should().Be(25);
    }

    [Fact]
    public void AddCorruption_NonPlayerCombatant_OnlyUpdatesLocal()
    {
        // Arrange
        var enemy = CreateEnemyCombatant(will: 3, currentStress: 0);
        enemy.CurrentCorruption = 0;

        // Act
        var result = _sut.AddCorruption(enemy, 20, "Blight Splash");

        // Assert - only combatant updated, no character source
        result.CurrentTotal.Should().Be(20);
        enemy.CurrentCorruption.Should().Be(20);
        enemy.CharacterSource.Should().BeNull();
    }

    [Fact]
    public void PurgeCorruption_ReducesCorruption()
    {
        // Arrange
        var character = CreateCharacter();
        character.Corruption = 50;

        // Act
        var result = _sut.PurgeCorruption(character, 20, "Ritual Cleansing");

        // Assert
        result.CurrentTotal.Should().Be(30);
        result.NetCorruptionApplied.Should().Be(-20);
        character.Corruption.Should().Be(30);
    }

    [Fact]
    public void PurgeCorruption_ClampsAtZero()
    {
        // Arrange
        var character = CreateCharacter();
        character.Corruption = 15;

        // Act - try to purge more than available
        var result = _sut.PurgeCorruption(character, 50, "Full Cleanse");

        // Assert
        result.CurrentTotal.Should().Be(0);
        result.NetCorruptionApplied.Should().Be(-15); // Only 15 purged
        character.Corruption.Should().Be(0);
    }

    [Fact]
    public void PurgeCorruption_TracksTierTransition()
    {
        // Arrange - at 45 (Corrupted)
        var character = CreateCharacter();
        character.Corruption = 45;

        // Act - purge to 35 (Tainted)
        var result = _sut.PurgeCorruption(character, 10, "Minor Cleansing");

        // Assert
        result.TierChanged.Should().BeTrue();
        result.PreviousTier.Should().Be(CorruptionTier.Corrupted);
        result.NewTier.Should().Be(CorruptionTier.Tainted);
    }

    [Fact]
    public void PurgeCorruption_NeverTriggersTerminal()
    {
        // Arrange
        var character = CreateCharacter();
        character.Corruption = 100;

        // Act
        var result = _sut.PurgeCorruption(character, 5, "Small Relief");

        // Assert
        result.IsTerminal.Should().BeFalse();
        result.CurrentTotal.Should().Be(95);
    }

    [Fact]
    public void GetCorruptionState_ReturnsCorrectState()
    {
        // Act & Assert
        _sut.GetCorruptionState(0).Tier.Should().Be(CorruptionTier.Pristine);
        _sut.GetCorruptionState(25).Tier.Should().Be(CorruptionTier.Tainted);
        _sut.GetCorruptionState(50).Tier.Should().Be(CorruptionTier.Corrupted);
        _sut.GetCorruptionState(70).Tier.Should().Be(CorruptionTier.Blighted);
        _sut.GetCorruptionState(90).Tier.Should().Be(CorruptionTier.Fractured);
        _sut.GetCorruptionState(100).Tier.Should().Be(CorruptionTier.Terminal);
    }

    #endregion

    #region Helper Methods for Corruption Tests

    private static CharacterEntity CreateCharacter()
    {
        return new CharacterEntity
        {
            Name = "Test Character",
            Will = 5,
            Might = 5,
            Finesse = 5,
            Sturdiness = 5,
            Wits = 5,
            Corruption = 0
        };
    }

    #endregion

    #region Breaking Point Tests (v0.3.0c)

    [Fact]
    public void ResolveBreakingPoint_Stabilized_WhenThreeOrMoreSuccesses()
    {
        // Arrange
        var character = CreateCharacter();
        character.PsychicStress = 100;

        _mockDice.Setup(d => d.Roll(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(new DiceResult(Successes: 3, Botches: 0, Rolls: [8, 9, 10, 2, 3]));

        // Act
        var result = _sut.ResolveBreakingPoint(character, "Test Source");

        // Assert
        result.Outcome.Should().Be(BreakingPointOutcome.Stabilized);
        result.NewStressLevel.Should().Be(75);
        result.AcquiredTrauma.Should().BeNull();
        result.WasDisoriented.Should().BeTrue();
        result.WasStunned.Should().BeFalse();
        result.ResolveSuccesses.Should().Be(3);
    }

    [Fact]
    public void ResolveBreakingPoint_Trauma_WhenFewerThanThreeSuccessesNoBotches()
    {
        // Arrange
        var character = CreateCharacter();
        character.PsychicStress = 100;

        _mockDice.Setup(d => d.Roll(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(new DiceResult(Successes: 2, Botches: 0, Rolls: [8, 9, 4, 5, 6]));

        // Act
        var result = _sut.ResolveBreakingPoint(character, "Test Source");

        // Assert
        result.Outcome.Should().Be(BreakingPointOutcome.Trauma);
        result.NewStressLevel.Should().Be(50);
        result.AcquiredTrauma.Should().NotBeNull();
        result.WasDisoriented.Should().BeFalse();
        result.WasStunned.Should().BeFalse();
        character.ActiveTraumas.Should().HaveCount(1);
    }

    [Fact]
    public void ResolveBreakingPoint_Catastrophe_WhenZeroSuccessesAndBotches()
    {
        // Arrange
        var character = CreateCharacter();
        character.PsychicStress = 100;

        _mockDice.Setup(d => d.Roll(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(new DiceResult(Successes: 0, Botches: 2, Rolls: [1, 1, 4, 5, 6]));

        // Act
        var result = _sut.ResolveBreakingPoint(character, "Test Source");

        // Assert
        result.Outcome.Should().Be(BreakingPointOutcome.Catastrophe);
        result.NewStressLevel.Should().Be(50);
        result.AcquiredTrauma.Should().NotBeNull();
        result.WasDisoriented.Should().BeFalse();
        result.WasStunned.Should().BeTrue();
        character.ActiveTraumas.Should().HaveCount(1);
    }

    [Fact]
    public void ResolveBreakingPoint_Trauma_NotCatastrophe_WhenZeroSuccessesButNoBotches()
    {
        // Arrange - edge case: zero successes but no botches is Trauma, not Catastrophe
        var character = CreateCharacter();
        character.PsychicStress = 100;

        _mockDice.Setup(d => d.Roll(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(new DiceResult(Successes: 0, Botches: 0, Rolls: [4, 5, 6, 7, 3]));

        // Act
        var result = _sut.ResolveBreakingPoint(character, "Test Source");

        // Assert
        result.Outcome.Should().Be(BreakingPointOutcome.Trauma);
        result.WasStunned.Should().BeFalse();
    }

    [Fact]
    public void ResolveBreakingPoint_AddsTraumaToCharacterActiveTraumas()
    {
        // Arrange
        var character = CreateCharacter();
        character.PsychicStress = 100;
        character.ActiveTraumas.Should().BeEmpty();

        _mockDice.Setup(d => d.Roll(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(new DiceResult(Successes: 1, Botches: 0, Rolls: [8, 4, 5, 6, 7]));

        // Act
        var result = _sut.ResolveBreakingPoint(character, "Battle Stress");

        // Assert
        character.ActiveTraumas.Should().HaveCount(1);
        var trauma = character.ActiveTraumas[0];
        trauma.Source.Should().Be("Battle Stress");
        trauma.IsActive.Should().BeTrue();
        trauma.DefinitionId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ApplyTraumaPenalties_AppliesAttributePenalties()
    {
        // Arrange
        var character = CreateCharacter();
        character.Finesse = 5;

        // Get "The Shakes" trauma which has -1 Finesse penalty
        var shakesDef = TraumaRegistry.GetById("TRM_SHAKES");
        shakesDef.Should().NotBeNull();

        var trauma = shakesDef!.CreateInstance("Test");

        // Act
        _sut.ApplyTraumaPenalties(character, trauma);

        // Assert
        character.Finesse.Should().Be(4); // 5 - 1 = 4
    }

    [Fact]
    public void ApplyTraumaPenalties_ClampsAttributeToMinimumOne()
    {
        // Arrange
        var character = CreateCharacter();
        character.Finesse = 1; // Already at minimum

        var shakesDef = TraumaRegistry.GetById("TRM_SHAKES");
        var trauma = shakesDef!.CreateInstance("Test");

        // Act
        _sut.ApplyTraumaPenalties(character, trauma);

        // Assert
        character.Finesse.Should().Be(1); // Should not go below 1
    }

    [Fact]
    public void GetTraumaAttributePenalty_ReturnsTotalPenaltiesForAttribute()
    {
        // Arrange
        var character = CreateCharacter();

        // Add two traumas with Finesse penalties
        var shakesDef = TraumaRegistry.GetById("TRM_SHAKES")!;
        var trauma1 = shakesDef.CreateInstance("Source1");
        character.ActiveTraumas.Add(trauma1);

        // Act
        var penalty = _sut.GetTraumaAttributePenalty(character, CharacterAttribute.Finesse);

        // Assert
        penalty.Should().Be(-1); // The Shakes has -1 Finesse
    }

    [Fact]
    public void GetTraumaAttributePenalty_IgnoresInactiveTraumas()
    {
        // Arrange
        var character = CreateCharacter();

        var shakesDef = TraumaRegistry.GetById("TRM_SHAKES")!;
        var trauma = shakesDef.CreateInstance("Source");
        trauma.IsActive = false; // Inactive trauma
        character.ActiveTraumas.Add(trauma);

        // Act
        var penalty = _sut.GetTraumaAttributePenalty(character, CharacterAttribute.Finesse);

        // Assert
        penalty.Should().Be(0); // Inactive trauma should not contribute
    }

    [Fact]
    public void HandleBreakingPoint_ResolvesBreakingPointForPlayerCombatant()
    {
        // Arrange
        var character = CreateCharacter();
        character.PsychicStress = 100;
        var combatant = Combatant.FromCharacter(character);
        combatant.CurrentStress = 100;

        _mockDice.Setup(d => d.Roll(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(new DiceResult(Successes: 3, Botches: 0, Rolls: [8, 9, 10, 4, 5]));

        // Act
        _sut.HandleBreakingPoint(combatant);

        // Assert
        combatant.CurrentStress.Should().Be(75); // Stabilized resets to 75
        character.PsychicStress.Should().Be(75);
    }

    [Fact]
    public void HandleBreakingPoint_ResetsStressTo75ForNonPlayerCombatant()
    {
        // Arrange
        var combatant = CreateEnemyCombatant();
        combatant.CurrentStress = 100;

        // Act
        _sut.HandleBreakingPoint(combatant);

        // Assert
        combatant.CurrentStress.Should().Be(75);
    }

    #endregion

    #region Trauma Registry Tests (v0.3.0c)

    [Fact]
    public void TraumaRegistry_HasAllStarterTraumas()
    {
        // Assert
        TraumaRegistry.All.Should().HaveCountGreaterThanOrEqualTo(10);
    }

    [Fact]
    public void TraumaRegistry_GetById_ReturnsCorrectTrauma()
    {
        // Act
        var trauma = TraumaRegistry.GetById("TRM_NYCTO");

        // Assert
        trauma.Should().NotBeNull();
        trauma!.Name.Should().Be("Nyctophobia");
        trauma.Type.Should().Be(TraumaType.Phobia);
    }

    [Fact]
    public void TraumaRegistry_GetById_ReturnsNullForInvalidId()
    {
        // Act
        var trauma = TraumaRegistry.GetById("INVALID_ID");

        // Assert
        trauma.Should().BeNull();
    }

    [Fact]
    public void TraumaRegistry_GetByType_ReturnsMatchingTraumas()
    {
        // Act
        var phobias = TraumaRegistry.GetByType(TraumaType.Phobia).ToList();

        // Assert
        phobias.Should().HaveCountGreaterThanOrEqualTo(3);
        phobias.Should().AllSatisfy(t => t.Type.Should().Be(TraumaType.Phobia));
    }

    [Fact]
    public void TraumaRegistry_GetRandom_ReturnsValidTrauma()
    {
        // Act
        var trauma = TraumaRegistry.GetRandom();

        // Assert
        trauma.Should().NotBeNull();
        trauma.DefinitionId.Should().NotBeNullOrEmpty();
        TraumaRegistry.All.Should().Contain(trauma);
    }

    [Fact]
    public void TraumaDefinition_CreateInstance_CreatesValidTrauma()
    {
        // Arrange
        var definition = TraumaRegistry.GetById("TRM_SHAKES")!;

        // Act
        var trauma = definition.CreateInstance("Test Battle");

        // Assert
        trauma.DefinitionId.Should().Be("TRM_SHAKES");
        trauma.Name.Should().Be("The Shakes");
        trauma.Source.Should().Be("Test Battle");
        trauma.IsActive.Should().BeTrue();
        trauma.Id.Should().NotBeEmpty();
    }

    #endregion
}
