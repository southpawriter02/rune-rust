using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Tests for ArchitecturalStyleService.
/// </summary>
[TestFixture]
public class ArchitecturalStyleServiceTests
{
    private SeededRandomService _random = null!;
    private ArchitecturalStyleService _service = null!;
    private const int TestSeed = 12345;

    [SetUp]
    public void SetUp()
    {
        _random = new SeededRandomService(TestSeed, NullLogger<SeededRandomService>.Instance);
        _service = new ArchitecturalStyleService(_random);
    }

    [Test]
    public void GetStyle_ExistingId_ReturnsStyle()
    {
        // Act
        var style = _service.GetStyle("rough-hewn");

        // Assert
        style.Should().NotBeNull();
        style!.Name.Should().Be("Rough-Hewn");
    }

    [Test]
    public void GetStyle_UnknownId_ReturnsNull()
    {
        // Act
        var style = _service.GetStyle("nonexistent");

        // Assert
        style.Should().BeNull();
    }

    [Test]
    public void GetStylesForBiome_ReturnsCompatibleStyles()
    {
        // Act
        var styles = _service.GetStylesForBiome("fungal-caverns");

        // Assert
        styles.Should().Contain(s => s.StyleId == "fungal-growth");
        styles.Should().Contain(s => s.StyleId == "rough-hewn"); // Universal
    }

    [Test]
    public void GetStylesForDepth_FiltersCorrectly()
    {
        // Act
        var shallowStyles = _service.GetStylesForDepth(1);
        var deepStyles = _service.GetStylesForDepth(5);

        // Assert
        shallowStyles.Should().Contain(s => s.StyleId == "rough-hewn");
        deepStyles.Should().Contain(s => s.StyleId == "ornate-temple");
    }

    [Test]
    public void SelectStyleForPosition_ReturnsValidStyle()
    {
        // Arrange
        var position = new Position3D(5, 5, -3);

        // Act
        var styleId = _service.SelectStyleForPosition(position, "stone-corridors");

        // Assert
        styleId.Should().NotBeNullOrEmpty();
        var style = _service.GetStyle(styleId);
        style.Should().NotBeNull();
    }

    [Test]
    public void GetRandomDescriptor_ReturnsDescriptor()
    {
        // Arrange
        var position = new Position3D(5, 5, -1);

        // Act
        var desc = _service.GetRandomDescriptor("rough-hewn", "walls", position);

        // Assert
        desc.Should().NotBeNullOrEmpty();
    }
}
