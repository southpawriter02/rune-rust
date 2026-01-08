namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Defines how a status effect behaves when applied to a target that already has it.
/// </summary>
/// <remarks>
/// <para>Stacking rules are configured per effect in the status-effects.json file.</para>
/// <para>Design decision: All buffs use Block to prevent overpowered stacking.</para>
/// </remarks>
public enum StackingRule
{
    /// <summary>
    /// Reapplication refreshes duration to maximum.
    /// Used by: Bleeding, Poisoned, Burning, Frozen, Weakened, Slowed, Exhausted, Wet, Chilled.
    /// </summary>
    RefreshDuration,

    /// <summary>
    /// Reapplication increases intensity/stacks up to a maximum.
    /// Each stack increases the effect's power (e.g., more DoT damage).
    /// </summary>
    Stack,

    /// <summary>
    /// Reapplication is blocked/resisted while effect is active.
    /// Used by: All buffs, Stunned, Blinded, Feared, Silenced, Cursed, Disarmed, On Fire, Electrified.
    /// </summary>
    Block
}
