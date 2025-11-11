namespace RuneAndRust.Core;

/// <summary>
/// [v0.6] Environmental hazard types with distinct mechanical behaviors
/// </summary>
public enum HazardType
{
    None = 0,

    // v0.4 Generic Hazards
    GenericDamage,          // Simple flat damage per turn (backward compatible)

    // v0.6 Specific Hazards
    ToxicFumes,             // Damage + Stress per turn (unavoidable)
    ToxicSludge,            // Dice-based damage per turn (drainable)
    UnstableFlooring,       // FINESSE check or fall damage (triggered on movement)
    ElectricalHazard,       // Variable electrical damage (contact-based)
    Radiation,              // Flat damage per turn (cumulative exposure)

    // Future expansion
    Fire,                   // Damage + potential for spreading
    Ice,                    // Movement penalty + cold damage
    Darkness,               // Accuracy penalty + stress
    Vacuum                  // Immediate lethal damage without protection
}
