using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using RuneAndRust.Core.Entities;
using RuneAndRust.Engine.Services;
using RuneAndRust.Persistence.Data;
using RuneAndRust.Persistence.Repositories;
using System.Text.Json;
using Xunit;

namespace RuneAndRust.Tests.Integration;

/// <summary>
/// Integration tests for template loading pipeline (v0.3.8).
/// Tests JSON deserialization → Database persistence → JSONB roundtrip integrity.
/// </summary>
public class TemplateLoadingIntegrationTests : IDisposable
{
    private readonly RuneAndRustDbContext _context;
    private readonly RoomTemplateRepository _roomTemplateRepo;
    private readonly BiomeDefinitionRepository _biomeDefRepo;
    private readonly GenericRepository<BiomeElement> _biomeElementRepo;
    private readonly TemplateLoaderService _loaderService;
    private readonly string _testDataPath;

    public TemplateLoadingIntegrationTests()
    {
        // Use in-memory database for testing
        var options = new DbContextOptionsBuilder<RuneAndRustDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        _context = new RuneAndRustDbContext(options);

        var roomTemplateLogger = Substitute.For<ILogger<GenericRepository<RoomTemplate>>>();
        var biomeDefLogger = Substitute.For<ILogger<GenericRepository<BiomeDefinition>>>();
        var biomeElementLogger = Substitute.For<ILogger<GenericRepository<BiomeElement>>>();
        var loaderLogger = Substitute.For<ILogger<TemplateLoaderService>>();

        _roomTemplateRepo = new RoomTemplateRepository(_context, roomTemplateLogger);
        _biomeDefRepo = new BiomeDefinitionRepository(_context, biomeDefLogger);
        _biomeElementRepo = new GenericRepository<BiomeElement>(_context, biomeElementLogger);

        _loaderService = new TemplateLoaderService(
            _roomTemplateRepo,
            _biomeDefRepo,
            _biomeElementRepo,
            loaderLogger);

        _testDataPath = Path.Combine(Path.GetTempPath(), $"test_templates_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDataPath);
        Directory.CreateDirectory(Path.Combine(_testDataPath, "templates"));
        Directory.CreateDirectory(Path.Combine(_testDataPath, "biomes"));
    }

    [Fact]
    public async Task LoadRoomTemplatesFromDirectory_LoadsValidTemplate_SuccessfullyPersists()
    {
        // Arrange
        var testTemplate = new RoomTemplate
        {
            TemplateId = "test_room",
            BiomeId = "test_biome",
            Size = "Medium",
            Archetype = "Chamber",
            NameTemplates = new List<string> { "The {Adjective} Chamber", "Chamber of {Adjective} Echoes" },
            Adjectives = new List<string> { "Dark", "Forgotten", "Ancient" },
            DescriptionTemplates = new List<string> { "A {Adjective} chamber. {Detail}." },
            Details = new List<string> { "Dust covers everything", "Echoes resound" },
            ValidConnections = new List<string> { "Corridor", "Chamber" },
            Tags = new List<string> { "Standard", "Atmospheric" },
            MinConnectionPoints = 1,
            MaxConnectionPoints = 3,
            Difficulty = "Medium"
        };

        var json = JsonSerializer.Serialize(testTemplate, new JsonSerializerOptions { WriteIndented = true });
        var filePath = Path.Combine(_testDataPath, "templates", "test_room.json");
        await File.WriteAllTextAsync(filePath, json);

        // Act
        await _loaderService.LoadRoomTemplatesFromDirectoryAsync(Path.Combine(_testDataPath, "templates"));
        await _roomTemplateRepo.SaveChangesAsync();

        // Assert
        var loaded = await _roomTemplateRepo.GetByTemplateIdAsync("test_room");
        Assert.NotNull(loaded);
        Assert.Equal("test_room", loaded!.TemplateId);
        Assert.Equal("test_biome", loaded.BiomeId);
        Assert.Equal(2, loaded.NameTemplates.Count);
        Assert.Equal(3, loaded.Adjectives.Count);
        Assert.Contains("Dark", loaded.Adjectives);
    }

    [Fact]
    public async Task LoadRoomTemplatesFromDirectory_JSONBRoundtrip_PreservesComplexData()
    {
        // Arrange
        var testTemplate = new RoomTemplate
        {
            TemplateId = "complex_room",
            BiomeId = "the_roots",
            Size = "Large",
            Archetype = "BossArena",
            NameTemplates = new List<string> { "The Reactor Core", "Heart of the Machine" },
            Adjectives = new List<string> { "Pulsing", "Unstable", "Corrupted" },
            DescriptionTemplates = new List<string>
            {
                "A {Adjective} reactor core dominates this space. {Detail}.",
                "This {Adjective} chamber thrums with power. {Detail}."
            },
            Details = new List<string>
            {
                "The reactor pulses with blinding runic light",
                "Cracks in the containment vessel glow ominously",
                "Warning runes flash in sequence along the walls"
            },
            ValidConnections = new List<string> { "Corridor", "Chamber", "Junction" },
            Tags = new List<string> { "BossRoom", "Critical", "Dangerous" },
            MinConnectionPoints = 1,
            MaxConnectionPoints = 2,
            Difficulty = "VeryHard"
        };

        var json = JsonSerializer.Serialize(testTemplate, new JsonSerializerOptions { WriteIndented = true });
        var filePath = Path.Combine(_testDataPath, "templates", "complex_room.json");
        await File.WriteAllTextAsync(filePath, json);

        // Act
        await _loaderService.LoadRoomTemplatesFromDirectoryAsync(Path.Combine(_testDataPath, "templates"));
        await _roomTemplateRepo.SaveChangesAsync();

        // Assert - Verify all JSONB arrays survived roundtrip
        var loaded = await _roomTemplateRepo.GetByTemplateIdAsync("complex_room");
        Assert.NotNull(loaded);

        // NameTemplates
        Assert.Equal(2, loaded!.NameTemplates.Count);
        Assert.Contains("The Reactor Core", loaded.NameTemplates);
        Assert.Contains("Heart of the Machine", loaded.NameTemplates);

        // Adjectives
        Assert.Equal(3, loaded.Adjectives.Count);
        Assert.Contains("Pulsing", loaded.Adjectives);
        Assert.Contains("Unstable", loaded.Adjectives);
        Assert.Contains("Corrupted", loaded.Adjectives);

        // DescriptionTemplates
        Assert.Equal(2, loaded.DescriptionTemplates.Count);

        // Details
        Assert.Equal(3, loaded.Details.Count);
        Assert.Contains("The reactor pulses with blinding runic light", loaded.Details);

        // ValidConnections
        Assert.Equal(3, loaded.ValidConnections.Count);
        Assert.Contains("Corridor", loaded.ValidConnections);

        // Tags
        Assert.Equal(3, loaded.Tags.Count);
        Assert.Contains("BossRoom", loaded.Tags);
        Assert.Contains("Critical", loaded.Tags);
        Assert.Contains("Dangerous", loaded.Tags);
    }

    [Fact]
    public async Task LoadBiomeDefinition_LoadsValidBiome_SuccessfullyPersists()
    {
        // Arrange
        var biomeJson = @"{
  ""BiomeId"": ""test_biome"",
  ""Name"": ""Test Biome"",
  ""Description"": ""A test biome for integration testing."",
  ""AvailableTemplates"": [""entry_hall"", ""corridor"", ""chamber""],
  ""DescriptorCategories"": {
    ""Adjectives"": [""Dark"", ""Forgotten""],
    ""Details"": [""Dust covers everything""],
    ""Sounds"": [""dripping water""],
    ""Smells"": [""rust and decay""]
  },
  ""MinRoomCount"": 5,
  ""MaxRoomCount"": 7,
  ""BranchingProbability"": 0.2,
  ""SecretRoomProbability"": 0.1,
  ""Elements"": {
    ""Elements"": [
      {
        ""ElementName"": ""Test Hazard"",
        ""ElementType"": ""DynamicHazard"",
        ""Weight"": 1.0,
        ""SpawnCost"": 1,
        ""AssociatedDataId"": ""steam_vent"",
        ""SpawnRules"": {
          ""NeverInEntryHall"": true,
          ""NeverInBossArena"": false,
          ""OnlyInLargeRooms"": false,
          ""RequiredArchetype"": null,
          ""RequiresRoomNameContains"": [],
          ""HigherWeightInSecretRooms"": false,
          ""SecretRoomWeightMultiplier"": null
        }
      }
    ]
  }
}";

        var filePath = Path.Combine(_testDataPath, "biomes", "test_biome.json");
        await File.WriteAllTextAsync(filePath, biomeJson);

        // Act
        await _loaderService.LoadBiomeDefinitionAsync(filePath);

        // Assert
        var loaded = await _biomeDefRepo.GetByBiomeIdAsync("test_biome");
        Assert.NotNull(loaded);
        Assert.Equal("test_biome", loaded!.BiomeId);
        Assert.Equal("Test Biome", loaded.Name);
        Assert.Equal(3, loaded.AvailableTemplates.Count);
        Assert.Contains("entry_hall", loaded.AvailableTemplates);

        // Verify nested BiomeDescriptorCategories
        Assert.NotNull(loaded.DescriptorCategories);
        Assert.Equal(2, loaded.DescriptorCategories.Adjectives.Count);
        Assert.Contains("Dark", loaded.DescriptorCategories.Adjectives);
        Assert.Single(loaded.DescriptorCategories.Sounds);
        Assert.Contains("dripping water", loaded.DescriptorCategories.Sounds);

        // Verify biome elements were loaded
        var elements = await _biomeElementRepo.GetAllAsync();
        var biomeElements = elements.Where(e => e.BiomeId == "test_biome").ToList();
        Assert.Single(biomeElements);
        Assert.Equal("Test Hazard", biomeElements[0].ElementName);
        Assert.Equal("DynamicHazard", biomeElements[0].ElementType);
        Assert.True(biomeElements[0].SpawnRules.NeverInEntryHall);
    }

    [Fact]
    public async Task LoadRoomTemplatesFromDirectory_UpsertLogic_UpdatesExistingTemplate()
    {
        // Arrange
        var initialTemplate = new RoomTemplate
        {
            TemplateId = "upsert_test",
            BiomeId = "test_biome",
            Size = "Small",
            Archetype = "Corridor",
            NameTemplates = new List<string> { "Old Name" },
            Adjectives = new List<string> { "Dark" },
            DescriptionTemplates = new List<string> { "Old description" },
            Details = new List<string> { "Old detail" },
            Difficulty = "Easy"
        };

        await _roomTemplateRepo.AddAsync(initialTemplate);
        await _roomTemplateRepo.SaveChangesAsync();

        // Create updated template with same TemplateId
        var updatedTemplate = new RoomTemplate
        {
            TemplateId = "upsert_test",
            BiomeId = "test_biome",
            Size = "Medium", // Changed
            Archetype = "Chamber", // Changed
            NameTemplates = new List<string> { "New Name" }, // Changed
            Adjectives = new List<string> { "Bright", "Shining" }, // Changed
            DescriptionTemplates = new List<string> { "New description" },
            Details = new List<string> { "New detail" },
            Difficulty = "Medium" // Changed
        };

        var json = JsonSerializer.Serialize(updatedTemplate, new JsonSerializerOptions { WriteIndented = true });
        var filePath = Path.Combine(_testDataPath, "templates", "upsert_test.json");
        await File.WriteAllTextAsync(filePath, json);

        // Act
        await _loaderService.LoadRoomTemplatesFromDirectoryAsync(Path.Combine(_testDataPath, "templates"));
        await _roomTemplateRepo.SaveChangesAsync();

        // Assert
        var allTemplates = await _roomTemplateRepo.GetAllAsync();
        var upsertedTemplates = allTemplates.Where(t => t.TemplateId == "upsert_test").ToList();

        Assert.Single(upsertedTemplates); // Should only have 1, not 2
        var loaded = upsertedTemplates[0];

        Assert.Equal("Medium", loaded.Size);
        Assert.Equal("Chamber", loaded.Archetype);
        Assert.Contains("New Name", loaded.NameTemplates);
        Assert.Contains("Bright", loaded.Adjectives);
        Assert.DoesNotContain("Dark", loaded.Adjectives);
    }

    [Fact]
    public async Task LoadBiomeDefinition_NestedElementSpawnRules_RoundtripCorrectly()
    {
        // Arrange
        var biomeJson = @"{
  ""BiomeId"": ""rules_test"",
  ""Name"": ""Rules Test Biome"",
  ""Description"": ""Test spawn rules."",
  ""AvailableTemplates"": [""entry_hall""],
  ""DescriptorCategories"": {},
  ""MinRoomCount"": 5,
  ""MaxRoomCount"": 7,
  ""BranchingProbability"": 0.1,
  ""SecretRoomProbability"": 0.05,
  ""Elements"": {
    ""Elements"": [
      {
        ""ElementName"": ""Complex Rules Element"",
        ""ElementType"": ""DynamicHazard"",
        ""Weight"": 2.5,
        ""SpawnCost"": 3,
        ""AssociatedDataId"": ""reactor_leak"",
        ""SpawnRules"": {
          ""NeverInEntryHall"": true,
          ""NeverInBossArena"": true,
          ""OnlyInLargeRooms"": true,
          ""RequiredArchetype"": ""Chamber"",
          ""RequiresRoomNameContains"": [""Reactor"", ""Core"", ""Engine""],
          ""HigherWeightInSecretRooms"": true,
          ""SecretRoomWeightMultiplier"": 4.0
        }
      }
    ]
  }
}";

        var filePath = Path.Combine(_testDataPath, "biomes", "rules_test.json");
        await File.WriteAllTextAsync(filePath, biomeJson);

        // Act
        await _loaderService.LoadBiomeDefinitionAsync(filePath);

        // Assert
        var elements = await _biomeElementRepo.GetAllAsync();
        var loaded = elements.First(e => e.ElementName == "Complex Rules Element");

        Assert.NotNull(loaded.SpawnRules);
        Assert.True(loaded.SpawnRules.NeverInEntryHall);
        Assert.True(loaded.SpawnRules.NeverInBossArena);
        Assert.True(loaded.SpawnRules.OnlyInLargeRooms);
        Assert.Equal("Chamber", loaded.SpawnRules.RequiredArchetype);
        Assert.Equal(3, loaded.SpawnRules.RequiresRoomNameContains!.Count);
        Assert.Contains("Reactor", loaded.SpawnRules.RequiresRoomNameContains);
        Assert.True(loaded.SpawnRules.HigherWeightInSecretRooms);
        Assert.Equal(4.0f, loaded.SpawnRules.SecretRoomWeightMultiplier);
    }

    public void Dispose()
    {
        _context.Dispose();
        if (Directory.Exists(_testDataPath))
        {
            Directory.Delete(_testDataPath, recursive: true);
        }
    }
}
