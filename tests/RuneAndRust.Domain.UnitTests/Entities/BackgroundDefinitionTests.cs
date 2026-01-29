using FluentAssertions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Unit tests for the <see cref="BackgroundDefinition"/> entity.
/// </summary>
/// <remarks>
/// Verifies factory method behavior, parameter validation, narrative hook
/// methods, and helper methods for the BackgroundDefinition entity.
/// Tests follow the patterns established by LineageDefinitionTests.
/// </remarks>
[TestFixture]
public class BackgroundDefinitionTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHOD TESTS - SUCCESSFUL CREATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that Create successfully creates a definition with all valid parameters.
    /// </summary>
    /// <remarks>
    /// Tests the Village Smith background with full configuration including
    /// narrative hooks. All properties should be populated correctly.
    /// </remarks>
    [Test]
    public void Create_WithValidParameters_CreatesDefinition()
    {
        // Arrange
        var narrativeHooks = new List<string>
        {
            "Recognize craftsmanship in ruins",
            "Repair broken equipment more easily",
            "Clan smiths may offer discounts or quests"
        };

        // Act
        var definition = BackgroundDefinition.Create(
            Background.VillageSmith,
            "Village Smith",
            "You worked the forge, shaping metal into tools of war and peace.",
            "The ring of hammer on anvil was your morning song.",
            "Blacksmith and metalworker",
            "Respected craftsperson, essential to any settlement",
            narrativeHooks);

        // Assert
        definition.Should().NotBeNull();
        definition.Id.Should().NotBe(Guid.Empty,
            "a new GUID should be generated for each definition");
        definition.BackgroundId.Should().Be(Background.VillageSmith);
        definition.DisplayName.Should().Be("Village Smith");
        definition.Description.Should().Contain("forge");
        definition.SelectionText.Should().Contain("hammer on anvil");
        definition.ProfessionBefore.Should().Be("Blacksmith and metalworker");
        definition.SocialStanding.Should().Contain("Respected craftsperson");
        definition.NarrativeHooks.Should().HaveCount(3);
        definition.NarrativeHooks[0].Should().Contain("craftsmanship");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHOD TESTS - VALIDATION FAILURES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that Create throws when displayName is null.
    /// </summary>
    [Test]
    public void Create_WithNullDisplayName_ThrowsArgumentException()
    {
        // Arrange
        Action act = () => BackgroundDefinition.Create(
            Background.VillageSmith,
            null!,
            "Description text.",
            "Selection text.",
            "Blacksmith and metalworker",
            "Respected craftsperson");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("displayName");
    }

    /// <summary>
    /// Verifies that Create throws when description is empty.
    /// </summary>
    [Test]
    public void Create_WithEmptyDescription_ThrowsArgumentException()
    {
        // Arrange
        Action act = () => BackgroundDefinition.Create(
            Background.VillageSmith,
            "Village Smith",
            "",
            "Selection text.",
            "Blacksmith and metalworker",
            "Respected craftsperson");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("description");
    }

    /// <summary>
    /// Verifies that Create throws when professionBefore is null.
    /// </summary>
    [Test]
    public void Create_WithNullProfessionBefore_ThrowsArgumentException()
    {
        // Arrange
        Action act = () => BackgroundDefinition.Create(
            Background.VillageSmith,
            "Village Smith",
            "Description text.",
            "Selection text.",
            null!,
            "Respected craftsperson");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("professionBefore");
    }

    /// <summary>
    /// Verifies that Create throws when socialStanding is whitespace.
    /// </summary>
    [Test]
    public void Create_WithWhitespaceSocialStanding_ThrowsArgumentException()
    {
        // Arrange
        Action act = () => BackgroundDefinition.Create(
            Background.VillageSmith,
            "Village Smith",
            "Description text.",
            "Selection text.",
            "Blacksmith and metalworker",
            "   ");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("socialStanding");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // NARRATIVE HOOK TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that HasNarrativeHooks returns true when hooks are defined.
    /// </summary>
    [Test]
    public void HasNarrativeHooks_WithHooks_ReturnsTrue()
    {
        // Arrange
        var definition = CreateVillageSmithDefinition();

        // Act
        var result = definition.HasNarrativeHooks();

        // Assert
        result.Should().BeTrue(
            "Village Smith has 3 narrative hooks defined");
    }

    /// <summary>
    /// Verifies that HasNarrativeHooks returns false when no hooks are defined.
    /// </summary>
    [Test]
    public void HasNarrativeHooks_WithoutHooks_ReturnsFalse()
    {
        // Arrange - create definition without narrative hooks
        var definition = BackgroundDefinition.Create(
            Background.VillageSmith,
            "Village Smith",
            "You worked the forge, shaping metal into tools of war and peace.",
            "The ring of hammer on anvil was your morning song.",
            "Blacksmith and metalworker",
            "Respected craftsperson, essential to any settlement");

        // Act
        var result = definition.HasNarrativeHooks();

        // Assert
        result.Should().BeFalse(
            "no narrative hooks were provided to the factory method");
    }

    /// <summary>
    /// Verifies that HasNarrativeHookContaining returns true when a matching keyword is found.
    /// </summary>
    /// <remarks>
    /// The search is case-insensitive, so "craftsmanship" matches
    /// "Recognize craftsmanship in ruins".
    /// </remarks>
    [Test]
    public void HasNarrativeHookContaining_WithMatchingKeyword_ReturnsTrue()
    {
        // Arrange
        var definition = CreateVillageSmithDefinition();

        // Act
        var result = definition.HasNarrativeHookContaining("craftsmanship");

        // Assert
        result.Should().BeTrue(
            "Village Smith has hook 'Recognize craftsmanship in ruins'");
    }

    /// <summary>
    /// Verifies that HasNarrativeHookContaining returns false when no hook matches.
    /// </summary>
    [Test]
    public void HasNarrativeHookContaining_WithNonMatchingKeyword_ReturnsFalse()
    {
        // Arrange
        var definition = CreateVillageSmithDefinition();

        // Act
        var result = definition.HasNarrativeHookContaining("combat");

        // Assert
        result.Should().BeFalse(
            "Village Smith has no hooks containing 'combat'");
    }

    /// <summary>
    /// Verifies that HasNarrativeHookContaining returns false for null or whitespace keywords.
    /// </summary>
    /// <remarks>
    /// Null and whitespace keywords are treated as non-matching to prevent
    /// false positives in the narrative event system.
    /// </remarks>
    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void HasNarrativeHookContaining_WithNullOrWhitespaceKeyword_ReturnsFalse(string? keyword)
    {
        // Arrange
        var definition = CreateVillageSmithDefinition();

        // Act
        var result = definition.HasNarrativeHookContaining(keyword!);

        // Assert
        result.Should().BeFalse(
            "null or whitespace keywords should never match any hooks");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // HELPER METHOD TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetCreationSummary returns a formatted profession and standing string.
    /// </summary>
    [Test]
    public void GetCreationSummary_ReturnsFormattedString()
    {
        // Arrange
        var definition = CreateVillageSmithDefinition();

        // Act
        var summary = definition.GetCreationSummary();

        // Assert
        summary.Should().Contain("Profession: Blacksmith and metalworker");
        summary.Should().Contain("Standing: Respected craftsperson, essential to any settlement");
        summary.Should().Contain("\n",
            "profession and standing should be separated by a newline");
    }

    /// <summary>
    /// Verifies that ToString returns the display name and background ID.
    /// </summary>
    [Test]
    public void ToString_ReturnsDisplayNameAndId()
    {
        // Arrange
        var definition = CreateVillageSmithDefinition();

        // Act
        var result = definition.ToString();

        // Assert
        result.Should().Contain("Village Smith");
        result.Should().Contain("VillageSmith");
        result.Should().Contain("Blacksmith and metalworker");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // OPTIONAL PARAMETER TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that omitting narrative hooks defaults to an empty list.
    /// </summary>
    [Test]
    public void Create_WithoutNarrativeHooks_DefaultsToEmptyList()
    {
        // Arrange & Act
        var definition = BackgroundDefinition.Create(
            Background.RuinDelver,
            "Ruin Delver",
            "The old places called to you, their secrets waiting to be found.",
            "While others feared the dark places, you saw opportunity.",
            "Scavenger and explorer",
            "Useful but looked down upon");

        // Assert
        definition.NarrativeHooks.Should().BeEmpty(
            "narrative hooks should default to an empty list when not provided");
        definition.GetNarrativeHookCount().Should().Be(0);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // TEST HELPERS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a standard Village Smith definition for testing.
    /// </summary>
    /// <returns>A fully populated BackgroundDefinition for Village Smith.</returns>
    private static BackgroundDefinition CreateVillageSmithDefinition()
    {
        return BackgroundDefinition.Create(
            Background.VillageSmith,
            "Village Smith",
            "You worked the forge, shaping metal into tools of war and peace.",
            "The ring of hammer on anvil was your morning song.",
            "Blacksmith and metalworker",
            "Respected craftsperson, essential to any settlement",
            new List<string>
            {
                "Recognize craftsmanship in ruins",
                "Repair broken equipment more easily",
                "Clan smiths may offer discounts or quests"
            });
    }
}
