using FluentAssertions;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Enums;

[TestFixture]
public class AIBehaviorTests
{
    [Test]
    public void AIBehavior_ShouldHaveFiveBehaviors()
    {
        // Arrange & Act
        var values = Enum.GetValues<AIBehavior>();

        // Assert
        values.Should().HaveCount(5);
    }

    [Test]
    [TestCase(AIBehavior.Aggressive, 0)]
    [TestCase(AIBehavior.Defensive, 1)]
    [TestCase(AIBehavior.Cowardly, 2)]
    [TestCase(AIBehavior.Support, 3)]
    [TestCase(AIBehavior.Chaotic, 4)]
    public void AIBehavior_ShouldHaveExpectedValues(AIBehavior behavior, int expectedValue)
    {
        // Assert
        ((int)behavior).Should().Be(expectedValue);
    }
}
