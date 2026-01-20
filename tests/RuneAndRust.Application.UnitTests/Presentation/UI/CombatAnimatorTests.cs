using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.DTOs;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;
using RuneAndRust.Infrastructure.Providers;
using RuneAndRust.Presentation.UI;

namespace RuneAndRust.Application.UnitTests.Presentation.UI;

/// <summary>
/// Unit tests for <see cref="CombatAnimator"/>.
/// </summary>
[TestFixture]
public class CombatAnimatorTests
{
    private Mock<ITerminalService> _mockTerminal = null!;
    private IAnimationProvider _provider = null!;
    private ScreenLayout _layout = null!;
    private CombatAnimator _animator = null!;

    [SetUp]
    public void Setup()
    {
        _mockTerminal = new Mock<ITerminalService>();
        _mockTerminal.Setup(t => t.SupportsUnicode).Returns(true);
        
        _provider = new AnimationProvider();
        
        var mockLogger = new Mock<ILogger<ScreenLayout>>();
        _layout = new ScreenLayout(_mockTerminal.Object, mockLogger.Object);
        
        _animator = new CombatAnimator(_mockTerminal.Object, _provider, _layout);
    }
    
    [TearDown]
    public void TearDown()
    {
        _layout?.Dispose();
    }

    #region QueueAnimation Tests

    [Test]
    public void QueueAnimation_AddsToQueue()
    {
        // Arrange
        var context = new AnimationContext("Hero", "Skeleton", 15);

        // Act
        _animator.QueueAnimation(AnimationType.AttackHit, context);

        // Assert
        _animator.QueueCount.Should().Be(1);
    }

    [Test]
    public void QueueAnimation_DisabledAnimations_DoesNotAdd()
    {
        // Arrange
        _animator.AnimationsEnabled = false;
        var context = new AnimationContext("Hero");

        // Act
        _animator.QueueAnimation(AnimationType.AttackHit, context);

        // Assert
        _animator.QueueCount.Should().Be(0);
    }

    [Test]
    public void QueueAnimation_MultipleAnimations_AllQueued()
    {
        // Arrange
        var context1 = new AnimationContext("Hero", "Skeleton", 15);
        var context2 = new AnimationContext("Hero", "Goblin", 8);

        // Act
        _animator.QueueAnimation(AnimationType.AttackHit, context1);
        _animator.QueueAnimation(AnimationType.AttackMiss, context2);

        // Assert
        _animator.QueueCount.Should().Be(2);
    }

    #endregion

    #region ClearQueue Tests

    [Test]
    public void ClearQueue_RemovesAllAnimations()
    {
        // Arrange
        _animator.QueueAnimation(AnimationType.AttackHit, new AnimationContext("Hero"));
        _animator.QueueAnimation(AnimationType.Death, new AnimationContext("Hero", "Skeleton"));

        // Act
        _animator.ClearQueue();

        // Assert
        _animator.QueueCount.Should().Be(0);
    }

    #endregion

    #region Property Tests

    [Test]
    public void AnimationsEnabled_DefaultIsTrue()
    {
        _animator.AnimationsEnabled.Should().BeTrue();
    }

    [Test]
    public void SpeedMultiplier_DefaultIsOne()
    {
        _animator.SpeedMultiplier.Should().Be(1.0f);
    }

    [Test]
    public void SpeedMultiplier_CanBeModified()
    {
        // Act
        _animator.SpeedMultiplier = 2.0f;

        // Assert
        _animator.SpeedMultiplier.Should().Be(2.0f);
    }

    [Test]
    public void IsPlaying_DefaultIsFalse()
    {
        _animator.IsPlaying.Should().BeFalse();
    }

    #endregion

    #region SkipCurrent Tests

    [Test]
    public void SkipCurrent_DoesNotThrow()
    {
        // Act & Assert
        var act = () => _animator.SkipCurrent();
        act.Should().NotThrow();
    }

    #endregion
}
