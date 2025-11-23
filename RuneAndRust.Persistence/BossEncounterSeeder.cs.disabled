using RuneAndRust.Core;
using Serilog;
using System.Text.Json;

namespace RuneAndRust.Persistence;

/// <summary>
/// v0.23.1: Seeds the database with boss encounter configurations
/// Converts BossDatabase definitions into persistent database records
/// </summary>
public class BossEncounterSeeder
{
    private static readonly ILogger _log = Log.ForContext<BossEncounterSeeder>();
    private readonly BossEncounterRepository _repository;

    public BossEncounterSeeder(BossEncounterRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Seed all boss encounters into the database
    /// Call this on application startup or database initialization
    /// </summary>
    public void SeedBossEncounters()
    {
        _log.Information("Seeding boss encounters into database");

        try
        {
            SeedRuinWardenBoss();
            SeedAethericAberrationBoss();
            SeedForlornArchivistBoss();
            SeedOmegaSentinelBoss();

            _log.Information("Boss encounter seeding completed successfully");
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to seed boss encounters");
            throw;
        }
    }

    // ═══════════════════════════════════════════════════════════
    // RUIN-WARDEN BOSS
    // ═══════════════════════════════════════════════════════════

    private void SeedRuinWardenBoss()
    {
        _log.Debug("Seeding Ruin-Warden boss");

        // Create boss encounter configuration
        var config = new BossEncounterConfig
        {
            EncounterId = 1, // Ruin-Warden encounter ID
            BossName = "Ruin-Warden",
            BossType = "Sector Boss",
            TotalPhases = 3,
            Phase2HpThreshold = 0.75f,
            Phase3HpThreshold = 0.50f,
            TransitionInvulnerabilityTurns = 1,
            EnrageTurnThreshold = null,
            EnrageHpThreshold = 0.25f,
            EnrageDamageMultiplier = 1.5f,
            EnrageSpeedBonus = 1
        };

        int bossEncounterId = _repository.CreateBossEncounter(config);

        // Phase 1: 100%-75% HP
        var phase1 = new BossPhaseDefinitionData
        {
            BossEncounterId = bossEncounterId,
            PhaseNumber = 1,
            PhaseName = "Defensive Protocol",
            PhaseDescription = "The Ruin-Warden initiates combat protocols. Shields online.",
            SpawnsAdds = false,
            DamageModifier = 1.0f,
            DefenseModifier = 0,
            SoakModifier = 0,
            RegenerationPerTurn = 0,
            BonusActionsPerTurn = 0
        };
        _repository.CreatePhaseDefinition(phase1);

        // Phase 2: 75%-50% HP
        var phase2 = new BossPhaseDefinitionData
        {
            BossEncounterId = bossEncounterId,
            PhaseNumber = 2,
            PhaseName = "Emergency Protocols",
            PhaseDescription = "⚡ The Ruin-Warden's systems overload! Emergency protocols activated!",
            SpawnsAdds = true,
            AddWaveComposition = JsonSerializer.Serialize(new List<AddSpawnDefinition>
            {
                new AddSpawnDefinition { EnemyType = EnemyType.CorruptedServitor, Count = 2 }
            }),
            DamageModifier = 1.2f,
            DefenseModifier = 2,
            SoakModifier = 0,
            RegenerationPerTurn = 3,
            BonusActionsPerTurn = 0
        };
        _repository.CreatePhaseDefinition(phase2);

        // Phase 3: 50%-0% HP (Desperate)
        var phase3 = new BossPhaseDefinitionData
        {
            BossEncounterId = bossEncounterId,
            PhaseNumber = 3,
            PhaseName = "Critical State",
            PhaseDescription = "💀 The Ruin-Warden enters CRITICAL STATE! All systems overclocked!",
            SpawnsAdds = false,
            DamageModifier = 1.5f,
            DefenseModifier = 3,
            SoakModifier = 0,
            RegenerationPerTurn = 5,
            BonusActionsPerTurn = 1
        };
        _repository.CreatePhaseDefinition(phase3);

        _log.Debug("Ruin-Warden boss seeded successfully");
    }

    // ═══════════════════════════════════════════════════════════
    // AETHERIC ABERRATION BOSS
    // ═══════════════════════════════════════════════════════════

    private void SeedAethericAberrationBoss()
    {
        _log.Debug("Seeding Aetheric Aberration boss");

        var config = new BossEncounterConfig
        {
            EncounterId = 2,
            BossName = "Aetheric Aberration",
            BossType = "Sector Boss",
            TotalPhases = 3,
            Phase2HpThreshold = 0.75f,
            Phase3HpThreshold = 0.50f,
            TransitionInvulnerabilityTurns = 1,
            EnrageTurnThreshold = null,
            EnrageHpThreshold = 0.25f,
            EnrageDamageMultiplier = 1.5f,
            EnrageSpeedBonus = 1
        };

        int bossEncounterId = _repository.CreateBossEncounter(config);

        // Phase 1
        var phase1 = new BossPhaseDefinitionData
        {
            BossEncounterId = bossEncounterId,
            PhaseNumber = 1,
            PhaseName = "Manifestation",
            PhaseDescription = "The Aetheric Aberration materializes, reality warping around it.",
            SpawnsAdds = false,
            DamageModifier = 1.0f,
            DefenseModifier = 0,
            RegenerationPerTurn = 0,
            BonusActionsPerTurn = 0
        };
        _repository.CreatePhaseDefinition(phase1);

        // Phase 2
        var phase2 = new BossPhaseDefinitionData
        {
            BossEncounterId = bossEncounterId,
            PhaseNumber = 2,
            PhaseName = "Reality Tear",
            PhaseDescription = "⚡ The Aberration's form destabilizes! Reality tears open!",
            SpawnsAdds = true,
            AddWaveComposition = JsonSerializer.Serialize(new List<AddSpawnDefinition>
            {
                new AddSpawnDefinition { EnemyType = EnemyType.BlightDrone, Count = 1 }
            }),
            DamageModifier = 1.3f,
            DefenseModifier = 0,
            RegenerationPerTurn = 0,
            BonusActionsPerTurn = 0
        };
        _repository.CreatePhaseDefinition(phase2);

        // Phase 3
        var phase3 = new BossPhaseDefinitionData
        {
            BossEncounterId = bossEncounterId,
            PhaseNumber = 3,
            PhaseName = "Desperate Void",
            PhaseDescription = "💀 The Aberration becomes DESPERATE! The veil between worlds shatters!",
            SpawnsAdds = false,
            DamageModifier = 1.6f,
            DefenseModifier = 0,
            RegenerationPerTurn = 4,
            BonusActionsPerTurn = 0
        };
        _repository.CreatePhaseDefinition(phase3);

        _log.Debug("Aetheric Aberration boss seeded successfully");
    }

    // ═══════════════════════════════════════════════════════════
    // FORLORN ARCHIVIST BOSS
    // ═══════════════════════════════════════════════════════════

    private void SeedForlornArchivistBoss()
    {
        _log.Debug("Seeding Forlorn Archivist boss");

        var config = new BossEncounterConfig
        {
            EncounterId = 3,
            BossName = "Forlorn Archivist",
            BossType = "Sector Boss",
            TotalPhases = 3,
            Phase2HpThreshold = 0.75f,
            Phase3HpThreshold = 0.50f,
            TransitionInvulnerabilityTurns = 1,
            EnrageTurnThreshold = null,
            EnrageHpThreshold = 0.25f,
            EnrageDamageMultiplier = 1.5f,
            EnrageSpeedBonus = 1
        };

        int bossEncounterId = _repository.CreateBossEncounter(config);

        // Phase 1
        var phase1 = new BossPhaseDefinitionData
        {
            BossEncounterId = bossEncounterId,
            PhaseNumber = 1,
            PhaseName = "Psychic Whispers",
            PhaseDescription = "The Forlorn Archivist raises its hands. Psychic whispers fill the air.",
            SpawnsAdds = false,
            DamageModifier = 1.0f,
            DefenseModifier = 0,
            RegenerationPerTurn = 0,
            BonusActionsPerTurn = 0
        };
        _repository.CreatePhaseDefinition(phase1);

        // Phase 2
        var phase2 = new BossPhaseDefinitionData
        {
            BossEncounterId = bossEncounterId,
            PhaseNumber = 2,
            PhaseName = "Psychic Intensification",
            PhaseDescription = "⚡ The Archivist's psychic aura intensifies! The dead stir!",
            SpawnsAdds = true,
            AddWaveComposition = JsonSerializer.Serialize(new List<AddSpawnDefinition>
            {
                new AddSpawnDefinition { EnemyType = EnemyType.CorruptedServitor, Count = 2 }
            }),
            DamageModifier = 1.25f,
            DefenseModifier = 0,
            RegenerationPerTurn = 0,
            BonusActionsPerTurn = 0
        };
        _repository.CreatePhaseDefinition(phase2);

        // Phase 3
        var phase3 = new BossPhaseDefinitionData
        {
            BossEncounterId = bossEncounterId,
            PhaseNumber = 3,
            PhaseName = "Madness Unleashed",
            PhaseDescription = "💀 MADNESS! The Archivist's mind fractures, releasing psychic storm!",
            SpawnsAdds = false,
            DamageModifier = 1.5f,
            DefenseModifier = 0,
            RegenerationPerTurn = 0,
            BonusActionsPerTurn = 1
        };
        _repository.CreatePhaseDefinition(phase3);

        _log.Debug("Forlorn Archivist boss seeded successfully");
    }

    // ═══════════════════════════════════════════════════════════
    // OMEGA SENTINEL BOSS
    // ═══════════════════════════════════════════════════════════

    private void SeedOmegaSentinelBoss()
    {
        _log.Debug("Seeding Omega Sentinel boss");

        var config = new BossEncounterConfig
        {
            EncounterId = 4,
            BossName = "Omega Sentinel",
            BossType = "World Boss",
            TotalPhases = 3,
            Phase2HpThreshold = 0.75f,
            Phase3HpThreshold = 0.50f,
            TransitionInvulnerabilityTurns = 2,
            EnrageTurnThreshold = null,
            EnrageHpThreshold = 0.25f,
            EnrageDamageMultiplier = 1.5f,
            EnrageSpeedBonus = 1
        };

        int bossEncounterId = _repository.CreateBossEncounter(config);

        // Phase 1
        var phase1 = new BossPhaseDefinitionData
        {
            BossEncounterId = bossEncounterId,
            PhaseNumber = 1,
            PhaseName = "Combat Systems Online",
            PhaseDescription = "The Omega Sentinel powers up. Combat systems online.",
            SpawnsAdds = false,
            DamageModifier = 1.0f,
            DefenseModifier = 0,
            SoakModifier = 0,
            RegenerationPerTurn = 0,
            BonusActionsPerTurn = 0
        };
        _repository.CreatePhaseDefinition(phase1);

        // Phase 2
        var phase2 = new BossPhaseDefinitionData
        {
            BossEncounterId = bossEncounterId,
            PhaseNumber = 2,
            PhaseName = "Overcharged",
            PhaseDescription = "⚡ The Sentinel overcharges! Hydraulics strain!",
            SpawnsAdds = false,
            DamageModifier = 1.3f,
            DefenseModifier = 3,
            SoakModifier = 2,
            RegenerationPerTurn = 0,
            BonusActionsPerTurn = 0
        };
        _repository.CreatePhaseDefinition(phase2);

        // Phase 3
        var phase3 = new BossPhaseDefinitionData
        {
            BossEncounterId = bossEncounterId,
            PhaseNumber = 3,
            PhaseName = "Omega Protocol",
            PhaseDescription = "💀 OMEGA PROTOCOLS ACTIVE! Maximum power!",
            SpawnsAdds = false,
            DamageModifier = 1.6f,
            DefenseModifier = 5,
            SoakModifier = 3,
            RegenerationPerTurn = 0,
            BonusActionsPerTurn = 0
        };
        _repository.CreatePhaseDefinition(phase3);

        _log.Debug("Omega Sentinel boss seeded successfully");
    }
}
