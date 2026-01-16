namespace RuneAndRust.Infrastructure.Services;

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.DTOs;
using RuneAndRust.Application.Interfaces;

/// <summary>
/// Configuration for sound effect definitions.
/// </summary>
/// <remarks>
/// <para>
/// Features:
/// <list type="bullet">
///   <item><description>Effect lookup by ID</description></item>
///   <item><description>Category-based organization</description></item>
///   <item><description>Built-in default configuration</description></item>
/// </list>
/// </para>
/// </remarks>
public class SoundEffectConfig : ISoundEffectConfig
{
    private readonly Dictionary<string, SoundEffectDefinition> _effects = new();
    private readonly Dictionary<string, List<string>> _categoryEffects = new();
    private readonly SoundEffectSettings _settings;
    private readonly ILogger<SoundEffectConfig> _logger;

    /// <summary>
    /// Creates a sound effect configuration from a config object.
    /// </summary>
    /// <param name="config">The sound effects configuration.</param>
    /// <param name="logger">Logger for configuration operations.</param>
    public SoundEffectConfig(SoundEffectsConfig config, ILogger<SoundEffectConfig> logger)
    {
        _logger = logger;
        _settings = config.Settings;

        foreach (var (categoryName, category) in config.Categories)
        {
            var categoryKey = categoryName.ToLowerInvariant();
            _categoryEffects[categoryKey] = new List<string>();

            foreach (var (effectName, effectConfig) in category.Effects)
            {
                var effectId = effectName.ToLowerInvariant();
                var definition = new SoundEffectDefinition
                {
                    EffectId = effectId,
                    Category = categoryKey,
                    Files = effectConfig.Files,
                    Volume = effectConfig.Volume,
                    Randomize = effectConfig.Randomize
                };

                _effects[effectId] = definition;
                _categoryEffects[categoryKey].Add(effectId);
            }
        }

        _logger.LogInformation("Loaded {EffectCount} effects across {CategoryCount} categories",
            _effects.Count, _categoryEffects.Count);
    }

    /// <summary>
    /// Creates a sound effect configuration with default values.
    /// </summary>
    /// <param name="logger">Logger for configuration operations.</param>
    public SoundEffectConfig(ILogger<SoundEffectConfig> logger) : this(CreateDefaultConfig(), logger)
    {
    }

    /// <inheritdoc />
    public SoundEffectDefinition? GetEffect(string effectId)
    {
        var key = effectId.ToLowerInvariant();
        return _effects.GetValueOrDefault(key);
    }

    /// <inheritdoc />
    public IReadOnlyList<string> GetEffectsInCategory(string category)
    {
        var key = category.ToLowerInvariant();
        return _categoryEffects.GetValueOrDefault(key) ?? (IReadOnlyList<string>)Array.Empty<string>();
    }

    /// <inheritdoc />
    public IReadOnlyList<string> GetAllEffectIds() => _effects.Keys.ToList();

    /// <inheritdoc />
    public IReadOnlyList<string> GetAllCategories() => _categoryEffects.Keys.ToList();

    /// <inheritdoc />
    public SoundEffectSettings GetSettings() => _settings;

    /// <summary>
    /// Creates a default configuration for testing and fallback.
    /// </summary>
    private static SoundEffectsConfig CreateDefaultConfig() => new()
    {
        Categories = new Dictionary<string, CategoryDefinition>
        {
            ["combat"] = new()
            {
                Effects = new Dictionary<string, EffectFileConfig>
                {
                    ["attack-hit"] = new()
                    {
                        Files = new List<string>
                        {
                            "audio/sfx/combat/hit_01.ogg",
                            "audio/sfx/combat/hit_02.ogg",
                            "audio/sfx/combat/hit_03.ogg"
                        },
                        Volume = 0.8f,
                        Randomize = true
                    },
                    ["attack-miss"] = new()
                    {
                        Files = new List<string> { "audio/sfx/combat/whoosh.ogg" },
                        Volume = 0.6f,
                        Randomize = false
                    },
                    ["attack-critical"] = new()
                    {
                        Files = new List<string> { "audio/sfx/combat/critical_hit.ogg" },
                        Volume = 1.0f,
                        Randomize = false
                    },
                    ["attack-blocked"] = new()
                    {
                        Files = new List<string> { "audio/sfx/combat/block.ogg" },
                        Volume = 0.7f,
                        Randomize = false
                    },
                    ["damage-fire"] = new()
                    {
                        Files = new List<string> { "audio/sfx/combat/fire_damage.ogg" },
                        Volume = 0.8f,
                        Randomize = false
                    },
                    ["damage-ice"] = new()
                    {
                        Files = new List<string> { "audio/sfx/combat/ice_damage.ogg" },
                        Volume = 0.8f,
                        Randomize = false
                    },
                    ["damage-lightning"] = new()
                    {
                        Files = new List<string> { "audio/sfx/combat/lightning_damage.ogg" },
                        Volume = 0.9f,
                        Randomize = false
                    },
                    ["damage-poison"] = new()
                    {
                        Files = new List<string> { "audio/sfx/combat/poison_damage.ogg" },
                        Volume = 0.7f,
                        Randomize = false
                    },
                    ["damage-holy"] = new()
                    {
                        Files = new List<string> { "audio/sfx/combat/holy_damage.ogg" },
                        Volume = 0.8f,
                        Randomize = false
                    },
                    ["damage-shadow"] = new()
                    {
                        Files = new List<string> { "audio/sfx/combat/shadow_damage.ogg" },
                        Volume = 0.8f,
                        Randomize = false
                    },
                    ["death-monster"] = new()
                    {
                        Files = new List<string>
                        {
                            "audio/sfx/combat/monster_death_01.ogg",
                            "audio/sfx/combat/monster_death_02.ogg"
                        },
                        Volume = 0.9f,
                        Randomize = true
                    },
                    ["death-player"] = new()
                    {
                        Files = new List<string> { "audio/sfx/combat/player_death.ogg" },
                        Volume = 1.0f,
                        Randomize = false
                    }
                }
            },
            ["ability"] = new()
            {
                Effects = new Dictionary<string, EffectFileConfig>
                {
                    ["ability-cast"] = new()
                    {
                        Files = new List<string> { "audio/sfx/ability/cast_generic.ogg" },
                        Volume = 0.7f,
                        Randomize = false
                    },
                    ["ability-fire"] = new()
                    {
                        Files = new List<string> { "audio/sfx/ability/fire_cast.ogg" },
                        Volume = 0.8f,
                        Randomize = false
                    },
                    ["ability-ice"] = new()
                    {
                        Files = new List<string> { "audio/sfx/ability/ice_cast.ogg" },
                        Volume = 0.8f,
                        Randomize = false
                    },
                    ["ability-lightning"] = new()
                    {
                        Files = new List<string> { "audio/sfx/ability/lightning_cast.ogg" },
                        Volume = 0.9f,
                        Randomize = false
                    },
                    ["ability-heal"] = new()
                    {
                        Files = new List<string> { "audio/sfx/ability/heal_cast.ogg" },
                        Volume = 0.7f,
                        Randomize = false
                    },
                    ["ability-buff"] = new()
                    {
                        Files = new List<string> { "audio/sfx/ability/buff_apply.ogg" },
                        Volume = 0.6f,
                        Randomize = false
                    },
                    ["ability-debuff"] = new()
                    {
                        Files = new List<string> { "audio/sfx/ability/debuff_apply.ogg" },
                        Volume = 0.7f,
                        Randomize = false
                    },
                    ["ability-shadow"] = new()
                    {
                        Files = new List<string> { "audio/sfx/ability/shadow_cast.ogg" },
                        Volume = 0.8f,
                        Randomize = false
                    },
                    ["ability-nature"] = new()
                    {
                        Files = new List<string> { "audio/sfx/ability/nature_cast.ogg" },
                        Volume = 0.7f,
                        Randomize = false
                    },
                    ["ability-expire"] = new()
                    {
                        Files = new List<string> { "audio/sfx/ability/effect_expire.ogg" },
                        Volume = 0.5f,
                        Randomize = false
                    }
                }
            },
            ["ui"] = new()
            {
                Effects = new Dictionary<string, EffectFileConfig>
                {
                    ["button-click"] = new()
                    {
                        Files = new List<string> { "audio/sfx/ui/click.ogg" },
                        Volume = 0.5f,
                        Randomize = false
                    },
                    ["button-hover"] = new()
                    {
                        Files = new List<string> { "audio/sfx/ui/hover.ogg" },
                        Volume = 0.3f,
                        Randomize = false
                    }
                }
            },
            ["items"] = new()
            {
                Effects = new Dictionary<string, EffectFileConfig>
                {
                    ["item-pickup"] = new()
                    {
                        Files = new List<string> { "audio/sfx/items/pickup.ogg" },
                        Volume = 0.7f,
                        Randomize = false
                    }
                }
            }
        },
        Settings = new SoundEffectSettings
        {
            MaxSimultaneous = 8,
            DefaultVolume = 0.8f
        }
    };
}
