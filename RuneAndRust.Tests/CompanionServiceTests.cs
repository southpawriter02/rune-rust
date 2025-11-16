using NUnit.Framework;
using RuneAndRust.Engine;
using RuneAndRust.Persistence;
using RuneAndRust.Core;
using Microsoft.Data.Sqlite;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.34.4: Tests for CompanionService
/// Validates combat processing, command parsing, System Crash mechanics, and resource management
/// </summary>
[TestFixture]
public class CompanionServiceTests
{
    private string _testDbPath = null!;
    private string _connectionString = null!;
    private SaveRepository _saveRepo = null!;
    private CompanionService _companionService = null!;
    private CompanionCommands _companionCommands = null!;
    private RecruitmentService _recruitmentService = null!;

    [SetUp]
    public void Setup()
    {
        // Create unique test database for each test
        _testDbPath = Path.Combine(Path.GetTempPath(), $"test_companion_service_{Guid.NewGuid()}.db");
        _connectionString = $"Data Source={_testDbPath}";

        // Initialize database with all required tables
        _saveRepo = new SaveRepository(_testDbPath);
        _saveRepo.InitializeDatabase();

        // Seed companion data
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var schemaPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Data/v0.34.1_companion_schema.sql");
        if (File.Exists(schemaPath))
        {
            var schemaSql = File.ReadAllText(schemaPath);
            var schemaCommand = connection.CreateCommand();
            schemaCommand.CommandText = schemaSql;
            schemaCommand.ExecuteNonQuery();
        }

        // Create test character
        var createCharCommand = connection.CreateCommand();
        createCharCommand.CommandText = @"
            INSERT INTO Characters (character_id, character_name, display_name, current_level, current_legend, legend_to_next_level)
            VALUES (1, 'test_player', 'Test Player', 1, 0, 100)
        ";
        createCharCommand.ExecuteNonQuery();

        // Initialize services
        _companionService = new CompanionService(_connectionString);
        _companionCommands = new CompanionCommands(_companionService);
        _recruitmentService = new RecruitmentService(_connectionString);

        // Recruit a test companion (Finnr - no faction requirement)
        _recruitmentService.RecruitCompanion(1, 2);
    }

    [TearDown]
    public void TearDown()
    {
        if (File.Exists(_testDbPath))
        {
            File.Delete(_testDbPath);
        }
    }

    // ============================================
    // COMMAND PARSING TESTS (4 tests)
    // ============================================

    /// <summary>
    /// Test 1: Parse direct command with valid target
    /// </summary>
    [Test]
    public void ParseCommandVerb_ValidTarget_ReturnsSuccess()
    {
        // Arrange
        var player = CreateTestPlayer();
        var enemies = new List<Enemy>
        {
            new Enemy { Name = "Corrupted Warden", Id = "warden_1", HP = 50, MaxHP = 50 }
        };

        // Act
        var result = _companionCommands.ParseCommandVerb(
            "command Finnr aetheric_bolt warden_1",
            characterId: 1,
            player,
            enemies);

        // Assert
        Assert.That(result.Success, Is.True, "Command should succeed");
        Assert.That(result.Message, Does.Contain("Finnr"), "Message should mention companion");
    }

    /// <summary>
    /// Test 2: Parse stance change command
    /// </summary>
    [Test]
    public void ParseStanceVerb_ValidStance_ChangesStance()
    {
        // Act: Change stance to defensive
        var result = _companionCommands.ParseStanceVerb("stance Finnr defensive", characterId: 1);

        // Assert
        Assert.That(result.Success, Is.True, "Stance change should succeed");
        Assert.That(result.Message, Does.Contain("DEFENSIVE"), "Message should confirm new stance");

        // Verify stance persisted in database
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT current_stance FROM Characters_Companions
            WHERE character_id = 1 AND companion_id = 2
        ";
        var stance = (string)command.ExecuteScalar()!;
        Assert.That(stance, Is.EqualTo("defensive"), "Stance should be updated in database");
    }

    /// <summary>
    /// Test 3: Invalid companion name error
    /// </summary>
    [Test]
    public void ParseCommandVerb_InvalidCompanion_ReturnsFailure()
    {
        // Arrange
        var player = CreateTestPlayer();
        var enemies = new List<Enemy>();

        // Act
        var result = _companionCommands.ParseCommandVerb(
            "command UnknownCompanion attack enemy",
            characterId: 1,
            player,
            enemies);

        // Assert
        Assert.That(result.Success, Is.False, "Command should fail");
        Assert.That(result.Message, Does.Contain("not found"), "Error should mention companion not found");
    }

    /// <summary>
    /// Test 4: Invalid ability name error
    /// </summary>
    [Test]
    public void ParseCommandVerb_InvalidAbility_ReturnsFailure()
    {
        // Arrange
        var player = CreateTestPlayer();
        var enemies = new List<Enemy>
        {
            new Enemy { Name = "Test Enemy", Id = "enemy_1", HP = 50, MaxHP = 50 }
        };

        // Act
        var result = _companionCommands.ParseCommandVerb(
            "command Finnr unknown_ability enemy_1",
            characterId: 1,
            player,
            enemies);

        // Assert
        Assert.That(result.Success, Is.False, "Command should fail for unknown ability");
        Assert.That(result.Message, Does.Contain("not found").Or.Contains("cannot be used"),
            "Error should mention ability issue");
    }

    // ============================================
    // COMBAT PROCESSING TESTS (5 tests)
    // ============================================

    /// <summary>
    /// Test 5: Process companion turn (Aggressive stance)
    /// </summary>
    [Test]
    public void ProcessCompanionTurn_AggressiveStance_SelectsAction()
    {
        // Arrange
        var companion = CreateTestCompanion();
        companion.CurrentStance = "aggressive";
        var player = CreateTestPlayer();
        var enemies = new List<Enemy>
        {
            new Enemy { Name = "Test Enemy", Id = "enemy_1", HP = 30, MaxHP = 50, BaseDamageDice = 2 }
        };

        // Act
        var action = _companionService.ProcessCompanionTurn(companion, player, enemies);

        // Assert
        Assert.That(action, Is.Not.Null, "Action should be selected");
        Assert.That(action.ActionType, Is.Not.EqualTo("Wait"), "Aggressive stance should take action");
        Assert.That(action.Reason, Is.Not.Empty, "Action should have reasoning");
    }

    /// <summary>
    /// Test 6: Process companion turn (Defensive stance)
    /// </summary>
    [Test]
    public void ProcessCompanionTurn_DefensiveStance_ProtectsPlayer()
    {
        // Arrange
        var companion = CreateTestCompanion();
        companion.CurrentStance = "defensive";
        var player = CreateTestPlayer();
        player.HP = 20; // Low HP to trigger defensive behavior
        player.MaxHP = 100;

        var enemies = new List<Enemy>
        {
            new Enemy { Name = "Threat to Player", Id = "enemy_1", HP = 50, MaxHP = 50, BaseDamageDice = 3 }
        };

        // Act
        var action = _companionService.ProcessCompanionTurn(companion, player, enemies);

        // Assert
        Assert.That(action, Is.Not.Null, "Action should be selected");
        // Defensive stance should target threats to player
        Assert.That(action.ActionType, Is.Not.EqualTo("Wait"), "Should take defensive action");
    }

    /// <summary>
    /// Test 7: Passive stance with no command skips turn
    /// </summary>
    [Test]
    public void ProcessCompanionTurn_PassiveStanceNoCommand_SkipsTurn()
    {
        // Arrange
        var companion = CreateTestCompanion();
        companion.CurrentStance = "passive";
        var player = CreateTestPlayer();
        var enemies = new List<Enemy>
        {
            new Enemy { Name = "Test Enemy", Id = "enemy_1", HP = 50, MaxHP = 50 }
        };

        // Act
        var action = _companionService.ProcessCompanionTurn(companion, player, enemies);

        // Assert
        Assert.That(action.ActionType, Is.EqualTo("Wait"), "Passive stance should wait without command");
        Assert.That(action.Reason, Does.Contain("Passive").Or.Contains("awaits"), "Reason should mention passive stance");
    }

    /// <summary>
    /// Test 8: Execute companion attack
    /// </summary>
    [Test]
    public void ExecuteCompanionAction_Attack_DamagesEnemy()
    {
        // Arrange
        var companion = CreateTestCompanion();
        var enemy = new Enemy { Name = "Test Enemy", Id = "enemy_1", HP = 50, MaxHP = 50 };
        var enemies = new List<Enemy> { enemy };
        var player = CreateTestPlayer();

        var action = new CompanionAction
        {
            ActionType = "Attack",
            TargetEnemy = enemy,
            Reason = "Test attack"
        };

        var initialHP = enemy.HP;

        // Act
        var success = _companionService.ExecuteCompanionAction(companion, action, enemies, player);

        // Assert
        Assert.That(success, Is.True, "Attack should succeed");
        Assert.That(enemy.HP, Is.LessThan(initialHP), "Enemy should take damage");
    }

    /// <summary>
    /// Test 9: Execute companion ability with resource cost
    /// </summary>
    [Test]
    public void ExecuteCompanionAction_Ability_ConsumesResources()
    {
        // Arrange
        var companion = CreateTestCompanion();
        companion.CurrentStamina = 50;
        companion.MaxStamina = 100;

        var ability = new CompanionAbility
        {
            AbilityID = 101,
            AbilityName = "Power Strike",
            ResourceCostType = "Stamina",
            ResourceCost = 20,
            TargetType = "single_target",
            RangeType = "melee"
        };
        companion.Abilities.Add(ability);

        var enemy = new Enemy { Name = "Test Enemy", Id = "enemy_1", HP = 50, MaxHP = 50 };
        var enemies = new List<Enemy> { enemy };
        var player = CreateTestPlayer();

        var action = new CompanionAction
        {
            ActionType = "UseAbility",
            AbilityName = "Power Strike",
            TargetEnemy = enemy,
            Reason = "Test ability"
        };

        // Act
        var success = _companionService.ExecuteCompanionAction(companion, action, enemies, player);

        // Assert
        Assert.That(success, Is.True, "Ability should execute");
        Assert.That(companion.CurrentStamina, Is.EqualTo(30), "Stamina should be consumed (50 - 20 = 30)");
    }

    // ============================================
    // SYSTEM CRASH & RECOVERY TESTS (4 tests)
    // ============================================

    /// <summary>
    /// Test 10: Companion reaches 0 HP triggers System Crash
    /// </summary>
    [Test]
    public void ApplyCompanionDamage_ReachesZeroHP_TriggersSystemCrash()
    {
        // Arrange
        var companion = CreateTestCompanion();
        companion.CurrentHitPoints = 10;
        var player = CreateTestPlayer();
        player.PsychicStress = 0;

        // Act
        _companionService.ApplyCompanionDamage(companion, damage: 15, player);

        // Assert
        Assert.That(companion.CurrentHitPoints, Is.EqualTo(0), "HP should be 0");
        Assert.That(companion.IsIncapacitated, Is.True, "Companion should be incapacitated");
        Assert.That(player.PsychicStress, Is.EqualTo(10), "Player should gain +10 Psychic Stress");
    }

    /// <summary>
    /// Test 11: Player receives +10 Psychic Stress on companion crash
    /// </summary>
    [Test]
    public void HandleSystemCrash_IncreasesPlayerPsychicStress()
    {
        // Arrange
        var companion = CreateTestCompanion();
        var player = CreateTestPlayer();
        player.PsychicStress = 30; // Existing stress

        // Act
        _companionService.HandleSystemCrash(companion, player);

        // Assert
        Assert.That(player.PsychicStress, Is.EqualTo(40), "Player Psychic Stress should increase by 10");
        Assert.That(companion.IsIncapacitated, Is.True, "Companion should be marked incapacitated");
    }

    /// <summary>
    /// Test 12: After-combat recovery restores 50% HP
    /// </summary>
    [Test]
    public void RecoverCompanion_AfterCombat_Restores50PercentHP()
    {
        // Arrange
        var companion = CreateTestCompanion();
        companion.MaxHitPoints = 100;
        companion.CurrentHitPoints = 0;
        companion.IsIncapacitated = true;

        // Act
        _companionService.RecoverCompanion(companion, characterId: 1);

        // Assert
        Assert.That(companion.CurrentHitPoints, Is.EqualTo(50), "Should restore 50% HP");
        Assert.That(companion.IsIncapacitated, Is.False, "Should no longer be incapacitated");
        Assert.That(companion.CurrentStamina, Is.EqualTo(companion.MaxStamina), "Should restore full Stamina");
    }

    /// <summary>
    /// Test 13: Field revival ability works mid-dungeon
    /// </summary>
    [Test]
    public void ReviveCompanion_MidDungeon_RestoresHP()
    {
        // Arrange
        var companion = CreateTestCompanion();
        companion.MaxHitPoints = 80;
        companion.CurrentHitPoints = 0;
        companion.IsIncapacitated = true;

        var healAmount = 30; // Field Repair heals 4d6 (~30 average)

        // Act
        _companionService.ReviveCompanion(companion, healAmount, characterId: 1);

        // Assert
        Assert.That(companion.CurrentHitPoints, Is.EqualTo(30), "Should restore specified HP amount");
        Assert.That(companion.IsIncapacitated, Is.False, "Should no longer be incapacitated");
    }

    // ============================================
    // RESOURCE MANAGEMENT TESTS (2 tests)
    // ============================================

    /// <summary>
    /// Test 14: Stamina consumed on ability use
    /// </summary>
    [Test]
    public void CommandCompanion_UsesAbility_ConsumesStamina()
    {
        // Arrange
        var companion = CreateTestCompanion();
        companion.CurrentStamina = 60;

        var ability = new CompanionAbility
        {
            AbilityID = 102,
            AbilityName = "Shield Bash",
            ResourceCostType = "Stamina",
            ResourceCost = 25,
            TargetType = "single_target"
        };
        companion.Abilities.Add(ability);

        var enemy = new Enemy { Name = "Enemy", Id = "enemy_1", HP = 50, MaxHP = 50 };
        var enemies = new List<Enemy> { enemy };
        var player = CreateTestPlayer();

        // Act
        var action = _companionService.CommandCompanion(
            companion,
            "Shield Bash",
            enemy,
            enemies,
            player);

        // Assert
        Assert.That(action.ActionType, Is.EqualTo("UseAbility"), "Should create UseAbility action");
        Assert.That(action.AbilityName, Is.EqualTo("Shield Bash"), "Should use correct ability");

        // Execute the action to consume resources
        _companionService.ExecuteCompanionAction(companion, action, enemies, player);
        Assert.That(companion.CurrentStamina, Is.EqualTo(35), "Stamina should be consumed (60 - 25 = 35)");
    }

    /// <summary>
    /// Test 15: Cannot use ability without sufficient resources
    /// </summary>
    [Test]
    public void CommandCompanion_InsufficientResources_ReturnsWaitAction()
    {
        // Arrange
        var companion = CreateTestCompanion();
        companion.CurrentStamina = 5; // Not enough for ability

        var ability = new CompanionAbility
        {
            AbilityID = 103,
            AbilityName = "Heavy Strike",
            ResourceCostType = "Stamina",
            ResourceCost = 30,
            TargetType = "single_target"
        };
        companion.Abilities.Add(ability);

        var enemy = new Enemy { Name = "Enemy", Id = "enemy_1", HP = 50, MaxHP = 50 };
        var enemies = new List<Enemy> { enemy };
        var player = CreateTestPlayer();

        // Act
        var action = _companionService.CommandCompanion(
            companion,
            "Heavy Strike",
            enemy,
            enemies,
            player);

        // Assert
        Assert.That(action.ActionType, Is.EqualTo("Wait"), "Should return Wait action when resources insufficient");
        Assert.That(action.Reason, Does.Contain("Insufficient"), "Reason should mention insufficient resources");
    }

    // ============================================
    // PARTY MANAGEMENT TESTS (3 tests)
    // ============================================

    /// <summary>
    /// Test 16: GetPartyCompanions returns active companions
    /// </summary>
    [Test]
    public void GetPartyCompanions_ReturnsActiveParty()
    {
        // Act
        var companions = _companionService.GetPartyCompanions(characterId: 1);

        // Assert
        Assert.That(companions.Count, Is.GreaterThan(0), "Should have at least one companion (Finnr)");
        var finnr = companions.FirstOrDefault(c => c.CompanionID == 2);
        Assert.That(finnr, Is.Not.Null, "Finnr should be in party");
        Assert.That(finnr!.DisplayName, Does.Contain("Finnr"), "Should have correct display name");
    }

    /// <summary>
    /// Test 17: GetCompanionByName fuzzy matching
    /// </summary>
    [Test]
    public void GetCompanionByName_FuzzyMatch_FindsCompanion()
    {
        // Act: Try partial name match
        var companion = _companionService.GetCompanionByName(characterId: 1, "Finnr");

        // Assert
        Assert.That(companion, Is.Not.Null, "Should find companion by partial name");
        Assert.That(companion!.CompanionID, Is.EqualTo(2), "Should be Finnr (ID 2)");
    }

    /// <summary>
    /// Test 18: Incapacitated companion skips turn
    /// </summary>
    [Test]
    public void ProcessCompanionTurn_Incapacitated_SkipsTurn()
    {
        // Arrange
        var companion = CreateTestCompanion();
        companion.IsIncapacitated = true;
        var player = CreateTestPlayer();
        var enemies = new List<Enemy>
        {
            new Enemy { Name = "Enemy", Id = "enemy_1", HP = 50, MaxHP = 50 }
        };

        // Act
        var action = _companionService.ProcessCompanionTurn(companion, player, enemies);

        // Assert
        Assert.That(action.ActionType, Is.EqualTo("Wait"), "Incapacitated companion should wait");
        Assert.That(action.Reason, Does.Contain("incapacitated").Or.Contains("System Crash"),
            "Reason should mention incapacitation");
    }

    /// <summary>
    /// Test 19: Sanctuary recovery fully restores companion
    /// </summary>
    [Test]
    public void SanctuaryRecovery_FullyRestoresCompanion()
    {
        // Arrange
        var companion = CreateTestCompanion();
        companion.MaxHitPoints = 100;
        companion.CurrentHitPoints = 10;
        companion.MaxStamina = 80;
        companion.CurrentStamina = 5;
        companion.IsIncapacitated = false;

        // Act
        _companionService.SanctuaryRecovery(companion, characterId: 1);

        // Assert
        Assert.That(companion.CurrentHitPoints, Is.EqualTo(100), "HP should be fully restored");
        Assert.That(companion.CurrentStamina, Is.EqualTo(80), "Stamina should be fully restored");
        Assert.That(companion.IsIncapacitated, Is.False, "Should not be incapacitated");
    }

    // ============================================
    // HELPER METHODS
    // ============================================

    private Companion CreateTestCompanion()
    {
        return new Companion
        {
            CompanionID = 2,
            CompanionName = "finnr",
            DisplayName = "Finnr the Forlorn",
            Archetype = "Adept",
            CombatRole = "Support/Control",
            BaseMight = 10,
            BaseFinesse = 12,
            BaseSturdiness = 10,
            BaseWits = 16,
            BaseWill = 14,
            MaxHitPoints = 70,
            CurrentHitPoints = 70,
            BaseDefense = 12,
            BaseSoak = 2,
            ResourceType = "Aether Pool",
            MaxAetherPool = 120,
            CurrentAetherPool = 120,
            MaxStamina = 0,
            CurrentStamina = 0,
            CurrentStance = "aggressive",
            IsIncapacitated = false,
            Abilities = new List<CompanionAbility>()
        };
    }

    private PlayerCharacter CreateTestPlayer()
    {
        return new PlayerCharacter
        {
            CharacterID = 1,
            Name = "Test Player",
            HP = 100,
            MaxHP = 100,
            Stamina = 80,
            MaxStamina = 80,
            PsychicStress = 0,
            Attributes = new Attributes { Might = 12, Finesse = 10, Sturdiness = 12, Wits = 10, Will = 10 }
        };
    }
}
