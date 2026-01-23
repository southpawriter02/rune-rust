// ------------------------------------------------------------------------------
// <copyright file="LockContext.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Value object encapsulating all context needed for a lockpicking attempt,
// including lock type, corruption level, tool quality, and attempt history.
// Part of v0.15.4a Lockpicking System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Extensions;

/// <summary>
/// Encapsulates all context needed for a lockpicking attempt.
/// </summary>
/// <remarks>
/// <para>
/// Contains information about the lock, tools being used, environmental factors,
/// and attempt history to calculate effective difficulty and dice modifiers.
/// </para>
/// <para>
/// The effective DC is calculated as:
/// <code>
/// EffectiveDc = BaseDc + CorruptionModifier + JammedModifier
/// </code>
/// Note: Previous attempts are tracked separately for failure escalation mechanics
/// but are not included in EffectiveDc by default.
/// </para>
/// <para>
/// The dice modifier is determined by tool quality:
/// <list type="bullet">
///   <item><description>Bare Hands: -2d10</description></item>
///   <item><description>Improvised Tools: +0</description></item>
///   <item><description>Proper Lockpicks: +1d10</description></item>
///   <item><description>Masterwork Tools: +2d10</description></item>
/// </list>
/// </para>
/// </remarks>
public sealed class LockContext
{
    // -------------------------------------------------------------------------
    // Constants
    // -------------------------------------------------------------------------

    /// <summary>
    /// DC modifier applied when a lock mechanism is jammed.
    /// </summary>
    public const int JammedDcPenalty = 2;

    /// <summary>
    /// Minimum DC that requires tools to attempt.
    /// </summary>
    public const int ToolRequirementThreshold = 10;

    // -------------------------------------------------------------------------
    // Core Properties
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the unique identifier of the lock entity (for tracking consequences).
    /// </summary>
    public string LockId { get; }

    /// <summary>
    /// Gets the type/classification of the lock.
    /// </summary>
    public LockType LockType { get; }

    /// <summary>
    /// Gets the corruption level affecting the lock.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Corruption levels and their DC modifiers:
    /// <list type="bullet">
    ///   <item><description>Normal: +0 DC</description></item>
    ///   <item><description>Glitched: +2 DC</description></item>
    ///   <item><description>Blighted: +4 DC</description></item>
    ///   <item><description>Resonance: +6 DC</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public CorruptionTier CorruptionLevel { get; }

    /// <summary>
    /// Gets the quality of tools being used for the attempt.
    /// </summary>
    public ToolQuality ToolQuality { get; }

    /// <summary>
    /// Gets the number of previous failed attempts on this lock by this character.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Each failed attempt may increase difficulty for subsequent attempts
    /// depending on game configuration (failure escalation rules).
    /// </para>
    /// </remarks>
    public int PreviousAttempts { get; }

    /// <summary>
    /// Gets whether the lock mechanism has been jammed by a previous fumble.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A jammed lock has DC permanently increased by 2 and cannot be
    /// opened with its associated key.
    /// </para>
    /// </remarks>
    public bool IsJammed { get; }

    /// <summary>
    /// Gets the display name of the lock for UI messages.
    /// </summary>
    public string? LockName { get; }

    // -------------------------------------------------------------------------
    // Computed DC Properties
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the base difficulty class from the lock type.
    /// </summary>
    public int BaseDc => (int)LockType;

    /// <summary>
    /// Gets the DC modifier from the corruption level.
    /// </summary>
    public int CorruptionDcModifier => CorruptionLevel switch
    {
        CorruptionTier.Normal => 0,
        CorruptionTier.Glitched => 2,
        CorruptionTier.Blighted => 4,
        CorruptionTier.Resonance => 6,
        _ => 0
    };

    /// <summary>
    /// Gets the DC modifier from the jammed mechanism status.
    /// </summary>
    public int JammedDcModifier => IsJammed ? JammedDcPenalty : 0;

    /// <summary>
    /// Gets the total effective difficulty class for the lockpicking check.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Sum of base DC, corruption modifier, and jammed modifier.
    /// Previous attempts are NOT included here; they are tracked separately
    /// for failure escalation mechanics if enabled in game configuration.
    /// </para>
    /// </remarks>
    public int EffectiveDc => BaseDc + CorruptionDcModifier + JammedDcModifier;

    // -------------------------------------------------------------------------
    // Computed Dice Properties
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the dice pool modifier from tool quality.
    /// </summary>
    public int ToolDiceModifier => ToolQuality.GetDiceModifier();

    // -------------------------------------------------------------------------
    // Tool Requirement Properties
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets whether tools are required to attempt this lock.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Locks with DC 10+ (SimpleLock and above) require at least improvised tools.
    /// Only ImprovisedLatch (DC 6) can be attempted bare-handed.
    /// </para>
    /// </remarks>
    public bool RequiresTools => BaseDc >= ToolRequirementThreshold;

    /// <summary>
    /// Gets whether the current tool quality meets the minimum requirements.
    /// </summary>
    public bool HasRequiredTools => !RequiresTools || ToolQuality >= ToolQuality.Improvised;

    /// <summary>
    /// Gets the minimum tool quality required for this lock.
    /// </summary>
    public ToolQuality MinimumToolQuality => LockType.GetMinimumToolQuality();

    /// <summary>
    /// Gets whether the current tools meet the recommended quality for this lock.
    /// </summary>
    public bool HasRecommendedTools => ToolQuality >= MinimumToolQuality;

    // -------------------------------------------------------------------------
    // Display Properties
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the display name of the lock, defaulting to the lock type if no name is set.
    /// </summary>
    public string DisplayName => LockName ?? LockType.GetDisplayName();

    /// <summary>
    /// Gets the difficulty rating string for display.
    /// </summary>
    public string DifficultyRating => LockType.GetDifficultyRating();

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a new lock context.
    /// </summary>
    /// <param name="lockId">Unique identifier for the lock.</param>
    /// <param name="lockType">Classification of the lock.</param>
    /// <param name="corruptionLevel">Corruption tier affecting the lock.</param>
    /// <param name="toolQuality">Quality of tools being used.</param>
    /// <param name="previousAttempts">Number of prior failed attempts.</param>
    /// <param name="isJammed">Whether the mechanism is jammed.</param>
    /// <param name="lockName">Optional display name.</param>
    /// <exception cref="ArgumentNullException">Thrown when lockId is null.</exception>
    /// <exception cref="ArgumentException">Thrown when lockId is empty or whitespace.</exception>
    public LockContext(
        string lockId,
        LockType lockType,
        CorruptionTier corruptionLevel = CorruptionTier.Normal,
        ToolQuality toolQuality = ToolQuality.Improvised,
        int previousAttempts = 0,
        bool isJammed = false,
        string? lockName = null)
    {
        ArgumentNullException.ThrowIfNull(lockId);
        if (string.IsNullOrWhiteSpace(lockId))
            throw new ArgumentException("Lock ID cannot be empty or whitespace.", nameof(lockId));

        LockId = lockId;
        LockType = lockType;
        CorruptionLevel = corruptionLevel;
        ToolQuality = toolQuality;
        PreviousAttempts = Math.Max(0, previousAttempts);
        IsJammed = isJammed;
        LockName = lockName;
    }

    // -------------------------------------------------------------------------
    // Mutation Methods (Create New Instances)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a copy of this context with the jammed flag set.
    /// </summary>
    /// <returns>A new LockContext with IsJammed = true.</returns>
    public LockContext WithJammed()
    {
        return new LockContext(
            LockId,
            LockType,
            CorruptionLevel,
            ToolQuality,
            PreviousAttempts,
            isJammed: true,
            LockName);
    }

    /// <summary>
    /// Creates a copy of this context with incremented previous attempts.
    /// </summary>
    /// <returns>A new LockContext with PreviousAttempts + 1.</returns>
    public LockContext WithFailedAttempt()
    {
        return new LockContext(
            LockId,
            LockType,
            CorruptionLevel,
            ToolQuality,
            PreviousAttempts + 1,
            IsJammed,
            LockName);
    }

    /// <summary>
    /// Creates a copy of this context with a different tool quality.
    /// </summary>
    /// <param name="newToolQuality">The new tool quality.</param>
    /// <returns>A new LockContext with the updated tool quality.</returns>
    public LockContext WithToolQuality(ToolQuality newToolQuality)
    {
        return new LockContext(
            LockId,
            LockType,
            CorruptionLevel,
            newToolQuality,
            PreviousAttempts,
            IsJammed,
            LockName);
    }

    // -------------------------------------------------------------------------
    // Conversion Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Converts this context to a SkillContext for skill check integration.
    /// </summary>
    /// <returns>A SkillContext with equipment and environment modifiers applied.</returns>
    /// <remarks>
    /// <para>
    /// The conversion creates modifiers for:
    /// <list type="bullet">
    ///   <item><description>Tool quality as equipment modifier (dice bonus/penalty)</description></item>
    ///   <item><description>Corruption level as environment modifier (DC increase)</description></item>
    ///   <item><description>Jammed status as situational modifier (DC increase)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public SkillContext ToSkillContext()
    {
        var equipmentModifiers = new List<EquipmentModifier>();
        var situationalModifiers = new List<SituationalModifier>();
        var environmentModifiers = new List<EnvironmentModifier>();

        // Add tool modifier as equipment
        if (ToolDiceModifier != 0)
        {
            var toolName = ToolQuality.GetDisplayName();
            equipmentModifiers.Add(new EquipmentModifier(
                EquipmentId: $"tool-{ToolQuality.ToString().ToLowerInvariant()}",
                EquipmentName: toolName,
                DiceModifier: ToolDiceModifier,
                DcModifier: 0,
                EquipmentCategory: EquipmentCategory.Tool,
                RequiredForCheck: RequiresTools));
        }

        // Add corruption modifier as environment
        if (CorruptionLevel != CorruptionTier.Normal)
        {
            environmentModifiers.Add(EnvironmentModifier.FromCorruption(CorruptionLevel));
        }

        // Add jammed modifier as situational
        if (IsJammed)
        {
            situationalModifiers.Add(new SituationalModifier(
                ModifierId: "mechanism-jammed",
                Name: "Mechanism Jammed",
                DiceModifier: 0,
                DcModifier: JammedDcPenalty,
                Source: "Previous fumble damaged the lock",
                Duration: ModifierDuration.Persistent,
                IsStackable: false,
                Description: "The lock mechanism is jammed from a previous failed attempt."));
        }

        return new SkillContext(
            equipmentModifiers.AsReadOnly(),
            situationalModifiers.AsReadOnly(),
            environmentModifiers.AsReadOnly(),
            Array.Empty<TargetModifier>(),
            Array.Empty<string>());
    }

    // -------------------------------------------------------------------------
    // Validation Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the reason why an attempt cannot be made, or null if attempt is allowed.
    /// </summary>
    /// <returns>A reason string if blocked; null if attempt is allowed.</returns>
    public string? GetBlockedReason()
    {
        if (RequiresTools && ToolQuality == ToolQuality.BareHands)
        {
            return $"This {LockType.GetDisplayName()} requires tools to attempt. " +
                   "A [Tinker's Toolkit] or at least improvised tools are needed.";
        }

        if (RequiresTools && !HasRequiredTools)
        {
            return $"Your {ToolQuality.GetDisplayName()} are insufficient for this " +
                   $"{LockType.GetDisplayName()}. Better tools are required.";
        }

        return null;
    }

    // -------------------------------------------------------------------------
    // Display Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns a human-readable description of the lock context.
    /// </summary>
    /// <returns>A formatted description string.</returns>
    public override string ToString()
    {
        var parts = new List<string>
        {
            $"{DisplayName} (DC {EffectiveDc})"
        };

        if (CorruptionLevel != CorruptionTier.Normal)
            parts.Add($"[{CorruptionLevel}]");

        if (IsJammed)
            parts.Add("[Jammed]");

        var diceModStr = ToolDiceModifier >= 0 ? $"+{ToolDiceModifier}" : $"{ToolDiceModifier}";
        parts.Add($"Tools: {ToolQuality.GetDisplayName()} ({diceModStr}d10)");

        if (PreviousAttempts > 0)
            parts.Add($"{PreviousAttempts} prior attempt(s)");

        return string.Join(", ", parts);
    }

    /// <summary>
    /// Returns a compact summary for logging.
    /// </summary>
    /// <returns>A log-friendly string representation.</returns>
    public string ToLogString()
    {
        return $"Lock[{LockId}:{LockType}] DC={EffectiveDc} " +
               $"tools={ToolQuality} jammed={IsJammed} attempts={PreviousAttempts}";
    }

    // -------------------------------------------------------------------------
    // Factory Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a simple lock context with default settings.
    /// </summary>
    /// <param name="lockId">Unique identifier for the lock.</param>
    /// <param name="lockType">Classification of the lock.</param>
    /// <param name="toolQuality">Quality of tools to use.</param>
    /// <returns>A new LockContext with default corruption and no previous attempts.</returns>
    public static LockContext Create(string lockId, LockType lockType, ToolQuality toolQuality = ToolQuality.Proper)
    {
        return new LockContext(lockId, lockType, toolQuality: toolQuality);
    }

    /// <summary>
    /// Creates a corrupted lock context.
    /// </summary>
    /// <param name="lockId">Unique identifier for the lock.</param>
    /// <param name="lockType">Classification of the lock.</param>
    /// <param name="corruption">Corruption tier.</param>
    /// <param name="toolQuality">Quality of tools to use.</param>
    /// <returns>A new LockContext with the specified corruption.</returns>
    public static LockContext CreateCorrupted(
        string lockId,
        LockType lockType,
        CorruptionTier corruption,
        ToolQuality toolQuality = ToolQuality.Proper)
    {
        return new LockContext(lockId, lockType, corruption, toolQuality);
    }
}
