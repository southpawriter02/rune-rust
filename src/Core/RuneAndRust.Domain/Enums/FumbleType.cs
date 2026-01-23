namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Categorizes types of fumble consequences by skill and effect.
/// </summary>
/// <remarks>
/// <para>
/// Each fumble type is associated with specific skills and has defined mechanical effects:
/// <list type="bullet">
///   <item><description>Blocking effects: Prevent skill checks entirely</description></item>
///   <item><description>Penalty effects: Apply dice or DC modifiers</description></item>
///   <item><description>Instant effects: Trigger immediate events (damage, combat)</description></item>
/// </list>
/// </para>
/// </remarks>
public enum FumbleType
{
    /// <summary>
    /// Rhetoric/Persuasion: Trust completely lost with target NPC.
    /// Persuasion checks are locked until recovery condition met.
    /// </summary>
    TrustShattered = 0,

    /// <summary>
    /// Rhetoric/Deception: Lie has been exposed to the target.
    /// -2d10 penalty to Deception checks with target's faction.
    /// </summary>
    LieExposed = 1,

    /// <summary>
    /// Rhetoric/Intimidation: Target accepts the challenge.
    /// Immediate combat initiated, enemy gains +1d10 to attacks.
    /// </summary>
    ChallengeAccepted = 2,

    /// <summary>
    /// System Bypass/Lockpicking: Lock mechanism has been jammed.
    /// Lock DC permanently +2, key cannot be used on this lock.
    /// </summary>
    MechanismJammed = 3,

    /// <summary>
    /// System Bypass/Terminal Hacking: Terminal has locked out the user.
    /// Terminal disabled and security alert triggered.
    /// </summary>
    SystemLockout = 4,

    /// <summary>
    /// System Bypass/Trap Disarmament: Trap triggered during disarmament.
    /// Trap immediately executes on the disarmer.
    /// </summary>
    ForcedExecution = 5,

    /// <summary>
    /// Acrobatics/Climbing: Lost grip and fell.
    /// Fall from current height, take fall damage plus stress.
    /// </summary>
    TheSlip = 6,

    /// <summary>
    /// Acrobatics/Leaping: Catastrophic landing failure.
    /// Bonus fall damage plus [Disoriented] status for 2 rounds.
    /// </summary>
    TheLongFall = 7,

    /// <summary>
    /// Acrobatics/Stealth: Alerted all nearby enemies.
    /// All enemies in adjacent rooms converge on location.
    /// </summary>
    SystemWideAlert = 8
}

/// <summary>
/// Extension methods for <see cref="FumbleType"/>.
/// </summary>
public static class FumbleTypeExtensions
{
    /// <summary>
    /// Determines if this fumble type blocks all targets or only the specific target.
    /// </summary>
    public static bool BlocksAllTargets(this FumbleType fumbleType)
    {
        return fumbleType switch
        {
            FumbleType.TrustShattered => false, // Only blocks specific NPC
            FumbleType.LieExposed => false,     // Only affects specific faction
            FumbleType.ChallengeAccepted => false, // Only affects specific enemy
            FumbleType.MechanismJammed => false, // Only affects specific lock
            FumbleType.SystemLockout => false,  // Only affects specific terminal
            FumbleType.ForcedExecution => false, // Only affects specific trap
            FumbleType.TheSlip => false,        // Instant effect
            FumbleType.TheLongFall => false,    // Instant effect
            FumbleType.SystemWideAlert => true, // Affects all enemies in area
            _ => false
        };
    }

    /// <summary>
    /// Determines if this fumble type is an instant effect (no duration).
    /// </summary>
    public static bool IsInstant(this FumbleType fumbleType)
    {
        return fumbleType switch
        {
            FumbleType.ForcedExecution => true,
            FumbleType.TheSlip => true,
            FumbleType.TheLongFall => false, // Has status duration
            _ => false
        };
    }

    /// <summary>
    /// Gets the associated skill ID for this fumble type.
    /// </summary>
    public static string GetAssociatedSkillId(this FumbleType fumbleType)
    {
        return fumbleType switch
        {
            FumbleType.TrustShattered => "persuasion",
            FumbleType.LieExposed => "deception",
            FumbleType.ChallengeAccepted => "intimidation",
            FumbleType.MechanismJammed => "lockpicking",
            FumbleType.SystemLockout => "hacking",
            FumbleType.ForcedExecution => "trap-disarmament",
            FumbleType.TheSlip => "climbing",
            FumbleType.TheLongFall => "leaping",
            FumbleType.SystemWideAlert => "stealth",
            _ => "unknown"
        };
    }

    /// <summary>
    /// Gets a human-readable name for this fumble type.
    /// </summary>
    public static string GetDisplayName(this FumbleType fumbleType)
    {
        return fumbleType switch
        {
            FumbleType.TrustShattered => "Trust Shattered",
            FumbleType.LieExposed => "Lie Exposed",
            FumbleType.ChallengeAccepted => "Challenge Accepted",
            FumbleType.MechanismJammed => "Mechanism Jammed",
            FumbleType.SystemLockout => "System Lockout",
            FumbleType.ForcedExecution => "Forced Execution",
            FumbleType.TheSlip => "The Slip",
            FumbleType.TheLongFall => "The Long Fall",
            FumbleType.SystemWideAlert => "System-Wide Alert",
            _ => "Unknown Fumble"
        };
    }
}
