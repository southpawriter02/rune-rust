using NUnit.Framework;
using RuneAndRust.Core;
using RuneAndRust.Engine;
using System.IO;
using System.Text.Json;

namespace RuneAndRust.Tests;

/// <summary>
/// Tests for TemplateLibrary service (v0.10)
/// </summary>
[TestFixture]
public class TemplateLibraryTests
{
    private string _testDataPath = string.Empty;
    private TemplateLibrary _library = null!;

    [SetUp]
    public void Setup()
    {
        // Create temporary test data directory
        _testDataPath = Path.Combine(Path.GetTempPath(), $"RuneRustTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDataPath);

        _library = new TemplateLibrary(_testDataPath);
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up test data
        if (Directory.Exists(_testDataPath))
        {
            Directory.Delete(_testDataPath, recursive: true);
        }
    }

    #region Template Loading Tests

    [Test]
    public void LoadTemplates_WithValidTemplate_LoadsSuccessfully()
    {
        // Arrange
        var template = CreateTestTemplate("test_corridor", RoomArchetype.Corridor);
        SaveTemplateToFile(template, "test_corridor.json");

        // Act
        _library.LoadTemplates();

        // Assert
        Assert.That(_library.GetTemplateCount(), Is.EqualTo(1));
        var loaded = _library.GetTemplate("test_corridor");
        Assert.That(loaded, Is.Not.Null);
        Assert.That(loaded!.TemplateId, Is.EqualTo("test_corridor"));
    }

    [Test]
    public void LoadTemplates_WithMultipleTemplates_LoadsAll()
    {
        // Arrange
        SaveTemplateToFile(CreateTestTemplate("corridor_1", RoomArchetype.Corridor), "corridor_1.json");
        SaveTemplateToFile(CreateTestTemplate("corridor_2", RoomArchetype.Corridor), "corridor_2.json");
        SaveTemplateToFile(CreateTestTemplate("chamber_1", RoomArchetype.Chamber), "chamber_1.json");

        // Act
        _library.LoadTemplates();

        // Assert
        Assert.That(_library.GetTemplateCount(), Is.EqualTo(3));
    }

    [Test]
    public void LoadTemplates_WithInvalidTemplate_SkipsInvalid()
    {
        // Arrange
        var valid = CreateTestTemplate("valid", RoomArchetype.Corridor);
        var invalid = new RoomTemplate
        {
            TemplateId = "invalid",
            // Missing required fields - will fail validation
        };

        SaveTemplateToFile(valid, "valid.json");
        SaveTemplateToFile(invalid, "invalid.json");

        // Act
        _library.LoadTemplates();

        // Assert
        Assert.That(_library.GetTemplateCount(), Is.EqualTo(1));
        Assert.That(_library.GetTemplate("valid"), Is.Not.Null);
        Assert.That(_library.GetTemplate("invalid"), Is.Null);
    }

    [Test]
    public void LoadTemplates_FromSubdirectories_LoadsAll()
    {
        // Arrange
        var corridorsDir = Path.Combine(_testDataPath, "corridors");
        var chambersDir = Path.Combine(_testDataPath, "chambers");
        Directory.CreateDirectory(corridorsDir);
        Directory.CreateDirectory(chambersDir);

        SaveTemplateToFile(CreateTestTemplate("corridor_1", RoomArchetype.Corridor),
            Path.Combine(corridorsDir, "corridor_1.json"));
        SaveTemplateToFile(CreateTestTemplate("chamber_1", RoomArchetype.Chamber),
            Path.Combine(chambersDir, "chamber_1.json"));

        // Act
        _library.LoadTemplates();

        // Assert
        Assert.That(_library.GetTemplateCount(), Is.EqualTo(2));
    }

    [Test]
    public void LoadTemplates_WithMissingDirectory_HandlesGracefully()
    {
        // Arrange
        var nonExistentPath = Path.Combine(Path.GetTempPath(), $"NonExistent_{Guid.NewGuid()}");
        var library = new TemplateLibrary(nonExistentPath);

        // Act & Assert (should not throw)
        Assert.DoesNotThrow(() => library.LoadTemplates());
        Assert.That(library.GetTemplateCount(), Is.EqualTo(0));
    }

    #endregion

    #region Retrieval Tests

    [Test]
    public void GetTemplate_WithValidId_ReturnsTemplate()
    {
        // Arrange
        SaveTemplateToFile(CreateTestTemplate("test", RoomArchetype.Corridor), "test.json");
        _library.LoadTemplates();

        // Act
        var result = _library.GetTemplate("test");

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.TemplateId, Is.EqualTo("test"));
    }

    [Test]
    public void GetTemplate_WithInvalidId_ReturnsNull()
    {
        // Arrange
        _library.LoadTemplates();

        // Act
        var result = _library.GetTemplate("nonexistent");

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetTemplatesByArchetype_ReturnsMatchingTemplates()
    {
        // Arrange
        SaveTemplateToFile(CreateTestTemplate("corridor_1", RoomArchetype.Corridor), "corridor_1.json");
        SaveTemplateToFile(CreateTestTemplate("corridor_2", RoomArchetype.Corridor), "corridor_2.json");
        SaveTemplateToFile(CreateTestTemplate("chamber_1", RoomArchetype.Chamber), "chamber_1.json");
        _library.LoadTemplates();

        // Act
        var corridors = _library.GetTemplatesByArchetype(RoomArchetype.Corridor);

        // Assert
        Assert.That(corridors.Count, Is.EqualTo(2));
        Assert.That(corridors.All(t => t.Archetype == RoomArchetype.Corridor), Is.True);
    }

    [Test]
    public void GetTemplatesByArchetypeAndDifficulty_ReturnsFilteredTemplates()
    {
        // Arrange
        SaveTemplateToFile(CreateTestTemplate("easy_corridor", RoomArchetype.Corridor, RoomDifficulty.Easy), "easy.json");
        SaveTemplateToFile(CreateTestTemplate("hard_corridor", RoomArchetype.Corridor, RoomDifficulty.Hard), "hard.json");
        _library.LoadTemplates();

        // Act
        var easyCorridors = _library.GetTemplatesByArchetypeAndDifficulty(RoomArchetype.Corridor, RoomDifficulty.Easy);

        // Assert
        Assert.That(easyCorridors.Count, Is.EqualTo(1));
        Assert.That(easyCorridors[0].TemplateId, Is.EqualTo("easy_corridor"));
    }

    [Test]
    public void GetTemplatesByBiome_ReturnsMatchingTemplates()
    {
        // Arrange
        var rootsTemplate = CreateTestTemplate("roots_corridor", RoomArchetype.Corridor);
        rootsTemplate.Biome = "the_roots";

        var trunkTemplate = CreateTestTemplate("trunk_corridor", RoomArchetype.Corridor);
        trunkTemplate.Biome = "the_trunk";

        SaveTemplateToFile(rootsTemplate, "roots.json");
        SaveTemplateToFile(trunkTemplate, "trunk.json");
        _library.LoadTemplates();

        // Act
        var rootsTemplates = _library.GetTemplatesByBiome("the_roots");

        // Assert
        Assert.That(rootsTemplates.Count, Is.EqualTo(1));
        Assert.That(rootsTemplates[0].Biome, Is.EqualTo("the_roots"));
    }

    [Test]
    public void GetTemplatesConnectingTo_ReturnsValidConnections()
    {
        // Arrange
        var corridor = CreateTestTemplate("corridor", RoomArchetype.Corridor);
        corridor.ValidConnections = new List<RoomArchetype> { RoomArchetype.Chamber, RoomArchetype.Junction };

        var chamber = CreateTestTemplate("chamber", RoomArchetype.Chamber);
        chamber.ValidConnections = new List<RoomArchetype> { RoomArchetype.Corridor };

        SaveTemplateToFile(corridor, "corridor.json");
        SaveTemplateToFile(chamber, "chamber.json");
        _library.LoadTemplates();

        // Act
        var connectingToChamber = _library.GetTemplatesConnectingTo(RoomArchetype.Chamber);

        // Assert
        Assert.That(connectingToChamber.Count, Is.EqualTo(1));
        Assert.That(connectingToChamber[0].TemplateId, Is.EqualTo("corridor"));
    }

    #endregion

    #region Random Selection Tests

    [Test]
    public void GetRandomTemplate_WithValidArchetype_ReturnsTemplate()
    {
        // Arrange
        SaveTemplateToFile(CreateTestTemplate("corridor_1", RoomArchetype.Corridor), "corridor_1.json");
        SaveTemplateToFile(CreateTestTemplate("corridor_2", RoomArchetype.Corridor), "corridor_2.json");
        _library.LoadTemplates();
        var rng = new Random(42);

        // Act
        var result = _library.GetRandomTemplate(rng, RoomArchetype.Corridor);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Archetype, Is.EqualTo(RoomArchetype.Corridor));
    }

    [Test]
    public void GetRandomTemplate_WithNoMatchingArchetype_ReturnsNull()
    {
        // Arrange
        SaveTemplateToFile(CreateTestTemplate("corridor", RoomArchetype.Corridor), "corridor.json");
        _library.LoadTemplates();
        var rng = new Random(42);

        // Act
        var result = _library.GetRandomTemplate(rng, RoomArchetype.BossArena);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetRandomTemplate_WithDifficulty_ReturnsMatchingTemplate()
    {
        // Arrange
        SaveTemplateToFile(CreateTestTemplate("easy", RoomArchetype.Corridor, RoomDifficulty.Easy), "easy.json");
        SaveTemplateToFile(CreateTestTemplate("hard", RoomArchetype.Corridor, RoomDifficulty.Hard), "hard.json");
        _library.LoadTemplates();
        var rng = new Random(42);

        // Act
        var result = _library.GetRandomTemplate(rng, RoomArchetype.Corridor, RoomDifficulty.Easy);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Difficulty, Is.EqualTo(RoomDifficulty.Easy));
    }

    [Test]
    public void GetRandomTemplate_WithDifficultyMismatch_FallsBackToArchetype()
    {
        // Arrange
        SaveTemplateToFile(CreateTestTemplate("easy", RoomArchetype.Corridor, RoomDifficulty.Easy), "easy.json");
        _library.LoadTemplates();
        var rng = new Random(42);

        // Act
        var result = _library.GetRandomTemplate(rng, RoomArchetype.Corridor, RoomDifficulty.Hard);

        // Assert (should fall back to Easy since no Hard templates exist)
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Archetype, Is.EqualTo(RoomArchetype.Corridor));
    }

    #endregion

    #region Statistics Tests

    [Test]
    public void GetTemplateStatistics_ReturnsCorrectCounts()
    {
        // Arrange
        SaveTemplateToFile(CreateTestTemplate("entry", RoomArchetype.EntryHall), "entry.json");
        SaveTemplateToFile(CreateTestTemplate("corridor_1", RoomArchetype.Corridor), "corridor_1.json");
        SaveTemplateToFile(CreateTestTemplate("corridor_2", RoomArchetype.Corridor), "corridor_2.json");
        SaveTemplateToFile(CreateTestTemplate("chamber", RoomArchetype.Chamber), "chamber.json");
        _library.LoadTemplates();

        // Act
        var stats = _library.GetTemplateStatistics();

        // Assert
        Assert.That(stats["Total"], Is.EqualTo(4));
        Assert.That(stats["EntryHalls"], Is.EqualTo(1));
        Assert.That(stats["Corridors"], Is.EqualTo(2));
        Assert.That(stats["Chambers"], Is.EqualTo(1));
        Assert.That(stats["Junctions"], Is.EqualTo(0));
    }

    #endregion

    #region Validation Tests

    [Test]
    public void ValidateLibrary_WithAllRequiredArchetypes_ReturnsTrue()
    {
        // Arrange
        SaveTemplateToFile(CreateTestTemplate("entry", RoomArchetype.EntryHall), "entry.json");
        SaveTemplateToFile(CreateTestTemplate("corridor", RoomArchetype.Corridor), "corridor.json");
        SaveTemplateToFile(CreateTestTemplate("chamber", RoomArchetype.Chamber), "chamber.json");
        SaveTemplateToFile(CreateTestTemplate("boss", RoomArchetype.BossArena), "boss.json");
        _library.LoadTemplates();

        // Act
        bool isValid = _library.ValidateLibrary();

        // Assert
        Assert.That(isValid, Is.True);
    }

    [Test]
    public void ValidateLibrary_WithMissingRequiredArchetype_ReturnsFalse()
    {
        // Arrange
        SaveTemplateToFile(CreateTestTemplate("entry", RoomArchetype.EntryHall), "entry.json");
        SaveTemplateToFile(CreateTestTemplate("corridor", RoomArchetype.Corridor), "corridor.json");
        // Missing Chamber and BossArena
        _library.LoadTemplates();

        // Act
        bool isValid = _library.ValidateLibrary();

        // Assert
        Assert.That(isValid, Is.False);
    }

    #endregion

    #region Helper Methods

    private RoomTemplate CreateTestTemplate(string id, RoomArchetype archetype,
        RoomDifficulty difficulty = RoomDifficulty.Easy)
    {
        return new RoomTemplate
        {
            TemplateId = id,
            Biome = "the_roots",
            Archetype = archetype,
            Size = RoomSize.Medium,
            Difficulty = difficulty,
            NameTemplates = new List<string> { $"The {{Adjective}} {archetype}" },
            Adjectives = new List<string> { "Rusted", "Twisted" },
            DescriptionTemplates = new List<string> { "A {{Adjective}} {archetype}. {{Detail}}." },
            Details = new List<string> { "Water drips from above", "Metal groans under strain" },
            MinConnectionPoints = archetype == RoomArchetype.Junction ? 3 : 1,
            MaxConnectionPoints = archetype == RoomArchetype.Junction ? 5 : 3,
            ValidConnections = new List<RoomArchetype> { RoomArchetype.Corridor, RoomArchetype.Chamber },
            Tags = new List<string> { "test" }
        };
    }

    private void SaveTemplateToFile(RoomTemplate template, string fileName)
    {
        var fullPath = Path.IsPathRooted(fileName)
            ? fileName
            : Path.Combine(_testDataPath, fileName);

        var json = JsonSerializer.Serialize(template, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(fullPath, json);
    }

    #endregion
}
