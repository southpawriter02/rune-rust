// ═══════════════════════════════════════════════════════════════════════════════
// SpecializationProviderTests.cs
// Unit tests for SpecializationProvider (v0.17.4d).
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
/// Unit tests for <see cref="SpecializationProvider"/> (v0.17.4d).
/// </summary>
/// <remarks>
/// <para>Tests cover:</para>
/// <list type="bullet">
///   <item><description>Loading specialization definitions from JSON configuration</description></item>
///   <item><description>Retrieving all specializations and individual specializations by enum</description></item>
///   <item><description>Filtering by archetype, path type, and special resource</description></item>
///   <item><description>Ability lookup across all specializations</description></item>
///   <item><description>Count and existence checking</description></item>
///   <item><description>Error handling for missing configuration</description></item>
///   <item><description>Caching behavior verification</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class SpecializationProviderTests
{
    // ═══════════════════════════════════════════════════════════════
    // TEST FIXTURES
    // ═══════════════════════════════════════════════════════════════

    private Mock<ILogger<SpecializationProvider>> _mockLogger = null!;
    private string _testConfigPath = null!;

    /// <summary>
    /// Sets up test dependencies before each test.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        _mockLogger = new Mock<ILogger<SpecializationProvider>>();

        // Use the actual config path for integration-style tests
        var baseDir = TestContext.CurrentContext.TestDirectory;
        _testConfigPath = Path.Combine(baseDir, "..", "..", "..", "..", "..", "config", "specializations.json");
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
        var act = () => new SpecializationProvider(null!, _testConfigPath);

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
        var provider = new SpecializationProvider(_mockLogger.Object, _testConfigPath);

        // Assert
        provider.Should().NotBeNull();
    }

    // ═══════════════════════════════════════════════════════════════
    // GetAll TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetAll returns exactly 17 specializations.
    /// </summary>
    [Test]
    public void GetAll_ReturnsSeventeenSpecializations()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new SpecializationProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var all = provider.GetAll();

        // Assert
        all.Should().HaveCount(17);
    }

    /// <summary>
    /// Verifies that all specializations have required display metadata.
    /// </summary>
    [Test]
    public void GetAll_AllHaveRequiredData()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new SpecializationProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var all = provider.GetAll();

        // Assert — every specialization should have non-empty display metadata
        foreach (var spec in all)
        {
            spec.DisplayName.Should().NotBeNullOrWhiteSpace(
                $"{spec.SpecializationId} should have a display name");
            spec.Tagline.Should().NotBeNullOrWhiteSpace(
                $"{spec.SpecializationId} should have a tagline");
            spec.Description.Should().NotBeNullOrWhiteSpace(
                $"{spec.SpecializationId} should have a description");
            spec.SelectionText.Should().NotBeNullOrWhiteSpace(
                $"{spec.SpecializationId} should have selection text");
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // GetBySpecializationId TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetBySpecializationId returns Berserkr with correct properties.
    /// Berserkr is a Heretical Warrior with Rage special resource and 3 ability tiers.
    /// </summary>
    [Test]
    public void GetBySpecializationId_Berserkr_ReturnsHereticalWarriorWithRage()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new SpecializationProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var berserkr = provider.GetBySpecializationId(SpecializationId.Berserkr);

        // Assert
        berserkr.Should().NotBeNull();
        berserkr!.SpecializationId.Should().Be(SpecializationId.Berserkr);
        berserkr.DisplayName.Should().Be("Berserkr");
        berserkr.ParentArchetype.Should().Be(Archetype.Warrior);
        berserkr.PathType.Should().Be(SpecializationPathType.Heretical);
        berserkr.IsHeretical.Should().BeTrue();
        berserkr.HasSpecialResource.Should().BeTrue();
        berserkr.SpecialResource.DisplayName.Should().Be("Rage");
    }

    /// <summary>
    /// Verifies that GetBySpecializationId returns Skjaldmaer as a Coherent Warrior.
    /// </summary>
    [Test]
    public void GetBySpecializationId_Skjaldmaer_ReturnsCoherentWarrior()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new SpecializationProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var skjaldmaer = provider.GetBySpecializationId(SpecializationId.Skjaldmaer);

        // Assert
        skjaldmaer.Should().NotBeNull();
        skjaldmaer!.SpecializationId.Should().Be(SpecializationId.Skjaldmaer);
        skjaldmaer.ParentArchetype.Should().Be(Archetype.Warrior);
        skjaldmaer.PathType.Should().Be(SpecializationPathType.Coherent);
        skjaldmaer.IsHeretical.Should().BeFalse();
        skjaldmaer.HasSpecialResource.Should().BeTrue();
        skjaldmaer.SpecialResource.DisplayName.Should().Be("Block Charges");
    }

    /// <summary>
    /// Verifies that GetBySpecializationId returns Skald as a Coherent Adept without
    /// a special resource.
    /// </summary>
    [Test]
    public void GetBySpecializationId_Skald_ReturnsCoherentAdeptWithoutResource()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new SpecializationProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var skald = provider.GetBySpecializationId(SpecializationId.Skald);

        // Assert
        skald.Should().NotBeNull();
        skald!.SpecializationId.Should().Be(SpecializationId.Skald);
        skald.ParentArchetype.Should().Be(Archetype.Adept);
        skald.PathType.Should().Be(SpecializationPathType.Coherent);
        skald.HasSpecialResource.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════
    // GetByArchetype TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that Warrior archetype has exactly 6 specializations.
    /// </summary>
    [Test]
    public void GetByArchetype_Warrior_ReturnsSixSpecializations()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new SpecializationProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var warriors = provider.GetByArchetype(Archetype.Warrior);

        // Assert
        warriors.Should().HaveCount(6);
        warriors.Should().OnlyContain(s => s.ParentArchetype == Archetype.Warrior);
    }

    /// <summary>
    /// Verifies that Skirmisher archetype has exactly 4 specializations.
    /// </summary>
    [Test]
    public void GetByArchetype_Skirmisher_ReturnsFourSpecializations()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new SpecializationProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var skirmishers = provider.GetByArchetype(Archetype.Skirmisher);

        // Assert
        skirmishers.Should().HaveCount(4);
        skirmishers.Should().OnlyContain(s => s.ParentArchetype == Archetype.Skirmisher);
    }

    /// <summary>
    /// Verifies that Mystic archetype has exactly 2 specializations (both Heretical).
    /// </summary>
    [Test]
    public void GetByArchetype_Mystic_ReturnsTwoHereticalSpecializations()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new SpecializationProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var mystics = provider.GetByArchetype(Archetype.Mystic);

        // Assert
        mystics.Should().HaveCount(2);
        mystics.Should().OnlyContain(s => s.ParentArchetype == Archetype.Mystic);
        mystics.Should().OnlyContain(s => s.IsHeretical,
            "all Mystic specializations are Heretical");
    }

    /// <summary>
    /// Verifies that Adept archetype has exactly 5 specializations (all Coherent).
    /// </summary>
    [Test]
    public void GetByArchetype_Adept_ReturnsFiveCoherentSpecializations()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new SpecializationProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var adepts = provider.GetByArchetype(Archetype.Adept);

        // Assert
        adepts.Should().HaveCount(5);
        adepts.Should().OnlyContain(s => s.ParentArchetype == Archetype.Adept);
        adepts.Should().OnlyContain(s => !s.IsHeretical,
            "all Adept specializations are Coherent");
    }

    // ═══════════════════════════════════════════════════════════════
    // PATH TYPE FILTER TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetHereticalSpecializations returns exactly 5 specializations.
    /// </summary>
    [Test]
    public void GetHereticalSpecializations_ReturnsFiveSpecializations()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new SpecializationProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var heretical = provider.GetHereticalSpecializations();

        // Assert
        heretical.Should().HaveCount(5);
        heretical.Should().OnlyContain(s => s.IsHeretical);
    }

    /// <summary>
    /// Verifies the specific 5 Heretical specializations are correct.
    /// </summary>
    [Test]
    public void GetHereticalSpecializations_ContainsExpectedSpecializations()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new SpecializationProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var heretical = provider.GetHereticalSpecializations();
        var ids = heretical.Select(s => s.SpecializationId).ToList();

        // Assert — the 5 Heretical specializations
        ids.Should().Contain(SpecializationId.Berserkr);
        ids.Should().Contain(SpecializationId.GorgeMaw);
        ids.Should().Contain(SpecializationId.MyrkGengr);
        ids.Should().Contain(SpecializationId.Seidkona);
        ids.Should().Contain(SpecializationId.EchoCaller);
    }

    /// <summary>
    /// Verifies that GetCoherentSpecializations returns exactly 12 specializations.
    /// </summary>
    [Test]
    public void GetCoherentSpecializations_ReturnsTwelveSpecializations()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new SpecializationProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var coherent = provider.GetCoherentSpecializations();

        // Assert
        coherent.Should().HaveCount(12);
        coherent.Should().OnlyContain(s => !s.IsHeretical);
    }

    // ═══════════════════════════════════════════════════════════════
    // COUNT AND EXISTS TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that Count returns 17.
    /// </summary>
    [Test]
    public void Count_ReturnsSeventeen()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new SpecializationProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var count = provider.Count;

        // Assert
        count.Should().Be(17);
    }

    /// <summary>
    /// Verifies that Exists returns true for a valid specialization ID.
    /// </summary>
    [Test]
    public void Exists_ValidId_ReturnsTrue()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new SpecializationProvider(_mockLogger.Object, _testConfigPath);

        // Act & Assert
        provider.Exists(SpecializationId.Berserkr).Should().BeTrue();
        provider.Exists(SpecializationId.Skjaldmaer).Should().BeTrue();
        provider.Exists(SpecializationId.Seidkona).Should().BeTrue();
        provider.Exists(SpecializationId.Einbui).Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════════════════
    // GetWithSpecialResource TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetWithSpecialResource returns exactly 5 specializations.
    /// </summary>
    [Test]
    public void GetWithSpecialResource_ReturnsFiveSpecializations()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new SpecializationProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var withResource = provider.GetWithSpecialResource();

        // Assert
        withResource.Should().HaveCount(5);
        withResource.Should().OnlyContain(s => s.HasSpecialResource);
    }

    /// <summary>
    /// Verifies that the correct 5 specializations have special resources.
    /// </summary>
    [Test]
    public void GetWithSpecialResource_ContainsExpectedSpecializations()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new SpecializationProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var withResource = provider.GetWithSpecialResource();
        var ids = withResource.Select(s => s.SpecializationId).ToList();

        // Assert — the 5 specializations with special resources
        ids.Should().Contain(SpecializationId.Berserkr);       // Rage
        ids.Should().Contain(SpecializationId.Skjaldmaer);     // Block Charges
        ids.Should().Contain(SpecializationId.IronBane);       // Righteous Fervor
        ids.Should().Contain(SpecializationId.Seidkona);       // Aether Resonance
        ids.Should().Contain(SpecializationId.EchoCaller);     // Echoes
    }

    // ═══════════════════════════════════════════════════════════════
    // GetAbility TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetAbility returns the ability and parent specialization
    /// for a valid ability ID (rage-strike belongs to Berserkr Tier 1).
    /// </summary>
    [Test]
    public void GetAbility_ValidAbilityId_ReturnsAbilityAndSpecialization()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new SpecializationProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var result = provider.GetAbility("rage-strike");

        // Assert
        result.Should().NotBeNull();
        result!.Value.Specialization.SpecializationId.Should().Be(SpecializationId.Berserkr);
        result.Value.Ability.DisplayName.Should().Be("Rage Strike");
        result.Value.Ability.IsPassive.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that GetAbility returns null for a non-existent ability ID.
    /// </summary>
    [Test]
    public void GetAbility_InvalidAbilityId_ReturnsNull()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new SpecializationProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var result = provider.GetAbility("non-existent-ability");

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Verifies that GetAbility throws for null or whitespace ability ID.
    /// </summary>
    [Test]
    public void GetAbility_NullOrWhitespace_ThrowsArgumentException()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new SpecializationProvider(_mockLogger.Object, _testConfigPath);

        // Act & Assert
        var actNull = () => provider.GetAbility(null!);
        actNull.Should().Throw<ArgumentException>();

        var actEmpty = () => provider.GetAbility("");
        actEmpty.Should().Throw<ArgumentException>();

        var actWhitespace = () => provider.GetAbility("   ");
        actWhitespace.Should().Throw<ArgumentException>();
    }

    // ═══════════════════════════════════════════════════════════════
    // BERSERKR ABILITY TIERS TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that Berserkr has 3 ability tiers with 9 total abilities.
    /// </summary>
    [Test]
    public void GetBySpecializationId_Berserkr_HasThreeAbilityTiers()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new SpecializationProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var berserkr = provider.GetBySpecializationId(SpecializationId.Berserkr);

        // Assert
        berserkr.Should().NotBeNull();
        berserkr!.HasAbilityTiers.Should().BeTrue();
        berserkr.AbilityTiers.Should().HaveCount(3);
        berserkr.TotalAbilityCount.Should().Be(9);
    }

    /// <summary>
    /// Verifies Berserkr Tier 1 has correct properties (free, no prerequisites).
    /// </summary>
    [Test]
    public void GetBySpecializationId_Berserkr_Tier1IsFreeWithThreeAbilities()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new SpecializationProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var berserkr = provider.GetBySpecializationId(SpecializationId.Berserkr);
        var tier1 = berserkr!.GetTier(1);

        // Assert
        tier1.Should().NotBeNull();
        tier1!.Value.UnlockCost.Should().Be(0, "Tier 1 is free");
        tier1.Value.RequiresPreviousTier.Should().BeFalse("Tier 1 has no prerequisites");
        tier1.Value.AbilityCount.Should().Be(3);
    }

    // ═══════════════════════════════════════════════════════════════
    // ERROR HANDLING TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that accessing data with a missing config file throws
    /// SpecializationConfigurationException.
    /// </summary>
    [Test]
    public void GetAll_WithMissingFile_ThrowsSpecializationConfigurationException()
    {
        // Arrange
        var nonExistentPath = "/tmp/nonexistent/specializations.json";
        var provider = new SpecializationProvider(_mockLogger.Object, nonExistentPath);

        // Act
        var act = () => provider.GetAll();

        // Assert
        act.Should().Throw<SpecializationConfigurationException>()
            .WithMessage("*not found*");
    }

    // ═══════════════════════════════════════════════════════════════
    // CACHING TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that repeated calls return the same cached instances.
    /// </summary>
    [Test]
    public void GetBySpecializationId_ReturnsSameInstanceOnRepeatedCalls()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new SpecializationProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var first = provider.GetBySpecializationId(SpecializationId.Berserkr);
        var second = provider.GetBySpecializationId(SpecializationId.Berserkr);

        // Assert — same reference means it's cached
        first.Should().BeSameAs(second);
    }

    // ═══════════════════════════════════════════════════════════════
    // SPECIAL RESOURCE DETAILS TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that Berserkr's Rage resource has correct values (0-100, starts 0, decays 5).
    /// </summary>
    [Test]
    public void GetBySpecializationId_Berserkr_RageResourceHasCorrectValues()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new SpecializationProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var berserkr = provider.GetBySpecializationId(SpecializationId.Berserkr);
        var rage = berserkr!.SpecialResource;

        // Assert
        rage.HasResource.Should().BeTrue();
        rage.DisplayName.Should().Be("Rage");
        rage.MinValue.Should().Be(0);
        rage.MaxValue.Should().Be(100);
        rage.StartsAt.Should().Be(0);
        rage.DecayPerTurn.Should().Be(5);
    }

    /// <summary>
    /// Verifies that Skjaldmaer's Block Charges resource has correct values (0-3, starts 3, regens 1).
    /// </summary>
    [Test]
    public void GetBySpecializationId_Skjaldmaer_BlockChargesResourceHasCorrectValues()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new SpecializationProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var skjaldmaer = provider.GetBySpecializationId(SpecializationId.Skjaldmaer);
        var blockCharges = skjaldmaer!.SpecialResource;

        // Assert
        blockCharges.HasResource.Should().BeTrue();
        blockCharges.DisplayName.Should().Be("Block Charges");
        blockCharges.MinValue.Should().Be(0);
        blockCharges.MaxValue.Should().Be(3);
        blockCharges.StartsAt.Should().Be(3);
        blockCharges.RegenPerTurn.Should().Be(1);
    }

    // ═══════════════════════════════════════════════════════════════
    // ALL SPECIALIZATIONS HAVE CORRECT PARENT ARCHETYPE TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that every specialization returned by GetByArchetype has
    /// the correct parent archetype for all 4 archetypes.
    /// </summary>
    [Test]
    public void GetByArchetype_AllArchetypes_HaveCorrectParentArchetypes()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new SpecializationProvider(_mockLogger.Object, _testConfigPath);

        // Act & Assert for each archetype
        foreach (var archetype in Enum.GetValues<Archetype>())
        {
            var specs = provider.GetByArchetype(archetype);
            specs.Should().OnlyContain(
                s => s.ParentArchetype == archetype,
                $"all specializations for {archetype} should have {archetype} as parent");
        }
    }

    /// <summary>
    /// Verifies that the sum of all archetype specialization counts equals 17.
    /// </summary>
    [Test]
    public void GetByArchetype_TotalAcrossAllArchetypes_EqualsSeventeen()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new SpecializationProvider(_mockLogger.Object, _testConfigPath);

        // Act
        var total = Enum.GetValues<Archetype>()
            .Sum(a => provider.GetByArchetype(a).Count);

        // Assert
        total.Should().Be(17);
    }
}
