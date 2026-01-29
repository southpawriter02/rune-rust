namespace RuneAndRust.Domain.UnitTests.Enums;

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;

/// <summary>
/// Unit tests for the <see cref="UniqueAchievementType"/> enum.
/// </summary>
/// <remarks>
/// These tests verify:
/// <list type="bullet">
///   <item><description>The enum has the expected number of types (6)</description></item>
///   <item><description>Each type has the correct integer value</description></item>
///   <item><description>None is the default value (0)</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class UniqueAchievementTypeTests
{
    /// <summary>
    /// Verifies the enum contains exactly 6 types.
    /// </summary>
    [Test]
    public void UniqueAchievementType_HasExpected6Types()
    {
        // Arrange & Act
        var types = Enum.GetValues<UniqueAchievementType>();

        // Assert (5 achievement types + None = 6)
        types.Should().HaveCount(6);
    }

    /// <summary>
    /// Verifies each achievement type has the correct integer value.
    /// </summary>
    /// <param name="type">The achievement type to test.</param>
    /// <param name="expectedValue">The expected integer value.</param>
    [Test]
    [TestCase(UniqueAchievementType.None, 0)]
    [TestCase(UniqueAchievementType.FirstMythForged, 1)]
    [TestCase(UniqueAchievementType.CollectorBronze, 2)]
    [TestCase(UniqueAchievementType.CollectorSilver, 3)]
    [TestCase(UniqueAchievementType.CollectorGold, 4)]
    [TestCase(UniqueAchievementType.ClassMaster, 5)]
    public void UniqueAchievementType_HasCorrectValues(
        UniqueAchievementType type,
        int expectedValue)
    {
        // Assert
        ((int)type).Should().Be(expectedValue);
    }

    /// <summary>
    /// Verifies None is the default value for an uninitialized UniqueAchievementType.
    /// </summary>
    [Test]
    public void UniqueAchievementType_Default_IsNone()
    {
        // Arrange
        UniqueAchievementType defaultType = default;

        // Assert
        defaultType.Should().Be(UniqueAchievementType.None);
    }
}
