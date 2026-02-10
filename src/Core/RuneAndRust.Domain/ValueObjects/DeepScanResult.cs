// ═══════════════════════════════════════════════════════════════════════════════
// DeepScanResult.cs
// Immutable value object representing the result of a Deep Scan examination
// attempt, including roll details, success level, layer information, and
// insight generation for the Jötun-Reader specialization.
// Version: 0.20.3a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

using System.Text;
using RuneAndRust.Domain.Enums;

/// <summary>
/// Result of a Deep Scan examination attempt with layer information and insight tracking.
/// </summary>
/// <remarks>
/// <para>
/// Named <c>DeepScanResult</c> to distinguish from the perception-layer
/// <see cref="ExaminationResult"/> which tracks general examination state.
/// </para>
/// <para>
/// Deep Scan results capture the full roll breakdown (base d20 + modifiers + 2d10 bonus),
/// the resulting success level, how many information layers were revealed,
/// and how much Lore Insight was generated.
/// </para>
/// <para>
/// Layer reveal mapping:
/// </para>
/// <list type="bullet">
///   <item><description><b>Failure:</b> 0 layers</description></item>
///   <item><description><b>Partial:</b> 1 layer (basic observation)</description></item>
///   <item><description><b>Success:</b> 2 layers (functional understanding)</description></item>
///   <item><description><b>Expert:</b> 3 layers (detailed analysis)</description></item>
///   <item><description><b>Master:</b> 4 layers (complete comprehension + bonus lore)</description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// var result = DeepScanResult.CreateSuccess(
///     targetId: machineId,
///     targetType: "Jötun Power Conduit",
///     baseRoll: 15,
///     modifiers: 12,
///     successLevel: ExaminationSuccessLevel.Expert,
///     information: new[] { "Layer 1...", "Layer 2...", "Layer 3..." },
///     insightGenerated: 1);
/// // result.TotalResult = 27, result.LayersRevealed = 3
/// </code>
/// </example>
public sealed record DeepScanResult
{
    /// <summary>
    /// Gets the ID of the examined object.
    /// </summary>
    public Guid TargetId { get; init; }

    /// <summary>
    /// Gets the type description of the target (e.g., "Jötun Power Conduit").
    /// </summary>
    public string TargetType { get; init; } = string.Empty;

    /// <summary>
    /// Gets the base d20 roll value.
    /// </summary>
    public int BaseRoll { get; init; }

    /// <summary>
    /// Gets the total modifiers applied to the roll (perception bonus + Deep Scan bonus).
    /// </summary>
    public int Modifiers { get; init; }

    /// <summary>
    /// Gets the final check result (<see cref="BaseRoll"/> + <see cref="Modifiers"/>).
    /// </summary>
    public int TotalResult { get; init; }

    /// <summary>
    /// Gets the number of information layers revealed (0–4).
    /// </summary>
    public int LayersRevealed { get; init; }

    /// <summary>
    /// Gets the success level of the examination.
    /// </summary>
    public ExaminationSuccessLevel SuccessLevel { get; init; }

    /// <summary>
    /// Gets the information revealed at each layer.
    /// </summary>
    public IReadOnlyList<string> Information { get; init; } = new List<string>();

    /// <summary>
    /// Gets the Lore Insight points generated from this examination.
    /// </summary>
    public int InsightGenerated { get; init; }

    /// <summary>
    /// Gets whether this was the first examination of this object by the character.
    /// </summary>
    public bool IsFirstExamination { get; init; }

    /// <summary>
    /// Creates a new Deep Scan result with calculated layers and total.
    /// </summary>
    /// <param name="targetId">ID of the examined object.</param>
    /// <param name="targetType">Type description of the target.</param>
    /// <param name="baseRoll">The base d20 roll.</param>
    /// <param name="modifiers">Total modifiers applied (perception + Deep Scan bonus).</param>
    /// <param name="successLevel">Determined success level.</param>
    /// <param name="information">Information revealed at each layer.</param>
    /// <param name="insightGenerated">Lore Insight points generated.</param>
    /// <param name="isFirstExamination">Whether this is the first examination of this object.</param>
    /// <returns>A new <see cref="DeepScanResult"/> instance.</returns>
    public static DeepScanResult CreateSuccess(
        Guid targetId,
        string targetType,
        int baseRoll,
        int modifiers,
        ExaminationSuccessLevel successLevel,
        IReadOnlyList<string> information,
        int insightGenerated,
        bool isFirstExamination = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(targetType);
        ArgumentNullException.ThrowIfNull(information);

        var totalResult = baseRoll + modifiers;
        var layersRevealed = successLevel switch
        {
            ExaminationSuccessLevel.Failure => 0,
            ExaminationSuccessLevel.Partial => 1,
            ExaminationSuccessLevel.Success => 2,
            ExaminationSuccessLevel.Expert => 3,
            ExaminationSuccessLevel.Master => 4,
            _ => 1
        };

        return new DeepScanResult
        {
            TargetId = targetId,
            TargetType = targetType,
            BaseRoll = baseRoll,
            Modifiers = modifiers,
            TotalResult = totalResult,
            LayersRevealed = layersRevealed,
            SuccessLevel = successLevel,
            Information = information,
            InsightGenerated = insightGenerated,
            IsFirstExamination = isFirstExamination
        };
    }

    /// <summary>
    /// Creates a failed Deep Scan result.
    /// </summary>
    /// <param name="targetId">ID of the examined object.</param>
    /// <param name="targetType">Type description of the target.</param>
    /// <param name="baseRoll">The base d20 roll.</param>
    /// <param name="modifiers">Total modifiers applied.</param>
    /// <returns>A new <see cref="DeepScanResult"/> with Failure success level.</returns>
    public static DeepScanResult CreateFailure(
        Guid targetId,
        string targetType,
        int baseRoll,
        int modifiers)
    {
        return new DeepScanResult
        {
            TargetId = targetId,
            TargetType = targetType,
            BaseRoll = baseRoll,
            Modifiers = modifiers,
            TotalResult = baseRoll + modifiers,
            LayersRevealed = 0,
            SuccessLevel = ExaminationSuccessLevel.Failure,
            Information = new List<string>(),
            InsightGenerated = 0,
            IsFirstExamination = false
        };
    }

    /// <summary>
    /// Determines if the Deep Scan succeeded (at least partial information gained).
    /// </summary>
    /// <returns><c>true</c> if success level is above Failure and layers were revealed.</returns>
    public bool IsSuccess() =>
        SuccessLevel != ExaminationSuccessLevel.Failure && LayersRevealed > 0;

    /// <summary>
    /// Gets the detail level from the success category (alias for <see cref="LayersRevealed"/>).
    /// </summary>
    /// <returns>Number of layers revealed (0–4).</returns>
    public int GetDetailLevel() => LayersRevealed;

    /// <summary>
    /// Formats the Deep Scan result for display.
    /// </summary>
    /// <returns>Multi-line formatted result string.</returns>
    public string GetFormattedResult()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Deep Scan of {TargetType}");
        sb.AppendLine($"Roll: {BaseRoll} + {Modifiers} = {TotalResult}");
        sb.AppendLine($"Success Level: {SuccessLevel}");
        sb.AppendLine($"Layers Revealed: {LayersRevealed}");
        if (InsightGenerated > 0)
            sb.AppendLine($"Lore Insight: +{InsightGenerated}");
        return sb.ToString();
    }

    /// <summary>
    /// Returns a human-readable representation of the Deep Scan result.
    /// </summary>
    public override string ToString() =>
        $"Deep Scan [{SuccessLevel}]: {TargetType} (Roll: {TotalResult}, Layers: {LayersRevealed})";
}
