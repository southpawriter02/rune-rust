using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Defines a skill synergy pairing for combined exploration checks.
/// </summary>
/// <remarks>
/// <para>
/// SkillSynergyDefinition captures the relationship between primary and secondary
/// skills in a combined check:
/// <list type="bullet">
///   <item><description>Which Wasteland Survival subsystem serves as the primary skill</description></item>
///   <item><description>Which secondary skill (Acrobatics or System Bypass) follows</description></item>
///   <item><description>When the secondary check triggers based on primary result</description></item>
///   <item><description>Narrative framing for the synergy</description></item>
/// </list>
/// </para>
/// <para>
/// Each synergy follows the pattern:
/// <list type="number">
///   <item><description>Primary check (Wasteland Survival) gates access to a situation</description></item>
///   <item><description>Secondary check determines how well the player exploits that situation</description></item>
/// </list>
/// </para>
/// </remarks>
/// <param name="SynergyType">Identifier for this synergy.</param>
/// <param name="PrimarySkill">The gating Wasteland Survival subsystem.</param>
/// <param name="SecondarySkillId">The follow-up skill identifier (e.g., "acrobatics", "system-bypass").</param>
/// <param name="SecondaryTiming">When the secondary check executes.</param>
/// <param name="Description">Player-facing description of this synergy.</param>
/// <seealso cref="SynergyType"/>
/// <seealso cref="SecondaryCheckTiming"/>
/// <seealso cref="WastelandSurvivalCheckType"/>
public readonly record struct SkillSynergyDefinition(
    SynergyType SynergyType,
    WastelandSurvivalCheckType PrimarySkill,
    string SecondarySkillId,
    SecondaryCheckTiming SecondaryTiming,
    string Description)
{
    // ═══════════════════════════════════════════════════════════════════════════
    // DERIVED PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether the secondary check always executes.
    /// </summary>
    public bool SecondaryAlwaysExecutes =>
        SecondaryTiming == SecondaryCheckTiming.Always;

    /// <summary>
    /// Gets whether the secondary check requires primary success.
    /// </summary>
    public bool RequiresPrimarySuccess =>
        SecondaryTiming == SecondaryCheckTiming.OnPrimarySuccess ||
        SecondaryTiming == SecondaryCheckTiming.OnPrimaryCritical;

    /// <summary>
    /// Gets whether the secondary check requires primary critical success.
    /// </summary>
    public bool RequiresPrimaryCritical =>
        SecondaryTiming == SecondaryCheckTiming.OnPrimaryCritical;

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates the Find Hidden Path synergy definition.
    /// </summary>
    /// <returns>A synergy definition for Navigation → Acrobatics.</returns>
    /// <remarks>
    /// Navigate to find a hidden path, then traverse the difficult terrain.
    /// Primary: Navigation (Wasteland Survival)
    /// Secondary: Acrobatics (traverse mode)
    /// </remarks>
    public static SkillSynergyDefinition FindHiddenPath() =>
        new(
            SynergyType.FindHiddenPath,
            WastelandSurvivalCheckType.Navigation,
            "acrobatics",
            SecondaryCheckTiming.OnPrimarySuccess,
            "Navigate to find a hidden path, then traverse the difficult terrain.");

    /// <summary>
    /// Creates the Track to Lair synergy definition.
    /// </summary>
    /// <returns>A synergy definition for Tracking → System Bypass.</returns>
    /// <remarks>
    /// Track prey to its lair, then bypass the entrance security.
    /// Primary: Tracking (Wasteland Survival)
    /// Secondary: System Bypass (lockpicking/entry mode)
    /// </remarks>
    public static SkillSynergyDefinition TrackToLair() =>
        new(
            SynergyType.TrackToLair,
            WastelandSurvivalCheckType.Tracking,
            "system-bypass",
            SecondaryCheckTiming.OnPrimarySuccess,
            "Track prey to its lair, then bypass the entrance security.");

    /// <summary>
    /// Creates the Avoid Patrol synergy definition.
    /// </summary>
    /// <returns>A synergy definition for Hazard Detection → Acrobatics.</returns>
    /// <remarks>
    /// Detect an approaching patrol, then use stealth to evade them.
    /// Primary: Hazard Detection (Wasteland Survival)
    /// Secondary: Acrobatics (stealth mode)
    /// </remarks>
    public static SkillSynergyDefinition AvoidPatrol() =>
        new(
            SynergyType.AvoidPatrol,
            WastelandSurvivalCheckType.HazardDetection,
            "acrobatics",
            SecondaryCheckTiming.OnPrimarySuccess,
            "Detect an approaching patrol, then use stealth to evade them.");

    /// <summary>
    /// Creates the Find and Loot synergy definition.
    /// </summary>
    /// <returns>A synergy definition for Foraging → System Bypass.</returns>
    /// <remarks>
    /// Discover a locked cache while foraging, then bypass the lock.
    /// Primary: Foraging (Wasteland Survival)
    /// Secondary: System Bypass (lockpicking mode)
    /// </remarks>
    public static SkillSynergyDefinition FindAndLoot() =>
        new(
            SynergyType.FindAndLoot,
            WastelandSurvivalCheckType.Foraging,
            "system-bypass",
            SecondaryCheckTiming.OnPrimarySuccess,
            "Discover a locked cache while foraging, then bypass the lock.");

    /// <summary>
    /// Gets the synergy definition for a given synergy type.
    /// </summary>
    /// <param name="synergyType">The synergy type to retrieve.</param>
    /// <returns>The corresponding synergy definition.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when an unknown synergy type is provided.</exception>
    public static SkillSynergyDefinition GetFor(SynergyType synergyType) =>
        synergyType switch
        {
            SynergyType.FindHiddenPath => FindHiddenPath(),
            SynergyType.TrackToLair => TrackToLair(),
            SynergyType.AvoidPatrol => AvoidPatrol(),
            SynergyType.FindAndLoot => FindAndLoot(),
            _ => throw new ArgumentOutOfRangeException(nameof(synergyType), synergyType, "Unknown synergy type.")
        };

    /// <summary>
    /// Gets all defined synergy definitions.
    /// </summary>
    /// <returns>A read-only list of all synergy definitions.</returns>
    public static IReadOnlyList<SkillSynergyDefinition> GetAll() =>
    [
        FindHiddenPath(),
        TrackToLair(),
        AvoidPatrol(),
        FindAndLoot()
    ];

    // ═══════════════════════════════════════════════════════════════════════════
    // STRING FORMATTING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a formatted display string for this synergy definition.
    /// </summary>
    /// <returns>A string describing the synergy and its skill pairing.</returns>
    public override string ToString() =>
        $"{SynergyType}: {PrimarySkill} → {SecondarySkillId} ({SecondaryTiming})";
}
