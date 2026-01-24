// ------------------------------------------------------------------------------
// <copyright file="TrapType.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Defines trap classifications with associated detection and disarmament DCs.
// Part of v0.15.4d Trap Disarmament System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Defines trap classifications with associated detection and disarmament DCs.
/// </summary>
/// <remarks>
/// <para>
/// Each trap type has specific characteristics:
/// <list type="bullet">
///   <item><description>Tripwire: Detection DC 8, Disarm DC 8, triggers alarm</description></item>
///   <item><description>PressurePlate: Detection DC 10, Disarm DC 12, deals 2d10 physical damage</description></item>
///   <item><description>Electrified: Detection DC 14, Disarm DC 16, deals 3d10 lightning damage</description></item>
///   <item><description>LaserGrid: Detection DC 18, Disarm DC 20, triggers alarm + lockdown</description></item>
///   <item><description>JotunDefense: Detection DC 22, Disarm DC 24, deals 5d10 damage + alert</description></item>
/// </list>
/// </para>
/// <para>
/// Tool requirements:
/// <list type="bullet">
///   <item><description>DC 4+ traps require [Tinker's Toolkit] (Proper or Masterwork tools)</description></item>
///   <item><description>Cannot attempt DC 4+ with BareHands</description></item>
/// </list>
/// </para>
/// </remarks>
public enum TrapType
{
    /// <summary>
    /// Simple wire that triggers an alarm when disturbed.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Detection DC: 8, Disarm DC: 8.
    /// Effect: Alarm (alerts nearby enemies).
    /// Salvage: [Trigger Mechanism], [Wire Bundle].
    /// </para>
    /// </remarks>
    Tripwire = 0,

    /// <summary>
    /// Weight-sensitive plate that triggers a crushing mechanism.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Detection DC: 10, Disarm DC: 12.
    /// Effect: 2d10 physical damage.
    /// Salvage: [High-Tension Spring], [Pressure Sensor].
    /// </para>
    /// </remarks>
    PressurePlate = 1,

    /// <summary>
    /// Conductive surface that delivers a powerful electric shock.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Detection DC: 14, Disarm DC: 16.
    /// Effect: 3d10 lightning damage.
    /// Salvage: [Capacitor], [Blighted Power Cell].
    /// </para>
    /// </remarks>
    Electrified = 2,

    /// <summary>
    /// Old World security beams that trigger alarms and lockdowns.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Detection DC: 18, Disarm DC: 20.
    /// Effect: Alarm + lockdown (doors seal).
    /// Salvage: [Sensor Module], [Focusing Crystal].
    /// </para>
    /// </remarks>
    LaserGrid = 3,

    /// <summary>
    /// Incomprehensible ancient defense mechanism.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Detection DC: 22, Disarm DC: 24.
    /// Effect: 5d10 physical damage + alert.
    /// Salvage: [Jötun Mechanism Fragment], [Ancient Power Core].
    /// </para>
    /// </remarks>
    JotunDefense = 4
}
