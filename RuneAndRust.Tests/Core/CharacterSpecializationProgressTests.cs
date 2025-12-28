using FluentAssertions;
using RuneAndRust.Core.Entities;
using Xunit;

namespace RuneAndRust.Tests.Core;

/// <summary>
/// Tests for the CharacterSpecializationProgress entity.
/// Validates junction table creation and timestamp behavior.
/// </summary>
/// <remarks>See: v0.4.1a for specialization system implementation.</remarks>
public class CharacterSpecializationProgressTests
{
    [Fact]
    public void Constructor_InitializesDefaults()
    {
        // Arrange & Act
        var progress = new CharacterSpecializationProgress();

        // Assert
        progress.CharacterId.Should().Be(Guid.Empty);
        progress.NodeId.Should().Be(Guid.Empty);
        progress.UnlockedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void CharacterId_CanBeSet()
    {
        // Arrange
        var characterId = Guid.NewGuid();

        // Act
        var progress = new CharacterSpecializationProgress { CharacterId = characterId };

        // Assert
        progress.CharacterId.Should().Be(characterId);
    }

    [Fact]
    public void NodeId_CanBeSet()
    {
        // Arrange
        var nodeId = Guid.NewGuid();

        // Act
        var progress = new CharacterSpecializationProgress { NodeId = nodeId };

        // Assert
        progress.NodeId.Should().Be(nodeId);
    }

    [Fact]
    public void UnlockedAt_SetsOnConstruction()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var progress = new CharacterSpecializationProgress();

        // Assert
        progress.UnlockedAt.Should().BeOnOrAfter(before);
        progress.UnlockedAt.Should().BeOnOrBefore(DateTime.UtcNow);
    }

    [Fact]
    public void UnlockedAt_CanBeOverridden()
    {
        // Arrange
        var customTime = new DateTime(2025, 1, 15, 10, 30, 0, DateTimeKind.Utc);

        // Act
        var progress = new CharacterSpecializationProgress { UnlockedAt = customTime };

        // Assert
        progress.UnlockedAt.Should().Be(customTime);
    }

    [Fact]
    public void FullProgress_HasAllProperties()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var nodeId = Guid.NewGuid();
        var unlockedAt = DateTime.UtcNow;

        // Act
        var progress = new CharacterSpecializationProgress
        {
            CharacterId = characterId,
            NodeId = nodeId,
            UnlockedAt = unlockedAt
        };

        // Assert
        progress.CharacterId.Should().Be(characterId);
        progress.NodeId.Should().Be(nodeId);
        progress.UnlockedAt.Should().Be(unlockedAt);
    }
}
