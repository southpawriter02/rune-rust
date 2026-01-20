// ═══════════════════════════════════════════════════════════════════════════════
// ComboContinuationDto.cs
// Data transfer object for combo continuation options.
// Version: 0.13.0c
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Presentation.Tui.DTOs;

/// <summary>
/// Data transfer object for combo continuation options.
/// </summary>
/// <param name="OptionNumber">Selection number for this option (e.g., [1], [2]).</param>
/// <param name="AbilityId">Ability ID that continues the combo.</param>
/// <param name="AbilityName">Display name of the ability.</param>
/// <param name="ComboName">Name of the combo being continued.</param>
/// <param name="StepProgress">Current progress string (e.g., "2/4").</param>
/// <param name="WindowRemaining">Turns remaining to continue.</param>
/// <param name="BonusPreview">Preview of bonus if combo continues.</param>
/// <remarks>
/// <para>
/// Used by <see cref="UI.ComboChainIndicator"/> to display available
/// continuation options in the "Continue combo:" prompt.
/// </para>
/// <para>
/// Display format: <c>[1] Slash - +10% damage bonus</c>
/// </para>
/// </remarks>
public record ComboContinuationDto(
    int OptionNumber,
    string AbilityId,
    string AbilityName,
    string ComboName,
    string StepProgress,
    int WindowRemaining,
    string BonusPreview);
