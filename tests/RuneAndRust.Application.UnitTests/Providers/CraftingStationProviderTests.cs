using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Infrastructure.Configuration;
using RuneAndRust.Infrastructure.Providers;

namespace RuneAndRust.Application.UnitTests.Providers;

/// <summary>
/// Unit tests for CraftingStationProvider (v0.11.2a).
/// </summary>
/// <remarks>
/// <para>Tests cover:</para>
/// <list type="bullet">
///   <item><description>Loading station definitions from settings</description></item>
///   <item><description>GetStation lookup by ID</description></item>
///   <item><description>GetAllStations enumeration</description></item>
///   <item><description>GetStationsForCategory filtering</description></item>
///   <item><description>GetStationsBySkill filtering</description></item>
///   <item><description>GetCraftingSkill retrieval</description></item>
///   <item><description>Exists and GetStationCount</description></item>
///   <item><description>Case-insensitive lookups</description></item>
///   <item><description>Empty configuration handling</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class CraftingStationProviderTests
{
    // ═══════════════════════════════════════════════════════════════
    // FIELDS
    // ═══════════════════════════════════════════════════════════════

    private Mock<ILogger<CraftingStationProvider>> _mockLogger = null!;
    private CraftingStationSettings _settings = null!;

    // ═══════════════════════════════════════════════════════════════
    // SETUP
    // ═══════════════════════════════════════════════════════════════

    [SetUp]
    public void SetUp()
    {
        _mockLogger = new Mock<ILogger<CraftingStationProvider>>();

        // Setup test settings with crafting station configurations
        _settings = new CraftingStationSettings
        {
            Stations =
            [
                new CraftingStationConfigDto
                {
                    Id = "anvil",
                    Name = "Anvil",
                    Description = "A sturdy iron anvil for smithing metal items.",
                    SupportedCategories = ["Weapon", "Tool", "Material"],
                    CraftingSkill = "smithing",
                    Icon = "icons/stations/anvil.png"
                },
                new CraftingStationConfigDto
                {
                    Id = "workbench",
                    Name = "Workbench",
                    Description = "A sturdy workbench for crafting leather and cloth items.",
                    SupportedCategories = ["Armor", "Accessory"],
                    CraftingSkill = "leatherworking",
                    Icon = "icons/stations/workbench.png"
                },
                new CraftingStationConfigDto
                {
                    Id = "alchemy-table",
                    Name = "Alchemy Table",
                    Description = "A bubbling alchemy table for brewing potions.",
                    SupportedCategories = ["Potion", "Consumable"],
                    CraftingSkill = "alchemy",
                    Icon = "icons/stations/alchemy-table.png"
                },
                new CraftingStationConfigDto
                {
                    Id = "enchanting-altar",
                    Name = "Enchanting Altar",
                    Description = "A glowing altar for magical enchantments.",
                    SupportedCategories = ["Accessory", "Weapon", "Armor"],
                    CraftingSkill = "enchanting",
                    Icon = "icons/stations/enchanting-altar.png"
                },
                new CraftingStationConfigDto
                {
                    Id = "cooking-fire",
                    Name = "Cooking Fire",
                    Description = "A crackling fire for preparing food.",
                    SupportedCategories = ["Consumable"],
                    CraftingSkill = "cooking",
                    Icon = null
                }
            ]
        };
    }

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that constructor throws ArgumentNullException for null settings.
    /// </summary>
    [Test]
    public void Constructor_WithNullSettings_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new CraftingStationProvider(null!, _mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("settings");
    }

    /// <summary>
    /// Verifies that constructor throws ArgumentNullException for null logger.
    /// </summary>
    [Test]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new CraftingStationProvider(Options.Create(_settings), null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    // ═══════════════════════════════════════════════════════════════
    // GetStation TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetStation returns the definition for a valid station ID.
    /// </summary>
    [Test]
    public void GetStation_WithValidId_ReturnsDefinition()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var anvil = provider.GetStation("anvil");

        // Assert
        anvil.Should().NotBeNull();
        anvil!.StationId.Should().Be("anvil");
        anvil.Name.Should().Be("Anvil");
        anvil.Description.Should().Be("A sturdy iron anvil for smithing metal items.");
        anvil.CraftingSkillId.Should().Be("smithing");
        anvil.IconPath.Should().Be("icons/stations/anvil.png");
        anvil.SupportedCategories.Should().HaveCount(3);
        anvil.SupportedCategories.Should().Contain(RecipeCategory.Weapon);
        anvil.SupportedCategories.Should().Contain(RecipeCategory.Tool);
        anvil.SupportedCategories.Should().Contain(RecipeCategory.Material);
    }

    /// <summary>
    /// Verifies that GetStation returns null for an invalid station ID.
    /// </summary>
    [Test]
    public void GetStation_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var result = provider.GetStation("unknown-station");

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Verifies that GetStation is case-insensitive.
    /// </summary>
    [Test]
    public void GetStation_CaseInsensitive_ReturnsDefinition()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var result1 = provider.GetStation("ANVIL");
        var result2 = provider.GetStation("Anvil");
        var result3 = provider.GetStation("aNvIl");

        // Assert
        result1.Should().NotBeNull();
        result2.Should().NotBeNull();
        result3.Should().NotBeNull();
        result1!.StationId.Should().Be(result2!.StationId);
        result2.StationId.Should().Be(result3!.StationId);
    }

    /// <summary>
    /// Verifies that GetStation returns null for null or empty ID.
    /// </summary>
    [Test]
    public void GetStation_WithNullOrEmptyId_ReturnsNull()
    {
        // Arrange
        var provider = CreateProvider();

        // Act & Assert
        provider.GetStation(null!).Should().BeNull();
        provider.GetStation("").Should().BeNull();
        provider.GetStation("   ").Should().BeNull();
    }

    // ═══════════════════════════════════════════════════════════════
    // GetAllStations TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetAllStations returns all loaded definitions.
    /// </summary>
    [Test]
    public void GetAllStations_ReturnsAllDefinitions()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var allStations = provider.GetAllStations();

        // Assert
        allStations.Should().HaveCount(5);
        allStations.Should().Contain(s => s.StationId == "anvil");
        allStations.Should().Contain(s => s.StationId == "workbench");
        allStations.Should().Contain(s => s.StationId == "alchemy-table");
        allStations.Should().Contain(s => s.StationId == "enchanting-altar");
        allStations.Should().Contain(s => s.StationId == "cooking-fire");
    }

    // ═══════════════════════════════════════════════════════════════
    // GetStationsForCategory TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetStationsForCategory returns stations supporting the category.
    /// </summary>
    [Test]
    public void GetStationsForCategory_ReturnsMatchingStations()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var weaponStations = provider.GetStationsForCategory(RecipeCategory.Weapon);
        var potionStations = provider.GetStationsForCategory(RecipeCategory.Potion);
        var consumableStations = provider.GetStationsForCategory(RecipeCategory.Consumable);

        // Assert
        // Weapon: anvil, enchanting-altar
        weaponStations.Should().HaveCount(2);
        weaponStations.Should().Contain(s => s.StationId == "anvil");
        weaponStations.Should().Contain(s => s.StationId == "enchanting-altar");

        // Potion: alchemy-table
        potionStations.Should().HaveCount(1);
        potionStations.Should().Contain(s => s.StationId == "alchemy-table");

        // Consumable: alchemy-table, cooking-fire
        consumableStations.Should().HaveCount(2);
        consumableStations.Should().Contain(s => s.StationId == "alchemy-table");
        consumableStations.Should().Contain(s => s.StationId == "cooking-fire");
    }

    /// <summary>
    /// Verifies that GetStationsForCategory returns stations for Armor category.
    /// </summary>
    [Test]
    public void GetStationsForCategory_Armor_ReturnsCorrectStations()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var armorStations = provider.GetStationsForCategory(RecipeCategory.Armor);

        // Assert
        // Armor: workbench, enchanting-altar
        armorStations.Should().HaveCount(2);
        armorStations.Should().Contain(s => s.StationId == "workbench");
        armorStations.Should().Contain(s => s.StationId == "enchanting-altar");
    }

    /// <summary>
    /// Verifies that GetStationsForCategory returns stations for Accessory category.
    /// </summary>
    [Test]
    public void GetStationsForCategory_Accessory_ReturnsCorrectStations()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var accessoryStations = provider.GetStationsForCategory(RecipeCategory.Accessory);

        // Assert
        // Accessory: workbench, enchanting-altar
        accessoryStations.Should().HaveCount(2);
        accessoryStations.Should().Contain(s => s.StationId == "workbench");
        accessoryStations.Should().Contain(s => s.StationId == "enchanting-altar");
    }

    /// <summary>
    /// Verifies that GetStationsForCategory returns stations for Tool category.
    /// </summary>
    [Test]
    public void GetStationsForCategory_Tool_ReturnsCorrectStations()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var toolStations = provider.GetStationsForCategory(RecipeCategory.Tool);

        // Assert
        // Tool: anvil only
        toolStations.Should().HaveCount(1);
        toolStations.Should().Contain(s => s.StationId == "anvil");
    }

    /// <summary>
    /// Verifies that GetStationsForCategory returns stations for Material category.
    /// </summary>
    [Test]
    public void GetStationsForCategory_Material_ReturnsCorrectStations()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var materialStations = provider.GetStationsForCategory(RecipeCategory.Material);

        // Assert
        // Material: anvil only
        materialStations.Should().HaveCount(1);
        materialStations.Should().Contain(s => s.StationId == "anvil");
    }

    // ═══════════════════════════════════════════════════════════════
    // GetStationsBySkill TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetStationsBySkill returns stations using the specified skill.
    /// </summary>
    [Test]
    public void GetStationsBySkill_ReturnsMatchingStations()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var smithingStations = provider.GetStationsBySkill("smithing");
        var alchemyStations = provider.GetStationsBySkill("alchemy");
        var enchantingStations = provider.GetStationsBySkill("enchanting");

        // Assert
        smithingStations.Should().HaveCount(1);
        smithingStations.Should().Contain(s => s.StationId == "anvil");

        alchemyStations.Should().HaveCount(1);
        alchemyStations.Should().Contain(s => s.StationId == "alchemy-table");

        enchantingStations.Should().HaveCount(1);
        enchantingStations.Should().Contain(s => s.StationId == "enchanting-altar");
    }

    /// <summary>
    /// Verifies that GetStationsBySkill is case-insensitive.
    /// </summary>
    [Test]
    public void GetStationsBySkill_CaseInsensitive_ReturnsMatchingStations()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var result1 = provider.GetStationsBySkill("SMITHING");
        var result2 = provider.GetStationsBySkill("Smithing");
        var result3 = provider.GetStationsBySkill("smithing");

        // Assert
        result1.Should().HaveCount(1);
        result2.Should().HaveCount(1);
        result3.Should().HaveCount(1);
    }

    /// <summary>
    /// Verifies that GetStationsBySkill returns empty for unknown skill.
    /// </summary>
    [Test]
    public void GetStationsBySkill_UnknownSkill_ReturnsEmpty()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var result = provider.GetStationsBySkill("unknown-skill");

        // Assert
        result.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies that GetStationsBySkill returns empty for null or empty skill.
    /// </summary>
    [Test]
    public void GetStationsBySkill_NullOrEmpty_ReturnsEmpty()
    {
        // Arrange
        var provider = CreateProvider();

        // Act & Assert
        provider.GetStationsBySkill(null!).Should().BeEmpty();
        provider.GetStationsBySkill("").Should().BeEmpty();
        provider.GetStationsBySkill("   ").Should().BeEmpty();
    }

    // ═══════════════════════════════════════════════════════════════
    // GetCraftingSkill TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetCraftingSkill returns the skill for a valid station.
    /// </summary>
    [Test]
    public void GetCraftingSkill_WithValidStation_ReturnsSkill()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var anvilSkill = provider.GetCraftingSkill("anvil");
        var alchemySkill = provider.GetCraftingSkill("alchemy-table");
        var cookingSkill = provider.GetCraftingSkill("cooking-fire");

        // Assert
        anvilSkill.Should().Be("smithing");
        alchemySkill.Should().Be("alchemy");
        cookingSkill.Should().Be("cooking");
    }

    /// <summary>
    /// Verifies that GetCraftingSkill returns null for an invalid station.
    /// </summary>
    [Test]
    public void GetCraftingSkill_WithInvalidStation_ReturnsNull()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var result = provider.GetCraftingSkill("unknown-station");

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Verifies that GetCraftingSkill returns null for null or empty station ID.
    /// </summary>
    [Test]
    public void GetCraftingSkill_NullOrEmpty_ReturnsNull()
    {
        // Arrange
        var provider = CreateProvider();

        // Act & Assert
        provider.GetCraftingSkill(null!).Should().BeNull();
        provider.GetCraftingSkill("").Should().BeNull();
        provider.GetCraftingSkill("   ").Should().BeNull();
    }

    // ═══════════════════════════════════════════════════════════════
    // Exists TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that Exists returns true for existing station.
    /// </summary>
    [Test]
    public void Exists_WithValidStation_ReturnsTrue()
    {
        // Arrange
        var provider = CreateProvider();

        // Act & Assert
        provider.Exists("anvil").Should().BeTrue();
        provider.Exists("workbench").Should().BeTrue();
        provider.Exists("alchemy-table").Should().BeTrue();
        provider.Exists("enchanting-altar").Should().BeTrue();
        provider.Exists("cooking-fire").Should().BeTrue();
    }

    /// <summary>
    /// Verifies that Exists returns false for non-existing station.
    /// </summary>
    [Test]
    public void Exists_WithInvalidStation_ReturnsFalse()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var result = provider.Exists("unknown-station");

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that Exists is case-insensitive.
    /// </summary>
    [Test]
    public void Exists_CaseInsensitive_ReturnsTrue()
    {
        // Arrange
        var provider = CreateProvider();

        // Act & Assert
        provider.Exists("ANVIL").Should().BeTrue();
        provider.Exists("Anvil").Should().BeTrue();
        provider.Exists("aNvIl").Should().BeTrue();
    }

    /// <summary>
    /// Verifies that Exists returns false for null or empty station ID.
    /// </summary>
    [Test]
    public void Exists_NullOrEmpty_ReturnsFalse()
    {
        // Arrange
        var provider = CreateProvider();

        // Act & Assert
        provider.Exists(null!).Should().BeFalse();
        provider.Exists("").Should().BeFalse();
        provider.Exists("   ").Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════
    // GetStationCount TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetStationCount returns the correct count.
    /// </summary>
    [Test]
    public void GetStationCount_ReturnsCorrectCount()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var count = provider.GetStationCount();

        // Assert
        count.Should().Be(5);
    }

    // ═══════════════════════════════════════════════════════════════
    // EMPTY CONFIGURATION TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that provider handles empty configuration gracefully.
    /// </summary>
    [Test]
    public void Provider_WithEmptyConfiguration_ReturnsEmptyResults()
    {
        // Arrange
        var emptySettings = new CraftingStationSettings
        {
            Stations = []
        };
        var provider = new CraftingStationProvider(
            Options.Create(emptySettings),
            _mockLogger.Object);

        // Act & Assert
        provider.GetStationCount().Should().Be(0);
        provider.GetAllStations().Should().BeEmpty();
        provider.GetStation("anvil").Should().BeNull();
        provider.GetStationsForCategory(RecipeCategory.Weapon).Should().BeEmpty();
        provider.GetStationsBySkill("smithing").Should().BeEmpty();
        provider.GetCraftingSkill("anvil").Should().BeNull();
        provider.Exists("anvil").Should().BeFalse();
    }

    /// <summary>
    /// Verifies that provider handles null Stations list gracefully.
    /// </summary>
    [Test]
    public void Provider_WithNullStationsList_ReturnsEmptyResults()
    {
        // Arrange
        var nullSettings = new CraftingStationSettings
        {
            Stations = null!
        };
        var provider = new CraftingStationProvider(
            Options.Create(nullSettings),
            _mockLogger.Object);

        // Act & Assert
        provider.GetStationCount().Should().Be(0);
        provider.GetAllStations().Should().BeEmpty();
    }

    // ═══════════════════════════════════════════════════════════════
    // DUPLICATE HANDLING TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that provider skips duplicate station IDs.
    /// </summary>
    [Test]
    public void Provider_WithDuplicateStationIds_SkipsDuplicates()
    {
        // Arrange
        var settingsWithDuplicates = new CraftingStationSettings
        {
            Stations =
            [
                new CraftingStationConfigDto
                {
                    Id = "anvil",
                    Name = "Anvil",
                    Description = "First anvil",
                    SupportedCategories = ["Weapon"],
                    CraftingSkill = "smithing"
                },
                new CraftingStationConfigDto
                {
                    Id = "anvil", // Duplicate
                    Name = "Second Anvil",
                    Description = "Duplicate anvil",
                    SupportedCategories = ["Tool"],
                    CraftingSkill = "smithing"
                }
            ]
        };
        var provider = new CraftingStationProvider(
            Options.Create(settingsWithDuplicates),
            _mockLogger.Object);

        // Act
        var count = provider.GetStationCount();
        var anvil = provider.GetStation("anvil");

        // Assert
        count.Should().Be(1);
        anvil.Should().NotBeNull();
        anvil!.Name.Should().Be("Anvil"); // First one wins
        anvil.Description.Should().Be("First anvil");
    }

    // ═══════════════════════════════════════════════════════════════
    // INVALID CONFIGURATION TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that provider skips station configs with missing ID.
    /// </summary>
    [Test]
    public void Provider_WithMissingId_SkipsInvalidConfig()
    {
        // Arrange
        var settingsWithMissingId = new CraftingStationSettings
        {
            Stations =
            [
                new CraftingStationConfigDto
                {
                    Id = null!, // Missing
                    Name = "Test Station",
                    Description = "Description",
                    SupportedCategories = ["Weapon"],
                    CraftingSkill = "smithing"
                },
                new CraftingStationConfigDto
                {
                    Id = "valid-station",
                    Name = "Valid Station",
                    Description = "Valid description",
                    SupportedCategories = ["Weapon"],
                    CraftingSkill = "smithing"
                }
            ]
        };
        var provider = new CraftingStationProvider(
            Options.Create(settingsWithMissingId),
            _mockLogger.Object);

        // Act
        var count = provider.GetStationCount();

        // Assert
        count.Should().Be(1);
        provider.Exists("valid-station").Should().BeTrue();
    }

    /// <summary>
    /// Verifies that provider skips station configs with empty categories.
    /// </summary>
    [Test]
    public void Provider_WithEmptyCategories_SkipsInvalidConfig()
    {
        // Arrange
        var settingsWithEmptyCategories = new CraftingStationSettings
        {
            Stations =
            [
                new CraftingStationConfigDto
                {
                    Id = "empty-categories-station",
                    Name = "Test Station",
                    Description = "Description",
                    SupportedCategories = [], // Empty
                    CraftingSkill = "smithing"
                },
                new CraftingStationConfigDto
                {
                    Id = "valid-station",
                    Name = "Valid Station",
                    Description = "Valid description",
                    SupportedCategories = ["Weapon"],
                    CraftingSkill = "smithing"
                }
            ]
        };
        var provider = new CraftingStationProvider(
            Options.Create(settingsWithEmptyCategories),
            _mockLogger.Object);

        // Act
        var count = provider.GetStationCount();

        // Assert
        count.Should().Be(1);
        provider.Exists("valid-station").Should().BeTrue();
        provider.Exists("empty-categories-station").Should().BeFalse();
    }

    /// <summary>
    /// Verifies that provider skips invalid category strings.
    /// </summary>
    [Test]
    public void Provider_WithInvalidCategoryStrings_SkipsInvalidCategories()
    {
        // Arrange
        var settingsWithInvalidCategories = new CraftingStationSettings
        {
            Stations =
            [
                new CraftingStationConfigDto
                {
                    Id = "mixed-categories-station",
                    Name = "Test Station",
                    Description = "Description",
                    SupportedCategories = ["Weapon", "InvalidCategory", "Armor"],
                    CraftingSkill = "smithing"
                }
            ]
        };
        var provider = new CraftingStationProvider(
            Options.Create(settingsWithInvalidCategories),
            _mockLogger.Object);

        // Act
        var station = provider.GetStation("mixed-categories-station");

        // Assert
        station.Should().NotBeNull();
        station!.SupportedCategories.Should().HaveCount(2); // Only Weapon and Armor
        station.SupportedCategories.Should().Contain(RecipeCategory.Weapon);
        station.SupportedCategories.Should().Contain(RecipeCategory.Armor);
    }

    // ═══════════════════════════════════════════════════════════════
    // HELPER METHODS
    // ═══════════════════════════════════════════════════════════════

    private CraftingStationProvider CreateProvider()
    {
        return new CraftingStationProvider(
            Options.Create(_settings),
            _mockLogger.Object);
    }
}
