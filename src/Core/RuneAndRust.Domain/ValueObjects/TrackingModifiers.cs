using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Contextual modifiers affecting tracking difficulty.
/// </summary>
/// <remarks>
/// <para>
/// Aggregates all condition-based modifiers that affect tracking checks.
/// Positive DC values increase difficulty, negative values decrease it.
/// </para>
/// <para>
/// Standard modifiers:
/// <list type="bullet">
///   <item><description>Blood trail: -4 DC</description></item>
///   <item><description>Rain (recent): +4 DC</description></item>
///   <item><description>Multiple targets: -2 DC</description></item>
///   <item><description>Single careful target: +2 DC</description></item>
///   <item><description>Fresh blood/injury: -2 DC per check</description></item>
///   <item><description>Each hour passed: +1 DC</description></item>
///   <item><description>Target deliberately hiding: +2 DC</description></item>
/// </list>
/// </para>
/// <para>
/// Equipment and familiarity modifiers affect the dice pool directly:
/// <list type="bullet">
///   <item><description>Equipment bonus: varies by gear quality</description></item>
///   <item><description>Familiar territory: +2d10</description></item>
/// </list>
/// </para>
/// </remarks>
public sealed class TrackingModifiers
{
    /// <summary>
    /// Whether the target is leaving a blood trail.
    /// </summary>
    /// <remarks>
    /// Applies -4 DC modifier. Blood is highly visible and provides
    /// consistent tracking signs.
    /// </remarks>
    public bool HasBloodTrail { get; }

    /// <summary>
    /// Whether recent rain has obscured the trail.
    /// </summary>
    /// <remarks>
    /// Applies +4 DC modifier. Rain washes away footprints, scent,
    /// and other tracking signs.
    /// </remarks>
    public bool HasRecentRain { get; }

    /// <summary>
    /// Number of targets being tracked.
    /// </summary>
    /// <remarks>
    /// Multiple targets (2+) make tracking easier (-2 DC) as they
    /// leave more signs of passage.
    /// </remarks>
    public int TargetCount { get; }

    /// <summary>
    /// Whether the target is being careful to hide their passage.
    /// </summary>
    /// <remarks>
    /// Applies +2 DC modifier. Target is actively concealing their trail
    /// through various counter-tracking techniques.
    /// </remarks>
    public bool TargetIsHiding { get; }

    /// <summary>
    /// Whether the target is freshly injured (bleeding each check).
    /// </summary>
    /// <remarks>
    /// Applies -2 DC modifier. Fresh injuries leave blood drops and
    /// slow the target, making them easier to follow.
    /// </remarks>
    public bool TargetIsFreshlyInjured { get; }

    /// <summary>
    /// Hours elapsed since trail was created.
    /// </summary>
    /// <remarks>
    /// Applies +1 DC per hour elapsed. Trails degrade over time as
    /// wind, weather, and other passage obscure the signs.
    /// </remarks>
    public int HoursElapsed { get; }

    /// <summary>
    /// Current navigation terrain type affecting check frequency.
    /// </summary>
    /// <remarks>
    /// Determines how often tracking checks are required during pursuit.
    /// More complex terrain requires more frequent checks.
    /// </remarks>
    public NavigationTerrainType Terrain { get; }

    /// <summary>
    /// Tracker's equipment bonus (survival kit, tracking gear).
    /// </summary>
    /// <remarks>
    /// Adds dice to the tracking pool. Typical values:
    /// <list type="bullet">
    ///   <item><description>No equipment: 0</description></item>
    ///   <item><description>Basic survival kit: +1d10</description></item>
    ///   <item><description>Tracker's kit: +2d10</description></item>
    /// </list>
    /// </remarks>
    public int EquipmentBonus { get; }

    /// <summary>
    /// Whether this is familiar territory to the tracker.
    /// </summary>
    /// <remarks>
    /// Applies +2d10 dice pool bonus. The tracker knows local terrain,
    /// common paths, and likely hiding spots.
    /// </remarks>
    public bool IsFamiliarTerritory { get; }

    /// <summary>
    /// Creates a new set of tracking modifiers.
    /// </summary>
    /// <param name="hasBloodTrail">Whether target is leaving blood trail.</param>
    /// <param name="hasRecentRain">Whether rain has obscured tracks.</param>
    /// <param name="targetCount">Number of targets being tracked.</param>
    /// <param name="targetIsHiding">Whether target is actively hiding trail.</param>
    /// <param name="targetIsFreshlyInjured">Whether target is freshly injured.</param>
    /// <param name="hoursElapsed">Hours since trail was created.</param>
    /// <param name="terrain">Navigation terrain type.</param>
    /// <param name="equipmentBonus">Equipment dice bonus.</param>
    /// <param name="isFamiliarTerritory">Whether tracker knows the area.</param>
    public TrackingModifiers(
        bool hasBloodTrail = false,
        bool hasRecentRain = false,
        int targetCount = 1,
        bool targetIsHiding = false,
        bool targetIsFreshlyInjured = false,
        int hoursElapsed = 0,
        NavigationTerrainType terrain = NavigationTerrainType.OpenWasteland,
        int equipmentBonus = 0,
        bool isFamiliarTerritory = false)
    {
        HasBloodTrail = hasBloodTrail;
        HasRecentRain = hasRecentRain;
        TargetCount = Math.Max(1, targetCount);
        TargetIsHiding = targetIsHiding;
        TargetIsFreshlyInjured = targetIsFreshlyInjured;
        HoursElapsed = Math.Max(0, hoursElapsed);
        Terrain = terrain;
        EquipmentBonus = equipmentBonus;
        IsFamiliarTerritory = isFamiliarTerritory;
    }

    /// <summary>
    /// Total DC modifier from all conditions.
    /// </summary>
    /// <remarks>
    /// Positive values increase difficulty, negative decrease it.
    /// This is added to the base DC from trail age.
    /// </remarks>
    public int TotalDcModifier
    {
        get
        {
            var modifier = 0;

            // Blood trail makes tracking easier
            if (HasBloodTrail)
            {
                modifier -= 4;
            }

            // Fresh injury provides ongoing blood trail bonus
            if (TargetIsFreshlyInjured)
            {
                modifier -= 2;
            }

            // Multiple targets leave more signs
            if (TargetCount >= 2)
            {
                modifier -= 2;
            }

            // Rain obscures tracks
            if (HasRecentRain)
            {
                modifier += 4;
            }

            // Careful targets are harder to follow
            if (TargetIsHiding)
            {
                modifier += 2;
            }

            // Time degrades the trail
            modifier += HoursElapsed;

            return modifier;
        }
    }

    /// <summary>
    /// Total dice pool modifier from equipment and familiarity.
    /// </summary>
    /// <remarks>
    /// Positive values add dice to the pool. This is added to the
    /// character's base Wasteland Survival dice pool.
    /// </remarks>
    public int TotalDiceModifier
    {
        get
        {
            var modifier = EquipmentBonus;

            // Familiar territory grants bonus
            if (IsFamiliarTerritory)
            {
                modifier += 2;
            }

            return modifier;
        }
    }

    /// <summary>
    /// Distance interval for pursuit checks based on terrain.
    /// </summary>
    /// <returns>Distance in miles between required checks.</returns>
    /// <remarks>
    /// More complex terrain requires more frequent checks as there are
    /// more opportunities to lose the trail at intersections and turns.
    /// </remarks>
    public float GetCheckIntervalMiles()
    {
        return Terrain switch
        {
            NavigationTerrainType.OpenWasteland => 2.0f,
            NavigationTerrainType.ModerateRuins => 1.0f,
            NavigationTerrainType.DenseRuins => 0.5f,
            NavigationTerrainType.Labyrinthine => 0.1f,
            NavigationTerrainType.GlitchedLabyrinth => 0.1f,
            _ => 1.0f
        };
    }

    /// <summary>
    /// Converts to SkillContext for skill check integration.
    /// </summary>
    /// <returns>A SkillContext with appropriate modifiers.</returns>
    /// <remarks>
    /// <para>
    /// Creates situational modifiers for tracking conditions that can be
    /// passed to SkillCheckService. DC modifiers are converted to situational
    /// modifiers, and dice modifiers are converted to equipment modifiers.
    /// </para>
    /// </remarks>
    public SkillContext ToSkillContext()
    {
        var equipmentModifiers = new List<EquipmentModifier>();
        var situationalModifiers = new List<SituationalModifier>();

        // Add equipment modifier if any
        if (EquipmentBonus != 0)
        {
            equipmentModifiers.Add(new EquipmentModifier(
                EquipmentId: "tracking-gear",
                EquipmentName: "Tracking Equipment",
                DiceModifier: EquipmentBonus,
                DcModifier: 0,
                EquipmentCategory: EquipmentCategory.Tool,
                RequiredForCheck: false));
        }

        // Add blood trail modifier
        if (HasBloodTrail)
        {
            situationalModifiers.Add(new SituationalModifier(
                ModifierId: "blood-trail",
                Name: "Blood Trail",
                DiceModifier: 0,
                DcModifier: -4,
                Source: "Target is leaving a blood trail",
                Duration: ModifierDuration.Persistent));
        }

        // Add fresh injury modifier
        if (TargetIsFreshlyInjured)
        {
            situationalModifiers.Add(new SituationalModifier(
                ModifierId: "fresh-injury",
                Name: "Fresh Injury",
                DiceModifier: 0,
                DcModifier: -2,
                Source: "Target is actively bleeding",
                Duration: ModifierDuration.Scene));
        }

        // Add multiple targets modifier
        if (TargetCount >= 2)
        {
            situationalModifiers.Add(new SituationalModifier(
                ModifierId: "multiple-targets",
                Name: "Multiple Targets",
                DiceModifier: 0,
                DcModifier: -2,
                Source: $"{TargetCount} targets leave more signs",
                Duration: ModifierDuration.Persistent));
        }

        // Add rain modifier
        if (HasRecentRain)
        {
            situationalModifiers.Add(new SituationalModifier(
                ModifierId: "recent-rain",
                Name: "Recent Rain",
                DiceModifier: 0,
                DcModifier: 4,
                Source: "Rain has obscured tracks",
                Duration: ModifierDuration.Scene));
        }

        // Add target hiding modifier
        if (TargetIsHiding)
        {
            situationalModifiers.Add(new SituationalModifier(
                ModifierId: "target-hiding",
                Name: "Target Hiding",
                DiceModifier: 0,
                DcModifier: 2,
                Source: "Target is actively concealing their trail",
                Duration: ModifierDuration.Persistent));
        }

        // Add time elapsed modifier
        if (HoursElapsed > 0)
        {
            situationalModifiers.Add(new SituationalModifier(
                ModifierId: "time-elapsed",
                Name: "Trail Degradation",
                DiceModifier: 0,
                DcModifier: HoursElapsed,
                Source: $"{HoursElapsed} hour(s) elapsed",
                Duration: ModifierDuration.Persistent));
        }

        // Add familiar territory modifier
        if (IsFamiliarTerritory)
        {
            situationalModifiers.Add(new SituationalModifier(
                ModifierId: "familiar-territory",
                Name: "Familiar Territory",
                DiceModifier: 2,
                DcModifier: 0,
                Source: "Tracker knows this area well",
                Duration: ModifierDuration.Persistent));
        }

        return new SkillContext(
            equipmentModifiers.AsReadOnly(),
            situationalModifiers.AsReadOnly(),
            Array.Empty<EnvironmentModifier>(),
            Array.Empty<TargetModifier>(),
            Array.Empty<string>());
    }

    /// <summary>
    /// Returns a human-readable description of active modifiers.
    /// </summary>
    /// <returns>Comma-separated list of active modifiers.</returns>
    public override string ToString()
    {
        var parts = new List<string>();

        if (HasBloodTrail)
        {
            parts.Add("Blood Trail (-4 DC)");
        }

        if (TargetIsFreshlyInjured)
        {
            parts.Add("Fresh Injury (-2 DC)");
        }

        if (TargetCount >= 2)
        {
            parts.Add($"Multiple Targets ({TargetCount}, -2 DC)");
        }

        if (HasRecentRain)
        {
            parts.Add("Recent Rain (+4 DC)");
        }

        if (TargetIsHiding)
        {
            parts.Add("Target Hiding (+2 DC)");
        }

        if (HoursElapsed > 0)
        {
            parts.Add($"Time Elapsed (+{HoursElapsed} DC)");
        }

        if (EquipmentBonus != 0)
        {
            var sign = EquipmentBonus >= 0 ? "+" : "";
            parts.Add($"Equipment ({sign}{EquipmentBonus}d10)");
        }

        if (IsFamiliarTerritory)
        {
            parts.Add("Familiar Territory (+2d10)");
        }

        return parts.Count > 0
            ? string.Join(", ", parts)
            : "No modifiers";
    }

    /// <summary>
    /// Creates default tracking modifiers with no conditions.
    /// </summary>
    /// <returns>A new TrackingModifiers instance with default values.</returns>
    public static TrackingModifiers Default => new();
}
