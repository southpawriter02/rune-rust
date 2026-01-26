namespace RuneAndRust.Domain.Entities;

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;

/// <summary>
/// Tracks the highest examination layer unlocked for a character-object pair.
/// </summary>
/// <remarks>
/// <para>
/// Persists examination progress so characters don't need to re-roll for
/// already-unlocked layers. Only stores the highest layer achieved, as
/// lower layers are automatically included.
/// </para>
/// <para>
/// Key behaviors:
/// </para>
/// <list type="bullet">
///   <item><description>Layer 1 (Cursory) is always available without state</description></item>
///   <item><description>Layer 2 (Detailed) is persisted after DC 12 success</description></item>
///   <item><description>Layer 3 (Expert) is persisted after DC 18 success</description></item>
///   <item><description>Layers never regress - only improve</description></item>
/// </list>
/// </remarks>
public class ExaminationState : IEntity
{
    /// <summary>
    /// Gets the unique identifier for this state record.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the character who performed the examination.
    /// </summary>
    public string CharacterId { get; private set; }

    /// <summary>
    /// Gets the object that was examined.
    /// </summary>
    public string ObjectId { get; private set; }

    /// <summary>
    /// Gets the highest examination layer unlocked (1, 2, or 3).
    /// </summary>
    public int HighestLayerUnlocked { get; private set; }

    /// <summary>
    /// Gets when this object was first examined by this character.
    /// </summary>
    public DateTime FirstExaminedAt { get; private set; }

    /// <summary>
    /// Gets when this object was last examined by this character.
    /// </summary>
    public DateTime LastExaminedAt { get; private set; }

    /// <summary>
    /// Gets the examination layer enum value.
    /// </summary>
    public ExaminationLayer Layer => (ExaminationLayer)HighestLayerUnlocked;

    /// <summary>
    /// Gets whether the character has Layer 2 (Detailed) knowledge.
    /// </summary>
    public bool HasDetailedKnowledge => HighestLayerUnlocked >= (int)ExaminationLayer.Detailed;

    /// <summary>
    /// Gets whether the character has Layer 3 (Expert) knowledge.
    /// </summary>
    public bool HasExpertKnowledge => HighestLayerUnlocked >= (int)ExaminationLayer.Expert;

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private ExaminationState()
    {
        CharacterId = null!;
        ObjectId = null!;
    }

    /// <summary>
    /// Creates a new examination state record.
    /// </summary>
    /// <param name="characterId">The character identifier.</param>
    /// <param name="objectId">The object identifier.</param>
    /// <param name="layer">The highest layer unlocked.</param>
    /// <returns>A new ExaminationState entity.</returns>
    /// <exception cref="ArgumentException">Thrown when IDs are null or whitespace.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when layer is out of range.</exception>
    public static ExaminationState Create(string characterId, string objectId, int layer)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(characterId);
        ArgumentException.ThrowIfNullOrWhiteSpace(objectId);
        ArgumentOutOfRangeException.ThrowIfLessThan(layer, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(layer, 3);

        var now = DateTime.UtcNow;
        return new ExaminationState
        {
            Id = Guid.NewGuid(),
            CharacterId = characterId,
            ObjectId = objectId,
            HighestLayerUnlocked = layer,
            FirstExaminedAt = now,
            LastExaminedAt = now
        };
    }

    /// <summary>
    /// Updates the highest layer if the new layer is higher.
    /// </summary>
    /// <param name="newLayer">The new layer achieved.</param>
    /// <returns>True if the layer was upgraded, false if unchanged.</returns>
    /// <remarks>
    /// Layers never regress - if the new layer is lower than or equal to
    /// the current highest, no change is made.
    /// </remarks>
    public bool UpdateLayer(int newLayer)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(newLayer, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(newLayer, 3);

        LastExaminedAt = DateTime.UtcNow;

        if (newLayer > HighestLayerUnlocked)
        {
            HighestLayerUnlocked = newLayer;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Returns a string representation of this state for debugging.
    /// </summary>
    /// <returns>A formatted string showing key state values.</returns>
    public override string ToString() =>
        $"ExaminationState(Character={CharacterId}, Object={ObjectId}, Layer={HighestLayerUnlocked})";
}
