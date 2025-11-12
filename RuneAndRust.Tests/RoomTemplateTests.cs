using NUnit.Framework;
using RuneAndRust.Core;

namespace RuneAndRust.Tests;

/// <summary>
/// Tests for RoomTemplate data model (v0.10)
/// </summary>
[TestFixture]
public class RoomTemplateTests
{
    #region Validation Tests

    [Test]
    public void IsValid_WithAllRequiredFields_ReturnsTrue()
    {
        // Arrange
        var template = new RoomTemplate
        {
            TemplateId = "test_corridor",
            Biome = "the_roots",
            Archetype = RoomArchetype.Corridor,
            NameTemplates = new List<string> { "The {Adjective} Corridor" },
            Adjectives = new List<string> { "Rusted" },
            DescriptionTemplates = new List<string> { "A {Adjective} passage. {Detail}." },
            Details = new List<string> { "Water drips from above" },
            ValidConnections = new List<RoomArchetype> { RoomArchetype.Chamber }
        };

        // Act
        bool result = template.IsValid();

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void IsValid_WithMissingTemplateId_ReturnsFalse()
    {
        // Arrange
        var template = new RoomTemplate
        {
            TemplateId = string.Empty,
            NameTemplates = new List<string> { "Test" },
            Adjectives = new List<string> { "Test" },
            DescriptionTemplates = new List<string> { "Test" },
            Details = new List<string> { "Test" },
            ValidConnections = new List<RoomArchetype> { RoomArchetype.Chamber }
        };

        // Act
        bool result = template.IsValid();

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void IsValid_WithEmptyNameTemplates_ReturnsFalse()
    {
        // Arrange
        var template = new RoomTemplate
        {
            TemplateId = "test",
            NameTemplates = new List<string>(), // Empty
            Adjectives = new List<string> { "Test" },
            DescriptionTemplates = new List<string> { "Test" },
            Details = new List<string> { "Test" },
            ValidConnections = new List<RoomArchetype> { RoomArchetype.Chamber }
        };

        // Act
        bool result = template.IsValid();

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void IsValid_WithEmptyValidConnections_ReturnsFalse()
    {
        // Arrange
        var template = new RoomTemplate
        {
            TemplateId = "test",
            NameTemplates = new List<string> { "Test" },
            Adjectives = new List<string> { "Test" },
            DescriptionTemplates = new List<string> { "Test" },
            Details = new List<string> { "Test" },
            ValidConnections = new List<RoomArchetype>() // Empty
        };

        // Act
        bool result = template.IsValid();

        // Assert
        Assert.That(result, Is.False);
    }

    #endregion

    #region Connection Tests

    [Test]
    public void CanConnectTo_WithValidArchetype_ReturnsTrue()
    {
        // Arrange
        var template = new RoomTemplate
        {
            ValidConnections = new List<RoomArchetype>
            {
                RoomArchetype.Chamber,
                RoomArchetype.Junction
            }
        };

        // Act
        bool result = template.CanConnectTo(RoomArchetype.Chamber);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void CanConnectTo_WithInvalidArchetype_ReturnsFalse()
    {
        // Arrange
        var template = new RoomTemplate
        {
            ValidConnections = new List<RoomArchetype>
            {
                RoomArchetype.Chamber,
                RoomArchetype.Junction
            }
        };

        // Act
        bool result = template.CanConnectTo(RoomArchetype.BossArena);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void CanConnectTo_WithMultipleValidArchetypes_ReturnsTrue()
    {
        // Arrange
        var template = new RoomTemplate
        {
            ValidConnections = new List<RoomArchetype>
            {
                RoomArchetype.Corridor,
                RoomArchetype.Chamber,
                RoomArchetype.Junction,
                RoomArchetype.SecretRoom
            }
        };

        // Act & Assert
        Assert.That(template.CanConnectTo(RoomArchetype.Corridor), Is.True);
        Assert.That(template.CanConnectTo(RoomArchetype.Chamber), Is.True);
        Assert.That(template.CanConnectTo(RoomArchetype.Junction), Is.True);
        Assert.That(template.CanConnectTo(RoomArchetype.SecretRoom), Is.True);
        Assert.That(template.CanConnectTo(RoomArchetype.BossArena), Is.False);
    }

    #endregion

    #region Connection Slots Tests

    [Test]
    public void GetAvailableConnectionSlots_ReturnsCorrectValue()
    {
        // Arrange
        var template = new RoomTemplate
        {
            MinConnectionPoints = 2,
            MaxConnectionPoints = 4
        };

        // Act
        int slots = template.GetAvailableConnectionSlots();

        // Assert
        Assert.That(slots, Is.EqualTo(2));
    }

    [Test]
    public void GetAvailableConnectionSlots_WithEqualMinMax_ReturnsZero()
    {
        // Arrange
        var template = new RoomTemplate
        {
            MinConnectionPoints = 3,
            MaxConnectionPoints = 3
        };

        // Act
        int slots = template.GetAvailableConnectionSlots();

        // Assert
        Assert.That(slots, Is.EqualTo(0));
    }

    #endregion

    #region Template Configuration Tests

    [Test]
    public void RoomTemplate_DefaultValues_SetCorrectly()
    {
        // Arrange & Act
        var template = new RoomTemplate();

        // Assert
        Assert.That(template.TemplateId, Is.EqualTo(string.Empty));
        Assert.That(template.Biome, Is.EqualTo("the_roots"));
        Assert.That(template.Size, Is.EqualTo(RoomSize.Medium));
        Assert.That(template.Archetype, Is.EqualTo(RoomArchetype.Chamber));
        Assert.That(template.MinConnectionPoints, Is.EqualTo(1));
        Assert.That(template.MaxConnectionPoints, Is.EqualTo(4));
        Assert.That(template.Difficulty, Is.EqualTo(RoomDifficulty.Easy));
    }

    [Test]
    public void RoomTemplate_EntryHallArchetype_ConfiguredCorrectly()
    {
        // Arrange
        var template = new RoomTemplate
        {
            TemplateId = "collapsed_entry_hall",
            Archetype = RoomArchetype.EntryHall,
            MinConnectionPoints = 1,
            MaxConnectionPoints = 2,
            ValidConnections = new List<RoomArchetype>
            {
                RoomArchetype.Corridor,
                RoomArchetype.Chamber
            }
        };

        // Assert
        Assert.That(template.Archetype, Is.EqualTo(RoomArchetype.EntryHall));
        Assert.That(template.CanConnectTo(RoomArchetype.Corridor), Is.True);
        Assert.That(template.CanConnectTo(RoomArchetype.Chamber), Is.True);
        Assert.That(template.CanConnectTo(RoomArchetype.Junction), Is.False);
    }

    [Test]
    public void RoomTemplate_JunctionArchetype_AllowsMultipleConnections()
    {
        // Arrange
        var template = new RoomTemplate
        {
            TemplateId = "shattered_junction",
            Archetype = RoomArchetype.Junction,
            MinConnectionPoints = 3,
            MaxConnectionPoints = 5,
            ValidConnections = new List<RoomArchetype>
            {
                RoomArchetype.Corridor,
                RoomArchetype.Chamber,
                RoomArchetype.SecretRoom
            }
        };

        // Assert
        Assert.That(template.MinConnectionPoints, Is.GreaterThanOrEqualTo(3));
        Assert.That(template.GetAvailableConnectionSlots(), Is.EqualTo(2));
    }

    #endregion
}
