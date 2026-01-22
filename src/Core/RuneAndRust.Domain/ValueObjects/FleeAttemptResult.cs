namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Result of an opportunity attack made during movement or failed flee attempt.
/// </summary>
/// <param name="AttackerId">The unique identifier of the attacking entity.</param>

/// <param name="AttackerName">The display name of the attacking entity.</param>
/// <param name="Hit">Whether the attack hit.</param>
/// <param name="Damage">The amount of damage dealt (0 if miss).</param>
/// <param name="AttackRoll">Details of the attack roll for display.</param>
/// <param name="TargetRemainingHp">Target's HP after the attack.</param>
/// <param name="TargetDied">Whether the target died from this attack.</param>
/// <param name="Message">Human-readable message describing the attack.</param>
public readonly record struct OpportunityAttackResult(
    Guid AttackerId,
    string AttackerName,
    bool Hit,
    int Damage,
    string AttackRoll = "",
    int TargetRemainingHp = 0,
    bool TargetDied = false,
    string Message = "");


/// <summary>
/// Result of an attempt to flee combat.
/// </summary>
/// <remarks>
/// <para>FleeAttemptResult captures the complete outcome of a flee attempt:</para>
/// <list type="bullet">
/// <item>The skill check result (roll, modifiers, total)</item>
/// <item>Whether the flee succeeded</item>
/// <item>Opportunity attacks taken on failure</item>
/// <item>The destination room if successful</item>
/// </list>
/// </remarks>
/// <param name="Success">Whether the flee attempt succeeded.</param>
/// <param name="SkillCheck">The skill check result.</param>
/// <param name="DifficultyClass">The DC that had to be beaten.</param>
/// <param name="OpportunityAttacks">Attacks taken while fleeing (on failure).</param>
/// <param name="TotalDamageTaken">Total damage from opportunity attacks.</param>
/// <param name="DestinationRoomId">The room fled to (if successful).</param>
public readonly record struct FleeAttemptResult(
    bool Success,
    SkillCheckResult SkillCheck,
    int DifficultyClass,
    IReadOnlyList<OpportunityAttackResult> OpportunityAttacks,
    int TotalDamageTaken,
    Guid? DestinationRoomId)
{
    /// <summary>
    /// Creates a successful flee result.
    /// </summary>
    /// <param name="check">The successful skill check.</param>
    /// <param name="dc">The difficulty class that was beaten.</param>
    /// <param name="destinationRoomId">The room the combatant fled to.</param>
    /// <returns>A successful flee result.</returns>
    public static FleeAttemptResult Succeeded(
        SkillCheckResult check,
        int dc,
        Guid destinationRoomId) =>
        new(true, check, dc, Array.Empty<OpportunityAttackResult>(), 0, destinationRoomId);

    /// <summary>
    /// Creates a failed flee result with opportunity attacks.
    /// </summary>
    /// <param name="check">The failed skill check.</param>
    /// <param name="dc">The difficulty class that wasn't beaten.</param>
    /// <param name="attacks">List of opportunity attacks taken.</param>
    /// <param name="totalDamage">Total damage from all opportunity attacks.</param>
    /// <returns>A failed flee result.</returns>
    public static FleeAttemptResult Failed(
        SkillCheckResult check,
        int dc,
        IReadOnlyList<OpportunityAttackResult> attacks,
        int totalDamage) =>
        new(false, check, dc, attacks, totalDamage, null);

    /// <summary>
    /// Returns a display string for the flee result.
    /// </summary>
    /// <remarks>
    /// v0.15.0c: Uses NetSuccesses (success-counting) instead of TotalResult.
    /// </remarks>
    public override string ToString()
    {
        var result = Success ? "SUCCESS" : "FAILED";
        return $"Flee {result}: {SkillCheck.NetSuccesses} net vs DC {DifficultyClass}";
    }
}
