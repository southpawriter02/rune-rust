using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for MonsterDropClassMapping value object (v0.16.0d).
/// </summary>
[TestFixture]
public class MonsterDropClassMappingTests
{
    // ═══════════════════════════════════════════════════════════════
    // Create TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Create_WithValidParameters_CreatesMappingCorrectly()
    {
        // Arrange & Act
        var mapping = MonsterDropClassMapping.Create(
            "dragon-boss",
            EnemyDropClass.Boss,
            dropCount: 3);

        // Assert
        mapping.MonsterTypeId.Should().Be("dragon-boss");
        mapping.DropClass.Should().Be(EnemyDropClass.Boss);
        mapping.DropCount.Should().Be(3);
    }

    [Test]
    public void Create_NormalizesMonsterTypeIdToLowercase()
    {
        // Arrange & Act
        var mapping = MonsterDropClassMapping.Create(
            "DRAGON-BOSS",
            EnemyDropClass.Boss);

        // Assert
        mapping.MonsterTypeId.Should().Be("dragon-boss");
    }

    [Test]
    public void Create_WithDefaultDropCount_DefaultsToOne()
    {
        // Arrange & Act
        var mapping = MonsterDropClassMapping.Create("goblin", EnemyDropClass.Standard);

        // Assert
        mapping.DropCount.Should().Be(1);
    }

    [Test]
    public void Create_WithNullMonsterTypeId_ThrowsArgumentException()
    {
        // Act
        var act = () => MonsterDropClassMapping.Create(null!, EnemyDropClass.Standard);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("monsterTypeId");
    }

    [Test]
    public void Create_WithWhitespaceMonsterTypeId_ThrowsArgumentException()
    {
        // Act
        var act = () => MonsterDropClassMapping.Create("   ", EnemyDropClass.Standard);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("monsterTypeId");
    }

    [Test]
    public void Create_WithNegativeDropCount_ThrowsArgumentOutOfRangeException()
    {
        // Act
        var act = () => MonsterDropClassMapping.Create("goblin", EnemyDropClass.Standard, dropCount: -1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("dropCount");
    }

    // ═══════════════════════════════════════════════════════════════
    // Default TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Default_ReturnsStandardClassWithOneItem()
    {
        // Act
        var defaultMapping = MonsterDropClassMapping.Default;

        // Assert
        defaultMapping.MonsterTypeId.Should().Be("unknown");
        defaultMapping.DropClass.Should().Be(EnemyDropClass.Standard);
        defaultMapping.DropCount.Should().Be(1);
    }

    // ═══════════════════════════════════════════════════════════════
    // Helper Property TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void DropsMultipleItems_WithCountGreaterThanOne_ReturnsTrue()
    {
        // Arrange
        var mapping = MonsterDropClassMapping.Create("boss", EnemyDropClass.Boss, dropCount: 3);

        // Act & Assert
        mapping.DropsMultipleItems.Should().BeTrue();
    }

    [Test]
    public void DropsMultipleItems_WithCountOfOne_ReturnsFalse()
    {
        // Arrange
        var mapping = MonsterDropClassMapping.Create("goblin", EnemyDropClass.Standard, dropCount: 1);

        // Act & Assert
        mapping.DropsMultipleItems.Should().BeFalse();
    }

    [Test]
    public void IsBoss_ForBossClass_ReturnsTrue()
    {
        // Arrange
        var mapping = MonsterDropClassMapping.Create("dragon", EnemyDropClass.Boss);

        // Act & Assert
        mapping.IsBoss.Should().BeTrue();
    }

    [Test]
    public void IsBoss_ForNonBossClass_ReturnsFalse()
    {
        // Arrange
        var mapping = MonsterDropClassMapping.Create("goblin", EnemyDropClass.Standard);

        // Act & Assert
        mapping.IsBoss.Should().BeFalse();
    }

    [Test]
    public void IsMiniBoss_ForMiniBossClass_ReturnsTrue()
    {
        // Arrange
        var mapping = MonsterDropClassMapping.Create("warden", EnemyDropClass.MiniBoss);

        // Act & Assert
        mapping.IsMiniBoss.Should().BeTrue();
    }

    [Test]
    public void IsElite_ForEliteClass_ReturnsTrue()
    {
        // Arrange
        var mapping = MonsterDropClassMapping.Create("elite-guard", EnemyDropClass.Elite);

        // Act & Assert
        mapping.IsElite.Should().BeTrue();
    }

    [Test]
    public void CanDropNothing_ForTrashClass_ReturnsTrue()
    {
        // Arrange
        var mapping = MonsterDropClassMapping.Create("servitor", EnemyDropClass.Trash);

        // Act & Assert
        mapping.CanDropNothing.Should().BeTrue();
    }

    [Test]
    public void CanDropNothing_ForNonTrashClass_ReturnsFalse()
    {
        // Arrange
        var mapping = MonsterDropClassMapping.Create("goblin", EnemyDropClass.Standard);

        // Act & Assert
        mapping.CanDropNothing.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════
    // Factory Helper TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void CreateBoss_SetsCorrectDefaults()
    {
        // Act
        var mapping = MonsterDropClassMapping.CreateBoss("dragon");

        // Assert
        mapping.DropClass.Should().Be(EnemyDropClass.Boss);
        mapping.DropCount.Should().Be(3);
    }

    [Test]
    public void CreateMiniBoss_SetsCorrectDefaults()
    {
        // Act
        var mapping = MonsterDropClassMapping.CreateMiniBoss("warden");

        // Assert
        mapping.DropClass.Should().Be(EnemyDropClass.MiniBoss);
        mapping.DropCount.Should().Be(2);
    }

    [Test]
    public void CreateElite_SetsCorrectDefaults()
    {
        // Act
        var mapping = MonsterDropClassMapping.CreateElite("elite-guard");

        // Assert
        mapping.DropClass.Should().Be(EnemyDropClass.Elite);
        mapping.DropCount.Should().Be(2);
    }

    // ═══════════════════════════════════════════════════════════════
    // ToString TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void ToString_ReturnsFormattedString()
    {
        // Arrange
        var mapping = MonsterDropClassMapping.Create("dragon-boss", EnemyDropClass.Boss, dropCount: 3);

        // Act
        var result = mapping.ToString();

        // Assert
        result.Should().Be("[MonsterDropClassMapping: dragon-boss -> Boss, drops=3]");
    }
}
