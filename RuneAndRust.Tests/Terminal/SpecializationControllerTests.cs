using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;
using RuneAndRust.Core.ViewModels;
using RuneAndRust.Terminal.Controllers;
using Xunit;
using Character = RuneAndRust.Core.Entities.Character;

namespace RuneAndRust.Tests.Terminal;

/// <summary>
/// Tests for SpecializationController.
/// Validates navigation, unlock, and input handling.
/// </summary>
/// <remarks>See: v0.4.1d (The Grid) for implementation.</remarks>
public class SpecializationControllerTests
{
    private readonly ISpecializationService _specService;
    private readonly ISpecializationGridViewModelBuilder _vmBuilder;
    private readonly ILogger<SpecializationController> _logger;
    private readonly SpecializationController _controller;

    public SpecializationControllerTests()
    {
        _specService = Substitute.For<ISpecializationService>();
        _vmBuilder = Substitute.For<ISpecializationGridViewModelBuilder>();
        _logger = Substitute.For<ILogger<SpecializationController>>();
        _controller = new SpecializationController(_specService, _vmBuilder, _logger);
    }

    private static Character CreateTestCharacter()
    {
        return new Character
        {
            Id = Guid.NewGuid(),
            Name = "TestChar",
            ProgressionPoints = 10,
            UnlockedSpecializationIds = new List<Guid> { Guid.NewGuid() }
        };
    }

    private static SpecializationGridViewModel CreateTestViewModel(int nodeCount = 4)
    {
        var nodes = new List<NodeViewModel>();
        for (int i = 0; i < nodeCount; i++)
        {
            nodes.Add(new NodeViewModel(
                NodeId: Guid.NewGuid(),
                AbilityId: Guid.NewGuid(),
                Name: $"Node{i + 1}",
                Description: $"Description for node {i + 1}",
                Tier: (i / 2) + 1,
                CostPP: 1,
                Status: i == 0 ? NodeStatus.Available : NodeStatus.Locked,
                PositionX: i,
                PositionY: 0,
                IsCapstone: false,
                ParentNodeIds: new List<Guid>()));
        }

        var nodesByTier = nodes
            .GroupBy(n => n.Tier)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<NodeViewModel>)g.ToList());

        return new SpecializationGridViewModel
        {
            SpecializationId = Guid.NewGuid(),
            SpecializationName = "Test Spec",
            SpecializationDescription = "Test Description",
            ProgressionPoints = 10,
            CharacterName = "TestChar",
            NodesByTier = nodesByTier,
            AllNodes = nodes,
            SelectedNodeIndex = 0,
            CurrentSpecIndex = 0,
            TotalSpecCount = 1
        };
    }

    #region Initialization Tests

    [Fact]
    public void IsInitialized_WhenNotInitialized_ReturnsFalse()
    {
        // Assert
        _controller.IsInitialized.Should().BeFalse();
    }

    [Fact]
    public async Task InitializeAsync_SetsIsInitializedToTrue()
    {
        // Arrange
        var character = CreateTestCharacter();
        var specId = character.UnlockedSpecializationIds[0];
        var vm = CreateTestViewModel();

        _vmBuilder.BuildAsync(character, specId).Returns(Task.FromResult(vm));

        // Act
        await _controller.InitializeAsync(character, specId);

        // Assert
        _controller.IsInitialized.Should().BeTrue();
        _controller.CurrentViewModel.Should().NotBeNull();
    }

    [Fact]
    public async Task InitializeAsync_SetsCurrentViewModel()
    {
        // Arrange
        var character = CreateTestCharacter();
        var specId = character.UnlockedSpecializationIds[0];
        var vm = CreateTestViewModel();

        _vmBuilder.BuildAsync(character, specId).Returns(Task.FromResult(vm));

        // Act
        await _controller.InitializeAsync(character, specId);

        // Assert
        _controller.CurrentViewModel.Should().Be(vm);
    }

    #endregion

    #region Exit Tests

    [Fact]
    public async Task HandleInputAsync_Escape_ReturnsExploration()
    {
        // Arrange
        var character = CreateTestCharacter();
        var specId = character.UnlockedSpecializationIds[0];
        var vm = CreateTestViewModel();
        _vmBuilder.BuildAsync(character, specId).Returns(Task.FromResult(vm));
        await _controller.InitializeAsync(character, specId);

        // Act
        var result = await _controller.HandleInputAsync(ConsoleKey.Escape);

        // Assert
        result.Should().Be(GamePhase.Exploration);
    }

    [Fact]
    public async Task HandleInputAsync_Q_ReturnsExploration()
    {
        // Arrange
        var character = CreateTestCharacter();
        var specId = character.UnlockedSpecializationIds[0];
        var vm = CreateTestViewModel();
        _vmBuilder.BuildAsync(character, specId).Returns(Task.FromResult(vm));
        await _controller.InitializeAsync(character, specId);

        // Act
        var result = await _controller.HandleInputAsync(ConsoleKey.Q);

        // Assert
        result.Should().Be(GamePhase.Exploration);
    }

    [Fact]
    public async Task HandleInputAsync_Escape_ResetsController()
    {
        // Arrange
        var character = CreateTestCharacter();
        var specId = character.UnlockedSpecializationIds[0];
        var vm = CreateTestViewModel();
        _vmBuilder.BuildAsync(character, specId).Returns(Task.FromResult(vm));
        await _controller.InitializeAsync(character, specId);

        // Act
        await _controller.HandleInputAsync(ConsoleKey.Escape);

        // Assert
        _controller.IsInitialized.Should().BeFalse();
        _controller.CurrentViewModel.Should().BeNull();
    }

    #endregion

    #region Navigation Tests

    [Fact]
    public async Task HandleInputAsync_DownArrow_StaysInSpecializationMenu()
    {
        // Arrange
        var character = CreateTestCharacter();
        var specId = character.UnlockedSpecializationIds[0];
        var vm = CreateTestViewModel();
        _vmBuilder.BuildAsync(character, specId).Returns(Task.FromResult(vm));
        await _controller.InitializeAsync(character, specId);

        // Act
        var result = await _controller.HandleInputAsync(ConsoleKey.DownArrow);

        // Assert
        result.Should().Be(GamePhase.SpecializationMenu);
    }

    [Fact]
    public async Task HandleInputAsync_UpArrow_StaysInSpecializationMenu()
    {
        // Arrange
        var character = CreateTestCharacter();
        var specId = character.UnlockedSpecializationIds[0];
        var vm = CreateTestViewModel();
        _vmBuilder.BuildAsync(character, specId).Returns(Task.FromResult(vm));
        await _controller.InitializeAsync(character, specId);

        // Act
        var result = await _controller.HandleInputAsync(ConsoleKey.UpArrow);

        // Assert
        result.Should().Be(GamePhase.SpecializationMenu);
    }

    [Fact]
    public async Task HandleInputAsync_LeftArrow_StaysInSpecializationMenu()
    {
        // Arrange
        var character = CreateTestCharacter();
        var specId = character.UnlockedSpecializationIds[0];
        var vm = CreateTestViewModel();
        _vmBuilder.BuildAsync(character, specId).Returns(Task.FromResult(vm));
        await _controller.InitializeAsync(character, specId);

        // Act
        var result = await _controller.HandleInputAsync(ConsoleKey.LeftArrow);

        // Assert
        result.Should().Be(GamePhase.SpecializationMenu);
    }

    [Fact]
    public async Task HandleInputAsync_RightArrow_StaysInSpecializationMenu()
    {
        // Arrange
        var character = CreateTestCharacter();
        var specId = character.UnlockedSpecializationIds[0];
        var vm = CreateTestViewModel();
        _vmBuilder.BuildAsync(character, specId).Returns(Task.FromResult(vm));
        await _controller.InitializeAsync(character, specId);

        // Act
        var result = await _controller.HandleInputAsync(ConsoleKey.RightArrow);

        // Assert
        result.Should().Be(GamePhase.SpecializationMenu);
    }

    #endregion

    #region Unlock Tests

    [Fact]
    public async Task HandleInputAsync_Enter_OnAvailableNode_CallsUnlockNodeAsync()
    {
        // Arrange
        var character = CreateTestCharacter();
        var specId = character.UnlockedSpecializationIds[0];
        var vm = CreateTestViewModel();
        _vmBuilder.BuildAsync(character, specId).Returns(Task.FromResult(vm));
        _vmBuilder.RefreshAsync(Arg.Any<SpecializationGridViewModel>(), character)
            .Returns(Task.FromResult(vm));
        _specService.UnlockNodeAsync(character, Arg.Any<Guid>())
            .Returns(Task.FromResult(NodeUnlockResult.Ok("Unlocked!", Guid.NewGuid(), "Node1", Guid.NewGuid(), 1, 1)));
        await _controller.InitializeAsync(character, specId);

        // Act
        await _controller.HandleInputAsync(ConsoleKey.Enter);

        // Assert
        await _specService.Received(1).UnlockNodeAsync(character, Arg.Any<Guid>());
    }

    [Fact]
    public async Task HandleInputAsync_Spacebar_OnAvailableNode_CallsUnlockNodeAsync()
    {
        // Arrange
        var character = CreateTestCharacter();
        var specId = character.UnlockedSpecializationIds[0];
        var vm = CreateTestViewModel();
        _vmBuilder.BuildAsync(character, specId).Returns(Task.FromResult(vm));
        _vmBuilder.RefreshAsync(Arg.Any<SpecializationGridViewModel>(), character)
            .Returns(Task.FromResult(vm));
        _specService.UnlockNodeAsync(character, Arg.Any<Guid>())
            .Returns(Task.FromResult(NodeUnlockResult.Ok("Unlocked!", Guid.NewGuid(), "Node1", Guid.NewGuid(), 1, 1)));
        await _controller.InitializeAsync(character, specId);

        // Act
        await _controller.HandleInputAsync(ConsoleKey.Spacebar);

        // Assert
        await _specService.Received(1).UnlockNodeAsync(character, Arg.Any<Guid>());
    }

    [Fact]
    public async Task HandleInputAsync_Enter_OnLockedNode_DoesNotCallUnlockNodeAsync()
    {
        // Arrange
        var character = CreateTestCharacter();
        var specId = character.UnlockedSpecializationIds[0];
        var nodes = new List<NodeViewModel>
        {
            new(Guid.NewGuid(), Guid.NewGuid(), "Node1", "Desc", 1, 1,
                NodeStatus.Locked, 0, 0, false, new List<Guid>())
        };
        var vm = new SpecializationGridViewModel
        {
            SpecializationId = specId,
            SpecializationName = "Test",
            ProgressionPoints = 10,
            CharacterName = "TestChar",
            NodesByTier = new Dictionary<int, IReadOnlyList<NodeViewModel>> { { 1, nodes } },
            AllNodes = nodes,
            SelectedNodeIndex = 0,
            CurrentSpecIndex = 0,
            TotalSpecCount = 1
        };

        _vmBuilder.BuildAsync(character, specId).Returns(Task.FromResult(vm));
        await _controller.InitializeAsync(character, specId);

        // Act
        await _controller.HandleInputAsync(ConsoleKey.Enter);

        // Assert
        await _specService.DidNotReceive().UnlockNodeAsync(Arg.Any<Character>(), Arg.Any<Guid>());
        _controller.CurrentViewModel!.FeedbackMessage.Should().Contain("Prerequisites not met");
    }

    [Fact]
    public async Task HandleInputAsync_Enter_OnUnlockedNode_DoesNotCallUnlockNodeAsync()
    {
        // Arrange
        var character = CreateTestCharacter();
        var specId = character.UnlockedSpecializationIds[0];
        var nodes = new List<NodeViewModel>
        {
            new(Guid.NewGuid(), Guid.NewGuid(), "Node1", "Desc", 1, 1,
                NodeStatus.Unlocked, 0, 0, false, new List<Guid>())
        };
        var vm = new SpecializationGridViewModel
        {
            SpecializationId = specId,
            SpecializationName = "Test",
            ProgressionPoints = 10,
            CharacterName = "TestChar",
            NodesByTier = new Dictionary<int, IReadOnlyList<NodeViewModel>> { { 1, nodes } },
            AllNodes = nodes,
            SelectedNodeIndex = 0,
            CurrentSpecIndex = 0,
            TotalSpecCount = 1
        };

        _vmBuilder.BuildAsync(character, specId).Returns(Task.FromResult(vm));
        await _controller.InitializeAsync(character, specId);

        // Act
        await _controller.HandleInputAsync(ConsoleKey.Enter);

        // Assert
        await _specService.DidNotReceive().UnlockNodeAsync(Arg.Any<Character>(), Arg.Any<Guid>());
        _controller.CurrentViewModel!.FeedbackMessage.Should().Contain("already inscribed");
    }

    [Fact]
    public async Task HandleInputAsync_Enter_OnAffordableNode_ShowsInsufficientPP()
    {
        // Arrange
        var character = CreateTestCharacter();
        var specId = character.UnlockedSpecializationIds[0];
        var nodes = new List<NodeViewModel>
        {
            new(Guid.NewGuid(), Guid.NewGuid(), "Node1", "Desc", 1, 5,
                NodeStatus.Affordable, 0, 0, false, new List<Guid>())
        };
        var vm = new SpecializationGridViewModel
        {
            SpecializationId = specId,
            SpecializationName = "Test",
            ProgressionPoints = 2,
            CharacterName = "TestChar",
            NodesByTier = new Dictionary<int, IReadOnlyList<NodeViewModel>> { { 1, nodes } },
            AllNodes = nodes,
            SelectedNodeIndex = 0,
            CurrentSpecIndex = 0,
            TotalSpecCount = 1
        };

        _vmBuilder.BuildAsync(character, specId).Returns(Task.FromResult(vm));
        await _controller.InitializeAsync(character, specId);

        // Act
        await _controller.HandleInputAsync(ConsoleKey.Enter);

        // Assert
        await _specService.DidNotReceive().UnlockNodeAsync(Arg.Any<Character>(), Arg.Any<Guid>());
        _controller.CurrentViewModel!.FeedbackMessage.Should().Contain("Insufficient PP");
    }

    #endregion

    #region Reset Tests

    [Fact]
    public void Reset_ClearsCurrentViewModel()
    {
        // Act
        _controller.Reset();

        // Assert
        _controller.CurrentViewModel.Should().BeNull();
        _controller.IsInitialized.Should().BeFalse();
    }

    #endregion

    #region Not Initialized Tests

    [Fact]
    public async Task HandleInputAsync_WhenNotInitialized_ReturnsExploration()
    {
        // Act
        var result = await _controller.HandleInputAsync(ConsoleKey.Enter);

        // Assert
        result.Should().Be(GamePhase.Exploration);
    }

    #endregion
}
