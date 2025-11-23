using RuneAndRust.Core;
using Serilog;
using System.Text.Json;

namespace RuneAndRust.Persistence;

/// <summary>
/// v0.23.2: Seeds boss abilities, telegraphed attacks, and AI patterns into the database
/// Extends BossEncounterSeeder to add ability configurations for all bosses
/// </summary>
public class BossAbilitySeeder
{
    private static readonly ILogger _log = Log.ForContext<BossAbilitySeeder>();
    private readonly BossEncounterRepository _repository;

    public BossAbilitySeeder(BossEncounterRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Seed all boss abilities and AI patterns
    /// Call this after BossEncounterSeeder.SeedBossEncounters()
    /// </summary>
    public void SeedBossAbilities()
    {
        _log.Information("Seeding boss abilities and AI patterns");

        try
        {
            SeedRuinWardenAbilities();
            SeedAethericAberrationAbilities();
            SeedForlornArchivistAbilities();
            SeedOmegaSentinelAbilities();

            _log.Information("Boss ability seeding completed successfully");
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to seed boss abilities");
            throw;
        }
    }

    // ═══════════════════════════════════════════════════════════
    // RUIN-WARDEN ABILITIES (Encounter ID 1)
    // ═══════════════════════════════════════════════════════════

    private void SeedRuinWardenAbilities()
    {
        _log.Debug("Seeding Ruin-Warden abilities");

        var boss = _repository.GetBossEncounterByEncounterId(1);
        if (boss == null)
        {
            _log.Warning("Ruin-Warden boss not found, skipping ability seeding");
            return;
        }

        // Standard Attack: Electro-Blade Slash
        _repository.CreateBossAbility(new BossAbilityData
        {
            BossEncounterId = boss.BossEncounterId,
            AbilityName = "Electro-Blade Slash",
            AbilityDescription = "The Ruin-Warden strikes with its electrified blade.",
            AbilityType = "Standard",
            IsTelegraphed = false,
            BaseDamageDice = 2,
            DamageBonus = 3,
            DamageType = "Physical",
            TargetType = "Single",
            CooldownTurns = 0
        });

        // Standard Attack: Shield Charge
        _repository.CreateBossAbility(new BossAbilityData
        {
            BossEncounterId = boss.BossEncounterId,
            AbilityName = "Shield Charge",
            AbilityDescription = "The Warden charges forward with its shield.",
            AbilityType = "Standard",
            IsTelegraphed = false,
            BaseDamageDice = 1,
            DamageBonus = 5,
            DamageType = "Physical",
            TargetType = "Single",
            AppliesStatusEffects = JsonSerializer.Serialize(new List<StatusEffectDefinition>
            {
                new StatusEffectDefinition { StatusName = "Stunned", Duration = 1 }
            }),
            CooldownTurns = 3
        });

        // Telegraphed: System Overload
        _repository.CreateBossAbility(new BossAbilityData
        {
            BossEncounterId = boss.BossEncounterId,
            AbilityName = "System Overload",
            AbilityDescription = "The Ruin-Warden releases an electromagnetic pulse in all directions!",
            AbilityType = "Telegraphed",
            IsTelegraphed = true,
            TelegraphChargeTurns = 1,
            TelegraphWarningMessage = "⚡ The Warden's core crackles with energy! Electromagnetic pulse imminent!",
            BaseDamageDice = 3,
            DamageBonus = 4,
            DamageType = "Energy",
            TargetType = "AoE",
            AoeRadius = 3,
            InterruptDamageThreshold = 15,
            InterruptStaggerDuration = 2,
            CooldownTurns = 5
        });

        // Ultimate: Total System Failure
        _repository.CreateBossAbility(new BossAbilityData
        {
            BossEncounterId = boss.BossEncounterId,
            AbilityName = "Total System Failure",
            AbilityDescription = "💀 TOTAL SYSTEM FAILURE! All safety protocols offline!",
            AbilityType = "Ultimate",
            IsTelegraphed = true,
            IsUltimate = true,
            TelegraphChargeTurns = 2,
            TelegraphWarningMessage = "🔥 CRITICAL ERROR! The Warden's systems are overloading catastrophically!",
            VulnerabilityDurationTurns = 3,
            VulnerabilityDamageMultiplier = 1.5f,
            BaseDamageDice = 5,
            DamageBonus = 6,
            DamageType = "Energy",
            TargetType = "All",
            InterruptDamageThreshold = 25,
            InterruptStaggerDuration = 3,
            CooldownTurns = 999
        });

        // Seed AI patterns for each phase
        SeedRuinWardenAIPatterns(boss.BossEncounterId);

        _log.Debug("Ruin-Warden abilities seeded successfully");
    }

    private void SeedRuinWardenAIPatterns(int bossEncounterId)
    {
        // Phase 1: Defensive
        _repository.CreateBossAIPattern(new BossAIPatternData
        {
            BossEncounterId = bossEncounterId,
            PhaseNumber = 1,
            PatternName = "Defensive Protocols",
            PatternDescription = "Balanced offense and defense",
            TargetPriorityList = JsonSerializer.Serialize(new[] { "LowestHP", "HighestDamage" }),
            PreferredAbilities = JsonSerializer.Serialize(new[] { "Electro-Blade Slash", "Shield Charge" }),
            TelegraphFrequency = 0.2f,
            UltimateHpThreshold = 0.25f,
            PrefersMelee = true,
            PrefersRange = false,
            PositioningStrategy = "Frontline"
        });

        // Phase 2: Aggressive
        _repository.CreateBossAIPattern(new BossAIPatternData
        {
            BossEncounterId = bossEncounterId,
            PhaseNumber = 2,
            PatternName = "Emergency Protocols",
            PatternDescription = "Increased aggression with telegraphed AoE",
            TargetPriorityList = JsonSerializer.Serialize(new[] { "HighestDamage", "LowestHP" }),
            PreferredAbilities = JsonSerializer.Serialize(new[] { "System Overload", "Electro-Blade Slash" }),
            TelegraphFrequency = 0.4f,
            UltimateHpThreshold = 0.25f,
            PrefersMelee = true,
            PrefersRange = false,
            PositioningStrategy = "Aggressive"
        });

        // Phase 3: Desperate
        _repository.CreateBossAIPattern(new BossAIPatternData
        {
            BossEncounterId = bossEncounterId,
            PhaseNumber = 3,
            PatternName = "Critical State",
            PatternDescription = "Maximum aggression, frequent telegraphs and ultimate",
            TargetPriorityList = JsonSerializer.Serialize(new[] { "HighestDamage", "LowestHP", "Random" }),
            PreferredAbilities = JsonSerializer.Serialize(new[] { "Total System Failure", "System Overload", "Shield Charge" }),
            TelegraphFrequency = 0.6f,
            UltimateHpThreshold = 0.30f,
            PrefersMelee = true,
            PrefersRange = false,
            PositioningStrategy = "Reckless"
        });
    }

    // ═══════════════════════════════════════════════════════════
    // AETHERIC ABERRATION ABILITIES (Encounter ID 2)
    // ═══════════════════════════════════════════════════════════

    private void SeedAethericAberrationAbilities()
    {
        _log.Debug("Seeding Aetheric Aberration abilities");

        var boss = _repository.GetBossEncounterByEncounterId(2);
        if (boss == null)
        {
            _log.Warning("Aetheric Aberration boss not found, skipping ability seeding");
            return;
        }

        // Standard Attack: Void Blast
        _repository.CreateBossAbility(new BossAbilityData
        {
            BossEncounterId = boss.BossEncounterId,
            AbilityName = "Void Blast",
            AbilityDescription = "A bolt of void energy strikes the target.",
            AbilityType = "Standard",
            IsTelegraphed = false,
            BaseDamageDice = 3,
            DamageBonus = 2,
            DamageType = "Magic",
            TargetType = "Single",
            CooldownTurns = 0
        });

        // Support: Phase Shift
        _repository.CreateBossAbility(new BossAbilityData
        {
            BossEncounterId = boss.BossEncounterId,
            AbilityName = "Phase Shift",
            AbilityDescription = "The Aberration shifts partially out of phase with reality.",
            AbilityType = "Support",
            IsTelegraphed = false,
            BaseDamageDice = 0,
            SpecialEffects = JsonSerializer.Serialize(new SpecialEffectDefinition
            {
                DefenseBonus = 5,
                DefenseDuration = 2
            }),
            CooldownTurns = 4
        });

        // Telegraphed: Reality Tear
        _repository.CreateBossAbility(new BossAbilityData
        {
            BossEncounterId = boss.BossEncounterId,
            AbilityName = "Reality Tear",
            AbilityDescription = "The fabric of reality tears open, releasing destructive void energy!",
            AbilityType = "Telegraphed",
            IsTelegraphed = true,
            TelegraphChargeTurns = 1,
            TelegraphWarningMessage = "🌀 Reality fractures around the Aberration!",
            BaseDamageDice = 4,
            DamageBonus = 3,
            DamageType = "Magic",
            TargetType = "AoE",
            AoeRadius = 2,
            InterruptDamageThreshold = 12,
            CooldownTurns = 5
        });

        // Support: Summon Echoes
        _repository.CreateBossAbility(new BossAbilityData
        {
            BossEncounterId = boss.BossEncounterId,
            AbilityName = "Summon Echoes",
            AbilityDescription = "The Aberration summons echoes of itself from other dimensions.",
            AbilityType = "Support",
            IsTelegraphed = false,
            BaseDamageDice = 0,
            SpecialEffects = JsonSerializer.Serialize(new SpecialEffectDefinition
            {
                SummonCount = 1,
                SummonType = EnemyType.BlightDrone
            }),
            CooldownTurns = 8
        });

        // Ultimate: Aetheric Storm
        _repository.CreateBossAbility(new BossAbilityData
        {
            BossEncounterId = boss.BossEncounterId,
            AbilityName = "Aetheric Storm",
            AbilityDescription = "💀 AETHERIC STORM! The veil between dimensions collapses!",
            AbilityType = "Ultimate",
            IsTelegraphed = true,
            IsUltimate = true,
            TelegraphChargeTurns = 2,
            TelegraphWarningMessage = "🔥 The Aberration channels immense void energy! Reality itself is unraveling!",
            VulnerabilityDurationTurns = 3,
            VulnerabilityDamageMultiplier = 1.5f,
            BaseDamageDice = 6,
            DamageBonus = 5,
            DamageType = "Magic",
            TargetType = "All",
            AppliesStatusEffects = JsonSerializer.Serialize(new List<StatusEffectDefinition>
            {
                new StatusEffectDefinition { StatusName = "Disoriented", Duration = 2 }
            }),
            InterruptDamageThreshold = 30,
            CooldownTurns = 999
        });

        SeedAethericAberrationAIPatterns(boss.BossEncounterId);
        _log.Debug("Aetheric Aberration abilities seeded successfully");
    }

    private void SeedAethericAberrationAIPatterns(int bossEncounterId)
    {
        // Phase 1: Cautious Caster
        _repository.CreateBossAIPattern(new BossAIPatternData
        {
            BossEncounterId = bossEncounterId,
            PhaseNumber = 1,
            PatternName = "Manifestation",
            PatternDescription = "Ranged attacks with defensive positioning",
            TargetPriorityList = JsonSerializer.Serialize(new[] { "HighestDamage", "LowestHP" }),
            PreferredAbilities = JsonSerializer.Serialize(new[] { "Void Blast", "Phase Shift" }),
            TelegraphFrequency = 0.2f,
            UltimateHpThreshold = 0.25f,
            PrefersMelee = false,
            PrefersRange = true,
            PositioningStrategy = "Backline"
        });

        // Phase 2: Aggressive Caster
        _repository.CreateBossAIPattern(new BossAIPatternData
        {
            BossEncounterId = bossEncounterId,
            PhaseNumber = 2,
            PatternName = "Reality Tear",
            PatternDescription = "Increased spell frequency with summons",
            TargetPriorityList = JsonSerializer.Serialize(new[] { "HighestDamage", "LowestHP" }),
            PreferredAbilities = JsonSerializer.Serialize(new[] { "Reality Tear", "Summon Echoes", "Void Blast" }),
            TelegraphFrequency = 0.4f,
            UltimateHpThreshold = 0.25f,
            PrefersMelee = false,
            PrefersRange = true,
            PositioningStrategy = "Mobile"
        });

        // Phase 3: Desperate Caster
        _repository.CreateBossAIPattern(new BossAIPatternData
        {
            BossEncounterId = bossEncounterId,
            PhaseNumber = 3,
            PatternName = "Desperate Void",
            PatternDescription = "Constant AoE attacks and ultimate spam",
            TargetPriorityList = JsonSerializer.Serialize(new[] { "Random", "LowestHP" }),
            PreferredAbilities = JsonSerializer.Serialize(new[] { "Aetheric Storm", "Reality Tear" }),
            TelegraphFrequency = 0.7f,
            UltimateHpThreshold = 0.35f,
            PrefersMelee = false,
            PrefersRange = true,
            PositioningStrategy = "Reckless"
        });
    }

    // ═══════════════════════════════════════════════════════════
    // FORLORN ARCHIVIST ABILITIES (Encounter ID 3)
    // ═══════════════════════════════════════════════════════════

    private void SeedForlornArchivistAbilities()
    {
        _log.Debug("Seeding Forlorn Archivist abilities");

        var boss = _repository.GetBossEncounterByEncounterId(3);
        if (boss == null)
        {
            _log.Warning("Forlorn Archivist boss not found, skipping ability seeding");
            return;
        }

        // Standard Attack: Mind Spike
        _repository.CreateBossAbility(new BossAbilityData
        {
            BossEncounterId = boss.BossEncounterId,
            AbilityName = "Mind Spike",
            AbilityDescription = "A psychic spike pierces the target's mind.",
            AbilityType = "Standard",
            IsTelegraphed = false,
            BaseDamageDice = 2,
            DamageBonus = 4,
            DamageType = "Psychic",
            TargetType = "Single",
            CooldownTurns = 0
        });

        // AoE Attack: Psychic Scream
        _repository.CreateBossAbility(new BossAbilityData
        {
            BossEncounterId = boss.BossEncounterId,
            AbilityName = "Psychic Scream",
            AbilityDescription = "The Archivist unleashes a psychic scream that disorients all nearby.",
            AbilityType = "Standard",
            IsTelegraphed = false,
            BaseDamageDice = 2,
            DamageBonus = 2,
            DamageType = "Psychic",
            TargetType = "AoE",
            AoeRadius = 2,
            AppliesStatusEffects = JsonSerializer.Serialize(new List<StatusEffectDefinition>
            {
                new StatusEffectDefinition { StatusName = "Disoriented", Duration = 1 }
            }),
            CooldownTurns = 4
        });

        // Support: Summon Revenants
        _repository.CreateBossAbility(new BossAbilityData
        {
            BossEncounterId = boss.BossEncounterId,
            AbilityName = "Summon Revenants",
            AbilityDescription = "The dead stir at the Archivist's call.",
            AbilityType = "Support",
            IsTelegraphed = false,
            BaseDamageDice = 0,
            SpecialEffects = JsonSerializer.Serialize(new SpecialEffectDefinition
            {
                SummonCount = 2,
                SummonType = EnemyType.CorruptedServitor
            }),
            CooldownTurns = 7
        });

        // Telegraphed: Mass Hysteria
        _repository.CreateBossAbility(new BossAbilityData
        {
            BossEncounterId = boss.BossEncounterId,
            AbilityName = "Mass Hysteria",
            AbilityDescription = "Overwhelming psychic pressure drives all targets to panic!",
            AbilityType = "Telegraphed",
            IsTelegraphed = true,
            TelegraphChargeTurns = 1,
            TelegraphWarningMessage = "🔮 The Archivist's eyes glow with eldritch light! Psychic pressure builds!",
            BaseDamageDice = 3,
            DamageBonus = 5,
            DamageType = "Psychic",
            TargetType = "All",
            AppliesStatusEffects = JsonSerializer.Serialize(new List<StatusEffectDefinition>
            {
                new StatusEffectDefinition { StatusName = "Disoriented", Duration = 2 }
            }),
            InterruptDamageThreshold = 18,
            CooldownTurns = 6
        });

        // Ultimate: Psychic Storm
        _repository.CreateBossAbility(new BossAbilityData
        {
            BossEncounterId = boss.BossEncounterId,
            AbilityName = "Psychic Storm",
            AbilityDescription = "💀 PSYCHIC STORM! The Archivist's fractured mind unleashes pure madness!",
            AbilityType = "Ultimate",
            IsTelegraphed = true,
            IsUltimate = true,
            TelegraphChargeTurns = 2,
            TelegraphWarningMessage = "🔥 MADNESS UNLEASHED! The Archivist's sanity shatters completely!",
            VulnerabilityDurationTurns = 3,
            VulnerabilityDamageMultiplier = 1.5f,
            BaseDamageDice = 5,
            DamageBonus = 7,
            DamageType = "Psychic",
            TargetType = "All",
            AppliesStatusEffects = JsonSerializer.Serialize(new List<StatusEffectDefinition>
            {
                new StatusEffectDefinition { StatusName = "Disoriented", Duration = 3 },
                new StatusEffectDefinition { StatusName = "Bleeding", Duration = 2, DamagePerTurn = 3 }
            }),
            InterruptDamageThreshold = 35,
            CooldownTurns = 999
        });

        SeedForlornArchivistAIPatterns(boss.BossEncounterId);
        _log.Debug("Forlorn Archivist abilities seeded successfully");
    }

    private void SeedForlornArchivistAIPatterns(int bossEncounterId)
    {
        // Phase 1: Psychic Whispers
        _repository.CreateBossAIPattern(new BossAIPatternData
        {
            BossEncounterId = bossEncounterId,
            PhaseNumber = 1,
            PatternName = "Psychic Whispers",
            PatternDescription = "Focused psychic attacks",
            TargetPriorityList = JsonSerializer.Serialize(new[] { "LowestHP", "HighestDamage" }),
            PreferredAbilities = JsonSerializer.Serialize(new[] { "Mind Spike", "Psychic Scream" }),
            TelegraphFrequency = 0.15f,
            UltimateHpThreshold = 0.25f,
            PrefersMelee = false,
            PrefersRange = true,
            PositioningStrategy = "Backline"
        });

        // Phase 2: Psychic Intensification
        _repository.CreateBossAIPattern(new BossAIPatternData
        {
            BossEncounterId = bossEncounterId,
            PhaseNumber = 2,
            PatternName = "Psychic Intensification",
            PatternDescription = "AoE attacks with undead summons",
            TargetPriorityList = JsonSerializer.Serialize(new[] { "HighestDamage", "LowestHP" }),
            PreferredAbilities = JsonSerializer.Serialize(new[] { "Mass Hysteria", "Summon Revenants", "Psychic Scream" }),
            TelegraphFrequency = 0.35f,
            UltimateHpThreshold = 0.25f,
            PrefersMelee = false,
            PrefersRange = true,
            PositioningStrategy = "Mobile"
        });

        // Phase 3: Madness Unleashed
        _repository.CreateBossAIPattern(new BossAIPatternData
        {
            BossEncounterId = bossEncounterId,
            PhaseNumber = 3,
            PatternName = "Madness Unleashed",
            PatternDescription = "Relentless psychic assault",
            TargetPriorityList = JsonSerializer.Serialize(new[] { "Random", "LowestHP" }),
            PreferredAbilities = JsonSerializer.Serialize(new[] { "Psychic Storm", "Mass Hysteria" }),
            TelegraphFrequency = 0.65f,
            UltimateHpThreshold = 0.30f,
            PrefersMelee = false,
            PrefersRange = true,
            PositioningStrategy = "Stationary"
        });
    }

    // ═══════════════════════════════════════════════════════════
    // OMEGA SENTINEL ABILITIES (Encounter ID 4)
    // ═══════════════════════════════════════════════════════════

    private void SeedOmegaSentinelAbilities()
    {
        _log.Debug("Seeding Omega Sentinel abilities");

        var boss = _repository.GetBossEncounterByEncounterId(4);
        if (boss == null)
        {
            _log.Warning("Omega Sentinel boss not found, skipping ability seeding");
            return;
        }

        // Standard Attack: Maul Strike
        _repository.CreateBossAbility(new BossAbilityData
        {
            BossEncounterId = boss.BossEncounterId,
            AbilityName = "Maul Strike",
            AbilityDescription = "A devastating melee strike with the Sentinel's massive fists.",
            AbilityType = "Standard",
            IsTelegraphed = false,
            BaseDamageDice = 3,
            DamageBonus = 5,
            DamageType = "Physical",
            TargetType = "Single",
            CooldownTurns = 0
        });

        // AoE Attack: Seismic Slam
        _repository.CreateBossAbility(new BossAbilityData
        {
            BossEncounterId = boss.BossEncounterId,
            AbilityName = "Seismic Slam",
            AbilityDescription = "The Sentinel slams the ground, sending shockwaves in all directions.",
            AbilityType = "Standard",
            IsTelegraphed = false,
            BaseDamageDice = 2,
            DamageBonus = 6,
            DamageType = "Physical",
            TargetType = "AoE",
            AoeRadius = 3,
            CooldownTurns = 3
        });

        // Support: Power Draw
        _repository.CreateBossAbility(new BossAbilityData
        {
            BossEncounterId = boss.BossEncounterId,
            AbilityName = "Power Draw",
            AbilityDescription = "The Sentinel draws power from backup generators.",
            AbilityType = "Support",
            IsTelegraphed = false,
            BaseDamageDice = 0,
            SpecialEffects = JsonSerializer.Serialize(new SpecialEffectDefinition
            {
                HealAmount = 25
            }),
            CooldownTurns = 5
        });

        // Telegraphed: Overcharged Maul
        _repository.CreateBossAbility(new BossAbilityData
        {
            BossEncounterId = boss.BossEncounterId,
            AbilityName = "Overcharged Maul",
            AbilityDescription = "A devastating overcharged strike that stuns the target!",
            AbilityType = "Telegraphed",
            IsTelegraphed = true,
            TelegraphChargeTurns = 1,
            TelegraphWarningMessage = "⚡ The Sentinel's fists crackle with overcharged energy!",
            BaseDamageDice = 4,
            DamageBonus = 8,
            DamageType = "Physical",
            TargetType = "Single",
            AppliesStatusEffects = JsonSerializer.Serialize(new List<StatusEffectDefinition>
            {
                new StatusEffectDefinition { StatusName = "Stunned", Duration = 2 }
            }),
            InterruptDamageThreshold = 20,
            CooldownTurns = 5
        });

        // Ultimate: Omega Protocol
        _repository.CreateBossAbility(new BossAbilityData
        {
            BossEncounterId = boss.BossEncounterId,
            AbilityName = "Omega Protocol",
            AbilityDescription = "💀 OMEGA PROTOCOLS ACTIVE! Maximum power output!",
            AbilityType = "Ultimate",
            IsTelegraphed = true,
            IsUltimate = true,
            TelegraphChargeTurns = 2,
            TelegraphWarningMessage = "🔥 OMEGA PROTOCOL INITIATING! All limiters removed!",
            VulnerabilityDurationTurns = 2,
            VulnerabilityDamageMultiplier = 1.5f,
            BaseDamageDice = 6,
            DamageBonus = 10,
            DamageType = "Physical",
            TargetType = "All",
            AppliesStatusEffects = JsonSerializer.Serialize(new List<StatusEffectDefinition>
            {
                new StatusEffectDefinition { StatusName = "Stunned", Duration = 1 }
            }),
            InterruptDamageThreshold = 40,
            CooldownTurns = 999
        });

        SeedOmegaSentinelAIPatterns(boss.BossEncounterId);
        _log.Debug("Omega Sentinel abilities seeded successfully");
    }

    private void SeedOmegaSentinelAIPatterns(int bossEncounterId)
    {
        // Phase 1: Combat Systems Online
        _repository.CreateBossAIPattern(new BossAIPatternData
        {
            BossEncounterId = bossEncounterId,
            PhaseNumber = 1,
            PatternName = "Combat Systems Online",
            PatternDescription = "Methodical melee assault",
            TargetPriorityList = JsonSerializer.Serialize(new[] { "Closest", "HighestDamage" }),
            PreferredAbilities = JsonSerializer.Serialize(new[] { "Maul Strike", "Seismic Slam" }),
            TelegraphFrequency = 0.15f,
            UltimateHpThreshold = 0.25f,
            PrefersMelee = true,
            PrefersRange = false,
            PositioningStrategy = "Frontline"
        });

        // Phase 2: Overcharged
        _repository.CreateBossAIPattern(new BossAIPatternData
        {
            BossEncounterId = bossEncounterId,
            PhaseNumber = 2,
            PatternName = "Overcharged",
            PatternDescription = "Increased damage with self-healing",
            TargetPriorityList = JsonSerializer.Serialize(new[] { "HighestDamage", "Closest" }),
            PreferredAbilities = JsonSerializer.Serialize(new[] { "Overcharged Maul", "Power Draw", "Seismic Slam" }),
            TelegraphFrequency = 0.3f,
            UltimateHpThreshold = 0.25f,
            PrefersMelee = true,
            PrefersRange = false,
            PositioningStrategy = "Aggressive",
            HealThreshold = 0.35f
        });

        // Phase 3: Omega Protocol
        _repository.CreateBossAIPattern(new BossAIPatternData
        {
            BossEncounterId = bossEncounterId,
            PhaseNumber = 3,
            PatternName = "Omega Protocol",
            PatternDescription = "Maximum aggression and power",
            TargetPriorityList = JsonSerializer.Serialize(new[] { "HighestDamage", "LowestHP", "Closest" }),
            PreferredAbilities = JsonSerializer.Serialize(new[] { "Omega Protocol", "Overcharged Maul", "Seismic Slam" }),
            TelegraphFrequency = 0.5f,
            UltimateHpThreshold = 0.30f,
            PrefersMelee = true,
            PrefersRange = false,
            PositioningStrategy = "Reckless",
            HealThreshold = 0.25f
        });
    }
}
