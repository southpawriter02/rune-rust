using NUnit.Framework;
using RuneAndRust.Core;
using RuneAndRust.Engine;

namespace RuneAndRust.Tests;

[TestFixture]
public class AbilityRankTests
{
    private SagaService _sagaService;
    private PlayerCharacter _player;
    private Ability _powerStrike;

    [SetUp]
    public void Setup()
    {
        _sagaService = new SagaService();

        _powerStrike = new Ability
        {
            Name = "Power Strike",
            Description = "A powerful melee attack",
            StaminaCost = 5,
            Type = AbilityType.Attack,
            CurrentRank = 1,
            MaxRank = 3,
            CostToRank2 = 5,
            AttributeUsed = "might",
            BonusDice = 2,
            SuccessThreshold = 3,
            DamageDice = 0
        };

        _player = new PlayerCharacter
        {
            Name = "Test Hero",
            Class = CharacterClass.Warrior,
            CurrentMilestone = 1,
            CurrentLegend = 0,
            ProgressionPoints = 10,
            Abilities = new List<Ability> { _powerStrike },
            Attributes = new Attributes(3, 3, 2, 2, 3)
        };
    }

    [Test]
    public void AdvanceAbilityRank_WithEnoughPP_AdvancesToRank2()
    {
        // Arrange
        _player.ProgressionPoints = 5;

        // Act
        bool result = _sagaService.AdvanceAbilityRank(_player, _powerStrike);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(_powerStrike.CurrentRank, Is.EqualTo(2));
        Assert.That(_player.ProgressionPoints, Is.EqualTo(0));
    }

    [Test]
    public void AdvanceAbilityRank_WithoutEnoughPP_ReturnsFalse()
    {
        // Arrange
        _player.ProgressionPoints = 3;

        // Act
        bool result = _sagaService.AdvanceAbilityRank(_player, _powerStrike);

        // Assert
        Assert.That(result, Is.False);
        Assert.That(_powerStrike.CurrentRank, Is.EqualTo(1));
        Assert.That(_player.ProgressionPoints, Is.EqualTo(3)); // PP unchanged
    }

    [Test]
    public void AdvanceAbilityRank_AlreadyAtRank2_ReturnsFalse()
    {
        // Arrange
        _powerStrike.CurrentRank = 2;
        _player.ProgressionPoints = 10;

        // Act
        bool result = _sagaService.AdvanceAbilityRank(_player, _powerStrike);

        // Assert
        Assert.That(result, Is.False); // Rank 3 locked for v0.5+
        Assert.That(_powerStrike.CurrentRank, Is.EqualTo(2));
    }

    [Test]
    public void AdvanceAbilityRank_PowerStrike_AppliesRank2Improvements()
    {
        // Arrange
        _player.ProgressionPoints = 5;

        // Act
        _sagaService.AdvanceAbilityRank(_player, _powerStrike);

        // Assert
        Assert.That(_powerStrike.BonusDice, Is.EqualTo(3)); // Was 2, now 3
        Assert.That(_powerStrike.SuccessThreshold, Is.EqualTo(2)); // Easier trigger
        Assert.That(_powerStrike.StaminaCost, Is.EqualTo(4)); // Was 5, now 4
    }

    [Test]
    public void AdvanceAbilityRank_ShieldWall_AppliesRank2Improvements()
    {
        // Arrange
        var shieldWall = new Ability
        {
            Name = "Shield Wall",
            CurrentRank = 1,
            CostToRank2 = 5,
            BonusDice = 1,
            DefensePercent = 50,
            DefenseDuration = 2
        };
        _player.Abilities.Add(shieldWall);
        _player.ProgressionPoints = 5;

        // Act
        _sagaService.AdvanceAbilityRank(_player, shieldWall);

        // Assert
        Assert.That(shieldWall.BonusDice, Is.EqualTo(2));
        Assert.That(shieldWall.DefensePercent, Is.EqualTo(75));
        Assert.That(shieldWall.DefenseDuration, Is.EqualTo(3));
    }

    [Test]
    public void AdvanceAbilityRank_QuickDodge_AppliesRank2Improvements()
    {
        // Arrange
        var quickDodge = new Ability
        {
            Name = "Quick Dodge",
            CurrentRank = 1,
            CostToRank2 = 5,
            BonusDice = 1,
            StaminaCost = 3
        };
        _player.Abilities.Add(quickDodge);
        _player.ProgressionPoints = 5;

        // Act
        _sagaService.AdvanceAbilityRank(_player, quickDodge);

        // Assert
        Assert.That(quickDodge.BonusDice, Is.EqualTo(2));
        Assert.That(quickDodge.StaminaCost, Is.EqualTo(2)); // Cheaper
    }

    [Test]
    public void AdvanceAbilityRank_AethericBolt_AppliesRank2Improvements()
    {
        // Arrange
        var aethericBolt = new Ability
        {
            Name = "Aetheric Bolt",
            CurrentRank = 1,
            CostToRank2 = 5,
            BonusDice = 2,
            DamageDice = 1
        };
        _player.Abilities.Add(aethericBolt);
        _player.ProgressionPoints = 5;

        // Act
        _sagaService.AdvanceAbilityRank(_player, aethericBolt);

        // Assert
        Assert.That(aethericBolt.BonusDice, Is.EqualTo(3));
        Assert.That(aethericBolt.DamageDice, Is.EqualTo(2)); // More damage
    }

    [Test]
    public void AdvanceAbilityRank_MultipleAbilities_TracksRanksIndependently()
    {
        // Arrange
        var ability2 = new Ability
        {
            Name = "Shield Wall",
            CurrentRank = 1,
            CostToRank2 = 5
        };
        _player.Abilities.Add(ability2);
        _player.ProgressionPoints = 10;

        // Act
        _sagaService.AdvanceAbilityRank(_player, _powerStrike);
        _sagaService.AdvanceAbilityRank(_player, ability2);

        // Assert
        Assert.That(_powerStrike.CurrentRank, Is.EqualTo(2));
        Assert.That(ability2.CurrentRank, Is.EqualTo(2));
        Assert.That(_player.ProgressionPoints, Is.EqualTo(0));
    }

    [Test]
    public void AbilityRank_DefaultValues_AreCorrect()
    {
        // Assert
        Assert.That(_powerStrike.CurrentRank, Is.EqualTo(1));
        Assert.That(_powerStrike.MaxRank, Is.EqualTo(3));
        Assert.That(_powerStrike.CostToRank2, Is.EqualTo(5));
    }
}
