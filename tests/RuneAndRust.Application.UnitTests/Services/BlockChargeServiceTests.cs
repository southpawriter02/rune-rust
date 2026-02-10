// ═══════════════════════════════════════════════════════════════════════════════
// BlockChargeServiceTests.cs
// Unit tests for the BlockChargeService application service.
// Version: 0.20.1a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.UnitTests.Services;

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.ValueObjects;

[TestFixture]
public class BlockChargeServiceTests
{
    private BlockChargeService _service = null!;
    private Mock<ILogger<BlockChargeService>> _mockLogger = null!;

    private static readonly Guid TestCharacterId = Guid.NewGuid();
    private const string TestCharacterName = "Test Skjaldmaer";

    [SetUp]
    public void SetUp()
    {
        _mockLogger = new Mock<ILogger<BlockChargeService>>();
        _service = new BlockChargeService(_mockLogger.Object);
    }

    [Test]
    public void SpendCharges_WithSufficientCharges_ReturnsSuccessAndReducedResource()
    {
        // Arrange
        var resource = BlockChargeResource.CreateDefault();

        // Act
        var (success, updated) = _service.SpendCharges(
            resource, 1, TestCharacterId, TestCharacterName);

        // Assert
        success.Should().BeTrue();
        updated.CurrentCharges.Should().Be(2);
    }

    [Test]
    public void SpendCharges_WithInsufficientCharges_ReturnsFalseAndUnchanged()
    {
        // Arrange
        var resource = BlockChargeResource.CreateDefault() with { CurrentCharges = 0 };

        // Act
        var (success, updated) = _service.SpendCharges(
            resource, 1, TestCharacterId, TestCharacterName);

        // Assert
        success.Should().BeFalse();
        updated.CurrentCharges.Should().Be(0);
    }

    [Test]
    public void RestoreAllCharges_RestoresToMax()
    {
        // Arrange
        var resource = BlockChargeResource.CreateDefault() with { CurrentCharges = 0 };

        // Act
        var restored = _service.RestoreAllCharges(
            resource, TestCharacterId, TestCharacterName);

        // Assert
        restored.CurrentCharges.Should().Be(3);
        restored.LastRestoredAt.Should().NotBeNull();
    }

    [Test]
    public void CanSpend_WithSufficientCharges_ReturnsTrue()
    {
        // Arrange
        var resource = BlockChargeResource.CreateDefault();

        // Act & Assert
        _service.CanSpend(resource, 1).Should().BeTrue();
        _service.CanSpend(resource, 3).Should().BeTrue();
    }

    [Test]
    public void CalculateBulwarkBonus_ReturnsCorrectValue()
    {
        // Arrange
        var resource = BlockChargeResource.CreateDefault() with { CurrentCharges = 2 };

        // Act
        var bonus = _service.CalculateBulwarkBonus(resource);

        // Assert
        bonus.Should().Be(10);
    }
}
