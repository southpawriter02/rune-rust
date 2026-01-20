namespace RuneAndRust.Domain.Entities;

/// <summary>
/// Represents a player's instance of an ability with cooldown and usage tracking.
/// </summary>
/// <remarks>
/// PlayerAbility tracks the mutable state of an ability for a specific player,
/// including cooldown remaining, times used, and unlock status. The ability's
/// definition (name, effects, cost) is stored in AbilityDefinition.
/// </remarks>
public class PlayerAbility
{
    /// <summary>
    /// Gets the ID of the ability definition this instance represents.
    /// </summary>
    public string AbilityDefinitionId { get; private set; }

    /// <summary>
    /// Gets the current cooldown remaining (0 = ready to use).
    /// </summary>
    public int CurrentCooldown { get; private set; }

    /// <summary>
    /// Gets the number of times this ability has been used.
    /// </summary>
    public int TimesUsed { get; private set; }

    /// <summary>
    /// Gets whether the ability has been unlocked.
    /// </summary>
    public bool IsUnlocked { get; private set; }

    /// <summary>
    /// Gets when the ability was unlocked.
    /// </summary>
    public DateTime? UnlockedAt { get; private set; }

    /// <summary>
    /// Gets whether the ability is ready to use (off cooldown and unlocked).
    /// </summary>
    public bool IsReady => CurrentCooldown == 0 && IsUnlocked;

    /// <summary>
    /// Gets whether the ability is on cooldown.
    /// </summary>
    public bool IsOnCooldown => CurrentCooldown > 0;

    /// <summary>
    /// Private parameterless constructor for EF Core.
    /// </summary>
    private PlayerAbility()
    {
        AbilityDefinitionId = null!;
    }

    /// <summary>
    /// Creates a new player ability instance.
    /// </summary>
    /// <param name="abilityDefinitionId">The ID of the ability definition.</param>
    /// <param name="isUnlocked">Whether the ability starts unlocked (default true).</param>
    /// <returns>A new PlayerAbility instance.</returns>
    /// <exception cref="ArgumentException">Thrown when abilityDefinitionId is null or whitespace.</exception>
    public static PlayerAbility Create(string abilityDefinitionId, bool isUnlocked = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(abilityDefinitionId);

        return new PlayerAbility
        {
            AbilityDefinitionId = abilityDefinitionId.ToLowerInvariant(),
            CurrentCooldown = 0,
            TimesUsed = 0,
            IsUnlocked = isUnlocked,
            UnlockedAt = isUnlocked ? DateTime.UtcNow : null
        };
    }

    /// <summary>
    /// Uses the ability, setting its cooldown and incrementing the usage count.
    /// </summary>
    /// <param name="cooldownDuration">The cooldown duration from the ability definition.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when cooldownDuration is negative.</exception>
    public void Use(int cooldownDuration)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(cooldownDuration);

        CurrentCooldown = cooldownDuration;
        TimesUsed++;
    }

    /// <summary>
    /// Reduces the current cooldown by the specified amount.
    /// </summary>
    /// <param name="amount">Amount to reduce (default 1).</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when amount is negative.</exception>
    public void ReduceCooldown(int amount = 1)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        CurrentCooldown = Math.Max(0, CurrentCooldown - amount);
    }

    /// <summary>
    /// Resets the cooldown to zero.
    /// </summary>
    public void ResetCooldown()
    {
        CurrentCooldown = 0;
    }

    /// <summary>
    /// Unlocks the ability for use.
    /// </summary>
    public void Unlock()
    {
        if (!IsUnlocked)
        {
            IsUnlocked = true;
            UnlockedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Locks the ability, preventing use.
    /// </summary>
    public void Lock()
    {
        IsUnlocked = false;
    }
}
