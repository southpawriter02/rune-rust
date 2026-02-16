using FluentAssertions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for <see cref="QuarryMarksResource"/> value object.
/// Tests mutable mark management including creation, FIFO replacement,
/// removal, querying, and lifecycle operations.
/// </summary>
/// <remarks>
/// <para>The Quarry Marks resource is the Veiðimaðr's unique target-tracking system.
/// Unlike <see cref="MedicalSuppliesResource"/> (immutable, creates new instances),
/// Quarry Marks are mutable — marks are added/removed in-place on the same resource.</para>
/// <para>Key behaviors tested:</para>
/// <list type="bullet">
/// <item>Factory <c>Create()</c> initializes empty with max=3</item>
/// <item>FIFO replacement when adding beyond capacity</item>
/// <item>Mark removal by target ID</item>
/// <item>Active mark querying</item>
/// <item>Clear all marks (encounter end)</item>
/// <item>Turn refresh (increments TurnsActive on each mark)</item>
/// </list>
/// <para>Introduced in v0.20.7a.</para>
/// </remarks>
[TestFixture]
public class QuarryMarksResourceTests
{
    // ===== Factory Tests =====

    [Test]
    public void Create_InitializesWithDefaultMaxMarks()
    {
        // Act
        var resource = QuarryMarksResource.Create();

        // Assert
        resource.MaxMarks.Should().Be(QuarryMarksResource.DefaultMaxMarks);
        resource.CurrentMarkCount.Should().Be(0);
        resource.ActiveMarks.Should().BeEmpty();
        resource.CanAddMark().Should().BeTrue();
    }

    // ===== AddMark Tests =====

    [Test]
    public void AddMark_BelowCapacity_AddsSuccessfully()
    {
        // Arrange
        var resource = QuarryMarksResource.Create();
        var targetId = Guid.NewGuid();
        var mark = QuarryMark.Create(targetId, "Draugr Scout", Guid.NewGuid());

        // Act
        var replaced = resource.AddMark(mark);

        // Assert
        replaced.Should().BeNull();
        resource.CurrentMarkCount.Should().Be(1);
        resource.HasMark(targetId).Should().BeTrue();
        resource.ActiveMarks.Should().ContainSingle();
    }

    [Test]
    public void AddMark_MultipleMarks_AddsAllWithinCapacity()
    {
        // Arrange
        var resource = QuarryMarksResource.Create();
        var mark1 = QuarryMark.Create(Guid.NewGuid(), "Draugr Scout", Guid.NewGuid());
        var mark2 = QuarryMark.Create(Guid.NewGuid(), "Corrupted Wolf", Guid.NewGuid());
        var mark3 = QuarryMark.Create(Guid.NewGuid(), "Troll Warden", Guid.NewGuid());

        // Act
        resource.AddMark(mark1);
        resource.AddMark(mark2);
        resource.AddMark(mark3);

        // Assert
        resource.CurrentMarkCount.Should().Be(3);
        resource.CanAddMark().Should().BeFalse();
        resource.HasMark(mark1.TargetId).Should().BeTrue();
        resource.HasMark(mark2.TargetId).Should().BeTrue();
        resource.HasMark(mark3.TargetId).Should().BeTrue();
    }

    [Test]
    public void AddMark_AtCapacity_ReplacesOldestMark()
    {
        // Arrange
        var resource = QuarryMarksResource.Create();
        var hunterId = Guid.NewGuid();
        var mark1 = QuarryMark.Create(Guid.NewGuid(), "Oldest Target", hunterId);
        var mark2 = QuarryMark.Create(Guid.NewGuid(), "Middle Target", hunterId);
        var mark3 = QuarryMark.Create(Guid.NewGuid(), "Newest Target", hunterId);
        var mark4 = QuarryMark.Create(Guid.NewGuid(), "Replacement Target", hunterId);

        resource.AddMark(mark1);
        resource.AddMark(mark2);
        resource.AddMark(mark3);

        // Act — 4th mark triggers FIFO replacement
        var replaced = resource.AddMark(mark4);

        // Assert
        replaced.Should().NotBeNull();
        replaced!.TargetName.Should().Be("Oldest Target");
        replaced.TargetId.Should().Be(mark1.TargetId);
        resource.CurrentMarkCount.Should().Be(3); // Still at max
        resource.HasMark(mark1.TargetId).Should().BeFalse(); // Oldest removed
        resource.HasMark(mark4.TargetId).Should().BeTrue(); // New mark added
        resource.HasMark(mark2.TargetId).Should().BeTrue(); // Middle retained
        resource.HasMark(mark3.TargetId).Should().BeTrue(); // Newest retained
    }

    // ===== RemoveMark Tests =====

    [Test]
    public void RemoveMark_ValidTarget_ReturnsTrue()
    {
        // Arrange
        var resource = QuarryMarksResource.Create();
        var targetId = Guid.NewGuid();
        var mark = QuarryMark.Create(targetId, "Draugr Scout", Guid.NewGuid());
        resource.AddMark(mark);

        // Act
        var removed = resource.RemoveMark(targetId);

        // Assert
        removed.Should().BeTrue();
        resource.CurrentMarkCount.Should().Be(0);
        resource.HasMark(targetId).Should().BeFalse();
    }

    [Test]
    public void RemoveMark_InvalidTarget_ReturnsFalse()
    {
        // Arrange
        var resource = QuarryMarksResource.Create();

        // Act
        var removed = resource.RemoveMark(Guid.NewGuid());

        // Assert
        removed.Should().BeFalse();
        resource.CurrentMarkCount.Should().Be(0);
    }

    // ===== Query Tests =====

    [Test]
    public void HasMark_ActiveMark_ReturnsTrue()
    {
        // Arrange
        var resource = QuarryMarksResource.Create();
        var targetId = Guid.NewGuid();
        var mark = QuarryMark.Create(targetId, "Corrupted Wolf", Guid.NewGuid());
        resource.AddMark(mark);

        // Act & Assert
        resource.HasMark(targetId).Should().BeTrue();
        resource.HasMark(Guid.NewGuid()).Should().BeFalse();
    }

    [Test]
    public void GetMark_ExistingTarget_ReturnsMark()
    {
        // Arrange
        var resource = QuarryMarksResource.Create();
        var targetId = Guid.NewGuid();
        var mark = QuarryMark.Create(targetId, "Troll Warden", Guid.NewGuid());
        resource.AddMark(mark);

        // Act
        var retrieved = resource.GetMark(targetId);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.TargetId.Should().Be(targetId);
        retrieved.TargetName.Should().Be("Troll Warden");
        retrieved.HitBonus.Should().Be(QuarryMark.DefaultHitBonus);
    }

    [Test]
    public void GetMark_NonExistentTarget_ReturnsNull()
    {
        // Arrange
        var resource = QuarryMarksResource.Create();

        // Act
        var retrieved = resource.GetMark(Guid.NewGuid());

        // Assert
        retrieved.Should().BeNull();
    }

    // ===== ClearAllMarks Tests =====

    [Test]
    public void ClearAllMarks_RemovesAllMarks()
    {
        // Arrange
        var resource = QuarryMarksResource.Create();
        var hunterId = Guid.NewGuid();
        resource.AddMark(QuarryMark.Create(Guid.NewGuid(), "Target 1", hunterId));
        resource.AddMark(QuarryMark.Create(Guid.NewGuid(), "Target 2", hunterId));
        resource.AddMark(QuarryMark.Create(Guid.NewGuid(), "Target 3", hunterId));
        resource.CurrentMarkCount.Should().Be(3);

        // Act
        resource.ClearAllMarks();

        // Assert
        resource.CurrentMarkCount.Should().Be(0);
        resource.ActiveMarks.Should().BeEmpty();
        resource.CanAddMark().Should().BeTrue();
    }

    // ===== Turn Refresh Tests =====

    [Test]
    public void RefreshMarksForNewTurn_IncrementsTurnsActiveOnAllMarks()
    {
        // Arrange
        var resource = QuarryMarksResource.Create();
        var hunterId = Guid.NewGuid();
        var mark1 = QuarryMark.Create(Guid.NewGuid(), "Target 1", hunterId);
        var mark2 = QuarryMark.Create(Guid.NewGuid(), "Target 2", hunterId);
        resource.AddMark(mark1);
        resource.AddMark(mark2);

        mark1.TurnsActive.Should().Be(0);
        mark2.TurnsActive.Should().Be(0);

        // Act
        resource.RefreshMarksForNewTurn();

        // Assert
        mark1.TurnsActive.Should().Be(1);
        mark2.TurnsActive.Should().Be(1);

        // Act — second turn
        resource.RefreshMarksForNewTurn();

        // Assert
        mark1.TurnsActive.Should().Be(2);
        mark2.TurnsActive.Should().Be(2);
    }

    // ===== GetOldestMark Tests =====

    [Test]
    public void GetOldestMark_WithMarks_ReturnsFirstAdded()
    {
        // Arrange
        var resource = QuarryMarksResource.Create();
        var hunterId = Guid.NewGuid();
        var oldest = QuarryMark.Create(Guid.NewGuid(), "Oldest", hunterId);
        var newer = QuarryMark.Create(Guid.NewGuid(), "Newer", hunterId);
        resource.AddMark(oldest);
        resource.AddMark(newer);

        // Act
        var result = resource.GetOldestMark();

        // Assert
        result.Should().NotBeNull();
        result!.TargetName.Should().Be("Oldest");
    }

    [Test]
    public void GetOldestMark_Empty_ReturnsNull()
    {
        // Arrange
        var resource = QuarryMarksResource.Create();

        // Act
        var result = resource.GetOldestMark();

        // Assert
        result.Should().BeNull();
    }

    // ===== Display Tests =====

    [Test]
    public void GetFormattedValue_ReturnsCurrentOverMax()
    {
        // Arrange
        var resource = QuarryMarksResource.Create();
        var hunterId = Guid.NewGuid();
        resource.AddMark(QuarryMark.Create(Guid.NewGuid(), "Target 1", hunterId));
        resource.AddMark(QuarryMark.Create(Guid.NewGuid(), "Target 2", hunterId));

        // Act
        var formatted = resource.GetFormattedValue();

        // Assert
        formatted.Should().Be("2/3");
    }

    [Test]
    public void GetFormattedValue_Empty_ReturnsZeroOverMax()
    {
        // Arrange
        var resource = QuarryMarksResource.Create();

        // Act
        var formatted = resource.GetFormattedValue();

        // Assert
        formatted.Should().Be("0/3");
    }
}
