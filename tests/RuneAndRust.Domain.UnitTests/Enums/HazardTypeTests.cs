using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Enums;

/// <summary>
/// Tests for the HazardType enum.
/// </summary>
[TestFixture]
public class HazardTypeTests
{
    [Test]
    public void HazardType_HasAllExpectedValues()
    {
        // Act
        var values = Enum.GetValues<HazardType>();

        // Assert
        values.Should().HaveCount(11);
        values.Should().Contain(HazardType.PoisonGas);
        values.Should().Contain(HazardType.Fire);
        values.Should().Contain(HazardType.Ice);
        values.Should().Contain(HazardType.Spikes);
        values.Should().Contain(HazardType.AcidPool);
        values.Should().Contain(HazardType.Darkness);
        values.Should().Contain(HazardType.Electricity);
        values.Should().Contain(HazardType.Radiant);
        values.Should().Contain(HazardType.Necrotic);
        values.Should().Contain(HazardType.Pit);
        values.Should().Contain(HazardType.Lava);
    }

    [Test]
    public void HazardType_CanCastToInt()
    {
        // Arrange & Act
        var poisonGas = (int)HazardType.PoisonGas;
        var necrotic = (int)HazardType.Necrotic;

        // Assert
        poisonGas.Should().Be(0);
        necrotic.Should().Be(8);
    }
}
