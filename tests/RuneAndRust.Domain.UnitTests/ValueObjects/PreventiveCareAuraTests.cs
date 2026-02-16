using FluentAssertions;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for <see cref="PreventiveCareAura"/>.
/// Validates computed constant properties and display methods
/// for the Bone-Setter Tier 3 passive aura ability result.
/// </summary>
[TestFixture]
public class PreventiveCareAuraTests
{
    [Test]
    public void AuraRadius_AlwaysFive()
    {
        // Arrange & Act
        var aura = new PreventiveCareAura();

        // Assert — Preventive Care radius is always 5 spaces
        aura.AuraRadius.Should().Be(5);
    }

    [Test]
    public void PoisonSaveBonus_AlwaysOne()
    {
        // Arrange & Act
        var aura = new PreventiveCareAura();

        // Assert — Poison saving throw bonus is always +1
        aura.PoisonSaveBonus.Should().Be(1);
    }

    [Test]
    public void DiseaseSaveBonus_AlwaysOne()
    {
        // Arrange & Act
        var aura = new PreventiveCareAura();

        // Assert — Disease saving throw bonus is always +1
        aura.DiseaseSaveBonus.Should().Be(1);
    }

    [Test]
    public void IsActive_AlwaysTrue()
    {
        // Arrange & Act
        var aura = new PreventiveCareAura();

        // Assert — passive ability is always active when evaluated
        aura.IsActive.Should().BeTrue();
    }

    [Test]
    public void GetAuraSummary_ContainsAllyCountAndRadius()
    {
        // Arrange
        var allyIds = new List<Guid>
        {
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid()
        };

        var aura = new PreventiveCareAura
        {
            BoneSetterId = Guid.NewGuid(),
            AffectedAllyIds = allyIds.AsReadOnly()
        };

        // Act
        var summary = aura.GetAuraSummary();

        // Assert
        summary.Should().Contain("5 spaces");
        summary.Should().Contain("3 allies");
        summary.Should().Contain("Preventive Care");
    }
}
