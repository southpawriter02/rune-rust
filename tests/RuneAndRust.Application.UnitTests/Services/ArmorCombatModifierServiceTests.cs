// =============================================================================
// ArmorCombatModifierServiceTests.cs
// =============================================================================
// v0.16.2f - Combat Integration
// =============================================================================
// Unit tests for ArmorCombatModifierService verifying combat penalty application,
// Galdr interference rules, and modifier calculations.
// =============================================================================

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for <see cref="ArmorCombatModifierService"/>.
/// </summary>
/// <remarks>
/// Tests verify:
/// <list type="bullet">
/// <item><description>Galdr blocking for casters in restrictive armor</description></item>
/// <item><description>Galdr penalty application</description></item>
/// <item><description>Attack modifier from proficiency</description></item>
/// <item><description>Defense modifier for Master proficiency</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class ArmorCombatModifierServiceTests
{
    // =========================================================================
    // Fields
    // =========================================================================

    private Mock<IArmorPenaltyCalculator> _mockPenaltyCalculator = null!;
    private Mock<IArchetypeArmorProficiencyProvider> _mockArchetypeProvider = null!;
    private Mock<ILogger<ArmorCombatModifierService>> _mockLogger = null!;
    private ArmorCombatModifierService _service = null!;

    // =========================================================================
    // Setup
    // =========================================================================

    [SetUp]
    public void SetUp()
    {
        _mockPenaltyCalculator = new Mock<IArmorPenaltyCalculator>();
        _mockArchetypeProvider = new Mock<IArchetypeArmorProficiencyProvider>();
        _mockLogger = new Mock<ILogger<ArmorCombatModifierService>>();

        _service = new ArmorCombatModifierService(
            _mockPenaltyCalculator.Object,
            _mockArchetypeProvider.Object,
            _mockLogger.Object);
    }

    // =========================================================================
    // Constructor Tests
    // =========================================================================

    [Test]
    public void Constructor_WithNullPenaltyCalculator_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => new ArmorCombatModifierService(
            null!,
            _mockArchetypeProvider.Object,
            _mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("penaltyCalculator");
    }

    [Test]
    public void Constructor_WithNullArchetypeProvider_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => new ArmorCombatModifierService(
            _mockPenaltyCalculator.Object,
            null!,
            _mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("archetypeProvider");
    }

    // =========================================================================
    // GetCombatState Tests
    // =========================================================================

    [Test]
    public void GetCombatState_MysticInHeavyArmor_BlocksGaldr()
    {
        // Arrange
        const string archetypeId = "mystic";
        const ArmorCategory armorCategory = ArmorCategory.Heavy;

        _mockArchetypeProvider
            .Setup(p => p.GetStartingProficiency(archetypeId, armorCategory))
            .Returns(ArmorProficiencyLevel.NonProficient);

        _mockArchetypeProvider
            .Setup(p => p.IsGaldrBlocked(archetypeId, armorCategory))
            .Returns(true);

        _mockArchetypeProvider
            .Setup(p => p.GetGaldrPenalty(archetypeId, armorCategory))
            .Returns(0); // Irrelevant when blocked

        _mockPenaltyCalculator
            .Setup(c => c.CalculatePenalties(armorCategory, ArmorProficiencyLevel.NonProficient))
            .Returns(CreateNonProficientHeavyPenalties());

        // Act
        var result = _service.GetCombatState(archetypeId, armorCategory);

        // Assert
        result.GaldrBlocked.Should().BeTrue();
        result.CanCastGaldr.Should().BeFalse();
        result.GaldrPenalty.Should().Be(0); // Penalty cleared when blocked
        result.DisplayWarnings.Should().Contain(w => w.Contains("BLOCKED"));
    }

    [Test]
    public void GetGaldrPenalty_AdeptInMediumArmor_ReturnsMinusTwo()
    {
        // Arrange
        const string archetypeId = "adept";
        const ArmorCategory armorCategory = ArmorCategory.Medium;
        const int expectedPenalty = -2;

        _mockArchetypeProvider
            .Setup(p => p.IsGaldrBlocked(archetypeId, armorCategory))
            .Returns(false);

        _mockArchetypeProvider
            .Setup(p => p.GetGaldrPenalty(archetypeId, armorCategory))
            .Returns(expectedPenalty);

        // Act
        var result = _service.GetGaldrPenalty(archetypeId, armorCategory);

        // Assert
        result.Should().Be(expectedPenalty);
    }

    [Test]
    public void GetAttackModifier_NonProficientInHeavy_ReturnsNegativeTwo()
    {
        // Arrange
        const ArmorCategory armorCategory = ArmorCategory.Heavy;
        const ArmorProficiencyLevel proficiency = ArmorProficiencyLevel.NonProficient;
        const int expectedAttackModifier = -2;

        _mockPenaltyCalculator
            .Setup(c => c.CalculatePenalties(armorCategory, proficiency))
            .Returns(EffectiveArmorPenalties.Create(
                originalCategory: armorCategory,
                basePenalties: ArmorPenalties.None,
                effectivePenalties: ArmorPenalties.None,
                proficiencyLevel: proficiency,
                attackModifier: expectedAttackModifier,
                defenseModifier: 0,
                originalTier: 3,
                effectiveTier: 3,
                wasMultiplied: true,
                wasTierReduced: false));

        // Act
        var result = _service.GetAttackModifier(armorCategory, proficiency);

        // Assert
        result.Should().Be(expectedAttackModifier);
    }

    [Test]
    public void GetDefenseModifier_MasterInHeavy_ReturnsPlusOne()
    {
        // Arrange
        const ArmorCategory armorCategory = ArmorCategory.Heavy;
        const ArmorProficiencyLevel proficiency = ArmorProficiencyLevel.Master;
        const int expectedDefenseModifier = 1;

        _mockPenaltyCalculator
            .Setup(c => c.CalculatePenalties(armorCategory, proficiency))
            .Returns(EffectiveArmorPenalties.Create(
                originalCategory: armorCategory,
                basePenalties: ArmorPenalties.None,
                effectivePenalties: ArmorPenalties.None,
                proficiencyLevel: proficiency,
                attackModifier: 0,
                defenseModifier: expectedDefenseModifier,
                originalTier: 3,
                effectiveTier: 1,
                wasMultiplied: false,
                wasTierReduced: true));

        // Act
        var result = _service.GetDefenseModifier(armorCategory, proficiency);

        // Assert
        result.Should().Be(expectedDefenseModifier);
    }

    // =========================================================================
    // CanCastGaldr Tests
    // =========================================================================

    [Test]
    public void CanCastGaldr_WarriorInAnyArmor_ReturnsTrue()
    {
        // Arrange - Warriors never have Galdr blocked
        const string archetypeId = "warrior";
        const ArmorCategory armorCategory = ArmorCategory.Heavy;

        _mockArchetypeProvider
            .Setup(p => p.IsGaldrBlocked(archetypeId, armorCategory))
            .Returns(false);

        // Act
        var result = _service.CanCastGaldr(archetypeId, armorCategory);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public void CanCastGaldr_MysticWithShield_ReturnsFalse()
    {
        // Arrange - Mystics have Galdr blocked by shields
        const string archetypeId = "mystic";
        const ArmorCategory armorCategory = ArmorCategory.Shields;

        _mockArchetypeProvider
            .Setup(p => p.IsGaldrBlocked(archetypeId, armorCategory))
            .Returns(true);

        // Act
        var result = _service.CanCastGaldr(archetypeId, armorCategory);

        // Assert
        result.Should().BeFalse();
    }

    // =========================================================================
    // Argument Validation Tests
    // =========================================================================

    [Test]
    public void GetCombatState_WithNullArchetypeId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => _service.GetCombatState(null!, ArmorCategory.Light);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void GetCombatState_WithEmptyArchetypeId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => _service.GetCombatState("", ArmorCategory.Light);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    // =========================================================================
    // Helper Methods
    // =========================================================================

    /// <summary>
    /// Creates typical non-proficient heavy armor penalties for testing.
    /// </summary>
    private static EffectiveArmorPenalties CreateNonProficientHeavyPenalties()
    {
        var heavyPenalties = ArmorPenalties.Create(
            agilityDicePenalty: -4,
            staminaCostModifier: 10,
            movementPenalty: -20,
            hasStealthDisadvantage: true);

        return EffectiveArmorPenalties.Create(
            originalCategory: ArmorCategory.Heavy,
            basePenalties: heavyPenalties,
            effectivePenalties: heavyPenalties,
            proficiencyLevel: ArmorProficiencyLevel.NonProficient,
            attackModifier: -2,
            defenseModifier: 0,
            originalTier: 3,
            effectiveTier: 3,
            wasMultiplied: true,
            wasTierReduced: false);
    }
}
