using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Presentation.Gui.ViewModels;

namespace RuneAndRust.Application.UnitTests.Presentation.Gui.ViewModels;

/// <summary>
/// Unit tests for <see cref="CombatSummaryViewModel"/>.
/// </summary>
[TestFixture]
public class CombatSummaryViewModelTests
{
    private CombatSummaryViewModel _viewModel = null!;

    [SetUp]
    public void SetUp()
    {
        _viewModel = new CombatSummaryViewModel();
    }

    /// <summary>
    /// Verifies victory header text.
    /// </summary>
    [Test]
    public void HeaderText_WhenVictory_ReturnsVictoryHeader()
    {
        // Arrange
        _viewModel.LoadVictory([], 0, 0, new CombatStatisticsViewModel());

        // Assert
        _viewModel.HeaderText.Should().Be("⚔ VICTORY! ⚔");
    }

    /// <summary>
    /// Verifies defeat header text.
    /// </summary>
    [Test]
    public void HeaderText_WhenDefeat_ReturnsDefeatHeader()
    {
        // Arrange
        _viewModel.LoadDefeat(new CombatStatisticsViewModel());

        // Assert
        _viewModel.HeaderText.Should().Be("💀 DEFEAT 💀");
    }

    /// <summary>
    /// Verifies XP text formatting.
    /// </summary>
    [Test]
    public void XpText_With125Xp_ReturnsFormattedText()
    {
        // Arrange
        _viewModel.LoadVictory([], 125, 0, new CombatStatisticsViewModel());

        // Assert
        _viewModel.XpText.Should().Contain("125");
        _viewModel.XpText.Should().Contain("XP");
    }

    /// <summary>
    /// Verifies gold text formatting.
    /// </summary>
    [Test]
    public void GoldText_With45Gold_ReturnsFormattedText()
    {
        // Arrange
        _viewModel.LoadVictory([], 0, 45, new CombatStatisticsViewModel());

        // Assert
        _viewModel.GoldText.Should().Contain("45");
        _viewModel.GoldText.Should().Contain("gold");
    }

    /// <summary>
    /// Verifies defeated enemies are formatted with skull.
    /// </summary>
    [Test]
    public void LoadVictory_WithEnemies_AddsSkulls()
    {
        // Arrange
        var enemies = new[] { "Goblin", "Skeleton" };

        // Act
        _viewModel.LoadVictory(enemies, 100, 50, new CombatStatisticsViewModel());

        // Assert
        _viewModel.DefeatedEnemies.Should().HaveCount(2);
        _viewModel.DefeatedEnemies[0].Should().Contain("☠");
        _viewModel.DefeatedEnemies[0].Should().Contain("Goblin");
    }

    /// <summary>
    /// Verifies HasLoot is true when loot exists.
    /// </summary>
    [Test]
    public void HasLoot_WithLoot_ReturnsTrue()
    {
        // Arrange
        _viewModel.AddLoot(new[] { new LootItemViewModel("sword-001", "Iron Sword") });

        // Assert
        _viewModel.HasLoot.Should().BeTrue();
    }

    /// <summary>
    /// Verifies CollectAll clears loot and fires event.
    /// </summary>
    [Test]
    public void CollectAll_WithLoot_ClearsAndFiresEvent()
    {
        // Arrange
        _viewModel.AddLoot(new[] { new LootItemViewModel("potion-001", "Health Potion") });
        var eventFired = false;
        _viewModel.LootCollected += _ => eventFired = true;

        // Act
        _viewModel.CollectAll();

        // Assert
        _viewModel.HasLoot.Should().BeFalse();
        eventFired.Should().BeTrue();
    }

    /// <summary>
    /// Verifies level-up information is set correctly.
    /// </summary>
    [Test]
    public void SetLevelUp_WithBonuses_SetsProperties()
    {
        // Arrange
        var bonuses = new[]
        {
            LevelUpBonusViewModel.StatIncrease("Max Health", 45, 50),
            LevelUpBonusViewModel.AbilityUnlocked("Shield Bash")
        };

        // Act
        _viewModel.SetLevelUp(5, bonuses);

        // Assert
        _viewModel.DidLevelUp.Should().BeTrue();
        _viewModel.NewLevel.Should().Be(5);
        _viewModel.LevelUpBonuses.Should().HaveCount(2);
        _viewModel.LevelUpText.Should().Contain("Level 5");
    }

    /// <summary>
    /// Verifies Continue command fires CloseRequested event.
    /// </summary>
    [Test]
    public void Continue_FiresCloseRequestedEvent()
    {
        // Arrange
        var eventFired = false;
        _viewModel.CloseRequested += () => eventFired = true;

        // Act
        _viewModel.Continue();

        // Assert
        eventFired.Should().BeTrue();
    }
}
