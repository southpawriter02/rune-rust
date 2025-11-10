using NUnit.Framework;
using RuneAndRust.Core;
using RuneAndRust.Engine;

namespace RuneAndRust.Tests;

[TestFixture]
public class ProgressionIntegrationTests
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
            ProgressionPoints = 2, // Starting PP
            MaxHP = 50,
            HP = 50,
            MaxStamina = 30,
            Stamina = 30,
            Attributes = new Attributes(3, 3, 2, 2, 3),
            Abilities = CreateWarriorAbilities()
        };
    }

    [Test]
    public void FullProgression_Milestone0To1_CompleteCycle()
    {
        // Starting state
        Assert.That(_player.CurrentMilestone, Is.EqualTo(0));
        Assert.That(_player.ProgressionPoints, Is.EqualTo(2));

        // Award Legend to reach Milestone 1 (need 100 Legend)
        _sagaService.AwardLegend(_player, 10, 1.0f, 1.0f);   // 10
        _sagaService.AwardLegend(_player, 10, 1.0f, 1.0f);   // 20
        _sagaService.AwardLegend(_player, 25, 1.0f, 1.25f);  // 51
        _sagaService.AwardLegend(_player, 25, 1.0f, 1.25f);  // 82
        _sagaService.AwardLegend(_player, 10, 1.0f, 1.0f);   // 92
        _sagaService.AwardLegend(_player, 10, 1.0f, 1.0f);   // 102

        Assert.That(_player.CurrentLegend, Is.EqualTo(102));
        Assert.That(_sagaService.CanReachMilestone(_player), Is.True);

        // Reach Milestone
        _sagaService.ReachMilestone(_player);

        Assert.That(_player.CurrentMilestone, Is.EqualTo(1));
        Assert.That(_player.ProgressionPoints, Is.EqualTo(3)); // 2 + 1
        Assert.That(_player.MaxHP, Is.EqualTo(60)); // 50 + 10
        Assert.That(_player.MaxStamina, Is.EqualTo(35)); // 30 + 5
        Assert.That(_player.HP, Is.EqualTo(60)); // Full heal
        Assert.That(_player.Stamina, Is.EqualTo(35)); // Full restore
    }

    [Test]
    public void FullProgression_SpendPP_OnAttributesAndAbilities()
    {
        // Reach Milestone 1 to get PP
        _player.CurrentLegend = 100;
        _sagaService.ReachMilestone(_player);

        Assert.That(_player.ProgressionPoints, Is.EqualTo(3)); // 2 starting + 1 from milestone

        // Spend 1 PP on Might
        bool result1 = _sagaService.SpendPPOnAttribute(_player, "might");
        Assert.That(result1, Is.True);
        Assert.That(_player.Attributes.Might, Is.EqualTo(4));
        Assert.That(_player.ProgressionPoints, Is.EqualTo(2));

        // Reach another milestone to get more PP
        _player.CurrentLegend = 150;
        _sagaService.ReachMilestone(_player);
        Assert.That(_player.ProgressionPoints, Is.EqualTo(3)); // 2 + 1

        // Spend 1 PP on Finesse
        bool result2 = _sagaService.SpendPPOnAttribute(_player, "finesse");
        Assert.That(result2, Is.True);
        Assert.That(_player.Attributes.Finesse, Is.EqualTo(4));
        Assert.That(_player.ProgressionPoints, Is.EqualTo(2));

        // Need more PP to afford ability rank advancement (costs 5)
        _player.ProgressionPoints = 5;

        // Advance Power Strike to Rank 2
        var powerStrike = _player.Abilities.First(a => a.Name == "Power Strike");
        bool result3 = _sagaService.AdvanceAbilityRank(_player, powerStrike);
        Assert.That(result3, Is.True);
        Assert.That(powerStrike.CurrentRank, Is.EqualTo(2));
        Assert.That(_player.ProgressionPoints, Is.EqualTo(0));
    }

    [Test]
    public void FullProgression_MultipleAttributes_ToMaxCap()
    {
        // Give player enough PP to max out an attribute
        _player.ProgressionPoints = 3; // Might is at 3, need 3 PP to reach 6

        // Increase Might to cap
        _sagaService.SpendPPOnAttribute(_player, "might"); // 3 → 4
        _sagaService.SpendPPOnAttribute(_player, "might"); // 4 → 5
        _sagaService.SpendPPOnAttribute(_player, "might"); // 5 → 6

        Assert.That(_player.Attributes.Might, Is.EqualTo(6));
        Assert.That(_player.ProgressionPoints, Is.EqualTo(0));

        // Try to go over cap
        _player.ProgressionPoints = 1;
        bool result = _sagaService.SpendPPOnAttribute(_player, "might");

        Assert.That(result, Is.False); // Cannot exceed cap
        Assert.That(_player.Attributes.Might, Is.EqualTo(6)); // Still at cap
        Assert.That(_player.ProgressionPoints, Is.EqualTo(1)); // PP not spent
    }

    [Test]
    public void FullProgression_Milestone0To3_AllThresholds()
    {
        // Milestone 0 → 1: 100 Legend
        _player.CurrentLegend = 100;
        _sagaService.ReachMilestone(_player);
        Assert.That(_player.CurrentMilestone, Is.EqualTo(1));
        Assert.That(_player.LegendToNextMilestone, Is.EqualTo(150));

        // Milestone 1 → 2: 150 Legend
        _player.CurrentLegend = 150;
        _sagaService.ReachMilestone(_player);
        Assert.That(_player.CurrentMilestone, Is.EqualTo(2));
        Assert.That(_player.LegendToNextMilestone, Is.EqualTo(200));

        // Milestone 2 → 3: 200 Legend
        _player.CurrentLegend = 200;
        _sagaService.ReachMilestone(_player);
        Assert.That(_player.CurrentMilestone, Is.EqualTo(3));
        Assert.That(_player.LegendToNextMilestone, Is.EqualTo(0)); // Max milestone

        // Try to go beyond max milestone
        _player.CurrentLegend = 1000;
        bool canReach = _sagaService.CanReachMilestone(_player);
        Assert.That(canReach, Is.False);
    }

    [Test]
    public void FullProgression_RealisticGameplay_V01Scenario()
    {
        // Simulate a full v0.1 playthrough
        var player = _player;

        // Starting: Milestone 0, 2 PP
        Assert.That(player.CurrentMilestone, Is.EqualTo(0));
        Assert.That(player.ProgressionPoints, Is.EqualTo(2));

        // Spend starting PP on attributes
        _sagaService.SpendPPOnAttribute(player, "might");    // Might: 3 → 4
        _sagaService.SpendPPOnAttribute(player, "sturdiness"); // Sturdiness: 3 → 4
        Assert.That(player.ProgressionPoints, Is.EqualTo(0));

        // Room 1: Fight 2 Servitors (normal)
        _sagaService.AwardLegend(player, 10, 1.0f, 1.0f);
        _sagaService.AwardLegend(player, 10, 1.0f, 1.0f);
        Assert.That(player.CurrentLegend, Is.EqualTo(20));

        // Room 2: Fight Blight-Drone (blight area)
        _sagaService.AwardLegend(player, 25, 1.0f, 1.25f); // 31 Legend
        Assert.That(player.CurrentLegend, Is.EqualTo(51));

        // Room 3: Fight Blight-Drone (blight area)
        _sagaService.AwardLegend(player, 25, 1.0f, 1.25f); // 31 Legend
        Assert.That(player.CurrentLegend, Is.EqualTo(82));

        // Room 4: Solve Puzzle (taxing)
        _sagaService.AwardLegend(player, 50, 1.0f, 1.25f); // 63 Legend
        Assert.That(player.CurrentLegend, Is.EqualTo(145));

        // Reach Milestone 1!
        Assert.That(_sagaService.CanReachMilestone(player), Is.True);
        _sagaService.ReachMilestone(player);
        Assert.That(player.CurrentMilestone, Is.EqualTo(1));
        Assert.That(player.ProgressionPoints, Is.EqualTo(1));

        // Boss Fight: Ruin-Warden (taxing)
        _sagaService.AwardLegend(player, 100, 1.0f, 1.25f); // 125 Legend
        Assert.That(player.CurrentLegend, Is.EqualTo(270)); // 145 + 125

        // Can reach Milestone 2!
        Assert.That(_sagaService.CanReachMilestone(player), Is.True);
        _sagaService.ReachMilestone(player);
        Assert.That(player.CurrentMilestone, Is.EqualTo(2));

        // Final stats
        Assert.That(player.MaxHP, Is.EqualTo(70)); // 50 + 10 + 10
        Assert.That(player.MaxStamina, Is.EqualTo(40)); // 30 + 5 + 5
        Assert.That(player.ProgressionPoints, Is.EqualTo(2)); // 1 + 1
        Assert.That(player.Attributes.Might, Is.EqualTo(4));
        Assert.That(player.Attributes.Sturdiness, Is.EqualTo(4));
    }

    [Test]
    public void FullProgression_SpendPPEfficiently_BalancedBuild()
    {
        // Give player 10 PP to build with
        _player.ProgressionPoints = 10;

        // Build strategy: 2 attributes + 1 ability rank
        _sagaService.SpendPPOnAttribute(_player, "might");    // 1 PP: 3 → 4
        _sagaService.SpendPPOnAttribute(_player, "finesse");  // 1 PP: 3 → 4
        _sagaService.SpendPPOnAttribute(_player, "sturdiness"); // 1 PP: 3 → 4

        Assert.That(_player.ProgressionPoints, Is.EqualTo(7));

        // Advance Power Strike to Rank 2
        var powerStrike = _player.Abilities.First(a => a.Name == "Power Strike");
        _sagaService.AdvanceAbilityRank(_player, powerStrike); // 5 PP

        Assert.That(_player.ProgressionPoints, Is.EqualTo(2));

        // Spend remaining on attributes
        _sagaService.SpendPPOnAttribute(_player, "will");      // 1 PP: 2 → 3
        _sagaService.SpendPPOnAttribute(_player, "wits");      // 1 PP: 2 → 3

        Assert.That(_player.ProgressionPoints, Is.EqualTo(0));

        // Verify final build
        Assert.That(_player.Attributes.Might, Is.EqualTo(4));
        Assert.That(_player.Attributes.Finesse, Is.EqualTo(4));
        Assert.That(_player.Attributes.Sturdiness, Is.EqualTo(4));
        Assert.That(_player.Attributes.Will, Is.EqualTo(3));
        Assert.That(_player.Attributes.Wits, Is.EqualTo(3));
        Assert.That(powerStrike.CurrentRank, Is.EqualTo(2));
    }

    private List<Ability> CreateWarriorAbilities()
    {
        return new List<Ability>
        {
            new Ability
            {
                Name = "Power Strike",
                Description = "A devastating melee attack",
                StaminaCost = 5,
                Type = AbilityType.Attack,
                CurrentRank = 1,
                CostToRank2 = 5,
                AttributeUsed = "might",
                BonusDice = 2,
                SuccessThreshold = 3
            },
            new Ability
            {
                Name = "Shield Wall",
                Description = "Raise your defenses",
                StaminaCost = 10,
                Type = AbilityType.Defense,
                CurrentRank = 1,
                CostToRank2 = 5,
                AttributeUsed = "sturdiness",
                BonusDice = 1,
                DefensePercent = 50,
                DefenseDuration = 2
            }
        };
    }
}
