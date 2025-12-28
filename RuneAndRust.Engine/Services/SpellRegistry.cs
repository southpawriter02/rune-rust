using RuneAndRust.Core.Models.Magic;
using Microsoft.Extensions.Logging;

namespace RuneAndRust.Engine.Services;

public class SpellRegistry
{
    private readonly Dictionary<string, Spell> _spells = new();
    private readonly ILogger<SpellRegistry> _logger;

    public SpellRegistry(ILogger<SpellRegistry> logger)
    {
        _logger = logger;
        InitializeSpells();
    }

    private void InitializeSpells()
    {
        _logger.LogTrace("Initializing SpellRegistry with default spells.");

        var wyrdLight = new Spell
        {
            Id = "spell_wyrdlight",
            Name = "Wyrd-Light",
            Cost = 2,
            CastTime = CastTimeType.Instant,
            School = MagicSchool.Vision,
            EffectJson = "{ \"Type\": \"Illuminated\" }"
        };
        Register(wyrdLight);

        var rotBolt = new Spell
        {
            Id = "spell_rotbolt",
            Name = "Rot-Bolt",
            Cost = 5,
            CastTime = CastTimeType.Instant,
            School = MagicSchool.Entropy,
            EffectJson = "{ \"Type\": \"Damage\", \"Amount\": \"1d8\", \"DamageType\": \"Necrotic\" }"
        };
        Register(rotBolt);

        _logger.LogDebug("SpellRegistry initialized with {Count} spells.", _spells.Count);
    }

    public void Register(Spell spell)
    {
        if (_spells.ContainsKey(spell.Id))
        {
            _logger.LogWarning("Overwriting existing spell {SpellId}", spell.Id);
        }

        _spells[spell.Id] = spell;
        _logger.LogTrace("Registered spell {SpellName} ({SpellId})", spell.Name, spell.Id);
    }

    public Spell? GetSpell(string idOrName)
    {
        _logger.LogTrace("Lookup spell: {Query}", idOrName);

        if (_spells.TryGetValue(idOrName, out var spell))
        {
            _logger.LogDebug("Found exact spell match: {SpellId}", spell.Id);
            return spell;
        }

        // Fuzzy search by name
        var match = _spells.Values.FirstOrDefault(s => s.Name.Equals(idOrName, StringComparison.OrdinalIgnoreCase));

        if (match != null)
        {
             _logger.LogTrace("Found spell by name: {SpellName}", match.Name);
             return match;
        }

        _logger.LogWarning("Spell lookup failed for: {Query}", idOrName);
        return null;
    }
}
