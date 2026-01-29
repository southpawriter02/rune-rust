using FluentAssertions;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Enums;

/// <summary>
/// Unit tests for the <see cref="Background"/> enum.
/// </summary>
/// <remarks>
/// Verifies that the Background enum contains all expected values with
/// correct explicit integer assignments for stable serialization.
/// Each background represents a pre-Silence profession that determines
/// starting skills, equipment, social standing, and narrative hooks.
/// </remarks>
[TestFixture]
public class BackgroundTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // ENUM VALUE TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that the Background enum contains exactly 6 values.
    /// </summary>
    /// <remarks>
    /// The six backgrounds cover distinct pre-Silence professions:
    /// VillageSmith, TravelingHealer, RuinDelver, ClanGuard,
    /// WanderingSkald, and OutcastScavenger.
    /// </remarks>
    [Test]
    public void Background_HasSixValues_CountEquals6()
    {
        // Arrange & Act
        var values = Enum.GetValues<Background>();

        // Assert
        values.Should().HaveCount(6,
            "the Background enum must define exactly 6 pre-Silence professions");
    }

    /// <summary>
    /// Verifies that all expected background values are defined in the enum.
    /// </summary>
    /// <remarks>
    /// Ensures no backgrounds are missing and no unexpected backgrounds
    /// have been added. The set of backgrounds is fixed by the design
    /// specification.
    /// </remarks>
    [Test]
    public void Background_ContainsAllExpectedValues()
    {
        // Arrange
        var expectedValues = new[]
        {
            Background.VillageSmith,
            Background.TravelingHealer,
            Background.RuinDelver,
            Background.ClanGuard,
            Background.WanderingSkald,
            Background.OutcastScavenger
        };

        // Act
        var actualValues = Enum.GetValues<Background>();

        // Assert
        actualValues.Should().BeEquivalentTo(expectedValues,
            "the Background enum must contain all six defined pre-Silence professions");
    }

    /// <summary>
    /// Verifies that enum values are explicitly assigned for stable serialization.
    /// </summary>
    /// <remarks>
    /// Explicit integer values (0-5) ensure that reordering enum members
    /// does not break existing serialized data or database records.
    /// These values match the design specification exactly.
    /// </remarks>
    [Test]
    public void Background_HasExplicitIntValues()
    {
        // Assert - each value maps to its expected integer
        ((int)Background.VillageSmith).Should().Be(0,
            "VillageSmith is the first background (index 0)");
        ((int)Background.TravelingHealer).Should().Be(1,
            "TravelingHealer is the second background (index 1)");
        ((int)Background.RuinDelver).Should().Be(2,
            "RuinDelver is the third background (index 2)");
        ((int)Background.ClanGuard).Should().Be(3,
            "ClanGuard is the fourth background (index 3)");
        ((int)Background.WanderingSkald).Should().Be(4,
            "WanderingSkald is the fifth background (index 4)");
        ((int)Background.OutcastScavenger).Should().Be(5,
            "OutcastScavenger is the sixth background (index 5)");
    }
}
