namespace RuneAndRust.Application.UnitTests.Services;

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="LegendaryPresentationService"/> class.
/// </summary>
/// <remarks>
/// These tests verify:
/// <list type="bullet">
///   <item><description>Drop announcements contain all required elements</description></item>
///   <item><description>Stat lines are formatted correctly</description></item>
///   <item><description>Item headers include tier prefix</description></item>
///   <item><description>Class affinities are formatted properly</description></item>
///   <item><description>Announcement frames are generated correctly</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class LegendaryPresentationServiceTests
{
    private LegendaryPresentationService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _service = new LegendaryPresentationService();
    }

    #region FormatDropAnnouncement Tests

    /// <summary>
    /// Verifies that drop announcement contains the legendary header.
    /// </summary>
    [Test]
    public void FormatDropAnnouncement_ContainsLegendaryHeader()
    {
        // Arrange
        var item = CreateTestUniqueItem();
        var dropEvent = MythForgedDropEvent.Create(item, DropSourceType.Boss, "boss");

        // Act
        var result = _service.FormatDropAnnouncement(dropEvent);

        // Assert
        result.Should().Contain("LEGENDARY DROP");
        result.Should().Contain("✦");
    }

    /// <summary>
    /// Verifies that drop announcement contains the item name.
    /// </summary>
    [Test]
    public void FormatDropAnnouncement_ContainsItemName()
    {
        // Arrange
        var item = CreateTestUniqueItem();
        var dropEvent = MythForgedDropEvent.Create(item, DropSourceType.Boss, "boss");

        // Act
        var result = _service.FormatDropAnnouncement(dropEvent);

        // Assert
        result.Should().Contain(item.Name);
    }

    /// <summary>
    /// Verifies that drop announcement contains the tier prefix.
    /// </summary>
    [Test]
    public void FormatDropAnnouncement_ContainsTierPrefix()
    {
        // Arrange
        var item = CreateTestUniqueItem();
        var dropEvent = MythForgedDropEvent.Create(item, DropSourceType.Boss, "boss");

        // Act
        var result = _service.FormatDropAnnouncement(dropEvent);

        // Assert
        result.Should().Contain("[Myth-Forged]");
    }

    /// <summary>
    /// Verifies that drop announcement contains atmospheric text.
    /// </summary>
    [Test]
    public void FormatDropAnnouncement_ContainsAtmosphericText()
    {
        // Arrange
        var item = CreateTestUniqueItem();
        var dropEvent = MythForgedDropEvent.Create(item, DropSourceType.Boss, "boss");

        // Act
        var result = _service.FormatDropAnnouncement(dropEvent);

        // Assert
        result.Should().Contain("ancient power");
    }

    /// <summary>
    /// Verifies that drop announcement contains first drop message when applicable.
    /// </summary>
    [Test]
    public void FormatDropAnnouncement_WhenFirstOfRun_ContainsFirstDropMessage()
    {
        // Arrange
        var item = CreateTestUniqueItem();
        var dropEvent = MythForgedDropEvent.Create(item, DropSourceType.Boss, "boss", isFirstOfRun: true);

        // Act
        var result = _service.FormatDropAnnouncement(dropEvent);

        // Assert
        result.Should().Contain("first legendary");
    }

    /// <summary>
    /// Verifies that drop announcement contains borders.
    /// </summary>
    [Test]
    public void FormatDropAnnouncement_ContainsBorders()
    {
        // Arrange
        var item = CreateTestUniqueItem();
        var dropEvent = MythForgedDropEvent.Create(item, DropSourceType.Boss, "boss");

        // Act
        var result = _service.FormatDropAnnouncement(dropEvent);

        // Assert
        result.Should().Contain("═══");
    }

    #endregion

    #region FormatExaminationText Tests

    /// <summary>
    /// Verifies that examination text contains item name and tier prefix.
    /// </summary>
    [Test]
    public void FormatExaminationText_ContainsItemHeaderWithTierPrefix()
    {
        // Arrange
        var item = CreateTestUniqueItem();

        // Act
        var result = _service.FormatExaminationText(item);

        // Assert
        result.Should().Contain("[Myth-Forged]");
        result.Should().Contain(item.Name);
    }

    /// <summary>
    /// Verifies that examination text contains item description.
    /// </summary>
    [Test]
    public void FormatExaminationText_ContainsDescription()
    {
        // Arrange
        var item = CreateTestUniqueItem();

        // Act
        var result = _service.FormatExaminationText(item);

        // Assert
        result.Should().Contain(item.Description);
    }

    /// <summary>
    /// Verifies that examination text contains flavor text when present.
    /// </summary>
    [Test]
    public void FormatExaminationText_ContainsFlavorText_WhenPresent()
    {
        // Arrange
        var item = CreateTestUniqueItem();

        // Act
        var result = _service.FormatExaminationText(item);

        // Assert
        result.Should().Contain(item.FlavorText);
    }

    /// <summary>
    /// Verifies that examination text contains stats section.
    /// </summary>
    [Test]
    public void FormatExaminationText_ContainsStatsSection()
    {
        // Arrange
        var item = CreateTestUniqueItem();

        // Act
        var result = _service.FormatExaminationText(item);

        // Assert
        result.Should().Contain("STATS");
        result.Should().Contain("Might");
    }

    /// <summary>
    /// Verifies that examination text contains requirements.
    /// </summary>
    [Test]
    public void FormatExaminationText_ContainsRequirements()
    {
        // Arrange
        var item = CreateTestUniqueItem();

        // Act
        var result = _service.FormatExaminationText(item);

        // Assert
        result.Should().Contain("Requirements");
        result.Should().Contain("Level");
    }

    #endregion

    #region FormatStatLine Tests

    /// <summary>
    /// Verifies that stat line formats multiple stats with pipe separators.
    /// </summary>
    [Test]
    public void FormatStatLine_WithMultipleStats_FormatsCorrectly()
    {
        // Arrange
        var stats = ItemStats.Create(might: 5, agility: 3, bonusDamage: 10);

        // Act
        var result = _service.FormatStatLine(stats);

        // Assert
        result.Should().Contain("+5 MIGHT");
        result.Should().Contain("+3 AGI");
        result.Should().Contain("+10 Damage");
        result.Should().Contain("|");
    }

    /// <summary>
    /// Verifies that stat line only includes non-zero stats.
    /// </summary>
    [Test]
    public void FormatStatLine_OmitsZeroStats()
    {
        // Arrange
        var stats = ItemStats.Create(might: 5);

        // Act
        var result = _service.FormatStatLine(stats);

        // Assert
        result.Should().Contain("+5 MIGHT");
        result.Should().NotContain("AGI");
        result.Should().NotContain("WILL");
    }

    /// <summary>
    /// Verifies that stat line returns placeholder for all-zero stats.
    /// </summary>
    [Test]
    public void FormatStatLine_WithAllZeroStats_ReturnsNoStatBonuses()
    {
        // Arrange
        var stats = ItemStats.Create();

        // Act
        var result = _service.FormatStatLine(stats);

        // Assert
        result.Should().Be("No stat bonuses");
    }

    /// <summary>
    /// Verifies that negative stats are formatted correctly.
    /// </summary>
    [Test]
    public void FormatStatLine_WithNegativeStats_FormatsWithMinus()
    {
        // Arrange
        var stats = ItemStats.Create(might: -2);

        // Act
        var result = _service.FormatStatLine(stats);

        // Assert
        result.Should().Contain("-2 MIGHT");
    }

    #endregion

    #region FormatItemHeader Tests

    /// <summary>
    /// Verifies that item header includes tier prefix and name.
    /// </summary>
    [Test]
    public void FormatItemHeader_IncludesTierPrefixAndName()
    {
        // Arrange
        var item = CreateTestUniqueItem();

        // Act
        var result = _service.FormatItemHeader(item);

        // Assert
        result.Should().Be("[Myth-Forged] Test Blade");
    }

    #endregion

    #region FormatClassAffinities Tests

    /// <summary>
    /// Verifies that empty class list returns "All Classes".
    /// </summary>
    [Test]
    public void FormatClassAffinities_WithEmptyList_ReturnsAllClasses()
    {
        // Act
        var result = _service.FormatClassAffinities(Array.Empty<string>());

        // Assert
        result.Should().Be("All Classes");
    }

    /// <summary>
    /// Verifies that class IDs are capitalized properly.
    /// </summary>
    [Test]
    public void FormatClassAffinities_CapitalizesClassNames()
    {
        // Arrange
        var classes = new[] { "warrior", "paladin" };

        // Act
        var result = _service.FormatClassAffinities(classes);

        // Assert
        result.Should().Contain("Warrior");
        result.Should().Contain("Paladin");
    }

    /// <summary>
    /// Verifies that kebab-case class IDs are converted to title case.
    /// </summary>
    [Test]
    public void FormatClassAffinities_HandlesKebabCase()
    {
        // Arrange
        var classes = new[] { "battle-mage", "shadow-knight" };

        // Act
        var result = _service.FormatClassAffinities(classes);

        // Assert
        result.Should().Contain("Battle Mage");
        result.Should().Contain("Shadow Knight");
    }

    #endregion

    #region GetAnnouncementFrame Tests

    /// <summary>
    /// Verifies that announcement frame returns matching top and bottom borders.
    /// </summary>
    [Test]
    public void GetAnnouncementFrame_ReturnsMatchingBorders()
    {
        // Act
        var (top, bottom) = _service.GetAnnouncementFrame();

        // Assert
        top.Should().Be(bottom);
    }

    /// <summary>
    /// Verifies that announcement frame uses the specified width.
    /// </summary>
    [Test]
    public void GetAnnouncementFrame_UsesSpecifiedWidth()
    {
        // Act
        var (top, _) = _service.GetAnnouncementFrame(50);

        // Assert
        top.Length.Should().Be(50);
    }

    /// <summary>
    /// Verifies that announcement frame uses default width of 47.
    /// </summary>
    [Test]
    public void GetAnnouncementFrame_UsesDefaultWidth()
    {
        // Act
        var (top, _) = _service.GetAnnouncementFrame();

        // Assert
        top.Length.Should().Be(47);
    }

    /// <summary>
    /// Verifies that announcement frame uses double-line characters.
    /// </summary>
    [Test]
    public void GetAnnouncementFrame_UsesDoubleLineCharacters()
    {
        // Act
        var (top, _) = _service.GetAnnouncementFrame();

        // Assert
        top.Should().Contain("═");
    }

    #endregion

    #region FormatEffectList Tests

    /// <summary>
    /// Verifies that empty effect list returns placeholder text.
    /// </summary>
    [Test]
    public void FormatEffectList_WithEmptyList_ReturnsNoSpecialEffects()
    {
        // Act
        var result = _service.FormatEffectList(Array.Empty<SpecialEffect>());

        // Assert
        result.Should().Be("No special effects");
    }

    /// <summary>
    /// Verifies that effects are formatted with bullet points.
    /// </summary>
    [Test]
    public void FormatEffectList_FormatsEffectsWithBullets()
    {
        // Arrange
        var effects = new[]
        {
            SpecialEffect.Create("life-steal", SpecialEffectType.LifeSteal, EffectTriggerType.OnDamageDealt, 0.15m)
        };

        // Act
        var result = _service.FormatEffectList(effects);

        // Assert
        result.Should().Contain("✦");
        result.Should().Contain("Life Steal");
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Creates a test UniqueItem for use in tests.
    /// </summary>
    private static UniqueItem CreateTestUniqueItem() =>
        UniqueItem.Create(
            itemId: "test-blade",
            name: "Test Blade",
            description: "A legendary blade forged in the heart of a dying star.",
            flavorText: "The blade sings with ancient power.",
            category: EquipmentCategory.Weapon,
            stats: ItemStats.Create(might: 5, agility: 2, bonusDamage: 10),
            dropSources: new[] { DropSource.Create(DropSourceType.Boss, "test-boss", 5.0m) },
            classAffinities: new[] { "warrior", "paladin" },
            requiredLevel: 10,
            specialEffectIds: new[] { "life-steal" });

    #endregion
}
