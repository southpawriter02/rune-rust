// ==============================================================================
// v0.38.11: NPC Descriptors & Dialogue Barks
// NPCReactionDescriptor.cs
// ==============================================================================
// Purpose: Emotional reaction descriptors for NPCs responding to events
// Pattern: Follows SkillCheckDescriptor structure from v0.38.10
// Integration: Used by NPCFlavorTextService to generate NPC emotional reactions
// ==============================================================================

namespace RuneAndRust.Core.NPCFlavor;

/// <summary>
/// Represents an emotional reaction descriptor for NPCs.
/// Describes how NPCs react to player actions, events, and environmental changes.
/// v0.38.11: NPC Descriptors & Dialogue Barks
/// </summary>
public class NPCReactionDescriptor
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
    /// Type of emotional reaction.
    /// Values: Surprised, Angry, Fearful, Relieved, Suspicious, Joyful,
    ///         Pained, Confused, Impressed, Disgusted, Grateful, Betrayed
    /// </summary>
    public string ReactionType { get; set; } = string.Empty;

    // ==================== TRIGGER CLASSIFICATION ====================

    /// <summary>
    /// Event that triggered the reaction.
    /// Values: PlayerApproaches, PlayerAttacks, AllyKilled, TreasureFound,
    ///         MechanismRepaired, TrapTriggered, QuestCompleted, BetrayalDetected,
    ///         GiftReceived, TheftDetected, BlightEncounter, MagicWitnessed
    /// </summary>
    public string TriggerEvent { get; set; } = string.Empty;

    // ==================== CONTEXTUAL MODIFIERS ====================

    /// <summary>
    /// Intensity of the reaction (optional).
    /// Values: Mild, Moderate, Strong, Extreme, NULL
    /// </summary>
    public string? Intensity { get; set; }

    /// <summary>
    /// Disposition toward player before the event (optional).
    /// Values: Hostile, Unfriendly, Neutral, Friendly, Allied, NULL
    /// Affects how the reaction is expressed
    /// </summary>
    public string? PriorDisposition { get; set; }

    /// <summary>
    /// Resulting action tendency (optional).
    /// Values: Approach, Flee, Attack, Assist, Ignore, Report, NULL
    /// Describes likely follow-up behavior
    /// </summary>
    public string? ActionTendency { get; set; }

    /// <summary>
    /// Biome-specific context (optional).
    /// Values: Muspelheim, Niflheim, Alfheim, The_Roots, NULL
    /// </summary>
    public string? BiomeContext { get; set; }

    // ==================== TEMPLATE TEXT ====================

    /// <summary>
    /// The reaction dialogue/description with {Variable} placeholders.
    ///
    /// Available variables:
    /// - NPC: {NPCName}, {NPCArchetype}, {NPCSubtype}
    /// - PLAYER: {PlayerName}, {PlayerClass}, {PlayerAction}
    /// - EVENT: {TriggerEvent}, {ItemName}, {AllyName}
    /// - EMOTION: {ReactionType}, {Intensity}
    /// - CONTEXT: {Disposition}, {Biome}, {Location}
    ///
    /// Example: "You have the look of someone who can actually fix things. Rare, these days."
    /// Example: "The Blight has touched you deeply. Be careful it doesn't consume you."
    /// Example: "Not worth it! Fall back!"
    /// </summary>
    public string ReactionText { get; set; } = string.Empty;

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
    /// Example: ["Combat", "Social", "Achievement", "Betrayal"]
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
    /// Valid reaction types (emotional states).
    /// </summary>
    public static class ReactionTypes
    {
        // Positive
        public const string Joyful = "Joyful";
        public const string Relieved = "Relieved";
        public const string Impressed = "Impressed";
        public const string Grateful = "Grateful";
        public const string Proud = "Proud";

        // Negative
        public const string Angry = "Angry";
        public const string Fearful = "Fearful";
        public const string Disgusted = "Disgusted";
        public const string Betrayed = "Betrayed";
        public const string Pained = "Pained";

        // Neutral/Complex
        public const string Surprised = "Surprised";
        public const string Suspicious = "Suspicious";
        public const string Confused = "Confused";
        public const string Curious = "Curious";
        public const string Resigned = "Resigned";
    }

    /// <summary>
    /// Valid trigger events.
    /// </summary>
    public static class TriggerEvents
    {
        // Player Actions
        public const string PlayerApproaches = "PlayerApproaches";
        public const string PlayerAttacks = "PlayerAttacks";
        public const string PlayerHelps = "PlayerHelps";
        public const string PlayerGifts = "PlayerGifts";
        public const string PlayerSteals = "PlayerSteals";

        // Combat Events
        public const string AllyKilled = "AllyKilled";
        public const string EnemyKilled = "EnemyKilled";
        public const string TakingDamage = "TakingDamage";
        public const string VictoryAchieved = "VictoryAchieved";

        // Discovery Events
        public const string TreasureFound = "TreasureFound";
        public const string SecretRevealed = "SecretRevealed";
        public const string MechanismRepaired = "MechanismRepaired";
        public const string AncientKnowledgeFound = "AncientKnowledgeFound";

        // Danger Events
        public const string TrapTriggered = "TrapTriggered";
        public const string BlightEncounter = "BlightEncounter";
        public const string StructuralCollapse = "StructuralCollapse";
        public const string AmbushDetected = "AmbushDetected";

        // Social Events
        public const string QuestCompleted = "QuestCompleted";
        public const string BetrayalDetected = "BetrayalDetected";
        public const string GiftReceived = "GiftReceived";
        public const string TheftDetected = "TheftDetected";

        // Mystical Events
        public const string MagicWitnessed = "MagicWitnessed";
        public const string RuneActivated = "RuneActivated";
        public const string ProphecyFulfilled = "ProphecyFulfilled";
    }

    /// <summary>
    /// Valid intensity levels.
    /// </summary>
    public static class IntensityLevels
    {
        public const string Mild = "Mild";
        public const string Moderate = "Moderate";
        public const string Strong = "Strong";
        public const string Extreme = "Extreme";
    }

    /// <summary>
    /// Valid action tendencies.
    /// </summary>
    public static class ActionTendencies
    {
        public const string Approach = "Approach";
        public const string Flee = "Flee";
        public const string Attack = "Attack";
        public const string Assist = "Assist";
        public const string Ignore = "Ignore";
        public const string Report = "Report";
        public const string Investigate = "Investigate";
        public const string Guard = "Guard";
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
}
