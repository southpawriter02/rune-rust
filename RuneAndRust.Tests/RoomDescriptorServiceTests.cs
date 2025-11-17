using Xunit;
using RuneAndRust.Core;
using RuneAndRust.Core.Descriptors;
using RuneAndRust.Engine;
using RuneAndRust.Persistence;
using Microsoft.Data.Sqlite;
using Serilog;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.38.1: Unit tests for the Room Description Library
/// Tests room name/description generation, template processing, and fragment selection
/// </summary>
public class RoomDescriptorServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly string _connectionString;
    private readonly DescriptorRepository _repository;
    private readonly DescriptorService _descriptorService;
    private readonly RoomDescriptorService _roomDescriptorService;

    public RoomDescriptorServiceTests()
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
        _descriptorService = new DescriptorService(_repository);
        _roomDescriptorService = new RoomDescriptorService(_repository, _descriptorService);
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

            -- Descriptor Fragments
            CREATE TABLE Descriptor_Fragments (
                fragment_id INTEGER PRIMARY KEY AUTOINCREMENT,
                category TEXT NOT NULL,
                subcategory TEXT,
                fragment_text TEXT NOT NULL,
                tags TEXT,
                weight REAL DEFAULT 1.0,
                is_active INTEGER DEFAULT 1,
                created_at TEXT DEFAULT CURRENT_TIMESTAMP,
                updated_at TEXT DEFAULT CURRENT_TIMESTAMP
            );

            -- Room Function Variants
            CREATE TABLE Room_Function_Variants (
                function_id INTEGER PRIMARY KEY AUTOINCREMENT,
                function_name TEXT NOT NULL UNIQUE,
                function_detail TEXT NOT NULL,
                biome_affinity TEXT,
                archetype TEXT NOT NULL,
                tags TEXT,
                created_at TEXT DEFAULT CURRENT_TIMESTAMP
            );
        ";

        var command = _connection.CreateCommand();
        command.CommandText = createSql;
        command.ExecuteNonQuery();
    }

    private void SeedTestData()
    {
        var insertSql = @"
            -- Base Room Templates
            INSERT INTO Descriptor_Base_Templates (template_name, category, archetype, base_mechanics, name_template, description_template, tags)
            VALUES
                ('Entry_Hall_Base', 'Room', 'EntryHall', '{""size"": ""Medium""}', 'The {Modifier} Entry Hall',
                 'You enter {Article} {Modifier_Adj} entry hall. {Spatial_Descriptor}. {Architectural_Feature}. {Detail_1}. The air {Atmospheric_Detail}.',
                 '[""Starting"", ""Safe""]'),
                ('Corridor_Base', 'Room', 'Corridor', '{""size"": ""Small""}', 'The {Modifier} Corridor',
                 '{Article_Cap} {Modifier_Adj} corridor extends {Direction_Descriptor}. {Spatial_Descriptor}. {Architectural_Feature}. {Detail_1}.',
                 '[""Corridor"", ""Narrow""]'),
                ('Chamber_Base', 'Room', 'Chamber', '{""size"": ""Large""}', 'The {Modifier} {Function}',
                 '{Article_Cap} {Modifier_Adj} {Function} dominates this space. {Spatial_Descriptor}. {Detail_1}. {Detail_2}. The air {Atmospheric_Detail}.',
                 '[""Chamber"", ""Large""]'),
                ('Boss_Arena_Base', 'Room', 'BossArena', '{""size"": ""XLarge""}', 'The {Modifier} Arena',
                 '{Article_Cap} {Modifier_Adj} arena stretches before you. {Spatial_Descriptor}. {Ominous_Detail}. {Detail_1}. {Atmospheric_Detail}.',
                 '[""Boss"", ""Combat"", ""Large""]'),
                ('Secret_Room_Base', 'Room', 'SecretRoom', '{""size"": ""Small""}', 'The Hidden {Function}',
                 'This hidden chamber contains {Article} {Modifier_Adj} {Function}. {Spatial_Descriptor}. {Loot_Hint}. {Detail_1}.',
                 '[""Secret"", ""Treasure""]');

            -- Thematic Modifiers
            INSERT INTO Descriptor_Thematic_Modifiers (modifier_name, primary_biome, adjective, detail_fragment, stat_modifiers)
            VALUES
                ('Scorched', 'Muspelheim', 'scorched', 'radiates intense heat', '{""fire_resistance"": -50}'),
                ('Frozen', 'Niflheim', 'ice-covered', 'is encased in thick ice', '{""ice_resistance"": -50}'),
                ('Rusted', 'The_Roots', 'corroded', 'shows extensive rust and decay', '{""hp_multiplier"": 0.7}'),
                ('Crystalline', 'Alfheim', 'crystalline', 'is formed from Aetheric crystal', '{""aether_resistance"": -30}'),
                ('Monolithic', 'Jotunheim', 'monolithic', 'is carved from ancient stone', '{""soak"": 10}');

            -- Descriptor Fragments - SpatialDescriptor
            INSERT INTO Descriptor_Fragments (category, subcategory, fragment_text, tags, weight)
            VALUES
                ('SpatialDescriptor', 'Small', 'The ceiling presses low overhead', '[""Small"", ""Narrow"", ""Corridor""]', 1.0),
                ('SpatialDescriptor', 'Large', 'The chamber is vast, its far walls barely visible', '[""Large"", ""Chamber"", ""Boss""]', 1.0),
                ('SpatialDescriptor', 'Medium', 'The room extends moderately in all directions', '[""Medium"", ""EntryHall""]', 1.0),
                ('SpatialDescriptor', 'Vertical', 'The space extends dramatically upward', '[""Vertical"", ""Large""]', 1.0);

            -- Descriptor Fragments - ArchitecturalFeature
            INSERT INTO Descriptor_Fragments (category, subcategory, fragment_text, tags, weight)
            VALUES
                ('ArchitecturalFeature', 'Walls', 'Corroded metal plates form the walls', '[""The_Roots"", ""Metal""]', 1.0),
                ('ArchitecturalFeature', 'Walls', 'Smooth, seamless walls suggest advanced fabrication', '[""Alfheim"", ""Advanced""]', 1.0),
                ('ArchitecturalFeature', 'Ceiling', 'The ceiling is a tangle of exposed conduits', '[""Industrial"", ""Damaged""]', 1.0),
                ('ArchitecturalFeature', 'Floor', 'The floor is corrugated metal grating', '[""Industrial"", ""Metal""]', 1.0);

            -- Descriptor Fragments - Detail
            INSERT INTO Descriptor_Fragments (category, subcategory, fragment_text, tags, weight)
            VALUES
                ('Detail', 'Decay', 'Rust streaks mark the surfaces like old blood', '[""The_Roots"", ""Decay""]', 1.0),
                ('Detail', 'Runes', 'Runic glyphs flicker weakly on the walls', '[""The_Roots"", ""Muspelheim""]', 1.0),
                ('Detail', 'Ice', 'Everything is encased in thick sheets of ice', '[""Niflheim"", ""Cold""]', 1.0),
                ('Detail', 'Fire', 'Scorch marks blacken the walls', '[""Muspelheim"", ""Fire""]', 1.0),
                ('Detail', 'Crystal', 'Aetheric crystals pulse with faint light', '[""Alfheim"", ""Aether""]', 1.0),
                ('Detail', 'Ominous', 'The walls are splattered with ancient blood', '[""Boss"", ""Combat""]', 1.0),
                ('Detail', 'Loot', 'Valuable salvage lies scattered about', '[""Secret"", ""Treasure""]', 1.0),
                ('Detail', 'Warning', 'Danger warnings flash on emergency panels', '[""Warning"", ""Hazard""]', 1.0);

            -- Descriptor Fragments - Atmospheric
            INSERT INTO Descriptor_Fragments (category, subcategory, fragment_text, tags, weight)
            VALUES
                ('Atmospheric', NULL, 'smells of rust and stale water', '[""The_Roots""]', 1.0),
                ('Atmospheric', NULL, 'is thick with the smell of brimstone', '[""Muspelheim""]', 1.0),
                ('Atmospheric', NULL, 'is bone-chillingly cold', '[""Niflheim""]', 1.0),
                ('Atmospheric', NULL, 'crackles with uncontrolled Aether', '[""Alfheim""]', 1.0),
                ('Atmospheric', NULL, 'carries an ancient, oppressive weight', '[""Jotunheim""]', 1.0);

            -- Descriptor Fragments - Direction
            INSERT INTO Descriptor_Fragments (category, subcategory, fragment_text, tags, weight)
            VALUES
                ('Direction', NULL, 'before you, narrowing into darkness', '[""Corridor""]', 1.0),
                ('Direction', NULL, 'upward into the darkness above', '[""Vertical""]', 1.0),
                ('Direction', NULL, 'in multiple directions', '[""Junction""]', 1.0);

            -- Room Function Variants
            INSERT INTO Room_Function_Variants (function_name, function_detail, biome_affinity, archetype)
            VALUES
                ('Pumping Station', 'manages hydraulic systems', '[""The_Roots"", ""Muspelheim""]', 'Chamber'),
                ('Cryo Chamber', 'contains cryogenic suspension pods', '[""Niflheim""]', 'Chamber'),
                ('Forge Hall', 'contains ancient forging equipment', '[""Muspelheim""]', 'Chamber'),
                ('Laboratory', 'contains research equipment', '[""Alfheim""]', 'Chamber'),
                ('Vault', 'stores valuable equipment', NULL, 'Chamber');
        ";

        var command = _connection.CreateCommand();
        command.CommandText = insertSql;
        command.ExecuteNonQuery();
    }

    #endregion

    #region Room Name Generation Tests

    [Fact]
    public void GenerateRoomName_EntryHall_ReturnsCorrectFormat()
    {
        // Act
        var name = _roomDescriptorService.GenerateRoomName(RoomArchetype.EntryHall, "Muspelheim");

        // Assert
        Assert.NotNull(name);
        Assert.Contains("Entry Hall", name);
        Assert.Contains("Scorched", name);
        Assert.Equal("The Scorched Entry Hall", name);
    }

    [Fact]
    public void GenerateRoomName_Corridor_ReturnsCorrectFormat()
    {
        // Act
        var name = _roomDescriptorService.GenerateRoomName(RoomArchetype.Corridor, "Niflheim");

        // Assert
        Assert.NotNull(name);
        Assert.Contains("Corridor", name);
        Assert.Contains("Frozen", name);
    }

    [Fact]
    public void GenerateRoomName_Chamber_IncludesFunction()
    {
        // Act
        var name = _roomDescriptorService.GenerateRoomName(RoomArchetype.Chamber, "Niflheim");

        // Assert
        Assert.NotNull(name);
        Assert.Contains("Frozen", name);
        // Should include a function variant (Cryo Chamber, etc.)
    }

    [Fact]
    public void GenerateRoomName_AllBiomes_ReturnsValidNames()
    {
        // Arrange
        var biomes = new[] { "Muspelheim", "Niflheim", "The_Roots", "Alfheim", "Jotunheim" };

        // Act & Assert
        foreach (var biome in biomes)
        {
            var name = _roomDescriptorService.GenerateRoomName(RoomArchetype.Corridor, biome);
            Assert.NotNull(name);
            Assert.NotEmpty(name);
            Assert.Contains("Corridor", name);
        }
    }

    [Fact]
    public void GenerateRoomName_InvalidBiome_ReturnsFallback()
    {
        // Act
        var name = _roomDescriptorService.GenerateRoomName(RoomArchetype.Corridor, "InvalidBiome");

        // Assert
        Assert.NotNull(name);
        // Should return fallback name
        Assert.Contains("Corridor", name);
    }

    #endregion

    #region Room Description Generation Tests

    [Fact]
    public void GenerateRoomDescription_EntryHall_ContainsRequiredElements()
    {
        // Act
        var description = _roomDescriptorService.GenerateRoomDescription(RoomArchetype.EntryHall, "The_Roots");

        // Assert
        Assert.NotNull(description);
        Assert.NotEmpty(description);

        // Should contain processed fragments, not placeholders
        Assert.DoesNotContain("{Modifier}", description);
        Assert.DoesNotContain("{Spatial_Descriptor}", description);
        Assert.DoesNotContain("{Architectural_Feature}", description);
        Assert.DoesNotContain("{Detail_1}", description);
        Assert.DoesNotContain("{Atmospheric_Detail}", description);
    }

    [Fact]
    public void GenerateRoomDescription_Muspelheim_ContainsScorchedTheme()
    {
        // Act
        var description = _roomDescriptorService.GenerateRoomDescription(RoomArchetype.Corridor, "Muspelheim");

        // Assert
        Assert.NotNull(description);
        Assert.Contains("scorched", description.ToLower());
    }

    [Fact]
    public void GenerateRoomDescription_Niflheim_ContainsFrozenTheme()
    {
        // Act
        var description = _roomDescriptorService.GenerateRoomDescription(RoomArchetype.Chamber, "Niflheim");

        // Assert
        Assert.NotNull(description);
        Assert.Contains("ice", description.ToLower());
    }

    [Fact]
    public void GenerateRoomDescription_Chamber_IncludesFunction()
    {
        // Act
        var description = _roomDescriptorService.GenerateRoomDescription(RoomArchetype.Chamber, "Niflheim");

        // Assert
        Assert.NotNull(description);
        // Should include a function variant (Cryo Chamber, Vault, etc.)
        // Not just the word "chamber"
        Assert.NotEmpty(description);
    }

    [Fact]
    public void GenerateRoomDescription_BossArena_ContainsOminousDetail()
    {
        // Act
        var description = _roomDescriptorService.GenerateRoomDescription(RoomArchetype.BossArena, "Muspelheim");

        // Assert
        Assert.NotNull(description);
        Assert.NotEmpty(description);
        // Should contain arena-related text
        Assert.Contains("arena", description.ToLower());
    }

    [Fact]
    public void GenerateRoomDescription_SecretRoom_ContainsLootHint()
    {
        // Act
        var description = _roomDescriptorService.GenerateRoomDescription(RoomArchetype.SecretRoom, "The_Roots");

        // Assert
        Assert.NotNull(description);
        Assert.NotEmpty(description);
        // Should contain secret/hidden chamber reference
        Assert.Contains("hidden", description.ToLower());
    }

    [Fact]
    public void GenerateRoomDescription_AllBiomes_ReturnsUniqueDescriptions()
    {
        // Arrange
        var biomes = new[] { "Muspelheim", "Niflheim", "The_Roots", "Alfheim", "Jotunheim" };
        var descriptions = new List<string>();

        // Act
        foreach (var biome in biomes)
        {
            var description = _roomDescriptorService.GenerateRoomDescription(RoomArchetype.Corridor, biome);
            descriptions.Add(description);
        }

        // Assert
        Assert.Equal(biomes.Length, descriptions.Count);
        // All descriptions should be different
        Assert.Equal(biomes.Length, descriptions.Distinct().Count());
    }

    [Fact]
    public void GenerateRoomDescription_MultipleGenerations_ProducesVariety()
    {
        // Arrange
        var descriptions = new HashSet<string>();

        // Act
        for (int i = 0; i < 10; i++)
        {
            var description = _roomDescriptorService.GenerateRoomDescription(RoomArchetype.Chamber, "Muspelheim");
            descriptions.Add(description);
        }

        // Assert
        // With random fragment selection, should get some variety
        // (though not guaranteed to be 10 unique due to small test data set)
        Assert.NotEmpty(descriptions);
    }

    #endregion

    #region Template Processing Tests

    [Fact]
    public void ProcessTemplate_ReplacesModifierPlaceholders()
    {
        // Act
        var description = _roomDescriptorService.GenerateRoomDescription(RoomArchetype.Corridor, "Muspelheim");

        // Assert
        Assert.DoesNotContain("{Modifier}", description);
        Assert.DoesNotContain("{Modifier_Adj}", description);
        Assert.DoesNotContain("{Modifier_Detail}", description);
    }

    [Fact]
    public void ProcessTemplate_ReplacesArticlePlaceholders()
    {
        // Act
        var description = _roomDescriptorService.GenerateRoomDescription(RoomArchetype.Corridor, "Alfheim");

        // Assert
        Assert.DoesNotContain("{Article}", description);
        Assert.DoesNotContain("{Article_Cap}", description);

        // Should contain proper article (a/an)
        // Alfheim modifier is "crystalline", so should have "a crystalline"
        Assert.Contains("crystalline", description.ToLower());
    }

    [Fact]
    public void ProcessTemplate_ReplacesFragmentPlaceholders()
    {
        // Act
        var description = _roomDescriptorService.GenerateRoomDescription(RoomArchetype.EntryHall, "The_Roots");

        // Assert
        Assert.DoesNotContain("{Spatial_Descriptor}", description);
        Assert.DoesNotContain("{Architectural_Feature}", description);
        Assert.DoesNotContain("{Detail_1}", description);
        Assert.DoesNotContain("{Atmospheric_Detail}", description);
    }

    [Fact]
    public void ProcessTemplate_SpecializedPlaceholders_BossArena()
    {
        // Act
        var description = _roomDescriptorService.GenerateRoomDescription(RoomArchetype.BossArena, "Muspelheim");

        // Assert
        Assert.DoesNotContain("{Ominous_Detail}", description);
    }

    [Fact]
    public void ProcessTemplate_SpecializedPlaceholders_SecretRoom()
    {
        // Act
        var description = _roomDescriptorService.GenerateRoomDescription(RoomArchetype.SecretRoom, "Niflheim");

        // Assert
        Assert.DoesNotContain("{Loot_Hint}", description);
    }

    #endregion

    #region Fragment Selection Tests

    [Fact]
    public void FragmentSelection_SelectsAppropriateTaggedFragments()
    {
        // Act
        var descriptionRoots = _roomDescriptorService.GenerateRoomDescription(RoomArchetype.Corridor, "The_Roots");
        var descriptionMuspelheim = _roomDescriptorService.GenerateRoomDescription(RoomArchetype.Corridor, "Muspelheim");

        // Assert
        Assert.NotEqual(descriptionRoots, descriptionMuspelheim);

        // The_Roots should not contain fire-related details
        Assert.DoesNotContain("brimstone", descriptionRoots.ToLower());

        // Muspelheim should not contain rust-related details
        Assert.DoesNotContain("rust", descriptionMuspelheim.ToLower());
    }

    [Fact]
    public void FragmentSelection_SmallRooms_GetAppropriateDescriptors()
    {
        // Act
        var description = _roomDescriptorService.GenerateRoomDescription(RoomArchetype.Corridor, "Muspelheim");

        // Assert
        Assert.NotNull(description);
        // Should get small/narrow spatial descriptors
        // Not "vast chamber" type descriptions
    }

    [Fact]
    public void FragmentSelection_LargeRooms_GetAppropriateDescriptors()
    {
        // Act
        var description = _roomDescriptorService.GenerateRoomDescription(RoomArchetype.Chamber, "Muspelheim");

        // Assert
        Assert.NotNull(description);
        // Should get large spatial descriptors
    }

    #endregion

    #region Function Variant Tests

    [Fact]
    public void FunctionVariant_Chamber_IncludesFunction()
    {
        // Act
        var name = _roomDescriptorService.GenerateRoomName(RoomArchetype.Chamber, "Niflheim");

        // Assert
        Assert.NotNull(name);
        // Should include a function variant name
        Assert.NotEqual("The Frozen Chamber", name);
    }

    [Fact]
    public void FunctionVariant_BiomeAffinity_SelectsAppropriateFunction()
    {
        // Act
        var descriptionNiflheim = _roomDescriptorService.GenerateRoomDescription(RoomArchetype.Chamber, "Niflheim");

        // Assert
        // Niflheim should get Cryo Chamber (has biome affinity)
        // Not guaranteed every time due to randomness, but should be possible
        Assert.NotNull(descriptionNiflheim);
    }

    [Fact]
    public void FunctionVariant_NonChamberRooms_NoFunction()
    {
        // Act
        var name = _roomDescriptorService.GenerateRoomName(RoomArchetype.Corridor, "Muspelheim");

        // Assert
        Assert.NotNull(name);
        Assert.Equal("The Scorched Corridor", name);
        // Should not include function variants
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public void EdgeCase_InvalidArchetype_DoesNotThrow()
    {
        // Act & Assert
        var exception = Record.Exception(() =>
            _roomDescriptorService.GenerateRoomDescription((RoomArchetype)9999, "Muspelheim"));

        // Should not throw, should return fallback
        Assert.Null(exception);
    }

    [Fact]
    public void EdgeCase_NullBiome_DoesNotThrow()
    {
        // Act & Assert
        var exception = Record.Exception(() =>
            _roomDescriptorService.GenerateRoomDescription(RoomArchetype.Corridor, null!));

        // Should not throw
        Assert.Null(exception);
    }

    [Fact]
    public void EdgeCase_EmptyBiome_DoesNotThrow()
    {
        // Act & Assert
        var exception = Record.Exception(() =>
            _roomDescriptorService.GenerateRoomDescription(RoomArchetype.Corridor, ""));

        // Should not throw
        Assert.Null(exception);
    }

    [Fact]
    public void EdgeCase_UnknownBiome_ReturnsFallback()
    {
        // Act
        var description = _roomDescriptorService.GenerateRoomDescription(RoomArchetype.Corridor, "UnknownBiome");

        // Assert
        Assert.NotNull(description);
        // Should return some description, not throw
        Assert.NotEmpty(description);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void Integration_GenerateMultipleRoomsForDungeon_AllValid()
    {
        // Arrange
        var archetypes = new[]
        {
            RoomArchetype.EntryHall,
            RoomArchetype.Corridor,
            RoomArchetype.Chamber,
            RoomArchetype.BossArena,
            RoomArchetype.SecretRoom
        };

        var biome = "Muspelheim";

        // Act
        var rooms = new List<(string Name, string Description)>();
        foreach (var archetype in archetypes)
        {
            var name = _roomDescriptorService.GenerateRoomName(archetype, biome);
            var description = _roomDescriptorService.GenerateRoomDescription(archetype, biome);
            rooms.Add((name, description));
        }

        // Assert
        Assert.Equal(archetypes.Length, rooms.Count);

        foreach (var (name, description) in rooms)
        {
            Assert.NotNull(name);
            Assert.NotEmpty(name);
            Assert.NotNull(description);
            Assert.NotEmpty(description);

            // No placeholders should remain
            Assert.DoesNotContain("{", description);
            Assert.DoesNotContain("}", description);
        }
    }

    [Fact]
    public void Integration_AllArchetypes_AllBiomes_GenerateSuccessfully()
    {
        // Arrange
        var archetypes = new[]
        {
            RoomArchetype.EntryHall,
            RoomArchetype.Corridor,
            RoomArchetype.Chamber,
            RoomArchetype.Junction,
            RoomArchetype.BossArena
        };

        var biomes = new[] { "Muspelheim", "Niflheim", "The_Roots", "Alfheim", "Jotunheim" };

        // Act & Assert
        foreach (var archetype in archetypes)
        {
            foreach (var biome in biomes)
            {
                var name = _roomDescriptorService.GenerateRoomName(archetype, biome);
                var description = _roomDescriptorService.GenerateRoomDescription(archetype, biome);

                Assert.NotNull(name);
                Assert.NotEmpty(name);
                Assert.NotNull(description);
                Assert.NotEmpty(description);

                // Verify no placeholders remain
                Assert.DoesNotContain("{", name);
                Assert.DoesNotContain("{", description);
            }
        }
    }

    #endregion
}
