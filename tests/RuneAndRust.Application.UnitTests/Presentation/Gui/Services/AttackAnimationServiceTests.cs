using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;
using RuneAndRust.Presentation.Gui.Controls;
using RuneAndRust.Presentation.Gui.Services;

namespace RuneAndRust.Application.UnitTests.Presentation.Gui.Services;

/// <summary>
/// Unit tests for <see cref="AttackAnimationService"/>.
/// </summary>
[TestFixture]
public class AttackAnimationServiceTests
{
    private Mock<ILogger<AttackAnimationService>> _mockLogger = null!;
    private Mock<ICombatGridOverlay> _mockOverlay = null!;
    private AttackAnimationService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _mockLogger = new Mock<ILogger<AttackAnimationService>>();
        _mockOverlay = new Mock<ICombatGridOverlay>();

        // Default screen position for grid position conversion
        _mockOverlay
            .Setup(o => o.GridToScreen(It.IsAny<GridPosition>()))
            .Returns(new Avalonia.Point(100, 100));

        _service = new AttackAnimationService(_mockLogger.Object, _mockOverlay.Object);
    }

    /// <summary>
    /// Verifies that damage popup creates control and adds to overlay.
    /// </summary>
    [Test]
    public async Task ShowDamagePopupAsync_WithValidPosition_AddsPopupToOverlay()
    {
        // Arrange
        var position = new GridPosition(3, 4);

        // Act
        await _service.ShowDamagePopupAsync(position, 25, false);

        // Assert
        _mockOverlay.Verify(
            o => o.AddPopup(It.IsAny<DamagePopupControl>(), It.IsAny<Avalonia.Point>()),
            Times.Once);
        _mockOverlay.Verify(
            o => o.RemovePopup(It.IsAny<DamagePopupControl>()),
            Times.Once);
    }

    /// <summary>
    /// Verifies that healing popup adds to overlay.
    /// </summary>
    [Test]
    public async Task ShowHealingPopupAsync_WithValidPosition_AddsPopupToOverlay()
    {
        // Arrange
        var position = new GridPosition(2, 2);

        // Act
        await _service.ShowHealingPopupAsync(position, 15);

        // Assert
        _mockOverlay.Verify(
            o => o.AddPopup(It.IsAny<DamagePopupControl>(), It.IsAny<Avalonia.Point>()),
            Times.Once);
    }

    /// <summary>
    /// Verifies that miss popup adds to overlay.
    /// </summary>
    [Test]
    public async Task ShowMissPopupAsync_WithValidPosition_AddsPopupToOverlay()
    {
        // Arrange
        var position = new GridPosition(1, 1);

        // Act
        await _service.ShowMissPopupAsync(position);

        // Assert
        _mockOverlay.Verify(
            o => o.AddPopup(It.IsAny<DamagePopupControl>(), It.IsAny<Avalonia.Point>()),
            Times.Once);
    }

    /// <summary>
    /// Verifies that block popup adds to overlay.
    /// </summary>
    [Test]
    public async Task ShowBlockPopupAsync_WithValidPosition_AddsPopupToOverlay()
    {
        // Arrange
        var position = new GridPosition(0, 0);

        // Act
        await _service.ShowBlockPopupAsync(position);

        // Assert
        _mockOverlay.Verify(
            o => o.AddPopup(It.IsAny<DamagePopupControl>(), It.IsAny<Avalonia.Point>()),
            Times.Once);
    }

    /// <summary>
    /// Verifies that highlight throws on null combatant.
    /// </summary>
    [Test]
    public async Task HighlightTurnStartAsync_WithNullCombatant_ThrowsArgumentNullException()
    {
        // Act
        var act = async () => await _service.HighlightTurnStartAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that status applied shows indicator.
    /// </summary>
    [Test]
    public async Task ShowStatusAppliedAsync_WithEffect_ShowsIndicator()
    {
        // Arrange
        var position = new GridPosition(3, 3);
        var effect = StatusEffectDefinition.Create(
            "burning",
            "Burning",
            "Taking fire damage",
            EffectCategory.Debuff,
            DurationType.Turns,
            baseDuration: 3);

        // Act
        await _service.ShowStatusAppliedAsync(position, effect);

        // Assert
        _mockOverlay.Verify(
            o => o.ShowStatusIndicator(position, It.IsAny<string>(), "+Burning", false),
            Times.Once);
        _mockOverlay.Verify(
            o => o.HideStatusIndicator(position),
            Times.Once);
    }

    /// <summary>
    /// Verifies that status expired shows fade-out indicator.
    /// </summary>
    [Test]
    public async Task ShowStatusExpiredAsync_WithEffect_ShowsFadeOutIndicator()
    {
        // Arrange
        var position = new GridPosition(4, 4);
        var effect = StatusEffectDefinition.Create(
            "blessed",
            "Blessed",
            "Receiving divine protection",
            EffectCategory.Buff,
            DurationType.Turns,
            baseDuration: 5);

        // Act
        await _service.ShowStatusExpiredAsync(position, effect);

        // Assert
        _mockOverlay.Verify(
            o => o.ShowStatusIndicator(position, It.IsAny<string>(), "-Blessed", true),
            Times.Once);
        _mockOverlay.Verify(
            o => o.HideStatusIndicator(position),
            Times.Once);
    }

    /// <summary>
    /// Verifies that service works without overlay (graceful degradation).
    /// </summary>
    [Test]
    public async Task ShowDamagePopupAsync_WithoutOverlay_CompletesWithoutError()
    {
        // Arrange
        var serviceWithoutOverlay = new AttackAnimationService(_mockLogger.Object);
        var position = new GridPosition(0, 0);

        // Act
        var act = async () => await serviceWithoutOverlay.ShowDamagePopupAsync(position, 10, false);

        // Assert
        await act.Should().NotThrowAsync();
    }
}
