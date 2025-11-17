namespace RuneAndRust.Core.CombatFlavor;

/// <summary>
/// v0.38.6: Determines CombatOutcome from attack results for flavor text generation
/// </summary>
public static class CombatOutcomeCalculator
{
    /// <summary>
    /// Calculates the combat outcome based on attack results
    /// </summary>
    /// <param name="attackSuccesses">Number of attack successes</param>
    /// <param name="defendSuccesses">Number of defend successes</param>
    /// <param name="damage">Actual damage dealt</param>
    /// <param name="maxPossibleDamage">Maximum possible damage (for percentage calculation)</param>
    /// <param name="isCriticalHit">Whether this was a critical hit</param>
    /// <returns>The combat outcome for flavor text selection</returns>
    public static CombatOutcome DetermineOutcome(
        int attackSuccesses,
        int defendSuccesses,
        int damage,
        int maxPossibleDamage,
        bool isCriticalHit = false)
    {
        // Critical hit takes precedence
        if (isCriticalHit)
        {
            return CombatOutcome.CriticalHit;
        }

        var netSuccesses = attackSuccesses - defendSuccesses;

        // Miss - attack failed to overcome defense
        if (netSuccesses <= 0 || damage == 0)
        {
            // If attack matched defense exactly, it's deflected
            if (netSuccesses == 0 && attackSuccesses > 0)
            {
                return CombatOutcome.Deflected;
            }
            return CombatOutcome.Miss;
        }

        // Hit - determine quality based on damage percentage
        if (maxPossibleDamage > 0)
        {
            var damagePercentage = (float)damage / maxPossibleDamage;

            return damagePercentage switch
            {
                >= 0.76f => CombatOutcome.DevastatingHit,  // 76-100%
                >= 0.26f => CombatOutcome.SolidHit,        // 26-75%
                _ => CombatOutcome.GlancingHit             // 1-25%
            };
        }

        // Fallback: Use net successes if we don't have max damage
        return netSuccesses switch
        {
            >= 5 => CombatOutcome.DevastatingHit,
            >= 2 => CombatOutcome.SolidHit,
            _ => CombatOutcome.GlancingHit
        };
    }

    /// <summary>
    /// Simplified outcome determination when only successes are available
    /// </summary>
    public static CombatOutcome DetermineOutcomeFromSuccesses(
        int attackSuccesses,
        int defendSuccesses,
        bool isCriticalHit = false)
    {
        if (isCriticalHit)
        {
            return CombatOutcome.CriticalHit;
        }

        var netSuccesses = attackSuccesses - defendSuccesses;

        if (netSuccesses <= 0)
        {
            return netSuccesses == 0 && attackSuccesses > 0
                ? CombatOutcome.Deflected
                : CombatOutcome.Miss;
        }

        return netSuccesses switch
        {
            >= 5 => CombatOutcome.DevastatingHit,
            >= 2 => CombatOutcome.SolidHit,
            _ => CombatOutcome.GlancingHit
        };
    }
}
