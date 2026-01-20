using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Tests for SlotFillResult value object.
/// </summary>
[TestFixture]
public class SlotFillResultTests
{
    [Test]
    public void Success_WithFilledSlots_ReturnsSuccessResult()
    {
        // Arrange
        var filledSlots = new Dictionary<string, object>
        {
            ["adjective"] = "dark",
            ["material"] = "stone"
        };

        // Act
        var result = SlotFillResult.Success(filledSlots, ["optional_01"]);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.FilledSlots.Should().HaveCount(2);
        result.UnfilledSlots.Should().Contain("optional_01");
        result.FailedSlots.Should().BeEmpty();
    }

    [Test]
    public void Failure_WithErrors_ReturnsFailureResult()
    {
        // Arrange
        var failedSlots = new List<string> { "required_01" };
        var errors = new Dictionary<string, string>
        {
            ["required_01"] = "No content available"
        };

        // Act
        var result = SlotFillResult.Failure(failedSlots, errors);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.FailedSlots.Should().Contain("required_01");
        result.Errors.Should().ContainKey("required_01");
    }

    [Test]
    public void GetDescriptionContent_WithValidSlot_ReturnsContent()
    {
        // Arrange
        var filledSlots = new Dictionary<string, object>
        {
            ["adjective"] = "dark"
        };
        var result = SlotFillResult.Success(filledSlots);

        // Act & Assert
        result.GetDescriptionContent("adjective").Should().Be("dark");
    }

    [Test]
    public void GetDescriptionContent_WithMissingSlot_ReturnsEmptyString()
    {
        // Arrange
        var result = SlotFillResult.Empty;

        // Act & Assert
        result.GetDescriptionContent("missing").Should().BeEmpty();
    }

    [Test]
    public void HasContent_WithFilledSlot_ReturnsTrue()
    {
        // Arrange
        var filledSlots = new Dictionary<string, object> { ["slot_01"] = "content" };
        var result = SlotFillResult.Success(filledSlots);

        // Act & Assert
        result.HasContent("slot_01").Should().BeTrue();
        result.HasContent("missing").Should().BeFalse();
    }

    [Test]
    public void Empty_ReturnsEmptySuccessResult()
    {
        // Act
        var result = SlotFillResult.Empty;

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.FilledSlots.Should().BeEmpty();
        result.FailedSlots.Should().BeEmpty();
    }
}
