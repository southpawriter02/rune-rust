namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents a hint discovered through expert examination, providing clues
/// to puzzle solutions and optionally unlocking new interactions.
/// </summary>
/// <remarks>
/// <para>
/// Examination puzzle hints are discovered when players achieve Layer 3 (Expert)
/// examination results on objects with linked hints. Hints vary in subtlety from
/// Level 1 (cryptic) to Level 3 (obvious).
/// </para>
/// <para>
/// This is distinct from the riddle-based PuzzleHint class used by the riddle system.
/// ExaminationPuzzleHint specifically integrates with the examination perception system.
/// </para>
/// </remarks>
/// <param name="HintId">Unique identifier for this hint (e.g., "hint-env-console-override-01").</param>
/// <param name="PuzzleId">The puzzle this hint relates to (e.g., "puzzle-level7-door").</param>
/// <param name="HintLevel">Subtlety level: 1 = subtle/cryptic, 2 = moderate, 3 = obvious/direct.</param>
/// <param name="RevealedBy">How this hint is discovered: "examination", "perception", or "dialogue".</param>
/// <param name="HintText">The hint text shown to the player upon discovery.</param>
/// <param name="UnlocksInteraction">Optional new command unlocked by this hint.</param>
public readonly record struct ExaminationPuzzleHint(
    string HintId,
    string PuzzleId,
    int HintLevel,
    string RevealedBy,
    string HintText,
    string? UnlocksInteraction)
{
    /// <summary>
    /// Gets whether this hint unlocks a new interaction/command.
    /// </summary>
    public bool HasInteractionUnlock => !string.IsNullOrEmpty(UnlocksInteraction);

    /// <summary>
    /// Gets whether this is a subtle hint (Level 1).
    /// </summary>
    public bool IsSubtle => HintLevel == 1;

    /// <summary>
    /// Gets whether this is a moderate hint (Level 2).
    /// </summary>
    public bool IsModerate => HintLevel == 2;

    /// <summary>
    /// Gets whether this is an obvious hint (Level 3).
    /// </summary>
    public bool IsObvious => HintLevel == 3;

    /// <summary>
    /// Gets whether this hint is revealed through examination.
    /// </summary>
    public bool IsExaminationHint => RevealedBy.Equals("examination", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Creates a display string for hint revelation.
    /// </summary>
    /// <returns>A formatted string for displaying the hint discovery.</returns>
    public string ToRevealString()
    {
        var interactionNote = HasInteractionUnlock
            ? $"\n[New command available: {UnlocksInteraction}]"
            : "";
        return $"[Hint revealed: {HintText}]{interactionNote}";
    }

    /// <summary>
    /// Gets the hint level description for display.
    /// </summary>
    /// <returns>A description of the hint's subtlety level.</returns>
    public string GetLevelDescription() => HintLevel switch
    {
        1 => "Cryptic",
        2 => "Moderate",
        3 => "Direct",
        _ => "Unknown"
    };

    /// <summary>
    /// Gets a summary for logging.
    /// </summary>
    public override string ToString() =>
        $"ExaminationPuzzleHint({HintId}, Level {HintLevel}, Puzzle {PuzzleId})";
}
