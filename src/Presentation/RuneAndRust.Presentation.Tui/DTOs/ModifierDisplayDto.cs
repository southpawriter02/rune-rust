// ═══════════════════════════════════════════════════════════════════════════════
// ModifierDisplayDto.cs
// Data transfer object for stat modifier display.
// Version: 0.13.0d
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Presentation.Tui.DTOs;

/// <summary>
/// Data transfer object for stat modifier display.
/// </summary>
/// <param name="StatName">Name of the affected stat (Attack, Defense, etc.).</param>
/// <param name="Value">Modifier value (can be positive or negative).</param>
/// <param name="IsPositive">Whether the modifier is positive (buff) or negative (debuff).</param>
/// <param name="DisplayString">Formatted display string (e.g., "+2 Attack", "-1 Defense").</param>
public record ModifierDisplayDto(
    string StatName,
    int Value,
    bool IsPositive,
    string DisplayString);
