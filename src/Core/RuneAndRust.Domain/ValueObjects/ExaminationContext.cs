namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Encapsulates the context for an examination check.
/// </summary>
/// <remarks>
/// <para>
/// Contains all information needed to perform a WITS-based examination:
/// </para>
/// <list type="bullet">
///   <item><description>ObjectId - The object being examined</description></item>
///   <item><description>ObjectType - The type/category of the object</description></item>
///   <item><description>CharacterId - The character performing the examination</description></item>
///   <item><description>WitsPool - The character's WITS dice pool</description></item>
///   <item><description>BiomeId - Environmental context for descriptor selection</description></item>
///   <item><description>PreviousHighestLayer - Cached knowledge from prior examinations</description></item>
/// </list>
/// <para>
/// Previous examination state allows returning cached results for already-unlocked
/// layers without requiring re-rolls, ensuring characters don't lose knowledge.
/// </para>
/// </remarks>
/// <param name="ObjectId">The unique identifier of the object being examined.</param>
/// <param name="ObjectType">The type/category of the object (e.g., "Door", "Machinery").</param>
/// <param name="CharacterId">The unique identifier of the examining character.</param>
/// <param name="WitsPool">The character's WITS dice pool for the check.</param>
/// <param name="BiomeId">The current biome for contextual descriptions.</param>
/// <param name="PreviousHighestLayer">The highest layer previously unlocked (0 if never examined).</param>
public readonly record struct ExaminationContext(
    string ObjectId,
    string ObjectType,
    string CharacterId,
    int WitsPool,
    string BiomeId,
    int PreviousHighestLayer)
{
    /// <summary>
    /// Gets whether this object has been examined before by this character.
    /// </summary>
    /// <remarks>
    /// Returns true if PreviousHighestLayer is greater than 0, indicating
    /// the character has some prior knowledge of this object.
    /// </remarks>
    public bool HasPreviousExamination => PreviousHighestLayer > 0;

    /// <summary>
    /// Gets whether the character already has Layer 2 (Detailed) knowledge.
    /// </summary>
    /// <remarks>
    /// When true, Layer 2 descriptions are automatically included without
    /// requiring a new WITS check against DC 12.
    /// </remarks>
    public bool HasDetailedKnowledge => PreviousHighestLayer >= (int)ExaminationLayer.Detailed;

    /// <summary>
    /// Gets whether the character already has Layer 3 (Expert) knowledge.
    /// </summary>
    /// <remarks>
    /// When true, all layers are included automatically without any checks.
    /// The character has full knowledge of this object.
    /// </remarks>
    public bool HasExpertKnowledge => PreviousHighestLayer >= (int)ExaminationLayer.Expert;

    /// <summary>
    /// Gets the DC required for the next layer to unlock.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    ///   <item><description>Returns 0 if only Layer 1 is needed (auto-unlock)</description></item>
    ///   <item><description>Returns 12 if Layer 2 is the next target</description></item>
    ///   <item><description>Returns 18 if Layer 3 is the next target</description></item>
    ///   <item><description>Returns -1 if all layers are already unlocked</description></item>
    /// </list>
    /// </remarks>
    public int NextLayerDc
    {
        get
        {
            // All layers unlocked - no check needed
            if (HasExpertKnowledge)
                return -1;

            // Layer 2 unlocked, need Layer 3 (DC 18)
            if (HasDetailedKnowledge)
                return 18;

            // Layer 1 unlocked, need Layer 2 (DC 12)
            if (HasPreviousExamination)
                return 12;

            // First examination - Layer 1 is auto (DC 0)
            return 0;
        }
    }

    /// <summary>
    /// Creates an examination context for a first-time examination.
    /// </summary>
    /// <param name="objectId">The object identifier.</param>
    /// <param name="objectType">The object type.</param>
    /// <param name="characterId">The character identifier.</param>
    /// <param name="witsPool">The WITS dice pool.</param>
    /// <param name="biomeId">The current biome.</param>
    /// <returns>A new ExaminationContext with no previous examination history.</returns>
    /// <exception cref="ArgumentException">Thrown when any ID parameter is null or whitespace.</exception>
    public static ExaminationContext FirstExamination(
        string objectId,
        string objectType,
        string characterId,
        int witsPool,
        string biomeId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(objectId);
        ArgumentException.ThrowIfNullOrWhiteSpace(objectType);
        ArgumentException.ThrowIfNullOrWhiteSpace(characterId);
        ArgumentException.ThrowIfNullOrWhiteSpace(biomeId);
        ArgumentOutOfRangeException.ThrowIfNegative(witsPool);

        return new ExaminationContext(
            objectId,
            objectType,
            characterId,
            witsPool,
            biomeId,
            PreviousHighestLayer: 0);
    }

    /// <summary>
    /// Creates an examination context with previous examination history.
    /// </summary>
    /// <param name="objectId">The object identifier.</param>
    /// <param name="objectType">The object type.</param>
    /// <param name="characterId">The character identifier.</param>
    /// <param name="witsPool">The WITS dice pool.</param>
    /// <param name="biomeId">The current biome.</param>
    /// <param name="previousLayer">The highest layer previously unlocked (clamped to 0-3).</param>
    /// <returns>A new ExaminationContext with examination history.</returns>
    /// <exception cref="ArgumentException">Thrown when any ID parameter is null or whitespace.</exception>
    public static ExaminationContext WithHistory(
        string objectId,
        string objectType,
        string characterId,
        int witsPool,
        string biomeId,
        int previousLayer)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(objectId);
        ArgumentException.ThrowIfNullOrWhiteSpace(objectType);
        ArgumentException.ThrowIfNullOrWhiteSpace(characterId);
        ArgumentException.ThrowIfNullOrWhiteSpace(biomeId);
        ArgumentOutOfRangeException.ThrowIfNegative(witsPool);

        return new ExaminationContext(
            objectId,
            objectType,
            characterId,
            witsPool,
            biomeId,
            PreviousHighestLayer: Math.Clamp(previousLayer, 0, 3));
    }

    /// <summary>
    /// Returns a string representation of this context for debugging.
    /// </summary>
    /// <returns>A formatted string showing key context values.</returns>
    public override string ToString() =>
        $"ExaminationContext(Object={ObjectId}, Character={CharacterId}, WITS={WitsPool}, " +
        $"PreviousLayer={PreviousHighestLayer}, NextDC={NextLayerDc})";
}
