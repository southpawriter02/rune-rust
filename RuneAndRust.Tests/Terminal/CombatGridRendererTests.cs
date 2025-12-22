using FluentAssertions;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.ViewModels;
using RuneAndRust.Terminal.Rendering;
using Xunit;

namespace RuneAndRust.Tests.Terminal;

/// <summary>
/// Tests for the CombatGridRenderer introduced in v0.3.6a.
/// Validates battlefield grid rendering and combatant formatting.
/// </summary>
public class CombatGridRendererTests
{
    #region Render Tests

    [Fact]
    public void Render_ReturnsPanel()
    {
        // Arrange
        var vm = CreateTestViewModel();

        // Act
        var result = CombatGridRenderer.Render(vm);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void Render_WithEmptyRows_ReturnsPanel()
    {
        // Arrange
        var vm = new CombatViewModel(
            RoundNumber: 1,
            ActiveCombatantName: "Test",
            TurnOrder: new List<CombatantView>(),
            CombatLog: new List<string>(),
            PlayerStats: CreateTestPlayerStats(),
            PlayerAbilities: null,
            PlayerFrontRow: new List<CombatantView>(),
            PlayerBackRow: new List<CombatantView>(),
            EnemyFrontRow: new List<CombatantView>(),
            EnemyBackRow: new List<CombatantView>()
        );

        // Act
        var result = CombatGridRenderer.Render(vm);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void Render_WithNullRows_ReturnsPanel()
    {
        // Arrange
        var vm = new CombatViewModel(
            RoundNumber: 1,
            ActiveCombatantName: "Test",
            TurnOrder: new List<CombatantView>(),
            CombatLog: new List<string>(),
            PlayerStats: CreateTestPlayerStats()
        );

        // Act
        var result = CombatGridRenderer.Render(vm);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region FormatCombatant Tests

    [Fact]
    public void FormatCombatant_Enemy_UsesRedColor()
    {
        // Arrange
        var combatant = CreateTestCombatantView(isPlayer: false, isActive: false);

        // Act
        var result = CombatGridRenderer.FormatCombatant(combatant, isEnemy: true);

        // Assert
        result.Should().Contain("[red]");
        result.Should().Contain("[/]");
    }

    [Fact]
    public void FormatCombatant_Player_UsesCyanColor()
    {
        // Arrange
        var combatant = CreateTestCombatantView(isPlayer: true, isActive: false);

        // Act
        var result = CombatGridRenderer.FormatCombatant(combatant, isEnemy: false);

        // Assert
        result.Should().Contain("[cyan]");
        result.Should().Contain("[/]");
    }

    [Fact]
    public void FormatCombatant_ActiveCombatant_ShowsMarker()
    {
        // Arrange
        var combatant = CreateTestCombatantView(isPlayer: false, isActive: true);

        // Act
        var result = CombatGridRenderer.FormatCombatant(combatant, isEnemy: true);

        // Assert
        result.Should().Contain("[bold yellow]>[/]");
    }

    [Fact]
    public void FormatCombatant_InactiveCombatant_NoMarker()
    {
        // Arrange
        var combatant = CreateTestCombatantView(isPlayer: false, isActive: false);

        // Act
        var result = CombatGridRenderer.FormatCombatant(combatant, isEnemy: true);

        // Assert
        result.Should().NotContain("[bold yellow]>[/]");
        result.Should().StartWith(" "); // Space instead of marker
    }

    [Fact]
    public void FormatCombatant_CriticalHealth_ShowsTripleExclamation()
    {
        // Arrange
        var combatant = new CombatantView(
            Id: Guid.NewGuid(),
            Name: "Wounded Enemy",
            IsPlayer: false,
            IsActive: false,
            HealthStatus: "[red]Critical[/]",
            StatusEffects: "[ ]",
            InitiativeDisplay: "10",
            Row: RowPosition.Front,
            IsTargeted: false
        );

        // Act
        var result = CombatGridRenderer.FormatCombatant(combatant, isEnemy: true);

        // Assert
        result.Should().Contain("[red]!!![/]");
    }

    [Fact]
    public void FormatCombatant_WoundedHealth_ShowsDoubleExclamation()
    {
        // Arrange
        var combatant = new CombatantView(
            Id: Guid.NewGuid(),
            Name: "Wounded Enemy",
            IsPlayer: false,
            IsActive: false,
            HealthStatus: "[yellow]Wounded[/]",
            StatusEffects: "[ ]",
            InitiativeDisplay: "10",
            Row: RowPosition.Front,
            IsTargeted: false
        );

        // Act
        var result = CombatGridRenderer.FormatCombatant(combatant, isEnemy: true);

        // Assert
        result.Should().Contain("[yellow]!![/]");
    }

    [Fact]
    public void FormatCombatant_TargetedCombatant_ShowsAsterisk()
    {
        // Arrange
        var combatant = new CombatantView(
            Id: Guid.NewGuid(),
            Name: "Target",
            IsPlayer: false,
            IsActive: false,
            HealthStatus: "[green]Healthy[/]",
            StatusEffects: "[ ]",
            InitiativeDisplay: "10",
            Row: RowPosition.Front,
            IsTargeted: true
        );

        // Act
        var result = CombatGridRenderer.FormatCombatant(combatant, isEnemy: true);

        // Assert
        result.Should().Contain("[bold white]*[/]");
    }

    [Fact]
    public void FormatCombatant_NameWithBrackets_EscapesMarkup()
    {
        // Arrange
        var combatant = new CombatantView(
            Id: Guid.NewGuid(),
            Name: "Test [Elite] Boss",
            IsPlayer: false,
            IsActive: false,
            HealthStatus: "[green]Healthy[/]",
            StatusEffects: "[ ]",
            InitiativeDisplay: "10",
            Row: RowPosition.Front,
            IsTargeted: false
        );

        // Act
        var result = CombatGridRenderer.FormatCombatant(combatant, isEnemy: true);

        // Assert
        // The name should have escaped brackets
        result.Should().Contain("[[Elite]]");
    }

    #endregion

    #region Helper Methods

    private static CombatViewModel CreateTestViewModel()
    {
        return new CombatViewModel(
            RoundNumber: 1,
            ActiveCombatantName: "Hero",
            TurnOrder: new List<CombatantView>
            {
                CreateTestCombatantView(isPlayer: true, isActive: true),
                CreateTestCombatantView(isPlayer: false, isActive: false)
            },
            CombatLog: new List<string> { "Combat begins!" },
            PlayerStats: CreateTestPlayerStats(),
            PlayerAbilities: null,
            PlayerFrontRow: new List<CombatantView>
            {
                CreateTestCombatantView(isPlayer: true, isActive: true)
            },
            PlayerBackRow: new List<CombatantView>(),
            EnemyFrontRow: new List<CombatantView>
            {
                CreateTestCombatantView(isPlayer: false, isActive: false)
            },
            EnemyBackRow: new List<CombatantView>()
        );
    }

    private static CombatantView CreateTestCombatantView(bool isPlayer, bool isActive)
    {
        return new CombatantView(
            Id: Guid.NewGuid(),
            Name: isPlayer ? "Hero" : "Goblin",
            IsPlayer: isPlayer,
            IsActive: isActive,
            HealthStatus: isPlayer ? "100/100" : "[green]Healthy[/]",
            StatusEffects: "[ ]",
            InitiativeDisplay: "15",
            Row: RowPosition.Front,
            IsTargeted: false
        );
    }

    private static PlayerStatsView CreateTestPlayerStats()
    {
        return new PlayerStatsView(
            CurrentHp: 100,
            MaxHp: 100,
            CurrentStamina: 50,
            MaxStamina: 50,
            CurrentStress: 0,
            MaxStress: 100,
            CurrentCorruption: 0,
            MaxCorruption: 100
        );
    }

    #endregion
}
