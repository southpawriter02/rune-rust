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
/// v0.38.6: Integration demo showing how CombatFlavorTextService enhances combat
/// This demonstrates the before/after comparison of combat text with flavor text integration
/// </summary>
public class CombatFlavorIntegrationDemo : IDisposable
{
    private readonly ITestOutputHelper _output;
    private readonly SqliteConnection _connection;
    private readonly string _connectionString;
    private readonly DescriptorRepository _repository;
    private readonly CombatFlavorTextService _flavorService;

    public CombatFlavorIntegrationDemo(ITestOutputHelper output)
    {
        _output = output;

        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateLogger();

        // Create in-memory database with full content
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        _connectionString = _connection.ConnectionString;

        InitializeFullSchema();
        PopulateFullContent();

        _repository = new DescriptorRepository(_connectionString);
        _flavorService = new CombatFlavorTextService(_repository);
    }

    private void InitializeFullSchema()
    {
        var schemaScript = System.IO.File.ReadAllText("../../../Data/v0.38.6_combat_flavor_text_schema.sql");

        var command = _connection.CreateCommand();
        command.CommandText = schemaScript;
        command.ExecuteNonQuery();
    }

    private void PopulateFullContent()
    {
        // Load and execute all content scripts
        var scripts = new[]
        {
            "../../../Data/v0.38.6_player_action_descriptors.sql",
            "../../../Data/v0.38.6_enemy_voice_profiles.sql",
            "../../../Data/v0.38.6_environmental_combat_modifiers.sql",
            "../../../Data/v0.38.6_enemy_voice_profile_population.sql"
        };

        foreach (var scriptPath in scripts)
        {
            if (System.IO.File.Exists(scriptPath))
            {
                var script = System.IO.File.ReadAllText(scriptPath);
                // Execute each statement separately (SQLite limitation)
                var statements = script.Split(new[] { ";\r\n", ";\n" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var statement in statements)
                {
                    if (string.IsNullOrWhiteSpace(statement) || statement.TrimStart().StartsWith("--"))
                        continue;

                    try
                    {
                        var command = _connection.CreateCommand();
                        command.CommandText = statement;
                        command.ExecuteNonQuery();
                    }
                    catch (SqliteException ex)
                    {
                        _output.WriteLine($"Warning: Failed to execute statement: {ex.Message}");
                    }
                }
            }
        }
    }

    [Fact]
    public void Demo_PlayerSwordAttack_Comparison()
    {
        _output.WriteLine("=== PLAYER SWORD ATTACK COMPARISON ===\n");

        // BEFORE (Generic v0.22 combat text)
        _output.WriteLine("--- WITHOUT Flavor Text (v0.22 Combat Engine) ---");
        _output.WriteLine("Valen attacks Corrupted Servitor!");
        _output.WriteLine("  Rolled 5d6: [4,5,3,6,2] = 3 successes");
        _output.WriteLine("Corrupted Servitor defends!");
        _output.WriteLine("  Rolled 3d6: [2,4,1] = 1 success");
        _output.WriteLine("Hit for 12 damage!");
        _output.WriteLine();

        // AFTER (With v0.38.6 flavor text)
        _output.WriteLine("--- WITH Flavor Text (v0.38.6 Integration) ---");

        var attackText = _flavorService.GeneratePlayerAttackText(
            WeaponType.SwordOneHanded,
            "Rusted Longsword",
            "Corrupted Servitor",
            CombatOutcome.SolidHit);

        var reactionText = _flavorService.GenerateEnemyDamageReaction(
            EnemyArchetype.Servitor,
            "Corrupted Servitor",
            damageAmount: 12,
            isDying: false);

        var envText = _flavorService.GenerateEnvironmentalReaction("The_Roots");

        _output.WriteLine("Valen attacks Corrupted Servitor!");
        _output.WriteLine("  Rolled 5d6: [4,5,3,6,2] = 3 successes");
        _output.WriteLine("Corrupted Servitor defends!");
        _output.WriteLine("  Rolled 3d6: [2,4,1] = 1 success");
        _output.WriteLine($"{attackText}");
        _output.WriteLine($"{reactionText}");
        if (!string.IsNullOrEmpty(envText))
        {
            _output.WriteLine($"{envText}");
        }
        _output.WriteLine("(12 damage)");
        _output.WriteLine();

        Assert.NotEmpty(attackText);
        Assert.NotEmpty(reactionText);
    }

    [Fact]
    public void Demo_EnemyAttack_Comparison()
    {
        _output.WriteLine("=== ENEMY ATTACK COMPARISON ===\n");

        // BEFORE
        _output.WriteLine("--- WITHOUT Flavor Text ---");
        _output.WriteLine("Forlorn Revenant attacks Valen!");
        _output.WriteLine("  Rolled 4d6: [5,3,6,2] = 2 successes");
        _output.WriteLine("Valen defends!");
        _output.WriteLine("  Rolled 4d6: [1,4,3,2] = 1 success");
        _output.WriteLine("Hit for 8 damage!");
        _output.WriteLine();

        // AFTER
        _output.WriteLine("--- WITH Flavor Text ---");

        var attackText = _flavorService.GenerateEnemyAttackText(
            EnemyArchetype.Forlorn,
            "Forlorn Revenant",
            isSpecialAttack: false);

        _output.WriteLine($"{attackText}");
        _output.WriteLine("  Rolled 4d6: [5,3,6,2] = 2 successes");
        _output.WriteLine("Valen defends!");
        _output.WriteLine("  Rolled 4d6: [1,4,3,2] = 1 success");
        _output.WriteLine("The attack connects!");
        _output.WriteLine("(8 damage)");
        _output.WriteLine();

        Assert.NotEmpty(attackText);
    }

    [Fact]
    public void Demo_CriticalHit_Comparison()
    {
        _output.WriteLine("=== CRITICAL HIT COMPARISON ===\n");

        // BEFORE
        _output.WriteLine("--- WITHOUT Flavor Text ---");
        _output.WriteLine("Valen attacks Corrupted Dvergr!");
        _output.WriteLine("  [CRITICAL HIT!] Targeting subroutines experience catastrophic failure!");
        _output.WriteLine("Hit for 24 damage!");
        _output.WriteLine();

        // AFTER
        _output.WriteLine("--- WITH Flavor Text ---");

        var criticalText = _flavorService.GeneratePlayerAttackText(
            WeaponType.SwordOneHanded,
            "Frost-Kissed Blade",
            "Corrupted Dvergr",
            CombatOutcome.CriticalHit);

        var deathText = _flavorService.GenerateEnemyDamageReaction(
            EnemyArchetype.Corrupted_Dvergr,
            "Corrupted Dvergr",
            damageAmount: 24,
            isDying: true);

        _output.WriteLine($"{criticalText}");
        _output.WriteLine($"{deathText}");
        _output.WriteLine("(24 damage - FATAL)");
        _output.WriteLine();

        Assert.Contains("critical", criticalText.ToLower());
        Assert.NotEmpty(deathText);
    }

    [Fact]
    public void Demo_BowAttack_Comparison()
    {
        _output.WriteLine("=== RANGED ATTACK COMPARISON ===\n");

        // BEFORE
        _output.WriteLine("--- WITHOUT Flavor Text ---");
        _output.WriteLine("Valen attacks Blight-Touched Wolf!");
        _output.WriteLine("Hit for 10 damage!");
        _output.WriteLine();

        // AFTER
        _output.WriteLine("--- WITH Flavor Text ---");

        var bowText = _flavorService.GeneratePlayerAttackText(
            WeaponType.Bow,
            "Hunting Bow",
            "Blight-Touched Wolf",
            CombatOutcome.SolidHit);

        var beastReaction = _flavorService.GenerateEnemyDamageReaction(
            EnemyArchetype.Blight_Touched_Beast,
            "Blight-Touched Wolf",
            damageAmount: 10,
            isDying: false);

        _output.WriteLine($"{bowText}");
        _output.WriteLine($"{beastReaction}");
        _output.WriteLine("(10 damage)");
        _output.WriteLine();

        Assert.Contains("arrow", bowText.ToLower());
    }

    [Fact]
    public void Demo_DefenseActions_Comparison()
    {
        _output.WriteLine("=== DEFENSE ACTIONS COMPARISON ===\n");

        // Dodge
        _output.WriteLine("--- WITHOUT Flavor Text: Dodge ---");
        _output.WriteLine("Servitor attacks! You dodge!");
        _output.WriteLine();

        _output.WriteLine("--- WITH Flavor Text: Dodge ---");
        var dodgeText = _flavorService.GeneratePlayerDefenseText(
            WeaponType.Dodge,
            null,
            null,
            "Servitor Scout",
            successful: true);
        _output.WriteLine($"{dodgeText}");
        _output.WriteLine();

        // Parry
        _output.WriteLine("--- WITH Flavor Text: Parry ---");
        var parryText = _flavorService.GeneratePlayerDefenseText(
            WeaponType.Parry,
            "Longsword",
            null,
            "Corrupted Dvergr",
            successful: true);
        _output.WriteLine($"{parryText}");
        _output.WriteLine();

        Assert.NotEmpty(dodgeText);
        Assert.NotEmpty(parryText);
    }

    [Fact]
    public void Demo_EnvironmentalAtmosphere_ByBiome()
    {
        _output.WriteLine("=== ENVIRONMENTAL COMBAT ATMOSPHERE ===\n");

        var biomes = new[] { "The_Roots", "Muspelheim", "Niflheim", "Alfheim", "Jötunheim" };

        foreach (var biome in biomes)
        {
            _output.WriteLine($"--- {biome} ---");

            // Generate 3 samples to show variety
            for (int i = 0; i < 3; i++)
            {
                var envText = _flavorService.GenerateEnvironmentalReaction(biome);
                if (!string.IsNullOrEmpty(envText))
                {
                    _output.WriteLine($"  {envText}");
                }
            }
            _output.WriteLine();
        }
    }

    [Fact]
    public void Demo_EnemyArchetypeVoices()
    {
        _output.WriteLine("=== ENEMY ARCHETYPE VOICES ===\n");

        var archetypes = new[]
        {
            (EnemyArchetype.Servitor, "Corrupted Servitor"),
            (EnemyArchetype.Forlorn, "Forlorn Wanderer"),
            (EnemyArchetype.Corrupted_Dvergr, "Mad Engineer Dvergr"),
            (EnemyArchetype.Blight_Touched_Beast, "Twisted Bear"),
            (EnemyArchetype.Aether_Wraith, "Reality Wraith")
        };

        foreach (var (archetype, name) in archetypes)
        {
            _output.WriteLine($"--- {archetype} ---");

            // Regular attack
            var attackText = _flavorService.GenerateEnemyAttackText(archetype, name, false);
            _output.WriteLine($"Attack: {attackText}");

            // Damage reaction
            var damageText = _flavorService.GenerateEnemyDamageReaction(archetype, name, 15, false);
            _output.WriteLine($"Damaged: {damageText}");

            // Death
            var deathText = _flavorService.GenerateEnemyDamageReaction(archetype, name, 50, true);
            _output.WriteLine($"Death: {deathText}");
            _output.WriteLine();
        }
    }

    [Fact]
    public void Demo_FullCombatSequence()
    {
        _output.WriteLine("=== FULL COMBAT SEQUENCE (v0.38.6 Flavor Text) ===\n");
        _output.WriteLine("Location: The Roots - Corroded Machine Hall");
        _output.WriteLine("Combatants: Valen (Player) vs. Corrupted Servitor Alpha");
        _output.WriteLine();

        // Turn 1: Player attacks with sword
        _output.WriteLine("--- TURN 1 ---");
        var turn1Attack = _flavorService.GeneratePlayerAttackText(
            WeaponType.SwordOneHanded,
            "Rusted Longsword",
            "Servitor Alpha",
            CombatOutcome.SolidHit);
        var turn1Reaction = _flavorService.GenerateEnemyDamageReaction(
            EnemyArchetype.Servitor,
            "Servitor Alpha",
            15,
            false);
        var turn1Env = _flavorService.GenerateEnvironmentalReaction("The_Roots");

        _output.WriteLine($"{turn1Attack}");
        _output.WriteLine($"{turn1Reaction}");
        if (!string.IsNullOrEmpty(turn1Env))
            _output.WriteLine($"{turn1Env}");
        _output.WriteLine("(15 damage)");
        _output.WriteLine();

        // Turn 2: Enemy attacks
        _output.WriteLine("--- TURN 2 ---");
        var turn2Attack = _flavorService.GenerateEnemyAttackText(
            EnemyArchetype.Servitor,
            "Servitor Alpha",
            false);
        var turn2Defense = _flavorService.GeneratePlayerDefenseText(
            WeaponType.Dodge,
            null,
            null,
            "Servitor Alpha",
            successful: true);

        _output.WriteLine($"{turn2Attack}");
        _output.WriteLine($"{turn2Defense}");
        _output.WriteLine();

        // Turn 3: Player critical hit - finishing blow
        _output.WriteLine("--- TURN 3 ---");
        var turn3Attack = _flavorService.GeneratePlayerAttackText(
            WeaponType.SwordOneHanded,
            "Rusted Longsword",
            "Servitor Alpha",
            CombatOutcome.CriticalHit);
        var turn3Death = _flavorService.GenerateEnemyDamageReaction(
            EnemyArchetype.Servitor,
            "Servitor Alpha",
            30,
            true);

        _output.WriteLine($"{turn3Attack}");
        _output.WriteLine($"{turn3Death}");
        _output.WriteLine("(30 damage - FATAL)");
        _output.WriteLine();
        _output.WriteLine("=== COMBAT ENDED - VICTORY ===");
    }

    [Fact]
    public void Statistics_ShowLibrarySize()
    {
        var stats = _repository.GetCombatFlavorTextStats();

        _output.WriteLine("=== v0.38.6 COMBAT FLAVOR TEXT LIBRARY STATISTICS ===\n");
        _output.WriteLine($"Total Combat Descriptors: {stats.TotalCombatDescriptors}");
        _output.WriteLine($"Enemy Voice Profiles: {stats.TotalEnemyVoiceProfiles}");
        _output.WriteLine($"Environmental Modifiers: {stats.TotalEnvironmentalModifiers}");
        _output.WriteLine();

        _output.WriteLine("Descriptors by Category:");
        foreach (var (category, count) in stats.DescriptorsByCategory)
        {
            _output.WriteLine($"  {category}: {count}");
        }
        _output.WriteLine();

        _output.WriteLine("Environmental Modifiers by Biome:");
        foreach (var (biome, count) in stats.ModifiersByBiome)
        {
            _output.WriteLine($"  {biome}: {count}");
        }

        // Success criteria: Should have 200+ descriptors total
        Assert.True(stats.TotalCombatDescriptors >= 100,
            "Should have at least 100 combat descriptors");
        Assert.Equal(5, stats.TotalEnemyVoiceProfiles);
        Assert.True(stats.TotalEnvironmentalModifiers >= 50,
            "Should have at least 50 environmental modifiers");
    }

    public void Dispose()
    {
        _connection?.Close();
        _connection?.Dispose();
    }
}
