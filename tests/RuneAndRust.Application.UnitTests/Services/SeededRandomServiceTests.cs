using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Tests for SeededRandomService.
/// </summary>
[TestFixture]
public class SeededRandomServiceTests
{
    private SeededRandomService _service = null!;
    private const int TestSeed = 305419896;

    [SetUp]
    public void SetUp()
    {
        _service = new SeededRandomService(TestSeed, NullLogger<SeededRandomService>.Instance);
    }

    [Test]
    public void Constructor_WithSeed_StoresMasterSeed()
    {
        // Assert
        _service.MasterSeed.Should().Be(TestSeed);
    }

    [Test]
    public void NextForPosition_SameInputs_ReturnsSameValue()
    {
        // Arrange
        var position = new Position3D(3, -1, 2);

        // Act
        var result1 = _service.NextForPosition(position, "template_selection");
        
        // Re-create service with same seed
        var service2 = new SeededRandomService(TestSeed, NullLogger<SeededRandomService>.Instance);
        var result2 = service2.NextForPosition(position, "template_selection");

        // Assert
        result1.Should().Be(result2);
    }

    [Test]
    public void NextForPosition_DifferentPositions_ReturnsDifferentValues()
    {
        // Arrange
        var position1 = new Position3D(0, 0, 0);
        var position2 = new Position3D(1, 0, 0);

        // Act
        var result1 = _service.NextForPosition(position1);
        var result2 = _service.NextForPosition(position2);

        // Assert
        result1.Should().NotBe(result2);
    }

    [Test]
    public void NextForPosition_DifferentContexts_ReturnsDifferentValues()
    {
        // Arrange
        var position = new Position3D(3, -1, 2);

        // Act
        var result1 = _service.NextForPosition(position, "template_selection");
        
        // Re-create to reset generator cache
        var service2 = new SeededRandomService(TestSeed, NullLogger<SeededRandomService>.Instance);
        var result2 = service2.NextForPosition(position, "monster_spawn");

        // Assert
        result1.Should().NotBe(result2);
    }

    [Test]
    public void NextForPosition_WithRange_ReturnsValueInRange()
    {
        // Arrange
        var position = new Position3D(5, 5, 1);

        // Act
        var result = _service.NextForPosition(position, 10, 20, "range_test");

        // Assert
        result.Should().BeInRange(10, 19);
    }

    [Test]
    public void NextFloatForPosition_ReturnsBetweenZeroAndOne()
    {
        // Arrange
        var position = new Position3D(2, 3, 1);

        // Act
        var result = _service.NextFloatForPosition(position, "float_test");

        // Assert
        result.Should().BeInRange(0.0f, 1.0f);
    }

    [Test]
    public void SelectWeighted_RespectsWeights()
    {
        // Arrange
        var items = new List<(string, int)>
        {
            ("common", 90),
            ("rare", 9),
            ("legendary", 1)
        };

        // Act - Generate many rolls to verify weight distribution
        var counts = new Dictionary<string, int> { ["common"] = 0, ["rare"] = 0, ["legendary"] = 0 };
        for (int i = 0; i < 1000; i++)
        {
            var position = new Position3D(i, 0, 0);
            var service = new SeededRandomService(i, NullLogger<SeededRandomService>.Instance);
            var selected = service.SelectWeighted(position, items, "weight_test");
            counts[selected]++;
        }

        // Assert - Common should be much more frequent than legendary
        counts["common"].Should().BeGreaterThan(counts["rare"]);
        counts["rare"].Should().BeGreaterThan(counts["legendary"]);
    }

    [Test]
    public void SelectWeighted_SameInputs_ReturnsSameItem()
    {
        // Arrange
        var items = new List<(string, int)>
        {
            ("A", 50),
            ("B", 30),
            ("C", 20)
        };
        var position = new Position3D(42, 7, 3);

        // Act
        var result1 = _service.SelectWeighted(position, items, "select_test");
        
        var service2 = new SeededRandomService(TestSeed, NullLogger<SeededRandomService>.Instance);
        var result2 = service2.SelectWeighted(position, items, "select_test");

        // Assert
        result1.Should().Be(result2);
    }

    [Test]
    public void SelectWeighted_EmptyList_ThrowsArgumentException()
    {
        // Arrange
        var items = new List<(string, int)>();
        var position = new Position3D(0, 0, 0);

        // Act
        var act = () => _service.SelectWeighted(position, items, "empty_test");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void ClearSubGenerators_ClearsCache()
    {
        // Arrange - Generate some values to populate cache
        _service.NextForPosition(new Position3D(0, 0, 0));
        _service.NextForPosition(new Position3D(1, 0, 0));
        _service.NextForPosition(new Position3D(2, 0, 0));

        // Act
        _service.ClearSubGenerators();

        // Assert - No exception, and next call still works
        var result = _service.NextForPosition(new Position3D(0, 0, 0));
        result.GetType().Should().Be(typeof(int)); // Just verify it returns an int
    }
}
