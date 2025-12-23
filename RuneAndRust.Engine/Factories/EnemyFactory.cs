using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models.Combat;
using CharacterAttribute = RuneAndRust.Core.Enums.Attribute;

namespace RuneAndRust.Engine.Factories;

/// <summary>
/// Factory for creating Enemy entities from templates with scaling and variance.
/// Implements the Prototype pattern - templates define base stats, factory hydrates into live entities.
/// Extended in v0.2.4a to hydrate abilities from repository.
/// </summary>
public class EnemyFactory : IEnemyFactory
{
    private readonly ILogger<EnemyFactory> _logger;
    private readonly IDiceService _dice;
    private readonly ICreatureTraitService _traitService;
    private readonly IActiveAbilityRepository _abilityRepository;
    private readonly Dictionary<string, EnemyTemplate> _templates;

    /// <summary>
    /// Minimum variance multiplier (95% of base).
    /// </summary>
    private const float VarianceMin = 0.95f;

    /// <summary>
    /// Maximum variance multiplier (105% of base).
    /// </summary>
    private const float VarianceMax = 1.05f;

    /// <summary>
    /// Default fallback template ID when requested template is not found.
    /// </summary>
    private const string FallbackTemplateId = "und_draugr_01";

    /// <summary>
    /// Initializes a new instance of the <see cref="EnemyFactory"/> class.
    /// </summary>
    /// <param name="logger">The logger for traceability.</param>
    /// <param name="dice">The dice service for variance rolls.</param>
    /// <param name="traitService">The trait service for Elite enhancement.</param>
    /// <param name="abilityRepository">The ability repository for hydrating enemy abilities (v0.2.4a).</param>
    public EnemyFactory(
        ILogger<EnemyFactory> logger,
        IDiceService dice,
        ICreatureTraitService traitService,
        IActiveAbilityRepository abilityRepository)
    {
        _logger = logger;
        _dice = dice;
        _traitService = traitService;
        _abilityRepository = abilityRepository;
        _templates = InitializeTemplates();

        _logger.LogInformation("EnemyFactory initialized with {Count} templates", _templates.Count);
    }

    /// <inheritdoc/>
    public async Task<Enemy> CreateFromTemplateAsync(EnemyTemplate template, int partyLevel = 1)
    {
        _logger.LogInformation(
            "Creating enemy from template: {Id} (Tier: {Tier}, PartyLevel: {Level})",
            template.Id, template.Tier, partyLevel);

        // Calculate scaling multiplier based on tier and party level
        var scaler = CalculateScaler(template.Tier, partyLevel);

        // Apply variance (+/- 5%) using dice roll
        var varianceRoll = _dice.RollSingle(11, "HP Variance"); // 0-10 mapped to 0.95-1.05
        var variance = VarianceMin + (varianceRoll * 0.01f);

        var finalHp = Math.Max(1, (int)(template.BaseHp * scaler * variance));
        var finalStamina = Math.Max(1, (int)(template.BaseStamina * scaler * variance));

        _logger.LogDebug(
            "Scaling: Base HP {Base} x Scaler {Scaler:F2} x Variance {Variance:F2} = {Final}",
            template.BaseHp, scaler, variance, finalHp);

        var enemy = new Enemy
        {
            Id = Guid.NewGuid(),
            Name = template.Name,
            MaxHp = finalHp,
            CurrentHp = finalHp,
            MaxStamina = finalStamina,
            CurrentStamina = finalStamina,
            WeaponDamageDie = template.WeaponDamageDie,
            WeaponName = template.WeaponName,
            ArmorSoak = template.BaseSoak,
            Attributes = new Dictionary<CharacterAttribute, int>(template.Attributes),
            // Template/AI properties
            TemplateId = template.Id,
            Archetype = template.Archetype,
            Tags = new List<string>(template.Tags)
        };

        _logger.LogTrace(
            "Created {Name}: HP={Hp}, Stamina={Stam}, Archetype={Archetype}, Tags=[{Tags}]",
            enemy.Name, enemy.MaxHp, enemy.MaxStamina, enemy.Archetype, string.Join(", ", enemy.Tags));

        // Apply Elite promotion if tier is Elite or higher (v0.2.2c)
        if (template.Tier >= ThreatTier.Elite)
        {
            // Add tier tag for trait service detection
            enemy.Tags.Add(template.Tier.ToString());
            _logger.LogDebug("Added tier tag {Tier} to {Name}", template.Tier, enemy.Name);

            // Apply traits via CreatureTraitService
            _traitService.EnhanceEnemy(enemy);
            _logger.LogInformation(
                "Elite enemy enhanced: {Name} with {TraitCount} traits: [{Traits}]",
                enemy.Name, enemy.ActiveTraits.Count, string.Join(", ", enemy.ActiveTraits));
        }

        // Hydrate abilities from repository (v0.2.4a)
        await HydrateAbilitiesAsync(enemy, template);

        _logger.LogInformation("[EnemyFactory] Created {Enemy} with {AbilityCount} abilities",
            enemy.Name, enemy.Abilities.Count);

        return enemy;
    }

    /// <inheritdoc/>
    public async Task<Enemy> CreateByIdAsync(string templateId, int partyLevel = 1)
    {
        if (!_templates.TryGetValue(templateId, out var template))
        {
            _logger.LogWarning(
                "Template not found: {Id}, using fallback template {Fallback}",
                templateId, FallbackTemplateId);
            template = _templates[FallbackTemplateId];
        }

        return await CreateFromTemplateAsync(template, partyLevel);
    }

    /// <summary>
    /// Hydrates abilities from the repository based on template AbilityNames (v0.2.4a).
    /// </summary>
    /// <param name="enemy">The enemy to populate with abilities.</param>
    /// <param name="template">The template containing ability names to resolve.</param>
    private async Task HydrateAbilitiesAsync(Enemy enemy, EnemyTemplate template)
    {
        if (template.AbilityNames.Count == 0)
        {
            _logger.LogTrace("[EnemyFactory] No abilities to hydrate for {Enemy}", enemy.Name);
            return;
        }

        _logger.LogDebug("[EnemyFactory] Hydrating {Count} abilities for {Enemy}",
            template.AbilityNames.Count, enemy.Name);

        foreach (var abilityName in template.AbilityNames)
        {
            var ability = await _abilityRepository.GetByNameAsync(abilityName);
            if (ability != null)
            {
                enemy.Abilities.Add(ability);
                _logger.LogTrace("[EnemyFactory] Added ability '{Ability}' to {Enemy}",
                    abilityName, enemy.Name);
            }
            else
            {
                _logger.LogWarning("[EnemyFactory] Ability '{Ability}' not found for {Enemy}",
                    abilityName, enemy.Name);
            }
        }
    }

    /// <inheritdoc/>
    public IReadOnlyList<string> GetTemplateIds() => _templates.Keys.ToList();

    /// <inheritdoc/>
    public EnemyTemplate? GetTemplate(string templateId) =>
        _templates.TryGetValue(templateId, out var template) ? template : null;

    /// <summary>
    /// Calculates the stat scaling multiplier based on threat tier and party level.
    /// </summary>
    /// <param name="tier">The enemy's threat tier.</param>
    /// <param name="partyLevel">The party's current level.</param>
    /// <returns>The scaling multiplier to apply to base stats.</returns>
    private static float CalculateScaler(ThreatTier tier, int partyLevel)
    {
        // Base scaling: 10% increase per level above 1
        var levelScale = 1.0f + ((partyLevel - 1) * 0.1f);

        return tier switch
        {
            ThreatTier.Minion => 0.6f * levelScale,    // Weak fodder
            ThreatTier.Standard => 1.0f * levelScale,  // Normal enemies
            ThreatTier.Elite => 1.5f * levelScale,     // Enhanced threats
            ThreatTier.Boss => 2.5f,                    // Fixed high (no level scaling)
            _ => 1.0f * levelScale
        };
    }

    /// <summary>
    /// Initializes the built-in enemy template registry.
    /// Templates are AAM-VOICE compliant for Aethelgardian lore.
    /// </summary>
    /// <returns>A dictionary of template ID to EnemyTemplate.</returns>
    private static Dictionary<string, EnemyTemplate> InitializeTemplates()
    {
        return new Dictionary<string, EnemyTemplate>
        {
            // ═══════════════════════════════════════════════════════════════
            // UNDYING (Construct-class: Security/Labor automatons)
            // ═══════════════════════════════════════════════════════════════

            ["und_draugr_01"] = new EnemyTemplate(
                Id: "und_draugr_01",
                Name: "Rusted Draugr",
                Description: "A corroded security automaton. Firmware echoes ancient patrol protocols.",
                Archetype: EnemyArchetype.DPS,
                Tier: ThreatTier.Standard,
                BaseHp: 60,
                BaseStamina: 40,
                BaseSoak: 2,
                Attributes: new Dictionary<CharacterAttribute, int>
                {
                    { CharacterAttribute.Sturdiness, 5 },
                    { CharacterAttribute.Might, 6 },
                    { CharacterAttribute.Wits, 3 },
                    { CharacterAttribute.Will, 3 },
                    { CharacterAttribute.Finesse, 4 }
                },
                WeaponDamageDie: 6,
                WeaponName: "Rusted Blade",
                Tags: new List<string> { "Undying", "IronHeart", "Construct" },
                AbilityNames: new List<string> { "Rusty Cleave" }
            ),

            ["und_haug_01"] = new EnemyTemplate(
                Id: "und_haug_01",
                Name: "Haugbui Laborer",
                Description: "A towering construction unit. Task-loop corrupted to singular purpose: remove obstacles.",
                Archetype: EnemyArchetype.Tank,
                Tier: ThreatTier.Standard,
                BaseHp: 90,
                BaseStamina: 35,
                BaseSoak: 4,
                Attributes: new Dictionary<CharacterAttribute, int>
                {
                    { CharacterAttribute.Sturdiness, 7 },
                    { CharacterAttribute.Might, 5 },
                    { CharacterAttribute.Wits, 2 },
                    { CharacterAttribute.Will, 4 },
                    { CharacterAttribute.Finesse, 2 }
                },
                WeaponDamageDie: 8,
                WeaponName: "Industrial Fist",
                Tags: new List<string> { "Undying", "IronHeart", "Construct" },
                AbilityNames: new List<string> { "Grave Chill", "Baleful Glare" }
            ),

            // ═══════════════════════════════════════════════════════════════
            // MECHANICAL (Servitor-class: Utility/Maintenance units)
            // ═══════════════════════════════════════════════════════════════

            ["mec_serv_01"] = new EnemyTemplate(
                Id: "mec_serv_01",
                Name: "Utility Servitor",
                Description: "A janitorial drone. Harmless alone, dangerous in clusters.",
                Archetype: EnemyArchetype.Swarm,
                Tier: ThreatTier.Minion,
                BaseHp: 25,
                BaseStamina: 20,
                BaseSoak: 0,
                Attributes: new Dictionary<CharacterAttribute, int>
                {
                    { CharacterAttribute.Sturdiness, 3 },
                    { CharacterAttribute.Might, 3 },
                    { CharacterAttribute.Wits, 2 },
                    { CharacterAttribute.Will, 1 },
                    { CharacterAttribute.Finesse, 4 }
                },
                WeaponDamageDie: 4,
                WeaponName: "Cleaning Implement",
                Tags: new List<string> { "Mechanical", "Construct" },
                AbilityNames: new List<string> { "Servo Slam", "Overclock" }
            ),

            // ═══════════════════════════════════════════════════════════════
            // BEASTS (Corrupted fauna of Aethelgard)
            // ═══════════════════════════════════════════════════════════════

            ["bst_vargr_01"] = new EnemyTemplate(
                Id: "bst_vargr_01",
                Name: "Ash-Vargr",
                Description: "A corrupted predator. Hunts in fog-shrouded ruins.",
                Archetype: EnemyArchetype.GlassCannon,
                Tier: ThreatTier.Standard,
                BaseHp: 45,
                BaseStamina: 50,
                BaseSoak: 1,
                Attributes: new Dictionary<CharacterAttribute, int>
                {
                    { CharacterAttribute.Sturdiness, 4 },
                    { CharacterAttribute.Might, 7 },
                    { CharacterAttribute.Wits, 4 },
                    { CharacterAttribute.Will, 2 },
                    { CharacterAttribute.Finesse, 6 }
                },
                WeaponDamageDie: 8,
                WeaponName: "Fangs",
                Tags: new List<string> { "Beast", "Blighted" },
                AbilityNames: new List<string> { "Savage Lunge" }
            ),

            // ═══════════════════════════════════════════════════════════════
            // HUMANOID (Survivors and scavengers)
            // ═══════════════════════════════════════════════════════════════

            ["hum_raider_01"] = new EnemyTemplate(
                Id: "hum_raider_01",
                Name: "Rust-Clan Scav",
                Description: "A desperate scavenger. Relies on salvaged tech and crude traps.",
                Archetype: EnemyArchetype.Support,
                Tier: ThreatTier.Standard,
                BaseHp: 50,
                BaseStamina: 45,
                BaseSoak: 2,
                Attributes: new Dictionary<CharacterAttribute, int>
                {
                    { CharacterAttribute.Sturdiness, 4 },
                    { CharacterAttribute.Might, 4 },
                    { CharacterAttribute.Wits, 5 },
                    { CharacterAttribute.Will, 4 },
                    { CharacterAttribute.Finesse, 5 }
                },
                WeaponDamageDie: 6,
                WeaponName: "Salvaged Pipe",
                Tags: new List<string> { "Humanoid" },
                AbilityNames: new List<string> { "Scavenger's Swing", "Intimidating Shout" }
            )
        };
    }
}
