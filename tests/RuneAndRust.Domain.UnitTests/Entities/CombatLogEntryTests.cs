using FluentAssertions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Entities;

[TestFixture]
public class CombatLogEntryTests
{
    [Test]
    public void Create_ShouldSetAllProperties()
    {
        // Arrange & Act
        var entry = CombatLogEntry.Create(
            roundNumber: 2,
            type: CombatLogType.Attack,
            message: "Test attack message",
            actorName: "Goblin",
            targetName: "Player",
            damage: 10,
            isCritical: true);

        // Assert
        entry.Id.Should().NotBeEmpty();
        entry.RoundNumber.Should().Be(2);
        entry.Type.Should().Be(CombatLogType.Attack);
        entry.Message.Should().Be("Test attack message");
        entry.ActorName.Should().Be("Goblin");
        entry.TargetName.Should().Be("Player");
        entry.Damage.Should().Be(10);
        entry.IsCritical.Should().BeTrue();
        entry.IsMiss.Should().BeFalse();
        entry.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Test]
    public void Create_HealEntry_ShouldTrackHealing()
    {
        // Arrange & Act
        var entry = CombatLogEntry.Create(
            roundNumber: 1,
            type: CombatLogType.Heal,
            message: "Healed",
            actorName: "Shaman",
            targetName: "Goblin",
            healing: 15);

        // Assert
        entry.Type.Should().Be(CombatLogType.Heal);
        entry.Healing.Should().Be(15);
        entry.Damage.Should().BeNull();
    }

    [Test]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var entry = CombatLogEntry.Create(
            roundNumber: 3,
            type: CombatLogType.Defeat,
            message: "Monster defeated!");

        // Act
        var result = entry.ToString();

        // Assert
        result.Should().Contain("[R3]");
        result.Should().Contain("Defeat");
        result.Should().Contain("Monster defeated!");
    }
}
