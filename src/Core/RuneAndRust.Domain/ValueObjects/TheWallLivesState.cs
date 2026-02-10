namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the state of The Wall Lives capstone ability for the Skjaldmær specialization.
/// Manages invulnerability duration and lethal damage prevention.
/// </summary>
/// <remarks>
/// <para>When active, this effect prevents the character's HP from dropping below 1.
/// The effect lasts for 3 turns and can only be used once per combat.</para>
/// <para>Damage flow integration:
/// <list type="number">
/// <item>Unbreakable reduction applied first</item>
/// <item>Armor/resistance applied</item>
/// <item>The Wall Lives caps damage to preserve 1 HP</item>
/// </list>
/// </para>
/// </remarks>
public sealed record TheWallLivesState
{
    /// <summary>
    /// Default duration of The Wall Lives effect in turns.
    /// </summary>
    public const int DefaultDuration = 3;

    /// <summary>
    /// Whether The Wall Lives effect is currently active.
    /// When active, the character cannot be reduced below 1 HP.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Turns remaining for this effect (0-3, decrements each turn).
    /// Once this reaches 0, the effect is disabled automatically.
    /// </summary>
    public int TurnsRemaining { get; private set; }

    /// <summary>
    /// UTC timestamp when the effect was activated.
    /// Used for audit trails and duration verification.
    /// </summary>
    public DateTime ActivatedAt { get; private set; }

    /// <summary>
    /// Activates The Wall Lives effect with its default duration.
    /// </summary>
    /// <remarks>
    /// Called when The Wall Lives ability is executed.
    /// Sets initial duration to <see cref="DefaultDuration"/> turns and marks as active.
    /// </remarks>
    public void Activate()
    {
        IsActive = true;
        TurnsRemaining = DefaultDuration;
        ActivatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Processes one turn of the effect, decrementing remaining duration.
    /// </summary>
    /// <remarks>
    /// Called at the end of each combat turn. Decrements <see cref="TurnsRemaining"/>
    /// and automatically deactivates if the counter reaches zero.
    /// </remarks>
    public void Tick()
    {
        if (!IsActive)
            return;

        TurnsRemaining--;

        if (TurnsRemaining <= 0)
        {
            IsActive = false;
        }
    }

    /// <summary>
    /// Checks if the effect has expired.
    /// </summary>
    /// <returns>True if the effect is inactive or has no turns remaining.</returns>
    public bool IsExpired() => !IsActive || TurnsRemaining <= 0;

    /// <summary>
    /// Prevents lethal damage while the effect is active by capping damage
    /// to ensure the character retains at least 1 HP.
    /// </summary>
    /// <param name="currentHp">The character's HP before damage application.</param>
    /// <param name="incomingDamage">The damage about to be applied.</param>
    /// <returns>
    /// The actual damage to apply. If the incoming damage would reduce HP below 1,
    /// it is capped to leave exactly 1 HP. Otherwise, the full damage is returned.
    /// </returns>
    /// <remarks>
    /// <para>Example:</para>
    /// <para>CurrentHP = 5, IncomingDamage = 10</para>
    /// <para>Normal: NewHP = 5 - 10 = -5 → 0 (death)</para>
    /// <para>Protected: Damage capped to 4, NewHP = 5 - 4 = 1 (survives)</para>
    /// </remarks>
    public int PreventLethalDamage(int currentHp, int incomingDamage)
    {
        if (!IsActive)
            return incomingDamage;

        // Calculate what HP would be after damage
        var resultingHp = currentHp - incomingDamage;

        // If damage would reduce below 1 HP, cap it
        if (resultingHp < 1)
        {
            // Damage is capped to leave exactly 1 HP
            var cappedDamage = currentHp - 1;
            return Math.Max(cappedDamage, 0); // Ensure non-negative
        }

        // Damage is not lethal, apply normally
        return incomingDamage;
    }

    /// <summary>
    /// Deactivates The Wall Lives effect immediately.
    /// </summary>
    /// <remarks>
    /// Called when the effect expires naturally, is dispelled, or combat ends.
    /// </remarks>
    public void Deactivate()
    {
        IsActive = false;
        TurnsRemaining = 0;
    }

    /// <summary>
    /// Gets the remaining duration of the effect in turns.
    /// </summary>
    /// <returns>
    /// Number of turns remaining (0-3) if active, or 0 if inactive.
    /// </returns>
    public int GetRemainingDuration() => IsActive ? TurnsRemaining : 0;
}
