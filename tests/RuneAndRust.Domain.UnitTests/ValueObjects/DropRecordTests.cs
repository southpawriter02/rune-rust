using FluentAssertions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="DropRecord"/> value object.
/// </summary>
/// <remarks>
/// Tests cover validation, ID normalization, timestamp handling,
/// and the ToString() method for debugging.
/// </remarks>
[TestFixture]
public class DropRecordTests
{
    /// <summary>
    /// Verifies that Create with valid data creates a DropRecord with correct properties.
    /// </summary>
    [Test]
    public void Create_WithValidData_CreatesRecord()
    {
        // Arrange & Act
        var record = DropRecord.Create(
            "shadowfang-blade",
            DropSourceType.Boss,
            "shadow-lord");

        // Assert
        record.ItemId.Should().Be("shadowfang-blade");
        record.SourceType.Should().Be(DropSourceType.Boss);
        record.SourceId.Should().Be("shadow-lord");
        record.DroppedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    /// <summary>
    /// Verifies that Create normalizes item ID and source ID to lowercase.
    /// </summary>
    [Test]
    public void Create_NormalizesIds()
    {
        // Arrange & Act
        var record = DropRecord.Create(
            "SHADOWFANG-BLADE",
            DropSourceType.Boss,
            "SHADOW-LORD");

        // Assert
        record.ItemId.Should().Be("shadowfang-blade");
        record.SourceId.Should().Be("shadow-lord");
    }

    /// <summary>
    /// Verifies that Create throws ArgumentException when itemId is null.
    /// </summary>
    [Test]
    public void Create_WithNullItemId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => DropRecord.Create(null!, DropSourceType.Boss, "boss-id");

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithParameterName("itemId");
    }

    /// <summary>
    /// Verifies that Create throws ArgumentException when itemId is empty.
    /// </summary>
    [Test]
    public void Create_WithEmptyItemId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => DropRecord.Create("", DropSourceType.Boss, "boss-id");

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithParameterName("itemId");
    }

    /// <summary>
    /// Verifies that Create throws ArgumentException when sourceId is null.
    /// </summary>
    [Test]
    public void Create_WithNullSourceId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => DropRecord.Create("item-id", DropSourceType.Boss, null!);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithParameterName("sourceId");
    }

    /// <summary>
    /// Verifies that Create throws ArgumentException when sourceId is whitespace.
    /// </summary>
    [Test]
    public void Create_WithWhitespaceSourceId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => DropRecord.Create("item-id", DropSourceType.Boss, "   ");

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithParameterName("sourceId");
    }

    /// <summary>
    /// Verifies that Create with a custom timestamp uses the provided time.
    /// </summary>
    [Test]
    public void Create_WithCustomTimestamp_UsesProvidedTime()
    {
        // Arrange
        var timestamp = new DateTime(2025, 1, 15, 12, 0, 0, DateTimeKind.Utc);

        // Act
        var record = DropRecord.Create(
            "item-id",
            DropSourceType.Container,
            "chest-id",
            timestamp);

        // Assert
        record.DroppedAt.Should().Be(timestamp);
    }

    /// <summary>
    /// Verifies that ToString returns a properly formatted string.
    /// </summary>
    [Test]
    public void ToString_ReturnsFormattedString()
    {
        // Arrange
        var timestamp = new DateTime(2025, 1, 15, 12, 0, 0, DateTimeKind.Utc);
        var record = DropRecord.Create(
            "shadowfang-blade",
            DropSourceType.Boss,
            "shadow-lord",
            timestamp);

        // Act
        var result = record.ToString();

        // Assert
        result.Should().Contain("shadowfang-blade");
        result.Should().Contain("Boss");
        result.Should().Contain("shadow-lord");
        result.Should().Contain("2025-01-15");
    }

    /// <summary>
    /// Verifies that two DropRecords with the same values are equal (record struct semantics).
    /// </summary>
    [Test]
    public void Equality_WithSameValues_AreEqual()
    {
        // Arrange
        var timestamp = new DateTime(2025, 1, 15, 12, 0, 0, DateTimeKind.Utc);
        var record1 = DropRecord.Create("shadowfang-blade", DropSourceType.Boss, "shadow-lord", timestamp);
        var record2 = DropRecord.Create("shadowfang-blade", DropSourceType.Boss, "shadow-lord", timestamp);

        // Assert
        record1.Should().Be(record2);
        (record1 == record2).Should().BeTrue();
    }

    /// <summary>
    /// Verifies that two DropRecords with different values are not equal.
    /// </summary>
    [Test]
    public void Equality_WithDifferentValues_AreNotEqual()
    {
        // Arrange
        var timestamp = new DateTime(2025, 1, 15, 12, 0, 0, DateTimeKind.Utc);
        var record1 = DropRecord.Create("item-a", DropSourceType.Boss, "boss-1", timestamp);
        var record2 = DropRecord.Create("item-b", DropSourceType.Boss, "boss-1", timestamp);

        // Assert
        record1.Should().NotBe(record2);
        (record1 != record2).Should().BeTrue();
    }

    /// <summary>
    /// Verifies that different source types are captured correctly.
    /// </summary>
    [TestCase(DropSourceType.Monster)]
    [TestCase(DropSourceType.Container)]
    [TestCase(DropSourceType.Boss)]
    [TestCase(DropSourceType.Vendor)]
    [TestCase(DropSourceType.Quest)]
    public void Create_WithDifferentSourceTypes_CapturesCorrectly(DropSourceType sourceType)
    {
        // Arrange & Act
        var record = DropRecord.Create("test-item", sourceType, "test-source");

        // Assert
        record.SourceType.Should().Be(sourceType);
    }
}
