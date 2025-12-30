using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.ViewModels;

namespace RuneAndRust.Engine.ViewModels;

/// <summary>
/// Builds SpecializationGridViewModel from character and specialization data.
/// Extracts ViewModel composition logic from GameService.
/// </summary>
/// <remarks>See: v0.4.1d (The Grid) for Specialization UI implementation.</remarks>
public class SpecializationGridViewModelBuilder : ISpecializationGridViewModelBuilder
{
    private readonly ISpecializationRepository _specRepo;
    private readonly ISpecializationService _specService;
    private readonly ILogger<SpecializationGridViewModelBuilder> _logger;

    public SpecializationGridViewModelBuilder(
        ISpecializationRepository specRepo,
        ISpecializationService specService,
        ILogger<SpecializationGridViewModelBuilder> logger)
    {
        _specRepo = specRepo;
        _specService = specService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<SpecializationGridViewModel> BuildAsync(Character character, Guid specializationId)
    {
        _logger.LogTrace("[SpecVM] BuildAsync for {CharName}, SpecId={SpecId}",
            character.Name, specializationId);

        var spec = await _specRepo.GetByIdAsync(specializationId);
        if (spec == null)
        {
            throw new ArgumentException($"Specialization {specializationId} not found", nameof(specializationId));
        }

        var nodes = await _specRepo.GetNodesForSpecializationAsync(specializationId);

        var nodeVms = new List<NodeViewModel>();
        foreach (var node in nodes.OrderBy(n => n.Tier).ThenBy(n => n.PositionY).ThenBy(n => n.PositionX))
        {
            var status = await ComputeNodeStatusAsync(character, node);
            _logger.LogTrace("[SpecVM] Node {NodeName}: Status={Status}", node.GetDisplayName(), status);

            nodeVms.Add(new NodeViewModel(
                NodeId: node.Id,
                AbilityId: node.AbilityId,
                Name: node.GetDisplayName(),
                Description: node.Ability?.Description ?? "No description available",
                Tier: node.Tier,
                CostPP: node.CostPP,
                Status: status,
                PositionX: node.PositionX,
                PositionY: node.PositionY,
                IsCapstone: node.IsCapstone,
                ParentNodeIds: node.ParentNodeIds));
        }

        var nodesByTier = nodeVms
            .GroupBy(n => n.Tier)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<NodeViewModel>)g.OrderBy(n => n.PositionY).ToList());

        // Count unlocked specs for tab indicator
        var totalSpecCount = character.UnlockedSpecializationIds.Count;
        var currentSpecIndex = character.UnlockedSpecializationIds.IndexOf(specializationId);
        if (currentSpecIndex < 0) currentSpecIndex = 0;

        var vm = new SpecializationGridViewModel
        {
            SpecializationId = specializationId,
            SpecializationName = spec.Name,
            SpecializationDescription = spec.Description,
            ProgressionPoints = character.ProgressionPoints,
            CharacterName = character.Name,
            NodesByTier = nodesByTier,
            AllNodes = nodeVms,
            SelectedNodeIndex = 0,
            CurrentSpecIndex = currentSpecIndex,
            TotalSpecCount = totalSpecCount > 0 ? totalSpecCount : 1
        };

        _logger.LogDebug("[SpecVM] Built VM with {NodeCount} nodes", nodeVms.Count);
        return vm;
    }

    /// <inheritdoc/>
    public async Task<SpecializationGridViewModel> RefreshAsync(
        SpecializationGridViewModel existing,
        Character character)
    {
        _logger.LogTrace("[SpecVM] RefreshAsync for {SpecName}", existing.SpecializationName);

        var refreshed = await BuildAsync(character, existing.SpecializationId);

        // Preserve selection (clamped to bounds)
        refreshed.SelectedNodeIndex = Math.Min(
            existing.SelectedNodeIndex,
            Math.Max(0, refreshed.AllNodes.Count - 1));

        // Preserve feedback from previous action
        refreshed.FeedbackMessage = existing.FeedbackMessage;
        refreshed.FeedbackIsSuccess = existing.FeedbackIsSuccess;

        _logger.LogDebug("[SpecVM] Refreshed, {UnlockedCount} now unlocked",
            refreshed.UnlockedCount);

        return refreshed;
    }

    /// <summary>
    /// Computes the display status for a node.
    /// </summary>
    private async Task<NodeStatus> ComputeNodeStatusAsync(Character character, SpecializationNode node)
    {
        // Check if already unlocked
        if (character.HasNode(node.Id))
        {
            return NodeStatus.Unlocked;
        }

        // Check prerequisites
        var (prereqsMet, _) = await _specService.ValidatePrerequisitesAsync(character, node);
        if (!prereqsMet)
        {
            return NodeStatus.Locked;
        }

        // Check PP
        if (character.ProgressionPoints < node.CostPP)
        {
            return NodeStatus.Affordable; // Prereqs met but can't afford
        }

        return NodeStatus.Available;
    }
}
