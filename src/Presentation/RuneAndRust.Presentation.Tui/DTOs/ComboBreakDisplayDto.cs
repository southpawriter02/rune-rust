// ═══════════════════════════════════════════════════════════════════════════════
// ComboBreakDisplayDto.cs
// Data transfer object for combo break notifications.
// Version: 0.13.0c
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Presentation.Tui.DTOs;

/// <summary>
/// Data transfer object for combo break notifications.
/// </summary>
/// <param name="ComboName">Name of the broken combo.</param>
/// <param name="ChainLength">Steps completed before break.</param>
/// <param name="Reason">Reason for combo breaking.</param>
/// <remarks>
/// <para>
/// Used by <see cref="UI.ComboChainIndicator"/> to display break notifications
/// when a combo fails due to window expiration or invalid action.
/// </para>
/// <para>
/// Display format: <c>COMBO BROKEN! Warrior's Fury ended at step 2</c>
/// </para>
/// </remarks>
public record ComboBreakDisplayDto(
    string ComboName,
    int ChainLength,
    string Reason);
