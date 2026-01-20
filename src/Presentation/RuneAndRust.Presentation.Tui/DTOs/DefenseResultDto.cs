// ═══════════════════════════════════════════════════════════════════════════════
// DefenseResultDto.cs
// Data transfer object for defense result display.
// Version: 0.13.0d
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Presentation.Tui.DTOs;

/// <summary>
/// Data transfer object for defense result display.
/// </summary>
/// <param name="Success">Whether the defense succeeded.</param>
/// <param name="ActionUsed">Name of the defense action used.</param>
/// <param name="OriginalDamage">Original incoming damage before reduction.</param>
/// <param name="ReducedDamage">Damage after reduction was applied.</param>
/// <param name="DamageReduced">Amount of damage that was reduced.</param>
/// <param name="Message">Result message describing the outcome.</param>
/// <param name="BonusEffects">Any bonus effects applied (e.g., counter-attack).</param>
public record DefenseResultDto(
    bool Success,
    string ActionUsed,
    int OriginalDamage,
    int ReducedDamage,
    int DamageReduced,
    string Message,
    IReadOnlyList<string> BonusEffects);
