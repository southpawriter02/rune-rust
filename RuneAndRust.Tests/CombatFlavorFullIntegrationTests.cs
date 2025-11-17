using Xunit;
using Xunit.Abstractions;
using RuneAndRust.Engine;
using RuneAndRust.Persistence;
using RuneAndRust.Core;
using RuneAndRust.Core.CombatFlavor;
using Microsoft.Data.Sqlite;
using Serilog;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.38.6: Full integration tests for CombatEngine with CombatFlavorTextService
/// Tests the actual combat engine with flavor text enhancement
/// </summary>
public class CombatFlavorFullIntegrationTests : IDisposable
{
    private readonly ITestOutputHelper _output;
    private readonly SqliteConnection _connection;
    private readonly string _connectionString;
    private readonly CombatEngine _combatEngine;

    public CombatFlavorFullIntegrationTests(ITestOutputHelper output)
    {
        _output = output;

        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.TestOutput(output)
            .CreateLogger();

        // Create in-memory database
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        _connectionString = _connection.ConnectionString;

        // Initialize combat flavor text schema and content
        InitializeDatabase();

        // Create services
        var diceService = new DiceService();
        var sagaService = new SagaService(_connectionString);
        var currencyService = new CurrencyService(_connectionString);
        var lootService = new LootService(_connectionString, diceService);
        var equipmentService = new EquipmentService(_connectionString);
        var hazardService = new HazardService(diceService, currencyService);
        var repository = new DescriptorRepository(_connectionString);
        var flavorTextService = new CombatFlavorTextService(repository);

        // Create combat engine with flavor text service
        _combatEngine = new CombatEngine(
            diceService,
            sagaService,
            lootService,
            equipmentService,
            hazardService,
            currencyService,
            flavorTextService: flavorTextService);
    }

    private void InitializeDatabase()
    {
        // Create all necessary tables
        var schemaCommand = _connection.CreateCommand();
        schemaCommand.CommandText = @"
            -- Combat flavor text tables
            CREATE TABLE Combat_Action_Descriptors (
                descriptor_id INTEGER PRIMARY KEY AUTOINCREMENT,
                category TEXT NOT NULL,
                weapon_type TEXT,
                enemy_archetype TEXT,
                outcome_type TEXT,
                descriptor_text TEXT NOT NULL,
                tags TEXT
            );

            CREATE TABLE Enemy_Voice_Profiles (
                profile_id INTEGER PRIMARY KEY AUTOINCREMENT,
                enemy_archetype TEXT NOT NULL UNIQUE,
                voice_description TEXT NOT NULL,
                setting_context TEXT NOT NULL,
                attack_descriptors TEXT NOT NULL,
                reaction_damage TEXT NOT NULL,
                reaction_death TEXT NOT NULL,
                special_attacks TEXT
            );

            CREATE TABLE Environmental_Combat_Modifiers (
                modifier_id INTEGER PRIMARY KEY AUTOINCREMENT,
                biome_name TEXT NOT NULL,
                modifier_type TEXT NOT NULL,
                descriptor_text TEXT NOT NULL,
                trigger_chance REAL DEFAULT 0.3
            );

            -- Sample data
            INSERT INTO Combat_Action_Descriptors (category, weapon_type, outcome_type, descriptor_text, tags)
            VALUES
            ('PlayerMeleeAttack', 'SwordOneHanded', 'SolidHit',
             'Your {Weapon} bites into the {Enemy}''s {Target_Location} with a satisfying crunch.',
             '[\"OneHanded\", \"SolidHit\"]'),
            ('PlayerMeleeAttack', 'SwordOneHanded', 'CriticalHit',
             'You find a critical weakness—your {Weapon} plunges into the {Enemy}''s {Vital_Location} with devastating precision!',
             '[\"OneHanded\", \"Critical\"]'),
            ('PlayerMeleeAttack', 'SwordOneHanded', 'Miss',
             'You swing your {Weapon} at the {Enemy}, but it sidesteps effortlessly.',
             '[\"OneHanded\", \"Miss\"]');

            INSERT INTO Combat_Action_Descriptors (category, enemy_archetype, outcome_type, descriptor_text, tags)
            VALUES
            ('EnemyAttack', 'Servitor', NULL,
             'The Servitor''s articulated limb swings at you with mechanical precision.',
             '[\"Servitor\", \"Attack\"]'),
            ('EnemyDefense', 'Servitor', 'SolidHit',
             'The Servitor''s chassis dents under your blow, circuits sparking.',
             '[\"Servitor\", \"Reaction\"]'),
            ('EnemyDefense', 'Servitor', 'CriticalHit',
             'The Servitor collapses, its corrupted runes dimming to darkness.',
             '[\"Servitor\", \"Death\"]');

            INSERT INTO Enemy_Voice_Profiles (enemy_archetype, voice_description, setting_context, attack_descriptors, reaction_damage, reaction_death, special_attacks)
            VALUES ('Servitor', 'Mechanical, emotionless', 'Corrupted machines', '[4]', '[5]', '[6]', '[]');

            INSERT INTO Environmental_Combat_Modifiers (biome_name, modifier_type, descriptor_text, trigger_chance)
            VALUES ('The_Roots', 'Reaction', 'Your strike sends rust flakes cascading from the ceiling.', 1.0);

            -- Required for CombatEngine
            CREATE TABLE Characters (
                character_id INTEGER PRIMARY KEY AUTOINCREMENT,
                name TEXT NOT NULL
            );

            CREATE TABLE Equipment (
                equipment_id INTEGER PRIMARY KEY AUTOINCREMENT,
                name TEXT NOT NULL,
                category TEXT NOT NULL,
                tier INTEGER DEFAULT 0
            );
        ";
        schemaCommand.ExecuteNonQuery();
    }

    [Fact]
    public void CombatEngine_WithFlavorText_EnhancesCombatLog()
    {
        // Arrange
        var player = new PlayerCharacter
        {
            Name = "Valen",
            HP = 50,
            MaxHP = 50,
            Attributes = new Attributes { Might = 4, Finesse = 3, Sturdiness = 3 },
            EquippedWeapon = new Equipment
            {
                Name = "Rusted Longsword",
                Category = "Weapon",
                WeaponAttribute = "MIGHT",
                DamageDice = 2,
                DamageBonus = 1,
                AccuracyBonus = 0,
                HandRequirement = 1
            }
        };

        var enemy = new Enemy
        {
            Name = "Corrupted Servitor",
            Type = EnemyType.CorruptedServitor,
            HP = 20,
            MaxHP = 20,
            Attributes = new Attributes { Might = 2, Finesse = 2, Sturdiness = 3 },
            BaseDamageDice = 1
        };

        var room = new Room
        {
            Id = 1,
            BiomeName = "The_Roots",
            Name = "Corroded Hall"
        };

        var combatState = _combatEngine.InitializeCombat(
            player,
            new List<Enemy> { enemy },
            room);

        // Act - Execute player attack
        _combatEngine.PlayerAttack(combatState, enemy);

        // Assert - Combat log should contain flavor text
        var combatLog = string.Join("\n", combatState.CombatLog);
        _output.WriteLine("=== COMBAT LOG ===");
        _output.WriteLine(combatLog);
        _output.WriteLine("==================");

        // Should contain attack rolls
        Assert.Contains("attacks", combatLog.ToLower());
        Assert.Contains("Rolled", combatLog);

        // Should contain flavor text elements (one of these should appear)
        bool hasFlavorText = combatLog.Contains("bites into") ||
                            combatLog.Contains("critical weakness") ||
                            combatLog.Contains("sidesteps") ||
                            combatLog.Contains("chassis dents") ||
                            combatLog.Contains("rust flakes");

        Assert.True(hasFlavorText, "Combat log should contain flavor text from v0.38.6 system");
    }

    [Fact]
    public void CombatEngine_CriticalHit_ShowsDramaticFlavorText()
    {
        // Arrange
        var player = new PlayerCharacter
        {
            Name = "Valen",
            HP = 50,
            MaxHP = 50,
            Attributes = new Attributes { Might = 6, Finesse = 6, Sturdiness = 4 }
        };

        player.EquippedWeapon = new Equipment
        {
            Name = "Frost-Kissed Blade",
            Category = "Weapon",
            WeaponAttribute = "MIGHT",
            DamageDice = 3,
            DamageBonus = 2,
            HandRequirement = 1
        };

        var enemy = new Enemy
        {
            Name = "Servitor Scout",
            Type = EnemyType.CorruptedServitor,
            HP = 10,
            MaxHP = 10,
            Attributes = new Attributes { Might = 1, Finesse = 1, Sturdiness = 2 }
        };

        var combatState = _combatEngine.InitializeCombat(player, new List<Enemy> { enemy });

        // Act - Keep attacking until we get a critical hit (or 10 attempts)
        int attempts = 0;
        string combatLog = "";

        while (enemy.IsAlive && attempts < 10)
        {
            combatState.CombatLog.Clear();
            _combatEngine.PlayerAttack(combatState, enemy);
            combatLog = string.Join("\n", combatState.CombatLog);
            attempts++;

            if (combatLog.Contains("CRITICAL") || combatLog.Contains("critical"))
            {
                break;
            }

            // Reset enemy for next attempt if still alive
            if (enemy.IsAlive)
            {
                enemy.HP = enemy.MaxHP;
            }
        }

        _output.WriteLine($"=== ATTEMPT #{attempts} ===");
        _output.WriteLine(combatLog);

        // Assert - If we got a critical, it should have dramatic flavor
        if (combatLog.Contains("CRITICAL") || combatLog.Contains("critical"))
        {
            _output.WriteLine("Critical hit achieved! Verifying flavor text...");
            Assert.True(combatLog.Length > 100, "Critical combat should have substantial flavor text");
        }
    }

    [Fact]
    public void EnemyArchetypeMapper_MapsCorrectly()
    {
        // Arrange & Act
        var servitorArchetype = EnemyArchetypeMapper.GetArchetype(EnemyType.CorruptedServitor);
        var forlornArchetype = EnemyArchetypeMapper.GetArchetype(EnemyType.ForlornScholar);
        var dvergArchetype = EnemyArchetypeMapper.GetArchetype(EnemyType.CorruptedEngineer);
        var beastArchetype = EnemyArchetypeMapper.GetArchetype(EnemyType.ScrapHound);
        var wraithArchetype = EnemyArchetypeMapper.GetArchetype(EnemyType.AethericAberration);

        // Assert
        Assert.Equal(EnemyArchetype.Servitor, servitorArchetype);
        Assert.Equal(EnemyArchetype.Forlorn, forlornArchetype);
        Assert.Equal(EnemyArchetype.Corrupted_Dvergr, dvergArchetype);
        Assert.Equal(EnemyArchetype.Blight_Touched_Beast, beastArchetype);
        Assert.Equal(EnemyArchetype.Aether_Wraith, wraithArchetype);

        _output.WriteLine("All enemy archetypes mapped correctly!");
    }

    [Fact]
    public void CombatOutcomeCalculator_DeterminesCorrectOutcomes()
    {
        // Arrange & Act
        var miss = CombatOutcomeCalculator.DetermineOutcomeFromSuccesses(2, 3);
        var deflected = CombatOutcomeCalculator.DetermineOutcomeFromSuccesses(2, 2);
        var glancing = CombatOutcomeCalculator.DetermineOutcomeFromSuccesses(3, 2);
        var solid = CombatOutcomeCalculator.DetermineOutcomeFromSuccesses(5, 2);
        var devastating = CombatOutcomeCalculator.DetermineOutcomeFromSuccesses(8, 2);
        var critical = CombatOutcomeCalculator.DetermineOutcomeFromSuccesses(10, 2, isCriticalHit: true);

        // Assert
        Assert.Equal(CombatOutcome.Miss, miss);
        Assert.Equal(CombatOutcome.Deflected, deflected);
        Assert.Equal(CombatOutcome.GlancingHit, glancing);
        Assert.Equal(CombatOutcome.SolidHit, solid);
        Assert.Equal(CombatOutcome.DevastatingHit, devastating);
        Assert.Equal(CombatOutcome.CriticalHit, critical);

        _output.WriteLine("All combat outcomes calculated correctly!");
    }

    [Fact]
    public void CombatEngine_WithoutFlavorService_StillWorks()
    {
        // Arrange - Create combat engine WITHOUT flavor text service
        var diceService = new DiceService();
        var sagaService = new SagaService(_connectionString);
        var currencyService = new CurrencyService(_connectionString);
        var lootService = new LootService(_connectionString, diceService);
        var equipmentService = new EquipmentService(_connectionString);
        var hazardService = new HazardService(diceService, currencyService);

        var vanillaCombatEngine = new CombatEngine(
            diceService,
            sagaService,
            lootService,
            equipmentService,
            hazardService,
            currencyService,
            flavorTextService: null); // No flavor text

        var player = new PlayerCharacter
        {
            Name = "Valen",
            HP = 50,
            MaxHP = 50,
            Attributes = new Attributes { Might = 4, Finesse = 3, Sturdiness = 3 }
        };

        var enemy = new Enemy
        {
            Name = "Test Enemy",
            Type = EnemyType.CorruptedServitor,
            HP = 20,
            MaxHP = 20,
            Attributes = new Attributes { Might = 2, Finesse = 2, Sturdiness = 3 }
        };

        var combatState = vanillaCombatEngine.InitializeCombat(player, new List<Enemy> { enemy });

        // Act
        vanillaCombatEngine.PlayerAttack(combatState, enemy);

        // Assert - Should still work with generic text
        var combatLog = string.Join("\n", combatState.CombatLog);
        _output.WriteLine("=== VANILLA COMBAT (No Flavor Text) ===");
        _output.WriteLine(combatLog);

        Assert.Contains("attacks", combatLog.ToLower());
        Assert.True(combatLog.Length > 0, "Combat should still produce log entries");
    }

    public void Dispose()
    {
        _connection?.Close();
        _connection?.Dispose();
    }
}
