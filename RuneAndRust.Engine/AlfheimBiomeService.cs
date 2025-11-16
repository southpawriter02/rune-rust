using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.31.2: Orchestration service for Alfheim biome.
/// Coordinates generation, hazards, enemies, and Aetheric energy mechanics.
///
/// Responsibilities:
/// - Apply [Runic Instability] ambient condition (Wild Magic Surges)
/// - Process [Psychic Resonance] high-intensity stress (ground zero of Great Silence)
/// - Integrate Reality Tear hazards
/// - Manage Aether Pool amplification for Mystics
/// - Coordinate with RunicInstabilityService and RealityTearService
/// </summary>
public class AlfheimBiomeService
{
    private static readonly ILogger _log = Log.ForContext<AlfheimBiomeService>();

    private readonly RunicInstabilityService _runicInstabilityService;
    private readonly RealityTearService _realityTearService;
    private readonly DiceService _diceService;

    /// <summary>
    /// v0.31.2 canonical: Psychic Stress per combat encounter in Alfheim (high-intensity)
    /// Double Helheim's +5, as Alfheim is ground zero of the Great Silence
    /// </summary>
    private const int STRESS_PER_ENCOUNTER = 10;

    /// <summary>
    /// v0.31.2 canonical: Psychic Stress per turn in combat (passive accumulation)
    /// </summary>
    private const int STRESS_PER_TURN = 2;

    public AlfheimBiomeService(
        RunicInstabilityService? runicInstabilityService = null,
        RealityTearService? realityTearService = null,
        DiceService? diceService = null)
    {
        _runicInstabilityService = runicInstabilityService ?? new RunicInstabilityService();
        _realityTearService = realityTearService ?? new RealityTearService();
        _diceService = diceService ?? new DiceService();

        _log.Information("AlfheimBiomeService initialized");
    }

    #region Combat Initialization

    /// <summary>
    /// Initialize combat in Alfheim biome.
    /// Applies [Runic Instability] and [Psychic Resonance] to all combatants.
    /// </summary>
    public void InitializeCombat(List<PlayerCharacter> party)
    {
        _log.Information("Initializing Alfheim combat for party of {Count}", party.Count);

        // Apply Aether Pool amplification to Mystics
        foreach (var character in party)
        {
            if (character.Archetype == "Mystic")
            {
                _runicInstabilityService.ApplyAetherPoolAmplification(character);
            }
        }

        // Apply initial Psychic Resonance stress
        ApplyPsychicResonanceEncounter(party);

        _log.Information("Alfheim combat initialized: [Runic Instability] and [Psychic Resonance] active");
    }

    /// <summary>
    /// Clean up combat effects when leaving Alfheim.
    /// Removes Aether Pool amplification.
    /// </summary>
    public void CleanupCombat(List<PlayerCharacter> party)
    {
        _log.Information("Cleaning up Alfheim combat effects for party of {Count}", party.Count);

        foreach (var character in party)
        {
            if (character.Archetype == "Mystic")
            {
                _runicInstabilityService.RemoveAetherPoolAmplification(character);
            }
        }
    }

    #endregion

    #region Psychic Resonance (High-Intensity)

    /// <summary>
    /// Apply high-intensity Psychic Resonance at combat start.
    /// Alfheim is ground zero - +10 Psychic Stress per encounter.
    /// </summary>
    public List<PsychicResonanceResult> ApplyPsychicResonanceEncounter(List<PlayerCharacter> party)
    {
        _log.Information(
            "Applying high-intensity [Psychic Resonance] to party of {Count} (ground zero of Great Silence)",
            party.Count);

        var results = new List<PsychicResonanceResult>();

        foreach (var character in party)
        {
            var result = new PsychicResonanceResult
            {
                CharacterName = character.Name,
                PreviousStress = character.PsychicStress
            };

            character.PsychicStress += STRESS_PER_ENCOUNTER;

            result.StressGained = STRESS_PER_ENCOUNTER;
            result.CurrentStress = character.PsychicStress;
            result.Message = $"{character.Name} gains +{STRESS_PER_ENCOUNTER} Psychic Stress from [Psychic Resonance]\n" +
                           $"   Ground zero of the Great Silence - seven billion silenced minds screaming at once\n" +
                           $"   Total Psychic Stress: {character.PsychicStress}";

            results.Add(result);

            _log.Information(
                "{Character} gains +{Stress} Psychic Stress from Alfheim [Psychic Resonance] ({Previous} → {Current})",
                character.Name, STRESS_PER_ENCOUNTER, result.PreviousStress, result.CurrentStress);
        }

        return results;
    }

    /// <summary>
    /// Apply per-turn Psychic Resonance stress.
    /// Called each combat turn for continuous stress accumulation.
    /// </summary>
    public List<PsychicResonanceResult> ProcessTurnStress(List<PlayerCharacter> party, int currentTurn)
    {
        _log.Debug("Processing Alfheim turn {Turn} Psychic Resonance stress", currentTurn);

        var results = new List<PsychicResonanceResult>();

        foreach (var character in party)
        {
            var result = new PsychicResonanceResult
            {
                CharacterName = character.Name,
                PreviousStress = character.PsychicStress
            };

            character.PsychicStress += STRESS_PER_TURN;

            result.StressGained = STRESS_PER_TURN;
            result.CurrentStress = character.PsychicStress;
            result.Message = $"+{STRESS_PER_TURN} Psychic Stress (Alfheim exposure, turn {currentTurn})";

            results.Add(result);
        }

        _log.Debug(
            "Applied +{Stress} Psychic Stress to {Count} party members (turn {Turn})",
            STRESS_PER_TURN, party.Count, currentTurn);

        return results;
    }

    #endregion

    #region Runic Instability Integration

    /// <summary>
    /// Check for Wild Magic Surge when Mystic uses ability in Alfheim.
    /// Returns surge result if triggered, null otherwise.
    /// </summary>
    public WildMagicSurgeResult? CheckWildMagicSurge(
        PlayerCharacter caster,
        string abilityName)
    {
        const int ALFHEIM_BIOME_ID = 6;
        return _runicInstabilityService.TryTriggerWildMagicSurge(caster, abilityName, ALFHEIM_BIOME_ID);
    }

    /// <summary>
    /// Apply Wild Magic Surge modification to ability damage.
    /// </summary>
    public int ApplySurgeToDamage(int baseDamage, WildMagicSurgeResult? surge)
    {
        if (surge == null)
        {
            return baseDamage;
        }

        return _runicInstabilityService.ApplySurgeToDamage(baseDamage, surge);
    }

    /// <summary>
    /// Apply Wild Magic Surge modification to ability range.
    /// </summary>
    public int ApplySurgeToRange(int baseRange, WildMagicSurgeResult? surge)
    {
        if (surge == null)
        {
            return baseRange;
        }

        return _runicInstabilityService.ApplySurgeToRange(baseRange, surge);
    }

    /// <summary>
    /// Apply Wild Magic Surge modification to ability target count.
    /// </summary>
    public int ApplySurgeToTargets(int baseTargets, WildMagicSurgeResult? surge)
    {
        if (surge == null)
        {
            return baseTargets;
        }

        return _runicInstabilityService.ApplySurgeToTargets(baseTargets, surge);
    }

    /// <summary>
    /// Apply Wild Magic Surge modification to ability duration.
    /// </summary>
    public int ApplySurgeToDuration(int baseDuration, WildMagicSurgeResult? surge)
    {
        if (surge == null)
        {
            return baseDuration;
        }

        return _runicInstabilityService.ApplySurgeToDuration(baseDuration, surge);
    }

    #endregion

    #region Reality Tear Integration

    /// <summary>
    /// Process character entering Reality Tear tile.
    /// </summary>
    public RealityTearResult ProcessRealityTearEncounter(
        PlayerCharacter character,
        GridPosition tearPosition,
        BattlefieldGrid grid)
    {
        return _realityTearService.ProcessRealityTearEncounter(character, tearPosition, grid);
    }

    /// <summary>
    /// Process enemy entering Reality Tear tile.
    /// </summary>
    public RealityTearResult ProcessRealityTearEncounter(
        Enemy enemy,
        GridPosition tearPosition,
        BattlefieldGrid grid)
    {
        return _realityTearService.ProcessRealityTearEncounter(enemy, tearPosition, grid);
    }

    #endregion

    #region Party Preparedness

    /// <summary>
    /// Check if party is prepared for Alfheim.
    /// Logs warnings if WILL or Energy Resistance is insufficient.
    /// </summary>
    public AlfheimPreparednessReport CheckPartyPreparedness(List<PlayerCharacter> party)
    {
        _log.Information("Checking party preparedness for Alfheim ({Count} characters)", party.Count);

        var report = new AlfheimPreparednessReport
        {
            Characters = new List<CharacterAlfheimPreparedness>()
        };

        foreach (var character in party)
        {
            // Check WILL attribute (for Psychic Resonance resistance)
            var charPrep = new CharacterAlfheimPreparedness
            {
                CharacterName = character.Name,
                Will = character.Attributes.Will,
                IsMystic = character.Archetype == "Mystic",
                RecommendedWill = 14 // High WILL recommended for extreme stress
            };

            if (character.Attributes.Will < 12)
            {
                charPrep.WarningLevel = "Critical";
                charPrep.WarningMessage = $"{character.Name} has CRITICAL lack of WILL ({character.Attributes.Will}). " +
                                         $"Expect severe Psychic Stress from [Psychic Resonance] (ground zero of Great Silence).";
                _log.Warning("{Character}: CRITICAL WILL ({Will})", character.Name, character.Attributes.Will);
            }
            else if (character.Attributes.Will < 14)
            {
                charPrep.WarningLevel = "Warning";
                charPrep.WarningMessage = $"{character.Name} has moderate WILL ({character.Attributes.Will}). " +
                                         $"Psychic Resonance will accumulate stress rapidly (+10 per encounter, +2 per turn).";
                _log.Warning("{Character}: Low WILL ({Will})", character.Name, character.Attributes.Will);
            }
            else
            {
                charPrep.WarningLevel = "Adequate";
                charPrep.WarningMessage = $"{character.Name} is prepared for Alfheim ({character.Attributes.Will} WILL).";
                _log.Information("{Character}: Adequate WILL ({Will})", character.Name, character.Attributes.Will);
            }

            // Special warning for Mystics about Wild Magic Surges
            if (character.Archetype == "Mystic")
            {
                charPrep.MysticWarning = "As a Mystic, all abilities have 25% chance to trigger Wild Magic Surge. " +
                                        "Surges modify abilities randomly (±50% damage/range/targets/duration) and generate +5 Stress each. " +
                                        "However, you gain +10% Aether Pool capacity in Alfheim.";
                _log.Information("{Character} is Mystic: Wild Magic Surges active, +10% Aether Pool",
                    character.Name);
            }

            report.Characters.Add(charPrep);
        }

        report.AverageWill = report.Characters.Average(c => c.Will);
        report.PartyIsAdequatelyPrepared = report.AverageWill >= 12;

        _log.Information("Party preparedness check complete: Avg WILL={Avg}, Adequate={IsAdequate}",
            report.AverageWill, report.PartyIsAdequatelyPrepared);

        return report;
    }

    #endregion
}

#region Data Transfer Objects

public class PsychicResonanceResult
{
    public string CharacterName { get; set; } = string.Empty;
    public int PreviousStress { get; set; }
    public int StressGained { get; set; }
    public int CurrentStress { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class AlfheimPreparednessReport
{
    public List<CharacterAlfheimPreparedness> Characters { get; set; } = new();
    public double AverageWill { get; set; }
    public bool PartyIsAdequatelyPrepared { get; set; }
}

public class CharacterAlfheimPreparedness
{
    public string CharacterName { get; set; } = string.Empty;
    public int Will { get; set; }
    public bool IsMystic { get; set; }
    public int RecommendedWill { get; set; }
    public string WarningLevel { get; set; } = string.Empty;
    public string WarningMessage { get; set; } = string.Empty;
    public string? MysticWarning { get; set; }
}

#endregion
