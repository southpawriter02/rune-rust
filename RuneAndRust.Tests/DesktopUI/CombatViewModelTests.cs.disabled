using Moq;
using RuneAndRust.Core;
using RuneAndRust.DesktopUI.Services;
using RuneAndRust.DesktopUI.ViewModels;
using SkiaSharp;
using Xunit;

namespace RuneAndRust.Tests.DesktopUI;

/// <summary>
/// Tests for CombatViewModel grid state management and interaction logic.
/// </summary>
public class CombatViewModelTests
{
    private readonly Mock<ISpriteService> _mockSpriteService;

    public CombatViewModelTests()
    {
        _mockSpriteService = new Mock<ISpriteService>();
        SetupMockSprites();
    }

    private void SetupMockSprites()
    {
        // Create mock bitmap for testing
        var mockBitmap = new SKBitmap(48, 48);

        _mockSpriteService
            .Setup(s => s.GetSpriteBitmap(It.IsAny<string>(), It.IsAny<int>()))
            .Returns(mockBitmap);
    }

    [Fact]
    public void Constructor_InitializesWithDefaultValues()
    {
        // Act
        var vm = new CombatViewModel(_mockSpriteService.Object);

        // Assert
        Assert.Equal("Tactical Combat", vm.Title);
        Assert.Equal(3, vm.Columns);
        Assert.Equal(80.0, vm.CellSize);
        Assert.Null(vm.SelectedPosition);
        Assert.Null(vm.HoveredPosition);
        Assert.NotNull(vm.HighlightedPositions);
        Assert.Empty(vm.HighlightedPositions);
        Assert.NotNull(vm.UnitSprites);
        Assert.Equal(6, vm.UnitSprites.Count); // Demo scenario has 6 units
        Assert.Equal("Select a unit to begin combat", vm.StatusMessage);
    }

    [Fact]
    public void InitializeDemoScenario_LoadsCorrectUnits()
    {
        // Act
        var vm = new CombatViewModel(_mockSpriteService.Object);

        // Assert - Player units
        Assert.True(vm.UnitSprites.ContainsKey(new GridPosition(Zone.Player, Row.Front, 0)));
        Assert.True(vm.UnitSprites.ContainsKey(new GridPosition(Zone.Player, Row.Front, 2)));
        Assert.True(vm.UnitSprites.ContainsKey(new GridPosition(Zone.Player, Row.Back, 1)));

        // Assert - Enemy units
        Assert.True(vm.UnitSprites.ContainsKey(new GridPosition(Zone.Enemy, Row.Front, 0)));
        Assert.True(vm.UnitSprites.ContainsKey(new GridPosition(Zone.Enemy, Row.Front, 1)));
        Assert.True(vm.UnitSprites.ContainsKey(new GridPosition(Zone.Enemy, Row.Back, 2)));

        // Verify sprite service was called for each unit
        _mockSpriteService.Verify(s => s.GetSpriteBitmap("warrior", 3), Times.Exactly(2));
        _mockSpriteService.Verify(s => s.GetSpriteBitmap("blessed", 3), Times.Once);
        _mockSpriteService.Verify(s => s.GetSpriteBitmap("goblin", 3), Times.Exactly(3));
    }

    [Fact]
    public void SelectedPosition_WhenSet_UpdatesHighlightedPositions()
    {
        // Arrange
        var vm = new CombatViewModel(_mockSpriteService.Object);
        var position = new GridPosition(Zone.Player, Row.Front, 1);

        // Act
        vm.SelectedPosition = position;

        // Assert
        Assert.Equal(position, vm.SelectedPosition);
        Assert.NotEmpty(vm.HighlightedPositions);

        // Should highlight adjacent columns
        Assert.Contains(new GridPosition(Zone.Player, Row.Front, 0), vm.HighlightedPositions);
        Assert.Contains(new GridPosition(Zone.Player, Row.Front, 2), vm.HighlightedPositions);

        // Should highlight opposite row in same zone
        Assert.Contains(new GridPosition(Zone.Player, Row.Back, 1), vm.HighlightedPositions);

        // Should highlight enemy front row at same column
        Assert.Contains(new GridPosition(Zone.Enemy, Row.Front, 1), vm.HighlightedPositions);
    }

    [Fact]
    public void SelectedPosition_FrontRowPlayer_HighlightsCorrectAttackTargets()
    {
        // Arrange
        var vm = new CombatViewModel(_mockSpriteService.Object);
        var frontPosition = new GridPosition(Zone.Player, Row.Front, 0);

        // Act
        vm.SelectedPosition = frontPosition;

        // Assert - Can attack enemy front row at same column
        Assert.Contains(new GridPosition(Zone.Enemy, Row.Front, 0), vm.HighlightedPositions);
    }

    [Fact]
    public void SelectedPosition_BackRowPlayer_DoesNotHighlightEnemyAttacks()
    {
        // Arrange
        var vm = new CombatViewModel(_mockSpriteService.Object);
        var backPosition = new GridPosition(Zone.Player, Row.Back, 1);

        // Act
        vm.SelectedPosition = backPosition;

        // Assert - Back row cannot attack (no enemy positions highlighted)
        Assert.DoesNotContain(new GridPosition(Zone.Enemy, Row.Front, 1), vm.HighlightedPositions);
        Assert.DoesNotContain(new GridPosition(Zone.Enemy, Row.Back, 1), vm.HighlightedPositions);
    }

    [Fact]
    public void SelectedPosition_EdgeColumn_OnlyHighlightsValidAdjacentColumns()
    {
        // Arrange
        var vm = new CombatViewModel(_mockSpriteService.Object);
        var leftEdge = new GridPosition(Zone.Player, Row.Front, 0);

        // Act
        vm.SelectedPosition = leftEdge;

        // Assert - Should only highlight column 1 (not -1)
        Assert.Contains(new GridPosition(Zone.Player, Row.Front, 1), vm.HighlightedPositions);
        Assert.DoesNotContain(new GridPosition(Zone.Player, Row.Front, -1), vm.HighlightedPositions);
    }

    [Fact]
    public void SelectedPosition_OccupiedCell_UpdatesStatusMessage()
    {
        // Arrange
        var vm = new CombatViewModel(_mockSpriteService.Object);
        var occupiedPosition = new GridPosition(Zone.Player, Row.Front, 0);

        // Act
        vm.SelectedPosition = occupiedPosition;

        // Assert
        Assert.Contains("Selected: Player/Front/Col0", vm.StatusMessage);
        Assert.Contains("Green cells show valid actions", vm.StatusMessage);
    }

    [Fact]
    public void SelectedPosition_EmptyCell_UpdatesStatusMessage()
    {
        // Arrange
        var vm = new CombatViewModel(_mockSpriteService.Object);
        var emptyPosition = new GridPosition(Zone.Player, Row.Front, 1);

        // Act
        vm.SelectedPosition = emptyPosition;

        // Assert
        Assert.Contains("Selected empty cell", vm.StatusMessage);
        Assert.Contains("Player/Front/Col1", vm.StatusMessage);
    }

    [Fact]
    public void SelectedPosition_Null_ClearsHighlightedPositions()
    {
        // Arrange
        var vm = new CombatViewModel(_mockSpriteService.Object);
        vm.SelectedPosition = new GridPosition(Zone.Player, Row.Front, 0);
        Assert.NotEmpty(vm.HighlightedPositions);

        // Act
        vm.SelectedPosition = null;

        // Assert
        Assert.Empty(vm.HighlightedPositions);
        Assert.Equal("Select a unit to begin combat", vm.StatusMessage);
    }

    [Fact]
    public void CellClickedCommand_SetsSelectedPosition()
    {
        // Arrange
        var vm = new CombatViewModel(_mockSpriteService.Object);
        var position = new GridPosition(Zone.Enemy, Row.Front, 1);

        // Act
        vm.CellClickedCommand.Execute(position).Subscribe();

        // Assert
        Assert.Equal(position, vm.SelectedPosition);
    }

    [Fact]
    public void CellHoveredCommand_SetsHoveredPosition()
    {
        // Arrange
        var vm = new CombatViewModel(_mockSpriteService.Object);
        var position = new GridPosition(Zone.Enemy, Row.Back, 2);

        // Act
        vm.CellHoveredCommand.Execute(position).Subscribe();

        // Assert
        Assert.Equal(position, vm.HoveredPosition);
    }

    [Fact]
    public void Columns_CanBeChanged()
    {
        // Arrange
        var vm = new CombatViewModel(_mockSpriteService.Object);
        int? changedValue = null;
        vm.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(vm.Columns))
                changedValue = vm.Columns;
        };

        // Act
        vm.Columns = 5;

        // Assert
        Assert.Equal(5, vm.Columns);
        Assert.Equal(5, changedValue);
    }

    [Fact]
    public void CellSize_CanBeChanged()
    {
        // Arrange
        var vm = new CombatViewModel(_mockSpriteService.Object);
        double? changedValue = null;
        vm.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(vm.CellSize))
                changedValue = vm.CellSize;
        };

        // Act
        vm.CellSize = 100.0;

        // Assert
        Assert.Equal(100.0, vm.CellSize);
        Assert.Equal(100.0, changedValue);
    }

    [Fact]
    public void PropertyChanges_RaiseNotifications()
    {
        // Arrange
        var vm = new CombatViewModel(_mockSpriteService.Object);
        var changedProperties = new List<string>();
        vm.PropertyChanged += (s, e) => changedProperties.Add(e.PropertyName!);

        // Act
        vm.SelectedPosition = new GridPosition(Zone.Player, Row.Front, 0);
        vm.HoveredPosition = new GridPosition(Zone.Player, Row.Front, 1);
        vm.Columns = 4;
        vm.CellSize = 90.0;

        // Assert
        Assert.Contains(nameof(vm.SelectedPosition), changedProperties);
        Assert.Contains(nameof(vm.HoveredPosition), changedProperties);
        Assert.Contains(nameof(vm.Columns), changedProperties);
        Assert.Contains(nameof(vm.CellSize), changedProperties);
        Assert.Contains(nameof(vm.HighlightedPositions), changedProperties);
        Assert.Contains(nameof(vm.StatusMessage), changedProperties);
    }

    [Fact]
    public void Commands_AreNotNull()
    {
        // Arrange & Act
        var vm = new CombatViewModel(_mockSpriteService.Object);

        // Assert
        Assert.NotNull(vm.CellClickedCommand);
        Assert.NotNull(vm.CellHoveredCommand);
    }
}
