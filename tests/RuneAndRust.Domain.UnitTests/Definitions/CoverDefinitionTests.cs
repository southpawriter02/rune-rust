using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Definitions;

/// <summary>
/// Unit tests for the CoverDefinition class.
/// </summary>
[TestFixture]
public class CoverDefinitionTests
{
    [Test]
    public void Create_WithValidParameters_CreatesDefinition()
    {
        // Arrange & Act
        var def = CoverDefinition.Create(
            id: "test-cover",
            name: "Test Cover",
            coverType: CoverType.Partial,
            defenseBonus: 3,
            isDestructible: true,
            maxHitPoints: 15,
            blocksMovement: true,
            blocksLOS: false,
            displayChar: '◆',
            description: "A test cover");

        // Assert
        def.Id.Should().Be("test-cover");
        def.Name.Should().Be("Test Cover");
        def.CoverType.Should().Be(CoverType.Partial);
        def.DefenseBonus.Should().Be(3);
        def.IsDestructible.Should().BeTrue();
        def.MaxHitPoints.Should().Be(15);
        def.BlocksMovement.Should().BeTrue();
        def.BlocksLOS.Should().BeFalse();
        def.DisplayChar.Should().Be('◆');
        def.Description.Should().Be("A test cover");
    }

    [Test]
    public void Create_WithNullId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => CoverDefinition.Create(
            id: null!,
            name: "Test",
            coverType: CoverType.Partial);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Create_WithEmptyId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => CoverDefinition.Create(
            id: "",
            name: "Test",
            coverType: CoverType.Partial);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Create_NormalizesIdToLowercase()
    {
        // Arrange & Act
        var def = CoverDefinition.Create(
            id: "TEST-COVER",
            name: "Test",
            coverType: CoverType.Partial);

        // Assert
        def.Id.Should().Be("test-cover");
    }

    [Test]
    public void Create_WithDefaultValues_UsesExpectedDefaults()
    {
        // Arrange & Act
        var def = CoverDefinition.Create(
            id: "simple",
            name: "Simple Cover",
            coverType: CoverType.Partial);

        // Assert
        def.DefenseBonus.Should().Be(2);
        def.IsDestructible.Should().BeFalse();
        def.MaxHitPoints.Should().Be(10);
        def.BlocksMovement.Should().BeTrue();
        def.BlocksLOS.Should().BeFalse();
        def.DisplayChar.Should().Be('▪');
        def.Description.Should().BeEmpty();
    }

    [Test]
    public void ProvidesCover_ForPartialCover_ReturnsTrue()
    {
        // Arrange
        var def = CoverDefinition.Create("partial", "Partial", CoverType.Partial);

        // Assert
        def.ProvidesCover.Should().BeTrue();
    }

    [Test]
    public void ProvidesCover_ForFullCover_ReturnsTrue()
    {
        // Arrange
        var def = CoverDefinition.Create("full", "Full", CoverType.Full);

        // Assert
        def.ProvidesCover.Should().BeTrue();
    }

    [Test]
    public void ProvidesCover_ForNoCover_ReturnsFalse()
    {
        // Arrange
        var def = CoverDefinition.Create("none", "None", CoverType.None);

        // Assert
        def.ProvidesCover.Should().BeFalse();
    }
}
