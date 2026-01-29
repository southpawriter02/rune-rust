// ═══════════════════════════════════════════════════════════════════════════════
// BackgroundProviderTests.cs
// Unit tests for BackgroundProvider (v0.17.1d).
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Exceptions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Infrastructure.Services;

#pragma warning disable NUnit2045 // Use Assert.Multiple

namespace RuneAndRust.Application.UnitTests.Providers;

/// <summary>
/// Unit tests for <see cref="BackgroundProvider"/> (v0.17.1d).
/// </summary>
/// <remarks>
/// <para>Tests cover:</para>
/// <list type="bullet">
///   <item><description>Loading background definitions from JSON configuration</description></item>
///   <item><description>Retrieving all backgrounds and individual backgrounds by enum</description></item>
///   <item><description>Accessing individual components (selection text, skill grants, equipment grants, hooks)</description></item>
///   <item><description>Error handling for missing or invalid configuration</description></item>
///   <item><description>Caching behavior verification</description></item>
///   <item><description>HasBackground existence checks</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class BackgroundProviderTests
{
    // ═══════════════════════════════════════════════════════════════
    // TEST FIXTURES
    // ═══════════════════════════════════════════════════════════════

    private Mock<ILogger<BackgroundProvider>> _mockLogger = null!;
    private string _testConfigPath = null!;

    /// <summary>
    /// Sets up test dependencies before each test.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        _mockLogger = new Mock<ILogger<BackgroundProvider>>();

        // Use the actual config path for integration-style tests
        var baseDir = TestContext.CurrentContext.TestDirectory;
        _testConfigPath = Path.Combine(baseDir, "..", "..", "..", "..", "..", "config", "backgrounds.json");
    }

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that constructor throws when logger is null.
    /// </summary>
    [Test]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => new BackgroundProvider(null!, _testConfigPath);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    /// <summary>
    /// Verifies that constructor succeeds with valid parameters.
    /// </summary>
    [Test]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        // Arrange & Act
        var provider = new BackgroundProvider(_mockLogger.Object, _testConfigPath);

        // Assert
        provider.Should().NotBeNull();
    }

    // ═══════════════════════════════════════════════════════════════
    // GetAllBackgrounds TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetAllBackgrounds returns exactly 6 backgrounds.
    /// </summary>
    [Test]
    public void GetAllBackgrounds_ReturnsExactlySixBackgrounds()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new BackgroundProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var backgrounds = provider.GetAllBackgrounds();

        // Assert
        backgrounds.Should().HaveCount(6);
        backgrounds.Should().Contain(b => b.BackgroundId == Background.VillageSmith);
        backgrounds.Should().Contain(b => b.BackgroundId == Background.TravelingHealer);
        backgrounds.Should().Contain(b => b.BackgroundId == Background.RuinDelver);
        backgrounds.Should().Contain(b => b.BackgroundId == Background.ClanGuard);
        backgrounds.Should().Contain(b => b.BackgroundId == Background.WanderingSkald);
        backgrounds.Should().Contain(b => b.BackgroundId == Background.OutcastScavenger);
    }

    // ═══════════════════════════════════════════════════════════════
    // GetBackground TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetBackground returns correct definition for a valid background.
    /// </summary>
    [Test]
    public void GetBackground_WithValidBackground_ReturnsDefinition()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new BackgroundProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var villageSmith = provider.GetBackground(Background.VillageSmith);

        // Assert
        villageSmith.Should().NotBeNull();
        villageSmith!.DisplayName.Should().Be("Village Smith");
        villageSmith.ProfessionBefore.Should().Be("Blacksmith and metalworker");
        villageSmith.SocialStanding.Should().Contain("Respected craftsperson");
    }

    /// <summary>
    /// Verifies that GetBackground returns Clan Guard with correct properties.
    /// </summary>
    [Test]
    public void GetBackground_ClanGuard_HasCorrectProperties()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new BackgroundProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var clanGuard = provider.GetBackground(Background.ClanGuard);

        // Assert
        clanGuard.Should().NotBeNull();
        clanGuard!.DisplayName.Should().Be("Clan Guard");
        clanGuard.ProfessionBefore.Should().Be("Warrior and protector");
        clanGuard.SocialStanding.Should().Contain("Honored defender");
        clanGuard.HasNarrativeHooks().Should().BeTrue();
        clanGuard.HasSkillGrants().Should().BeTrue();
        clanGuard.HasEquipmentGrants().Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════════════════
    // GetSelectionText TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetSelectionText returns text for a valid background.
    /// </summary>
    [Test]
    public void GetSelectionText_WithValidBackground_ReturnsText()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new BackgroundProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var text = provider.GetSelectionText(Background.VillageSmith);

        // Assert
        text.Should().Contain("hammer on anvil");
    }

    /// <summary>
    /// Verifies that GetSelectionText returns default message for invalid background.
    /// </summary>
    [Test]
    public void GetSelectionText_WithInvalidBackground_ReturnsDefaultMessage()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new BackgroundProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var text = provider.GetSelectionText((Background)999);

        // Assert
        text.Should().Contain("Unknown background");
    }

    // ═══════════════════════════════════════════════════════════════
    // GetSkillGrants TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetSkillGrants returns correct grants for Village Smith.
    /// </summary>
    [Test]
    public void GetSkillGrants_ForVillageSmith_ReturnsCraftAndMight()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new BackgroundProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var grants = provider.GetSkillGrants(Background.VillageSmith);

        // Assert
        grants.Should().HaveCount(2);
        grants.Should().Contain(g => g.SkillId == "craft" && g.BonusAmount == 2);
        grants.Should().Contain(g => g.SkillId == "might" && g.BonusAmount == 1);
    }

    /// <summary>
    /// Verifies that GetSkillGrants returns empty list for invalid background.
    /// </summary>
    [Test]
    public void GetSkillGrants_WithInvalidBackground_ReturnsEmptyList()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new BackgroundProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var grants = provider.GetSkillGrants((Background)999);

        // Assert
        grants.Should().BeEmpty();
    }

    // ═══════════════════════════════════════════════════════════════
    // GetEquipmentGrants TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetEquipmentGrants returns correct grants for Clan Guard.
    /// </summary>
    [Test]
    public void GetEquipmentGrants_ForClanGuard_ReturnsShieldAndSpear()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new BackgroundProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var grants = provider.GetEquipmentGrants(Background.ClanGuard);

        // Assert
        grants.Should().HaveCount(2);
        grants.Should().Contain(g => g.ItemId == "shield" && g.IsEquipped && g.Slot == EquipmentSlot.Shield);
        grants.Should().Contain(g => g.ItemId == "spear" && g.IsEquipped && g.Slot == EquipmentSlot.Weapon);
    }

    /// <summary>
    /// Verifies that GetEquipmentGrants returns consumables with correct quantities.
    /// </summary>
    [Test]
    public void GetEquipmentGrants_ForTravelingHealer_ReturnsBandagesx5()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new BackgroundProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var grants = provider.GetEquipmentGrants(Background.TravelingHealer);

        // Assert
        grants.Should().HaveCount(2);
        grants.Should().Contain(g => g.ItemId == "bandages" && g.Quantity == 5);
        grants.Should().Contain(g => g.ItemId == "healers-kit" && g.Quantity == 1);
    }

    // ═══════════════════════════════════════════════════════════════
    // GetNarrativeHooks TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetNarrativeHooks returns hooks for a valid background.
    /// </summary>
    [Test]
    public void GetNarrativeHooks_ForRuinDelver_ReturnsThreeHooks()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new BackgroundProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var hooks = provider.GetNarrativeHooks(Background.RuinDelver);

        // Assert
        hooks.Should().HaveCount(3);
        hooks.Should().Contain(h => h.Contains("ruin", StringComparison.OrdinalIgnoreCase));
    }

    // ═══════════════════════════════════════════════════════════════
    // HasBackground TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that HasBackground returns true for a loaded background.
    /// </summary>
    [Test]
    public void HasBackground_WithLoadedBackground_ReturnsTrue()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new BackgroundProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var exists = provider.HasBackground(Background.WanderingSkald);

        // Assert
        exists.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that HasBackground returns false for an invalid background.
    /// </summary>
    [Test]
    public void HasBackground_WithInvalidBackground_ReturnsFalse()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new BackgroundProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var exists = provider.HasBackground((Background)999);

        // Assert
        exists.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════
    // ERROR HANDLING TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that provider throws when config file is missing.
    /// </summary>
    [Test]
    public void GetAllBackgrounds_WithMissingFile_ThrowsBackgroundConfigurationException()
    {
        // Arrange
        var provider = new BackgroundProvider(_mockLogger.Object, "/nonexistent/path/backgrounds.json");

        // Act
        var act = () => provider.GetAllBackgrounds();

        // Assert
        act.Should().Throw<BackgroundConfigurationException>()
            .WithMessage("*not found*");
    }

    // ═══════════════════════════════════════════════════════════════
    // CACHING TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that backgrounds are cached after first access.
    /// </summary>
    [Test]
    public void GetBackground_IsCachedAfterFirstAccess()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new BackgroundProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var first = provider.GetBackground(Background.VillageSmith);
        var second = provider.GetBackground(Background.VillageSmith);

        // Assert - Same instance should be returned (reference equality)
        ReferenceEquals(first, second).Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════════════════
    // ALL BACKGROUNDS DATA VERIFICATION TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that each background has both skill and equipment grants loaded.
    /// </summary>
    [Test]
    public void GetAllBackgrounds_AllBackgroundsHaveSkillAndEquipmentGrants()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new BackgroundProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var backgrounds = provider.GetAllBackgrounds();

        // Assert - every background should have both skill and equipment grants
        foreach (var bg in backgrounds)
        {
            bg.HasSkillGrants().Should().BeTrue(
                $"{bg.DisplayName} should have skill grants");
            bg.HasEquipmentGrants().Should().BeTrue(
                $"{bg.DisplayName} should have equipment grants");
            bg.HasNarrativeHooks().Should().BeTrue(
                $"{bg.DisplayName} should have narrative hooks");
            bg.GetTotalSkillBonus().Should().Be(3,
                $"{bg.DisplayName} should have total skill bonus of 3 (+2 primary, +1 secondary)");
        }
    }

    /// <summary>
    /// Verifies that Outcast Scavenger grants Survival +2 and Stealth +1.
    /// </summary>
    [Test]
    public void GetSkillGrants_ForOutcastScavenger_ReturnsSurvivalAndStealth()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new BackgroundProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var grants = provider.GetSkillGrants(Background.OutcastScavenger);

        // Assert
        grants.Should().HaveCount(2);
        grants.Should().Contain(g => g.SkillId == "survival" && g.BonusAmount == 2);
        grants.Should().Contain(g => g.SkillId == "stealth" && g.BonusAmount == 1);
    }
}
