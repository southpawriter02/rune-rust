using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;
using RuneAndRust.Core.Settings;
using RuneAndRust.Core.ValueObjects;
using RuneAndRust.Engine.Services;
using Xunit;

namespace RuneAndRust.Tests.Engine;

/// <summary>
/// Tests for the AmbienceService class (v0.3.19c).
/// Validates turn-based ambient soundscape generation.
/// </summary>
public class AmbienceServiceTests : IDisposable
{
    private readonly Mock<IAudioService> _mockAudioService;
    private readonly Mock<IRoomRepository> _mockRoomRepository;
    private readonly Mock<ILogger<AmbienceService>> _mockLogger;
    private readonly IServiceScopeFactory _scopeFactory;

    // Store original setting to restore after tests
    private readonly bool _originalAmbienceEnabled;

    public AmbienceServiceTests()
    {
        _mockAudioService = new Mock<IAudioService>();
        _mockRoomRepository = new Mock<IRoomRepository>();
        _mockLogger = new Mock<ILogger<AmbienceService>>();

        // Store original value
        _originalAmbienceEnabled = GameSettings.AmbienceEnabled;

        // Set up mock scope factory
        var mockScope = new Mock<IServiceScope>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IRoomRepository)))
            .Returns(_mockRoomRepository.Object);
        mockScope.Setup(s => s.ServiceProvider).Returns(mockServiceProvider.Object);

        var mockScopeFactory = new Mock<IServiceScopeFactory>();
        mockScopeFactory.Setup(f => f.CreateScope()).Returns(mockScope.Object);
        _scopeFactory = mockScopeFactory.Object;
    }

    public void Dispose()
    {
        // Restore original setting
        GameSettings.AmbienceEnabled = _originalAmbienceEnabled;
    }

    private AmbienceService CreateService()
    {
        return new AmbienceService(
            _mockAudioService.Object,
            _scopeFactory,
            _mockLogger.Object);
    }

    #region IsEnabled Tests

    [Fact]
    public void IsEnabled_ReturnsTrue_WhenGameSettingsAmbienceEnabled()
    {
        // Arrange
        GameSettings.AmbienceEnabled = true;
        var sut = CreateService();

        // Act & Assert
        sut.IsEnabled.Should().BeTrue();
    }

    [Fact]
    public void IsEnabled_ReturnsFalse_WhenGameSettingsAmbienceDisabled()
    {
        // Arrange
        GameSettings.AmbienceEnabled = false;
        var sut = CreateService();

        // Act & Assert
        sut.IsEnabled.Should().BeFalse();
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WhenDisabled_DoesNotPlaySound()
    {
        // Arrange
        GameSettings.AmbienceEnabled = false;
        var sut = CreateService();
        var roomId = Guid.NewGuid();

        // Act - Call many times to ensure no sound plays
        for (int i = 0; i < 10; i++)
        {
            await sut.UpdateAsync(roomId);
        }

        // Assert
        _mockAudioService.Verify(
            a => a.PlayAsync(It.IsAny<SoundCue>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenEnabled_EventuallyPlaysSound()
    {
        // Arrange
        GameSettings.AmbienceEnabled = true;
        var sut = CreateService();
        var roomId = Guid.NewGuid();

        var room = new Room
        {
            Id = roomId,
            Name = "Test Room",
            Position = Coordinate.Origin,
            BiomeType = BiomeType.Ruin
        };

        _mockRoomRepository
            .Setup(r => r.GetByIdAsync(roomId))
            .ReturnsAsync(room);

        // Act - Call enough times to trigger (max interval is 5)
        for (int i = 0; i < 6; i++)
        {
            await sut.UpdateAsync(roomId);
        }

        // Assert - Should have played at least once
        _mockAudioService.Verify(
            a => a.PlayAsync(It.Is<SoundCue>(c => c.Category == SoundCategory.Ambience)),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task UpdateAsync_WhenRoomNotFound_DoesNotPlaySound()
    {
        // Arrange
        GameSettings.AmbienceEnabled = true;
        var sut = CreateService();
        var roomId = Guid.NewGuid();

        _mockRoomRepository
            .Setup(r => r.GetByIdAsync(roomId))
            .ReturnsAsync((Room?)null);

        // Act - Call enough times to trigger
        for (int i = 0; i < 10; i++)
        {
            await sut.UpdateAsync(roomId);
        }

        // Assert
        _mockAudioService.Verify(
            a => a.PlayAsync(It.IsAny<SoundCue>()),
            Times.Never);
    }

    #endregion

    #region Biome Profile Tests

    [Theory]
    [InlineData(BiomeType.Ruin, 400, 600)]
    [InlineData(BiomeType.Industrial, 100, 300)]
    [InlineData(BiomeType.Organic, 600, 900)]
    [InlineData(BiomeType.Void, 200, 250)]
    public async Task UpdateAsync_BiomeProfile_UsesCorrectFrequencyRange(
        BiomeType biome,
        int minFreq,
        int maxFreq)
    {
        // Arrange
        GameSettings.AmbienceEnabled = true;
        var sut = CreateService();
        var roomId = Guid.NewGuid();

        var room = new Room
        {
            Id = roomId,
            Name = "Test Room",
            Position = Coordinate.Origin,
            BiomeType = biome
        };

        _mockRoomRepository
            .Setup(r => r.GetByIdAsync(roomId))
            .ReturnsAsync(room);

        // Capture the played cue
        SoundCue? capturedCue = null;
        _mockAudioService
            .Setup(a => a.PlayAsync(It.IsAny<SoundCue>()))
            .Callback<SoundCue>(c => capturedCue = c)
            .Returns(Task.CompletedTask);

        // Act - Call enough times to trigger
        for (int i = 0; i < 6; i++)
        {
            await sut.UpdateAsync(roomId);
        }

        // Assert
        capturedCue.Should().NotBeNull();
        capturedCue!.Frequency.Should().BeInRange(minFreq, maxFreq);
    }

    #endregion

    #region Reset Tests

    [Fact]
    public async Task Reset_ResetsCounterAndDelaysTrigger()
    {
        // Arrange
        GameSettings.AmbienceEnabled = true;
        var sut = CreateService();
        var roomId = Guid.NewGuid();

        var room = new Room
        {
            Id = roomId,
            Name = "Test Room",
            Position = Coordinate.Origin,
            BiomeType = BiomeType.Ruin
        };

        _mockRoomRepository
            .Setup(r => r.GetByIdAsync(roomId))
            .ReturnsAsync(room);

        // Advance counter to near trigger point
        await sut.UpdateAsync(roomId);
        await sut.UpdateAsync(roomId);

        // Act - Reset and verify counter restarts
        sut.Reset();

        // Clear any previous calls
        _mockAudioService.Invocations.Clear();

        // After reset, should need at least 2 more calls to trigger
        await sut.UpdateAsync(roomId);

        // Assert - First call after reset should not trigger
        _mockAudioService.Verify(
            a => a.PlayAsync(It.IsAny<SoundCue>()),
            Times.Never);
    }

    #endregion

    #region Turn Interval Tests

    [Fact]
    public async Task UpdateAsync_DoesNotTrigger_BeforeMinimumTurns()
    {
        // Arrange
        GameSettings.AmbienceEnabled = true;
        var sut = CreateService();
        var roomId = Guid.NewGuid();

        var room = new Room
        {
            Id = roomId,
            Name = "Test Room",
            Position = Coordinate.Origin,
            BiomeType = BiomeType.Ruin
        };

        _mockRoomRepository
            .Setup(r => r.GetByIdAsync(roomId))
            .ReturnsAsync(room);

        // Act - Call only once (minimum is 2)
        await sut.UpdateAsync(roomId);

        // Assert - Should not have triggered yet
        _mockAudioService.Verify(
            a => a.PlayAsync(It.IsAny<SoundCue>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_SoundCue_HasAmbienceCategory()
    {
        // Arrange
        GameSettings.AmbienceEnabled = true;
        var sut = CreateService();
        var roomId = Guid.NewGuid();

        var room = new Room
        {
            Id = roomId,
            Name = "Test Room",
            Position = Coordinate.Origin,
            BiomeType = BiomeType.Industrial
        };

        _mockRoomRepository
            .Setup(r => r.GetByIdAsync(roomId))
            .ReturnsAsync(room);

        SoundCue? capturedCue = null;
        _mockAudioService
            .Setup(a => a.PlayAsync(It.IsAny<SoundCue>()))
            .Callback<SoundCue>(c => capturedCue = c)
            .Returns(Task.CompletedTask);

        // Act - Call enough times to trigger
        for (int i = 0; i < 6; i++)
        {
            await sut.UpdateAsync(roomId);
        }

        // Assert
        capturedCue.Should().NotBeNull();
        capturedCue!.Category.Should().Be(SoundCategory.Ambience);
    }

    [Fact]
    public async Task UpdateAsync_SoundCue_HasReducedVolume()
    {
        // Arrange
        GameSettings.AmbienceEnabled = true;
        var sut = CreateService();
        var roomId = Guid.NewGuid();

        var room = new Room
        {
            Id = roomId,
            Name = "Test Room",
            Position = Coordinate.Origin,
            BiomeType = BiomeType.Void
        };

        _mockRoomRepository
            .Setup(r => r.GetByIdAsync(roomId))
            .ReturnsAsync(room);

        SoundCue? capturedCue = null;
        _mockAudioService
            .Setup(a => a.PlayAsync(It.IsAny<SoundCue>()))
            .Callback<SoundCue>(c => capturedCue = c)
            .Returns(Task.CompletedTask);

        // Act - Call enough times to trigger
        for (int i = 0; i < 6; i++)
        {
            await sut.UpdateAsync(roomId);
        }

        // Assert - Volume should be reduced (0.6 in the service)
        capturedCue.Should().NotBeNull();
        capturedCue!.VolumeMultiplier.Should().BeLessThan(1.0f);
    }

    #endregion
}
