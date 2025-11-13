namespace RuneAndRust.Core;

/// <summary>
/// v0.19.8: Runic Constructs created by Vard-Warden Mystics
/// Physical manifestations of stable Aether (barriers and sanctified zones)
/// </summary>
public class RunicConstruct
{
    public string ConstructId { get; set; } = Guid.NewGuid().ToString();
    public ConstructType Type { get; set; }
    public int CurrentHP { get; set; }
    public int MaxHP { get; set; }
    public int DurationRemaining { get; set; } // Turns remaining
    public string Location { get; set; } = "Front"; // "Front" or "Back" row
    public string OwnerId { get; set; } = string.Empty; // Character who created it
    public bool IsActive { get; set; } = true;

    // For Sanctified Ground zones
    public int HealPerTurn { get; set; } = 0; // d6 healing for allies
    public int DamageToBlightedPerTurn { get; set; } = 0; // d6 damage to Blighted/Undying
    public bool CleansesDebuffs { get; set; } = false; // Aegis of Sanctity upgrade

    /// <summary>
    /// Take damage (for Runic Barriers)
    /// </summary>
    public bool TakeDamage(int damage)
    {
        if (Type != ConstructType.RunicBarrier)
        {
            return false; // Only barriers can take damage
        }

        CurrentHP -= damage;

        if (CurrentHP <= 0)
        {
            IsActive = false;
            return true; // Barrier destroyed
        }

        return false; // Still standing
    }

    /// <summary>
    /// Countdown duration (called at end of turn)
    /// </summary>
    public void CountdownDuration()
    {
        DurationRemaining--;

        if (DurationRemaining <= 0)
        {
            IsActive = false;
        }
    }

    /// <summary>
    /// Heal the barrier (Reinforce Ward ability)
    /// </summary>
    public void Heal(int amount)
    {
        if (Type != ConstructType.RunicBarrier)
        {
            return; // Only barriers can be healed
        }

        CurrentHP += amount;
        CurrentHP = Math.Min(CurrentHP, MaxHP); // Don't exceed max
    }

    /// <summary>
    /// Boost zone effectiveness (Reinforce Ward ability)
    /// </summary>
    public void BoostZone(int extraDuration, int extraHealing = 0)
    {
        if (Type != ConstructType.SanctifiedGround)
        {
            return; // Only zones can be boosted
        }

        DurationRemaining += extraDuration;
        if (extraHealing > 0)
        {
            HealPerTurn += extraHealing;
        }
    }
}

/// <summary>
/// Types of Runic Constructs
/// </summary>
public enum ConstructType
{
    RunicBarrier,       // Physical wall that blocks movement and line-of-sight
    SanctifiedGround    // Healing/purifying zone
}
