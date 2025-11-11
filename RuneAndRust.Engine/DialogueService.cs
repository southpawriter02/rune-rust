using RuneAndRust.Core;
using RuneAndRust.Core.Dialogue;
using System.Text.Json;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// Manages dialogue trees and conversation flow (v0.8)
/// </summary>
public class DialogueService
{
    private static readonly ILogger _log = Log.ForContext<DialogueService>();
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
        _log.Debug("Loading dialogue database from: {DataPath}", _dialogueDataPath);

        if (!Directory.Exists(_dialogueDataPath))
        {
            _log.Warning("Dialogue data path not found: {DataPath}", _dialogueDataPath);
            Console.WriteLine($"Warning: Dialogue data path not found: {_dialogueDataPath}");
            return;
        }

        var dialogueFiles = Directory.GetFiles(_dialogueDataPath, "*.json");
        _log.Debug("Found {FileCount} dialogue files to load", dialogueFiles.Length);

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
                _log.Debug("Loaded dialogue file: {FileName}", Path.GetFileName(file));
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error loading dialogue from file: {FileName}", Path.GetFileName(file));
                Console.WriteLine($"Error loading dialogue from {file}: {ex.Message}");
            }
        }

        _log.Information("Loaded {NodeCount} dialogue nodes from {FileCount} files", _dialogueDatabase.Count, dialogueFiles.Length);
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
            _log.Information("Conversation started with NPC: {NpcId} ({NpcName}), Root Node: {RootDialogueId}",
                npc.Id, npc.Name, npc.RootDialogueId);
        }
        else
        {
            _log.Warning("NPC {NpcId} ({NpcName}) has no root dialogue ID", npc.Id, npc.Name);
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

        _log.Information("Dialogue option selected: {OptionText}, NextNode: {NextNodeId}",
            option.Text, option.NextNodeId ?? "END");

        // Move to next node
        DialogueNode? nextNode = null;
        if (!string.IsNullOrEmpty(option.NextNodeId))
        {
            nextNode = _dialogueDatabase.GetValueOrDefault(option.NextNodeId);
            _currentNode = nextNode;

            if (nextNode != null)
            {
                _log.Debug("Transitioned to dialogue node: {NodeId}", nextNode.Id);
            }
            else
            {
                _log.Warning("Next node not found: {NextNodeId}", option.NextNodeId);
            }
        }
        else
        {
            _currentNode = null; // End conversation
            _log.Information("Dialogue ended (no next node)");
        }

        return (nextNode, outcome);
    }

    /// <summary>
    /// Processes a dialogue outcome
    /// </summary>
    public List<string> ProcessOutcome(DialogueOutcome outcome, PlayerCharacter player, NPC npc)
    {
        var messages = new List<string>();

        _log.Information("Processing dialogue outcome: Type={OutcomeType}, NPC={NpcName}",
            outcome.Type, npc.Name);

        switch (outcome.Type)
        {
            case OutcomeType.Information:
                // Just reveals information, no mechanical effect
                _log.Debug("Outcome: Information revealed (no mechanical effect)");
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

                    _log.Information("Reputation changed via dialogue: Faction={Faction}, Change={Change}",
                        outcome.AffectedFaction.Value, outcome.ReputationChange);
                }
                break;

            case OutcomeType.QuestGiven:
                messages.Add($"[Quest] {outcome.Data} added to quest log");
                _log.Information("Quest given via dialogue: QuestId={QuestId}", outcome.Data);
                break;

            case OutcomeType.QuestComplete:
                messages.Add($"[Quest] {outcome.Data} completed!");
                _log.Information("Quest completed via dialogue: QuestId={QuestId}", outcome.Data);
                break;

            case OutcomeType.ItemReceived:
                messages.Add($"[Item] Received: {outcome.Data}");
                _log.Information("Item received via dialogue: ItemId={ItemId}", outcome.Data);
                break;

            case OutcomeType.ItemLost:
                messages.Add($"[Item] Lost: {outcome.Data}");
                _log.Information("Item lost via dialogue: ItemId={ItemId}", outcome.Data);
                break;

            case OutcomeType.InitiateCombat:
                messages.Add($"{npc.Name} attacks!");
                npc.IsHostile = true;
                _log.Information("Combat initiated via dialogue: NPC={NpcName} ({NpcId})", npc.Name, npc.Id);
                break;

            case OutcomeType.EndConversation:
                _log.Debug("Outcome: End conversation");
                break;
        }

        return messages;
    }

    /// <summary>
    /// Ends the current conversation
    /// </summary>
    public void EndConversation()
    {
        if (_currentNPC != null)
        {
            _log.Information("Conversation ended with NPC: {NpcName} ({NpcId})", _currentNPC.Name, _currentNPC.Id);
        }

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
            bool passed = attributeValue >= check.TargetValue;

            _log.Debug("Skill check: Attribute={Attribute}, Required={Required}, Actual={Actual}, Result={Result}",
                check.Attribute, check.TargetValue, attributeValue, passed ? "Pass" : "Fail");

            if (!passed)
                return false;
        }

        // Skill check (if specified)
        if (check.Skill.HasValue)
        {
            // Check if player has the specialization
            bool hasSpec = player.Specialization == check.Skill.Value;

            _log.Debug("Specialization check: Required={Required}, PlayerHas={PlayerSpec}, Result={Result}",
                check.Skill.Value, player.Specialization, hasSpec ? "Pass" : "Fail");

            if (!hasSpec)
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
