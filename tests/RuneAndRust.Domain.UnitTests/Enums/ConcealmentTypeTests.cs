using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Enums;

/// <summary>
/// Unit tests for the <see cref="ConcealmentType"/> enum.
/// Verifies that all concealment types are correctly defined with expected values.
/// </summary>
/// <remarks>
/// <para>Introduced in v0.20.7c for the Apex Predator passive ability.</para>
/// <para>ConcealmentType is distinct from <see cref="CoverType"/>:</para>
/// <list type="bullet">
/// <item>CoverType provides physical defense (AC bonus) — handled by Hunter's Eye (T2)</item>
/// <item>ConcealmentType affects perception (miss chance) — handled by Apex Predator (T3)</item>
/// </list>
/// </remarks>
[TestFixture]
public class ConcealmentTypeTests
{
    [Test]
    public void ConcealmentType_HasExpectedValues()
    {
        // Arrange & Act
        var values = Enum.GetValues<ConcealmentType>();

        // Assert
        values.Should().HaveCount(5,
            "ConcealmentType should have 5 values: None, LightObscurement, Invisibility, MagicalCamo, Hidden");
        values.Should().Contain(ConcealmentType.None);
        values.Should().Contain(ConcealmentType.LightObscurement);
        values.Should().Contain(ConcealmentType.Invisibility);
        values.Should().Contain(ConcealmentType.MagicalCamo);
        values.Should().Contain(ConcealmentType.Hidden);
    }

    [Test]
    public void ConcealmentType_ParsesFromString()
    {
        // Arrange & Act
        var none = Enum.Parse<ConcealmentType>("None", ignoreCase: true);
        var lightObscurement = Enum.Parse<ConcealmentType>("LightObscurement", ignoreCase: true);
        var invisibility = Enum.Parse<ConcealmentType>("Invisibility", ignoreCase: true);
        var magicalCamo = Enum.Parse<ConcealmentType>("MagicalCamo", ignoreCase: true);
        var hidden = Enum.Parse<ConcealmentType>("Hidden", ignoreCase: true);

        // Assert
        none.Should().Be(ConcealmentType.None);
        lightObscurement.Should().Be(ConcealmentType.LightObscurement);
        invisibility.Should().Be(ConcealmentType.Invisibility);
        magicalCamo.Should().Be(ConcealmentType.MagicalCamo);
        hidden.Should().Be(ConcealmentType.Hidden);
    }

    [Test]
    public void ConcealmentType_HasCorrectIntegerValues()
    {
        // Assert
        ((int)ConcealmentType.None).Should().Be(0);
        ((int)ConcealmentType.LightObscurement).Should().Be(1);
        ((int)ConcealmentType.Invisibility).Should().Be(2);
        ((int)ConcealmentType.MagicalCamo).Should().Be(3);
        ((int)ConcealmentType.Hidden).Should().Be(4);
    }
}
