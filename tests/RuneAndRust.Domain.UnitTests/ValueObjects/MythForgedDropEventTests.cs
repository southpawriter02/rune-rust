namespace RuneAndRust.Domain.UnitTests.ValueObjects;

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="MythForgedDropEvent"/> value object.
/// </summary>
/// <remarks>
/// These tests verify:
/// <list type="bullet">
///   <item><description>Valid drop events are created correctly</description></item>
///   <item><description>Factory method validates input parameters</description></item>
///   <item><description>Atmospheric text varies by source type</description></item>
///   <item><description>First drop message is returned only when appropriate</description></item>
///   <item><description>Source IDs are normalized to lowercase</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class MythForgedDropEventTests
{
    #region Create - Valid Cases

    /// <summary>
    /// Verifies that a valid MythForgedDropEvent is created with all properties set correctly.
    /// </summary>
    [Test]
    public void Create_WithValidData_CreatesEvent()
    {
        // Arrange
        var item = CreateTestUniqueItem();

        // Act
        var dropEvent = MythForgedDropEvent.Create(
            item,
            DropSourceType.Boss,
            "shadow-lord",
            isFirstOfRun: true,
            playerLevel: 10);

        // Assert
        dropEvent.Item.Should().Be(item);
        dropEvent.SourceType.Should().Be(DropSourceType.Boss);
        dropEvent.SourceId.Should().Be("shadow-lord");
        dropEvent.IsFirstOfRun.Should().BeTrue();
        dropEvent.PlayerLevel.Should().Be(10);
        dropEvent.DroppedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    /// <summary>
    /// Verifies that source IDs are normalized to lowercase.
    /// </summary>
    [Test]
    public void Create_NormalizesSourceIdToLowercase()
    {
        // Arrange
        var item = CreateTestUniqueItem();

        // Act
        var dropEvent = MythForgedDropEvent.Create(
            item,
            DropSourceType.Boss,
            "SHADOW-LORD");

        // Assert
        dropEvent.SourceId.Should().Be("shadow-lord");
    }

    /// <summary>
    /// Verifies that custom timestamp is preserved.
    /// </summary>
    [Test]
    public void Create_WithCustomTimestamp_PreservesTimestamp()
    {
        // Arrange
        var item = CreateTestUniqueItem();
        var customTime = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        // Act
        var dropEvent = MythForgedDropEvent.Create(
            item,
            DropSourceType.Boss,
            "boss",
            droppedAt: customTime);

        // Assert
        dropEvent.DroppedAt.Should().Be(customTime);
    }

    /// <summary>
    /// Verifies that default values are applied correctly.
    /// </summary>
    [Test]
    public void Create_WithDefaults_SetsDefaultValues()
    {
        // Arrange
        var item = CreateTestUniqueItem();

        // Act
        var dropEvent = MythForgedDropEvent.Create(
            item,
            DropSourceType.Monster,
            "goblin");

        // Assert
        dropEvent.IsFirstOfRun.Should().BeFalse();
        dropEvent.PlayerLevel.Should().Be(1);
    }

    #endregion

    #region Create - Validation

    /// <summary>
    /// Verifies that null item throws ArgumentNullException.
    /// </summary>
    [Test]
    public void Create_WithNullItem_ThrowsArgumentNullException()
    {
        // Act
        var act = () => MythForgedDropEvent.Create(
            null!,
            DropSourceType.Boss,
            "boss");

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("item");
    }

    /// <summary>
    /// Verifies that null source ID throws ArgumentException.
    /// </summary>
    [Test]
    public void Create_WithNullSourceId_ThrowsArgumentException()
    {
        // Arrange
        var item = CreateTestUniqueItem();

        // Act
        var act = () => MythForgedDropEvent.Create(
            item,
            DropSourceType.Boss,
            null!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("sourceId");
    }

    /// <summary>
    /// Verifies that empty source ID throws ArgumentException.
    /// </summary>
    [Test]
    public void Create_WithEmptySourceId_ThrowsArgumentException()
    {
        // Arrange
        var item = CreateTestUniqueItem();

        // Act
        var act = () => MythForgedDropEvent.Create(
            item,
            DropSourceType.Boss,
            "");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Verifies that whitespace source ID throws ArgumentException.
    /// </summary>
    [Test]
    public void Create_WithWhitespaceSourceId_ThrowsArgumentException()
    {
        // Arrange
        var item = CreateTestUniqueItem();

        // Act
        var act = () => MythForgedDropEvent.Create(
            item,
            DropSourceType.Boss,
            "   ");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Verifies that player level less than 1 throws ArgumentOutOfRangeException.
    /// </summary>
    [Test]
    public void Create_WithZeroPlayerLevel_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var item = CreateTestUniqueItem();

        // Act
        var act = () => MythForgedDropEvent.Create(
            item,
            DropSourceType.Boss,
            "boss",
            playerLevel: 0);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("playerLevel");
    }

    /// <summary>
    /// Verifies that negative player level throws ArgumentOutOfRangeException.
    /// </summary>
    [Test]
    public void Create_WithNegativePlayerLevel_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var item = CreateTestUniqueItem();

        // Act
        var act = () => MythForgedDropEvent.Create(
            item,
            DropSourceType.Boss,
            "boss",
            playerLevel: -5);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    #endregion

    #region GetAtmosphericText Tests

    /// <summary>
    /// Verifies that boss source returns expected atmospheric text.
    /// </summary>
    [Test]
    public void GetAtmosphericText_ForBoss_ReturnsExpectedText()
    {
        // Arrange
        var dropEvent = MythForgedDropEvent.Create(
            CreateTestUniqueItem(),
            DropSourceType.Boss,
            "boss");

        // Act
        var text = dropEvent.GetAtmosphericText();

        // Assert
        text.Should().Contain("ancient power");
        text.Should().Contain("legendary relic");
    }

    /// <summary>
    /// Verifies that container source returns expected atmospheric text.
    /// </summary>
    [Test]
    public void GetAtmosphericText_ForContainer_ReturnsExpectedText()
    {
        // Arrange
        var dropEvent = MythForgedDropEvent.Create(
            CreateTestUniqueItem(),
            DropSourceType.Container,
            "chest");

        // Act
        var text = dropEvent.GetAtmosphericText();

        // Assert
        text.Should().Contain("golden light");
        text.Should().Contain("legendary awaits");
    }

    /// <summary>
    /// Verifies that quest source returns expected atmospheric text.
    /// </summary>
    [Test]
    public void GetAtmosphericText_ForQuest_ReturnsExpectedText()
    {
        // Arrange
        var dropEvent = MythForgedDropEvent.Create(
            CreateTestUniqueItem(),
            DropSourceType.Quest,
            "main-quest");

        // Act
        var text = dropEvent.GetAtmosphericText();

        // Assert
        text.Should().Contain("rewarded");
        text.Should().Contain("legend");
    }

    /// <summary>
    /// Verifies that monster source returns expected atmospheric text.
    /// </summary>
    [Test]
    public void GetAtmosphericText_ForMonster_ReturnsExpectedText()
    {
        // Arrange
        var dropEvent = MythForgedDropEvent.Create(
            CreateTestUniqueItem(),
            DropSourceType.Monster,
            "dragon");

        // Act
        var text = dropEvent.GetAtmosphericText();

        // Assert
        text.Should().Contain("fallen foe");
        text.Should().Contain("treasure");
    }

    /// <summary>
    /// Verifies that vendor source returns expected atmospheric text.
    /// </summary>
    [Test]
    public void GetAtmosphericText_ForVendor_ReturnsExpectedText()
    {
        // Arrange
        var dropEvent = MythForgedDropEvent.Create(
            CreateTestUniqueItem(),
            DropSourceType.Vendor,
            "blacksmith");

        // Act
        var text = dropEvent.GetAtmosphericText();

        // Assert
        text.Should().Contain("merchant");
        text.Should().Contain("finest work");
    }

    #endregion

    #region GetFirstDropMessage Tests

    /// <summary>
    /// Verifies that first drop message is returned when IsFirstOfRun is true.
    /// </summary>
    [Test]
    public void GetFirstDropMessage_WhenFirstOfRun_ReturnsMessage()
    {
        // Arrange
        var dropEvent = MythForgedDropEvent.Create(
            CreateTestUniqueItem(),
            DropSourceType.Boss,
            "boss",
            isFirstOfRun: true);

        // Act
        var message = dropEvent.GetFirstDropMessage();

        // Assert
        message.Should().NotBeNull();
        message.Should().Contain("first legendary");
    }

    /// <summary>
    /// Verifies that null is returned when IsFirstOfRun is false.
    /// </summary>
    [Test]
    public void GetFirstDropMessage_WhenNotFirst_ReturnsNull()
    {
        // Arrange
        var dropEvent = MythForgedDropEvent.Create(
            CreateTestUniqueItem(),
            DropSourceType.Boss,
            "boss",
            isFirstOfRun: false);

        // Act
        var message = dropEvent.GetFirstDropMessage();

        // Assert
        message.Should().BeNull();
    }

    #endregion

    #region ToString Tests

    /// <summary>
    /// Verifies that ToString returns a useful representation.
    /// </summary>
    [Test]
    public void ToString_ReturnsFormattedString()
    {
        // Arrange
        var dropEvent = MythForgedDropEvent.Create(
            CreateTestUniqueItem(),
            DropSourceType.Boss,
            "shadow-lord");

        // Act
        var result = dropEvent.ToString();

        // Assert
        result.Should().Contain("MythForgedDropEvent");
        result.Should().Contain("test-item");
        result.Should().Contain("Boss");
        result.Should().Contain("shadow-lord");
    }

    #endregion

    #region TimeSinceDrop Tests

    /// <summary>
    /// Verifies that TimeSinceDrop returns a reasonable timespan.
    /// </summary>
    [Test]
    public void TimeSinceDrop_ReturnsPositiveTimespan()
    {
        // Arrange
        var dropEvent = MythForgedDropEvent.Create(
            CreateTestUniqueItem(),
            DropSourceType.Boss,
            "boss",
            droppedAt: DateTime.UtcNow.AddMinutes(-5));

        // Act
        var elapsed = dropEvent.TimeSinceDrop;

        // Assert
        elapsed.TotalMinutes.Should().BeApproximately(5, 0.1);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Creates a test UniqueItem for use in tests.
    /// </summary>
    private static UniqueItem CreateTestUniqueItem() =>
        UniqueItem.Create(
            itemId: "test-item",
            name: "Test Item",
            description: "A test item for unit testing.",
            flavorText: "Testing is the path to quality.",
            category: EquipmentCategory.Weapon,
            stats: ItemStats.Create(might: 5, bonusDamage: 10),
            dropSources: new[] { DropSource.Create(DropSourceType.Boss, "test-boss", 5.0m) },
            classAffinities: new[] { "warrior" });

    #endregion
}
