namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Categorizes types of hidden elements that can be discovered through perception.
/// </summary>
/// <remarks>
/// <para>
/// Each type has distinct gameplay implications:
/// <list type="bullet">
///   <item><description>Traps deal damage or apply effects when triggered</description></item>
///   <item><description>Secret doors provide alternate routes</description></item>
///   <item><description>Hidden caches contain valuable loot</description></item>
///   <item><description>Secret compartments offer smaller storage finds</description></item>
///   <item><description>Ambush sites warn of enemy presence</description></item>
///   <item><description>Clue elements advance investigation objectives</description></item>
/// </list>
/// </para>
/// </remarks>
public enum HiddenElementType
{
    /// <summary>
    /// A concealed trap mechanism (pressure plate, tripwire, magical glyph).
    /// </summary>
    /// <remarks>
    /// Discovering a trap allows the character to avoid or disarm it.
    /// Interaction data typically includes: triggerType, damage, disarmDc.
    /// </remarks>
    Trap = 0,

    /// <summary>
    /// A concealed passage or doorway.
    /// </summary>
    /// <remarks>
    /// Discovering a secret door reveals a new exit from the room.
    /// Interaction data typically includes: destinationRoomId, unlockRequirements.
    /// </remarks>
    SecretDoor = 1,

    /// <summary>
    /// A hidden container with valuable contents.
    /// </summary>
    /// <remarks>
    /// Caches contain loot that can be collected.
    /// Interaction data typically includes: lootTableId, containerType.
    /// </remarks>
    HiddenCache = 2,

    /// <summary>
    /// A small hidden storage space within furniture or walls.
    /// </summary>
    /// <remarks>
    /// Smaller than caches, often containing single items or notes.
    /// Interaction data typically includes: contents, openMethod.
    /// </remarks>
    SecretCompartment = 3,

    /// <summary>
    /// A location where enemies are hiding in wait.
    /// </summary>
    /// <remarks>
    /// Discovering an ambush site allows preparation or avoidance.
    /// Interaction data typically includes: enemyIds, triggerConditions.
    /// </remarks>
    AmbushSite = 4,

    /// <summary>
    /// An element relevant to an ongoing investigation.
    /// </summary>
    /// <remarks>
    /// Clues contribute to solving mysteries or puzzles.
    /// Interaction data typically includes: clueId, relatedPuzzleId.
    /// </remarks>
    ClueElement = 5,

    /// <summary>
    /// Legacy alias for <see cref="HiddenCache"/>.
    /// </summary>
    [Obsolete("Use HiddenCache instead.")]
    Cache = 2
}
