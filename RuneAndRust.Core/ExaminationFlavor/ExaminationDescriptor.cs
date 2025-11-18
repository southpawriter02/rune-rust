// ==============================================================================
// v0.38.9: Perception & Examination Descriptors
// ExaminationDescriptor.cs
// ==============================================================================
// Purpose: Object examination with layered detail levels
// Pattern: Follows CombatActionDescriptor and GaldrActionDescriptor structure
// Integration: Used by ExaminationFlavorTextService to generate layered narratives
// ==============================================================================

namespace RuneAndRust.Core.ExaminationFlavor;

/// <summary>
/// Represents an object examination descriptor with layered detail levels.
/// Describes objects at Cursory, Detailed, and Expert examination levels.
/// </summary>
public class ExaminationDescriptor
{
    // ==================== IDENTITY ====================

    /// <summary>
    /// Unique identifier for this descriptor.
    /// </summary>
    public int DescriptorId { get; set; }

    // ==================== CLASSIFICATION ====================

    /// <summary>
    /// Primary category of the object.
    /// Values: Door, Machinery, Decorative, Structural, Container, Furniture,
    ///         Environmental, Corpse
    /// </summary>
    public string ObjectCategory { get; set; } = string.Empty;

    /// <summary>
    /// Specific type within the category.
    /// Examples: LockedDoor, BlastDoor, ServitorCorpse, AncientConsole,
    ///           WallInscription, Skeleton, SupportPillar
    /// </summary>
    public string? ObjectType { get; set; }

    /// <summary>
    /// Detail level for this examination.
    /// Values: Cursory (no check), Detailed (WITS DC 12), Expert (WITS DC 18)
    /// </summary>
    public string DetailLevel { get; set; } = string.Empty;

    // ==================== CONTEXT ====================

    /// <summary>
    /// Biome where this examination applies.
    /// Values: The_Roots, Muspelheim, Niflheim, Alfheim, Jotunheim, NULL (any)
    /// </summary>
    public string? BiomeName { get; set; }

    /// <summary>
    /// Current state of the object.
    /// Values: Intact, Damaged, Destroyed, Locked, Sealed, Operational, Dormant
    /// </summary>
    public string? ObjectState { get; set; }

    // ==================== TEMPLATE TEXT ====================

    /// <summary>
    /// The examination text template with {Variable} placeholders.
    ///
    /// Available variables:
    /// - {Object}, {ObjectState}, {Material}, {Age}
    /// - {Biome}, {Location}, {Direction}
    /// - {Player}, {WITS}
    /// - {Faction}, {Era}, {Manufacturer}
    ///
    /// Example: "A heavy iron door reinforced with Jötun metalwork..."
    /// </summary>
    public string DescriptorText { get; set; } = string.Empty;

    // ==================== METADATA ====================

    /// <summary>
    /// Probability weight for random selection (1.0 = default).
    /// </summary>
    public float Weight { get; set; } = 1.0f;

    /// <summary>
    /// Whether this descriptor is active and can be selected.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// JSON array of tags for categorization.
    /// Example: ["Lore", "Technical", "Historical"]
    /// </summary>
    public string? Tags { get; set; }

    // ==================== ENUMERATIONS ====================

    /// <summary>
    /// Valid object categories.
    /// </summary>
    public static class Categories
    {
        public const string Door = "Door";
        public const string Machinery = "Machinery";
        public const string Decorative = "Decorative";
        public const string Structural = "Structural";
        public const string Container = "Container";
        public const string Furniture = "Furniture";
        public const string Environmental = "Environmental";
        public const string Corpse = "Corpse";
    }

    /// <summary>
    /// Valid detail levels.
    /// </summary>
    public static class DetailLevels
    {
        public const string Cursory = "Cursory";      // No check required
        public const string Detailed = "Detailed";    // WITS DC 12
        public const string Expert = "Expert";        // WITS DC 18
    }

    /// <summary>
    /// Common object types.
    /// </summary>
    public static class ObjectTypes
    {
        // Doors
        public const string LockedDoor = "LockedDoor";
        public const string BlastDoor = "BlastDoor";
        public const string SecurityDoor = "SecurityDoor";

        // Machinery
        public const string ServitorCorpse = "ServitorCorpse";
        public const string AncientConsole = "AncientConsole";
        public const string ControlPanel = "ControlPanel";
        public const string PowerConduit = "PowerConduit";

        // Decorative
        public const string WallInscription = "WallInscription";
        public const string Skeleton = "Skeleton";
        public const string Mural = "Mural";
        public const string Statue = "Statue";

        // Structural
        public const string SupportPillar = "SupportPillar";
        public const string Wall = "Wall";
        public const string Floor = "Floor";
        public const string Ceiling = "Ceiling";
    }

    /// <summary>
    /// Valid object states.
    /// </summary>
    public static class States
    {
        public const string Intact = "Intact";
        public const string Damaged = "Damaged";
        public const string Destroyed = "Destroyed";
        public const string Locked = "Locked";
        public const string Sealed = "Sealed";
        public const string Operational = "Operational";
        public const string Dormant = "Dormant";
    }
}
