namespace RuneAndRust.Domain.UnitTests.Enums;

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;

/// <summary>
/// Unit tests for <see cref="CorruptionStage"/> enum.
/// Verifies that all corruption stage tiers have the correct integer values
/// and that exactly 6 stages are defined for the Runic Blight Corruption system.
/// </summary>
[TestFixture]
public class CorruptionStageTests
{
    // -------------------------------------------------------------------------
    // Enum Values — Verification
    // -------------------------------------------------------------------------

    [Test]
    public void CorruptionStage_HasExpectedValues()
    {
        // Assert — Verify all expected stages exist with correct integer values
        ((int)CorruptionStage.Uncorrupted).Should().Be(0);
        ((int)CorruptionStage.Tainted).Should().Be(1);
        ((int)CorruptionStage.Infected).Should().Be(2);
        ((int)CorruptionStage.Blighted).Should().Be(3);
        ((int)CorruptionStage.Corrupted).Should().Be(4);
        ((int)CorruptionStage.Consumed).Should().Be(5);
    }

    [Test]
    public void CorruptionStage_HasCorrectCount()
    {
        // Assert — Verify exactly 6 stages exist
        Enum.GetValues<CorruptionStage>().Should().HaveCount(6);
    }
}
