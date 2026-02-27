using Microsoft.Extensions.Logging;
using RuneAndRust.Application.DTOs;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Orchestrates dialogue interactions between player and NPCs.
/// Bridges IDialogueProvider data to the presentation layer.
/// Scoped service — one instance per game session.
/// </summary>
public class DialogueService : IDialogueService
{
    // ═══════════════════════════════════════════════════════════════
    // FIELDS
    // ═══════════════════════════════════════════════════════════════

    private readonly IDialogueProvider _dialogueProvider;
    private readonly IGameEventLogger? _eventLogger;
    private readonly ILogger<DialogueService> _logger;

    /// <summary>Current active dialogue state. Null if no dialogue is active.</summary>
    private ActiveDialogueState? _currentDialogue;

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════

    public DialogueService(
        IDialogueProvider dialogueProvider,
        ILogger<DialogueService> logger,
        IGameEventLogger? eventLogger = null)
    {
        _dialogueProvider = dialogueProvider ?? throw new ArgumentNullException(nameof(dialogueProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _eventLogger = eventLogger;

        _logger.LogDebug("DialogueService initialized");
    }

    // ═══════════════════════════════════════════════════════════════
    // INTERFACE IMPLEMENTATION
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public bool IsDialogueActive => _currentDialogue != null;

    /// <inheritdoc />
    public DialogueNodeDto? StartDialogue(Npc npc)
    {
        ArgumentNullException.ThrowIfNull(npc);

        if (!npc.IsAlive)
        {
            _logger.LogWarning("Cannot start dialogue with dead NPC: {NpcName}", npc.Name);
            return null;
        }

        // Mark as met on first interaction
        if (!npc.HasBeenMet)
            npc.Meet();

        // If NPC has no dialogue tree, return a greeting-only node
        if (!npc.HasDialogue || npc.RootDialogueId == null)
        {
            _logger.LogDebug("NPC {NpcName} has no dialogue tree, using greeting-only", npc.Name);

            var greetingNode = new DialogueNodeDto
            {
                Id = "greeting",
                Text = npc.CurrentGreeting,
                Options = [new DialogueOptionDto { Text = "Farewell.", Outcome = new DialogueOutcomeDto { Type = "EndConversation" } }],
                EndsConversation = false
            };

            _currentDialogue = new ActiveDialogueState
            {
                NpcDefinitionId = npc.DefinitionId,
                NpcName = npc.Name,
                RootDialogueId = "greeting-only",
                CurrentNodeId = "greeting",
                CurrentNode = greetingNode
            };

            return greetingNode;
        }

        // Load dialogue tree
        var tree = _dialogueProvider.GetDialogueTree(npc.RootDialogueId);
        if (tree == null || tree.Count == 0)
        {
            _logger.LogWarning(
                "Dialogue tree '{RootId}' not found for NPC '{NpcName}', using greeting-only",
                npc.RootDialogueId, npc.Name);

            var fallbackNode = new DialogueNodeDto
            {
                Id = "greeting",
                Text = npc.CurrentGreeting,
                Options = [new DialogueOptionDto { Text = "Farewell.", Outcome = new DialogueOutcomeDto { Type = "EndConversation" } }],
                EndsConversation = false
            };

            _currentDialogue = new ActiveDialogueState
            {
                NpcDefinitionId = npc.DefinitionId,
                NpcName = npc.Name,
                RootDialogueId = npc.RootDialogueId,
                CurrentNodeId = "greeting",
                CurrentNode = fallbackNode
            };

            return fallbackNode;
        }

        // Get the first node (the root)
        var rootNode = tree[0];

        _currentDialogue = new ActiveDialogueState
        {
            NpcDefinitionId = npc.DefinitionId,
            NpcName = npc.Name,
            RootDialogueId = npc.RootDialogueId,
            CurrentNodeId = rootNode.Id,
            CurrentNode = rootNode
        };

        _eventLogger?.LogInteraction("DialogueStarted",
            $"Started dialogue with {npc.Name}",
            data: new Dictionary<string, object>
            {
                ["NpcId"] = npc.DefinitionId,
                ["RootDialogueId"] = npc.RootDialogueId
            });

        _logger.LogInformation(
            "Dialogue started with {NpcName} ({NpcId}), root: {RootId}",
            npc.Name, npc.DefinitionId, npc.RootDialogueId);

        return rootNode;
    }

    /// <inheritdoc />
    public DialogueNodeDto? ProcessChoice(string rootDialogueId, string choiceNodeId, DialogueOutcomeDto? outcome)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rootDialogueId);

        if (string.IsNullOrWhiteSpace(choiceNodeId))
        {
            // Null/empty node ID means conversation ends
            EndDialogue();
            return null;
        }

        var nextNode = _dialogueProvider.GetNode(rootDialogueId, choiceNodeId);
        if (nextNode == null)
        {
            _logger.LogWarning(
                "Dialogue node '{NodeId}' not found in tree '{RootId}', ending conversation",
                choiceNodeId, rootDialogueId);
            EndDialogue();
            return null;
        }

        // Update current state
        if (_currentDialogue != null)
        {
            _currentDialogue = _currentDialogue with
            {
                CurrentNodeId = nextNode.Id,
                CurrentNode = nextNode
            };
        }

        _logger.LogDebug(
            "Dialogue advanced to node '{NodeId}' in tree '{RootId}'",
            nextNode.Id, rootDialogueId);

        // Check if this node ends the conversation
        if (nextNode.EndsConversation)
        {
            EndDialogue();
        }

        return nextNode;
    }

    /// <inheritdoc />
    public void ProcessOutcome(DialogueOutcomeDto outcome, Npc npc)
    {
        ArgumentNullException.ThrowIfNull(outcome);
        ArgumentNullException.ThrowIfNull(npc);

        switch (outcome.Type.ToLowerInvariant())
        {
            case "endconversation":
                EndDialogue();
                break;

            case "reputationchange":
                npc.AdjustDisposition(outcome.ReputationChange);
                _logger.LogDebug(
                    "NPC {NpcName} disposition adjusted by {Amount} to {Current}",
                    npc.Name, outcome.ReputationChange, npc.CurrentDisposition);
                break;

            case "questgiven":
                // Quest acceptance is handled by QuestService via GameSessionService
                _logger.LogDebug(
                    "Dialogue outcome: quest '{QuestId}' offered to player",
                    outcome.Data);
                break;

            case "openshop":
                _logger.LogDebug("Dialogue outcome: open shop for NPC {NpcName}", npc.Name);
                break;

            case "giveitem":
                _logger.LogDebug("Dialogue outcome: give item '{ItemId}' to player", outcome.Data);
                break;

            case "takeitem":
                _logger.LogDebug("Dialogue outcome: take item '{ItemId}' from player", outcome.Data);
                break;

            case "information":
                _logger.LogDebug("Dialogue outcome: information revealed");
                break;

            case "initiatecombat":
                _logger.LogDebug("Dialogue outcome: combat initiated");
                break;

            default:
                _logger.LogWarning("Unknown dialogue outcome type: {Type}", outcome.Type);
                break;
        }

        _eventLogger?.LogInteraction("DialogueOutcome",
            $"Dialogue outcome: {outcome.Type}",
            data: new Dictionary<string, object>
            {
                ["OutcomeType"] = outcome.Type,
                ["NpcId"] = npc.DefinitionId,
                ["Data"] = outcome.Data ?? ""
            });
    }

    /// <inheritdoc />
    public ActiveDialogueState? GetCurrentDialogue()
    {
        return _currentDialogue;
    }

    /// <inheritdoc />
    public void EndDialogue()
    {
        if (_currentDialogue != null)
        {
            _logger.LogDebug(
                "Dialogue ended with {NpcName}",
                _currentDialogue.NpcName);

            _eventLogger?.LogInteraction("DialogueEnded",
                $"Ended dialogue with {_currentDialogue.NpcName}");

            _currentDialogue = null;
        }
    }
}
