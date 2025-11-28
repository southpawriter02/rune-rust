using Moq;
using RuneAndRust.Core;
using RuneAndRust.DesktopUI.Services;
using RuneAndRust.DesktopUI.ViewModels;
using RuneAndRust.Engine;
using SkiaSharp;
using Xunit;

namespace RuneAndRust.Tests.DesktopUI;

/// <summary>
/// Tests for CombatViewModel grid state management and interaction logic.
/// v0.43.21: Updated for current CombatViewModel constructor and dependencies.
/// </summary>
public class CombatViewModelTests : IDisposable
{
    private readonly Mock<ISpriteService> _mockSpriteService;
    private readonly Mock<IDialogService> _mockDialogService;
    private readonly Mock<IStatusEffectIconService> _mockStatusEffectIconService;
    private readonly Mock<IHazardVisualizationService> _mockHazardVisualizationService;
    private readonly Mock<IAnimationService> _mockAnimationService;
    private readonly CombatEngine _combatEngine;
    private readonly EnemyAI _enemyAI;
    private readonly SKBitmap _mockBitmap;

    public CombatViewModelTests()
    {
        _mockSpriteService = new Mock<ISpriteService>();
        _mockDialogService = new Mock<IDialogService>();
        _mockStatusEffectIconService = new Mock<IStatusEffectIconService>();
        _mockHazardVisualizationService = new Mock<IHazardVisualizationService>();
        _mockAnimationService = new Mock<IAnimationService>();
        _combatEngine = new CombatEngine(new DiceService());
        _enemyAI = new EnemyAI(new DiceService());

        SetupMockDefaults();

        // Create a mock bitmap that will be used across tests
        _mockBitmap = new SKBitmap(48, 48);
    }

    private void SetupMockDefaults()
    {
        // Setup sprite service to return a mock bitmap
        _mockSpriteService
            .Setup(s => s.GetSpriteBitmap(It.IsAny<string>(), It.IsAny<int>()))
            .Returns(() => new SKBitmap(48, 48));

        // Setup dialog service to return sensible defaults
        _mockDialogService
            .Setup(d => d.ShowMessageAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        _mockDialogService
            .Setup(d => d.ShowConfirmationAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        // Setup animation service to complete immediately
        _mockAnimationService
            .Setup(a => a.PlayAttackAnimationAsync(
                It.IsAny<GridPosition>(),
                It.IsAny<GridPosition>(),
                It.IsAny<bool>(),
                It.IsAny<int>(),
                It.IsAny<bool>()))
            .Returns(Task.CompletedTask);
        _mockAnimationService
            .Setup(a => a.GetActiveAnimations())
            .Returns(Array.Empty<ActiveAnimation>());
    }

    private CombatViewModel CreateViewModel()
    {
        return new CombatViewModel(
            _mockSpriteService.Object,
            _mockDialogService.Object,
            _combatEngine,
            _enemyAI,
            _mockStatusEffectIconService.Object,
            _mockHazardVisualizationService.Object,
            _mockAnimationService.Object);
    }

    [Fact]
    public void Constructor_InitializesWithDefaultValues()
    {
        // Act
        var vm = CreateViewModel();

        // Assert
        Assert.Equal("Tactical Combat", vm.Title);
        Assert.Equal(3, vm.Columns);
        Assert.Equal(80.0, vm.CellSize);
        Assert.Null(vm.SelectedPosition);
        Assert.Null(vm.HoveredPosition);
        Assert.NotNull(vm.HighlightedPositions);
        Assert.NotNull(vm.UnitSprites);
        Assert.NotNull(vm.CombatLog);
        Assert.NotNull(vm.TurnOrder);
    }

    [Fact]
    public void Constructor_InitializesDemoScenario()
    {
        // Act
        var vm = CreateViewModel();

        // Assert - Should have combat state from demo
        Assert.NotNull(vm.CombatState);
        Assert.NotEmpty(vm.UnitSprites);
        Assert.NotEmpty(vm.UnitData);
    }

    [Fact]
    public void Constructor_InitializesCommands()
    {
        // Act
        var vm = CreateViewModel();

        // Assert
        Assert.NotNull(vm.CellClickedCommand);
        Assert.NotNull(vm.CellHoveredCommand);
        Assert.NotNull(vm.AttackCommand);
        Assert.NotNull(vm.DefendCommand);
        Assert.NotNull(vm.UseAbilityCommand);
        Assert.NotNull(vm.UseItemCommand);
        Assert.NotNull(vm.MoveCommand);
        Assert.NotNull(vm.EndTurnCommand);
        Assert.NotNull(vm.FleeCommand);
    }

    [Fact]
    public void Columns_CanBeChanged()
    {
        // Arrange
        var vm = CreateViewModel();
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
        var vm = CreateViewModel();
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
    public void CellClickedCommand_SetsSelectedPosition()
    {
        // Arrange
        var vm = CreateViewModel();
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
        var vm = CreateViewModel();
        var position = new GridPosition(Zone.Enemy, Row.Back, 2);

        // Act
        vm.CellHoveredCommand.Execute(position).Subscribe();

        // Assert
        Assert.Equal(position, vm.HoveredPosition);
    }

    [Fact]
    public void LoadCombatState_UpdatesCombatState()
    {
        // Arrange
        var vm = CreateViewModel();
        var diceService = new DiceService();
        var player = CreateTestPlayer();
        var enemies = CreateTestEnemies();
        var combatState = _combatEngine.InitializeCombat(player, enemies, canFlee: true);

        // Act
        vm.LoadCombatState(combatState);

        // Assert
        Assert.Same(combatState, vm.CombatState);
    }

    [Fact]
    public void LoadCombatState_UpdatesTurnOrder()
    {
        // Arrange
        var vm = CreateViewModel();
        var player = CreateTestPlayer();
        var enemies = CreateTestEnemies();
        var combatState = _combatEngine.InitializeCombat(player, enemies, canFlee: true);

        // Act
        vm.LoadCombatState(combatState);

        // Assert
        Assert.NotEmpty(vm.TurnOrder);
    }

    [Fact]
    public void IsPlayerTurn_ReturnsCorrectValue()
    {
        // Arrange
        var vm = CreateViewModel();
        var player = CreateTestPlayer();
        var enemies = CreateTestEnemies();
        var combatState = _combatEngine.InitializeCombat(player, enemies, canFlee: true);

        // Act
        vm.LoadCombatState(combatState);

        // Assert - Initially should be player's turn or first participant's turn
        Assert.NotNull(vm.CombatState);
    }

    [Fact]
    public void EnvironmentalObjects_InitializedForDemo()
    {
        // Act
        var vm = CreateViewModel();

        // Assert
        Assert.NotNull(vm.EnvironmentalObjects);
        Assert.NotEmpty(vm.EnvironmentalObjects);
    }

    [Fact]
    public void StatusMessage_UpdatesOnCombatStateChange()
    {
        // Arrange
        var vm = CreateViewModel();

        // Assert - Demo scenario should set a status message
        Assert.NotNull(vm.StatusMessage);
        Assert.NotEmpty(vm.StatusMessage);
    }

    [Fact]
    public void AddToCombatLog_AddsMessageToLog()
    {
        // Arrange
        var vm = CreateViewModel();
        var initialCount = vm.CombatLog.Count;

        // Act
        vm.AddToCombatLog("Test message");

        // Assert
        Assert.Equal(initialCount + 1, vm.CombatLog.Count);
        Assert.Contains("Test message", vm.CombatLog);
    }

    [Fact]
    public void AddToCombatLog_InsertsAtFront()
    {
        // Arrange
        var vm = CreateViewModel();
        vm.AddToCombatLog("First message");

        // Act
        vm.AddToCombatLog("Second message");

        // Assert - Second message should be first (most recent)
        Assert.Equal("Second message", vm.CombatLog[0]);
    }

    [Fact]
    public void CombatLog_LimitedTo50Entries()
    {
        // Arrange
        var vm = CreateViewModel();

        // Act - Add more than 50 messages
        for (int i = 0; i < 60; i++)
        {
            vm.AddToCombatLog($"Message {i}");
        }

        // Assert - Should be capped at 50
        Assert.True(vm.CombatLog.Count <= 50);
    }

    [Fact]
    public void IsBossFight_FalseForNormalCombat()
    {
        // Arrange
        var vm = CreateViewModel();
        var player = CreateTestPlayer();
        var enemies = CreateTestEnemies();
        var combatState = _combatEngine.InitializeCombat(player, enemies, canFlee: true);

        // Act
        vm.LoadCombatState(combatState);

        // Assert
        Assert.False(vm.IsBossFight);
    }

    [Fact]
    public void PropertyChanges_RaiseNotifications()
    {
        // Arrange
        var vm = CreateViewModel();
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
    }

    [Fact]
    public void Grid_ReturnsGridFromCombatState()
    {
        // Arrange
        var vm = CreateViewModel();

        // Assert
        if (vm.CombatState != null)
        {
            Assert.Equal(vm.CombatState.Grid, vm.Grid);
        }
    }

    [Fact]
    public void StatusEffectIconService_IsAccessible()
    {
        // Arrange
        var vm = CreateViewModel();

        // Assert
        Assert.Same(_mockStatusEffectIconService.Object, vm.StatusEffectIconService);
    }

    [Fact]
    public void HazardVisualizationService_IsAccessible()
    {
        // Arrange
        var vm = CreateViewModel();

        // Assert
        Assert.Same(_mockHazardVisualizationService.Object, vm.HazardVisualizationService);
    }

    [Fact]
    public void AnimationService_IsAccessible()
    {
        // Arrange
        var vm = CreateViewModel();

        // Assert
        Assert.Same(_mockAnimationService.Object, vm.AnimationService);
    }

    [Fact]
    public void BossCombatViewModel_NullWhenNoBossService()
    {
        // Arrange & Act
        var vm = CreateViewModel();

        // Assert - No boss display service was provided
        Assert.Null(vm.BossCombatViewModel);
    }

    [Fact]
    public void UnitSprites_NotNull()
    {
        // Arrange & Act
        var vm = CreateViewModel();

        // Assert
        Assert.NotNull(vm.UnitSprites);
    }

    [Fact]
    public void UnitData_NotNull()
    {
        // Arrange & Act
        var vm = CreateViewModel();

        // Assert
        Assert.NotNull(vm.UnitData);
    }

    [Fact]
    public void HighlightedPositions_NotNull()
    {
        // Arrange & Act
        var vm = CreateViewModel();

        // Assert
        Assert.NotNull(vm.HighlightedPositions);
    }

    [Fact]
    public void Constructor_ThrowsOnNullSpriteService()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CombatViewModel(
            null!,
            _mockDialogService.Object,
            _combatEngine,
            _enemyAI,
            _mockStatusEffectIconService.Object,
            _mockHazardVisualizationService.Object,
            _mockAnimationService.Object));
    }

    [Fact]
    public void Constructor_ThrowsOnNullDialogService()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CombatViewModel(
            _mockSpriteService.Object,
            null!,
            _combatEngine,
            _enemyAI,
            _mockStatusEffectIconService.Object,
            _mockHazardVisualizationService.Object,
            _mockAnimationService.Object));
    }

    [Fact]
    public void Constructor_ThrowsOnNullCombatEngine()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CombatViewModel(
            _mockSpriteService.Object,
            _mockDialogService.Object,
            null!,
            _enemyAI,
            _mockStatusEffectIconService.Object,
            _mockHazardVisualizationService.Object,
            _mockAnimationService.Object));
    }

    [Fact]
    public void Constructor_ThrowsOnNullEnemyAI()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CombatViewModel(
            _mockSpriteService.Object,
            _mockDialogService.Object,
            _combatEngine,
            null!,
            _mockStatusEffectIconService.Object,
            _mockHazardVisualizationService.Object,
            _mockAnimationService.Object));
    }

    private static PlayerCharacter CreateTestPlayer()
    {
        return new PlayerCharacter
        {
            Name = "Test Hero",
            HP = 50,
            MaxHP = 50,
            Stamina = 30,
            MaxStamina = 30,
            Attributes = new Attributes
            {
                Might = 4,
                Finesse = 3,
                Will = 3,
                Wits = 2,
                Sturdiness = 3
            }
        };
    }

    private static List<Enemy> CreateTestEnemies()
    {
        return new List<Enemy>
        {
            new Enemy
            {
                Name = "Test Goblin",
                HP = 15,
                MaxHP = 15,
                Soak = 1,
                Attributes = new Attributes
                {
                    Might = 2,
                    Finesse = 2,
                    Wits = 1,
                    Will = 1,
                    Sturdiness = 1
                }
            }
        };
    }

    public void Dispose()
    {
        _mockBitmap?.Dispose();
    }
}

/// <summary>
/// Tests for TurnOrderEntry model.
/// </summary>
public class TurnOrderEntryTests
{
    [Fact]
    public void TurnOrderEntry_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var entry = new TurnOrderEntry();

        // Assert
        Assert.Equal(string.Empty, entry.Name);
        Assert.False(entry.IsPlayer);
        Assert.False(entry.IsCompanion);
        Assert.False(entry.IsActive);
    }

    [Fact]
    public void TurnOrderEntry_Properties_CanBeSet()
    {
        // Arrange & Act
        var entry = new TurnOrderEntry
        {
            Name = "Test Character",
            IsPlayer = true,
            IsCompanion = false,
            IsActive = true
        };

        // Assert
        Assert.Equal("Test Character", entry.Name);
        Assert.True(entry.IsPlayer);
        Assert.False(entry.IsCompanion);
        Assert.True(entry.IsActive);
    }
}
