using FluentAssertions;
using RuneAndRust.Core.Enums;
using RuneAndRust.Engine.Services;
using Xunit;

namespace RuneAndRust.Tests.Engine;

/// <summary>
/// Tests for the CombatLogFormatter introduced in v0.3.6b.
/// Validates damage type coloring and message formatting.
/// </summary>
public class CombatLogFormatterTests
{
    #region GetDamageColor Tests

    [Fact]
    public void GetDamageColor_Physical_ReturnsWhite()
    {
        // Act
        var result = CombatLogFormatter.GetDamageColor(DamageType.Physical);

        // Assert
        result.Should().Be("white");
    }

    [Fact]
    public void GetDamageColor_Fire_ReturnsOrange()
    {
        // Act
        var result = CombatLogFormatter.GetDamageColor(DamageType.Fire);

        // Assert
        result.Should().Be("orange1");
    }

    [Fact]
    public void GetDamageColor_Ice_ReturnsCyan()
    {
        // Act
        var result = CombatLogFormatter.GetDamageColor(DamageType.Ice);

        // Assert
        result.Should().Be("cyan1");
    }

    [Fact]
    public void GetDamageColor_Lightning_ReturnsYellow()
    {
        // Act
        var result = CombatLogFormatter.GetDamageColor(DamageType.Lightning);

        // Assert
        result.Should().Be("yellow");
    }

    [Fact]
    public void GetDamageColor_Poison_ReturnsGreen()
    {
        // Act
        var result = CombatLogFormatter.GetDamageColor(DamageType.Poison);

        // Assert
        result.Should().Be("green");
    }

    [Fact]
    public void GetDamageColor_Acid_ReturnsLime()
    {
        // Act
        var result = CombatLogFormatter.GetDamageColor(DamageType.Acid);

        // Assert
        result.Should().Be("lime");
    }

    [Fact]
    public void GetDamageColor_Psychic_ReturnsMagenta()
    {
        // Act
        var result = CombatLogFormatter.GetDamageColor(DamageType.Psychic);

        // Assert
        result.Should().Be("magenta");
    }

    [Fact]
    public void GetDamageColor_Blight_ReturnsMaroon()
    {
        // Act
        var result = CombatLogFormatter.GetDamageColor(DamageType.Blight);

        // Assert
        result.Should().Be("maroon");
    }

    #endregion

    #region FormatHitMessage Tests

    [Fact]
    public void FormatHitMessage_Physical_NoTypeLabel()
    {
        // Act
        var result = CombatLogFormatter.FormatHitMessage(
            "Hero", "Goblin", 15, DamageType.Physical);

        // Assert
        result.Should().Contain("[white]15[/]");
        result.Should().NotContain("Physical");
    }

    [Fact]
    public void FormatHitMessage_Fire_IncludesTypeLabel()
    {
        // Act
        var result = CombatLogFormatter.FormatHitMessage(
            "Hero", "Goblin", 15, DamageType.Fire);

        // Assert
        result.Should().Contain("[orange1]15 Fire[/]");
    }

    [Fact]
    public void FormatHitMessage_Critical_AddsCriticalPrefix()
    {
        // Act
        var result = CombatLogFormatter.FormatHitMessage(
            "Hero", "Goblin", 25, DamageType.Physical, isCritical: true);

        // Assert
        result.Should().Contain("[bold yellow]CRITICAL![/]");
    }

    [Fact]
    public void FormatHitMessage_IncludesAttackerName()
    {
        // Act
        var result = CombatLogFormatter.FormatHitMessage(
            "Test Hero", "Test Enemy", 10, DamageType.Physical);

        // Assert
        result.Should().Contain("[cyan]Test Hero[/]");
    }

    [Fact]
    public void FormatHitMessage_IncludesTargetName()
    {
        // Act
        var result = CombatLogFormatter.FormatHitMessage(
            "Hero", "Rusted Draugr", 10, DamageType.Physical);

        // Assert
        result.Should().Contain("[red]Rusted Draugr[/]");
    }

    [Fact]
    public void FormatHitMessage_EscapesBracketsInNames()
    {
        // Act
        var result = CombatLogFormatter.FormatHitMessage(
            "Hero [Elite]", "Goblin [Chief]", 10, DamageType.Physical);

        // Assert
        result.Should().Contain("[[Elite]]");
        result.Should().Contain("[[Chief]]");
    }

    #endregion

    #region FormatMissMessage Tests

    [Fact]
    public void FormatMissMessage_IncludesNames()
    {
        // Act
        var result = CombatLogFormatter.FormatMissMessage("Hero", "Enemy");

        // Assert
        result.Should().Contain("[cyan]Hero[/]");
        result.Should().Contain("[red]Enemy[/]");
        result.Should().Contain("misses");
    }

    [Fact]
    public void FormatMissMessage_HasGreyMissIndicator()
    {
        // Act
        var result = CombatLogFormatter.FormatMissMessage("Hero", "Enemy");

        // Assert
        result.Should().Contain("[grey]misses![/]");
    }

    #endregion

    #region FormatDeathMessage Tests

    [Fact]
    public void FormatDeathMessage_Player_UsesBoldRed()
    {
        // Act
        var result = CombatLogFormatter.FormatDeathMessage("Test Hero", isPlayer: true);

        // Assert
        result.Should().Contain("[bold red]");
        result.Should().Contain("has fallen");
    }

    [Fact]
    public void FormatDeathMessage_Enemy_UsesGreyDefeated()
    {
        // Act
        var result = CombatLogFormatter.FormatDeathMessage("Goblin", isPlayer: false);

        // Assert
        result.Should().Contain("[grey]defeated![/]");
    }

    #endregion

    #region FormatStatusApplied Tests

    [Fact]
    public void FormatStatusApplied_IncludesTargetAndEffect()
    {
        // Act
        var result = CombatLogFormatter.FormatStatusApplied("Enemy", "Bleeding");

        // Assert
        result.Should().Contain("[red]Enemy[/]");
        result.Should().Contain("[yellow]Bleeding[/]");
    }

    #endregion

    #region FormatAbilityUse Tests

    [Fact]
    public void FormatAbilityUse_IncludesCasterAndAbility()
    {
        // Act
        var result = CombatLogFormatter.FormatAbilityUse("Hero", "Power Strike");

        // Assert
        result.Should().Contain("[cyan]Hero[/]");
        result.Should().Contain("[yellow]Power Strike[/]");
    }

    #endregion

    #region FormatHealMessage Tests

    [Fact]
    public void FormatHealMessage_ShowsGreenHealing()
    {
        // Act
        var result = CombatLogFormatter.FormatHealMessage("Hero", 25);

        // Assert
        result.Should().Contain("[cyan]Hero[/]");
        result.Should().Contain("[green]25[/]");
        result.Should().Contain("HP");
    }

    #endregion

    #region Combat Flow Messages Tests

    [Fact]
    public void FormatRoundStart_IncludesRoundNumber()
    {
        // Act
        var result = CombatLogFormatter.FormatRoundStart(3);

        // Assert
        result.Should().Contain("Round 3");
        result.Should().Contain("[bold white]");
    }

    [Fact]
    public void FormatTurnStart_Player_UsesCyan()
    {
        // Act
        var result = CombatLogFormatter.FormatTurnStart("Hero", isPlayer: true);

        // Assert
        result.Should().Contain("[cyan]Hero[/]");
    }

    [Fact]
    public void FormatTurnStart_Enemy_UsesRed()
    {
        // Act
        var result = CombatLogFormatter.FormatTurnStart("Goblin", isPlayer: false);

        // Assert
        result.Should().Contain("[red]Goblin[/]");
    }

    [Fact]
    public void FormatVictory_ShowsBoldGreen()
    {
        // Act
        var result = CombatLogFormatter.FormatVictory();

        // Assert
        result.Should().Contain("[bold green]");
        result.Should().Contain("Victory");
    }

    [Fact]
    public void FormatDefeat_ShowsBoldRed()
    {
        // Act
        var result = CombatLogFormatter.FormatDefeat();

        // Assert
        result.Should().Contain("[bold red]");
        result.Should().Contain("Defeat");
    }

    #endregion
}
