namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the activation result of an active Wasteland Survival specialization ability.
/// </summary>
/// <remarks>
/// <para>
/// Active abilities are triggered by player action rather than being applied
/// automatically. When activated, they provide effects such as revealing information,
/// marking areas, or granting temporary bonuses.
/// </para>
/// <para>
/// <b>Activation Types:</b>
/// <list type="bullet">
///   <item><description><see cref="AbilityActivationType.PostCheck"/>: Activated after a successful check (e.g., Predator's Eye)</description></item>
///   <item><description><see cref="AbilityActivationType.PathReveal"/>: Reveals a safe or advantageous path (e.g., Mire Knowledge, Rooftop Routes)</description></item>
///   <item><description><see cref="AbilityActivationType.ZoneMarker"/>: Marks an area for persistent bonuses (e.g., Hunting Grounds)</description></item>
/// </list>
/// </para>
/// <para>
/// The <see cref="AdditionalData"/> dictionary contains ability-specific information
/// that the calling service can use to implement the effect. For example, Predator's Eye
/// might include creature weakness type and behavior patterns.
/// </para>
/// </remarks>
/// <param name="AbilityId">The unique identifier of the ability that was activated.</param>
/// <param name="Type">The type of activation effect.</param>
/// <param name="EffectDescription">A human-readable description of what happened.</param>
/// <param name="AdditionalData">Ability-specific data for implementing the effect.</param>
/// <seealso cref="SpecializationBonus"/>
/// <seealso cref="HuntingGroundsMarker"/>
public readonly record struct AbilityActivation(
    string AbilityId,
    AbilityActivationType Type,
    string EffectDescription,
    IReadOnlyDictionary<string, string> AdditionalData)
{
    // =========================================================================
    // COMPUTED PROPERTIES
    // =========================================================================

    /// <summary>
    /// Gets a value indicating whether this activation is valid.
    /// </summary>
    /// <value>True if the ability ID is not null or whitespace.</value>
    public bool IsValid => !string.IsNullOrWhiteSpace(AbilityId);

    /// <summary>
    /// Gets a value indicating whether this activation has additional data.
    /// </summary>
    /// <value>True if <see cref="AdditionalData"/> contains any entries.</value>
    public bool HasAdditionalData => AdditionalData.Count > 0;

    /// <summary>
    /// Gets a value indicating whether this is a post-check activation.
    /// </summary>
    /// <value>True if <see cref="Type"/> is <see cref="AbilityActivationType.PostCheck"/>.</value>
    public bool IsPostCheck => Type == AbilityActivationType.PostCheck;

    /// <summary>
    /// Gets a value indicating whether this reveals a path.
    /// </summary>
    /// <value>True if <see cref="Type"/> is <see cref="AbilityActivationType.PathReveal"/>.</value>
    public bool RevealsPath => Type == AbilityActivationType.PathReveal;

    /// <summary>
    /// Gets a value indicating whether this marks a zone.
    /// </summary>
    /// <value>True if <see cref="Type"/> is <see cref="AbilityActivationType.ZoneMarker"/>.</value>
    public bool MarksZone => Type == AbilityActivationType.ZoneMarker;

    // =========================================================================
    // DATA ACCESS METHODS
    // =========================================================================

    /// <summary>
    /// Gets a value from additional data by key, or a default value if not found.
    /// </summary>
    /// <param name="key">The key to look up.</param>
    /// <param name="defaultValue">The default value to return if key is not found.</param>
    /// <returns>The value associated with the key, or the default value.</returns>
    /// <example>
    /// <code>
    /// var activation = AbilityActivation.PredatorsEye("fire", "aggressive-territorial");
    /// var weakness = activation.GetData("weakness", "unknown");
    /// // Returns "fire"
    /// </code>
    /// </example>
    public string GetData(string key, string defaultValue = "")
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return defaultValue;
        }

        return AdditionalData.TryGetValue(key, out var value) ? value : defaultValue;
    }

    /// <summary>
    /// Checks if the additional data contains a specific key.
    /// </summary>
    /// <param name="key">The key to check for.</param>
    /// <returns>True if the key exists in additional data.</returns>
    public bool HasData(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return false;
        }

        return AdditionalData.ContainsKey(key);
    }

    // =========================================================================
    // FACTORY METHODS - VEIOIMAOR ABILITIES
    // =========================================================================

    /// <summary>
    /// Creates a Predator's Eye activation result.
    /// </summary>
    /// <param name="creatureWeakness">The weakness type of the tracked creature.</param>
    /// <param name="behaviorPattern">The behavior pattern of the tracked creature.</param>
    /// <returns>An ability activation for Predator's Eye.</returns>
    /// <remarks>
    /// <para>
    /// Predator's Eye is activated after a successful tracking check against a
    /// living creature. It reveals the creature's weakness and behavior patterns,
    /// which can be used in subsequent combat or interaction.
    /// </para>
    /// <para>
    /// <b>Additional Data Keys:</b>
    /// <list type="bullet">
    ///   <item><description>"weakness": The creature's vulnerability (e.g., "fire", "cold")</description></item>
    ///   <item><description>"behavior": The creature's behavior pattern (e.g., "aggressive-territorial")</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // After successfully tracking a wasteland wolf
    /// var activation = AbilityActivation.PredatorsEye("fire", "pack-hunter");
    /// Console.WriteLine(activation.EffectDescription);
    /// // Output: "You discern the creature's vulnerability to fire and recognize its pack-hunter behavior."
    /// </code>
    /// </example>
    public static AbilityActivation PredatorsEye(string creatureWeakness, string behaviorPattern)
    {
        var data = new Dictionary<string, string>
        {
            ["weakness"] = creatureWeakness ?? "unknown",
            ["behavior"] = behaviorPattern ?? "unknown"
        };

        var weaknessText = string.IsNullOrWhiteSpace(creatureWeakness)
            ? "an unknown vulnerability"
            : $"vulnerability to {creatureWeakness}";
        var behaviorText = string.IsNullOrWhiteSpace(behaviorPattern)
            ? "unknown behavior"
            : $"{behaviorPattern} behavior";

        return new AbilityActivation(
            AbilityId: "predators-eye",
            Type: AbilityActivationType.PostCheck,
            EffectDescription: $"You discern the creature's {weaknessText} and recognize its {behaviorText}.",
            AdditionalData: data);
    }

    /// <summary>
    /// Creates a Hunting Grounds activation result.
    /// </summary>
    /// <param name="areaId">The identifier of the marked area.</param>
    /// <param name="areaName">The display name of the marked area.</param>
    /// <returns>An ability activation for Hunting Grounds.</returns>
    /// <remarks>
    /// <para>
    /// Hunting Grounds is activated when a Veioimaor marks an area as their
    /// hunting territory. All Wasteland Survival checks in that area receive
    /// +2d10 until the character rests.
    /// </para>
    /// <para>
    /// <b>Additional Data Keys:</b>
    /// <list type="bullet">
    ///   <item><description>"areaId": The unique identifier of the marked area</description></item>
    ///   <item><description>"areaName": The display name of the area</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var activation = AbilityActivation.HuntingGrounds("area-12", "The Rusted Valley");
    /// Console.WriteLine(activation.EffectDescription);
    /// // Output: "You mark The Rusted Valley as your hunting grounds. +2d10 to all Wasteland Survival checks in this area until rest."
    /// </code>
    /// </example>
    public static AbilityActivation HuntingGrounds(string areaId, string areaName)
    {
        var data = new Dictionary<string, string>
        {
            ["areaId"] = areaId ?? string.Empty,
            ["areaName"] = areaName ?? "this area"
        };

        var displayName = string.IsNullOrWhiteSpace(areaName) ? "this area" : areaName;

        return new AbilityActivation(
            AbilityId: "hunting-grounds",
            Type: AbilityActivationType.ZoneMarker,
            EffectDescription: $"You mark {displayName} as your hunting grounds. +2d10 to all Wasteland Survival checks in this area until rest.",
            AdditionalData: data);
    }

    // =========================================================================
    // FACTORY METHODS - MYR-STALKER ABILITIES
    // =========================================================================

    /// <summary>
    /// Creates a Mire Knowledge activation result.
    /// </summary>
    /// <param name="pathDescription">A description of the safe path through the bog.</param>
    /// <param name="hazardsAvoided">Optional comma-separated list of avoided hazards.</param>
    /// <returns>An ability activation for Mire Knowledge.</returns>
    /// <remarks>
    /// <para>
    /// Mire Knowledge is activated when a Myr-Stalker is navigating a bog or
    /// swamp environment. It reveals a safe path through the hazards, allowing
    /// the party to avoid dangers that would otherwise require checks or cause damage.
    /// </para>
    /// <para>
    /// <b>Additional Data Keys:</b>
    /// <list type="bullet">
    ///   <item><description>"path": A description of the safe path</description></item>
    ///   <item><description>"hazardsAvoided": List of hazards that are avoided by taking this path</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var activation = AbilityActivation.MireKnowledge(
    ///     "Follow the moss-covered stones west, then skirt the standing water",
    ///     "toxic gas pocket, quicksand");
    /// </code>
    /// </example>
    public static AbilityActivation MireKnowledge(string pathDescription, string? hazardsAvoided = null)
    {
        var data = new Dictionary<string, string>
        {
            ["path"] = pathDescription ?? "a safe route through the mire",
            ["hazardsAvoided"] = hazardsAvoided ?? string.Empty
        };

        var description = string.IsNullOrWhiteSpace(pathDescription)
            ? "You identify a safe path through the mire."
            : $"You identify a safe path: {pathDescription}";

        if (!string.IsNullOrWhiteSpace(hazardsAvoided))
        {
            description += $" This route avoids: {hazardsAvoided}.";
        }

        return new AbilityActivation(
            AbilityId: "mire-knowledge",
            Type: AbilityActivationType.PathReveal,
            EffectDescription: description,
            AdditionalData: data);
    }

    // =========================================================================
    // FACTORY METHODS - GANTRY-RUNNER ABILITIES
    // =========================================================================

    /// <summary>
    /// Creates a Rooftop Routes activation result.
    /// </summary>
    /// <param name="routeDescription">A description of the elevated route.</param>
    /// <param name="destinationReachable">The destination that can be reached via this route.</param>
    /// <returns>An ability activation for Rooftop Routes.</returns>
    /// <remarks>
    /// <para>
    /// Rooftop Routes is activated when a Gantry-Runner is seeking an elevated path
    /// through ruins. It reveals a safe route across rooftops and gantries that
    /// may bypass ground-level obstacles or dangers.
    /// </para>
    /// <para>
    /// <b>Additional Data Keys:</b>
    /// <list type="bullet">
    ///   <item><description>"route": A description of the elevated path</description></item>
    ///   <item><description>"destination": The location that can be reached via this route</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var activation = AbilityActivation.RooftopRoutes(
    ///     "Jump to the fire escape, cross via the gantry, drop down at the old water tower",
    ///     "The abandoned market district");
    /// </code>
    /// </example>
    public static AbilityActivation RooftopRoutes(string routeDescription, string? destinationReachable = null)
    {
        var data = new Dictionary<string, string>
        {
            ["route"] = routeDescription ?? "an elevated route across the rooftops",
            ["destination"] = destinationReachable ?? string.Empty
        };

        var description = string.IsNullOrWhiteSpace(routeDescription)
            ? "You spot an elevated route across the rooftops."
            : $"You spot an elevated route: {routeDescription}";

        if (!string.IsNullOrWhiteSpace(destinationReachable))
        {
            description += $" This leads to {destinationReachable}.";
        }

        return new AbilityActivation(
            AbilityId: "rooftop-routes",
            Type: AbilityActivationType.PathReveal,
            EffectDescription: description,
            AdditionalData: data);
    }

    // =========================================================================
    // UTILITY METHODS
    // =========================================================================

    /// <summary>
    /// Creates an empty, invalid activation.
    /// </summary>
    /// <returns>An empty <see cref="AbilityActivation"/>.</returns>
    /// <remarks>
    /// Used as a default return value when no ability is activated.
    /// Check <see cref="IsValid"/> before using the activation.
    /// </remarks>
    public static AbilityActivation Empty() => new(
        AbilityId: string.Empty,
        Type: AbilityActivationType.PostCheck,
        EffectDescription: string.Empty,
        AdditionalData: new Dictionary<string, string>());

    /// <summary>
    /// Returns a formatted display string for this activation.
    /// </summary>
    /// <returns>A string suitable for display in the UI.</returns>
    public string ToDisplayString()
    {
        if (!IsValid)
        {
            return "No ability activated";
        }

        return EffectDescription;
    }

    /// <summary>
    /// Returns a string representation of this activation.
    /// </summary>
    /// <returns>A detailed string representation.</returns>
    public override string ToString() =>
        $"AbilityActivation {{ AbilityId = {AbilityId}, Type = {Type}, " +
        $"DataKeys = [{string.Join(", ", AdditionalData.Keys)}] }}";
}

/// <summary>
/// Types of ability activations.
/// </summary>
/// <remarks>
/// Determines how the ability effect is applied and what kind of result
/// the calling service should expect.
/// </remarks>
public enum AbilityActivationType
{
    /// <summary>
    /// Ability is activated after a successful skill check.
    /// </summary>
    /// <remarks>
    /// Post-check abilities provide information or effects based on the
    /// result of a completed check. For example, Predator's Eye is activated
    /// after a successful tracking check to reveal creature information.
    /// </remarks>
    PostCheck = 0,

    /// <summary>
    /// Ability reveals a safe or advantageous path.
    /// </summary>
    /// <remarks>
    /// Path reveal abilities allow the character to identify routes that
    /// others would miss. The revealed path may bypass hazards, avoid
    /// enemies, or provide tactical advantages.
    /// </remarks>
    PathReveal = 1,

    /// <summary>
    /// Ability marks an area for persistent bonuses.
    /// </summary>
    /// <remarks>
    /// Zone marker abilities designate an area where the character and their
    /// allies receive ongoing bonuses. The marker typically persists until
    /// the character rests or leaves the area.
    /// </remarks>
    ZoneMarker = 2
}
