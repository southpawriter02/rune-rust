using FluentAssertions;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Enums;

/// <summary>
/// Unit tests for the <see cref="Lineage"/> enum.
/// </summary>
/// <remarks>
/// Verifies that the Lineage enum contains all expected values with
/// correct explicit integer assignments for stable serialization.
/// </remarks>
[TestFixture]
public class LineageTests
{
    /// <summary>
    /// Verifies that all expected lineage values are defined.
    /// </summary>
    [Test]
    public void Lineage_ContainsAllExpectedValues()
    {
        // Arrange
        var expectedValues = new[]
        {
            Lineage.ClanBorn,
            Lineage.RuneMarked,
            Lineage.IronBlooded,
            Lineage.VargrKin
        };

        // Act
        var actualValues = Enum.GetValues<Lineage>();

        // Assert
        actualValues.Should().BeEquivalentTo(expectedValues);
        actualValues.Should().HaveCount(4);
    }

    /// <summary>
    /// Verifies that enum values are explicitly assigned for stable serialization.
    /// </summary>
    [Test]
    public void Lineage_HasExplicitIntValues()
    {
        // Assert
        ((int)Lineage.ClanBorn).Should().Be(0);
        ((int)Lineage.RuneMarked).Should().Be(1);
        ((int)Lineage.IronBlooded).Should().Be(2);
        ((int)Lineage.VargrKin).Should().Be(3);
    }
}
