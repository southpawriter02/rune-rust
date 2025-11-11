using RuneAndRust.Core;
using RuneAndRust.Core.Dialogue;
using System.Text.Json;

namespace RuneAndRust.Engine;

/// <summary>
/// Manages dialogue trees and conversation flow (v0.8)
/// </summary>
public class DialogueService
{
    private readonly Dictionary<string, DialogueNode> _dialogueDatabase = new();
    private readonly string _dialogueDataPath;
    private DialogueNode? _currentNode;
    private NPC? _currentNPC;

    public DialogueService(string dataPath = "Data/Dialogues")
    {
        _dialogueDataPath = dataPath;
    }

    /// <summary>
    /// Loads all dialogue trees from JSON files
    /// </summary>
    public void LoadDialogueDatabase()
    {
        if (!Directory.Exists(_dialogueDataPath))
        {
            Console.WriteLine($"Warning: Dialogue data path not found: {_dialogueDataPath}");
            return;
        }

        var dialogueFiles = Directory.GetFiles(_dialogueDataPath, "*.json");
        foreach (var file in dialogueFiles)
        {
            try
            {
                var json = File.ReadAllText(file);
                var nodes = JsonSerializer.Deserialize<List<DialogueNode>>(json);
                if (nodes != null)
                {
                    foreach (var node in nodes)
                    {
                        if (!string.IsNullOrEmpty(node.Id))
                        {
                            _dialogueDatabase[node.Id] = node;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading dialogue from {file}: {ex.Message}");
            }
        }

        Console.WriteLine($"Loaded {_dialogueDatabase.Count} dialogue nodes");
    }

    /// <summary>
    /// Starts a conversation with an NPC
    /// </summary>
    public DialogueNode? StartConversation(NPC npc, PlayerCharacter player)
    {
        _currentNPC = npc;

        // Get the root dialogue node for this NPC
        if (!string.IsNullOrEmpty(npc.RootDialogueId))
        {
            _currentNode = _dialogueDatabase.GetValueOrDefault(npc.RootDialogueId);
        }

        return _currentNode;
    }

    /// <summary>
    /// Gets available options for the current dialogue node, filtering by skill checks
    /// </summary>
    public List<DialogueOption> GetAvailableOptions(DialogueNode node, PlayerCharacter player)
    {
        var availableOptions = new List<DialogueOption>();

        foreach (var option in node.Options)
        {
            // Check if option requires a skill check
            if (option.SkillCheck != null)
            {
                // v0.8 uses "hard checks" - hide option if player doesn't meet requirement
                if (!MeetsRequirement(player, option.SkillCheck))
                {
                    continue; // Skip this option
                }
            }

            availableOptions.Add(option);
        }

        return availableOptions;
    }

    /// <summary>
    /// Selects a dialogue option and moves to the next node
    /// </summary>
    public (DialogueNode? nextNode, DialogueOutcome? outcome) SelectOption(
        DialogueOption option,
        PlayerCharacter player)
    {
        DialogueOutcome? outcome = option.Outcome;

        // Move to next node
        DialogueNode? nextNode = null;
        if (!string.IsNullOrEmpty(option.NextNodeId))
        {
            nextNode = _dialogueDatabase.GetValueOrDefault(option.NextNodeId);
            _currentNode = nextNode;
        }
        else
        {
            _currentNode = null; // End conversation
        }

        return (nextNode, outcome);
    }

    /// <summary>
    /// Processes a dialogue outcome
    /// </summary>
    public List<string> ProcessOutcome(DialogueOutcome outcome, PlayerCharacter player, NPC npc)
    {
        var messages = new List<string>();

        switch (outcome.Type)
        {
            case OutcomeType.Information:
                // Just reveals information, no mechanical effect
                break;

            case OutcomeType.ReputationChange:
                if (outcome.AffectedFaction.HasValue)
                {
                    var log = new List<string>();
                    player.FactionReputations.ModifyReputation(
                        outcome.AffectedFaction.Value,
                        outcome.ReputationChange,
                        $"Dialogue with {npc.Name}",
                        log);
                    messages.AddRange(log);

                    // Update NPC disposition
                    npc.UpdateDisposition(player.FactionReputations.GetReputation(npc.Faction));
                }
                break;

            case OutcomeType.QuestGiven:
                messages.Add($"[Quest] {outcome.Data} added to quest log");
                break;

            case OutcomeType.QuestComplete:
                messages.Add($"[Quest] {outcome.Data} completed!");
                break;

            case OutcomeType.ItemReceived:
                messages.Add($"[Item] Received: {outcome.Data}");
                break;

            case OutcomeType.ItemLost:
                messages.Add($"[Item] Lost: {outcome.Data}");
                break;

            case OutcomeType.InitiateCombat:
                messages.Add($"{npc.Name} attacks!");
                npc.IsHostile = true;
                break;

            case OutcomeType.EndConversation:
                // Just ends the conversation
                break;
        }

        return messages;
    }

    /// <summary>
    /// Ends the current conversation
    /// </summary>
    public void EndConversation()
    {
        _currentNode = null;
        _currentNPC = null;
    }

    /// <summary>
    /// Checks if player meets a skill check requirement
    /// </summary>
    public bool MeetsRequirement(PlayerCharacter player, SkillCheckRequirement check)
    {
        // Attribute check
        if (!string.IsNullOrEmpty(check.Attribute))
        {
            int attributeValue = player.GetAttributeValue(check.Attribute);
            if (attributeValue < check.TargetValue)
                return false;
        }

        // Skill check (if specified)
        if (check.Skill.HasValue)
        {
            // Check if player has the specialization
            if (player.Specialization != check.Skill.Value)
            {
                return false;
            }

            // v0.8: Simplified skill check - just need to have the specialization
            // v1.0+ can add skill ranks
        }

        return true;
    }

    /// <summary>
    /// Formats a skill check requirement for display
    /// </summary>
    public string FormatSkillCheckTag(SkillCheckRequirement check)
    {
        if (!string.IsNullOrEmpty(check.Attribute))
        {
            return $"[{check.Attribute.ToUpper()} {check.TargetValue}]";
        }
        else if (check.Skill.HasValue)
        {
            return $"[{check.Skill.Value} {check.SkillRanks}]";
        }

        return string.Empty;
    }

    /// <summary>
    /// Gets a dialogue node by ID
    /// </summary>
    public DialogueNode? GetDialogueNode(string nodeId)
    {
        return _dialogueDatabase.GetValueOrDefault(nodeId);
    }

    /// <summary>
    /// Checks if currently in a conversation
    /// </summary>
    public bool IsInConversation()
    {
        return _currentNode != null && _currentNPC != null;
    }

    public DialogueNode? CurrentNode => _currentNode;
    public NPC? CurrentNPC => _currentNPC;
}
