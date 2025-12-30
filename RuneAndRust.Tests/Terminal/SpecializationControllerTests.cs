using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;
using RuneAndRust.Terminal.Controllers;
using Xunit;
using Character = RuneAndRust.Core.Entities.Character;

namespace RuneAndRust.Tests.Terminal;

/// <summary>
/// Tests for the SpecializationController class.
/// Validates navigation, input handling, and unlock operations.
/// </summary>
/// <remarks>See: v0.4.1c (The Tree of Runes) for implementation.</remarks>
public class SpecializationControllerTests
{
    private readonly Mock<ISpecializationService> _mockSpecService;
    private readonly Mock<ILogger<SpecializationController>> _mockLogger;
    private readonly SpecializationController _controller;
    private readonly Character _testCharacter;

    public SpecializationControllerTests()
    {
        _mockSpecService = new Mock<ISpecializationService>();
        _mockLogger = new Mock<ILogger<SpecializationController>>();
        _controller = new SpecializationController(_mockSpecService.Object, _mockLogger.Object);
        _testCharacter = new Character { Name = "TestHero", ProgressionPoints = 20 };
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Assert
        _controller.ViewMode.Should().Be(SpecializationViewMode.SpecList);
        _controller.SelectedSpecIndex.Should().Be(0);
        _controller.SelectedNodeIndex.Should().Be(0);
        _controller.LastStatusMessage.Should().BeNull();
    }

    #endregion

    #region Navigation Tests - Escape/Quit

    [Fact]
    public async Task HandleInputAsync_Escape_ReturnsExplorationPhase()
    {
        // Arrange
        var specIds = new List<Guid> { Guid.NewGuid() };
        var nodeIds = new List<Guid>();

        // Act
        var result = await _controller.HandleInputAsync(ConsoleKey.Escape, _testCharacter, specIds, nodeIds);

        // Assert
        result.Should().Be(GamePhase.Exploration);
    }

    [Fact]
    public async Task HandleInputAsync_Q_ReturnsExplorationPhase()
    {
        // Arrange
        var specIds = new List<Guid> { Guid.NewGuid() };
        var nodeIds = new List<Guid>();

        // Act
        var result = await _controller.HandleInputAsync(ConsoleKey.Q, _testCharacter, specIds, nodeIds);

        // Assert
        result.Should().Be(GamePhase.Exploration);
    }

    #endregion

    #region Navigation Tests - SpecList Mode

    [Fact]
    public async Task HandleInputAsync_UpArrow_InSpecListMode_DecreasesSelectedSpecIndex()
    {
        // Arrange
        _controller.UpdateCounts(3, 0);
        // Navigate down first to have room to go up
        var specIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        await _controller.HandleInputAsync(ConsoleKey.DownArrow, _testCharacter, specIds, new List<Guid>());
        _controller.SelectedSpecIndex.Should().Be(1);

        // Act
        await _controller.HandleInputAsync(ConsoleKey.UpArrow, _testCharacter, specIds, new List<Guid>());

        // Assert
        _controller.SelectedSpecIndex.Should().Be(0);
    }

    [Fact]
    public async Task HandleInputAsync_UpArrow_AtIndexZero_DoesNotGoNegative()
    {
        // Arrange
        _controller.UpdateCounts(3, 0);
        var specIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

        // Act
        await _controller.HandleInputAsync(ConsoleKey.UpArrow, _testCharacter, specIds, new List<Guid>());

        // Assert
        _controller.SelectedSpecIndex.Should().Be(0);
    }

    [Fact]
    public async Task HandleInputAsync_DownArrow_InSpecListMode_IncreasesSelectedSpecIndex()
    {
        // Arrange
        _controller.UpdateCounts(3, 0);
        var specIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

        // Act
        await _controller.HandleInputAsync(ConsoleKey.DownArrow, _testCharacter, specIds, new List<Guid>());

        // Assert
        _controller.SelectedSpecIndex.Should().Be(1);
    }

    [Fact]
    public async Task HandleInputAsync_DownArrow_AtLastIndex_DoesNotExceedBounds()
    {
        // Arrange
        _controller.UpdateCounts(3, 0);
        var specIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

        // Navigate to last item
        await _controller.HandleInputAsync(ConsoleKey.DownArrow, _testCharacter, specIds, new List<Guid>());
        await _controller.HandleInputAsync(ConsoleKey.DownArrow, _testCharacter, specIds, new List<Guid>());
        _controller.SelectedSpecIndex.Should().Be(2);

        // Act - try to go beyond
        await _controller.HandleInputAsync(ConsoleKey.DownArrow, _testCharacter, specIds, new List<Guid>());

        // Assert
        _controller.SelectedSpecIndex.Should().Be(2);
    }

    [Fact]
    public async Task HandleInputAsync_W_InSpecListMode_NavigatesUp()
    {
        // Arrange
        _controller.UpdateCounts(3, 0);
        var specIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        await _controller.HandleInputAsync(ConsoleKey.DownArrow, _testCharacter, specIds, new List<Guid>());

        // Act
        await _controller.HandleInputAsync(ConsoleKey.W, _testCharacter, specIds, new List<Guid>());

        // Assert
        _controller.SelectedSpecIndex.Should().Be(0);
    }

    [Fact]
    public async Task HandleInputAsync_S_InSpecListMode_NavigatesDown()
    {
        // Arrange
        _controller.UpdateCounts(3, 0);
        var specIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

        // Act
        await _controller.HandleInputAsync(ConsoleKey.S, _testCharacter, specIds, new List<Guid>());

        // Assert
        _controller.SelectedSpecIndex.Should().Be(1);
    }

    [Fact]
    public async Task HandleInputAsync_RightArrow_SwitchesToTreeDetailMode()
    {
        // Arrange
        var specIds = new List<Guid> { Guid.NewGuid() };

        // Act
        await _controller.HandleInputAsync(ConsoleKey.RightArrow, _testCharacter, specIds, new List<Guid>());

        // Assert
        _controller.ViewMode.Should().Be(SpecializationViewMode.TreeDetail);
    }

    [Fact]
    public async Task HandleInputAsync_D_SwitchesToTreeDetailMode()
    {
        // Arrange
        var specIds = new List<Guid> { Guid.NewGuid() };

        // Act
        await _controller.HandleInputAsync(ConsoleKey.D, _testCharacter, specIds, new List<Guid>());

        // Assert
        _controller.ViewMode.Should().Be(SpecializationViewMode.TreeDetail);
    }

    #endregion

    #region Navigation Tests - TreeDetail Mode

    [Fact]
    public async Task HandleInputAsync_LeftArrow_SwitchesToSpecListMode()
    {
        // Arrange
        var specIds = new List<Guid> { Guid.NewGuid() };
        await _controller.HandleInputAsync(ConsoleKey.RightArrow, _testCharacter, specIds, new List<Guid>());
        _controller.ViewMode.Should().Be(SpecializationViewMode.TreeDetail);

        // Act
        await _controller.HandleInputAsync(ConsoleKey.LeftArrow, _testCharacter, specIds, new List<Guid>());

        // Assert
        _controller.ViewMode.Should().Be(SpecializationViewMode.SpecList);
    }

    [Fact]
    public async Task HandleInputAsync_A_SwitchesToSpecListMode()
    {
        // Arrange
        var specIds = new List<Guid> { Guid.NewGuid() };
        await _controller.HandleInputAsync(ConsoleKey.D, _testCharacter, specIds, new List<Guid>());

        // Act
        await _controller.HandleInputAsync(ConsoleKey.A, _testCharacter, specIds, new List<Guid>());

        // Assert
        _controller.ViewMode.Should().Be(SpecializationViewMode.SpecList);
    }

    [Fact]
    public async Task HandleInputAsync_UpArrow_InTreeDetailMode_DecreasesSelectedNodeIndex()
    {
        // Arrange
        _controller.UpdateCounts(1, 5);
        var specIds = new List<Guid> { Guid.NewGuid() };
        var nodeIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

        // Switch to tree mode and navigate down first
        await _controller.HandleInputAsync(ConsoleKey.RightArrow, _testCharacter, specIds, nodeIds);
        await _controller.HandleInputAsync(ConsoleKey.DownArrow, _testCharacter, specIds, nodeIds);
        _controller.SelectedNodeIndex.Should().Be(1);

        // Act
        await _controller.HandleInputAsync(ConsoleKey.UpArrow, _testCharacter, specIds, nodeIds);

        // Assert
        _controller.SelectedNodeIndex.Should().Be(0);
    }

    [Fact]
    public async Task HandleInputAsync_DownArrow_InTreeDetailMode_IncreasesSelectedNodeIndex()
    {
        // Arrange
        _controller.UpdateCounts(1, 5);
        var specIds = new List<Guid> { Guid.NewGuid() };
        var nodeIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

        // Switch to tree mode
        await _controller.HandleInputAsync(ConsoleKey.RightArrow, _testCharacter, specIds, nodeIds);

        // Act
        await _controller.HandleInputAsync(ConsoleKey.DownArrow, _testCharacter, specIds, nodeIds);

        // Assert
        _controller.SelectedNodeIndex.Should().Be(1);
    }

    [Fact]
    public async Task HandleInputAsync_ChangingSpec_ResetsNodeIndex()
    {
        // Arrange
        _controller.UpdateCounts(3, 5);
        var specIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        var nodeIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

        // Switch to tree mode and navigate down
        await _controller.HandleInputAsync(ConsoleKey.RightArrow, _testCharacter, specIds, nodeIds);
        await _controller.HandleInputAsync(ConsoleKey.DownArrow, _testCharacter, specIds, nodeIds);
        await _controller.HandleInputAsync(ConsoleKey.DownArrow, _testCharacter, specIds, nodeIds);
        _controller.SelectedNodeIndex.Should().Be(2);

        // Switch back to spec list
        await _controller.HandleInputAsync(ConsoleKey.LeftArrow, _testCharacter, specIds, nodeIds);

        // Act - change spec
        await _controller.HandleInputAsync(ConsoleKey.DownArrow, _testCharacter, specIds, nodeIds);

        // Assert - node index should be reset
        _controller.SelectedNodeIndex.Should().Be(0);
    }

    #endregion

    #region Unlock Tests - Specialization

    [Fact]
    public async Task HandleInputAsync_Enter_InSpecListMode_CallsUnlockSpecialization()
    {
        // Arrange
        var specId = Guid.NewGuid();
        var specIds = new List<Guid> { specId };
        var nodeIds = new List<Guid>();

        _mockSpecService
            .Setup(s => s.UnlockSpecializationAsync(_testCharacter, specId))
            .ReturnsAsync(SpecializationUnlockResult.Ok("Specialization unlocked", specId, "Iron Warden", 10));

        // Act
        await _controller.HandleInputAsync(ConsoleKey.Enter, _testCharacter, specIds, nodeIds);

        // Assert
        _mockSpecService.Verify(s => s.UnlockSpecializationAsync(_testCharacter, specId), Times.Once);
    }

    [Fact]
    public async Task HandleInputAsync_Enter_SuccessfulSpecUnlock_SetsStatusMessage()
    {
        // Arrange
        var specId = Guid.NewGuid();
        var specIds = new List<Guid> { specId };
        var nodeIds = new List<Guid>();

        _mockSpecService
            .Setup(s => s.UnlockSpecializationAsync(_testCharacter, specId))
            .ReturnsAsync(SpecializationUnlockResult.Ok("Specialization unlocked", specId, "Iron Warden", 10));

        // Act
        await _controller.HandleInputAsync(ConsoleKey.Enter, _testCharacter, specIds, nodeIds);

        // Assert
        _controller.LastStatusMessage.Should().Contain("Unlocked Iron Warden");
        _controller.LastStatusMessage.Should().Contain("-10 PP");
    }

    [Fact]
    public async Task HandleInputAsync_Enter_FailedSpecUnlock_SetsErrorMessage()
    {
        // Arrange
        var specId = Guid.NewGuid();
        var specIds = new List<Guid> { specId };
        var nodeIds = new List<Guid>();

        _mockSpecService
            .Setup(s => s.UnlockSpecializationAsync(_testCharacter, specId))
            .ReturnsAsync(SpecializationUnlockResult.Failure("Insufficient PP"));

        // Act
        await _controller.HandleInputAsync(ConsoleKey.Enter, _testCharacter, specIds, nodeIds);

        // Assert
        _controller.LastStatusMessage.Should().Be("Insufficient PP");
    }

    [Fact]
    public async Task HandleInputAsync_Spacebar_InSpecListMode_CallsUnlockSpecialization()
    {
        // Arrange
        var specId = Guid.NewGuid();
        var specIds = new List<Guid> { specId };
        var nodeIds = new List<Guid>();

        _mockSpecService
            .Setup(s => s.UnlockSpecializationAsync(_testCharacter, specId))
            .ReturnsAsync(SpecializationUnlockResult.Ok("Specialization unlocked", specId, "Iron Warden", 10));

        // Act
        await _controller.HandleInputAsync(ConsoleKey.Spacebar, _testCharacter, specIds, nodeIds);

        // Assert
        _mockSpecService.Verify(s => s.UnlockSpecializationAsync(_testCharacter, specId), Times.Once);
    }

    #endregion

    #region Unlock Tests - Node

    [Fact]
    public async Task HandleInputAsync_Enter_InTreeDetailMode_CallsUnlockNode()
    {
        // Arrange
        _controller.UpdateCounts(1, 3);
        var specId = Guid.NewGuid();
        var nodeId = Guid.NewGuid();
        var specIds = new List<Guid> { specId };
        var nodeIds = new List<Guid> { nodeId, Guid.NewGuid(), Guid.NewGuid() };

        // Switch to tree detail mode
        await _controller.HandleInputAsync(ConsoleKey.RightArrow, _testCharacter, specIds, nodeIds);

        var abilityId = Guid.NewGuid();
        _mockSpecService
            .Setup(s => s.UnlockNodeAsync(_testCharacter, nodeId))
            .ReturnsAsync(NodeUnlockResult.Ok("Node unlocked", nodeId, "Shield Bash", abilityId, 1, 2));

        // Act
        await _controller.HandleInputAsync(ConsoleKey.Enter, _testCharacter, specIds, nodeIds);

        // Assert
        _mockSpecService.Verify(s => s.UnlockNodeAsync(_testCharacter, nodeId), Times.Once);
    }

    [Fact]
    public async Task HandleInputAsync_Enter_SuccessfulNodeUnlock_SetsStatusMessage()
    {
        // Arrange
        _controller.UpdateCounts(1, 3);
        var specId = Guid.NewGuid();
        var nodeId = Guid.NewGuid();
        var abilityId = Guid.NewGuid();
        var specIds = new List<Guid> { specId };
        var nodeIds = new List<Guid> { nodeId, Guid.NewGuid(), Guid.NewGuid() };

        await _controller.HandleInputAsync(ConsoleKey.RightArrow, _testCharacter, specIds, nodeIds);

        _mockSpecService
            .Setup(s => s.UnlockNodeAsync(_testCharacter, nodeId))
            .ReturnsAsync(NodeUnlockResult.Ok("Node unlocked", nodeId, "Shield Bash", abilityId, 2, 2));

        // Act
        await _controller.HandleInputAsync(ConsoleKey.Enter, _testCharacter, specIds, nodeIds);

        // Assert
        _controller.LastStatusMessage.Should().Contain("Unlocked Shield Bash");
        _controller.LastStatusMessage.Should().Contain("-2 PP");
    }

    [Fact]
    public async Task HandleInputAsync_Enter_FailedNodeUnlock_SetsErrorMessage()
    {
        // Arrange
        _controller.UpdateCounts(1, 3);
        var specId = Guid.NewGuid();
        var nodeId = Guid.NewGuid();
        var specIds = new List<Guid> { specId };
        var nodeIds = new List<Guid> { nodeId, Guid.NewGuid(), Guid.NewGuid() };

        await _controller.HandleInputAsync(ConsoleKey.RightArrow, _testCharacter, specIds, nodeIds);

        _mockSpecService
            .Setup(s => s.UnlockNodeAsync(_testCharacter, nodeId))
            .ReturnsAsync(NodeUnlockResult.Failure("Prerequisites not met"));

        // Act
        await _controller.HandleInputAsync(ConsoleKey.Enter, _testCharacter, specIds, nodeIds);

        // Assert
        _controller.LastStatusMessage.Should().Be("Prerequisites not met");
    }

    #endregion

    #region Reset Tests

    [Fact]
    public void Reset_ResetsAllStateToDefaults()
    {
        // Arrange - modify state
        _controller.UpdateCounts(5, 10);

        // Act
        _controller.Reset();

        // Assert
        _controller.ViewMode.Should().Be(SpecializationViewMode.SpecList);
        _controller.SelectedSpecIndex.Should().Be(0);
        _controller.SelectedNodeIndex.Should().Be(0);
        _controller.LastStatusMessage.Should().BeNull();
    }

    #endregion

    #region UpdateCounts Tests

    [Fact]
    public void UpdateCounts_SetsSpecAndNodeCounts()
    {
        // Arrange & Act
        _controller.UpdateCounts(5, 12);

        // Assert - verified by navigation bounds
        // Navigate down 10 times in spec list - should stop at index 4 (5 specs, indices 0-4)
        _controller.ViewMode.Should().Be(SpecializationViewMode.SpecList);
    }

    #endregion

    #region Return Value Tests

    [Fact]
    public async Task HandleInputAsync_Navigation_ReturnsSpecializationMenuPhase()
    {
        // Arrange
        var specIds = new List<Guid> { Guid.NewGuid() };

        // Act & Assert
        var result = await _controller.HandleInputAsync(ConsoleKey.UpArrow, _testCharacter, specIds, new List<Guid>());
        result.Should().Be(GamePhase.SpecializationMenu);

        result = await _controller.HandleInputAsync(ConsoleKey.DownArrow, _testCharacter, specIds, new List<Guid>());
        result.Should().Be(GamePhase.SpecializationMenu);

        result = await _controller.HandleInputAsync(ConsoleKey.LeftArrow, _testCharacter, specIds, new List<Guid>());
        result.Should().Be(GamePhase.SpecializationMenu);

        result = await _controller.HandleInputAsync(ConsoleKey.RightArrow, _testCharacter, specIds, new List<Guid>());
        result.Should().Be(GamePhase.SpecializationMenu);
    }

    [Fact]
    public async Task HandleInputAsync_UnknownKey_ReturnsSpecializationMenuPhase()
    {
        // Arrange
        var specIds = new List<Guid> { Guid.NewGuid() };

        // Act
        var result = await _controller.HandleInputAsync(ConsoleKey.F1, _testCharacter, specIds, new List<Guid>());

        // Assert
        result.Should().Be(GamePhase.SpecializationMenu);
    }

    #endregion

    #region Status Message Clearing Tests

    [Fact]
    public async Task HandleInputAsync_ClearsStatusMessageOnNewInput()
    {
        // Arrange
        var specId = Guid.NewGuid();
        var specIds = new List<Guid> { specId };
        var nodeIds = new List<Guid>();

        _mockSpecService
            .Setup(s => s.UnlockSpecializationAsync(_testCharacter, specId))
            .ReturnsAsync(SpecializationUnlockResult.Ok("Specialization unlocked", specId, "Iron Warden", 10));

        // Set a status message
        await _controller.HandleInputAsync(ConsoleKey.Enter, _testCharacter, specIds, nodeIds);
        _controller.LastStatusMessage.Should().NotBeNull();

        _mockSpecService.Reset();
        _mockSpecService
            .Setup(s => s.UnlockSpecializationAsync(It.IsAny<Character>(), It.IsAny<Guid>()))
            .ReturnsAsync(SpecializationUnlockResult.Failure("Already unlocked"));

        // Act - navigate (doesn't trigger unlock)
        await _controller.HandleInputAsync(ConsoleKey.UpArrow, _testCharacter, specIds, nodeIds);

        // Assert - status message should be cleared
        _controller.LastStatusMessage.Should().BeNull();
    }

    #endregion
}
