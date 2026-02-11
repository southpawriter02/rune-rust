// ═══════════════════════════════════════════════════════════════════════════════
// IBerserkrRageService.cs
// Interface for managing the Berserkr specialization's Rage resource,
// including generation, spending, decay, and threshold monitoring.
// Distinct from IRageService (v0.18.4d Trauma Economy) — this is
// specialization-specific resource management.
// Version: 0.20.5a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Service interface for managing the Berserkr specialization's Rage resource.
/// </summary>
/// <remarks>
/// <para>
/// This interface is distinct from <see cref="IRageService"/> (v0.18.4d Trauma Economy).
/// While <see cref="IRageService"/> manages Rage as a general combat emotion resource
/// with damage/soak bonuses, this interface manages the Berserkr's specialization-specific
/// Rage that fuels Fury Strike and other Berserkr abilities.
/// </para>
/// <para>
/// Key operations:
/// </para>
/// <list type="bullet">
///   <item><description><b>Initialize:</b> Set up Rage tracking for a Berserkr character</description></item>
///   <item><description><b>AddRage:</b> Gain Rage from Pain is Fuel, Blood Scent, etc.</description></item>
///   <item><description><b>SpendRage:</b> Consume Rage for Fury Strike and other abilities</description></item>
///   <item><description><b>DecayRage:</b> Reduce Rage during out-of-combat periods</description></item>
///   <item><description><b>Monitor:</b> Query Rage levels and Enraged state</description></item>
/// </list>
/// </remarks>
/// <seealso cref="RageResource"/>
/// <seealso cref="IBerserkrAbilityService"/>
public interface IBerserkrRageService
{
    /// <summary>
    /// Initializes Rage tracking for a Berserkr character.
    /// Creates a new <see cref="RageResource"/> at 0/100.
    /// </summary>
    /// <param name="characterId">Character's unique identifier.</param>
    void InitializeRage(Guid characterId);

    /// <summary>
    /// Gets the current Rage resource for a character.
    /// </summary>
    /// <param name="characterId">Character's unique identifier.</param>
    /// <returns>The character's RageResource, or <c>null</c> if not initialized.</returns>
    RageResource? GetRage(Guid characterId);

    /// <summary>
    /// Adds Rage to a character's resource pool from a named source.
    /// </summary>
    /// <param name="characterId">Character's unique identifier.</param>
    /// <param name="amount">Number of Rage points to add. Must be positive.</param>
    /// <param name="source">Description of the Rage source (e.g., "Pain is Fuel").</param>
    void AddRage(Guid characterId, int amount, string source);

    /// <summary>
    /// Attempts to spend Rage for an ability activation.
    /// </summary>
    /// <param name="characterId">Character's unique identifier.</param>
    /// <param name="amount">Number of Rage points to spend. Must be positive.</param>
    /// <returns><c>true</c> if the Rage was successfully spent; <c>false</c> if insufficient.</returns>
    bool SpendRage(Guid characterId, int amount);

    /// <summary>
    /// Applies out-of-combat Rage decay for a character.
    /// Reduces Rage by <see cref="RageResource.OutOfCombatDecay"/> per call.
    /// </summary>
    /// <param name="characterId">Character's unique identifier.</param>
    void DecayRageOutOfCombat(Guid characterId);

    /// <summary>
    /// Resets a character's Rage to zero.
    /// </summary>
    /// <param name="characterId">Character's unique identifier.</param>
    void ResetRage(Guid characterId);

    /// <summary>
    /// Gets all character IDs currently in the Enraged state (80+ Rage).
    /// </summary>
    /// <returns>Enumerable of character IDs with 80+ Rage.</returns>
    IEnumerable<Guid> GetEnragedCharacters();
}
