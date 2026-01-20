// ═══════════════════════════════════════════════════════════════════════════════
// StanceDisplayDto.cs
// Data transfer object for stance display.
// Version: 0.13.0d
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Presentation.Tui.DTOs;

/// <summary>
/// Data transfer object for stance display.
/// </summary>
/// <param name="StanceId">Unique identifier of the stance.</param>
/// <param name="StanceName">Display name of the stance.</param>
/// <param name="Category">Category of the stance (Aggressive, Defensive, etc.).</param>
/// <param name="Icon">Icon string for the stance.</param>
/// <param name="Color">Display color for the stance.</param>
/// <param name="Modifiers">Active stat modifiers from this stance.</param>
/// <param name="Description">Description of the stance's purpose.</param>
public record StanceDisplayDto(
    string StanceId,
    string StanceName,
    string Category,
    string Icon,
    ConsoleColor Color,
    IReadOnlyList<ModifierDisplayDto> Modifiers,
    string Description);
