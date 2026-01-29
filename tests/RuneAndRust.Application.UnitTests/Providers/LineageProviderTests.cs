// ═══════════════════════════════════════════════════════════════════════════════
// LineageProviderTests.cs
// Unit tests for LineageProvider (v0.17.0e).
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
/// Unit tests for <see cref="LineageProvider"/> (v0.17.0e).
/// </summary>
/// <remarks>
/// <para>Tests cover:</para>
/// <list type="bullet">
///   <item><description>Loading lineage definitions from JSON configuration</description></item>
///   <item><description>Retrieving all lineages and individual lineages by enum</description></item>
///   <item><description>Accessing individual lineage components (attributes, bonuses, traits, trauma)</description></item>
///   <item><description>Error handling for missing or invalid configuration</description></item>
///   <item><description>Caching behavior verification</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class LineageProviderTests
{
    // ═══════════════════════════════════════════════════════════════
    // TEST FIXTURES
    // ═══════════════════════════════════════════════════════════════

    private Mock<ILogger<LineageProvider>> _mockLogger = null!;
    private string _testConfigPath = null!;

    /// <summary>
    /// Sets up test dependencies before each test.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        _mockLogger = new Mock<ILogger<LineageProvider>>();

        // Use the actual config path for integration-style tests
        var baseDir = TestContext.CurrentContext.TestDirectory;
        _testConfigPath = Path.Combine(baseDir, "..", "..", "..", "..", "..", "config", "lineages.json");
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
        var act = () => new LineageProvider(null!, _testConfigPath);

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
        var provider = new LineageProvider(_mockLogger.Object, _testConfigPath);

        // Assert
        provider.Should().NotBeNull();
    }

    // ═══════════════════════════════════════════════════════════════
    // GetAllLineages TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetAllLineages returns exactly 4 lineages.
    /// </summary>
    [Test]
    public void GetAllLineages_ReturnsExactlyFourLineages()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new LineageProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var lineages = provider.GetAllLineages();

        // Assert
        lineages.Should().HaveCount(4);
        lineages.Should().Contain(l => l.LineageId == Lineage.ClanBorn);
        lineages.Should().Contain(l => l.LineageId == Lineage.RuneMarked);
        lineages.Should().Contain(l => l.LineageId == Lineage.IronBlooded);
        lineages.Should().Contain(l => l.LineageId == Lineage.VargrKin);
    }

    // ═══════════════════════════════════════════════════════════════
    // GetLineage TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetLineage returns correct definition for each lineage.
    /// </summary>
    [Test]
    public void GetLineage_WithValidLineage_ReturnsDefinition()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new LineageProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var clanBorn = provider.GetLineage(Lineage.ClanBorn);

        // Assert
        clanBorn.Should().NotBeNull();
        clanBorn!.DisplayName.Should().Be("Clan-Born");
        clanBorn.SelectionText.Should().Contain("Stable Code");
    }

    /// <summary>
    /// Verifies that GetLineage returns Rune-Marked with correct properties.
    /// </summary>
    [Test]
    public void GetLineage_RuneMarked_HasCorrectProperties()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new LineageProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var runeMarked = provider.GetLineage(Lineage.RuneMarked);

        // Assert
        runeMarked.Should().NotBeNull();
        runeMarked!.DisplayName.Should().Be("Rune-Marked");
        runeMarked.AttributeModifiers.WillModifier.Should().Be(2);
        runeMarked.AttributeModifiers.SturdinessModifier.Should().Be(-1);
    }

    // ═══════════════════════════════════════════════════════════════
    // GetAttributeModifiers TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetAttributeModifiers returns correct values for Rune-Marked.
    /// </summary>
    [Test]
    public void GetAttributeModifiers_ForRuneMarked_ReturnsCorrectValues()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new LineageProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var modifiers = provider.GetAttributeModifiers(Lineage.RuneMarked);

        // Assert
        modifiers.WillModifier.Should().Be(2);
        modifiers.SturdinessModifier.Should().Be(-1);
        modifiers.MightModifier.Should().Be(0);
        modifiers.HasFlexibleBonus.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that Clan-Born has flexible bonus.
    /// </summary>
    [Test]
    public void GetAttributeModifiers_ForClanBorn_HasFlexibleBonus()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new LineageProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var modifiers = provider.GetAttributeModifiers(Lineage.ClanBorn);

        // Assert
        modifiers.HasFlexibleBonus.Should().BeTrue();
        modifiers.FlexibleBonusAmount.Should().Be(1);
    }

    // ═══════════════════════════════════════════════════════════════
    // GetPassiveBonuses TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetPassiveBonuses returns correct values for Clan-Born.
    /// </summary>
    [Test]
    public void GetPassiveBonuses_ForClanBorn_ReturnsMaxHpBonus()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new LineageProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var bonuses = provider.GetPassiveBonuses(Lineage.ClanBorn);

        // Assert
        bonuses.MaxHpBonus.Should().Be(5);
        bonuses.SkillBonuses.Should().Contain(sb => sb.SkillId == "social");
    }

    /// <summary>
    /// Verifies that Vargr-Kin has movement bonus.
    /// </summary>
    [Test]
    public void GetPassiveBonuses_ForVargrKin_HasMovementBonus()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new LineageProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var bonuses = provider.GetPassiveBonuses(Lineage.VargrKin);

        // Assert
        bonuses.MovementBonus.Should().Be(1);
        bonuses.SkillBonuses.Should().Contain(sb => sb.SkillId == "survival");
    }

    // ═══════════════════════════════════════════════════════════════
    // GetUniqueTrait TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetUniqueTrait returns Hazard Acclimation for Iron-Blooded.
    /// </summary>
    [Test]
    public void GetUniqueTrait_ForIronBlooded_ReturnsHazardAcclimation()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new LineageProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var trait = provider.GetUniqueTrait(Lineage.IronBlooded);

        // Assert
        trait.TraitName.Should().Contain("Hazard Acclimation");
        trait.BonusDice.Should().Be(1);
        trait.EffectType.Should().Be(LineageTraitEffectType.BonusDiceToResolve);
    }

    /// <summary>
    /// Verifies that Rune-Marked has Aether-Tainted trait.
    /// </summary>
    [Test]
    public void GetUniqueTrait_ForRuneMarked_ReturnsAetherTainted()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new LineageProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var trait = provider.GetUniqueTrait(Lineage.RuneMarked);

        // Assert
        trait.TraitName.Should().Contain("Aether-Tainted");
        trait.PercentModifier.Should().BeApproximately(0.10f, 0.001f);
    }

    // ═══════════════════════════════════════════════════════════════
    // GetTraumaBaseline TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetTraumaBaseline returns permanent corruption for Rune-Marked.
    /// </summary>
    [Test]
    public void GetTraumaBaseline_ForRuneMarked_ReturnsPermanentCorruption()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new LineageProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var baseline = provider.GetTraumaBaseline(Lineage.RuneMarked);

        // Assert
        baseline.StartingCorruption.Should().Be(5);
        baseline.CorruptionResistanceModifier.Should().Be(-1);
        baseline.HasPermanentCorruption.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that Iron-Blooded has stress vulnerability.
    /// </summary>
    [Test]
    public void GetTraumaBaseline_ForIronBlooded_HasStressVulnerability()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new LineageProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var baseline = provider.GetTraumaBaseline(Lineage.IronBlooded);

        // Assert
        baseline.StressResistanceModifier.Should().Be(-1);
        baseline.HasStressVulnerability.Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════════════════
    // ERROR HANDLING TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that provider throws when config file is missing.
    /// </summary>
    [Test]
    public void GetAllLineages_WithMissingFile_ThrowsLineageConfigurationException()
    {
        // Arrange
        var provider = new LineageProvider(_mockLogger.Object, "/nonexistent/path/lineages.json");

        // Act
        var act = () => provider.GetAllLineages();

        // Assert
        act.Should().Throw<LineageConfigurationException>()
            .WithMessage("*not found*");
    }

    // ═══════════════════════════════════════════════════════════════
    // CACHING TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that lineages are cached after first access.
    /// </summary>
    [Test]
    public void GetLineage_IsCachedAfterFirstAccess()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new LineageProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var first = provider.GetLineage(Lineage.ClanBorn);
        var second = provider.GetLineage(Lineage.ClanBorn);

        // Assert - Same instance should be returned (reference equality)
        ReferenceEquals(first, second).Should().BeTrue();
    }
}
