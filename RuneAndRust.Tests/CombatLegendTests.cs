using NUnit.Framework;
using RuneAndRust.Core;
using RuneAndRust.Engine;

namespace RuneAndRust.Tests;

[TestFixture]
public class CombatLegendTests
{
    private DiceService _diceService;
    private SagaService _sagaService;
    private CombatEngine _combatEngine;
    private PlayerCharacter _player;

    [SetUp]
    public void Setup()
    {
        _diceService = new DiceService();
        _sagaService = new SagaService();
        var lootService = new LootService();
        var equipmentService = new EquipmentService();
        var traumaService = new TraumaEconomyService();
        var hazardService = new HazardService(_diceService, traumaService);
        var currencyService = new CurrencyService();
        _combatEngine = new CombatEngine(_diceService, _sagaService, lootService, equipmentService, hazardService, currencyService);

        _player = new PlayerCharacter
        {
            Name = "Test Hero",
            Class = CharacterClass.Warrior,
            CurrentMilestone = 0,
            CurrentLegend = 0,
            LegendToNextMilestone = 100,
            ProgressionPoints = 2,
            MaxHP = 50,
            HP = 50,
            MaxStamina = 30,
            Stamina = 30,
            AP = 10,
            Attributes = new Attributes(3, 3, 2, 2, 3),
            WeaponName = "Test Sword",
            WeaponAttribute = "might",
            BaseDamage = 1,
            Abilities = new List<Ability>()
        };
    }

    [Test]
    public void AwardCombatLegend_SingleEnemy_NormalCombat()
    {
        // Arrange
        var enemies = new List<Enemy>
        {
            new Enemy
            {
                Name = "Corrupted Servitor",
                BaseLegendValue = 10,
                HP = 0, // Defeated
                MaxHP = 15,
                HP = 0
            }
        };

        var combat = _combatEngine.InitializeCombat(_player, enemies);

        // Act
        bool milestoneReached = _combatEngine.AwardCombatLegend(combat, 1.0f);

        // Assert
        Assert.That(_player.CurrentLegend, Is.EqualTo(10));
        Assert.That(milestoneReached, Is.False); // Not enough for milestone
    }

    [Test]
    public void AwardCombatLegend_SingleEnemy_BlightCombat()
    {
        // Arrange
        var enemies = new List<Enemy>
        {
            new Enemy
            {
                Name = "Blight-Drone",
                BaseLegendValue = 25,
                HP = 0,
                MaxHP = 25,
                HP = 0
            }
        };

        var combat = _combatEngine.InitializeCombat(_player, enemies);

        // Act
        bool milestoneReached = _combatEngine.AwardCombatLegend(combat, 1.25f);

        // Assert
        // 25 * 1.0 * 1.25 = 31 (rounded down from 31.25)
        Assert.That(_player.CurrentLegend, Is.EqualTo(31));
    }

    [Test]
    public void AwardCombatLegend_BossFight_WithTrauma()
    {
        // Arrange
        var enemies = new List<Enemy>
        {
            new Enemy
            {
                Name = "Ruin-Warden",
                BaseLegendValue = 100,
                HP = 0,
                MaxHP = 80,
                HP = 0,
                IsBoss = true
            }
        };

        var combat = _combatEngine.InitializeCombat(_player, enemies);

        // Act
        bool milestoneReached = _combatEngine.AwardCombatLegend(combat, 1.25f);

        // Assert
        // 100 * 1.0 * 1.25 = 125
        Assert.That(_player.CurrentLegend, Is.EqualTo(125));
        Assert.That(milestoneReached, Is.True); // Exceeds 100 Legend threshold
    }

    [Test]
    public void AwardCombatLegend_MultipleEnemies_CumulativeAward()
    {
        // Arrange
        var enemies = new List<Enemy>
        {
            new Enemy { Name = "Servitor 1", BaseLegendValue = 10, HP = 0 },
            new Enemy { Name = "Servitor 2", BaseLegendValue = 10, HP = 0 },
            new Enemy { Name = "Drone", BaseLegendValue = 25, HP = 0 }
        };

        var combat = _combatEngine.InitializeCombat(_player, enemies);

        // Act
        bool milestoneReached = _combatEngine.AwardCombatLegend(combat, 1.0f);

        // Assert
        Assert.That(_player.CurrentLegend, Is.EqualTo(45)); // 10 + 10 + 25
    }

    [Test]
    public void AwardCombatLegend_OnlyDefeatedEnemies_CountTowardLegend()
    {
        // Arrange
        var enemies = new List<Enemy>
        {
            new Enemy { Name = "Defeated", BaseLegendValue = 10, HP = 0 },
            new Enemy { Name = "Alive", BaseLegendValue = 25, HP = 10 }
        };

        var combat = _combatEngine.InitializeCombat(_player, enemies);

        // Act
        _combatEngine.AwardCombatLegend(combat, 1.0f);

        // Assert
        Assert.That(_player.CurrentLegend, Is.EqualTo(10)); // Only defeated enemy
    }

    [Test]
    public void AwardCombatLegend_ReachingMilestone_SignalsCorrectly()
    {
        // Arrange
        _player.CurrentLegend = 90; // Close to milestone
        var enemies = new List<Enemy>
        {
            new Enemy { Name = "Servitor", BaseLegendValue = 10, HP = 0 }
        };

        var combat = _combatEngine.InitializeCombat(_player, enemies);

        // Act
        bool milestoneReached = _combatEngine.AwardCombatLegend(combat, 1.0f);

        // Assert
        Assert.That(_player.CurrentLegend, Is.EqualTo(100));
        Assert.That(milestoneReached, Is.True);
        Assert.That(_sagaService.CanReachMilestone(_player), Is.True);
    }

    [Test]
    public void AwardCombatLegend_NoEnemiesDefeated_AwardsZeroLegend()
    {
        // Arrange
        var enemies = new List<Enemy>
        {
            new Enemy { Name = "Alive Enemy", BaseLegendValue = 100, HP = 10 }
        };

        var combat = _combatEngine.InitializeCombat(_player, enemies);

        // Act
        bool milestoneReached = _combatEngine.AwardCombatLegend(combat, 1.0f);

        // Assert
        Assert.That(_player.CurrentLegend, Is.EqualTo(0));
        Assert.That(milestoneReached, Is.False);
    }

    [Test]
    public void IsCombatOver_AllEnemiesDefeated_AwardsLegend()
    {
        // Arrange
        var enemies = new List<Enemy>
        {
            new Enemy
            {
                Name = "Servitor",
                BaseLegendValue = 10,
                HP = 1,
                MaxHP = 15,
                IsAlive = true,
                Attributes = new Attributes(2, 2, 0, 0, 2)
            }
        };

        var combat = _combatEngine.InitializeCombat(_player, enemies);

        // Defeat the enemy
        enemies[0].HP = 0;

        // Act
        bool isOver = _combatEngine.IsCombatOver(combat);

        // Assert
        Assert.That(isOver, Is.True);
        Assert.That(_player.CurrentLegend, Is.GreaterThan(0)); // Legend awarded
    }

    [Test]
    public void CombatLegend_Progression_IntegrationTest()
    {
        // Scenario: Player fights through multiple encounters to reach Milestone 1
        _player.CurrentLegend = 0;
        _player.CurrentMilestone = 0;
        _player.LegendToNextMilestone = 100;

        // Fight 1: 2 Servitors (normal combat)
        var fight1 = new List<Enemy>
        {
            new Enemy { BaseLegendValue = 10, HP = 0 },
            new Enemy { BaseLegendValue = 10, HP = 0 }
        };
        var combat1 = _combatEngine.InitializeCombat(_player, fight1);
        _combatEngine.AwardCombatLegend(combat1, 1.0f);
        Assert.That(_player.CurrentLegend, Is.EqualTo(20)); // 10 + 10

        // Fight 2: Blight-Drone (blight area, 1.25x trauma)
        var fight2 = new List<Enemy>
        {
            new Enemy { BaseLegendValue = 25, HP = 0 }
        };
        var combat2 = _combatEngine.InitializeCombat(_player, fight2);
        _combatEngine.AwardCombatLegend(combat2, 1.25f);
        Assert.That(_player.CurrentLegend, Is.EqualTo(51)); // 20 + 31

        // Fight 3: Another Blight-Drone
        var fight3 = new List<Enemy>
        {
            new Enemy { BaseLegendValue = 25, HP = 0 }
        };
        var combat3 = _combatEngine.InitializeCombat(_player, fight3);
        _combatEngine.AwardCombatLegend(combat3, 1.25f);
        Assert.That(_player.CurrentLegend, Is.EqualTo(82)); // 51 + 31

        // Fight 4: 2 more Servitors (normal)
        var fight4 = new List<Enemy>
        {
            new Enemy { BaseLegendValue = 10, HP = 0 },
            new Enemy { BaseLegendValue = 10, HP = 0 }
        };
        var combat4 = _combatEngine.InitializeCombat(_player, fight4);
        bool milestoneReached = _combatEngine.AwardCombatLegend(combat4, 1.0f);

        Assert.That(_player.CurrentLegend, Is.EqualTo(102)); // 82 + 20
        Assert.That(milestoneReached, Is.True);
        Assert.That(_sagaService.CanReachMilestone(_player), Is.True);
    }
}
