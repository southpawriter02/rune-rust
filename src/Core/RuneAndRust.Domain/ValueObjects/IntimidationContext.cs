// ------------------------------------------------------------------------------
// <copyright file="IntimidationContext.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Encapsulates all factors affecting an intimidation attempt.
// Part of v0.15.3d Intimidation System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Extensions;

/// <summary>
/// Encapsulates all factors affecting an intimidation attempt.
/// </summary>
/// <remarks>
/// <para>
/// The intimidation context captures the target's resistance tier, the player's
/// chosen approach (MIGHT or WILL), relative strength, reputation effects,
/// equipment display, and ally/backup presence. Unlike other social interactions,
/// intimidation has no DC modifiers - all factors affect dice pools instead.
/// </para>
/// <para>
/// Key features:
/// </para>
/// <list type="bullet">
///   <item>
///     <description>Dual attribute choice: MIGHT (Physical) or WILL (Mental)</description>
///   </item>
///   <item>
///     <description>Base DC from target type only (8/12/16/20/24)</description>
///   </item>
///   <item>
///     <description>All modifiers affect dice pools, not DC</description>
///   </item>
///   <item>
///     <description>Mandatory reputation cost (Cost of Fear)</description>
///   </item>
/// </list>
/// <para>
/// This is an immutable value object created via the IntimidationService.
/// </para>
/// </remarks>
/// <param name="TargetId">The NPC being intimidated.</param>
/// <param name="TargetType">The resistance tier of the target NPC, determining base DC.</param>
/// <param name="TargetFactionId">The faction ID affected by this intimidation attempt.</param>
/// <param name="Approach">The approach chosen (Physical=MIGHT or Mental=WILL).</param>
/// <param name="RelativeStrength">Power dynamic based on level comparison.</param>
/// <param name="HasReputation">Whether player has [Honored] or [Feared] status (+1d10).</param>
/// <param name="WieldingArtifact">Whether player is visibly wielding an [Artifact] (+1d10).</param>
/// <param name="IntimidatingAlly">Whether player has an intimidating ally present (+1d10).</param>
/// <param name="TargetHasBackup">Whether the NPC has backup nearby (NPC +1d10).</param>
/// <param name="PlayerLevel">The player's current level for strength comparison.</param>
/// <param name="NpcLevel">The NPC's level for strength comparison.</param>
public readonly record struct IntimidationContext(
    string TargetId,
    IntimidationTarget TargetType,
    string TargetFactionId,
    IntimidationApproach Approach,
    RelativeStrength RelativeStrength,
    bool HasReputation,
    bool WieldingArtifact,
    bool IntimidatingAlly,
    bool TargetHasBackup,
    int PlayerLevel,
    int NpcLevel)
{
    /// <summary>
    /// Gets the base DC from target resistance tier.
    /// </summary>
    /// <remarks>
    /// DC values: Coward (8), Common (12), Veteran (16), Elite (20), FactionLeader (24).
    /// </remarks>
    public int BaseDc => TargetType.GetBaseDc();

    /// <summary>
    /// Gets the effective DC for this intimidation attempt.
    /// </summary>
    /// <remarks>
    /// Intimidation uses base DC from target type only.
    /// All other factors affect dice pools, not DC.
    /// </remarks>
    public int EffectiveDc => BaseDc;

    /// <summary>
    /// Gets the attribute name used for this intimidation.
    /// </summary>
    /// <remarks>
    /// Returns "MIGHT" for Physical approach, "WILL" for Mental approach.
    /// </remarks>
    public string AttributeName => Approach.GetAttributeName();

    /// <summary>
    /// Gets the bonus dice added to the player's pool.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Player bonus sources:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>PlayerStronger relative strength: +1d10</description></item>
    ///   <item><description>[Honored] or [Feared] reputation: +1d10</description></item>
    ///   <item><description>Wielding visible [Artifact]: +1d10</description></item>
    ///   <item><description>Intimidating ally present: +1d10</description></item>
    /// </list>
    /// </remarks>
    public int PlayerBonusDice
    {
        get
        {
            var bonus = 0;

            // Relative strength bonus
            if (RelativeStrength == RelativeStrength.PlayerStronger)
            {
                bonus += 1;
            }

            // [Honored] or [Feared] reputation
            if (HasReputation)
            {
                bonus += 1;
            }

            // Wielding visible [Artifact]
            if (WieldingArtifact)
            {
                bonus += 1;
            }

            // Intimidating ally present
            if (IntimidatingAlly)
            {
                bonus += 1;
            }

            return bonus;
        }
    }

    /// <summary>
    /// Gets the bonus dice added to the NPC's resistance pool.
    /// </summary>
    /// <remarks>
    /// <para>
    /// NPC bonus sources:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>PlayerWeaker relative strength: +1d10</description></item>
    ///   <item><description>NPC has backup nearby: +1d10</description></item>
    /// </list>
    /// </remarks>
    public int NpcBonusDice
    {
        get
        {
            var bonus = 0;

            // Relative strength penalty (NPC bonus)
            if (RelativeStrength == RelativeStrength.PlayerWeaker)
            {
                bonus += 1;
            }

            // NPC has backup nearby
            if (TargetHasBackup)
            {
                bonus += 1;
            }

            return bonus;
        }
    }

    /// <summary>
    /// Gets whether this target is likely to resist on fumble.
    /// </summary>
    /// <remarks>
    /// Veteran, Elite, and FactionLeader targets are more likely to
    /// trigger [Challenge Accepted] on fumble.
    /// </remarks>
    public bool TargetLikelyToResist => TargetType.IsLikelyToResist();

    /// <summary>
    /// Gets the target's typical response to successful intimidation.
    /// </summary>
    public string TargetTypicalResponse => TargetType.GetTypicalResponse();

    /// <summary>
    /// Gets whether this intimidation attempt has high risk.
    /// </summary>
    /// <remarks>
    /// High risk when targeting Veteran or higher tiers (who are likely to resist
    /// and trigger [Challenge Accepted] on fumble), or when the player is
    /// outmatched (PlayerWeaker).
    /// </remarks>
    public bool IsHighRisk =>
        TargetType >= IntimidationTarget.Veteran ||
        RelativeStrength == RelativeStrength.PlayerWeaker;

    /// <summary>
    /// Gets whether the player has the advantage in this encounter.
    /// </summary>
    public bool PlayerHasAdvantage => PlayerBonusDice > NpcBonusDice;

    /// <summary>
    /// Gets whether the NPC has the advantage in this encounter.
    /// </summary>
    public bool NpcHasAdvantage => NpcBonusDice > PlayerBonusDice;

    /// <summary>
    /// Builds a detailed modifier breakdown for display.
    /// </summary>
    /// <returns>A multi-line string showing all modifiers.</returns>
    public string ToModifierBreakdown()
    {
        var lines = new List<string>
        {
            $"Intimidation: {Approach.GetDisplayName()}",
            $"Target: {TargetId} ({TargetType.GetDisplayName()}, DC {BaseDc})",
            $"Faction: {TargetFactionId}",
            $"Relative Strength: {RelativeStrength.GetShortDescription()}"
        };

        // Player modifiers
        if (PlayerBonusDice > 0)
        {
            lines.Add("Player Bonuses:");
            if (RelativeStrength == RelativeStrength.PlayerStronger)
            {
                lines.Add("  Outmatches Target: +1d10");
            }

            if (HasReputation)
            {
                lines.Add("  [Honored/Feared] Reputation: +1d10");
            }

            if (WieldingArtifact)
            {
                lines.Add("  Wielding [Artifact]: +1d10");
            }

            if (IntimidatingAlly)
            {
                lines.Add("  Intimidating Ally: +1d10");
            }

            lines.Add($"  Total Player Bonus: +{PlayerBonusDice}d10");
        }

        // NPC modifiers
        if (NpcBonusDice > 0)
        {
            lines.Add("NPC Resistance Bonuses:");
            if (RelativeStrength == RelativeStrength.PlayerWeaker)
            {
                lines.Add("  Stronger Than Player: +1d10");
            }

            if (TargetHasBackup)
            {
                lines.Add("  Has Backup Nearby: +1d10");
            }

            lines.Add($"  Total NPC Bonus: +{NpcBonusDice}d10");
        }

        lines.Add($"Effective DC: {EffectiveDc}");

        if (IsHighRisk)
        {
            lines.Add("WARNING: High-risk intimidation attempt");
        }

        if (TargetLikelyToResist)
        {
            lines.Add("NOTE: Target likely to resist on fumble ([Challenge Accepted])");
        }

        return string.Join(Environment.NewLine, lines);
    }

    /// <summary>
    /// Creates a minimal context for testing purposes.
    /// </summary>
    /// <param name="targetId">The target NPC ID.</param>
    /// <param name="targetType">The target resistance tier.</param>
    /// <param name="factionId">The faction ID.</param>
    /// <returns>A minimal IntimidationContext for testing.</returns>
    public static IntimidationContext CreateMinimal(
        string targetId,
        IntimidationTarget targetType = IntimidationTarget.Common,
        string factionId = "test-faction")
    {
        return new IntimidationContext(
            TargetId: targetId,
            TargetType: targetType,
            TargetFactionId: factionId,
            Approach: IntimidationApproach.Physical,
            RelativeStrength: RelativeStrength.Equal,
            HasReputation: false,
            WieldingArtifact: false,
            IntimidatingAlly: false,
            TargetHasBackup: false,
            PlayerLevel: 5,
            NpcLevel: 5);
    }

    /// <summary>
    /// Creates a context for testing purposes with additional parameters.
    /// </summary>
    /// <param name="targetId">The target NPC ID.</param>
    /// <param name="targetType">The target resistance tier.</param>
    /// <param name="targetFactionId">The faction ID.</param>
    /// <param name="approach">The intimidation approach (Physical/Mental).</param>
    /// <param name="relativeStrength">The relative power dynamic.</param>
    /// <returns>An IntimidationContext for testing.</returns>
    public static IntimidationContext CreateMinimal(
        string targetId,
        IntimidationTarget targetType,
        string targetFactionId,
        IntimidationApproach approach,
        RelativeStrength relativeStrength)
    {
        return new IntimidationContext(
            TargetId: targetId,
            TargetType: targetType,
            TargetFactionId: targetFactionId,
            Approach: approach,
            RelativeStrength: relativeStrength,
            HasReputation: false,
            WieldingArtifact: false,
            IntimidatingAlly: false,
            TargetHasBackup: false,
            PlayerLevel: 5,
            NpcLevel: 5);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        var playerMod = PlayerBonusDice > 0 ? $"+{PlayerBonusDice}d10" : "no bonus";
        var npcMod = NpcBonusDice > 0 ? $"NPC +{NpcBonusDice}d10" : "";
        var modifiers = npcMod.Length > 0 ? $", {npcMod}" : "";

        return $"Intimidate {TargetId}: {Approach.GetShortName()} (DC {EffectiveDc}, {playerMod}{modifiers})";
    }
}
