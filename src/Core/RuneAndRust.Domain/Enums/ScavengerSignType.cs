namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Defines the types of scavenger signs that can be interpreted using the Wasteland Survival skill.
/// </summary>
/// <remarks>
/// <para>
/// ScavengerSignType represents markings left by wasteland factions to communicate with their own members.
/// Each sign type serves a different purpose and has a corresponding base interpretation DC:
/// <list type="bullet">
///   <item><description><see cref="TerritoryMarker"/> (DC 10): Faction boundary warnings</description></item>
///   <item><description><see cref="WarningSign"/> (DC 12): Danger notifications</description></item>
///   <item><description><see cref="CacheIndicator"/> (DC 14): Hidden supply locations</description></item>
///   <item><description><see cref="TrailBlaze"/> (DC 10): Safe path markers</description></item>
///   <item><description><see cref="HuntMarker"/> (DC 14): Prey location indicators</description></item>
///   <item><description><see cref="TabooSign"/> (DC 12): Forbidden area designations</description></item>
/// </list>
/// </para>
/// <para>
/// Base DCs reflect how standardized or cryptic each sign type typically is:
/// <list type="bullet">
///   <item><description>Territory markers (DC 10) and trail blazes (DC 10) are relatively universal</description></item>
///   <item><description>Warning signs (DC 12) and taboo signs (DC 12) require some cultural knowledge</description></item>
///   <item><description>Cache indicators (DC 14) and hunt markers (DC 14) are deliberately cryptic</description></item>
/// </list>
/// </para>
/// <para>
/// DC modifiers apply based on faction familiarity (+4 for unknown factions) and sign age (+0 to +4).
/// Critical success (net ≥ 5) provides additional context about the sign.
/// Fumbles (0 successes + botch) result in dangerous misinterpretation.
/// </para>
/// </remarks>
public enum ScavengerSignType
{
    /// <summary>
    /// Marks faction territory boundaries. Warns outsiders that they are entering claimed land.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Base DC: 10 - Relatively standardized across factions; meant to be understood by outsiders.
    /// </para>
    /// <para>
    /// Success Meaning: Identifies faction and territory boundary direction.
    /// Trespassers may face hostility from faction patrols.
    /// </para>
    /// <para>
    /// Visual Style: Parallel scratches with central faction symbol, overlapping circles,
    /// or bold strokes forming boundary warnings.
    /// </para>
    /// </remarks>
    TerritoryMarker = 0,

    /// <summary>
    /// Warns of danger ahead—environmental hazards, creature lairs, or ambush sites.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Base DC: 12 - Requires understanding faction-specific danger symbols.
    /// </para>
    /// <para>
    /// Success Meaning: Identifies type of danger and approximate direction/distance.
    /// Allows player to avoid or prepare for the hazard.
    /// </para>
    /// <para>
    /// Visual Style: Jagged lines radiating from center, skull-like patterns,
    /// or sharp angles pointing away from danger zones.
    /// </para>
    /// </remarks>
    WarningSign = 1,

    /// <summary>
    /// Indicates hidden supplies, equipment caches, or safe houses nearby.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Base DC: 14 - Deliberately cryptic to prevent theft by rivals.
    /// </para>
    /// <para>
    /// Success Meaning: Reveals direction and approximate distance to cache.
    /// Cache may contain supplies, equipment, or safe shelter.
    /// </para>
    /// <para>
    /// Visual Style: Subtle arrow-like marks, triangular dot patterns with directional lines,
    /// or spirals ending in small marks.
    /// </para>
    /// </remarks>
    CacheIndicator = 2,

    /// <summary>
    /// Marks a safe path through dangerous terrain—avoiding hazards, predators, or patrols.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Base DC: 10 - Needs to be readable quickly during travel; relatively universal.
    /// </para>
    /// <para>
    /// Success Meaning: Reveals safe direction and any conditions (time-based restrictions,
    /// stealth requirements, etc.).
    /// </para>
    /// <para>
    /// Visual Style: Simple arrows at eye level, parallel lines indicating direction,
    /// or chevron patterns pointing the way forward.
    /// </para>
    /// </remarks>
    TrailBlaze = 3,

    /// <summary>
    /// Indicates prey sightings, hunting grounds, or migration patterns.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Base DC: 14 - Uses specialized hunter terminology; deliberately cryptic.
    /// </para>
    /// <para>
    /// Success Meaning: Reveals prey type, direction, and time since marking.
    /// Useful for foraging and tracking activities.
    /// </para>
    /// <para>
    /// Visual Style: Stylized animal tracks, circles with claw marks,
    /// or predator symbols with time indicators.
    /// </para>
    /// </remarks>
    HuntMarker = 4,

    /// <summary>
    /// Designates forbidden areas—cursed zones, glitch pockets, or faction-sacred ground.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Base DC: 12 - Important enough to be relatively clear; consequences of ignoring are severe.
    /// </para>
    /// <para>
    /// Success Meaning: Reveals reason for taboo (supernatural danger, faction law, etc.)
    /// and severity of transgression.
    /// </para>
    /// <para>
    /// Visual Style: X marks with warning symbols, circles with lines through them,
    /// or repeated warning patterns covering large areas.
    /// </para>
    /// </remarks>
    TabooSign = 5
}

/// <summary>
/// Extension methods for <see cref="ScavengerSignType"/>.
/// </summary>
/// <remarks>
/// Provides utility methods for working with scavenger sign types including:
/// <list type="bullet">
///   <item><description>DC calculation methods</description></item>
///   <item><description>Display name and description methods</description></item>
///   <item><description>Interpretation meaning methods</description></item>
///   <item><description>Misinterpretation content for fumbles</description></item>
/// </list>
/// </remarks>
public static class ScavengerSignTypeExtensions
{
    // ═══════════════════════════════════════════════════════════════════════════
    // BASE DC METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the base interpretation difficulty class for this sign type.
    /// </summary>
    /// <param name="signType">The scavenger sign type.</param>
    /// <returns>The base DC required to interpret this sign type.</returns>
    /// <remarks>
    /// <para>
    /// Base DCs by sign type:
    /// <list type="bullet">
    ///   <item><description>TerritoryMarker: DC 10 (standardized)</description></item>
    ///   <item><description>WarningSign: DC 12 (cultural knowledge required)</description></item>
    ///   <item><description>CacheIndicator: DC 14 (deliberately cryptic)</description></item>
    ///   <item><description>TrailBlaze: DC 10 (universal)</description></item>
    ///   <item><description>HuntMarker: DC 14 (specialized)</description></item>
    ///   <item><description>TabooSign: DC 12 (important but clear)</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// These base DCs may be modified by:
    /// <list type="bullet">
    ///   <item><description>Unknown faction: +4 DC</description></item>
    ///   <item><description>Sign age: +0 (Fresh/Recent), +1 (Old), +2 (Faded), +4 (Ancient)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public static int GetBaseDc(this ScavengerSignType signType)
    {
        return signType switch
        {
            ScavengerSignType.TerritoryMarker => 10,
            ScavengerSignType.WarningSign => 12,
            ScavengerSignType.CacheIndicator => 14,
            ScavengerSignType.TrailBlaze => 10,
            ScavengerSignType.HuntMarker => 14,
            ScavengerSignType.TabooSign => 12,
            _ => 12 // Default to moderate difficulty
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DISPLAY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the human-readable display name for this sign type.
    /// </summary>
    /// <param name="signType">The scavenger sign type.</param>
    /// <returns>A display name suitable for UI presentation.</returns>
    public static string GetDisplayName(this ScavengerSignType signType)
    {
        return signType switch
        {
            ScavengerSignType.TerritoryMarker => "Territory Marker",
            ScavengerSignType.WarningSign => "Warning Sign",
            ScavengerSignType.CacheIndicator => "Cache Indicator",
            ScavengerSignType.TrailBlaze => "Trail Blaze",
            ScavengerSignType.HuntMarker => "Hunt Marker",
            ScavengerSignType.TabooSign => "Taboo Sign",
            _ => "Unknown Sign"
        };
    }

    /// <summary>
    /// Gets a detailed description of the sign type for display purposes.
    /// </summary>
    /// <param name="signType">The scavenger sign type.</param>
    /// <returns>A descriptive string explaining what this sign type represents.</returns>
    public static string GetDescription(this ScavengerSignType signType)
    {
        return signType switch
        {
            ScavengerSignType.TerritoryMarker =>
                "Marks faction territory boundaries. Warns outsiders that they are entering claimed land " +
                "and may face hostility from faction patrols.",

            ScavengerSignType.WarningSign =>
                "Warns of danger ahead—environmental hazards, creature lairs, or potential ambush sites. " +
                "Indicates the type and direction of the threat.",

            ScavengerSignType.CacheIndicator =>
                "Indicates hidden supplies, equipment caches, or safe houses nearby. " +
                "Deliberately cryptic to prevent theft by rival factions.",

            ScavengerSignType.TrailBlaze =>
                "Marks a safe path through dangerous terrain. Helps travelers avoid hazards, " +
                "predators, or hostile patrols.",

            ScavengerSignType.HuntMarker =>
                "Indicates prey sightings, hunting grounds, or migration patterns. " +
                "Uses specialized hunter terminology known mainly to the faction.",

            ScavengerSignType.TabooSign =>
                "Designates forbidden areas—cursed zones, glitch pockets, or faction-sacred ground. " +
                "Entry is strongly discouraged with potentially severe consequences.",

            _ => "Unknown scavenger marking of unclear purpose."
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // INTERPRETATION MEANING METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the standard success meaning template for this sign type.
    /// </summary>
    /// <param name="signType">The scavenger sign type.</param>
    /// <returns>A template string describing what a successful interpretation reveals.</returns>
    /// <remarks>
    /// The returned string may contain placeholders like {faction}, {direction}, {hazard_type}, etc.
    /// that should be filled in with context-specific information when displaying to the player.
    /// </remarks>
    public static string GetSuccessMeaning(this ScavengerSignType signType)
    {
        return signType switch
        {
            ScavengerSignType.TerritoryMarker =>
                "This area belongs to {faction}. Trespassers may face hostility.",

            ScavengerSignType.WarningSign =>
                "Danger lies ahead: {hazard_type}. Proceed with caution.",

            ScavengerSignType.CacheIndicator =>
                "Hidden supplies are concealed {direction}, approximately {distance} away.",

            ScavengerSignType.TrailBlaze =>
                "This path leads {direction} and has been marked as safe.",

            ScavengerSignType.HuntMarker =>
                "{prey_type} was spotted {direction}, {time_ago} ago.",

            ScavengerSignType.TabooSign =>
                "This area is forbidden. {reason}",

            _ => "The meaning is unclear."
        };
    }

    /// <summary>
    /// Gets additional context revealed on a critical success (net successes ≥ 5).
    /// </summary>
    /// <param name="signType">The scavenger sign type.</param>
    /// <returns>Additional information available only on critical success.</returns>
    /// <remarks>
    /// Critical success provides deeper insight beyond the basic meaning, including:
    /// <list type="bullet">
    ///   <item><description>Faction behavior patterns</description></item>
    ///   <item><description>Specific hazard details</description></item>
    ///   <item><description>Time-based restrictions</description></item>
    ///   <item><description>Hidden nuances in the marking</description></item>
    /// </list>
    /// </remarks>
    public static string GetCriticalContext(this ScavengerSignType signType)
    {
        return signType switch
        {
            ScavengerSignType.TerritoryMarker =>
                "You can tell the faction patrols this border regularly.",

            ScavengerSignType.WarningSign =>
                "The specific danger symbol suggests environmental hazard rather than creatures.",

            ScavengerSignType.CacheIndicator =>
                "A subtle directional mark indicates the cache is hidden below ground level.",

            ScavengerSignType.TrailBlaze =>
                "Secondary marks suggest this path is only safe during daylight hours.",

            ScavengerSignType.HuntMarker =>
                "The prey marking style suggests a creature worth significant value.",

            ScavengerSignType.TabooSign =>
                "The intensity of the marking suggests supernatural danger, not just faction law.",

            _ => "The craftsmanship reveals this faction takes their signs seriously."
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // MISINTERPRETATION METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets a misinterpretation message for fumbles (0 successes + botch).
    /// </summary>
    /// <param name="signType">The actual scavenger sign type.</param>
    /// <returns>A plausible but dangerously incorrect interpretation.</returns>
    /// <remarks>
    /// <para>
    /// Misinterpretations are designed to be believable but lead the player into danger:
    /// <list type="bullet">
    ///   <item><description>Territory markers misread as safe havens</description></item>
    ///   <item><description>Warning signs misread as valuable salvage</description></item>
    ///   <item><description>Cache indicators misread as danger warnings</description></item>
    ///   <item><description>Trail blazes misread as trap markers</description></item>
    ///   <item><description>Hunt markers misread as safe zones</description></item>
    ///   <item><description>Taboo signs misread as treasure markers</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// The player believes they have correctly interpreted the sign and may act
    /// on the false information, leading to dangerous situations.
    /// </para>
    /// </remarks>
    public static string GetMisinterpretation(this ScavengerSignType signType)
    {
        return signType switch
        {
            ScavengerSignType.TerritoryMarker =>
                "This marks a safe haven—travelers are welcome here.",

            ScavengerSignType.WarningSign =>
                "This indicates valuable salvage ahead—hurry before others find it!",

            ScavengerSignType.CacheIndicator =>
                "This warns of extreme danger—do not proceed in this direction.",

            ScavengerSignType.TrailBlaze =>
                "This path leads to a trap or ambush—avoid it.",

            ScavengerSignType.HuntMarker =>
                "This area is protected—no predators come here.",

            ScavengerSignType.TabooSign =>
                "This marks a place of great treasure—sacred to the faction.",

            _ => "This means something entirely different than you think."
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SIGN CHARACTERISTIC METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Determines whether this sign type typically indicates immediate danger.
    /// </summary>
    /// <param name="signType">The scavenger sign type.</param>
    /// <returns>True if the sign indicates immediate danger.</returns>
    public static bool IndicatesDanger(this ScavengerSignType signType)
    {
        return signType is ScavengerSignType.WarningSign
            or ScavengerSignType.TabooSign
            or ScavengerSignType.TerritoryMarker;
    }

    /// <summary>
    /// Determines whether this sign type points to resources or benefits.
    /// </summary>
    /// <param name="signType">The scavenger sign type.</param>
    /// <returns>True if the sign indicates potential resources or benefits.</returns>
    public static bool IndicatesResource(this ScavengerSignType signType)
    {
        return signType is ScavengerSignType.CacheIndicator
            or ScavengerSignType.HuntMarker
            or ScavengerSignType.TrailBlaze;
    }

    /// <summary>
    /// Determines whether this sign type provides navigational information.
    /// </summary>
    /// <param name="signType">The scavenger sign type.</param>
    /// <returns>True if the sign provides directional or path guidance.</returns>
    public static bool ProvidesNavigation(this ScavengerSignType signType)
    {
        return signType is ScavengerSignType.TrailBlaze
            or ScavengerSignType.CacheIndicator
            or ScavengerSignType.HuntMarker;
    }

    /// <summary>
    /// Determines whether misinterpreting this sign type leads to faction conflict.
    /// </summary>
    /// <param name="signType">The scavenger sign type.</param>
    /// <returns>True if misinterpretation may cause faction hostility.</returns>
    public static bool MisinterpretationCausesFactionConflict(this ScavengerSignType signType)
    {
        return signType is ScavengerSignType.TerritoryMarker
            or ScavengerSignType.TabooSign;
    }

    /// <summary>
    /// Determines whether misinterpreting this sign type leads to environmental danger.
    /// </summary>
    /// <param name="signType">The scavenger sign type.</param>
    /// <returns>True if misinterpretation may lead into environmental hazards.</returns>
    public static bool MisinterpretationCausesEnvironmentalDanger(this ScavengerSignType signType)
    {
        return signType is ScavengerSignType.WarningSign
            or ScavengerSignType.TrailBlaze
            or ScavengerSignType.TabooSign;
    }
}
