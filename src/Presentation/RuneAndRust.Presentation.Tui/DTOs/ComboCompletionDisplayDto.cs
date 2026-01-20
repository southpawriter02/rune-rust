// ═══════════════════════════════════════════════════════════════════════════════
// ComboCompletionDisplayDto.cs
// Data transfer object for combo completion notifications.
// Version: 0.13.0c
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Presentation.Tui.DTOs;

/// <summary>
/// Data transfer object for combo completion notifications.
/// </summary>
/// <param name="ComboName">Name of the completed combo.</param>
/// <param name="TotalSteps">Total steps in the combo.</param>
/// <param name="BonusEffects">List of applied bonus effect descriptions.</param>
/// <remarks>
/// <para>
/// Used by <see cref="UI.ComboChainIndicator"/> to display completion notifications
/// with the full bonus effects applied.
/// </para>
/// <para>
/// Display format:
/// <code>
/// COMBO COMPLETE! Warrior's Fury (4 steps)
///   ✓ +25% damage bonus
///   ✓ Apply Burning
/// </code>
/// </para>
/// </remarks>
public record ComboCompletionDisplayDto(
    string ComboName,
    int TotalSteps,
    IReadOnlyList<string> BonusEffects);
