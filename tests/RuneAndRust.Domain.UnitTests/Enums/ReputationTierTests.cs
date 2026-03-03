using FluentAssertions;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Enums;

/// <summary>
/// Unit tests for the <see cref="ReputationTier"/> enum.
/// Verifies the enum has exactly 6 values with correct names and integer backing values.
/// </summary>
[TestFixture]
public class ReputationTierTests
{
    [Test]
    public void ReputationTier_HasSixValues()
    {
        // Arrange & Act
        var values = Enum.GetValues<ReputationTier>();

        // Assert
        values.Should().HaveCount(6);
        values.Should().Contain(ReputationTier.Hated);
        values.Should().Contain(ReputationTier.Hostile);
        values.Should().Contain(ReputationTier.Neutral);
        values.Should().Contain(ReputationTier.Friendly);
        values.Should().Contain(ReputationTier.Allied);
        values.Should().Contain(ReputationTier.Exalted);
    }

    [Test]
    public void ReputationTier_HasCorrectIntegerValues()
    {
        // Assert — backing values form a contiguous sequence 0-5
        ((int)ReputationTier.Hated).Should().Be(0);
        ((int)ReputationTier.Hostile).Should().Be(1);
        ((int)ReputationTier.Neutral).Should().Be(2);
        ((int)ReputationTier.Friendly).Should().Be(3);
        ((int)ReputationTier.Allied).Should().Be(4);
        ((int)ReputationTier.Exalted).Should().Be(5);
    }

    [Test]
    public void ReputationTier_TiersAreOrderedByValue()
    {
        // Assert — tiers can be compared via their backing int values
        ((int)ReputationTier.Hated).Should().BeLessThan((int)ReputationTier.Hostile);
        ((int)ReputationTier.Hostile).Should().BeLessThan((int)ReputationTier.Neutral);
        ((int)ReputationTier.Neutral).Should().BeLessThan((int)ReputationTier.Friendly);
        ((int)ReputationTier.Friendly).Should().BeLessThan((int)ReputationTier.Allied);
        ((int)ReputationTier.Allied).Should().BeLessThan((int)ReputationTier.Exalted);
    }

    [Test]
    public void EventCategory_ContainsReputation()
    {
        // Verify the Reputation event category was added
        var values = Enum.GetValues<EventCategory>();
        values.Should().Contain(EventCategory.Reputation);
    }

    [Test]
    public void WitnessType_HasThreeValues()
    {
        // Arrange & Act
        var values = Enum.GetValues<WitnessType>();

        // Assert
        values.Should().HaveCount(3);
        values.Should().Contain(WitnessType.Direct);
        values.Should().Contain(WitnessType.Witnessed);
        values.Should().Contain(WitnessType.Unwitnessed);
    }
}
