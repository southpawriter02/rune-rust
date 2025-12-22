using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Entities;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// Service for evaluating spawn rules and adjusting weights for biome elements.
/// Implements Phase 1 spawn rules (archetype, size, room name checks).
/// Phase 2 rules (RequiresEnemyType, RequiresHazardType) are deferred.
/// Part of the Dynamic Room Engine (v0.4.0).
/// </summary>
public class ElementSpawnEvaluator : IElementSpawnEvaluator
{
    private readonly ILogger<ElementSpawnEvaluator> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ElementSpawnEvaluator"/> class.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    public ElementSpawnEvaluator(ILogger<ElementSpawnEvaluator> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public bool CanSpawn(BiomeElement element, Room room, RoomTemplate template, SpawnContext context)
    {
        var rules = element.SpawnRules;

        // Rule: NeverInEntryHall
        if (rules.NeverInEntryHall == true && template.Archetype.Equals("EntryHall", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogDebug("[SpawnEvaluator] {Element} blocked by NeverInEntryHall rule", element.ElementName);
            return false;
        }

        // Rule: NeverInBossArena
        if (rules.NeverInBossArena == true && template.Archetype.Equals("BossArena", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogDebug("[SpawnEvaluator] {Element} blocked by NeverInBossArena rule", element.ElementName);
            return false;
        }

        // Rule: OnlyInLargeRooms
        if (rules.OnlyInLargeRooms == true && !template.Size.Equals("Large", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogDebug("[SpawnEvaluator] {Element} blocked by OnlyInLargeRooms rule (room size: {Size})",
                element.ElementName, template.Size);
            return false;
        }

        // Rule: RequiredArchetype
        if (!string.IsNullOrEmpty(rules.RequiredArchetype) &&
            !template.Archetype.Equals(rules.RequiredArchetype, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogDebug("[SpawnEvaluator] {Element} blocked by RequiredArchetype rule (required: {Required}, actual: {Actual})",
                element.ElementName, rules.RequiredArchetype, template.Archetype);
            return false;
        }

        // Rule: RequiresRoomNameContains
        if (rules.RequiresRoomNameContains != null && rules.RequiresRoomNameContains.Count > 0)
        {
            var nameContainsKeyword = rules.RequiresRoomNameContains.Any(keyword =>
                room.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase));

            if (!nameContainsKeyword)
            {
                _logger.LogDebug("[SpawnEvaluator] {Element} blocked by RequiresRoomNameContains rule (required keywords: {Keywords})",
                    element.ElementName, string.Join(", ", rules.RequiresRoomNameContains));
                return false;
            }
        }

        // Phase 2 Rules (Deferred - require multi-pass spawning)
        // TODO: Implement RequiresEnemyType (check context.SpawnedEnemyTypes)
        // TODO: Implement RequiresHazardType (check context.SpawnedHazardTypes)

        _logger.LogDebug("[SpawnEvaluator] {Element} passed all spawn rule checks", element.ElementName);
        return true;
    }

    /// <inheritdoc/>
    public float GetAdjustedWeight(BiomeElement element, Room room, RoomTemplate template)
    {
        var baseWeight = element.Weight;
        var rules = element.SpawnRules;

        // Rule: HigherWeightInSecretRooms
        if (rules.HigherWeightInSecretRooms == true &&
            template.Tags.Contains("Secret", StringComparer.OrdinalIgnoreCase))
        {
            var multiplier = rules.SecretRoomWeightMultiplier ?? 2.0f; // Default 2x if not specified
            var adjustedWeight = baseWeight * multiplier;

            _logger.LogDebug("[SpawnEvaluator] {Element} weight adjusted for secret room: {Base} -> {Adjusted} (x{Multiplier})",
                element.ElementName, baseWeight, adjustedWeight, multiplier);

            return adjustedWeight;
        }

        return baseWeight;
    }
}
