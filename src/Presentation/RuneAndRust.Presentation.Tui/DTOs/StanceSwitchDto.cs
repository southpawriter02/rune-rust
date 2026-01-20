// ═══════════════════════════════════════════════════════════════════════════════
// StanceSwitchDto.cs
// Data transfer object for stance switch display.
// Version: 0.13.0d
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Presentation.Tui.DTOs;

/// <summary>
/// Data transfer object for stance switch display.
/// </summary>
/// <param name="FromStanceName">Name of the previous stance.</param>
/// <param name="ToStanceName">Name of the new stance.</param>
/// <param name="FromModifiers">Modifiers from previous stance for comparison.</param>
/// <param name="ToModifiers">Modifiers from new stance for comparison.</param>
public record StanceSwitchDto(
    string FromStanceName,
    string ToStanceName,
    IReadOnlyList<ModifierDisplayDto> FromModifiers,
    IReadOnlyList<ModifierDisplayDto> ToModifiers);
