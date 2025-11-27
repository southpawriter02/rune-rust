using NUnit.Framework;
using RuneAndRust.Core;
using RuneAndRust.Core.Population;
using RuneAndRust.Engine;

namespace RuneAndRust.Tests;

[TestFixture]
public class LootServiceTests
{
    private LootService _lootService;

    [SetUp]
    public void Setup()
    {
        _lootService = new LootService();
    }

    #region GenerateLoot Tests

    [Test]
    public void GenerateLoot_CorruptedServitor_GeneratesAppropriateQuality()
    {
        // Arrange
        var enemy = CreateTestEnemy("Corrupted Servitor", 1);
        var player = CreateTestPlayer(CharacterClass.Warrior);

        // Act & Assert - Run multiple times to test probabilistic behavior
        var lootCounts = new Dictionary<QualityTier, int>
        {
            { QualityTier.JuryRigged, 0 },
            { QualityTier.Scavenged, 0 }
        };

        int iterations = 100;
        int nullCount = 0;

        for (int i = 0; i < iterations; i++)
        {
            var loot = _lootService.GenerateLoot(enemy, player);
            if (loot == null)
            {
                nullCount++;
            }
            else
            {
                if (lootCounts.ContainsKey(loot.Quality))
                {
                    lootCounts[loot.Quality]++;
                }
            }
        }

        // Assert - Corrupted Servitor should drop mostly Jury-Rigged or Scavenged
        // Drop table: 60% Jury-Rigged, 30% Scavenged, 10% Nothing
        Assert.That(lootCounts[QualityTier.JuryRigged], Is.GreaterThan(30)); // Should be ~60
        Assert.That(lootCounts[QualityTier.Scavenged], Is.GreaterThan(10)); // Should be ~30
        Assert.That(nullCount, Is.GreaterThan(0)); // Should be ~10
    }

    [Test]
    public void GenerateLoot_BlightDrone_GeneratesBetterQuality()
    {
        // Arrange
        var enemy = CreateTestEnemy("Blight-Drone", 2);
        var player = CreateTestPlayer(CharacterClass.Scavenger);

        // Act & Assert
        var lootCounts = new Dictionary<QualityTier, int>
        {
            { QualityTier.Scavenged, 0 },
            { QualityTier.ClanForged, 0 },
            { QualityTier.Optimized, 0 }
        };

        int iterations = 100;

        for (int i = 0; i < iterations; i++)
        {
            var loot = _lootService.GenerateLoot(enemy, player);
            if (loot != null && lootCounts.ContainsKey(loot.Quality))
            {
                lootCounts[loot.Quality]++;
            }
        }

        // Assert - Blight-Drone should drop Scavenged, Clan-Forged, or Optimized
        // Drop table: 40% Scavenged, 40% Clan-Forged, 20% Optimized
        Assert.That(lootCounts[QualityTier.Scavenged], Is.GreaterThan(20)); // Should be ~40
        Assert.That(lootCounts[QualityTier.ClanForged], Is.GreaterThan(20)); // Should be ~40
        Assert.That(lootCounts[QualityTier.Optimized], Is.GreaterThan(5)); // Should be ~20
    }

    [Test]
    public void GenerateLoot_RuinWarden_GeneratesLegendaryQuality()
    {
        // Arrange
        var enemy = CreateTestEnemy("Ruin-Warden", 3);
        enemy.IsBoss = true;
        var player = CreateTestPlayer(CharacterClass.Mystic);

        // Act & Assert
        var lootCounts = new Dictionary<QualityTier, int>
        {
            { QualityTier.Optimized, 0 },
            { QualityTier.MythForged, 0 }
        };

        int iterations = 100;

        for (int i = 0; i < iterations; i++)
        {
            var loot = _lootService.GenerateLoot(enemy, player);
            if (loot != null && lootCounts.ContainsKey(loot.Quality))
            {
                lootCounts[loot.Quality]++;
            }
        }

        // Assert - Ruin-Warden always drops loot (30% Optimized, 70% Myth-Forged)
        Assert.That(lootCounts[QualityTier.Optimized], Is.GreaterThan(15)); // Should be ~30
        Assert.That(lootCounts[QualityTier.MythForged], Is.GreaterThan(50)); // Should be ~70
        Assert.That(lootCounts[QualityTier.Optimized] + lootCounts[QualityTier.MythForged], Is.EqualTo(iterations));
    }

    [Test]
    public void GenerateLoot_ClassAppropriateWeapons_MatchesPlayerClass()
    {
        // Arrange
        var enemy = CreateTestEnemy("Blight-Drone", 2);
        var warrior = CreateTestPlayer(CharacterClass.Warrior);
        var scavenger = CreateTestPlayer(CharacterClass.Scavenger);
        var mystic = CreateTestPlayer(CharacterClass.Mystic);

        // Act
        int iterations = 50;
        var warriorWeapons = 0;
        var scavengerWeapons = 0;
        var mysticWeapons = 0;

        for (int i = 0; i < iterations; i++)
        {
            var warriorLoot = _lootService.GenerateLoot(enemy, warrior);
            if (warriorLoot?.Type == EquipmentType.Weapon)
            {
                // Warrior weapons should be Axe or Greatsword
                Assert.That(warriorLoot.WeaponCategory, Is.EqualTo(Core.WeaponCategory.Axe).Or.EqualTo(Core.WeaponCategory.Greatsword));
                warriorWeapons++;
            }

            var scavengerLoot = _lootService.GenerateLoot(enemy, scavenger);
            if (scavengerLoot?.Type == EquipmentType.Weapon)
            {
                // Scavenger weapons should be Spear or Dagger
                Assert.That(scavengerLoot.WeaponCategory, Is.EqualTo(Core.WeaponCategory.Spear).Or.EqualTo(Core.WeaponCategory.Dagger));
                scavengerWeapons++;
            }

            var mysticLoot = _lootService.GenerateLoot(enemy, mystic);
            if (mysticLoot?.Type == EquipmentType.Weapon)
            {
                // Mystic weapons should be Staff or Focus
                Assert.That(mysticLoot.WeaponCategory, Is.EqualTo(Core.WeaponCategory.Staff).Or.EqualTo(Core.WeaponCategory.Focus));
                mysticWeapons++;
            }
        }

        // Assert - Should have generated at least some class-appropriate weapons
        Assert.That(warriorWeapons, Is.GreaterThan(0));
        Assert.That(scavengerWeapons, Is.GreaterThan(0));
        Assert.That(mysticWeapons, Is.GreaterThan(0));
    }

    #endregion

    #region Starting Loot Tests

    [Test]
    public void CreateStartingWeapon_Warrior_ReturnsAxe()
    {
        // Arrange
        var warriorClass = CharacterClass.Warrior;

        // Act
        var weapon = _lootService.CreateStartingWeapon(warriorClass);

        // Assert
        Assert.That(weapon, Is.Not.Null);
        Assert.That(weapon.Type, Is.EqualTo(EquipmentType.Weapon));
        Assert.That(weapon.WeaponCategory, Is.EqualTo(Core.WeaponCategory.Axe));
        Assert.That(weapon.Quality, Is.EqualTo(QualityTier.JuryRigged));
    }

    [Test]
    public void CreateStartingWeapon_Scavenger_ReturnsSpear()
    {
        // Arrange
        var scavengerClass = CharacterClass.Scavenger;

        // Act
        var weapon = _lootService.CreateStartingWeapon(scavengerClass);

        // Assert
        Assert.That(weapon, Is.Not.Null);
        Assert.That(weapon.Type, Is.EqualTo(EquipmentType.Weapon));
        Assert.That(weapon.WeaponCategory, Is.EqualTo(Core.WeaponCategory.Spear));
        Assert.That(weapon.Quality, Is.EqualTo(QualityTier.JuryRigged));
    }

    [Test]
    public void CreateStartingWeapon_Mystic_ReturnsStaff()
    {
        // Arrange
        var mysticClass = CharacterClass.Mystic;

        // Act
        var weapon = _lootService.CreateStartingWeapon(mysticClass);

        // Assert
        Assert.That(weapon, Is.Not.Null);
        Assert.That(weapon.Type, Is.EqualTo(EquipmentType.Weapon));
        Assert.That(weapon.WeaponCategory, Is.EqualTo(Core.WeaponCategory.Staff));
        Assert.That(weapon.Quality, Is.EqualTo(QualityTier.JuryRigged));
    }

    [Test]
    public void CreatePuzzleReward_AllClasses_ReturnsClanForgedWeapon()
    {
        // Arrange & Act
        var warriorReward = _lootService.CreatePuzzleReward(CharacterClass.Warrior);
        var scavengerReward = _lootService.CreatePuzzleReward(CharacterClass.Scavenger);
        var mysticReward = _lootService.CreatePuzzleReward(CharacterClass.Mystic);

        // Assert
        Assert.That(warriorReward, Is.Not.Null);
        Assert.That(warriorReward.Quality, Is.EqualTo(QualityTier.ClanForged));
        Assert.That(warriorReward.Type, Is.EqualTo(EquipmentType.Weapon));

        Assert.That(scavengerReward, Is.Not.Null);
        Assert.That(scavengerReward.Quality, Is.EqualTo(QualityTier.ClanForged));
        Assert.That(scavengerReward.Type, Is.EqualTo(EquipmentType.Weapon));

        Assert.That(mysticReward, Is.Not.Null);
        Assert.That(mysticReward.Quality, Is.EqualTo(QualityTier.ClanForged));
        Assert.That(mysticReward.Type, Is.EqualTo(EquipmentType.Weapon));
    }

    [Test]
    public void PlaceStartingLoot_AddsItemToRoom()
    {
        // Arrange
        var room = CreateTestRoom();
        var weapon = _lootService.CreateStartingWeapon(CharacterClass.Warrior);

        // Act
        _lootService.PlaceStartingLoot(room, weapon!);

        // Assert
        Assert.That(room.ItemsOnGround, Contains.Item(weapon));
        Assert.That(room.ItemsOnGround.Count, Is.EqualTo(1));
    }

    [Test]
    public void PlaceStartingLoot_NullItem_DoesNotAddToRoom()
    {
        // Arrange
        var room = CreateTestRoom();

        // Act
        _lootService.PlaceStartingLoot(room, null!);

        // Assert
        Assert.That(room.ItemsOnGround.Count, Is.EqualTo(0));
    }

    #endregion

    #region Loot Distribution Tests

    [Test]
    public void GenerateLoot_WeaponArmorDistribution_IsBalanced()
    {
        // Arrange
        var enemy = CreateTestEnemy("Blight-Drone", 2);
        var player = CreateTestPlayer(CharacterClass.Warrior);

        // Act
        int weaponCount = 0;
        int armorCount = 0;
        int iterations = 200;

        for (int i = 0; i < iterations; i++)
        {
            var loot = _lootService.GenerateLoot(enemy, player);
            if (loot != null)
            {
                if (loot.Type == EquipmentType.Weapon) weaponCount++;
                else if (loot.Type == EquipmentType.Armor) armorCount++;
            }
        }

        // Assert - Should have roughly balanced distribution (60% weapon, 40% armor)
        Assert.That(weaponCount, Is.GreaterThan(armorCount * 0.8)); // Weapons should be more common
        Assert.That(armorCount, Is.GreaterThan(iterations * 0.2)); // But armor should still drop
    }

    [Test]
    public void GenerateLoot_NullEnemy_ReturnsNull()
    {
        // Arrange
        var player = CreateTestPlayer(CharacterClass.Warrior);

        // Act
        var loot = _lootService.GenerateLoot(null!, player);

        // Assert
        Assert.That(loot, Is.Null);
    }

    [Test]
    public void GenerateLoot_NullPlayer_ReturnsNull()
    {
        // Arrange
        var enemy = CreateTestEnemy("Test Enemy", 1);

        // Act
        var loot = _lootService.GenerateLoot(enemy, null);

        // Assert
        Assert.That(loot, Is.Null);
    }

    #endregion

    #region Helper Methods

    private PlayerCharacter CreateTestPlayer(CharacterClass characterClass)
    {
        return new PlayerCharacter
        {
            Name = "TestPlayer",
            Class = characterClass,
            CurrentMilestone = 1,
            Attributes = new Attributes(might: 3, finesse: 2, wits: 2, will: 2, sturdiness: 3),
            HP = 30,
            MaxHP = 30,
            Stamina = 20,
            MaxStamina = 20,
            AP = 10
        };
    }

    private Enemy CreateTestEnemy(string name, int tier)
    {
        return new Enemy
        {
            Name = name,
            MaxHP = 20,
            HP = 20,
            ThreatLevel = (ThreatLevel)tier,
            IsBoss = false,
            Attributes = new Attributes(might: 2, finesse: 2, wits: 1, will: 1, sturdiness: 2)
        };
    }

    private Room CreateTestRoom()
    {
        return new Room
        {
            Id = 1,
            Name = "Test Room",
            Description = "A test room",
            Exits = new Dictionary<string, string>(),
            Enemies = new List<Enemy>(),
            ItemsOnGround = new List<Equipment>()
        };
    }

    #endregion
}
