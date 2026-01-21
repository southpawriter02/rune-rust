// ═══════════════════════════════════════════════════════════════════════════════
// AnimationOptimizerTests.cs
// Unit tests for AnimationOptimizer utility.
// Version: 0.13.5f
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Presentation.Shared.Interfaces;
using RuneAndRust.Presentation.Shared.Utilities;

namespace RuneAndRust.Presentation.Shared.UnitTests.Utilities;

/// <summary>
/// Unit tests for <see cref="AnimationOptimizer"/>.
/// </summary>
[TestFixture]
public class AnimationOptimizerTests
{
    private Mock<ILogger> _mockLogger = null!;
    private Mock<IAccessibilityService> _mockAccessibility = null!;

    [SetUp]
    public void SetUp()
    {
        _mockLogger = new Mock<ILogger>();
        _mockAccessibility = new Mock<IAccessibilityService>();
    }

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Constructor_WithDefaultParameters_CreatesInstance()
    {
        // Act
        var optimizer = new AnimationOptimizer();

        // Assert
        optimizer.Should().NotBeNull();
    }

    [Test]
    public void Constructor_WithAccessibilityService_SetsUpCorrectly()
    {
        // Arrange
        _mockAccessibility.Setup(a => a.IsReducedMotionEnabled).Returns(true);

        // Act
        var optimizer = new AnimationOptimizer(accessibility: _mockAccessibility.Object);

        // Assert
        optimizer.ShouldAnimate().Should().BeFalse();
    }

    [Test]
    public void Constructor_WithLogger_DoesNotThrow()
    {
        // Act
        var act = () => new AnimationOptimizer(logger: _mockLogger.Object);

        // Assert
        act.Should().NotThrow();
    }

    // ═══════════════════════════════════════════════════════════════
    // REDUCED MOTION TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void ShouldAnimate_WhenReducedMotionEnabled_ReturnsFalse()
    {
        // Arrange
        _mockAccessibility.Setup(a => a.IsReducedMotionEnabled).Returns(true);
        var optimizer = new AnimationOptimizer(accessibility: _mockAccessibility.Object);

        // Act
        var result = optimizer.ShouldAnimate();

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void ShouldAnimate_WhenReducedMotionDisabled_ReturnsTrue()
    {
        // Arrange
        _mockAccessibility.Setup(a => a.IsReducedMotionEnabled).Returns(false);
        var optimizer = new AnimationOptimizer(accessibility: _mockAccessibility.Object);

        // Act
        var result = optimizer.ShouldAnimate();

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public void ShouldAnimate_WhenNoAccessibilityService_ReturnsTrue()
    {
        // Arrange
        var optimizer = new AnimationOptimizer();

        // Act
        var result = optimizer.ShouldAnimate();

        // Assert
        result.Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════════════════
    // DURATION OPTIMIZATION TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void GetOptimizedDuration_WhenReducedMotion_ReturnsZero()
    {
        // Arrange
        _mockAccessibility.Setup(a => a.IsReducedMotionEnabled).Returns(true);
        var optimizer = new AnimationOptimizer(accessibility: _mockAccessibility.Object);
        var baseDuration = TimeSpan.FromMilliseconds(500);

        // Act
        var result = optimizer.GetOptimizedDuration(baseDuration);

        // Assert
        result.Should().Be(TimeSpan.Zero);
    }

    [Test]
    public void GetOptimizedDuration_WhenNormalMode_ReturnsBaseDuration()
    {
        // Arrange
        _mockAccessibility.Setup(a => a.IsReducedMotionEnabled).Returns(false);
        var optimizer = new AnimationOptimizer(accessibility: _mockAccessibility.Object);
        var baseDuration = TimeSpan.FromMilliseconds(500);

        // Act
        var result = optimizer.GetOptimizedDuration(baseDuration);

        // Assert
        // At 60 FPS (default), should return base duration
        result.Should().Be(baseDuration);
    }

    [Test]
    public void GetOptimizedDuration_WithZeroDuration_ReturnsZero()
    {
        // Arrange
        var optimizer = new AnimationOptimizer();

        // Act
        var result = optimizer.GetOptimizedDuration(TimeSpan.Zero);

        // Assert
        result.Should().Be(TimeSpan.Zero);
    }

    // ═══════════════════════════════════════════════════════════════
    // FRAME RATE TRACKING TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void CurrentFps_InitiallyReturnsTargetFps()
    {
        // Arrange
        var optimizer = new AnimationOptimizer();

        // Act
        var fps = optimizer.CurrentFps;

        // Assert
        fps.Should().BeApproximately(60.0, 1.0); // 60 FPS target
    }

    [Test]
    public void TrackFrame_DoesNotThrow()
    {
        // Arrange
        var optimizer = new AnimationOptimizer();

        // Act
        var act = () => optimizer.TrackFrame();

        // Assert
        act.Should().NotThrow();
    }

    [Test]
    public void GetFrameSkip_AtTargetFrameRate_ReturnsZero()
    {
        // Arrange
        var optimizer = new AnimationOptimizer();

        // Act - At default (60 FPS assumption)
        var frameSkip = optimizer.GetFrameSkip();

        // Assert
        frameSkip.Should().Be(0);
    }

    [Test]
    public void IsLowPowerMode_Initially_ReturnsFalse()
    {
        // Arrange
        var optimizer = new AnimationOptimizer();

        // Act & Assert
        optimizer.IsLowPowerMode.Should().BeFalse();
    }

    [Test]
    public void AverageFrameTimeMs_Initially_ReturnsTargetFrameTime()
    {
        // Arrange
        var optimizer = new AnimationOptimizer();

        // Act & Assert - 60 FPS = ~16.67ms
        optimizer.AverageFrameTimeMs.Should().BeApproximately(16.67, 0.1);
    }

    // ═══════════════════════════════════════════════════════════════
    // RESET TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Reset_RestoresToInitialState()
    {
        // Arrange
        var optimizer = new AnimationOptimizer();
        // Track some frames to change state
        for (int i = 0; i < 10; i++)
        {
            optimizer.TrackFrame();
            Thread.Sleep(10);
        }

        // Act
        optimizer.Reset();

        // Assert
        optimizer.AverageFrameTimeMs.Should().BeApproximately(16.67, 0.1);
    }

    // ═══════════════════════════════════════════════════════════════
    // INTEGRATION TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void AnimationOptimizer_WithFullConfiguration_WorksCorrectly()
    {
        // Arrange
        _mockAccessibility.Setup(a => a.IsReducedMotionEnabled).Returns(false);
        var optimizer = new AnimationOptimizer(
            accessibility: _mockAccessibility.Object,
            logger: _mockLogger.Object);

        // Act
        var shouldAnimate = optimizer.ShouldAnimate();
        var duration = optimizer.GetOptimizedDuration(TimeSpan.FromMilliseconds(300));
        var frameSkip = optimizer.GetFrameSkip();

        // Assert
        shouldAnimate.Should().BeTrue();
        duration.TotalMilliseconds.Should().BeGreaterThan(0);
        frameSkip.Should().BeGreaterThanOrEqualTo(0);
    }

    [Test]
    public void AnimationOptimizer_AllMethodsCallableInSequence()
    {
        // Arrange
        var optimizer = new AnimationOptimizer();

        // Act - Simulate typical usage pattern
        optimizer.TrackFrame();
        var shouldAnimate = optimizer.ShouldAnimate();
        TimeSpan duration = TimeSpan.Zero;
        if (shouldAnimate)
        {
            duration = optimizer.GetOptimizedDuration(TimeSpan.FromMilliseconds(200));
        }
        var frameSkip = optimizer.GetFrameSkip();
        optimizer.Reset();

        // Assert - All operations complete without error
        shouldAnimate.Should().BeTrue();
        duration.TotalMilliseconds.Should().BeGreaterThanOrEqualTo(0);
        frameSkip.Should().BeGreaterThanOrEqualTo(0);
    }
}
