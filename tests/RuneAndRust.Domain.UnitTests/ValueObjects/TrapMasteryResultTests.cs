using FluentAssertions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for <see cref="TrapMasteryResult"/> value object.
/// Tests static factory methods for both placement and detection modes.
/// </summary>
/// <remarks>
/// <para>TrapMasteryResult encapsulates the outcome of using the Trap Mastery ability,
/// which has two distinct modes: trap placement and enemy trap detection.</para>
/// <para>Key behaviors tested:</para>
/// <list type="bullet">
/// <item><c>CreatePlacementSuccess()</c> — builds success result with trap reference</item>
/// <item><c>CreatePlacementFailure()</c> — builds failure result with reason</item>
/// <item><c>CreateDetectionSuccess()</c> — builds success result with detection info</item>
/// <item><c>CreateDetectionFailure()</c> — builds failure result with roll details</item>
/// <item><c>GetDescription()</c> — narrative text for combat logging</item>
/// <item><c>WasSuccessful()</c> / <c>GetTrapCount()</c> — result accessors</item>
/// </list>
/// <para>Introduced in v0.20.7b. Coherent path — zero Corruption risk.</para>
/// </remarks>
[TestFixture]
public class TrapMasteryResultTests
{
    // ===== Placement Success Tests =====

    [Test]
    public void CreatePlacementSuccess_SetsProperties()
    {
        // Arrange
        var trap = TrapInstance.Create(Guid.NewGuid(), 5, 3, TrapType.Spike);

        // Act
        var result = TrapMasteryResult.CreatePlacementSuccess(trap, 5, 3);

        // Assert
        result.Type.Should().Be(TrapMasteryResult.ResultType.TrapPlaced);
        result.Success.Should().BeTrue();
        result.PlacedTrap.Should().Be(trap);
        result.LocationX.Should().Be(5);
        result.LocationY.Should().Be(3);
        result.Message.Should().Contain("spike");
        result.Message.Should().Contain("(5, 3)");
        result.WasSuccessful().Should().BeTrue();
        result.GetTrapCount().Should().Be(1);

        // Detection-specific fields should be at defaults
        result.PerceptionRoll.Should().BeNull();
        result.PerceptionDc.Should().BeNull();
        result.PerceptionBonus.Should().BeNull();
        result.DetectedTrapsCount.Should().Be(0);
    }

    // ===== Placement Failure Tests =====

    [Test]
    public void CreatePlacementFailure_SetsFailureState()
    {
        // Act
        var result = TrapMasteryResult.CreatePlacementFailure(7, 2,
            "Maximum active traps reached (2).");

        // Assert
        result.Type.Should().Be(TrapMasteryResult.ResultType.TrapPlaced);
        result.Success.Should().BeFalse();
        result.PlacedTrap.Should().BeNull();
        result.LocationX.Should().Be(7);
        result.LocationY.Should().Be(2);
        result.Message.Should().Be("Maximum active traps reached (2).");
        result.WasSuccessful().Should().BeFalse();
        result.GetTrapCount().Should().Be(0);
    }

    // ===== Detection Success Tests =====

    [Test]
    public void CreateDetectionSuccess_SetsCountAndDescriptions()
    {
        // Arrange
        var descriptions = new List<string>
        {
            "Spike trap at (3, 4)",
            "Net trap at (5, 6)"
        };

        // Act
        var result = TrapMasteryResult.CreateDetectionSuccess(
            count: 2,
            descriptions: descriptions,
            roll: 19,
            dc: 13,
            bonus: 4);

        // Assert
        result.Type.Should().Be(TrapMasteryResult.ResultType.TrapsDetected);
        result.Success.Should().BeTrue();
        result.DetectedTrapsCount.Should().Be(2);
        result.DetectedTrapDescriptions.Should().HaveCount(2);
        result.DetectedTrapDescriptions[0].Should().Contain("Spike trap");
        result.DetectedTrapDescriptions[1].Should().Contain("Net trap");
        result.PerceptionRoll.Should().Be(19);
        result.PerceptionDc.Should().Be(13);
        result.PerceptionBonus.Should().Be(4);
        result.Message.Should().Contain("2 traps");
        result.WasSuccessful().Should().BeTrue();
        result.GetTrapCount().Should().Be(2);

        // Placement-specific fields should be at defaults
        result.PlacedTrap.Should().BeNull();
    }

    // ===== Detection Failure Tests =====

    [Test]
    public void CreateDetectionFailure_SetsFailureState()
    {
        // Act
        var result = TrapMasteryResult.CreateDetectionFailure(
            roll: 8,
            dc: 13,
            bonus: 4);

        // Assert
        result.Type.Should().Be(TrapMasteryResult.ResultType.TrapsDetected);
        result.Success.Should().BeFalse();
        result.DetectedTrapsCount.Should().Be(0);
        result.DetectedTrapDescriptions.Should().BeEmpty();
        result.PerceptionRoll.Should().Be(8);
        result.PerceptionDc.Should().Be(13);
        result.PerceptionBonus.Should().Be(4);
        result.Message.Should().Contain("don't sense");
        result.WasSuccessful().Should().BeFalse();
        result.GetTrapCount().Should().Be(0);
    }

    // ===== GetDescription Tests =====

    [Test]
    public void GetDescription_PlacementSuccess_ContainsTrapInfo()
    {
        // Arrange
        var trap = TrapInstance.Create(Guid.NewGuid(), 4, 6, TrapType.Net);
        var result = TrapMasteryResult.CreatePlacementSuccess(trap, 4, 6);

        // Act
        var description = result.GetDescription();

        // Assert
        description.Should().Contain("Trap placed");
        description.Should().Contain("Net");
        description.Should().Contain("(4, 6)");
    }

    [Test]
    public void GetDescription_PlacementFailure_ContainsFailureReason()
    {
        // Arrange
        var result = TrapMasteryResult.CreatePlacementFailure(4, 6, "Too many traps.");

        // Act
        var description = result.GetDescription();

        // Assert
        description.Should().Contain("failed");
        description.Should().Contain("(4, 6)");
    }

    [Test]
    public void GetDescription_DetectionSuccess_ContainsRollInfo()
    {
        // Arrange
        var result = TrapMasteryResult.CreateDetectionSuccess(
            count: 1,
            descriptions: ["Hidden trap ahead"],
            roll: 17,
            dc: 13,
            bonus: 4);

        // Act
        var description = result.GetDescription();

        // Assert
        description.Should().Contain("1 trap");
        description.Should().Contain("17");
        description.Should().Contain("DC 13");
    }

    [Test]
    public void GetDescription_DetectionFailure_ContainsNoTrapsFound()
    {
        // Arrange
        var result = TrapMasteryResult.CreateDetectionFailure(roll: 6, dc: 13, bonus: 4);

        // Act
        var description = result.GetDescription();

        // Assert
        description.Should().Contain("No traps found");
        description.Should().Contain("6");
        description.Should().Contain("DC 13");
    }
}
