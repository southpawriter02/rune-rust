namespace RuneAndRust.Domain.UnitTests.Enums;

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;

/// <summary>
/// Unit tests for <see cref="TraumaType"/> enum.
/// Verifies enum values and default value.
/// </summary>
[TestFixture]
public class TraumaTypeTests
{
    // -------------------------------------------------------------------------
    // Enum Value Count
    // -------------------------------------------------------------------------

    [Test]
    public void TraumaType_ShouldHaveExactlySixValues()
    {
        // Arrange & Act
        var values = Enum.GetValues<TraumaType>();

        // Assert
        values.Should().HaveCount(6);
    }

    // -------------------------------------------------------------------------
    // Default Value
    // -------------------------------------------------------------------------

    [Test]
    public void TraumaType_ShouldHaveCognitiveAsDefault()
    {
        // Arrange & Act
        var defaultType = default(TraumaType);

        // Assert
        defaultType.Should().Be(TraumaType.Cognitive);
    }

    // -------------------------------------------------------------------------
    // Expected Values
    // -------------------------------------------------------------------------

    [Test]
    public void TraumaType_ShouldContainAllExpectedValues()
    {
        // Arrange & Act
        var values = Enum.GetValues<TraumaType>();

        // Assert
        values.Should().Contain(TraumaType.Cognitive);
        values.Should().Contain(TraumaType.Emotional);
        values.Should().Contain(TraumaType.Physical);
        values.Should().Contain(TraumaType.Social);
        values.Should().Contain(TraumaType.Existential);
        values.Should().Contain(TraumaType.Corruption);
    }

    // -------------------------------------------------------------------------
    // Explicit Values
    // -------------------------------------------------------------------------

    [Test]
    [TestCase(TraumaType.Cognitive, 0)]
    [TestCase(TraumaType.Emotional, 1)]
    [TestCase(TraumaType.Physical, 2)]
    [TestCase(TraumaType.Social, 3)]
    [TestCase(TraumaType.Existential, 4)]
    [TestCase(TraumaType.Corruption, 5)]
    public void TraumaType_ShouldHaveCorrectUnderlyingValue(
        TraumaType type,
        int expectedValue)
    {
        // Assert
        ((int)type).Should().Be(expectedValue);
    }

    // -------------------------------------------------------------------------
    // Category Distinctions
    // -------------------------------------------------------------------------

    [Test]
    [TestCase(TraumaType.Cognitive)]
    [TestCase(TraumaType.Emotional)]
    [TestCase(TraumaType.Existential)]
    public void TraumaType_MentalTraumas_ShouldBeDistinguishable(TraumaType mentalType)
    {
        // Assert - mental traumas are distinguishable from physical/corruption
        mentalType.Should().NotBe(TraumaType.Physical);
        mentalType.Should().NotBe(TraumaType.Corruption);
    }

    [Test]
    public void TraumaType_Corruption_ShouldBeBlightRelated()
    {
        // Arrange
        var corruptionType = TraumaType.Corruption;

        // Assert - corruption is the only blight-related type
        corruptionType.Should().Be(TraumaType.Corruption);
        ((int)corruptionType).Should().Be(5);
    }
}
