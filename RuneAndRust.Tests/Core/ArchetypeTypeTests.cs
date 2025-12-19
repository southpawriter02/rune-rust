using FluentAssertions;
using RuneAndRust.Core.Enums;
using Xunit;

namespace RuneAndRust.Tests.Core;

/// <summary>
/// Tests for the ArchetypeType enum.
/// Validates the four character archetypes: Warrior, Skirmisher, Adept, Mystic.
/// </summary>
public class ArchetypeTypeTests
{
    [Fact]
    public void ArchetypeType_ShouldHaveExactlyFourValues()
    {
        // Arrange
        var values = Enum.GetValues<ArchetypeType>();

        // Assert
        values.Should().HaveCount(4, "Rune & Rust has exactly four archetypes");
    }

    [Fact]
    public void ArchetypeType_ShouldContain_Warrior()
    {
        // Assert
        Enum.IsDefined(typeof(ArchetypeType), ArchetypeType.Warrior).Should().BeTrue();
    }

    [Fact]
    public void ArchetypeType_ShouldContain_Skirmisher()
    {
        // Assert
        Enum.IsDefined(typeof(ArchetypeType), ArchetypeType.Skirmisher).Should().BeTrue();
    }

    [Fact]
    public void ArchetypeType_ShouldContain_Adept()
    {
        // Assert
        Enum.IsDefined(typeof(ArchetypeType), ArchetypeType.Adept).Should().BeTrue();
    }

    [Fact]
    public void ArchetypeType_ShouldContain_Mystic()
    {
        // Assert
        Enum.IsDefined(typeof(ArchetypeType), ArchetypeType.Mystic).Should().BeTrue();
    }

    [Fact]
    public void ArchetypeType_EnumValues_ShouldBeSequential()
    {
        // Assert
        ((int)ArchetypeType.Warrior).Should().Be(0);
        ((int)ArchetypeType.Skirmisher).Should().Be(1);
        ((int)ArchetypeType.Adept).Should().Be(2);
        ((int)ArchetypeType.Mystic).Should().Be(3);
    }

    [Theory]
    [InlineData(ArchetypeType.Warrior, "Warrior")]
    [InlineData(ArchetypeType.Skirmisher, "Skirmisher")]
    [InlineData(ArchetypeType.Adept, "Adept")]
    [InlineData(ArchetypeType.Mystic, "Mystic")]
    public void ArchetypeType_ToString_ReturnsExpectedName(ArchetypeType archetype, string expectedName)
    {
        // Assert
        archetype.ToString().Should().Be(expectedName);
    }
}
