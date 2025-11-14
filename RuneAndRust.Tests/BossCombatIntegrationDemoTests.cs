using NUnit.Framework;
using RuneAndRust.Core;
using RuneAndRust.Engine;
using RuneAndRust.Persistence;
using Serilog;

namespace RuneAndRust.Tests;

/// <summary>
/// Demonstration tests showing boss combat integration workflow
/// These tests serve as examples of how to use BossCombatIntegration
/// </summary>
[TestFixture]
public class BossCombatIntegrationDemoTests
{
    private BossEncounterRepository _repository = null!;
    private BossCombatIntegration _bossIntegration = null!;
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
        _testDbPath = Path.Combine(Path.GetTempPath(), $"test_boss_combat_demo_{Guid.NewGuid()}.db");
        var testDir = Path.GetDirectoryName(_testDbPath)!;

        _repository = new BossEncounterRepository(testDir);
        _diceService = new DiceService(seed: 42);
        _bossIntegration = new BossCombatIntegration(_repository, _diceService);

        // Seed all boss data
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
    // DEMONSTRATION TEST: Complete Boss Combat Flow
    // PPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP

    [Test]
    public void Demo_CompleteBossCombatWorkflow()
    {
        // This test demonstrates a complete boss combat from start to finish
        _log.Information("TPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPW");
        _log.Information("Q DEMONSTRATION: Complete Boss Combat Workflow");
        _log.Information("ZPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP]");

        // === STEP 1: Create Combat State ===
        _log.Information("STEP 1: Creating combat state with boss enemy");

        var player = new PlayerCharacter
        {
            Id = "demo_player",
            Name = "Brave Hero",
            HP = 80,
            MaxHP = 80,
            Stamina = 20,
            MaxStamina = 20,
            Attributes = new AttributeSet
            {
                Might = 4,
                Finesse = 3,
                Wits = 3,
                Will = 2,
                Sturdiness = 4
            }
        };

        var boss = new Enemy
        {
            Id = "demo_ruin_warden",
            Name = "Ruin-Warden",
            Type = EnemyType.RuinWarden,
            IsBoss = true,
            HP = 100,
            MaxHP = 100,
            Defense = 2,
            Soak = 1,
            DamageBonus = 3,
            Attributes = new AttributeSet
            {
                Might = 5,
                Finesse = 2,
                Wits = 2,
                Will = 3,
                Sturdiness = 5
            }
        };

        var combatState = new CombatState
        {
            Player = player,
            Enemies = new List<Enemy> { boss },
            IsActive = true,
            TurnNumber = 1
        };

        // === STEP 2: Initialize Boss Encounter ===
        _log.Information("STEP 2: Initializing boss encounter");

        _bossIntegration.InitializeBossEncounters(combatState);

        Assert.That(boss.BossEncounterId, Is.Not.Null, "Boss should be initialized with encounter ID");
        Assert.That(boss.CurrentPhase, Is.EqualTo(1), "Boss should start in phase 1");
        _log.Information($"Boss initialized: Phase={boss.CurrentPhase}, EncounterId={boss.BossEncounterId}");

        // Log combat initialization messages
        foreach (var message in combatState.CombatLog.Skip(combatState.CombatLog.Count - 5))
        {
            _log.Information($"  {message}");
        }

        // === STEP 3: Simulate Combat Turns ===
        _log.Information("");
        _log.Information("STEP 3: Simulating combat turns");

        int currentTurn = 1;
        bool bossTelegraphing = false;

        // Turn 1: Boss decides to telegraph
        _log.Information($"--- Turn {currentTurn} ---");

        var abilityToTelegraph = _bossIntegration.ShouldBossTelegraph(boss, currentTurn);
        if (abilityToTelegraph != null)
        {
            _log.Information($"Boss is telegraphing: {abilityToTelegraph.AbilityName}");
            _bossIntegration.BeginBossTelegraph(combatState, boss, abilityToTelegraph, currentTurn);
            bossTelegraphing = true;

            // Display telegraph warning from combat log
            var lastMessage = combatState.CombatLog.Last();
            _log.Information($"  Telegraph Warning: {lastMessage.Substring(0, Math.Min(100, lastMessage.Length))}...");
        }

        // Display active telegraphs
        var telegraphs = _bossIntegration.GetActiveTelegraphsDisplay(combatState);
        if (telegraphs.Any())
        {
            _log.Information("Active Telegraphs:");
            foreach (var telegraph in telegraphs)
            {
                _log.Information($"  {telegraph}");
            }
        }

        currentTurn++;

        // Turn 2: Player attacks, deals damage
        _log.Information($"--- Turn {currentTurn} ---");
        _log.Information("Player attacks boss");

        int damageDealt = 15;
        boss.HP -= damageDealt;

        _log.Information($"Dealt {damageDealt} damage to boss (HP: {boss.HP}/{boss.MaxHP})");

        // Check for interrupt
        _bossIntegration.CheckTelegraphInterrupt(combatState, boss, damageDealt);

        // Check for phase transition
        _bossIntegration.ProcessBossAction(combatState, boss);

        // Process end of turn
        _bossIntegration.ProcessEndOfTurn(combatState, currentTurn);

        currentTurn++;

        // === STEP 4: Trigger Phase 2 (75% HP) ===
        _log.Information("");
        _log.Information("STEP 4: Triggering phase 2 transition");

        boss.HP = 70; // Force phase 2
        _bossIntegration.ProcessBossAction(combatState, boss);

        Assert.That(boss.CurrentPhase, Is.EqualTo(2), "Boss should be in phase 2");
        _log.Information($"Boss transitioned to phase {boss.CurrentPhase}");

        // Check if adds were spawned
        if (combatState.Enemies.Count > 1)
        {
            _log.Information($"Phase 2 spawned {combatState.Enemies.Count - 1} adds!");
            foreach (var enemy in combatState.Enemies.Where(e => !e.IsBoss))
            {
                _log.Information($"  - {enemy.Name} (HP: {enemy.HP})");
            }
        }

        // === STEP 5: Trigger Enrage (25% HP) ===
        _log.Information("");
        _log.Information("STEP 5: Triggering enrage condition");

        boss.HP = 20; // Force enrage
        _bossIntegration.ProcessBossAction(combatState, boss);

        Assert.That(boss.IsEnraged, Is.True, "Boss should be enraged");
        _log.Information($"Boss is now ENRAGED! (HP: {boss.HP}/{boss.MaxHP})");

        // Display boss status
        var bossStatus = _bossIntegration.GetBossStatusDisplay(combatState);
        if (bossStatus.Any())
        {
            _log.Information("Boss Status:");
            foreach (var status in bossStatus)
            {
                _log.Information($"  {status}");
            }
        }

        // === STEP 6: Defeat Boss and Generate Loot ===
        _log.Information("");
        _log.Information("STEP 6: Defeating boss and generating loot");

        boss.HP = 0;
        boss.IsAlive = false;

        _bossIntegration.GenerateBossLoot(combatState, boss, player.Id);

        // Log loot messages
        _log.Information("Loot Generated:");
        foreach (var message in combatState.CombatLog.Skip(combatState.CombatLog.Count - 10))
        {
            if (message.Contains("LOOT") || message.Contains("(") || message.Contains("=°") || message.Contains("='"))
            {
                _log.Information($"  {message}");
            }
        }

        // === STEP 7: Cleanup ===
        _log.Information("");
        _log.Information("STEP 7: Cleaning up boss combat state");

        _bossIntegration.ClearBossCombatState(combatState);

        _log.Information("Combat complete! Boss defeated!");
        _log.Information("ZPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP]");
    }

    // PPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP
    // DEMONSTRATION TEST: Telegraph Interrupt
    // PPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP

    [Test]
    public void Demo_TelegraphInterrupt()
    {
        _log.Information("TPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPW");
        _log.Information("Q DEMONSTRATION: Telegraph Interrupt Mechanic");
        _log.Information("ZPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP]");

        var boss = CreateTestBoss();
        var combatState = CreateTestCombatState(boss);

        // Initialize boss
        _bossIntegration.InitializeBossEncounters(combatState);

        // Get an interruptible ability
        var abilities = _repository.GetBossAbilities(boss.BossEncounterId!.Value);
        var interruptible = abilities.FirstOrDefault(a => a.IsTelegraphed && a.InterruptDamageThreshold > 0);

        if (interruptible != null)
        {
            _log.Information($"Boss telegraphing: {interruptible.AbilityName}");
            _log.Information($"Interrupt threshold: {interruptible.InterruptDamageThreshold} damage");

            // Start telegraph
            _bossIntegration.BeginBossTelegraph(combatState, boss, interruptible, currentTurn: 1);

            // Player deals damage over multiple turns
            int totalDamage = 0;
            int turn = 1;

            while (totalDamage < interruptible.InterruptDamageThreshold)
            {
                int damage = 10;
                totalDamage += damage;

                _log.Information($"Turn {turn}: Dealing {damage} damage (Total: {totalDamage}/{interruptible.InterruptDamageThreshold})");

                _bossIntegration.CheckTelegraphInterrupt(combatState, boss, damage);

                if (boss.StaggeredTurnsRemaining > 0)
                {
                    _log.Information($"¡ INTERRUPTED! Boss is staggered for {boss.StaggeredTurnsRemaining} turns!");
                    break;
                }

                turn++;
            }
        }

        _log.Information("ZPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP]");
    }

    // PPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP
    // DEMONSTRATION TEST: All Four Bosses
    // PPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP

    [Test]
    public void Demo_AllBossEncounters()
    {
        _log.Information("TPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPW");
        _log.Information("Q DEMONSTRATION: All Four Boss Encounters");
        _log.Information("ZPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP]");

        var bossTypes = new[]
        {
            (EnemyType.RuinWarden, "Ruin-Warden", 1),
            (EnemyType.AethericAberration, "Aetheric Aberration", 2),
            (EnemyType.ForlornArchivist, "Forlorn Archivist", 3),
            (EnemyType.OmegaSentinel, "Omega Sentinel", 4)
        };

        var player = CreateTestPlayer();

        foreach (var (type, name, encounterId) in bossTypes)
        {
            _log.Information("");
            _log.Information($"PPP {name} PPP");

            var boss = new Enemy
            {
                Id = $"boss_{encounterId}",
                Name = name,
                Type = type,
                IsBoss = true,
                HP = 100,
                MaxHP = 100
            };

            var combatState = new CombatState
            {
                Player = player,
                Enemies = new List<Enemy> { boss },
                IsActive = true
            };

            // Initialize and verify
            _bossIntegration.InitializeBossEncounters(combatState);

            Assert.That(boss.BossEncounterId, Is.EqualTo(encounterId), $"{name} should have correct encounter ID");
            _log.Information($"Initialized: EncounterId={boss.BossEncounterId}, Phase={boss.CurrentPhase}");

            // Get boss abilities
            var abilities = _repository.GetBossAbilities(boss.BossEncounterId.Value);
            _log.Information($"Abilities: {abilities.Count} total");
            _log.Information($"  Telegraphed: {abilities.Count(a => a.IsTelegraphed)}");
            _log.Information($"  Ultimates: {abilities.Count(a => a.IsUltimate)}");

            // Test loot generation
            int tdr = encounterId switch
            {
                1 => 50,
                2 => 55,
                3 => 60,
                4 => 100,
                _ => 50
            };

            boss.HP = 0;
            _bossIntegration.GenerateBossLoot(combatState, boss, player.Id);

            // Count loot types
            var lastLogs = combatState.CombatLog.Skip(Math.Max(0, combatState.CombatLog.Count - 15)).ToList();
            bool hasArtifact = lastLogs.Any(l => l.Contains("ARTIFACT"));
            bool hasUnique = lastLogs.Any(l => l.Contains("UNIQUE"));

            _log.Information($"Loot: Artifact={hasArtifact}, Unique={hasUnique}");
        }

        _log.Information("ZPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP]");
    }

    // PPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP
    // HELPER METHODS
    // PPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP

    private Enemy CreateTestBoss()
    {
        return new Enemy
        {
            Id = $"test_boss_{Guid.NewGuid()}",
            Name = "Ruin-Warden",
            Type = EnemyType.RuinWarden,
            IsBoss = true,
            HP = 100,
            MaxHP = 100,
            Defense = 2,
            Soak = 1
        };
    }

    private PlayerCharacter CreateTestPlayer()
    {
        return new PlayerCharacter
        {
            Id = $"test_player_{Guid.NewGuid()}",
            Name = "Test Hero",
            HP = 80,
            MaxHP = 80,
            Stamina = 20,
            MaxStamina = 20,
            CraftingComponents = new Dictionary<ComponentType, int>()
        };
    }

    private CombatState CreateTestCombatState(Enemy boss)
    {
        return new CombatState
        {
            Player = CreateTestPlayer(),
            Enemies = new List<Enemy> { boss },
            IsActive = true,
            TurnNumber = 1
        };
    }

    private static readonly ILogger _log = Log.ForContext<BossCombatIntegrationDemoTests>();
}
