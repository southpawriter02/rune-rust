using RuneAndRust.Core;
using RuneAndRust.Core.NPCFlavor;
using RuneAndRust.Persistence;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.38.11: NPC Descriptors & Dialogue Barks Service
/// Generates rich flavor text for NPC physical descriptions, ambient dialogue, and reactions
/// Philosophy: Every NPC contributes to world-building through appearance and voice
/// </summary>
public class NPCFlavorTextService
{
    private readonly DescriptorRepository _repository;
    private static readonly ILogger _log = Log.ForContext<NPCFlavorTextService>();

    public NPCFlavorTextService(DescriptorRepository repository)
    {
        _repository = repository;
    }

    #region Physical Description Generation

    /// <summary>
    /// Generates a physical description for an NPC
    /// </summary>
    public string GenerateNPCPhysicalDescription(
        string npcArchetype,
        string npcSubtype,
        string descriptorType = "FullBody",
        string? condition = null,
        string? biomeContext = null,
        string? ageCategory = null)
    {
        var descriptor = _repository.GetRandomNPCPhysicalDescriptor(
            npcArchetype,
            npcSubtype,
            descriptorType,
            condition,
            biomeContext,
            ageCategory);

        if (descriptor == null)
        {
            _log.Warning("No physical descriptor found for {Archetype} {Subtype} {Type}",
                npcArchetype, npcSubtype, descriptorType);
            return GetFallbackPhysicalDescription(npcArchetype, npcSubtype);
        }

        return descriptor.DescriptorText;
    }

    /// <summary>
    /// Generates a complete NPC appearance (multiple descriptor types combined)
    /// </summary>
    public NPCAppearanceResult GenerateCompleteAppearance(
        string npcArchetype,
        string npcSubtype,
        string? condition = null,
        string? biomeContext = null,
        string? ageCategory = null)
    {
        var fullBody = GenerateNPCPhysicalDescription(
            npcArchetype, npcSubtype, "FullBody", condition, biomeContext, ageCategory);

        var bearing = GenerateNPCPhysicalDescription(
            npcArchetype, npcSubtype, "Bearing", condition, biomeContext, ageCategory);

        var distinguishing = GenerateNPCPhysicalDescription(
            npcArchetype, npcSubtype, "Distinguishing", condition, biomeContext, ageCategory);

        return new NPCAppearanceResult
        {
            FullBodyDescription = fullBody,
            BearingDescription = bearing,
            DistinguishingFeatures = distinguishing,
            CombinedDescription = $"{fullBody} {bearing}".Trim()
        };
    }

    private string GetFallbackPhysicalDescription(string archetype, string subtype)
    {
        return archetype switch
        {
            "Dvergr" => "A stocky Dvergr with weathered features and practical clothing.",
            "Seidkona" => "A mystic figure draped in ritual garments, eyes distant and knowing.",
            "Bandit" or "Raider" => "A hardened survivor with scars and mismatched gear.",
            "Merchant" => "A well-dressed trader with calculating eyes.",
            "Guard" => "A watchful guard in worn armor, hand near their weapon.",
            "Citizen" => "An ordinary citizen showing the wear of hard times.",
            "Forlorn" => "A Blight-touched figure, barely recognizable as once-human.",
            _ => "A figure of indeterminate appearance."
        };
    }

    #endregion

    #region Ambient Bark Generation

    /// <summary>
    /// Generates an ambient dialogue bark for an NPC
    /// </summary>
    public string GenerateAmbientBark(
        string npcArchetype,
        string npcSubtype,
        string barkType,
        string? activityContext = null,
        string? dispositionContext = null,
        string? biomeContext = null,
        string? triggerCondition = null)
    {
        var descriptor = _repository.GetRandomNPCAmbientBarkDescriptor(
            npcArchetype,
            npcSubtype,
            barkType,
            activityContext,
            dispositionContext,
            biomeContext,
            triggerCondition);

        if (descriptor == null)
        {
            _log.Warning("No ambient bark found for {Archetype} {Subtype} {BarkType}",
                npcArchetype, npcSubtype, barkType);
            return GetFallbackAmbientBark(npcArchetype, barkType);
        }

        return descriptor.DialogueText;
    }

    /// <summary>
    /// Generates a context-appropriate ambient bark based on NPC state
    /// </summary>
    public string GenerateContextualBark(
        string npcArchetype,
        string npcSubtype,
        string activityContext,
        string dispositionContext,
        string? biomeContext = null)
    {
        // Determine appropriate bark type based on context
        var barkType = DetermineBarkType(activityContext, dispositionContext);

        return GenerateAmbientBark(
            npcArchetype,
            npcSubtype,
            barkType,
            activityContext,
            dispositionContext,
            biomeContext);
    }

    private string DetermineBarkType(string activity, string disposition)
    {
        // Activity-specific barks
        if (activity == "Working" || activity == "Crafting")
            return "AtWork";
        if (activity == "Fighting")
            return "BattleCry";
        if (activity == "Trading")
            return "Greeting";

        // Disposition-based barks
        if (disposition == "Hostile")
            return "Threat";
        if (disposition == "Unfriendly")
            return "Suspicion";
        if (disposition == "Friendly" || disposition == "Allied")
            return "Encouragement";

        // Default to idle conversation
        return "IdleConversation";
    }

    private string GetFallbackAmbientBark(string archetype, string barkType)
    {
        return (archetype, barkType) switch
        {
            ("Dvergr", "AtWork") => "The mechanism needs adjustment. Precision matters.",
            ("Dvergr", "IdleConversation") => "We built this to last. And it has.",
            ("Seidkona", "Teaching") => "The runes speak if you learn to listen.",
            ("Seidkona", "Warning") => "The veil is thin here. Be cautious.",
            ("Bandit", "Threat") => "Hand over your supplies. Don't make this difficult.",
            ("Raider", "Threat") => "Wrong place, wrong time. That's your problem.",
            ("Merchant", "Greeting") => "Looking to trade? I have what you need.",
            ("Guard", "Observation") => "Keep moving. Nothing to see here.",
            _ => "*mutters to themselves*"
        };
    }

    #endregion

    #region Reaction Generation

    /// <summary>
    /// Generates an emotional reaction for an NPC to an event
    /// </summary>
    public NPCReactionResult GenerateReaction(
        string npcArchetype,
        string npcSubtype,
        string reactionType,
        string triggerEvent,
        string? intensity = null,
        string? priorDisposition = null,
        string? biomeContext = null)
    {
        var descriptor = _repository.GetRandomNPCReactionDescriptor(
            npcArchetype,
            npcSubtype,
            reactionType,
            triggerEvent,
            intensity,
            priorDisposition,
            actionTendency: null,
            biomeContext);

        if (descriptor == null)
        {
            _log.Warning("No reaction descriptor found for {Archetype} {Subtype} {Reaction} {Trigger}",
                npcArchetype, npcSubtype, reactionType, triggerEvent);

            return new NPCReactionResult
            {
                ReactionText = GetFallbackReaction(npcArchetype, reactionType, triggerEvent),
                ReactionType = reactionType,
                ActionTendency = DetermineActionTendency(reactionType, triggerEvent)
            };
        }

        return new NPCReactionResult
        {
            ReactionText = descriptor.ReactionText,
            ReactionType = descriptor.ReactionType,
            ActionTendency = descriptor.ActionTendency ?? DetermineActionTendency(reactionType, triggerEvent),
            Intensity = descriptor.Intensity ?? intensity ?? "Moderate"
        };
    }

    /// <summary>
    /// Generates a reaction to player approach based on disposition
    /// </summary>
    public string GeneratePlayerApproachReaction(
        string npcArchetype,
        string npcSubtype,
        string disposition)
    {
        var reactionType = disposition switch
        {
            "Hostile" => "Angry",
            "Unfriendly" => "Suspicious",
            "Neutral" => "Curious",
            "Friendly" => "Joyful",
            "Allied" => "Relieved",
            _ => "Surprised"
        };

        var result = GenerateReaction(
            npcArchetype,
            npcSubtype,
            reactionType,
            "PlayerApproaches",
            priorDisposition: disposition);

        return result.ReactionText;
    }

    /// <summary>
    /// Generates a combat-related reaction
    /// </summary>
    public NPCReactionResult GenerateCombatReaction(
        string npcArchetype,
        string npcSubtype,
        string combatEvent,  // "TakingDamage", "AllyKilled", "VictoryAchieved"
        string intensity = "Moderate")
    {
        var reactionType = combatEvent switch
        {
            "TakingDamage" => "Pained",
            "AllyKilled" => "Angry",
            "VictoryAchieved" => "Joyful",
            "PlayerAttacks" => "Angry",
            _ => "Surprised"
        };

        return GenerateReaction(
            npcArchetype,
            npcSubtype,
            reactionType,
            combatEvent,
            intensity);
    }

    private string DetermineActionTendency(string reactionType, string triggerEvent)
    {
        return (reactionType, triggerEvent) switch
        {
            ("Fearful", _) => "Flee",
            ("Angry", "PlayerAttacks") => "Attack",
            ("Angry", _) => "Approach",
            ("Grateful", _) => "Assist",
            ("Joyful", _) => "Approach",
            ("Suspicious", _) => "Guard",
            ("Curious", _) => "Investigate",
            _ => "Ignore"
        };
    }

    private string GetFallbackReaction(string archetype, string reaction, string trigger)
    {
        return (archetype, reaction, trigger) switch
        {
            ("Dvergr", "Impressed", "MechanismRepaired") => "Good work. You understand the engineering.",
            ("Dvergr", "Angry", "PlayerAttacks") => "Sloppy and violent. Typical.",
            ("Seidkona", "Suspicious", "PlayerApproaches") => "The Blight clings to you. What do you want?",
            ("Seidkona", "Impressed", "MagicWitnessed") => "You have the spark. Interesting.",
            ("Bandit", "Angry", "PlayerAttacks") => "You'll pay for that!",
            ("Raider", "Fearful", "TakingDamage") => "Not worth it! I'm out!",
            ("Merchant", "Joyful", "GiftReceived") => "Much appreciated! Let's do business.",
            ("Guard", "Angry", "TheftDetected") => "Thief! Stop right there!",
            _ => "*reacts to the situation*"
        };
    }

    #endregion

    #region Variable Processing

    /// <summary>
    /// Processes variable placeholders in descriptor text
    /// </summary>
    public string ProcessVariables(
        string template,
        string? npcName = null,
        string? playerName = null,
        string? biome = null,
        Dictionary<string, string>? additionalVariables = null)
    {
        var result = template;

        // Standard replacements
        if (!string.IsNullOrEmpty(npcName))
            result = result.Replace("{NPCName}", npcName);

        if (!string.IsNullOrEmpty(playerName))
            result = result.Replace("{PlayerName}", playerName);

        if (!string.IsNullOrEmpty(biome))
            result = result.Replace("{Biome}", biome);

        // Additional custom variables
        if (additionalVariables != null)
        {
            foreach (var (key, value) in additionalVariables)
            {
                result = result.Replace($"{{{key}}}", value);
            }
        }

        return result;
    }

    #endregion
}

/// <summary>
/// Result of NPC appearance generation
/// </summary>
public class NPCAppearanceResult
{
    public string FullBodyDescription { get; set; } = string.Empty;
    public string BearingDescription { get; set; } = string.Empty;
    public string DistinguishingFeatures { get; set; } = string.Empty;
    public string CombinedDescription { get; set; } = string.Empty;
}

/// <summary>
/// Result of NPC reaction generation
/// </summary>
public class NPCReactionResult
{
    public string ReactionText { get; set; } = string.Empty;
    public string ReactionType { get; set; } = string.Empty;
    public string? ActionTendency { get; set; }
    public string Intensity { get; set; } = "Moderate";
}
