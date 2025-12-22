using RuneAndRust.Core.Enums;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// Formats combat events into rich Spectre.Console markup (v0.3.6b).
/// Centralizes all combat log formatting with damage type coloring.
/// Stateless static class - no DI registration required.
/// </summary>
public static class CombatLogFormatter
{
    #region Damage Color Mapping

    /// <summary>
    /// Gets the Spectre.Console color name for a damage type.
    /// </summary>
    /// <param name="type">The damage type to get color for.</param>
    /// <returns>A Spectre.Console color name string.</returns>
    public static string GetDamageColor(DamageType type) => type switch
    {
        DamageType.Physical => "white",
        DamageType.Fire => "orange1",
        DamageType.Ice => "cyan1",
        DamageType.Lightning => "yellow",
        DamageType.Poison => "green",
        DamageType.Acid => "lime",
        DamageType.Psychic => "magenta",
        DamageType.Blight => "maroon",
        _ => "white"
    };

    #endregion

    #region Attack Messages

    /// <summary>
    /// Formats a successful attack message with damage type coloring.
    /// </summary>
    /// <param name="attackerName">Name of the attacker.</param>
    /// <param name="targetName">Name of the target.</param>
    /// <param name="damage">Final damage dealt.</param>
    /// <param name="damageType">Type of damage dealt.</param>
    /// <param name="isCritical">Whether this was a critical hit.</param>
    /// <returns>A Spectre.Console markup string for the combat log.</returns>
    public static string FormatHitMessage(
        string attackerName,
        string targetName,
        int damage,
        DamageType damageType,
        bool isCritical = false)
    {
        var color = GetDamageColor(damageType);
        var critPrefix = isCritical ? "[bold yellow]CRITICAL![/] " : "";

        // Only show damage type label for non-Physical damage
        var damageLabel = damageType == DamageType.Physical ? "" : $" {damageType}";

        return $"{critPrefix}[cyan]{EscapeMarkup(attackerName)}[/] hits [red]{EscapeMarkup(targetName)}[/] for [{color}]{damage}{damageLabel}[/] damage!";
    }

    /// <summary>
    /// Formats a miss message.
    /// </summary>
    /// <param name="attackerName">Name of the attacker.</param>
    /// <param name="targetName">Name of the target.</param>
    /// <returns>A Spectre.Console markup string for the combat log.</returns>
    public static string FormatMissMessage(string attackerName, string targetName)
    {
        return $"[cyan]{EscapeMarkup(attackerName)}[/] attacks [red]{EscapeMarkup(targetName)}[/] but [grey]misses![/]";
    }

    /// <summary>
    /// Formats a glancing blow message (reduced damage).
    /// </summary>
    /// <param name="attackerName">Name of the attacker.</param>
    /// <param name="targetName">Name of the target.</param>
    /// <param name="damage">Final damage dealt.</param>
    /// <param name="damageType">Type of damage dealt.</param>
    /// <returns>A Spectre.Console markup string for the combat log.</returns>
    public static string FormatGlancingMessage(
        string attackerName,
        string targetName,
        int damage,
        DamageType damageType)
    {
        var color = GetDamageColor(damageType);
        var damageLabel = damageType == DamageType.Physical ? "" : $" {damageType}";

        return $"[cyan]{EscapeMarkup(attackerName)}[/] [grey]glances[/] [red]{EscapeMarkup(targetName)}[/] for [{color}]{damage}{damageLabel}[/] damage.";
    }

    #endregion

    #region Status Effect Messages

    /// <summary>
    /// Formats a status effect application message.
    /// </summary>
    /// <param name="targetName">Name of the affected target.</param>
    /// <param name="effectName">Name of the status effect.</param>
    /// <returns>A Spectre.Console markup string for the combat log.</returns>
    public static string FormatStatusApplied(string targetName, string effectName)
    {
        return $"[red]{EscapeMarkup(targetName)}[/] is now [yellow]{effectName}[/]!";
    }

    /// <summary>
    /// Formats a status effect expiration message.
    /// </summary>
    /// <param name="targetName">Name of the affected target.</param>
    /// <param name="effectName">Name of the status effect.</param>
    /// <returns>A Spectre.Console markup string for the combat log.</returns>
    public static string FormatStatusExpired(string targetName, string effectName)
    {
        return $"[grey]{effectName}[/] wears off from [cyan]{EscapeMarkup(targetName)}[/].";
    }

    #endregion

    #region Death Messages

    /// <summary>
    /// Formats a death message.
    /// </summary>
    /// <param name="targetName">Name of the fallen combatant.</param>
    /// <param name="isPlayer">Whether the fallen combatant is a player.</param>
    /// <returns>A Spectre.Console markup string for the combat log.</returns>
    public static string FormatDeathMessage(string targetName, bool isPlayer)
    {
        return isPlayer
            ? $"[bold red]{EscapeMarkup(targetName)} has fallen![/]"
            : $"[red]{EscapeMarkup(targetName)}[/] is [grey]defeated![/]";
    }

    #endregion

    #region Combat Flow Messages

    /// <summary>
    /// Formats a round start message.
    /// </summary>
    /// <param name="roundNumber">The round number starting.</param>
    /// <returns>A Spectre.Console markup string for the combat log.</returns>
    public static string FormatRoundStart(int roundNumber)
    {
        return $"[bold white]─── Round {roundNumber} ───[/]";
    }

    /// <summary>
    /// Formats a turn start message.
    /// </summary>
    /// <param name="combatantName">Name of the combatant starting their turn.</param>
    /// <param name="isPlayer">Whether this is the player's turn.</param>
    /// <returns>A Spectre.Console markup string for the combat log.</returns>
    public static string FormatTurnStart(string combatantName, bool isPlayer)
    {
        var color = isPlayer ? "cyan" : "red";
        return $"[{color}]{EscapeMarkup(combatantName)}[/]'s turn.";
    }

    /// <summary>
    /// Formats a combat victory message.
    /// </summary>
    /// <returns>A Spectre.Console markup string for the combat log.</returns>
    public static string FormatVictory()
    {
        return "[bold green]Victory! All enemies defeated.[/]";
    }

    /// <summary>
    /// Formats a combat defeat message.
    /// </summary>
    /// <returns>A Spectre.Console markup string for the combat log.</returns>
    public static string FormatDefeat()
    {
        return "[bold red]Defeat! You have fallen in battle.[/]";
    }

    #endregion

    #region Ability Messages

    /// <summary>
    /// Formats an ability use message.
    /// </summary>
    /// <param name="casterName">Name of the ability user.</param>
    /// <param name="abilityName">Name of the ability used.</param>
    /// <returns>A Spectre.Console markup string for the combat log.</returns>
    public static string FormatAbilityUse(string casterName, string abilityName)
    {
        return $"[cyan]{EscapeMarkup(casterName)}[/] uses [yellow]{abilityName}[/]!";
    }

    /// <summary>
    /// Formats a healing message.
    /// </summary>
    /// <param name="targetName">Name of the healed target.</param>
    /// <param name="amount">Amount healed.</param>
    /// <returns>A Spectre.Console markup string for the combat log.</returns>
    public static string FormatHealMessage(string targetName, int amount)
    {
        return $"[cyan]{EscapeMarkup(targetName)}[/] recovers [green]{amount}[/] HP!";
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Escapes special Spectre.Console markup characters in text.
    /// </summary>
    /// <param name="text">Text to escape.</param>
    /// <returns>Escaped text safe for Spectre markup.</returns>
    private static string EscapeMarkup(string text)
    {
        return text.Replace("[", "[[").Replace("]", "]]");
    }

    #endregion
}
