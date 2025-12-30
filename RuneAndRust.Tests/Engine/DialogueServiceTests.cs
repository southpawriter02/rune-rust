using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Core.Conditions;
using RuneAndRust.Core.Effects;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Events;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;
using RuneAndRust.Engine.Services;
using Xunit;
using Character = RuneAndRust.Core.Entities.Character;

namespace RuneAndRust.Tests.Engine;

/// <summary>
/// Unit tests for the DialogueService (v0.4.2c - The Voice).
/// Tests dialogue flow, session management, and event publishing.
/// </summary>
public class DialogueServiceTests
{
    private readonly Mock<IDialogueRepository> _mockRepository;
    private readonly Mock<IDialogueConditionEvaluator> _mockEvaluator;
    private readonly Mock<IDialogueEffectExecutor> _mockExecutor;
    private readonly Mock<IEventBus> _mockEventBus;
    private readonly Mock<ILogger<DialogueService>> _mockLogger;
    private readonly GameState _gameState;
    private readonly DialogueService _sut;

    public DialogueServiceTests()
    {
        _mockRepository = new Mock<IDialogueRepository>();
        _mockEvaluator = new Mock<IDialogueConditionEvaluator>();
        _mockExecutor = new Mock<IDialogueEffectExecutor>();
        _mockEventBus = new Mock<IEventBus>();
        _mockLogger = new Mock<ILogger<DialogueService>>();
        _gameState = new GameState();

        _sut = new DialogueService(
            _mockRepository.Object,
            _mockEvaluator.Object,
            _mockExecutor.Object,
            _mockEventBus.Object,
            _mockLogger.Object);

        // Default: Evaluator returns available for all options
        _mockEvaluator
            .Setup(e => e.EvaluateOptionAsync(It.IsAny<Character>(), It.IsAny<DialogueOption>()))
            .ReturnsAsync(OptionVisibilityResult.Available(Guid.NewGuid()));

        _mockEvaluator
            .Setup(e => e.EvaluateNodeOptionsAsync(It.IsAny<Character>(), It.IsAny<DialogueNode>()))
            .ReturnsAsync(new List<OptionVisibilityResult>());

        // Default: No effects
        _mockExecutor
            .Setup(e => e.ExecuteEffectsAsync(It.IsAny<Character>(), It.IsAny<IEnumerable<DialogueEffect>>(), It.IsAny<GameState>()))
            .ReturnsAsync(new List<DialogueEffectResult>());
    }

    private Character CreateTestCharacter(string name = "TestChar")
    {
        return new Character
        {
            Id = Guid.NewGuid(),
            Name = name,
            Level = 1
        };
    }

    private DialogueTree CreateTestTree(string treeId = "test_tree", string npcName = "Test NPC")
    {
        var tree = new DialogueTree
        {
            Id = Guid.NewGuid(),
            TreeId = treeId,
            NpcName = npcName,
            RootNodeId = "root"
        };

        var rootNode = new DialogueNode
        {
            Id = Guid.NewGuid(),
            TreeId = tree.Id,
            NodeId = "root",
            SpeakerName = npcName,
            Text = "Hello, traveler!",
            IsTerminal = false,
            Tree = tree
        };

        var option1 = new DialogueOption
        {
            Id = Guid.NewGuid(),
            NodeId = rootNode.Id,
            Text = "Tell me more.",
            NextNodeId = "node_more",
            DisplayOrder = 0,
            Node = rootNode
        };

        var option2 = new DialogueOption
        {
            Id = Guid.NewGuid(),
            NodeId = rootNode.Id,
            Text = "Goodbye.",
            NextNodeId = null, // Terminal option
            DisplayOrder = 1,
            Node = rootNode
        };

        rootNode.Options.Add(option1);
        rootNode.Options.Add(option2);

        var moreNode = new DialogueNode
        {
            Id = Guid.NewGuid(),
            TreeId = tree.Id,
            NodeId = "node_more",
            SpeakerName = npcName,
            Text = "Here is more information.",
            IsTerminal = false,
            Tree = tree
        };

        var moreOption = new DialogueOption
        {
            Id = Guid.NewGuid(),
            NodeId = moreNode.Id,
            Text = "Thanks!",
            NextNodeId = null,
            DisplayOrder = 0,
            Node = moreNode
        };

        moreNode.Options.Add(moreOption);

        tree.Nodes.Add(rootNode);
        tree.Nodes.Add(moreNode);

        return tree;
    }

    // ═══════════════════════════════════════════════════════════════════════
    // StartDialogueAsync Tests (8 tests)
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task StartDialogueAsync_ValidTree_ReturnsSuccess()
    {
        // Arrange
        var character = CreateTestCharacter();
        var tree = CreateTestTree();

        _mockRepository
            .Setup(r => r.GetTreeByIdAsync("test_tree"))
            .ReturnsAsync(tree);

        // Act
        var result = await _sut.StartDialogueAsync(character, "test_tree", _gameState);

        // Assert
        result.Success.Should().BeTrue();
        result.ViewModel.Should().NotBeNull();
        result.ViewModel!.NpcName.Should().Be("Test NPC");
        result.ViewModel.CurrentNodeId.Should().Be("root");
        result.Session.Should().NotBeNull();
    }

    [Fact]
    public async Task StartDialogueAsync_TreeNotFound_ReturnsFailed()
    {
        // Arrange
        var character = CreateTestCharacter();

        _mockRepository
            .Setup(r => r.GetTreeByIdAsync("nonexistent"))
            .ReturnsAsync((DialogueTree?)null);

        // Act
        var result = await _sut.StartDialogueAsync(character, "nonexistent", _gameState);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not found");
    }

    [Fact]
    public async Task StartDialogueAsync_AlreadyInDialogue_ReturnsFailed()
    {
        // Arrange
        var character = CreateTestCharacter();
        var tree = CreateTestTree();

        _mockRepository
            .Setup(r => r.GetTreeByIdAsync("test_tree"))
            .ReturnsAsync(tree);

        // Start first dialogue
        await _sut.StartDialogueAsync(character, "test_tree", _gameState);

        // Act - Try to start another
        var result = await _sut.StartDialogueAsync(character, "test_tree", _gameState);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Already in");
    }

    [Fact]
    public async Task StartDialogueAsync_SetsCurrentDialogueSession()
    {
        // Arrange
        var character = CreateTestCharacter();
        var tree = CreateTestTree();

        _mockRepository
            .Setup(r => r.GetTreeByIdAsync("test_tree"))
            .ReturnsAsync(tree);

        // Act
        await _sut.StartDialogueAsync(character, "test_tree", _gameState);

        // Assert
        _gameState.CurrentDialogueSession.Should().NotBeNull();
        _gameState.CurrentDialogueSession!.CharacterId.Should().Be(character.Id);
    }

    [Fact]
    public async Task StartDialogueAsync_PublishesDialogueStartedEvent()
    {
        // Arrange
        var character = CreateTestCharacter();
        var tree = CreateTestTree();

        _mockRepository
            .Setup(r => r.GetTreeByIdAsync("test_tree"))
            .ReturnsAsync(tree);

        DialogueStartedEvent? capturedEvent = null;
        _mockEventBus
            .Setup(e => e.PublishAsync(It.IsAny<DialogueStartedEvent>()))
            .Callback<DialogueStartedEvent>(e => capturedEvent = e)
            .Returns(Task.CompletedTask);

        // Act
        await _sut.StartDialogueAsync(character, "test_tree", _gameState);

        // Assert
        capturedEvent.Should().NotBeNull();
        capturedEvent!.TreeId.Should().Be("test_tree");
        capturedEvent.NpcName.Should().Be("Test NPC");
        capturedEvent.CharacterId.Should().Be(character.Id);
    }

    [Fact]
    public async Task StartDialogueAsync_AddsRootNodeToVisitedNodes()
    {
        // Arrange
        var character = CreateTestCharacter();
        var tree = CreateTestTree();

        _mockRepository
            .Setup(r => r.GetTreeByIdAsync("test_tree"))
            .ReturnsAsync(tree);

        // Act
        var result = await _sut.StartDialogueAsync(character, "test_tree", _gameState);

        // Assert
        result.Session!.VisitedNodeIds.Should().Contain("root");
    }

    [Fact]
    public async Task StartDialogueAsync_RootNodeNotFound_ReturnsFailed()
    {
        // Arrange
        var character = CreateTestCharacter();
        var tree = new DialogueTree
        {
            Id = Guid.NewGuid(),
            TreeId = "bad_tree",
            NpcName = "Test NPC",
            RootNodeId = "nonexistent_root"
        };

        _mockRepository
            .Setup(r => r.GetTreeByIdAsync("bad_tree"))
            .ReturnsAsync(tree);

        // Act
        var result = await _sut.StartDialogueAsync(character, "bad_tree", _gameState);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Root node");
    }

    [Fact]
    public async Task StartDialogueAsync_ReturnsCorrectOptionsInViewModel()
    {
        // Arrange
        var character = CreateTestCharacter();
        var tree = CreateTestTree();

        _mockRepository
            .Setup(r => r.GetTreeByIdAsync("test_tree"))
            .ReturnsAsync(tree);

        // Act
        var result = await _sut.StartDialogueAsync(character, "test_tree", _gameState);

        // Assert
        result.ViewModel!.Options.Should().HaveCount(2);
        result.ViewModel.Options[0].Text.Should().Be("Tell me more.");
        result.ViewModel.Options[1].Text.Should().Be("Goodbye.");
    }

    // ═══════════════════════════════════════════════════════════════════════
    // SelectOptionAsync Tests (12 tests)
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task SelectOptionAsync_ValidOption_NavigatesToNextNode()
    {
        // Arrange
        var character = CreateTestCharacter();
        var tree = CreateTestTree();

        _mockRepository
            .Setup(r => r.GetTreeByIdAsync("test_tree"))
            .ReturnsAsync(tree);

        await _sut.StartDialogueAsync(character, "test_tree", _gameState);
        var optionId = tree.Nodes.First(n => n.NodeId == "root").Options.First(o => o.NextNodeId == "node_more").Id.ToString();

        // Act
        var result = await _sut.SelectOptionAsync(character, optionId, _gameState);

        // Assert
        result.Success.Should().BeTrue();
        result.DialogueEnded.Should().BeFalse();
        result.ViewModel!.CurrentNodeId.Should().Be("node_more");
    }

    [Fact]
    public async Task SelectOptionAsync_TerminalOption_EndsDialogue()
    {
        // Arrange
        var character = CreateTestCharacter();
        var tree = CreateTestTree();

        _mockRepository
            .Setup(r => r.GetTreeByIdAsync("test_tree"))
            .ReturnsAsync(tree);

        await _sut.StartDialogueAsync(character, "test_tree", _gameState);
        var terminalOption = tree.Nodes.First(n => n.NodeId == "root").Options.First(o => o.NextNodeId == null);

        // Act
        var result = await _sut.SelectOptionAsync(character, terminalOption.Id.ToString(), _gameState);

        // Assert
        result.Success.Should().BeTrue();
        result.DialogueEnded.Should().BeTrue();
        result.EndReason.Should().Be(DialogueEndReason.PlayerExit);
        _gameState.CurrentDialogueSession.Should().BeNull();
    }

    [Fact]
    public async Task SelectOptionAsync_NoActiveSession_ReturnsFailed()
    {
        // Arrange
        var character = CreateTestCharacter();

        // Act
        var result = await _sut.SelectOptionAsync(character, "some_option", _gameState);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("No active");
    }

    [Fact]
    public async Task SelectOptionAsync_WrongCharacter_ReturnsFailed()
    {
        // Arrange
        var character1 = CreateTestCharacter("Char1");
        var character2 = CreateTestCharacter("Char2");
        var tree = CreateTestTree();

        _mockRepository
            .Setup(r => r.GetTreeByIdAsync("test_tree"))
            .ReturnsAsync(tree);

        await _sut.StartDialogueAsync(character1, "test_tree", _gameState);

        // Act - Different character tries to interact
        var result = await _sut.SelectOptionAsync(character2, "some_option", _gameState);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not in this dialogue");
    }

    [Fact]
    public async Task SelectOptionAsync_OptionNotFound_ReturnsFailed()
    {
        // Arrange
        var character = CreateTestCharacter();
        var tree = CreateTestTree();

        _mockRepository
            .Setup(r => r.GetTreeByIdAsync("test_tree"))
            .ReturnsAsync(tree);

        await _sut.StartDialogueAsync(character, "test_tree", _gameState);

        // Act
        var result = await _sut.SelectOptionAsync(character, "nonexistent_option", _gameState);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not found");
    }

    [Fact]
    public async Task SelectOptionAsync_UnavailableOption_ReturnsFailed()
    {
        // Arrange
        var character = CreateTestCharacter();
        var tree = CreateTestTree();
        var option = tree.Nodes.First().Options.First();

        _mockRepository
            .Setup(r => r.GetTreeByIdAsync("test_tree"))
            .ReturnsAsync(tree);

        _mockEvaluator
            .Setup(e => e.EvaluateOptionAsync(character, option))
            .ReturnsAsync(OptionVisibilityResult.Locked(
                option.Id,
                "Requires higher reputation",
                "Need Friendly disposition",
                new List<ConditionResult>()));

        await _sut.StartDialogueAsync(character, "test_tree", _gameState);

        // Act
        var result = await _sut.SelectOptionAsync(character, option.Id.ToString(), _gameState);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("reputation");
    }

    [Fact]
    public async Task SelectOptionAsync_ExecutesEffects()
    {
        // Arrange
        var character = CreateTestCharacter();
        var tree = CreateTestTree();
        var option = tree.Nodes.First().Options.First(o => o.NextNodeId != null);
        option.Effects.Add(new SetFlagEffect { FlagKey = "test_flag", Value = true });

        _mockRepository
            .Setup(r => r.GetTreeByIdAsync("test_tree"))
            .ReturnsAsync(tree);

        var effectResults = new List<DialogueEffectResult>
        {
            DialogueEffectResult.Successful(DialogueEffectType.SetFlag, "Flag set")
        };

        _mockExecutor
            .Setup(e => e.ExecuteEffectsAsync(character, option.Effects, _gameState))
            .ReturnsAsync(effectResults);

        await _sut.StartDialogueAsync(character, "test_tree", _gameState);

        // Act
        var result = await _sut.SelectOptionAsync(character, option.Id.ToString(), _gameState);

        // Assert
        result.EffectResults.Should().HaveCount(1);
        _mockExecutor.Verify(e => e.ExecuteEffectsAsync(character, option.Effects, _gameState), Times.Once);
    }

    [Fact]
    public async Task SelectOptionAsync_PublishesOptionSelectedEvent()
    {
        // Arrange
        var character = CreateTestCharacter();
        var tree = CreateTestTree();
        var option = tree.Nodes.First().Options.First(o => o.NextNodeId != null);

        _mockRepository
            .Setup(r => r.GetTreeByIdAsync("test_tree"))
            .ReturnsAsync(tree);

        DialogueOptionSelectedEvent? capturedEvent = null;
        _mockEventBus
            .Setup(e => e.PublishAsync(It.IsAny<DialogueOptionSelectedEvent>()))
            .Callback<DialogueOptionSelectedEvent>(e => capturedEvent = e)
            .Returns(Task.CompletedTask);

        await _sut.StartDialogueAsync(character, "test_tree", _gameState);

        // Act
        await _sut.SelectOptionAsync(character, option.Id.ToString(), _gameState);

        // Assert
        capturedEvent.Should().NotBeNull();
        capturedEvent!.FromNodeId.Should().Be("root");
        capturedEvent.ToNodeId.Should().Be("node_more");
    }

    [Fact]
    public async Task SelectOptionAsync_IncrementsOptionsSelectedCount()
    {
        // Arrange
        var character = CreateTestCharacter();
        var tree = CreateTestTree();
        var option = tree.Nodes.First().Options.First(o => o.NextNodeId != null);

        _mockRepository
            .Setup(r => r.GetTreeByIdAsync("test_tree"))
            .ReturnsAsync(tree);

        await _sut.StartDialogueAsync(character, "test_tree", _gameState);

        // Act
        await _sut.SelectOptionAsync(character, option.Id.ToString(), _gameState);

        // Assert
        _gameState.CurrentDialogueSession!.OptionsSelectedCount.Should().Be(1);
    }

    [Fact]
    public async Task SelectOptionAsync_NextNodeNotFound_ReturnsFailedAndEndsDialogue()
    {
        // Arrange
        var character = CreateTestCharacter();
        var tree = CreateTestTree();
        var option = tree.Nodes.First().Options.First();
        option.NextNodeId = "nonexistent_node"; // Point to non-existent node

        _mockRepository
            .Setup(r => r.GetTreeByIdAsync("test_tree"))
            .ReturnsAsync(tree);

        await _sut.StartDialogueAsync(character, "test_tree", _gameState);

        // Act
        var result = await _sut.SelectOptionAsync(character, option.Id.ToString(), _gameState);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not found");
        _gameState.CurrentDialogueSession.Should().BeNull(); // Session ended due to error
    }

    [Fact]
    public async Task SelectOptionAsync_AddsNewNodeToVisitedNodes()
    {
        // Arrange
        var character = CreateTestCharacter();
        var tree = CreateTestTree();
        var option = tree.Nodes.First().Options.First(o => o.NextNodeId == "node_more");

        _mockRepository
            .Setup(r => r.GetTreeByIdAsync("test_tree"))
            .ReturnsAsync(tree);

        await _sut.StartDialogueAsync(character, "test_tree", _gameState);

        // Act
        await _sut.SelectOptionAsync(character, option.Id.ToString(), _gameState);

        // Assert
        _gameState.CurrentDialogueSession!.VisitedNodeIds.Should().Contain("node_more");
    }

    [Fact]
    public async Task SelectOptionAsync_TerminalNode_EndsWithNpcExit()
    {
        // Arrange
        var character = CreateTestCharacter();
        var tree = CreateTestTree();

        // Make node_more a terminal node with no options
        var moreNode = tree.Nodes.First(n => n.NodeId == "node_more");
        moreNode.IsTerminal = true;
        moreNode.Options.Clear();

        var option = tree.Nodes.First().Options.First(o => o.NextNodeId == "node_more");

        _mockRepository
            .Setup(r => r.GetTreeByIdAsync("test_tree"))
            .ReturnsAsync(tree);

        await _sut.StartDialogueAsync(character, "test_tree", _gameState);

        // Act
        var result = await _sut.SelectOptionAsync(character, option.Id.ToString(), _gameState);

        // Assert
        result.DialogueEnded.Should().BeTrue();
        result.EndReason.Should().Be(DialogueEndReason.NpcExit);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // EndDialogueAsync Tests (5 tests)
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task EndDialogueAsync_ValidSession_ReturnsSuccess()
    {
        // Arrange
        var character = CreateTestCharacter();
        var tree = CreateTestTree();

        _mockRepository
            .Setup(r => r.GetTreeByIdAsync("test_tree"))
            .ReturnsAsync(tree);

        await _sut.StartDialogueAsync(character, "test_tree", _gameState);

        // Act
        var result = await _sut.EndDialogueAsync(DialogueEndReason.PlayerCancel, _gameState);

        // Assert
        result.Success.Should().BeTrue();
        result.Reason.Should().Be(DialogueEndReason.PlayerCancel);
    }

    [Fact]
    public async Task EndDialogueAsync_NoActiveSession_ReturnsFailed()
    {
        // Act
        var result = await _sut.EndDialogueAsync(DialogueEndReason.PlayerCancel, _gameState);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("No active");
    }

    [Fact]
    public async Task EndDialogueAsync_ClearsCurrentDialogueSession()
    {
        // Arrange
        var character = CreateTestCharacter();
        var tree = CreateTestTree();

        _mockRepository
            .Setup(r => r.GetTreeByIdAsync("test_tree"))
            .ReturnsAsync(tree);

        await _sut.StartDialogueAsync(character, "test_tree", _gameState);

        // Act
        await _sut.EndDialogueAsync(DialogueEndReason.PlayerCancel, _gameState);

        // Assert
        _gameState.CurrentDialogueSession.Should().BeNull();
    }

    [Fact]
    public async Task EndDialogueAsync_PublishesDialogueEndedEvent()
    {
        // Arrange
        var character = CreateTestCharacter();
        var tree = CreateTestTree();

        _mockRepository
            .Setup(r => r.GetTreeByIdAsync("test_tree"))
            .ReturnsAsync(tree);

        DialogueEndedEvent? capturedEvent = null;
        _mockEventBus
            .Setup(e => e.PublishAsync(It.IsAny<DialogueEndedEvent>()))
            .Callback<DialogueEndedEvent>(e => capturedEvent = e)
            .Returns(Task.CompletedTask);

        await _sut.StartDialogueAsync(character, "test_tree", _gameState);

        // Act
        await _sut.EndDialogueAsync(DialogueEndReason.Interrupted, _gameState);

        // Assert
        capturedEvent.Should().NotBeNull();
        capturedEvent!.Reason.Should().Be(DialogueEndReason.Interrupted);
        capturedEvent.TreeId.Should().Be("test_tree");
    }

    [Fact]
    public async Task EndDialogueAsync_ReturnsSessionStatistics()
    {
        // Arrange
        var character = CreateTestCharacter();
        var tree = CreateTestTree();
        var option = tree.Nodes.First().Options.First(o => o.NextNodeId != null);

        _mockRepository
            .Setup(r => r.GetTreeByIdAsync("test_tree"))
            .ReturnsAsync(tree);

        await _sut.StartDialogueAsync(character, "test_tree", _gameState);
        await _sut.SelectOptionAsync(character, option.Id.ToString(), _gameState);

        // Act
        var result = await _sut.EndDialogueAsync(DialogueEndReason.PlayerCancel, _gameState);

        // Assert
        result.NodesVisited.Should().Be(2); // root + node_more
        result.OptionsSelected.Should().Be(1);
        result.Duration.Should().BeGreaterThan(TimeSpan.Zero);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // GetCurrentDialogueAsync Tests (3 tests)
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task GetCurrentDialogueAsync_NoSession_ReturnsNull()
    {
        // Arrange
        var character = CreateTestCharacter();

        // Act
        var result = await _sut.GetCurrentDialogueAsync(character, _gameState);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetCurrentDialogueAsync_WrongCharacter_ReturnsNull()
    {
        // Arrange
        var character1 = CreateTestCharacter("Char1");
        var character2 = CreateTestCharacter("Char2");
        var tree = CreateTestTree();

        _mockRepository
            .Setup(r => r.GetTreeByIdAsync("test_tree"))
            .ReturnsAsync(tree);

        await _sut.StartDialogueAsync(character1, "test_tree", _gameState);

        // Act
        var result = await _sut.GetCurrentDialogueAsync(character2, _gameState);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetCurrentDialogueAsync_ValidSession_ReturnsViewModel()
    {
        // Arrange
        var character = CreateTestCharacter();
        var tree = CreateTestTree();

        _mockRepository
            .Setup(r => r.GetTreeByIdAsync("test_tree"))
            .ReturnsAsync(tree);

        await _sut.StartDialogueAsync(character, "test_tree", _gameState);

        // Act
        var result = await _sut.GetCurrentDialogueAsync(character, _gameState);

        // Assert
        result.Should().NotBeNull();
        result!.NpcName.Should().Be("Test NPC");
        result.CurrentNodeId.Should().Be("root");
    }

    // ═══════════════════════════════════════════════════════════════════════
    // IsInDialogue Tests (2 tests)
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task IsInDialogue_NoSession_ReturnsFalse()
    {
        // Arrange
        var characterId = Guid.NewGuid();

        // Act
        var result = _sut.IsInDialogue(characterId, _gameState);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsInDialogue_WithSession_ReturnsTrue()
    {
        // Arrange
        var character = CreateTestCharacter();
        var tree = CreateTestTree();

        _mockRepository
            .Setup(r => r.GetTreeByIdAsync("test_tree"))
            .ReturnsAsync(tree);

        await _sut.StartDialogueAsync(character, "test_tree", _gameState);

        // Act
        var result = _sut.IsInDialogue(character.Id, _gameState);

        // Assert
        result.Should().BeTrue();
    }
}
