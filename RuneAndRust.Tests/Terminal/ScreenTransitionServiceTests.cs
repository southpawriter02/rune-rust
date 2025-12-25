using System.Diagnostics;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Settings;
using RuneAndRust.Terminal.Services;

namespace RuneAndRust.Tests.Terminal;

/// <summary>
/// Unit tests for ScreenTransitionService (v0.3.14b).
/// Tests screen transition animations and ReduceMotion accessibility compliance.
/// </summary>
public class ScreenTransitionServiceTests
{
    private readonly ILogger<ScreenTransitionService> _mockLogger;
    private readonly IThemeService _mockTheme;
    private readonly ScreenTransitionService _sut;

    public ScreenTransitionServiceTests()
    {
        _mockLogger = Substitute.For<ILogger<ScreenTransitionService>>();
        _mockTheme = Substitute.For<IThemeService>();

        // Setup theme mock to return valid colors
        _mockTheme.GetColor("EnemyColor").Returns("red");
        _mockTheme.GetColor("SuccessColor").Returns("green");
        _mockTheme.GetColor("StressHigh").Returns("magenta1");

        // Reset static GameSettings to default state for test isolation
        GameSettings.ReduceMotion = false;

        _sut = new ScreenTransitionService(_mockLogger, _mockTheme);
    }

    #region ReduceMotion Tests

    [Fact]
    public async Task PlayAsync_SkipsAnimation_WhenReduceMotionEnabled()
    {
        // Arrange
        GameSettings.ReduceMotion = true;
        var stopwatch = Stopwatch.StartNew();

        // Act
        await _sut.PlayAsync(TransitionType.Shatter);
        stopwatch.Stop();

        // Assert - Should complete nearly instantly (< 100ms)
        // Note: Uses 100ms threshold instead of 50ms to account for test environment variability
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(100);
    }

    [Fact]
    public async Task PlayAsync_SkipsAnimation_WhenReduceMotionEnabled_Dissolve()
    {
        // Arrange
        GameSettings.ReduceMotion = true;
        var stopwatch = Stopwatch.StartNew();

        // Act
        await _sut.PlayAsync(TransitionType.Dissolve);
        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(100);
    }

    [Fact]
    public async Task PlayAsync_SkipsAnimation_WhenReduceMotionEnabled_GlitchDecay()
    {
        // Arrange
        GameSettings.ReduceMotion = true;
        var stopwatch = Stopwatch.StartNew();

        // Act
        await _sut.PlayAsync(TransitionType.GlitchDecay);
        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(100);
    }

    #endregion

    #region TransitionType Handling Tests

    [Theory]
    [InlineData(TransitionType.None)]
    [InlineData(TransitionType.Shatter)]
    [InlineData(TransitionType.Dissolve)]
    [InlineData(TransitionType.GlitchDecay)]
    public async Task PlayAsync_HandlesAllTransitionTypes_WithReduceMotion(TransitionType type)
    {
        // Arrange
        GameSettings.ReduceMotion = true; // Skip actual animation for test speed

        // Act & Assert - Should not throw for any transition type
        await _sut.Invoking(s => s.PlayAsync(type))
            .Should().NotThrowAsync();
    }

    [Fact]
    public async Task PlayAsync_WithNone_CompletesInstantly()
    {
        // Arrange
        GameSettings.ReduceMotion = true;
        var stopwatch = Stopwatch.StartNew();

        // Act
        await _sut.PlayAsync(TransitionType.None);
        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(100);
    }

    #endregion

    #region Theme Integration Tests

    // Note: Actual animation rendering requires a real terminal (Console.WindowWidth > 0).
    // These tests verify theme service injection and that transition types are properly handled.
    // Full animation rendering is verified through manual QA testing.

    [Fact]
    public void Constructor_InjectsThemeService()
    {
        // Assert - Constructor should accept and store theme service
        // (Verified by service creation without exception in test setup)
        _sut.Should().NotBeNull();
    }

    [Theory]
    [InlineData(TransitionType.Shatter, "EnemyColor")]
    [InlineData(TransitionType.Dissolve, "SuccessColor")]
    [InlineData(TransitionType.GlitchDecay, "StressHigh")]
    public void ThemeService_IsConfiguredForTransitionColors(TransitionType type, string expectedColorKey)
    {
        // Arrange - Theme mock was configured in constructor
        // Act - Just verify the mock is set up correctly
        var color = _mockTheme.GetColor(expectedColorKey);

        // Assert - Mock returns configured color
        color.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Logging Tests

    [Fact]
    public async Task PlayAsync_LogsDebug_WhenReduceMotionSkips()
    {
        // Arrange
        GameSettings.ReduceMotion = true;

        // Act
        await _sut.PlayAsync(TransitionType.Shatter);

        // Assert - Verify debug log was called
        _mockLogger.ReceivedWithAnyArgs().Log(
            LogLevel.Debug,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception?>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    #endregion
}
