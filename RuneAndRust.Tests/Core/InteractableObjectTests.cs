using FluentAssertions;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using Xunit;

namespace RuneAndRust.Tests.Core;

/// <summary>
/// Tests for the InteractableObject entity.
/// Validates three-tier description system and container mechanics.
/// </summary>
public class InteractableObjectTests
{
    #region Identity Properties

    [Fact]
    public void InteractableObject_NewInstance_ShouldHaveUniqueId()
    {
        // Arrange & Act
        var obj1 = new InteractableObject();
        var obj2 = new InteractableObject();

        // Assert
        obj1.Id.Should().NotBeEmpty();
        obj2.Id.Should().NotBeEmpty();
        obj1.Id.Should().NotBe(obj2.Id);
    }

    [Fact]
    public void InteractableObject_DefaultRoomId_ShouldBeEmpty()
    {
        // Arrange & Act
        var obj = new InteractableObject();

        // Assert
        obj.RoomId.Should().Be(Guid.Empty);
    }

    [Fact]
    public void InteractableObject_DefaultName_ShouldBeEmpty()
    {
        // Arrange & Act
        var obj = new InteractableObject();

        // Assert
        obj.Name.Should().BeEmpty();
    }

    [Fact]
    public void InteractableObject_DefaultObjectType_ShouldBeFurniture()
    {
        // Arrange & Act
        var obj = new InteractableObject();

        // Assert
        obj.ObjectType.Should().Be(ObjectType.Furniture);
    }

    #endregion

    #region Description Layers

    [Fact]
    public void InteractableObject_DefaultDescription_ShouldBeEmpty()
    {
        // Arrange & Act
        var obj = new InteractableObject();

        // Assert
        obj.Description.Should().BeEmpty();
    }

    [Fact]
    public void InteractableObject_DetailedDescription_ShouldBeNullByDefault()
    {
        // Arrange & Act
        var obj = new InteractableObject();

        // Assert
        obj.DetailedDescription.Should().BeNull();
    }

    [Fact]
    public void InteractableObject_ExpertDescription_ShouldBeNullByDefault()
    {
        // Arrange & Act
        var obj = new InteractableObject();

        // Assert
        obj.ExpertDescription.Should().BeNull();
    }

    [Fact]
    public void InteractableObject_CanSetAllDescriptionLayers()
    {
        // Arrange
        var obj = new InteractableObject
        {
            Description = "A rusted chest.",
            DetailedDescription = "The hinges are corroded but functional.",
            ExpertDescription = "Pre-Glitch ranger insignia marks this as supply cache."
        };

        // Assert
        obj.Description.Should().Be("A rusted chest.");
        obj.DetailedDescription.Should().Be("The hinges are corroded but functional.");
        obj.ExpertDescription.Should().Be("Pre-Glitch ranger insignia marks this as supply cache.");
    }

    #endregion

    #region Container Properties

    [Fact]
    public void InteractableObject_DefaultIsContainer_ShouldBeFalse()
    {
        // Arrange & Act
        var obj = new InteractableObject();

        // Assert
        obj.IsContainer.Should().BeFalse();
    }

    [Fact]
    public void InteractableObject_DefaultIsOpen_ShouldBeFalse()
    {
        // Arrange & Act
        var obj = new InteractableObject();

        // Assert
        obj.IsOpen.Should().BeFalse();
    }

    [Fact]
    public void InteractableObject_DefaultIsLocked_ShouldBeFalse()
    {
        // Arrange & Act
        var obj = new InteractableObject();

        // Assert
        obj.IsLocked.Should().BeFalse();
    }

    [Fact]
    public void InteractableObject_DefaultLockDifficulty_ShouldBeZero()
    {
        // Arrange & Act
        var obj = new InteractableObject();

        // Assert
        obj.LockDifficulty.Should().Be(0);
    }

    [Fact]
    public void InteractableObject_Container_CanBeOpened()
    {
        // Arrange
        var obj = new InteractableObject { IsContainer = true, IsOpen = false };

        // Act
        obj.IsOpen = true;

        // Assert
        obj.IsOpen.Should().BeTrue();
    }

    [Fact]
    public void InteractableObject_Container_CanBeLocked()
    {
        // Arrange
        var obj = new InteractableObject
        {
            IsContainer = true,
            IsLocked = true,
            LockDifficulty = 2
        };

        // Assert
        obj.IsLocked.Should().BeTrue();
        obj.LockDifficulty.Should().Be(2);
    }

    #endregion

    #region Examination State

    [Fact]
    public void InteractableObject_DefaultHasBeenExamined_ShouldBeFalse()
    {
        // Arrange & Act
        var obj = new InteractableObject();

        // Assert
        obj.HasBeenExamined.Should().BeFalse();
    }

    [Fact]
    public void InteractableObject_DefaultHighestExaminationTier_ShouldBeZero()
    {
        // Arrange & Act
        var obj = new InteractableObject();

        // Assert
        obj.HighestExaminationTier.Should().Be(0);
    }

    [Fact]
    public void InteractableObject_ExaminationTier_CanBeSet()
    {
        // Arrange
        var obj = new InteractableObject();

        // Act
        obj.HasBeenExamined = true;
        obj.HighestExaminationTier = 2;

        // Assert
        obj.HasBeenExamined.Should().BeTrue();
        obj.HighestExaminationTier.Should().Be(2);
    }

    [Theory]
    [InlineData(0, "Base tier - no detailed info revealed")]
    [InlineData(1, "Detailed tier - 1+ net successes")]
    [InlineData(2, "Expert tier - 3+ net successes")]
    public void InteractableObject_ExaminationTier_AcceptsValidValues(int tier, string description)
    {
        // Arrange
        var obj = new InteractableObject();

        // Act
        obj.HighestExaminationTier = tier;

        // Assert
        obj.HighestExaminationTier.Should().Be(tier, description);
    }

    #endregion

    #region Metadata

    [Fact]
    public void InteractableObject_CreatedAt_ShouldBeSetOnCreation()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;

        // Act
        var obj = new InteractableObject();
        var afterCreation = DateTime.UtcNow;

        // Assert
        obj.CreatedAt.Should().BeOnOrAfter(beforeCreation);
        obj.CreatedAt.Should().BeOnOrBefore(afterCreation);
    }

    [Fact]
    public void InteractableObject_LastModified_ShouldBeSetOnCreation()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;

        // Act
        var obj = new InteractableObject();
        var afterCreation = DateTime.UtcNow;

        // Assert
        obj.LastModified.Should().BeOnOrAfter(beforeCreation);
        obj.LastModified.Should().BeOnOrBefore(afterCreation);
    }

    #endregion

    #region Complete Object Configuration

    [Fact]
    public void InteractableObject_CompleteConfiguration_ShouldSetAllProperties()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        var beforeCreation = DateTime.UtcNow;

        // Act
        var obj = new InteractableObject
        {
            RoomId = roomId,
            Name = "Rusted Chest",
            ObjectType = ObjectType.Container,
            Description = "A corroded metal chest sits against the wall.",
            DetailedDescription = "Despite the rust, the hinges still seem functional.",
            ExpertDescription = "Faint inscriptions suggest Pre-Glitch ranger origin.",
            IsContainer = true,
            IsOpen = false,
            IsLocked = true,
            LockDifficulty = 2,
            HasBeenExamined = false,
            HighestExaminationTier = 0
        };

        // Assert
        obj.Id.Should().NotBeEmpty();
        obj.RoomId.Should().Be(roomId);
        obj.Name.Should().Be("Rusted Chest");
        obj.ObjectType.Should().Be(ObjectType.Container);
        obj.Description.Should().StartWith("A corroded metal chest");
        obj.DetailedDescription.Should().Contain("hinges");
        obj.ExpertDescription.Should().Contain("Pre-Glitch");
        obj.IsContainer.Should().BeTrue();
        obj.IsOpen.Should().BeFalse();
        obj.IsLocked.Should().BeTrue();
        obj.LockDifficulty.Should().Be(2);
        obj.HasBeenExamined.Should().BeFalse();
        obj.HighestExaminationTier.Should().Be(0);
        obj.CreatedAt.Should().BeOnOrAfter(beforeCreation);
    }

    #endregion
}
