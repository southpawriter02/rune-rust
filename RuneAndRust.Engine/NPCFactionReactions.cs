using RuneAndRust.Core;
using RuneAndRust.Core.Territory;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.35.3: Manages NPC behavior modifications based on territorial control
/// NPCs react differently to players based on which faction controls their sector
/// </summary>
public class NPCFactionReactions
{
    private static readonly ILogger _log = Log.ForContext<NPCFactionReactions>();

    // Faction hostility matrix: defines which factions are hostile to each other
    private static readonly Dictionary<string, string[]> HostileFactions = new()
    {
        ["IronBanes"] = new[] { "GodSleeperCultists", "Undying" },
        ["GodSleeperCultists"] = new[] { "IronBanes", "JotunReaders" },
        ["JotunReaders"] = new[] { "GodSleeperCultists" },
        ["RustClans"] = Array.Empty<string>(), // Neutral traders
        ["Independents"] = Array.Empty<string>()
    };

    /// <summary>
    /// Get modified NPC behavior based on controlling faction
    /// </summary>
    public NPCBehaviorModifier GetFactionModifiedBehavior(NPC npc, string? controllingFaction)
    {
        var modifier = new NPCBehaviorModifier();

        // No faction control = default behavior
        if (string.IsNullOrEmpty(controllingFaction) || controllingFaction == "Independents")
        {
            modifier.HostilityLevel = "Neutral";
            modifier.PriceModifier = 1.0;
            modifier.InformationWillingness = "Medium";
            modifier.GreetingDialogue = npc.InitialGreeting;
            return modifier;
        }

        // Convert NPC faction enum to string for comparison
        string npcFactionName = npc.Faction.ToString();

        // Same faction = friendly
        if (npcFactionName == controllingFaction)
        {
            modifier.HostilityLevel = "Friendly";
            modifier.PriceModifier = 0.85; // 15% discount
            modifier.InformationWillingness = "High";
            modifier.GreetingDialogue = GetFriendlyGreeting(controllingFaction);
            modifier.DispositionModifier = +20;

            _log.Debug("NPC {NpcId} friendly: same faction as controller {Faction}",
                npc.Id, controllingFaction);
        }
        // Hostile faction = suspicious/hostile
        else if (IsHostileFaction(npcFactionName, controllingFaction))
        {
            modifier.HostilityLevel = "Suspicious";
            modifier.PriceModifier = 1.25; // 25% markup
            modifier.InformationWillingness = "Low";
            modifier.GreetingDialogue = GetSuspiciousGreeting(npcFactionName);
            modifier.DispositionModifier = -20;

            _log.Debug("NPC {NpcId} suspicious: hostile faction {NpcFaction} vs controller {ControlFaction}",
                npc.Id, npcFactionName, controllingFaction);
        }
        // Neutral faction
        else
        {
            modifier.HostilityLevel = "Neutral";
            modifier.PriceModifier = 1.0;
            modifier.InformationWillingness = "Medium";
            modifier.GreetingDialogue = GetNeutralGreeting();
            modifier.DispositionModifier = 0;

            _log.Debug("NPC {NpcId} neutral: no hostility between {NpcFaction} and {ControlFaction}",
                npc.Id, npcFactionName, controllingFaction);
        }

        return modifier;
    }

    /// <summary>
    /// Check if two factions are hostile to each other
    /// </summary>
    private bool IsHostileFaction(string factionA, string factionB)
    {
        if (HostileFactions.TryGetValue(factionA, out var hostileList))
        {
            return hostileList.Contains(factionB);
        }

        // Check reverse relationship
        if (HostileFactions.TryGetValue(factionB, out var reverseHostileList))
        {
            return reverseHostileList.Contains(factionA);
        }

        return false;
    }

    /// <summary>
    /// Apply behavior modifier to NPC
    /// Updates NPC's current disposition and returns modified greeting
    /// </summary>
    public string ApplyBehaviorModifier(NPC npc, NPCBehaviorModifier modifier)
    {
        // Update disposition
        npc.CurrentDisposition = Math.Clamp(
            npc.BaseDisposition + modifier.DispositionModifier,
            -100,
            100);

        _log.Debug("Applied behavior modifier to NPC {NpcId}: Disposition {Base} → {Current}",
            npc.Id, npc.BaseDisposition, npc.CurrentDisposition);

        // Return modified greeting
        return modifier.GreetingDialogue ?? npc.InitialGreeting;
    }

    /// <summary>
    /// Get modified merchant prices based on faction control
    /// </summary>
    public int GetModifiedPrice(int basePrice, NPCBehaviorModifier modifier)
    {
        int modifiedPrice = (int)(basePrice * modifier.PriceModifier);

        _log.Debug("Price modified: {BasePrice} → {ModifiedPrice} (modifier: {Modifier})",
            basePrice, modifiedPrice, modifier.PriceModifier);

        return modifiedPrice;
    }

    // Greeting dialogue generators

    private string GetFriendlyGreeting(string faction)
    {
        return faction switch
        {
            "IronBanes" => "Welcome, ally. The Purity Oath recognizes friends of the cause.",
            "JotunReaders" => "Greetings, scholar. Your pursuit of knowledge is welcome here.",
            "GodSleeperCultists" => "The awakened ones welcome you, faithful servant.",
            "RustClans" => "Good to see a fellow trader. Business is prosperous.",
            _ => "Welcome, friend. Good to see allies here."
        };
    }

    private string GetSuspiciousGreeting(string npcFaction)
    {
        return npcFaction switch
        {
            "IronBanes" => "You're not from around here. State your business quickly.",
            "JotunReaders" => "An outsider. What knowledge do you seek in our territory?",
            "GodSleeperCultists" => "The awakened ones see all. Why have you come here?",
            "RustClans" => "Hmph. Trade is trade, but I don't trust you.",
            _ => "You're not from around here. What do you want?"
        };
    }

    private string GetNeutralGreeting()
    {
        return "Greetings, traveler. What brings you to this sector?";
    }

    /// <summary>
    /// Get dialogue availability based on information willingness
    /// </summary>
    public bool IsDialogueAvailable(NPCBehaviorModifier modifier, string dialogueType)
    {
        return dialogueType switch
        {
            "Rumors" => modifier.InformationWillingness != "Low",
            "Quests" => modifier.InformationWillingness == "High",
            "FactionInfo" => modifier.InformationWillingness == "High",
            "Trade" => true, // Always available
            _ => modifier.InformationWillingness == "High"
        };
    }
}
