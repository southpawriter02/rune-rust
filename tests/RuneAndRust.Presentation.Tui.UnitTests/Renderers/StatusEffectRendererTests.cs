using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Presentation.Tui.DTOs;
using RuneAndRust.Presentation.Tui.Renderers;

namespace RuneAndRust.Presentation.Tui.UnitTests.Renderers;

/// <summary>
/// Unit tests for <see cref="StatusEffectRenderer"/>.
/// </summary>
[TestFixture]
public class StatusEffectRendererTests
{
    private Mock<ITerminalService> _mockTerminal = null!;
    private StatusEffectRenderer _renderer = null!;

    [SetUp]
    public void SetUp()
    {
        _mockTerminal = new Mock<ITerminalService>();
        _mockTerminal.Setup(t => t.SupportsUnicode).Returns(true);
        _mockTerminal.Setup(t => t.SupportsColor).Returns(true);

        _renderer = new StatusEffectRenderer(
            _mockTerminal.Object,
            null,
            NullLogger<StatusEffectRenderer>.Instance);
    }

    // ═══════════════════════════════════════════════════════════════
    // FORMAT EFFECT DISPLAY TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void FormatEffectDisplay_DoTEffect_UsesCorrectIcon()
    {
        // Arrange
        var dto = new StatusEffectDisplayDto
        {
            EffectId = "burning",
            Name = "Burning",
            Category = EffectCategory.Debuff,
            EffectType = StatusEffectType.DamageOverTime,
            RemainingDuration = 3,
            DurationType = DurationType.Turns,
            CurrentStacks = 1,
            MaxStacks = 1,
            DamageType = "fire"
        };

        // Act
        var result = _renderer.FormatEffectDisplay(dto);

        // Assert
        result.Should().StartWith("[");
        result.Should().EndWith("]");
        result.Should().Contain("F"); // Fire damage type icon
        result.Should().Contain("3t"); // 3 turns remaining
    }

    [Test]
    public void FormatEffectDisplay_WithStacks_IncludesStackCount()
    {
        // Arrange
        var dto = new StatusEffectDisplayDto
        {
            EffectId = "poison",
            Name = "Poisoned",
            Category = EffectCategory.Debuff,
            EffectType = StatusEffectType.DamageOverTime,
            RemainingDuration = 5,
            DurationType = DurationType.Turns,
            CurrentStacks = 3,
            MaxStacks = 5,
            DamageType = "poison"
        };

        // Act
        var result = _renderer.FormatEffectDisplay(dto);

        // Assert
        result.Should().Contain("3x"); // Stack count indicator
        result.Should().Contain("P"); // Poison icon
    }

    // ═══════════════════════════════════════════════════════════════
    // GET EFFECT ICON TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void GetEffectIcon_AllTypes_ReturnsMappedCharacter()
    {
        // Arrange & Act & Assert
        foreach (StatusEffectType effectType in Enum.GetValues<StatusEffectType>())
        {
            var icon = _renderer.GetEffectIcon(effectType);

            // Assert: Each call should return a valid single character
            icon.Should().NotBe('\0', because: $"effect type {effectType} should have a mapped icon");
        }
    }

    [Test]
    public void GetEffectIcon_StatModifier_ReturnsPlusSign()
    {
        // Arrange & Act
        var icon = _renderer.GetEffectIcon(StatusEffectType.StatModifier);

        // Assert
        icon.Should().Be('+');
    }

    // ═══════════════════════════════════════════════════════════════
    // FORMAT DURATION TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void FormatDuration_TurnBased_AppendsTurnSuffix()
    {
        // Arrange & Act
        var result = _renderer.FormatDuration(5, DurationType.Turns);

        // Assert
        result.Should().Be("5t");
    }

    [Test]
    public void FormatDuration_Permanent_ReturnsEmpty()
    {
        // Arrange & Act
        var result = _renderer.FormatDuration(null, DurationType.Permanent);

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public void FormatDuration_Triggered_ReturnsQuestionMark()
    {
        // Arrange & Act
        var result = _renderer.FormatDuration(null, DurationType.Triggered);

        // Assert
        result.Should().Be("?");
    }

    // ═══════════════════════════════════════════════════════════════
    // FORMAT STACKS TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void FormatStacks_SingleStack_ReturnsEmpty()
    {
        // Arrange & Act
        var result = _renderer.FormatStacks(1, 3);

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public void FormatStacks_MultipleStacks_ReturnsStackIndicator()
    {
        // Arrange & Act
        var result = _renderer.FormatStacks(3, 5);

        // Assert
        result.Should().Be("3x");
    }

    [Test]
    public void FormatStacks_AtMaxStacks_ReturnsMaxIndicator()
    {
        // Arrange & Act
        var result = _renderer.FormatStacks(5, 5);

        // Assert
        result.Should().Be("5x!");
    }

    // ═══════════════════════════════════════════════════════════════
    // GET DAMAGE TYPE ICON TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    [TestCase("fire", 'F')]
    [TestCase("ice", 'I')]
    [TestCase("cold", 'I')]
    [TestCase("poison", 'P')]
    [TestCase("lightning", 'L')]
    [TestCase("bleed", 'B')]
    public void GetDamageTypeIcon_KnownTypes_ReturnsCorrectIcon(string damageType, char expectedIcon)
    {
        // Arrange & Act
        var result = _renderer.GetDamageTypeIcon(damageType);

        // Assert
        result.Should().Be(expectedIcon);
    }

    [Test]
    public void GetDamageTypeIcon_UnknownType_ReturnsDefault()
    {
        // Arrange & Act
        var result = _renderer.GetDamageTypeIcon("chaos");

        // Assert
        result.Should().Be('D'); // Default damage icon
    }

    // ═══════════════════════════════════════════════════════════════
    // GET IMMUNITY INDICATOR TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void GetImmunityIndicator_UnicodeSupported_ReturnsUnicode()
    {
        // Arrange (already setup with SupportsUnicode = true)

        // Act
        var result = _renderer.GetImmunityIndicator();

        // Assert
        result.Should().Be("🛡");
    }

    [Test]
    public void GetImmunityIndicator_UnicodeNotSupported_ReturnsAscii()
    {
        // Arrange
        _mockTerminal.Setup(t => t.SupportsUnicode).Returns(false);
        var asciiRenderer = new StatusEffectRenderer(_mockTerminal.Object);

        // Act
        var result = asciiRenderer.GetImmunityIndicator();

        // Assert
        result.Should().Be("[X]");
    }
}
