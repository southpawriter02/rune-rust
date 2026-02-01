// ═══════════════════════════════════════════════════════════════════════════════
// TraumaAcquisitionResult.cs
// Immutable record representing the outcome of a trauma acquisition attempt.
// Version: 0.18.3b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.Records;

/// <summary>
/// Represents the outcome of a trauma acquisition attempt.
/// </summary>
/// <remarks>
/// <para>
/// This record is returned when attempting to acquire a trauma on a character.
/// It distinguishes between successful new acquisitions, stacked acquisitions,
/// and failed attempts to add a trauma the character already has.
/// </para>
/// <para>
/// <b>Success conditions:</b>
/// <list type="bullet">
///   <item><description>First time acquiring trauma (IsNewTrauma=true)</description></item>
///   <item><description>Stackable trauma being re-acquired (IsNewTrauma=false, StackCount increases)</description></item>
/// </list>
/// </para>
/// <para>
/// <b>Failure conditions:</b>
/// <list type="bullet">
///   <item><description>Non-stackable trauma and character already has it (Success=false)</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var result = TraumaAcquisitionResult.CreateNew(
///     traumaId: "survivors-guilt",
///     traumaName: "Survivor's Guilt",
///     source: "AllyDeath",
///     triggersRetirementCheck: true
/// );
/// </code>
/// </example>
public record TraumaAcquisitionResult
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Properties
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether the trauma acquisition was successful.
    /// </summary>
    /// <remarks>
    /// <para>True if trauma was newly acquired or successfully stacked.</para>
    /// <para>False if character already has non-stackable trauma.</para>
    /// </remarks>
    public bool Success { get; init; }

    /// <summary>
    /// Gets the trauma ID that was acquired or attempted.
    /// </summary>
    /// <remarks>
    /// Matches the TraumaDefinition.TraumaId (e.g., "survivors-guilt").
    /// </remarks>
    public string TraumaId { get; init; }

    /// <summary>
    /// Gets the display name of the trauma.
    /// </summary>
    /// <remarks>
    /// Human-readable name for UI display (e.g., "Survivor's Guilt").
    /// </remarks>
    public string TraumaName { get; init; }

    /// <summary>
    /// Gets the source/trigger of the acquisition.
    /// </summary>
    /// <remarks>
    /// What caused the trauma acquisition (e.g., "AllyDeath", "CorruptionThreshold75").
    /// </remarks>
    public string Source { get; init; }

    /// <summary>
    /// Gets whether this is a new trauma or a stacked instance.
    /// </summary>
    /// <remarks>
    /// <para>True if character did not previously have this trauma.</para>
    /// <para>False if stacking an existing trauma or if acquisition failed.</para>
    /// </remarks>
    public bool IsNewTrauma { get; init; }

    /// <summary>
    /// Gets the current stack count after acquisition.
    /// </summary>
    /// <remarks>
    /// <para>1 for newly acquired traumas.</para>
    /// <para>2+ for stacked traumas.</para>
    /// <para>0 for failed acquisitions.</para>
    /// </remarks>
    public int NewStackCount { get; init; }

    /// <summary>
    /// Gets whether this acquisition triggers a retirement check.
    /// </summary>
    /// <remarks>
    /// <para>True if the trauma is a retirement trauma or stacking to critical level.</para>
    /// <para>False for normal traumas or failed acquisitions.</para>
    /// </remarks>
    public bool TriggersRetirementCheck { get; init; }

    /// <summary>
    /// Gets a message describing the acquisition result.
    /// </summary>
    /// <remarks>
    /// Human-readable message for display to player.
    /// </remarks>
    public string Message { get; init; }

    // ═══════════════════════════════════════════════════════════════════════════
    // Constructors
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of the <see cref="TraumaAcquisitionResult"/> record.
    /// </summary>
    /// <remarks>
    /// Use factory methods (<see cref="CreateNew"/>, <see cref="CreateStacked"/>,
    /// <see cref="CreateFailure"/>) instead of this constructor directly.
    /// </remarks>
    private TraumaAcquisitionResult()
    {
        TraumaId = string.Empty;
        TraumaName = string.Empty;
        Source = string.Empty;
        Message = string.Empty;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Factory Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a successful acquisition result for a first-time trauma.
    /// </summary>
    /// <param name="traumaId">The trauma definition ID (e.g., "survivors-guilt").</param>
    /// <param name="traumaName">The display name (e.g., "Survivor's Guilt").</param>
    /// <param name="source">The acquisition source (e.g., "AllyDeath").</param>
    /// <param name="triggersRetirementCheck">Whether this triggers a retirement check.</param>
    /// <returns>A new TraumaAcquisitionResult indicating successful new acquisition.</returns>
    /// <example>
    /// <code>
    /// var result = TraumaAcquisitionResult.CreateNew(
    ///     traumaId: "survivors-guilt",
    ///     traumaName: "Survivor's Guilt",
    ///     source: "AllyDeath",
    ///     triggersRetirementCheck: true
    /// );
    /// // result.Success == true
    /// // result.IsNewTrauma == true
    /// // result.NewStackCount == 1
    /// </code>
    /// </example>
    public static TraumaAcquisitionResult CreateNew(
        string traumaId,
        string traumaName,
        string source,
        bool triggersRetirementCheck)
    {
        var message = $"You have acquired {traumaName}.";
        if (triggersRetirementCheck)
        {
            message += " [RETIREMENT CHECK REQUIRED]";
        }

        return new TraumaAcquisitionResult
        {
            Success = true,
            TraumaId = traumaId,
            TraumaName = traumaName,
            Source = source,
            IsNewTrauma = true,
            NewStackCount = 1,
            TriggersRetirementCheck = triggersRetirementCheck,
            Message = message
        };
    }

    /// <summary>
    /// Creates a successful acquisition result for a stacked trauma.
    /// </summary>
    /// <param name="traumaId">The trauma definition ID (e.g., "reality-doubt").</param>
    /// <param name="traumaName">The display name (e.g., "Reality Doubt").</param>
    /// <param name="source">The acquisition source.</param>
    /// <param name="newStackCount">The new stack count after increment (must be >= 2).</param>
    /// <param name="triggersRetirementCheck">Whether stacking triggers a retirement check.</param>
    /// <returns>A new TraumaAcquisitionResult indicating successful stacking.</returns>
    /// <example>
    /// <code>
    /// var result = TraumaAcquisitionResult.CreateStacked(
    ///     traumaId: "reality-doubt",
    ///     traumaName: "Reality Doubt",
    ///     source: "WitnessingHorror",
    ///     newStackCount: 5,
    ///     triggersRetirementCheck: true // 5+ triggers retirement
    /// );
    /// // result.Success == true
    /// // result.IsNewTrauma == false
    /// // result.NewStackCount == 5
    /// </code>
    /// </example>
    public static TraumaAcquisitionResult CreateStacked(
        string traumaId,
        string traumaName,
        string source,
        int newStackCount,
        bool triggersRetirementCheck)
    {
        var message = $"{traumaName} has worsened (x{newStackCount}).";
        if (triggersRetirementCheck)
        {
            message += " [CRITICAL - RETIREMENT CHECK REQUIRED]";
        }

        return new TraumaAcquisitionResult
        {
            Success = true,
            TraumaId = traumaId,
            TraumaName = traumaName,
            Source = source,
            IsNewTrauma = false,
            NewStackCount = newStackCount,
            TriggersRetirementCheck = triggersRetirementCheck,
            Message = message
        };
    }

    /// <summary>
    /// Creates a failed acquisition result for a non-stackable trauma.
    /// </summary>
    /// <param name="traumaId">The trauma definition ID.</param>
    /// <param name="traumaName">The display name.</param>
    /// <param name="source">The attempted source.</param>
    /// <returns>A new TraumaAcquisitionResult indicating failed acquisition.</returns>
    /// <example>
    /// <code>
    /// var result = TraumaAcquisitionResult.CreateFailure(
    ///     traumaId: "machine-affinity",
    ///     traumaName: "Machine Affinity",
    ///     source: "ForlornContact"
    /// );
    /// // result.Success == false
    /// // result.NewStackCount == 0
    /// </code>
    /// </example>
    public static TraumaAcquisitionResult CreateFailure(
        string traumaId,
        string traumaName,
        string source)
    {
        return new TraumaAcquisitionResult
        {
            Success = false,
            TraumaId = traumaId,
            TraumaName = traumaName,
            Source = source,
            IsNewTrauma = false,
            NewStackCount = 0,
            TriggersRetirementCheck = false,
            Message = $"Character already has {traumaName} (cannot stack)."
        };
    }
}
