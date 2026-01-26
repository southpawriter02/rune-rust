using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for room entry perception value objects.
/// </summary>
[TestFixture]
public class RoomEntryPerceptionTests
{
    [Test]
    public void GatedElement_Create_SetsAllProperties()
    {
        // Act
        var element = GatedElement.Create(
            "elem-1",
            12,
            "You notice faint scratch marks on the wall.",
            "environmental",
            50);

        // Assert
        element.ElementId.Should().Be("elem-1");
        element.RequiredDc.Should().Be(12);
        element.DescriptionFragment.Should().Contain("scratch marks");
        element.Category.Should().Be("environmental");
        element.Priority.Should().Be(50);
    }

    [Test]
    public void PerceptionGatedDescription_EvaluateForPerception_RevealsCorrectly()
    {
        // Arrange
        var elements = new[]
        {
            GatedElement.Create("e1", 8, "Low DC element"),
            GatedElement.Create("e2", 12, "Medium DC element"),
            GatedElement.Create("e3", 16, "High DC element")
        };
        var description = PerceptionGatedDescription.Create(
            "room-1", "Base description.", elements);

        // Act
        var evaluated = description.EvaluateForPerception(12);

        // Assert
        evaluated.RevealedCount.Should().Be(2);
        evaluated.GatedElements[0].IsRevealed.Should().BeTrue();
        evaluated.GatedElements[1].IsRevealed.Should().BeTrue();
        evaluated.GatedElements[2].IsRevealed.Should().BeFalse();
    }

    [Test]
    public void RoomEntryContext_Create_SetsRequiredProperties()
    {
        // Act
        var context = RoomEntryContext.Create(
            "player-1", "room-1", 14, "forest", true);

        // Assert
        context.CharacterId.Should().Be("player-1");
        context.RoomId.Should().Be("room-1");
        context.PassivePerceptionValue.Should().Be(14);
        context.Biome.Should().Be("forest");
        context.IsFirstVisit.Should().BeTrue();
    }

    [Test]
    public void RevealedElement_ExcessPerception_CalculatesCorrectly()
    {
        // Arrange
        var element = RevealedElement.Create(
            "trap-1", "Trap", 12, 15, "You spot a tripwire.");

        // Assert
        element.ExcessPerception.Should().Be(3);
    }

    [Test]
    public void RoomEntryPerceptionResult_HasNewDiscoveries_ReturnsCorrectly()
    {
        // Arrange
        var emptyResult = RoomEntryPerceptionResult.Empty(
            "room-1", "player-1", "Base.", 12);

        // Assert
        emptyResult.HasNewDiscoveries.Should().BeFalse();
        emptyResult.TotalDiscoveries.Should().Be(0);
    }

    [Test]
    public void AutoDetectedElement_Create_SetsAbilityInfo()
    {
        // Act
        var element = AutoDetectedElement.Create(
            "trap-1", "Trap",
            "sixth-sense", "[Sixth Sense]",
            8, "Your instincts warn you of danger.");

        // Assert
        element.DetectingAbilityId.Should().Be("sixth-sense");
        element.DetectingAbilityName.Should().Be("[Sixth Sense]");
        element.DistanceFeet.Should().Be(8);
    }
}
