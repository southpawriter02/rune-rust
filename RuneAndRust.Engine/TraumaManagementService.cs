using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// Manages trauma recovery and management mechanisms
/// v0.15: Trauma Economy
/// </summary>
public class TraumaManagementService
{
    private static readonly ILogger _log = Log.ForContext<TraumaManagementService>();
    private readonly Random _rng;
    private readonly TraumaEconomyService _traumaEconomy;

    // Costs and effectiveness
    private const int TherapyCost = 100; // Cogs
    private const int MedicationCost = 50; // Cogs
    private const int TherapyBufferDays = 7; // Extra days before progression
    private const float Level1ImprovementChance = 0.2f; // 20% chance to slightly reduce Level 1 trauma

    public TraumaManagementService(TraumaEconomyService traumaEconomy, Random? rng = null)
    {
        _traumaEconomy = traumaEconomy;
        _rng = rng ?? new Random();
    }

    /// <summary>
    /// Attempts to manage a trauma through various methods
    /// </summary>
    /// <param name="character">The character managing their trauma</param>
    /// <param name="trauma">The trauma to manage</param>
    /// <param name="method">The management method to use</param>
    /// <returns>True if management was successful, false otherwise</returns>
    public (bool success, string message) ManageTrauma(PlayerCharacter character, Trauma trauma, ManagementMethod method)
    {
        if (trauma.IsManagedThisSession)
        {
            return (false, "You've already addressed this trauma today. Rest before managing again.");
        }

        switch (method)
        {
            case ManagementMethod.Rest:
                return ManageThroughRest(character, trauma);

            case ManagementMethod.Therapy:
                return ManageThroughTherapy(character, trauma);

            case ManagementMethod.Medication:
                return ManageThroughMedication(character, trauma);

            case ManagementMethod.Ritual:
                return ManageThroughRitual(character, trauma);

            default:
                return (false, "Unknown management method.");
        }
    }

    /// <summary>
    /// Manages trauma through rest (free, minimal effect)
    /// </summary>
    private (bool success, string message) ManageThroughRest(PlayerCharacter character, Trauma trauma)
    {
        // Temporary stress reduction
        character.PsychicStress = Math.Max(0, character.PsychicStress - 10);

        // Reset management timer
        trauma.IsManagedThisSession = true;
        trauma.DaysSinceManagement = 0;

        _log.Information("Trauma managed through rest: Character={CharacterName}, Trauma={TraumaId}",
            character.Name, trauma.TraumaId);

        return (true,
            $"You take time to address your {trauma.Name}.\n" +
            "The weight lifts slightly. Temporarily.\n" +
            "-10 Stress");
    }

    /// <summary>
    /// Manages trauma through therapy (expensive, prevents progression)
    /// </summary>
    private (bool success, string message) ManageThroughTherapy(PlayerCharacter character, Trauma trauma)
    {
        // Check if player can afford it
        if (character.Currency < TherapyCost)
        {
            return (false, $"Therapy costs {TherapyCost} Cogs. You cannot afford it.");
        }

        // Deduct cost
        character.Currency -= TherapyCost;

        // Prevent progression for longer period
        trauma.IsManagedThisSession = true;
        trauma.DaysSinceManagement = -TherapyBufferDays; // Extra buffer

        _log.Information("Trauma managed through therapy: Character={CharacterName}, Trauma={TraumaId}, Cost={Cost}",
            character.Name, trauma.TraumaId, TherapyCost);

        var message = $"You spend time with a therapist.\n" +
                     "Talking helps. Doesn't fix it, but helps.\n" +
                     $"-{TherapyCost} Cogs";

        // Small permanent benefit for Level 1 traumas
        if (trauma.ProgressionLevel == 1 && _rng.NextDouble() < Level1ImprovementChance)
        {
            ReduceTraumaEffects(trauma, 0.9f);
            message += "\n\nThe therapy helps. You feel slightly better equipped to cope.";
            _log.Information("Therapy provided permanent improvement: Trauma={TraumaId}", trauma.TraumaId);
        }

        return (true, message);
    }

    /// <summary>
    /// Manages trauma through medication (moderate cost, temporary effect reduction)
    /// </summary>
    private (bool success, string message) ManageThroughMedication(PlayerCharacter character, Trauma trauma)
    {
        // Check if player can afford it
        if (character.Currency < MedicationCost)
        {
            return (false, $"Medication costs {MedicationCost} Cogs. You cannot afford it.");
        }

        // Deduct cost
        character.Currency -= MedicationCost;

        // Reduce stress and reset management
        character.PsychicStress = Math.Max(0, character.PsychicStress - 20);
        trauma.IsManagedThisSession = true;
        trauma.DaysSinceManagement = 0;

        _log.Information("Trauma managed through medication: Character={CharacterName}, Trauma={TraumaId}, Cost={Cost}",
            character.Name, trauma.TraumaId, MedicationCost);

        return (true,
            $"You take medication to manage the symptoms.\n" +
            "The edges blur. The thoughts quiet.\n" +
            $"-{MedicationCost} Cogs, -20 Stress\n\n" +
            "The effects are temporary, but provide relief.");
    }

    /// <summary>
    /// Manages trauma through rituals (special methods, varies by trauma)
    /// </summary>
    private (bool success, string message) ManageThroughRitual(PlayerCharacter character, Trauma trauma)
    {
        // Ritual effectiveness varies by trauma type
        string ritualDescription;
        int stressReduction = 15;

        switch (trauma.Category)
        {
            case TraumaCategory.Fear:
                ritualDescription = "You practice exposure techniques, facing your fears in small, controlled doses.";
                break;

            case TraumaCategory.Isolation:
                ritualDescription = "You write letters to the lost. They'll never be read, but it helps.";
                break;

            case TraumaCategory.Paranoia:
                ritualDescription = "You perform security checks, counting and verifying. The ritual soothes.";
                break;

            case TraumaCategory.Obsession:
                ritualDescription = "You engage with your compulsions deliberately, maintaining control.";
                break;

            case TraumaCategory.Dissociation:
                ritualDescription = "You ground yourself. Five things you see. Four you hear. Three you touch.";
                stressReduction = 20; // More effective for dissociation
                break;

            case TraumaCategory.Corruption:
                ritualDescription = "You meditate on human memories. Try to remember what it was like.";
                stressReduction = 5; // Less effective for corruption
                break;

            default:
                ritualDescription = "You perform a calming ritual.";
                break;
        }

        // Apply effects
        character.PsychicStress = Math.Max(0, character.PsychicStress - stressReduction);
        trauma.IsManagedThisSession = true;
        trauma.DaysSinceManagement = 0;

        _log.Information("Trauma managed through ritual: Character={CharacterName}, Trauma={TraumaId}, Category={Category}",
            character.Name, trauma.TraumaId, trauma.Category);

        return (true,
            $"{ritualDescription}\n" +
            $"-{stressReduction} Stress");
    }

    /// <summary>
    /// Reduces trauma effect intensity by a multiplier
    /// </summary>
    private void ReduceTraumaEffects(Trauma trauma, float multiplier)
    {
        foreach (var effect in trauma.Effects)
        {
            switch (effect)
            {
                case StressMultiplierEffect stressEffect:
                    var bonus = stressEffect.Multiplier - 1.0f;
                    stressEffect.Multiplier = 1.0f + (bonus * multiplier);
                    break;

                case PassiveStressEffect passiveEffect:
                    passiveEffect.StressPerTurn = Math.Max(1, (int)(passiveEffect.StressPerTurn * multiplier));
                    break;

                case AttributePenaltyEffect attrEffect:
                    attrEffect.Penalty = Math.Max(1, (int)(attrEffect.Penalty * multiplier));
                    break;
            }
        }
    }

    /// <summary>
    /// Resets all trauma management flags (call after rest/long rest)
    /// </summary>
    public void ResetManagementFlags(PlayerCharacter character)
    {
        foreach (var trauma in character.Traumas)
        {
            trauma.IsManagedThisSession = false;
        }

        _log.Debug("Reset trauma management flags: Character={CharacterName}, TraumaCount={Count}",
            character.Name, character.Traumas.Count);
    }

    /// <summary>
    /// Gets available management methods for a trauma
    /// </summary>
    public List<(ManagementMethod method, string description, int? cost)> GetAvailableMethods(
        PlayerCharacter character, Trauma trauma)
    {
        var methods = new List<(ManagementMethod, string, int?)>
        {
            (ManagementMethod.Rest, "Take time to address the trauma (-10 Stress)", null),
            (ManagementMethod.Ritual, $"Perform a {trauma.Category} ritual (-15-20 Stress)", null)
        };

        // Therapy and medication require currency
        if (character.Currency >= TherapyCost)
        {
            methods.Add((ManagementMethod.Therapy,
                $"Seek professional therapy (prevents progression for {TherapyBufferDays} days)", TherapyCost));
        }
        else
        {
            methods.Add((ManagementMethod.Therapy,
                $"[Cannot afford] Seek professional therapy", TherapyCost));
        }

        if (character.Currency >= MedicationCost)
        {
            methods.Add((ManagementMethod.Medication,
                "Take medication (-20 Stress, temporary relief)", MedicationCost));
        }
        else
        {
            methods.Add((ManagementMethod.Medication,
                "[Cannot afford] Take medication", MedicationCost));
        }

        return methods;
    }
}

/// <summary>
/// Methods for managing trauma
/// </summary>
public enum ManagementMethod
{
    Rest,        // Free, minimal effect
    Therapy,     // Expensive, prevents progression
    Medication,  // Moderate cost, reduces effects temporarily
    Ritual       // Special methods (varies by trauma)
}
