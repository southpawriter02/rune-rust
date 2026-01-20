using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Presentation.Gui.ViewModels;

namespace RuneAndRust.Application.UnitTests.Presentation.Gui.ViewModels;

/// <summary>
/// Unit tests for <see cref="PlayerStatusViewModel"/>.
/// </summary>
[TestFixture]
public class PlayerStatusViewModelTests
{
    private PlayerStatusViewModel _viewModel = null!;

    [SetUp]
    public void SetUp()
    {
        _viewModel = new PlayerStatusViewModel();
    }

    /// <summary>
    /// Verifies that the default constructor initializes sample data.
    /// </summary>
    [Test]
    public void Constructor_WithNoParams_HasSampleData()
    {
        // Assert
        _viewModel.PlayerName.Should().Be("Hero");
        _viewModel.ClassName.Should().Be("Warrior");
        _viewModel.Level.Should().Be(5);
        _viewModel.CurrentHp.Should().Be(65);
        _viewModel.MaxHp.Should().Be(100);
    }

    /// <summary>
    /// Verifies that UpdateCharacterInfo updates name, class, and level.
    /// </summary>
    [Test]
    public void UpdateCharacterInfo_WithValidData_UpdatesProperties()
    {
        // Act
        _viewModel.UpdateCharacterInfo("Aragorn", "Ranger", 10);

        // Assert
        _viewModel.PlayerName.Should().Be("Aragorn");
        _viewModel.ClassName.Should().Be("Ranger");
        _viewModel.Level.Should().Be(10);
    }

    /// <summary>
    /// Verifies that UpdateResources updates HP, MP, and XP values.
    /// </summary>
    [Test]
    public void UpdateResources_WithValidData_UpdatesAllBars()
    {
        // Act
        _viewModel.UpdateResources(50, 100, 25, 50, 750, 1000);

        // Assert
        _viewModel.CurrentHp.Should().Be(50);
        _viewModel.MaxHp.Should().Be(100);
        _viewModel.CurrentMp.Should().Be(25);
        _viewModel.MaxMp.Should().Be(50);
        _viewModel.CurrentXp.Should().Be(750);
        _viewModel.XpToNextLevel.Should().Be(1000);
    }

    /// <summary>
    /// Verifies that CalculateModifier returns correct D&D-style modifier.
    /// </summary>
    [TestCase(10, 0)]
    [TestCase(16, 3)]
    [TestCase(8, -1)]
    [TestCase(20, 5)]
    [TestCase(1, -4)]
    public void CalculateModifier_WithValue_ReturnsCorrectModifier(int statValue, int expected)
    {
        // Act
        var result = PlayerStatusViewModel.CalculateModifier(statValue);

        // Assert
        result.Should().Be(expected);
    }

    /// <summary>
    /// Verifies that UpdateStat updates stat value and recalculates modifier.
    /// </summary>
    [Test]
    public void UpdateStat_WithStrength_UpdatesValueAndModifier()
    {
        // Act
        _viewModel.UpdateStat("strength", 18);

        // Assert
        _viewModel.Strength.Should().Be(18);
        _viewModel.StrengthModifier.Should().Be(4);
    }

    /// <summary>
    /// Verifies that UpdateEquipmentSlot updates the slot with item info.
    /// </summary>
    [Test]
    public void UpdateEquipmentSlot_WithItem_UpdatesSlot()
    {
        // Act
        _viewModel.UpdateEquipmentSlot(0, "Great Sword", "+10 atk");

        // Assert
        _viewModel.Equipment[0].ItemName.Should().Be("Great Sword");
        _viewModel.Equipment[0].ItemBonus.Should().Be("+10 atk");
        _viewModel.Equipment[0].IsEmpty.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that Clear resets all properties to default.
    /// </summary>
    [Test]
    public void Clear_ResetsAllPropertiesToDefault()
    {
        // Act
        _viewModel.Clear();

        // Assert
        _viewModel.PlayerName.Should().Be("Unknown");
        _viewModel.ClassName.Should().Be("Adventurer");
        _viewModel.Level.Should().Be(1);
        _viewModel.CurrentHp.Should().Be(100);
        _viewModel.Gold.Should().Be(0);
    }

    /// <summary>
    /// Verifies that equipment collection has 5 slots after initialization.
    /// </summary>
    [Test]
    public void Equipment_AfterInit_HasFiveSlots()
    {
        // Assert
        _viewModel.Equipment.Should().HaveCount(5);
        _viewModel.Equipment[0].SlotName.Should().Be("Weapon");
        _viewModel.Equipment[1].SlotName.Should().Be("Armor");
        _viewModel.Equipment[2].SlotName.Should().Be("Shield");
        _viewModel.Equipment[3].SlotName.Should().Be("Ring");
        _viewModel.Equipment[4].SlotName.Should().Be("Amulet");
    }
}
