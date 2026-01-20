using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Events;

namespace RuneAndRust.Domain.UnitTests.Events;

/// <summary>
/// Tests for category-specific event records.
/// </summary>
[TestFixture]
public class CategoryEventTests
{
    [Test]
    public void CombatEvent_Attack_CreatesCorrectEvent()
    {
        // Arrange
        var attackerId = Guid.NewGuid();
        var targetId = Guid.NewGuid();

        // Act
        var evt = CombatEvent.Attack(attackerId, targetId, 25, true);

        // Assert
        evt.Category.Should().Be(EventCategory.Combat);
        evt.EventType.Should().Be("Attack");
        evt.AttackerId.Should().Be(attackerId);
        evt.TargetId.Should().Be(targetId);
        evt.Damage.Should().Be(25);
        evt.IsCritical.Should().BeTrue();
    }

    [Test]
    public void CombatEvent_Death_CreatesCorrectEvent()
    {
        // Arrange
        var targetId = Guid.NewGuid();

        // Act
        var evt = CombatEvent.Death(targetId);

        // Assert
        evt.EventType.Should().Be("Death");
        evt.TargetId.Should().Be(targetId);
    }

    [Test]
    public void ExplorationEvent_Moved_CreatesCorrectEvent()
    {
        // Arrange
        var from = Guid.NewGuid();
        var to = Guid.NewGuid();

        // Act
        var evt = ExplorationEvent.Moved(from, to, Direction.North);

        // Assert
        evt.EventType.Should().Be("Moved");
        evt.FromRoomId.Should().Be(from);
        evt.ToRoomId.Should().Be(to);
        evt.Direction.Should().Be(Direction.North);
    }

    [Test]
    public void InteractionEvent_Interacted_CreatesCorrectEvent()
    {
        // Arrange
        var objectId = Guid.NewGuid();

        // Act
        var evt = InteractionEvent.Interacted(objectId, "Iron Door", InteractionType.Open, ObjectState.Open);

        // Assert
        evt.EventType.Should().Be("Open");
        evt.ObjectName.Should().Be("Iron Door");
        evt.InteractionPerformed.Should().Be(InteractionType.Open);
        evt.ResultState.Should().Be(ObjectState.Open);
    }

    [Test]
    public void QuestEvent_Completed_CreatesCorrectEvent()
    {
        // Arrange
        var playerId = Guid.NewGuid();

        // Act
        var evt = QuestEvent.Completed("quest-1", "The Lost Artifact", playerId);

        // Assert
        evt.EventType.Should().Be("QuestCompleted");
        evt.QuestId.Should().Be("quest-1");
        evt.QuestName.Should().Be("The Lost Artifact");
        evt.PlayerId.Should().Be(playerId);
    }

    [Test]
    public void CharacterEvent_LevelUp_CreatesCorrectEvent()
    {
        // Arrange
        var playerId = Guid.NewGuid();

        // Act
        var evt = CharacterEvent.LevelUp(playerId, 5);

        // Assert
        evt.EventType.Should().Be("LevelUp");
        evt.NewValue.Should().Be(5);
    }

    [Test]
    public void DiceEvent_Rolled_CreatesCorrectEvent()
    {
        // Act
        var evt = DiceEvent.Rolled("2d6+3", [4, 5], 12);

        // Assert
        evt.EventType.Should().Be("DiceRolled");
        evt.Notation.Should().Be("2d6+3");
        evt.Results.Should().BeEquivalentTo(new[] { 4, 5 });
        evt.Total.Should().Be(12);
    }
}
