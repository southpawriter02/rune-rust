using NUnit.Framework;
using RuneAndRust.Core;
using RuneAndRust.Engine;
using RuneAndRust.Persistence;
using System.Text.Json;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.23.1: Integration tests for database-backed boss encounter system
/// Tests multi-phase bosses, add spawning, enrage mechanics, and database persistence
/// </summary>
[TestFixture]
public class BossEncounterTests
{
    private BossEncounterRepository _repository = null!;
    private BossEncounterSeeder _seeder = null!;
    private BossEncounterService _service = null!;
    private DiceService _diceService = null!;
    private string _testDbPath = string.Empty;

    [SetUp]
    public void Setup()
    {
        // Create test database in temp directory
        _testDbPath = Path.Combine(Path.GetTempPath(), $"test_boss_{Guid.NewGuid()}.db");
        var testDir = Path.GetDirectoryName(_testDbPath)!;

        _repository = new BossEncounterRepository(testDir);
        _seeder = new BossEncounterSeeder(_repository);
        _diceService = new DiceService(seed: 42);
        _service = new BossEncounterService(_repository, _diceService);

        // Seed boss encounters
        _seeder.SeedBossEncounters();
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up test database
        if (File.Exists(_testDbPath))
        {
            File.Delete(_testDbPath);
        }
    }

    #region Phase Transition Tests

    [Test]
    public void BossPhaseTransition_TriggersAtCorrectHPThreshold()
    {
        // Arrange
        var boss = CreateTestBoss(maxHP: 100, currentHP: 74); // 74% HP
        var combatState = new CombatState();

        _service.InitializeBossEncounter(boss, encounterId: 1); // Ruin-Warden

        // Act
        var transitionMessage = _service.CheckPhaseTransitions(boss, combatState);

        // Assert
        Assert.That(transitionMessage, Is.Not.Null);
        Assert.That(transitionMessage, Does.Contain("PHASE 2"));
        Assert.That(_service.GetCurrentPhase(boss), Is.EqualTo(2));
    }

    [Test]
    public void BossPhaseTransition_DoesNotTriggerAboveThreshold()
    {
        // Arrange
        var boss = CreateTestBoss(maxHP: 100, currentHP: 76); // 76% HP (above 75% threshold)
        var combatState = new CombatState();

        _service.InitializeBossEncounter(boss, encounterId: 1);

        // Act
        var transitionMessage = _service.CheckPhaseTransitions(boss, combatState);

        // Assert
        Assert.That(transitionMessage, Is.Null);
        Assert.That(_service.GetCurrentPhase(boss), Is.EqualTo(1));
    }

    [Test]
    public void BossPhaseTransition_GrantsInvulnerability()
    {
        // Arrange
        var boss = CreateTestBoss(maxHP: 100, currentHP: 74);
        var combatState = new CombatState();

        _service.InitializeBossEncounter(boss, encounterId: 1);

        // Act
        _service.CheckPhaseTransitions(boss, combatState);

        // Assert
        Assert.That(_service.IsBossInvulnerable(boss), Is.True);
        Assert.That(_service.GetInvulnerabilityTurns(boss), Is.EqualTo(1));
    }

    [Test]
    public void BossInvulnerability_ExpiresAfterTurns()
    {
        // Arrange
        var boss = CreateTestBoss(maxHP: 100, currentHP: 74);
        var combatState = new CombatState();

        _service.InitializeBossEncounter(boss, encounterId: 1);
        _service.CheckPhaseTransitions(boss, combatState);

        // Act - Process end of turn
        _service.ProcessEndOfTurn(boss);

        // Assert
        Assert.That(_service.IsBossInvulnerable(boss), Is.False);
        Assert.That(_service.GetInvulnerabilityTurns(boss), Is.EqualTo(0));
    }

    [Test]
    public void BossPhaseTransition_SpawnsAdds()
    {
        // Arrange
        var boss = CreateTestBoss(maxHP: 100, currentHP: 74);
        var combatState = new CombatState { Enemies = new List<Enemy>() };

        _service.InitializeBossEncounter(boss, encounterId: 1); // Ruin-Warden spawns 2 adds in phase 2

        // Act
        _service.CheckPhaseTransitions(boss, combatState);

        // Assert
        Assert.That(combatState.Enemies.Count, Is.EqualTo(2)); // 2 Corrupted Servitors spawned
    }

    [Test]
    public void MultiPhaseTransition_ShouldProgressThroughAllPhases()
    {
        // Arrange
        var boss = CreateTestBoss(maxHP: 100, currentHP: 100);
        var combatState = new CombatState { Enemies = new List<Enemy>() };

        _service.InitializeBossEncounter(boss, encounterId: 1);

        // Act & Assert: Phase 1 → 2 at 75%
        boss.HP = 74;
        var msg = _service.CheckPhaseTransitions(boss, combatState);
        Assert.That(msg, Is.Not.Null);
        Assert.That(_service.GetCurrentPhase(boss), Is.EqualTo(2));

        // Act & Assert: Phase 2 → 3 at 50%
        boss.HP = 49;
        msg = _service.CheckPhaseTransitions(boss, combatState);
        Assert.That(msg, Is.Not.Null);
        Assert.That(_service.GetCurrentPhase(boss), Is.EqualTo(3));

        // Verify phase 3 is final phase
        boss.HP = 24;
        msg = _service.CheckPhaseTransitions(boss, combatState);
        Assert.That(msg, Is.Null); // No phase 4
        Assert.That(_service.GetCurrentPhase(boss), Is.EqualTo(3));
    }

    #endregion

    #region Enrage Mechanics Tests

    [Test]
    public void BossEnrage_TriggersAt25PercentHP()
    {
        // Arrange
        var boss = CreateTestBoss(maxHP: 100, currentHP: 24); // 24% HP
        _service.InitializeBossEncounter(boss, encounterId: 1);

        // Act
        var enrageMessage = _service.CheckEnrageConditions(boss, currentTurn: 5);

        // Assert
        Assert.That(enrageMessage, Is.Not.Null);
        Assert.That(enrageMessage, Does.Contain("ENRAGE"));
        Assert.That(_service.IsBossEnraged(boss), Is.True);
    }

    [Test]
    public void BossEnrage_DoesNotTriggerAboveThreshold()
    {
        // Arrange
        var boss = CreateTestBoss(maxHP: 100, currentHP: 26); // 26% HP (above 25% threshold)
        _service.InitializeBossEncounter(boss, encounterId: 1);

        // Act
        var enrageMessage = _service.CheckEnrageConditions(boss, currentTurn: 5);

        // Assert
        Assert.That(enrageMessage, Is.Null);
        Assert.That(_service.IsBossEnraged(boss), Is.False);
    }

    [Test]
    public void BossEnrage_IncreasesDamage()
    {
        // Arrange
        var boss = CreateTestBoss(maxHP: 100, currentHP: 24);
        var initialDamageBonus = boss.DamageBonus;

        _service.InitializeBossEncounter(boss, encounterId: 1);

        // Act
        _service.CheckEnrageConditions(boss, currentTurn: 5);

        // Assert
        Assert.That(boss.DamageBonus, Is.GreaterThan(initialDamageBonus));
    }

    #endregion

    #region Add Wave Tests

    [Test]
    public void OnAddKilled_DecrementAddTracking()
    {
        // Arrange
        var boss = CreateTestBoss(maxHP: 100, currentHP: 74);
        var combatState = new CombatState { Enemies = new List<Enemy>() };

        _service.InitializeBossEncounter(boss, encounterId: 1);
        _service.CheckPhaseTransitions(boss, combatState); // Spawns 2 adds

        // Act
        _service.OnAddKilled(boss.Id);

        // Assert - tracking updated (verified in database)
        var bossState = _repository.GetBossCombatState(boss.Id);
        Assert.That(bossState, Is.Not.Null);
        Assert.That(bossState!.CurrentAddsAlive, Is.EqualTo(1));
    }

    #endregion

    #region Regeneration Tests

    [Test]
    public void ProcessRegeneration_RestoresHP()
    {
        // Arrange
        var boss = CreateTestBoss(maxHP: 100, currentHP: 50);

        // Act
        var regenMessage = _service.ProcessRegeneration(boss, regenAmount: 10);

        // Assert
        Assert.That(boss.HP, Is.EqualTo(60));
        Assert.That(regenMessage, Does.Contain("regenerates"));
    }

    [Test]
    public void ProcessRegeneration_DoesNotExceedMaxHP()
    {
        // Arrange
        var boss = CreateTestBoss(maxHP: 100, currentHP: 98);

        // Act
        var regenMessage = _service.ProcessRegeneration(boss, regenAmount: 10);

        // Assert
        Assert.That(boss.HP, Is.EqualTo(100)); // Capped at MaxHP
        Assert.That(regenMessage, Does.Contain("regenerates 2 HP")); // Only healed 2
    }

    #endregion

    #region Database Persistence Tests

    [Test]
    public void BossEncounterRepository_CreateAndRetrieveBoss()
    {
        // Arrange
        var config = new BossEncounterConfig
        {
            EncounterId = 999,
            BossName = "Test Boss",
            BossType = "Test",
            TotalPhases = 2,
            Phase2HpThreshold = 0.5f
        };

        // Act
        int bossId = _repository.CreateBossEncounter(config);
        var retrieved = _repository.GetBossEncounter(bossId);

        // Assert
        Assert.That(retrieved, Is.Not.Null);
        Assert.That(retrieved!.BossName, Is.EqualTo("Test Boss"));
        Assert.That(retrieved.TotalPhases, Is.EqualTo(2));
    }

    [Test]
    public void BossEncounterRepository_CreateAndRetrievePhaseDefinition()
    {
        // Arrange
        var config = new BossEncounterConfig
        {
            EncounterId = 998,
            BossName = "Test Boss 2",
            BossType = "Test",
            TotalPhases = 2
        };
        int bossId = _repository.CreateBossEncounter(config);

        var phase = new BossPhaseDefinitionData
        {
            BossEncounterId = bossId,
            PhaseNumber = 1,
            PhaseName = "Test Phase",
            PhaseDescription = "Test phase description",
            DamageModifier = 1.5f,
            RegenerationPerTurn = 5
        };

        // Act
        _repository.CreatePhaseDefinition(phase);
        var retrieved = _repository.GetPhaseDefinition(bossId, 1);

        // Assert
        Assert.That(retrieved, Is.Not.Null);
        Assert.That(retrieved!.PhaseName, Is.EqualTo("Test Phase"));
        Assert.That(retrieved.DamageModifier, Is.EqualTo(1.5f));
        Assert.That(retrieved.RegenerationPerTurn, Is.EqualTo(5));
    }

    [Test]
    public void BossEncounterRepository_InitializeAndRetrieveCombatState()
    {
        // Arrange
        var boss = CreateTestBoss(maxHP: 100, currentHP: 100);
        _service.InitializeBossEncounter(boss, encounterId: 1);

        // Act
        var combatState = _repository.GetBossCombatState(boss.Id);

        // Assert
        Assert.That(combatState, Is.Not.Null);
        Assert.That(combatState!.EnemyId, Is.EqualTo(boss.Id));
        Assert.That(combatState.CurrentPhase, Is.EqualTo(1));
        Assert.That(combatState.IsEnraged, Is.False);
    }

    [Test]
    public void BossEncounterSeeder_SeedsAllBosses()
    {
        // Assert - verify all 4 bosses were seeded
        var ruinWarden = _repository.GetBossEncounterByEncounterId(1);
        var aberration = _repository.GetBossEncounterByEncounterId(2);
        var archivist = _repository.GetBossEncounterByEncounterId(3);
        var sentinel = _repository.GetBossEncounterByEncounterId(4);

        Assert.That(ruinWarden, Is.Not.Null);
        Assert.That(aberration, Is.Not.Null);
        Assert.That(archivist, Is.Not.Null);
        Assert.That(sentinel, Is.Not.Null);

        Assert.That(ruinWarden!.BossName, Is.EqualTo("Ruin-Warden"));
        Assert.That(aberration!.BossName, Is.EqualTo("Aetheric Aberration"));
        Assert.That(archivist!.BossName, Is.EqualTo("Forlorn Archivist"));
        Assert.That(sentinel!.BossName, Is.EqualTo("Omega Sentinel"));
    }

    #endregion

    #region Boss Configuration Tests

    [Test]
    public void GetBossConfig_ReturnsCorrectConfiguration()
    {
        // Arrange
        var boss = CreateTestBoss(maxHP: 100, currentHP: 100);
        _service.InitializeBossEncounter(boss, encounterId: 1);

        // Act
        var config = _service.GetBossConfig(boss);

        // Assert
        Assert.That(config, Is.Not.Null);
        Assert.That(config!.BossName, Is.EqualTo("Ruin-Warden"));
        Assert.That(config.TotalPhases, Is.EqualTo(3));
        Assert.That(config.EnrageHpThreshold, Is.EqualTo(0.25f));
    }

    [Test]
    public void GetCurrentPhaseDefinition_ReturnsCorrectPhase()
    {
        // Arrange
        var boss = CreateTestBoss(maxHP: 100, currentHP: 74);
        var combatState = new CombatState();

        _service.InitializeBossEncounter(boss, encounterId: 1);
        _service.CheckPhaseTransitions(boss, combatState); // Transition to phase 2

        // Act
        var phaseDef = _service.GetCurrentPhaseDefinition(boss);

        // Assert
        Assert.That(phaseDef, Is.Not.Null);
        Assert.That(phaseDef!.PhaseNumber, Is.EqualTo(2));
        Assert.That(phaseDef.PhaseName, Is.EqualTo("Emergency Protocols"));
        Assert.That(phaseDef.DamageModifier, Is.EqualTo(1.2f));
    }

    #endregion

    #region Helper Methods

    private Enemy CreateTestBoss(int maxHP = 100, int currentHP = 100)
    {
        return new Enemy
        {
            Id = $"boss_{Guid.NewGuid().ToString().Substring(0, 8)}",
            Name = "Test Boss",
            Type = EnemyType.RuinWarden,
            MaxHP = maxHP,
            HP = currentHP,
            IsBoss = true,
            Phase = 1,
            BaseDamageDice = 2,
            DamageBonus = 1,
            Attributes = new Attributes(might: 5, finesse: 3, wits: 2, will: 2, sturdiness: 5)
        };
    }

    #endregion
}
