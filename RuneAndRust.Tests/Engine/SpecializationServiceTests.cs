using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Events;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;
using RuneAndRust.Engine.Services;
using Xunit;

namespace RuneAndRust.Tests.Engine;

/// <summary>
/// Tests for the SpecializationService class.
/// Validates specialization unlocking, node purchasing, prerequisite validation,
/// and event publishing.
/// </summary>
/// <remarks>See: v0.4.1b (The Unlock) for system design.</remarks>
public class SpecializationServiceTests
{
    private readonly Mock<ISpecializationRepository> _mockRepo;
    private readonly Mock<IEventBus> _mockEventBus;
    private readonly Mock<ILogger<SpecializationService>> _mockLogger;
    private readonly SpecializationService _sut;

    // Captured events for verification
    private SpecializationUnlockedEvent? _capturedSpecEvent;
    private NodeUnlockedEvent? _capturedNodeEvent;

    public SpecializationServiceTests()
    {
        _mockRepo = new Mock<ISpecializationRepository>();
        _mockEventBus = new Mock<IEventBus>();
        _mockLogger = new Mock<ILogger<SpecializationService>>();

        // Capture published events
        _mockEventBus
            .Setup(e => e.Publish(It.IsAny<SpecializationUnlockedEvent>()))
            .Callback<SpecializationUnlockedEvent>(evt => _capturedSpecEvent = evt);

        _mockEventBus
            .Setup(e => e.Publish(It.IsAny<NodeUnlockedEvent>()))
            .Callback<NodeUnlockedEvent>(evt => _capturedNodeEvent = evt);

        _sut = new SpecializationService(
            _mockRepo.Object,
            _mockEventBus.Object,
            _mockLogger.Object);
    }

    #region Helper Methods

    private static Character CreateTestCharacter(
        int pp = 15,
        int level = 5,
        ArchetypeType archetype = ArchetypeType.Warrior)
    {
        return new Character
        {
            Id = Guid.NewGuid(),
            Name = "TestHero",
            ProgressionPoints = pp,
            Level = level,
            Archetype = archetype,
            UnlockedSpecializationIds = new List<Guid>(),
            SpecializationProgress = new List<CharacterSpecializationProgress>()
        };
    }

    private static Specialization CreateTestSpec(
        ArchetypeType archetype = ArchetypeType.Warrior,
        int requiredLevel = 1)
    {
        return new Specialization
        {
            Id = Guid.NewGuid(),
            Name = "Berserkr",
            Type = SpecializationType.Berserkr,
            RequiredArchetype = archetype,
            RequiredLevel = requiredLevel,
            Nodes = new List<SpecializationNode>()
        };
    }

    private static SpecializationNode CreateTestNode(
        Guid specId,
        int tier = 1,
        int costPp = 1,
        List<Guid>? parentNodeIds = null)
    {
        return new SpecializationNode
        {
            Id = Guid.NewGuid(),
            SpecializationId = specId,
            AbilityId = Guid.NewGuid(),
            Tier = tier,
            CostPP = costPp,
            ParentNodeIds = parentNodeIds ?? new List<Guid>(),
            DisplayName = $"Tier{tier}Node",
            Ability = new ActiveAbility { Id = Guid.NewGuid(), Name = $"Tier{tier}Ability" }
        };
    }

    #endregion

    // ═══════════════════════════════════════════════════════════════════════
    // UnlockSpecializationAsync Tests (16 tests)
    // ═══════════════════════════════════════════════════════════════════════

    #region UnlockSpecializationAsync - Success Tests

    [Fact]
    public async Task UnlockSpecializationAsync_DeductsPP_FromCharacter()
    {
        // Arrange
        var character = CreateTestCharacter(pp: 15);
        var spec = CreateTestSpec();
        _mockRepo.Setup(r => r.GetByIdAsync(spec.Id)).ReturnsAsync(spec);

        // Act
        await _sut.UnlockSpecializationAsync(character, spec.Id);

        // Assert
        character.ProgressionPoints.Should().Be(5); // 15 - 10 = 5
    }

    [Fact]
    public async Task UnlockSpecializationAsync_AddsSpecIdToUnlockedList()
    {
        // Arrange
        var character = CreateTestCharacter();
        var spec = CreateTestSpec();
        _mockRepo.Setup(r => r.GetByIdAsync(spec.Id)).ReturnsAsync(spec);

        // Act
        await _sut.UnlockSpecializationAsync(character, spec.Id);

        // Assert
        character.UnlockedSpecializationIds.Should().Contain(spec.Id);
    }

    [Fact]
    public async Task UnlockSpecializationAsync_UpdatesLastModified()
    {
        // Arrange
        var character = CreateTestCharacter();
        var originalTime = character.LastModified;
        var spec = CreateTestSpec();
        _mockRepo.Setup(r => r.GetByIdAsync(spec.Id)).ReturnsAsync(spec);

        // Act
        await Task.Delay(10); // Ensure time difference
        await _sut.UnlockSpecializationAsync(character, spec.Id);

        // Assert
        character.LastModified.Should().BeAfter(originalTime);
    }

    [Fact]
    public async Task UnlockSpecializationAsync_ReturnsSuccess_WithCorrectDetails()
    {
        // Arrange
        var character = CreateTestCharacter();
        var spec = CreateTestSpec();
        _mockRepo.Setup(r => r.GetByIdAsync(spec.Id)).ReturnsAsync(spec);

        // Act
        var result = await _sut.UnlockSpecializationAsync(character, spec.Id);

        // Assert
        result.Success.Should().BeTrue();
        result.SpecializationId.Should().Be(spec.Id);
        result.SpecializationName.Should().Be(spec.Name);
        result.PpSpent.Should().Be(10);
    }

    [Fact]
    public async Task UnlockSpecializationAsync_PublishesEvent_OnSuccess()
    {
        // Arrange
        var character = CreateTestCharacter();
        var spec = CreateTestSpec();
        _mockRepo.Setup(r => r.GetByIdAsync(spec.Id)).ReturnsAsync(spec);

        // Act
        await _sut.UnlockSpecializationAsync(character, spec.Id);

        // Assert
        _mockEventBus.Verify(
            e => e.Publish(It.IsAny<SpecializationUnlockedEvent>()),
            Times.Once);
    }

    [Fact]
    public async Task UnlockSpecializationAsync_EventContainsCorrectData()
    {
        // Arrange
        var character = CreateTestCharacter();
        var spec = CreateTestSpec();
        _mockRepo.Setup(r => r.GetByIdAsync(spec.Id)).ReturnsAsync(spec);

        // Act
        await _sut.UnlockSpecializationAsync(character, spec.Id);

        // Assert
        _capturedSpecEvent.Should().NotBeNull();
        _capturedSpecEvent!.CharacterId.Should().Be(character.Id);
        _capturedSpecEvent.CharacterName.Should().Be(character.Name);
        _capturedSpecEvent.SpecializationId.Should().Be(spec.Id);
        _capturedSpecEvent.SpecializationName.Should().Be(spec.Name);
        _capturedSpecEvent.ProgressionPointsSpent.Should().Be(10);
    }

    [Fact]
    public async Task UnlockSpecializationAsync_ExactlyTenPP_Succeeds()
    {
        // Arrange
        var character = CreateTestCharacter(pp: 10);
        var spec = CreateTestSpec();
        _mockRepo.Setup(r => r.GetByIdAsync(spec.Id)).ReturnsAsync(spec);

        // Act
        var result = await _sut.UnlockSpecializationAsync(character, spec.Id);

        // Assert
        result.Success.Should().BeTrue();
        character.ProgressionPoints.Should().Be(0);
    }

    [Fact]
    public async Task UnlockSpecializationAsync_HigherLevel_Succeeds()
    {
        // Arrange
        var character = CreateTestCharacter(level: 10);
        var spec = CreateTestSpec(requiredLevel: 5);
        _mockRepo.Setup(r => r.GetByIdAsync(spec.Id)).ReturnsAsync(spec);

        // Act
        var result = await _sut.UnlockSpecializationAsync(character, spec.Id);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task UnlockSpecializationAsync_MultipleSpecs_CanUnlockSequentially()
    {
        // Arrange
        var character = CreateTestCharacter(pp: 30);
        var spec1 = CreateTestSpec();
        var spec2 = CreateTestSpec();
        _mockRepo.Setup(r => r.GetByIdAsync(spec1.Id)).ReturnsAsync(spec1);
        _mockRepo.Setup(r => r.GetByIdAsync(spec2.Id)).ReturnsAsync(spec2);

        // Act
        var result1 = await _sut.UnlockSpecializationAsync(character, spec1.Id);
        var result2 = await _sut.UnlockSpecializationAsync(character, spec2.Id);

        // Assert
        result1.Success.Should().BeTrue();
        result2.Success.Should().BeTrue();
        character.UnlockedSpecializationIds.Should().HaveCount(2);
        character.ProgressionPoints.Should().Be(10); // 30 - 10 - 10 = 10
    }

    #endregion

    #region UnlockSpecializationAsync - Failure Tests

    [Fact]
    public async Task UnlockSpecializationAsync_Fails_WhenSpecNotFound()
    {
        // Arrange
        var character = CreateTestCharacter();
        var missingId = Guid.NewGuid();
        _mockRepo.Setup(r => r.GetByIdAsync(missingId)).ReturnsAsync((Specialization?)null);

        // Act
        var result = await _sut.UnlockSpecializationAsync(character, missingId);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
    }

    [Fact]
    public async Task UnlockSpecializationAsync_Fails_WhenAlreadyUnlocked()
    {
        // Arrange
        var spec = CreateTestSpec();
        var character = CreateTestCharacter();
        character.UnlockedSpecializationIds.Add(spec.Id);
        _mockRepo.Setup(r => r.GetByIdAsync(spec.Id)).ReturnsAsync(spec);

        // Act
        var result = await _sut.UnlockSpecializationAsync(character, spec.Id);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("already unlocked");
    }

    [Fact]
    public async Task UnlockSpecializationAsync_Fails_WhenWrongArchetype()
    {
        // Arrange
        var character = CreateTestCharacter(archetype: ArchetypeType.Warrior);
        var spec = CreateTestSpec(archetype: ArchetypeType.Adept);
        _mockRepo.Setup(r => r.GetByIdAsync(spec.Id)).ReturnsAsync(spec);

        // Act
        var result = await _sut.UnlockSpecializationAsync(character, spec.Id);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Adept");
    }

    [Fact]
    public async Task UnlockSpecializationAsync_Fails_WhenLevelTooLow()
    {
        // Arrange
        var character = CreateTestCharacter(level: 3);
        var spec = CreateTestSpec(requiredLevel: 5);
        _mockRepo.Setup(r => r.GetByIdAsync(spec.Id)).ReturnsAsync(spec);

        // Act
        var result = await _sut.UnlockSpecializationAsync(character, spec.Id);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Level 5");
    }

    [Fact]
    public async Task UnlockSpecializationAsync_Fails_WhenInsufficientPP()
    {
        // Arrange
        var character = CreateTestCharacter(pp: 9);
        var spec = CreateTestSpec();
        _mockRepo.Setup(r => r.GetByIdAsync(spec.Id)).ReturnsAsync(spec);

        // Act
        var result = await _sut.UnlockSpecializationAsync(character, spec.Id);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Insufficient PP");
    }

    [Fact]
    public async Task UnlockSpecializationAsync_NoStateChange_OnFailure()
    {
        // Arrange
        var character = CreateTestCharacter(pp: 15);
        var initialPP = character.ProgressionPoints;
        var spec = CreateTestSpec(archetype: ArchetypeType.Adept); // Wrong archetype
        _mockRepo.Setup(r => r.GetByIdAsync(spec.Id)).ReturnsAsync(spec);

        // Act
        await _sut.UnlockSpecializationAsync(character, spec.Id);

        // Assert
        character.ProgressionPoints.Should().Be(initialPP);
        character.UnlockedSpecializationIds.Should().BeEmpty();
    }

    [Fact]
    public async Task UnlockSpecializationAsync_NoEvent_OnFailure()
    {
        // Arrange
        var character = CreateTestCharacter(pp: 5); // Not enough PP
        var spec = CreateTestSpec();
        _mockRepo.Setup(r => r.GetByIdAsync(spec.Id)).ReturnsAsync(spec);

        // Act
        await _sut.UnlockSpecializationAsync(character, spec.Id);

        // Assert
        _mockEventBus.Verify(
            e => e.Publish(It.IsAny<SpecializationUnlockedEvent>()),
            Times.Never);
    }

    #endregion

    // ═══════════════════════════════════════════════════════════════════════
    // CanUnlockSpecializationAsync Tests (6 tests)
    // ═══════════════════════════════════════════════════════════════════════

    #region CanUnlockSpecializationAsync Tests

    [Fact]
    public async Task CanUnlockSpecializationAsync_ReturnsTrue_WhenAllRequirementsMet()
    {
        // Arrange
        var character = CreateTestCharacter(pp: 15, level: 5);
        var spec = CreateTestSpec(requiredLevel: 3);
        _mockRepo.Setup(r => r.GetByIdAsync(spec.Id)).ReturnsAsync(spec);

        // Act
        var result = await _sut.CanUnlockSpecializationAsync(character, spec.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CanUnlockSpecializationAsync_ReturnsFalse_WhenSpecNotFound()
    {
        // Arrange
        var character = CreateTestCharacter();
        var missingId = Guid.NewGuid();
        _mockRepo.Setup(r => r.GetByIdAsync(missingId)).ReturnsAsync((Specialization?)null);

        // Act
        var result = await _sut.CanUnlockSpecializationAsync(character, missingId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CanUnlockSpecializationAsync_ReturnsFalse_WhenAlreadyUnlocked()
    {
        // Arrange
        var spec = CreateTestSpec();
        var character = CreateTestCharacter();
        character.UnlockedSpecializationIds.Add(spec.Id);
        _mockRepo.Setup(r => r.GetByIdAsync(spec.Id)).ReturnsAsync(spec);

        // Act
        var result = await _sut.CanUnlockSpecializationAsync(character, spec.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CanUnlockSpecializationAsync_ReturnsFalse_WhenWrongArchetype()
    {
        // Arrange
        var character = CreateTestCharacter(archetype: ArchetypeType.Warrior);
        var spec = CreateTestSpec(archetype: ArchetypeType.Adept);
        _mockRepo.Setup(r => r.GetByIdAsync(spec.Id)).ReturnsAsync(spec);

        // Act
        var result = await _sut.CanUnlockSpecializationAsync(character, spec.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CanUnlockSpecializationAsync_ReturnsFalse_WhenLevelTooLow()
    {
        // Arrange
        var character = CreateTestCharacter(level: 2);
        var spec = CreateTestSpec(requiredLevel: 5);
        _mockRepo.Setup(r => r.GetByIdAsync(spec.Id)).ReturnsAsync(spec);

        // Act
        var result = await _sut.CanUnlockSpecializationAsync(character, spec.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CanUnlockSpecializationAsync_ReturnsFalse_WhenInsufficientPP()
    {
        // Arrange
        var character = CreateTestCharacter(pp: 5);
        var spec = CreateTestSpec();
        _mockRepo.Setup(r => r.GetByIdAsync(spec.Id)).ReturnsAsync(spec);

        // Act
        var result = await _sut.CanUnlockSpecializationAsync(character, spec.Id);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    // ═══════════════════════════════════════════════════════════════════════
    // UnlockNodeAsync Tests (18 tests)
    // ═══════════════════════════════════════════════════════════════════════

    #region UnlockNodeAsync - Success Tests

    [Fact]
    public async Task UnlockNodeAsync_DeductsPP_FromCharacter()
    {
        // Arrange
        var spec = CreateTestSpec();
        var node = CreateTestNode(spec.Id, tier: 2, costPp: 2);
        var character = CreateTestCharacter(pp: 10);
        character.UnlockedSpecializationIds.Add(spec.Id);

        _mockRepo.Setup(r => r.GetNodeByIdAsync(node.Id)).ReturnsAsync(node);
        _mockRepo.Setup(r => r.GetNodesForSpecializationAsync(spec.Id))
            .ReturnsAsync(new List<SpecializationNode> { node });

        // Act
        await _sut.UnlockNodeAsync(character, node.Id);

        // Assert
        character.ProgressionPoints.Should().Be(8); // 10 - 2 = 8
    }

    [Fact]
    public async Task UnlockNodeAsync_RecordsUnlock_InRepository()
    {
        // Arrange
        var spec = CreateTestSpec();
        var node = CreateTestNode(spec.Id);
        var character = CreateTestCharacter();
        character.UnlockedSpecializationIds.Add(spec.Id);

        _mockRepo.Setup(r => r.GetNodeByIdAsync(node.Id)).ReturnsAsync(node);

        // Act
        await _sut.UnlockNodeAsync(character, node.Id);

        // Assert
        _mockRepo.Verify(r => r.RecordNodeUnlockAsync(character.Id, node.Id), Times.Once);
    }

    [Fact]
    public async Task UnlockNodeAsync_CallsSaveChanges()
    {
        // Arrange
        var spec = CreateTestSpec();
        var node = CreateTestNode(spec.Id);
        var character = CreateTestCharacter();
        character.UnlockedSpecializationIds.Add(spec.Id);

        _mockRepo.Setup(r => r.GetNodeByIdAsync(node.Id)).ReturnsAsync(node);

        // Act
        await _sut.UnlockNodeAsync(character, node.Id);

        // Assert
        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UnlockNodeAsync_AddsToSpecializationProgress()
    {
        // Arrange
        var spec = CreateTestSpec();
        var node = CreateTestNode(spec.Id);
        var character = CreateTestCharacter();
        character.UnlockedSpecializationIds.Add(spec.Id);

        _mockRepo.Setup(r => r.GetNodeByIdAsync(node.Id)).ReturnsAsync(node);

        // Act
        await _sut.UnlockNodeAsync(character, node.Id);

        // Assert
        character.SpecializationProgress.Should().ContainSingle();
        character.SpecializationProgress.First().NodeId.Should().Be(node.Id);
    }

    [Fact]
    public async Task UnlockNodeAsync_UpdatesLastModified()
    {
        // Arrange
        var spec = CreateTestSpec();
        var node = CreateTestNode(spec.Id);
        var character = CreateTestCharacter();
        character.UnlockedSpecializationIds.Add(spec.Id);
        var originalTime = character.LastModified;

        _mockRepo.Setup(r => r.GetNodeByIdAsync(node.Id)).ReturnsAsync(node);

        // Act
        await Task.Delay(10);
        await _sut.UnlockNodeAsync(character, node.Id);

        // Assert
        character.LastModified.Should().BeAfter(originalTime);
    }

    [Fact]
    public async Task UnlockNodeAsync_ReturnsSuccess_WithCorrectDetails()
    {
        // Arrange
        var spec = CreateTestSpec();
        var node = CreateTestNode(spec.Id, tier: 2, costPp: 2);
        var character = CreateTestCharacter();
        character.UnlockedSpecializationIds.Add(spec.Id);

        _mockRepo.Setup(r => r.GetNodeByIdAsync(node.Id)).ReturnsAsync(node);

        // Act
        var result = await _sut.UnlockNodeAsync(character, node.Id);

        // Assert
        result.Success.Should().BeTrue();
        result.NodeId.Should().Be(node.Id);
        result.NodeName.Should().Be(node.GetDisplayName());
        result.AbilityId.Should().Be(node.AbilityId);
        result.Tier.Should().Be(2);
        result.PpSpent.Should().Be(2);
    }

    [Fact]
    public async Task UnlockNodeAsync_PublishesEvent_OnSuccess()
    {
        // Arrange
        var spec = CreateTestSpec();
        var node = CreateTestNode(spec.Id);
        var character = CreateTestCharacter();
        character.UnlockedSpecializationIds.Add(spec.Id);

        _mockRepo.Setup(r => r.GetNodeByIdAsync(node.Id)).ReturnsAsync(node);

        // Act
        await _sut.UnlockNodeAsync(character, node.Id);

        // Assert
        _mockEventBus.Verify(
            e => e.Publish(It.IsAny<NodeUnlockedEvent>()),
            Times.Once);
    }

    [Fact]
    public async Task UnlockNodeAsync_EventContainsCorrectData()
    {
        // Arrange
        var spec = CreateTestSpec();
        var node = CreateTestNode(spec.Id, tier: 3, costPp: 3);
        var character = CreateTestCharacter();
        character.UnlockedSpecializationIds.Add(spec.Id);

        _mockRepo.Setup(r => r.GetNodeByIdAsync(node.Id)).ReturnsAsync(node);

        // Act
        await _sut.UnlockNodeAsync(character, node.Id);

        // Assert
        _capturedNodeEvent.Should().NotBeNull();
        _capturedNodeEvent!.CharacterId.Should().Be(character.Id);
        _capturedNodeEvent.CharacterName.Should().Be(character.Name);
        _capturedNodeEvent.NodeId.Should().Be(node.Id);
        _capturedNodeEvent.NodeName.Should().Be(node.GetDisplayName());
        _capturedNodeEvent.AbilityId.Should().Be(node.AbilityId);
        _capturedNodeEvent.Tier.Should().Be(3);
        _capturedNodeEvent.IsCapstone.Should().BeFalse();
        _capturedNodeEvent.ProgressionPointsSpent.Should().Be(3);
    }

    [Fact]
    public async Task UnlockNodeAsync_Tier1Node_NoPrerequisites()
    {
        // Arrange
        var spec = CreateTestSpec();
        var node = CreateTestNode(spec.Id, tier: 1, costPp: 1);
        var character = CreateTestCharacter();
        character.UnlockedSpecializationIds.Add(spec.Id);

        _mockRepo.Setup(r => r.GetNodeByIdAsync(node.Id)).ReturnsAsync(node);

        // Act
        var result = await _sut.UnlockNodeAsync(character, node.Id);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task UnlockNodeAsync_Tier2Node_RequiresParent()
    {
        // Arrange
        var spec = CreateTestSpec();
        var parentNode = CreateTestNode(spec.Id, tier: 1);
        var childNode = CreateTestNode(spec.Id, tier: 2, costPp: 2,
            parentNodeIds: new List<Guid> { parentNode.Id });

        var character = CreateTestCharacter();
        character.UnlockedSpecializationIds.Add(spec.Id);
        character.SpecializationProgress.Add(new CharacterSpecializationProgress
        {
            CharacterId = character.Id,
            NodeId = parentNode.Id
        });

        _mockRepo.Setup(r => r.GetNodeByIdAsync(childNode.Id)).ReturnsAsync(childNode);
        _mockRepo.Setup(r => r.GetNodeByIdAsync(parentNode.Id)).ReturnsAsync(parentNode);

        // Act
        var result = await _sut.UnlockNodeAsync(character, childNode.Id);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task UnlockNodeAsync_VariableCosts_WorkCorrectly()
    {
        // Arrange
        var spec = CreateTestSpec();
        var tier1 = CreateTestNode(spec.Id, tier: 1, costPp: 1);
        var tier2 = CreateTestNode(spec.Id, tier: 2, costPp: 2);
        var tier3 = CreateTestNode(spec.Id, tier: 3, costPp: 3);

        var character = CreateTestCharacter(pp: 10);
        character.UnlockedSpecializationIds.Add(spec.Id);

        _mockRepo.Setup(r => r.GetNodeByIdAsync(tier1.Id)).ReturnsAsync(tier1);
        _mockRepo.Setup(r => r.GetNodeByIdAsync(tier2.Id)).ReturnsAsync(tier2);
        _mockRepo.Setup(r => r.GetNodeByIdAsync(tier3.Id)).ReturnsAsync(tier3);

        // Act
        await _sut.UnlockNodeAsync(character, tier1.Id);
        await _sut.UnlockNodeAsync(character, tier2.Id);
        await _sut.UnlockNodeAsync(character, tier3.Id);

        // Assert
        character.ProgressionPoints.Should().Be(4); // 10 - 1 - 2 - 3 = 4
    }

    #endregion

    #region UnlockNodeAsync - Failure Tests

    [Fact]
    public async Task UnlockNodeAsync_Fails_WhenNodeNotFound()
    {
        // Arrange
        var character = CreateTestCharacter();
        var missingId = Guid.NewGuid();
        _mockRepo.Setup(r => r.GetNodeByIdAsync(missingId)).ReturnsAsync((SpecializationNode?)null);

        // Act
        var result = await _sut.UnlockNodeAsync(character, missingId);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
    }

    [Fact]
    public async Task UnlockNodeAsync_Fails_WhenAlreadyUnlocked()
    {
        // Arrange
        var spec = CreateTestSpec();
        var node = CreateTestNode(spec.Id);
        var character = CreateTestCharacter();
        character.UnlockedSpecializationIds.Add(spec.Id);
        character.SpecializationProgress.Add(new CharacterSpecializationProgress
        {
            CharacterId = character.Id,
            NodeId = node.Id
        });

        _mockRepo.Setup(r => r.GetNodeByIdAsync(node.Id)).ReturnsAsync(node);

        // Act
        var result = await _sut.UnlockNodeAsync(character, node.Id);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("already unlocked");
    }

    [Fact]
    public async Task UnlockNodeAsync_Fails_WhenSpecNotUnlocked()
    {
        // Arrange
        var spec = CreateTestSpec();
        var node = CreateTestNode(spec.Id);
        var character = CreateTestCharacter();
        // NOT adding spec to UnlockedSpecializationIds

        _mockRepo.Setup(r => r.GetNodeByIdAsync(node.Id)).ReturnsAsync(node);

        // Act
        var result = await _sut.UnlockNodeAsync(character, node.Id);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("specialization first");
    }

    [Fact]
    public async Task UnlockNodeAsync_Fails_WhenParentNotUnlocked()
    {
        // Arrange
        var spec = CreateTestSpec();
        var parentNode = CreateTestNode(spec.Id, tier: 1);
        var childNode = CreateTestNode(spec.Id, tier: 2, costPp: 2,
            parentNodeIds: new List<Guid> { parentNode.Id });

        var character = CreateTestCharacter();
        character.UnlockedSpecializationIds.Add(spec.Id);
        // NOT unlocking parent node

        _mockRepo.Setup(r => r.GetNodeByIdAsync(childNode.Id)).ReturnsAsync(childNode);
        _mockRepo.Setup(r => r.GetNodeByIdAsync(parentNode.Id)).ReturnsAsync(parentNode);

        // Act
        var result = await _sut.UnlockNodeAsync(character, childNode.Id);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Requires");
    }

    [Fact]
    public async Task UnlockNodeAsync_Fails_WhenInsufficientPP()
    {
        // Arrange
        var spec = CreateTestSpec();
        var node = CreateTestNode(spec.Id, tier: 3, costPp: 3);
        var character = CreateTestCharacter(pp: 2);
        character.UnlockedSpecializationIds.Add(spec.Id);

        _mockRepo.Setup(r => r.GetNodeByIdAsync(node.Id)).ReturnsAsync(node);

        // Act
        var result = await _sut.UnlockNodeAsync(character, node.Id);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Insufficient PP");
    }

    [Fact]
    public async Task UnlockNodeAsync_NoStateChange_OnFailure()
    {
        // Arrange
        var spec = CreateTestSpec();
        var node = CreateTestNode(spec.Id, tier: 3, costPp: 3);
        var character = CreateTestCharacter(pp: 2); // Not enough PP
        character.UnlockedSpecializationIds.Add(spec.Id);
        var initialPP = character.ProgressionPoints;

        _mockRepo.Setup(r => r.GetNodeByIdAsync(node.Id)).ReturnsAsync(node);

        // Act
        await _sut.UnlockNodeAsync(character, node.Id);

        // Assert
        character.ProgressionPoints.Should().Be(initialPP);
        character.SpecializationProgress.Should().BeEmpty();
    }

    [Fact]
    public async Task UnlockNodeAsync_NoEvent_OnFailure()
    {
        // Arrange
        var spec = CreateTestSpec();
        var node = CreateTestNode(spec.Id);
        var character = CreateTestCharacter();
        // NOT adding spec to UnlockedSpecializationIds - will fail

        _mockRepo.Setup(r => r.GetNodeByIdAsync(node.Id)).ReturnsAsync(node);

        // Act
        await _sut.UnlockNodeAsync(character, node.Id);

        // Assert
        _mockEventBus.Verify(
            e => e.Publish(It.IsAny<NodeUnlockedEvent>()),
            Times.Never);
    }

    #endregion

    // ═══════════════════════════════════════════════════════════════════════
    // ValidatePrerequisitesAsync Tests (12 tests)
    // ═══════════════════════════════════════════════════════════════════════

    #region ValidatePrerequisitesAsync Tests

    [Fact]
    public async Task ValidatePrerequisitesAsync_Tier1_ReturnsTrue_NoParents()
    {
        // Arrange
        var spec = CreateTestSpec();
        var node = CreateTestNode(spec.Id, tier: 1);
        var character = CreateTestCharacter();

        // Act
        var (isValid, _) = await _sut.ValidatePrerequisitesAsync(character, node);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidatePrerequisitesAsync_Tier2_ReturnsTrue_WhenParentUnlocked()
    {
        // Arrange
        var spec = CreateTestSpec();
        var parentNode = CreateTestNode(spec.Id, tier: 1);
        var childNode = CreateTestNode(spec.Id, tier: 2,
            parentNodeIds: new List<Guid> { parentNode.Id });

        var character = CreateTestCharacter();
        character.SpecializationProgress.Add(new CharacterSpecializationProgress
        {
            CharacterId = character.Id,
            NodeId = parentNode.Id
        });

        // Act
        var (isValid, _) = await _sut.ValidatePrerequisitesAsync(character, childNode);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidatePrerequisitesAsync_Tier2_ReturnsFalse_WhenParentMissing()
    {
        // Arrange
        var spec = CreateTestSpec();
        var parentNode = CreateTestNode(spec.Id, tier: 1);
        var childNode = CreateTestNode(spec.Id, tier: 2,
            parentNodeIds: new List<Guid> { parentNode.Id });

        var character = CreateTestCharacter();
        // NOT unlocking parent

        _mockRepo.Setup(r => r.GetNodeByIdAsync(parentNode.Id)).ReturnsAsync(parentNode);

        // Act
        var (isValid, reason) = await _sut.ValidatePrerequisitesAsync(character, childNode);

        // Assert
        isValid.Should().BeFalse();
        reason.Should().Contain("Requires");
    }

    [Fact]
    public async Task ValidatePrerequisitesAsync_Tier3_RequiresAllParents()
    {
        // Arrange
        var spec = CreateTestSpec();
        var parent1 = CreateTestNode(spec.Id, tier: 2);
        var parent2 = CreateTestNode(spec.Id, tier: 2);
        var childNode = CreateTestNode(spec.Id, tier: 3,
            parentNodeIds: new List<Guid> { parent1.Id, parent2.Id });

        var character = CreateTestCharacter();
        character.SpecializationProgress.Add(new CharacterSpecializationProgress
        {
            CharacterId = character.Id,
            NodeId = parent1.Id
        });
        character.SpecializationProgress.Add(new CharacterSpecializationProgress
        {
            CharacterId = character.Id,
            NodeId = parent2.Id
        });

        // Act
        var (isValid, _) = await _sut.ValidatePrerequisitesAsync(character, childNode);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidatePrerequisitesAsync_Tier3_ReturnsFalse_WhenAnyParentMissing()
    {
        // Arrange
        var spec = CreateTestSpec();
        var parent1 = CreateTestNode(spec.Id, tier: 2);
        var parent2 = CreateTestNode(spec.Id, tier: 2);
        var childNode = CreateTestNode(spec.Id, tier: 3,
            parentNodeIds: new List<Guid> { parent1.Id, parent2.Id });

        var character = CreateTestCharacter();
        // Only unlock one parent
        character.SpecializationProgress.Add(new CharacterSpecializationProgress
        {
            CharacterId = character.Id,
            NodeId = parent1.Id
        });

        _mockRepo.Setup(r => r.GetNodeByIdAsync(parent2.Id)).ReturnsAsync(parent2);

        // Act
        var (isValid, _) = await _sut.ValidatePrerequisitesAsync(character, childNode);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public async Task ValidatePrerequisitesAsync_Capstone_RequiresAllTier3Nodes()
    {
        // Arrange
        var spec = CreateTestSpec();
        var tier3A = CreateTestNode(spec.Id, tier: 3);
        var tier3B = CreateTestNode(spec.Id, tier: 3);
        var capstone = CreateTestNode(spec.Id, tier: 4, costPp: 5);

        var character = CreateTestCharacter();
        character.SpecializationProgress.Add(new CharacterSpecializationProgress
        {
            CharacterId = character.Id,
            NodeId = tier3A.Id
        });
        character.SpecializationProgress.Add(new CharacterSpecializationProgress
        {
            CharacterId = character.Id,
            NodeId = tier3B.Id
        });

        _mockRepo.Setup(r => r.GetNodesForSpecializationAsync(spec.Id))
            .ReturnsAsync(new List<SpecializationNode> { tier3A, tier3B, capstone });

        // Act
        var (isValid, _) = await _sut.ValidatePrerequisitesAsync(character, capstone);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidatePrerequisitesAsync_Capstone_ReturnsFalse_WhenAnyTier3Missing()
    {
        // Arrange
        var spec = CreateTestSpec();
        var tier3A = CreateTestNode(spec.Id, tier: 3);
        var tier3B = CreateTestNode(spec.Id, tier: 3);
        var capstone = CreateTestNode(spec.Id, tier: 4, costPp: 5);

        var character = CreateTestCharacter();
        // Only unlock one Tier 3
        character.SpecializationProgress.Add(new CharacterSpecializationProgress
        {
            CharacterId = character.Id,
            NodeId = tier3A.Id
        });

        _mockRepo.Setup(r => r.GetNodesForSpecializationAsync(spec.Id))
            .ReturnsAsync(new List<SpecializationNode> { tier3A, tier3B, capstone });

        // Act
        var (isValid, reason) = await _sut.ValidatePrerequisitesAsync(character, capstone);

        // Assert
        isValid.Should().BeFalse();
        reason.Should().Contain("Capstone");
    }

    [Fact]
    public async Task ValidatePrerequisitesAsync_Capstone_ReturnsTrue_WhenAllTier3Unlocked()
    {
        // Arrange
        var spec = CreateTestSpec();
        var tier3A = CreateTestNode(spec.Id, tier: 3);
        var tier3B = CreateTestNode(spec.Id, tier: 3);
        var tier3C = CreateTestNode(spec.Id, tier: 3);
        var capstone = CreateTestNode(spec.Id, tier: 4, costPp: 5);

        var character = CreateTestCharacter();
        character.SpecializationProgress.Add(new CharacterSpecializationProgress { CharacterId = character.Id, NodeId = tier3A.Id });
        character.SpecializationProgress.Add(new CharacterSpecializationProgress { CharacterId = character.Id, NodeId = tier3B.Id });
        character.SpecializationProgress.Add(new CharacterSpecializationProgress { CharacterId = character.Id, NodeId = tier3C.Id });

        _mockRepo.Setup(r => r.GetNodesForSpecializationAsync(spec.Id))
            .ReturnsAsync(new List<SpecializationNode> { tier3A, tier3B, tier3C, capstone });

        // Act
        var (isValid, _) = await _sut.ValidatePrerequisitesAsync(character, capstone);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidatePrerequisitesAsync_Capstone_IgnoresOtherTiers()
    {
        // Arrange
        var spec = CreateTestSpec();
        var tier1 = CreateTestNode(spec.Id, tier: 1);
        var tier2 = CreateTestNode(spec.Id, tier: 2);
        var tier3 = CreateTestNode(spec.Id, tier: 3);
        var capstone = CreateTestNode(spec.Id, tier: 4, costPp: 5);

        var character = CreateTestCharacter();
        // Only unlock Tier 3 (not Tier 1 or 2)
        character.SpecializationProgress.Add(new CharacterSpecializationProgress
        {
            CharacterId = character.Id,
            NodeId = tier3.Id
        });

        _mockRepo.Setup(r => r.GetNodesForSpecializationAsync(spec.Id))
            .ReturnsAsync(new List<SpecializationNode> { tier1, tier2, tier3, capstone });

        // Act
        var (isValid, _) = await _sut.ValidatePrerequisitesAsync(character, capstone);

        // Assert - Should pass because only Tier 3 is checked for capstone
        isValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidatePrerequisitesAsync_ReturnsCorrectFailureReason()
    {
        // Arrange
        var spec = CreateTestSpec();
        var parentNode = CreateTestNode(spec.Id, tier: 1);
        parentNode.DisplayName = "Iron Stance";
        var childNode = CreateTestNode(spec.Id, tier: 2,
            parentNodeIds: new List<Guid> { parentNode.Id });

        var character = CreateTestCharacter();
        _mockRepo.Setup(r => r.GetNodeByIdAsync(parentNode.Id)).ReturnsAsync(parentNode);

        // Act
        var (_, reason) = await _sut.ValidatePrerequisitesAsync(character, childNode);

        // Assert
        reason.Should().Contain("Iron Stance");
    }

    [Fact]
    public async Task ValidatePrerequisitesAsync_EmptyParentList_ReturnsTrue()
    {
        // Arrange
        var spec = CreateTestSpec();
        var node = CreateTestNode(spec.Id, tier: 1);
        node.ParentNodeIds = new List<Guid>(); // Explicitly empty
        var character = CreateTestCharacter();

        // Act
        var (isValid, _) = await _sut.ValidatePrerequisitesAsync(character, node);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidatePrerequisitesAsync_MultipleParents_AllRequired()
    {
        // Arrange
        var spec = CreateTestSpec();
        var parent1 = CreateTestNode(spec.Id, tier: 1);
        var parent2 = CreateTestNode(spec.Id, tier: 1);
        var parent3 = CreateTestNode(spec.Id, tier: 1);
        var childNode = CreateTestNode(spec.Id, tier: 2,
            parentNodeIds: new List<Guid> { parent1.Id, parent2.Id, parent3.Id });

        var character = CreateTestCharacter();
        character.SpecializationProgress.Add(new CharacterSpecializationProgress { CharacterId = character.Id, NodeId = parent1.Id });
        character.SpecializationProgress.Add(new CharacterSpecializationProgress { CharacterId = character.Id, NodeId = parent2.Id });
        character.SpecializationProgress.Add(new CharacterSpecializationProgress { CharacterId = character.Id, NodeId = parent3.Id });

        // Act
        var (isValid, _) = await _sut.ValidatePrerequisitesAsync(character, childNode);

        // Assert
        isValid.Should().BeTrue();
    }

    #endregion

    // ═══════════════════════════════════════════════════════════════════════
    // CanUnlockNodeAsync Tests (6 tests)
    // ═══════════════════════════════════════════════════════════════════════

    #region CanUnlockNodeAsync Tests

    [Fact]
    public async Task CanUnlockNodeAsync_ReturnsTrue_WhenAllRequirementsMet()
    {
        // Arrange
        var spec = CreateTestSpec();
        var node = CreateTestNode(spec.Id, tier: 1, costPp: 1);
        var character = CreateTestCharacter(pp: 10);
        character.UnlockedSpecializationIds.Add(spec.Id);

        _mockRepo.Setup(r => r.GetNodeByIdAsync(node.Id)).ReturnsAsync(node);

        // Act
        var result = await _sut.CanUnlockNodeAsync(character, node.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CanUnlockNodeAsync_ReturnsFalse_WhenNodeNotFound()
    {
        // Arrange
        var character = CreateTestCharacter();
        var missingId = Guid.NewGuid();
        _mockRepo.Setup(r => r.GetNodeByIdAsync(missingId)).ReturnsAsync((SpecializationNode?)null);

        // Act
        var result = await _sut.CanUnlockNodeAsync(character, missingId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CanUnlockNodeAsync_ReturnsFalse_WhenAlreadyUnlocked()
    {
        // Arrange
        var spec = CreateTestSpec();
        var node = CreateTestNode(spec.Id);
        var character = CreateTestCharacter();
        character.UnlockedSpecializationIds.Add(spec.Id);
        character.SpecializationProgress.Add(new CharacterSpecializationProgress
        {
            CharacterId = character.Id,
            NodeId = node.Id
        });

        _mockRepo.Setup(r => r.GetNodeByIdAsync(node.Id)).ReturnsAsync(node);

        // Act
        var result = await _sut.CanUnlockNodeAsync(character, node.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CanUnlockNodeAsync_ReturnsFalse_WhenSpecNotUnlocked()
    {
        // Arrange
        var spec = CreateTestSpec();
        var node = CreateTestNode(spec.Id);
        var character = CreateTestCharacter();
        // NOT adding spec

        _mockRepo.Setup(r => r.GetNodeByIdAsync(node.Id)).ReturnsAsync(node);

        // Act
        var result = await _sut.CanUnlockNodeAsync(character, node.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CanUnlockNodeAsync_ReturnsFalse_WhenPrereqsFail()
    {
        // Arrange
        var spec = CreateTestSpec();
        var parentNode = CreateTestNode(spec.Id, tier: 1);
        var childNode = CreateTestNode(spec.Id, tier: 2,
            parentNodeIds: new List<Guid> { parentNode.Id });

        var character = CreateTestCharacter();
        character.UnlockedSpecializationIds.Add(spec.Id);
        // NOT unlocking parent

        _mockRepo.Setup(r => r.GetNodeByIdAsync(childNode.Id)).ReturnsAsync(childNode);
        _mockRepo.Setup(r => r.GetNodeByIdAsync(parentNode.Id)).ReturnsAsync(parentNode);

        // Act
        var result = await _sut.CanUnlockNodeAsync(character, childNode.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CanUnlockNodeAsync_ReturnsFalse_WhenInsufficientPP()
    {
        // Arrange
        var spec = CreateTestSpec();
        var node = CreateTestNode(spec.Id, tier: 3, costPp: 3);
        var character = CreateTestCharacter(pp: 2);
        character.UnlockedSpecializationIds.Add(spec.Id);

        _mockRepo.Setup(r => r.GetNodeByIdAsync(node.Id)).ReturnsAsync(node);

        // Act
        var result = await _sut.CanUnlockNodeAsync(character, node.Id);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    // ═══════════════════════════════════════════════════════════════════════
    // Query Operation Tests (4 tests)
    // ═══════════════════════════════════════════════════════════════════════

    #region Query Operation Tests

    [Fact]
    public async Task GetAvailableSpecializationsAsync_ReturnsMatchingArchetype()
    {
        // Arrange
        var character = CreateTestCharacter(archetype: ArchetypeType.Warrior);
        var spec1 = CreateTestSpec(archetype: ArchetypeType.Warrior);
        var spec2 = CreateTestSpec(archetype: ArchetypeType.Warrior);

        _mockRepo.Setup(r => r.GetByArchetypeAsync(ArchetypeType.Warrior))
            .ReturnsAsync(new List<Specialization> { spec1, spec2 });

        // Act
        var result = await _sut.GetAvailableSpecializationsAsync(character);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAvailableSpecializationsAsync_ReturnsEmpty_WhenNoMatch()
    {
        // Arrange
        var character = CreateTestCharacter(archetype: ArchetypeType.Ranger);
        _mockRepo.Setup(r => r.GetByArchetypeAsync(ArchetypeType.Ranger))
            .ReturnsAsync(new List<Specialization>());

        // Act
        var result = await _sut.GetAvailableSpecializationsAsync(character);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetNodesWithStatusAsync_ReturnsAllNodes()
    {
        // Arrange
        var spec = CreateTestSpec();
        var node1 = CreateTestNode(spec.Id, tier: 1);
        var node2 = CreateTestNode(spec.Id, tier: 2);
        var node3 = CreateTestNode(spec.Id, tier: 3);
        var character = CreateTestCharacter();

        _mockRepo.Setup(r => r.GetNodesForSpecializationAsync(spec.Id))
            .ReturnsAsync(new List<SpecializationNode> { node1, node2, node3 });

        // Act
        var result = await _sut.GetNodesWithStatusAsync(character, spec.Id);

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public void GetSpecializationUnlockCost_Returns10()
    {
        // Act
        var cost = _sut.GetSpecializationUnlockCost();

        // Assert
        cost.Should().Be(10);
    }

    #endregion

    // ═══════════════════════════════════════════════════════════════════════
    // Edge Case / Integration Tests (6 tests)
    // ═══════════════════════════════════════════════════════════════════════

    #region Edge Case / Integration Tests

    [Fact]
    public async Task FullTree_CanUnlockFromTier1ToCapstone()
    {
        // Arrange
        var spec = CreateTestSpec();
        var tier1 = CreateTestNode(spec.Id, tier: 1, costPp: 1);
        var tier2 = CreateTestNode(spec.Id, tier: 2, costPp: 2,
            parentNodeIds: new List<Guid> { tier1.Id });
        var tier3 = CreateTestNode(spec.Id, tier: 3, costPp: 3,
            parentNodeIds: new List<Guid> { tier2.Id });
        var capstone = CreateTestNode(spec.Id, tier: 4, costPp: 5);

        var character = CreateTestCharacter(pp: 50);
        character.UnlockedSpecializationIds.Add(spec.Id);

        _mockRepo.Setup(r => r.GetNodeByIdAsync(tier1.Id)).ReturnsAsync(tier1);
        _mockRepo.Setup(r => r.GetNodeByIdAsync(tier2.Id)).ReturnsAsync(tier2);
        _mockRepo.Setup(r => r.GetNodeByIdAsync(tier3.Id)).ReturnsAsync(tier3);
        _mockRepo.Setup(r => r.GetNodeByIdAsync(capstone.Id)).ReturnsAsync(capstone);
        _mockRepo.Setup(r => r.GetNodesForSpecializationAsync(spec.Id))
            .ReturnsAsync(new List<SpecializationNode> { tier1, tier2, tier3, capstone });

        // Act
        var result1 = await _sut.UnlockNodeAsync(character, tier1.Id);
        var result2 = await _sut.UnlockNodeAsync(character, tier2.Id);
        var result3 = await _sut.UnlockNodeAsync(character, tier3.Id);
        var resultCap = await _sut.UnlockNodeAsync(character, capstone.Id);

        // Assert
        result1.Success.Should().BeTrue();
        result2.Success.Should().BeTrue();
        result3.Success.Should().BeTrue();
        resultCap.Success.Should().BeTrue();
        character.ProgressionPoints.Should().Be(39); // 50 - 1 - 2 - 3 - 5 = 39
    }

    [Fact]
    public async Task FullTree_CannotSkipTiers()
    {
        // Arrange
        var spec = CreateTestSpec();
        var tier1 = CreateTestNode(spec.Id, tier: 1);
        var tier2 = CreateTestNode(spec.Id, tier: 2, costPp: 2,
            parentNodeIds: new List<Guid> { tier1.Id });

        var character = CreateTestCharacter(pp: 50);
        character.UnlockedSpecializationIds.Add(spec.Id);
        // NOT unlocking tier1

        _mockRepo.Setup(r => r.GetNodeByIdAsync(tier2.Id)).ReturnsAsync(tier2);
        _mockRepo.Setup(r => r.GetNodeByIdAsync(tier1.Id)).ReturnsAsync(tier1);

        // Act
        var result = await _sut.UnlockNodeAsync(character, tier2.Id);

        // Assert
        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task MultipleCharacters_IndependentProgress()
    {
        // Arrange
        var spec = CreateTestSpec();
        var node = CreateTestNode(spec.Id);
        var char1 = CreateTestCharacter();
        var char2 = CreateTestCharacter();
        char1.UnlockedSpecializationIds.Add(spec.Id);
        char2.UnlockedSpecializationIds.Add(spec.Id);

        _mockRepo.Setup(r => r.GetNodeByIdAsync(node.Id)).ReturnsAsync(node);

        // Act
        await _sut.UnlockNodeAsync(char1, node.Id);

        // Assert - char2 should not have the node
        char1.SpecializationProgress.Should().ContainSingle();
        char2.SpecializationProgress.Should().BeEmpty();
    }

    [Fact]
    public async Task UnlockNode_AfterSpecUnlock_Works()
    {
        // Arrange
        var spec = CreateTestSpec();
        var node = CreateTestNode(spec.Id);
        var character = CreateTestCharacter(pp: 20);

        _mockRepo.Setup(r => r.GetByIdAsync(spec.Id)).ReturnsAsync(spec);
        _mockRepo.Setup(r => r.GetNodeByIdAsync(node.Id)).ReturnsAsync(node);

        // Act
        var specResult = await _sut.UnlockSpecializationAsync(character, spec.Id);
        var nodeResult = await _sut.UnlockNodeAsync(character, node.Id);

        // Assert
        specResult.Success.Should().BeTrue();
        nodeResult.Success.Should().BeTrue();
    }

    [Fact]
    public async Task UnlockNode_BeforeSpecUnlock_Fails()
    {
        // Arrange
        var spec = CreateTestSpec();
        var node = CreateTestNode(spec.Id);
        var character = CreateTestCharacter(pp: 20);
        // NOT unlocking spec first

        _mockRepo.Setup(r => r.GetNodeByIdAsync(node.Id)).ReturnsAsync(node);

        // Act
        var result = await _sut.UnlockNodeAsync(character, node.Id);

        // Assert
        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task Capstone_RequiresAllTier3_NotJustParents()
    {
        // Arrange
        var spec = CreateTestSpec();
        var tier3A = CreateTestNode(spec.Id, tier: 3);
        var tier3B = CreateTestNode(spec.Id, tier: 3);
        // Capstone has ParentNodeIds only pointing to tier3A, but should require BOTH tier3 nodes
        var capstone = CreateTestNode(spec.Id, tier: 4, costPp: 5,
            parentNodeIds: new List<Guid> { tier3A.Id });

        var character = CreateTestCharacter(pp: 50);
        character.UnlockedSpecializationIds.Add(spec.Id);
        // Only unlock tier3A (the parent)
        character.SpecializationProgress.Add(new CharacterSpecializationProgress
        {
            CharacterId = character.Id,
            NodeId = tier3A.Id
        });

        _mockRepo.Setup(r => r.GetNodeByIdAsync(capstone.Id)).ReturnsAsync(capstone);
        _mockRepo.Setup(r => r.GetNodesForSpecializationAsync(spec.Id))
            .ReturnsAsync(new List<SpecializationNode> { tier3A, tier3B, capstone });

        // Act
        var result = await _sut.UnlockNodeAsync(character, capstone.Id);

        // Assert - Should fail because tier3B is not unlocked
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Capstone");
    }

    #endregion
}
