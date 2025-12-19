using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Entities;

/// <summary>
/// Represents an interactable object within a room.
/// Supports multi-tier examination (WITS-based) and container mechanics.
/// </summary>
public class InteractableObject
{
    #region Identity

    /// <summary>
    /// Gets or sets the unique identifier for this object.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the ID of the room containing this object.
    /// </summary>
    public Guid RoomId { get; set; }

    /// <summary>
    /// Gets or sets the display name of the object.
    /// Used for player targeting (e.g., "examine chest").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of object, determining available interactions.
    /// </summary>
    public ObjectType ObjectType { get; set; } = ObjectType.Furniture;

    #endregion

    #region Description Layers

    /// <summary>
    /// Gets or sets the base description visible to all players.
    /// This is always shown regardless of examination roll.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the detailed description revealed with 1+ net successes.
    /// Contains additional sensory details and observable information.
    /// </summary>
    public string? DetailedDescription { get; set; }

    /// <summary>
    /// Gets or sets the expert description revealed with 3+ net successes.
    /// Contains hidden lore, mechanical insights, or secret information.
    /// </summary>
    public string? ExpertDescription { get; set; }

    #endregion

    #region Container Properties

    /// <summary>
    /// Gets or sets whether this object can contain other items.
    /// </summary>
    public bool IsContainer { get; set; } = false;

    /// <summary>
    /// Gets or sets whether the container is currently open.
    /// Only relevant if IsContainer is true.
    /// </summary>
    public bool IsOpen { get; set; } = false;

    /// <summary>
    /// Gets or sets whether the container is locked.
    /// Locked containers cannot be opened without a key or lockpicking.
    /// </summary>
    public bool IsLocked { get; set; } = false;

    /// <summary>
    /// Gets or sets the difficulty to pick the lock (net successes required).
    /// Only relevant if IsLocked is true.
    /// </summary>
    public int LockDifficulty { get; set; } = 0;

    #endregion

    #region Examination State

    /// <summary>
    /// Gets or sets whether this object has been examined at least once.
    /// </summary>
    public bool HasBeenExamined { get; set; } = false;

    /// <summary>
    /// Gets or sets the highest examination tier achieved.
    /// 0 = none/base only, 1 = detailed revealed, 2 = expert revealed.
    /// </summary>
    public int HighestExaminationTier { get; set; } = 0;

    #endregion

    #region Loot Properties

    /// <summary>
    /// Gets or sets whether this container/corpse has been searched for loot.
    /// Once searched, loot is generated and placed in the container.
    /// </summary>
    public bool HasBeenSearched { get; set; } = false;

    /// <summary>
    /// Gets or sets the quality tier for loot generation.
    /// Higher tiers yield better quality items. Null means no loot.
    /// </summary>
    public QualityTier? LootTier { get; set; }

    #endregion

    #region Metadata

    /// <summary>
    /// Gets or sets the timestamp when this object was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the timestamp when this object was last modified.
    /// </summary>
    public DateTime LastModified { get; set; } = DateTime.UtcNow;

    #endregion
}
