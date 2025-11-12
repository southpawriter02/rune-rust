using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// Manages the Trauma Economy system - Psychic Stress and Runic Blight Corruption
/// v0.15: Added Breaking Point mechanics and Trauma acquisition
/// </summary>
public class TraumaEconomyService
{
    private static readonly ILogger _log = Log.ForContext<TraumaEconomyService>();
    private readonly Random _rng;
    private const int MaxTraumaValue = 100;
    private const int MinTraumaValue = 0;
    private const int BreakingPointStress = 60; // Stress resets to this after Breaking Point

    public TraumaEconomyService(Random? rng = null)
    {
        _rng = rng ?? new Random();
    }

    /// <summary>
    /// Adds Psychic Stress to a character with optional WILL-based Resolve Check reduction
    /// v0.15: Now triggers Breaking Point at 100 Stress
    /// </summary>
    /// <param name="character">The character gaining stress</param>
    /// <param name="baseAmount">Base stress amount before resistance</param>
    /// <param name="source">What caused the stress (for Breaking Point context)</param>
    /// <param name="allowResolveCheck">If true, WILL can reduce the stress gain</param>
    /// <param name="resolveSuccesses">Number of successes from WILL Resolve Check (if applicable)</param>
    /// <returns>Tuple of (actual stress gained, trauma acquired if Breaking Point occurred)</returns>
    public (int stressGained, Trauma? traumaAcquired) AddStress(
        PlayerCharacter character,
        int baseAmount,
        string source = "unknown",
        bool allowResolveCheck = false,
        int resolveSuccesses = 0)
    {
        if (baseAmount <= 0) return (0, null);

        int actualAmount = baseAmount;

        // Apply Resolve Check reduction if allowed
        if (allowResolveCheck && resolveSuccesses > 0)
        {
            actualAmount = Math.Max(0, baseAmount - resolveSuccesses);
            _log.Debug("Stress reduced by Resolve Check: Character={CharacterName}, BaseAmount={Base}, Successes={Successes}, ReducedAmount={Reduced}",
                character.Name, baseAmount, resolveSuccesses, actualAmount);
        }

        // Apply trauma stress multipliers
        float stressMultiplier = character.GetTraumaStressMultiplier(source);
        actualAmount = (int)(actualAmount * stressMultiplier);

        // Apply stress gain
        int oldStress = character.PsychicStress;
        character.PsychicStress = Math.Clamp(character.PsychicStress + actualAmount, MinTraumaValue, MaxTraumaValue);
        int actualGained = character.PsychicStress - oldStress;

        var threshold = GetStressThreshold(character);
        var oldThreshold = GetStressThresholdForValue(oldStress);

        _log.Information("Stress gained: Character={CharacterName}, Amount={Amount}, OldStress={Old}, NewStress={New}, Threshold={Threshold}, Source={Source}",
            character.Name, actualGained, oldStress, character.PsychicStress, threshold, source);

        // Log threshold transitions
        if (oldThreshold != threshold)
        {
            _log.Warning("Stress threshold crossed: Character={CharacterName}, OldThreshold={OldThreshold}, NewThreshold={NewThreshold}",
                character.Name, oldThreshold, threshold);
        }

        // v0.15: Check for Breaking Point
        Trauma? acquiredTrauma = null;
        if (character.PsychicStress >= 100 && oldStress < 100)
        {
            acquiredTrauma = TriggerBreakingPoint(character, source);
        }

        return (actualGained, acquiredTrauma);
    }

    /// <summary>
    /// Adds Corruption to a character (cannot be resisted)
    /// v0.15: Now triggers threshold effects at 25/50/75/100
    /// </summary>
    /// <param name="character">The character gaining corruption</param>
    /// <param name="amount">Corruption amount (always applies in full)</param>
    /// <param name="source">What caused the corruption</param>
    /// <returns>Tuple of (actual corruption gained, list of thresholds crossed)</returns>
    public (int corruptionGained, List<int> thresholdsCrossed) AddCorruption(
        PlayerCharacter character,
        int amount,
        string source = "unknown")
    {
        if (amount <= 0) return (0, new List<int>());

        int oldCorruption = character.Corruption;
        character.Corruption = Math.Clamp(character.Corruption + amount, MinTraumaValue, MaxTraumaValue);
        int actualGained = character.Corruption - oldCorruption;

        var threshold = GetCorruptionThreshold(character);
        var oldThreshold = GetCorruptionThresholdForValue(oldCorruption);

        _log.Information("Corruption gained: Character={CharacterName}, Amount={Amount}, OldCorruption={Old}, NewCorruption={New}, Threshold={Threshold}, Source={Source}",
            character.Name, actualGained, oldCorruption, character.Corruption, threshold, source);

        // v0.15: Check for threshold crossings
        var thresholdsCrossed = new List<int>();

        if (oldCorruption < 25 && character.Corruption >= 25)
        {
            TriggerCorruptionThreshold25(character);
            thresholdsCrossed.Add(25);
        }

        if (oldCorruption < 50 && character.Corruption >= 50)
        {
            TriggerCorruptionThreshold50(character);
            thresholdsCrossed.Add(50);
        }

        if (oldCorruption < 75 && character.Corruption >= 75)
        {
            TriggerCorruptionThreshold75(character);
            thresholdsCrossed.Add(75);
        }

        if (oldCorruption < 100 && character.Corruption >= 100)
        {
            TriggerTerminalCorruption(character);
            thresholdsCrossed.Add(100);
        }

        // Log threshold transitions
        if (oldThreshold != threshold)
        {
            _log.Warning("Corruption threshold crossed: Character={CharacterName}, OldThreshold={OldThreshold}, NewThreshold={NewThreshold}",
                character.Name, oldThreshold, threshold);
        }

        return (actualGained, thresholdsCrossed);
    }

    /// <summary>
    /// Clears Psychic Stress to 0 (Sanctuary Rest)
    /// </summary>
    public void ClearStress(PlayerCharacter character)
    {
        int oldStress = character.PsychicStress;
        character.PsychicStress = 0;

        _log.Information("Stress cleared: Character={CharacterName}, OldStress={Old}, Recovered={Recovered}",
            character.Name, oldStress, oldStress);
    }

    /// <summary>
    /// Gets the current Stress threshold for UI display
    /// </summary>
    public StressThreshold GetStressThreshold(PlayerCharacter character)
    {
        return character.PsychicStress switch
        {
            >= 76 => StressThreshold.Critical,
            >= 51 => StressThreshold.Severe,
            >= 26 => StressThreshold.Strained,
            _ => StressThreshold.Safe
        };
    }

    /// <summary>
    /// Gets the current Corruption threshold for UI display
    /// </summary>
    public CorruptionThreshold GetCorruptionThreshold(PlayerCharacter character)
    {
        return character.Corruption switch
        {
            >= 81 => CorruptionThreshold.Extreme,
            >= 61 => CorruptionThreshold.High,
            >= 41 => CorruptionThreshold.Moderate,
            >= 21 => CorruptionThreshold.Low,
            _ => CorruptionThreshold.Minimal
        };
    }

    /// <summary>
    /// Gets the color code for stress display
    /// </summary>
    public ConsoleColor GetStressColor(StressThreshold threshold)
    {
        return threshold switch
        {
            StressThreshold.Critical => ConsoleColor.Red,
            StressThreshold.Severe => ConsoleColor.DarkYellow,
            StressThreshold.Strained => ConsoleColor.Yellow,
            _ => ConsoleColor.Green
        };
    }

    /// <summary>
    /// Gets the color code for corruption display
    /// </summary>
    public ConsoleColor GetCorruptionColor(CorruptionThreshold threshold)
    {
        return threshold switch
        {
            CorruptionThreshold.Extreme => ConsoleColor.Red,
            CorruptionThreshold.High => ConsoleColor.Red,
            CorruptionThreshold.Moderate => ConsoleColor.DarkYellow,
            CorruptionThreshold.Low => ConsoleColor.Yellow,
            _ => ConsoleColor.Green
        };
    }

    /// <summary>
    /// Gets the threshold status text for display
    /// </summary>
    public string GetStressThresholdText(StressThreshold threshold)
    {
        return threshold switch
        {
            StressThreshold.Critical => "Critical",
            StressThreshold.Severe => "Severe",
            StressThreshold.Strained => "Strained",
            _ => "Safe"
        };
    }

    /// <summary>
    /// Gets the threshold status text for display
    /// </summary>
    public string GetCorruptionThresholdText(CorruptionThreshold threshold)
    {
        return threshold switch
        {
            CorruptionThreshold.Extreme => "Extreme",
            CorruptionThreshold.High => "High",
            CorruptionThreshold.Moderate => "Moderate",
            CorruptionThreshold.Low => "Low",
            _ => "Minimal"
        };
    }

    /// <summary>
    /// Checks if character should receive a threshold warning
    /// </summary>
    public (bool shouldWarn, string warningMessage) CheckForThresholdWarning(PlayerCharacter character)
    {
        // Check Stress thresholds
        if (character.PsychicStress >= 75)
        {
            return (true, $"‼️ CRITICAL: You are near your Breaking Point!\nPsychic Stress: {character.PsychicStress}/100 (Critical)\nYou MUST rest before reaching 100 Stress.");
        }
        else if (character.PsychicStress >= 50)
        {
            return (true, $"⚠️ WARNING: Your mind is fracturing.\nPsychic Stress: {character.PsychicStress}/100 (Severe)\nSeek Sanctuary Rest soon.");
        }

        // Check Corruption thresholds
        if (character.Corruption >= 75)
        {
            return (true, $"‼️ CRITICAL: You are losing yourself to the Blight.\nCorruption: {character.Corruption}/100 (High)\nThere may be no coming back from this.");
        }
        else if (character.Corruption >= 50)
        {
            return (true, $"⚠️ WARNING: The Blight is rewriting your code.\nCorruption: {character.Corruption}/100 (Moderate)\nYou are permanently changed.");
        }

        return (false, string.Empty);
    }

    /// <summary>
    /// Generates a meter bar for visual display
    /// </summary>
    public string GenerateMeterBar(int current, int max, int barLength = 10)
    {
        int filledBlocks = (int)Math.Round((double)current / max * barLength);
        filledBlocks = Math.Clamp(filledBlocks, 0, barLength);

        string filled = new string('█', filledBlocks);
        string empty = new string('░', barLength - filledBlocks);

        return $"[{filled}{empty}]";
    }

    /// <summary>
    /// Helper method to get stress threshold for a specific value
    /// </summary>
    private StressThreshold GetStressThresholdForValue(int stress)
    {
        return stress switch
        {
            >= 76 => StressThreshold.Critical,
            >= 51 => StressThreshold.Severe,
            >= 26 => StressThreshold.Strained,
            _ => StressThreshold.Safe
        };
    }

    /// <summary>
    /// Helper method to get corruption threshold for a specific value
    /// </summary>
    private CorruptionThreshold GetCorruptionThresholdForValue(int corruption)
    {
        return corruption switch
        {
            >= 81 => CorruptionThreshold.Extreme,
            >= 61 => CorruptionThreshold.High,
            >= 41 => CorruptionThreshold.Moderate,
            >= 21 => CorruptionThreshold.Low,
            _ => CorruptionThreshold.Minimal
        };
    }

    // v0.15: Breaking Point Mechanics

    /// <summary>
    /// Triggers a Breaking Point when stress reaches 100
    /// </summary>
    /// <param name="character">The character suffering the Breaking Point</param>
    /// <param name="source">What caused the Breaking Point</param>
    /// <returns>The trauma acquired</returns>
    private Trauma TriggerBreakingPoint(PlayerCharacter character, string source)
    {
        _log.Warning("Breaking Point triggered: Character={CharacterName}, Source={Source}, Corruption={Corruption}",
            character.Name, source, character.Corruption);

        // Select appropriate trauma based on source and context
        var trauma = TraumaLibrary.SelectTraumaForSource(source, character.Corruption, _rng);

        // Don't acquire duplicates
        if (character.HasTrauma(trauma.TraumaId))
        {
            _log.Information("Character already has trauma {TraumaId}, not acquiring duplicate", trauma.TraumaId);
            // Still reset stress but no new trauma
            character.PsychicStress = BreakingPointStress;
            return trauma; // Return existing trauma for narrative purposes
        }

        // Set acquisition details
        trauma.AcquisitionSource = source;
        trauma.AcquiredAt = DateTime.Now;
        trauma.DaysSinceManagement = 0;
        trauma.IsManagedThisSession = false;

        // Acquire trauma
        character.Traumas.Add(trauma);

        // Apply immediate effects
        foreach (var effect in trauma.Effects)
        {
            effect.Apply(character);
        }

        // Reset stress to 60 (not 0 - character is still rattled)
        character.PsychicStress = BreakingPointStress;

        _log.Warning("Breaking Point resolved: Character={CharacterName}, AcquiredTrauma={TraumaId}, StressReset={Stress}",
            character.Name, trauma.TraumaId, BreakingPointStress);

        return trauma;
    }

    /// <summary>
    /// Gets the Breaking Point narrative text for display
    /// </summary>
    public string GetBreakingPointNarrative(Trauma trauma, string source)
    {
        var narrative = new System.Text.StringBuilder();

        narrative.AppendLine();
        narrative.AppendLine("═══════════════════════════════════════════════════════");
        narrative.AppendLine("                  === BREAKING POINT ===");
        narrative.AppendLine("═══════════════════════════════════════════════════════");
        narrative.AppendLine();
        narrative.AppendLine("The weight of it all crashes down.");
        narrative.AppendLine();
        narrative.AppendLine("The Silence. The decay. The endless rust.");
        narrative.AppendLine("The machines that won't stay dead.");
        narrative.AppendLine();
        narrative.AppendLine("Your hands shake. Your breath comes in gasps.");
        narrative.AppendLine("The walls feel too close.");
        narrative.AppendLine();
        narrative.AppendLine("Something inside you... breaks.");
        narrative.AppendLine();
        narrative.AppendLine($"▶ You have acquired: {trauma.Name}");
        narrative.AppendLine();
        narrative.AppendLine($"  \"{trauma.Description}\"");
        narrative.AppendLine();

        // Show effects
        narrative.AppendLine("  Effects:");
        foreach (var effect in trauma.Effects)
        {
            narrative.AppendLine($"    - {effect.GetDescription()}");
        }

        narrative.AppendLine();
        narrative.AppendLine($"  Source: {source}");
        narrative.AppendLine();
        narrative.AppendLine("  This trauma is permanent. It cannot be removed,");
        narrative.AppendLine("  only managed through rest, therapy, or other means.");
        narrative.AppendLine();
        narrative.AppendLine($"  Your Stress has been reset to {BreakingPointStress}/100.");
        narrative.AppendLine("═══════════════════════════════════════════════════════");

        return narrative.ToString();
    }

    /// <summary>
    /// Applies passive stress from environmental traumas
    /// </summary>
    public int ApplyPassiveTraumaStress(PlayerCharacter character, string condition)
    {
        int totalStress = character.GetTraumaPassiveStress(condition);

        if (totalStress > 0)
        {
            var (gained, trauma) = AddStress(character, totalStress, $"trauma_passive_{condition}");
            return gained;
        }

        return 0;
    }

    // v0.15: Corruption Threshold Effects

    /// <summary>
    /// Corruption Threshold 25: Minor corruption effects
    /// </summary>
    private void TriggerCorruptionThreshold25(PlayerCharacter character)
    {
        _log.Warning("Corruption Threshold 25 crossed: Character={CharacterName}", character.Name);

        // Effect: +1 Tech (understanding machines), -1 to Social checks
        // Note: These would be applied dynamically when checks are made
        // The actual implementation depends on how the game calculates skill checks
    }

    /// <summary>
    /// Corruption Threshold 50: Moderate corruption effects
    /// </summary>
    private void TriggerCorruptionThreshold50(PlayerCharacter character)
    {
        _log.Warning("Corruption Threshold 50 crossed: Character={CharacterName}", character.Name);

        // Effect: Cannot gain reputation with human factions
        // Effect: +2 Tech, -2 Social
        // Note: These flags would need to be checked in reputation/dialogue systems
    }

    /// <summary>
    /// Corruption Threshold 75: Severe corruption effects
    /// </summary>
    private void TriggerCorruptionThreshold75(PlayerCharacter character)
    {
        _log.Warning("Corruption Threshold 75 crossed: Character={CharacterName}", character.Name);

        // Force acquisition of [MACHINE AFFINITY] trauma if not present
        if (!character.HasTrauma("machine_affinity"))
        {
            var machineAffinity = TraumaLibrary.GetTrauma("machine_affinity");
            if (machineAffinity != null)
            {
                machineAffinity.AcquisitionSource = "corruption_threshold_75";
                machineAffinity.AcquiredAt = DateTime.Now;
                character.Traumas.Add(machineAffinity);

                // Apply immediate corruption from Machine Affinity
                foreach (var effect in machineAffinity.Effects)
                {
                    effect.Apply(character);
                }

                _log.Warning("Machine Affinity trauma acquired at Corruption 75: Character={CharacterName}",
                    character.Name);
            }
        }

        // Effect: NPCs react with fear/distrust
        // This would need to be integrated with NPC dialogue/interaction systems
    }

    /// <summary>
    /// Corruption Threshold 100: Terminal Corruption (game over state)
    /// </summary>
    private void TriggerTerminalCorruption(PlayerCharacter character)
    {
        _log.Error("TERMINAL CORRUPTION: Character={CharacterName}", character.Name);

        // This is effectively a game over state
        // The character can continue but is fundamentally changed
        // Implementation note: The game loop should check for this condition
    }

    /// <summary>
    /// Gets corruption threshold narrative for display
    /// </summary>
    public string GetCorruptionThresholdNarrative(int threshold, PlayerCharacter character)
    {
        var narrative = new System.Text.StringBuilder();

        narrative.AppendLine();
        narrative.AppendLine("═══════════════════════════════════════════════════════");

        switch (threshold)
        {
            case 25:
                narrative.AppendLine("           === CORRUPTION THRESHOLD: 25 ===");
                narrative.AppendLine("═══════════════════════════════════════════════════════");
                narrative.AppendLine();
                narrative.AppendLine("You feel... different.");
                narrative.AppendLine("The machines' logic makes more sense than it should.");
                narrative.AppendLine();
                narrative.AppendLine("Effects:");
                narrative.AppendLine("  • +1 to Tech checks");
                narrative.AppendLine("  • -1 to Social checks");
                break;

            case 50:
                narrative.AppendLine("           === CORRUPTION THRESHOLD: 50 ===");
                narrative.AppendLine("═══════════════════════════════════════════════════════");
                narrative.AppendLine();
                narrative.AppendLine("Human thought patterns feel... inefficient. Flawed.");
                narrative.AppendLine("You prefer the company of machines.");
                narrative.AppendLine();
                narrative.AppendLine("Effects:");
                narrative.AppendLine("  • +2 to Tech checks");
                narrative.AppendLine("  • -2 to Social checks");
                narrative.AppendLine("  • Cannot gain reputation with human factions");
                break;

            case 75:
                narrative.AppendLine("           === CORRUPTION THRESHOLD: 75 ===");
                narrative.AppendLine("═══════════════════════════════════════════════════════");
                narrative.AppendLine();
                narrative.AppendLine("Am I still human? Should I be?");
                narrative.AppendLine("The Jötun-Readers were right. Flesh is inefficient.");
                narrative.AppendLine();
                if (!character.HasTrauma("machine_affinity"))
                {
                    narrative.AppendLine("You have acquired: [MACHINE AFFINITY]");
                    narrative.AppendLine();
                }
                narrative.AppendLine("Effects:");
                narrative.AppendLine("  • NPCs react with fear and distrust");
                narrative.AppendLine("  • Further psychological deterioration");
                break;

            case 100:
                narrative.AppendLine("           === TERMINAL CORRUPTION ===");
                narrative.AppendLine("═══════════════════════════════════════════════════════");
                narrative.AppendLine();
                narrative.AppendLine("DIAGNOSTIC COMPLETE");
                narrative.AppendLine("SUBJECT: Compromised");
                narrative.AppendLine("HUMANITY INDEX: 0.00");
                narrative.AppendLine("RECOMMENDATION: Decommission");
                narrative.AppendLine();
                narrative.AppendLine("You are no longer yourself.");
                narrative.AppendLine("Perhaps you never were.");
                narrative.AppendLine();
                narrative.AppendLine("═══ GAME OVER: TERMINAL CORRUPTION ═══");
                break;
        }

        narrative.AppendLine("═══════════════════════════════════════════════════════");

        return narrative.ToString();
    }
}
