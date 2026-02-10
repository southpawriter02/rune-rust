// ═══════════════════════════════════════════════════════════════════════════════
// JotunTechnologyType.cs
// Types of dormant Jötun technology that can be activated by the Voice of the
// Giants capstone ability.
// Version: 0.20.3c
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Types of Jötun technology that can be activated by the Voice of the Giants
/// capstone ability.
/// </summary>
/// <remarks>
/// <para>
/// Each technology type produces different effects when activated. Effects
/// vary from area healing (Medical Bay) to environmental control and fast
/// travel. All activations last 1 hour by default.
/// </para>
/// </remarks>
/// <seealso cref="JotunReaderAbilityId"/>
public enum JotunTechnologyType
{
    /// <summary>Restores power to connected systems.</summary>
    PowerNode = 0,

    /// <summary>Activates automated defense systems.</summary>
    DefenseGrid = 1,

    /// <summary>Enables fast travel within complex.</summary>
    TransportSystem = 2,

    /// <summary>Provides healing in area.</summary>
    MedicalBay = 3,

    /// <summary>Unlocks restricted data access.</summary>
    ArchiveTerminal = 4,

    /// <summary>Enables advanced crafting.</summary>
    Fabricator = 5,

    /// <summary>Reveals hidden creatures/objects.</summary>
    SensorArray = 6,

    /// <summary>Controls environmental conditions.</summary>
    EnvironmentalControl = 7
}
