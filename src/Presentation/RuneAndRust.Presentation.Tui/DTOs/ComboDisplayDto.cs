// ═══════════════════════════════════════════════════════════════════════════════
// ComboDisplayDto.cs
// Data transfer object for combo chain display.
// Version: 0.13.0c
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Presentation.Tui.DTOs;

/// <summary>
/// Data transfer object for combo chain display.
/// </summary>
/// <param name="ComboId">Unique identifier of the combo.</param>
/// <param name="ComboName">Display name of the combo.</param>
/// <param name="Steps">List of steps in the combo sequence.</param>
/// <param name="CurrentStep">Current step number (1-indexed).</param>
/// <param name="TotalSteps">Total number of steps in combo.</param>
/// <param name="WindowRemaining">Turns remaining to continue combo.</param>
/// <param name="BonusEffects">List of bonus effect descriptions.</param>
/// <param name="ProgressPercent">Completion progress as percentage (0-100).</param>
/// <remarks>
/// <para>
/// Used by <see cref="UI.ComboChainIndicator"/> to render the complete combo chain.
/// Contains all information needed for chain display, bonus preview, and window timer.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var combo = new ComboDisplayDto(
///     ComboId: "warriors-fury",
///     ComboName: "Warrior's Fury",
///     Steps: stepList,
///     CurrentStep: 2,
///     TotalSteps: 4,
///     WindowRemaining: 2,
///     BonusEffects: new[] { "+25% damage" },
///     ProgressPercent: 50);
/// </code>
/// </example>
public record ComboDisplayDto(
    string ComboId,
    string ComboName,
    IReadOnlyList<ComboStepDisplayDto> Steps,
    int CurrentStep,
    int TotalSteps,
    int WindowRemaining,
    IReadOnlyList<string> BonusEffects,
    int ProgressPercent);
