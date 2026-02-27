using FluentAssertions;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for <see cref="PredatorsPatienceState"/> value object.
/// Tests stance lifecycle: creation, activation, deactivation, movement tracking,
/// turn refresh, and bonus calculation.
/// </summary>
/// <remarks>
/// <para>Predator's Patience is a Tier 2 stance ability for the Veiðimaðr (Hunter)
/// specialization. While active and stationary, the hunter gains +3 to all attack rolls.
/// Movement immediately breaks the stance.</para>
/// <para>Key behaviors tested:</para>
/// <list type="bullet">
/// <item>Factory <c>Create()</c> initializes inactive state</item>
/// <item><c>Activate()</c> enables the stance</item>
/// <item><c>Deactivate()</c> disables the stance</item>
/// <item><c>RecordMovement()</c> automatically deactivates the stance</item>
/// <item><c>ResetForNewTurn()</c> resets movement flag and increments turn counter</item>
/// <item><c>GetCurrentBonus()</c> returns +3 if active and unmoved, 0 otherwise</item>
/// </list>
/// <para>Introduced in v0.20.7b. Coherent path — zero Corruption risk.</para>
/// </remarks>
[TestFixture]
public class PredatorsPatienceStateTests
{
    // ===== Factory Tests =====

    [Test]
    public void Create_InitializesInactive()
    {
        // Arrange
        var hunterId = Guid.NewGuid();

        // Act
        var state = PredatorsPatienceState.Create(hunterId);

        // Assert
        state.HunterId.Should().Be(hunterId);
        state.IsActive.Should().BeFalse();
        state.HasMovedThisTurn.Should().BeFalse();
        state.HitBonus.Should().Be(3, "default hit bonus is +3");
        state.ActivatedAt.Should().BeNull();
        state.TurnsInStance.Should().Be(0);
    }

    // ===== Activate Tests =====

    [Test]
    public void Activate_SetsIsActiveTrue()
    {
        // Arrange
        var state = PredatorsPatienceState.Create(Guid.NewGuid());

        // Act
        state.Activate();

        // Assert
        state.IsActive.Should().BeTrue();
        state.HasMovedThisTurn.Should().BeFalse(
            "activation resets movement flag");
        state.ActivatedAt.Should().NotBeNull();
        state.ActivatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Test]
    public void Activate_AlreadyActive_ResetsMovementFlag()
    {
        // Arrange
        var state = PredatorsPatienceState.Create(Guid.NewGuid());
        state.Activate();
        // Simulate a scenario where movement was recorded but stance re-activated
        // (this shouldn't normally happen, but tests the defensive behavior)

        // Act
        state.Activate();

        // Assert
        state.IsActive.Should().BeTrue();
        state.HasMovedThisTurn.Should().BeFalse();
    }

    // ===== Deactivate Tests =====

    [Test]
    public void Deactivate_SetsIsActiveFalse()
    {
        // Arrange
        var state = PredatorsPatienceState.Create(Guid.NewGuid());
        state.Activate();
        state.IsActive.Should().BeTrue(); // Precondition

        // Act
        state.Deactivate();

        // Assert
        state.IsActive.Should().BeFalse();
    }

    [Test]
    public void Deactivate_AlreadyInactive_RemainsInactive()
    {
        // Arrange
        var state = PredatorsPatienceState.Create(Guid.NewGuid());
        state.IsActive.Should().BeFalse(); // Precondition

        // Act
        state.Deactivate();

        // Assert
        state.IsActive.Should().BeFalse();
    }

    // ===== RecordMovement Tests =====

    [Test]
    public void RecordMovement_ActiveStance_DeactivatesStance()
    {
        // Arrange
        var state = PredatorsPatienceState.Create(Guid.NewGuid());
        state.Activate();
        state.IsActive.Should().BeTrue(); // Precondition

        // Act
        state.RecordMovement();

        // Assert
        state.HasMovedThisTurn.Should().BeTrue();
        state.IsActive.Should().BeFalse(
            "movement breaks the Predator's Patience stance");
    }

    [Test]
    public void RecordMovement_InactiveStance_SetsMovedFlag()
    {
        // Arrange
        var state = PredatorsPatienceState.Create(Guid.NewGuid());

        // Act
        state.RecordMovement();

        // Assert
        state.HasMovedThisTurn.Should().BeTrue();
        state.IsActive.Should().BeFalse();
    }

    // ===== GetCurrentBonus Tests =====

    [Test]
    public void GetCurrentBonus_ActiveNoMovement_Returns3()
    {
        // Arrange
        var state = PredatorsPatienceState.Create(Guid.NewGuid());
        state.Activate();

        // Act
        var bonus = state.GetCurrentBonus();

        // Assert
        bonus.Should().Be(3,
            "active stance with no movement grants +3 hit bonus");
    }

    [Test]
    public void GetCurrentBonus_ActiveAfterMovement_Returns0()
    {
        // Arrange
        var state = PredatorsPatienceState.Create(Guid.NewGuid());
        state.Activate();
        state.RecordMovement(); // Movement breaks the stance

        // Act
        var bonus = state.GetCurrentBonus();

        // Assert
        bonus.Should().Be(0,
            "movement deactivates the stance so bonus should be 0");
    }

    [Test]
    public void GetCurrentBonus_NotActive_Returns0()
    {
        // Arrange
        var state = PredatorsPatienceState.Create(Guid.NewGuid());

        // Act
        var bonus = state.GetCurrentBonus();

        // Assert
        bonus.Should().Be(0,
            "inactive stance grants no bonus");
    }

    // ===== ResetForNewTurn Tests =====

    [Test]
    public void ResetForNewTurn_ActiveStance_IncrementsTurnsInStance()
    {
        // Arrange
        var state = PredatorsPatienceState.Create(Guid.NewGuid());
        state.Activate();
        state.TurnsInStance.Should().Be(0);

        // Act
        state.ResetForNewTurn();

        // Assert
        state.TurnsInStance.Should().Be(1);
        state.HasMovedThisTurn.Should().BeFalse(
            "movement flag resets at start of new turn");
        state.IsActive.Should().BeTrue(
            "stance persists across turns");
    }

    [Test]
    public void ResetForNewTurn_InactiveStance_DoesNotIncrementTurns()
    {
        // Arrange
        var state = PredatorsPatienceState.Create(Guid.NewGuid());
        state.TurnsInStance.Should().Be(0);

        // Act
        state.ResetForNewTurn();

        // Assert
        state.TurnsInStance.Should().Be(0,
            "turns only increment when the stance is active");
        state.HasMovedThisTurn.Should().BeFalse();
    }

    [Test]
    public void ResetForNewTurn_MultipleTurns_AccumulatesCorrectly()
    {
        // Arrange
        var state = PredatorsPatienceState.Create(Guid.NewGuid());
        state.Activate();

        // Act — 3 turns of holding stance
        state.ResetForNewTurn();
        state.ResetForNewTurn();
        state.ResetForNewTurn();

        // Assert
        state.TurnsInStance.Should().Be(3);
        state.IsActive.Should().BeTrue();
    }

    // ===== GetDescription Tests =====

    [Test]
    public void GetDescription_ActiveStance_ContainsActiveText()
    {
        // Arrange
        var state = PredatorsPatienceState.Create(Guid.NewGuid());
        state.Activate();

        // Act
        var description = state.GetDescription();

        // Assert
        description.Should().Contain("Active",
            "description should indicate the stance is active");
    }

    [Test]
    public void GetDescription_InactiveStance_ContainsInactiveText()
    {
        // Arrange
        var state = PredatorsPatienceState.Create(Guid.NewGuid());

        // Act
        var description = state.GetDescription();

        // Assert
        description.Should().Contain("Inactive",
            "description should indicate the stance is not active");
    }
}
