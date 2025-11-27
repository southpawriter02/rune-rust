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
    private BossAbilitySeeder _abilitySeeder = null!;
    private BossEncounterService _service = null!;
    private TelegraphedAbilityService _telegraphService = null!;
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
        _abilitySeeder = new BossAbilitySeeder(_repository);
        _diceService = new DiceService(seed: 42);
        _service = new BossEncounterService(_repository, _diceService);
        _telegraphService = new TelegraphedAbilityService(_repository, _diceService);

        // Seed boss encounters and abilities
        _seeder.SeedBossEncounters();
        _abilitySeeder.SeedBossAbilities();
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

    #region Telegraphed Ability Tests (v0.23.2)

    [Test]
    public void BossAbilitySeeder_SeedsAllAbilities()
    {
        // Assert - verify abilities were seeded for all bosses
        var ruinWardenConfig = _repository.GetBossEncounterByEncounterId(1);
        Assert.That(ruinWardenConfig, Is.Not.Null);

        var ruinWardenAbilities = _repository.GetBossAbilities(ruinWardenConfig!.BossEncounterId);
        Assert.That(ruinWardenAbilities.Count, Is.GreaterThan(0));
        Assert.That(ruinWardenAbilities.Any(a => a.IsTelegraphed), Is.True);
        Assert.That(ruinWardenAbilities.Any(a => a.IsUltimate), Is.True);
    }

    [Test]
    public void TelegraphedAbility_BeginsCharging()
    {
        // Arrange
        var boss = CreateTestBoss(maxHP: 100, currentHP: 100);
        _service.InitializeBossEncounter(boss, encounterId: 1);

        var abilities = _repository.GetTelegraphedAbilities(
            _repository.GetBossEncounterByEncounterId(1)!.BossEncounterId);
        var telegraphedAbility = abilities.First(a => a.IsTelegraphed && !a.IsUltimate);

        // Act
        var message = _telegraphService.BeginTelegraph(boss, telegraphedAbility, currentTurn: 1);

        // Assert
        Assert.That(message, Is.Not.Null);
        Assert.That(message, Does.Contain("WARNING"));
        Assert.That(message, Does.Contain(telegraphedAbility.AbilityName));

        var activeTelegraphs = _telegraphService.GetActiveTelegraphs(boss.Id);
        Assert.That(activeTelegraphs.Count, Is.EqualTo(1));
        Assert.That(activeTelegraphs[0].IsCharging, Is.True);
    }

    [Test]
    public void TelegraphedAbility_ExecutesAfterChargeTurns()
    {
        // Arrange
        var boss = CreateTestBoss(maxHP: 100, currentHP: 100);
        var player = new PlayerCharacter { Name = "Test Player", HP = 50, MaxHP = 50 };
        var combatState = new CombatState { Enemies = new List<Enemy> { boss } };

        _service.InitializeBossEncounter(boss, encounterId: 1);

        var abilities = _repository.GetTelegraphedAbilities(
            _repository.GetBossEncounterByEncounterId(1)!.BossEncounterId);
        var telegraphedAbility = abilities.First(a => a.IsTelegraphed && !a.IsUltimate);

        // Start charging at turn 1
        _telegraphService.BeginTelegraph(boss, telegraphedAbility, currentTurn: 1);

        // Act - Process turns until ready
        int chargeTurns = telegraphedAbility.TelegraphChargeTurns;
        int executeTurn = 1 + chargeTurns;

        var readyAbilities = _telegraphService.ProcessActiveTelegraphs(
            new List<Enemy> { boss }, executeTurn);

        // Assert
        Assert.That(readyAbilities.Count, Is.EqualTo(1));
        Assert.That(readyAbilities[0].ability.AbilityName, Is.EqualTo(telegraphedAbility.AbilityName));

        // Execute the ability
        var message = _telegraphService.ExecuteTelegraphedAbility(
            boss, telegraphedAbility, player, combatState);

        Assert.That(message, Does.Contain(telegraphedAbility.AbilityName.ToUpper()));
        Assert.That(player.HP, Is.LessThan(50)); // Damage was dealt
    }

    [Test]
    public void TelegraphedAbility_CanBeInterrupted()
    {
        // Arrange
        var boss = CreateTestBoss(maxHP: 100, currentHP: 100);
        _service.InitializeBossEncounter(boss, encounterId: 1);

        var abilities = _repository.GetTelegraphedAbilities(
            _repository.GetBossEncounterByEncounterId(1)!.BossEncounterId);
        var telegraphedAbility = abilities.First(a => a.IsTelegraphed && a.InterruptDamageThreshold > 0);

        // Start charging
        _telegraphService.BeginTelegraph(boss, telegraphedAbility, currentTurn: 1);

        // Act - Deal damage exceeding interrupt threshold
        int damage = telegraphedAbility.InterruptDamageThreshold + 5;
        var interruptMessage = _telegraphService.CheckTelegraphInterrupt(boss, damage);

        // Assert
        Assert.That(interruptMessage, Is.Not.Null);
        Assert.That(interruptMessage, Does.Contain("INTERRUPTED"));
        Assert.That(interruptMessage, Does.Contain(telegraphedAbility.AbilityName));

        var activeTelegraphs = _telegraphService.GetActiveTelegraphs(boss.Id);
        Assert.That(activeTelegraphs.Count, Is.EqualTo(0)); // Telegraph was removed
    }

    [Test]
    public void TelegraphedAbility_AccumulatesInterruptDamage()
    {
        // Arrange
        var boss = CreateTestBoss(maxHP: 100, currentHP: 100);
        _service.InitializeBossEncounter(boss, encounterId: 1);

        var abilities = _repository.GetTelegraphedAbilities(
            _repository.GetBossEncounterByEncounterId(1)!.BossEncounterId);
        var telegraphedAbility = abilities.First(a => a.IsTelegraphed && a.InterruptDamageThreshold > 10);

        _telegraphService.BeginTelegraph(boss, telegraphedAbility, currentTurn: 1);

        // Act - Deal damage in multiple hits
        int threshold = telegraphedAbility.InterruptDamageThreshold;
        _telegraphService.CheckTelegraphInterrupt(boss, threshold / 2);
        _telegraphService.CheckTelegraphInterrupt(boss, threshold / 2);
        var interruptMessage = _telegraphService.CheckTelegraphInterrupt(boss, 5);

        // Assert - Should interrupt after accumulated damage exceeds threshold
        Assert.That(interruptMessage, Is.Not.Null);
        Assert.That(interruptMessage, Does.Contain("INTERRUPTED"));
    }

    [Test]
    public void UltimateAbility_GrantsVulnerabilityWindow()
    {
        // Arrange
        var boss = CreateTestBoss(maxHP: 100, currentHP: 100);
        var player = new PlayerCharacter { Name = "Test Player", HP = 100, MaxHP = 100 };
        var combatState = new CombatState { Enemies = new List<Enemy> { boss } };

        _service.InitializeBossEncounter(boss, encounterId: 1);

        var abilities = _repository.GetBossAbilities(
            _repository.GetBossEncounterByEncounterId(1)!.BossEncounterId);
        var ultimateAbility = abilities.First(a => a.IsUltimate);

        // Act - Execute ultimate
        var message = _telegraphService.ExecuteTelegraphedAbility(
            boss, ultimateAbility, player, combatState);

        // Assert
        Assert.That(message, Does.Contain("VULNERABLE"));
        Assert.That(boss.VulnerableTurnsRemaining, Is.GreaterThan(0));
        Assert.That(boss.VulnerabilityDamageMultiplier, Is.GreaterThan(1.0f));
    }

    [Test]
    public void VulnerabilityWindow_IncreaseDamageTaken()
    {
        // Arrange
        var boss = CreateTestBoss(maxHP: 100, currentHP: 100);
        _telegraphService.ApplyVulnerabilityWindow(boss, duration: 3, damageMultiplier: 1.5f);

        // Act
        var multiplier = _telegraphService.GetVulnerabilityMultiplier(boss);

        // Assert
        Assert.That(multiplier, Is.EqualTo(1.5f));
        Assert.That(boss.VulnerableTurnsRemaining, Is.EqualTo(3));
    }

    [Test]
    public void VulnerabilityWindow_ExpiresAfterTurns()
    {
        // Arrange
        var boss = CreateTestBoss(maxHP: 100, currentHP: 100);
        _telegraphService.ApplyVulnerabilityWindow(boss, duration: 2, damageMultiplier: 1.5f);

        // Act - Process 2 turns
        _telegraphService.ProcessVulnerabilityWindow(boss);
        Assert.That(boss.VulnerableTurnsRemaining, Is.EqualTo(1));

        var expiryMessage = _telegraphService.ProcessVulnerabilityWindow(boss);

        // Assert
        Assert.That(boss.VulnerableTurnsRemaining, Is.EqualTo(0));
        Assert.That(boss.VulnerabilityDamageMultiplier, Is.EqualTo(1.0f));
        Assert.That(expiryMessage, Does.Contain("ended"));
    }

    [Test]
    public void BossAIPatterns_SeededForAllPhases()
    {
        // Arrange
        var ruinWardenConfig = _repository.GetBossEncounterByEncounterId(1);
        Assert.That(ruinWardenConfig, Is.Not.Null);

        // Act
        var aiPatterns = _repository.GetBossAIPatterns(ruinWardenConfig!.BossEncounterId);

        // Assert
        Assert.That(aiPatterns.Count, Is.EqualTo(3)); // 3 phases
        Assert.That(aiPatterns[0].PhaseNumber, Is.EqualTo(1));
        Assert.That(aiPatterns[1].PhaseNumber, Is.EqualTo(2));
        Assert.That(aiPatterns[2].PhaseNumber, Is.EqualTo(3));

        // Phase 3 should have highest telegraph frequency
        Assert.That(aiPatterns[2].TelegraphFrequency, Is.GreaterThan(aiPatterns[0].TelegraphFrequency));
    }

    [Test]
    public void BossAIPattern_RetrievedByPhase()
    {
        // Arrange
        var ruinWardenConfig = _repository.GetBossEncounterByEncounterId(1);
        Assert.That(ruinWardenConfig, Is.Not.Null);

        // Act
        var phase2Pattern = _repository.GetBossAIPattern(ruinWardenConfig!.BossEncounterId, 2);

        // Assert
        Assert.That(phase2Pattern, Is.Not.Null);
        Assert.That(phase2Pattern!.PhaseNumber, Is.EqualTo(2));
        Assert.That(phase2Pattern.PatternName, Is.Not.Empty);
    }

    [Test]
    public void Telegraph_ClearedWhenCombatEnds()
    {
        // Arrange
        var boss = CreateTestBoss(maxHP: 100, currentHP: 100);
        _service.InitializeBossEncounter(boss, encounterId: 1);

        var abilities = _repository.GetTelegraphedAbilities(
            _repository.GetBossEncounterByEncounterId(1)!.BossEncounterId);
        var telegraphedAbility = abilities.First(a => a.IsTelegraphed);

        _telegraphService.BeginTelegraph(boss, telegraphedAbility, currentTurn: 1);

        // Verify telegraph is active
        var activeTelegraphs = _telegraphService.GetActiveTelegraphs(boss.Id);
        Assert.That(activeTelegraphs.Count, Is.EqualTo(1));

        // Act - Clear telegraphs
        _telegraphService.ClearTelegraphs(boss.Id);

        // Assert
        activeTelegraphs = _telegraphService.GetActiveTelegraphs(boss.Id);
        Assert.That(activeTelegraphs.Count, Is.EqualTo(0));
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
