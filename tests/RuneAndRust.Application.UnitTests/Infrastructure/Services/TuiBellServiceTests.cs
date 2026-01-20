using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using RuneAndRust.Application.Enums;
using RuneAndRust.Infrastructure.Services;

namespace RuneAndRust.Application.UnitTests.Infrastructure.Services;

/// <summary>
/// Unit tests for <see cref="TuiBellService"/>.
/// </summary>
[TestFixture]
public class TuiBellServiceTests
{
    private TuiBellService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _service = new TuiBellService(NullLogger<TuiBellService>.Instance);
    }

    /// <summary>
    /// Verifies service is enabled by default.
    /// </summary>
    [Test]
    public void IsEnabled_ByDefault_ReturnsTrue()
    {
        // Assert
        _service.IsEnabled.Should().BeTrue();
    }

    /// <summary>
    /// Verifies SetEnabled toggles the enabled state.
    /// </summary>
    [Test]
    public void SetEnabled_ToFalse_DisablesBells()
    {
        // Act
        _service.SetEnabled(false);

        // Assert
        _service.IsEnabled.Should().BeFalse();
    }

    /// <summary>
    /// Verifies SetEnabled can re-enable after disable.
    /// </summary>
    [Test]
    public void SetEnabled_ToTrue_ReenablesBells()
    {
        // Arrange
        _service.SetEnabled(false);
        _service.IsEnabled.Should().BeFalse();

        // Act
        _service.SetEnabled(true);

        // Assert
        _service.IsEnabled.Should().BeTrue();
    }

    /// <summary>
    /// Verifies Bell does not throw when enabled.
    /// </summary>
    [Test]
    public void Bell_WhenEnabled_DoesNotThrow()
    {
        // Act & Assert - Should not throw
        var act = () => _service.Bell(BellType.Info);
        act.Should().NotThrow();
    }

    /// <summary>
    /// Verifies Bell does not throw when disabled.
    /// </summary>
    [Test]
    public void Bell_WhenDisabled_DoesNotThrow()
    {
        // Arrange
        _service.SetEnabled(false);

        // Act & Assert - Should not throw
        var act = () => _service.Bell(BellType.Combat);
        act.Should().NotThrow();
    }

    /// <summary>
    /// Verifies BellCustom does not throw with valid frequency.
    /// </summary>
    [Test]
    public void BellCustom_WithValidFrequency_DoesNotThrow()
    {
        // Act & Assert
        var act = () => _service.BellCustom(800, 100);
        act.Should().NotThrow();
    }

    /// <summary>
    /// Verifies all bell types are supported.
    /// </summary>
    [Test]
    public void Bell_AllTypes_DoNotThrow()
    {
        // Act & Assert - All types should work without throwing
        foreach (var bellType in Enum.GetValues<BellType>())
        {
            var act = () => _service.Bell(bellType);
            act.Should().NotThrow($"BellType.{bellType} should not throw");
        }
    }
}
