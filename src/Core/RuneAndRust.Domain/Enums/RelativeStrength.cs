// ------------------------------------------------------------------------------
// <copyright file="RelativeStrength.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Represents the relative power dynamic between player and NPC for intimidation.
// Part of v0.15.3d Intimidation System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the relative power dynamic between the player and an NPC
/// during an intimidation attempt. Based on level comparison and
/// affects dice modifiers for intimidation checks.
/// </summary>
/// <remarks>
/// <para>
/// Intimidation effectiveness depends heavily on perceived power differential.
/// A clearly outmatched target is more likely to comply, while a target who
/// feels capable of handling the threat is more likely to resist.
/// </para>
/// <para>
/// The comparison is based on character levels:
/// </para>
/// <list type="bullet">
///   <item>
///     <description>
///       <b>PlayerWeaker</b>: Player level is 2+ below NPC level.
///       NPC gains +1d10 as they feel capable of handling the threat.
///     </description>
///   </item>
///   <item>
///     <description>
///       <b>Equal</b>: Player level is within ±1 of NPC level.
///       No modifier applied to either side.
///     </description>
///   </item>
///   <item>
///     <description>
///       <b>PlayerStronger</b>: Player level is 2+ above NPC level.
///       Player gains +1d10 as the target feels outmatched.
///     </description>
///   </item>
/// </list>
/// </remarks>
public enum RelativeStrength
{
    /// <summary>
    /// The player is weaker than the target (lower level by 2+).
    /// </summary>
    /// <remarks>
    /// NPC gains +1d10 to their resistance as they feel capable of
    /// handling the threat. The target may even find the attempt amusing.
    /// </remarks>
    PlayerWeaker = 0,

    /// <summary>
    /// The player and target are roughly equal in power (within ±1 level).
    /// </summary>
    /// <remarks>
    /// No modifier applied to either side. The outcome depends entirely
    /// on the other factors: target type, reputation, equipment, and allies.
    /// </remarks>
    Equal = 1,

    /// <summary>
    /// The player is stronger than the target (higher level by 2+).
    /// </summary>
    /// <remarks>
    /// Player gains +1d10 to their intimidation pool as the target
    /// recognizes they are outmatched. The target's survival instincts
    /// make them more susceptible to fear.
    /// </remarks>
    PlayerStronger = 2
}
