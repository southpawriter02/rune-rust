namespace RuneAndRust.Domain.Enums;

/// <summary>
/// When an effect trigger activates.
/// </summary>
public enum TriggerTiming
{
    /// <summary>When effect is first applied.</summary>
    OnApply,

    /// <summary>At the start of target's turn.</summary>
    OnTurnStart,

    /// <summary>At the end of target's turn.</summary>
    OnTurnEnd,

    /// <summary>Each turn (alias for OnTurnStart).</summary>
    OnTick,

    /// <summary>When effect expires naturally.</summary>
    OnExpire,

    /// <summary>When effect is removed (any way).</summary>
    OnRemove,

    /// <summary>When target takes damage.</summary>
    OnDamaged,

    /// <summary>When target makes an attack.</summary>
    OnAttack
}
