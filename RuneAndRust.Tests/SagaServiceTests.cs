using NUnit.Framework;
using RuneAndRust.Core;
using RuneAndRust.Engine;

namespace RuneAndRust.Tests;

[TestFixture]
public class SagaServiceTests
{
    private SagaService _sagaService;
    private PlayerCharacter _player;

    [SetUp]
    public void Setup()
    {
        _sagaService = new SagaService();
        _player = new PlayerCharacter
        {
            Name = "Test Hero",
            Class = CharacterClass.Warrior,
            CurrentMilestone = 0,
            CurrentLegend = 0,
            LegendToNextMilestone = 100,
            ProgressionPoints = 2,
            MaxHP = 50,
            HP = 50,
            MaxStamina = 30,
            Stamina = 30,
            Attributes = new Attributes(3, 3, 2, 2, 3)
        };
    }

    #region Legend Award Tests

    [Test]
    public void AwardLegend_BasicFormula_CalculatesCorrectly()
    {
        // Arrange
        int baseLegendValue = 10;
        float difficultyMod = 1.0f;
        float traumaMod = 1.0f;

        // Act
        _sagaService.AwardLegend(_player, baseLegendValue, difficultyMod, traumaMod);

        // Assert
        Assert.That(_player.CurrentLegend, Is.EqualTo(10));
    }

    [Test]
    public void AwardLegend_WithTraumaMod_CalculatesCorrectly()
    {
        // Arrange: Boss fight with taxing trauma
        int baseLegendValue = 100;
        float difficultyMod = 1.0f;
        float traumaMod = 1.25f;

        // Act
        _sagaService.AwardLegend(_player, baseLegendValue, difficultyMod, traumaMod);

        // Assert
        Assert.That(_player.CurrentLegend, Is.EqualTo(125));
    }

    [Test]
    public void AwardLegend_MultipleSources_Accumulates()
    {
        // Arrange & Act
        _sagaService.AwardLegend(_player, 10, 1.0f, 1.0f);  // 10 Legend
        _sagaService.AwardLegend(_player, 25, 1.0f, 1.25f); // 31 Legend
        _sagaService.AwardLegend(_player, 100, 1.0f, 1.25f); // 125 Legend

        // Assert
        Assert.That(_player.CurrentLegend, Is.EqualTo(166)); // 10 + 31 + 125
    }

    [Test]
    public void AwardLegend_AtMaxMilestone_DoesNotAward()
    {
        // Arrange
        _player.CurrentMilestone = 3; // Max milestone

        // Act
        _sagaService.AwardLegend(_player, 100, 1.0f, 1.0f);

        // Assert
        Assert.That(_player.CurrentLegend, Is.EqualTo(0));
    }

    #endregion

    #region Milestone Tests

    [Test]
    public void CanReachMilestone_WithEnoughLegend_ReturnsTrue()
    {
        // Arrange
        _player.CurrentLegend = 100;
        _player.LegendToNextMilestone = 100;

        // Act
        bool canReach = _sagaService.CanReachMilestone(_player);

        // Assert
        Assert.That(canReach, Is.True);
    }

    [Test]
    public void CanReachMilestone_WithoutEnoughLegend_ReturnsFalse()
    {
        // Arrange
        _player.CurrentLegend = 50;
        _player.LegendToNextMilestone = 100;

        // Act
        bool canReach = _sagaService.CanReachMilestone(_player);

        // Assert
        Assert.That(canReach, Is.False);
    }

    [Test]
    public void CanReachMilestone_AtMaxMilestone_ReturnsFalse()
    {
        // Arrange
        _player.CurrentMilestone = 3;
        _player.CurrentLegend = 1000;

        // Act
        bool canReach = _sagaService.CanReachMilestone(_player);

        // Assert
        Assert.That(canReach, Is.False);
    }

    [Test]
    public void ReachMilestone_IncrementsAndRewards()
    {
        // Arrange
        _player.CurrentLegend = 100;
        _player.LegendToNextMilestone = 100;
        int oldHP = _player.HP;
        int oldMaxHP = _player.MaxHP;
        int oldStamina = _player.Stamina;
        int oldMaxStamina = _player.MaxStamina;
        int oldPP = _player.ProgressionPoints;

        // Act
        _sagaService.ReachMilestone(_player);

        // Assert
        Assert.That(_player.CurrentMilestone, Is.EqualTo(1));
        Assert.That(_player.MaxHP, Is.EqualTo(oldMaxHP + 10));
        Assert.That(_player.MaxStamina, Is.EqualTo(oldMaxStamina + 5));
        Assert.That(_player.ProgressionPoints, Is.EqualTo(oldPP + 1));
        Assert.That(_player.HP, Is.EqualTo(_player.MaxHP)); // Full heal
        Assert.That(_player.Stamina, Is.EqualTo(_player.MaxStamina)); // Full restore
    }

    [Test]
    public void ReachMilestone_UpdatesNextMilestoneThreshold()
    {
        // Arrange
        _player.CurrentLegend = 100;
        _player.LegendToNextMilestone = 100;

        // Act
        _sagaService.ReachMilestone(_player);

        // Assert: Milestone 1→2 should require 150 Legend
        Assert.That(_player.LegendToNextMilestone, Is.EqualTo(150));
    }

    [Test]
    public void CalculateLegendToNextMilestone_FollowsFormula()
    {
        // Assert: (CurrentMilestone × 50) + 100
        Assert.That(_sagaService.CalculateLegendToNextMilestone(0), Is.EqualTo(100)); // M0→M1
        Assert.That(_sagaService.CalculateLegendToNextMilestone(1), Is.EqualTo(150)); // M1→M2
        Assert.That(_sagaService.CalculateLegendToNextMilestone(2), Is.EqualTo(200)); // M2→M3
        Assert.That(_sagaService.CalculateLegendToNextMilestone(3), Is.EqualTo(0));   // Max
    }

    #endregion

    #region PP Spending on Attributes Tests

    [Test]
    public void SpendPPOnAttribute_WithEnoughPP_IncreasesAttribute()
    {
        // Arrange
        _player.ProgressionPoints = 1;
        int oldMight = _player.Attributes.Might;

        // Act
        bool result = _sagaService.SpendPPOnAttribute(_player, "might");

        // Assert
        Assert.That(result, Is.True);
        Assert.That(_player.Attributes.Might, Is.EqualTo(oldMight + 1));
        Assert.That(_player.ProgressionPoints, Is.EqualTo(0));
    }

    [Test]
    public void SpendPPOnAttribute_WithoutEnoughPP_ReturnsFalse()
    {
        // Arrange
        _player.ProgressionPoints = 0;

        // Act
        bool result = _sagaService.SpendPPOnAttribute(_player, "might");

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void SpendPPOnAttribute_AtCap_ReturnsFalse()
    {
        // Arrange
        _player.ProgressionPoints = 5;
        _player.Attributes.Might = 6; // At cap

        // Act
        bool result = _sagaService.SpendPPOnAttribute(_player, "might");

        // Assert
        Assert.That(result, Is.False);
        Assert.That(_player.ProgressionPoints, Is.EqualTo(5)); // PP not spent
    }

    [Test]
    public void SpendPPOnAttribute_InvalidAttribute_ThrowsException()
    {
        // Arrange
        _player.ProgressionPoints = 1;

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            _sagaService.SpendPPOnAttribute(_player, "invalid"));
    }

    [Test]
    [TestCase("might")]
    [TestCase("finesse")]
    [TestCase("wits")]
    [TestCase("will")]
    [TestCase("sturdiness")]
    public void SpendPPOnAttribute_AllAttributes_WorksCorrectly(string attributeName)
    {
        // Arrange
        _player.ProgressionPoints = 1;
        int oldValue = _player.GetAttributeValue(attributeName);

        // Act
        bool result = _sagaService.SpendPPOnAttribute(_player, attributeName);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(_player.GetAttributeValue(attributeName), Is.EqualTo(oldValue + 1));
    }

    #endregion

    #region Trauma Modifier Tests

    [Test]
    [TestCase("normal", 1.0f)]
    [TestCase("puzzle", 1.25f)]
    [TestCase("boss", 1.25f)]
    [TestCase("blight", 1.25f)]
    public void CalculateTraumaMod_ReturnsCorrectModifier(string encounterType, float expectedMod)
    {
        // Act
        float traumaMod = _sagaService.CalculateTraumaMod(encounterType);

        // Assert
        Assert.That(traumaMod, Is.EqualTo(expectedMod));
    }

    [Test]
    public void CalculateTraumaMod_UnknownType_ReturnsDefault()
    {
        // Act
        float traumaMod = _sagaService.CalculateTraumaMod("unknown");

        // Assert
        Assert.That(traumaMod, Is.EqualTo(1.0f));
    }

    #endregion
}
