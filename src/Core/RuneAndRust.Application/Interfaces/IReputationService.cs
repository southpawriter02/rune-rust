using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Service for managing player faction reputation.
/// </summary>
/// <remarks>
/// <para>Handles all reputation business logic including applying deltas with witness
/// multipliers, clamping to [-100, +100], detecting tier transitions, and emitting
/// game events for reputation changes.</para>
///
/// <para>This service operates on the <see cref="Player"/> entity directly, modifying
/// its faction reputation dictionary. It uses <see cref="IFactionDefinitionProvider"/>
/// to validate faction IDs and retrieve display names.</para>
///
/// <para><b>Usage in quest completion flow:</b></para>
/// <code>
/// // In GameSessionService after quest completion:
/// var results = _reputationService.ApplyReputationChanges(
///     player,
///     questReward.ReputationChanges,
///     WitnessContext.Direct);
///
/// foreach (var result in results)
/// {
///     session.AddEvent(GameEvent.Reputation("ReputationChanged", result.Message, player.Id));
///     if (result.TierChanged)
///         session.AddEvent(GameEvent.Reputation("TierChanged", result.TierTransitionMessage!, player.Id));
/// }
/// </code>
/// </remarks>
public interface IReputationService
{
    /// <summary>
    /// Applies reputation changes from a quest reward or game action.
    /// </summary>
    /// <param name="player">The player whose reputation to modify.</param>
    /// <param name="reputationChanges">
    /// Dictionary of faction ID → reputation delta. Positive values increase standing,
    /// negative values decrease it. Unknown faction IDs are logged and skipped.
    /// </param>
    /// <param name="witnessContext">
    /// Describes how the action was observed. The context's multiplier scales the raw delta
    /// before application (Direct = 100%, Witnessed = 75%, Unwitnessed = 0%).
    /// </param>
    /// <returns>
    /// A list of <see cref="ReputationChangeResult"/> objects, one per affected faction.
    /// Empty list if <paramref name="reputationChanges"/> is null or empty.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="player"/> is null.</exception>
    IReadOnlyList<ReputationChangeResult> ApplyReputationChanges(
        Player player,
        IReadOnlyDictionary<string, int> reputationChanges,
        WitnessContext witnessContext);

    /// <summary>
    /// Gets the player's current reputation tier with a faction.
    /// </summary>
    /// <param name="player">The player to query.</param>
    /// <param name="factionId">The faction ID (case-insensitive).</param>
    /// <returns>
    /// The <see cref="ReputationTier"/> for the faction.
    /// Returns <see cref="ReputationTier.Neutral"/> for unknown factions
    /// or factions where no reputation has been established.
    /// </returns>
    ReputationTier GetTier(Player player, string factionId);

    /// <summary>
    /// Checks whether the player meets a minimum reputation value requirement.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <param name="factionId">The faction ID (case-insensitive).</param>
    /// <param name="minimumValue">The minimum reputation value required (e.g., 25 for Friendly).</param>
    /// <returns>
    /// <c>true</c> if the player's reputation value with the faction is ≥ <paramref name="minimumValue"/>;
    /// <c>false</c> otherwise.
    /// </returns>
    bool MeetsReputationRequirement(Player player, string factionId, int minimumValue);

    /// <summary>
    /// Checks whether the player meets a minimum tier requirement with a faction.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <param name="factionId">The faction ID (case-insensitive).</param>
    /// <param name="minimumTier">The minimum tier required.</param>
    /// <returns>
    /// <c>true</c> if the player's reputation tier with the faction is ≥ <paramref name="minimumTier"/>;
    /// <c>false</c> otherwise.
    /// </returns>
    bool MeetsTierRequirement(Player player, string factionId, ReputationTier minimumTier);

    /// <summary>
    /// Gets the display name for a faction.
    /// </summary>
    /// <param name="factionId">The faction ID (case-insensitive).</param>
    /// <returns>
    /// The faction's display name (e.g., "Iron-Banes"), or the raw ID
    /// if the faction isn't found in configuration.
    /// </returns>
    string GetFactionName(string factionId);

    /// <summary>
    /// Gets all known faction IDs from the configuration.
    /// </summary>
    /// <returns>A read-only list of all faction IDs.</returns>
    IReadOnlyList<string> GetAllFactionIds();
}
