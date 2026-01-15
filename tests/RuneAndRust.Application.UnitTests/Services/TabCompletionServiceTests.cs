using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Application.DTOs;
using RuneAndRust.Application.Services;
using RuneAndRust.Application.Services.Completion;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for <see cref="TabCompletionService"/>.
/// </summary>
[TestFixture]
public class TabCompletionServiceTests
{
    private TabCompletionService _service = null!;

    [SetUp]
    public void Setup()
    {
        _service = new TabCompletionService();
    }

    #region GetCompletions Tests

    [Test]
    public void GetCompletions_WithMatchingSource_ReturnsMatches()
    {
        // Arrange
        var commands = new[] { "attack", "armor", "abilities" };
        _service.RegisterSource(new CommandCompletionSource(commands));
        var context = CompletionContext.FromInput("a");

        // Act
        var result = _service.GetCompletions("a", context);

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain("attack");
    }

    [Test]
    public void GetCompletions_NoMatch_ReturnsEmpty()
    {
        // Arrange
        var commands = new[] { "attack", "armor" };
        _service.RegisterSource(new CommandCompletionSource(commands));
        var context = CompletionContext.FromInput("z");

        // Act
        var result = _service.GetCompletions("z", context);

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public void GetCompletions_CaseInsensitive_MatchesBothCases()
    {
        // Arrange
        var commands = new[] { "Attack", "ARMOR" };
        _service.RegisterSource(new CommandCompletionSource(commands));
        var context = CompletionContext.FromInput("a");

        // Act
        var result = _service.GetCompletions("a", context);

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region GetBestCompletion Tests

    [Test]
    public void GetBestCompletion_SingleMatch_ReturnsMatch()
    {
        // Arrange
        var commands = new[] { "attack", "armor" };
        _service.RegisterSource(new CommandCompletionSource(commands));
        var context = CompletionContext.FromInput("att");

        // Act
        var result = _service.GetBestCompletion("att", context);

        // Assert
        result.Should().Be("attack");
    }

    [Test]
    public void GetBestCompletion_MultipleMatches_ReturnsNull()
    {
        // Arrange
        var commands = new[] { "attack", "armor" };
        _service.RegisterSource(new CommandCompletionSource(commands));
        var context = CompletionContext.FromInput("a");

        // Act
        var result = _service.GetBestCompletion("a", context);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region Complete Tests

    [Test]
    public void Complete_SingleMatch_ReplacesWord()
    {
        // Arrange
        var commands = new[] { "attack", "armor" };
        _service.RegisterSource(new CommandCompletionSource(commands));
        var context = CompletionContext.FromInput("att");

        // Act
        var result = _service.Complete("att", context);

        // Assert
        result.Should().Be("attack");
    }

    #endregion

    #region Source Management Tests

    [Test]
    public void RegisterSource_AddsSourcesToCollection()
    {
        // Arrange
        var source = new CommandCompletionSource(new[] { "test" });

        // Act
        _service.RegisterSource(source);

        // Assert
        _service.Sources.Should().Contain(source);
    }

    [Test]
    public void UnregisterSource_RemovesFromCollection()
    {
        // Arrange
        var source = new CommandCompletionSource(new[] { "test" });
        _service.RegisterSource(source);

        // Act
        _service.UnregisterSource(source);

        // Assert
        _service.Sources.Should().NotContain(source);
    }

    #endregion
}
