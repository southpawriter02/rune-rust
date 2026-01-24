using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Encapsulates the context for a navigation attempt in the Wasteland Survival system.
/// </summary>
/// <remarks>
/// <para>
/// NavigationContext captures all factors that affect navigation difficulty:
/// <list type="bullet">
///   <item><description>Terrain type determines base DC (8 to 24)</description></item>
///   <item><description>Equipment (compass) provides dice bonuses (+1d10)</description></item>
///   <item><description>Familiarity with territory provides significant advantage (+2d10)</description></item>
///   <item><description>Weather conditions can impose penalties (varies by type)</description></item>
///   <item><description>Night travel without light imposes additional penalty (-2d10)</description></item>
/// </list>
/// </para>
/// <para>
/// This value object is immutable and can be converted to a SkillContext for
/// integration with the skill check system via <see cref="ToSkillContext"/>.
/// </para>
/// <para>
/// Weather modifier mapping:
/// <list type="bullet">
///   <item><description>Clear: +1d10 (good visibility)</description></item>
///   <item><description>Cloudy: +0 (normal visibility)</description></item>
///   <item><description>LightRain: -1d10 (reduced visibility)</description></item>
///   <item><description>HeavyRain: -2d10 (poor visibility)</description></item>
///   <item><description>Fog: -3d10 (very poor visibility)</description></item>
///   <item><description>Storm: -4d10 (severe conditions)</description></item>
/// </list>
/// </para>
/// </remarks>
/// <param name="TerrainType">The type of terrain being navigated.</param>
/// <param name="Destination">The target location identifier.</param>
/// <param name="HasCompass">Whether the character has a working compass (+1d10, ineffective in glitched terrain).</param>
/// <param name="FamiliarTerritory">Whether the character knows this area (+2d10).</param>
/// <param name="WeatherConditions">Current weather affecting visibility.</param>
/// <param name="IsNightWithoutLight">Whether it is night without artificial light source (-2d10).</param>
public readonly record struct NavigationContext(
    NavigationTerrainType TerrainType,
    string Destination,
    bool HasCompass = false,
    bool FamiliarTerritory = false,
    WeatherType WeatherConditions = WeatherType.Clear,
    bool IsNightWithoutLight = false)
{
    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTANTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Bonus dice from having a working compass.
    /// </summary>
    public const int CompassBonusDice = 1;

    /// <summary>
    /// Bonus dice from familiar territory.
    /// </summary>
    public const int FamiliarTerritoryBonusDice = 2;

    /// <summary>
    /// Penalty dice for traveling at night without light.
    /// </summary>
    public const int NightWithoutLightPenalty = 2;

    // ═══════════════════════════════════════════════════════════════════════════
    // DIFFICULTY CLASS CALCULATIONS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the base difficulty class for this terrain type.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    ///   <item><description>OpenWasteland: DC 8</description></item>
    ///   <item><description>ModerateRuins: DC 12</description></item>
    ///   <item><description>DenseRuins: DC 16</description></item>
    ///   <item><description>Labyrinthine: DC 20</description></item>
    ///   <item><description>GlitchedLabyrinth: DC 24</description></item>
    /// </list>
    /// </remarks>
    public int BaseDc => TerrainType.GetBaseDc();

    // ═══════════════════════════════════════════════════════════════════════════
    // DICE POOL CALCULATIONS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether the compass is effective in this context.
    /// </summary>
    /// <remarks>
    /// Compasses do not function properly in <see cref="NavigationTerrainType.GlitchedLabyrinth"/>
    /// terrain due to reality distortions affecting magnetic fields.
    /// </remarks>
    public bool CompassEffective => HasCompass && TerrainType.IsCompassEffective();

    /// <summary>
    /// Gets the compass modifier (0 if no compass or compass is ineffective).
    /// </summary>
    public int CompassModifier => CompassEffective ? CompassBonusDice : 0;

    /// <summary>
    /// Gets the familiar territory modifier (0 if not familiar).
    /// </summary>
    public int FamiliarTerritoryModifier => FamiliarTerritory ? FamiliarTerritoryBonusDice : 0;

    /// <summary>
    /// Gets the weather modifier based on current conditions.
    /// </summary>
    /// <remarks>
    /// Weather affects visibility and navigation difficulty:
    /// <list type="bullet">
    ///   <item><description>Clear: +1 (good visibility)</description></item>
    ///   <item><description>Cloudy: +0 (normal visibility)</description></item>
    ///   <item><description>LightRain: -1 (reduced visibility)</description></item>
    ///   <item><description>HeavyRain: -2 (poor visibility)</description></item>
    ///   <item><description>Fog: -3 (very poor visibility)</description></item>
    ///   <item><description>Storm: -4 (severe conditions)</description></item>
    /// </list>
    /// </remarks>
    public int WeatherModifier => WeatherConditions switch
    {
        WeatherType.Clear => 1,
        WeatherType.Cloudy => 0,
        WeatherType.LightRain => -1,
        WeatherType.HeavyRain => -2,
        WeatherType.Fog => -3,
        WeatherType.Storm => -4,
        _ => 0
    };

    /// <summary>
    /// Gets the night penalty (0 if not night or has light source).
    /// </summary>
    public int NightPenalty => IsNightWithoutLight ? -NightWithoutLightPenalty : 0;

    /// <summary>
    /// Gets the total dice modifier from all contextual factors.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Modifiers stack additively:
    /// <list type="bullet">
    ///   <item><description>Working compass: +1d10 (not in glitched terrain)</description></item>
    ///   <item><description>Familiar territory: +2d10</description></item>
    ///   <item><description>Weather: varies from +1 to -4</description></item>
    ///   <item><description>Night (no light): -2d10</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Formula: CompassModifier + FamiliarTerritoryModifier + WeatherModifier + NightPenalty
    /// </para>
    /// </remarks>
    public int TotalDiceModifier => CompassModifier + FamiliarTerritoryModifier + WeatherModifier + NightPenalty;

    // ═══════════════════════════════════════════════════════════════════════════
    // SKILL CONTEXT CONVERSION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Converts this context to a SkillContext for the skill check system.
    /// </summary>
    /// <returns>A SkillContext configured for Wasteland Survival navigation.</returns>
    /// <remarks>
    /// <para>
    /// Creates situational modifiers for each factor affecting the navigation roll:
    /// <list type="bullet">
    ///   <item><description>Compass bonus as an equipment modifier</description></item>
    ///   <item><description>Familiar territory as a situational modifier</description></item>
    ///   <item><description>Weather as an environment modifier</description></item>
    ///   <item><description>Night penalty as a situational modifier</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public SkillContext ToSkillContext()
    {
        var situationalModifiers = new List<SituationalModifier>();
        var equipmentModifiers = new List<EquipmentModifier>();
        var environmentModifiers = new List<EnvironmentModifier>();

        // Add compass bonus as equipment modifier (if effective)
        if (CompassEffective)
        {
            equipmentModifiers.Add(new EquipmentModifier(
                EquipmentId: "navigation-compass",
                EquipmentName: "Working Compass",
                DiceModifier: CompassBonusDice,
                DcModifier: 0,
                EquipmentCategory: EquipmentCategory.Tool,
                RequiredForCheck: false,
                Description: "Magnetic compass aids navigation"));
        }
        else if (HasCompass && !CompassEffective)
        {
            // Note ineffective compass in glitched terrain
            equipmentModifiers.Add(new EquipmentModifier(
                EquipmentId: "navigation-compass-glitched",
                EquipmentName: "Compass (Ineffective)",
                DiceModifier: 0,
                DcModifier: 0,
                EquipmentCategory: EquipmentCategory.Tool,
                RequiredForCheck: false,
                Description: "Compass does not function in glitch-corrupted terrain"));
        }

        // Add familiar territory bonus as situational modifier
        if (FamiliarTerritory)
        {
            situationalModifiers.Add(new SituationalModifier(
                ModifierId: "navigation-familiar-territory",
                Name: "Familiar Territory",
                DiceModifier: FamiliarTerritoryBonusDice,
                DcModifier: 0,
                Source: "Previously explored this area",
                Duration: ModifierDuration.Instant));
        }

        // Add weather modifier as environment modifier
        if (WeatherModifier != 0)
        {
            var weatherName = WeatherConditions switch
            {
                WeatherType.Clear => "Clear Visibility",
                WeatherType.Cloudy => "Overcast",
                WeatherType.LightRain => "Light Rain",
                WeatherType.HeavyRain => "Heavy Rain",
                WeatherType.Fog => "Dense Fog",
                WeatherType.Storm => "Storm",
                _ => "Weather Conditions"
            };

            environmentModifiers.Add(new EnvironmentModifier(
                ModifierId: $"navigation-weather-{WeatherConditions.ToString().ToLowerInvariant()}",
                Name: weatherName,
                DiceModifier: WeatherModifier,
                DcModifier: 0,
                Description: $"Current weather: {WeatherConditions}"));
        }

        // Add night penalty as situational modifier
        if (IsNightWithoutLight)
        {
            situationalModifiers.Add(new SituationalModifier(
                ModifierId: "navigation-night-no-light",
                Name: "Night (No Light)",
                DiceModifier: NightPenalty,
                DcModifier: 0,
                Source: "Traveling at night without light source",
                Duration: ModifierDuration.Instant));
        }

        return new SkillContext(
            equipmentModifiers.AsReadOnly(),
            situationalModifiers.AsReadOnly(),
            environmentModifiers.AsReadOnly(),
            Array.Empty<TargetModifier>(),
            Array.Empty<string>());
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // STRING FORMATTING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a display string describing the navigation context.
    /// </summary>
    /// <returns>A formatted string showing navigation parameters.</returns>
    /// <example>
    /// "Navigate to Sector 7 | Terrain: Dense Ruins (DC 16) | Modifier: +1d10"
    /// </example>
    public string ToDisplayString()
    {
        var parts = new List<string>
        {
            $"Navigate to {Destination}",
            $"Terrain: {TerrainType.GetDisplayName()} (DC {BaseDc})"
        };

        if (HasCompass)
        {
            parts.Add(CompassEffective ? "Compass: +1d10" : "Compass: Ineffective (glitched area)");
        }

        if (FamiliarTerritory)
        {
            parts.Add("Familiar Territory: +2d10");
        }

        parts.Add($"Weather: {WeatherConditions} ({GetWeatherModifierString()})");

        if (IsNightWithoutLight)
        {
            parts.Add("Night (no light): -2d10");
        }

        parts.Add($"Total Modifier: {GetTotalModifierString()}");

        return string.Join(" | ", parts);
    }

    /// <summary>
    /// Returns a detailed diagnostic string for logging.
    /// </summary>
    /// <returns>A multi-line string with complete context details.</returns>
    public string ToDetailedString()
    {
        return $"NavigationContext\n" +
               $"  Destination: {Destination}\n" +
               $"  Terrain: {TerrainType.GetDisplayName()} (DC {BaseDc})\n" +
               $"  Compass: {(HasCompass ? (CompassEffective ? "+1d10" : "Ineffective") : "None")}\n" +
               $"  Familiar Territory: {(FamiliarTerritory ? "+2d10" : "No")}\n" +
               $"  Weather: {WeatherConditions} ({GetWeatherModifierString()})\n" +
               $"  Night (no light): {(IsNightWithoutLight ? "-2d10" : "No")}\n" +
               $"  Total Modifier: {GetTotalModifierString()}";
    }

    /// <summary>
    /// Returns a human-readable description of the navigation context.
    /// </summary>
    /// <returns>A formatted string describing the navigation parameters.</returns>
    public override string ToString()
    {
        return $"Navigation to {Destination} through {TerrainType.GetDisplayName()} ({GetTotalModifierString()})";
    }

    private string GetWeatherModifierString()
    {
        var mod = WeatherModifier;
        return mod >= 0 ? $"+{mod}d10" : $"{mod}d10";
    }

    private string GetTotalModifierString()
    {
        var mod = TotalDiceModifier;
        return mod >= 0 ? $"+{mod}d10" : $"{mod}d10";
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a simple navigation context with minimal parameters.
    /// </summary>
    /// <param name="destination">The target location identifier.</param>
    /// <param name="terrainType">The type of terrain being navigated.</param>
    /// <returns>A basic navigation context with default modifiers.</returns>
    public static NavigationContext CreateSimple(
        string destination,
        NavigationTerrainType terrainType)
    {
        return new NavigationContext(
            TerrainType: terrainType,
            Destination: destination);
    }

    /// <summary>
    /// Creates a navigation context with equipment and territory bonuses.
    /// </summary>
    /// <param name="destination">The target location identifier.</param>
    /// <param name="terrainType">The type of terrain being navigated.</param>
    /// <param name="hasCompass">Whether the character has a working compass.</param>
    /// <param name="familiarTerritory">Whether the character knows this area.</param>
    /// <returns>A navigation context with the specified bonuses.</returns>
    public static NavigationContext CreateWithBonuses(
        string destination,
        NavigationTerrainType terrainType,
        bool hasCompass = false,
        bool familiarTerritory = false)
    {
        return new NavigationContext(
            TerrainType: terrainType,
            Destination: destination,
            HasCompass: hasCompass,
            FamiliarTerritory: familiarTerritory);
    }

    /// <summary>
    /// Creates a navigation context with all conditions specified.
    /// </summary>
    /// <param name="destination">The target location identifier.</param>
    /// <param name="terrainType">The type of terrain being navigated.</param>
    /// <param name="hasCompass">Whether the character has a working compass.</param>
    /// <param name="familiarTerritory">Whether the character knows this area.</param>
    /// <param name="weatherConditions">Current weather affecting visibility.</param>
    /// <param name="isNight">Whether it is night without artificial light.</param>
    /// <returns>A fully specified navigation context.</returns>
    public static NavigationContext CreateFull(
        string destination,
        NavigationTerrainType terrainType,
        bool hasCompass,
        bool familiarTerritory,
        WeatherType weatherConditions,
        bool isNight = false)
    {
        return new NavigationContext(
            TerrainType: terrainType,
            Destination: destination,
            HasCompass: hasCompass,
            FamiliarTerritory: familiarTerritory,
            WeatherConditions: weatherConditions,
            IsNightWithoutLight: isNight);
    }
}
