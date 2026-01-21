// ═══════════════════════════════════════════════════════════════════════════════
// IconUtilities.cs
// Shared icon/symbol utilities for TUI and GUI presentation layers.
// Version: 0.13.5e
// ═══════════════════════════════════════════════════════════════════════════════

using RuneAndRust.Presentation.Shared.Enums;

namespace RuneAndRust.Presentation.Shared.Utilities;

/// <summary>
/// Provides static utility methods for retrieving icons and symbols used
/// consistently across TUI and GUI presentation layers.
/// </summary>
/// <remarks>
/// <para>
/// This utility class centralizes icon lookup operations that were previously
/// duplicated across multiple components. By consolidating these methods,
/// we ensure consistent iconography throughout the application.
/// </para>
/// <para>
/// All icon methods support a Unicode/ASCII fallback pattern:
/// </para>
/// <list type="bullet">
///   <item><description><c>useUnicode: true</c> - Returns Unicode symbols (🔥, ⚔, ↑)</description></item>
///   <item><description><c>useUnicode: false</c> - Returns ASCII fallbacks ([F], [P], ^)</description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Get direction arrow
/// var arrow = IconUtilities.GetDirectionIcon(Direction.North); // "↑"
/// 
/// // Get damage type icon with ASCII fallback (using string-based ID)
/// var fire = IconUtilities.GetDamageTypeIcon("fire", useUnicode: false); // "[F]"
/// 
/// // Get status indicator
/// var check = IconUtilities.GetStatusIcon(StatusType.Success); // "✓"
/// </code>
/// </example>
public static class IconUtilities
{
    // ═══════════════════════════════════════════════════════════════════════════
    // DIRECTION ICONS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets an icon representing a direction.
    /// </summary>
    /// <param name="direction">The direction to represent.</param>
    /// <param name="useUnicode">
    /// If <c>true</c> (default), returns Unicode arrows.
    /// If <c>false</c>, returns ASCII characters.
    /// </param>
    /// <returns>
    /// A string representing the direction (e.g., "↑" for North, "^" for ASCII).
    /// </returns>
    /// <example>
    /// <code>
    /// IconUtilities.GetDirectionIcon(Direction.North);        // "↑"
    /// IconUtilities.GetDirectionIcon(Direction.NorthEast);    // "↗"
    /// IconUtilities.GetDirectionIcon(Direction.North, false); // "^"
    /// IconUtilities.GetDirectionIcon(Direction.None);         // "·"
    /// </code>
    /// </example>
    public static string GetDirectionIcon(Direction direction, bool useUnicode = true)
    {
        if (useUnicode)
        {
            return direction switch
            {
                Direction.North => "↑",
                Direction.NorthEast => "↗",
                Direction.East => "→",
                Direction.SouthEast => "↘",
                Direction.South => "↓",
                Direction.SouthWest => "↙",
                Direction.West => "←",
                Direction.NorthWest => "↖",
                Direction.None => "·",
                _ => "·"
            };
        }

        return direction switch
        {
            Direction.North => "^",
            Direction.NorthEast => "/",
            Direction.East => ">",
            Direction.SouthEast => "\\",
            Direction.South => "v",
            Direction.SouthWest => "/",
            Direction.West => "<",
            Direction.NorthWest => "\\",
            Direction.None => ".",
            _ => "."
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DAMAGE TYPE ICONS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets an icon representing a damage type by its string identifier.
    /// </summary>
    /// <param name="damageTypeId">
    /// The damage type identifier (e.g., "physical", "fire", "ice").
    /// Case-insensitive matching is used.
    /// </param>
    /// <param name="useUnicode">
    /// If <c>true</c> (default), returns Unicode symbols.
    /// If <c>false</c>, returns ASCII brackets notation.
    /// </param>
    /// <returns>
    /// A string representing the damage type (e.g., "🔥" for Fire, "[F]" for ASCII).
    /// Returns a default icon for unrecognized damage types.
    /// </returns>
    /// <example>
    /// <code>
    /// IconUtilities.GetDamageTypeIcon("fire");         // "🔥"
    /// IconUtilities.GetDamageTypeIcon("physical");    // "⚔"
    /// IconUtilities.GetDamageTypeIcon("fire", false); // "[F]"
    /// IconUtilities.GetDamageTypeIcon("FIRE");        // "🔥" (case-insensitive)
    /// IconUtilities.GetDamageTypeIcon("unknown");     // "✦" (default)
    /// </code>
    /// </example>
    public static string GetDamageTypeIcon(string damageTypeId, bool useUnicode = true)
    {
        var normalizedId = damageTypeId?.ToLowerInvariant() ?? string.Empty;

        if (useUnicode)
        {
            return normalizedId switch
            {
                "physical" => "⚔",
                "fire" => "🔥",
                "ice" => "❄",
                "lightning" => "⚡",
                "poison" => "☠",
                "healing" => "💚",
                "arcane" => "✨",
                "holy" => "☀",
                "shadow" => "🌑",
                "nature" => "🌿",
                _ => "✦"
            };
        }

        return normalizedId switch
        {
            "physical" => "[P]",
            "fire" => "[F]",
            "ice" => "[I]",
            "lightning" => "[L]",
            "poison" => "[X]",
            "healing" => "[H]",
            "arcane" => "[A]",
            "holy" => "[O]",
            "shadow" => "[S]",
            "nature" => "[N]",
            _ => "[?]"
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DICE ICONS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets an icon representing a die face value (1-6).
    /// </summary>
    /// <param name="value">The die face value (1-6 for Unicode, any for ASCII).</param>
    /// <param name="useUnicode">
    /// If <c>true</c> (default) and value is 1-6, returns Unicode die faces.
    /// Otherwise, returns bracketed number.
    /// </param>
    /// <returns>
    /// A string representing the die value:
    /// Unicode: "⚀" to "⚅" for values 1-6.
    /// ASCII: "[1]" to "[n]" for any value.
    /// </returns>
    /// <example>
    /// <code>
    /// IconUtilities.GetDieFaceIcon(1);        // "⚀"
    /// IconUtilities.GetDieFaceIcon(6);        // "⚅"
    /// IconUtilities.GetDieFaceIcon(8);        // "[8]" (beyond d6 range)
    /// IconUtilities.GetDieFaceIcon(3, false); // "[3]"
    /// </code>
    /// </example>
    public static string GetDieFaceIcon(int value, bool useUnicode = true)
    {
        // Unicode die faces exist only for values 1-6
        if (useUnicode && value >= 1 && value <= 6)
        {
            // Unicode die faces: ⚀ (U+2680) through ⚅ (U+2685)
            return ((char)(0x2680 + value - 1)).ToString();
        }

        return $"[{value}]";
    }

    /// <summary>
    /// Gets an icon representing a dice type.
    /// </summary>
    /// <param name="diceType">The type of die to represent.</param>
    /// <param name="useUnicode">
    /// If <c>true</c> (default), returns Unicode dice with subscript notation.
    /// If <c>false</c>, returns standard "dN" notation.
    /// </param>
    /// <returns>
    /// A string representing the dice type (e.g., "🎲₂₀" for D20, "d20" for ASCII).
    /// </returns>
    /// <example>
    /// <code>
    /// IconUtilities.GetDiceIcon(DiceType.D20);        // "🎲₂₀"
    /// IconUtilities.GetDiceIcon(DiceType.D6);         // "🎲₆"
    /// IconUtilities.GetDiceIcon(DiceType.D20, false); // "d20"
    /// </code>
    /// </example>
    public static string GetDiceIcon(DiceType diceType, bool useUnicode = true)
    {
        if (useUnicode)
        {
            return diceType switch
            {
                DiceType.D4 => "🎲₄",
                DiceType.D6 => "🎲₆",
                DiceType.D8 => "🎲₈",
                DiceType.D10 => "🎲₁₀",
                DiceType.D12 => "🎲₁₂",
                DiceType.D20 => "🎲₂₀",
                DiceType.D100 => "🎲₁₀₀",
                _ => "🎲"
            };
        }

        return $"d{(int)diceType}";
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // STATUS INDICATORS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets a status indicator icon.
    /// </summary>
    /// <param name="status">The status type to represent.</param>
    /// <param name="useUnicode">
    /// If <c>true</c> (default), returns Unicode symbols.
    /// If <c>false</c>, returns ASCII bracketed notation.
    /// </param>
    /// <returns>
    /// A string representing the status (e.g., "✓" for Success, "[OK]" for ASCII).
    /// </returns>
    /// <example>
    /// <code>
    /// IconUtilities.GetStatusIcon(StatusType.Success);      // "✓"
    /// IconUtilities.GetStatusIcon(StatusType.Failure);      // "✗"
    /// IconUtilities.GetStatusIcon(StatusType.Warning);      // "⚠"
    /// IconUtilities.GetStatusIcon(StatusType.Success, false); // "[OK]"
    /// </code>
    /// </example>
    public static string GetStatusIcon(StatusType status, bool useUnicode = true)
    {
        if (useUnicode)
        {
            return status switch
            {
                StatusType.Success => "✓",
                StatusType.Failure => "✗",
                StatusType.Warning => "⚠",
                StatusType.Info => "ℹ",
                StatusType.Pending => "◌",
                StatusType.InProgress => "◐",
                _ => "○"
            };
        }

        return status switch
        {
            StatusType.Success => "[OK]",
            StatusType.Failure => "[X]",
            StatusType.Warning => "[!]",
            StatusType.Info => "[i]",
            StatusType.Pending => "[ ]",
            StatusType.InProgress => "[.]",
            _ => "[ ]"
        };
    }
}
