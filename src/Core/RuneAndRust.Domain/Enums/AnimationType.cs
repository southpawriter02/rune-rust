namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Types of combat animations.
/// </summary>
public enum AnimationType
{
    /// <summary>Normal attack hit.</summary>
    AttackHit,
    
    /// <summary>Attack missed.</summary>
    AttackMiss,
    
    /// <summary>Critical hit (enhanced hit animation).</summary>
    CriticalHit,
    
    /// <summary>Ability being cast.</summary>
    AbilityCast,
    
    /// <summary>Ability effect on target.</summary>
    AbilityEffect,
    
    /// <summary>Healing effect.</summary>
    Heal,
    
    /// <summary>Floating damage number.</summary>
    DamageNumber,
    
    /// <summary>Entity death sequence.</summary>
    Death,
    
    /// <summary>Status effect applied.</summary>
    StatusApplied,
    
    /// <summary>Status effect removed.</summary>
    StatusRemoved
}
