using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Events;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;
using Character = RuneAndRust.Core.Entities.Character;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// Service for managing specialization unlocks and node purchases.
/// Handles PP spending, prerequisite validation, and event publishing.
/// </summary>
/// <remarks>See: v0.4.1b (The Unlock) for implementation.</remarks>
public class SpecializationService : ISpecializationService
{
    private readonly ISpecializationRepository _specRepo;
    private readonly IEventBus _eventBus;
    private readonly ILogger<SpecializationService> _logger;

    /// <summary>
    /// The fixed PP cost to unlock any specialization.
    /// </summary>
    public const int SpecializationUnlockCost = 10;

    /// <summary>
    /// Initializes a new instance of the <see cref="SpecializationService"/> class.
    /// </summary>
    /// <param name="specRepo">The specialization repository.</param>
    /// <param name="eventBus">The event bus for publishing events.</param>
    /// <param name="logger">The logger for traceability.</param>
    public SpecializationService(
        ISpecializationRepository specRepo,
        IEventBus eventBus,
        ILogger<SpecializationService> logger)
    {
        _specRepo = specRepo;
        _eventBus = eventBus;
        _logger = logger;
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Specialization Unlock Operations
    // ═══════════════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public async Task<SpecializationUnlockResult> UnlockSpecializationAsync(Character character, Guid specId)
    {
        _logger.LogTrace(
            "[SpecService] UnlockSpecializationAsync called for {CharName}: SpecId={SpecId}",
            character.Name, specId);

        // 1. Fetch specialization
        var spec = await _specRepo.GetByIdAsync(specId);
        if (spec == null)
        {
            _logger.LogWarning(
                "[SpecService] Unlock failed: Specialization {SpecId} not found",
                specId);
            return SpecializationUnlockResult.Failure("Specialization not found.");
        }

        // 2. Check if already unlocked
        if (character.HasSpecialization(specId))
        {
            _logger.LogWarning(
                "[SpecService] Unlock failed: {CharName} already has {SpecName}",
                character.Name, spec.Name);
            return SpecializationUnlockResult.Failure($"You have already unlocked {spec.Name}.");
        }

        // 3. Validate archetype match
        if (character.Archetype != spec.RequiredArchetype)
        {
            _logger.LogWarning(
                "[SpecService] Unlock failed: {CharName} is {CharArch}, requires {ReqArch}",
                character.Name, character.Archetype, spec.RequiredArchetype);
            return SpecializationUnlockResult.Failure(
                $"{spec.Name} requires the {spec.RequiredArchetype} archetype.");
        }

        // 4. Validate level requirement
        if (character.Level < spec.RequiredLevel)
        {
            _logger.LogWarning(
                "[SpecService] Unlock failed: {CharName} is Level {CharLevel}, requires {ReqLevel}",
                character.Name, character.Level, spec.RequiredLevel);
            return SpecializationUnlockResult.Failure(
                $"{spec.Name} requires Level {spec.RequiredLevel}. You are Level {character.Level}.");
        }

        // 5. Validate PP availability
        if (character.ProgressionPoints < SpecializationUnlockCost)
        {
            _logger.LogWarning(
                "[SpecService] Unlock failed: {CharName} has {Have} PP, needs {Need}",
                character.Name, character.ProgressionPoints, SpecializationUnlockCost);
            return SpecializationUnlockResult.Failure(
                $"Insufficient PP. Need {SpecializationUnlockCost}, have {character.ProgressionPoints}.");
        }

        // 6. Execute transaction
        character.ProgressionPoints -= SpecializationUnlockCost;
        character.UnlockedSpecializationIds.Add(specId);
        character.LastModified = DateTime.UtcNow;

        _logger.LogInformation(
            "[SpecService] {CharName} unlocked {SpecName}! Cost: {Cost} PP, Remaining: {Remaining} PP",
            character.Name, spec.Name, SpecializationUnlockCost, character.ProgressionPoints);

        // 7. Publish event
        var evt = new SpecializationUnlockedEvent(
            character.Id,
            character.Name,
            spec.Id,
            spec.Name,
            SpecializationUnlockCost);

        _eventBus.Publish(evt);

        _logger.LogDebug(
            "[SpecService] SpecializationUnlockedEvent published for {CharName}: {SpecName}",
            character.Name, spec.Name);

        return SpecializationUnlockResult.Ok(
            $"Unlocked {spec.Name}!",
            spec.Id,
            spec.Name,
            SpecializationUnlockCost);
    }

    /// <inheritdoc />
    public async Task<bool> CanUnlockSpecializationAsync(Character character, Guid specId)
    {
        var spec = await _specRepo.GetByIdAsync(specId);
        if (spec == null) return false;
        if (character.HasSpecialization(specId)) return false;
        if (character.Archetype != spec.RequiredArchetype) return false;
        if (character.Level < spec.RequiredLevel) return false;
        if (character.ProgressionPoints < SpecializationUnlockCost) return false;

        return true;
    }

    /// <inheritdoc />
    public int GetSpecializationUnlockCost() => SpecializationUnlockCost;

    // ═══════════════════════════════════════════════════════════════════════
    // Node Unlock Operations
    // ═══════════════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public async Task<NodeUnlockResult> UnlockNodeAsync(Character character, Guid nodeId)
    {
        _logger.LogTrace(
            "[SpecService] UnlockNodeAsync called for {CharName}: NodeId={NodeId}",
            character.Name, nodeId);

        // 1. Fetch node
        var node = await _specRepo.GetNodeByIdAsync(nodeId);
        if (node == null)
        {
            _logger.LogWarning(
                "[SpecService] Node unlock failed: Node {NodeId} not found",
                nodeId);
            return NodeUnlockResult.Failure("Node not found.");
        }

        var nodeName = node.GetDisplayName();

        // 2. Check if already unlocked
        if (character.HasNode(nodeId))
        {
            _logger.LogWarning(
                "[SpecService] Node unlock failed: {CharName} already has {NodeName}",
                character.Name, nodeName);
            return NodeUnlockResult.Failure($"You have already unlocked {nodeName}.");
        }

        // 3. Validate specialization is unlocked
        if (!character.HasSpecialization(node.SpecializationId))
        {
            _logger.LogWarning(
                "[SpecService] Node unlock failed: {CharName} has not unlocked specialization {SpecId}",
                character.Name, node.SpecializationId);
            return NodeUnlockResult.Failure("You must unlock the specialization first.");
        }

        // 4. Validate prerequisites
        var (prereqValid, prereqReason) = await ValidatePrerequisitesAsync(character, node);
        if (!prereqValid)
        {
            _logger.LogWarning(
                "[SpecService] Node unlock failed: Prerequisite check failed for {NodeName}. Reason: {Reason}",
                nodeName, prereqReason);
            return NodeUnlockResult.Failure(prereqReason ?? "Prerequisites not met.");
        }

        // 5. Validate PP availability
        if (character.ProgressionPoints < node.CostPP)
        {
            _logger.LogWarning(
                "[SpecService] Node unlock failed: {CharName} has {Have} PP, needs {Need}",
                character.Name, character.ProgressionPoints, node.CostPP);
            return NodeUnlockResult.Failure(
                $"Insufficient PP. Need {node.CostPP}, have {character.ProgressionPoints}.");
        }

        // 6. Execute transaction
        character.ProgressionPoints -= node.CostPP;
        await _specRepo.RecordNodeUnlockAsync(character.Id, nodeId);
        await _specRepo.SaveChangesAsync();

        // Update local collection for immediate HasNode checks
        character.SpecializationProgress.Add(new CharacterSpecializationProgress
        {
            CharacterId = character.Id,
            NodeId = nodeId,
            UnlockedAt = DateTime.UtcNow
        });

        character.LastModified = DateTime.UtcNow;

        _logger.LogInformation(
            "[SpecService] {CharName} unlocked node {NodeName} (Tier {Tier})! Cost: {Cost} PP, Remaining: {Remaining} PP",
            character.Name, nodeName, node.Tier, node.CostPP, character.ProgressionPoints);

        // 7. Publish event
        var evt = new NodeUnlockedEvent(
            character.Id,
            character.Name,
            node.Id,
            nodeName,
            node.AbilityId,
            node.Tier,
            node.IsCapstone,
            node.CostPP);

        _eventBus.Publish(evt);

        _logger.LogDebug(
            "[SpecService] NodeUnlockedEvent published for {CharName}: {NodeName} (Tier {Tier}, Capstone={IsCapstone})",
            character.Name, nodeName, node.Tier, node.IsCapstone);

        return NodeUnlockResult.Ok(
            $"Unlocked {nodeName}!",
            node.Id,
            nodeName,
            node.AbilityId,
            node.Tier,
            node.CostPP);
    }

    /// <inheritdoc />
    public async Task<bool> CanUnlockNodeAsync(Character character, Guid nodeId)
    {
        var node = await _specRepo.GetNodeByIdAsync(nodeId);
        if (node == null) return false;
        if (character.HasNode(nodeId)) return false;
        if (!character.HasSpecialization(node.SpecializationId)) return false;

        var (prereqValid, _) = await ValidatePrerequisitesAsync(character, node);
        if (!prereqValid) return false;

        if (character.ProgressionPoints < node.CostPP) return false;

        return true;
    }

    /// <inheritdoc />
    public async Task<(bool IsValid, string? FailureReason)> ValidatePrerequisitesAsync(
        Character character, SpecializationNode node)
    {
        _logger.LogTrace(
            "[SpecService] ValidatePrerequisitesAsync: Node={NodeId}, Tier={Tier}, IsCapstone={IsCapstone}",
            node.Id, node.Tier, node.IsCapstone);

        // Capstone (Tier 4): Requires ALL Tier 3 nodes in the tree
        if (node.IsCapstone)
        {
            var allNodes = await _specRepo.GetNodesForSpecializationAsync(node.SpecializationId);
            var tier3Nodes = allNodes.Where(n => n.Tier == 3).ToList();

            foreach (var tier3Node in tier3Nodes)
            {
                if (!character.HasNode(tier3Node.Id))
                {
                    var missingName = tier3Node.GetDisplayName();
                    _logger.LogDebug(
                        "[SpecService] Capstone prereq failed: Missing Tier 3 node {NodeName}",
                        missingName);
                    return (false, $"Capstone requires all Tier 3 nodes. Missing: {missingName}");
                }
            }

            _logger.LogDebug(
                "[SpecService] Capstone prereq check passed: All {Count} Tier 3 nodes unlocked",
                tier3Nodes.Count);
            return (true, null);
        }

        // Tier 1 (root nodes): No prerequisites
        if (node.ParentNodeIds.Count == 0)
        {
            _logger.LogDebug("[SpecService] Tier 1 node has no prerequisites");
            return (true, null);
        }

        // Standard nodes: All parent nodes must be unlocked
        foreach (var parentId in node.ParentNodeIds)
        {
            if (!character.HasNode(parentId))
            {
                var parentNode = await _specRepo.GetNodeByIdAsync(parentId);
                var parentName = parentNode?.GetDisplayName() ?? "Unknown";
                _logger.LogDebug(
                    "[SpecService] Prereq failed: Missing parent node {ParentName} ({ParentId})",
                    parentName, parentId);
                return (false, $"Requires: {parentName}");
            }
        }

        _logger.LogDebug(
            "[SpecService] All {Count} parent prerequisites satisfied",
            node.ParentNodeIds.Count);
        return (true, null);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Query Operations
    // ═══════════════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public async Task<IEnumerable<Specialization>> GetAvailableSpecializationsAsync(Character character)
    {
        _logger.LogDebug(
            "[SpecService] GetAvailableSpecializationsAsync for {CharName} ({Archetype})",
            character.Name, character.Archetype);

        return await _specRepo.GetByArchetypeAsync(character.Archetype);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<SpecializationNode>> GetNodesWithStatusAsync(Character character, Guid specId)
    {
        _logger.LogDebug(
            "[SpecService] GetNodesWithStatusAsync for {CharName}, SpecId={SpecId}",
            character.Name, specId);

        return await _specRepo.GetNodesForSpecializationAsync(specId);
    }
}
