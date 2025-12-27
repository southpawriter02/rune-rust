using Microsoft.Extensions.Logging;
using NSubstitute;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Settings;
using RuneAndRust.Terminal.Services;
using Xunit;

namespace RuneAndRust.Tests.Terminal;

/// <summary>
/// Unit tests for VisualEffectService (v0.3.9a).
/// Tests border flash effects, ReduceMotion accessibility, and color mapping.
/// </summary>
public class VisualEffectServiceTests
{
    private readonly ILogger<VisualEffectService> _logger;
    private readonly VisualEffectService _service;

    public VisualEffectServiceTests()
    {
        _logger = Substitute.For<ILogger<VisualEffectService>>();
        _service = new VisualEffectService(_logger);

        // Reset GameSettings for each test
        GameSettings.ReduceMotion = false;
    }

    #region SetBorderOverride Tests

    [Fact]
    public void SetBorderOverride_StoresColor()
    {
        // Act
        _service.SetBorderOverride("red");

        // Assert
        Assert.Equal("red", _service.GetBorderOverride());
    }

    [Fact]
    public void SetBorderOverride_AcceptsNull()
    {
        // Arrange
        _service.SetBorderOverride("red");

        // Act
        _service.SetBorderOverride(null);

        // Assert
        Assert.Null(_service.GetBorderOverride());
    }

    [Fact]
    public void SetBorderOverride_OverwritesPreviousColor()
    {
        // Arrange
        _service.SetBorderOverride("red");

        // Act
        _service.SetBorderOverride("gold1");

        // Assert
        Assert.Equal("gold1", _service.GetBorderOverride());
    }

    #endregion

    #region GetBorderOverride Tests

    [Fact]
    public void GetBorderOverride_ReturnsNull_WhenNoOverrideSet()
    {
        // Assert
        Assert.Null(_service.GetBorderOverride());
    }

    [Fact]
    public void GetBorderOverride_ReturnsSetColor()
    {
        // Arrange
        _service.SetBorderOverride("magenta1");

        // Assert
        Assert.Equal("magenta1", _service.GetBorderOverride());
    }

    #endregion

    #region ClearBorderOverride Tests

    [Fact]
    public void ClearBorderOverride_RemovesOverride()
    {
        // Arrange
        _service.SetBorderOverride("red");

        // Act
        _service.ClearBorderOverride();

        // Assert
        Assert.Null(_service.GetBorderOverride());
    }

    [Fact]
    public void ClearBorderOverride_IsIdempotent()
    {
        // Act - Clear when no override is set
        _service.ClearBorderOverride();

        // Assert
        Assert.Null(_service.GetBorderOverride());
    }

    #endregion

    #region TriggerEffectAsync Tests

    [Fact]
    public async Task TriggerEffectAsync_SkipsEffect_WhenReduceMotionEnabled()
    {
        // Arrange
        GameSettings.ReduceMotion = true;

        // Act
        await _service.TriggerEffectAsync(VisualEffectType.DamageFlash);

        // Assert - Override should never have been set
        Assert.Null(_service.GetBorderOverride());
    }

    [Fact]
    public async Task TriggerEffectAsync_SkipsEffect_WhenEffectTypeIsNone()
    {
        // Act
        await _service.TriggerEffectAsync(VisualEffectType.None);

        // Assert
        Assert.Null(_service.GetBorderOverride());
    }

    [Fact]
    public async Task TriggerEffectAsync_SetsOverrideWithExpiry_ThenCheckExpiredClears()
    {
        // Act - v0.3.23b: TriggerEffectAsync now sets expiry instead of auto-clearing
        await _service.TriggerEffectAsync(VisualEffectType.DamageFlash, intensity: 1);

        // Assert - Override still set after await (expiry-based clearing)
        // The override persists until CheckExpiredOverrides() is called and time has passed
        // Since we just awaited the delay, calling CheckExpiredOverrides should clear it
        var cleared = _service.CheckExpiredOverrides();
        Assert.True(cleared, "Override should have expired and been cleared");
        Assert.Null(_service.GetBorderOverride());
    }

    [Theory]
    [InlineData(VisualEffectType.DamageFlash, "red")]
    [InlineData(VisualEffectType.CriticalFlash, "gold1")]
    [InlineData(VisualEffectType.HealFlash, "green")]
    [InlineData(VisualEffectType.TraumaFlash, "magenta1")]
    [InlineData(VisualEffectType.VictoryFlash, "bold gold1")]
    public async Task TriggerEffectAsync_UsesCorrectColor_ForEffectType(VisualEffectType effectType, string expectedColor)
    {
        // Arrange - We'll capture the color during the effect
        string? capturedColor = null;

        // Act - Start the effect and immediately capture the color
        var effectTask = _service.TriggerEffectAsync(effectType, intensity: 1);

        // Small delay to ensure the color is set before we capture it
        await Task.Delay(10);
        capturedColor = _service.GetBorderOverride();

        // Wait for effect to complete
        await effectTask;

        // Assert
        Assert.Equal(expectedColor, capturedColor);
    }

    [Theory]
    [InlineData(1, 150)]
    [InlineData(2, 300)]
    [InlineData(3, 450)]
    public async Task TriggerEffectAsync_ScalesDuration_WithIntensity(int intensity, int expectedMinDuration)
    {
        // Arrange
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        await _service.TriggerEffectAsync(VisualEffectType.DamageFlash, intensity);
        stopwatch.Stop();

        // Assert - Duration should be at least the expected minimum
        // Allow some tolerance for test execution overhead
        Assert.True(stopwatch.ElapsedMilliseconds >= expectedMinDuration - 50,
            $"Effect duration {stopwatch.ElapsedMilliseconds}ms was less than expected minimum {expectedMinDuration}ms");
    }

    [Fact]
    public async Task TriggerEffectAsync_ClampsIntensity_ToMinimum()
    {
        // Arrange
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act - Pass 0 intensity (should be clamped to 1)
        await _service.TriggerEffectAsync(VisualEffectType.DamageFlash, intensity: 0);
        stopwatch.Stop();

        // Assert - Should use minimum intensity (150ms)
        Assert.True(stopwatch.ElapsedMilliseconds >= 100,
            $"Effect duration {stopwatch.ElapsedMilliseconds}ms suggests intensity was not clamped");
    }

    [Fact]
    public async Task TriggerEffectAsync_ClampsIntensity_ToMaximum()
    {
        // Arrange
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act - Pass 10 intensity (should be clamped to 3)
        await _service.TriggerEffectAsync(VisualEffectType.DamageFlash, intensity: 10);
        stopwatch.Stop();

        // Assert - Should use maximum intensity (450ms), not 1500ms
        Assert.True(stopwatch.ElapsedMilliseconds < 600,
            $"Effect duration {stopwatch.ElapsedMilliseconds}ms suggests intensity was not clamped to max");
    }

    #endregion

    #region GameSettings Integration Tests

    [Fact]
    public async Task TriggerEffectAsync_RespectReduceMotion_ChangedMidSession()
    {
        // Arrange - Start with effects enabled
        GameSettings.ReduceMotion = false;

        // Act - First effect should work
        await _service.TriggerEffectAsync(VisualEffectType.DamageFlash);

        // Change setting
        GameSettings.ReduceMotion = true;

        // Store before calling
        _service.SetBorderOverride("test");

        // Second effect should be skipped
        await _service.TriggerEffectAsync(VisualEffectType.CriticalFlash);

        // Assert - The "test" override should still be there since effect was skipped
        Assert.Equal("test", _service.GetBorderOverride());
    }

    #endregion

    #region v0.3.23b: OnInvalidateVisuals Event Tests

    [Fact]
    public void SetBorderOverride_RaisesOnInvalidateVisuals()
    {
        // Arrange
        var eventRaised = false;
        _service.OnInvalidateVisuals += () => eventRaised = true;

        // Act
        _service.SetBorderOverride("red");

        // Assert
        Assert.True(eventRaised);
    }

    [Fact]
    public void ClearBorderOverride_RaisesOnInvalidateVisuals()
    {
        // Arrange
        _service.SetBorderOverride("red");
        var eventRaised = false;
        _service.OnInvalidateVisuals += () => eventRaised = true;

        // Act
        _service.ClearBorderOverride();

        // Assert
        Assert.True(eventRaised);
    }

    [Fact]
    public async Task TriggerEffectAsync_RaisesOnInvalidateVisuals()
    {
        // Arrange
        var eventRaised = false;
        _service.OnInvalidateVisuals += () => eventRaised = true;

        // Act
        await _service.TriggerEffectAsync(VisualEffectType.DamageFlash, intensity: 1);

        // Assert
        Assert.True(eventRaised);
    }

    #endregion

    #region v0.3.23b: CheckExpiredOverrides Tests

    [Fact]
    public void CheckExpiredOverrides_ReturnsFalse_WhenNoOverride()
    {
        // Act
        var result = _service.CheckExpiredOverrides();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CheckExpiredOverrides_ReturnsFalse_WhenOverrideNotExpired()
    {
        // Arrange - Set a manual override (no expiry)
        _service.SetBorderOverride("red");

        // Act
        var result = _service.CheckExpiredOverrides();

        // Assert - Manual overrides don't expire
        Assert.False(result);
        Assert.Equal("red", _service.GetBorderOverride());
    }

    [Fact]
    public async Task CheckExpiredOverrides_ReturnsTrue_AndClears_WhenExpired()
    {
        // Arrange - Trigger an effect with short duration
        await _service.TriggerEffectAsync(VisualEffectType.DamageFlash, intensity: 1);

        // Act - The effect has now expired (we awaited the delay)
        var result = _service.CheckExpiredOverrides();

        // Assert
        Assert.True(result);
        Assert.Null(_service.GetBorderOverride());
    }

    [Fact]
    public async Task CheckExpiredOverrides_RaisesOnInvalidateVisuals_WhenClearing()
    {
        // Arrange
        await _service.TriggerEffectAsync(VisualEffectType.DamageFlash, intensity: 1);
        var eventRaised = false;
        _service.OnInvalidateVisuals += () => eventRaised = true;

        // Act
        _service.CheckExpiredOverrides();

        // Assert
        Assert.True(eventRaised);
    }

    #endregion
}
