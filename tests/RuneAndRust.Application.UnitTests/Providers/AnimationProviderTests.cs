using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Infrastructure.Providers;

namespace RuneAndRust.Application.UnitTests.Providers;

/// <summary>
/// Unit tests for <see cref="AnimationProvider"/>.
/// </summary>
[TestFixture]
public class AnimationProviderTests
{
    private AnimationProvider _provider = null!;

    [SetUp]
    public void Setup()
    {
        _provider = new AnimationProvider();
    }

    #region GetAnimation Tests

    [Test]
    public void GetAnimation_AttackHit_ReturnsDefinition()
    {
        // Act
        var result = _provider.GetAnimation(AnimationType.AttackHit);

        // Assert
        result.Should().NotBeNull();
        result!.Type.Should().Be(AnimationType.AttackHit);
        result.Frames.Should().NotBeEmpty();
    }

    [Test]
    public void GetAnimation_AttackHit_HasVerbs()
    {
        // Act
        var result = _provider.GetAnimation(AnimationType.AttackHit);

        // Assert
        result!.Verbs.Should().NotBeNull();
        result.Verbs.Should().Contain("SLASH");
    }

    [Test]
    public void GetAnimation_CriticalHit_ReturnsDefinition()
    {
        // Act
        var result = _provider.GetAnimation(AnimationType.CriticalHit);

        // Assert
        result.Should().NotBeNull();
        result!.Frames.Any(f => f.TextTemplate.Contains("CRITICAL")).Should().BeTrue();
    }

    [Test]
    public void GetAnimation_Death_ReturnsDefinition()
    {
        // Act
        var result = _provider.GetAnimation(AnimationType.Death);

        // Assert
        result.Should().NotBeNull();
        result!.Frames.Any(f => f.TextTemplate.Contains("DEFEATED")).Should().BeTrue();
    }

    #endregion

    #region AvailableAnimations Tests

    [Test]
    public void AvailableAnimations_ContainsAllTypes()
    {
        // Act
        var available = _provider.AvailableAnimations;

        // Assert
        available.Should().Contain(AnimationType.AttackHit);
        available.Should().Contain(AnimationType.AttackMiss);
        available.Should().Contain(AnimationType.CriticalHit);
        available.Should().Contain(AnimationType.Heal);
        available.Should().Contain(AnimationType.Death);
    }

    [Test]
    public void AvailableAnimations_HasCorrectCount()
    {
        // Act
        var available = _provider.AvailableAnimations;

        // Assert
        available.Count.Should().Be(10); // All AnimationType values
    }

    #endregion
}
