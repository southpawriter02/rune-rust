using FluentAssertions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Entities;

[TestFixture]
public class InteractionDescriptorTests
{
    [Test]
    public void Constructor_WithValidParameters_CreatesDescriptor()
    {
        // Arrange & Act
        var descriptor = new InteractionDescriptor(
            InteractionCategory.MechanicalObject,
            "Lever",
            "Active",
            "A lever locked in the up position, mechanism still engaged");

        // Assert
        descriptor.Id.Should().NotBeEmpty();
        descriptor.Category.Should().Be(InteractionCategory.MechanicalObject);
        descriptor.SubCategory.Should().Be("Lever");
        descriptor.State.Should().Be("Active");
        descriptor.DescriptorText.Should().Be("A lever locked in the up position, mechanism still engaged");
        descriptor.BiomeAffinity.Should().BeNull();
        descriptor.Weight.Should().Be(1);
    }

    [Test]
    public void Constructor_WithBiomeAffinity_StoresBiome()
    {
        // Arrange & Act
        var descriptor = new InteractionDescriptor(
            InteractionCategory.Container,
            "SalvageCrate",
            "Intact",
            "A sealed crate",
            Biome.TheRoots);

        // Assert
        descriptor.BiomeAffinity.Should().Be(Biome.TheRoots);
    }

    [Test]
    public void Constructor_WithCustomWeight_StoresWeight()
    {
        // Arrange & Act
        var descriptor = new InteractionDescriptor(
            InteractionCategory.Discovery,
            "Secret",
            "Major",
            "A sealed vault",
            weight: 5);

        // Assert
        descriptor.Weight.Should().Be(5);
    }

    [Test]
    public void Constructor_WithEmptySubCategory_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => new InteractionDescriptor(
            InteractionCategory.MechanicalObject,
            "",
            "Active",
            "Some text");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("subCategory");
    }

    [Test]
    public void Constructor_WithEmptyState_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => new InteractionDescriptor(
            InteractionCategory.MechanicalObject,
            "Lever",
            "",
            "Some text");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("state");
    }

    [Test]
    public void Constructor_WithEmptyDescriptorText_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => new InteractionDescriptor(
            InteractionCategory.MechanicalObject,
            "Lever",
            "Active",
            "");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("descriptorText");
    }

    [Test]
    public void Constructor_WithZeroWeight_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => new InteractionDescriptor(
            InteractionCategory.MechanicalObject,
            "Lever",
            "Active",
            "Some text",
            weight: 0);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("weight");
    }

    [Test]
    public void CreateMechanicalObject_CreatesCorrectCategory()
    {
        // Arrange & Act
        var descriptor = InteractionDescriptor.CreateMechanicalObject(
            "Door",
            "Locked",
            "The door is sealed");

        // Assert
        descriptor.Category.Should().Be(InteractionCategory.MechanicalObject);
        descriptor.SubCategory.Should().Be("Door");
        descriptor.State.Should().Be("Locked");
    }

    [Test]
    public void CreateContainer_CreatesCorrectCategory()
    {
        // Arrange & Act
        var descriptor = InteractionDescriptor.CreateContainer(
            "Corpse",
            "Fresh",
            "Blood still wet");

        // Assert
        descriptor.Category.Should().Be(InteractionCategory.Container);
        descriptor.SubCategory.Should().Be("Corpse");
        descriptor.State.Should().Be("Fresh");
    }

    [Test]
    public void CreateWitsSuccess_CreatesCorrectCategory()
    {
        // Arrange & Act
        var descriptor = InteractionDescriptor.CreateWitsSuccess(
            "Critical",
            "You understand this object in ways its makers might not have intended");

        // Assert
        descriptor.Category.Should().Be(InteractionCategory.WitsSuccess);
        descriptor.SubCategory.Should().Be("WitsCheck");
        descriptor.State.Should().Be("Critical");
    }

    [Test]
    public void CreateWitsFailure_CreatesCorrectCategory()
    {
        // Arrange & Act
        var descriptor = InteractionDescriptor.CreateWitsFailure(
            "CriticalFail",
            "You have no idea what you're looking at");

        // Assert
        descriptor.Category.Should().Be(InteractionCategory.WitsFailure);
        descriptor.SubCategory.Should().Be("WitsCheck");
        descriptor.State.Should().Be("CriticalFail");
    }

    [Test]
    public void CreateDiscovery_CreatesCorrectCategory()
    {
        // Arrange & Act
        var descriptor = InteractionDescriptor.CreateDiscovery(
            "Secret",
            "Major",
            "A sealed vault");

        // Assert
        descriptor.Category.Should().Be(InteractionCategory.Discovery);
        descriptor.SubCategory.Should().Be("Secret");
        descriptor.State.Should().Be("Major");
    }

    [Test]
    public void CreateContainerInteraction_CreatesCorrectCategory()
    {
        // Arrange & Act
        var descriptor = InteractionDescriptor.CreateContainerInteraction(
            "Opening",
            "Unlock",
            "The lock yields");

        // Assert
        descriptor.Category.Should().Be(InteractionCategory.ContainerInteraction);
        descriptor.SubCategory.Should().Be("Opening");
        descriptor.State.Should().Be("Unlock");
    }

    [Test]
    public void CreateSkillSpecific_CreatesCorrectCategory()
    {
        // Arrange & Act
        var descriptor = InteractionDescriptor.CreateSkillSpecific(
            "Trade",
            "Bodging",
            "You assess the salvage");

        // Assert
        descriptor.Category.Should().Be(InteractionCategory.SkillSpecific);
        descriptor.SubCategory.Should().Be("Trade");
        descriptor.State.Should().Be("Bodging");
    }

    [Test]
    public void CreateEnvironmental_CreatesCorrectCategory()
    {
        // Arrange & Act
        var descriptor = InteractionDescriptor.CreateEnvironmental(
            "Traversal",
            "Easy",
            "You pull yourself up. Simple");

        // Assert
        descriptor.Category.Should().Be(InteractionCategory.Environmental);
        descriptor.SubCategory.Should().Be("Traversal");
        descriptor.State.Should().Be("Easy");
    }

    [Test]
    public void ToString_ReturnsExpectedFormat()
    {
        // Arrange
        var descriptor = new InteractionDescriptor(
            InteractionCategory.MechanicalObject,
            "Lever",
            "Active",
            "A lever");

        // Act
        var result = descriptor.ToString();

        // Assert
        result.Should().Be("MechanicalObject/Lever/Active");
    }
}
