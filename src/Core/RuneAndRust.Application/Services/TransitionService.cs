using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service for managing biome transitions.
/// </summary>
public class TransitionService : ITransitionService
{
    private readonly List<BiomeTransition> _transitions = [];
    private readonly ILogger<TransitionService> _logger;
    private readonly IGameEventLogger? _eventLogger;

    public TransitionService(
        ILogger<TransitionService>? logger = null,
        IGameEventLogger? eventLogger = null)
    {
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<TransitionService>.Instance;
        _eventLogger = eventLogger;

        RegisterDefaultTransitions();
        _logger.LogDebug("TransitionService initialized with {Count} transitions", _transitions.Count);
    }

    /// <inheritdoc/>
    public BiomeTransition? GetTransition(string sourceBiomeId, string targetBiomeId) =>
        _transitions.FirstOrDefault(t => t.AppliesTo(sourceBiomeId, targetBiomeId));

    /// <inheritdoc/>
    public IReadOnlyList<string> GetConnectableBiomes(string sourceBiomeId, int depth)
    {
        var biomes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var transition in _transitions)
        {
            if (!transition.IsAllowed || !transition.IsValidAtDepth(depth))
                continue;

            if (transition.SourceBiomeId.Equals(sourceBiomeId, StringComparison.OrdinalIgnoreCase))
                biomes.Add(transition.TargetBiomeId);
            else if (transition.IsBidirectional &&
                     transition.TargetBiomeId.Equals(sourceBiomeId, StringComparison.OrdinalIgnoreCase))
                biomes.Add(transition.SourceBiomeId);
        }

        return biomes.ToList();
    }

    /// <inheritdoc/>
    public TransitionBlend CreateBlend(string sourceBiomeId, string targetBiomeId, int roomIndex, int totalRooms)
    {
        if (totalRooms <= 1)
            return TransitionBlend.Pure(targetBiomeId);

        var ratio = (float)roomIndex / (totalRooms - 1);
        var blend = TransitionBlend.ForPosition(sourceBiomeId, targetBiomeId, ratio);

        _eventLogger?.LogExploration("BiomeTransition", $"Transitioning from {sourceBiomeId} to {targetBiomeId}",
            data: new Dictionary<string, object>
            {
                ["sourceBiome"] = sourceBiomeId,
                ["targetBiome"] = targetBiomeId,
                ["roomIndex"] = roomIndex,
                ["totalRooms"] = totalRooms,
                ["blendRatio"] = ratio
            });

        return blend;
    }

    /// <inheritdoc/>
    public bool IsTransitionAllowed(string sourceBiomeId, string targetBiomeId, int depth)
    {
        var transition = GetTransition(sourceBiomeId, targetBiomeId);
        if (transition == null)
        {
            _logger.LogDebug("No transition defined between {Source} and {Target}",
                sourceBiomeId, targetBiomeId);
            return false;
        }

        if (!transition.IsAllowed)
        {
            _logger.LogDebug("Transition from {Source} to {Target} is not allowed",
                sourceBiomeId, targetBiomeId);
            return false;
        }

        if (!transition.IsValidAtDepth(depth))
        {
            _logger.LogDebug("Transition from {Source} to {Target} invalid at depth {Depth}",
                sourceBiomeId, targetBiomeId, depth);
            return false;
        }

        return true;
    }

    /// <inheritdoc/>
    public void RegisterTransition(BiomeTransition transition)
    {
        ArgumentNullException.ThrowIfNull(transition);
        _transitions.Add(transition);
        _logger.LogDebug("Registered transition: {Source} → {Target}",
            transition.SourceBiomeId, transition.TargetBiomeId);
    }

    /// <inheritdoc/>
    public IReadOnlyList<BiomeTransition> GetAllTransitions() => _transitions.ToList();

    private void RegisterDefaultTransitions()
    {
        // Stone Corridors ↔ Fungal Caverns (gradual, depth 2+)
        RegisterTransition(BiomeTransition.Create(
            "stone-corridors", "fungal-caverns",
            TransitionStyle.Gradual,
            minDepth: 2));

        // Fungal Caverns ↔ Flooded Depths (gradual, depth 4+)
        RegisterTransition(BiomeTransition.Create(
            "fungal-caverns", "flooded-depths",
            TransitionStyle.Gradual,
            minDepth: 4));

        // Stone Corridors ↔ Flooded Depths (vertical, depth 4+)
        RegisterTransition(BiomeTransition.Create(
            "stone-corridors", "flooded-depths",
            TransitionStyle.Vertical,
            minDepth: 4,
            transitionLength: 1));

        // Volcanic and Flooded are opposites - not allowed
        RegisterTransition(BiomeTransition.Create(
            "volcanic-caverns", "flooded-depths",
            TransitionStyle.Abrupt,
            isAllowed: false));
    }
}
