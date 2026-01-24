using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the context for a foraging attempt, including biome,
/// search duration, equipment bonuses, and area exhaustion state.
/// </summary>
/// <remarks>
/// <para>
/// Captures all information needed to perform a foraging attempt in the wasteland.
/// The context determines dice pool modifiers, base DC, and time investment.
/// </para>
/// <para>
/// Key factors affecting foraging:
/// <list type="bullet">
///   <item><description>SearchDuration: Longer searches add more bonus dice</description></item>
///   <item><description>ForageTarget: Determines base DC and primary yield type</description></item>
///   <item><description>EquipmentBonus: Survival kits and tools add bonus dice</description></item>
///   <item><description>PreviousSearches: Searching the same area repeatedly reduces yields</description></item>
/// </list>
/// </para>
/// <para>
/// Usage pattern:
/// <code>
/// var context = new ForagingContext(
///     characterId: player.Id.ToString(),
///     biomeId: "industrial-ruins",
///     searchDuration: SearchDuration.Thorough,
///     forageTarget: ForageTarget.CommonSalvage,
///     equipmentBonus: 1,  // Survival kit
///     previousSearches: 0);
///
/// // Check modifiers
/// var bonus = context.TotalDiceModifier;  // +3 (2 from duration + 1 from equipment)
/// var dc = context.BaseDc;                 // 10 for CommonSalvage
/// </code>
/// </para>
/// </remarks>
/// <param name="CharacterId">The unique identifier of the foraging character.</param>
/// <param name="BiomeId">The biome/environment being searched (affects loot tables).</param>
/// <param name="SearchDuration">How long the character will search (affects bonus dice).</param>
/// <param name="ForageTarget">What type of resources to prioritize (affects base DC).</param>
/// <param name="EquipmentBonus">Bonus dice from equipment (survival kit, etc.).</param>
/// <param name="PreviousSearches">Number of times this area has been searched recently.</param>
public readonly record struct ForagingContext(
    string CharacterId,
    string BiomeId,
    SearchDuration SearchDuration,
    ForageTarget ForageTarget,
    int EquipmentBonus = 0,
    int PreviousSearches = 0)
{
    // ═══════════════════════════════════════════════════════════════════════════
    // TIME CALCULATIONS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the time required for this search in minutes.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    ///   <item><description>Quick: 10 minutes</description></item>
    ///   <item><description>Thorough: 60 minutes (1 hour)</description></item>
    ///   <item><description>Complete: 240 minutes (4 hours)</description></item>
    /// </list>
    /// </remarks>
    public int TimeMinutes => SearchDuration switch
    {
        SearchDuration.Quick => 10,
        SearchDuration.Thorough => 60,
        SearchDuration.Complete => 240,
        _ => 10
    };

    /// <summary>
    /// Gets the time required for this search as a TimeSpan.
    /// </summary>
    /// <remarks>
    /// Convenience property for working with time-based calculations.
    /// </remarks>
    public TimeSpan TimeSpan => TimeSpan.FromMinutes(TimeMinutes);

    // ═══════════════════════════════════════════════════════════════════════════
    // DICE POOL CALCULATIONS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the bonus dice from the search duration.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Longer searches provide more bonus dice:
    /// <list type="bullet">
    ///   <item><description>Quick: +0 bonus dice</description></item>
    ///   <item><description>Thorough: +2 bonus dice</description></item>
    ///   <item><description>Complete: +4 bonus dice</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public int DurationBonusDice => SearchDuration switch
    {
        SearchDuration.Quick => 0,
        SearchDuration.Thorough => 2,
        SearchDuration.Complete => 4,
        _ => 0
    };

    /// <summary>
    /// Gets the penalty for searching a previously-searched area.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Each previous search in this area reduces the dice pool by 1,
    /// representing diminishing returns from repeated scavenging.
    /// The penalty is capped at -3 to prevent completely unproductive searches.
    /// </para>
    /// <para>
    /// Penalty progression:
    /// <list type="bullet">
    ///   <item><description>0 previous searches: No penalty</description></item>
    ///   <item><description>1 previous search: -1 die</description></item>
    ///   <item><description>2 previous searches: -2 dice</description></item>
    ///   <item><description>3+ previous searches: -3 dice (maximum)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public int ExhaustionPenalty => Math.Min(PreviousSearches, 3);

    /// <summary>
    /// Calculates the total bonus/penalty to the dice pool.
    /// </summary>
    /// <remarks>
    /// Formula: DurationBonusDice + EquipmentBonus - ExhaustionPenalty
    /// </remarks>
    public int TotalDiceModifier => DurationBonusDice + EquipmentBonus - ExhaustionPenalty;

    // ═══════════════════════════════════════════════════════════════════════════
    // DIFFICULTY CLASS CALCULATIONS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the base DC for the selected forage target.
    /// </summary>
    /// <remarks>
    /// <para>
    /// DC by target type:
    /// <list type="bullet">
    ///   <item><description>CommonSalvage: DC 10 (easy, always available)</description></item>
    ///   <item><description>UsefulSupplies: DC 14 (moderate difficulty)</description></item>
    ///   <item><description>ValuableComponents: DC 18 (requires skill)</description></item>
    ///   <item><description>HiddenCache: DC 22 (difficult, high reward)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public int BaseDc => ForageTarget switch
    {
        ForageTarget.CommonSalvage => 10,
        ForageTarget.UsefulSupplies => 14,
        ForageTarget.ValuableComponents => 18,
        ForageTarget.HiddenCache => 22,
        _ => 10
    };

    // ═══════════════════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether this is an exhausted (previously searched) area.
    /// </summary>
    /// <remarks>
    /// True if the area has been searched before, indicating diminishing returns.
    /// </remarks>
    public bool IsExhaustedArea => PreviousSearches > 0;

    /// <summary>
    /// Gets whether the character has equipment bonuses.
    /// </summary>
    public bool HasEquipmentBonus => EquipmentBonus > 0;

    /// <summary>
    /// Gets the net dice modifier (positive if gaining dice, negative if losing).
    /// </summary>
    /// <remarks>
    /// Returns the sign-aware modifier value for display purposes.
    /// </remarks>
    public string DiceModifierDisplay => TotalDiceModifier >= 0
        ? $"+{TotalDiceModifier}"
        : TotalDiceModifier.ToString();

    // ═══════════════════════════════════════════════════════════════════════════
    // SKILL CONTEXT CONVERSION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Converts this context to a SkillContext for the skill check system.
    /// </summary>
    /// <returns>A SkillContext configured for Wasteland Survival.</returns>
    /// <remarks>
    /// <para>
    /// Creates situational modifiers for each factor affecting the foraging roll:
    /// <list type="bullet">
    ///   <item><description>Duration bonus as a situational modifier</description></item>
    ///   <item><description>Equipment bonus as an equipment modifier</description></item>
    ///   <item><description>Exhaustion penalty as a negative situational modifier</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public SkillContext ToSkillContext()
    {
        var situationalModifiers = new List<SituationalModifier>();
        var equipmentModifiers = new List<EquipmentModifier>();

        // Add duration bonus as situational modifier
        if (DurationBonusDice > 0)
        {
            var durationName = SearchDuration switch
            {
                SearchDuration.Thorough => "Thorough Search",
                SearchDuration.Complete => "Complete Search",
                _ => "Quick Search"
            };

            situationalModifiers.Add(new SituationalModifier(
                ModifierId: $"foraging-duration-{SearchDuration.ToString().ToLowerInvariant()}",
                Name: durationName,
                DiceModifier: DurationBonusDice,
                DcModifier: 0,
                Source: $"Extended search time ({TimeMinutes} minutes)",
                Duration: ModifierDuration.Instant));
        }

        // Add equipment bonus as equipment modifier
        if (EquipmentBonus > 0)
        {
            equipmentModifiers.Add(new EquipmentModifier(
                EquipmentId: "foraging-equipment",
                EquipmentName: "Foraging Equipment",
                DiceModifier: EquipmentBonus,
                DcModifier: 0,
                EquipmentCategory: EquipmentCategory.Tool,
                RequiredForCheck: false,
                Description: "Survival kit and foraging tools"));
        }

        // Add exhaustion penalty as negative situational modifier
        if (ExhaustionPenalty > 0)
        {
            situationalModifiers.Add(new SituationalModifier(
                ModifierId: "foraging-exhaustion",
                Name: "Area Exhaustion",
                DiceModifier: -ExhaustionPenalty,
                DcModifier: 0,
                Source: $"Previously searched {PreviousSearches} time(s)",
                Duration: ModifierDuration.Instant));
        }

        return new SkillContext(
            equipmentModifiers.AsReadOnly(),
            situationalModifiers.AsReadOnly(),
            Array.Empty<EnvironmentModifier>(),
            Array.Empty<TargetModifier>(),
            Array.Empty<string>());
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // STRING FORMATTING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a display string for the foraging context.
    /// </summary>
    /// <returns>A formatted string showing search parameters.</returns>
    /// <example>
    /// "Thorough search for CommonSalvage | Biome: industrial-ruins | Bonus: +3d10 | Time: 60 min"
    /// </example>
    public string ToDisplayString()
    {
        return $"{SearchDuration} search for {ForageTarget} | " +
               $"Biome: {BiomeId} | " +
               $"Bonus: {DiceModifierDisplay}d10 | " +
               $"Time: {TimeMinutes} min";
    }

    /// <summary>
    /// Returns a detailed diagnostic string for logging.
    /// </summary>
    /// <returns>A multi-line string with complete context details.</returns>
    public string ToDetailedString()
    {
        return $"ForagingContext\n" +
               $"  Character: {CharacterId}\n" +
               $"  Biome: {BiomeId}\n" +
               $"  Duration: {SearchDuration} ({TimeMinutes} min)\n" +
               $"  Target: {ForageTarget} (DC {BaseDc})\n" +
               $"  Duration Bonus: +{DurationBonusDice}d10\n" +
               $"  Equipment Bonus: +{EquipmentBonus}d10\n" +
               $"  Area Exhaustion: -{ExhaustionPenalty}d10 ({PreviousSearches} previous searches)\n" +
               $"  Total Modifier: {DiceModifierDisplay}d10";
    }

    /// <summary>
    /// Returns a human-readable description of the foraging context.
    /// </summary>
    /// <returns>A formatted string describing the search parameters.</returns>
    public override string ToString()
    {
        return $"Foraging [{CharacterId}]: {SearchDuration} search for {ForageTarget} in {BiomeId} ({DiceModifierDisplay}d10)";
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a foraging context for a quick search with no equipment.
    /// </summary>
    /// <param name="characterId">The ID of the foraging character.</param>
    /// <param name="biomeId">The biome being searched.</param>
    /// <param name="target">The forage target type.</param>
    /// <returns>A context configured for a quick, basic search.</returns>
    /// <remarks>
    /// Quick searches are time-efficient but provide no bonus dice.
    /// </remarks>
    public static ForagingContext CreateQuick(
        string characterId,
        string biomeId,
        ForageTarget target = ForageTarget.CommonSalvage)
    {
        return new ForagingContext(
            CharacterId: characterId,
            BiomeId: biomeId,
            SearchDuration: SearchDuration.Quick,
            ForageTarget: target,
            EquipmentBonus: 0,
            PreviousSearches: 0);
    }

    /// <summary>
    /// Creates a foraging context for a thorough search.
    /// </summary>
    /// <param name="characterId">The ID of the foraging character.</param>
    /// <param name="biomeId">The biome being searched.</param>
    /// <param name="target">The forage target type.</param>
    /// <param name="equipmentBonus">Bonus dice from equipment.</param>
    /// <returns>A context configured for a thorough search with +2d10 bonus.</returns>
    /// <remarks>
    /// Thorough searches take 1 hour but provide +2 bonus dice.
    /// </remarks>
    public static ForagingContext CreateThorough(
        string characterId,
        string biomeId,
        ForageTarget target = ForageTarget.CommonSalvage,
        int equipmentBonus = 0)
    {
        return new ForagingContext(
            CharacterId: characterId,
            BiomeId: biomeId,
            SearchDuration: SearchDuration.Thorough,
            ForageTarget: target,
            EquipmentBonus: equipmentBonus,
            PreviousSearches: 0);
    }

    /// <summary>
    /// Creates a foraging context for a complete search.
    /// </summary>
    /// <param name="characterId">The ID of the foraging character.</param>
    /// <param name="biomeId">The biome being searched.</param>
    /// <param name="target">The forage target type.</param>
    /// <param name="equipmentBonus">Bonus dice from equipment.</param>
    /// <returns>A context configured for a complete search with +4d10 bonus.</returns>
    /// <remarks>
    /// Complete searches take 4 hours but provide +4 bonus dice and
    /// the highest chance for rare finds and cache discovery.
    /// </remarks>
    public static ForagingContext CreateComplete(
        string characterId,
        string biomeId,
        ForageTarget target = ForageTarget.CommonSalvage,
        int equipmentBonus = 0)
    {
        return new ForagingContext(
            CharacterId: characterId,
            BiomeId: biomeId,
            SearchDuration: SearchDuration.Complete,
            ForageTarget: target,
            EquipmentBonus: equipmentBonus,
            PreviousSearches: 0);
    }
}
