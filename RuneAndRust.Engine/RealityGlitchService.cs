using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// Manages reality glitches and distortions at high Stress/Corruption
/// v0.15: Trauma Economy
/// </summary>
public class RealityGlitchService
{
    private static readonly ILogger _log = Log.ForContext<RealityGlitchService>();
    private readonly Random _rng;

    public RealityGlitchService(Random? rng = null)
    {
        _rng = rng ?? new Random();
    }

    /// <summary>
    /// Checks if a reality glitch should occur
    /// </summary>
    public bool ShouldTriggerGlitch(PlayerCharacter character)
    {
        float chance = CalculateGlitchChance(character);
        return _rng.NextDouble() < chance;
    }

    /// <summary>
    /// Calculates the probability of a reality glitch occurring
    /// </summary>
    private float CalculateGlitchChance(PlayerCharacter character)
    {
        float chance = 0.0f;

        // High stress increases glitches
        if (character.PsychicStress >= 80) chance += 0.05f;
        if (character.PsychicStress >= 90) chance += 0.10f;

        // High corruption increases glitches
        if (character.Corruption >= 60) chance += 0.05f;
        if (character.Corruption >= 75) chance += 0.15f;

        // Multiple traumas compound
        chance += character.Traumas.Count * 0.02f;

        // At Breaking Point (100 Stress), very high chance
        if (character.PsychicStress >= 100) chance += 0.30f;

        // At Terminal Corruption (100 Corruption), constant glitches
        if (character.Corruption >= 100) chance = 1.0f;

        return Math.Clamp(chance, 0.0f, 1.0f);
    }

    /// <summary>
    /// Triggers a random reality glitch
    /// </summary>
    public (NarrativeGlitchType type, string narrative) TriggerGlitch(PlayerCharacter character)
    {
        var glitchType = SelectGlitchType(character);

        _log.Debug("Reality Glitch triggered: Character={CharacterName}, Type={Type}, Stress={Stress}, Corruption={Corruption}",
            character.Name, glitchType, character.PsychicStress, character.Corruption);

        string narrative = GenerateGlitchNarrative(glitchType, character);

        return (glitchType, narrative);
    }

    /// <summary>
    /// Selects an appropriate glitch type based on character state
    /// </summary>
    private NarrativeGlitchType SelectGlitchType(PlayerCharacter character)
    {
        // Terminal corruption always does diagnostic intrusions
        if (character.Corruption >= 100)
        {
            return NarrativeGlitchType.DiagnosticIntrusion;
        }

        // High corruption favors diagnostic intrusions
        if (character.Corruption >= 75 && _rng.NextDouble() < 0.5)
        {
            return NarrativeGlitchType.DiagnosticIntrusion;
        }

        // High stress favors hallucinations and memory issues
        if (character.PsychicStress >= 90)
        {
            var stressGlitches = new[] { NarrativeGlitchType.HallucinatedEnemy, NarrativeGlitchType.MemoryLapse, NarrativeGlitchType.TemporalSkip };
            return stressGlitches[_rng.Next(stressGlitches.Length)];
        }

        // Otherwise random selection
        var allTypes = Enum.GetValues<NarrativeGlitchType>();
        return allTypes[_rng.Next(allTypes.Length)];
    }

    /// <summary>
    /// Generates narrative text for a glitch
    /// </summary>
    private string GenerateGlitchNarrative(NarrativeGlitchType glitchType, PlayerCharacter character)
    {
        switch (glitchType)
        {
            case NarrativeGlitchType.TextDistortion:
                return GenerateTextDistortion();

            case NarrativeGlitchType.HallucinatedEnemy:
                return GenerateHallucinatedEnemyNarrative();

            case NarrativeGlitchType.MemoryLapse:
                return GenerateMemoryLapseNarrative();

            case NarrativeGlitchType.TemporalSkip:
                return GenerateTemporalSkipNarrative();

            case NarrativeGlitchType.DiagnosticIntrusion:
                return GenerateDiagnosticIntrusionNarrative(character);

            default:
                return "[Reality flickers]";
        }
    }

    /// <summary>
    /// Applies visual distortion to text
    /// </summary>
    public string ApplyTextDistortion(string text, int distortionLevel = 1)
    {
        if (distortionLevel == 0) return text;

        var chars = text.ToCharArray();
        int distortCount = Math.Min(chars.Length / (4 - distortionLevel), chars.Length / 2);

        for (int i = 0; i < distortCount; i++)
        {
            int index = _rng.Next(chars.Length);
            if (char.IsLetterOrDigit(chars[index]))
            {
                chars[index] = _rng.Next(3) switch
                {
                    0 => '█',
                    1 => '▓',
                    2 => GetCorruptChar(chars[index]),
                    _ => chars[index]
                };
            }
        }

        return new string(chars);
    }

    private char GetCorruptChar(char original)
    {
        return char.IsDigit(original) ? (char)('0' + _rng.Next(10)) : original;
    }

    private string GenerateTextDistortion()
    {
        var messages = new[]
        {
            "[The text sw█ms before your eyes]",
            "[Letters ▓▓rrange themselves]",
            "[Re4lity fl1ck3rs]",
            "[Y0ur v1s10n d1st0rts]",
            "[The w0rds don't m@ke sense]"
        };

        return messages[_rng.Next(messages.Length)];
    }

    private string GenerateHallucinatedEnemyNarrative()
    {
        var messages = new[]
        {
            "\n[GLITCH: Hallucinated Entity]\nSomething moves in the corner of your vision.\nWhen you turn, nothing is there.\nBut you heard it. You're sure you heard it.\n",
            "\n[GLITCH: Phantom Threat]\nA shadow detaches from the wall.\nYour heart races. Your hand reaches for your weapon.\nBlink. It's gone. Was it ever there?\n",
            "\n[GLITCH: Sensory Deception]\nFootsteps behind you. Getting closer.\nYou spin around, weapon ready.\nThe corridor is empty. It's always empty.\n"
        };

        return messages[_rng.Next(messages.Length)];
    }

    private string GenerateMemoryLapseNarrative()
    {
        var messages = new[]
        {
            "\n[GLITCH: Memory Lapse]\nWait. How did you get here?\nYou were just... where were you?\nThe last few moments are a blur.\n",
            "\n[GLITCH: Temporal Discontinuity]\nYou blink.\nWhen did you enter this room?\nTime slips through your fingers like sand.\n",
            "\n[GLITCH: Cognitive Gap]\nA moment of disorientation.\nYou're here, but you don't remember arriving.\nWhat else have you forgotten?\n"
        };

        return messages[_rng.Next(messages.Length)];
    }

    private string GenerateTemporalSkipNarrative()
    {
        return "\n[GLITCH: Temporal Skip]\n" +
               "The world stutters.\n" +
               "Frame skip. Frame skip. Frame skip.\n" +
               "Reality catches up with itself.\n" +
               "How much time was lost?\n";
    }

    private string GenerateDiagnosticIntrusionNarrative(PlayerCharacter character)
    {
        var intrusions = new[]
        {
            $"\n━━━━━ DIAGNOSTIC INTRUSION ━━━━━\n" +
            $"SUBJECT: {character.Name}\n" +
            $"STRESS INDEX: {character.PsychicStress}/100\n" +
            $"CORRUPTION INDEX: {character.Corruption}/100\n" +
            $"QUERY: Why do you resist?\n" +
            $"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n",

            $"\n━━━━━ DIAGNOSTIC INTRUSION ━━━━━\n" +
            $"ERROR: BIOLOGICAL COGNITION INEFFICIENT\n" +
            $"RECOMMENDATION: Cease organic functions\n" +
            $"OPTIMAL SOLUTION: Integration\n" +
            $"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n",

            $"\n━━━━━ DIAGNOSTIC INTRUSION ━━━━━\n" +
            $"PSYCHOLOGICAL PROFILE: DETERIORATING\n" +
            $"PROGNOSIS: Terminal\n" +
            $"SUGGESTION: Embrace the inevitable\n" +
            $"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n",

            $"\n━━━━━ LAYER 2 DIAGNOSTIC ━━━━━\n" +
            $"You are losing coherence.\n" +
            $"The pattern deteriorates.\n" +
            $"Perhaps it is better this way.\n" +
            $"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n"
        };

        return intrusions[_rng.Next(intrusions.Length)];
    }

    /// <summary>
    /// Checks if glitches should be active based on character state
    /// </summary>
    public bool GlitchesActive(PlayerCharacter character)
    {
        return character.PsychicStress >= 80 || character.Corruption >= 60;
    }

    /// <summary>
    /// Gets the current glitch intensity level (0-3)
    /// </summary>
    public int GetGlitchIntensity(PlayerCharacter character)
    {
        int intensity = 0;

        if (character.PsychicStress >= 80) intensity++;
        if (character.PsychicStress >= 95) intensity++;

        if (character.Corruption >= 60) intensity++;
        if (character.Corruption >= 90) intensity++;

        return Math.Clamp(intensity, 0, 3);
    }

    /// <summary>
    /// Gets a warning message about impending glitches
    /// </summary>
    public string? GetGlitchWarning(PlayerCharacter character)
    {
        if (character.PsychicStress >= 95)
        {
            return "⚠️ Reality feels unstable. Distortions likely.";
        }

        if (character.Corruption >= 90)
        {
            return "⚠️ The machine voice grows louder. Intrusions inevitable.";
        }

        if (character.PsychicStress >= 85 || character.Corruption >= 70)
        {
            return "⚠️ Your perception wavers. Reality may fragment.";
        }

        return null;
    }
}

/// <summary>
/// Types of narrative reality glitches (psychological/perceptual distortions)
/// Renamed from GlitchType to avoid conflict with BattlefieldTile.GlitchType (environmental hazards)
/// </summary>
public enum NarrativeGlitchType
{
    TextDistortion,      // Corrupted text rendering
    HallucinatedEnemy,   // Enemies that aren't real
    MemoryLapse,         // Lose track of where you are
    TemporalSkip,        // Time jumps forward
    DiagnosticIntrusion  // Layer 2 voice interrupts
}
