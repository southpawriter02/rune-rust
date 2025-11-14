using Microsoft.VisualStudio.TestTools.UnitTesting;
using RuneAndRust.Core;
using RuneAndRust.Engine;
using RuneAndRust.Persistence;
using Serilog;
using System.Text.Json;

namespace RuneAndRust.Tests;

[TestClass]
public class BossLootServiceTests
{
    private string _testDbPath = null!;
    private BossEncounterRepository _repository = null!;
    private BossLootService _lootService = null!;
    private DiceService _diceService = null!;

    [TestInitialize]
    public void Setup()
    {
        // Configure Serilog for tests
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateLogger();

        // Create test database
        _testDbPath = Path.Combine(Path.GetTempPath(), $"test_boss_loot_{Guid.NewGuid()}.db");
        _repository = new BossEncounterRepository(_testDbPath);
        _diceService = new DiceService();
        _lootService = new BossLootService(_repository, _diceService);

        // Seed test data
        SeedTestData();
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (File.Exists(_testDbPath))
        {
            File.Delete(_testDbPath);
        }
    }

    private void SeedTestData()
    {
        // Create test boss encounter
        int bossEncounterId = _repository.CreateBossEncounter(new BossEncounterConfig
        {
            EncounterId = 999,
            BossName = "Test Boss",
            BossType = "Test",
            TotalPhases = 1,
            Phase2HpThreshold = 0.75f,
            Phase3HpThreshold = 0.50f,
            TransitionInvulnerabilityTurns = 1,
            EnrageHpThreshold = 0.25f,
            EnrageDamageMultiplier = 1.5f,
            EnrageSpeedBonus = 1
        });

        // Create loot table
        _repository.CreateBossLootTable(new BossLootTableData
        {
            BossEncounterId = bossEncounterId,
            GuaranteedDropCount = 2,
            MinimumQualityTier = "Clan-Forged",
            ClanForgedChance = 40,
            RuneCarvedChance = 45,
            ArtifactChance = 15,
            SilverMarksMin = 100,
            SilverMarksMax = 200,
            DropsUniqueItem = true,
            DropsCraftingMaterials = true,
            CraftingMaterialPool = JsonSerializer.Serialize(new List<CraftingMaterialDefinition>
            {
                new() { MaterialName = "Test Material", DropChance = 100, QuantityMin = 1, QuantityMax = 3 }
            })
        });

        // Create test artifacts
        int testArtifact = _repository.CreateArtifact(new ArtifactData
        {
            ArtifactName = "Test Artifact",
            ArtifactType = "Weapon",
            Description = "A test artifact",
            MightBonus = 2,
            FinesseBonus = 1,
            DefenseBonus = 1,
            MinimumTdr = 50,
            SetName = "Test Set",
            SetPieceCount = 4,
            UniqueEffectName = "Test Effect",
            UniqueEffectDescription = "Does test things"
        });

        // Create unique item
        int uniqueItem = _repository.CreateArtifact(new ArtifactData
        {
            ArtifactName = "Test Unique Item",
            ArtifactType = "Accessory",
            Description = "A unique test item",
            WitsBonus = 2,
            MinimumTdr = 50
        });

        _repository.CreateBossUniqueItem(new BossUniqueItemData
        {
            BossEncounterId = bossEncounterId,
            ArtifactId = uniqueItem,
            DropChance = 100,
            DropCountMin = 1,
            DropCountMax = 1,
            DropsOncePerCharacter = true
        });

        // Create set bonuses
        _repository.CreateSetBonus(new SetBonusData
        {
            SetName = "Test Set",
            PiecesRequired = 2,
            BonusName = "Test Bonus 2pc",
            BonusDescription = "Grants +1 to all attributes"
        });

        _repository.CreateSetBonus(new SetBonusData
        {
            SetName = "Test Set",
            PiecesRequired = 4,
            BonusName = "Test Bonus 4pc",
            BonusDescription = "Grants +2 to all attributes"
        });
    }

    // PPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP
    // LOOT TABLE TESTS
    // PPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP

    [TestMethod]
    public void TestCreateAndRetrieveBossLootTable()
    {
        // Arrange: done in SeedTestData

        // Act
        var lootTable = _repository.GetBossLootTable(1);

        // Assert
        Assert.IsNotNull(lootTable, "Loot table should exist");
        Assert.AreEqual(2, lootTable.GuaranteedDropCount, "Guaranteed drop count should be 2");
        Assert.AreEqual("Clan-Forged", lootTable.MinimumQualityTier, "Minimum quality should be Clan-Forged");
        Assert.AreEqual(40, lootTable.ClanForgedChance, "Clan-Forged chance should be 40%");
        Assert.AreEqual(45, lootTable.RuneCarvedChance, "Rune-Carved chance should be 45%");
        Assert.AreEqual(15, lootTable.ArtifactChance, "Artifact chance should be 15%");
    }

    // PPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP
    // LOOT GENERATION TESTS
    // PPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP

    [TestMethod]
    public void TestGenerateBossLoot_ReturnsItems()
    {
        // Arrange
        string characterId = "test_character_1";
        int bossTdr = 60;

        // Act
        var result = _lootService.GenerateBossLoot(1, characterId, bossTdr);

        // Assert
        Assert.IsNotNull(result, "Result should not be null");
        Assert.IsTrue(result.Items.Count > 0, "Should generate at least one item");
        Assert.IsTrue(result.SilverMarks >= 100 && result.SilverMarks <= 200,
            $"Silver marks should be between 100-200, got {result.SilverMarks}");
        Assert.IsNotNull(result.LogMessage, "Log message should not be null");
        Assert.IsTrue(result.LogMessage.Contains("BOSS LOOT GENERATION"), "Log should contain header");
    }

    [TestMethod]
    public void TestGenerateBossLoot_GuaranteedDropCount()
    {
        // Arrange
        string characterId = "test_character_2";
        int bossTdr = 60;

        // Act
        var result = _lootService.GenerateBossLoot(1, characterId, bossTdr);

        // Assert: Should have at least 2 guaranteed drops (may have more with artifacts/uniques)
        int guaranteedDrops = result.Items.Count(i => !i.IsArtifact);
        Assert.IsTrue(guaranteedDrops >= 2,
            $"Should have at least 2 guaranteed drops, got {guaranteedDrops}");
    }

    [TestMethod]
    public void TestGenerateBossLoot_CraftingMaterials()
    {
        // Arrange
        string characterId = "test_character_3";
        int bossTdr = 60;

        // Act
        var result = _lootService.GenerateBossLoot(1, characterId, bossTdr);

        // Assert
        Assert.IsTrue(result.CraftingMaterials.Count > 0,
            "Should generate crafting materials");
        Assert.IsTrue(result.CraftingMaterials.Any(m => m.MaterialName == "Test Material"),
            "Should include test material");
        Assert.IsTrue(result.LogMessage.Contains("Test Material"),
            "Log should mention crafting materials");
    }

    // PPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP
    // ARTIFACT TESTS
    // PPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP

    [TestMethod]
    public void TestCreateAndRetrieveArtifact()
    {
        // Act
        var artifact = _repository.GetArtifact(1);

        // Assert
        Assert.IsNotNull(artifact, "Artifact should exist");
        Assert.AreEqual("Test Artifact", artifact.ArtifactName, "Artifact name should match");
        Assert.AreEqual("Weapon", artifact.ArtifactType, "Artifact type should be Weapon");
        Assert.AreEqual(2, artifact.MightBonus, "Might bonus should be 2");
        Assert.AreEqual("Test Set", artifact.SetName, "Set name should match");
    }

    [TestMethod]
    public void TestGetArtifactsByTDR()
    {
        // Act
        var artifacts = _repository.GetArtifactsByTDR(60);

        // Assert
        Assert.IsTrue(artifacts.Count > 0, "Should find artifacts for TDR 60");
        Assert.IsTrue(artifacts.All(a => a.MinimumTdr <= 60),
            "All artifacts should have MinimumTdr <= 60");
    }

    [TestMethod]
    public void TestGetArtifactsByTDR_FiltersByTDR()
    {
        // Arrange: Create high-TDR artifact
        _repository.CreateArtifact(new ArtifactData
        {
            ArtifactName = "High TDR Artifact",
            ArtifactType = "Weapon",
            Description = "Requires high TDR",
            MinimumTdr = 100
        });

        // Act
        var lowTdrArtifacts = _repository.GetArtifactsByTDR(60);
        var highTdrArtifacts = _repository.GetArtifactsByTDR(100);

        // Assert
        Assert.IsFalse(lowTdrArtifacts.Any(a => a.ArtifactName == "High TDR Artifact"),
            "Low TDR query should not include high TDR artifacts");
        Assert.IsTrue(highTdrArtifacts.Any(a => a.ArtifactName == "High TDR Artifact"),
            "High TDR query should include high TDR artifacts");
    }

    // PPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP
    // UNIQUE ITEM TESTS
    // PPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP

    [TestMethod]
    public void TestUniqueItemDrops_OncePerCharacter()
    {
        // Arrange
        string characterId = "test_character_unique";
        int bossTdr = 60;

        // Act - First kill
        var firstResult = _lootService.GenerateBossLoot(1, characterId, bossTdr);
        int firstUniqueCount = firstResult.Items.Count(i => i.IsUnique);

        // Act - Second kill (should not drop again)
        var secondResult = _lootService.GenerateBossLoot(1, characterId, bossTdr);
        int secondUniqueCount = secondResult.Items.Count(i => i.IsUnique);

        // Assert
        Assert.AreEqual(1, firstUniqueCount, "First kill should drop unique item");
        Assert.AreEqual(0, secondUniqueCount, "Second kill should not drop unique item again");
    }

    [TestMethod]
    public void TestHasReceivedUniqueItem()
    {
        // Arrange
        string characterId = "test_character_check";
        int artifactId = 2; // Test unique item

        // Act - Before receiving
        bool beforeReceiving = _repository.HasReceivedUniqueItem(characterId, artifactId);

        // Record receipt
        _repository.RecordUniqueItemDrop(characterId, artifactId);

        // Act - After receiving
        bool afterReceiving = _repository.HasReceivedUniqueItem(characterId, artifactId);

        // Assert
        Assert.IsFalse(beforeReceiving, "Character should not have received item initially");
        Assert.IsTrue(afterReceiving, "Character should have received item after recording");
    }

    [TestMethod]
    public void TestDifferentCharacters_CanReceiveSameUnique()
    {
        // Arrange
        string character1 = "character_1";
        string character2 = "character_2";
        int bossTdr = 60;

        // Act
        var result1 = _lootService.GenerateBossLoot(1, character1, bossTdr);
        var result2 = _lootService.GenerateBossLoot(1, character2, bossTdr);

        // Assert
        Assert.AreEqual(1, result1.Items.Count(i => i.IsUnique),
            "Character 1 should receive unique item");
        Assert.AreEqual(1, result2.Items.Count(i => i.IsUnique),
            "Character 2 should also receive unique item");
    }

    // PPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP
    // SET BONUS TESTS
    // PPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP

    [TestMethod]
    public void TestCreateAndRetrieveSetBonuses()
    {
        // Act
        var setBonuses = _repository.GetSetBonuses("Test Set");

        // Assert
        Assert.AreEqual(2, setBonuses.Count, "Should have 2 set bonuses");
        Assert.IsTrue(setBonuses.Any(b => b.PiecesRequired == 2),
            "Should have 2-piece bonus");
        Assert.IsTrue(setBonuses.Any(b => b.PiecesRequired == 4),
            "Should have 4-piece bonus");
    }

    [TestMethod]
    public void TestGetSetBonuses_OrderedByPiecesRequired()
    {
        // Act
        var setBonuses = _repository.GetSetBonuses("Test Set");

        // Assert
        Assert.AreEqual(2, setBonuses[0].PiecesRequired,
            "First bonus should require 2 pieces");
        Assert.AreEqual(4, setBonuses[1].PiecesRequired,
            "Second bonus should require 4 pieces");
    }

    // PPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP
    // QUALITY TIER TESTS
    // PPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP

    [TestMethod]
    public void TestGeneratedItems_RespectQualityTiers()
    {
        // Arrange
        string characterId = "test_quality";
        int bossTdr = 60;

        // Act - Generate multiple times to test distribution
        var qualityTiers = new List<string>();
        for (int i = 0; i < 20; i++)
        {
            var result = _lootService.GenerateBossLoot(1, $"{characterId}_{i}", bossTdr);
            qualityTiers.AddRange(result.Items.Where(item => !item.IsUnique).Select(item => item.QualityTier));
        }

        // Assert - Should have variety of quality tiers
        var distinctTiers = qualityTiers.Distinct().ToList();
        Assert.IsTrue(distinctTiers.Count > 1,
            "Should generate items of different quality tiers");
        Assert.IsTrue(distinctTiers.All(t => t == "Clan-Forged" || t == "Rune-Carved" || t == "Artifact"),
            "All quality tiers should be valid");
    }

    // PPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP
    // INTEGRATION TESTS
    // PPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP

    [TestMethod]
    public void TestFullLootGeneration_WithRealSeeder()
    {
        // Arrange: Create new database with real seeded data
        string realDbPath = Path.Combine(Path.GetTempPath(), $"test_real_boss_loot_{Guid.NewGuid()}.db");
        var realRepository = new BossEncounterRepository(realDbPath);
        var realLootService = new BossLootService(realRepository, _diceService);

        try
        {
            // Seed boss encounters
            var bossSeeder = new BossEncounterSeeder(realRepository);
            bossSeeder.SeedBossEncounters();

            // Seed boss loot
            var lootSeeder = new BossLootSeeder(realRepository);
            lootSeeder.SeedBossLoot();

            // Act: Generate loot from Ruin-Warden (encounter ID 1)
            var result = realLootService.GenerateBossLoot(1, "integration_test_character", 50);

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsTrue(result.Items.Count >= 2, "Should have at least 2 guaranteed drops");
            Assert.IsTrue(result.SilverMarks > 0, "Should have silver marks");
            Assert.IsTrue(result.CraftingMaterials.Count > 0, "Should have crafting materials");
            Assert.IsTrue(result.LogMessage.Contains("Corrupted Servo") ||
                          result.LogMessage.Contains("Ancient Circuit") ||
                          result.LogMessage.Contains("Ruin Fragment"),
                "Should drop Ruin-Warden materials");
        }
        finally
        {
            if (File.Exists(realDbPath))
            {
                File.Delete(realDbPath);
            }
        }
    }

    [TestMethod]
    public void TestArtifactSets_AllPiecesPresent()
    {
        // Arrange: Create new database with real seeded data
        string realDbPath = Path.Combine(Path.GetTempPath(), $"test_artifact_sets_{Guid.NewGuid()}.db");
        var realRepository = new BossEncounterRepository(realDbPath);

        try
        {
            var bossSeeder = new BossEncounterSeeder(realRepository);
            bossSeeder.SeedBossEncounters();

            var lootSeeder = new BossLootSeeder(realRepository);
            lootSeeder.SeedBossLoot();

            // Act: Get all artifacts for each set
            var guardianAegisSet = realRepository.GetArtifactsBySet("Guardian's Aegis");
            var voidTouchedSet = realRepository.GetArtifactsBySet("Void-Touched Vestments");
            var shadowReaverSet = realRepository.GetArtifactsBySet("Shadow Reaver's Arsenal");

            // Assert
            Assert.IsTrue(guardianAegisSet.Count >= 4,
                $"Guardian's Aegis should have at least 4 pieces, got {guardianAegisSet.Count}");
            Assert.IsTrue(voidTouchedSet.Count >= 4,
                $"Void-Touched Vestments should have at least 4 pieces, got {voidTouchedSet.Count}");
            Assert.IsTrue(shadowReaverSet.Count >= 4,
                $"Shadow Reaver's Arsenal should have at least 4 pieces, got {shadowReaverSet.Count}");
        }
        finally
        {
            if (File.Exists(realDbPath))
            {
                File.Delete(realDbPath);
            }
        }
    }
}
