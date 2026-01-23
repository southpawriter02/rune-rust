// ------------------------------------------------------------------------------
// <copyright file="LockType.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Classification of lock types with associated base difficulty classes.
// Part of v0.15.4a Lockpicking System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Classification of lock types with associated base difficulty classes.
/// </summary>
/// <remarks>
/// <para>
/// Lock difficulty follows a progression from improvised latches found in
/// makeshift shelters to legendary Jötun-forged mechanisms from the Old World.
/// </para>
/// <para>
/// Base DCs assume normal conditions. Additional modifiers apply for:
/// <list type="bullet">
///   <item><description>Corruption level ([Glitched] +2 DC, [Blighted] +4 DC)</description></item>
///   <item><description>Previous failed attempts (failure escalation if enabled)</description></item>
///   <item><description>[Mechanism Jammed] status (+2 DC permanent)</description></item>
/// </list>
/// </para>
/// <para>
/// Tool requirements:
/// <list type="bullet">
///   <item><description>DC 6 (Improvised): Can attempt bare-handed at -2d10</description></item>
///   <item><description>DC 10+ (Simple and above): Requires [Tinker's Toolkit]</description></item>
/// </list>
/// </para>
/// </remarks>
public enum LockType
{
    /// <summary>
    /// Improvised latch or simple hook closure.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Found on makeshift shelters, scrap containers, and jury-rigged doors.
    /// Can be opened bare-handed with patience.
    /// </para>
    /// <para>
    /// Base DC: 6 (requires ~1 net success).
    /// Tool requirement: None (can attempt bare-handed).
    /// Salvage: Wire Bundle, Small Spring.
    /// </para>
    /// </remarks>
    ImprovisedLatch = 6,

    /// <summary>
    /// Basic pin tumbler or simple mechanical lock.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Common on residential doors, basic chests, and standard containers.
    /// Requires at least improvised tools to attempt.
    /// </para>
    /// <para>
    /// Base DC: 10 (requires ~2 net successes).
    /// Tool requirement: Improvised or better.
    /// Salvage: High-Tension Spring, Pin Set.
    /// </para>
    /// </remarks>
    SimpleLock = 10,

    /// <summary>
    /// Standard quality lock with multiple pins or wards.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Found on commercial properties, secure storage, and quality containers.
    /// Requires [Tinker's Toolkit] or equivalent proper tools.
    /// </para>
    /// <para>
    /// Base DC: 14 (requires ~2-3 net successes).
    /// Tool requirement: Proper (Tinker's Toolkit).
    /// Salvage: High-Tension Spring, Pin Set.
    /// </para>
    /// </remarks>
    StandardLock = 14,

    /// <summary>
    /// Complex mechanism with advanced security features.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Found on high-security installations, vaults, and valuable caches.
    /// May include false pins, trap triggers, or anti-pick features.
    /// </para>
    /// <para>
    /// Base DC: 18 (requires ~3-4 net successes).
    /// Tool requirement: Proper (Tinker's Toolkit).
    /// Salvage: Circuit Fragment, Power Cell Fragment.
    /// </para>
    /// </remarks>
    ComplexLock = 18,

    /// <summary>
    /// Master-crafted lock of exceptional quality.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Rare locks created by skilled Dvergr craftsmen or master locksmiths.
    /// Often guards the most valuable treasures.
    /// </para>
    /// <para>
    /// Base DC: 22 (requires ~4-5 net successes).
    /// Tool requirement: Proper (Tinker's Toolkit).
    /// Salvage: Circuit Fragment, Power Cell Fragment.
    /// </para>
    /// </remarks>
    MasterLock = 22,

    /// <summary>
    /// Legendary Old World lock of Jötun manufacture.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Ancient mechanisms of incomprehensible complexity from the World Before.
    /// May incorporate runic elements or require specific bypass rituals.
    /// Often [Glitched] or [Blighted], further increasing difficulty.
    /// </para>
    /// <para>
    /// Base DC: 26 (requires ~5+ net successes or master abilities).
    /// Tool requirement: Proper (Tinker's Toolkit).
    /// Salvage: Encryption Chip, Biometric Sensor.
    /// </para>
    /// </remarks>
    JotunForged = 26
}
