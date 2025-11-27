using NUnit.Framework;
using RuneAndRust.Core;
using RuneAndRust.Engine;
using RuneAndRust.Persistence;
using Serilog;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.23 Integration Tests: Full boss encounter system workflow
/// Tests the complete integration of phases, abilities, telegraphing, and loot
/// </summary>
[TestFixture]
public class BossSystemIntegrationTests
{
    private BossEncounterRepository _repository = null!;
    private BossEncounterService _encounterService = null!;
    private TelegraphedAbilityService _telegraphService = null!;
    private BossLootService _lootService = null!;
    private DiceService _diceService = null!;
    private string _testDbPath = string.Empty;

    [SetUp]
    public void Setup()
    {
        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateLogger();

        // Create test database
        _testDbPath = Path.Combine(Path.GetTempPath(), $"test_boss_integration_{Guid.NewGuid()}.db");
        var testDir = Path.GetDirectoryName(_testDbPath)!;

        _repository = new BossEncounterRepository(testDir);
        _diceService = new DiceService(seed: 42);
        _encounterService = new BossEncounterService(_repository, _diceService);
        _telegraphService = new TelegraphedAbilityService(_repository, _diceService);
        _lootService = new BossLootService(_repository, _diceService);

        // Seed all boss data using master seeder
        var masterSeeder = new BossMasterSeeder(_repository);
        masterSeeder.SeedAllBossData();
    }

    [TearDown]
    public void TearDown()
    {
        if (File.Exists(_testDbPath))
        {
            File.Delete(_testDbPath);
        }
    }

    // PPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP
    // MASTER SEEDER TESTS
    // PPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP

    [Test]
    public void MasterSeeder_SeedsAllData()
    {
        // Act
        var masterSeeder = new BossMasterSeeder(_repository);
        var validation = masterSeeder.ValidateSeedData();

        // Assert
        Assert.That(validation.IsValid, Is.True, "Validation should pass");
        Assert.That(validation.Errors.Count, Is.EqualTo(0), "Should have no errors");
        Assert.That(validation.BossEncountersValidated, Is.EqualTo(4), "Should validate 4 boss encounters");
        Assert.That(validation.PhasesValidated, Is.EqualTo(12), "Should validate 12 phases (3 per boss)");
        Assert.That(validation.AbilitiesValidated, Is.GreaterThan(30), "Should have 30+ abilities");
        Assert.That(validation.LootTablesValidated, Is.EqualTo(4), "Should have 4 loot tables");
        Assert.That(validation.ArtifactsValidated, Is.GreaterThan(12), "Should have 12+ artifacts");
    }

    // PPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP
    // FULL BOSS ENCOUNTER WORKFLOW
    // PPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP

    [Test]
    public void FullWorkflow_RuinWardenEncounter()
    {
        // Arrange: Create boss and player
        var boss = new Enemy
        {
            Id = "boss_ruin_warden",
            Name = "Ruin-Warden",
            Type = EnemyType.Boss,
            IsBoss = true,
            HP = 100,
            MaxHP = 100,
            Defense = 2,
            DamageBonus = 2
        };

        var player = new PlayerCharacter
        {
            Id = "player_test",
            Name = "Test Player",
            HP = 50,
            MaxHP = 50,
            Stamina = 20,
            MaxStamina = 20
        };

        var combatState = new CombatState
        {
            Enemies = new List<Enemy> { boss }
        };

        // Act: Initialize boss encounter (v0.23.1)
        _encounterService.InitializeBossEncounter(boss, encounterId: 1);

        // Assert: Boss is initialized
        Assert.That(boss.CurrentPhase, Is.EqualTo(1), "Boss should start in phase 1");
        Assert.That(boss.BossEncounterId, Is.GreaterThan(0), "Boss encounter ID should be set");

        // Act: Trigger phase 2 (75% HP)
        boss.HP = 70; // 70% HP
        var phase2Message = _encounterService.CheckPhaseTransition(boss);

        // Assert: Phase 2 triggered
        Assert.That(phase2Message, Is.Not.Null, "Should trigger phase 2");
        Assert.That(boss.CurrentPhase, Is.EqualTo(2), "Boss should be in phase 2");
        Assert.That(phase2Message, Does.Contain("Emergency Protocols"), "Should show phase 2 name");

        // Act: Get boss abilities for phase 2
        var abilities = _repository.GetBossAbilities(boss.BossEncounterId!.Value);
        var telegraphedAbilities = abilities.Where(a => a.IsTelegraphed && a.PhaseNumber == 2).ToList();

        Assert.That(telegraphedAbilities.Count, Is.GreaterThan(0), "Phase 2 should have telegraphed abilities");

        // Act: Start telegraphing an ability (v0.23.2)
        var telegraphedAbility = telegraphedAbilities.First();
        var telegraphMessage = _telegraphService.BeginTelegraph(boss, telegraphedAbility, currentTurn: 1);

        // Assert: Telegraph started
        Assert.That(telegraphMessage, Does.Contain("WARNING"), "Should show telegraph warning");
        Assert.That(telegraphMessage, Does.Contain(telegraphedAbility.AbilityName), "Should mention ability name");

        // Act: Process turns until telegraph executes
        int executeTurn = 1 + telegraphedAbility.TelegraphChargeTurns;
        var readyAbilities = _telegraphService.ProcessActiveTelegraphs(new List<Enemy> { boss }, executeTurn);

        // Assert: Ability is ready
        Assert.That(readyAbilities.Count, Is.EqualTo(1), "Ability should be ready to execute");

        // Act: Execute telegraphed ability
        var executeMessage = _telegraphService.ExecuteTelegraphedAbility(boss, telegraphedAbility, player, combatState);

        // Assert: Ability executed
        Assert.That(executeMessage, Does.Contain(telegraphedAbility.AbilityName.ToUpper()), "Should show ability name");
        Assert.That(player.HP, Is.LessThan(50), "Player should take damage");

        // Act: Trigger phase 3 (50% HP)
        boss.HP = 45; // 45% HP
        var phase3Message = _encounterService.CheckPhaseTransition(boss);

        // Assert: Phase 3 triggered
        Assert.That(phase3Message, Is.Not.Null, "Should trigger phase 3");
        Assert.That(boss.CurrentPhase, Is.EqualTo(3), "Boss should be in phase 3");

        // Act: Trigger enrage (25% HP)
        boss.HP = 20; // 20% HP
        var enrageMessage = _encounterService.CheckEnrageCondition(boss);

        // Assert: Enrage triggered
        Assert.That(enrageMessage, Is.Not.Null, "Should trigger enrage");
        Assert.That(boss.IsEnraged, Is.True, "Boss should be enraged");

        // Act: Defeat boss and generate loot (v0.23.3)
        boss.HP = 0;
        var lootResult = _lootService.GenerateBossLoot(boss.BossEncounterId.Value, player.Id, bossTdr: 50);

        // Assert: Loot generated
        Assert.That(lootResult.Items.Count, Is.GreaterThan(0), "Should generate loot");
        Assert.That(lootResult.SilverMarks, Is.InRange(150, 300), "Should generate silver marks");
        Assert.That(lootResult.CraftingMaterials.Count, Is.GreaterThan(0), "Should generate crafting materials");
        Assert.That(lootResult.LogMessage, Does.Contain("BOSS LOOT"), "Should contain loot message");

        // Check for unique item (Warden's Core Fragment)
        var uniqueItem = lootResult.Items.FirstOrDefault(i => i.IsUnique);
        Assert.That(uniqueItem, Is.Not.Null, "Should drop unique item on first kill");
        Assert.That(uniqueItem!.ItemName, Does.Contain("Warden"), "Should be Warden-specific unique");

        // Act: Kill boss again with same character
        var secondLootResult = _lootService.GenerateBossLoot(boss.BossEncounterId.Value, player.Id, bossTdr: 50);

        // Assert: Unique item not dropped again
        var secondUniqueItem = secondLootResult.Items.FirstOrDefault(i => i.IsUnique);
        Assert.That(secondUniqueItem, Is.Null, "Should not drop unique item again for same character");
    }

    // PPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP
    // TELEGRAPH INTERRUPTION WORKFLOW
    // PPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP

    [Test]
    public void FullWorkflow_TelegraphInterruption()
    {
        // Arrange
        var boss = new Enemy
        {
            Id = "boss_test_interrupt",
            Name = "Test Boss",
            Type = EnemyType.Boss,
            IsBoss = true,
            HP = 100,
            MaxHP = 100
        };

        _encounterService.InitializeBossEncounter(boss, encounterId: 1);

        // Get interruptible ability
        var abilities = _repository.GetBossAbilities(boss.BossEncounterId!.Value);
        var interruptibleAbility = abilities.FirstOrDefault(a => a.IsTelegraphed && a.InterruptDamageThreshold > 0);

        Assert.That(interruptibleAbility, Is.Not.Null, "Should have interruptible ability");

        // Act: Start telegraph
        _telegraphService.BeginTelegraph(boss, interruptibleAbility!, currentTurn: 1);

        // Act: Deal damage to interrupt
        int damageNeeded = interruptibleAbility!.InterruptDamageThreshold + 5;
        var interruptMessage = _telegraphService.CheckTelegraphInterrupt(boss, damageNeeded);

        // Assert: Telegraph interrupted
        Assert.That(interruptMessage, Is.Not.Null, "Should interrupt telegraph");
        Assert.That(interruptMessage, Does.Contain("INTERRUPTED"), "Should show interrupt message");
        Assert.That(boss.StaggeredTurnsRemaining, Is.GreaterThan(0), "Boss should be staggered");

        var activeTelegraphs = _telegraphService.GetActiveTelegraphs(boss.Id);
        Assert.That(activeTelegraphs.Count, Is.EqualTo(0), "Telegraph should be cleared");
    }

    // PPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP
    // ULTIMATE ABILITY VULNERABILITY WORKFLOW
    // PPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP

    [Test]
    public void FullWorkflow_UltimateVulnerability()
    {
        // Arrange
        var boss = new Enemy
        {
            Id = "boss_test_ultimate",
            Name = "Test Boss",
            Type = EnemyType.Boss,
            IsBoss = true,
            HP = 100,
            MaxHP = 100
        };

        var player = new PlayerCharacter
        {
            Id = "player_test",
            Name = "Test Player",
            HP = 100,
            MaxHP = 100
        };

        var combatState = new CombatState { Enemies = new List<Enemy> { boss } };

        _encounterService.InitializeBossEncounter(boss, encounterId: 1);

        // Get ultimate ability
        var abilities = _repository.GetBossAbilities(boss.BossEncounterId!.Value);
        var ultimateAbility = abilities.FirstOrDefault(a => a.IsUltimate);

        Assert.That(ultimateAbility, Is.Not.Null, "Should have ultimate ability");

        // Act: Execute ultimate
        var executeMessage = _telegraphService.ExecuteTelegraphedAbility(boss, ultimateAbility!, player, combatState);

        // Assert: Vulnerability window applied
        Assert.That(executeMessage, Does.Contain("VULNERABLE"), "Should show vulnerability message");
        Assert.That(boss.VulnerableTurnsRemaining, Is.GreaterThan(0), "Boss should be vulnerable");
        Assert.That(boss.VulnerabilityDamageMultiplier, Is.GreaterThan(1.0f), "Should have damage multiplier");

        // Act: Get vulnerability multiplier
        float multiplier = _telegraphService.GetVulnerabilityMultiplier(boss);

        // Assert: Multiplier active
        Assert.That(multiplier, Is.EqualTo(boss.VulnerabilityDamageMultiplier), "Multiplier should match");

        // Act: Process turns until vulnerability expires
        int initialTurns = boss.VulnerableTurnsRemaining;
        for (int i = 0; i < initialTurns; i++)
        {
            _telegraphService.ProcessVulnerabilityWindow(boss);
        }

        // Assert: Vulnerability expired
        Assert.That(boss.VulnerableTurnsRemaining, Is.EqualTo(0), "Vulnerability should expire");
        Assert.That(boss.VulnerabilityDamageMultiplier, Is.EqualTo(1.0f), "Multiplier should reset");
    }

    // PPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP
    // ARTIFACT SET BONUS WORKFLOW
    // PPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP

    [Test]
    public void FullWorkflow_ArtifactSetCollection()
    {
        // Arrange: Defeat multiple bosses to collect set pieces
        var player = new PlayerCharacter
        {
            Id = "collector_player",
            Name = "Set Collector"
        };

        var guardianArtifacts = new List<GeneratedItem>();

        // Act: Kill Ruin-Warden for Guardian's Aegis pieces
        var ruinWardenLoot = _lootService.GenerateBossLoot(1, player.Id, bossTdr: 50);
        guardianArtifacts.AddRange(ruinWardenLoot.Items.Where(i => i.SetName == "Guardian's Aegis"));

        // Act: Kill Omega Sentinel for more Guardian's Aegis pieces
        var omegaLoot = _lootService.GenerateBossLoot(4, player.Id, bossTdr: 100);
        guardianArtifacts.AddRange(omegaLoot.Items.Where(i => i.SetName == "Guardian's Aegis"));

        // Assert: Can collect set pieces from multiple bosses
        _log.Information("Collected {Count} Guardian's Aegis pieces", guardianArtifacts.Count);

        // Act: Get set bonuses for Guardian's Aegis
        var setBonuses = _repository.GetSetBonuses("Guardian's Aegis");

        // Assert: Set bonuses exist
        Assert.That(setBonuses.Count, Is.EqualTo(2), "Should have 2 set bonuses (2pc/4pc)");
        Assert.That(setBonuses[0].PiecesRequired, Is.EqualTo(2), "First bonus requires 2 pieces");
        Assert.That(setBonuses[1].PiecesRequired, Is.EqualTo(4), "Second bonus requires 4 pieces");
        Assert.That(setBonuses[0].BonusName, Is.EqualTo("Fortified Stance"), "2pc bonus name");
        Assert.That(setBonuses[1].BonusName, Is.EqualTo("Unwavering Wall"), "4pc bonus name");
    }

    // PPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP
    // MULTI-BOSS ENCOUNTER WORKFLOW
    // PPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP

    [Test]
    public void FullWorkflow_AllFourBosses()
    {
        // Arrange
        var player = new PlayerCharacter
        {
            Id = "legendary_hunter",
            Name = "Boss Hunter"
        };

        var bossResults = new List<(string bossName, LootGenerationResult loot)>();

        // Act: Defeat all 4 bosses
        for (int encounterId = 1; encounterId <= 4; encounterId++)
        {
            var encounter = _repository.GetBossEncounterByEncounterId(encounterId);
            Assert.That(encounter, Is.Not.Null, $"Encounter {encounterId} should exist");

            int bossTdr = encounterId switch
            {
                1 => 50,  // Ruin-Warden
                2 => 55,  // Aetheric Aberration
                3 => 60,  // Forlorn Archivist
                4 => 100, // Omega Sentinel
                _ => 50
            };

            var loot = _lootService.GenerateBossLoot(encounterId, player.Id, bossTdr);
            bossResults.Add((encounter!.BossName, loot));
        }

        // Assert: All bosses dropped loot
        Assert.That(bossResults.Count, Is.EqualTo(4), "Should have loot from 4 bosses");

        foreach (var (bossName, loot) in bossResults)
        {
            Assert.That(loot.Items.Count, Is.GreaterThan(0), $"{bossName} should drop items");
            Assert.That(loot.SilverMarks, Is.GreaterThan(0), $"{bossName} should drop silver marks");
            _log.Information("{BossName}: {ItemCount} items, {Silver} silver marks",
                bossName, loot.Items.Count, loot.SilverMarks);
        }

        // Assert: Omega Sentinel has better loot
        var omegaLoot = bossResults.Last().loot;
        var ruinWardenLoot = bossResults.First().loot;

        Assert.That(omegaLoot.SilverMarks, Is.GreaterThan(ruinWardenLoot.SilverMarks),
            "Omega Sentinel should drop more silver than Ruin-Warden");

        // Assert: Each boss dropped unique item
        var uniqueCount = bossResults.Sum(r => r.loot.Items.Count(i => i.IsUnique));
        Assert.That(uniqueCount, Is.EqualTo(4), "Should have 4 unique items (one per boss)");

        // Assert: Collected artifacts from multiple sets
        var allArtifacts = bossResults.SelectMany(r => r.loot.Items.Where(i => !string.IsNullOrEmpty(i.SetName))).ToList();
        var distinctSets = allArtifacts.Select(a => a.SetName).Distinct().ToList();

        _log.Information("Collected artifacts from {SetCount} different sets", distinctSets.Count);
        foreach (var setName in distinctSets)
        {
            var setCount = allArtifacts.Count(a => a.SetName == setName);
            _log.Information("  {SetName}: {Count} pieces", setName, setCount);
        }
    }

    // PPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP
    // PHASE TRANSITION WITH ADDS WORKFLOW
    // PPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP

    [Test]
    public void FullWorkflow_PhaseTransitionWithAdds()
    {
        // Arrange
        var boss = new Enemy
        {
            Id = "boss_spawner",
            Name = "Ruin-Warden",
            Type = EnemyType.Boss,
            IsBoss = true,
            HP = 100,
            MaxHP = 100
        };

        var combatState = new CombatState
        {
            Enemies = new List<Enemy> { boss }
        };

        _encounterService.InitializeBossEncounter(boss, encounterId: 1);

        int initialEnemyCount = combatState.Enemies.Count;

        // Act: Trigger phase 2 (should spawn adds)
        boss.HP = 70;
        var transitionMessage = _encounterService.ProcessPhaseTransition(boss, combatState);

        // Assert: Phase 2 triggered
        Assert.That(boss.CurrentPhase, Is.EqualTo(2), "Should be in phase 2");
        Assert.That(transitionMessage, Does.Contain("Emergency Protocols"), "Should show phase 2 message");

        // Assert: Adds spawned
        Assert.That(combatState.Enemies.Count, Is.GreaterThan(initialEnemyCount),
            "Phase 2 should spawn adds");

        var adds = combatState.Enemies.Where(e => e.Type == EnemyType.CorruptedServitor).ToList();
        Assert.That(adds.Count, Is.EqualTo(2), "Should spawn 2 Corrupted Servitors");

        _log.Information("Phase 2 spawned {Count} adds: {Adds}",
            adds.Count, string.Join(", ", adds.Select(a => a.Name)));
    }
}
