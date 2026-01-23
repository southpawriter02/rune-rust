using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Fluent builder for constructing <see cref="SkillContext"/> objects.
/// </summary>
/// <remarks>
/// <para>
/// Provides a chainable API for adding modifiers from various sources.
/// The builder is reusable after calling <see cref="Build"/> or <see cref="Reset"/>.
/// </para>
/// <para>
/// Example usage:
/// <code>
/// var context = builder
///     .WithEquipment("toolkit", "Tinker's Toolkit", diceModifier: 2)
///     .WithSituation(SituationalModifier.TimePressure("Combat"))
///     .WithEnvironment(SurfaceType.Stable, LightingLevel.Dim)
///     .Build();
/// </code>
/// </para>
/// </remarks>
public class SkillContextBuilder : ISkillContextBuilder
{
    private readonly ILogger<SkillContextBuilder> _logger;
    private readonly List<EquipmentModifier> _equipmentModifiers = new();
    private readonly List<SituationalModifier> _situationalModifiers = new();
    private readonly List<EnvironmentModifier> _environmentModifiers = new();
    private readonly List<TargetModifier> _targetModifiers = new();
    private readonly List<string> _appliedStatuses = new();

    /// <summary>
    /// Creates a new skill context builder.
    /// </summary>
    /// <param name="logger">Logger for debug output.</param>
    public SkillContextBuilder(ILogger<SkillContextBuilder> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public ISkillContextBuilder WithEquipment(
        string equipmentId,
        string name,
        int diceModifier = 0,
        int dcModifier = 0,
        EquipmentCategory category = EquipmentCategory.Tool,
        bool required = false)
    {
        var modifier = new EquipmentModifier(
            equipmentId, name, diceModifier, dcModifier, category, required);

        return WithEquipment(modifier);
    }

    /// <inheritdoc/>
    public ISkillContextBuilder WithEquipment(EquipmentModifier modifier)
    {
        _equipmentModifiers.Add(modifier);
        _logger.LogDebug("Added equipment modifier: {Modifier}", modifier.ToShortDescription());
        return this;
    }

    /// <inheritdoc/>
    public ISkillContextBuilder WithSituation(
        string modifierId,
        string name,
        int diceModifier = 0,
        int dcModifier = 0,
        string? source = null,
        ModifierDuration duration = ModifierDuration.Instant)
    {
        var modifier = new SituationalModifier(
            modifierId, name, diceModifier, dcModifier,
            source ?? name, duration);

        return WithSituation(modifier);
    }

    /// <inheritdoc/>
    public ISkillContextBuilder WithSituation(SituationalModifier modifier)
    {
        // Check for stackability
        if (!modifier.IsStackable)
        {
            var existing = _situationalModifiers.FindIndex(m => m.ModifierId == modifier.ModifierId);
            if (existing >= 0)
            {
                _logger.LogDebug(
                    "Non-stackable modifier {Id} already exists, skipping duplicate",
                    modifier.ModifierId);
                return this;
            }
        }

        _situationalModifiers.Add(modifier);
        _logger.LogDebug("Added situational modifier: {Modifier}", modifier.ToShortDescription());
        return this;
    }

    /// <inheritdoc/>
    public ISkillContextBuilder WithEnvironment(
        SurfaceType? surface = null,
        LightingLevel? lighting = null)
    {
        if (surface.HasValue && surface != SurfaceType.Normal)
        {
            WithEnvironment(EnvironmentModifier.FromSurface(surface.Value));
        }

        if (lighting.HasValue && lighting != LightingLevel.Normal)
        {
            WithEnvironment(EnvironmentModifier.FromLighting(lighting.Value));
        }

        return this;
    }

    /// <inheritdoc/>
    public ISkillContextBuilder WithCorruption(CorruptionTier tier)
    {
        if (tier != CorruptionTier.Normal)
        {
            WithEnvironment(EnvironmentModifier.FromCorruption(tier));
        }

        return this;
    }

    /// <inheritdoc/>
    public ISkillContextBuilder WithEnvironment(EnvironmentModifier modifier)
    {
        // Environment modifiers of the same type (surface, lighting, corruption) should replace existing ones
        var existingIndex = _environmentModifiers.FindIndex(m =>
            (modifier.SurfaceType.HasValue && m.SurfaceType.HasValue) ||
            (modifier.LightingLevel.HasValue && m.LightingLevel.HasValue) ||
            (modifier.CorruptionTier.HasValue && m.CorruptionTier.HasValue));

        if (existingIndex >= 0)
        {
            _logger.LogDebug(
                "Replacing existing environment modifier with: {Modifier}",
                modifier.ToShortDescription());
            _environmentModifiers[existingIndex] = modifier;
        }
        else
        {
            _environmentModifiers.Add(modifier);
            _logger.LogDebug("Added environment modifier: {Modifier}", modifier.ToShortDescription());
        }

        return this;
    }

    /// <inheritdoc/>
    public ISkillContextBuilder WithTargetDisposition(Disposition disposition, string? targetId = null)
    {
        return WithTarget(TargetModifier.FromDisposition(disposition, targetId));
    }

    /// <inheritdoc/>
    public ISkillContextBuilder WithTargetSuspicion(int suspicionLevel, string? targetId = null)
    {
        return WithTarget(TargetModifier.FromSuspicion(suspicionLevel, targetId));
    }

    /// <inheritdoc/>
    public ISkillContextBuilder WithTarget(TargetModifier modifier)
    {
        _targetModifiers.Add(modifier);
        _logger.LogDebug("Added target modifier: {Modifier}", modifier.ToShortDescription());
        return this;
    }

    /// <inheritdoc/>
    public ISkillContextBuilder WithAppliedStatus(string statusId)
    {
        if (!_appliedStatuses.Contains(statusId))
        {
            _appliedStatuses.Add(statusId);
            _logger.LogDebug("Added status to apply: {Status}", statusId);
        }

        return this;
    }

    /// <inheritdoc/>
    public SkillContext Build()
    {
        var context = new SkillContext(
            _equipmentModifiers.ToList().AsReadOnly(),
            _situationalModifiers.ToList().AsReadOnly(),
            _environmentModifiers.ToList().AsReadOnly(),
            _targetModifiers.ToList().AsReadOnly(),
            _appliedStatuses.ToList().AsReadOnly());

        _logger.LogDebug(
            "Built SkillContext: {Dice} dice, {Dc} DC ({Count} modifiers)",
            context.TotalDiceModifier,
            context.TotalDcModifier,
            context.ModifierCount);

        // Auto-reset after build for reuse
        Reset();

        return context;
    }

    /// <inheritdoc/>
    public ISkillContextBuilder Reset()
    {
        _equipmentModifiers.Clear();
        _situationalModifiers.Clear();
        _environmentModifiers.Clear();
        _targetModifiers.Clear();
        _appliedStatuses.Clear();

        return this;
    }
}
