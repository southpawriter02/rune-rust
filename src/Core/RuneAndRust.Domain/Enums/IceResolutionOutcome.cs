// ------------------------------------------------------------------------------
// <copyright file="IceResolutionOutcome.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// The possible outcomes of an ICE encounter resolution.
// Part of v0.15.4c ICE Countermeasures implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// The possible outcomes of an ICE encounter resolution.
/// </summary>
/// <remarks>
/// <para>
/// ICE encounters can result in four possible outcomes, determining
/// what consequences are applied to the character and terminal state.
/// </para>
/// <para>
/// <b>Outcome Resolution by ICE Type:</b>
/// </para>
/// <list type="bullet">
///   <item>
///     <description>
///       <b>Passive ICE:</b> Character wins → Evaded, ICE wins → IceWon (location revealed).
///     </description>
///   </item>
///   <item>
///     <description>
///       <b>Active ICE:</b> Character wins → CharacterWon (ICE disabled), ICE wins → IceWon (disconnect).
///     </description>
///   </item>
///   <item>
///     <description>
///       <b>Lethal ICE:</b> Save success → CharacterWon (escape), Save fail → IceWon (neural strike).
///     </description>
///   </item>
/// </list>
/// </remarks>
public enum IceResolutionOutcome
{
    /// <summary>
    /// Encounter has not yet been resolved.
    /// </summary>
    /// <remarks>
    /// This is the initial state of an ICE encounter after it has been
    /// triggered but before the character has attempted to resolve it.
    /// </remarks>
    Pending = 0,

    /// <summary>
    /// Character won the contested check or save.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The character successfully dealt with the ICE threat. The specific
    /// outcome depends on ICE type:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Active ICE: ICE is disabled, may grant bonus dice.</description></item>
    ///   <item><description>Lethal ICE: Character escaped, minimal consequences.</description></item>
    /// </list>
    /// </remarks>
    CharacterWon = 1,

    /// <summary>
    /// Character lost the contested check or failed the save.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The ICE successfully activated against the character. Consequences
    /// depend on ICE type:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Passive ICE: Location revealed, alert level increased.</description></item>
    ///   <item><description>Active ICE: Forced disconnect, temporary lockout.</description></item>
    ///   <item><description>Lethal ICE: Psychic damage, stress, permanent lockout.</description></item>
    /// </list>
    /// </remarks>
    IceWon = 2,

    /// <summary>
    /// Character successfully evaded the ICE (Passive only).
    /// </summary>
    /// <remarks>
    /// <para>
    /// The character slipped past the ICE without triggering its full effects.
    /// This outcome is specific to Passive (Trace) ICE and indicates the
    /// character avoided location detection.
    /// </para>
    /// <para>
    /// Unlike CharacterWon, evading Passive ICE does not disable it—the ICE
    /// may re-trigger on subsequent failures.
    /// </para>
    /// </remarks>
    Evaded = 3
}
