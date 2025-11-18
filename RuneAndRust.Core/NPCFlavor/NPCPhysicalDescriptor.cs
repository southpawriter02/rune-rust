// ==============================================================================
// v0.38.11: NPC Descriptors & Dialogue Barks
// NPCPhysicalDescriptor.cs
// ==============================================================================
// Purpose: Physical appearance descriptors for NPCs by archetype and subtype
// Pattern: Follows SkillCheckDescriptor structure from v0.38.10
// Integration: Used by NPCFlavorTextService to generate NPC appearance descriptions
// ==============================================================================

namespace RuneAndRust.Core.NPCFlavor;

/// <summary>
/// Represents a physical appearance descriptor for NPCs.
/// Describes NPC physical features, bearing, clothing, and distinguishing characteristics.
/// v0.38.11: NPC Descriptors & Dialogue Barks
/// </summary>
public class NPCPhysicalDescriptor
{
    // ==================== IDENTITY ====================

    /// <summary>
    /// Unique identifier for this descriptor.
    /// </summary>
    public int DescriptorId { get; set; }

    // ==================== CLASSIFICATION ====================

    /// <summary>
    /// NPC archetype/faction.
    /// Values: Dvergr, Seidkona, Bandit, Raider, Merchant, Guard, Citizen, Forlorn
    /// </summary>
    public string NPCArchetype { get; set; } = string.Empty;

    /// <summary>
    /// Specific subtype within the archetype.
    /// Dvergr: Tinkerer, Runecaster, Merchant
    /// Seidkona: WanderingSeidkona, YoungAcolyte, Seidmadr
    /// Bandit: Scout, Leader, DesperateOutcast
    /// Raider: Veteran, Brute, Scavenger
    /// Merchant: Prosperous, Struggling, Shrewd
    /// Guard: Veteran, Rookie, Captain
    /// Citizen: Laborer, Artisan, Elder
    /// Forlorn: Fresh, Deteriorated, Ancient
    /// </summary>
    public string NPCSubtype { get; set; } = string.Empty;

    /// <summary>
    /// Descriptor focus area.
    /// Values: FullBody, Face, Clothing, Equipment, Bearing, Distinguishing
    /// </summary>
    public string DescriptorType { get; set; } = string.Empty;

    // ==================== CONTEXTUAL MODIFIERS ====================

    /// <summary>
    /// NPC condition/state (optional).
    /// Values: Healthy, Wounded, Exhausted, Affluent, Impoverished, NULL
    /// </summary>
    public string? Condition { get; set; }

    /// <summary>
    /// Biome-specific context (optional).
    /// Values: Muspelheim, Niflheim, Alfheim, The_Roots, NULL
    /// Affects clothing choices, weathering, environmental damage
    /// </summary>
    public string? BiomeContext { get; set; }

    /// <summary>
    /// Age category (optional).
    /// Values: Young, MiddleAged, Elderly, Ageless, NULL
    /// </summary>
    public string? AgeCategory { get; set; }

    // ==================== TEMPLATE TEXT ====================

    /// <summary>
    /// The flavor text template with {Variable} placeholders.
    ///
    /// Available variables:
    /// - NPC: {NPCName}, {NPCArchetype}, {NPCSubtype}
    /// - PHYSICAL: {Height}, {Build}, {HairColor}, {EyeColor}
    /// - EQUIPMENT: {Weapon}, {Armor}, {Tool}, {Accessory}
    /// - CONDITION: {Wounds}, {Scars}, {Weathering}
    /// - ENVIRONMENT: {Biome}, {Weather}
    ///
    /// Example: "A stocky Dvergr covered in soot and machine oil, tools hanging from every belt loop."
    /// Example: "This Seiðkona's eyes are distant, as if always seeing through the veil of reality."
    /// </summary>
    public string DescriptorText { get; set; } = string.Empty;

    // ==================== METADATA ====================

    /// <summary>
    /// Probability weight for random selection (1.0 = default).
    /// Higher values increase selection chance.
    /// </summary>
    public float Weight { get; set; } = 1.0f;

    /// <summary>
    /// Whether this descriptor is active and can be selected.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// JSON array of tags for categorization and filtering.
    /// Example: ["Intimidating", "Memorable", "Distinctive", "Professional"]
    /// </summary>
    public string? Tags { get; set; }

    // ==================== ENUMERATIONS ====================

    /// <summary>
    /// Valid NPC archetypes.
    /// </summary>
    public static class NPCArchetypes
    {
        public const string Dvergr = "Dvergr";
        public const string Seidkona = "Seidkona";
        public const string Bandit = "Bandit";
        public const string Raider = "Raider";
        public const string Merchant = "Merchant";
        public const string Guard = "Guard";
        public const string Citizen = "Citizen";
        public const string Forlorn = "Forlorn";
    }

    /// <summary>
    /// Valid descriptor types.
    /// </summary>
    public static class DescriptorTypes
    {
        public const string FullBody = "FullBody";           // Complete appearance
        public const string Face = "Face";                   // Facial features
        public const string Clothing = "Clothing";           // Attire and dress
        public const string Equipment = "Equipment";         // Tools, weapons, gear
        public const string Bearing = "Bearing";             // Posture, demeanor
        public const string Distinguishing = "Distinguishing"; // Unique features
    }

    /// <summary>
    /// Valid condition states.
    /// </summary>
    public static class Conditions
    {
        public const string Healthy = "Healthy";
        public const string Wounded = "Wounded";
        public const string Exhausted = "Exhausted";
        public const string Affluent = "Affluent";
        public const string Impoverished = "Impoverished";
        public const string BattleReady = "BattleReady";
    }

    /// <summary>
    /// Valid age categories.
    /// </summary>
    public static class AgeCategories
    {
        public const string Young = "Young";
        public const string MiddleAged = "MiddleAged";
        public const string Elderly = "Elderly";
        public const string Ageless = "Ageless";           // For Forlorn/mystical beings
    }
}
