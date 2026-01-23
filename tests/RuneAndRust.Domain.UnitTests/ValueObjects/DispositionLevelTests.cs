namespace RuneAndRust.Domain.UnitTests.ValueObjects;

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Extensions;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Unit tests for <see cref="DispositionLevel"/> value object.
/// </summary>
[TestFixture]
public class DispositionLevelTests
{
    #region Factory Methods

    [Test]
    public void CreateNeutral_ReturnsZeroValue()
    {
        // Arrange & Act
        var disposition = DispositionLevel.CreateNeutral();

        // Assert
        disposition.Value.Should().Be(0);
        disposition.Category.Should().Be(NpcDisposition.Neutral);
        disposition.DiceModifier.Should().Be(0);
        disposition.IsLocked.Should().BeFalse();
    }

    [Test]
    [TestCase(100, NpcDisposition.Ally, 3)]
    [TestCase(75, NpcDisposition.Ally, 3)]
    [TestCase(74, NpcDisposition.Friendly, 2)]
    [TestCase(50, NpcDisposition.Friendly, 2)]
    [TestCase(49, NpcDisposition.NeutralPositive, 1)]
    [TestCase(10, NpcDisposition.NeutralPositive, 1)]
    [TestCase(9, NpcDisposition.Neutral, 0)]
    [TestCase(0, NpcDisposition.Neutral, 0)]
    [TestCase(-9, NpcDisposition.Neutral, 0)]
    [TestCase(-10, NpcDisposition.Unfriendly, -1)]
    [TestCase(-49, NpcDisposition.Unfriendly, -1)]
    [TestCase(-50, NpcDisposition.Hostile, -2)]
    [TestCase(-100, NpcDisposition.Hostile, -2)]
    public void Create_WithValue_ReturnsCorrectCategoryAndModifier(int value, NpcDisposition expectedCategory, int expectedModifier)
    {
        // Arrange & Act
        var disposition = DispositionLevel.Create(value);

        // Assert
        disposition.ClampedValue.Should().Be(value);
        disposition.Category.Should().Be(expectedCategory);
        disposition.DiceModifier.Should().Be(expectedModifier);
    }

    [Test]
    public void Create_WithValueAbove100_ClampedTo100()
    {
        // Arrange & Act
        var disposition = DispositionLevel.Create(150);

        // Assert
        disposition.ClampedValue.Should().Be(100);
        disposition.Category.Should().Be(NpcDisposition.Ally);
    }

    [Test]
    public void Create_WithValueBelowMinus100_ClampedToMinus100()
    {
        // Arrange & Act
        var disposition = DispositionLevel.Create(-150);

        // Assert
        disposition.ClampedValue.Should().Be(-100);
        disposition.Category.Should().Be(NpcDisposition.Hostile);
    }

    [Test]
    [TestCase(NpcDisposition.Ally, 87)]
    [TestCase(NpcDisposition.Friendly, 62)]
    [TestCase(NpcDisposition.NeutralPositive, 30)]
    [TestCase(NpcDisposition.Neutral, 0)]
    [TestCase(NpcDisposition.Unfriendly, -30)]
    [TestCase(NpcDisposition.Hostile, -75)]
    public void FromCategory_ReturnsMidpointValue(NpcDisposition category, int expectedMidpoint)
    {
        // Arrange & Act
        var disposition = DispositionLevel.FromCategory(category);

        // Assert
        disposition.Value.Should().Be(expectedMidpoint);
        disposition.Category.Should().Be(category);
    }

    #endregion

    #region Modify

    [Test]
    public void Modify_WhenUnlocked_ChangesValue()
    {
        // Arrange
        var disposition = DispositionLevel.Create(50);

        // Act
        var modified = disposition.Modify(10);

        // Assert
        modified.Value.Should().Be(60);
        modified.Category.Should().Be(NpcDisposition.Friendly);
    }

    [Test]
    public void Modify_WhenLocked_ReturnsUnchanged()
    {
        // Arrange
        var disposition = DispositionLevel.Create(50).Lock("Trust Shattered");

        // Act
        var modified = disposition.Modify(10);

        // Assert
        modified.Value.Should().Be(50);
        modified.IsLocked.Should().BeTrue();
    }

    [Test]
    public void Modify_NegativeChange_DecreasesValue()
    {
        // Arrange
        var disposition = DispositionLevel.Create(50);

        // Act
        var modified = disposition.Modify(-30);

        // Assert
        modified.Value.Should().Be(20);
        modified.Category.Should().Be(NpcDisposition.NeutralPositive);
    }

    [Test]
    public void Modify_CrossesThreshold_ChangesCategory()
    {
        // Arrange
        var disposition = DispositionLevel.Create(48); // NeutralPositive

        // Act
        var modified = disposition.Modify(10);

        // Assert
        modified.Value.Should().Be(58);
        modified.Category.Should().Be(NpcDisposition.Friendly);
    }

    #endregion

    #region Lock/Unlock

    [Test]
    public void Lock_SetsIsLockedAndReason()
    {
        // Arrange
        var disposition = DispositionLevel.Create(50);

        // Act
        var locked = disposition.Lock("Trust Shattered");

        // Assert
        locked.IsLocked.Should().BeTrue();
        locked.LockedReason.Should().Be("Trust Shattered");
        locked.Value.Should().Be(50);
    }

    [Test]
    public void Unlock_ClearsIsLockedAndReason()
    {
        // Arrange
        var disposition = DispositionLevel.Create(50).Lock("Trust Shattered");

        // Act
        var unlocked = disposition.Unlock();

        // Assert
        unlocked.IsLocked.Should().BeFalse();
        unlocked.LockedReason.Should().BeNull();
    }

    #endregion

    #region Properties

    [Test]
    [TestCase(NpcDisposition.Ally, true, false)]
    [TestCase(NpcDisposition.Friendly, true, false)]
    [TestCase(NpcDisposition.NeutralPositive, true, false)]
    [TestCase(NpcDisposition.Neutral, false, false)]
    [TestCase(NpcDisposition.Unfriendly, false, true)]
    [TestCase(NpcDisposition.Hostile, false, true)]
    public void IsPositive_And_IsNegative_ReflectCategory(
        NpcDisposition category, bool expectedPositive, bool expectedNegative)
    {
        // Arrange
        var disposition = DispositionLevel.FromCategory(category);

        // Assert
        disposition.IsPositive.Should().Be(expectedPositive);
        disposition.IsNegative.Should().Be(expectedNegative);
    }

    [Test]
    public void BlocksSomeInteractions_OnlyTrueForHostile()
    {
        // Arrange & Act
        var hostile = DispositionLevel.FromCategory(NpcDisposition.Hostile);
        var unfriendly = DispositionLevel.FromCategory(NpcDisposition.Unfriendly);

        // Assert
        hostile.BlocksSomeInteractions.Should().BeTrue();
        unfriendly.BlocksSomeInteractions.Should().BeFalse();
    }

    [Test]
    public void AllowsAlliance_OnlyTrueForFriendlyAndAlly()
    {
        // Arrange & Act
        var ally = DispositionLevel.FromCategory(NpcDisposition.Ally);
        var friendly = DispositionLevel.FromCategory(NpcDisposition.Friendly);
        var neutral = DispositionLevel.FromCategory(NpcDisposition.Neutral);

        // Assert
        ally.AllowsAlliance.Should().BeTrue();
        friendly.AllowsAlliance.Should().BeTrue();
        neutral.AllowsAlliance.Should().BeFalse();
    }

    #endregion

    #region DistanceTo

    [Test]
    public void DistanceTo_SameCategory_ReturnsZero()
    {
        // Arrange
        var disposition = DispositionLevel.Create(60); // Friendly

        // Act
        var distance = disposition.DistanceTo(NpcDisposition.Friendly);

        // Assert
        distance.Should().Be(0);
    }

    [Test]
    public void DistanceTo_HigherCategory_ReturnsPositiveDistance()
    {
        // Arrange
        var disposition = DispositionLevel.Create(60); // Friendly

        // Act
        var distance = disposition.DistanceTo(NpcDisposition.Ally);

        // Assert
        distance.Should().Be(15); // Need to reach 75
    }

    #endregion

    #region ToString

    [Test]
    public void ToDescription_IncludesAllComponents()
    {
        // Arrange
        var disposition = DispositionLevel.Create(60);

        // Act
        var description = disposition.ToDescription();

        // Assert
        description.Should().Contain("Friendly");
        description.Should().Contain("60");
        description.Should().Contain("+2d10");
    }

    [Test]
    public void ToDescription_WhenLocked_IncludesLockedStatus()
    {
        // Arrange
        var disposition = DispositionLevel.Create(50).Lock("Trust Shattered");

        // Act
        var description = disposition.ToDescription();

        // Assert
        description.Should().Contain("[LOCKED:");
        description.Should().Contain("Trust Shattered");
    }

    #endregion
}
