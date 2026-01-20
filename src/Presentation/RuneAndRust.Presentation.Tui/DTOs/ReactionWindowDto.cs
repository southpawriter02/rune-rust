// ═══════════════════════════════════════════════════════════════════════════════
// ReactionWindowDto.cs
// Data transfer object for reaction window timing.
// Version: 0.13.0d
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Presentation.Tui.DTOs;

/// <summary>
/// Data transfer object for reaction window timing.
/// </summary>
/// <param name="TotalDuration">Total duration of the reaction window.</param>
/// <param name="RemainingTime">Time remaining in the window.</param>
/// <param name="ProgressPercent">Progress as percentage (0.0-1.0, where 1.0 = full time remaining).</param>
public record ReactionWindowDto(
    TimeSpan TotalDuration,
    TimeSpan RemainingTime,
    float ProgressPercent);
