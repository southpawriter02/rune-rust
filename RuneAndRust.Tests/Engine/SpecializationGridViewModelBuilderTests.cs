using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.ViewModels;
using RuneAndRust.Engine.ViewModels;
using Xunit;

namespace RuneAndRust.Tests.Engine;

/// <summary>
/// Tests for SpecializationGridViewModelBuilder.
/// Validates ViewModel construction, status computation, and refresh behavior.
/// </summary>
/// <remarks>See: v0.4.1d (The Grid) for implementation.</remarks>
public class SpecializationGridViewModelBuilderTests
{
    private readonly ISpecializationRepository _specRepo;
    private readonly ISpecializationService _specService;
    private readonly ILogger<SpecializationGridViewModelBuilder> _logger;
    private readonly SpecializationGridViewModelBuilder _builder;

    public SpecializationGridViewModelBuilderTests()
    {
        _specRepo = Substitute.For<ISpecializationRepository>();
        _specService = Substitute.For<ISpecializationService>();
        _logger = Substitute.For<ILogger<SpecializationGridViewModelBuilder>>();
        _builder = new SpecializationGridViewModelBuilder(_specRepo, _specService, _logger);
    }

    private static Character CreateTestCharacter()
    {
        return new Character
        {
            Id = Guid.NewGuid(),
            Name = "TestChar",
            ProgressionPoints = 10,
            UnlockedSpecializationIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() },
            SpecializationProgress = new List<CharacterSpecializationProgress>()
        };
    }

    private static Specialization CreateTestSpecialization(Guid id)
    {
        return new Specialization
        {
            Id = id,
            Name = "Iron Warden",
            Description = "A defensive specialization"
        };
    }

    private static List<SpecializationNode> CreateTestNodes(Guid specId)
    {
        var abilityId = Guid.NewGuid();
        return new List<SpecializationNode>
        {
            new()
            {
                Id = Guid.NewGuid(),
                SpecializationId = specId,
                Tier = 1,
                CostPP = 1,
                PositionX = 0,
                PositionY = 0,
                AbilityId = abilityId,
                Ability = new ActiveAbility { Id = abilityId, Name = "Shield Bash", Description = "Bash with shield" },
                ParentNodeIds = new List<Guid>()
            },
            new()
            {
                Id = Guid.NewGuid(),
                SpecializationId = specId,
                Tier = 1,
                CostPP = 1,
                PositionX = 0,
                PositionY = 1,
                AbilityId = abilityId,
                Ability = new ActiveAbility { Id = abilityId, Name = "Shield Wall", Description = "Create a wall" },
                ParentNodeIds = new List<Guid>()
            },
            new()
            {
                Id = Guid.NewGuid(),
                SpecializationId = specId,
                Tier = 2,
                CostPP = 2,
                PositionX = 1,
                PositionY = 0,
                AbilityId = abilityId,
                Ability = new ActiveAbility { Id = abilityId, Name = "Iron Guard", Description = "Guard with iron" },
                ParentNodeIds = new List<Guid>()
            },
            new()
            {
                Id = Guid.NewGuid(),
                SpecializationId = specId,
                Tier = 4, // Tier 4 is automatically IsCapstone
                CostPP = 5,
                PositionX = 3,
                PositionY = 0,
                AbilityId = abilityId,
                Ability = new ActiveAbility { Id = abilityId, Name = "Unbreakable", Description = "Cannot be broken" },
                ParentNodeIds = new List<Guid>()
            }
        };
    }

    #region BuildAsync Tests

    [Fact]
    public async Task BuildAsync_ReturnsViewModelWithCorrectSpecName()
    {
        // Arrange
        var character = CreateTestCharacter();
        var specId = character.UnlockedSpecializationIds[0];
        var spec = CreateTestSpecialization(specId);
        var nodes = CreateTestNodes(specId);

        _specRepo.GetByIdAsync(specId).Returns(spec);
        _specRepo.GetNodesForSpecializationAsync(specId).Returns(nodes);
        _specService.ValidatePrerequisitesAsync(character, Arg.Any<SpecializationNode>())
            .Returns((true, ""));

        // Act
        var vm = await _builder.BuildAsync(character, specId);

        // Assert
        vm.SpecializationName.Should().Be("Iron Warden");
    }

    [Fact]
    public async Task BuildAsync_ReturnsViewModelWithCorrectCharacterName()
    {
        // Arrange
        var character = CreateTestCharacter();
        var specId = character.UnlockedSpecializationIds[0];
        var spec = CreateTestSpecialization(specId);
        var nodes = CreateTestNodes(specId);

        _specRepo.GetByIdAsync(specId).Returns(spec);
        _specRepo.GetNodesForSpecializationAsync(specId).Returns(nodes);
        _specService.ValidatePrerequisitesAsync(character, Arg.Any<SpecializationNode>())
            .Returns((true, ""));

        // Act
        var vm = await _builder.BuildAsync(character, specId);

        // Assert
        vm.CharacterName.Should().Be("TestChar");
    }

    [Fact]
    public async Task BuildAsync_ReturnsViewModelWithCorrectProgressionPoints()
    {
        // Arrange
        var character = CreateTestCharacter();
        var specId = character.UnlockedSpecializationIds[0];
        var spec = CreateTestSpecialization(specId);
        var nodes = CreateTestNodes(specId);

        _specRepo.GetByIdAsync(specId).Returns(spec);
        _specRepo.GetNodesForSpecializationAsync(specId).Returns(nodes);
        _specService.ValidatePrerequisitesAsync(character, Arg.Any<SpecializationNode>())
            .Returns((true, ""));

        // Act
        var vm = await _builder.BuildAsync(character, specId);

        // Assert
        vm.ProgressionPoints.Should().Be(10);
    }

    [Fact]
    public async Task BuildAsync_GroupsNodesByTier()
    {
        // Arrange
        var character = CreateTestCharacter();
        var specId = character.UnlockedSpecializationIds[0];
        var spec = CreateTestSpecialization(specId);
        var nodes = CreateTestNodes(specId);

        _specRepo.GetByIdAsync(specId).Returns(spec);
        _specRepo.GetNodesForSpecializationAsync(specId).Returns(nodes);
        _specService.ValidatePrerequisitesAsync(character, Arg.Any<SpecializationNode>())
            .Returns((true, ""));

        // Act
        var vm = await _builder.BuildAsync(character, specId);

        // Assert
        vm.NodesByTier.Should().ContainKey(1);
        vm.NodesByTier.Should().ContainKey(2);
        vm.NodesByTier.Should().ContainKey(4);
        vm.NodesByTier[1].Should().HaveCount(2);
        vm.NodesByTier[2].Should().HaveCount(1);
        vm.NodesByTier[4].Should().HaveCount(1);
    }

    [Fact]
    public async Task BuildAsync_SetsCorrectTotalNodeCount()
    {
        // Arrange
        var character = CreateTestCharacter();
        var specId = character.UnlockedSpecializationIds[0];
        var spec = CreateTestSpecialization(specId);
        var nodes = CreateTestNodes(specId);

        _specRepo.GetByIdAsync(specId).Returns(spec);
        _specRepo.GetNodesForSpecializationAsync(specId).Returns(nodes);
        _specService.ValidatePrerequisitesAsync(character, Arg.Any<SpecializationNode>())
            .Returns((true, ""));

        // Act
        var vm = await _builder.BuildAsync(character, specId);

        // Assert
        vm.TotalNodes.Should().Be(4);
        vm.AllNodes.Should().HaveCount(4);
    }

    [Fact]
    public async Task BuildAsync_SetsCorrectSpecIndexForMultiSpec()
    {
        // Arrange
        var character = CreateTestCharacter();
        var specId = character.UnlockedSpecializationIds[1]; // Second spec
        var spec = CreateTestSpecialization(specId);
        var nodes = CreateTestNodes(specId);

        _specRepo.GetByIdAsync(specId).Returns(spec);
        _specRepo.GetNodesForSpecializationAsync(specId).Returns(nodes);
        _specService.ValidatePrerequisitesAsync(character, Arg.Any<SpecializationNode>())
            .Returns((true, ""));

        // Act
        var vm = await _builder.BuildAsync(character, specId);

        // Assert
        vm.CurrentSpecIndex.Should().Be(1);
        vm.TotalSpecCount.Should().Be(2);
    }

    [Fact]
    public async Task BuildAsync_ThrowsWhenSpecializationNotFound()
    {
        // Arrange
        var character = CreateTestCharacter();
        var specId = Guid.NewGuid();

        _specRepo.GetByIdAsync(specId).Returns((Specialization?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _builder.BuildAsync(character, specId));
    }

    [Fact]
    public async Task BuildAsync_MarksTier4NodesAsCapstone()
    {
        // Arrange
        var character = CreateTestCharacter();
        var specId = character.UnlockedSpecializationIds[0];
        var spec = CreateTestSpecialization(specId);
        var nodes = CreateTestNodes(specId);

        _specRepo.GetByIdAsync(specId).Returns(spec);
        _specRepo.GetNodesForSpecializationAsync(specId).Returns(nodes);
        _specService.ValidatePrerequisitesAsync(character, Arg.Any<SpecializationNode>())
            .Returns((true, ""));

        // Act
        var vm = await _builder.BuildAsync(character, specId);

        // Assert - Tier 4 node should be capstone
        var capstoneNode = vm.AllNodes.First(n => n.Tier == 4);
        capstoneNode.IsCapstone.Should().BeTrue();
    }

    #endregion

    #region Status Computation Tests

    [Fact]
    public async Task BuildAsync_UnlockedNode_HasUnlockedStatus()
    {
        // Arrange
        var character = CreateTestCharacter();
        var specId = character.UnlockedSpecializationIds[0];
        var spec = CreateTestSpecialization(specId);
        var nodes = CreateTestNodes(specId);
        var unlockedNodeId = nodes[0].Id;

        // Add node to character's progress (this is how nodes are tracked as unlocked)
        character.SpecializationProgress.Add(new CharacterSpecializationProgress
        {
            CharacterId = character.Id,
            NodeId = unlockedNodeId,
            UnlockedAt = DateTime.UtcNow
        });

        _specRepo.GetByIdAsync(specId).Returns(spec);
        _specRepo.GetNodesForSpecializationAsync(specId).Returns(nodes);
        _specService.ValidatePrerequisitesAsync(character, Arg.Any<SpecializationNode>())
            .Returns((true, ""));

        // Act
        var vm = await _builder.BuildAsync(character, specId);

        // Assert
        var node = vm.AllNodes.First(n => n.NodeId == unlockedNodeId);
        node.Status.Should().Be(NodeStatus.Unlocked);
    }

    [Fact]
    public async Task BuildAsync_LockedNode_HasLockedStatus()
    {
        // Arrange
        var character = CreateTestCharacter();
        var specId = character.UnlockedSpecializationIds[0];
        var spec = CreateTestSpecialization(specId);
        var nodes = CreateTestNodes(specId);

        _specRepo.GetByIdAsync(specId).Returns(spec);
        _specRepo.GetNodesForSpecializationAsync(specId).Returns(nodes);
        _specService.ValidatePrerequisitesAsync(character, Arg.Any<SpecializationNode>())
            .Returns((false, "Prerequisites not met"));

        // Act
        var vm = await _builder.BuildAsync(character, specId);

        // Assert
        vm.AllNodes.All(n => n.Status == NodeStatus.Locked).Should().BeTrue();
    }

    [Fact]
    public async Task BuildAsync_AvailableNode_HasAvailableStatus()
    {
        // Arrange
        var character = CreateTestCharacter();
        character.ProgressionPoints = 10;
        var specId = character.UnlockedSpecializationIds[0];
        var spec = CreateTestSpecialization(specId);
        var nodes = CreateTestNodes(specId);

        _specRepo.GetByIdAsync(specId).Returns(spec);
        _specRepo.GetNodesForSpecializationAsync(specId).Returns(nodes);
        _specService.ValidatePrerequisitesAsync(character, Arg.Any<SpecializationNode>())
            .Returns((true, ""));

        // Act
        var vm = await _builder.BuildAsync(character, specId);

        // Assert
        vm.AllNodes.All(n => n.Status == NodeStatus.Available).Should().BeTrue();
    }

    [Fact]
    public async Task BuildAsync_AffordableNode_HasAffordableStatus()
    {
        // Arrange
        var character = CreateTestCharacter();
        character.ProgressionPoints = 0; // Can't afford anything
        var specId = character.UnlockedSpecializationIds[0];
        var spec = CreateTestSpecialization(specId);
        var nodes = CreateTestNodes(specId);

        _specRepo.GetByIdAsync(specId).Returns(spec);
        _specRepo.GetNodesForSpecializationAsync(specId).Returns(nodes);
        _specService.ValidatePrerequisitesAsync(character, Arg.Any<SpecializationNode>())
            .Returns((true, "")); // Prereqs met but can't afford

        // Act
        var vm = await _builder.BuildAsync(character, specId);

        // Assert
        vm.AllNodes.All(n => n.Status == NodeStatus.Affordable).Should().BeTrue();
    }

    #endregion

    #region RefreshAsync Tests

    [Fact]
    public async Task RefreshAsync_PreservesSelectedNodeIndex()
    {
        // Arrange
        var character = CreateTestCharacter();
        var specId = character.UnlockedSpecializationIds[0];
        var spec = CreateTestSpecialization(specId);
        var nodes = CreateTestNodes(specId);

        _specRepo.GetByIdAsync(specId).Returns(spec);
        _specRepo.GetNodesForSpecializationAsync(specId).Returns(nodes);
        _specService.ValidatePrerequisitesAsync(character, Arg.Any<SpecializationNode>())
            .Returns((true, ""));

        var existing = await _builder.BuildAsync(character, specId);
        existing.SelectedNodeIndex = 2;

        // Act
        var refreshed = await _builder.RefreshAsync(existing, character);

        // Assert
        refreshed.SelectedNodeIndex.Should().Be(2);
    }

    [Fact]
    public async Task RefreshAsync_ClampsSelectedNodeIndex_WhenOutOfBounds()
    {
        // Arrange
        var character = CreateTestCharacter();
        var specId = character.UnlockedSpecializationIds[0];
        var spec = CreateTestSpecialization(specId);
        var nodes = CreateTestNodes(specId);

        _specRepo.GetByIdAsync(specId).Returns(spec);
        _specRepo.GetNodesForSpecializationAsync(specId).Returns(nodes);
        _specService.ValidatePrerequisitesAsync(character, Arg.Any<SpecializationNode>())
            .Returns((true, ""));

        var existing = await _builder.BuildAsync(character, specId);
        existing.SelectedNodeIndex = 100; // Way out of bounds

        // Act
        var refreshed = await _builder.RefreshAsync(existing, character);

        // Assert
        refreshed.SelectedNodeIndex.Should().Be(3); // Clamped to max valid index (4 nodes - 1)
    }

    [Fact]
    public async Task RefreshAsync_PreservesFeedbackMessage()
    {
        // Arrange
        var character = CreateTestCharacter();
        var specId = character.UnlockedSpecializationIds[0];
        var spec = CreateTestSpecialization(specId);
        var nodes = CreateTestNodes(specId);

        _specRepo.GetByIdAsync(specId).Returns(spec);
        _specRepo.GetNodesForSpecializationAsync(specId).Returns(nodes);
        _specService.ValidatePrerequisitesAsync(character, Arg.Any<SpecializationNode>())
            .Returns((true, ""));

        var existing = await _builder.BuildAsync(character, specId);
        existing.FeedbackMessage = "Node unlocked!";
        existing.FeedbackIsSuccess = true;

        // Act
        var refreshed = await _builder.RefreshAsync(existing, character);

        // Assert
        refreshed.FeedbackMessage.Should().Be("Node unlocked!");
        refreshed.FeedbackIsSuccess.Should().BeTrue();
    }

    #endregion

    #region Empty Nodes Tests

    [Fact]
    public async Task BuildAsync_WithNoNodes_ReturnsEmptyNodesByTier()
    {
        // Arrange
        var character = CreateTestCharacter();
        var specId = character.UnlockedSpecializationIds[0];
        var spec = CreateTestSpecialization(specId);

        _specRepo.GetByIdAsync(specId).Returns(spec);
        _specRepo.GetNodesForSpecializationAsync(specId).Returns(new List<SpecializationNode>());

        // Act
        var vm = await _builder.BuildAsync(character, specId);

        // Assert
        vm.NodesByTier.Should().BeEmpty();
        vm.AllNodes.Should().BeEmpty();
        vm.TotalNodes.Should().Be(0);
    }

    #endregion
}
