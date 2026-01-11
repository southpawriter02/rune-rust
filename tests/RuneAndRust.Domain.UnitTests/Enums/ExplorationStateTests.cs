using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Enums;

/// <summary>
/// Tests for the ExplorationState enum.
/// </summary>
[TestFixture]
public class ExplorationStateTests
{
    [Test]
    public void ExplorationState_HasCorrectValues()
    {
        // Assert
        ((int)ExplorationState.Unexplored).Should().Be(0);
        ((int)ExplorationState.Visited).Should().Be(1);
        ((int)ExplorationState.Cleared).Should().Be(2);
    }

    [Test]
    public void ExplorationState_HasCorrectOrder()
    {
        // Assert - States should be in progression order
        ((int)ExplorationState.Unexplored).Should().BeLessThan((int)ExplorationState.Visited);
        ((int)ExplorationState.Visited).Should().BeLessThan((int)ExplorationState.Cleared);
    }
}
