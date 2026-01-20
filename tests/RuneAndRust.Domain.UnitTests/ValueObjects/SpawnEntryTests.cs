using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Tests for SpawnEntry value object.
/// </summary>
[TestFixture]
public class SpawnEntryTests
{
    [Test]
    public void Create_WithValidParameters_CreatesEntry()
    {
        // Act
        var entry = SpawnEntry.Create("goblin", weight: 80, minDepth: 2, maxDepth: 5);

        // Assert
        entry.EntityId.Should().Be("goblin");
        entry.Weight.Should().Be(80);
        entry.MinDepth.Should().Be(2);
        entry.MaxDepth.Should().Be(5);
    }

    [Test]
    public void Create_WithMaxDepthLessThanMinDepth_ThrowsArgumentException()
    {
        // Act
        var act = () => SpawnEntry.Create("goblin", minDepth: 5, maxDepth: 2);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void IsValidForDepth_WithDepthInRange_ReturnsTrue()
    {
        // Arrange
        var entry = SpawnEntry.Create("goblin", minDepth: 2, maxDepth: 5);

        // Act & Assert
        entry.IsValidForDepth(2).Should().BeTrue();
        entry.IsValidForDepth(3).Should().BeTrue();
        entry.IsValidForDepth(5).Should().BeTrue();
    }

    [Test]
    public void IsValidForDepth_WithDepthOutOfRange_ReturnsFalse()
    {
        // Arrange
        var entry = SpawnEntry.Create("goblin", minDepth: 2, maxDepth: 5);

        // Act & Assert
        entry.IsValidForDepth(1).Should().BeFalse();
        entry.IsValidForDepth(6).Should().BeFalse();
    }

    [Test]
    public void HasRequiredTags_WithAllTagsPresent_ReturnsTrue()
    {
        // Arrange
        var entry = SpawnEntry.Create("goblin", requiredTags: new[] { "dark", "underground" });

        // Act & Assert
        entry.HasRequiredTags(new[] { "dark", "underground", "damp" }).Should().BeTrue();
    }

    [Test]
    public void HasRequiredTags_WithMissingTags_ReturnsFalse()
    {
        // Arrange
        var entry = SpawnEntry.Create("goblin", requiredTags: new[] { "dark", "underground" });

        // Act & Assert
        entry.HasRequiredTags(new[] { "dark" }).Should().BeFalse();
    }
}
