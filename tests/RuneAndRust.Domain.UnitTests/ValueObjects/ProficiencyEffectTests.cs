using FluentAssertions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="ProficiencyEffect"/> value object.
/// </summary>
[TestFixture]
public class ProficiencyEffectTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Factory Method Tests - Create
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that Create method produces a valid ProficiencyEffect with valid parameters.
    /// </summary>
    [Test]
    public void Create_WithValidParameters_CreatesProficiencyEffect()
    {
        // Arrange & Act
        var effect = ProficiencyEffect.Create(
            WeaponProficiencyLevel.Expert,
            attackModifier: 1,
            damageModifier: 0,
            canUseSpecialProperties: true,
            TechniqueAccess.Advanced,
            "Expert",
            "Advanced training mastered.");

        // Assert
        effect.Level.Should().Be(WeaponProficiencyLevel.Expert);
        effect.AttackModifier.Should().Be(1);
        effect.DamageModifier.Should().Be(0);
        effect.CanUseSpecialProperties.Should().BeTrue();
        effect.UnlockedTechniques.Should().Be(TechniqueAccess.Advanced);
        effect.DisplayName.Should().Be("Expert");
        effect.Description.Should().Be("Advanced training mastered.");
    }

    /// <summary>
    /// Verifies that Create throws when displayName is null.
    /// </summary>
    [Test]
    public void Create_WithNullDisplayName_ThrowsArgumentException()
    {
        // Arrange
        var act = () => ProficiencyEffect.Create(
            WeaponProficiencyLevel.Proficient,
            0, 0, true,
            TechniqueAccess.Basic,
            null!,
            "Description");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Verifies that Create throws when description is empty.
    /// </summary>
    [Test]
    public void Create_WithEmptyDescription_ThrowsArgumentException()
    {
        // Arrange
        var act = () => ProficiencyEffect.Create(
            WeaponProficiencyLevel.Proficient,
            0, 0, true,
            TechniqueAccess.Basic,
            "Proficient",
            "   ");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Verifies that Create throws when attackModifier is out of range.
    /// </summary>
    [Test]
    [TestCase(-6)]
    [TestCase(6)]
    public void Create_WithOutOfRangeAttackModifier_ThrowsArgumentOutOfRangeException(int attackModifier)
    {
        // Arrange
        var act = () => ProficiencyEffect.Create(
            WeaponProficiencyLevel.Proficient,
            attackModifier,
            0, true,
            TechniqueAccess.Basic,
            "Test",
            "Test description");

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// Verifies that Create throws when damageModifier is out of range.
    /// </summary>
    [Test]
    [TestCase(-6)]
    [TestCase(6)]
    public void Create_WithOutOfRangeDamageModifier_ThrowsArgumentOutOfRangeException(int damageModifier)
    {
        // Arrange
        var act = () => ProficiencyEffect.Create(
            WeaponProficiencyLevel.Proficient,
            0,
            damageModifier,
            true,
            TechniqueAccess.Basic,
            "Test",
            "Test description");

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Static Factory Method Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that CreateNonProficient produces correct default values.
    /// </summary>
    [Test]
    public void CreateNonProficient_ReturnsCorrectDefaults()
    {
        // Act
        var effect = ProficiencyEffect.CreateNonProficient();

        // Assert
        effect.Level.Should().Be(WeaponProficiencyLevel.NonProficient);
        effect.AttackModifier.Should().Be(-3);
        effect.DamageModifier.Should().Be(-2);
        effect.CanUseSpecialProperties.Should().BeFalse();
        effect.UnlockedTechniques.Should().Be(TechniqueAccess.None);
        effect.DisplayName.Should().Be("Non-Proficient");
    }

    /// <summary>
    /// Verifies that CreateProficient produces correct default values.
    /// </summary>
    [Test]
    public void CreateProficient_ReturnsCorrectDefaults()
    {
        // Act
        var effect = ProficiencyEffect.CreateProficient();

        // Assert
        effect.Level.Should().Be(WeaponProficiencyLevel.Proficient);
        effect.AttackModifier.Should().Be(0);
        effect.DamageModifier.Should().Be(0);
        effect.CanUseSpecialProperties.Should().BeTrue();
        effect.UnlockedTechniques.Should().Be(TechniqueAccess.Basic);
    }

    /// <summary>
    /// Verifies that CreateExpert produces correct default values.
    /// </summary>
    [Test]
    public void CreateExpert_ReturnsCorrectDefaults()
    {
        // Act
        var effect = ProficiencyEffect.CreateExpert();

        // Assert
        effect.Level.Should().Be(WeaponProficiencyLevel.Expert);
        effect.AttackModifier.Should().Be(1);
        effect.DamageModifier.Should().Be(0);
        effect.CanUseSpecialProperties.Should().BeTrue();
        effect.UnlockedTechniques.Should().Be(TechniqueAccess.Advanced);
    }

    /// <summary>
    /// Verifies that CreateMaster produces correct default values.
    /// </summary>
    [Test]
    public void CreateMaster_ReturnsCorrectDefaults()
    {
        // Act
        var effect = ProficiencyEffect.CreateMaster();

        // Assert
        effect.Level.Should().Be(WeaponProficiencyLevel.Master);
        effect.AttackModifier.Should().Be(2);
        effect.DamageModifier.Should().Be(1);
        effect.CanUseSpecialProperties.Should().BeTrue();
        effect.UnlockedTechniques.Should().Be(TechniqueAccess.Signature);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Derived Property Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies HasAttackPenalty is true for negative attack modifiers.
    /// </summary>
    [Test]
    public void HasAttackPenalty_ForNonProficient_ReturnsTrue()
    {
        // Arrange
        var effect = ProficiencyEffect.CreateNonProficient();

        // Assert
        effect.HasAttackPenalty.Should().BeTrue();
    }

    /// <summary>
    /// Verifies HasAttackPenalty is false for non-negative attack modifiers.
    /// </summary>
    [Test]
    public void HasAttackPenalty_ForProficient_ReturnsFalse()
    {
        // Arrange
        var effect = ProficiencyEffect.CreateProficient();

        // Assert
        effect.HasAttackPenalty.Should().BeFalse();
    }

    /// <summary>
    /// Verifies HasAttackBonus is true for positive attack modifiers.
    /// </summary>
    [Test]
    public void HasAttackBonus_ForMaster_ReturnsTrue()
    {
        // Arrange
        var effect = ProficiencyEffect.CreateMaster();

        // Assert
        effect.HasAttackBonus.Should().BeTrue();
    }

    /// <summary>
    /// Verifies HasDamagePenalty is true for negative damage modifiers.
    /// </summary>
    [Test]
    public void HasDamagePenalty_ForNonProficient_ReturnsTrue()
    {
        // Arrange
        var effect = ProficiencyEffect.CreateNonProficient();

        // Assert
        effect.HasDamagePenalty.Should().BeTrue();
    }

    /// <summary>
    /// Verifies HasDamageBonus is true for positive damage modifiers.
    /// </summary>
    [Test]
    public void HasDamageBonus_ForMaster_ReturnsTrue()
    {
        // Arrange
        var effect = ProficiencyEffect.CreateMaster();

        // Assert
        effect.HasDamageBonus.Should().BeTrue();
    }

    /// <summary>
    /// Verifies HasDamageBonus is false for expert (0 damage modifier).
    /// </summary>
    [Test]
    public void HasDamageBonus_ForExpert_ReturnsFalse()
    {
        // Arrange
        var effect = ProficiencyEffect.CreateExpert();

        // Assert
        effect.HasDamageBonus.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Technique Access Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies CanUseBasicTechniques for all levels.
    /// </summary>
    [Test]
    [TestCase(WeaponProficiencyLevel.NonProficient, false)]
    [TestCase(WeaponProficiencyLevel.Proficient, true)]
    [TestCase(WeaponProficiencyLevel.Expert, true)]
    [TestCase(WeaponProficiencyLevel.Master, true)]
    public void CanUseBasicTechniques_ReturnsExpectedValue(
        WeaponProficiencyLevel level, bool expected)
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

        // Assert
        effect.CanUseBasicTechniques.Should().Be(expected);
    }

    /// <summary>
    /// Verifies CanUseAdvancedTechniques for all levels.
    /// </summary>
    [Test]
    [TestCase(WeaponProficiencyLevel.NonProficient, false)]
    [TestCase(WeaponProficiencyLevel.Proficient, false)]
    [TestCase(WeaponProficiencyLevel.Expert, true)]
    [TestCase(WeaponProficiencyLevel.Master, true)]
    public void CanUseAdvancedTechniques_ReturnsExpectedValue(
        WeaponProficiencyLevel level, bool expected)
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

        // Assert
        effect.CanUseAdvancedTechniques.Should().Be(expected);
    }

    /// <summary>
    /// Verifies CanUseSignatureTechniques for all levels.
    /// </summary>
    [Test]
    [TestCase(WeaponProficiencyLevel.NonProficient, false)]
    [TestCase(WeaponProficiencyLevel.Proficient, false)]
    [TestCase(WeaponProficiencyLevel.Expert, false)]
    [TestCase(WeaponProficiencyLevel.Master, true)]
    public void CanUseSignatureTechniques_ReturnsExpectedValue(
        WeaponProficiencyLevel level, bool expected)
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

        // Assert
        effect.CanUseSignatureTechniques.Should().Be(expected);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Format Method Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies FormatAttackModifier produces correct format for positive values.
    /// </summary>
    [Test]
    public void FormatAttackModifier_WithPositiveValue_ReturnsPlusFormat()
    {
        // Arrange
        var effect = ProficiencyEffect.CreateMaster();

        // Act
        var result = effect.FormatAttackModifier();

        // Assert
        result.Should().Be("+2");
    }

    /// <summary>
    /// Verifies FormatAttackModifier produces correct format for negative values.
    /// </summary>
    [Test]
    public void FormatAttackModifier_WithNegativeValue_ReturnsMinusFormat()
    {
        // Arrange
        var effect = ProficiencyEffect.CreateNonProficient();

        // Act
        var result = effect.FormatAttackModifier();

        // Assert
        result.Should().Be("-3");
    }

    /// <summary>
    /// Verifies FormatAttackModifier produces correct format for zero.
    /// </summary>
    [Test]
    public void FormatAttackModifier_WithZeroValue_ReturnsPlusZeroFormat()
    {
        // Arrange
        var effect = ProficiencyEffect.CreateProficient();

        // Act
        var result = effect.FormatAttackModifier();

        // Assert
        result.Should().Be("+0");
    }

    /// <summary>
    /// Verifies FormatDamageModifier produces correct format for positive values.
    /// </summary>
    [Test]
    public void FormatDamageModifier_WithPositiveValue_ReturnsPlusFormat()
    {
        // Arrange
        var effect = ProficiencyEffect.CreateMaster();

        // Act
        var result = effect.FormatDamageModifier();

        // Assert
        result.Should().Be("+1");
    }

    /// <summary>
    /// Verifies FormatDamageModifier produces correct format for negative values.
    /// </summary>
    [Test]
    public void FormatDamageModifier_WithNegativeValue_ReturnsMinusFormat()
    {
        // Arrange
        var effect = ProficiencyEffect.CreateNonProficient();

        // Act
        var result = effect.FormatDamageModifier();

        // Assert
        result.Should().Be("-2");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ToString Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies ToString produces a readable summary.
    /// </summary>
    [Test]
    public void ToString_ReturnsFormattedSummary()
    {
        // Arrange
        var effect = ProficiencyEffect.CreateExpert();

        // Act
        var result = effect.ToString();

        // Assert
        result.Should().Contain("Expert");
        result.Should().Contain("Atk +1");
        result.Should().Contain("Dmg +0");
        result.Should().Contain("Special: True");
        result.Should().Contain("Techniques: Advanced");
    }

    /// <summary>
    /// Verifies ToDebugString produces detailed output.
    /// </summary>
    [Test]
    public void ToDebugString_ReturnsDetailedOutput()
    {
        // Arrange
        var effect = ProficiencyEffect.CreateMaster();

        // Act
        var result = effect.ToDebugString();

        // Assert
        result.Should().Contain("ProficiencyEffect");
        result.Should().Contain("Level: Master");
        result.Should().Contain("(3)");
        result.Should().Contain("+2");
        result.Should().Contain("+1");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // LevelValue Property Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies LevelValue returns the integer representation of the level.
    /// </summary>
    [Test]
    [TestCase(WeaponProficiencyLevel.NonProficient, 0)]
    [TestCase(WeaponProficiencyLevel.Proficient, 1)]
    [TestCase(WeaponProficiencyLevel.Expert, 2)]
    [TestCase(WeaponProficiencyLevel.Master, 3)]
    public void LevelValue_ReturnsIntegerRepresentation(
        WeaponProficiencyLevel level, int expectedValue)
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

        // Assert
        effect.LevelValue.Should().Be(expectedValue);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Identity Property Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies IsNonProficient property for all levels.
    /// </summary>
    [Test]
    public void IsNonProficient_ForNonProficient_ReturnsTrue()
    {
        // Arrange
        var effect = ProficiencyEffect.CreateNonProficient();

        // Assert
        effect.IsNonProficient.Should().BeTrue();
    }

    /// <summary>
    /// Verifies IsNonProficient property for other levels.
    /// </summary>
    [Test]
    public void IsNonProficient_ForOtherLevels_ReturnsFalse()
    {
        // Assert
        ProficiencyEffect.CreateProficient().IsNonProficient.Should().BeFalse();
        ProficiencyEffect.CreateExpert().IsNonProficient.Should().BeFalse();
        ProficiencyEffect.CreateMaster().IsNonProficient.Should().BeFalse();
    }

    /// <summary>
    /// Verifies IsMaster property for Master level.
    /// </summary>
    [Test]
    public void IsMaster_ForMaster_ReturnsTrue()
    {
        // Arrange
        var effect = ProficiencyEffect.CreateMaster();

        // Assert
        effect.IsMaster.Should().BeTrue();
    }

    /// <summary>
    /// Verifies IsMaster property for other levels.
    /// </summary>
    [Test]
    public void IsMaster_ForOtherLevels_ReturnsFalse()
    {
        // Assert
        ProficiencyEffect.CreateNonProficient().IsMaster.Should().BeFalse();
        ProficiencyEffect.CreateProficient().IsMaster.Should().BeFalse();
        ProficiencyEffect.CreateExpert().IsMaster.Should().BeFalse();
    }
}
