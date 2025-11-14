using RuneAndRust.Core;

namespace RuneAndRust.Engine;

/// <summary>
/// Handles WILL-based Resolve Checks for resisting mental/spiritual threats
/// v0.20.2: Integrated with cover system for metaphysical protection
/// </summary>
public class ResolveCheckService
{
    private readonly DiceService _diceService;
    private readonly CoverService _coverService; // [v0.20.2]

    public ResolveCheckService(DiceService diceService)
    {
        _diceService = diceService;
        _coverService = new CoverService(); // [v0.20.2]
    }

    /// <summary>
    /// Rolls a WILL-based Resolve Check against a DC (binary pass/fail)
    /// Used for Forlorn enemy auras
    /// v0.20.2: Now considers metaphysical cover bonuses from grid position
    /// </summary>
    /// <param name="character">Character making the check</param>
    /// <param name="dc">Difficulty Class (target number of successes)</param>
    /// <param name="grid">Optional battlefield grid for cover bonuses</param>
    /// <param name="attacker">Optional attacker for cover calculation</param>
    /// <returns>Tuple of (success, successes rolled, roll details)</returns>
    public (bool success, int successes, string rollDetails) RollResolveCheck(PlayerCharacter character, int dc, BattlefieldGrid? grid = null, object? attacker = null)
    {
        int willValue = character.GetAttributeValue("will");

        // [v0.20.2] Apply metaphysical cover bonus
        int coverBonus = 0;
        if (grid != null)
        {
            var bonus = _coverService.CalculateCoverBonus(character, attacker, AttackType.Psychic, grid);
            coverBonus = bonus.ResolveBonus;
            willValue += coverBonus;
        }

        var result = _diceService.Roll(willValue);
        bool success = result.Successes >= dc;

        string coverText = coverBonus > 0 ? $" (+{coverBonus} from cover)" : "";
        string rollDetails = $"WILL Resolve Check (DC {dc}): Rolled {willValue} dice{coverText} → {FormatDiceRoll(result)} → {result.Successes} successes";

        return (success, result.Successes, rollDetails);
    }

    /// <summary>
    /// Calculates stress reduction from environmental sources based on WILL successes
    /// Used for Psychic Resonance zones
    /// </summary>
    /// <param name="successes">Number of successes from WILL roll</param>
    /// <param name="baseStress">Base stress amount before reduction</param>
    /// <returns>Reduced stress amount (minimum 0)</returns>
    public int CalculateStressReduction(int successes, int baseStress)
    {
        int reduction = successes; // 1:1 reduction
        return Math.Max(0, baseStress - reduction);
    }

    /// <summary>
    /// Performs a full environmental stress check with WILL resistance
    /// v0.20.2: Now considers metaphysical cover bonuses from grid position
    /// </summary>
    /// <param name="character">Character being affected</param>
    /// <param name="baseStress">Base stress from environment</param>
    /// <param name="grid">Optional battlefield grid for cover bonuses</param>
    /// <returns>Tuple of (actual stress to apply, successes, roll details)</returns>
    public (int stressToApply, int successes, string rollDetails) RollEnvironmentalStressResistance(PlayerCharacter character, int baseStress, BattlefieldGrid? grid = null)
    {
        int willValue = character.GetAttributeValue("will");

        // [v0.20.2] Apply metaphysical cover bonus
        int coverBonus = 0;
        if (grid != null)
        {
            var bonus = _coverService.CalculateCoverBonus(character, null, AttackType.Psychic, grid);
            coverBonus = bonus.ResolveBonus;
            willValue += coverBonus;
        }

        var result = _diceService.Roll(willValue);
        int reducedStress = CalculateStressReduction(result.Successes, baseStress);

        string coverText = coverBonus > 0 ? $" (+{coverBonus} from metaphysical cover)" : "";
        string rollDetails = $"WILL Resolve Check ({baseStress} Stress): Rolled {willValue} dice{coverText} → {FormatDiceRoll(result)} → {result.Successes} successes\n" +
                           $"Stress reduced by {result.Successes}. Gaining {reducedStress} Psychic Stress.";

        return (reducedStress, result.Successes, rollDetails);
    }

    /// <summary>
    /// Formats dice roll results for display
    /// </summary>
    private string FormatDiceRoll(DiceResult result)
    {
        // Format: [2, 4, 5, 6, 6, 3]
        return $"[{string.Join(", ", result.Rolls)}]";
    }

    /// <summary>
    /// Generates flavor text for Resolve Check results
    /// </summary>
    public string GetResolveCheckFlavorText(bool success, int stress)
    {
        if (success && stress == 0)
        {
            return "You steel your mind against the Blight's assault.";
        }
        else if (stress <= 5)
        {
            return "The screaming in your mind is muted, but present.";
        }
        else if (stress <= 10)
        {
            return "The Cursed Choir gnaws at the edges of your sanity.";
        }
        else
        {
            return "The Blight's psychic scream tears through your consciousness.";
        }
    }

    /// <summary>
    /// Generates flavor text for Forlorn aura checks
    /// </summary>
    public string GetForlornAuraFlavorText(string enemyName, bool resisted)
    {
        if (resisted)
        {
            return $"The {enemyName}'s presence gnaws at your sanity, but you resist...";
        }
        else
        {
            return $"The {enemyName}'s twisted existence assaults your mind...";
        }
    }
}
