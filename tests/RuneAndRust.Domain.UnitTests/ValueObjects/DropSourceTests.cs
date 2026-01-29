using FluentAssertions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="DropSource"/> value object.
/// </summary>
[TestFixture]
public class DropSourceTests
{
    /// <summary>
    /// Verifies that Create with valid data creates a DropSource.
    /// </summary>
    [Test]
    public void Create_WithValidData_CreatesDropSource()
    {
        // Arrange & Act
        var source = DropSource.Create(DropSourceType.Boss, "shadow-lord", 5.0m);

        // Assert
        source.SourceType.Should().Be(DropSourceType.Boss);
        source.SourceId.Should().Be("shadow-lord");
        source.DropChance.Should().Be(5.0m);
    }

    /// <summary>
    /// Verifies that Create normalizes source ID to lowercase.
    /// </summary>
    [Test]
    public void Create_NormalizesSourceId()
    {
        // Arrange & Act
        var source = DropSource.Create(DropSourceType.Monster, "GOBLIN-CHIEF", 2.5m);

        // Assert
        source.SourceId.Should().Be("goblin-chief");
    }

    /// <summary>
    /// Verifies that Create throws when source ID is null.
    /// </summary>
    [Test]
    public void Create_WithNullSourceId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => DropSource.Create(DropSourceType.Monster, null!, 1.0m);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Verifies that Create throws when source ID is empty.
    /// </summary>
    [Test]
    public void Create_WithEmptySourceId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => DropSource.Create(DropSourceType.Monster, "", 1.0m);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Verifies that Create throws when drop chance is negative.
    /// </summary>
    [Test]
    public void Create_WithNegativeDropChance_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => DropSource.Create(DropSourceType.Monster, "goblin", -1.0m);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// Verifies that Create throws when drop chance exceeds 100.
    /// </summary>
    [Test]
    public void Create_WithDropChanceOver100_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => DropSource.Create(DropSourceType.Quest, "main-quest", 150.0m);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// Verifies that IsGuaranteed returns true for 100% drop chance.
    /// </summary>
    [Test]
    public void IsGuaranteed_WithFullDropChance_ReturnsTrue()
    {
        // Arrange
        var source = DropSource.Create(DropSourceType.Quest, "main-quest", 100.0m);

        // Assert
        source.IsGuaranteed.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that IsGuaranteed returns false for less than 100% drop chance.
    /// </summary>
    [Test]
    public void IsGuaranteed_WithPartialDropChance_ReturnsFalse()
    {
        // Arrange
        var source = DropSource.Create(DropSourceType.Boss, "dragon", 50.0m);

        // Assert
        source.IsGuaranteed.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that ToString formats correctly.
    /// </summary>
    [Test]
    public void ToString_FormatsCorrectly()
    {
        // Arrange
        var source = DropSource.Create(DropSourceType.Boss, "shadow-lord", 5.0m);

        // Act
        var result = source.ToString();

        // Assert
        result.Should().Contain("Boss");
        result.Should().Contain("shadow-lord");
        result.Should().Contain("5.00%");
    }
}
