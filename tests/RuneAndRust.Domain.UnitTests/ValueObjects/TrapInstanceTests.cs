using FluentAssertions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for <see cref="TrapInstance"/> value object.
/// Tests the full trap lifecycle: creation, triggering, disarming, destruction,
/// and turn tracking.
/// </summary>
/// <remarks>
/// <para>TrapInstance represents a single hunting trap placed by the Veiðimaðr.
/// Traps follow a one-way lifecycle: Armed → Triggered/Disarmed → Destroyed.</para>
/// <para>Key behaviors tested:</para>
/// <list type="bullet">
/// <item>Factory <c>Create()</c> initializes with Armed status and validates parameters</item>
/// <item><c>Trigger()</c> transitions Armed → Triggered and records target info</item>
/// <item><c>Disarm()</c> transitions Armed → Disarmed</item>
/// <item><c>Destroy()</c> transitions any state → Destroyed</item>
/// <item><c>IncrementTurn()</c> tracks trap age</item>
/// <item>Invalid state transitions throw <see cref="InvalidOperationException"/></item>
/// </list>
/// <para>Introduced in v0.20.7b. Coherent path — zero Corruption risk.</para>
/// </remarks>
[TestFixture]
public class TrapInstanceTests
{
    // ===== Factory Tests =====

    [Test]
    public void Create_ValidParams_ReturnsArmedTrap()
    {
        // Arrange
        var placedBy = Guid.NewGuid();

        // Act
        var trap = TrapInstance.Create(placedBy, 5, 3, TrapType.Spike);

        // Assert
        trap.PlacedBy.Should().Be(placedBy);
        trap.X.Should().Be(5);
        trap.Y.Should().Be(3);
        trap.Type.Should().Be(TrapType.Spike);
        trap.Status.Should().Be(TrapStatus.Armed);
        trap.DamageRoll.Should().Be(TrapInstance.DefaultDamageRoll);
        trap.ImmobilizeTurns.Should().Be(TrapInstance.DefaultImmobilizeTurns);
        trap.DetectionDc.Should().Be(TrapInstance.DefaultDetectionDc);
        trap.TrapId.Should().NotBe(Guid.Empty);
        trap.TriggeredAt.Should().BeNull();
        trap.TriggeringTarget.Should().BeNull();
        trap.TurnsPlaced.Should().Be(0);
    }

    [Test]
    public void Create_EmptyPlacedBy_ThrowsArgumentException()
    {
        // Act
        var act = () => TrapInstance.Create(Guid.Empty, 0, 0, TrapType.Net);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("placedBy")
            .WithMessage("*PlacedBy ID cannot be empty*");
    }

    [Test]
    public void Create_AllTrapTypes_Succeed()
    {
        // Arrange
        var placedBy = Guid.NewGuid();

        // Act & Assert — each trap type creates successfully
        foreach (var trapType in Enum.GetValues<TrapType>())
        {
            var trap = TrapInstance.Create(placedBy, 0, 0, trapType);
            trap.Type.Should().Be(trapType);
            trap.Status.Should().Be(TrapStatus.Armed);
        }
    }

    // ===== Trigger Tests =====

    [Test]
    public void Trigger_ArmedTrap_SetsTriggeredStatus()
    {
        // Arrange
        var trap = TrapInstance.Create(Guid.NewGuid(), 5, 3, TrapType.Deadfall);
        var targetId = Guid.NewGuid();

        // Act
        trap.Trigger(targetId);

        // Assert
        trap.Status.Should().Be(TrapStatus.Triggered);
        trap.TriggeringTarget.Should().Be(targetId);
        trap.TriggeredAt.Should().NotBeNull();
        trap.TriggeredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Test]
    public void Trigger_TriggeredTrap_ThrowsInvalidOperationException()
    {
        // Arrange
        var trap = TrapInstance.Create(Guid.NewGuid(), 5, 3, TrapType.PitFall);
        trap.Trigger(Guid.NewGuid()); // Already triggered

        // Act
        var act = () => trap.Trigger(Guid.NewGuid());

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot trigger trap*")
            .WithMessage("*Triggered*");
    }

    [Test]
    public void Trigger_DisarmedTrap_ThrowsInvalidOperationException()
    {
        // Arrange
        var trap = TrapInstance.Create(Guid.NewGuid(), 5, 3, TrapType.Snare);
        trap.Disarm(); // Now disarmed

        // Act
        var act = () => trap.Trigger(Guid.NewGuid());

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot trigger trap*")
            .WithMessage("*Disarmed*");
    }

    // ===== Disarm Tests =====

    [Test]
    public void Disarm_ArmedTrap_SetsDisarmedStatus()
    {
        // Arrange
        var trap = TrapInstance.Create(Guid.NewGuid(), 2, 7, TrapType.Net);

        // Act
        trap.Disarm();

        // Assert
        trap.Status.Should().Be(TrapStatus.Disarmed);
    }

    [Test]
    public void Disarm_TriggeredTrap_ThrowsInvalidOperationException()
    {
        // Arrange
        var trap = TrapInstance.Create(Guid.NewGuid(), 2, 7, TrapType.Spike);
        trap.Trigger(Guid.NewGuid()); // Already triggered

        // Act
        var act = () => trap.Disarm();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot disarm trap*")
            .WithMessage("*Triggered*");
    }

    // ===== Destroy Tests =====

    [Test]
    public void Destroy_ArmedTrap_SetsDestroyedStatus()
    {
        // Arrange
        var trap = TrapInstance.Create(Guid.NewGuid(), 1, 1, TrapType.Deadfall);

        // Act
        trap.Destroy();

        // Assert
        trap.Status.Should().Be(TrapStatus.Destroyed);
    }

    [Test]
    public void Destroy_TriggeredTrap_SetsDestroyedStatus()
    {
        // Arrange
        var trap = TrapInstance.Create(Guid.NewGuid(), 1, 1, TrapType.PitFall);
        trap.Trigger(Guid.NewGuid());

        // Act
        trap.Destroy();

        // Assert
        trap.Status.Should().Be(TrapStatus.Destroyed,
            "Destroy() can be called from any state");
    }

    [Test]
    public void Destroy_DisarmedTrap_SetsDestroyedStatus()
    {
        // Arrange
        var trap = TrapInstance.Create(Guid.NewGuid(), 1, 1, TrapType.Snare);
        trap.Disarm();

        // Act
        trap.Destroy();

        // Assert
        trap.Status.Should().Be(TrapStatus.Destroyed,
            "Destroy() can be called from any state");
    }

    // ===== IncrementTurn Tests =====

    [Test]
    public void IncrementTurn_IncreasesTurnsPlaced()
    {
        // Arrange
        var trap = TrapInstance.Create(Guid.NewGuid(), 3, 4, TrapType.Net);
        trap.TurnsPlaced.Should().Be(0);

        // Act
        trap.IncrementTurn();

        // Assert
        trap.TurnsPlaced.Should().Be(1);

        // Act — second increment
        trap.IncrementTurn();

        // Assert
        trap.TurnsPlaced.Should().Be(2);
    }

    // ===== Description Tests =====

    [Test]
    public void GetDescription_ArmedTrap_ContainsArmedText()
    {
        // Arrange
        var trap = TrapInstance.Create(Guid.NewGuid(), 5, 3, TrapType.Spike);

        // Act
        var description = trap.GetDescription();

        // Assert
        description.Should().Contain("Spike");
        description.Should().Contain("(5, 3)");
        description.Should().Contain("armed");
    }

    [Test]
    public void GetDamageText_ReturnsFormattedDamageString()
    {
        // Arrange
        var trap = TrapInstance.Create(Guid.NewGuid(), 0, 0, TrapType.Deadfall);

        // Act
        var damageText = trap.GetDamageText();

        // Assert
        damageText.Should().Contain("1d8");
        damageText.Should().Contain("piercing");
        damageText.Should().Contain("1-turn immobilize");
    }
}
