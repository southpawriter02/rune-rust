namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Defines when a status effect can be applied.
/// </summary>
/// <remarks>
/// <para>Triggers are used by weapons, abilities, and combat events to determine
/// when effects should be applied to targets.</para>
/// </remarks>
public enum EffectTrigger
{
    /// <summary>Applied when an attack hits.</summary>
    OnHit,

    /// <summary>Applied when damage is dealt.</summary>
    OnDamage,

    /// <summary>Applied when taking damage.</summary>
    OnDamageTaken,

    /// <summary>Applied at the start of turn.</summary>
    OnTurnStart,

    /// <summary>Applied at the end of turn.</summary>
    OnTurnEnd,

    /// <summary>Applied when using an ability.</summary>
    OnAbilityUse,

    /// <summary>Applied when entering combat.</summary>
    OnCombatStart,

    /// <summary>Applied manually (items, abilities).</summary>
    Manual
}
