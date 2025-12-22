using FluentAssertions;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.ViewModels;
using RuneAndRust.Terminal.Rendering;
using Xunit;

namespace RuneAndRust.Tests.Terminal;

/// <summary>
/// Tests for the intent icon display in CombatGridRenderer introduced in v0.3.6c.
/// Validates intent and status icon formatting for combatants.
/// </summary>
public class CombatGridRendererIntentTests
{
    #region Intent Icon Tests

    [Fact]
    public void FormatCombatant_Enemy_WithIntentIcon_ShowsIcon()
    {
        // Arrange
        var combatant = CreateCombatantView(
            isPlayer: false,
            isEnemy: true,
            intentIcon: "⚔️"
        );

        // Act
        var result = CombatGridRenderer.FormatCombatant(combatant, isEnemy: true);

        // Assert
        result.Should().Contain("⚔️");
    }

    [Fact]
    public void FormatCombatant_Enemy_WithQuestionMarkIntent_ShowsQuestionMark()
    {
        // Arrange
        var combatant = CreateCombatantView(
            isPlayer: false,
            isEnemy: true,
            intentIcon: "?"
        );

        // Act
        var result = CombatGridRenderer.FormatCombatant(combatant, isEnemy: true);

        // Assert
        result.Should().Contain("?");
    }

    [Fact]
    public void FormatCombatant_Player_NoIntentIcon()
    {
        // Arrange
        var combatant = CreateCombatantView(
            isPlayer: true,
            isEnemy: false,
            intentIcon: null
        );

        // Act
        var result = CombatGridRenderer.FormatCombatant(combatant, isEnemy: false);

        // Assert
        result.Should().NotContain("⚔️");
        result.Should().NotContain("🛡️");
        result.Should().NotContain("💨");
        result.Should().NotContain("💤");
    }

    [Fact]
    public void FormatCombatant_Enemy_WithDefendIntent_ShowsShieldIcon()
    {
        // Arrange
        var combatant = CreateCombatantView(
            isPlayer: false,
            isEnemy: true,
            intentIcon: "🛡️"
        );

        // Act
        var result = CombatGridRenderer.FormatCombatant(combatant, isEnemy: true);

        // Assert
        result.Should().Contain("🛡️");
    }

    #endregion

    #region Status Icon Tests

    [Fact]
    public void FormatCombatant_WithStatusIcons_ShowsIcons()
    {
        // Arrange
        var combatant = CreateCombatantView(
            isPlayer: false,
            isEnemy: true,
            statusIcons: "🩸 🤢"
        );

        // Act
        var result = CombatGridRenderer.FormatCombatant(combatant, isEnemy: true);

        // Assert
        result.Should().Contain("🩸");
        result.Should().Contain("🤢");
    }

    [Fact]
    public void FormatCombatant_WithStackedStatus_ShowsStackCount()
    {
        // Arrange
        var combatant = CreateCombatantView(
            isPlayer: false,
            isEnemy: true,
            statusIcons: "🩸×3"
        );

        // Act
        var result = CombatGridRenderer.FormatCombatant(combatant, isEnemy: true);

        // Assert
        result.Should().Contain("🩸×3");
    }

    [Fact]
    public void FormatCombatant_NoStatusIcons_DoesNotAddExtraSpace()
    {
        // Arrange
        var combatant = CreateCombatantView(
            isPlayer: false,
            isEnemy: true,
            statusIcons: null,
            intentIcon: "⚔️"
        );

        // Act
        var result = CombatGridRenderer.FormatCombatant(combatant, isEnemy: true);

        // Assert - Should contain intent but no status section
        result.Should().Contain("⚔️");
        result.Should().NotContain("🩸");
    }

    #endregion

    #region Combined Display Tests

    [Fact]
    public void FormatCombatant_Enemy_WithIntentAndStatus_ShowsBoth()
    {
        // Arrange
        var combatant = CreateCombatantView(
            isPlayer: false,
            isEnemy: true,
            intentIcon: "⚔️",
            statusIcons: "🩸"
        );

        // Act
        var result = CombatGridRenderer.FormatCombatant(combatant, isEnemy: true);

        // Assert
        result.Should().Contain("⚔️");
        result.Should().Contain("🩸");
    }

    [Fact]
    public void FormatCombatant_ActiveEnemy_ShowsActiveMarker()
    {
        // Arrange
        var combatant = CreateCombatantView(
            isPlayer: false,
            isEnemy: true,
            isActive: true,
            intentIcon: "⚔️"
        );

        // Act
        var result = CombatGridRenderer.FormatCombatant(combatant, isEnemy: true);

        // Assert
        result.Should().Contain("[bold yellow]>[/]");
    }

    [Fact]
    public void FormatCombatant_TargetedEnemy_ShowsTargetIndicator()
    {
        // Arrange
        var combatant = CreateCombatantView(
            isPlayer: false,
            isEnemy: true,
            isTargeted: true,
            intentIcon: "⚔️"
        );

        // Act
        var result = CombatGridRenderer.FormatCombatant(combatant, isEnemy: true);

        // Assert
        result.Should().Contain("[bold white]*[/]");
    }

    #endregion

    #region Helper Methods

    private static CombatantView CreateCombatantView(
        bool isPlayer,
        bool isEnemy,
        string? intentIcon = null,
        string? statusIcons = null,
        bool isActive = false,
        bool isTargeted = false,
        string healthStatus = "Healthy")
    {
        return new CombatantView(
            Id: Guid.NewGuid(),
            Name: isPlayer ? "Test Hero" : "Test Enemy",
            IsPlayer: isPlayer,
            IsActive: isActive,
            HealthStatus: healthStatus,
            StatusEffects: "[ ]",
            InitiativeDisplay: "10",
            Row: RowPosition.Front,
            IsTargeted: isTargeted,
            IntentIcon: intentIcon,
            StatusIcons: statusIcons
        );
    }

    #endregion
}
