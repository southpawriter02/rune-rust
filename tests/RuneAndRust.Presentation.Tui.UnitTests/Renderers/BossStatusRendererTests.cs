using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.DTOs;
using RuneAndRust.Presentation.Tui.Renderers;

namespace RuneAndRust.Presentation.Tui.UnitTests.Renderers;

/// <summary>
/// Unit tests for <see cref="BossStatusRenderer"/>.
/// </summary>
[TestFixture]
public class BossStatusRendererTests
{
    private BossStatusRenderer _renderer = null!;

    [SetUp]
    public void SetUp()
    {
        _renderer = new BossStatusRenderer(
            null,
            NullLogger<BossStatusRenderer>.Instance);
    }

    // ═══════════════════════════════════════════════════════════════
    // FORMAT PHASE TEXT TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void FormatPhaseText_WithValidInput_ReturnsFormattedString()
    {
        // Arrange
        var phaseName = "Empowered";
        var phaseNumber = 2;

        // Act
        var result = _renderer.FormatPhaseText(phaseName, phaseNumber);

        // Assert
        result.Should().Be("Phase: 2 - EMPOWERED");
    }

    [Test]
    public void FormatPhaseText_ConvertsToUppercase()
    {
        // Arrange
        var phaseName = "fury mode";
        var phaseNumber = 3;

        // Act
        var result = _renderer.FormatPhaseText(phaseName, phaseNumber);

        // Assert
        result.Should().Be("Phase: 3 - FURY MODE");
    }

    [Test]
    public void FormatPhaseText_WithPhaseOne_ReturnsCorrectFormat()
    {
        // Arrange
        var phaseName = "Rise";
        var phaseNumber = 1;

        // Act
        var result = _renderer.FormatPhaseText(phaseName, phaseNumber);

        // Assert
        result.Should().Be("Phase: 1 - RISE");
    }

    // ═══════════════════════════════════════════════════════════════
    // GET PHASE COLOR TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    [TestCase(BossBehavior.Aggressive, ConsoleColor.Red)]
    [TestCase(BossBehavior.Tactical, ConsoleColor.Cyan)]
    [TestCase(BossBehavior.Defensive, ConsoleColor.Blue)]
    [TestCase(BossBehavior.Enraged, ConsoleColor.DarkRed)]
    [TestCase(BossBehavior.Summoner, ConsoleColor.Magenta)]
    public void GetPhaseColor_WithBehavior_ReturnsCorrectColor(BossBehavior behavior, ConsoleColor expected)
    {
        // Arrange & Act
        var result = _renderer.GetPhaseColor(behavior);

        // Assert
        result.Should().Be(expected);
    }

    // ═══════════════════════════════════════════════════════════════
    // FORMAT VULNERABILITY WINDOW TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void FormatVulnerabilityWindow_WithTurnsAndMultiplier_ReturnsFormattedString()
    {
        // Arrange
        var turns = 2;
        var multiplier = 1.5f;

        // Act
        var result = _renderer.FormatVulnerabilityWindow(turns, multiplier);

        // Assert
        result.Should().Contain("2 turns");
        result.Should().Contain("+50%");
        result.Should().Contain("VULNERABILITY WINDOW OPEN");
    }

    [Test]
    public void FormatVulnerabilityWindow_WithOneTurn_UsesSingularForm()
    {
        // Arrange
        var turns = 1;
        var multiplier = 1.5f;

        // Act
        var result = _renderer.FormatVulnerabilityWindow(turns, multiplier);

        // Assert
        result.Should().Contain("1 turn");
        result.Should().NotContain("1 turns");
    }

    [Test]
    public void FormatVulnerabilityWindow_WithDifferentMultiplier_CalculatesCorrectBonus()
    {
        // Arrange
        var turns = 3;
        var multiplier = 2.0f;

        // Act
        var result = _renderer.FormatVulnerabilityWindow(turns, multiplier);

        // Assert
        result.Should().Contain("+100%");
    }

    // ═══════════════════════════════════════════════════════════════
    // FORMAT ENRAGE MODIFIERS TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void FormatEnrageModifiers_WithMultipleModifiers_ReturnsCommaSeparatedString()
    {
        // Arrange
        var modifiers = new Dictionary<string, float>
        {
            { "damage", 1.5f },
            { "attackSpeed", 1.25f }
        };

        // Act
        var result = _renderer.FormatEnrageModifiers(modifiers);

        // Assert
        result.Should().Contain("+50% damage");
        result.Should().Contain("+25% attack speed");
        result.Should().Contain(", ");
    }

    [Test]
    public void FormatEnrageModifiers_WithSingleModifier_ReturnsWithoutComma()
    {
        // Arrange
        var modifiers = new Dictionary<string, float>
        {
            { "damage", 1.5f }
        };

        // Act
        var result = _renderer.FormatEnrageModifiers(modifiers);

        // Assert
        result.Should().Be("+50% damage");
    }

    [Test]
    public void FormatEnrageModifiers_WithNegativeModifier_ReturnsNegativePercent()
    {
        // Arrange
        var modifiers = new Dictionary<string, float>
        {
            { "defense", 0.75f }
        };

        // Act
        var result = _renderer.FormatEnrageModifiers(modifiers);

        // Assert
        result.Should().Be("-25% defense");
    }

    // ═══════════════════════════════════════════════════════════════
    // GET WARNING COLOR TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    [TestCase(5, ConsoleColor.Red)]     // Critical: <=5%
    [TestCase(4, ConsoleColor.Red)]
    [TestCase(10, ConsoleColor.DarkYellow)] // High: 5-10%
    [TestCase(6, ConsoleColor.DarkYellow)]
    [TestCase(15, ConsoleColor.Yellow)]     // Low: >10%
    [TestCase(11, ConsoleColor.Yellow)]
    public void GetWarningColor_WithHealthPercent_ReturnsCorrectColor(int percent, ConsoleColor expected)
    {
        // Arrange & Act
        var result = _renderer.GetWarningColor(percent);

        // Assert
        result.Should().Be(expected);
    }

    // ═══════════════════════════════════════════════════════════════
    // FORMAT TRANSITION BOX TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void FormatTransitionBox_WithValidDto_ReturnsMultipleLines()
    {
        // Arrange
        var dto = new PhaseTransitionDto(
            OldPhaseNumber: 1,
            NewPhaseNumber: 2,
            PhaseName: "Empowered",
            TransitionText: "Rise, my minions!",
            BossName: "Skeleton King");

        // Act
        var result = _renderer.FormatTransitionBox(dto);

        // Assert
        result.Should().HaveCountGreaterThan(5);
        result.Should().Contain(line => line.Contains("PHASE TRANSITION"));
        result.Should().Contain(line => line.Contains("Skeleton King"));
        result.Should().Contain(line => line.Contains("Phase 2"));
        result.Should().Contain(line => line.Contains("Rise, my minions!"));
    }

    [Test]
    public void FormatTransitionBox_WithoutTransitionText_OmitsQuoteLine()
    {
        // Arrange
        var dto = new PhaseTransitionDto(
            OldPhaseNumber: 1,
            NewPhaseNumber: 2,
            PhaseName: "Empowered",
            TransitionText: null,
            BossName: "Skeleton King");

        // Act
        var result = _renderer.FormatTransitionBox(dto);

        // Assert
        result.Should().NotContain(line => line.Contains("\""));
    }

    // ═══════════════════════════════════════════════════════════════
    // FORMAT ENRAGE WARNING TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void FormatEnrageWarning_WithHealthPercent_ReturnsFormattedString()
    {
        // Arrange
        var percent = 5;

        // Act
        var result = _renderer.FormatEnrageWarning(percent);

        // Assert
        result.Should().Be("[!] Enrage in: 5% health");
    }

    // ═══════════════════════════════════════════════════════════════
    // FORMAT ENRAGE ACTIVE TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void FormatEnrageActive_ReturnsEnragedHeader()
    {
        // Arrange & Act
        var result = _renderer.FormatEnrageActive();

        // Assert
        result.Should().Be("[!!!] ENRAGED");
    }

    // ═══════════════════════════════════════════════════════════════
    // FORMAT VULNERABILITY URGENT TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void FormatVulnerabilityUrgent_ReturnsUrgentMessage()
    {
        // Arrange & Act
        var result = _renderer.FormatVulnerabilityUrgent();

        // Assert
        result.Should().Be("[!!!] LAST CHANCE - Vulnerability closing!");
    }

    // ═══════════════════════════════════════════════════════════════
    // FORMAT WINDOW CLOSED TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void FormatWindowClosed_ReturnsClosedMessage()
    {
        // Arrange & Act
        var result = _renderer.FormatWindowClosed();

        // Assert
        result.Should().Be("[x] Vulnerability window closed");
    }
}
