// ═══════════════════════════════════════════════════════════════════════════════
// ArchetypeSpecializationMappingTests.cs
// Unit tests for the ArchetypeSpecializationMapping value object verifying
// static archetype mappings, factory method validation, specialization
// availability checks, lookup methods, and display formatting.
// Version: 0.17.3d
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="ArchetypeSpecializationMapping"/> value object.
/// </summary>
/// <remarks>
/// <para>
/// Verifies that ArchetypeSpecializationMapping correctly:
/// </para>
/// <list type="bullet">
///   <item><description>Provides canonical mappings for all 4 archetypes via static properties</description></item>
///   <item><description>Warrior has 6 specializations with guardian as recommended first</description></item>
///   <item><description>Skirmisher has 4 specializations with shadow-dancer as recommended first</description></item>
///   <item><description>Mystic has 2 specializations with elementalist as recommended first</description></item>
///   <item><description>Adept has 5 specializations with alchemist as recommended first</description></item>
///   <item><description>Creates validated mappings via the Create factory method</description></item>
///   <item><description>Normalizes specialization IDs to lowercase during creation</description></item>
///   <item><description>Rejects null, empty, or invalid parameters during creation</description></item>
///   <item><description>Checks specialization availability with case-insensitive matching</description></item>
///   <item><description>Returns correct mappings via GetForArchetype lookup</description></item>
///   <item><description>Produces correctly formatted display lists and debug strings</description></item>
/// </list>
/// </remarks>
/// <seealso cref="ArchetypeSpecializationMapping"/>
/// <seealso cref="Archetype"/>
[TestFixture]
public class ArchetypeSpecializationMappingTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // STATIC PROPERTY TESTS — Warrior
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that the Warrior archetype has exactly 6 available specializations
    /// with all expected IDs present and guardian as the recommended first.
    /// </summary>
    [Test]
    public void Warrior_HasSixSpecializations()
    {
        // Arrange & Act
        var mapping = ArchetypeSpecializationMapping.Warrior;

        // Assert
        mapping.ArchetypeId.Should().Be(Archetype.Warrior);
        mapping.Count.Should().Be(6);
        mapping.RecommendedFirst.Should().Be("guardian");
        mapping.HasSpecializations.Should().BeTrue();
        mapping.AvailableSpecializations.Should().Contain("guardian");
        mapping.AvailableSpecializations.Should().Contain("berserker");
        mapping.AvailableSpecializations.Should().Contain("weapon-master");
        mapping.AvailableSpecializations.Should().Contain("vanguard");
        mapping.AvailableSpecializations.Should().Contain("juggernaut");
        mapping.AvailableSpecializations.Should().Contain("battle-commander");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // STATIC PROPERTY TESTS — Skirmisher
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that the Skirmisher archetype has exactly 4 available specializations
    /// with all expected IDs present and shadow-dancer as the recommended first.
    /// </summary>
    [Test]
    public void Skirmisher_HasFourSpecializations()
    {
        // Arrange & Act
        var mapping = ArchetypeSpecializationMapping.Skirmisher;

        // Assert
        mapping.ArchetypeId.Should().Be(Archetype.Skirmisher);
        mapping.Count.Should().Be(4);
        mapping.RecommendedFirst.Should().Be("shadow-dancer");
        mapping.HasSpecializations.Should().BeTrue();
        mapping.AvailableSpecializations.Should().Contain("shadow-dancer");
        mapping.AvailableSpecializations.Should().Contain("duelist");
        mapping.AvailableSpecializations.Should().Contain("ranger");
        mapping.AvailableSpecializations.Should().Contain("acrobat");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // STATIC PROPERTY TESTS — Mystic
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that the Mystic archetype has exactly 2 available specializations
    /// with elementalist and void-weaver present and elementalist as recommended.
    /// </summary>
    [Test]
    public void Mystic_HasTwoSpecializations()
    {
        // Arrange & Act
        var mapping = ArchetypeSpecializationMapping.Mystic;

        // Assert
        mapping.ArchetypeId.Should().Be(Archetype.Mystic);
        mapping.Count.Should().Be(2);
        mapping.RecommendedFirst.Should().Be("elementalist");
        mapping.HasSpecializations.Should().BeTrue();
        mapping.AvailableSpecializations.Should().Contain("elementalist");
        mapping.AvailableSpecializations.Should().Contain("void-weaver");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // STATIC PROPERTY TESTS — Adept
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that the Adept archetype has exactly 5 available specializations
    /// with all expected IDs present and alchemist as the recommended first.
    /// </summary>
    [Test]
    public void Adept_HasFiveSpecializations()
    {
        // Arrange & Act
        var mapping = ArchetypeSpecializationMapping.Adept;

        // Assert
        mapping.ArchetypeId.Should().Be(Archetype.Adept);
        mapping.Count.Should().Be(5);
        mapping.RecommendedFirst.Should().Be("alchemist");
        mapping.HasSpecializations.Should().BeTrue();
        mapping.AvailableSpecializations.Should().Contain("alchemist");
        mapping.AvailableSpecializations.Should().Contain("artificer");
        mapping.AvailableSpecializations.Should().Contain("tactician");
        mapping.AvailableSpecializations.Should().Contain("herbalist");
        mapping.AvailableSpecializations.Should().Contain("chronicler");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // STATIC PROPERTY TESTS — Cross-Archetype
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that all canonical archetype mappings report HasSpecializations
    /// as true, since every archetype has at least 2 specializations.
    /// </summary>
    [Test]
    public void AllArchetypes_HaveSpecializations()
    {
        // Arrange & Act & Assert
        ArchetypeSpecializationMapping.Warrior.HasSpecializations.Should().BeTrue();
        ArchetypeSpecializationMapping.Skirmisher.HasSpecializations.Should().BeTrue();
        ArchetypeSpecializationMapping.Mystic.HasSpecializations.Should().BeTrue();
        ArchetypeSpecializationMapping.Adept.HasSpecializations.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that all canonical archetype mappings have the correct
    /// specialization counts: Warrior (6), Adept (5), Skirmisher (4), Mystic (2).
    /// </summary>
    [Test]
    public void AllArchetypes_HaveCorrectCounts()
    {
        // Arrange & Act & Assert
        ArchetypeSpecializationMapping.Warrior.Count.Should().Be(6);
        ArchetypeSpecializationMapping.Skirmisher.Count.Should().Be(4);
        ArchetypeSpecializationMapping.Mystic.Count.Should().Be(2);
        ArchetypeSpecializationMapping.Adept.Count.Should().Be(5);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHOD TESTS — Create
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that <see cref="ArchetypeSpecializationMapping.Create"/> creates
    /// a valid mapping with correct properties when given valid data.
    /// </summary>
    [Test]
    public void Create_WithValidData_CreatesMapping()
    {
        // Arrange & Act
        var mapping = ArchetypeSpecializationMapping.Create(
            Archetype.Warrior,
            new[] { "guardian", "berserker" },
            "guardian");

        // Assert
        mapping.ArchetypeId.Should().Be(Archetype.Warrior);
        mapping.Count.Should().Be(2);
        mapping.RecommendedFirst.Should().Be("guardian");
        mapping.AvailableSpecializations.Should().Contain("guardian");
        mapping.AvailableSpecializations.Should().Contain("berserker");
    }

    /// <summary>
    /// Verifies that <see cref="ArchetypeSpecializationMapping.Create"/> normalizes
    /// all specialization IDs to lowercase for consistent lookups.
    /// </summary>
    [Test]
    public void Create_NormalizesSpecializationIdsToLowercase()
    {
        // Arrange & Act
        var mapping = ArchetypeSpecializationMapping.Create(
            Archetype.Warrior,
            new[] { "Guardian", "BERSERKER", "Weapon-Master" },
            "guardian");

        // Assert
        mapping.AvailableSpecializations.Should().Contain("guardian");
        mapping.AvailableSpecializations.Should().Contain("berserker");
        mapping.AvailableSpecializations.Should().Contain("weapon-master");
        mapping.AvailableSpecializations.Should().NotContain("Guardian");
        mapping.AvailableSpecializations.Should().NotContain("BERSERKER");
    }

    /// <summary>
    /// Verifies that <see cref="ArchetypeSpecializationMapping.Create"/> normalizes
    /// the recommended first specialization ID to lowercase.
    /// </summary>
    [Test]
    public void Create_NormalizesRecommendedFirstToLowercase()
    {
        // Arrange & Act
        var mapping = ArchetypeSpecializationMapping.Create(
            Archetype.Warrior,
            new[] { "guardian", "berserker" },
            "Guardian");

        // Assert
        mapping.RecommendedFirst.Should().Be("guardian");
    }

    /// <summary>
    /// Verifies that <see cref="ArchetypeSpecializationMapping.Create"/> throws
    /// <see cref="ArgumentNullException"/> when availableSpecializations is null.
    /// </summary>
    [Test]
    public void Create_WithNullSpecializations_ThrowsArgumentNullException()
    {
        // Arrange
        Action act = () => ArchetypeSpecializationMapping.Create(
            Archetype.Warrior,
            null!,
            "guardian");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that <see cref="ArchetypeSpecializationMapping.Create"/> throws
    /// <see cref="ArgumentException"/> when availableSpecializations is empty.
    /// </summary>
    [Test]
    public void Create_WithEmptySpecializations_ThrowsArgumentException()
    {
        // Arrange
        Action act = () => ArchetypeSpecializationMapping.Create(
            Archetype.Warrior,
            Array.Empty<string>(),
            "guardian");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*At least one specialization must be available*");
    }

    /// <summary>
    /// Verifies that <see cref="ArchetypeSpecializationMapping.Create"/> throws
    /// <see cref="ArgumentException"/> when recommendedFirst is null.
    /// </summary>
    [Test]
    public void Create_WithNullRecommendedFirst_ThrowsArgumentException()
    {
        // Arrange
        Action act = () => ArchetypeSpecializationMapping.Create(
            Archetype.Warrior,
            new[] { "guardian" },
            null!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Verifies that <see cref="ArchetypeSpecializationMapping.Create"/> throws
    /// <see cref="ArgumentException"/> when recommendedFirst is whitespace.
    /// </summary>
    [Test]
    public void Create_WithWhitespaceRecommendedFirst_ThrowsArgumentException()
    {
        // Arrange
        Action act = () => ArchetypeSpecializationMapping.Create(
            Archetype.Warrior,
            new[] { "guardian" },
            "   ");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Verifies that <see cref="ArchetypeSpecializationMapping.Create"/> throws
    /// <see cref="ArgumentException"/> when the recommended first specialization
    /// is not present in the available specializations list.
    /// </summary>
    [Test]
    public void Create_WithRecommendedNotInList_ThrowsArgumentException()
    {
        // Arrange
        Action act = () => ArchetypeSpecializationMapping.Create(
            Archetype.Warrior,
            new[] { "guardian", "berserker" },
            "invalid-spec");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*must be in available list*");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SPECIALIZATION AVAILABILITY TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that <see cref="ArchetypeSpecializationMapping.IsSpecializationAvailable"/>
    /// returns true for specializations in the archetype's available list.
    /// </summary>
    [Test]
    public void IsSpecializationAvailable_WithValidSpec_ReturnsTrue()
    {
        // Arrange
        var mapping = ArchetypeSpecializationMapping.Warrior;

        // Act & Assert
        mapping.IsSpecializationAvailable("guardian").Should().BeTrue();
        mapping.IsSpecializationAvailable("berserker").Should().BeTrue();
        mapping.IsSpecializationAvailable("weapon-master").Should().BeTrue();
        mapping.IsSpecializationAvailable("vanguard").Should().BeTrue();
        mapping.IsSpecializationAvailable("juggernaut").Should().BeTrue();
        mapping.IsSpecializationAvailable("battle-commander").Should().BeTrue();
    }

    /// <summary>
    /// Verifies that <see cref="ArchetypeSpecializationMapping.IsSpecializationAvailable"/>
    /// returns false for specializations not in the archetype's available list.
    /// </summary>
    [Test]
    public void IsSpecializationAvailable_WithInvalidSpec_ReturnsFalse()
    {
        // Arrange
        var mapping = ArchetypeSpecializationMapping.Warrior;

        // Act & Assert
        mapping.IsSpecializationAvailable("elementalist").Should().BeFalse();
        mapping.IsSpecializationAvailable("shadow-dancer").Should().BeFalse();
        mapping.IsSpecializationAvailable("alchemist").Should().BeFalse();
        mapping.IsSpecializationAvailable("nonexistent").Should().BeFalse();
    }

    /// <summary>
    /// Verifies that <see cref="ArchetypeSpecializationMapping.IsSpecializationAvailable"/>
    /// performs case-insensitive comparison against available specializations.
    /// </summary>
    [Test]
    public void IsSpecializationAvailable_CaseInsensitive_ReturnsTrue()
    {
        // Arrange
        var mapping = ArchetypeSpecializationMapping.Warrior;

        // Act & Assert
        mapping.IsSpecializationAvailable("Guardian").Should().BeTrue();
        mapping.IsSpecializationAvailable("GUARDIAN").Should().BeTrue();
        mapping.IsSpecializationAvailable("BERSERKER").Should().BeTrue();
        mapping.IsSpecializationAvailable("Weapon-Master").Should().BeTrue();
    }

    /// <summary>
    /// Verifies that <see cref="ArchetypeSpecializationMapping.IsSpecializationAvailable"/>
    /// returns false for null, empty, and whitespace input without throwing.
    /// </summary>
    [Test]
    public void IsSpecializationAvailable_WithNullOrWhitespace_ReturnsFalse()
    {
        // Arrange
        var mapping = ArchetypeSpecializationMapping.Warrior;

        // Act & Assert
        mapping.IsSpecializationAvailable(null!).Should().BeFalse();
        mapping.IsSpecializationAvailable("").Should().BeFalse();
        mapping.IsSpecializationAvailable("   ").Should().BeFalse();
    }

    /// <summary>
    /// Verifies that specializations are archetype-exclusive: Mystic's specializations
    /// are not available to Warrior, and vice versa.
    /// </summary>
    [Test]
    public void IsSpecializationAvailable_CrossArchetype_ReturnsFalse()
    {
        // Arrange
        var warrior = ArchetypeSpecializationMapping.Warrior;
        var mystic = ArchetypeSpecializationMapping.Mystic;

        // Act & Assert — Mystic specs not available to Warrior
        warrior.IsSpecializationAvailable("elementalist").Should().BeFalse();
        warrior.IsSpecializationAvailable("void-weaver").Should().BeFalse();

        // Warrior specs not available to Mystic
        mystic.IsSpecializationAvailable("guardian").Should().BeFalse();
        mystic.IsSpecializationAvailable("berserker").Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // GET FOR ARCHETYPE TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that <see cref="ArchetypeSpecializationMapping.GetForArchetype"/>
    /// returns the correct mapping for each archetype with expected counts.
    /// </summary>
    [Test]
    public void GetForArchetype_ReturnsCorrectMapping_ForEachArchetype()
    {
        // Arrange & Act
        var warrior = ArchetypeSpecializationMapping.GetForArchetype(Archetype.Warrior);
        var skirmisher = ArchetypeSpecializationMapping.GetForArchetype(Archetype.Skirmisher);
        var mystic = ArchetypeSpecializationMapping.GetForArchetype(Archetype.Mystic);
        var adept = ArchetypeSpecializationMapping.GetForArchetype(Archetype.Adept);

        // Assert
        warrior.Count.Should().Be(6);
        warrior.RecommendedFirst.Should().Be("guardian");

        skirmisher.Count.Should().Be(4);
        skirmisher.RecommendedFirst.Should().Be("shadow-dancer");

        mystic.Count.Should().Be(2);
        mystic.RecommendedFirst.Should().Be("elementalist");

        adept.Count.Should().Be(5);
        adept.RecommendedFirst.Should().Be("alchemist");
    }

    /// <summary>
    /// Verifies that <see cref="ArchetypeSpecializationMapping.GetForArchetype"/>
    /// throws <see cref="ArgumentOutOfRangeException"/> for invalid archetype values.
    /// </summary>
    [Test]
    public void GetForArchetype_WithInvalidArchetype_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        Action act = () => ArchetypeSpecializationMapping.GetForArchetype((Archetype)99);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // GET AVAILABLE IDS TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that <see cref="ArchetypeSpecializationMapping.GetAvailableIds"/>
    /// returns all specialization IDs for the mapping.
    /// </summary>
    [Test]
    public void GetAvailableIds_ReturnsAllIds()
    {
        // Arrange
        var mapping = ArchetypeSpecializationMapping.Mystic;

        // Act
        var ids = mapping.GetAvailableIds().ToList();

        // Assert
        ids.Should().HaveCount(2);
        ids.Should().Contain("elementalist");
        ids.Should().Contain("void-weaver");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DISPLAY FORMATTING TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that <see cref="ArchetypeSpecializationMapping.GetDisplayList"/>
    /// with highlighting enabled marks the recommended specialization with ★.
    /// </summary>
    [Test]
    public void GetDisplayList_WithHighlight_MarksRecommended()
    {
        // Arrange
        var mapping = ArchetypeSpecializationMapping.Skirmisher;

        // Act
        var displayList = mapping.GetDisplayList(highlightRecommended: true);

        // Assert
        displayList.Should().HaveCount(4);
        displayList[0].Should().Be("★ Shadow Dancer (Recommended)");
        displayList[1].Should().Be("Duelist");
        displayList[2].Should().Be("Ranger");
        displayList[3].Should().Be("Acrobat");
    }

    /// <summary>
    /// Verifies that <see cref="ArchetypeSpecializationMapping.GetDisplayList"/>
    /// with highlighting disabled returns plain Title Case names without markers.
    /// </summary>
    [Test]
    public void GetDisplayList_WithoutHighlight_NoMarker()
    {
        // Arrange
        var mapping = ArchetypeSpecializationMapping.Skirmisher;

        // Act
        var displayList = mapping.GetDisplayList(highlightRecommended: false);

        // Assert
        displayList.Should().HaveCount(4);
        displayList[0].Should().Be("Shadow Dancer");
        displayList[1].Should().Be("Duelist");
        displayList[2].Should().Be("Ranger");
        displayList[3].Should().Be("Acrobat");
    }

    /// <summary>
    /// Verifies that <see cref="ArchetypeSpecializationMapping.GetDisplayList"/>
    /// correctly converts multi-word kebab-case IDs to Title Case display names.
    /// </summary>
    [Test]
    public void GetDisplayList_ConvertsKebabCaseToTitleCase()
    {
        // Arrange
        var mapping = ArchetypeSpecializationMapping.Warrior;

        // Act
        var displayList = mapping.GetDisplayList(highlightRecommended: false);

        // Assert
        displayList.Should().Contain("Weapon Master");
        displayList.Should().Contain("Battle Commander");
    }

    /// <summary>
    /// Verifies that <see cref="ArchetypeSpecializationMapping.GetDisplayList"/>
    /// uses default highlighting (true) when no parameter is specified.
    /// </summary>
    [Test]
    public void GetDisplayList_DefaultHighlight_MarksRecommended()
    {
        // Arrange
        var mapping = ArchetypeSpecializationMapping.Mystic;

        // Act
        var displayList = mapping.GetDisplayList();

        // Assert
        displayList[0].Should().Contain("★");
        displayList[0].Should().Contain("Recommended");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // TOSTRING TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that <see cref="ArchetypeSpecializationMapping.ToString"/> returns
    /// a formatted debug string with archetype, count, and recommended first.
    /// </summary>
    [Test]
    public void ToString_ReturnsFormattedString()
    {
        // Arrange
        var mapping = ArchetypeSpecializationMapping.Warrior;

        // Act
        var result = mapping.ToString();

        // Assert
        result.Should().Be("Warrior: 6 specializations (recommended: guardian)");
    }

    /// <summary>
    /// Verifies that <see cref="ArchetypeSpecializationMapping.ToString"/> returns
    /// correctly formatted output for the Mystic archetype with 2 specializations.
    /// </summary>
    [Test]
    public void ToString_Mystic_ReturnsFormattedString()
    {
        // Arrange
        var mapping = ArchetypeSpecializationMapping.Mystic;

        // Act
        var result = mapping.ToString();

        // Assert
        result.Should().Be("Mystic: 2 specializations (recommended: elementalist)");
    }
}
