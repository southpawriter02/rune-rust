using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Presentation.Configuration;
using RuneAndRust.Presentation.UI;

namespace RuneAndRust.Application.UnitTests.Presentation.UI;

/// <summary>
/// Unit tests for <see cref="CombatantListPanel"/>.
/// </summary>
[TestFixture]
public class CombatantListPanelTests
{
    private Mock<ITerminalService> _mockTerminal = null!;
    private HealthBarDisplay _healthBar = null!;
    private CombatantListPanel _panel = null!;

    [SetUp]
    public void Setup()
    {
        _mockTerminal = new Mock<ITerminalService>();
        _mockTerminal.Setup(t => t.SupportsUnicode).Returns(true);
        
        _healthBar = new HealthBarDisplay(_mockTerminal.Object, HealthBarConfig.CreateDefault());
        _panel = new CombatantListPanel(_mockTerminal.Object, _healthBar);
    }

    #region Render Tests

    [Test]
    public void Render_WithCombatants_ReturnsLines()
    {
        // Arrange
        var combatants = new List<(string, int, int, int, int, bool, bool, IReadOnlyList<string>)>
        {
            ("Hero", 80, 100, 30, 50, true, true, Array.Empty<string>()),
            ("Goblin", 15, 30, 0, 0, false, false, Array.Empty<string>())
        };

        // Act
        var result = _panel.Render(combatants);

        // Assert
        result.Should().NotBeEmpty();
        result.Any(line => line.Contains("Hero")).Should().BeTrue();
        result.Any(line => line.Contains("Goblin")).Should().BeTrue();
    }

    [Test]
    public void Render_WithCurrentTurn_ShowsTurnIndicator()
    {
        // Arrange
        var combatants = new List<(string, int, int, int, int, bool, bool, IReadOnlyList<string>)>
        {
            ("Hero", 80, 100, 30, 50, true, true, Array.Empty<string>())
        };

        // Act
        var result = _panel.Render(combatants);

        // Assert
        result.Any(line => line.Contains('>')).Should().BeTrue();
    }

    [Test]
    public void Render_WithHealthBarDisplays_IncludesHpInfo()
    {
        // Arrange
        var combatants = new List<(string, int, int, int, int, bool, bool, IReadOnlyList<string>)>
        {
            ("Hero", 80, 100, 0, 0, true, false, Array.Empty<string>())
        };

        // Act
        var result = _panel.Render(combatants);

        // Assert
        result.Any(line => line.Contains("HP")).Should().BeTrue();
        result.Any(line => line.Contains("80/100")).Should().BeTrue();
    }

    [Test]
    public void Render_WithMana_IncludesMpBar()
    {
        // Arrange
        var combatants = new List<(string, int, int, int, int, bool, bool, IReadOnlyList<string>)>
        {
            ("Mage", 50, 50, 30, 50, true, false, Array.Empty<string>())
        };

        // Act
        var result = _panel.Render(combatants);

        // Assert
        result.Any(line => line.Contains("MP")).Should().BeTrue();
    }

    [Test]
    public void Render_WithStatusEffects_IncludesEffects()
    {
        // Arrange
        var statuses = new List<string> { "Poisoned", "Burning" };
        var combatants = new List<(string, int, int, int, int, bool, bool, IReadOnlyList<string>)>
        {
            ("Hero", 80, 100, 0, 0, true, false, statuses)
        };

        // Act
        var result = _panel.Render(combatants);

        // Assert
        result.Any(line => line.Contains("[Poisoned]")).Should().BeTrue();
        result.Any(line => line.Contains("[Burning]")).Should().BeTrue();
    }

    #endregion
}
