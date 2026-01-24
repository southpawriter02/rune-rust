using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Contextual information for a counter-tracking (trail concealment) attempt.
/// </summary>
/// <remarks>
/// <para>
/// Captures the environment and techniques being used when a character attempts
/// to conceal their trail from potential pursuers. The context determines which
/// techniques are valid and provides the information needed for the concealment
/// check.
/// </para>
/// <para>
/// Technique requirements:
/// <list type="bullet">
///   <item><description>HardSurfaces: Always available</description></item>
///   <item><description>BrushTracks: Requires <see cref="HasFoliageOrDebris"/></description></item>
///   <item><description>FalseTrail: Always available</description></item>
///   <item><description>WaterCrossing: Requires <see cref="HasWaterNearby"/></description></item>
///   <item><description>Backtracking: Always available</description></item>
/// </list>
/// </para>
/// <para>
/// Usage pattern:
/// <code>
/// var context = new CounterTrackingContext(
///     concealerId: player.Id.ToString(),
///     techniquesUsed: new[] { ConcealmentTechnique.BrushTracks, ConcealmentTechnique.Backtracking },
///     environmentId: "wasteland-zone-7",
///     hasWaterNearby: false,
///     hasFoliageOrDebris: true);
///
/// if (!context.AreAllTechniquesValid())
/// {
///     var invalid = context.GetInvalidTechniques();
///     // Handle invalid technique selection
/// }
/// </code>
/// </para>
/// </remarks>
/// <param name="ConcealerId">The ID of the character attempting to conceal their trail.</param>
/// <param name="TechniquesUsed">The concealment techniques being employed.</param>
/// <param name="EnvironmentId">Optional identifier for the current environment/area.</param>
/// <param name="HasWaterNearby">Whether a water body is available for crossing.</param>
/// <param name="HasFoliageOrDebris">Whether foliage or debris is available for brushing tracks.</param>
public readonly record struct CounterTrackingContext(
    string ConcealerId,
    IReadOnlyList<ConcealmentTechnique> TechniquesUsed,
    string? EnvironmentId,
    bool HasWaterNearby,
    bool HasFoliageOrDebris)
{
    // ═══════════════════════════════════════════════════════════════════════════
    // VALIDATION METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the validity status for each technique used.
    /// </summary>
    /// <returns>
    /// A dictionary mapping each technique to its validity status.
    /// True indicates the technique can be used in the current environment,
    /// false indicates a missing requirement.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Technique requirements:
    /// <list type="bullet">
    ///   <item><description>WaterCrossing: Requires HasWaterNearby = true</description></item>
    ///   <item><description>BrushTracks: Requires HasFoliageOrDebris = true</description></item>
    ///   <item><description>All others: Always valid</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public IReadOnlyDictionary<ConcealmentTechnique, bool> GetTechniqueValidity()
    {
        var validity = new Dictionary<ConcealmentTechnique, bool>();

        foreach (var technique in TechniquesUsed)
        {
            validity[technique] = IsTechniqueValid(technique);
        }

        return validity;
    }

    /// <summary>
    /// Checks if a specific technique is valid in the current environment.
    /// </summary>
    /// <param name="technique">The technique to validate.</param>
    /// <returns>True if the technique can be used, false otherwise.</returns>
    /// <remarks>
    /// Environmental requirements:
    /// <list type="bullet">
    ///   <item><description>WaterCrossing requires water nearby</description></item>
    ///   <item><description>BrushTracks requires foliage or debris</description></item>
    ///   <item><description>All other techniques are always available</description></item>
    /// </list>
    /// </remarks>
    public bool IsTechniqueValid(ConcealmentTechnique technique)
    {
        return technique switch
        {
            ConcealmentTechnique.WaterCrossing => HasWaterNearby,
            ConcealmentTechnique.BrushTracks => HasFoliageOrDebris,
            _ => true // HardSurfaces, FalseTrail, Backtracking always available
        };
    }

    /// <summary>
    /// Checks if all selected techniques are valid in the current environment.
    /// </summary>
    /// <returns>True if all techniques can be used, false if any have unmet requirements.</returns>
    /// <remarks>
    /// Returns true if TechniquesUsed is empty (no techniques = nothing to validate).
    /// </remarks>
    public bool AreAllTechniquesValid()
    {
        return TechniquesUsed.All(IsTechniqueValid);
    }

    /// <summary>
    /// Gets the list of invalid techniques that cannot be used in the current environment.
    /// </summary>
    /// <returns>
    /// A list of techniques that have unmet requirements.
    /// Empty if all techniques are valid.
    /// </returns>
    /// <remarks>
    /// Use this to provide feedback to the player about why their technique
    /// selection cannot be used.
    /// </remarks>
    public IReadOnlyList<ConcealmentTechnique> GetInvalidTechniques()
    {
        var self = this;
        return TechniquesUsed.Where(t => !self.IsTechniqueValid(t)).ToList();
    }

    /// <summary>
    /// Gets only the valid techniques from the selected list.
    /// </summary>
    /// <returns>
    /// A list of techniques that can be used in the current environment.
    /// </returns>
    /// <remarks>
    /// Useful for filtering out invalid techniques before performing the check.
    /// </remarks>
    public IReadOnlyList<ConcealmentTechnique> GetValidTechniques()
    {
        return TechniquesUsed.Where(IsTechniqueValid).ToList();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Whether any techniques are selected.
    /// </summary>
    /// <remarks>
    /// A concealment attempt without techniques uses only the base roll.
    /// </remarks>
    public bool HasTechniques => TechniquesUsed.Count > 0;

    /// <summary>
    /// The number of techniques being used.
    /// </summary>
    public int TechniqueCount => TechniquesUsed.Count;

    /// <summary>
    /// The number of valid techniques (usable in current environment).
    /// </summary>
    public int ValidTechniqueCount => TechniquesUsed.Count(IsTechniqueValid);

    /// <summary>
    /// The number of invalid techniques (missing requirements).
    /// </summary>
    public int InvalidTechniqueCount
    {
        get
        {
            var count = 0;
            foreach (var technique in TechniquesUsed)
            {
                if (!IsTechniqueValid(technique))
                    count++;
            }
            return count;
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SKILL CONTEXT CONVERSION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Converts this counter-tracking context to a SkillContext for integration
    /// with the skill check system.
    /// </summary>
    /// <returns>A SkillContext with appropriate situational modifiers.</returns>
    /// <remarks>
    /// <para>
    /// Creates situational modifiers for each valid technique used. Invalid
    /// techniques are excluded from the context. The technique bonuses are
    /// applied as dice modifiers (not DC modifiers) since they enhance the
    /// concealer's roll rather than changing the target DC.
    /// </para>
    /// <para>
    /// Note: The actual bonus calculation is handled by CounterTrackingService.
    /// This method creates a SkillContext for narrative and logging purposes,
    /// describing what techniques are in use.
    /// </para>
    /// </remarks>
    public SkillContext ToSkillContext()
    {
        var situationalModifiers = new List<SituationalModifier>();

        foreach (var technique in TechniquesUsed.Where(IsTechniqueValid))
        {
            var (name, description) = technique switch
            {
                ConcealmentTechnique.HardSurfaces =>
                    ("Hard Surfaces", "Walking on surfaces that don't hold tracks"),
                ConcealmentTechnique.BrushTracks =>
                    ("Brush Tracks", "Sweeping away tracks with foliage or debris"),
                ConcealmentTechnique.FalseTrail =>
                    ("False Trail", "Creating misleading tracks in a different direction"),
                ConcealmentTechnique.WaterCrossing =>
                    ("Water Crossing", "Breaking the trail by crossing through water"),
                ConcealmentTechnique.Backtracking =>
                    ("Backtracking", "Walking backwards to obscure direction of travel"),
                _ => ("Unknown Technique", "Unknown concealment technique")
            };

            situationalModifiers.Add(new SituationalModifier(
                ModifierId: $"concealment-{technique.ToString().ToLowerInvariant()}",
                Name: name,
                DiceModifier: 0, // Actual bonus applied by service
                DcModifier: 0,
                Source: description,
                Duration: ModifierDuration.Instant));
        }

        return new SkillContext(
            Array.Empty<EquipmentModifier>(),
            situationalModifiers.AsReadOnly(),
            Array.Empty<EnvironmentModifier>(),
            Array.Empty<TargetModifier>(),
            Array.Empty<string>());
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // STRING FORMATTING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a human-readable description of the counter-tracking context.
    /// </summary>
    /// <returns>A formatted string describing techniques and environment.</returns>
    public override string ToString()
    {
        if (TechniquesUsed.Count == 0)
        {
            return $"Counter-tracking: No techniques selected";
        }

        var validTechniques = GetValidTechniques();
        var invalidTechniques = GetInvalidTechniques();

        var parts = new List<string>();

        if (validTechniques.Count > 0)
        {
            parts.Add($"Using: {string.Join(", ", validTechniques)}");
        }

        if (invalidTechniques.Count > 0)
        {
            parts.Add($"Invalid: {string.Join(", ", invalidTechniques)}");
        }

        return $"Counter-tracking [{ConcealerId}]: {string.Join(" | ", parts)}";
    }

    /// <summary>
    /// Returns a detailed diagnostic string for logging.
    /// </summary>
    /// <returns>A multi-line string with complete context details.</returns>
    public string ToDetailedString()
    {
        string techniqueList;
        if (TechniquesUsed.Count > 0)
        {
            var parts = new List<string>();
            foreach (var technique in TechniquesUsed)
            {
                var suffix = IsTechniqueValid(technique) ? "" : " [INVALID]";
                parts.Add($"{technique}{suffix}");
            }
            techniqueList = string.Join(", ", parts);
        }
        else
        {
            techniqueList = "None";
        }

        return $"CounterTrackingContext\n" +
               $"  Concealer: {ConcealerId}\n" +
               $"  Environment: {EnvironmentId ?? "Unknown"}\n" +
               $"  Water Nearby: {HasWaterNearby}\n" +
               $"  Foliage/Debris: {HasFoliageOrDebris}\n" +
               $"  Techniques: {techniqueList}\n" +
               $"  Valid: {ValidTechniqueCount}/{TechniqueCount}";
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a counter-tracking context with default environment (no special features).
    /// </summary>
    /// <param name="concealerId">The ID of the character attempting concealment.</param>
    /// <param name="techniques">The techniques to use.</param>
    /// <returns>A context with no water and no foliage available.</returns>
    /// <remarks>
    /// Only HardSurfaces, FalseTrail, and Backtracking will be valid.
    /// </remarks>
    public static CounterTrackingContext CreateDefault(
        string concealerId,
        params ConcealmentTechnique[] techniques)
    {
        return new CounterTrackingContext(
            ConcealerId: concealerId,
            TechniquesUsed: techniques,
            EnvironmentId: null,
            HasWaterNearby: false,
            HasFoliageOrDebris: false);
    }

    /// <summary>
    /// Creates a counter-tracking context for a forested/ruined area.
    /// </summary>
    /// <param name="concealerId">The ID of the character attempting concealment.</param>
    /// <param name="environmentId">Optional environment identifier.</param>
    /// <param name="techniques">The techniques to use.</param>
    /// <returns>A context with foliage/debris available but no water.</returns>
    /// <remarks>
    /// BrushTracks will be valid in addition to the always-available techniques.
    /// </remarks>
    public static CounterTrackingContext CreateWithDebris(
        string concealerId,
        string? environmentId,
        params ConcealmentTechnique[] techniques)
    {
        return new CounterTrackingContext(
            ConcealerId: concealerId,
            TechniquesUsed: techniques,
            EnvironmentId: environmentId,
            HasWaterNearby: false,
            HasFoliageOrDebris: true);
    }

    /// <summary>
    /// Creates a counter-tracking context near water.
    /// </summary>
    /// <param name="concealerId">The ID of the character attempting concealment.</param>
    /// <param name="environmentId">Optional environment identifier.</param>
    /// <param name="hasFoliage">Whether foliage is also available.</param>
    /// <param name="techniques">The techniques to use.</param>
    /// <returns>A context with water available.</returns>
    /// <remarks>
    /// WaterCrossing will be valid in addition to the always-available techniques.
    /// </remarks>
    public static CounterTrackingContext CreateNearWater(
        string concealerId,
        string? environmentId,
        bool hasFoliage,
        params ConcealmentTechnique[] techniques)
    {
        return new CounterTrackingContext(
            ConcealerId: concealerId,
            TechniquesUsed: techniques,
            EnvironmentId: environmentId,
            HasWaterNearby: true,
            HasFoliageOrDebris: hasFoliage);
    }
}
