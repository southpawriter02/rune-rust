// ═══════════════════════════════════════════════════════════════════════════════
// ArchetypeProviderTests.cs
// Unit tests for ArchetypeProvider (v0.17.3e).
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
/// Unit tests for <see cref="ArchetypeProvider"/> (v0.17.3e).
/// </summary>
/// <remarks>
/// <para>Tests cover:</para>
/// <list type="bullet">
///   <item><description>Loading archetype definitions from JSON configuration</description></item>
///   <item><description>Retrieving all archetypes and individual archetypes by enum</description></item>
///   <item><description>Accessing resource bonuses, starting abilities, and specialization mappings</description></item>
///   <item><description>Selection text retrieval</description></item>
///   <item><description>Recommended build retrieval with lineage fallback</description></item>
///   <item><description>Specialization availability checking</description></item>
///   <item><description>Error handling for missing configuration</description></item>
///   <item><description>Caching behavior verification</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class ArchetypeProviderTests
{
    // ═══════════════════════════════════════════════════════════════
    // TEST FIXTURES
    // ═══════════════════════════════════════════════════════════════

    private Mock<ILogger<ArchetypeProvider>> _mockLogger = null!;
    private string _testConfigPath = null!;

    /// <summary>
    /// Sets up test dependencies before each test.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        _mockLogger = new Mock<ILogger<ArchetypeProvider>>();

        // Use the actual config path for integration-style tests
        var baseDir = TestContext.CurrentContext.TestDirectory;
        _testConfigPath = Path.Combine(baseDir, "..", "..", "..", "..", "..", "config", "archetypes.json");
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
        var act = () => new ArchetypeProvider(null!, _testConfigPath);

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
        var provider = new ArchetypeProvider(_mockLogger.Object, _testConfigPath);

        // Assert
        provider.Should().NotBeNull();
    }

    // ═══════════════════════════════════════════════════════════════
    // GetAllArchetypes TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetAllArchetypes returns exactly 4 archetypes.
    /// </summary>
    [Test]
    public void GetAllArchetypes_ReturnsFourArchetypes()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new ArchetypeProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var archetypes = provider.GetAllArchetypes();

        // Assert
        archetypes.Should().HaveCount(4);
        archetypes.Should().Contain(a => a.ArchetypeId == Archetype.Warrior);
        archetypes.Should().Contain(a => a.ArchetypeId == Archetype.Skirmisher);
        archetypes.Should().Contain(a => a.ArchetypeId == Archetype.Mystic);
        archetypes.Should().Contain(a => a.ArchetypeId == Archetype.Adept);
    }

    /// <summary>
    /// Verifies that all archetypes have required data loaded.
    /// </summary>
    [Test]
    public void GetAllArchetypes_AllHaveRequiredData()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new ArchetypeProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var archetypes = provider.GetAllArchetypes();

        // Assert — every archetype should have non-empty display metadata
        foreach (var archetype in archetypes)
        {
            archetype.DisplayName.Should().NotBeNullOrWhiteSpace(
                $"{archetype.ArchetypeId} should have a display name");
            archetype.Tagline.Should().NotBeNullOrWhiteSpace(
                $"{archetype.ArchetypeId} should have a tagline");
            archetype.Description.Should().NotBeNullOrWhiteSpace(
                $"{archetype.ArchetypeId} should have a description");
            archetype.CombatRole.Should().NotBeNullOrWhiteSpace(
                $"{archetype.ArchetypeId} should have a combat role");
            archetype.SelectionText.Should().NotBeNullOrWhiteSpace(
                $"{archetype.ArchetypeId} should have selection text");
            archetype.PlaystyleDescription.Should().NotBeNullOrWhiteSpace(
                $"{archetype.ArchetypeId} should have a playstyle description");
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // GetArchetype TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetArchetype returns the Warrior definition with correct properties.
    /// </summary>
    [Test]
    public void GetArchetype_Warrior_ReturnsDefinition()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new ArchetypeProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var warrior = provider.GetArchetype(Archetype.Warrior);

        // Assert
        warrior.Should().NotBeNull();
        warrior!.DisplayName.Should().Be("Warrior");
        warrior.Tagline.Should().Be("The Unyielding Bulwark");
        warrior.CombatRole.Should().Be("Tank / Melee DPS");
        warrior.PrimaryResource.Should().Be(ResourceType.Stamina);
    }

    /// <summary>
    /// Verifies that GetArchetype returns the Mystic definition with AetherPool resource.
    /// </summary>
    [Test]
    public void GetArchetype_Mystic_HasAetherPoolResource()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new ArchetypeProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var mystic = provider.GetArchetype(Archetype.Mystic);

        // Assert
        mystic.Should().NotBeNull();
        mystic!.PrimaryResource.Should().Be(ResourceType.AetherPool);
        mystic.CombatRole.Should().Be("Caster / Control");
    }

    /// <summary>
    /// Verifies that GetArchetype returns null for an invalid archetype value.
    /// </summary>
    [Test]
    public void GetArchetype_InvalidArchetype_ReturnsNull()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new ArchetypeProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var result = provider.GetArchetype((Archetype)999);

        // Assert
        result.Should().BeNull();
    }

    // ═══════════════════════════════════════════════════════════════
    // GetResourceBonuses TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetResourceBonuses returns correct values for the Warrior
    /// archetype: +49 HP, +5 Stamina, 0 AP, 0 Movement.
    /// </summary>
    [Test]
    public void GetResourceBonuses_Warrior_ReturnsCorrectBonuses()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new ArchetypeProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var bonuses = provider.GetResourceBonuses(Archetype.Warrior);

        // Assert
        bonuses.MaxHpBonus.Should().Be(49);
        bonuses.MaxStaminaBonus.Should().Be(5);
        bonuses.MaxAetherPoolBonus.Should().Be(0);
        bonuses.MovementBonus.Should().Be(0);
        bonuses.HasSpecialBonus.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that GetResourceBonuses returns the special ConsumableEffectiveness
    /// bonus for the Adept archetype.
    /// </summary>
    [Test]
    public void GetResourceBonuses_Adept_HasSpecialBonus()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new ArchetypeProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var bonuses = provider.GetResourceBonuses(Archetype.Adept);

        // Assert
        bonuses.MaxHpBonus.Should().Be(30);
        bonuses.HasSpecialBonus.Should().BeTrue();
        bonuses.SpecialBonus!.Value.BonusType.Should().Be("ConsumableEffectiveness");
        bonuses.SpecialBonus!.Value.BonusValue.Should().Be(0.20f);
    }

    /// <summary>
    /// Verifies that GetResourceBonuses returns the Aether Pool bonus for the Mystic.
    /// </summary>
    [Test]
    public void GetResourceBonuses_Mystic_HasAetherPoolBonus()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new ArchetypeProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var bonuses = provider.GetResourceBonuses(Archetype.Mystic);

        // Assert
        bonuses.MaxHpBonus.Should().Be(20);
        bonuses.MaxAetherPoolBonus.Should().Be(20);
        bonuses.HasAetherPoolBonus.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that GetResourceBonuses returns the Movement bonus for the Skirmisher.
    /// </summary>
    [Test]
    public void GetResourceBonuses_Skirmisher_HasMovementBonus()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new ArchetypeProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var bonuses = provider.GetResourceBonuses(Archetype.Skirmisher);

        // Assert
        bonuses.MovementBonus.Should().Be(1);
        bonuses.HasMovementBonus.Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════════════════
    // GetStartingAbilities TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetStartingAbilities returns exactly 3 abilities for the Warrior.
    /// </summary>
    [Test]
    public void GetStartingAbilities_Warrior_ReturnsThreeAbilities()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new ArchetypeProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var abilities = provider.GetStartingAbilities(Archetype.Warrior);

        // Assert
        abilities.Should().HaveCount(3);
        abilities.Should().Contain(a => a.AbilityId == "power-strike" && a.AbilityType == AbilityType.Active);
        abilities.Should().Contain(a => a.AbilityId == "defensive-stance" && a.AbilityType == AbilityType.Stance);
        abilities.Should().Contain(a => a.AbilityId == "iron-will" && a.AbilityType == AbilityType.Passive);
    }

    /// <summary>
    /// Verifies that GetStartingAbilities returns exactly 3 abilities for the Mystic.
    /// </summary>
    [Test]
    public void GetStartingAbilities_Mystic_ReturnsThreeAbilities()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new ArchetypeProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var abilities = provider.GetStartingAbilities(Archetype.Mystic);

        // Assert
        abilities.Should().HaveCount(3);
        abilities.Should().Contain(a => a.AbilityId == "aether-bolt" && a.AbilityType == AbilityType.Active);
        abilities.Should().Contain(a => a.AbilityId == "aether-shield" && a.AbilityType == AbilityType.Active);
        abilities.Should().Contain(a => a.AbilityId == "aether-sense" && a.AbilityType == AbilityType.Passive);
    }

    /// <summary>
    /// Verifies that all archetypes have exactly 3 starting abilities.
    /// </summary>
    [Test]
    public void GetStartingAbilities_AllArchetypes_HaveThreeAbilities()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new ArchetypeProvider(_mockLogger.Object, _testConfigPath);

        // Act & Assert
        foreach (var archetype in Enum.GetValues<Archetype>())
        {
            var abilities = provider.GetStartingAbilities(archetype);
            abilities.Should().HaveCount(3,
                $"Archetype {archetype} should have exactly 3 starting abilities");
        }
    }

    /// <summary>
    /// Verifies that GetStartingAbilities returns an empty list for unknown archetypes.
    /// </summary>
    [Test]
    public void GetStartingAbilities_InvalidArchetype_ReturnsEmptyList()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new ArchetypeProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var abilities = provider.GetStartingAbilities((Archetype)999);

        // Assert
        abilities.Should().BeEmpty();
    }

    // ═══════════════════════════════════════════════════════════════
    // GetSpecializationMapping TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetSpecializationMapping returns 6 specializations for the Warrior.
    /// </summary>
    [Test]
    public void GetSpecializationMapping_Warrior_ReturnsSixSpecs()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new ArchetypeProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var mapping = provider.GetSpecializationMapping(Archetype.Warrior);

        // Assert
        mapping.Count.Should().Be(6);
        mapping.RecommendedFirst.Should().Be("guardian");
    }

    /// <summary>
    /// Verifies that GetSpecializationMapping returns 2 specializations for the Mystic.
    /// </summary>
    [Test]
    public void GetSpecializationMapping_Mystic_ReturnsTwoSpecs()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new ArchetypeProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var mapping = provider.GetSpecializationMapping(Archetype.Mystic);

        // Assert
        mapping.Count.Should().Be(2);
        mapping.RecommendedFirst.Should().Be("elementalist");
    }

    // ═══════════════════════════════════════════════════════════════
    // GetSelectionText TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetSelectionText returns non-empty text for the Warrior.
    /// </summary>
    [Test]
    public void GetSelectionText_Warrior_ReturnsNonEmptyText()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new ArchetypeProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var text = provider.GetSelectionText(Archetype.Warrior);

        // Assert
        text.Should().NotBeNullOrWhiteSpace();
        text.Should().Contain("shield");
    }

    /// <summary>
    /// Verifies that GetSelectionText returns empty string for unknown archetypes.
    /// </summary>
    [Test]
    public void GetSelectionText_InvalidArchetype_ReturnsEmpty()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new ArchetypeProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var text = provider.GetSelectionText((Archetype)999);

        // Assert
        text.Should().BeEmpty();
    }

    // ═══════════════════════════════════════════════════════════════
    // GetRecommendedBuild TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetRecommendedBuild returns a build for the Warrior archetype.
    /// </summary>
    [Test]
    public void GetRecommendedBuild_Warrior_ReturnsBuild()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new ArchetypeProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var build = provider.GetRecommendedBuild(Archetype.Warrior);

        // Assert
        build.Should().NotBeNull();
        build!.Value.Name.Should().Be("Standard Warrior");
        build.Value.Might.Should().Be(4);
        build.Value.Finesse.Should().Be(3);
        build.Value.Wits.Should().Be(2);
        build.Value.Will.Should().Be(2);
        build.Value.Sturdiness.Should().Be(4);
        build.Value.TotalAttributePoints.Should().Be(15);
    }

    /// <summary>
    /// Verifies that GetRecommendedBuild returns the default build when no
    /// lineage-specific build exists.
    /// </summary>
    [Test]
    public void GetRecommendedBuild_WithLineageNoMatch_FallsBackToDefault()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new ArchetypeProvider(_mockLogger.Object, _testConfigPath);

        // Act — request a lineage-specific build that doesn't exist
        var build = provider.GetRecommendedBuild(Archetype.Warrior, Lineage.IronBlooded);

        // Assert — should fall back to default (no lineage) build
        build.Should().NotBeNull();
        build!.Value.Name.Should().Be("Standard Warrior");
        build.Value.HasOptimalLineage.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that GetRecommendedBuild returns null for unknown archetypes.
    /// </summary>
    [Test]
    public void GetRecommendedBuild_InvalidArchetype_ReturnsNull()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new ArchetypeProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var build = provider.GetRecommendedBuild((Archetype)999);

        // Assert
        build.Should().BeNull();
    }

    /// <summary>
    /// Verifies that each archetype has at least one recommended build configured.
    /// </summary>
    [Test]
    public void GetRecommendedBuild_AllArchetypes_HaveDefaultBuild()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new ArchetypeProvider(_mockLogger.Object, _testConfigPath);

        // Act & Assert
        foreach (var archetype in Enum.GetValues<Archetype>())
        {
            var build = provider.GetRecommendedBuild(archetype);
            build.Should().NotBeNull(
                $"Archetype {archetype} should have a default recommended build");
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // IsSpecializationAvailable TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that IsSpecializationAvailable returns true for a valid
    /// specialization of the Warrior archetype.
    /// </summary>
    [Test]
    public void IsSpecializationAvailable_ValidSpec_ReturnsTrue()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new ArchetypeProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var available = provider.IsSpecializationAvailable(Archetype.Warrior, "guardian");

        // Assert
        available.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that IsSpecializationAvailable returns false for a completely
    /// unknown specialization.
    /// </summary>
    [Test]
    public void IsSpecializationAvailable_InvalidSpec_ReturnsFalse()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new ArchetypeProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var available = provider.IsSpecializationAvailable(Archetype.Warrior, "nonexistent");

        // Assert
        available.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that IsSpecializationAvailable returns false when checking
    /// a specialization from a different archetype.
    /// </summary>
    [Test]
    public void IsSpecializationAvailable_CrossArchetype_ReturnsFalse()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new ArchetypeProvider(_mockLogger.Object, _testConfigPath);

        // Act — elementalist is Mystic-only
        var available = provider.IsSpecializationAvailable(Archetype.Warrior, "elementalist");

        // Assert
        available.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════
    // ERROR HANDLING TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that provider throws when config file is missing.
    /// </summary>
    [Test]
    public void GetAllArchetypes_WithMissingFile_ThrowsArchetypeConfigurationException()
    {
        // Arrange
        var provider = new ArchetypeProvider(_mockLogger.Object, "/nonexistent/path/archetypes.json");

        // Act
        var act = () => provider.GetAllArchetypes();

        // Assert
        act.Should().Throw<ArchetypeConfigurationException>()
            .WithMessage("*not found*");
    }

    // ═══════════════════════════════════════════════════════════════
    // CACHING TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that archetypes are cached after first access.
    /// </summary>
    [Test]
    public void GetArchetype_IsCachedAfterFirstAccess()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new ArchetypeProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var first = provider.GetArchetype(Archetype.Warrior);
        var second = provider.GetArchetype(Archetype.Warrior);

        // Assert — Same instance should be returned (reference equality)
        ReferenceEquals(first, second).Should().BeTrue();
    }
}
