using FluentAssertions;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for <see cref="CripplingShotResult"/> value object.
/// Tests cover movement reduction calculations, duration constants, and description generation.
/// </summary>
/// <remarks>
/// <para>Crippling Shot is a Tier 3 active ability for the Veiðimaðr (Hunter) specialization.
/// It halves a marked quarry's movement speed for 2 turns by consuming a Quarry Mark.</para>
/// <para>Key behaviors tested:</para>
/// <list type="bullet">
/// <item><see cref="CripplingShotResult.ReducedMovementSpeed"/>: OriginalMovementSpeed / 2 (integer division)</item>
/// <item><see cref="CripplingShotResult.DurationTurns"/>: Always 2</item>
/// <item><see cref="CripplingShotResult.GetDescription"/>: Narrative text for combat logging</item>
/// <item><see cref="CripplingShotResult.GetDurationText"/>: Duration display text</item>
/// <item><see cref="CripplingShotResult.GetStatusMessage"/>: Concise combat log message</item>
/// </list>
/// <para>Introduced in v0.20.7c. Coherent path — zero Corruption risk.</para>
/// </remarks>
[TestFixture]
public class CripplingShotResultTests
{
    // ===== ReducedMovementSpeed Tests =====

    [Test]
    public void ReducedMovementSpeed_EvenNumber_HalvesExactly()
    {
        // Arrange
        var result = new CripplingShotResult
        {
            HunterId = Guid.NewGuid(),
            TargetId = Guid.NewGuid(),
            TargetName = "Troll Scout",
            OriginalMovementSpeed = 6,
            MarkConsumed = true
        };

        // Act & Assert
        result.ReducedMovementSpeed.Should().Be(3,
            "6 / 2 = 3 — even numbers halve exactly");
    }

    [Test]
    public void ReducedMovementSpeed_OddNumber_IntegerDivisionRoundsDown()
    {
        // Arrange
        var result = new CripplingShotResult
        {
            HunterId = Guid.NewGuid(),
            TargetId = Guid.NewGuid(),
            TargetName = "Corrupted Wolf",
            OriginalMovementSpeed = 7,
            MarkConsumed = true
        };

        // Act & Assert
        result.ReducedMovementSpeed.Should().Be(3,
            "7 / 2 = 3 via integer division — odd values round down");
    }

    [Test]
    public void ReducedMovementSpeed_SpeedOfOne_BecomesZero()
    {
        // Arrange
        var result = new CripplingShotResult
        {
            HunterId = Guid.NewGuid(),
            TargetId = Guid.NewGuid(),
            TargetName = "Wounded Draugr",
            OriginalMovementSpeed = 1,
            MarkConsumed = true
        };

        // Act & Assert
        result.ReducedMovementSpeed.Should().Be(0,
            "1 / 2 = 0 — effectively immobilized");
    }

    // ===== DurationTurns Tests =====

    [Test]
    public void DurationTurns_AlwaysTwo()
    {
        // Arrange
        var result = new CripplingShotResult
        {
            HunterId = Guid.NewGuid(),
            TargetId = Guid.NewGuid(),
            TargetName = "Any Target",
            OriginalMovementSpeed = 6,
            MarkConsumed = true
        };

        // Act & Assert
        result.DurationTurns.Should().Be(2,
            "Crippling Shot always lasts exactly 2 turns — cannot be extended or stacked");
    }

    // ===== GetDescription Test =====

    [Test]
    public void GetDescription_ContainsMovementReduction()
    {
        // Arrange
        var result = new CripplingShotResult
        {
            HunterId = Guid.NewGuid(),
            TargetId = Guid.NewGuid(),
            TargetName = "Frost Giant",
            OriginalMovementSpeed = 8,
            MarkConsumed = true
        };

        // Act
        var description = result.GetDescription();

        // Assert
        description.Should().Contain("Crippling Shot",
            "description should identify the ability");
        description.Should().Contain("Frost Giant");
        description.Should().Contain("8");
        description.Should().Contain("4",
            "description should show the reduced movement speed (8/2=4)");
        description.Should().Contain("2 turns");
        description.Should().Contain("Quarry Mark consumed");
    }

    // ===== GetDurationText Test =====

    [Test]
    public void GetDurationText_ContainsTurns()
    {
        // Arrange
        var result = new CripplingShotResult
        {
            HunterId = Guid.NewGuid(),
            TargetId = Guid.NewGuid(),
            TargetName = "Target",
            OriginalMovementSpeed = 6,
            MarkConsumed = true
        };

        // Act
        var durationText = result.GetDurationText();

        // Assert
        durationText.Should().Contain("2 turns",
            "duration text should display the number of turns remaining");
    }

    // ===== GetStatusMessage Test =====

    [Test]
    public void GetStatusMessage_ContainsCripplingShot()
    {
        // Arrange
        var result = new CripplingShotResult
        {
            HunterId = Guid.NewGuid(),
            TargetId = Guid.NewGuid(),
            TargetName = "Shadow Stalker",
            OriginalMovementSpeed = 10,
            MarkConsumed = true
        };

        // Act
        var message = result.GetStatusMessage();

        // Assert
        message.Should().Contain("CRIPPLING SHOT",
            "status message should include the ability name in uppercase for combat log visibility");
        message.Should().Contain("Shadow Stalker");
        message.Should().Contain("10");
        message.Should().Contain("5",
            "status message should show reduced speed (10/2=5)");
    }

    // ===== MarkConsumed Preservation Test =====

    [Test]
    public void MarkConsumed_PreservesValue()
    {
        // Arrange & Act
        var result = new CripplingShotResult
        {
            HunterId = Guid.NewGuid(),
            TargetId = Guid.NewGuid(),
            TargetName = "Target",
            OriginalMovementSpeed = 6,
            MarkConsumed = true
        };

        // Assert
        result.MarkConsumed.Should().BeTrue(
            "MarkConsumed should preserve the init value — always true for successful execution");
    }
}
