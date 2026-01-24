using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Service interface for scavenger sign interpretation operations in the Wasteland Survival system.
/// </summary>
/// <remarks>
/// <para>
/// Provides functionality for characters to interpret scavenger signs left by wasteland factions
/// using the Wasteland Survival skill. Signs can reveal territory boundaries, hidden caches,
/// safe paths, danger warnings, and more.
/// </para>
/// <para>
/// Interpretation outcomes:
/// <list type="bullet">
///   <item><description>Success (net successes >= DC): Sign type and meaning revealed</description></item>
///   <item><description>Critical (net successes >= 5): Additional context provided</description></item>
///   <item><description>Failure (net successes &lt; DC): "Markings incomprehensible"</description></item>
///   <item><description>Fumble (0 successes + botch): Misinterpretation with false info</description></item>
/// </list>
/// </para>
/// <para>
/// DC calculation:
/// <list type="bullet">
///   <item><description>Base DC: From sign type (10-14)</description></item>
///   <item><description>Unknown faction: +4 DC modifier</description></item>
///   <item><description>Sign age: +0 (Fresh/Recent), +1 (Old), +2 (Faded), +4 (Ancient)</description></item>
/// </list>
/// </para>
/// <para>
/// Sign types and base DCs:
/// <list type="bullet">
///   <item><description>TerritoryMarker: DC 10 (faction boundary warnings)</description></item>
///   <item><description>WarningSign: DC 12 (danger notifications)</description></item>
///   <item><description>CacheIndicator: DC 14 (hidden supply locations)</description></item>
///   <item><description>TrailBlaze: DC 10 (safe path markers)</description></item>
///   <item><description>HuntMarker: DC 14 (prey location indicators)</description></item>
///   <item><description>TabooSign: DC 12 (forbidden area designations)</description></item>
/// </list>
/// </para>
/// </remarks>
public interface ISignInterpretationService
{
    // ═══════════════════════════════════════════════════════════════════════════
    // SIGN INTERPRETATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Attempts to interpret a scavenger sign.
    /// </summary>
    /// <param name="player">The player attempting interpretation.</param>
    /// <param name="sign">The sign to interpret.</param>
    /// <returns>
    /// A <see cref="SignInterpretationResult"/> indicating the outcome.
    /// On success, includes sign type, meaning, and faction information.
    /// On fumble, includes false information that the player believes to be true.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="player"/> is null.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Performs a Wasteland Survival skill check against the calculated DC.
    /// DC = BaseDC (from sign type) + Age modifier + Unknown faction modifier (+4 if unknown).
    /// </para>
    /// <para>
    /// If the player successfully interprets a sign from an unknown faction,
    /// that faction is automatically added to the player's known factions.
    /// </para>
    /// <para>
    /// Interpretation outcomes:
    /// <list type="bullet">
    ///   <item><description>Success: Sign type and meaning revealed</description></item>
    ///   <item><description>Critical (net >= 5): Additional context about the sign</description></item>
    ///   <item><description>Failure: No information gained</description></item>
    ///   <item><description>Fumble: Misinterpretation - player believes false information</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Example:
    /// <code>
    /// var sign = ScavengerSign.TerritoryMarker("iron-covenant", SignAge.Recent);
    /// var result = signService.InterpretSign(player, sign);
    /// if (result.Interpreted)
    /// {
    ///     Console.WriteLine(result.ToDisplayString());
    /// }
    /// </code>
    /// </para>
    /// </remarks>
    SignInterpretationResult InterpretSign(Player player, ScavengerSign sign);

    // ═══════════════════════════════════════════════════════════════════════════
    // DC CALCULATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Calculates the full interpretation DC for a sign including all modifiers.
    /// </summary>
    /// <param name="player">The player attempting interpretation.</param>
    /// <param name="sign">The sign to interpret.</param>
    /// <returns>
    /// The total DC including base DC, age modifier, and faction familiarity modifier.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="player"/> is null.
    /// </exception>
    /// <remarks>
    /// <para>
    /// DC calculation:
    /// <code>Final DC = Base DC + Age Modifier + (Unknown Faction ? +4 : 0)</code>
    /// </para>
    /// <para>
    /// Example calculations:
    /// <list type="bullet">
    ///   <item><description>Fresh TerritoryMarker from known faction: 10 + 0 + 0 = DC 10</description></item>
    ///   <item><description>Faded CacheIndicator from unknown faction: 14 + 2 + 4 = DC 20</description></item>
    ///   <item><description>Ancient TabooSign from major faction: 12 + 4 + 0 = DC 16</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    int GetSignDc(Player player, ScavengerSign sign);

    /// <summary>
    /// Gets the base interpretation DC for a sign type.
    /// </summary>
    /// <param name="signType">The type of sign.</param>
    /// <returns>The base DC for interpreting this sign type (10, 12, or 14).</returns>
    /// <remarks>
    /// Base DCs by sign type:
    /// <list type="bullet">
    ///   <item><description>TerritoryMarker: DC 10</description></item>
    ///   <item><description>WarningSign: DC 12</description></item>
    ///   <item><description>CacheIndicator: DC 14</description></item>
    ///   <item><description>TrailBlaze: DC 10</description></item>
    ///   <item><description>HuntMarker: DC 14</description></item>
    ///   <item><description>TabooSign: DC 12</description></item>
    /// </list>
    /// </remarks>
    int GetBaseDc(ScavengerSignType signType);

    /// <summary>
    /// Gets the DC modifier from sign age.
    /// </summary>
    /// <param name="age">The age of the sign.</param>
    /// <returns>The DC modifier to add (+0 to +4).</returns>
    /// <remarks>
    /// Age modifiers:
    /// <list type="bullet">
    ///   <item><description>Fresh: +0</description></item>
    ///   <item><description>Recent: +0</description></item>
    ///   <item><description>Old: +1</description></item>
    ///   <item><description>Faded: +2</description></item>
    ///   <item><description>Ancient: +4</description></item>
    /// </list>
    /// </remarks>
    int GetAgeDcModifier(SignAge age);

    /// <summary>
    /// Gets the DC modifier for unknown faction signs.
    /// </summary>
    /// <returns>The DC modifier applied when a faction is unknown (+4).</returns>
    int GetUnknownFactionModifier();

    // ═══════════════════════════════════════════════════════════════════════════
    // SIGN MEANING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the standard meaning template for a sign type.
    /// </summary>
    /// <param name="signType">The type of sign.</param>
    /// <returns>A template string describing what this sign type conveys.</returns>
    /// <remarks>
    /// Returns a template with placeholders (e.g., {faction}, {direction}, {hazard_type})
    /// that can be filled in with context-specific information.
    /// </remarks>
    string GetSignMeaning(ScavengerSignType signType);

    /// <summary>
    /// Gets the critical success context for a sign type.
    /// </summary>
    /// <param name="signType">The type of sign.</param>
    /// <returns>Additional information revealed only on critical success (net >= 5).</returns>
    /// <remarks>
    /// Critical context provides deeper insight such as:
    /// <list type="bullet">
    ///   <item><description>TerritoryMarker: Patrol frequency information</description></item>
    ///   <item><description>WarningSign: Specific danger type details</description></item>
    ///   <item><description>CacheIndicator: Hidden entrance location</description></item>
    ///   <item><description>TrailBlaze: Time-based restrictions</description></item>
    ///   <item><description>HuntMarker: Prey value information</description></item>
    ///   <item><description>TabooSign: Supernatural danger indicator</description></item>
    /// </list>
    /// </remarks>
    string GetCriticalContext(ScavengerSignType signType);

    /// <summary>
    /// Gets a misinterpretation message for a fumbled sign reading.
    /// </summary>
    /// <param name="signType">The actual sign type that was misread.</param>
    /// <returns>A plausible but dangerously incorrect interpretation.</returns>
    /// <remarks>
    /// Misinterpretations are designed to be believable but lead the player into danger:
    /// <list type="bullet">
    ///   <item><description>Territory markers misread as safe havens</description></item>
    ///   <item><description>Warning signs misread as valuable salvage</description></item>
    ///   <item><description>Cache indicators misread as danger warnings</description></item>
    ///   <item><description>Trail blazes misread as trap markers</description></item>
    ///   <item><description>Hunt markers misread as safe zones</description></item>
    ///   <item><description>Taboo signs misread as treasure markers</description></item>
    /// </list>
    /// </remarks>
    string GetMisinterpretation(ScavengerSignType signType);

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTION KNOWLEDGE
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Checks whether a faction is known to the player.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <param name="factionId">The faction ID to check.</param>
    /// <returns>
    /// <c>true</c> if the faction is known (major faction or previously learned);
    /// <c>false</c> otherwise.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="player"/> is null.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Major factions (Iron Covenant, Rust Walkers, Silent Ones, Verdant Circle, Ash-Born)
    /// are always considered known to all players.
    /// </para>
    /// <para>
    /// Other factions become known when:
    /// <list type="bullet">
    ///   <item><description>Player successfully interprets their sign</description></item>
    ///   <item><description>Player meets faction members</description></item>
    ///   <item><description>Player learns through dialogue or documents</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    bool IsFactionKnown(Player player, string factionId);

    // ═══════════════════════════════════════════════════════════════════════════
    // SIGN INFORMATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the display name for a sign type.
    /// </summary>
    /// <param name="signType">The type of sign.</param>
    /// <returns>A human-readable display name.</returns>
    string GetSignDisplayName(ScavengerSignType signType);

    /// <summary>
    /// Gets a detailed description of a sign type.
    /// </summary>
    /// <param name="signType">The type of sign.</param>
    /// <returns>A detailed description explaining what this sign type represents.</returns>
    string GetSignDescription(ScavengerSignType signType);

    /// <summary>
    /// Gets a display name for a faction ID.
    /// </summary>
    /// <param name="factionId">The faction ID.</param>
    /// <returns>A human-readable faction name.</returns>
    /// <remarks>
    /// Converts kebab-case faction IDs to display names:
    /// <list type="bullet">
    ///   <item><description>"iron-covenant" → "Iron Covenant"</description></item>
    ///   <item><description>"rust-walkers" → "Rust Walkers"</description></item>
    ///   <item><description>"silent-ones" → "Silent Ones"</description></item>
    ///   <item><description>"verdant-circle" → "Verdant Circle"</description></item>
    ///   <item><description>"ash-born" → "Ash-Born"</description></item>
    /// </list>
    /// For unknown faction IDs, attempts to convert kebab-case to title case.
    /// </remarks>
    string GetFactionDisplayName(string factionId);

    // ═══════════════════════════════════════════════════════════════════════════
    // SIGN AGE INFORMATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets a narrative description of a sign's age.
    /// </summary>
    /// <param name="age">The age of the sign.</param>
    /// <returns>A narrative description suitable for player display.</returns>
    string GetAgeDisplayString(SignAge age);

    /// <summary>
    /// Gets a warning about information reliability based on sign age.
    /// </summary>
    /// <param name="age">The age of the sign.</param>
    /// <returns>A warning string if the sign is old enough to potentially contain outdated information, or null if reliable.</returns>
    string? GetReliabilityWarning(SignAge age);

    // ═══════════════════════════════════════════════════════════════════════════
    // INTERPRETATION PREREQUISITES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Checks whether the player can attempt sign interpretation.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns>True if the player can attempt interpretation; otherwise, false.</returns>
    /// <remarks>
    /// Interpretation may be blocked by:
    /// <list type="bullet">
    ///   <item><description>Active Blinded status effect</description></item>
    ///   <item><description>Insufficient light level</description></item>
    ///   <item><description>Other incapacitating conditions</description></item>
    /// </list>
    /// </remarks>
    bool CanInterpret(Player player);

    /// <summary>
    /// Gets the reason why interpretation is blocked, if any.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns>A human-readable reason why interpretation is blocked, or null if allowed.</returns>
    string? GetInterpretationBlockedReason(Player player);
}
