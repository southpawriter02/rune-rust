namespace RuneAndRust.Domain.UnitTests.Entities;

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Unit tests for <see cref="TraumaEconomyState"/> entity.
/// Verifies factory methods, computed properties, warning system,
/// and cross-system calculations.
/// </summary>
[TestFixture]
public class TraumaEconomyStateTests
{
    // -------------------------------------------------------------------------
    // Factory Method — Create
    // -------------------------------------------------------------------------

    [Test]
    public void Create_WithValidCharacterId_ReturnsPopulatedState()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var stressState = StressState.Create(45);
        var corruptionState = CorruptionState.Create(30);
        var cpsState = CpsState.Create(45);

        // Act
        var state = TraumaEconomyState.Create(
            characterId,
            stressState,
            corruptionState,
            cpsState,
            traumas: null);

        // Assert
        state.CharacterId.Should().Be(characterId);
        state.StressState.CurrentStress.Should().Be(45);
        state.CorruptionState.CurrentCorruption.Should().Be(30);
        state.CpsState.Stage.Should().Be(CpsStage.GlimmerMadness);
        state.Traumas.Should().BeEmpty();
    }

    [Test]
    public void Create_WithEmptyCharacterId_ThrowsArgumentException()
    {
        // Arrange
        var stressState = StressState.Create(0);
        var corruptionState = CorruptionState.Create(0);
        var cpsState = CpsState.Create(0);

        // Act
        var act = () => TraumaEconomyState.Create(
            Guid.Empty,
            stressState,
            corruptionState,
            cpsState,
            traumas: null);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("characterId");
    }

    [Test]
    public void Create_WithTraumas_IncludesTraumasInState()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var trauma = CharacterTrauma.Create(
            characterId,
            "survivors-guilt",
            "AllyDeath",
            DateTime.UtcNow);
        var traumas = new List<CharacterTrauma> { trauma }.AsReadOnly();

        // Act
        var state = TraumaEconomyState.Create(
            characterId,
            StressState.Calm,
            CorruptionState.Uncorrupted,
            CpsState.Create(0),
            traumas);

        // Assert
        state.Traumas.Should().HaveCount(1);
        state.TraumaCount.Should().Be(1);
        state.ActiveTraumaIds.Should().Contain("survivors-guilt");
    }

    // -------------------------------------------------------------------------
    // Computed Properties — Effective Stats
    // -------------------------------------------------------------------------

    [Test]
    [TestCase(CorruptionStage.Uncorrupted, 100)] // 0% penalty
    [TestCase(CorruptionStage.Tainted, 95)]      // 5% penalty
    [TestCase(CorruptionStage.Infected, 90)]     // 10% penalty
    [TestCase(CorruptionStage.Blighted, 85)]     // 15% penalty
    [TestCase(CorruptionStage.Corrupted, 80)]    // 20% penalty
    [TestCase(CorruptionStage.Consumed, 75)]     // 25% penalty
    public void EffectiveMaxHp_AppliesCorruptionStagePenalty(
        CorruptionStage stage,
        int expectedHp)
    {
        // Arrange
        var corruption = stage switch
        {
            CorruptionStage.Uncorrupted => 0,
            CorruptionStage.Tainted => 20,
            CorruptionStage.Infected => 40,
            CorruptionStage.Blighted => 60,
            CorruptionStage.Corrupted => 80,
            CorruptionStage.Consumed => 100,
            _ => 0
        };

        var state = TraumaEconomyState.Create(
            Guid.NewGuid(),
            StressState.Calm,
            CorruptionState.Create(corruption),
            CpsState.Create(0),
            traumas: null);

        // Assert
        state.EffectiveMaxHp.Should().Be(expectedHp);
    }

    [Test]
    public void EffectiveResolve_ClampedToMinimumOne()
    {
        // Arrange - Consumed stage means -5 Resolve, but from base 10 still > 1
        var state = TraumaEconomyState.Create(
            Guid.NewGuid(),
            StressState.Calm,
            CorruptionState.Create(100), // Consumed stage
            CpsState.Create(0),
            traumas: null);

        // Assert
        state.EffectiveResolve.Should().BeGreaterThanOrEqualTo(1);
    }

    // -------------------------------------------------------------------------
    // Computed Properties — Penalties
    // -------------------------------------------------------------------------

    [Test]
    [TestCase(0, 0)]   // Calm = 0 defense penalty
    [TestCase(20, 1)]  // Uneasy = -1
    [TestCase(40, 2)]  // Anxious = -2
    [TestCase(60, 3)]  // Panicked = -3
    [TestCase(80, 4)]  // Breaking = -4
    [TestCase(100, 5)] // Trauma = -5
    public void TotalDefensePenalty_MatchesStressThreshold(
        int stress,
        int expectedPenalty)
    {
        // Arrange
        var state = TraumaEconomyState.Create(
            Guid.NewGuid(),
            StressState.Create(stress),
            CorruptionState.Uncorrupted,
            CpsState.Create(stress),
            traumas: null);

        // Assert
        state.TotalDefensePenalty.Should().Be(expectedPenalty);
    }

    [Test]
    public void TotalSkillPenalty_CombinesStressAndCps()
    {
        // Arrange - Stress at Breaking (HasSkillDisadvantage=true) + CPS at RuinMadness
        var state = TraumaEconomyState.Create(
            Guid.NewGuid(),
            StressState.Create(85), // Breaking threshold -> HasSkillDisadvantage = true
            CorruptionState.Uncorrupted,
            CpsState.Create(65),    // RuinMadness -> +2 CPS disadvantage
            traumas: null);

        // Assert - 1 (stress) + 2 (CPS RuinMadness) = 3
        state.TotalSkillPenalty.Should().Be(3);
    }

    // -------------------------------------------------------------------------
    // Computed Properties — Critical/Terminal States
    // -------------------------------------------------------------------------

    [Test]
    [TestCase(79, false)]
    [TestCase(80, true)]
    [TestCase(100, true)]
    public void IsCriticalState_WhenStressAtOrAbove80_ReturnsTrue(
        int stress,
        bool expectedCritical)
    {
        // Arrange
        var state = TraumaEconomyState.Create(
            Guid.NewGuid(),
            StressState.Create(stress),
            CorruptionState.Uncorrupted,
            CpsState.Create(stress),
            traumas: null);

        // Assert
        state.IsCriticalState.Should().Be(expectedCritical);
    }

    [Test]
    public void IsCriticalState_WhenCorruptionAtOrAbove80_ReturnsTrue()
    {
        // Arrange
        var state = TraumaEconomyState.Create(
            Guid.NewGuid(),
            StressState.Calm,
            CorruptionState.Create(80), // Corruption critical
            CpsState.Create(0),
            traumas: null);

        // Assert
        state.IsCriticalState.Should().BeTrue();
    }

    [Test]
    [TestCase(99, false)]
    [TestCase(100, true)]
    public void IsTerminalState_WhenStressAt100_ReturnsTrue(
        int stress,
        bool expectedTerminal)
    {
        // Arrange
        var state = TraumaEconomyState.Create(
            Guid.NewGuid(),
            StressState.Create(stress),
            CorruptionState.Uncorrupted,
            CpsState.Create(stress),
            traumas: null);

        // Assert
        state.IsTerminalState.Should().Be(expectedTerminal);
    }

    [Test]
    public void HasMultipleCriticalSystems_WhenBothStressAndCorruptionCritical_ReturnsTrue()
    {
        // Arrange
        var state = TraumaEconomyState.Create(
            Guid.NewGuid(),
            StressState.Create(85),     // Critical
            CorruptionState.Create(90), // Critical
            CpsState.Create(85),
            traumas: null);

        // Assert
        state.HasMultipleCriticalSystems.Should().BeTrue();
    }

    // -------------------------------------------------------------------------
    // Warning System
    // -------------------------------------------------------------------------

    [Test]
    public void GetWarningLevel_WithLowValues_ReturnsNone()
    {
        // Arrange
        var state = TraumaEconomyState.Create(
            Guid.NewGuid(),
            StressState.Create(30),
            CorruptionState.Create(20),
            CpsState.Create(30),
            traumas: null);

        // Assert
        state.GetWarningLevel().Should().Be(WarningLevel.None);
    }

    [Test]
    public void GetWarningLevel_WithStressAt70_ReturnsWarning()
    {
        // Arrange - Use lower CPS to avoid it pushing to Critical
        var state = TraumaEconomyState.Create(
            Guid.NewGuid(),
            StressState.Create(75),
            CorruptionState.Uncorrupted,
            CpsState.Create(10),    // Low CPS, no panic trigger
            traumas: null);

        // Assert
        state.GetWarningLevel().Should().Be(WarningLevel.Warning);
    }

    [Test]
    public void GetWarningLevel_WithStressAt80_ReturnsCritical()
    {
        // Arrange
        var state = TraumaEconomyState.Create(
            Guid.NewGuid(),
            StressState.Create(85),
            CorruptionState.Uncorrupted,
            CpsState.Create(85),
            traumas: null);

        // Assert
        state.GetWarningLevel().Should().Be(WarningLevel.Critical);
    }

    [Test]
    public void GetWarningLevel_WithStressAt100_ReturnsTerminal()
    {
        // Arrange
        var state = TraumaEconomyState.Create(
            Guid.NewGuid(),
            StressState.Create(100),
            CorruptionState.Uncorrupted,
            CpsState.Create(100),
            traumas: null);

        // Assert
        state.GetWarningLevel().Should().Be(WarningLevel.Terminal);
    }

    [Test]
    public void ActiveWarnings_WithHighStress_ContainsWarningMessage()
    {
        // Arrange
        var state = TraumaEconomyState.Create(
            Guid.NewGuid(),
            StressState.Create(85),
            CorruptionState.Uncorrupted,
            CpsState.Create(85),
            traumas: null);

        // Assert
        state.ActiveWarnings.Should().Contain(w => w.Contains("sanity"));
    }

    [Test]
    public void ActiveWarnings_WithTraumas_ContainsTraumaMessage()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var trauma = CharacterTrauma.Create(
            characterId,
            "survivors-guilt",
            "AllyDeath",
            DateTime.UtcNow);
        var traumas = new List<CharacterTrauma> { trauma }.AsReadOnly();

        var state = TraumaEconomyState.Create(
            characterId,
            StressState.Calm,
            CorruptionState.Uncorrupted,
            CpsState.Create(0),
            traumas);

        // Assert
        state.ActiveWarnings.Should().Contain(w => w.Contains("trauma"));
    }

    // -------------------------------------------------------------------------
    // ToString
    // -------------------------------------------------------------------------

    [Test]
    public void ToString_FormatsCorrectly()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var state = TraumaEconomyState.Create(
            characterId,
            StressState.Create(45),
            CorruptionState.Create(30),
            CpsState.Create(45),
            traumas: null);

        // Act
        var result = state.ToString();

        // Assert
        result.Should().Contain("Stress=45");
        result.Should().Contain("Corruption=30");
        result.Should().Contain("Traumas=0");
    }
}
