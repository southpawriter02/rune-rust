using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Presentation.Gui.ViewModels;

namespace RuneAndRust.Application.UnitTests.Presentation.Gui.ViewModels;

/// <summary>
/// Unit tests for <see cref="RoomDisplayViewModel"/>.
/// </summary>
[TestFixture]
public class RoomDisplayViewModelTests
{
    private RoomDisplayViewModel _viewModel = null!;

    [SetUp]
    public void SetUp()
    {
        _viewModel = new RoomDisplayViewModel();
    }

    /// <summary>
    /// Verifies that the default constructor initializes sample data.
    /// </summary>
    [Test]
    public void Constructor_WithNoParams_HasSampleData()
    {
        // Assert
        _viewModel.RoomName.Should().Be("Dark Cave");
        _viewModel.HasAsciiArt.Should().BeTrue();
        _viewModel.Exits.Should().HaveCount(2);
    }

    /// <summary>
    /// Verifies that UpdateRoom updates the room name and description.
    /// </summary>
    [Test]
    public void UpdateRoom_WithValidData_UpdatesProperties()
    {
        // Act
        _viewModel.UpdateRoom("Forest Clearing", "A peaceful clearing in the forest.");

        // Assert
        _viewModel.RoomName.Should().Be("Forest Clearing");
        _viewModel.RoomDescription.Should().Be("A peaceful clearing in the forest.");
    }

    /// <summary>
    /// Verifies that UpdateRoom with ASCII art sets HasAsciiArt to true.
    /// </summary>
    [Test]
    public void UpdateRoom_WithAsciiArt_SetsHasAsciiArt()
    {
        // Arrange
        var artLines = new List<string> { "╔═══╗", "║   ║", "╚═══╝" };

        // Act
        _viewModel.UpdateRoom("Test Room", "A test room.", artLines);

        // Assert
        _viewModel.HasAsciiArt.Should().BeTrue();
        _viewModel.AsciiArt.Should().HaveCount(3);
    }

    /// <summary>
    /// Verifies that UpdateRoom with legend updates ArtLegendItems.
    /// </summary>
    [Test]
    public void UpdateRoom_WithLegend_UpdatesArtLegendItems()
    {
        // Arrange
        var legend = new Dictionary<char, string> { { '~', "water" }, { '#', "wall" } };
        var artLines = new List<string> { "~~~###" };

        // Act
        _viewModel.UpdateRoom("Test Room", "Description", artLines, legend);

        // Assert
        _viewModel.HasArtLegend.Should().BeTrue();
        _viewModel.ArtLegendItems.Should().HaveCount(2);
    }

    /// <summary>
    /// Verifies that UpdateExits replaces the exits collection.
    /// </summary>
    [Test]
    public void UpdateExits_WithNewExits_ReplacesExitCollection()
    {
        // Arrange
        var exits = new List<ExitViewModel>
        {
            new(Direction.North, false),
            new(Direction.South, true),
            new(Direction.West, false)
        };

        // Act
        _viewModel.UpdateExits(exits);

        // Assert
        _viewModel.Exits.Should().HaveCount(3);
        _viewModel.Exits.Should().Contain(e => e.Direction == Direction.South && e.IsLocked);
    }

    /// <summary>
    /// Verifies that UpdateVisibleEntities sets HasVisibleEntities correctly.
    /// </summary>
    [Test]
    public void UpdateVisibleEntities_WithEntities_SetsHasVisibleEntities()
    {
        // Arrange
        var monsters = new List<EntityViewModel>
        {
            new("Goblin", "G", true, "A green goblin")
        };
        var items = new List<ItemViewModel>
        {
            new("Gold Coins", "$", false, "Shiny gold")
        };

        // Act
        _viewModel.UpdateVisibleEntities(monsters, items);

        // Assert
        _viewModel.HasVisibleEntities.Should().BeTrue();
        _viewModel.VisibleMonsters.Should().HaveCount(1);
        _viewModel.VisibleItems.Should().HaveCount(1);
    }

    /// <summary>
    /// Verifies that Clear resets all properties to default.
    /// </summary>
    [Test]
    public void Clear_ResetsAllPropertiesToDefault()
    {
        // Act
        _viewModel.Clear();

        // Assert
        _viewModel.RoomName.Should().Be("Unknown Location");
        _viewModel.HasAsciiArt.Should().BeFalse();
        _viewModel.Exits.Should().BeEmpty();
        _viewModel.HasVisibleEntities.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that MoveToExitCommand can execute.
    /// </summary>
    [Test]
    public void MoveToExitCommand_CanExecute_ReturnsTrue()
    {
        // Assert
        _viewModel.MoveToExitCommand.CanExecute(Direction.North).Should().BeTrue();
    }
}
