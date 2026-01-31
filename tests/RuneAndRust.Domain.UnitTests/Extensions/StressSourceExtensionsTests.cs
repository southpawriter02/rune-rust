namespace RuneAndRust.Domain.UnitTests.Extensions;

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Extensions;

/// <summary>
/// Unit tests for <see cref="StressSourceExtensions"/> extension methods.
/// </summary>
[TestFixture]
public class StressSourceExtensionsTests
{
    // -------------------------------------------------------------------------
    // GetDescription — All Sources
    // -------------------------------------------------------------------------

    [Test]
    [TestCase(StressSource.Combat)]
    [TestCase(StressSource.Exploration)]
    [TestCase(StressSource.Narrative)]
    [TestCase(StressSource.Heretical)]
    [TestCase(StressSource.Environmental)]
    [TestCase(StressSource.Corruption)]
    public void GetDescription_ReturnsNonEmptyForAllSources(StressSource source)
    {
        // Act
        var result = source.GetDescription();

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
    }

    [Test]
    public void GetDescription_ReturnsExpectedValues()
    {
        // Assert — spot check specific descriptions
        StressSource.Combat.GetDescription().Should().Contain("Combat");
        StressSource.Corruption.GetDescription().Should().Contain("Blight");
    }

    // -------------------------------------------------------------------------
    // TypicallyResistable — Resistable vs Unavoidable
    // -------------------------------------------------------------------------

    [Test]
    [TestCase(StressSource.Combat, true)]
    [TestCase(StressSource.Exploration, true)]
    [TestCase(StressSource.Narrative, false)]
    [TestCase(StressSource.Heretical, true)]
    [TestCase(StressSource.Environmental, true)]
    [TestCase(StressSource.Corruption, false)]
    public void TypicallyResistable_ReturnsExpected(StressSource source, bool expected)
    {
        // Act
        var result = source.TypicallyResistable();

        // Assert
        result.Should().Be(expected);
    }

    [Test]
    public void TypicallyResistable_NarrativeIsNotResistable()
    {
        // Act & Assert — Narrative stress is unavoidable story-driven impact
        StressSource.Narrative.TypicallyResistable().Should().BeFalse();
    }

    [Test]
    public void TypicallyResistable_CorruptionIsNotResistable()
    {
        // Act & Assert — Corruption stress is automatic from Corruption failure
        StressSource.Corruption.TypicallyResistable().Should().BeFalse();
    }
}
