using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Defines the contract for Seiðkona Aether Resonance resource management.
/// Handles per-character Resonance tracking including build, reset, and accumulated
/// Aetheric damage tracking for the Unraveling capstone.
/// </summary>
/// <remarks>
/// <para>Named <c>ISeidkonaResonanceService</c> following per-specialization naming convention
/// to avoid collision with other resource services.</para>
/// <para>Key responsibilities:</para>
/// <list type="bullet">
/// <item>Initialize and track per-character Aether Resonance (0–10)</item>
/// <item>Build Resonance through casting (primarily Seiðr Bolt, +1 per cast)</item>
/// <item>Reset Resonance (Unraveling capstone or character death)</item>
/// <item>Track Accumulated Aetheric Damage for Unraveling payoff</item>
/// <item>Log threshold crossings when Resonance enters higher risk tiers</item>
/// </list>
/// <para>Unlike <see cref="IBerserkrRageService"/>, Resonance is never spent by normal abilities —
/// it only builds, and only the Unraveling capstone (v0.20.8c) resets it. This creates
/// an escalating Corruption risk that cannot be mitigated through ability usage.</para>
/// </remarks>
public interface ISeidkonaResonanceService
{
    /// <summary>
    /// Initializes the Aether Resonance resource and Accumulated Aetheric Damage tracker
    /// for a character at zero values.
    /// </summary>
    /// <param name="player">The Seiðkona player to initialize.</param>
    /// <remarks>
    /// Should be called when a player selects the Seiðkona specialization
    /// or at the start of a new game session.
    /// </remarks>
    void InitializeResonance(Player player);

    /// <summary>
    /// Gets the current Aether Resonance resource for a character.
    /// </summary>
    /// <param name="player">The player to query.</param>
    /// <returns>
    /// The player's <see cref="AetherResonanceResource"/> if initialized;
    /// null if the player has no Resonance resource.
    /// </returns>
    AetherResonanceResource? GetResonance(Player player);

    /// <summary>
    /// Builds Aether Resonance from a specified source, capped at maximum (10).
    /// Logs threshold crossings when Resonance enters higher Corruption risk tiers.
    /// </summary>
    /// <param name="player">The Seiðkona player building Resonance.</param>
    /// <param name="amount">Amount of Resonance to build. Must be positive.</param>
    /// <param name="source">Descriptive source of the Resonance build (e.g., "Seiðr Bolt cast").</param>
    /// <returns>The actual amount of Resonance gained (may be less if capped at max).</returns>
    int BuildResonance(Player player, int amount, string source);

    /// <summary>
    /// Resets a character's Aether Resonance to zero.
    /// Called by the Unraveling capstone (v0.20.8c) or upon character death.
    /// </summary>
    /// <param name="player">The Seiðkona player to reset.</param>
    /// <param name="source">Descriptive source of the reset (e.g., "Unraveling capstone").</param>
    void ResetResonance(Player player, string source);

    /// <summary>
    /// Gets the current Accumulated Aetheric Damage tracker for a character.
    /// </summary>
    /// <param name="player">The player to query.</param>
    /// <returns>
    /// The player's <see cref="AccumulatedAethericDamage"/> if initialized;
    /// null if the player has no damage tracker.
    /// </returns>
    AccumulatedAethericDamage? GetAccumulatedDamage(Player player);

    /// <summary>
    /// Adds Aetheric damage to the accumulated tracker for the Unraveling capstone.
    /// Called after each successful Seiðr Bolt or similar Aetheric ability.
    /// </summary>
    /// <param name="player">The Seiðkona player accumulating damage.</param>
    /// <param name="damage">The Aetheric damage dealt by the cast.</param>
    void AddAccumulatedDamage(Player player, int damage);

    /// <summary>
    /// Resets the Accumulated Aetheric Damage tracker to zero.
    /// Called by the Unraveling capstone after releasing all accumulated damage,
    /// or upon character death.
    /// </summary>
    /// <param name="player">The Seiðkona player to reset.</param>
    void ResetAccumulatedDamage(Player player);
}
