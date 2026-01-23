// ------------------------------------------------------------------------------
// <copyright file="LockTypeExtensions.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Extension methods for LockType enum providing display names, DC calculations,
// and tool requirements.
// Part of v0.15.4a Lockpicking System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.Extensions;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Extension methods for <see cref="LockType"/>.
/// </summary>
public static class LockTypeExtensions
{
    // -------------------------------------------------------------------------
    // Display Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the human-readable display name for a lock type.
    /// </summary>
    /// <param name="lockType">The lock type.</param>
    /// <returns>A human-readable name (e.g., "Standard Lock").</returns>
    public static string GetDisplayName(this LockType lockType)
    {
        return lockType switch
        {
            LockType.ImprovisedLatch => "Improvised Latch",
            LockType.SimpleLock => "Simple Lock",
            LockType.StandardLock => "Standard Lock",
            LockType.ComplexLock => "Complex Lock",
            LockType.MasterLock => "Master Lock",
            LockType.JotunForged => "Jötun-Forged Lock",
            _ => "Unknown Lock"
        };
    }

    /// <summary>
    /// Gets a description of the lock type for flavor text.
    /// </summary>
    /// <param name="lockType">The lock type.</param>
    /// <returns>A descriptive string for the lock type.</returns>
    public static string GetDescription(this LockType lockType)
    {
        return lockType switch
        {
            LockType.ImprovisedLatch =>
                "A makeshift closure held together with wire or bent metal.",
            LockType.SimpleLock =>
                "A basic pin tumbler lock found on residential doors.",
            LockType.StandardLock =>
                "A quality lock with multiple security pins.",
            LockType.ComplexLock =>
                "An advanced mechanism with anti-pick features.",
            LockType.MasterLock =>
                "A masterwork lock crafted by Dvergr artisans.",
            LockType.JotunForged =>
                "An ancient mechanism of incomprehensible complexity from the World Before.",
            _ => "An unknown lock type."
        };
    }

    // -------------------------------------------------------------------------
    // DC Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the base difficulty class for the lock type.
    /// </summary>
    /// <param name="lockType">The lock type.</param>
    /// <returns>The base DC value (enum value).</returns>
    public static int GetBaseDc(this LockType lockType) => (int)lockType;

    /// <summary>
    /// Gets the approximate net successes required to pick this lock type.
    /// </summary>
    /// <param name="lockType">The lock type.</param>
    /// <returns>
    /// Approximate net successes needed. This is a guideline based on
    /// success-counting mechanics where DC/5 roughly equals required successes.
    /// </returns>
    public static int GetApproximateSuccessesRequired(this LockType lockType)
    {
        return lockType switch
        {
            LockType.ImprovisedLatch => 1,
            LockType.SimpleLock => 2,
            LockType.StandardLock => 3,
            LockType.ComplexLock => 4,
            LockType.MasterLock => 4,
            LockType.JotunForged => 5,
            _ => 3
        };
    }

    // -------------------------------------------------------------------------
    // Tool Requirement Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Determines if tools are required to attempt this lock type.
    /// </summary>
    /// <param name="lockType">The lock type.</param>
    /// <returns>
    /// True if tools are required (DC 10+); false for ImprovisedLatch which can
    /// be attempted bare-handed.
    /// </returns>
    public static bool RequiresTools(this LockType lockType)
    {
        return (int)lockType >= 10;
    }

    /// <summary>
    /// Determines if bare-handed attempts are possible on this lock type.
    /// </summary>
    /// <param name="lockType">The lock type.</param>
    /// <returns>True only for ImprovisedLatch; false otherwise.</returns>
    public static bool AllowsBareHands(this LockType lockType)
    {
        return lockType == LockType.ImprovisedLatch;
    }

    /// <summary>
    /// Gets the minimum tool quality required for this lock type.
    /// </summary>
    /// <param name="lockType">The lock type.</param>
    /// <returns>The minimum ToolQuality required.</returns>
    public static ToolQuality GetMinimumToolQuality(this LockType lockType)
    {
        return lockType switch
        {
            LockType.ImprovisedLatch => ToolQuality.BareHands,
            LockType.SimpleLock => ToolQuality.Improvised,
            _ => ToolQuality.Proper
        };
    }

    // -------------------------------------------------------------------------
    // Salvage Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the salvage table identifier for this lock type.
    /// </summary>
    /// <param name="lockType">The lock type.</param>
    /// <returns>The salvage table name for configuration lookup.</returns>
    public static string GetSalvageTable(this LockType lockType)
    {
        return lockType switch
        {
            LockType.ImprovisedLatch => "mechanical-basic",
            LockType.SimpleLock => "mechanical-standard",
            LockType.StandardLock => "mechanical-standard",
            LockType.ComplexLock => "electronic-standard",
            LockType.MasterLock => "electronic-standard",
            LockType.JotunForged => "electronic-rare",
            _ => "mechanical-basic"
        };
    }

    /// <summary>
    /// Determines if this lock type yields mechanical components on salvage.
    /// </summary>
    /// <param name="lockType">The lock type.</param>
    /// <returns>True for ImprovisedLatch through StandardLock.</returns>
    public static bool YieldsMechanicalComponents(this LockType lockType)
    {
        return lockType <= LockType.StandardLock;
    }

    /// <summary>
    /// Determines if this lock type yields electronic components on salvage.
    /// </summary>
    /// <param name="lockType">The lock type.</param>
    /// <returns>True for ComplexLock and above.</returns>
    public static bool YieldsElectronicComponents(this LockType lockType)
    {
        return lockType >= LockType.ComplexLock;
    }

    // -------------------------------------------------------------------------
    // Classification Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Determines if this is a high-security lock (ComplexLock and above).
    /// </summary>
    /// <param name="lockType">The lock type.</param>
    /// <returns>True for ComplexLock, MasterLock, and JotunForged.</returns>
    public static bool IsHighSecurity(this LockType lockType)
    {
        return (int)lockType >= 18;
    }

    /// <summary>
    /// Determines if this is an Old World (Jötun) artifact lock.
    /// </summary>
    /// <param name="lockType">The lock type.</param>
    /// <returns>True only for JotunForged locks.</returns>
    public static bool IsJotunArtifact(this LockType lockType)
    {
        return lockType == LockType.JotunForged;
    }

    /// <summary>
    /// Gets a difficulty rating for display purposes.
    /// </summary>
    /// <param name="lockType">The lock type.</param>
    /// <returns>A rating string (e.g., "Easy", "Moderate", "Hard", "Extreme").</returns>
    public static string GetDifficultyRating(this LockType lockType)
    {
        return lockType switch
        {
            LockType.ImprovisedLatch => "Trivial",
            LockType.SimpleLock => "Easy",
            LockType.StandardLock => "Moderate",
            LockType.ComplexLock => "Hard",
            LockType.MasterLock => "Very Hard",
            LockType.JotunForged => "Extreme",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Gets a color hint for UI display based on difficulty.
    /// </summary>
    /// <param name="lockType">The lock type.</param>
    /// <returns>A color name suggestion (e.g., "green", "yellow", "red").</returns>
    public static string GetColorHint(this LockType lockType)
    {
        return lockType switch
        {
            LockType.ImprovisedLatch => "gray",
            LockType.SimpleLock => "green",
            LockType.StandardLock => "yellow",
            LockType.ComplexLock => "orange",
            LockType.MasterLock => "red",
            LockType.JotunForged => "purple",
            _ => "white"
        };
    }
}
