using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Events;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;
using Character = RuneAndRust.Core.Entities.Character;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// Service for managing dialogue sessions and conversation flow.
/// </summary>
/// <remarks>See: v0.4.2c (The Voice) for DialogueService implementation.</remarks>
public class DialogueService : IDialogueService
{
    private readonly IDialogueRepository _dialogueRepository;
    private readonly IDialogueConditionEvaluator _conditionEvaluator;
    private readonly IDialogueEffectExecutor _effectExecutor;
    private readonly IEventBus _eventBus;
    private readonly ILogger<DialogueService> _logger;

    /// <summary>
    /// Initializes a new instance of the DialogueService.
    /// </summary>
    public DialogueService(
        IDialogueRepository dialogueRepository,
        IDialogueConditionEvaluator conditionEvaluator,
        IDialogueEffectExecutor effectExecutor,
        IEventBus eventBus,
        ILogger<DialogueService> logger)
    {
        _dialogueRepository = dialogueRepository ?? throw new ArgumentNullException(nameof(dialogueRepository));
        _conditionEvaluator = conditionEvaluator ?? throw new ArgumentNullException(nameof(conditionEvaluator));
        _effectExecutor = effectExecutor ?? throw new ArgumentNullException(nameof(effectExecutor));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<DialogueStartResult> StartDialogueAsync(
        Character character,
        string treeId,
        GameState gameState)
    {
        _logger.LogInformation(
            "Starting dialogue for character {CharacterId} with tree {TreeId}",
            character.Id,
            treeId);

        // Check if already in dialogue
        if (gameState.CurrentDialogueSession != null)
        {
            _logger.LogWarning(
                "Character {CharacterId} is already in dialogue session {SessionId}",
                character.Id,
                gameState.CurrentDialogueSession.SessionId);

            return DialogueStartResult.Failed("Already in an active dialogue session");
        }

        // Get the dialogue tree
        var tree = await _dialogueRepository.GetTreeByIdAsync(treeId);
        if (tree == null)
        {
            _logger.LogWarning("Dialogue tree {TreeId} not found", treeId);
            return DialogueStartResult.Failed($"Dialogue tree '{treeId}' not found");
        }

        // Get the root node
        var rootNode = tree.Nodes.FirstOrDefault(n => n.NodeId == tree.RootNodeId);
        if (rootNode == null)
        {
            _logger.LogError(
                "Root node {RootNodeId} not found in tree {TreeId}",
                tree.RootNodeId,
                treeId);
            return DialogueStartResult.Failed($"Root node '{tree.RootNodeId}' not found");
        }

        // Create the session
        var session = new DialogueSession
        {
            Tree = tree,
            CurrentNode = rootNode,
            CharacterId = character.Id
        };
        session.VisitedNodeIds.Add(rootNode.NodeId);

        // Store in game state
        gameState.CurrentDialogueSession = session;

        // Build the view model
        var viewModel = await BuildViewModelAsync(character, session);

        // Publish event
        await _eventBus.PublishAsync(new DialogueStartedEvent
        {
            SessionId = session.SessionId,
            TreeId = treeId,
            NpcName = tree.NpcName,
            CharacterId = character.Id
        });

        _logger.LogInformation(
            "Dialogue session {SessionId} started with tree {TreeId}",
            session.SessionId,
            treeId);

        return DialogueStartResult.Successful(viewModel, session);
    }

    /// <inheritdoc/>
    public async Task<DialogueStepResult> SelectOptionAsync(
        Character character,
        string optionId,
        GameState gameState)
    {
        _logger.LogDebug(
            "Character {CharacterId} selecting option {OptionId}",
            character.Id,
            optionId);

        // Validate session
        var session = gameState.CurrentDialogueSession;
        if (session == null)
        {
            _logger.LogWarning("No active dialogue session for character {CharacterId}", character.Id);
            return DialogueStepResult.Failed("No active dialogue session");
        }

        if (session.CharacterId != character.Id)
        {
            _logger.LogWarning(
                "Character {CharacterId} attempting to interact with session belonging to {SessionCharacterId}",
                character.Id,
                session.CharacterId);
            return DialogueStepResult.Failed("Character is not in this dialogue session");
        }

        // Find the option
        var option = session.CurrentNode.Options
            .FirstOrDefault(o => o.Id.ToString() == optionId || o.Text == optionId);

        if (option == null)
        {
            _logger.LogWarning("Option {OptionId} not found in current node", optionId);
            return DialogueStepResult.Failed($"Option '{optionId}' not found");
        }

        // Check if option is available
        var visibilityResult = await _conditionEvaluator.EvaluateOptionAsync(character, option);
        if (!visibilityResult.IsAvailable)
        {
            _logger.LogWarning(
                "Option {OptionId} is not available for character {CharacterId}: {Reason}",
                optionId,
                character.Id,
                visibilityResult.LockReason);
            return DialogueStepResult.Failed(visibilityResult.LockReason ?? "Option is not available");
        }

        // Execute effects
        var effectResults = new List<DialogueEffectResult>();
        if (option.HasEffects)
        {
            effectResults = (await _effectExecutor.ExecuteEffectsAsync(
                character,
                option.Effects,
                gameState)).ToList();

            _logger.LogDebug(
                "Executed {EffectCount} effects for option {OptionId}",
                effectResults.Count,
                optionId);
        }

        session.OptionsSelectedCount++;

        // Check if this is a terminal option
        if (option.IsTerminal)
        {
            // End the dialogue
            var endResult = await EndDialogueInternalAsync(
                DialogueEndReason.PlayerExit,
                session,
                gameState);

            // Publish option selected event
            await _eventBus.PublishAsync(new DialogueOptionSelectedEvent
            {
                SessionId = session.SessionId,
                TreeId = session.Tree.TreeId,
                FromNodeId = session.CurrentNode.NodeId,
                OptionId = optionId,
                ToNodeId = null,
                CharacterId = character.Id,
                EffectsExecuted = effectResults.Count
            });

            return DialogueStepResult.End(DialogueEndReason.PlayerExit, effectResults);
        }

        // Navigate to next node
        var nextNode = session.Tree.Nodes.FirstOrDefault(n => n.NodeId == option.NextNodeId);
        if (nextNode == null)
        {
            _logger.LogError(
                "Next node {NextNodeId} not found in tree {TreeId}",
                option.NextNodeId,
                session.Tree.TreeId);

            // End dialogue due to error
            await EndDialogueInternalAsync(DialogueEndReason.Error, session, gameState);
            return DialogueStepResult.Failed($"Next node '{option.NextNodeId}' not found");
        }

        var previousNodeId = session.CurrentNode.NodeId;

        // Update session
        session.CurrentNode = nextNode;
        session.VisitedNodeIds.Add(nextNode.NodeId);

        // Publish option selected event
        await _eventBus.PublishAsync(new DialogueOptionSelectedEvent
        {
            SessionId = session.SessionId,
            TreeId = session.Tree.TreeId,
            FromNodeId = previousNodeId,
            OptionId = optionId,
            ToNodeId = nextNode.NodeId,
            CharacterId = character.Id,
            EffectsExecuted = effectResults.Count
        });

        // Check if the new node is terminal with no options
        if (nextNode.IsTerminal && nextNode.Options.Count == 0)
        {
            var endResult = await EndDialogueInternalAsync(
                DialogueEndReason.NpcExit,
                session,
                gameState);

            return DialogueStepResult.End(DialogueEndReason.NpcExit, effectResults);
        }

        // Build new view model
        var viewModel = await BuildViewModelAsync(character, session);

        _logger.LogDebug(
            "Advanced to node {NodeId} in dialogue session {SessionId}",
            nextNode.NodeId,
            session.SessionId);

        return DialogueStepResult.Continue(viewModel, effectResults);
    }

    /// <inheritdoc/>
    public async Task<DialogueEndResult> EndDialogueAsync(
        DialogueEndReason reason,
        GameState gameState)
    {
        var session = gameState.CurrentDialogueSession;
        if (session == null)
        {
            _logger.LogWarning("No active dialogue session to end");
            return DialogueEndResult.Failed("No active dialogue session");
        }

        return await EndDialogueInternalAsync(reason, session, gameState);
    }

    /// <inheritdoc/>
    public async Task<DialogueViewModel?> GetCurrentDialogueAsync(
        Character character,
        GameState gameState)
    {
        var session = gameState.CurrentDialogueSession;
        if (session == null || session.CharacterId != character.Id)
        {
            return null;
        }

        return await BuildViewModelAsync(character, session);
    }

    /// <inheritdoc/>
    public bool IsInDialogue(Guid characterId, GameState gameState)
    {
        return gameState.CurrentDialogueSession?.CharacterId == characterId;
    }

    /// <summary>
    /// Internal method to end dialogue and clean up state.
    /// </summary>
    private async Task<DialogueEndResult> EndDialogueInternalAsync(
        DialogueEndReason reason,
        DialogueSession session,
        GameState gameState)
    {
        var duration = DateTime.UtcNow - session.StartedAt;

        _logger.LogInformation(
            "Ending dialogue session {SessionId} with reason {Reason}. " +
            "Duration: {Duration}, Nodes: {NodesVisited}, Options: {OptionsSelected}",
            session.SessionId,
            reason,
            duration,
            session.VisitedNodeIds.Count,
            session.OptionsSelectedCount);

        // Publish event
        await _eventBus.PublishAsync(new DialogueEndedEvent
        {
            SessionId = session.SessionId,
            TreeId = session.Tree.TreeId,
            CharacterId = session.CharacterId,
            Reason = reason,
            Duration = duration,
            NodesVisited = session.VisitedNodeIds.Count,
            OptionsSelected = session.OptionsSelectedCount
        });

        // Clear session from game state
        gameState.CurrentDialogueSession = null;

        return DialogueEndResult.Successful(
            reason,
            session.SessionId,
            duration,
            session.VisitedNodeIds.Count,
            session.OptionsSelectedCount);
    }

    /// <summary>
    /// Builds a view model for the current dialogue state.
    /// </summary>
    private async Task<DialogueViewModel> BuildViewModelAsync(
        Character character,
        DialogueSession session)
    {
        var node = session.CurrentNode;
        var tree = session.Tree;

        // Evaluate all options
        var optionVisibilities = await _conditionEvaluator.EvaluateNodeOptionsAsync(character, node);

        // Build option view models
        var optionViewModels = new List<DialogueOptionViewModel>();
        var options = node.Options.OrderBy(o => o.DisplayOrder).ToList();

        for (int i = 0; i < options.Count; i++)
        {
            var option = options[i];
            var visibility = optionVisibilities.FirstOrDefault(v => v.OptionId == option.Id);

            // Skip hidden options that failed conditions
            if (visibility != null && !visibility.IsAvailable &&
                option.VisibilityMode == OptionVisibility.Hidden)
            {
                continue;
            }

            optionViewModels.Add(new DialogueOptionViewModel
            {
                OptionId = option.Id.ToString(),
                Text = option.Text,
                IsAvailable = visibility?.IsAvailable ?? true,
                IsVisible = true,
                LockedReason = visibility?.LockReason,
                VisibilityMode = option.VisibilityMode,
                DisplayOrder = option.DisplayOrder
            });
        }

        return new DialogueViewModel
        {
            SessionId = session.SessionId,
            NpcName = tree.NpcName,
            NpcTitle = tree.NpcTitle,
            SpeakerName = node.SpeakerName,
            DialogueText = node.Text,
            CurrentNodeId = node.NodeId,
            Options = optionViewModels,
            IsTerminalNode = node.IsTerminal,
            CanCancel = true
        };
    }
}
