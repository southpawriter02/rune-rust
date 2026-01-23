namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Defines operations for managing chase encounters.
/// </summary>
/// <remarks>
/// <para>
/// The chase service orchestrates chase sequences from initiation through
/// resolution. It generates obstacles, processes rounds, and determines
/// outcomes based on participant skill checks and distance tracking.
/// </para>
/// <para>
/// <b>Chase Distance Track:</b>
/// <list type="bullet">
///   <item><description>0: Caught - Pursuer catches fleeing character</description></item>
///   <item><description>1-2: Close - Pursuer can attempt capture</description></item>
///   <item><description>3: Near - Default starting position</description></item>
///   <item><description>4-5: Far - Fleeing character pulling ahead</description></item>
///   <item><description>6+: Escaped - Fleeing character escapes</description></item>
/// </list>
/// </para>
/// <para>
/// <b>v0.15.2e:</b> Initial implementation of chase service contract.
/// </para>
/// </remarks>
public interface IChaseService
{
    /// <summary>
    /// Initiates a new chase encounter between two characters.
    /// </summary>
    /// <param name="fleeingId">ID of the character attempting to flee.</param>
    /// <param name="pursuerId">ID of the character pursuing.</param>
    /// <param name="startDistance">Starting position on distance track (default 3).</param>
    /// <param name="maxRounds">Optional maximum round limit.</param>
    /// <returns>The newly created chase state.</returns>
    /// <exception cref="ArgumentException">If character IDs are invalid.</exception>
    ChaseState StartChase(
        string fleeingId,
        string pursuerId,
        int startDistance = 3,
        int? maxRounds = null);

    /// <summary>
    /// Generates an appropriate obstacle for the current chase round.
    /// </summary>
    /// <param name="chaseId">ID of the chase to generate an obstacle for.</param>
    /// <param name="environmentTag">Optional environment context for obstacle selection.</param>
    /// <returns>A chase obstacle for both participants to attempt.</returns>
    /// <exception cref="InvalidOperationException">If chase is not in progress or not found.</exception>
    ChaseObstacle GenerateObstacle(string chaseId, string? environmentTag = null);

    /// <summary>
    /// Processes a complete chase round with both participants' attempts.
    /// </summary>
    /// <param name="chaseId">ID of the chase to process.</param>
    /// <param name="fleeingDicePool">The dice pool size for the fleeing character.</param>
    /// <param name="pursuerDicePool">The dice pool size for the pursuer.</param>
    /// <returns>The result of the round including distance changes and status.</returns>
    /// <exception cref="InvalidOperationException">If chase is not in progress or not found.</exception>
    ChaseRoundResult ProcessRound(
        string chaseId,
        int fleeingDicePool,
        int pursuerDicePool);

    /// <summary>
    /// Marks a chase as abandoned by the specified character.
    /// </summary>
    /// <param name="chaseId">ID of the chase to abandon.</param>
    /// <param name="byCharacterId">ID of the character abandoning.</param>
    /// <exception cref="InvalidOperationException">If chase is not in progress or not found.</exception>
    /// <exception cref="ArgumentException">If character is not a participant.</exception>
    void AbandonChase(string chaseId, string byCharacterId);

    /// <summary>
    /// Gets the current state of a chase encounter.
    /// </summary>
    /// <param name="chaseId">ID of the chase to retrieve.</param>
    /// <returns>The chase state, or null if not found.</returns>
    ChaseState? GetChase(string chaseId);

    /// <summary>
    /// Gets all active (in progress) chase encounters.
    /// </summary>
    /// <returns>Collection of active chase states.</returns>
    IReadOnlyList<ChaseState> GetActiveChases();

    /// <summary>
    /// Gets the current zone description for a chase.
    /// </summary>
    /// <param name="chaseId">ID of the chase.</param>
    /// <returns>Zone name (Caught, Close, Near, Far, Lost) or null if not found.</returns>
    string? GetCurrentZone(string chaseId);

    /// <summary>
    /// Calculates the distance change for a given skill outcome.
    /// </summary>
    /// <param name="outcome">The skill check outcome.</param>
    /// <param name="isFleeingCharacter">Whether this is for the fleeing character.</param>
    /// <returns>The distance change value.</returns>
    int CalculateDistanceChange(Domain.Enums.SkillOutcome outcome, bool isFleeingCharacter);
}
