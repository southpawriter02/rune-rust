namespace RuneAndRust.Domain.Entities;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Tracks a lasting consequence from a fumbled (catastrophically failed) skill check.
/// </summary>
/// <remarks>
/// <para>
/// Fumble consequences persist beyond the immediate skill check, affecting future
/// interactions until they expire or their recovery condition is met.
/// </para>
/// <para>
/// Examples:
/// <list type="bullet">
///   <item><description>TrustShattered: Persuasion locked with an NPC until a favor is completed</description></item>
///   <item><description>MechanismJammed: Lock DC permanently +2 for this specific lock</description></item>
///   <item><description>SystemLockout: Terminal disabled until admin reset</description></item>
/// </list>
/// </para>
/// </remarks>
public sealed class FumbleConsequence
{
    /// <summary>
    /// Gets the unique identifier for this consequence.
    /// </summary>
    public string ConsequenceId { get; }

    /// <summary>
    /// Gets the identifier of the character who fumbled.
    /// </summary>
    public string CharacterId { get; }

    /// <summary>
    /// Gets the identifier of the skill that was fumbled.
    /// </summary>
    public string SkillId { get; }

    /// <summary>
    /// Gets the type of fumble consequence.
    /// </summary>
    public FumbleType ConsequenceType { get; }

    /// <summary>
    /// Gets the optional identifier of the target affected by the consequence.
    /// </summary>
    /// <remarks>
    /// This may be an NPC ID, device ID, location ID, or null for area-wide effects.
    /// </remarks>
    public string? TargetId { get; }

    /// <summary>
    /// Gets or sets a value indicating whether this consequence is currently active.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Gets the timestamp when this consequence was applied.
    /// </summary>
    public DateTime AppliedAt { get; }

    /// <summary>
    /// Gets the optional timestamp when this consequence expires automatically.
    /// </summary>
    /// <remarks>
    /// Null indicates the consequence does not expire automatically and requires recovery.
    /// </remarks>
    public DateTime? ExpiresAt { get; }

    /// <summary>
    /// Gets the human-readable description of this consequence.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gets the optional condition that must be met to recover from this consequence.
    /// </summary>
    public string? RecoveryCondition { get; }

    /// <summary>
    /// Gets the timestamp when this consequence was deactivated, if applicable.
    /// </summary>
    public DateTime? DeactivatedAt { get; private set; }

    /// <summary>
    /// Gets the reason this consequence was deactivated, if applicable.
    /// </summary>
    public string? DeactivationReason { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FumbleConsequence"/> class.
    /// </summary>
    public FumbleConsequence(
        string consequenceId,
        string characterId,
        string skillId,
        FumbleType consequenceType,
        string? targetId,
        DateTime appliedAt,
        DateTime? expiresAt,
        string description,
        string? recoveryCondition)
    {
        if (string.IsNullOrWhiteSpace(consequenceId))
            throw new ArgumentException("Consequence ID is required.", nameof(consequenceId));
        if (string.IsNullOrWhiteSpace(characterId))
            throw new ArgumentException("Character ID is required.", nameof(characterId));
        if (string.IsNullOrWhiteSpace(skillId))
            throw new ArgumentException("Skill ID is required.", nameof(skillId));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description is required.", nameof(description));

        ConsequenceId = consequenceId;
        CharacterId = characterId;
        SkillId = skillId;
        ConsequenceType = consequenceType;
        TargetId = targetId;
        IsActive = true;
        AppliedAt = appliedAt;
        ExpiresAt = expiresAt;
        Description = description;
        RecoveryCondition = recoveryCondition;
    }

    /// <summary>
    /// Determines if this consequence has expired based on the current time.
    /// </summary>
    /// <param name="currentTime">The current time to check against.</param>
    /// <returns>True if the consequence has expired; otherwise, false.</returns>
    public bool IsExpired(DateTime currentTime)
    {
        return ExpiresAt.HasValue && currentTime >= ExpiresAt.Value;
    }

    /// <summary>
    /// Determines if the recovery condition has been met.
    /// </summary>
    /// <param name="completedConditions">List of condition identifiers that have been completed.</param>
    /// <returns>True if the recovery condition is met; otherwise, false.</returns>
    public bool CanRecover(IEnumerable<string> completedConditions)
    {
        if (string.IsNullOrWhiteSpace(RecoveryCondition))
        {
            return false; // No recovery possible without a condition
        }

        return completedConditions.Contains(RecoveryCondition, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Deactivates this consequence, marking it as resolved.
    /// </summary>
    /// <param name="reason">The reason for deactivation.</param>
    /// <param name="deactivatedAt">The timestamp of deactivation.</param>
    public void Deactivate(string reason, DateTime deactivatedAt)
    {
        if (!IsActive)
        {
            return; // Already deactivated
        }

        IsActive = false;
        DeactivatedAt = deactivatedAt;
        DeactivationReason = reason;
    }

    /// <summary>
    /// Determines if this consequence blocks a skill check against a specific target.
    /// </summary>
    /// <param name="skillId">The skill being used.</param>
    /// <param name="targetId">The target of the skill check.</param>
    /// <returns>True if this consequence blocks the check; otherwise, false.</returns>
    public bool BlocksCheck(string skillId, string? targetId)
    {
        if (!IsActive)
        {
            return false;
        }

        // Must match skill
        if (!string.Equals(SkillId, skillId, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // If consequence has no target, it blocks all checks of this skill
        if (string.IsNullOrWhiteSpace(TargetId))
        {
            return ConsequenceType.BlocksAllTargets();
        }

        // If consequence has a target, only block checks against that target
        return string.Equals(TargetId, targetId, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets any dice penalty applied by this consequence.
    /// </summary>
    /// <returns>The dice pool penalty (negative value) or 0 if no penalty.</returns>
    public int GetDicePenalty()
    {
        if (!IsActive)
        {
            return 0;
        }

        return ConsequenceType switch
        {
            FumbleType.LieExposed => -2,
            FumbleType.ChallengeAccepted => 0, // Enemy gets bonus, not player penalty
            _ => 0
        };
    }

    /// <summary>
    /// Gets any DC modifier applied by this consequence.
    /// </summary>
    /// <returns>The DC modifier (positive = harder) or 0 if no modifier.</returns>
    public int GetDcModifier()
    {
        if (!IsActive)
        {
            return 0;
        }

        return ConsequenceType switch
        {
            FumbleType.MechanismJammed => 2,
            _ => 0
        };
    }
}
