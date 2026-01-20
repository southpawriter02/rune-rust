namespace RuneAndRust.Domain.Enums;

/// <summary>
/// What action a trigger performs.
/// </summary>
public enum TriggerActionType
{
    /// <summary>Deal damage to target.</summary>
    Damage,

    /// <summary>Heal target.</summary>
    Heal,

    /// <summary>Apply another effect.</summary>
    ApplyEffect,

    /// <summary>Remove an effect.</summary>
    RemoveEffect,

    /// <summary>Temporary stat change.</summary>
    ModifyStat,

    /// <summary>Custom action handler.</summary>
    Custom
}
