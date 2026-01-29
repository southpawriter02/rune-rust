using FluentAssertions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="MythForgedContext"/> value object.
/// </summary>
/// <remarks>
/// Tests cover creation, validation, ID normalization, optional parameters,
/// and string representation for debugging.
/// </remarks>
[TestFixture]
public class MythForgedContextTests
{
    /// <summary>
    /// Verifies that Create with valid data creates a context with correct properties.
    /// </summary>
    [Test]
    public void Create_WithValidData_CreatesContext()
    {
        // Arrange & Act
        var context = MythForgedContext.Create(
            DropSourceType.Boss,
            "shadow-lord",
            "warrior",
            10);

        // Assert
        context.SourceType.Should().Be(DropSourceType.Boss);
        context.SourceId.Should().Be("shadow-lord");
        context.PlayerClassId.Should().Be("warrior");
        context.PlayerLevel.Should().Be(10);
        context.RandomSeed.Should().BeNull();
    }

    /// <summary>
    /// Verifies that Create normalizes source ID and class ID to lowercase.
    /// </summary>
    [Test]
    public void Create_NormalizesIds()
    {
        // Arrange & Act
        var context = MythForgedContext.Create(
            DropSourceType.Boss,
            "SHADOW-LORD",
            "WARRIOR",
            10);

        // Assert
        context.SourceId.Should().Be("shadow-lord");
        context.PlayerClassId.Should().Be("warrior");
    }

    /// <summary>
    /// Verifies that Create allows null player class for universal drops.
    /// </summary>
    [Test]
    public void Create_WithNullPlayerClass_Allowed()
    {
        // Arrange & Act
        var context = MythForgedContext.Create(
            DropSourceType.Container,
            "chest",
            playerClassId: null);

        // Assert
        context.PlayerClassId.Should().BeNull();
        context.PlayerLevel.Should().Be(1); // Default
    }

    /// <summary>
    /// Verifies that Create throws ArgumentOutOfRangeException for invalid level.
    /// </summary>
    [Test]
    public void Create_WithInvalidLevel_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => MythForgedContext.Create(
            DropSourceType.Boss,
            "boss",
            "warrior",
            playerLevel: 0);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
           .WithParameterName("playerLevel");
    }

    /// <summary>
    /// Verifies that Create throws ArgumentException for null source ID.
    /// </summary>
    [Test]
    public void Create_WithNullSourceId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => MythForgedContext.Create(
            DropSourceType.Boss,
            null!,
            "warrior",
            10);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithParameterName("sourceId");
    }

    /// <summary>
    /// Verifies that Create throws ArgumentException for empty source ID.
    /// </summary>
    [Test]
    public void Create_WithEmptySourceId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => MythForgedContext.Create(
            DropSourceType.Boss,
            "",
            "warrior",
            10);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithParameterName("sourceId");
    }

    /// <summary>
    /// Verifies that Create throws ArgumentException for whitespace source ID.
    /// </summary>
    [Test]
    public void Create_WithWhitespaceSourceId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => MythForgedContext.Create(
            DropSourceType.Boss,
            "   ",
            "warrior",
            10);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithParameterName("sourceId");
    }

    /// <summary>
    /// Verifies that Create with random seed stores the value correctly.
    /// </summary>
    [Test]
    public void Create_WithRandomSeed_StoresValue()
    {
        // Arrange & Act
        var context = MythForgedContext.Create(
            DropSourceType.Boss,
            "boss",
            randomSeed: 42);

        // Assert
        context.RandomSeed.Should().Be(42);
    }

    /// <summary>
    /// Verifies that ToString returns a properly formatted string.
    /// </summary>
    [Test]
    public void ToString_ReturnsFormattedString()
    {
        // Arrange
        var context = MythForgedContext.Create(
            DropSourceType.Boss,
            "shadow-lord",
            "warrior",
            10);

        // Act
        var result = context.ToString();

        // Assert
        result.Should().Be("MythForgedContext[Boss:shadow-lord, Class:warrior, Lv:10]");
    }

    /// <summary>
    /// Verifies that ToString handles null class correctly.
    /// </summary>
    [Test]
    public void ToString_WithNullClass_ShowsNone()
    {
        // Arrange
        var context = MythForgedContext.Create(
            DropSourceType.Container,
            "chest");

        // Act
        var result = context.ToString();

        // Assert
        result.Should().Be("MythForgedContext[Container:chest, Class:none, Lv:1]");
    }

    /// <summary>
    /// Verifies that two contexts with same values are equal.
    /// </summary>
    [Test]
    public void Equality_WithSameValues_AreEqual()
    {
        // Arrange
        var context1 = MythForgedContext.Create(DropSourceType.Boss, "shadow-lord", "warrior", 10, 42);
        var context2 = MythForgedContext.Create(DropSourceType.Boss, "shadow-lord", "warrior", 10, 42);

        // Assert
        context1.Should().Be(context2);
        (context1 == context2).Should().BeTrue();
    }

    /// <summary>
    /// Verifies that two contexts with different values are not equal.
    /// </summary>
    [Test]
    public void Equality_WithDifferentValues_AreNotEqual()
    {
        // Arrange
        var context1 = MythForgedContext.Create(DropSourceType.Boss, "shadow-lord", "warrior", 10);
        var context2 = MythForgedContext.Create(DropSourceType.Boss, "shadow-lord", "mage", 10);

        // Assert
        context1.Should().NotBe(context2);
        (context1 != context2).Should().BeTrue();
    }

    /// <summary>
    /// Verifies different source types are captured correctly.
    /// </summary>
    [TestCase(DropSourceType.Monster)]
    [TestCase(DropSourceType.Container)]
    [TestCase(DropSourceType.Boss)]
    [TestCase(DropSourceType.Vendor)]
    [TestCase(DropSourceType.Quest)]
    public void Create_WithDifferentSourceTypes_CapturesCorrectly(DropSourceType sourceType)
    {
        // Arrange & Act
        var context = MythForgedContext.Create(sourceType, "test-source");

        // Assert
        context.SourceType.Should().Be(sourceType);
    }
}
