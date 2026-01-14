using FluentAssertions;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Enums;

/// <summary>
/// Unit tests for the <see cref="TerrainType"/> enum (v0.5.2a).
/// </summary>
[TestFixture]
public class TerrainTypeTests
{
    [Test]
    public void TerrainType_HasExpectedValues()
    {
        // Assert
        ((int)TerrainType.Normal).Should().Be(0);
        ((int)TerrainType.Difficult).Should().Be(1);
        ((int)TerrainType.Impassable).Should().Be(2);
        ((int)TerrainType.Hazardous).Should().Be(3);
    }

    [Test]
    public void TerrainType_CanParseFromString()
    {
        // Act & Assert
        Enum.TryParse<TerrainType>("Normal", out var normal).Should().BeTrue();
        normal.Should().Be(TerrainType.Normal);

        Enum.TryParse<TerrainType>("Difficult", out var difficult).Should().BeTrue();
        difficult.Should().Be(TerrainType.Difficult);

        Enum.TryParse<TerrainType>("Impassable", out var impassable).Should().BeTrue();
        impassable.Should().Be(TerrainType.Impassable);

        Enum.TryParse<TerrainType>("Hazardous", out var hazardous).Should().BeTrue();
        hazardous.Should().Be(TerrainType.Hazardous);
    }
}
