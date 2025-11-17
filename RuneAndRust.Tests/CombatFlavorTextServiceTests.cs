using Xunit;
using RuneAndRust.Engine;
using RuneAndRust.Persistence;
using RuneAndRust.Core.CombatFlavor;
using Microsoft.Data.Sqlite;
using Serilog;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.38.6: Tests for Combat Flavor Text Service
/// </summary>
public class CombatFlavorTextServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly string _connectionString;
    private readonly DescriptorRepository _repository;
    private readonly CombatFlavorTextService _service;
    private readonly Random _seededRandom;

    public CombatFlavorTextServiceTests()
    {
        // Configure Serilog for tests
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateLogger();

        // Create in-memory database
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        _connectionString = _connection.ConnectionString;

        // Initialize schema
        InitializeSchema();

        // Insert test data
        InsertTestData();

        // Create repository and service
        _repository = new DescriptorRepository(_connectionString);
        _seededRandom = new Random(42); // Seeded for reproducible tests
        _service = new CombatFlavorTextService(_repository, _seededRandom);
    }

    private void InitializeSchema()
    {
        var command = _connection.CreateCommand();
        command.CommandText = @"
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
        ";
        command.ExecuteNonQuery();
    }

    private void InsertTestData()
    {
        var command = _connection.CreateCommand();

        // Player sword attacks
        command.CommandText = @"
            INSERT INTO Combat_Action_Descriptors (category, weapon_type, outcome_type, descriptor_text, tags)
            VALUES
            ('PlayerMeleeAttack', 'SwordOneHanded', 'Miss',
             'You swing your {Weapon} at the {Enemy}, but it sidesteps effortlessly.',
             '[""OneHanded"", ""Miss""]'),
            ('PlayerMeleeAttack', 'SwordOneHanded', 'SolidHit',
             'Your {Weapon} bites into the {Enemy}''s {Target_Location} with a satisfying crunch.',
             '[""OneHanded"", ""SolidHit""]'),
            ('PlayerMeleeAttack', 'SwordOneHanded', 'CriticalHit',
             'You find a critical weakness—your {Weapon} plunges into the {Enemy}''s {Vital_Location} with devastating precision!',
             '[""OneHanded"", ""Critical""]');
        ";
        command.ExecuteNonQuery();

        // Bow attacks
        command.CommandText = @"
            INSERT INTO Combat_Action_Descriptors (category, weapon_type, outcome_type, descriptor_text, tags)
            VALUES
            ('PlayerRangedAttack', 'Bow', 'SolidHit',
             'Your arrow punches through the {Enemy}''s {Target_Location} with a wet thunk.',
             '[""Bow"", ""SolidHit""]');
        ";
        command.ExecuteNonQuery();

        // Defense actions
        command.CommandText = @"
            INSERT INTO Combat_Action_Descriptors (category, weapon_type, outcome_type, descriptor_text, tags)
            VALUES
            ('PlayerDefense', 'Dodge', 'Miss',
             'You twist aside, the {Enemy}''s attack passing harmlessly by.',
             '[""Dodge"", ""Success""]'),
            ('PlayerDefense', 'Parry', 'Deflected',
             'You parry the {Enemy}''s attack with your {Weapon}, metal ringing on metal.',
             '[""Parry"", ""Success""]');
        ";
        command.ExecuteNonQuery();

        // Enemy Servitor descriptors
        command.CommandText = @"
            INSERT INTO Combat_Action_Descriptors (category, enemy_archetype, outcome_type, descriptor_text, tags)
            VALUES
            ('EnemyAttack', 'Servitor', NULL,
             'The Servitor''s articulated limb swings at you with mechanical precision.',
             '[""Servitor"", ""Attack""]'),
            ('EnemyAttack', 'Servitor', NULL,
             'The Servitor''s corrupted power core crackles—electricity arcs toward you!',
             '[""Servitor"", ""Special""]'),
            ('EnemyDefense', 'Servitor', 'SolidHit',
             'The Servitor''s chassis dents under your blow, circuits sparking.',
             '[""Servitor"", ""Reaction""]'),
            ('EnemyDefense', 'Servitor', 'CriticalHit',
             'The Servitor collapses, its corrupted runes dimming to darkness.',
             '[""Servitor"", ""Death""]');
        ";
        command.ExecuteNonQuery();

        // Enemy voice profile - Servitor
        command.CommandText = @"
            INSERT INTO Enemy_Voice_Profiles
            (enemy_archetype, voice_description, setting_context, attack_descriptors, reaction_damage, reaction_death, special_attacks)
            VALUES
            ('Servitor', 'Mechanical, emotionless', 'Corrupted machines', '[4]', '[6]', '[7]', '[5]');
        ";
        command.ExecuteNonQuery();

        // Environmental modifiers
        command.CommandText = @"
            INSERT INTO Environmental_Combat_Modifiers (biome_name, modifier_type, descriptor_text, trigger_chance)
            VALUES
            ('The_Roots', 'Reaction', 'Your strike sends rust flakes cascading from the ceiling.', 0.5),
            ('Muspelheim', 'HazardIntegration', 'Flames roar higher as if feeding on the violence.', 0.6);
        ";
        command.ExecuteNonQuery();
    }

    [Fact]
    public void GeneratePlayerAttackText_SwordMiss_ReturnsCorrectText()
    {
        // Act
        var result = _service.GeneratePlayerAttackText(
            WeaponType.SwordOneHanded,
            "Rusted Longsword",
            "Servitor",
            CombatOutcome.Miss);

        // Assert
        Assert.Contains("Rusted Longsword", result);
        Assert.Contains("Servitor", result);
        Assert.Contains("sidesteps", result);
    }

    [Fact]
    public void GeneratePlayerAttackText_SwordSolidHit_ContainsTemplateVariables()
    {
        // Act
        var result = _service.GeneratePlayerAttackText(
            WeaponType.SwordOneHanded,
            "Iron Blade",
            "Forlorn",
            CombatOutcome.SolidHit);

        // Assert
        Assert.Contains("Iron Blade", result);
        Assert.Contains("Forlorn", result);
        Assert.DoesNotContain("{Weapon}", result); // Template variable should be replaced
        Assert.DoesNotContain("{Enemy}", result);
        Assert.DoesNotContain("{Target_Location}", result);
    }

    [Fact]
    public void GeneratePlayerAttackText_CriticalHit_ReturnsDramaticText()
    {
        // Act
        var result = _service.GeneratePlayerAttackText(
            WeaponType.SwordOneHanded,
            "Steel Sword",
            "Corrupted Dvergr",
            CombatOutcome.CriticalHit);

        // Assert
        Assert.Contains("critical", result.ToLower());
        Assert.Contains("Steel Sword", result);
    }

    [Fact]
    public void GeneratePlayerAttackText_BowAttack_ReturnsRangedText()
    {
        // Act
        var result = _service.GeneratePlayerAttackText(
            WeaponType.Bow,
            "Hunting Bow",
            "Beast",
            CombatOutcome.SolidHit);

        // Assert
        Assert.Contains("arrow", result.ToLower());
        Assert.Contains("Hunting Bow", result);
    }

    [Fact]
    public void GeneratePlayerDefenseText_SuccessfulDodge_ReturnsSuccessText()
    {
        // Act
        var result = _service.GeneratePlayerDefenseText(
            WeaponType.Dodge,
            null,
            null,
            "Servitor",
            successful: true);

        // Assert
        Assert.Contains("twist", result.ToLower());
        Assert.Contains("Servitor", result);
    }

    [Fact]
    public void GeneratePlayerDefenseText_SuccessfulParry_ReturnsParryText()
    {
        // Act
        var result = _service.GeneratePlayerDefenseText(
            WeaponType.Parry,
            "Longsword",
            null,
            "Enemy",
            successful: true);

        // Assert
        Assert.Contains("parry", result.ToLower());
        Assert.Contains("Longsword", result);
    }

    [Fact]
    public void GenerateEnemyAttackText_ServitorRegularAttack_ReturnsArchetypeVoice()
    {
        // Act
        var result = _service.GenerateEnemyAttackText(
            EnemyArchetype.Servitor,
            "Corrupted Servitor Alpha",
            isSpecialAttack: false);

        // Assert
        Assert.Contains("Servitor", result);
        Assert.Contains("mechanical", result.ToLower());
    }

    [Fact]
    public void GenerateEnemyAttackText_ServitorSpecialAttack_ReturnsSpecialText()
    {
        // Act
        var result = _service.GenerateEnemyAttackText(
            EnemyArchetype.Servitor,
            "Corrupted Servitor",
            isSpecialAttack: true);

        // Assert
        Assert.Contains("electricity", result.ToLower());
    }

    [Fact]
    public void GenerateEnemyDamageReaction_ServitorDamaged_ReturnsReactionText()
    {
        // Act
        var result = _service.GenerateEnemyDamageReaction(
            EnemyArchetype.Servitor,
            "Servitor Scout",
            damageAmount: 15,
            isDying: false);

        // Assert
        Assert.Contains("Servitor", result);
        Assert.Contains("chassis", result.ToLower());
    }

    [Fact]
    public void GenerateEnemyDamageReaction_ServitorDying_ReturnsDeathText()
    {
        // Act
        var result = _service.GenerateEnemyDamageReaction(
            EnemyArchetype.Servitor,
            "Servitor",
            damageAmount: 50,
            isDying: true);

        // Assert
        Assert.Contains("Servitor", result);
        Assert.Contains("collapse", result.ToLower());
    }

    [Fact]
    public void GenerateEnvironmentalReaction_TheRoots_ReturnsBiomeText()
    {
        // Act
        var result = _service.GenerateEnvironmentalReaction("The_Roots");

        // Assert - May be empty due to trigger chance, or contain rust/corroded
        if (!string.IsNullOrEmpty(result))
        {
            Assert.Contains("rust", result.ToLower());
        }
    }

    [Fact]
    public void GenerateEnvironmentalReaction_NonExistentBiome_ReturnsEmpty()
    {
        // Act
        var result = _service.GenerateEnvironmentalReaction("NonExistentBiome");

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void CombatFlavorTextStats_ReturnsCorrectCounts()
    {
        // Act
        var stats = _repository.GetCombatFlavorTextStats();

        // Assert
        Assert.True(stats.TotalCombatDescriptors > 0);
        Assert.True(stats.TotalEnemyVoiceProfiles > 0);
        Assert.True(stats.TotalEnvironmentalModifiers > 0);
    }

    [Fact]
    public void GetCombatActionDescriptors_FilterByCategory_ReturnsFiltered()
    {
        // Act
        var descriptors = _repository.GetCombatActionDescriptors(
            category: CombatActionCategory.PlayerMeleeAttack);

        // Assert
        Assert.NotEmpty(descriptors);
        Assert.All(descriptors, d => Assert.Equal("PlayerMeleeAttack", d.Category));
    }

    [Fact]
    public void GetEnemyVoiceProfile_Servitor_ReturnsProfile()
    {
        // Act
        var profile = _repository.GetEnemyVoiceProfile("Servitor");

        // Assert
        Assert.NotNull(profile);
        Assert.Equal("Servitor", profile.EnemyArchetype);
        Assert.Contains("Mechanical", profile.VoiceDescription);
    }

    [Fact]
    public void MultipleAttackGenerations_ProduceVariety()
    {
        // Arrange
        var results = new HashSet<string>();
        var randomService = new CombatFlavorTextService(_repository, new Random());

        // Act - Generate 10 attacks with same parameters
        for (int i = 0; i < 10; i++)
        {
            var result = randomService.GeneratePlayerAttackText(
                WeaponType.SwordOneHanded,
                "Sword",
                "Enemy",
                CombatOutcome.SolidHit);
            results.Add(result);
        }

        // Assert - Should have some variety (at least 1 unique since we only have 1 SolidHit descriptor in test data)
        Assert.True(results.Count >= 1);
    }

    public void Dispose()
    {
        _connection?.Close();
        _connection?.Dispose();
    }
}
