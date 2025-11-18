// ==============================================================================
// v0.38.11: NPC Descriptors & Dialogue Barks
// NPCAmbientBarkDescriptor.cs
// ==============================================================================
// Purpose: Ambient dialogue barks for NPCs by archetype, activity, and context
// Pattern: Follows SkillCheckDescriptor structure from v0.38.10
// Integration: Used by NPCFlavorTextService to generate ambient NPC dialogue
// ==============================================================================

namespace RuneAndRust.Core.NPCFlavor;

/// <summary>
/// Represents an ambient dialogue bark descriptor for NPCs.
/// Describes what NPCs say while idle, working, or observing their environment.
/// v0.38.11: NPC Descriptors & Dialogue Barks
/// </summary>
public class NPCAmbientBarkDescriptor
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
    /// </summary>
    public string NPCSubtype { get; set; } = string.Empty;

    /// <summary>
    /// Type of bark/dialogue.
    /// Values: AtWork, IdleConversation, Observation, Warning, Celebration,
    ///         Concern, Suspicion, Encouragement, Complaint, Teaching,
    ///         Threat, Fleeing, Wounded
    /// </summary>
    public string BarkType { get; set; } = string.Empty;

    // ==================== CONTEXTUAL MODIFIERS ====================

    /// <summary>
    /// Activity context (optional).
    /// Values: Working, Idle, Trading, Guarding, Crafting, Traveling,
    ///         Fighting, Resting, Searching, NULL
    /// </summary>
    public string? ActivityContext { get; set; }

    /// <summary>
    /// Disposition toward player (optional).
    /// Values: Hostile, Unfriendly, Neutral, Friendly, Allied, NULL
    /// </summary>
    public string? DispositionContext { get; set; }

    /// <summary>
    /// Biome-specific context (optional).
    /// Values: Muspelheim, Niflheim, Alfheim, The_Roots, NULL
    /// </summary>
    public string? BiomeContext { get; set; }

    /// <summary>
    /// Trigger condition (optional).
    /// Values: PlayerNearby, PlayerAbsent, AllyPresent, EnemyNear,
    ///         DangerDetected, ResourceFound, NULL
    /// </summary>
    public string? TriggerCondition { get; set; }

    // ==================== TEMPLATE TEXT ====================

    /// <summary>
    /// The dialogue bark text with {Variable} placeholders.
    ///
    /// Available variables:
    /// - NPC: {NPCName}, {NPCArchetype}, {NPCSubtype}
    /// - PLAYER: {PlayerName}, {PlayerClass}, {PlayerFaction}
    /// - WORLD: {Biome}, {LocationName}, {TimeOfDay}
    /// - CONTEXT: {Activity}, {Disposition}, {Condition}
    /// - TECHNICAL: {Tool}, {Resource}, {ItemName}
    ///
    /// Example: "Tolerance specifications are off by point-oh-three millimeters. Unacceptable."
    /// Example: "The runes still answer, even here. Fehu, Uruz, Thurisaz..."
    /// Example: "Your gear or your life. Choose quick."
    /// </summary>
    public string DialogueText { get; set; } = string.Empty;

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
    /// Example: ["Dvergr_Technical", "Threatening", "Mystical", "Cultural_Reference"]
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
    /// Valid bark types.
    /// </summary>
    public static class BarkTypes
    {
        // Peaceful/Neutral
        public const string AtWork = "AtWork";
        public const string IdleConversation = "IdleConversation";
        public const string Observation = "Observation";
        public const string Teaching = "Teaching";
        public const string Celebration = "Celebration";

        // Cautionary
        public const string Warning = "Warning";
        public const string Concern = "Concern";
        public const string Suspicion = "Suspicion";
        public const string Complaint = "Complaint";

        // Positive
        public const string Encouragement = "Encouragement";
        public const string Greeting = "Greeting";

        // Hostile
        public const string Threat = "Threat";
        public const string Insult = "Insult";

        // Combat
        public const string Wounded = "Wounded";
        public const string Fleeing = "Fleeing";
        public const string BattleCry = "BattleCry";
    }

    /// <summary>
    /// Valid activity contexts.
    /// </summary>
    public static class ActivityContexts
    {
        public const string Working = "Working";
        public const string Idle = "Idle";
        public const string Trading = "Trading";
        public const string Guarding = "Guarding";
        public const string Crafting = "Crafting";
        public const string Traveling = "Traveling";
        public const string Fighting = "Fighting";
        public const string Resting = "Resting";
        public const string Searching = "Searching";
        public const string Performing_Ritual = "Performing_Ritual";
    }

    /// <summary>
    /// Valid disposition contexts.
    /// </summary>
    public static class DispositionContexts
    {
        public const string Hostile = "Hostile";
        public const string Unfriendly = "Unfriendly";
        public const string Neutral = "Neutral";
        public const string Friendly = "Friendly";
        public const string Allied = "Allied";
    }

    /// <summary>
    /// Valid trigger conditions.
    /// </summary>
    public static class TriggerConditions
    {
        public const string PlayerNearby = "PlayerNearby";
        public const string PlayerAbsent = "PlayerAbsent";
        public const string AllyPresent = "AllyPresent";
        public const string EnemyNear = "EnemyNear";
        public const string DangerDetected = "DangerDetected";
        public const string ResourceFound = "ResourceFound";
        public const string MechanismRepaired = "MechanismRepaired";
        public const string RitualComplete = "RitualComplete";
    }
}
