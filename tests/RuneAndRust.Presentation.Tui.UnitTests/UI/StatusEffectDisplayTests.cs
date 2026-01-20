using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Presentation.Tui.Renderers;
using RuneAndRust.Presentation.Tui.UI;

namespace RuneAndRust.Presentation.Tui.UnitTests.UI;

/// <summary>
/// Unit tests for <see cref="StatusEffectDisplay"/>.
/// </summary>
[TestFixture]
public class StatusEffectDisplayTests
{
    private Mock<ITerminalService> _mockTerminal = null!;
    private StatusEffectRenderer _renderer = null!;
    private StatusEffectDisplay _display = null!;

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

        _display = new StatusEffectDisplay(
            _mockTerminal.Object,
            _renderer,
            NullLogger<StatusEffectDisplay>.Instance);
    }

    // ═══════════════════════════════════════════════════════════════
    // RENDER BUFF ROW TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void RenderBuffRow_WithNoBuffs_ReturnsNoneMessage()
    {
        // Arrange
        var buffs = new List<ActiveStatusEffect>();

        // Act
        var result = _display.RenderBuffRow(buffs);

        // Assert
        result.Should().Be("BUFFS: (none)");
    }

    [Test]
    public void RenderBuffRow_WithMultipleBuffs_FormatsCorrectly()
    {
        // Arrange
        var regenDef = CreateTestDefinition("regenerating", EffectCategory.Buff, healPerTurn: 5);
        var shieldDef = CreateTestDefinition("shielded", EffectCategory.Buff);

        var buffs = new List<ActiveStatusEffect>
        {
            ActiveStatusEffect.Create(regenDef),
            ActiveStatusEffect.Create(shieldDef)
        };

        // Act
        var result = _display.RenderBuffRow(buffs);

        // Assert
        result.Should().StartWith("BUFFS:");
        result.Should().Contain("["); // Contains effect icons
        result.Should().Contain("]");
        result.Should().NotContain("(none)");
    }

    // ═══════════════════════════════════════════════════════════════
    // RENDER DEBUFF ROW TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void RenderDebuffRow_WithDebuffs_FormatsWithDebuffIcons()
    {
        // Arrange
        var poisonDef = CreateTestDefinition("poisoned", EffectCategory.Debuff, damagePerTurn: 3, damageType: "poison");
        var debuffs = new List<ActiveStatusEffect>
        {
            ActiveStatusEffect.Create(poisonDef)
        };

        // Act
        var result = _display.RenderDebuffRow(debuffs);

        // Assert
        result.Should().StartWith("DEBUFFS:");
        result.Should().Contain("[");
        result.Should().Contain("P"); // Poison icon
        result.Should().NotContain("(none)");
    }

    // ═══════════════════════════════════════════════════════════════
    // TOOLTIP TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void ShowTooltip_WithStackingEffect_ShowsStackInfo()
    {
        // Arrange
        var poisonDef = CreateTestDefinition(
            "poisoned",
            EffectCategory.Debuff,
            damagePerTurn: 2,
            damageType: "poison",
            maxStacks: 3);

        var effect = ActiveStatusEffect.Create(poisonDef);
        effect.AddStacks(1); // Now at 2 stacks
        effect.AddStacks(1); // Now at 3 stacks

        // Act
        var lines = _display.ShowTooltip(effect);

        // Assert
        lines.Should().NotBeEmpty();
        var joined = string.Join("\n", lines);
        joined.Should().Contain("POISONED");
        joined.Should().Contain("Stacks:");
        joined.Should().Contain("3");
    }

    // ═══════════════════════════════════════════════════════════════
    // HELPER METHODS
    // ═══════════════════════════════════════════════════════════════

    private static StatusEffectDefinition CreateTestDefinition(
        string id,
        EffectCategory category,
        int baseDuration = 3,
        int? damagePerTurn = null,
        string? damageType = null,
        int? healPerTurn = null,
        int maxStacks = 1)
    {
        var def = StatusEffectDefinition.Create(
            id,
            id.ToUpperInvariant(),
            $"Test {id} effect",
            category,
            DurationType.Turns,
            baseDuration,
            StackingRule.RefreshDuration,
            maxStacks);

        if (damagePerTurn.HasValue)
        {
            def = def.WithDamageOverTime(damagePerTurn.Value, damageType ?? "physical");
        }

        if (healPerTurn.HasValue)
        {
            def = def.WithHealingOverTime(healPerTurn.Value);
        }

        return def;
    }
}
