// ═══════════════════════════════════════════════════════════════════════════════
// ComboStepDisplayDto.cs
// Data transfer object for individual combo step display.
// Version: 0.13.0c
// ═══════════════════════════════════════════════════════════════════════════════

using RuneAndRust.Presentation.Tui.Enums;

namespace RuneAndRust.Presentation.Tui.DTOs;

/// <summary>
/// Data transfer object for individual combo step display.
/// </summary>
/// <param name="StepNumber">Position in combo sequence (1-indexed).</param>
/// <param name="AbilityId">Ability ID for this step.</param>
/// <param name="AbilityName">Display name of the ability.</param>
/// <param name="State">Current visual state of this step.</param>
/// <param name="TargetRequirement">Target requirement description (e.g., "SameTarget").</param>
/// <remarks>
/// <para>
/// Used by <see cref="ComboDisplayDto"/> to represent each step in a combo chain.
/// The <see cref="State"/> determines the visual styling (symbol and color).
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var step = new ComboStepDisplayDto(
///     StepNumber: 2,
///     AbilityId: "slash",
///     AbilityName: "Slash",
///     State: ComboStepState.Current,
///     TargetRequirement: "SameTarget");
/// </code>
/// </example>
public record ComboStepDisplayDto(
    int StepNumber,
    string AbilityId,
    string AbilityName,
    ComboStepState State,
    string TargetRequirement);
