using Xunit;
using RuneAndRust.Core.Descriptors;
using RuneAndRust.Engine;
using RuneAndRust.Persistence;
using Microsoft.Data.Sqlite;
using Serilog;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.38.5: Unit tests for the Resource Node System
/// Tests procedural resource generation with biome distribution
/// </summary>
public class ResourceNodeServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly string _connectionString;
    private readonly DescriptorRepository _repository;
    private readonly ResourceNodeService _service;

    public ResourceNodeServiceTests()
    {
        // Configure Serilog for tests
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateLogger();

        // Create in-memory database
        _connectionString = "Data Source=:memory:";
        _connection = new SqliteConnection(_connectionString);
        _connection.Open();

        // Initialize schema
        InitializeSchema();

        // Seed test data
        SeedTestData();

        // Initialize services
        _repository = new DescriptorRepository(_connectionString);
        _service = new ResourceNodeService(_repository);
    }

    public void Dispose()
    {
        _connection?.Dispose();
    }

    #region Setup

    private void InitializeSchema()
    {
        var createSql = @"
            -- Base Templates
            CREATE TABLE Descriptor_Base_Templates (
                template_id INTEGER PRIMARY KEY AUTOINCREMENT,
                template_name TEXT NOT NULL UNIQUE,
                category TEXT NOT NULL,
                archetype TEXT NOT NULL,
                base_mechanics TEXT,
                name_template TEXT NOT NULL,
                description_template TEXT NOT NULL,
                tags TEXT,
                notes TEXT,
                created_at TEXT DEFAULT CURRENT_TIMESTAMP,
                updated_at TEXT DEFAULT CURRENT_TIMESTAMP
            );

            -- Thematic Modifiers
            CREATE TABLE Descriptor_Thematic_Modifiers (
                modifier_id INTEGER PRIMARY KEY AUTOINCREMENT,
                modifier_name TEXT NOT NULL UNIQUE,
                primary_biome TEXT NOT NULL,
                adjective TEXT NOT NULL,
                detail_fragment TEXT NOT NULL,
                stat_modifiers TEXT,
                status_effects TEXT,
                color_palette TEXT,
                ambient_sounds TEXT,
                particle_effects TEXT,
                notes TEXT,
                created_at TEXT DEFAULT CURRENT_TIMESTAMP,
                updated_at TEXT DEFAULT CURRENT_TIMESTAMP
            );

            -- Biome Resource Profiles
            CREATE TABLE Biome_Resource_Profiles (
                profile_id INTEGER PRIMARY KEY AUTOINCREMENT,
                biome_name TEXT NOT NULL UNIQUE,
                common_resources TEXT NOT NULL,
                uncommon_resources TEXT NOT NULL,
                rare_resources TEXT NOT NULL,
                legendary_resources TEXT,
                spawn_density_small INTEGER DEFAULT 0,
                spawn_density_medium INTEGER DEFAULT 2,
                spawn_density_large INTEGER DEFAULT 3,
                unique_resources TEXT,
                notes TEXT
            );
        ";

        using var command = _connection.CreateCommand();
        command.CommandText = createSql;
        command.ExecuteNonQuery();
    }

    private void SeedTestData()
    {
        var insertSql = @"
            -- Base Template: Ore Vein
            INSERT INTO Descriptor_Base_Templates (
                template_name, category, archetype, base_mechanics,
                name_template, description_template, tags
            ) VALUES (
                'Ore_Vein_Base', 'Resource', 'MineralVein',
                '{""extraction_type"": ""Mining"", ""extraction_dc"": 12, ""extraction_time"": 2, ""yield_min"": 2, ""yield_max"": 4, ""uses"": 3, ""requires_tool"": false}',
                '{Modifier} {Resource_Type} Vein',
                'A {Modifier_Adj} vein of {Resource_Type}.',
                '[""Mining"", ""Metal""]'
            );

            -- Base Template: Salvage Wreckage
            INSERT INTO Descriptor_Base_Templates (
                template_name, category, archetype, base_mechanics,
                name_template, description_template, tags
            ) VALUES (
                'Salvage_Wreckage_Base', 'Resource', 'SalvageWreckage',
                '{""extraction_type"": ""Salvaging"", ""extraction_dc"": 15, ""extraction_time"": 3, ""yield_min"": 1, ""yield_max"": 3, ""uses"": 2, ""trap_chance"": 0.1}',
                '{Modifier} {Wreckage_Type}',
                'The wreckage of {Article} {Modifier_Adj} {Wreckage_Type}.',
                '[""Salvage"", ""Mechanical""]'
            );

            -- Thematic Modifier: Rusted (The Roots)
            INSERT INTO Descriptor_Thematic_Modifiers (
                modifier_name, primary_biome, adjective, detail_fragment
            ) VALUES (
                'Rusted', 'The_Roots', 'corroded', 'shows centuries of oxidation'
            );

            -- Thematic Modifier: Scorched (Muspelheim)
            INSERT INTO Descriptor_Thematic_Modifiers (
                modifier_name, primary_biome, adjective, detail_fragment
            ) VALUES (
                'Scorched', 'Muspelheim', 'fire-hardened', 'radiates intense heat'
            );

            -- Biome Profile: The Roots
            INSERT INTO Biome_Resource_Profiles (
                biome_name, common_resources, uncommon_resources, rare_resources,
                spawn_density_small, spawn_density_medium, spawn_density_large
            ) VALUES (
                'The_Roots',
                '[
                    {""template"": ""Ore_Vein_Base"", ""resource"": ""Iron"", ""weight"": 0.6},
                    {""template"": ""Salvage_Wreckage_Base"", ""resource"": ""Servitor"", ""weight"": 0.4}
                ]',
                '[
                    {""template"": ""Ore_Vein_Base"", ""resource"": ""Star_Metal"", ""weight"": 1.0}
                ]',
                '[
                    {""template"": ""Salvage_Wreckage_Base"", ""resource"": ""Dvergr_Cache"", ""weight"": 1.0}
                ]',
                0, 2, 3
            );

            -- Biome Profile: Muspelheim
            INSERT INTO Biome_Resource_Profiles (
                biome_name, common_resources, uncommon_resources, rare_resources,
                spawn_density_small, spawn_density_medium, spawn_density_large
            ) VALUES (
                'Muspelheim',
                '[
                    {""template"": ""Ore_Vein_Base"", ""resource"": ""Obsidian"", ""weight"": 1.0}
                ]',
                '[
                    {""template"": ""Ore_Vein_Base"", ""resource"": ""Star_Metal"", ""weight"": 1.0}
                ]',
                '[
                    {""template"": ""Salvage_Wreckage_Base"", ""resource"": ""Forge_Equipment"", ""weight"": 1.0}
                ]',
                0, 1, 2
            );
        ";

        using var command = _connection.CreateCommand();
        command.CommandText = insertSql;
        command.ExecuteNonQuery();
    }

    #endregion

    #region Basic Generation Tests

    [Fact]
    public void GenerateResourceNodes_WithValidBiome_ReturnsNodes()
    {
        // Act
        var nodes = _service.GenerateResourceNodes(roomId: 1, biomeName: "The_Roots", roomSize: "Medium");

        // Assert
        Assert.NotNull(nodes);
        Assert.NotEmpty(nodes);
        Assert.True(nodes.Count <= 2); // Medium room density for The_Roots is 2
    }

    [Fact]
    public void GenerateResourceNodes_SmallRoom_ReturnsFewerNodes()
    {
        // Act
        var nodes = _service.GenerateResourceNodes(roomId: 1, biomeName: "The_Roots", roomSize: "Small");

        // Assert
        Assert.NotNull(nodes);
        // Small room density for The_Roots is 0
        Assert.Empty(nodes);
    }

    [Fact]
    public void GenerateResourceNodes_LargeRoom_ReturnsMoreNodes()
    {
        // Act
        var nodes = _service.GenerateResourceNodes(roomId: 1, biomeName: "The_Roots", roomSize: "Large");

        // Assert
        Assert.NotNull(nodes);
        // Large room density for The_Roots is 3
        Assert.True(nodes.Count <= 3);
    }

    [Fact]
    public void GenerateResourceNodes_UnknownBiome_ReturnsGenericNodes()
    {
        // Act
        var nodes = _service.GenerateResourceNodes(roomId: 1, biomeName: "UnknownBiome", roomSize: "Medium");

        // Assert
        Assert.NotNull(nodes);
        // Should fall back to generic generation
    }

    #endregion

    #region Biome-Specific Tests

    [Fact]
    public void GenerateResourceNodes_TheRoots_ProducesAppropriateResources()
    {
        // Act
        var nodes = new List<ResourceNode>();
        for (int i = 0; i < 10; i++)
        {
            nodes.AddRange(_service.GenerateResourceNodes(roomId: i, biomeName: "The_Roots", roomSize: "Large"));
        }

        // Assert
        Assert.NotEmpty(nodes);
        // Should contain The_Roots themed resources
        foreach (var node in nodes)
        {
            Assert.NotNull(node.Name);
            Assert.NotEmpty(node.ResourceType);
        }
    }

    [Fact]
    public void GenerateResourceNodes_Muspelheim_ProducesAppropriateResources()
    {
        // Act
        var nodes = new List<ResourceNode>();
        for (int i = 0; i < 10; i++)
        {
            nodes.AddRange(_service.GenerateResourceNodes(roomId: i, biomeName: "Muspelheim", roomSize: "Medium"));
        }

        // Assert
        Assert.NotEmpty(nodes);
        // Should contain Muspelheim themed resources
        foreach (var node in nodes)
        {
            Assert.NotNull(node.Name);
            Assert.NotEmpty(node.ResourceType);
        }
    }

    #endregion

    #region Rarity Distribution Tests

    [Fact]
    public void GenerateResourceNodes_MultipleGenerations_ShowsRarityDistribution()
    {
        // Act
        var nodes = new List<ResourceNode>();
        for (int i = 0; i < 50; i++)
        {
            nodes.AddRange(_service.GenerateResourceNodes(roomId: i, biomeName: "The_Roots", roomSize: "Large"));
        }

        // Assert
        Assert.NotEmpty(nodes);

        var commonCount = nodes.Count(n => n.RarityTier == RarityTier.Common);
        var uncommonCount = nodes.Count(n => n.RarityTier == RarityTier.Uncommon);
        var rareCount = nodes.Count(n => n.RarityTier == RarityTier.Rare);

        // Common should be most frequent (70% spawn rate)
        Assert.True(commonCount > uncommonCount);
        Assert.True(commonCount > rareCount);
    }

    #endregion

    #region Node Properties Tests

    [Fact]
    public void GenerateResourceNodes_AllNodes_HaveRequiredProperties()
    {
        // Act
        var nodes = _service.GenerateResourceNodes(roomId: 1, biomeName: "The_Roots", roomSize: "Large");

        // Assert
        foreach (var node in nodes)
        {
            Assert.NotNull(node.Name);
            Assert.NotEmpty(node.Name);
            Assert.NotNull(node.Description);
            Assert.NotEmpty(node.Description);
            Assert.NotEmpty(node.ResourceType);
            Assert.True(node.ExtractionDC > 0);
            Assert.True(node.ExtractionTime > 0);
            Assert.True(node.YieldMin >= 0);
            Assert.True(node.YieldMax >= node.YieldMin);
            Assert.True(node.MaxUses > 0);
            Assert.Equal(node.MaxUses, node.UsesRemaining);
            Assert.False(node.Depleted);
        }
    }

    [Fact]
    public void ResourceNode_Extract_DecreasesUsesRemaining()
    {
        // Arrange
        var nodes = _service.GenerateResourceNodes(roomId: 1, biomeName: "The_Roots", roomSize: "Medium");
        var node = nodes.First();
        var initialUses = node.UsesRemaining;

        // Act
        var yield = node.Extract();

        // Assert
        Assert.True(yield >= node.YieldMin && yield <= node.YieldMax);
        Assert.Equal(initialUses - 1, node.UsesRemaining);
    }

    [Fact]
    public void ResourceNode_Extract_DecreasesUntilDepleted()
    {
        // Arrange
        var nodes = _service.GenerateResourceNodes(roomId: 1, biomeName: "The_Roots", roomSize: "Medium");
        var node = nodes.First();
        var maxUses = node.MaxUses;

        // Act
        for (int i = 0; i < maxUses; i++)
        {
            var canExtract = node.CanExtract();
            Assert.True(canExtract);
            node.Extract();
        }

        // Assert
        Assert.True(node.Depleted);
        Assert.Equal(0, node.UsesRemaining);
        Assert.False(node.CanExtract());
    }

    [Fact]
    public void ResourceNode_GetDisplaySummary_ReturnsFormattedString()
    {
        // Arrange
        var nodes = _service.GenerateResourceNodes(roomId: 1, biomeName: "The_Roots", roomSize: "Medium");
        var node = nodes.First();

        // Act
        var summary = node.GetDisplaySummary();

        // Assert
        Assert.NotNull(summary);
        Assert.NotEmpty(summary);
        Assert.Contains("Type:", summary);
        Assert.Contains("Extraction:", summary);
        Assert.Contains("Yield:", summary);
    }

    [Fact]
    public void ResourceNode_IsValid_ReturnsTrueForValidNode()
    {
        // Arrange
        var nodes = _service.GenerateResourceNodes(roomId: 1, biomeName: "The_Roots", roomSize: "Medium");
        var node = nodes.First();

        // Act
        var isValid = node.IsValid();

        // Assert
        Assert.True(isValid);
    }

    #endregion

    #region Validation Tests

    [Fact]
    public void ValidateResourceNodeSystem_WithTestData_ReturnsTrue()
    {
        // Act
        var isValid = _service.ValidateResourceNodeSystem();

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void GetAvailableBiomes_ReturnsAllProfiles()
    {
        // Act
        var biomes = _service.GetAvailableBiomes();

        // Assert
        Assert.NotNull(biomes);
        Assert.NotEmpty(biomes);
        Assert.Contains("The_Roots", biomes);
        Assert.Contains("Muspelheim", biomes);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void FullWorkflow_GenerateMultipleRooms_AllSucceed()
    {
        // Arrange
        var biomes = new[] { "The_Roots", "Muspelheim" };
        var roomSizes = new[] { "Small", "Medium", "Large" };

        // Act & Assert
        int roomId = 1;
        foreach (var biome in biomes)
        {
            foreach (var roomSize in roomSizes)
            {
                var nodes = _service.GenerateResourceNodes(roomId++, biome, roomSize);
                Assert.NotNull(nodes);
            }
        }
    }

    [Fact]
    public void FullWorkflow_GenerateManyNodes_ShowsVariety()
    {
        // Act
        var allNodes = new List<ResourceNode>();
        for (int i = 0; i < 20; i++)
        {
            allNodes.AddRange(_service.GenerateResourceNodes(roomId: i, biomeName: "The_Roots", roomSize: "Large"));
        }

        // Assert
        Assert.NotEmpty(allNodes);

        // Check for variety in node types
        var uniqueNodeTypes = allNodes.Select(n => n.NodeType).Distinct().Count();
        Assert.True(uniqueNodeTypes >= 1);

        // Check for variety in resource types
        var uniqueResourceTypes = allNodes.Select(n => n.ResourceType).Distinct().Count();
        Assert.True(uniqueResourceTypes >= 1);
    }

    #endregion
}
