using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// Manages the Trauma Economy system - Psychic Stress and Runic Blight Corruption
/// </summary>
public class TraumaEconomyService
{
    private static readonly ILogger _log = Log.ForContext<TraumaEconomyService>();
    private const int MaxTraumaValue = 100;
    private const int MinTraumaValue = 0;

    /// <summary>
    /// Adds Psychic Stress to a character with optional WILL-based Resolve Check reduction
    /// </summary>
    /// <param name="character">The character gaining stress</param>
    /// <param name="baseAmount">Base stress amount before resistance</param>
    /// <param name="allowResolveCheck">If true, WILL can reduce the stress gain</param>
    /// <param name="resolveSuccesses">Number of successes from WILL Resolve Check (if applicable)</param>
    /// <returns>Actual stress gained after resistance</returns>
    public int AddStress(PlayerCharacter character, int baseAmount, bool allowResolveCheck = false, int resolveSuccesses = 0)
    {
        if (baseAmount <= 0) return 0;

        int actualAmount = baseAmount;

        // Apply Resolve Check reduction if allowed
        if (allowResolveCheck && resolveSuccesses > 0)
        {
            actualAmount = Math.Max(0, baseAmount - resolveSuccesses);
            _log.Debug("Stress reduced by Resolve Check: Character={CharacterName}, BaseAmount={Base}, Successes={Successes}, ReducedAmount={Reduced}",
                character.Name, baseAmount, resolveSuccesses, actualAmount);
        }

        // Apply stress gain
        int oldStress = character.PsychicStress;
        character.PsychicStress = Math.Clamp(character.PsychicStress + actualAmount, MinTraumaValue, MaxTraumaValue);
        int actualGained = character.PsychicStress - oldStress;

        var threshold = GetStressThreshold(character);
        var oldThreshold = GetStressThresholdForValue(oldStress);

        _log.Information("Stress gained: Character={CharacterName}, Amount={Amount}, OldStress={Old}, NewStress={New}, Threshold={Threshold}",
            character.Name, actualGained, oldStress, character.PsychicStress, threshold);

        // Log threshold transitions
        if (oldThreshold != threshold)
        {
            _log.Warning("Stress threshold crossed: Character={CharacterName}, OldThreshold={OldThreshold}, NewThreshold={NewThreshold}",
                character.Name, oldThreshold, threshold);
        }

        return actualGained;
    }

    /// <summary>
    /// Adds Corruption to a character (cannot be resisted)
    /// </summary>
    /// <param name="character">The character gaining corruption</param>
    /// <param name="amount">Corruption amount (always applies in full)</param>
    /// <returns>Actual corruption gained</returns>
    public int AddCorruption(PlayerCharacter character, int amount)
    {
        if (amount <= 0) return 0;

        int oldCorruption = character.Corruption;
        character.Corruption = Math.Clamp(character.Corruption + amount, MinTraumaValue, MaxTraumaValue);
        int actualGained = character.Corruption - oldCorruption;

        var threshold = GetCorruptionThreshold(character);
        var oldThreshold = GetCorruptionThresholdForValue(oldCorruption);

        _log.Information("Corruption gained: Character={CharacterName}, Amount={Amount}, OldCorruption={Old}, NewCorruption={New}, Threshold={Threshold}",
            character.Name, actualGained, oldCorruption, character.Corruption, threshold);

        // Log threshold transitions
        if (oldThreshold != threshold)
        {
            _log.Warning("Corruption threshold crossed: Character={CharacterName}, OldThreshold={OldThreshold}, NewThreshold={NewThreshold}",
                character.Name, oldThreshold, threshold);
        }

        return actualGained;
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
}
