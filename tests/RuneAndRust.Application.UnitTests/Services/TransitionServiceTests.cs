using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Application.Services;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Tests for TransitionService.
/// </summary>
[TestFixture]
public class TransitionServiceTests
{
    private TransitionService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _service = new TransitionService();
    }

    [Test]
    public void GetTransition_ExistingPair_ReturnsTransition()
    {
        // Act
        var transition = _service.GetTransition("stone-corridors", "fungal-caverns");

        // Assert
        transition.Should().NotBeNull();
        transition!.SourceBiomeId.Should().Be("stone-corridors");
    }

    [Test]
    public void GetTransition_NonExistingPair_ReturnsNull()
    {
        // Act
        var transition = _service.GetTransition("unknown", "biome");

        // Assert
        transition.Should().BeNull();
    }

    [Test]
    public void GetConnectableBiomes_ReturnsValidTargets()
    {
        // Act
        var biomes = _service.GetConnectableBiomes("stone-corridors", depth: 4);

        // Assert
        biomes.Should().Contain("fungal-caverns");
        biomes.Should().Contain("flooded-depths");
    }

    [Test]
    public void CreateBlend_MidPoint_ReturnsHalfRatio()
    {
        // Act (room 1 of 3 = index 1, total 3 → ratio 0.5)
        var blend = _service.CreateBlend("stone", "fungal", 1, 3);

        // Assert
        blend.Ratio.Should().Be(0.5f);
    }

    [Test]
    public void IsTransitionAllowed_AllowedTransition_ReturnsTrue()
    {
        // Act
        var allowed = _service.IsTransitionAllowed("stone-corridors", "fungal-caverns", depth: 3);

        // Assert
        allowed.Should().BeTrue();
    }

    [Test]
    public void IsTransitionAllowed_ForbiddenTransition_ReturnsFalse()
    {
        // Act (volcanic ↔ flooded is not allowed)
        var allowed = _service.IsTransitionAllowed("volcanic-caverns", "flooded-depths", depth: 5);

        // Assert
        allowed.Should().BeFalse();
    }
}
