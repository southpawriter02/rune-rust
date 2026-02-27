namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Lifecycle states for hunting traps placed by the Veiðimaðr (Hunter) specialization.
/// </summary>
/// <remarks>
/// <para>Traps follow a one-way lifecycle: Armed → Triggered/Disarmed → Destroyed.
/// Once triggered or destroyed, a trap cannot be rearmed.</para>
/// <para>Introduced in v0.20.7b as part of the Trap Mastery ability.</para>
/// </remarks>
public enum TrapStatus
{
    /// <summary>
    /// Trap is armed and ready to trigger when an enemy enters its space.
    /// </summary>
    Armed = 0,

    /// <summary>
    /// Trap has been triggered by a target stepping on it.
    /// Damage and immobilize effects have been applied.
    /// </summary>
    Triggered = 1,

    /// <summary>
    /// Trap has been safely disarmed (by the hunter or by an enemy).
    /// No damage was dealt.
    /// </summary>
    Disarmed = 2,

    /// <summary>
    /// Trap has been destroyed and cannot be rearmed or reused.
    /// Final state for all traps after encounter end or manual destruction.
    /// </summary>
    Destroyed = 3
}
