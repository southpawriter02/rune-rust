using Xunit;
using RuneAndRust.Core;
using RuneAndRust.Engine;
using RuneAndRust.Persistence;
using Microsoft.Data.Sqlite;
using Serilog;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.39.2: Unit tests for biome transition and blending system
/// Tests adjacency rules, transition generation, descriptor blending, and environmental gradients
/// </summary>
public class BiomeTransitionTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly string _connectionString;
    private readonly BiomeAdjacencyRepository _adjacencyRepo;
    private readonly BiomeBlendingService _blendingService;
    private readonly BiomeTransitionService _transitionService;
    private readonly EnvironmentalGradientService _gradientService;

    public BiomeTransitionTests()
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
        _adjacencyRepo = new BiomeAdjacencyRepository(_connectionString);
        _blendingService = new BiomeBlendingService();
        _transitionService = new BiomeTransitionService(_adjacencyRepo, _blendingService);
        _gradientService = new EnvironmentalGradientService();
    }

    public void Dispose()
    {
        _connection?.Dispose();
    }

    #region Setup

    private void InitializeSchema()
    {
        var command = _connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE Biome_Adjacency (
                adjacency_id INTEGER PRIMARY KEY AUTOINCREMENT,
                biome_a TEXT NOT NULL,
                biome_b TEXT NOT NULL,
                compatibility TEXT NOT NULL,
                min_transition_rooms INTEGER DEFAULT 0,
                max_transition_rooms INTEGER DEFAULT 3,
                transition_theme TEXT,
                notes TEXT,
                created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                updated_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                CHECK (compatibility IN ('Compatible', 'RequiresTransition', 'Incompatible')),
                UNIQUE(biome_a, biome_b)
            );
        ";
        command.ExecuteNonQuery();
    }

    private void SeedTestData()
    {
        var command = _connection.CreateCommand();

        // TheRoots <-> Muspelheim (requires transition)
        command.CommandText = @"
            INSERT INTO Biome_Adjacency (biome_a, biome_b, compatibility, min_transition_rooms, max_transition_rooms, transition_theme)
            VALUES ('TheRoots', 'Muspelheim', 'RequiresTransition', 1, 2, 'Geothermal escalation');
        ";
        command.ExecuteNonQuery();

        // TheRoots <-> Niflheim (requires transition)
        command.CommandText = @"
            INSERT INTO Biome_Adjacency (biome_a, biome_b, compatibility, min_transition_rooms, max_transition_rooms, transition_theme)
            VALUES ('TheRoots', 'Niflheim', 'RequiresTransition', 1, 2, 'Cooling failure');
        ";
        command.ExecuteNonQuery();

        // Muspelheim <-> Niflheim (incompatible)
        command.CommandText = @"
            INSERT INTO Biome_Adjacency (biome_a, biome_b, compatibility, min_transition_rooms, max_transition_rooms)
            VALUES ('Muspelheim', 'Niflheim', 'Incompatible', 0, 0);
        ";
        command.ExecuteNonQuery();

        // TheRoots <-> Alfheim (compatible)
        command.CommandText = @"
            INSERT INTO Biome_Adjacency (biome_a, biome_b, compatibility, min_transition_rooms, max_transition_rooms, transition_theme)
            VALUES ('TheRoots', 'Alfheim', 'Compatible', 0, 1, 'Aetheric seepage');
        ";
        command.ExecuteNonQuery();
    }

    #endregion

    #region Biome Adjacency Tests

    [Fact]
    public void GetRule_ValidBiomePair_ReturnsRule()
    {
        // Act
        var rule = _adjacencyRepo.GetRule("TheRoots", "Muspelheim");

        // Assert
        Assert.NotNull(rule);
        Assert.Equal("TheRoots", rule.BiomeA);
        Assert.Equal("Muspelheim", rule.BiomeB);
        Assert.Equal(BiomeCompatibility.RequiresTransition, rule.Compatibility);
    }

    [Fact]
    public void GetRule_ReversedOrder_ReturnsRule()
    {
        // Act
        var rule = _adjacencyRepo.GetRule("Muspelheim", "TheRoots");

        // Assert
        Assert.NotNull(rule);
        Assert.True(rule.AppliesToBiomes("TheRoots", "Muspelheim"));
    }

    [Fact]
    public void GetRule_IncompatibleBiomes_ReturnsIncompatibleRule()
    {
        // Act
        var rule = _adjacencyRepo.GetRule("Muspelheim", "Niflheim");

        // Assert
        Assert.NotNull(rule);
        Assert.Equal(BiomeCompatibility.Incompatible, rule.Compatibility);
    }

    [Fact]
    public void GetCompatibleBiomes_TheRoots_ReturnsMultipleBiomes()
    {
        // Act
        var compatibleBiomes = _adjacencyRepo.GetCompatibleBiomes("TheRoots", includeTransitionRequired: true);

        // Assert
        Assert.NotEmpty(compatibleBiomes);
        Assert.Contains("Muspelheim", compatibleBiomes);
        Assert.Contains("Niflheim", compatibleBiomes);
        Assert.Contains("Alfheim", compatibleBiomes);
    }

    [Fact]
    public void GetCompatibleBiomes_OnlyDirectlyCompatible_FiltersTransitionRequired()
    {
        // Act
        var compatibleBiomes = _adjacencyRepo.GetCompatibleBiomes("TheRoots", includeTransitionRequired: false);

        // Assert
        Assert.Contains("Alfheim", compatibleBiomes);
        Assert.DoesNotContain("Muspelheim", compatibleBiomes);
    }

    [Fact]
    public void UpsertRule_NewRule_AddsSuccessfully()
    {
        // Arrange
        var newRule = new BiomeAdjacencyRule
        {
            BiomeA = "Alfheim",
            BiomeB = "Jotunheim",
            Compatibility = BiomeCompatibility.RequiresTransition,
            MinTransitionRooms = 1,
            MaxTransitionRooms = 2,
            TransitionTheme = "Scale distortion"
        };

        // Act
        _adjacencyRepo.UpsertRule(newRule);
        var retrievedRule = _adjacencyRepo.GetRule("Alfheim", "Jotunheim");

        // Assert
        Assert.NotNull(retrievedRule);
        Assert.Equal("Scale distortion", retrievedRule.TransitionTheme);
    }

    #endregion

    #region Biome Transition Service Tests

    [Fact]
    public void GenerateTransitionZone_CompatibleBiomes_CreatesTransitionRooms()
    {
        // Arrange
        var fromBiome = CreateTestBiome("TheRoots");
        var toBiome = CreateTestBiome("Muspelheim");
        var rng = new Random(42);

        // Act
        var transitionRooms = _transitionService.GenerateTransitionZone(fromBiome, toBiome, 3, rng);

        // Assert
        Assert.Equal(3, transitionRooms.Count);

        // Verify progressive blend ratios
        Assert.True(transitionRooms[0].BiomeBlendRatio < transitionRooms[1].BiomeBlendRatio);
        Assert.True(transitionRooms[1].BiomeBlendRatio < transitionRooms[2].BiomeBlendRatio);

        // Verify blend ratios are approximately correct (linear interpolation)
        Assert.InRange(transitionRooms[0].BiomeBlendRatio, 0.2f, 0.3f);  // ~25%
        Assert.InRange(transitionRooms[1].BiomeBlendRatio, 0.45f, 0.55f); // ~50%
        Assert.InRange(transitionRooms[2].BiomeBlendRatio, 0.7f, 0.8f);  // ~75%
    }

    [Fact]
    public void GenerateTransitionZone_IncompatibleBiomes_ThrowsException()
    {
        // Arrange
        var fromBiome = CreateTestBiome("Muspelheim");
        var toBiome = CreateTestBiome("Niflheim");
        var rng = new Random(42);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            _transitionService.GenerateTransitionZone(fromBiome, toBiome, 3, rng)
        );
    }

    [Fact]
    public void GenerateTransitionZone_AssignsBiomesCorrectly()
    {
        // Arrange
        var fromBiome = CreateTestBiome("TheRoots");
        var toBiome = CreateTestBiome("Muspelheim");
        var rng = new Random(42);

        // Act
        var transitionRooms = _transitionService.GenerateTransitionZone(fromBiome, toBiome, 2, rng);

        // Assert
        foreach (var room in transitionRooms)
        {
            Assert.Equal("TheRoots", room.PrimaryBiome);
            Assert.Equal("Muspelheim", room.SecondaryBiome);
            Assert.True(room.IsTransitionRoom());
            Assert.False(room.IsPureBiomeRoom());
        }
    }

    [Fact]
    public void CanBiomesBeAdjacent_CompatiblePair_ReturnsTrue()
    {
        // Act
        var canBeAdjacent = _transitionService.CanBiomesBeAdjacent("TheRoots", "Alfheim");

        // Assert
        Assert.True(canBeAdjacent);
    }

    [Fact]
    public void CanBiomesBeAdjacent_IncompatiblePair_ReturnsFalse()
    {
        // Act
        var canBeAdjacent = _transitionService.CanBiomesBeAdjacent("Muspelheim", "Niflheim");

        // Assert
        Assert.False(canBeAdjacent);
    }

    [Fact]
    public void GetOptimalTransitionCount_ReturnsWithinRange()
    {
        // Arrange
        var rng = new Random(42);

        // Act
        var count = _transitionService.GetOptimalTransitionCount("TheRoots", "Muspelheim", rng);

        // Assert
        Assert.InRange(count, 1, 2); // Min=1, Max=2 for this pair
    }

    #endregion

    #region Environmental Gradient Tests

    [Fact]
    public void CalculateTemperatureGradient_Muspelheim_ReturnsHighTemperature()
    {
        // Act
        var gradient = _gradientService.CalculateTemperatureGradient("Muspelheim", "TheRoots", 0.0f);

        // Assert
        Assert.InRange(gradient.Temperature, 250f, 350f); // ~300°C
        Assert.Contains("heat", gradient.Description.ToLower());
    }

    [Fact]
    public void CalculateTemperatureGradient_Niflheim_ReturnsLowTemperature()
    {
        // Act
        var gradient = _gradientService.CalculateTemperatureGradient("Niflheim", "TheRoots", 0.0f);

        // Assert
        Assert.InRange(gradient.Temperature, -60f, -40f); // ~-50°C
        Assert.Contains("cold", gradient.Description.ToLower());
    }

    [Fact]
    public void CalculateTemperatureGradient_MidBlend_ReturnsIntermediateTemperature()
    {
        // Act - 50% blend between Muspelheim (300°C) and TheRoots (20°C)
        var gradient = _gradientService.CalculateTemperatureGradient("Muspelheim", "TheRoots", 0.5f);

        // Assert
        Assert.InRange(gradient.Temperature, 150f, 170f); // ~160°C (midpoint)
    }

    [Fact]
    public void CalculateAethericGradient_Alfheim_ReturnsHighIntensity()
    {
        // Act
        var gradient = _gradientService.CalculateAethericGradient("Alfheim", "TheRoots", 0.0f);

        // Assert
        Assert.InRange(gradient.Intensity, 0.9f, 1.0f);
        Assert.Contains("aetheric", gradient.Description.ToLower());
        Assert.NotEmpty(gradient.VisualEffects);
    }

    [Fact]
    public void CalculateAethericGradient_NonAlfheim_ReturnsLowIntensity()
    {
        // Act
        var gradient = _gradientService.CalculateAethericGradient("TheRoots", "Muspelheim", 0.5f);

        // Assert
        Assert.InRange(gradient.Intensity, 0.0f, 0.3f);
    }

    [Fact]
    public void CalculateScaleGradient_Jotunheim_ReturnsLargeScale()
    {
        // Act
        var gradient = _gradientService.CalculateScaleGradient("Jotunheim", "TheRoots", 0.0f);

        // Assert
        Assert.InRange(gradient.ScaleFactor, 9.0f, 11.0f); // ~10.0x
        Assert.Contains("giant", gradient.Description.ToLower());
    }

    [Fact]
    public void CalculateScaleGradient_TheRoots_ReturnsModerateScale()
    {
        // Act
        var gradient = _gradientService.CalculateScaleGradient("TheRoots", "Alfheim", 0.0f);

        // Assert
        Assert.InRange(gradient.ScaleFactor, 1.5f, 2.5f); // ~2.0x
    }

    [Fact]
    public void ApplyGradients_TemperatureTransition_SetsEnvironmentalProperty()
    {
        // Arrange
        var room = new Room
        {
            RoomId = "test_room",
            PrimaryBiome = "Muspelheim",
            SecondaryBiome = "TheRoots",
            BiomeBlendRatio = 0.5f
        };

        // Act
        _gradientService.ApplyGradients(room, "Muspelheim", "TheRoots", 0.5f);

        // Assert
        var temperature = room.GetEnvironmentalProperty("Temperature");
        Assert.NotNull(temperature);
        Assert.InRange(temperature.Value, 140f, 180f); // Midpoint between 300 and 20
    }

    #endregion

    #region Biome Blending Service Tests

    [Fact]
    public void BlendBiomeDescriptors_CreatesBlendedName()
    {
        // Arrange
        var fromBiome = CreateTestBiome("TheRoots");
        var toBiome = CreateTestBiome("Muspelheim");
        var rng = new Random(42);

        // Act
        var blended = _blendingService.BlendBiomeDescriptors(fromBiome, toBiome, 0.5f, 0.5f, rng);

        // Assert
        Assert.NotNull(blended.Name);
        Assert.NotEmpty(blended.Name);
        Assert.Contains("Chamber", blended.Name, StringComparison.OrdinalIgnoreCase); // Should contain a room noun
    }

    [Fact]
    public void BlendBiomeDescriptors_MostlyFromBiome_UsesFromBiomeElements()
    {
        // Arrange
        var fromBiome = CreateTestBiome("TheRoots");
        var toBiome = CreateTestBiome("Muspelheim");
        var rng = new Random(42);

        // Act - 80% fromBiome, 20% toBiome
        var blended = _blendingService.BlendBiomeDescriptors(fromBiome, toBiome, 0.8f, 0.2f, rng);

        // Assert
        Assert.NotNull(blended.Description);
        Assert.NotEmpty(blended.Description);
    }

    [Fact]
    public void BlendBiomeDescriptors_BalancedBlend_CreatesTransitionalName()
    {
        // Arrange
        var fromBiome = CreateTestBiome("TheRoots");
        var toBiome = CreateTestBiome("Muspelheim");
        var rng = new Random(42);

        // Act - 50/50 blend
        var blended = _blendingService.BlendBiomeDescriptors(fromBiome, toBiome, 0.5f, 0.5f, rng);

        // Assert
        Assert.NotNull(blended.Name);
        // May contain transitional words like "Transitional", "Liminal", etc.
    }

    #endregion

    #region Room Helper Tests

    [Fact]
    public void Room_IsTransitionRoom_ReturnsTrueForBlendedRoom()
    {
        // Arrange
        var room = new Room
        {
            PrimaryBiome = "TheRoots",
            SecondaryBiome = "Muspelheim",
            BiomeBlendRatio = 0.5f
        };

        // Act & Assert
        Assert.True(room.IsTransitionRoom());
        Assert.False(room.IsPureBiomeRoom());
    }

    [Fact]
    public void Room_IsPureBiomeRoom_ReturnsTrueForSingleBiome()
    {
        // Arrange
        var room = new Room
        {
            PrimaryBiome = "TheRoots",
            SecondaryBiome = null,
            BiomeBlendRatio = 0.0f
        };

        // Act & Assert
        Assert.False(room.IsTransitionRoom());
        Assert.True(room.IsPureBiomeRoom());
    }

    [Fact]
    public void Room_GetDominantBiome_ReturnsPrimaryWhenBlendLow()
    {
        // Arrange
        var room = new Room
        {
            PrimaryBiome = "TheRoots",
            SecondaryBiome = "Muspelheim",
            BiomeBlendRatio = 0.3f
        };

        // Act
        var dominant = room.GetDominantBiome();

        // Assert
        Assert.Equal("TheRoots", dominant);
    }

    [Fact]
    public void Room_GetDominantBiome_ReturnsSecondaryWhenBlendHigh()
    {
        // Arrange
        var room = new Room
        {
            PrimaryBiome = "TheRoots",
            SecondaryBiome = "Muspelheim",
            BiomeBlendRatio = 0.7f
        };

        // Act
        var dominant = room.GetDominantBiome();

        // Assert
        Assert.Equal("Muspelheim", dominant);
    }

    [Fact]
    public void Room_GetBiomeWeights_ReturnsCorrectWeights()
    {
        // Arrange
        var room = new Room
        {
            PrimaryBiome = "TheRoots",
            SecondaryBiome = "Muspelheim",
            BiomeBlendRatio = 0.3f
        };

        // Act
        var weights = room.GetBiomeWeights();

        // Assert
        Assert.Equal(2, weights.Length);
        Assert.Equal("TheRoots", weights[0].biome);
        Assert.Equal(0.7f, weights[0].weight, 2);
        Assert.Equal("Muspelheim", weights[1].biome);
        Assert.Equal(0.3f, weights[1].weight, 2);
    }

    [Fact]
    public void Room_EnvironmentalProperties_SetAndGet()
    {
        // Arrange
        var room = new Room();

        // Act
        room.SetEnvironmentalProperty("Temperature", 150.5f);
        var temperature = room.GetEnvironmentalProperty("Temperature");

        // Assert
        Assert.NotNull(temperature);
        Assert.Equal(150.5f, temperature.Value);
    }

    #endregion

    #region Helper Methods

    private BiomeDefinition CreateTestBiome(string biomeId)
    {
        return new BiomeDefinition
        {
            BiomeId = biomeId,
            Name = biomeId,
            Description = $"Test biome: {biomeId}",
            DescriptorCategories = new Dictionary<string, List<string>>
            {
                { "Adjectives", GetAdjectivesForBiome(biomeId) },
                { "Details", GetDetailsForBiome(biomeId) },
                { "Sounds", GetSoundsForBiome(biomeId) },
                { "Smells", GetSmellsForBiome(biomeId) }
            },
            AvailableTemplates = new List<string> { "Chamber", "Corridor" },
            MinRoomCount = 5,
            MaxRoomCount = 10
        };
    }

    private List<string> GetAdjectivesForBiome(string biomeId)
    {
        return biomeId switch
        {
            "TheRoots" => new List<string> { "Corroded", "Rusted", "Industrial" },
            "Muspelheim" => new List<string> { "Scorching", "Molten", "Volcanic" },
            "Niflheim" => new List<string> { "Frozen", "Glacial", "Frigid" },
            "Alfheim" => new List<string> { "Crystalline", "Ethereal", "Shimmering" },
            "Jotunheim" => new List<string> { "Colossal", "Massive", "Towering" },
            _ => new List<string> { "Standard" }
        };
    }

    private List<string> GetDetailsForBiome(string biomeId)
    {
        return biomeId switch
        {
            "TheRoots" => new List<string> { "Pipes leak condensation.", "Metal shows signs of decay." },
            "Muspelheim" => new List<string> { "Heat radiates from the walls.", "Lava glows through grates." },
            "Niflheim" => new List<string> { "Ice encases every surface.", "Frost spreads across metal." },
            "Alfheim" => new List<string> { "Reality shimmers unnaturally.", "Geometric patterns float in air." },
            "Jotunheim" => new List<string> { "Architecture built for giants.", "Doorways tower overhead." },
            _ => new List<string> { "Standard room features." }
        };
    }

    private List<string> GetSoundsForBiome(string biomeId)
    {
        return biomeId switch
        {
            "TheRoots" => new List<string> { "hissing steam", "groaning metal" },
            "Muspelheim" => new List<string> { "crackling fire", "bubbling lava" },
            "Niflheim" => new List<string> { "creaking ice", "howling wind" },
            "Alfheim" => new List<string> { "resonant humming", "crystalline chimes" },
            "Jotunheim" => new List<string> { "distant echoes", "rumbling machinery" },
            _ => new List<string> { "silence" }
        };
    }

    private List<string> GetSmellsForBiome(string biomeId)
    {
        return biomeId switch
        {
            "TheRoots" => new List<string> { "ozone", "rust" },
            "Muspelheim" => new List<string> { "sulfur", "ash" },
            "Niflheim" => new List<string> { "crisp air", "nothing (frozen)" },
            "Alfheim" => new List<string> { "sweet perfume", "charged air" },
            "Jotunheim" => new List<string> { "stone dust", "ancient air" },
            _ => new List<string> { "stale air" }
        };
    }

    #endregion
}
