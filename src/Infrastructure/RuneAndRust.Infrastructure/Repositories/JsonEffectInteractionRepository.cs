using System.Text.Json;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Infrastructure.Repositories;

/// <summary>
/// JSON-based repository for effect interaction definitions.
/// </summary>
public class JsonEffectInteractionRepository : IEffectInteractionRepository
{
    private readonly List<EffectInteraction> _interactions = new();
    private readonly Dictionary<string, List<EffectInteraction>> _byTriggerEffect = new();
    private readonly Dictionary<string, EffectInteraction> _byId = new();
    private readonly string _configPath;
    private bool _loaded;

    public JsonEffectInteractionRepository(string configPath)
    {
        _configPath = configPath ?? throw new ArgumentNullException(nameof(configPath));
    }

    public IReadOnlyList<EffectInteraction> GetAll()
    {
        EnsureLoaded();
        return _interactions.AsReadOnly();
    }

    public IReadOnlyList<EffectInteraction> GetByTriggerEffect(string triggerEffectId)
    {
        EnsureLoaded();
        var normalized = triggerEffectId.ToLowerInvariant();
        return _byTriggerEffect.TryGetValue(normalized, out var list)
            ? list.AsReadOnly()
            : Array.Empty<EffectInteraction>();
    }

    public EffectInteraction? GetById(string interactionId)
    {
        EnsureLoaded();
        return _byId.TryGetValue(interactionId.ToLowerInvariant(), out var interaction)
            ? interaction
            : null;
    }

    public EffectInteraction? GetForDamageType(string effectId, string damageType)
    {
        EnsureLoaded();
        var normalizedEffect = effectId.ToLowerInvariant();
        var normalizedDamage = damageType.ToLowerInvariant();

        if (_byTriggerEffect.TryGetValue(normalizedEffect, out var list))
        {
            return list.FirstOrDefault(i =>
                i.WithTrigger.Equals(normalizedDamage, StringComparison.OrdinalIgnoreCase));
        }
        return null;
    }

    private void EnsureLoaded()
    {
        if (_loaded) return;

        if (!File.Exists(_configPath))
        {
            _loaded = true;
            return; // No interactions configured
        }

        var json = File.ReadAllText(_configPath);
        var config = JsonSerializer.Deserialize<InteractionsConfig>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (config?.Interactions == null)
        {
            _loaded = true;
            return;
        }

        foreach (var dto in config.Interactions)
        {
            var interaction = new EffectInteraction(
                dto.Id.ToLowerInvariant(),
                dto.TriggerEffectId.ToLowerInvariant(),
                dto.WithTrigger.ToLowerInvariant(),
                dto.BonusDamagePercent ?? 0,
                dto.BonusDamageType,
                dto.ApplyEffect,
                dto.RemoveEffect,
                dto.Message ?? string.Empty);

            _interactions.Add(interaction);
            _byId[interaction.Id] = interaction;

            if (!_byTriggerEffect.ContainsKey(interaction.TriggerEffectId))
                _byTriggerEffect[interaction.TriggerEffectId] = new List<EffectInteraction>();
            _byTriggerEffect[interaction.TriggerEffectId].Add(interaction);
        }

        _loaded = true;
    }

    private class InteractionsConfig
    {
        public List<InteractionDto>? Interactions { get; set; }
    }

    private class InteractionDto
    {
        public string Id { get; set; } = string.Empty;
        public string TriggerEffectId { get; set; } = string.Empty;
        public string WithTrigger { get; set; } = string.Empty;
        public int? BonusDamagePercent { get; set; }
        public string? BonusDamageType { get; set; }
        public string? ApplyEffect { get; set; }
        public string? RemoveEffect { get; set; }
        public string? Message { get; set; }
    }
}
