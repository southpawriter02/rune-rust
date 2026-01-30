// ═══════════════════════════════════════════════════════════════════════════════
// ArchetypeApplicationServiceTests.cs
// Unit tests for ArchetypeApplicationService (v0.17.3f).
// Tests cover constructor validation, archetype application, validation,
// and preview generation for all archetype scenarios.
// Version: 0.17.3f
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

#pragma warning disable NUnit2045 // Use Assert.Multiple

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for <see cref="ArchetypeApplicationService"/> (v0.17.3f).
/// </summary>
/// <remarks>
/// <para>Tests cover:</para>
/// <list type="bullet">
///   <item><description>Constructor null validation for IArchetypeProvider and ILogger dependencies</description></item>
///   <item><description>Successful archetype application with resource bonuses and abilities</description></item>
///   <item><description>Failure result when character is null</description></item>
///   <item><description>Failure result when character already has an archetype (permanent choice)</description></item>
///   <item><description>Failure result when archetype definition is not found</description></item>
///   <item><description>Mystic-specific Aether Pool bonus verification</description></item>
///   <item><description>Skirmisher-specific Movement bonus verification</description></item>
///   <item><description>Validation via CanApplyArchetype for valid and invalid scenarios</description></item>
///   <item><description>Preview generation via GetApplicationPreview</description></item>
///   <item><description>Archetype identifier set on character after successful application</description></item>
///   <item><description>Abilities granted to character after successful application</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class ArchetypeApplicationServiceTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // TEST FIXTURES
    // ═══════════════════════════════════════════════════════════════════════════

    private Mock<IArchetypeProvider> _mockProvider = null!;
    private Mock<ILogger<ArchetypeApplicationService>> _mockLogger = null!;
    private ArchetypeApplicationService _service = null!;

    /// <summary>
    /// Sets up test dependencies before each test. Configures mock provider
    /// with default Warrior archetype data and creates the service instance.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        _mockProvider = new Mock<IArchetypeProvider>();
        _mockLogger = new Mock<ILogger<ArchetypeApplicationService>>();

        // Set up default provider responses for all archetypes
        SetupWarriorProvider();
        SetupSkirmisherProvider();
        SetupMysticProvider();
        SetupAdeptProvider();

        _service = new ArchetypeApplicationService(
            _mockProvider.Object,
            _mockLogger.Object);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that the constructor throws when the archetype provider is null.
    /// </summary>
    [Test]
    public void Constructor_NullProvider_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => new ArchetypeApplicationService(
            null!,
            _mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("archetypeProvider");
    }

    /// <summary>
    /// Verifies that the constructor throws when the logger is null.
    /// </summary>
    [Test]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => new ArchetypeApplicationService(
            _mockProvider.Object,
            null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ApplyArchetype TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that applying Warrior archetype to a valid character succeeds,
    /// returning the correct archetype, resource bonuses, and abilities.
    /// </summary>
    [Test]
    public void ApplyArchetype_Warrior_AppliesSuccessfully()
    {
        // Arrange
        var player = new Player("TestHero");

        // Act
        var result = _service.ApplyArchetype(player, Archetype.Warrior);

        // Assert - Result is successful
        result.Success.Should().BeTrue();
        result.AppliedArchetype.Should().Be(Archetype.Warrior);
        result.FailureReason.Should().BeNull();

        // Assert - Resource bonuses match Warrior profile
        result.ResourceBonusesApplied.Should().NotBeNull();
        result.ResourceBonusesApplied!.Value.MaxHpBonus.Should().Be(49);
        result.ResourceBonusesApplied!.Value.MaxStaminaBonus.Should().Be(5);
        result.ResourceBonusesApplied!.Value.MaxAetherPoolBonus.Should().Be(0);
        result.ResourceBonusesApplied!.Value.MovementBonus.Should().Be(0);

        // Assert - 3 abilities granted
        result.AbilitiesGranted.Should().HaveCount(3);

        // Assert - Specializations available
        result.AvailableSpecializations.Should().NotBeNull();
        result.AvailableSpecializations!.Value.Count.Should().Be(6);
    }

    /// <summary>
    /// Verifies that the archetype identifier is set on the character after
    /// successful application.
    /// </summary>
    [Test]
    public void ApplyArchetype_SetsArchetypeIdOnCharacter()
    {
        // Arrange
        var player = new Player("TestHero");

        // Act
        _service.ApplyArchetype(player, Archetype.Warrior);

        // Assert
        player.HasClass.Should().BeTrue();
        player.ArchetypeId.Should().Be("warrior");
    }

    /// <summary>
    /// Verifies that starting abilities are added to the character's ability
    /// collection after successful application.
    /// </summary>
    [Test]
    public void ApplyArchetype_GrantsAbilitiesToCharacter()
    {
        // Arrange
        var player = new Player("TestHero");

        // Act
        _service.ApplyArchetype(player, Archetype.Warrior);

        // Assert - All 3 abilities should be present on the player
        player.HasAbility("power-strike").Should().BeTrue();
        player.HasAbility("defensive-stance").Should().BeTrue();
        player.HasAbility("iron-will").Should().BeTrue();
    }

    /// <summary>
    /// Verifies that applying an archetype to a null character returns a failure result.
    /// </summary>
    [Test]
    public void ApplyArchetype_NullCharacter_ReturnsFailed()
    {
        // Act
        var result = _service.ApplyArchetype(null!, Archetype.Warrior);

        // Assert
        result.Success.Should().BeFalse();
        result.FailureReason.Should().Contain("character is required");
        result.AbilitiesGranted.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies that applying an archetype to a character that already has one
    /// returns a failure result with a permanence message.
    /// </summary>
    [Test]
    public void ApplyArchetype_CharacterHasArchetype_ReturnsFailed()
    {
        // Arrange
        var player = new Player("TestHero");
        player.SetClass("skirmisher", "skirmisher");

        // Act
        var result = _service.ApplyArchetype(player, Archetype.Warrior);

        // Assert
        result.Success.Should().BeFalse();
        result.FailureReason.Should().Contain("already has archetype");
        result.FailureReason.Should().Contain("permanent choice");
    }

    /// <summary>
    /// Verifies that applying an invalid (undefined) archetype enum value
    /// returns a failure result.
    /// </summary>
    [Test]
    public void ApplyArchetype_InvalidArchetypeEnumValue_ReturnsFailed()
    {
        // Arrange
        var player = new Player("TestHero");
        var invalidArchetype = (Archetype)99;

        // Act
        var result = _service.ApplyArchetype(player, invalidArchetype);

        // Assert
        result.Success.Should().BeFalse();
        result.FailureReason.Should().Contain("Invalid archetype");
    }

    /// <summary>
    /// Verifies that applying Mystic archetype correctly returns the Aether Pool bonus.
    /// </summary>
    [Test]
    public void ApplyArchetype_Mystic_ReturnsAetherPoolBonus()
    {
        // Arrange
        var player = new Player("TestHero");

        // Act
        var result = _service.ApplyArchetype(player, Archetype.Mystic);

        // Assert
        result.Success.Should().BeTrue();
        result.AppliedArchetype.Should().Be(Archetype.Mystic);
        result.ResourceBonusesApplied!.Value.MaxAetherPoolBonus.Should().Be(20);
        result.ResourceBonusesApplied!.Value.MaxHpBonus.Should().Be(20);
    }

    /// <summary>
    /// Verifies that applying Skirmisher archetype correctly returns the Movement bonus.
    /// </summary>
    [Test]
    public void ApplyArchetype_Skirmisher_ReturnsMovementBonus()
    {
        // Arrange
        var player = new Player("TestHero");

        // Act
        var result = _service.ApplyArchetype(player, Archetype.Skirmisher);

        // Assert
        result.Success.Should().BeTrue();
        result.AppliedArchetype.Should().Be(Archetype.Skirmisher);
        result.ResourceBonusesApplied!.Value.MovementBonus.Should().Be(1);
    }

    /// <summary>
    /// Verifies that applying Adept archetype returns the special consumable
    /// effectiveness bonus in the result.
    /// </summary>
    [Test]
    public void ApplyArchetype_Adept_ReturnsSpecialBonus()
    {
        // Arrange
        var player = new Player("TestHero");

        // Act
        var result = _service.ApplyArchetype(player, Archetype.Adept);

        // Assert
        result.Success.Should().BeTrue();
        result.ResourceBonusesApplied!.Value.HasSpecialBonus.Should().BeTrue();
        result.ResourceBonusesApplied!.Value.SpecialBonus!.Value.BonusType
            .Should().Be("ConsumableEffectiveness");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CanApplyArchetype TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that CanApplyArchetype returns valid for a character without
    /// an existing archetype.
    /// </summary>
    [Test]
    public void CanApplyArchetype_ValidCharacter_ReturnsValid()
    {
        // Arrange
        var player = new Player("TestHero");

        // Act
        var validation = _service.CanApplyArchetype(player, Archetype.Warrior);

        // Assert
        validation.IsValid.Should().BeTrue();
        validation.Issues.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies that CanApplyArchetype returns invalid for a null character.
    /// </summary>
    [Test]
    public void CanApplyArchetype_NullCharacter_ReturnsInvalid()
    {
        // Act
        var validation = _service.CanApplyArchetype(null, Archetype.Warrior);

        // Assert
        validation.IsValid.Should().BeFalse();
        validation.Issues.Should().ContainSingle()
            .Which.Should().Contain("character is required");
    }

    /// <summary>
    /// Verifies that CanApplyArchetype returns invalid for a character that
    /// already has an archetype (permanent choice).
    /// </summary>
    [Test]
    public void CanApplyArchetype_ExistingArchetype_ReturnsInvalid()
    {
        // Arrange
        var player = new Player("TestHero");
        player.SetClass("mystic", "mystic");

        // Act
        var validation = _service.CanApplyArchetype(player, Archetype.Warrior);

        // Assert
        validation.IsValid.Should().BeFalse();
        validation.Issues.Should().Contain(i => i.Contains("already has archetype"));
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // GetApplicationPreview TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetApplicationPreview returns complete preview data for a
    /// valid archetype, including display name, resource bonuses, abilities, and
    /// specializations.
    /// </summary>
    [Test]
    public void GetApplicationPreview_Warrior_ReturnsCompletePreview()
    {
        // Act
        var preview = _service.GetApplicationPreview(Archetype.Warrior);

        // Assert
        preview.Archetype.Should().Be(Archetype.Warrior);
        preview.DisplayName.Should().Be("Warrior");
        preview.ResourceBonuses.MaxHpBonus.Should().Be(49);
        preview.StartingAbilities.Should().HaveCount(3);
        preview.AvailableSpecializations.Count.Should().Be(6);
    }

    /// <summary>
    /// Verifies that GetApplicationPreview for Mystic returns Aether Pool
    /// bonuses and 2 specializations.
    /// </summary>
    [Test]
    public void GetApplicationPreview_Mystic_ReturnsAetherPoolBonusAndTwoSpecs()
    {
        // Act
        var preview = _service.GetApplicationPreview(Archetype.Mystic);

        // Assert
        preview.Archetype.Should().Be(Archetype.Mystic);
        preview.ResourceBonuses.MaxAetherPoolBonus.Should().Be(20);
        preview.AvailableSpecializations.Count.Should().Be(2);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // TEST HELPERS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Configures the mock provider with Warrior archetype data.
    /// Warrior: HP+49, Stamina+5, 3 abilities, 6 specializations.
    /// </summary>
    private void SetupWarriorProvider()
    {
        _mockProvider.Setup(p => p.GetArchetype(Archetype.Warrior))
            .Returns(ArchetypeDefinition.Create(
                Archetype.Warrior,
                "Warrior",
                "The Unyielding Bulwark",
                "Warriors are the frontline fighters.",
                "You are the shield between the innocent and the horror.",
                "Tank / Melee DPS",
                ResourceType.Stamina,
                "Stand in the front, absorb damage, protect allies"));

        _mockProvider.Setup(p => p.GetResourceBonuses(Archetype.Warrior))
            .Returns(ArchetypeResourceBonuses.Warrior);

        _mockProvider.Setup(p => p.GetStartingAbilities(Archetype.Warrior))
            .Returns(new List<ArchetypeAbilityGrant>
            {
                ArchetypeAbilityGrant.CreateActive(
                    "power-strike", "Power Strike", "Basic melee attack."),
                ArchetypeAbilityGrant.CreateStance(
                    "defensive-stance", "Defensive Stance", "+2 Defense, -1 Attack."),
                ArchetypeAbilityGrant.CreatePassive(
                    "iron-will", "Iron Will", "+10% HP regeneration while resting.")
            });

        _mockProvider.Setup(p => p.GetSpecializationMapping(Archetype.Warrior))
            .Returns(ArchetypeSpecializationMapping.Warrior);
    }

    /// <summary>
    /// Configures the mock provider with Skirmisher archetype data.
    /// Skirmisher: HP+30, Stamina+5, Movement+1, 3 abilities, 4 specializations.
    /// </summary>
    private void SetupSkirmisherProvider()
    {
        _mockProvider.Setup(p => p.GetArchetype(Archetype.Skirmisher))
            .Returns(ArchetypeDefinition.Create(
                Archetype.Skirmisher,
                "Skirmisher",
                "Swift as Shadow",
                "Skirmishers excel at mobility and precision strikes.",
                "Speed is survival. Strike fast, strike true.",
                "Mobile DPS",
                ResourceType.Stamina,
                "High mobility, hit-and-run tactics, evasion"));

        _mockProvider.Setup(p => p.GetResourceBonuses(Archetype.Skirmisher))
            .Returns(ArchetypeResourceBonuses.Skirmisher);

        _mockProvider.Setup(p => p.GetStartingAbilities(Archetype.Skirmisher))
            .Returns(new List<ArchetypeAbilityGrant>
            {
                ArchetypeAbilityGrant.CreateActive(
                    "quick-strike", "Quick Strike", "Fast melee attack."),
                ArchetypeAbilityGrant.CreateActive(
                    "evasive-maneuvers", "Evasive Maneuvers", "+2 Evasion."),
                ArchetypeAbilityGrant.CreatePassive(
                    "opportunist", "Opportunist", "+1 Movement speed.")
            });

        _mockProvider.Setup(p => p.GetSpecializationMapping(Archetype.Skirmisher))
            .Returns(ArchetypeSpecializationMapping.Skirmisher);
    }

    /// <summary>
    /// Configures the mock provider with Mystic archetype data.
    /// Mystic: HP+20, AP+20, 3 abilities, 2 specializations.
    /// </summary>
    private void SetupMysticProvider()
    {
        _mockProvider.Setup(p => p.GetArchetype(Archetype.Mystic))
            .Returns(ArchetypeDefinition.Create(
                Archetype.Mystic,
                "Mystic",
                "Wielder of Tainted Aether",
                "Mystics channel the corrupted Aether.",
                "The Aether flows through you like a river of broken logic.",
                "Caster / Control",
                ResourceType.AetherPool,
                "Ranged magical damage, crowd control, Aether management"));

        _mockProvider.Setup(p => p.GetResourceBonuses(Archetype.Mystic))
            .Returns(ArchetypeResourceBonuses.Mystic);

        _mockProvider.Setup(p => p.GetStartingAbilities(Archetype.Mystic))
            .Returns(new List<ArchetypeAbilityGrant>
            {
                ArchetypeAbilityGrant.CreateActive(
                    "aether-bolt", "Aether Bolt", "Ranged Aether attack."),
                ArchetypeAbilityGrant.CreateActive(
                    "aether-shield", "Aether Shield", "Restore 10 AP."),
                ArchetypeAbilityGrant.CreatePassive(
                    "aether-sense", "Aether Sense", "+10% AP regeneration.")
            });

        _mockProvider.Setup(p => p.GetSpecializationMapping(Archetype.Mystic))
            .Returns(ArchetypeSpecializationMapping.Mystic);
    }

    /// <summary>
    /// Configures the mock provider with Adept archetype data.
    /// Adept: HP+30, +20% Consumable Effectiveness, 3 abilities, 5 specializations.
    /// </summary>
    private void SetupAdeptProvider()
    {
        _mockProvider.Setup(p => p.GetArchetype(Archetype.Adept))
            .Returns(ArchetypeDefinition.Create(
                Archetype.Adept,
                "Adept",
                "Master of Mundane Arts",
                "Adepts excel at support, utility, and making the most of limited resources.",
                "While others rely on strength or magic, you rely on knowledge.",
                "Support / Utility",
                ResourceType.Stamina,
                "Support allies, maximize resources, utility skills"));

        _mockProvider.Setup(p => p.GetResourceBonuses(Archetype.Adept))
            .Returns(ArchetypeResourceBonuses.Adept);

        _mockProvider.Setup(p => p.GetStartingAbilities(Archetype.Adept))
            .Returns(new List<ArchetypeAbilityGrant>
            {
                ArchetypeAbilityGrant.CreateActive(
                    "precise-strike", "Precise Strike", "Targeted melee attack."),
                ArchetypeAbilityGrant.CreateActive(
                    "assess-weakness", "Assess Weakness", "Analyze enemy."),
                ArchetypeAbilityGrant.CreatePassive(
                    "resourceful", "Resourceful", "+20% consumable effectiveness.")
            });

        _mockProvider.Setup(p => p.GetSpecializationMapping(Archetype.Adept))
            .Returns(ArchetypeSpecializationMapping.Adept);
    }
}
