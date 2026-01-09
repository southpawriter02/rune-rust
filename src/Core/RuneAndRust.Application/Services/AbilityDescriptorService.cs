using Microsoft.Extensions.Logging;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Context information for ability descriptor selection.
/// </summary>
public record AbilityDescriptorContext
{
    /// <summary>
    /// The ability identifier.
    /// </summary>
    public string AbilityId { get; init; } = string.Empty;

    /// <summary>
    /// The ability's damage type if applicable.
    /// </summary>
    public string? DamageType { get; init; }

    /// <summary>
    /// Tags from the ability definition.
    /// </summary>
    public IReadOnlyList<string> AbilityTags { get; init; } = [];

    /// <summary>
    /// The target type (self, enemy, area).
    /// </summary>
    public string TargetType { get; init; } = string.Empty;

    /// <summary>
    /// The caster's name.
    /// </summary>
    public string CasterName { get; init; } = string.Empty;

    /// <summary>
    /// The target's name if applicable.
    /// </summary>
    public string? TargetName { get; init; }

    /// <summary>
    /// Effects that were applied.
    /// </summary>
    public IReadOnlyList<string> AppliedEffects { get; init; } = [];

    /// <summary>
    /// The environment context for biome-aware descriptors.
    /// </summary>
    public EnvironmentContext? Environment { get; init; }
}

/// <summary>
/// Generates descriptive ability narratives.
/// </summary>
public class AbilityDescriptorService
{
    private readonly DescriptorService _descriptorService;
    private readonly ILogger<AbilityDescriptorService> _logger;

    public AbilityDescriptorService(
        DescriptorService descriptorService,
        ILogger<AbilityDescriptorService> logger)
    {
        _descriptorService = descriptorService ?? throw new ArgumentNullException(nameof(descriptorService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogDebug("AbilityDescriptorService initialized");
    }

    /// <summary>
    /// Generates an ability activation description (casting/channeling phase).
    /// </summary>
    /// <param name="context">The ability context.</param>
    /// <returns>A narrative description of ability activation.</returns>
    public string GetActivationDescription(AbilityDescriptorContext context)
    {
        _logger.LogDebug("GetActivationDescription: Ability={Ability}", context.AbilityId);

        // Try ability-specific activation first
        var description = _descriptorService.GetDescriptor(
            $"abilities.activation_{context.AbilityId}",
            context.AbilityTags);

        // Fall back to damage-type specific
        if (string.IsNullOrEmpty(description) && !string.IsNullOrEmpty(context.DamageType))
        {
            description = _descriptorService.GetDescriptor(
                $"abilities.activation_{context.DamageType}",
                context.AbilityTags);
        }

        // Fall back to generic casting
        if (string.IsNullOrEmpty(description))
        {
            description = _descriptorService.GetDescriptor("abilities.casting", context.AbilityTags);
        }

        return FormatWithCaster(description, context.CasterName) ?? $"{context.CasterName} channels power";
    }

    /// <summary>
    /// Generates an ability impact description (effect hitting target).
    /// </summary>
    /// <param name="context">The ability context.</param>
    /// <returns>A narrative description of the ability's impact.</returns>
    public string GetImpactDescription(AbilityDescriptorContext context)
    {
        _logger.LogDebug(
            "GetImpactDescription: Ability={Ability}, DamageType={DamageType}",
            context.AbilityId, context.DamageType);

        // Try damage-type specific effect pool
        if (!string.IsNullOrEmpty(context.DamageType))
        {
            var effectDescription = _descriptorService.GetDescriptor(
                $"abilities.effects_{context.DamageType}",
                context.AbilityTags);

            if (!string.IsNullOrEmpty(effectDescription))
            {
                return FormatWithTarget(effectDescription, context.TargetName) ?? effectDescription;
            }
        }

        // Fall back to generic impact
        var impact = _descriptorService.GetDescriptor("abilities.impact", context.AbilityTags);
        return FormatWithTarget(impact, context.TargetName) ?? "The ability takes effect";
    }

    /// <summary>
    /// Generates an ability aftermath description (lingering effects).
    /// </summary>
    /// <param name="context">The ability context.</param>
    /// <returns>A narrative description of lingering effects, or null if none.</returns>
    public string? GetAftermathDescription(AbilityDescriptorContext context)
    {
        if (context.AppliedEffects.Count == 0) return null;

        var description = _descriptorService.GetDescriptor("abilities.duration", context.AbilityTags);
        return description;
    }

    /// <summary>
    /// Generates a complete ability use narrative combining all phases.
    /// </summary>
    /// <param name="context">The ability context.</param>
    /// <returns>A complete narrative of the ability use.</returns>
    public string GetCompleteAbilityNarrative(AbilityDescriptorContext context)
    {
        var parts = new List<string>();

        var activation = GetActivationDescription(context);
        if (!string.IsNullOrEmpty(activation))
            parts.Add(activation);

        var impact = GetImpactDescription(context);
        if (!string.IsNullOrEmpty(impact))
            parts.Add(impact);

        var aftermath = GetAftermathDescription(context);
        if (!string.IsNullOrEmpty(aftermath))
            parts.Add(aftermath);

        return string.Join(" ", parts);
    }

    /// <summary>
    /// Generates a status effect application description.
    /// </summary>
    /// <param name="effectId">The status effect identifier.</param>
    /// <param name="targetName">The target's name.</param>
    /// <returns>A description of the status effect being applied.</returns>
    public string GetStatusEffectAppliedDescription(string effectId, string targetName)
    {
        var description = _descriptorService.GetDescriptor($"status.applied_{effectId}");

        if (string.IsNullOrEmpty(description))
        {
            description = _descriptorService.GetDescriptor("status.applied_generic");
        }

        return FormatWithTarget(description, targetName) ?? $"{targetName} is affected by {effectId}";
    }

    /// <summary>
    /// Generates a status effect removal description.
    /// </summary>
    /// <param name="effectId">The status effect identifier.</param>
    /// <param name="targetName">The target's name.</param>
    /// <returns>A description of the status effect being removed.</returns>
    public string GetStatusEffectRemovedDescription(string effectId, string targetName)
    {
        var description = _descriptorService.GetDescriptor($"status.removed_{effectId}");

        if (string.IsNullOrEmpty(description))
        {
            description = _descriptorService.GetDescriptor("status.removed_generic");
        }

        return FormatWithTarget(description, targetName) ?? $"The {effectId} effect on {targetName} fades";
    }

    private static string? FormatWithCaster(string? template, string casterName)
    {
        if (string.IsNullOrEmpty(template)) return null;
        return template.Replace("{caster}", casterName);
    }

    private static string? FormatWithTarget(string? template, string? targetName)
    {
        if (string.IsNullOrEmpty(template)) return null;
        if (string.IsNullOrEmpty(targetName)) return template;
        return template.Replace("{target}", targetName);
    }
}
