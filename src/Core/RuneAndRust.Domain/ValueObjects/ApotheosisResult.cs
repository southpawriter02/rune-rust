namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the result of an Apotheosis state transition.
/// </summary>
/// <remarks>
/// <para>
/// Tracks entry into, maintenance of, and exit from Apotheosis state.
/// Apotheosis is the ultimate magical state for Arcanists, granting
/// +5 spell power, +20% crit chance, and ultimate abilities.
/// </para>
/// <para>
/// Apotheosis costs 10 stress per turn to maintain and auto-exits if stress reaches 100.
/// Cannot voluntarily exit during combat.
/// </para>
/// </remarks>
/// <param name="EnteredApotheosis">Whether character just entered Apotheosis.</param>
/// <param name="TurnsRemaining">Estimated turns remaining in Apotheosis based on stress budget.</param>
/// <param name="AbilitiesUnlocked">List of newly unlocked ultimate ability IDs.</param>
/// <param name="StressCostPerTurn">Stress cost per turn (constant 10).</param>
/// <param name="ExitedApotheosis">Whether character exited Apotheosis.</param>
/// <param name="ExitReason">Reason for exit if applicable (e.g., "Stress max", "Coherence drop", "Combat ended").</param>
/// <param name="FinalCoherence">Final coherence value after transition.</param>
public record ApotheosisResult(
    bool EnteredApotheosis,
    int? TurnsRemaining,
    IReadOnlyList<string>? AbilitiesUnlocked,
    int StressCostPerTurn,
    bool ExitedApotheosis,
    string? ExitReason,
    int FinalCoherence)
{
    /// <summary>
    /// Gets an Apotheosis result representing no state change.
    /// </summary>
    /// <param name="currentCoherence">The current coherence value.</param>
    /// <returns>A result indicating no Apotheosis state change.</returns>
    public static ApotheosisResult NoChange(int currentCoherence) => new(
        EnteredApotheosis: false,
        TurnsRemaining: null,
        AbilitiesUnlocked: null,
        StressCostPerTurn: 0,
        ExitedApotheosis: false,
        ExitReason: null,
        FinalCoherence: currentCoherence);

    /// <summary>
    /// Gets whether state changed (entered or exited).
    /// </summary>
    public bool StateChanged => EnteredApotheosis || ExitedApotheosis;

    /// <summary>
    /// Gets whether abilities were newly unlocked.
    /// </summary>
    public bool AbilitiesUnlockedThisTransition =>
        AbilitiesUnlocked != null && AbilitiesUnlocked.Count > 0;

    /// <summary>
    /// Gets whether the character is currently in Apotheosis state.
    /// </summary>
    /// <remarks>
    /// True if entered and not exited, or if TurnsRemaining has a value.
    /// </remarks>
    public bool IsActive => EnteredApotheosis && !ExitedApotheosis;

    /// <summary>
    /// Gets the count of abilities unlocked.
    /// </summary>
    public int AbilityUnlockCount => AbilitiesUnlocked?.Count ?? 0;

    /// <summary>
    /// Creates a display string for this Apotheosis transition.
    /// </summary>
    /// <returns>A human-readable description of the Apotheosis state.</returns>
    public string ToDisplayString()
    {
        if (EnteredApotheosis)
        {
            var abilities = AbilitiesUnlockedThisTransition
                ? $" ({string.Join(", ", AbilitiesUnlocked!)})"
                : "";
            var turnsInfo = TurnsRemaining.HasValue
                ? $", ~{TurnsRemaining} turns"
                : "";
            return $"APOTHEOSIS ENTERED{abilities} (Cost: {StressCostPerTurn}/turn{turnsInfo})";
        }

        if (ExitedApotheosis)
        {
            return $"Apotheosis ended: {ExitReason ?? "Unknown"} (Coherence: {FinalCoherence})";
        }

        if (TurnsRemaining.HasValue)
        {
            return $"Apotheosis active (~{TurnsRemaining} turns remaining)";
        }

        return $"Not in Apotheosis (Coherence: {FinalCoherence})";
    }
}
