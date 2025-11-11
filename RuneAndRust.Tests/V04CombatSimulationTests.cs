using RuneAndRust.Core;
using RuneAndRust.Engine;
using Xunit;
using Xunit.Abstractions;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.4 Combat Simulation Tests
/// Generates statistical data for balance analysis and playtest guide
/// </summary>
public class V04CombatSimulationTests
{
    private readonly ITestOutputHelper _output;
    private readonly DiceService _diceService;
    private readonly CombatEngine _combatEngine;
    private readonly EnemyAI _enemyAI;

    public V04CombatSimulationTests(ITestOutputHelper output)
    {
        _output = output;
        _diceService = new DiceService();
        _enemyAI = new EnemyAI(_diceService);
        var sagaService = new SagaService();
        var lootService = new LootService();
        var equipmentService = new EquipmentService();
        var traumaService = new TraumaEconomyService();
        var hazardService = new HazardService(_diceService, traumaService);
        _combatEngine = new CombatEngine(_diceService, sagaService, lootService, equipmentService, hazardService);
    }

    #region Enemy Factory Validation

    [Theory]
    [InlineData(EnemyType.ScrapHound, 10, "Scrap-Hound")]
    [InlineData(EnemyType.TestSubject, 15, "Test Subject")]
    [InlineData(EnemyType.WarFrame, 50, "War-Frame")]
    [InlineData(EnemyType.ForlornScholar, 30, "Forlorn Scholar")]
    [InlineData(EnemyType.AethericAberration, 60, "Aetheric Aberration")]
    public void V04_NewEnemies_ShouldInstantiateCorrectly(EnemyType type, int expectedHP, string expectedName)
    {
        // Arrange & Act
        var enemy = EnemyFactory.CreateEnemy(type);

        // Assert
        Assert.NotNull(enemy);
        Assert.Equal(type, enemy.Type);
        Assert.Equal(expectedHP, enemy.HP);
        Assert.Equal(expectedHP, enemy.MaxHP);
        Assert.Equal(expectedName, enemy.Name);
        Assert.True(enemy.LegendValue > 0, "Enemy should grant Legend");
    }

    [Fact]
    public void V04_AllNewEnemies_ShouldHaveValidAttributes()
    {
        // Arrange
        var newEnemyTypes = new[]
        {
            EnemyType.ScrapHound,
            EnemyType.TestSubject,
            EnemyType.WarFrame,
            EnemyType.ForlornScholar,
            EnemyType.AethericAberration
        };

        // Act & Assert
        foreach (var type in newEnemyTypes)
        {
            var enemy = EnemyFactory.CreateEnemy(type);

            Assert.True(enemy.Attributes.Might >= 0, $"{type} has invalid MIGHT");
            Assert.True(enemy.Attributes.Finesse >= 0, $"{type} has invalid FINESSE");
            Assert.True(enemy.Attributes.Sturdiness >= 0, $"{type} has invalid STURDINESS");
            Assert.True(enemy.BaseDamageDice > 0, $"{type} has no damage dice");
        }
    }

    #endregion

    #region Combat Simulation - Individual Encounters

    [Fact]
    public void V04_Warrior_VsWarFrame_SimulateCombat()
    {
        // Arrange
        var iterations = 100;
        var results = new CombatSimulationResult("Warrior vs War-Frame", iterations);

        // Act
        for (int i = 0; i < iterations; i++)
        {
            var player = CharacterFactory.CreateCharacter(CharacterClass.Warrior, "TestWarrior");
            player.MaxHP = 40; // Assume some progression
            player.HP = 40;

            var enemy = EnemyFactory.CreateEnemy(EnemyType.WarFrame);
            var combatResult = SimulateCombat(player, new List<Enemy> { enemy });

            results.RecordResult(combatResult);
        }

        // Assert & Output
        _output.WriteLine("=== WARRIOR VS WAR-FRAME (100 simulations) ===");
        OutputCombatResults(results);

        Assert.True(results.WinRate > 0.5, "Warrior should have >50% win rate vs War-Frame");
    }

    [Fact]
    public void V04_Mystic_VsAethericAberration_SimulateCombat()
    {
        // Arrange
        var iterations = 100;
        var results = new CombatSimulationResult("Mystic vs Aetheric Aberration", iterations);

        // Act
        for (int i = 0; i < iterations; i++)
        {
            var player = CharacterFactory.CreateCharacter(CharacterClass.Mystic, "TestMystic");
            player.MaxHP = 35; // Mystics have lower HP
            player.HP = 35;
            player.Attributes.Will = 5; // High WILL for magic defense

            var enemy = EnemyFactory.CreateEnemy(EnemyType.AethericAberration);
            var combatResult = SimulateCombat(player, new List<Enemy> { enemy });

            results.RecordResult(combatResult);
        }

        // Assert & Output
        _output.WriteLine("=== MYSTIC VS AETHERIC ABERRATION (100 simulations) ===");
        OutputCombatResults(results);

        Assert.True(results.WinRate > 0.3, "Mystic should have reasonable chance vs Aberration");
    }

    [Fact]
    public void V04_Scavenger_VsTestSubject_SimulateCombat()
    {
        // Arrange
        var iterations = 100;
        var results = new CombatSimulationResult("Scavenger vs 2x Test Subject", iterations);

        // Act
        for (int i = 0; i < iterations; i++)
        {
            var player = CharacterFactory.CreateCharacter(CharacterClass.Scavenger, "TestScavenger");
            player.MaxHP = 35;
            player.HP = 35;

            var enemies = new List<Enemy>
            {
                EnemyFactory.CreateEnemy(EnemyType.TestSubject),
                EnemyFactory.CreateEnemy(EnemyType.TestSubject)
            };

            var combatResult = SimulateCombat(player, enemies);
            results.RecordResult(combatResult);
        }

        // Assert & Output
        _output.WriteLine("=== SCAVENGER VS 2X TEST SUBJECT (100 simulations) ===");
        OutputCombatResults(results);

        Assert.True(results.WinRate > 0.6, "Scavenger should handle Test Subjects well");
    }

    #endregion

    #region Path Simulation

    [Fact]
    public void V04_EastWing_FullPathSimulation()
    {
        // Arrange
        var iterations = 50;
        var pathResults = new PathSimulationResult("East Wing (Combat Path)", iterations);

        // Act
        for (int i = 0; i < iterations; i++)
        {
            var player = CharacterFactory.CreateCharacter(CharacterClass.Warrior, "TestWarrior");
            player.MaxHP = 30;
            player.HP = 30;

            // Room 2: Corridor (2x Servitor)
            var room2 = SimulateCombat(player, new List<Enemy>
            {
                EnemyFactory.CreateEnemy(EnemyType.CorruptedServitor),
                EnemyFactory.CreateEnemy(EnemyType.CorruptedServitor)
            });
            pathResults.AddEncounter(room2);
            if (!room2.PlayerWon) { pathResults.RecordDeath(2); continue; }

            // Room 3: Salvage Bay (1x Servitor + 1x Scrap-Hound)
            var room3 = SimulateCombat(player, new List<Enemy>
            {
                EnemyFactory.CreateEnemy(EnemyType.CorruptedServitor),
                EnemyFactory.CreateEnemy(EnemyType.ScrapHound)
            });
            pathResults.AddEncounter(room3);
            if (!room3.PlayerWon) { pathResults.RecordDeath(3); continue; }

            // Room 5: Arsenal (3x Blight-Drone)
            var room5 = SimulateCombat(player, new List<Enemy>
            {
                EnemyFactory.CreateEnemy(EnemyType.BlightDrone),
                EnemyFactory.CreateEnemy(EnemyType.BlightDrone),
                EnemyFactory.CreateEnemy(EnemyType.BlightDrone)
            });
            pathResults.AddEncounter(room5);
            if (!room5.PlayerWon) { pathResults.RecordDeath(5); continue; }

            // Room 6: Training Chamber (War-Frame mini-boss)
            var room6 = SimulateCombat(player, new List<Enemy>
            {
                EnemyFactory.CreateEnemy(EnemyType.WarFrame)
            });
            pathResults.AddEncounter(room6);
            if (!room6.PlayerWon) { pathResults.RecordDeath(6); continue; }

            // Room 7: Ammunition Forge (2x Blight-Drone + hazard)
            var room7Combat = SimulateCombat(player, new List<Enemy>
            {
                EnemyFactory.CreateEnemy(EnemyType.BlightDrone),
                EnemyFactory.CreateEnemy(EnemyType.BlightDrone)
            });
            // Simulate environmental hazard damage (average 3.5 per turn)
            var hazardDamage = (int)(room7Combat.TurnsToComplete * 3.5);
            player.HP -= hazardDamage;
            room7Combat.PlayerHPRemaining -= hazardDamage;
            pathResults.AddEncounter(room7Combat);
            if (!room7Combat.PlayerWon || player.HP <= 0) { pathResults.RecordDeath(7); continue; }

            // Room 11: Vault Antechamber (3x Blight-Drone + 1x Scrap-Hound)
            var room11 = SimulateCombat(player, new List<Enemy>
            {
                EnemyFactory.CreateEnemy(EnemyType.BlightDrone),
                EnemyFactory.CreateEnemy(EnemyType.BlightDrone),
                EnemyFactory.CreateEnemy(EnemyType.BlightDrone),
                EnemyFactory.CreateEnemy(EnemyType.ScrapHound)
            });
            pathResults.AddEncounter(room11);
            if (!room11.PlayerWon) { pathResults.RecordDeath(11); continue; }

            // Boss: Ruin-Warden
            var bossFight = SimulateCombat(player, new List<Enemy>
            {
                EnemyFactory.CreateEnemy(EnemyType.RuinWarden)
            });
            pathResults.AddEncounter(bossFight);
            if (!bossFight.PlayerWon) { pathResults.RecordDeath(14); continue; }

            pathResults.RecordCompletion();
        }

        // Assert & Output
        _output.WriteLine("=== EAST WING FULL PATH SIMULATION (50 runs) ===");
        OutputPathResults(pathResults);

        Assert.True(pathResults.CompletionRate > 0.1, "At least 10% should complete East Wing");
    }

    [Fact]
    public void V04_WestWing_FullPathSimulation()
    {
        // Arrange
        var iterations = 50;
        var pathResults = new PathSimulationResult("West Wing (Exploration Path)", iterations);

        // Act
        for (int i = 0; i < iterations; i++)
        {
            var player = CharacterFactory.CreateCharacter(CharacterClass.Scavenger, "TestScavenger");
            player.MaxHP = 30;
            player.HP = 30;

            // Room 2: Corridor (2x Servitor)
            var room2 = SimulateCombat(player, new List<Enemy>
            {
                EnemyFactory.CreateEnemy(EnemyType.CorruptedServitor),
                EnemyFactory.CreateEnemy(EnemyType.CorruptedServitor)
            });
            pathResults.AddEncounter(room2);
            if (!room2.PlayerWon) { pathResults.RecordDeath(2); continue; }

            // Room 3: Salvage Bay
            var room3 = SimulateCombat(player, new List<Enemy>
            {
                EnemyFactory.CreateEnemy(EnemyType.CorruptedServitor),
                EnemyFactory.CreateEnemy(EnemyType.ScrapHound)
            });
            pathResults.AddEncounter(room3);
            if (!room3.PlayerWon) { pathResults.RecordDeath(3); continue; }

            // Room 8: Research Archives (puzzle, no combat)
            // Skip - puzzle only

            // Room 9: Specimen Containment (2x Test Subject)
            var room9 = SimulateCombat(player, new List<Enemy>
            {
                EnemyFactory.CreateEnemy(EnemyType.TestSubject),
                EnemyFactory.CreateEnemy(EnemyType.TestSubject)
            });
            pathResults.AddEncounter(room9);
            if (!room9.PlayerWon) { pathResults.RecordDeath(9); continue; }

            // Room 10: Observation Deck (Forlorn Scholar - assume combat)
            var room10 = SimulateCombat(player, new List<Enemy>
            {
                EnemyFactory.CreateEnemy(EnemyType.ForlornScholar)
            });
            pathResults.AddEncounter(room10);
            if (!room10.PlayerWon) { pathResults.RecordDeath(10); continue; }

            // Room 11: Vault Antechamber
            var room11 = SimulateCombat(player, new List<Enemy>
            {
                EnemyFactory.CreateEnemy(EnemyType.BlightDrone),
                EnemyFactory.CreateEnemy(EnemyType.BlightDrone),
                EnemyFactory.CreateEnemy(EnemyType.BlightDrone),
                EnemyFactory.CreateEnemy(EnemyType.ScrapHound)
            });
            pathResults.AddEncounter(room11);
            if (!room11.PlayerWon) { pathResults.RecordDeath(11); continue; }

            // Boss: Ruin-Warden
            var bossFight = SimulateCombat(player, new List<Enemy>
            {
                EnemyFactory.CreateEnemy(EnemyType.RuinWarden)
            });
            pathResults.AddEncounter(bossFight);
            if (!bossFight.PlayerWon) { pathResults.RecordDeath(14); continue; }

            pathResults.RecordCompletion();
        }

        // Assert & Output
        _output.WriteLine("=== WEST WING FULL PATH SIMULATION (50 runs) ===");
        OutputPathResults(pathResults);

        Assert.True(pathResults.CompletionRate > 0.1, "At least 10% should complete West Wing");
    }

    #endregion

    #region Environmental Hazard Tests

    [Fact]
    public void V04_EnvironmentalHazard_ShouldApplyDamagePerTurn()
    {
        // Arrange
        var player = CharacterFactory.CreateCharacter(CharacterClass.Warrior, "TestWarrior");
        player.HP = 50;
        var startingHP = player.HP;

        var room = new Room
        {
            Name = "Ammunition Forge",
            HasEnvironmentalHazard = true,
            IsHazardActive = true,
            HazardDamagePerTurn = 6, // 1d6 average ~3.5
            Enemies = new List<Enemy> { EnemyFactory.CreateEnemy(EnemyType.BlightDrone) }
        };

        var combat = _combatEngine.InitializeCombat(player, room.Enemies, room, true);

        // Act - Simulate 3 turns of combat
        int turns = 0;
        while (combat.IsActive && turns < 3 && player.HP > 0)
        {
            if (combat.IsPlayerTurn())
            {
                // Player attacks
                _combatEngine.ProcessPlayerAttack(combat, 0);
            }
            else
            {
                // Enemy turn
                _combatEngine.ProcessEnemyTurn(combat);
            }

            turns++;
        }

        // Assert
        var damageTaken = startingHP - player.HP;
        _output.WriteLine($"Environmental hazard test:");
        _output.WriteLine($"  Starting HP: {startingHP}");
        _output.WriteLine($"  Final HP: {player.HP}");
        _output.WriteLine($"  Damage taken: {damageTaken}");
        _output.WriteLine($"  Turns simulated: {turns}");

        Assert.True(damageTaken > 0, "Player should take damage from hazard");
    }

    #endregion

    #region Enemy AI Behavior Tests

    [Fact]
    public void V04_WarFrame_ShouldUseVariedAbilities()
    {
        // Arrange
        var warFrame = EnemyFactory.CreateEnemy(EnemyType.WarFrame);
        var actionsUsed = new HashSet<EnemyAction>();
        var iterations = 100;

        // Act
        for (int i = 0; i < iterations; i++)
        {
            warFrame.HP = warFrame.MaxHP; // Reset HP
            var action = _enemyAI.DetermineAction(warFrame);
            actionsUsed.Add(action);
        }

        // Assert
        _output.WriteLine($"War-Frame AI Variety Test ({iterations} iterations):");
        _output.WriteLine($"  Unique actions used: {actionsUsed.Count}");
        foreach (var action in actionsUsed)
        {
            _output.WriteLine($"    - {action}");
        }

        Assert.True(actionsUsed.Count >= 3, "War-Frame should use at least 3 different abilities");
        Assert.Contains(EnemyAction.PrecisionStrike, actionsUsed);
    }

    [Fact]
    public void V04_AethericAberration_ShouldChangePhases()
    {
        // Arrange
        var aberration = EnemyFactory.CreateEnemy(EnemyType.AethericAberration);
        var phase1Actions = new HashSet<EnemyAction>();
        var phase2Actions = new HashSet<EnemyAction>();

        // Act - Phase 1 (100% HP)
        aberration.HP = aberration.MaxHP;
        for (int i = 0; i < 50; i++)
        {
            var action = _enemyAI.DetermineAction(aberration);
            phase1Actions.Add(action);
        }

        // Act - Phase 2 (25% HP)
        aberration.HP = aberration.MaxHP / 4;
        for (int i = 0; i < 50; i++)
        {
            var action = _enemyAI.DetermineAction(aberration);
            phase2Actions.Add(action);
        }

        // Assert
        _output.WriteLine("Aetheric Aberration Phase Transition Test:");
        _output.WriteLine($"  Phase 1 actions: {string.Join(", ", phase1Actions)}");
        _output.WriteLine($"  Phase 2 actions: {string.Join(", ", phase2Actions)}");

        Assert.Contains(EnemyAction.VoidBlast, phase1Actions);
        Assert.Contains(EnemyAction.AethericStorm, phase2Actions);
        Assert.DoesNotContain(EnemyAction.AethericStorm, phase1Actions);
    }

    #endregion

    #region Helper Methods

    private CombatResult SimulateCombat(PlayerCharacter player, List<Enemy> enemies)
    {
        var result = new CombatResult
        {
            PlayerStartingHP = player.HP,
            TotalEnemyHP = enemies.Sum(e => e.HP)
        };

        var combat = _combatEngine.InitializeCombat(player, enemies, null, true);
        int turns = 0;
        const int maxTurns = 100; // Prevent infinite loops

        while (combat.IsActive && turns < maxTurns)
        {
            if (combat.IsPlayerTurn())
            {
                // Simple AI: attack first enemy
                if (combat.Enemies.Count > 0)
                {
                    _combatEngine.ProcessPlayerAttack(combat, 0);
                }
            }
            else
            {
                _combatEngine.ProcessEnemyTurn(combat);
            }

            turns++;

            // Check for combat end
            if (_combatEngine.IsCombatOver(combat))
            {
                break;
            }
        }

        result.TurnsToComplete = turns;
        result.PlayerHPRemaining = player.HP;
        result.PlayerWon = player.IsAlive;
        result.DamageTaken = result.PlayerStartingHP - result.PlayerHPRemaining;

        return result;
    }

    private void OutputCombatResults(CombatSimulationResult results)
    {
        _output.WriteLine($"Encounter: {results.EncounterName}");
        _output.WriteLine($"Iterations: {results.TotalSimulations}");
        _output.WriteLine($"Win Rate: {results.WinRate:P1} ({results.Wins}/{results.TotalSimulations})");
        _output.WriteLine($"Avg Turns: {results.AverageTurns:F1}");
        _output.WriteLine($"Avg HP Remaining: {results.AverageHPRemaining:F1}");
        _output.WriteLine($"Avg Damage Taken: {results.AverageDamageTaken:F1}");
        _output.WriteLine($"Min HP Remaining: {results.MinHPRemaining}");
        _output.WriteLine($"Max Turns: {results.MaxTurns}");
        _output.WriteLine("");
    }

    private void OutputPathResults(PathSimulationResult results)
    {
        _output.WriteLine($"Path: {results.PathName}");
        _output.WriteLine($"Iterations: {results.TotalSimulations}");
        _output.WriteLine($"Completion Rate: {results.CompletionRate:P1} ({results.Completions}/{results.TotalSimulations})");
        _output.WriteLine($"Total Encounters: {results.TotalEncounters}");
        _output.WriteLine($"Avg Turns per Encounter: {results.AverageTurnsPerEncounter:F1}");
        _output.WriteLine($"Most Dangerous Room: {results.MostDangerousRoom}");
        _output.WriteLine($"Death Distribution:");

        foreach (var kvp in results.DeathsByRoom.OrderBy(x => x.Key))
        {
            _output.WriteLine($"  Room {kvp.Key}: {kvp.Value} deaths");
        }

        _output.WriteLine("");
    }

    #endregion

    #region Result Data Structures

    private class CombatResult
    {
        public bool PlayerWon { get; set; }
        public int PlayerStartingHP { get; set; }
        public int PlayerHPRemaining { get; set; }
        public int TotalEnemyHP { get; set; }
        public int TurnsToComplete { get; set; }
        public int DamageTaken { get; set; }
    }

    private class CombatSimulationResult
    {
        public string EncounterName { get; }
        public int TotalSimulations { get; }
        public int Wins { get; private set; }
        public List<int> HPRemaining { get; } = new();
        public List<int> Turns { get; } = new();
        public List<int> DamageTaken { get; } = new();

        public CombatSimulationResult(string name, int total)
        {
            EncounterName = name;
            TotalSimulations = total;
        }

        public void RecordResult(CombatResult result)
        {
            if (result.PlayerWon) Wins++;
            HPRemaining.Add(result.PlayerHPRemaining);
            Turns.Add(result.TurnsToComplete);
            DamageTaken.Add(result.DamageTaken);
        }

        public double WinRate => (double)Wins / TotalSimulations;
        public double AverageTurns => Turns.Any() ? Turns.Average() : 0;
        public double AverageHPRemaining => HPRemaining.Any() ? HPRemaining.Average() : 0;
        public double AverageDamageTaken => DamageTaken.Any() ? DamageTaken.Average() : 0;
        public int MinHPRemaining => HPRemaining.Any() ? HPRemaining.Min() : 0;
        public int MaxTurns => Turns.Any() ? Turns.Max() : 0;
    }

    private class PathSimulationResult
    {
        public string PathName { get; }
        public int TotalSimulations { get; }
        public int Completions { get; private set; }
        public Dictionary<int, int> DeathsByRoom { get; } = new();
        private List<CombatResult> _allEncounters = new();

        public PathSimulationResult(string name, int total)
        {
            PathName = name;
            TotalSimulations = total;
        }

        public void AddEncounter(CombatResult result)
        {
            _allEncounters.Add(result);
        }

        public void RecordDeath(int roomNumber)
        {
            if (!DeathsByRoom.ContainsKey(roomNumber))
                DeathsByRoom[roomNumber] = 0;

            DeathsByRoom[roomNumber]++;
        }

        public void RecordCompletion()
        {
            Completions++;
        }

        public double CompletionRate => (double)Completions / TotalSimulations;
        public int TotalEncounters => _allEncounters.Count;
        public double AverageTurnsPerEncounter =>
            _allEncounters.Any() ? _allEncounters.Average(e => e.TurnsToComplete) : 0;

        public int MostDangerousRoom =>
            DeathsByRoom.Any() ? DeathsByRoom.OrderByDescending(kvp => kvp.Value).First().Key : 0;
    }

    #endregion
}
