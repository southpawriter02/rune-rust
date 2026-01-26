using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for <see cref="ActiveSearchResult"/> value object.
/// </summary>
[TestFixture]
public class ActiveSearchResultTests
{
    [Test]
    public void TotalDiscoveries_SumsAllDiscoveries()
    {
        // Arrange
        var result = new ActiveSearchResult
        {
            RoomId = "room-1",
            CharacterId = "char-1",
            WitsCheckResult = 4,
            ItemsFound = new List<FoundItem>
            {
                FoundItem.Create("item-1", "Key", "under desk", 2, "You find a key.")
            },
            HiddenElementsRevealed = new List<string> { "secret-door-1", "compartment-1" },
            ContainersSearched = new List<string> { "chest-1" },
            TimeSpent = 10,
            HintsDiscovered = new List<string> { "hint-1" }
        };

        // Assert
        result.TotalDiscoveries.Should().Be(4); // 1 item + 2 elements + 1 hint
        result.HasDiscoveries.Should().BeTrue();
    }

    [Test]
    public void Empty_CreatesEmptyResult()
    {
        // Arrange & Act
        var result = ActiveSearchResult.Empty("room-1", "char-1", 3, 10);

        // Assert
        result.RoomId.Should().Be("room-1");
        result.CharacterId.Should().Be("char-1");
        result.WitsCheckResult.Should().Be(3);
        result.TimeSpent.Should().Be(10);
        result.HasDiscoveries.Should().BeFalse();
        result.TotalDiscoveries.Should().Be(0);
    }

    [Test]
    public void HasItems_WithItems_ReturnsTrue()
    {
        // Arrange
        var result = new ActiveSearchResult
        {
            RoomId = "room-1",
            CharacterId = "char-1",
            WitsCheckResult = 4,
            ItemsFound = new List<FoundItem>
            {
                FoundItem.Create("item-1", "Key", "under desk", 2, "Found it.")
            },
            HiddenElementsRevealed = Array.Empty<string>(),
            ContainersSearched = Array.Empty<string>(),
            TimeSpent = 10,
            HintsDiscovered = Array.Empty<string>()
        };

        // Assert
        result.HasItems.Should().BeTrue();
        result.HasHiddenElements.Should().BeFalse();
    }
}

/// <summary>
/// Unit tests for <see cref="FoundItem"/> value object.
/// </summary>
[TestFixture]
public class FoundItemTests
{
    [Test]
    public void Create_SetsAllProperties()
    {
        // Arrange & Act
        var item = FoundItem.Create(
            "item-123",
            "Ancient Keycard",
            "wedged behind shelving",
            3,
            "You notice something clatters to the floor...",
            "container-1",
            wasHidden: true);

        // Assert
        item.ItemId.Should().Be("item-123");
        item.ItemName.Should().Be("Ancient Keycard");
        item.Location.Should().Be("wedged behind shelving");
        item.DiscoveryDc.Should().Be(3);
        item.ContainerId.Should().Be("container-1");
        item.WasHidden.Should().BeTrue();
        item.WasInContainer.Should().BeTrue();
    }

    [Test]
    public void WasInContainer_WithoutContainer_ReturnsFalse()
    {
        // Arrange
        var item = FoundItem.Create("id", "Name", "location", 2, "text");

        // Assert
        item.WasInContainer.Should().BeFalse();
    }
}

/// <summary>
/// Unit tests for <see cref="SearchParameters"/> value object.
/// </summary>
[TestFixture]
public class SearchParametersTests
{
    [Test]
    public void ForRoom_CreatesRoomSearchParameters()
    {
        // Act
        var parameters = SearchParameters.ForRoom("char-1", "room-1");

        // Assert
        parameters.CharacterId.Should().Be("char-1");
        parameters.TargetType.Should().Be(SearchTarget.Room);
        parameters.TargetId.Should().Be("room-1");
        parameters.IncludeContainers.Should().BeTrue();
        parameters.ApplyActiveBonus.Should().BeTrue();
        parameters.IsRoomSearch.Should().BeTrue();
    }

    [Test]
    public void ForContainer_CreatesContainerSearchParameters()
    {
        // Act
        var parameters = SearchParameters.ForContainer("char-1", "chest-1");

        // Assert
        parameters.TargetType.Should().Be(SearchTarget.Container);
        parameters.TargetId.Should().Be("chest-1");
        parameters.IncludeContainers.Should().BeFalse();
        parameters.IsContainerSearch.Should().BeTrue();
    }
}
