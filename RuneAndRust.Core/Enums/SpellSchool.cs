namespace RuneAndRust.Core.Enums;

/// <summary>
/// Represents the magical school or discipline to which a spell belongs.
/// Each school governs a different aspect of magical manipulation.
/// </summary>
/// <remarks>
/// Schools (v0.4.3b - The Grimoire):
/// - Destruction: Offensive magic dealing damage
/// - Restoration: Healing and protective magic
/// - Alteration: Transformation and manipulation magic
/// - Divination: Knowledge and perception magic
/// </remarks>
public enum SpellSchool
{
    /// <summary>
    /// Offensive magic focused on dealing damage and destruction.
    /// Spells that harm, burn, freeze, or otherwise injure targets.
    /// </summary>
    Destruction = 0,

    /// <summary>
    /// Healing and protective magic focused on restoration.
    /// Spells that heal wounds, cure ailments, or shield allies.
    /// </summary>
    Restoration = 1,

    /// <summary>
    /// Transformation magic focused on changing properties.
    /// Spells that alter physical attributes, environments, or states.
    /// </summary>
    Alteration = 2,

    /// <summary>
    /// Knowledge and perception magic focused on information.
    /// Spells that reveal hidden things, predict outcomes, or detect threats.
    /// </summary>
    Divination = 3
}
