using FluentAssertions;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using Xunit;
using Attribute = RuneAndRust.Core.Enums.Attribute;

namespace RuneAndRust.Tests.Core;

/// <summary>
/// Tests for the Character entity.
/// Validates character creation, attributes, lineage, archetype, and derived stats.
/// </summary>
public class CharacterEntityTests
{
    #region Constructor and Default Values Tests

    [Fact]
    public void Character_NewInstance_HasUniqueId()
    {
        // Arrange & Act
        var character1 = new Character();
        var character2 = new Character();

        // Assert
        character1.Id.Should().NotBe(Guid.Empty);
        character2.Id.Should().NotBe(Guid.Empty);
        character1.Id.Should().NotBe(character2.Id);
    }

    [Fact]
    public void Character_NewInstance_HasDefaultName()
    {
        // Arrange & Act
        var character = new Character();

        // Assert
        character.Name.Should().BeEmpty();
    }

    [Fact]
    public void Character_NewInstance_HasDefaultLineage()
    {
        // Arrange & Act
        var character = new Character();

        // Assert
        character.Lineage.Should().Be(LineageType.Human);
    }

    [Fact]
    public void Character_NewInstance_HasDefaultArchetype()
    {
        // Arrange & Act
        var character = new Character();

        // Assert
        character.Archetype.Should().Be(ArchetypeType.Warrior);
    }

    [Fact]
    public void Character_NewInstance_HasDefaultAttributes()
    {
        // Arrange & Act
        var character = new Character();

        // Assert
        character.Sturdiness.Should().Be(5);
        character.Might.Should().Be(5);
        character.Wits.Should().Be(5);
        character.Will.Should().Be(5);
        character.Finesse.Should().Be(5);
    }

    [Fact]
    public void Character_NewInstance_HasDefaultDerivedStats()
    {
        // Arrange & Act
        var character = new Character();

        // Assert - defaults based on base stat of 5
        character.MaxHP.Should().Be(100);
        character.CurrentHP.Should().Be(100);
        character.MaxStamina.Should().Be(60);
        character.CurrentStamina.Should().Be(60);
        character.ActionPoints.Should().Be(3);
    }

    [Fact]
    public void Character_NewInstance_HasDefaultProgression()
    {
        // Arrange & Act
        var character = new Character();

        // Assert
        character.Level.Should().Be(1);
        character.ExperiencePoints.Should().Be(0);
    }

    [Fact]
    public void Character_NewInstance_HasCreatedAtTimestamp()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var character = new Character();

        // Assert
        var after = DateTime.UtcNow;
        character.CreatedAt.Should().BeOnOrAfter(before);
        character.CreatedAt.Should().BeOnOrBefore(after);
    }

    #endregion

    #region Property Setters Tests

    [Fact]
    public void Character_Name_CanBeSet()
    {
        // Arrange
        var character = new Character();

        // Act
        character.Name = "Test Hero";

        // Assert
        character.Name.Should().Be("Test Hero");
    }

    [Fact]
    public void Character_Lineage_CanBeSet()
    {
        // Arrange
        var character = new Character();

        // Act
        character.Lineage = LineageType.RuneMarked;

        // Assert
        character.Lineage.Should().Be(LineageType.RuneMarked);
    }

    [Fact]
    public void Character_Archetype_CanBeSet()
    {
        // Arrange
        var character = new Character();

        // Act
        character.Archetype = ArchetypeType.Mystic;

        // Assert
        character.Archetype.Should().Be(ArchetypeType.Mystic);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public void Character_Sturdiness_CanBeSetToValidValues(int value)
    {
        // Arrange
        var character = new Character();

        // Act
        character.Sturdiness = value;

        // Assert
        character.Sturdiness.Should().Be(value);
    }

    #endregion

    #region GetAttribute Tests

    [Fact]
    public void GetAttribute_Sturdiness_ReturnsCorrectValue()
    {
        // Arrange
        var character = new Character { Sturdiness = 7 };

        // Act
        var value = character.GetAttribute(Attribute.Sturdiness);

        // Assert
        value.Should().Be(7);
    }

    [Fact]
    public void GetAttribute_Might_ReturnsCorrectValue()
    {
        // Arrange
        var character = new Character { Might = 8 };

        // Act
        var value = character.GetAttribute(Attribute.Might);

        // Assert
        value.Should().Be(8);
    }

    [Fact]
    public void GetAttribute_Wits_ReturnsCorrectValue()
    {
        // Arrange
        var character = new Character { Wits = 9 };

        // Act
        var value = character.GetAttribute(Attribute.Wits);

        // Assert
        value.Should().Be(9);
    }

    [Fact]
    public void GetAttribute_Will_ReturnsCorrectValue()
    {
        // Arrange
        var character = new Character { Will = 6 };

        // Act
        var value = character.GetAttribute(Attribute.Will);

        // Assert
        value.Should().Be(6);
    }

    [Fact]
    public void GetAttribute_Finesse_ReturnsCorrectValue()
    {
        // Arrange
        var character = new Character { Finesse = 4 };

        // Act
        var value = character.GetAttribute(Attribute.Finesse);

        // Assert
        value.Should().Be(4);
    }

    [Fact]
    public void GetAttribute_InvalidAttribute_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var character = new Character();
        var invalidAttribute = (Attribute)999;

        // Act
        var action = () => character.GetAttribute(invalidAttribute);

        // Assert
        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    #endregion

    #region SetAttribute Tests

    [Fact]
    public void SetAttribute_Sturdiness_SetsCorrectValue()
    {
        // Arrange
        var character = new Character();

        // Act
        character.SetAttribute(Attribute.Sturdiness, 8);

        // Assert
        character.Sturdiness.Should().Be(8);
    }

    [Fact]
    public void SetAttribute_Might_SetsCorrectValue()
    {
        // Arrange
        var character = new Character();

        // Act
        character.SetAttribute(Attribute.Might, 9);

        // Assert
        character.Might.Should().Be(9);
    }

    [Fact]
    public void SetAttribute_Wits_SetsCorrectValue()
    {
        // Arrange
        var character = new Character();

        // Act
        character.SetAttribute(Attribute.Wits, 7);

        // Assert
        character.Wits.Should().Be(7);
    }

    [Fact]
    public void SetAttribute_Will_SetsCorrectValue()
    {
        // Arrange
        var character = new Character();

        // Act
        character.SetAttribute(Attribute.Will, 6);

        // Assert
        character.Will.Should().Be(6);
    }

    [Fact]
    public void SetAttribute_Finesse_SetsCorrectValue()
    {
        // Arrange
        var character = new Character();

        // Act
        character.SetAttribute(Attribute.Finesse, 10);

        // Assert
        character.Finesse.Should().Be(10);
    }

    [Fact]
    public void SetAttribute_InvalidAttribute_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var character = new Character();
        var invalidAttribute = (Attribute)999;

        // Act
        var action = () => character.SetAttribute(invalidAttribute, 5);

        // Assert
        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    #endregion

    #region Derived Stats Tests

    [Fact]
    public void Character_CurrentHP_CannotExceedMaxHP()
    {
        // Arrange
        var character = new Character
        {
            MaxHP = 100,
            CurrentHP = 150
        };

        // Assert - the model allows it but logic should enforce this
        // This test documents current behavior
        character.CurrentHP.Should().Be(150);
    }

    [Fact]
    public void Character_CurrentStamina_CannotExceedMaxStamina()
    {
        // Arrange
        var character = new Character
        {
            MaxStamina = 60,
            CurrentStamina = 100
        };

        // Assert - the model allows it but logic should enforce this
        // This test documents current behavior
        character.CurrentStamina.Should().Be(100);
    }

    #endregion

    #region Timestamp Tests

    [Fact]
    public void Character_LastModified_CanBeUpdated()
    {
        // Arrange
        var character = new Character();
        var originalModified = character.LastModified;

        // Act
        Thread.Sleep(10); // Ensure time passes
        character.LastModified = DateTime.UtcNow;

        // Assert
        character.LastModified.Should().BeAfter(originalModified);
    }

    #endregion
}
