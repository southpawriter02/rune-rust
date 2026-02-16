using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Defines the contract for Berserkr Rage resource management.
/// Handles per-character Rage tracking including gain, spend, decay, and threshold transitions.
/// </summary>
/// <remarks>
/// <para>Named <c>IBerserkrRageService</c> to avoid collision with the existing
/// <c>IRageService</c> from v0.18.4d which handles general rage mechanics.</para>
/// <para>Key responsibilities:</para>
/// <list type="bullet">
/// <item>Initialize and track per-character Rage resources</item>
/// <item>Apply Rage gains from various sources (damage taken, bloodied enemies, abilities)</item>
/// <item>Spend Rage for ability activation (Fury Strike, Unstoppable, Intimidating Presence)</item>
/// <item>Decay Rage outside combat at a fixed rate</item>
/// <item>Log threshold transitions for combat awareness</item>
/// </list>
/// </remarks>
public interface IBerserkrRageService
{
    /// <summary>
    /// Initializes the Rage resource for a character at zero Rage.
    /// </summary>
    /// <param name="player">The Berserkr player to initialize.</param>
    /// <remarks>
    /// Should be called when a player selects the Berserkr specialization
    /// or at the start of a new game session.
    /// </remarks>
    void InitializeRage(Player player);

    /// <summary>
    /// Gets the current Rage resource for a character.
    /// </summary>
    /// <param name="player">The player to query.</param>
    /// <returns>
    /// The player's <see cref="RageResource"/> if initialized;
    /// null if the player has no Rage resource.
    /// </returns>
    RageResource? GetRage(Player player);

    /// <summary>
    /// Adds Rage from a specified source, capped at maximum.
    /// Logs threshold transitions when Rage crosses a level boundary.
    /// </summary>
    /// <param name="player">The Berserkr player gaining Rage.</param>
    /// <param name="amount">Amount of Rage to add. Must be positive.</param>
    /// <param name="source">Descriptive source of the Rage gain (e.g., "Pain is Fuel").</param>
    /// <returns>The actual amount of Rage gained (may be less if capped).</returns>
    int AddRage(Player player, int amount, string source);

    /// <summary>
    /// Attempts to spend the specified amount of Rage for ability activation.
    /// </summary>
    /// <param name="player">The Berserkr player spending Rage.</param>
    /// <param name="amount">Amount of Rage to spend. Must be positive.</param>
    /// <returns>True if sufficient Rage was available and spent; False otherwise.</returns>
    /// <remarks>
    /// Spending is atomic — if insufficient Rage is available,
    /// no Rage is deducted and False is returned.
    /// </remarks>
    bool SpendRage(Player player, int amount);

    /// <summary>
    /// Applies out-of-combat Rage decay (fixed 10 Rage per round).
    /// </summary>
    /// <param name="player">The Berserkr player whose Rage should decay.</param>
    /// <returns>The actual amount of Rage lost.</returns>
    int DecayRageOutOfCombat(Player player);

    /// <summary>
    /// Resets a character's Rage to zero. Used at session end or rest.
    /// </summary>
    /// <param name="player">The Berserkr player to reset.</param>
    void ResetRage(Player player);

    /// <summary>
    /// Gets all characters currently in the Enraged state (Rage 80+).
    /// Used for combat-wide Corruption risk assessment.
    /// </summary>
    /// <param name="players">Collection of players to check.</param>
    /// <returns>Enumerable of players with Rage at or above the Enraged threshold.</returns>
    IEnumerable<Player> GetEnragedCharacters(IEnumerable<Player> players);
}
