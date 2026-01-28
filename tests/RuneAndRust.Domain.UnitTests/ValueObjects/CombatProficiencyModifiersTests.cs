using FluentAssertions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="CombatProficiencyModifiers"/> value object.
/// </summary>
[TestFixture]
public class CombatProficiencyModifiersTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Static Factory Property Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that NonProficient static property returns correct default values.
    /// </summary>
    [Test]
    public void NonProficient_ReturnsCorrectDefaults()
    {
        // Act
        var modifiers = CombatProficiencyModifiers.NonProficient;

        // Assert
        modifiers.ProficiencyLevel.Should().Be(WeaponProficiencyLevel.NonProficient);
        modifiers.AttackModifier.Should().Be(-3);
        modifiers.DamageModifier.Should().Be(-2);
        modifiers.CanUseSpecialProperties.Should().BeFalse();
        modifiers.UnlockedTechniqueLevel.Should().Be(TechniqueAccess.None);
    }

    /// <summary>
    /// Verifies that Proficient static property returns correct default values.
    /// </summary>
    [Test]
    public void Proficient_ReturnsCorrectDefaults()
    {
        // Act
        var modifiers = CombatProficiencyModifiers.Proficient;

        // Assert
        modifiers.ProficiencyLevel.Should().Be(WeaponProficiencyLevel.Proficient);
        modifiers.AttackModifier.Should().Be(0);
        modifiers.DamageModifier.Should().Be(0);
        modifiers.CanUseSpecialProperties.Should().BeTrue();
        modifiers.UnlockedTechniqueLevel.Should().Be(TechniqueAccess.Basic);
    }

    /// <summary>
    /// Verifies that Expert static property returns correct default values.
    /// </summary>
    [Test]
    public void Expert_ReturnsCorrectDefaults()
    {
        // Act
        var modifiers = CombatProficiencyModifiers.Expert;

        // Assert
        modifiers.ProficiencyLevel.Should().Be(WeaponProficiencyLevel.Expert);
        modifiers.AttackModifier.Should().Be(1);
        modifiers.DamageModifier.Should().Be(0);
        modifiers.CanUseSpecialProperties.Should().BeTrue();
        modifiers.UnlockedTechniqueLevel.Should().Be(TechniqueAccess.Advanced);
    }

    /// <summary>
    /// Verifies that Master static property returns correct default values.
    /// </summary>
    [Test]
    public void Master_ReturnsCorrectDefaults()
    {
        // Act
        var modifiers = CombatProficiencyModifiers.Master;

        // Assert
        modifiers.ProficiencyLevel.Should().Be(WeaponProficiencyLevel.Master);
        modifiers.AttackModifier.Should().Be(2);
        modifiers.DamageModifier.Should().Be(1);
        modifiers.CanUseSpecialProperties.Should().BeTrue();
        modifiers.UnlockedTechniqueLevel.Should().Be(TechniqueAccess.Signature);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Convenience Property Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies IsNonProficient returns true only for NonProficient level.
    /// </summary>
    [Test]
    public void IsNonProficient_ForNonProficient_ReturnsTrue()
    {
        // Arrange
        var modifiers = CombatProficiencyModifiers.NonProficient;

        // Assert
        modifiers.IsNonProficient.Should().BeTrue();
    }

    /// <summary>
    /// Verifies IsNonProficient returns false for other levels.
    /// </summary>
    [Test]
    public void IsNonProficient_ForOtherLevels_ReturnsFalse()
    {
        // Assert
        CombatProficiencyModifiers.Proficient.IsNonProficient.Should().BeFalse();
        CombatProficiencyModifiers.Expert.IsNonProficient.Should().BeFalse();
        CombatProficiencyModifiers.Master.IsNonProficient.Should().BeFalse();
    }

    /// <summary>
    /// Verifies HasPenalty returns true when modifiers are negative.
    /// </summary>
    [Test]
    public void HasPenalty_ForNonProficient_ReturnsTrue()
    {
        // Arrange
        var modifiers = CombatProficiencyModifiers.NonProficient;

        // Assert
        modifiers.HasPenalty.Should().BeTrue();
    }

    /// <summary>
    /// Verifies HasPenalty returns false when modifiers are non-negative.
    /// </summary>
    [Test]
    public void HasPenalty_ForProficient_ReturnsFalse()
    {
        // Arrange
        var modifiers = CombatProficiencyModifiers.Proficient;

        // Assert
        modifiers.HasPenalty.Should().BeFalse();
    }

    /// <summary>
    /// Verifies HasBonus returns true when modifiers are positive.
    /// </summary>
    [Test]
    public void HasBonus_ForMaster_ReturnsTrue()
    {
        // Arrange
        var modifiers = CombatProficiencyModifiers.Master;

        // Assert
        modifiers.HasBonus.Should().BeTrue();
    }

    /// <summary>
    /// Verifies HasBonus returns false when modifiers are non-positive.
    /// </summary>
    [Test]
    public void HasBonus_ForProficient_ReturnsFalse()
    {
        // Arrange
        var modifiers = CombatProficiencyModifiers.Proficient;

        // Assert
        modifiers.HasBonus.Should().BeFalse();
    }

    /// <summary>
    /// Verifies IsExpertOrHigher for all levels.
    /// </summary>
    [Test]
    [TestCase(WeaponProficiencyLevel.NonProficient, false)]
    [TestCase(WeaponProficiencyLevel.Proficient, false)]
    [TestCase(WeaponProficiencyLevel.Expert, true)]
    [TestCase(WeaponProficiencyLevel.Master, true)]
    public void IsExpertOrHigher_ReturnsExpectedValue(
        WeaponProficiencyLevel level, bool expected)
    {
        // Arrange
        var modifiers = GetModifiersForLevel(level);

        // Assert
        modifiers.IsExpertOrHigher.Should().Be(expected);
    }

    /// <summary>
    /// Verifies IsMaster returns true only for Master level.
    /// </summary>
    [Test]
    public void IsMaster_ForMaster_ReturnsTrue()
    {
        // Arrange
        var modifiers = CombatProficiencyModifiers.Master;

        // Assert
        modifiers.IsMaster.Should().BeTrue();
    }

    /// <summary>
    /// Verifies IsMaster returns false for other levels.
    /// </summary>
    [Test]
    public void IsMaster_ForOtherLevels_ReturnsFalse()
    {
        // Assert
        CombatProficiencyModifiers.NonProficient.IsMaster.Should().BeFalse();
        CombatProficiencyModifiers.Proficient.IsMaster.Should().BeFalse();
        CombatProficiencyModifiers.Expert.IsMaster.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FromEffect Factory Method Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies FromEffect correctly maps ProficiencyEffect to CombatProficiencyModifiers.
    /// </summary>
    [Test]
    public void FromEffect_WithValidEffect_CreatesCorrectModifiers()
    {
        // Arrange
        var effect = ProficiencyEffect.CreateExpert();

        // Act
        var modifiers = CombatProficiencyModifiers.FromEffect(
            WeaponProficiencyLevel.Expert, effect);

        // Assert
        modifiers.ProficiencyLevel.Should().Be(WeaponProficiencyLevel.Expert);
        modifiers.AttackModifier.Should().Be(effect.AttackModifier);
        modifiers.DamageModifier.Should().Be(effect.DamageModifier);
        modifiers.CanUseSpecialProperties.Should().Be(effect.CanUseSpecialProperties);
        modifiers.UnlockedTechniqueLevel.Should().Be(effect.UnlockedTechniques);
    }

    /// <summary>
    /// Verifies FromEffect works for all proficiency levels.
    /// </summary>
    [Test]
    [TestCase(WeaponProficiencyLevel.NonProficient)]
    [TestCase(WeaponProficiencyLevel.Proficient)]
    [TestCase(WeaponProficiencyLevel.Expert)]
    [TestCase(WeaponProficiencyLevel.Master)]
    public void FromEffect_ForAllLevels_CreatesMatchingModifiers(WeaponProficiencyLevel level)
    {
        // Arrange
        var effect = level switch
        {
            WeaponProficiencyLevel.NonProficient => ProficiencyEffect.CreateNonProficient(),
            WeaponProficiencyLevel.Proficient => ProficiencyEffect.CreateProficient(),
            WeaponProficiencyLevel.Expert => ProficiencyEffect.CreateExpert(),
            WeaponProficiencyLevel.Master => ProficiencyEffect.CreateMaster(),
            _ => throw new ArgumentOutOfRangeException(nameof(level))
        };

        // Act
        var modifiers = CombatProficiencyModifiers.FromEffect(level, effect);

        // Assert
        modifiers.ProficiencyLevel.Should().Be(level);
        modifiers.AttackModifier.Should().Be(effect.AttackModifier);
        modifiers.DamageModifier.Should().Be(effect.DamageModifier);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Description Property Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies Description for NonProficient includes penalty information.
    /// </summary>
    [Test]
    public void Description_ForNonProficient_IncludesPenaltyInfo()
    {
        // Arrange
        var modifiers = CombatProficiencyModifiers.NonProficient;

        // Act
        var description = modifiers.Description;

        // Assert
        description.Should().Contain("NonProficient");
        description.Should().Contain("-3");
        description.Should().Contain("-2");
        description.Should().Contain("special properties blocked");
    }

    /// <summary>
    /// Verifies Description for Master includes bonus information.
    /// </summary>
    [Test]
    public void Description_ForMaster_IncludesBonusInfo()
    {
        // Arrange
        var modifiers = CombatProficiencyModifiers.Master;

        // Act
        var description = modifiers.Description;

        // Assert
        description.Should().Contain("Master");
        description.Should().Contain("+2");
        description.Should().Contain("+1");
    }

    /// <summary>
    /// Verifies Description for Proficient is simple level name.
    /// </summary>
    [Test]
    public void Description_ForProficient_ReturnsSimpleName()
    {
        // Arrange
        var modifiers = CombatProficiencyModifiers.Proficient;

        // Act
        var description = modifiers.Description;

        // Assert
        description.Should().Be("Proficient");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ToString Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies ToString returns the Description property.
    /// </summary>
    [Test]
    public void ToString_ReturnsDescription()
    {
        // Arrange
        var modifiers = CombatProficiencyModifiers.Expert;

        // Act
        var result = modifiers.ToString();

        // Assert
        result.Should().Be(modifiers.Description);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Helper Methods
    // ═══════════════════════════════════════════════════════════════════════════

    private static CombatProficiencyModifiers GetModifiersForLevel(WeaponProficiencyLevel level)
    {
        return level switch
        {
            WeaponProficiencyLevel.NonProficient => CombatProficiencyModifiers.NonProficient,
            WeaponProficiencyLevel.Proficient => CombatProficiencyModifiers.Proficient,
            WeaponProficiencyLevel.Expert => CombatProficiencyModifiers.Expert,
            WeaponProficiencyLevel.Master => CombatProficiencyModifiers.Master,
            _ => throw new ArgumentOutOfRangeException(nameof(level))
        };
    }
}
