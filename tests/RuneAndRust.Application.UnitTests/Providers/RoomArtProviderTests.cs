using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Infrastructure.Providers;

namespace RuneAndRust.Application.UnitTests.Providers;

/// <summary>
/// Unit tests for <see cref="RoomArtProvider"/>.
/// </summary>
[TestFixture]
public class RoomArtProviderTests
{
    private RoomArtProvider _provider = null!;

    [SetUp]
    public void Setup()
    {
        _provider = new RoomArtProvider();
    }

    #region GetArtForRoom Tests

    [Test]
    public void GetArtForRoom_WithExistingRoom_ReturnsArt()
    {
        // Act
        var art = _provider.GetArtForRoom("dark-cave");

        // Assert
        art.Should().NotBeNull();
        art!.RoomId.Should().Be("dark-cave");
    }

    [Test]
    public void GetArtForRoom_CaseInsensitive_ReturnsArt()
    {
        // Act
        var art = _provider.GetArtForRoom("DARK-CAVE");

        // Assert
        art.Should().NotBeNull();
    }

    [Test]
    public void GetArtForRoom_WithUnknownRoom_ReturnsNull()
    {
        // Act
        var art = _provider.GetArtForRoom("unknown-room");

        // Assert
        art.Should().BeNull();
    }

    #endregion

    #region HasArt Tests

    [Test]
    public void HasArt_WithExistingRoom_ReturnsTrue()
    {
        // Act
        var hasArt = _provider.HasArt("throne-room");

        // Assert
        hasArt.Should().BeTrue();
    }

    [Test]
    public void HasArt_WithUnknownRoom_ReturnsFalse()
    {
        // Act
        var hasArt = _provider.HasArt("unknown-room");

        // Assert
        hasArt.Should().BeFalse();
    }

    #endregion

    #region AvailableArtIds Tests

    [Test]
    public void AvailableArtIds_ContainsDefaultRooms()
    {
        // Assert
        _provider.AvailableArtIds.Should().Contain("dark-cave");
        _provider.AvailableArtIds.Should().Contain("throne-room");
        _provider.AvailableArtIds.Should().Contain("forest-clearing");
    }

    #endregion

    #region GetDefaultArt Tests

    [Test]
    public void GetDefaultArt_ReturnsDefaultSettings()
    {
        // Act
        var settings = _provider.GetDefaultArt();

        // Assert
        settings.Should().NotBeNull();
        settings.Template.Should().Be("simple-box");
        settings.ShowExits.Should().BeTrue();
        settings.ShowContents.Should().BeTrue();
    }

    #endregion
}
