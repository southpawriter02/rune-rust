// ═══════════════════════════════════════════════════════════════════════════════
// ArchetypeArmorProficiencySetTests.cs
// Unit tests for the ArchetypeArmorProficiencySet entity.
// Version: 0.16.2c
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Unit tests for <see cref="ArchetypeArmorProficiencySet"/>.
/// </summary>
/// <remarks>
/// Tests cover:
/// <list type="bullet">
///   <item><description>Factory method validation and creation</description></item>
///   <item><description>Proficiency checking for various archetype-armor combinations</description></item>
///   <item><description>Galdr interference rules for caster archetypes</description></item>
///   <item><description>Penalty calculations and blocking behavior</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class ArchetypeArmorProficiencySetTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Test Data Helpers
    // ═══════════════════════════════════════════════════════════════════════════

    private static ArchetypeArmorProficiencySet CreateWarriorSet() =>
        ArchetypeArmorProficiencySet.Create(
            "warrior",
            "Warrior",
            new[] { ArmorCategory.Light, ArmorCategory.Medium, ArmorCategory.Heavy, ArmorCategory.Shields },
            new[] { ArmorCategory.Specialized });

    private static ArchetypeArmorProficiencySet CreateSkirmisherSet() =>
        ArchetypeArmorProficiencySet.Create(
            "skirmisher",
            "Skirmisher",
            new[] { ArmorCategory.Light, ArmorCategory.Medium, ArmorCategory.Shields },
            new[] { ArmorCategory.Heavy, ArmorCategory.Specialized },
            specialRestrictions: "Cannot effectively use Tower Shields");

    private static ArchetypeArmorProficiencySet CreateMysticSet() =>
        ArchetypeArmorProficiencySet.Create(
            "mystic",
            "Mystic",
            new[] { ArmorCategory.Light },
            new[] { ArmorCategory.Medium, ArmorCategory.Heavy, ArmorCategory.Shields, ArmorCategory.Specialized },
            GaldrInterference.MysticRules,
            "Galdr blocked in Heavy armor");

    private static ArchetypeArmorProficiencySet CreateAdeptSet() =>
        ArchetypeArmorProficiencySet.Create(
            "adept",
            "Adept",
            new[] { ArmorCategory.Light },
            new[] { ArmorCategory.Medium, ArmorCategory.Heavy, ArmorCategory.Shields, ArmorCategory.Specialized },
            GaldrInterference.AdeptRules,
            "-4 WITS penalty in Heavy armor");

    // ═══════════════════════════════════════════════════════════════════════════
    // Factory Method Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void Create_WithValidParameters_CreatesSet()
    {
        // Arrange & Act
        var set = CreateWarriorSet();

        // Assert
        set.Should().NotBeNull();
        set.Id.Should().NotBeEmpty();
        set.ArchetypeId.Should().Be("warrior");
        set.DisplayName.Should().Be("Warrior");
        set.ProficientCategories.Should().HaveCount(4);
        set.NonProficientCategories.Should().HaveCount(1);
        set.GaldrInterference.Should().BeNull();
        set.SpecialRestrictions.Should().BeNull();
    }

    [Test]
    public void Create_WithGaldrInterference_StoresRules()
    {
        // Arrange & Act
        var set = CreateMysticSet();

        // Assert
        set.GaldrInterference.Should().NotBeNull();
        set.GaldrInterference!.Value.HeavyArmorBlocked.Should().BeTrue();
        set.GaldrInterference.Value.MediumArmorPenalty.Should().Be(-2);
        set.SpecialRestrictions.Should().Be("Galdr blocked in Heavy armor");
    }

    [Test]
    public void Create_NormalizesArchetypeIdToLowercase()
    {
        // Arrange & Act
        var set = ArchetypeArmorProficiencySet.Create(
            "WARRIOR",
            "Warrior",
            new[] { ArmorCategory.Light },
            new[] { ArmorCategory.Heavy });

        // Assert
        set.ArchetypeId.Should().Be("warrior");
    }

    [Test]
    public void Create_WithNullArchetypeId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => ArchetypeArmorProficiencySet.Create(
            null!,
            "Warrior",
            new[] { ArmorCategory.Light },
            new[] { ArmorCategory.Heavy });

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Create_WithEmptyDisplayName_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => ArchetypeArmorProficiencySet.Create(
            "warrior",
            "",
            new[] { ArmorCategory.Light },
            new[] { ArmorCategory.Heavy });

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Create_WithNullProficientCategories_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => ArchetypeArmorProficiencySet.Create(
            "warrior",
            "Warrior",
            null!,
            new[] { ArmorCategory.Heavy });

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Proficiency Query Tests - Warrior
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void IsProficientWith_WarriorWithHeavy_ReturnsTrue()
    {
        // Arrange
        var set = CreateWarriorSet();

        // Act
        var result = set.IsProficientWith(ArmorCategory.Heavy);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public void IsProficientWith_WarriorWithSpecialized_ReturnsFalse()
    {
        // Arrange
        var set = CreateWarriorSet();

        // Act
        var result = set.IsProficientWith(ArmorCategory.Specialized);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void GetStartingProficiency_WarriorWithHeavy_ReturnsProficient()
    {
        // Arrange
        var set = CreateWarriorSet();

        // Act
        var result = set.GetStartingProficiency(ArmorCategory.Heavy);

        // Assert
        result.Should().Be(ArmorProficiencyLevel.Proficient);
    }

    [Test]
    public void GetStartingProficiency_WarriorWithSpecialized_ReturnsNonProficient()
    {
        // Arrange
        var set = CreateWarriorSet();

        // Act
        var result = set.GetStartingProficiency(ArmorCategory.Specialized);

        // Assert
        result.Should().Be(ArmorProficiencyLevel.NonProficient);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Proficiency Query Tests - Skirmisher
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void IsProficientWith_SkirmisherWithMedium_ReturnsTrue()
    {
        // Arrange
        var set = CreateSkirmisherSet();

        // Act
        var result = set.IsProficientWith(ArmorCategory.Medium);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public void IsProficientWith_SkirmisherWithHeavy_ReturnsFalse()
    {
        // Arrange
        var set = CreateSkirmisherSet();

        // Act
        var result = set.IsProficientWith(ArmorCategory.Heavy);

        // Assert
        result.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Galdr Interference Tests - Mystic
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void IsGaldrBlockedBy_MysticWithHeavy_ReturnsTrue()
    {
        // Arrange
        var set = CreateMysticSet();

        // Act
        var blocked = set.IsGaldrBlockedBy(ArmorCategory.Heavy);

        // Assert
        blocked.Should().BeTrue();
    }

    [Test]
    public void IsGaldrBlockedBy_MysticWithShields_ReturnsTrue()
    {
        // Arrange
        var set = CreateMysticSet();

        // Act
        var blocked = set.IsGaldrBlockedBy(ArmorCategory.Shields);

        // Assert
        blocked.Should().BeTrue();
    }

    [Test]
    public void IsGaldrBlockedBy_MysticWithMedium_ReturnsFalse()
    {
        // Arrange
        var set = CreateMysticSet();

        // Act
        var blocked = set.IsGaldrBlockedBy(ArmorCategory.Medium);

        // Assert
        blocked.Should().BeFalse("Medium armor causes penalty, not blocking");
    }

    [Test]
    public void GetGaldrPenalty_MysticWithMedium_ReturnsMinusTwo()
    {
        // Arrange
        var set = CreateMysticSet();

        // Act
        var penalty = set.GetGaldrPenalty(ArmorCategory.Medium);

        // Assert
        penalty.Should().Be(-2);
    }

    [Test]
    public void GetGaldrPenalty_MysticWithLight_ReturnsZero()
    {
        // Arrange
        var set = CreateMysticSet();

        // Act
        var penalty = set.GetGaldrPenalty(ArmorCategory.Light);

        // Assert
        penalty.Should().Be(0);
    }

    [Test]
    public void GetGaldrPenalty_MysticWithHeavy_ReturnsZero_BecauseBlocked()
    {
        // Arrange
        var set = CreateMysticSet();

        // Act
        var penalty = set.GetGaldrPenalty(ArmorCategory.Heavy);

        // Assert
        penalty.Should().Be(0, "Heavy armor blocks casting, so penalty is irrelevant");
    }

    [Test]
    public void CanCastGaldr_MysticWithMedium_ReturnsTrue()
    {
        // Arrange
        var set = CreateMysticSet();

        // Act
        var canCast = set.CanCastGaldr(ArmorCategory.Medium);

        // Assert
        canCast.Should().BeTrue("Can cast with penalty");
    }

    [Test]
    public void CanCastGaldr_MysticWithHeavy_ReturnsFalse()
    {
        // Arrange
        var set = CreateMysticSet();

        // Act
        var canCast = set.CanCastGaldr(ArmorCategory.Heavy);

        // Assert
        canCast.Should().BeFalse("Heavy armor blocks casting completely");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Galdr Interference Tests - Adept (WITS-based)
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void IsGaldrBlockedBy_AdeptWithHeavy_ReturnsFalse()
    {
        // Arrange
        var set = CreateAdeptSet();

        // Act
        var blocked = set.IsGaldrBlockedBy(ArmorCategory.Heavy);

        // Assert
        blocked.Should().BeFalse("Adepts have penalties in heavy armor, not blocking");
    }

    [Test]
    public void IsGaldrBlockedBy_AdeptWithShields_ReturnsTrue()
    {
        // Arrange
        var set = CreateAdeptSet();

        // Act
        var blocked = set.IsGaldrBlockedBy(ArmorCategory.Shields);

        // Assert
        blocked.Should().BeTrue("Shields block WITS abilities for Adepts");
    }

    [Test]
    public void GetGaldrPenalty_AdeptWithHeavy_ReturnsMinusFour()
    {
        // Arrange
        var set = CreateAdeptSet();

        // Act
        var penalty = set.GetGaldrPenalty(ArmorCategory.Heavy);

        // Assert
        penalty.Should().Be(-4);
    }

    [Test]
    public void GetGaldrPenalty_AdeptWithMedium_ReturnsMinusTwo()
    {
        // Arrange
        var set = CreateAdeptSet();

        // Act
        var penalty = set.GetGaldrPenalty(ArmorCategory.Medium);

        // Assert
        penalty.Should().Be(-2);
    }

    [Test]
    public void CanCastGaldr_AdeptWithHeavy_ReturnsTrue()
    {
        // Arrange
        var set = CreateAdeptSet();

        // Act
        var canCast = set.CanCastGaldr(ArmorCategory.Heavy);

        // Assert
        canCast.Should().BeTrue("Adepts can use WITS abilities in heavy armor with -4 penalty");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Non-Caster Galdr Tests (Warrior/Skirmisher)
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void IsGaldrBlockedBy_WarriorWithHeavy_ReturnsFalse()
    {
        // Arrange
        var set = CreateWarriorSet();

        // Act
        var blocked = set.IsGaldrBlockedBy(ArmorCategory.Heavy);

        // Assert
        blocked.Should().BeFalse("Warriors have no Galdr to block");
    }

    [Test]
    public void GetGaldrPenalty_WarriorWithAnyArmor_ReturnsZero()
    {
        // Arrange
        var set = CreateWarriorSet();

        // Act & Assert
        set.GetGaldrPenalty(ArmorCategory.Light).Should().Be(0);
        set.GetGaldrPenalty(ArmorCategory.Medium).Should().Be(0);
        set.GetGaldrPenalty(ArmorCategory.Heavy).Should().Be(0);
        set.GetGaldrPenalty(ArmorCategory.Shields).Should().Be(0);
    }

    [Test]
    public void HasGaldrInterference_Warrior_ReturnsFalse()
    {
        // Arrange
        var set = CreateWarriorSet();

        // Act
        var hasInterference = set.HasGaldrInterference;

        // Assert
        hasInterference.Should().BeFalse();
    }

    [Test]
    public void HasGaldrInterference_Mystic_ReturnsTrue()
    {
        // Arrange
        var set = CreateMysticSet();

        // Act
        var hasInterference = set.HasGaldrInterference;

        // Assert
        hasInterference.Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Formatting Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void FormatProficientCategories_Warrior_ReturnsAllCategories()
    {
        // Arrange
        var set = CreateWarriorSet();

        // Act
        var formatted = set.FormatProficientCategories();

        // Assert
        formatted.Should().Contain("Light");
        formatted.Should().Contain("Medium");
        formatted.Should().Contain("Heavy");
        formatted.Should().Contain("Shields");
    }

    [Test]
    public void FormatNonProficientCategories_Mystic_ReturnsMultipleCategories()
    {
        // Arrange
        var set = CreateMysticSet();

        // Act
        var formatted = set.FormatNonProficientCategories();

        // Assert
        formatted.Should().Contain("Medium");
        formatted.Should().Contain("Heavy");
        formatted.Should().Contain("Shields");
        formatted.Should().Contain("Specialized");
    }

    [Test]
    public void ToString_Mystic_IncludesArchetypeAndGaldrStatus()
    {
        // Arrange
        var set = CreateMysticSet();

        // Act
        var result = set.ToString();

        // Assert
        result.Should().Contain("mystic");
        result.Should().Contain("HasGaldr: True");
    }
}
