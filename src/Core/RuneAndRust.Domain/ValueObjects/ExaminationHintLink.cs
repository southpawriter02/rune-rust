namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Links an examinable object to a puzzle hint, defining when and how
/// the hint is revealed through the examination system.
/// </summary>
/// <remarks>
/// <para>
/// ExaminationHintLink creates the connection between objects in the game
/// world and puzzle hints. When a player examines an object and achieves
/// the required layer, the linked hint is revealed if all conditions are met.
/// </para>
/// <para>
/// Optional reveal conditions allow for prerequisite-based hints that only
/// appear after certain items are obtained or other hints are discovered.
/// </para>
/// </remarks>
/// <param name="ObjectId">The examinable object ID that can reveal this hint.</param>
/// <param name="RequiredLayer">The examination layer required (typically 3 for Expert).</param>
/// <param name="PuzzleId">The puzzle this link relates to.</param>
/// <param name="HintId">The specific hint to reveal.</param>
/// <param name="RevealCondition">Optional prerequisite condition (item ID, prior hint ID, or null).</param>
public readonly record struct ExaminationHintLink(
    string ObjectId,
    int RequiredLayer,
    string PuzzleId,
    string HintId,
    string? RevealCondition)
{
    /// <summary>
    /// Gets whether this link has a prerequisite condition.
    /// </summary>
    public bool HasCondition => !string.IsNullOrEmpty(RevealCondition);

    /// <summary>
    /// Gets whether this link requires expert examination (Layer 3).
    /// </summary>
    public bool RequiresExpertExamination => RequiredLayer >= 3;

    /// <summary>
    /// Gets whether this link requires detailed examination (Layer 2+).
    /// </summary>
    public bool RequiresDetailedExamination => RequiredLayer >= 2;

    /// <summary>
    /// Checks if the required layer has been achieved.
    /// </summary>
    /// <param name="achievedLayer">The layer achieved by the examination.</param>
    /// <returns>True if the achieved layer meets or exceeds the requirement.</returns>
    public bool IsLayerSatisfied(int achievedLayer) => achievedLayer >= RequiredLayer;

    /// <summary>
    /// Gets a summary for logging.
    /// </summary>
    public override string ToString() =>
        $"ExaminationHintLink({ObjectId} -> {HintId}, Layer {RequiredLayer})";
}
