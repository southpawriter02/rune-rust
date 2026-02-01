// ═══════════════════════════════════════════════════════════════════════════════
// TraumaCheckResult.cs
// Immutable record representing the complete outcome of a trauma check roll.
// Version: 0.18.3c
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.Records;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the result of a trauma check roll.
/// </summary>
/// <remarks>
/// <para>
/// This record contains all information about a trauma check, including
/// the dice rolled, successes achieved, and whether a trauma was acquired.
/// </para>
/// <para>
/// Check Resolution:
/// <list type="bullet">
/// <item>Roll RESOLVE attribute dice pool (base or reduced by corruption)</item>
/// <item>Count successes (each die showing 4+ is one success)</item>
/// <item>Compare to SuccessesNeeded for this trigger</item>
/// <item>If successes &lt; required: character acquires random trauma</item>
/// <item>If successes &gt;= required: character avoids trauma this time</item>
/// </list>
/// </para>
/// <para>
/// Corruption Penalty Formula:
/// <c>penalty = floor(corruption_value / 20)</c>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var result = TraumaCheckResult.CreatePassed(
///     characterId: characterId,
///     trigger: TraumaCheckTrigger.AllyDeath,
///     diceRolled: 3,
///     successesNeeded: 2,
///     successesAchieved: 2,
///     modifiers: new[] { "Grieving Bond" }
/// );
/// </code>
/// </example>
public record TraumaCheckResult
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Properties
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the character making the check.
    /// </summary>
    public Guid CharacterId { get; init; }

    /// <summary>
    /// Gets the trigger event for this check.
    /// </summary>
    public TraumaCheckTrigger Trigger { get; init; }

    /// <summary>
    /// Gets the number of dice rolled.
    /// </summary>
    /// <remarks>
    /// Base = RESOLVE attribute value.
    /// Reduced by floor(corruption/20).
    /// Minimum of 1 die.
    /// </remarks>
    public int DiceRolled { get; init; }

    /// <summary>
    /// Gets the successes needed to pass this check.
    /// </summary>
    /// <remarks>
    /// Determined by trigger type (1-4):
    /// <list type="bullet">
    /// <item>CriticalFailure: 1</item>
    /// <item>AllyDeath, NearDeathExperience, ProlongedExposure: 2</item>
    /// <item>StressThreshold100, WitnessingHorror: 3</item>
    /// <item>CorruptionThreshold100, RuinMadnessEscape: 4</item>
    /// </list>
    /// </remarks>
    public int SuccessesNeeded { get; init; }

    /// <summary>
    /// Gets the successes achieved on the roll.
    /// </summary>
    public int SuccessesAchieved { get; init; }

    /// <summary>
    /// Gets whether the check was passed.
    /// </summary>
    /// <remarks>
    /// <para>True if SuccessesAchieved &gt;= SuccessesNeeded.</para>
    /// <para>False = character acquires trauma.</para>
    /// </remarks>
    public bool Passed { get; init; }

    /// <summary>
    /// Gets the trauma ID acquired (if check failed).
    /// </summary>
    /// <remarks>
    /// Null if check passed or no suitable trauma exists.
    /// Matches TraumaDefinition.TraumaId (e.g., "survivors-guilt").
    /// </remarks>
    public string? TraumaAcquired { get; init; }

    /// <summary>
    /// Gets the modifiers applied to this check.
    /// </summary>
    /// <remarks>
    /// Examples: "Grieving Bond", "Corrupted Mind", "Strong Will".
    /// Modifiers affect dice pool or difficulty.
    /// </remarks>
    public IReadOnlyList<string> Modifiers { get; init; }

    /// <summary>
    /// Gets the number of dice removed due to corruption.
    /// </summary>
    /// <remarks>
    /// Calculated as: floor(corruption/20).
    /// Range: 0-5 depending on corruption level.
    /// </remarks>
    public int ResolvePenaltyApplied { get; init; }

    // ═══════════════════════════════════════════════════════════════════════════
    // Constructors
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of the <see cref="TraumaCheckResult"/> record.
    /// </summary>
    /// <remarks>
    /// Use factory methods (<see cref="CreatePassed"/>, <see cref="CreateFailed"/>)
    /// instead of this constructor directly.
    /// </remarks>
    private TraumaCheckResult()
    {
        Modifiers = new List<string>();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Factory Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a passed trauma check result.
    /// </summary>
    /// <param name="characterId">Character making check.</param>
    /// <param name="trigger">Trigger event type.</param>
    /// <param name="diceRolled">Dice rolled.</param>
    /// <param name="successesNeeded">Successes needed.</param>
    /// <param name="successesAchieved">Successes rolled.</param>
    /// <param name="modifiers">Applied modifiers.</param>
    /// <param name="resolvePenalty">Corruption penalty applied.</param>
    /// <returns>A passed TraumaCheckResult.</returns>
    /// <example>
    /// <code>
    /// var result = TraumaCheckResult.CreatePassed(
    ///     characterId: characterId,
    ///     trigger: TraumaCheckTrigger.AllyDeath,
    ///     diceRolled: 3,
    ///     successesNeeded: 2,
    ///     successesAchieved: 2,
    ///     modifiers: new[] { "Strong Will" },
    ///     resolvePenalty: 0
    /// );
    /// // result.Passed == true
    /// // result.TraumaAcquired == null
    /// </code>
    /// </example>
    public static TraumaCheckResult CreatePassed(
        Guid characterId,
        TraumaCheckTrigger trigger,
        int diceRolled,
        int successesNeeded,
        int successesAchieved,
        IReadOnlyList<string>? modifiers = null,
        int resolvePenalty = 0)
    {
        return new TraumaCheckResult
        {
            CharacterId = characterId,
            Trigger = trigger,
            DiceRolled = diceRolled,
            SuccessesNeeded = successesNeeded,
            SuccessesAchieved = successesAchieved,
            Passed = true,
            TraumaAcquired = null,
            Modifiers = modifiers ?? new List<string>(),
            ResolvePenaltyApplied = resolvePenalty
        };
    }

    /// <summary>
    /// Creates a failed trauma check result with trauma acquired.
    /// </summary>
    /// <param name="characterId">Character making check.</param>
    /// <param name="trigger">Trigger event type.</param>
    /// <param name="diceRolled">Dice rolled.</param>
    /// <param name="successesNeeded">Successes needed.</param>
    /// <param name="successesAchieved">Successes rolled.</param>
    /// <param name="traumaAcquired">Trauma ID acquired.</param>
    /// <param name="modifiers">Applied modifiers.</param>
    /// <param name="resolvePenalty">Corruption penalty applied.</param>
    /// <returns>A failed TraumaCheckResult.</returns>
    /// <example>
    /// <code>
    /// var result = TraumaCheckResult.CreateFailed(
    ///     characterId: characterId,
    ///     trigger: TraumaCheckTrigger.AllyDeath,
    ///     diceRolled: 2,
    ///     successesNeeded: 2,
    ///     successesAchieved: 0,
    ///     traumaAcquired: "survivors-guilt",
    ///     modifiers: new[] { "Corrupted Mind" },
    ///     resolvePenalty: 1
    /// );
    /// // result.Passed == false
    /// // result.TraumaAcquired == "survivors-guilt"
    /// </code>
    /// </example>
    public static TraumaCheckResult CreateFailed(
        Guid characterId,
        TraumaCheckTrigger trigger,
        int diceRolled,
        int successesNeeded,
        int successesAchieved,
        string traumaAcquired,
        IReadOnlyList<string>? modifiers = null,
        int resolvePenalty = 0)
    {
        return new TraumaCheckResult
        {
            CharacterId = characterId,
            Trigger = trigger,
            DiceRolled = diceRolled,
            SuccessesNeeded = successesNeeded,
            SuccessesAchieved = successesAchieved,
            Passed = false,
            TraumaAcquired = traumaAcquired,
            Modifiers = modifiers ?? new List<string>(),
            ResolvePenaltyApplied = resolvePenalty
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ToString Override
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the display string for this result.
    /// </summary>
    /// <returns>Human-readable summary of the check result.</returns>
    public override string ToString() =>
        $"{Trigger}: {DiceRolled}d[RESOLVE] → {SuccessesAchieved}/{SuccessesNeeded} " +
        (Passed ? "PASSED" : $"FAILED → {TraumaAcquired}") +
        (ResolvePenaltyApplied > 0 ? $" [-{ResolvePenaltyApplied} corruption]" : "");
}
