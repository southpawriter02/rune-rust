using FluentAssertions;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Enums;

/// <summary>
/// Tests for CombatState enum values.
/// </summary>
[TestFixture]
public class CombatStateTests
{
    [Test]
    public void CombatState_ShouldHaveNotStartedAsDefault()
    {
        // Arrange & Act
        var defaultState = default(CombatState);

        // Assert
        defaultState.Should().Be(CombatState.NotStarted);
    }

    [Test]
    public void CombatState_ShouldHaveAllExpectedValues()
    {
        // Arrange & Act
        var values = Enum.GetValues<CombatState>();

        // Assert
        values.Should().HaveCount(5);
        values.Should().Contain(CombatState.NotStarted);
        values.Should().Contain(CombatState.Active);
        values.Should().Contain(CombatState.Victory);
        values.Should().Contain(CombatState.PlayerDefeated);
        values.Should().Contain(CombatState.Fled);
    }

    [Test]
    [TestCase(CombatState.Victory)]
    [TestCase(CombatState.PlayerDefeated)]
    [TestCase(CombatState.Fled)]
    public void CombatState_TerminalStates_ShouldBeDistinguishable(CombatState terminalState)
    {
        // Assert - terminal states are distinguishable from active/not started
        terminalState.Should().NotBe(CombatState.NotStarted);
        terminalState.Should().NotBe(CombatState.Active);
    }
}
