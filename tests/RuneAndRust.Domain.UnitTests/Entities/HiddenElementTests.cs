using FluentAssertions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Entities;

[TestFixture]
public class HiddenElementTests
{
    private readonly Guid _testRoomId = Guid.NewGuid();

    [Test]
    public void CreateTrap_CreatesValidTrapElement()
    {
        // Arrange & Act
        var trap = HiddenElement.CreateTrap(
            "Pressure Plate",
            "Your trained eye catches a discrepancy—a pressure plate!",
            15, 20, 14, _testRoomId);

        // Assert
        trap.ElementType.Should().Be(HiddenElementType.Trap);
        trap.Name.Should().Be("Pressure Plate");
        trap.DetectionDC.Should().Be(15);
        trap.TrapDamage.Should().Be(20);
        trap.DisarmDC.Should().Be(14);
        trap.IsRevealed.Should().BeFalse();
        trap.IsDisarmed.Should().BeFalse();
    }

    [Test]
    public void CreateSecretDoor_CreatesValidSecretDoorElement()
    {
        // Arrange
        var targetRoomId = Guid.NewGuid();

        // Act
        var door = HiddenElement.CreateSecretDoor(
            "Hidden Passage",
            "Air currents suggest a space behind this wall.",
            16, _testRoomId, targetRoomId);

        // Assert
        door.ElementType.Should().Be(HiddenElementType.SecretDoor);
        door.Name.Should().Be("Hidden Passage");
        door.DetectionDC.Should().Be(16);
        door.LeadsToRoomId.Should().Be(targetRoomId);
        door.IsRevealed.Should().BeFalse();
    }

    [Test]
    public void CreateCache_CreatesValidCacheElement()
    {
        // Arrange & Act
        var cache = HiddenElement.CreateCache(
            "Loose Floor Panel",
            "Something's hidden here—you spot a loose floor panel.",
            14, _testRoomId, "Gold coins, a rusty key");

        // Assert
        cache.ElementType.Should().Be(HiddenElementType.Cache);
        cache.Name.Should().Be("Loose Floor Panel");
        cache.DetectionDC.Should().Be(14);
        cache.CacheContents.Should().Be("Gold coins, a rusty key");
        cache.IsRevealed.Should().BeFalse();
        cache.IsLooted.Should().BeFalse();
    }

    [Test]
    public void Reveal_SetsIsRevealedToTrue()
    {
        // Arrange
        var trap = HiddenElement.CreateTrap("Test", "Test text", 10, 10, 10, _testRoomId);

        // Act
        trap.Reveal();

        // Assert
        trap.IsRevealed.Should().BeTrue();
    }

    [Test]
    public void TryDisarm_OnTrap_SetsIsDisarmedToTrue()
    {
        // Arrange
        var trap = HiddenElement.CreateTrap("Test", "Test text", 10, 10, 10, _testRoomId);

        // Act
        var result = trap.TryDisarm();

        // Assert
        result.Should().BeTrue();
        trap.IsDisarmed.Should().BeTrue();
    }

    [Test]
    public void TryDisarm_OnNonTrap_ReturnsFalse()
    {
        // Arrange
        var cache = HiddenElement.CreateCache("Test", "Test text", 10, _testRoomId, "contents");

        // Act
        var result = cache.TryDisarm();

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void TryLoot_OnCache_SetsIsLootedToTrue()
    {
        // Arrange
        var cache = HiddenElement.CreateCache("Test", "Test text", 10, _testRoomId, "contents");

        // Act
        var result = cache.TryLoot();

        // Assert
        result.Should().BeTrue();
        cache.IsLooted.Should().BeTrue();
    }

    [Test]
    public void TryLoot_OnAlreadyLootedCache_ReturnsFalse()
    {
        // Arrange
        var cache = HiddenElement.CreateCache("Test", "Test text", 10, _testRoomId, "contents");
        cache.TryLoot();

        // Act
        var result = cache.TryLoot();

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void CanBeDetectedBy_WhenPassivePerceptionMeetsDC_ReturnsTrue()
    {
        // Arrange
        var trap = HiddenElement.CreateTrap("Test", "Test text", 15, 10, 10, _testRoomId);

        // Act & Assert
        trap.CanBeDetectedBy(15).Should().BeTrue();
        trap.CanBeDetectedBy(16).Should().BeTrue();
        trap.CanBeDetectedBy(20).Should().BeTrue();
    }

    [Test]
    public void CanBeDetectedBy_WhenPassivePerceptionBelowDC_ReturnsFalse()
    {
        // Arrange
        var trap = HiddenElement.CreateTrap("Test", "Test text", 15, 10, 10, _testRoomId);

        // Act & Assert
        trap.CanBeDetectedBy(14).Should().BeFalse();
        trap.CanBeDetectedBy(10).Should().BeFalse();
        trap.CanBeDetectedBy(1).Should().BeFalse();
    }

    [Test]
    public void CanBeDetectedBy_WhenAlreadyRevealed_ReturnsFalse()
    {
        // Arrange
        var trap = HiddenElement.CreateTrap("Test", "Test text", 15, 10, 10, _testRoomId);
        trap.Reveal();

        // Act & Assert
        trap.CanBeDetectedBy(20).Should().BeFalse();
    }

    [Test]
    public void CreateTrap_WithInvalidDamage_ThrowsException()
    {
        // Arrange & Act
        var act = () => HiddenElement.CreateTrap("Test", "Test", 10, 0, 10, _testRoomId);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void CreateTrap_WithInvalidDisarmDC_ThrowsException()
    {
        // Arrange & Act
        var act = () => HiddenElement.CreateTrap("Test", "Test", 10, 10, 0, _testRoomId);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void CreateCache_WithEmptyContents_ThrowsException()
    {
        // Arrange & Act
        var act = () => HiddenElement.CreateCache("Test", "Test", 10, _testRoomId, "");

        // Assert
        act.Should().Throw<ArgumentException>();
    }
}
