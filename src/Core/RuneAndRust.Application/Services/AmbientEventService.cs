using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Configuration;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Generates ambient atmospheric events based on context.
/// </summary>
/// <remarks>
/// This service creates random atmospheric occurrences that add life
/// to the environment. Events are filtered by biome, trigger type,
/// and cooldown to prevent spam while maintaining immersion.
/// </remarks>
public class AmbientEventService
{
    private readonly DescriptorService _descriptorService;
    private readonly AmbientEventConfiguration _config;
    private readonly ILogger<AmbientEventService> _logger;
    private readonly IGameEventLogger? _eventLogger;
    private readonly Random _random = new();

    /// <summary>
    /// Minimum time between ambient events.
    /// </summary>
    private static readonly TimeSpan MinEventCooldown = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Base chance for an ambient event to trigger (0.0 to 1.0).
    /// </summary>
    private const float BaseEventChance = 0.15f;

    public AmbientEventService(
        DescriptorService descriptorService,
        AmbientEventConfiguration config,
        ILogger<AmbientEventService> logger,
        IGameEventLogger? eventLogger = null)
    {
        _descriptorService = descriptorService ?? throw new ArgumentNullException(nameof(descriptorService));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _eventLogger = eventLogger;

        _logger.LogDebug(
            "AmbientEventService initialized with {EventPools} event pools",
            _config.EventPools.Count);
    }

    /// <summary>
    /// Attempts to generate an ambient event based on context.
    /// </summary>
    /// <param name="context">The ambient event context.</param>
    /// <returns>An ambient event if triggered, otherwise AmbientEvent.None.</returns>
    public AmbientEvent TryGenerateEvent(AmbientEventContext context)
    {
        // Check cooldown
        if (context.TimeSinceLastEvent < MinEventCooldown)
        {
            _logger.LogDebug("Event on cooldown, skipping");
            return AmbientEvent.None;
        }

        // Suppress events during combat (except combat trigger type)
        if (context.InCombat && context.Trigger != AmbientEventTrigger.Combat)
        {
            _logger.LogDebug("In combat, suppressing non-combat events");
            return AmbientEvent.None;
        }

        // Calculate event chance based on trigger and context
        var chance = CalculateEventChance(context);

        if (_random.NextDouble() > chance)
        {
            _logger.LogDebug("Event roll failed (chance: {Chance})", chance);
            return AmbientEvent.None;
        }

        // Generate the event
        return GenerateEvent(context);
    }

    /// <summary>
    /// Forces generation of an ambient event (bypasses chance roll).
    /// </summary>
    /// <param name="context">The ambient event context.</param>
    /// <returns>An ambient event.</returns>
    public AmbientEvent ForceGenerateEvent(AmbientEventContext context)
    {
        return GenerateEvent(context);
    }

    /// <summary>
    /// Gets an event of a specific type.
    /// </summary>
    /// <param name="eventType">The type of event to generate.</param>
    /// <param name="context">The ambient event context.</param>
    /// <returns>An ambient event of the specified type.</returns>
    public AmbientEvent GetEventOfType(AmbientEventType eventType, AmbientEventContext context)
    {
        var tags = BuildEventTags(context);
        var biome = context.Environment.Biome ?? "dungeon";

        // Try biome-specific pool first
        var poolPath = $"ambient.{eventType.ToString().ToLowerInvariant()}_{biome}";
        var description = _descriptorService.GetDescriptor(poolPath);

        // Fall back to generic pool
        if (string.IsNullOrEmpty(description))
        {
            poolPath = $"ambient.{eventType.ToString().ToLowerInvariant()}";
            description = _descriptorService.GetDescriptor(poolPath);
        }

        if (string.IsNullOrEmpty(description))
        {
            return AmbientEvent.None;
        }

        var ambientEvent = new AmbientEvent
        {
            EventType = eventType,
            Description = description,
            Intensity = DetermineIntensity(eventType, context),
            IsInterruptive = ShouldInterrupt(eventType, context)
        };

        _eventLogger?.LogEnvironment("AmbientEvent", description,
            data: new Dictionary<string, object>
            {
                ["eventType"] = eventType.ToString(),
                ["biome"] = context.Environment.Biome ?? "unknown",
                ["intensity"] = ambientEvent.Intensity,
                ["isInterruptive"] = ambientEvent.IsInterruptive,
                ["trigger"] = context.Trigger.ToString()
            });

        return ambientEvent;
    }

    private AmbientEvent GenerateEvent(AmbientEventContext context)
    {
        // Select event type based on context
        var eventType = SelectEventType(context);

        _logger.LogDebug(
            "Generating {EventType} event for biome {Biome}",
            eventType, context.Environment.Biome);

        return GetEventOfType(eventType, context);
    }

    private AmbientEventType SelectEventType(AmbientEventContext context)
    {
        var weights = new Dictionary<AmbientEventType, float>
        {
            { AmbientEventType.Sound, 35f },
            { AmbientEventType.Environmental, 30f },
            { AmbientEventType.Visual, 20f },
            { AmbientEventType.Creature, 15f }
        };

        // Adjust weights based on biome
        var biome = context.Environment.Biome;
        if (biome == "cave" || biome == "dungeon")
        {
            weights[AmbientEventType.Sound] += 10f;
            weights[AmbientEventType.Creature] += 5f;
        }
        else if (biome == "forest")
        {
            weights[AmbientEventType.Creature] += 15f;
            weights[AmbientEventType.Environmental] += 5f;
        }
        else if (biome == "volcanic")
        {
            weights[AmbientEventType.Environmental] += 15f;
            weights[AmbientEventType.Visual] += 10f;
        }

        // Adjust based on trigger
        if (context.Trigger == AmbientEventTrigger.Combat)
        {
            weights[AmbientEventType.Sound] += 20f;
        }

        return SelectWeighted(weights);
    }

    private float CalculateEventChance(AmbientEventContext context)
    {
        var chance = _config.BaseProbability > 0 ? _config.BaseProbability : BaseEventChance;

        // Increase chance based on time in room
        chance += context.TurnsInRoom * 0.02f;

        // Trigger-specific modifiers
        chance *= context.Trigger switch
        {
            AmbientEventTrigger.RoomEntry => 0.5f,  // Less likely on entry
            AmbientEventTrigger.Exploration => 1.0f,
            AmbientEventTrigger.Periodic => 1.2f,
            AmbientEventTrigger.Combat => 0.3f,     // Rare during combat
            _ => 1.0f
        };

        // Cap at 50%
        return Math.Min(chance, 0.5f);
    }

    private float DetermineIntensity(AmbientEventType eventType, AmbientEventContext context)
    {
        // Base intensity by type
        var intensity = eventType switch
        {
            AmbientEventType.Sound => 0.4f,
            AmbientEventType.Visual => 0.5f,
            AmbientEventType.Creature => 0.6f,
            AmbientEventType.Environmental => 0.3f,
            _ => 0.5f
        };

        // Combat increases intensity
        if (context.InCombat)
        {
            intensity += 0.2f;
        }

        return Math.Clamp(intensity, 0f, 1f);
    }

    private bool ShouldInterrupt(AmbientEventType eventType, AmbientEventContext context)
    {
        // Only high-intensity events during exploration interrupt
        if (context.InCombat) return false;

        return eventType == AmbientEventType.Creature &&
               _random.NextDouble() > 0.7;
    }

    private List<string> BuildEventTags(AmbientEventContext context)
    {
        var tags = new List<string>();

        if (!string.IsNullOrEmpty(context.Environment.Biome))
            tags.Add(context.Environment.Biome);

        if (!string.IsNullOrEmpty(context.Environment.Climate))
            tags.Add(context.Environment.Climate);

        if (!string.IsNullOrEmpty(context.Environment.Lighting))
            tags.Add(context.Environment.Lighting);

        if (context.InCombat)
            tags.Add("combat");

        return tags;
    }

    private T SelectWeighted<T>(Dictionary<T, float> weights) where T : notnull
    {
        var total = weights.Values.Sum();
        var roll = _random.NextDouble() * total;

        foreach (var (item, weight) in weights)
        {
            roll -= weight;
            if (roll <= 0)
                return item;
        }

        return weights.Keys.First();
    }
}
