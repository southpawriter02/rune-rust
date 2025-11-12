using RuneAndRust.Core;
using RuneAndRust.Core.Population;

namespace RuneAndRust.Engine.CoherentGlitch.Rules;

/// <summary>
/// Balance Rule: Secret rooms have significantly increased loot (v0.12)
/// Reward player exploration - was 3× multiplier, now 5× (balance tuning)
/// </summary>
public class SecretRoomRewardRule : CoherentGlitchRule
{
    public SecretRoomRewardRule()
    {
        RuleId = "secret_room_reward";
        Description = "Secret rooms have 5× loot multiplier to reward exploration";
        Priority = RulePriority.Medium;
        Type = RuleType.Weighted;
    }

    public override bool ShouldApply(Room room, PopulationContext context)
    {
        return room.GeneratedNodeType == NodeType.Secret;
    }

    public override void Apply(Room room, PopulationContext context)
    {
        // Increase loot multiplier dramatically
        context.LootMultiplier = 5.0; // Was 3.0, now 5.0 (v0.12 balance tuning)

        // Guarantee at least one Rare-quality loot node
        var guaranteedLoot = new HiddenContainer
        {
            Quality = LootQuality.Rare,
            EstimatedCogsValue = 150,
            IsHidden = false, // Not hidden - secret room itself is the discovery
            Position = new Vector2(5, 5)
        };

        guaranteedLoot.Name = "Intact Storage Locker";
        guaranteedLoot.Description = "A remarkably well-preserved storage locker. " +
                                    "Its contents are pristine—untouched for 800 years.";

        room.LootNodes.Add(guaranteedLoot);

        // Add environmental detail
        room.Description += " This chamber was somehow spared from the Glitch's ravages. " +
                          "Equipment here is in surprisingly good condition.";

        _log.Information("Coherent Glitch Rule applied: SecretRoomReward, " +
            "Room={RoomId}, LootMultiplier={Multiplier}",
            room.RoomId, context.LootMultiplier);
    }
}
