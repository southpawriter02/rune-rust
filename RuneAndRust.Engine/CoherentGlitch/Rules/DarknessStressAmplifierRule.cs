using RuneAndRust.Core;
using RuneAndRust.Core.Population;

namespace RuneAndRust.Engine.CoherentGlitch.Rules;

/// <summary>
/// Weighted Rule: [Darkness] amplifies psychic resonance effects (v0.12)
/// Darkness + Psychic Resonance = extreme stress
/// </summary>
public class DarknessStressAmplifierRule : CoherentGlitchRule
{
    public DarknessStressAmplifierRule()
    {
        RuleId = "darkness_stress_amplifier";
        Description = "Darkness condition amplifies psychic stress";
        Priority = RulePriority.High;
        Type = RuleType.Weighted;
    }

    public override bool ShouldApply(Room room, PopulationContext context)
    {
        return room.HasAmbientCondition("[Darkness]");
    }

    public override void Apply(Room room, PopulationContext context)
    {
        // Increase psychic resonance spawn weight
        context.BiomeElements.ModifyWeight("psychic_resonance", 2.0f);

        // If room has both Darkness and Psychic Resonance, amplify stress
        if (room.HasAmbientCondition("[Psychic Resonance]"))
        {
            var darkness = room.AmbientConditions
                .OfType<DarknessCondition>()
                .FirstOrDefault();

            var psychicRes = room.AmbientConditions
                .OfType<PsychicResonanceCondition>()
                .FirstOrDefault();

            if (darkness != null && psychicRes != null)
            {
                // Double the combined stress
                darkness.StressPerTurn += 1; // Was 1, now 2
                psychicRes.StressPerTurn += 1; // Was 2, now 3

                // Add narrative description
                room.Description += " The oppressive darkness amplifies the psychic resonance, " +
                                  "creating a suffocating atmosphere of dread.";

                _log.Information("Coherent Glitch Rule applied: DarknessStressAmplifier, " +
                    "Room={RoomId}, TotalStress={TotalStress}/turn",
                    room.RoomId, darkness.StressPerTurn + psychicRes.StressPerTurn);
            }
        }
    }
}
