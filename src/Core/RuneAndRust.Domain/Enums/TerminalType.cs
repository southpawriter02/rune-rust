// ------------------------------------------------------------------------------
// <copyright file="TerminalType.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Classification of terminal types with associated base difficulty classes and ICE ratings.
// Part of v0.15.4b Terminal Hacking System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Classification of terminal types with associated base difficulty classes and ICE ratings.
/// </summary>
/// <remarks>
/// <para>
/// Terminal difficulty follows a progression from civilian data ports found in
/// everyday use to legendary Jötun Archives of incomprehensible complexity.
/// </para>
/// <para>
/// Base DCs represent the Layer 1 (Access) difficulty. Subsequent layers may
/// have modified DCs based on security level and data type.
/// </para>
/// <para>
/// ICE (Intrusion Countermeasures Electronics) ratings indicate the type and
/// danger level of defensive systems present. ICE mechanics are detailed in v0.15.4c.
/// </para>
/// </remarks>
public enum TerminalType
{
    /// <summary>
    /// Civilian data port for everyday use.
    /// </summary>
    /// <remarks>
    /// Found in residential areas, public spaces, and merchant stalls.
    /// Minimal security, no ICE countermeasures.
    /// Base DC: 8 (Layer 1 Access).
    /// ICE Rating: None.
    /// </remarks>
    CivilianDataPort = 8,

    /// <summary>
    /// Corporate mainframe with standard business security.
    /// </summary>
    /// <remarks>
    /// Found in corporate offices, trade companies, and guild halls.
    /// Standard security with passive trace countermeasures.
    /// Base DC: 12 (Layer 1 Access).
    /// ICE Rating: Passive (Trace).
    /// </remarks>
    CorporateMainframe = 12,

    /// <summary>
    /// Security hub with active defense systems.
    /// </summary>
    /// <remarks>
    /// Found in security stations, watch posts, and enforcement facilities.
    /// Enhanced security with active countermeasures.
    /// Base DC: 16 (Layer 1 Access).
    /// ICE Rating: Active (Attack).
    /// </remarks>
    SecurityHub = 16,

    /// <summary>
    /// Military server with hardened defenses.
    /// </summary>
    /// <remarks>
    /// Found in military installations, armories, and command centers.
    /// Heavy security with aggressive countermeasures.
    /// Base DC: 20 (Layer 1 Access).
    /// ICE Rating: Active (Attack) + Lethal backup.
    /// </remarks>
    MilitaryServer = 20,

    /// <summary>
    /// Ancient Jötun Archive of Old World origin.
    /// </summary>
    /// <remarks>
    /// Found in ruins, ancient facilities, and excavation sites.
    /// Incomprehensible security from the World Before.
    /// Often [Glitched] or [Blighted], adding corruption modifiers.
    /// Base DC: 24 (Layer 1 Access).
    /// ICE Rating: Lethal (Neural).
    /// </remarks>
    JotunArchive = 24,

    /// <summary>
    /// Glitched Manifold with unpredictable behavior.
    /// </summary>
    /// <remarks>
    /// Corrupted terminals where reality itself is unstable.
    /// DC varies based on glitch cycle phase (determined at encounter).
    /// May provide unexpected benefits or catastrophic failures.
    /// Base DC: 0 (determined dynamically, typically 12-20).
    /// ICE Rating: Unpredictable.
    /// </remarks>
    GlitchedManifold = 0
}
