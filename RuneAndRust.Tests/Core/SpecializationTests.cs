using FluentAssertions;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using Xunit;

namespace RuneAndRust.Tests.Core;

/// <summary>
/// Tests for the Specialization entity.
/// Validates specialization creation, defaults, and relationships.
/// </summary>
/// <remarks>See: v0.4.1a for specialization system implementation.</remarks>
public class SpecializationTests
{
    [Fact]
    public void Constructor_InitializesDefaults()
    {
        // Arrange & Act
        var spec = new Specialization();

        // Assert
        spec.Id.Should().NotBeEmpty();
        spec.Name.Should().BeEmpty();
        spec.Description.Should().BeEmpty();
        spec.Nodes.Should().NotBeNull();
        spec.Nodes.Should().BeEmpty();
        spec.RequiredLevel.Should().Be(1);
        spec.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Id_GeneratesUniqueValues()
    {
        // Arrange & Act
        var spec1 = new Specialization();
        var spec2 = new Specialization();

        // Assert
        spec1.Id.Should().NotBe(spec2.Id);
    }

    [Fact]
    public void RequiredArchetype_EnforcesType()
    {
        // Arrange
        var spec = new Specialization
        {
            RequiredArchetype = ArchetypeType.Warrior
        };

        // Assert
        spec.RequiredArchetype.Should().Be(ArchetypeType.Warrior);
    }

    [Fact]
    public void RequiredLevel_DefaultsToOne()
    {
        // Arrange & Act
        var spec = new Specialization();

        // Assert
        spec.RequiredLevel.Should().Be(1);
    }

    [Fact]
    public void Nodes_EmptyByDefault()
    {
        // Arrange & Act
        var spec = new Specialization();

        // Assert
        spec.Nodes.Should().BeEmpty();
    }

    [Fact]
    public void Type_CanBeSet()
    {
        // Arrange
        var spec = new Specialization
        {
            Type = SpecializationType.Berserkr
        };

        // Assert
        spec.Type.Should().Be(SpecializationType.Berserkr);
    }

    [Fact]
    public void Description_CanBeEmpty()
    {
        // Arrange & Act
        var spec = new Specialization { Description = string.Empty };

        // Assert
        spec.Description.Should().BeEmpty();
    }

    [Fact]
    public void CreatedAt_SetsOnConstruction()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var spec = new Specialization();

        // Assert
        spec.CreatedAt.Should().BeOnOrAfter(before);
        spec.CreatedAt.Should().BeOnOrBefore(DateTime.UtcNow);
    }

    [Fact]
    public void Nodes_CanAddMultiple()
    {
        // Arrange
        var spec = new Specialization();
        var node1 = new SpecializationNode { Tier = 1 };
        var node2 = new SpecializationNode { Tier = 2 };

        // Act
        spec.Nodes.Add(node1);
        spec.Nodes.Add(node2);

        // Assert
        spec.Nodes.Should().HaveCount(2);
        spec.Nodes.Should().Contain(node1);
        spec.Nodes.Should().Contain(node2);
    }

    [Fact]
    public void FullSpecialization_HasAllProperties()
    {
        // Arrange & Act
        var spec = new Specialization
        {
            Type = SpecializationType.Skald,
            Name = "Skald",
            Description = "A voice wielder",
            RequiredArchetype = ArchetypeType.Skirmisher,
            RequiredLevel = 3
        };

        // Assert
        spec.Type.Should().Be(SpecializationType.Skald);
        spec.Name.Should().Be("Skald");
        spec.Description.Should().Be("A voice wielder");
        spec.RequiredArchetype.Should().Be(ArchetypeType.Skirmisher);
        spec.RequiredLevel.Should().Be(3);
    }
}
