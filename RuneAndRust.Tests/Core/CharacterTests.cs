using FluentAssertions;
using RuneAndRust.Core.Models;
using Xunit;
using Attribute = RuneAndRust.Core.Enums.Attribute;

namespace RuneAndRust.Tests.Core;

/// <summary>
/// Tests for the Character entity.
/// Validates character creation, attribute access, and data integrity.
/// </summary>
public class CharacterTests
{
    [Fact]
    public void Character_NewCharacter_HasUniqueId()
    {
        // Arrange & Act
        var character1 = new Character("Hero One");
        var character2 = new Character("Hero Two");

        // Assert
        character1.Id.Should().NotBeEmpty();
        character2.Id.Should().NotBeEmpty();
        character1.Id.Should().NotBe(character2.Id, "each character should have a unique Id");
    }

    [Fact]
    public void Character_Name_CanBeSet()
    {
        // Arrange
        var character = new Character("Original Name");

        // Act
        character.Name = "New Name";

        // Assert
        character.Name.Should().Be("New Name");
    }

    [Fact]
    public void Character_Name_IsSetByConstructor()
    {
        // Arrange & Act
        var character = new Character("Test Character");

        // Assert
        character.Name.Should().Be("Test Character");
    }

    [Fact]
    public void Character_Attributes_InitializedWithDefaults()
    {
        // Arrange & Act
        var character = new Character("Test Character");

        // Assert
        character.Attributes.Should().HaveCount(5);
        character.Attributes.Values.Should().AllBeEquivalentTo(5, "default attribute value is 5");
    }

    [Fact]
    public void Character_GetAttribute_ReturnsCorrectValue()
    {
        // Arrange
        var character = new Character("Test Character");
        character.SetAttribute(Attribute.Might, 8);

        // Act
        var value = character.GetAttribute(Attribute.Might);

        // Assert
        value.Should().Be(8);
    }

    [Fact]
    public void Character_SetAttribute_UpdatesValue()
    {
        // Arrange
        var character = new Character("Test Character");

        // Act
        character.SetAttribute(Attribute.Finesse, 7);

        // Assert
        character.GetAttribute(Attribute.Finesse).Should().Be(7);
    }

    [Fact]
    public void Character_AllAttributes_CanBeAccessed()
    {
        // Arrange
        var character = new Character("Test Character");

        // Act & Assert
        character.GetAttribute(Attribute.Sturdiness).Should().Be(5);
        character.GetAttribute(Attribute.Might).Should().Be(5);
        character.GetAttribute(Attribute.Wits).Should().Be(5);
        character.GetAttribute(Attribute.Will).Should().Be(5);
        character.GetAttribute(Attribute.Finesse).Should().Be(5);
    }

    [Fact]
    public void Character_AllAttributes_CanBeModified()
    {
        // Arrange
        var character = new Character("Test Character");

        // Act
        character.SetAttribute(Attribute.Sturdiness, 6);
        character.SetAttribute(Attribute.Might, 7);
        character.SetAttribute(Attribute.Wits, 8);
        character.SetAttribute(Attribute.Will, 9);
        character.SetAttribute(Attribute.Finesse, 10);

        // Assert
        character.GetAttribute(Attribute.Sturdiness).Should().Be(6);
        character.GetAttribute(Attribute.Might).Should().Be(7);
        character.GetAttribute(Attribute.Wits).Should().Be(8);
        character.GetAttribute(Attribute.Will).Should().Be(9);
        character.GetAttribute(Attribute.Finesse).Should().Be(10);
    }

    [Fact]
    public void Character_Attributes_ContainsAllFiveAttributes()
    {
        // Arrange & Act
        var character = new Character("Test Character");

        // Assert
        character.Attributes.Should().ContainKey(Attribute.Sturdiness);
        character.Attributes.Should().ContainKey(Attribute.Might);
        character.Attributes.Should().ContainKey(Attribute.Wits);
        character.Attributes.Should().ContainKey(Attribute.Will);
        character.Attributes.Should().ContainKey(Attribute.Finesse);
    }

    [Fact]
    public void Character_Id_IsImmutable()
    {
        // Arrange
        var character = new Character("Test Character");
        var originalId = character.Id;

        // Act - Cannot directly set Id due to init-only getter
        // Just verify it doesn't change on access
        var secondAccess = character.Id;

        // Assert
        secondAccess.Should().Be(originalId);
    }
}
