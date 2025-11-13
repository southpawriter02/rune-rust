using RuneAndRust.Core;
using RuneAndRust.Core.Population;
using PopulationRoomArchetype = RuneAndRust.Core.Population.RoomArchetype;

namespace RuneAndRust.Engine.CoherentGlitch.Rules;

/// <summary>
/// Contextual Rule: Maintenance Hubs show signs of Haugbui-Class organization (v0.12)
/// Environmental storytelling: Haugbui obsessively STACKs and organizes
/// </summary>
public class MaintenanceHubOrganizationRule : CoherentGlitchRule
{
    public MaintenanceHubOrganizationRule()
    {
        RuleId = "maintenance_hub_organization";
        Description = "Maintenance Hubs have organized terrain from Haugbui activity";
        Priority = RulePriority.Medium;
        Type = RuleType.Contextual;
    }

    public override bool ShouldApply(Room room, PopulationContext context)
    {
        return room.Archetype == PopulationRoomArchetype.MaintenanceHub ||
               room.Name.Contains("maintenance", StringComparison.OrdinalIgnoreCase);
    }

    public override void Apply(Room room, PopulationContext context)
    {
        // Add organized rubble piles (Haugbui obsession)
        int pilesToAdd = context.Rng.Next(2, 5);

        for (int i = 0; i < pilesToAdd; i++)
        {
            var rubble = new RubblePile
            {
                Position = new Vector2(
                    context.Rng.Next(0, 10),
                    context.Rng.Next(0, 10)
                ),
                IsOrganized = true
            };

            rubble.Description = "Debris meticulously stacked into a perfect cube. " +
                               "The work of an obsessive automaton.";

            room.StaticTerrain.Add(rubble);
        }

        // Add machinery wreckage (what they're trying to maintain)
        var wreckage = new MachineryWreckage
        {
            MachineryType = "maintenance tools",
            Position = new Vector2(5, 5)
        };

        wreckage.Description = "Corroded maintenance tools, arranged in precise rows. " +
                             "Someone—or something—has been organizing this equipment.";

        room.StaticTerrain.Add(wreckage);

        // Increase Haugbui-Class spawn weight
        context.BiomeElements.ModifyWeight("haugbui_class", 3.0f);

        // Add environmental detail
        room.Description += " The chamber shows signs of compulsive organization—" +
                          "equipment arranged in perfect rows, debris stacked into geometric shapes.";

        _log.Information("Coherent Glitch Rule applied: MaintenanceHubOrganization, " +
            "Room={RoomId}, OrganizedPiles={Count}",
            room.RoomId, pilesToAdd);
    }
}
