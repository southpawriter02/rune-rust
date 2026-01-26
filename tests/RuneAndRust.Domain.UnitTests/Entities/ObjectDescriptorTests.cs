using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Unit tests for <see cref="ObjectDescriptor"/> entity.
/// </summary>
[TestFixture]
public class ObjectDescriptorTests
{
    #region Create Tests

    [Test]
    public void Create_WithValidParameters_CreatesDescriptor()
    {
        // Arrange & Act
        var descriptor = ObjectDescriptor.Create(
            descriptorId: "locked-door-cursory-01",
            category: ObjectCategory.Door,
            objectType: "LockedDoor",
            layer: 1,
            descriptorText: "A heavy iron door.");

        // Assert
        descriptor.DescriptorId.Should().Be("locked-door-cursory-01");
        descriptor.ObjectCategory.Should().Be(ObjectCategory.Door);
        descriptor.ObjectType.Should().Be("LockedDoor");
        descriptor.Layer.Should().Be(1);
        descriptor.LayerName.Should().Be("Cursory");
        descriptor.RequiredDc.Should().Be(0);
        descriptor.DescriptorText.Should().Be("A heavy iron door.");
        descriptor.IsUniversal.Should().BeTrue();
        descriptor.Weight.Should().Be(1);
        descriptor.RevealsHint.Should().BeFalse();
        descriptor.RevealsSolutionId.Should().BeNull();
    }

    [Test]
    public void Create_Layer2_SetsRequiredDcTo12()
    {
        // Arrange & Act
        var descriptor = ObjectDescriptor.Create(
            "console-detailed-01",
            ObjectCategory.Machinery,
            "Console",
            layer: 2,
            "The console displays status information.");

        // Assert
        descriptor.Layer.Should().Be(2);
        descriptor.LayerName.Should().Be("Detailed");
        descriptor.RequiredDc.Should().Be(12);
    }

    [Test]
    public void Create_Layer3_SetsRequiredDcTo18()
    {
        // Arrange & Act
        var descriptor = ObjectDescriptor.Create(
            "console-expert-01",
            ObjectCategory.Machinery,
            "Console",
            layer: 3,
            "The console has override capabilities.",
            revealsHint: true,
            solutionId: "console-override");

        // Assert
        descriptor.Layer.Should().Be(3);
        descriptor.LayerName.Should().Be("Expert");
        descriptor.RequiredDc.Should().Be(18);
        descriptor.RevealsHint.Should().BeTrue();
        descriptor.RevealsSolutionId.Should().Be("console-override");
    }

    [Test]
    public void Create_WithBiomeAffinity_SetsAffinity()
    {
        // Arrange & Act
        var descriptor = ObjectDescriptor.Create(
            "console-detailed-muspelheim-01",
            ObjectCategory.Machinery,
            "Console",
            layer: 2,
            "Heat-scorched console.",
            biomeAffinity: "Muspelheim");

        // Assert
        descriptor.BiomeAffinity.Should().Be("Muspelheim");
        descriptor.IsUniversal.Should().BeFalse();
    }

    [Test]
    public void Create_NormalizesDescriptorIdToLowercase()
    {
        // Arrange & Act
        var descriptor = ObjectDescriptor.Create(
            "Locked-Door-Cursory-01",
            ObjectCategory.Door,
            "LockedDoor",
            1,
            "A door.");

        // Assert
        descriptor.DescriptorId.Should().Be("locked-door-cursory-01");
    }

    #endregion

    #region MatchesBiome Tests

    [Test]
    public void MatchesBiome_UniversalDescriptor_MatchesAllBiomes()
    {
        // Arrange
        var descriptor = ObjectDescriptor.Create(
            "door-01", ObjectCategory.Door, "LockedDoor", 1, "A door.");

        // Act & Assert
        descriptor.MatchesBiome("Muspelheim").Should().BeTrue();
        descriptor.MatchesBiome("The Roots").Should().BeTrue();
        descriptor.MatchesBiome(null).Should().BeTrue();
    }

    [Test]
    public void MatchesBiome_BiomeSpecificDescriptor_MatchesOnlyItsOwnBiome()
    {
        // Arrange
        var descriptor = ObjectDescriptor.Create(
            "console-muspelheim-01",
            ObjectCategory.Machinery,
            "Console",
            2,
            "Heat-scorched console.",
            biomeAffinity: "Muspelheim");

        // Act & Assert
        descriptor.MatchesBiome("Muspelheim").Should().BeTrue();
        descriptor.MatchesBiome("muspelheim").Should().BeTrue("case-insensitive");
        descriptor.MatchesBiome("The Roots").Should().BeFalse();
        descriptor.MatchesBiome(null).Should().BeFalse();
    }

    #endregion

    #region Validation Tests

    [Test]
    public void Create_NullDescriptorId_ThrowsArgumentException()
    {
        // Act
        var act = () => ObjectDescriptor.Create(
            null!, ObjectCategory.Door, "LockedDoor", 1, "A door.");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Create_InvalidLayer_ThrowsArgumentOutOfRangeException()
    {
        // Act
        var actLow = () => ObjectDescriptor.Create(
            "door-01", ObjectCategory.Door, "LockedDoor", 0, "A door.");
        var actHigh = () => ObjectDescriptor.Create(
            "door-01", ObjectCategory.Door, "LockedDoor", 4, "A door.");

        // Assert
        actLow.Should().Throw<ArgumentOutOfRangeException>();
        actHigh.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void Create_InvalidWeight_ThrowsArgumentOutOfRangeException()
    {
        // Act
        var act = () => ObjectDescriptor.Create(
            "door-01", ObjectCategory.Door, "LockedDoor", 1, "A door.",
            weight: 0);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    #endregion
}
