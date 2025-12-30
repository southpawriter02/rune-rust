using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Models;
using CharacterEntity = RuneAndRust.Core.Entities.Character;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Service for managing faction reputation and disposition.
/// </summary>
/// <remarks>See: v0.4.2a (The Repute) for Faction System implementation.</remarks>
public interface IFactionService
{
    // ═══════════════════════════════════════════════════════════════════════
    // Reputation Modification
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Modifies a character's reputation with a faction.
    /// Clamps result to [-100, +100] and fires events as appropriate.
    /// </summary>
    /// <param name="character">The character to modify.</param>
    /// <param name="faction">The faction to modify reputation with.</param>
    /// <param name="amount">The amount to add (positive) or subtract (negative).</param>
    /// <param name="source">Optional source description for logging/events.</param>
    /// <returns>Result containing before/after state.</returns>
    Task<ReputationChangeResult> ModifyReputationAsync(
        CharacterEntity character,
        FactionType faction,
        int amount,
        string? source = null);

    /// <summary>
    /// Sets a character's reputation to a specific value.
    /// Used for initialization or special game events.
    /// </summary>
    /// <param name="character">The character to modify.</param>
    /// <param name="faction">The faction to set reputation with.</param>
    /// <param name="value">The exact value to set (clamped to [-100, +100]).</param>
    /// <param name="source">Optional source description.</param>
    /// <returns>Result containing before/after state.</returns>
    Task<ReputationChangeResult> SetReputationAsync(
        CharacterEntity character,
        FactionType faction,
        int value,
        string? source = null);

    // ═══════════════════════════════════════════════════════════════════════
    // Reputation Queries
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets a character's current reputation value with a faction.
    /// Returns 0 if no standing exists (or faction's default reputation).
    /// </summary>
    /// <param name="character">The character to query.</param>
    /// <param name="faction">The faction to query.</param>
    /// <returns>The reputation value (-100 to +100).</returns>
    Task<int> GetReputationAsync(CharacterEntity character, FactionType faction);

    /// <summary>
    /// Gets the disposition tier for a given reputation value.
    /// Pure function, does not require database access.
    /// </summary>
    /// <param name="reputation">The reputation value.</param>
    /// <returns>The corresponding disposition tier.</returns>
    Disposition GetDisposition(int reputation);

    /// <summary>
    /// Gets complete faction standing info for a character.
    /// Combines reputation and disposition with faction metadata.
    /// </summary>
    /// <param name="character">The character to query.</param>
    /// <param name="faction">The faction to query.</param>
    /// <returns>Complete standing info.</returns>
    Task<FactionStandingInfo> GetFactionStandingAsync(CharacterEntity character, FactionType faction);

    /// <summary>
    /// Gets all faction standings for a character.
    /// Returns standings for all factions, including defaults for untracked ones.
    /// </summary>
    /// <param name="character">The character to query.</param>
    /// <returns>Dictionary of faction type to standing info.</returns>
    Task<IReadOnlyDictionary<FactionType, FactionStandingInfo>> GetAllStandingsAsync(CharacterEntity character);

    // ═══════════════════════════════════════════════════════════════════════
    // Convenience Checks
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Checks if a character is hostile with a faction (Hated or Hostile disposition).
    /// </summary>
    Task<bool> IsHostileAsync(CharacterEntity character, FactionType faction);

    /// <summary>
    /// Checks if a character is friendly with a faction (Friendly or Exalted disposition).
    /// </summary>
    Task<bool> IsFriendlyAsync(CharacterEntity character, FactionType faction);

    /// <summary>
    /// Checks if a character meets a minimum disposition requirement.
    /// </summary>
    /// <param name="character">The character to check.</param>
    /// <param name="faction">The faction to check.</param>
    /// <param name="minDisposition">The minimum required disposition.</param>
    /// <returns>True if character's disposition >= minDisposition.</returns>
    Task<bool> MeetsDispositionRequirementAsync(
        CharacterEntity character,
        FactionType faction,
        Disposition minDisposition);

    // ═══════════════════════════════════════════════════════════════════════
    // Faction Metadata
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets metadata for a faction (name, description, default reputation).
    /// </summary>
    Task<Faction?> GetFactionAsync(FactionType faction);

    /// <summary>
    /// Gets all faction definitions.
    /// </summary>
    Task<IEnumerable<Faction>> GetAllFactionsAsync();
}
