using RuneAndRust.Core;
using RuneAndRust.Core.Population;

namespace RuneAndRust.Engine.CoherentGlitch.Rules;

/// <summary>
/// Balance Rule: Adjusts hidden container discovery DC (v0.12)
/// Was DC 15 (too hard), now DC 12 (40%+ discovery rate target)
/// </summary>
public class HiddenContainerDiscoveryRule : CoherentGlitchRule
{
    public HiddenContainerDiscoveryRule()
    {
        RuleId = "hidden_container_discovery";
        Description = "Adjusts hidden container discovery DC for better findability";
        Priority = RulePriority.Low;
        Type = RuleType.Weighted;
    }

    public override bool ShouldApply(Room room, PopulationContext context)
    {
        return room.LootNodes.Any(l => l is HiddenContainer);
    }

    public override void Apply(Room room, PopulationContext context)
    {
        var hiddenContainers = room.LootNodes.OfType<HiddenContainer>().ToList();

        foreach (var container in hiddenContainers)
        {
            // Reduce DC from 15 to 12 (v0.12 balance tuning)
            if (container.DiscoveryDC > 12)
            {
                container.DiscoveryDC = 12;
            }

            // Add visual hints to room description
            if (!room.Description.Contains("concealed") && !room.Description.Contains("hidden"))
            {
                room.Description += " Something about this room seems off—" +
                                  "as if there's more here than meets the eye.";
            }
        }

        _log.Information("Coherent Glitch Rule applied: HiddenContainerDiscovery, " +
            "Room={RoomId}, ContainersAdjusted={Count}",
            room.RoomId, hiddenContainers.Count);
    }
}
