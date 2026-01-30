// ═══════════════════════════════════════════════════════════════════════════════
// AttributeProviderTests.cs
// Unit tests for AttributeProvider (v0.17.2e).
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
/// Unit tests for <see cref="AttributeProvider"/> (v0.17.2e).
/// </summary>
/// <remarks>
/// <para>Tests cover:</para>
/// <list type="bullet">
///   <item><description>Loading attribute descriptions from JSON configuration</description></item>
///   <item><description>Retrieving all attribute descriptions and individual descriptions by enum</description></item>
///   <item><description>Recommended build lookup by archetype ID (case-insensitive)</description></item>
///   <item><description>Point-buy configuration loading and access</description></item>
///   <item><description>Starting points calculation (Adept vs. standard)</description></item>
///   <item><description>Error handling for missing or invalid configuration</description></item>
///   <item><description>Constructor validation</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class AttributeProviderTests
{
    // ═══════════════════════════════════════════════════════════════
    // TEST FIXTURES
    // ═══════════════════════════════════════════════════════════════

    private Mock<ILogger<AttributeProvider>> _mockLogger = null!;
    private string _testConfigPath = null!;

    /// <summary>
    /// Sets up test dependencies before each test.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        _mockLogger = new Mock<ILogger<AttributeProvider>>();

        // Use the actual config path for integration-style tests
        var baseDir = TestContext.CurrentContext.TestDirectory;
        _testConfigPath = Path.Combine(baseDir, "..", "..", "..", "..", "..", "config", "attributes.json");
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
        var act = () => new AttributeProvider(null!, _testConfigPath);

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
        var provider = new AttributeProvider(_mockLogger.Object, _testConfigPath);

        // Assert
        provider.Should().NotBeNull();
    }

    // ═══════════════════════════════════════════════════════════════
    // GetAttributeDescription TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetAttributeDescription returns correct data for Might.
    /// </summary>
    [Test]
    public void GetAttributeDescription_ValidAttribute_ReturnsDescription()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new AttributeProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var description = provider.GetAttributeDescription(CoreAttribute.Might);

        // Assert
        description.DisplayName.Should().Be("MIGHT");
        description.Attribute.Should().Be(CoreAttribute.Might);
        description.ShortDescription.Should().Contain("Physical power");
        description.HasAffectedStats.Should().BeTrue();
        description.HasAffectedSkills.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that each CoreAttribute has a valid description with expected display names.
    /// </summary>
    /// <param name="attribute">The core attribute to check.</param>
    /// <param name="expectedDisplayName">The expected uppercase display name.</param>
    [Test]
    [TestCase(CoreAttribute.Might, "MIGHT")]
    [TestCase(CoreAttribute.Finesse, "FINESSE")]
    [TestCase(CoreAttribute.Wits, "WITS")]
    [TestCase(CoreAttribute.Will, "WILL")]
    [TestCase(CoreAttribute.Sturdiness, "STURDINESS")]
    public void GetAttributeDescription_AllAttributes_HaveCorrectDisplayNames(
        CoreAttribute attribute, string expectedDisplayName)
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new AttributeProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var description = provider.GetAttributeDescription(attribute);

        // Assert
        description.DisplayName.Should().Be(expectedDisplayName);
    }

    // ═══════════════════════════════════════════════════════════════
    // GetAllAttributeDescriptions TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetAllAttributeDescriptions returns exactly 5 descriptions.
    /// </summary>
    [Test]
    public void GetAllAttributeDescriptions_ReturnsFiveDescriptions()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new AttributeProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var descriptions = provider.GetAllAttributeDescriptions();

        // Assert
        descriptions.Should().HaveCount(5);
    }

    /// <summary>
    /// Verifies that all descriptions have affected stats and skills populated.
    /// </summary>
    [Test]
    public void GetAllAttributeDescriptions_AllHaveAffectedStatsAndSkills()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new AttributeProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var descriptions = provider.GetAllAttributeDescriptions();

        // Assert
        foreach (var desc in descriptions)
        {
            desc.HasAffectedStats.Should().BeTrue(
                $"{desc.DisplayName} should have affected stats");
            desc.HasAffectedSkills.Should().BeTrue(
                $"{desc.DisplayName} should have affected skills");
            desc.ShortDescription.Should().NotBeNullOrWhiteSpace(
                $"{desc.DisplayName} should have a short description");
            desc.DetailedDescription.Should().NotBeNullOrWhiteSpace(
                $"{desc.DisplayName} should have a detailed description");
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // GetRecommendedBuild TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetRecommendedBuild returns correct values for Warrior.
    /// </summary>
    [Test]
    public void GetRecommendedBuild_Warrior_ReturnsCorrectValues()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new AttributeProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var build = provider.GetRecommendedBuild("warrior");

        // Assert
        build.CurrentMight.Should().Be(4);
        build.CurrentFinesse.Should().Be(3);
        build.CurrentWits.Should().Be(2);
        build.CurrentWill.Should().Be(2);
        build.CurrentSturdiness.Should().Be(4);
        build.TotalPoints.Should().Be(15);
        build.IsComplete.Should().BeTrue();
        build.Mode.Should().Be(AttributeAllocationMode.Simple);
    }

    /// <summary>
    /// Verifies that all archetype builds return complete allocations.
    /// </summary>
    /// <param name="archetypeId">The archetype to check.</param>
    [Test]
    [TestCase("warrior")]
    [TestCase("skirmisher")]
    [TestCase("mystic")]
    [TestCase("adept")]
    public void GetRecommendedBuild_AllArchetypes_ReturnComplete(string archetypeId)
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new AttributeProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var build = provider.GetRecommendedBuild(archetypeId);

        // Assert
        build.IsComplete.Should().BeTrue($"{archetypeId} build should be complete");
        build.Mode.Should().Be(AttributeAllocationMode.Simple);
        build.SelectedArchetypeId.Should().Be(archetypeId);
    }

    /// <summary>
    /// Verifies that GetRecommendedBuild throws for unknown archetype.
    /// </summary>
    [Test]
    public void GetRecommendedBuild_UnknownArchetype_ThrowsArgumentException()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new AttributeProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var act = () => provider.GetRecommendedBuild("unknown");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Unknown archetype*");
    }

    /// <summary>
    /// Verifies that archetype lookup is case-insensitive.
    /// </summary>
    /// <param name="archetypeId">The archetype ID in various cases.</param>
    [Test]
    [TestCase("WARRIOR")]
    [TestCase("Warrior")]
    [TestCase("warrior")]
    [TestCase("WaRrIoR")]
    public void GetRecommendedBuild_CaseInsensitive_ReturnsCorrectBuild(string archetypeId)
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new AttributeProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var build = provider.GetRecommendedBuild(archetypeId);

        // Assert
        build.CurrentMight.Should().Be(4);
        build.CurrentSturdiness.Should().Be(4);
    }

    // ═══════════════════════════════════════════════════════════════
    // GetStartingPoints TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that Adept gets 14 starting points.
    /// </summary>
    [Test]
    public void GetStartingPoints_Adept_Returns14()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new AttributeProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var points = provider.GetStartingPoints("adept");

        // Assert
        points.Should().Be(14);
    }

    /// <summary>
    /// Verifies that Warrior gets 15 starting points.
    /// </summary>
    [Test]
    public void GetStartingPoints_Warrior_Returns15()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new AttributeProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var points = provider.GetStartingPoints("warrior");

        // Assert
        points.Should().Be(15);
    }

    /// <summary>
    /// Verifies that null archetype returns default 15 points.
    /// </summary>
    [Test]
    public void GetStartingPoints_Null_Returns15()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new AttributeProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var points = provider.GetStartingPoints(null);

        // Assert
        points.Should().Be(15);
    }

    // ═══════════════════════════════════════════════════════════════
    // GetPointBuyConfiguration TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that point-buy configuration has correct standard values.
    /// </summary>
    [Test]
    public void GetPointBuyConfiguration_ReturnsValidConfig()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new AttributeProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var config = provider.GetPointBuyConfiguration();

        // Assert
        config.StartingPoints.Should().Be(15);
        config.AdeptStartingPoints.Should().Be(14);
        config.MinAttributeValue.Should().Be(1);
        config.MaxAttributeValue.Should().Be(10);
        config.CostTableEntryCount.Should().Be(9);
        config.HasCostTable.Should().BeTrue();
        config.MaxCumulativeCost.Should().Be(11);
    }

    // ═══════════════════════════════════════════════════════════════
    // ERROR HANDLING TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that provider throws when config file is missing.
    /// </summary>
    [Test]
    public void GetAllAttributeDescriptions_WithMissingFile_ThrowsAttributeConfigurationException()
    {
        // Arrange
        var provider = new AttributeProvider(_mockLogger.Object, "/nonexistent/path/attributes.json");

        // Act
        var act = () => provider.GetAllAttributeDescriptions();

        // Assert
        act.Should().Throw<AttributeConfigurationException>()
            .WithMessage("*not found*");
    }

    // ═══════════════════════════════════════════════════════════════
    // CACHING TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that descriptions are cached after first access.
    /// </summary>
    [Test]
    public void GetAttributeDescription_IsCachedAfterFirstAccess()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new AttributeProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var first = provider.GetAttributeDescription(CoreAttribute.Might);
        var second = provider.GetAttributeDescription(CoreAttribute.Might);

        // Assert - Same values should be returned (value equality for record structs)
        first.Should().Be(second);
    }
}
