// ------------------------------------------------------------------------------
// <copyright file="ILockpickingService.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Service interface for executing lockpicking attempts as part of the
// System Bypass skill subsystem.
// Part of v0.15.4a Lockpicking System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.Interfaces;

using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Service for executing lockpicking attempts using the System Bypass skill.
/// </summary>
/// <remarks>
/// <para>
/// Implements the lockpicking subsystem of the System Bypass skill.
/// Lockpicking in Aethelgard follows cargo cult mechanics—characters manipulate
/// incomprehensible mechanisms through pattern recognition rather than true understanding.
/// </para>
/// <para>
/// The service handles:
/// <list type="bullet">
///   <item><description>Prerequisite validation (tool requirements, skill availability)</description></item>
///   <item><description>DC calculation (lock type + corruption + jammed status)</description></item>
///   <item><description>Dice pool modification (tool quality bonuses/penalties)</description></item>
///   <item><description>Outcome processing (success, failure, fumble, critical)</description></item>
///   <item><description>Consequence management (mechanism jammed on fumble)</description></item>
///   <item><description>Salvage determination (components on critical success)</description></item>
/// </list>
/// </para>
/// <para>
/// Lockpicking outcomes:
/// <list type="bullet">
///   <item><description>Success: Lock opens normally</description></item>
///   <item><description>Critical Success: Lock opens + salvage component</description></item>
///   <item><description>Failure: Lock remains closed, can retry</description></item>
///   <item><description>Fumble: [Mechanism Jammed] - DC +2 permanent, key useless</description></item>
/// </list>
/// </para>
/// </remarks>
public interface ILockpickingService
{
    // -------------------------------------------------------------------------
    // Core Lockpicking Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Attempts to pick a lock.
    /// </summary>
    /// <param name="player">The player attempting to pick the lock.</param>
    /// <param name="lockContext">Context containing lock and tool information.</param>
    /// <returns>The result of the lockpicking attempt.</returns>
    /// <remarks>
    /// <para>
    /// The attempt process:
    /// <list type="number">
    ///   <item><description>Validate prerequisites (tools, skill availability)</description></item>
    ///   <item><description>Build skill context with modifiers</description></item>
    ///   <item><description>Perform skill check against effective DC</description></item>
    ///   <item><description>Process outcome and apply consequences</description></item>
    ///   <item><description>Return result with updated lock state</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Thrown when player or lockContext is null.
    /// </exception>
    LockpickingResult AttemptLockpick(Player player, LockContext lockContext);

    /// <summary>
    /// Determines if a player can attempt to pick a specific lock.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <param name="lockContext">The lock context.</param>
    /// <returns>True if the attempt can be made; otherwise false.</returns>
    /// <remarks>
    /// <para>
    /// An attempt may be blocked if:
    /// <list type="bullet">
    ///   <item><description>Lock DC ≥ 10 and player has no tools (bare hands)</description></item>
    ///   <item><description>Tools are insufficient for the lock type</description></item>
    ///   <item><description>Player lacks the lockpicking skill entirely</description></item>
    ///   <item><description>A blocking fumble consequence exists on the lock</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    bool CanAttempt(Player player, LockContext lockContext);

    /// <summary>
    /// Gets the reason why an attempt cannot be made.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <param name="lockContext">The lock context.</param>
    /// <returns>Reason string if cannot attempt; null if can attempt.</returns>
    /// <remarks>
    /// <para>
    /// Use this method to provide feedback to the player about why their
    /// lockpicking attempt is blocked. Returns null if the attempt is allowed.
    /// </para>
    /// </remarks>
    string? GetAttemptBlockedReason(Player player, LockContext lockContext);

    // -------------------------------------------------------------------------
    // Salvage Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets possible salvage components for a lock type.
    /// </summary>
    /// <param name="lockType">The type of lock.</param>
    /// <returns>List of possible salvageable components with their rarities.</returns>
    /// <remarks>
    /// <para>
    /// Salvage availability by lock type:
    /// <list type="bullet">
    ///   <item><description>ImprovisedLatch: Wire Bundle, Small Spring (Common)</description></item>
    ///   <item><description>SimpleLock/StandardLock: High-Tension Spring, Pin Set (Uncommon)</description></item>
    ///   <item><description>ComplexLock/MasterLock: Circuit Fragment, Power Cell Fragment (Rare)</description></item>
    ///   <item><description>JotunForged: Encryption Chip, Biometric Sensor (Legendary)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    IReadOnlyList<SalvageableComponent> GetPossibleSalvage(LockType lockType);

    /// <summary>
    /// Randomly selects a salvage component for a lock type.
    /// </summary>
    /// <param name="lockType">The type of lock.</param>
    /// <returns>A randomly selected component, or null if no salvage available.</returns>
    /// <remarks>
    /// Called internally during critical success processing to determine
    /// what component the player salvages from the lock.
    /// </remarks>
    SalvageableComponent? SelectRandomSalvage(LockType lockType);

    // -------------------------------------------------------------------------
    // Information Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the effective DC for a lockpicking attempt.
    /// </summary>
    /// <param name="lockContext">The lock context.</param>
    /// <returns>The calculated effective DC.</returns>
    /// <remarks>
    /// <para>
    /// DC calculation: BaseDC + CorruptionModifier + JammedModifier
    /// Note: Tool quality affects dice pool, not DC.
    /// </para>
    /// </remarks>
    int GetEffectiveDc(LockContext lockContext);

    /// <summary>
    /// Gets the dice pool modifier for a lockpicking attempt.
    /// </summary>
    /// <param name="lockContext">The lock context.</param>
    /// <returns>The total dice modifier from tools and context.</returns>
    /// <remarks>
    /// <para>
    /// Dice modifier is primarily determined by tool quality:
    /// <list type="bullet">
    ///   <item><description>BareHands: -2d10</description></item>
    ///   <item><description>Improvised: +0</description></item>
    ///   <item><description>Proper: +1d10</description></item>
    ///   <item><description>Masterwork: +2d10</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    int GetDiceModifier(LockContext lockContext);

    /// <summary>
    /// Gets an estimate of success probability for a lockpicking attempt.
    /// </summary>
    /// <param name="player">The player attempting the lock.</param>
    /// <param name="lockContext">The lock context.</param>
    /// <returns>
    /// An estimated success rate as a percentage (0-100), or null if cannot be estimated.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This is a rough estimate for UI display purposes. The actual probability
    /// depends on dice rolls and cannot be precisely calculated.
    /// </para>
    /// </remarks>
    int? EstimateSuccessRate(Player player, LockContext lockContext);

    /// <summary>
    /// Gets descriptive text about the lock difficulty.
    /// </summary>
    /// <param name="lockContext">The lock context.</param>
    /// <returns>A human-readable difficulty assessment.</returns>
    string GetDifficultyDescription(LockContext lockContext);
}
