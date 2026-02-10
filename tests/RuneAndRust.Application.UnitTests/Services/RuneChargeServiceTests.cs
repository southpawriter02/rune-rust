// ═══════════════════════════════════════════════════════════════════════════════
// RuneChargeServiceTests.cs
// Unit tests for the RuneChargeService application service, validating
// charge spending, craft generation, restoration, and logging behavior.
// Version: 0.20.2a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.UnitTests.Services;

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Tests for <see cref="RuneChargeService"/>.
/// </summary>
[TestFixture]
public class RuneChargeServiceTests
{
    private Mock<ILogger<RuneChargeService>> _loggerMock = null!;
    private RuneChargeService _service = null!;
    private readonly Guid _characterId = Guid.NewGuid();
    private const string CharacterName = "TestRunesmith";

    [SetUp]
    public void SetUp()
    {
        _loggerMock = new Mock<ILogger<RuneChargeService>>();
        _service = new RuneChargeService(_loggerMock.Object);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SpendCharges Tests
    // ─────────────────────────────────────────────────────────────────────────

    [Test]
    public void SpendCharges_WithSufficientCharges_ReturnsSuccessAndReduced()
    {
        // Arrange
        var resource = RuneChargeResource.CreateDefault();

        // Act
        var (success, updated) = _service.SpendCharges(
            resource, 1, _characterId, CharacterName);

        // Assert
        success.Should().BeTrue();
        updated.CurrentCharges.Should().Be(4);
    }

    [Test]
    public void SpendCharges_WithInsufficientCharges_ReturnsFalseAndUnchanged()
    {
        // Arrange
        var resource = RuneChargeResource.CreateDefault();
        var (_, depleted) = resource.TrySpend(4);

        // Act — try to spend 3 from 1
        var (success, result) = _service.SpendCharges(
            depleted, 3, _characterId, CharacterName);

        // Assert
        success.Should().BeFalse();
        result.CurrentCharges.Should().Be(1);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GenerateFromCraft Tests
    // ─────────────────────────────────────────────────────────────────────────

    [Test]
    public void GenerateFromCraft_StandardCraft_DelegatesCorrectly()
    {
        // Arrange
        var resource = RuneChargeResource.CreateDefault();
        var (_, depleted) = resource.TrySpend(2);

        // Act
        var generated = _service.GenerateFromCraft(
            depleted, isComplexCraft: false, _characterId, CharacterName);

        // Assert
        generated.CurrentCharges.Should().Be(4);
    }

    [Test]
    public void GenerateFromCraft_WhenFullyCharged_ReturnsUnchanged()
    {
        // Arrange
        var resource = RuneChargeResource.CreateDefault();

        // Act
        var result = _service.GenerateFromCraft(
            resource, isComplexCraft: false, _characterId, CharacterName);

        // Assert
        result.CurrentCharges.Should().Be(5);
        result.IsFullyCharged.Should().BeTrue();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // RestoreAllCharges Tests
    // ─────────────────────────────────────────────────────────────────────────

    [Test]
    public void RestoreAllCharges_RestoresToMax()
    {
        // Arrange
        var resource = RuneChargeResource.CreateDefault();
        var (_, depleted) = resource.TrySpend(5);

        // Act
        var restored = _service.RestoreAllCharges(
            depleted, _characterId, CharacterName);

        // Assert
        restored.CurrentCharges.Should().Be(5);
        restored.IsFullyCharged.Should().BeTrue();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // CanSpend Tests
    // ─────────────────────────────────────────────────────────────────────────

    [Test]
    public void CanSpend_WithSufficientCharges_ReturnsTrue()
    {
        // Arrange
        var resource = RuneChargeResource.CreateDefault();

        // Act
        var canSpend = _service.CanSpend(resource, 3);

        // Assert
        canSpend.Should().BeTrue();
    }

    [Test]
    public void CanSpend_WithInsufficientCharges_ReturnsFalse()
    {
        // Arrange
        var resource = RuneChargeResource.CreateDefault();
        var (_, depleted) = resource.TrySpend(4);

        // Act
        var canSpend = _service.CanSpend(depleted, 3);

        // Assert
        canSpend.Should().BeFalse();
    }
}
