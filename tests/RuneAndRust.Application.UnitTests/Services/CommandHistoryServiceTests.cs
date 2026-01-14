using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Application.Services;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for <see cref="CommandHistoryService"/>.
/// </summary>
[TestFixture]
public class CommandHistoryServiceTests
{
    private CommandHistoryService _service = null!;

    [SetUp]
    public void Setup()
    {
        _service = new CommandHistoryService();
    }

    #region Add Tests

    [Test]
    public void Add_ValidCommand_IncreasesCount()
    {
        // Act
        _service.Add("attack goblin");

        // Assert
        _service.Count.Should().Be(1);
    }

    [Test]
    public void Add_EmptyCommand_NotAdded()
    {
        // Act
        _service.Add("");
        _service.Add("   ");

        // Assert
        _service.Count.Should().Be(0);
    }

    [Test]
    public void Add_DuplicateCommand_MovesToFront()
    {
        // Arrange
        _service.Add("first");
        _service.Add("second");
        _service.Add("first"); // Duplicate

        // Assert
        _service.Count.Should().Be(2);
        _service.GetPrevious().Should().Be("first");
    }

    [Test]
    public void Add_TrimsWhitespace()
    {
        // Act
        _service.Add("  attack  ");

        // Assert
        _service.GetPrevious().Should().Be("attack");
    }

    #endregion

    #region Navigation Tests

    [Test]
    public void GetPrevious_WithHistory_ReturnsOlderCommands()
    {
        // Arrange
        _service.Add("first");
        _service.Add("second");
        _service.Add("third");

        // Act & Assert
        _service.GetPrevious().Should().Be("third");
        _service.GetPrevious().Should().Be("second");
        _service.GetPrevious().Should().Be("first");
    }

    [Test]
    public void GetPrevious_AtOldest_StaysAtOldest()
    {
        // Arrange
        _service.Add("only");
        _service.GetPrevious(); // Move to "only"

        // Act
        var result = _service.GetPrevious();

        // Assert
        result.Should().Be("only");
    }

    [Test]
    public void GetPrevious_EmptyHistory_ReturnsNull()
    {
        // Act
        var result = _service.GetPrevious();

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void GetNext_NavigatesForward()
    {
        // Arrange
        _service.Add("first");
        _service.Add("second");
        _service.GetPrevious(); // "second"
        _service.GetPrevious(); // "first"

        // Act
        var result = _service.GetNext();

        // Assert
        result.Should().Be("second");
    }

    [Test]
    public void GetNext_AtNewest_ReturnsEmpty()
    {
        // Arrange
        _service.Add("command");
        _service.GetPrevious(); // "command"

        // Act
        var result = _service.GetNext();

        // Assert
        result.Should().Be(""); // Returns to new input
    }

    #endregion

    #region Input Preservation Tests

    [Test]
    public void SaveCurrentInput_RestoresWhenNavigatingBack()
    {
        // Arrange
        _service.Add("history");
        _service.SaveCurrentInput("partial");
        _service.GetPrevious(); // Move to history

        // Act
        var restored = _service.GetNext();

        // Assert
        restored.Should().Be("partial");
    }

    #endregion

    #region Property Tests

    [Test]
    public void IsNavigating_FalseInitially()
    {
        _service.IsNavigating.Should().BeFalse();
    }

    [Test]
    public void IsNavigating_TrueAfterGetPrevious()
    {
        // Arrange
        _service.Add("command");

        // Act
        _service.GetPrevious();

        // Assert
        _service.IsNavigating.Should().BeTrue();
    }

    [Test]
    public void MaxSize_ReturnsDefaultValue()
    {
        _service.MaxSize.Should().Be(100);
    }

    #endregion

    #region Clear Tests

    [Test]
    public void Clear_RemovesAllHistory()
    {
        // Arrange
        _service.Add("one");
        _service.Add("two");

        // Act
        _service.Clear();

        // Assert
        _service.Count.Should().Be(0);
    }

    #endregion

    #region GetAll Tests

    [Test]
    public void GetAll_ReturnsNewestFirst()
    {
        // Arrange
        _service.Add("first");
        _service.Add("second");

        // Act
        var all = _service.GetAll();

        // Assert
        all.Should().HaveCount(2);
        all[0].Should().Be("second");
        all[1].Should().Be("first");
    }

    #endregion
}
