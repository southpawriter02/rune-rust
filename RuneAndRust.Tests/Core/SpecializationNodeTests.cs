using FluentAssertions;
using RuneAndRust.Core.Entities;
using Xunit;

namespace RuneAndRust.Tests.Core;

/// <summary>
/// Tests for the SpecializationNode entity.
/// Validates node creation, tier logic, and parent relationships.
/// </summary>
/// <remarks>See: v0.4.1a for specialization system implementation.</remarks>
public class SpecializationNodeTests
{
    [Fact]
    public void Constructor_InitializesDefaults()
    {
        // Arrange & Act
        var node = new SpecializationNode();

        // Assert
        node.Id.Should().NotBeEmpty();
        node.Tier.Should().Be(1);
        node.CostPP.Should().Be(1);
        node.PositionX.Should().Be(0);
        node.PositionY.Should().Be(0);
        node.ParentNodeIds.Should().NotBeNull();
        node.ParentNodeIds.Should().BeEmpty();
        node.DisplayName.Should().BeNull();
    }

    [Fact]
    public void Id_GeneratesUniqueValues()
    {
        // Arrange & Act
        var node1 = new SpecializationNode();
        var node2 = new SpecializationNode();

        // Assert
        node1.Id.Should().NotBe(node2.Id);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void Tier_CanBeSetToValidValues(int tier)
    {
        // Arrange & Act
        var node = new SpecializationNode { Tier = tier };

        // Assert
        node.Tier.Should().Be(tier);
    }

    [Fact]
    public void ParentNodeIds_EmptyForTier1()
    {
        // Arrange & Act
        var node = new SpecializationNode
        {
            Tier = 1,
            ParentNodeIds = new List<Guid>()
        };

        // Assert
        node.ParentNodeIds.Should().BeEmpty();
        node.Tier.Should().Be(1);
    }

    [Fact]
    public void ParentNodeIds_CanHaveMultipleParents()
    {
        // Arrange
        var parent1 = Guid.NewGuid();
        var parent2 = Guid.NewGuid();

        // Act
        var node = new SpecializationNode
        {
            Tier = 3,
            ParentNodeIds = new List<Guid> { parent1, parent2 }
        };

        // Assert
        node.ParentNodeIds.Should().HaveCount(2);
        node.ParentNodeIds.Should().Contain(parent1);
        node.ParentNodeIds.Should().Contain(parent2);
    }

    [Fact]
    public void CostPP_DefaultsToOne()
    {
        // Arrange & Act
        var node = new SpecializationNode();

        // Assert
        node.CostPP.Should().Be(1);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(5)]
    public void CostPP_CanBeSetToPositiveValues(int cost)
    {
        // Arrange & Act
        var node = new SpecializationNode { CostPP = cost };

        // Assert
        node.CostPP.Should().Be(cost);
    }

    [Fact]
    public void IsCapstone_TrueForTier4()
    {
        // Arrange
        var node = new SpecializationNode { Tier = 4 };

        // Act
        var result = node.IsCapstone;

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void IsCapstone_FalseForLowerTiers(int tier)
    {
        // Arrange
        var node = new SpecializationNode { Tier = tier };

        // Act
        var result = node.IsCapstone;

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Position_DefaultsToZero()
    {
        // Arrange & Act
        var node = new SpecializationNode();

        // Assert
        node.PositionX.Should().Be(0);
        node.PositionY.Should().Be(0);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 0)]
    [InlineData(2, 1)]
    [InlineData(0, 3)]
    public void Position_CanBeSetToAnyValue(int x, int y)
    {
        // Arrange & Act
        var node = new SpecializationNode
        {
            PositionX = x,
            PositionY = y
        };

        // Assert
        node.PositionX.Should().Be(x);
        node.PositionY.Should().Be(y);
    }

    [Fact]
    public void DisplayName_OverridesAbilityName()
    {
        // Arrange
        var ability = new ActiveAbility { Name = "Original Name" };
        var node = new SpecializationNode
        {
            Ability = ability,
            DisplayName = "Custom Name"
        };

        // Act
        var displayName = node.GetDisplayName();

        // Assert
        displayName.Should().Be("Custom Name");
    }

    [Fact]
    public void GetDisplayName_FallsBackToAbilityName()
    {
        // Arrange
        var ability = new ActiveAbility { Name = "Ability Name" };
        var node = new SpecializationNode
        {
            Ability = ability,
            DisplayName = null
        };

        // Act
        var displayName = node.GetDisplayName();

        // Assert
        displayName.Should().Be("Ability Name");
    }

    [Fact]
    public void GetDisplayName_ReturnsUnknown_WhenNoAbility()
    {
        // Arrange
        var node = new SpecializationNode
        {
            Ability = null!,
            DisplayName = null
        };

        // Act
        var displayName = node.GetDisplayName();

        // Assert
        displayName.Should().Be("Unknown");
    }

    [Fact]
    public void AbilityId_CanBeSet()
    {
        // Arrange
        var abilityId = Guid.NewGuid();

        // Act
        var node = new SpecializationNode { AbilityId = abilityId };

        // Assert
        node.AbilityId.Should().Be(abilityId);
    }

    [Fact]
    public void SpecializationId_CanBeSet()
    {
        // Arrange
        var specId = Guid.NewGuid();

        // Act
        var node = new SpecializationNode { SpecializationId = specId };

        // Assert
        node.SpecializationId.Should().Be(specId);
    }
}
