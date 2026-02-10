// ═══════════════════════════════════════════════════════════════════════════════
// IJotunReaderAbilityService.cs
// Interface defining operations for Jötun-Reader specialization abilities.
// Currently covers Tier 1 operations; extended by future versions for
// Tiers 2–4.
// Version: 0.20.3a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Defines operations for Jötun-Reader specialization abilities.
/// </summary>
/// <remarks>
/// <para>
/// This interface covers Tier 1 abilities introduced in v0.20.3a:
/// </para>
/// <list type="bullet">
///   <item><description>
///     <b>Deep Scan (Active):</b> Bonus dice on machinery/terminal examinations
///   </description></item>
///   <item><description>
///     <b>Pattern Recognition (Passive):</b> Auto-succeed Layer 2 on Jötun tech
///   </description></item>
///   <item><description>
///     <b>Ancient Tongues (Passive):</b> Comprehend Jötun and Dvergr scripts
///   </description></item>
/// </list>
/// <para>
/// Future versions will extend this interface with Tier 2 (v0.20.3b),
/// Tier 3, and Capstone (v0.20.3c) operations.
/// </para>
/// </remarks>
/// <seealso cref="JotunReaderAbilityId"/>
/// <seealso cref="LoreInsightResource"/>
public interface IJotunReaderAbilityService
{
    // ═══════ Deep Scan (Active — Tier 1) ═══════

    /// <summary>
    /// Executes a Deep Scan on the specified target, adding +2d10 to the
    /// examination check for valid machinery/terminal/Jötun tech targets.
    /// </summary>
    /// <param name="targetId">ID of the object to examine.</param>
    /// <param name="targetType">Type description of the target (e.g., "terminal").</param>
    /// <param name="baseRoll">The base d20 roll from the character.</param>
    /// <param name="perceptionModifier">Character's perception modifier.</param>
    /// <returns>
    /// A <see cref="DeepScanResult"/> if the target is valid;
    /// otherwise, <c>null</c>.
    /// </returns>
    DeepScanResult? ExecuteDeepScan(
        Guid targetId,
        string targetType,
        int baseRoll,
        int perceptionModifier);

    /// <summary>
    /// Determines if the specified target type is valid for Deep Scan.
    /// </summary>
    /// <param name="targetType">Target type string to validate.</param>
    /// <returns><c>true</c> if the target can be Deep Scanned.</returns>
    bool IsValidDeepScanTarget(string targetType);

    // ═══════ Pattern Recognition (Passive — Tier 1) ═══════

    /// <summary>
    /// Applies Pattern Recognition to determine if a Jötun tech examination
    /// automatically succeeds at Layer 2 (functional understanding).
    /// </summary>
    /// <param name="targetType">Type description of the target.</param>
    /// <returns><c>true</c> if the target is Jötun technology and auto-succeeds.</returns>
    bool ApplyPatternRecognition(string targetType);

    /// <summary>
    /// Determines if the specified target type qualifies as Jötun technology
    /// for Pattern Recognition purposes.
    /// </summary>
    /// <param name="targetType">Target type string to check.</param>
    /// <returns><c>true</c> if the target is classified as Jötun technology.</returns>
    bool IsJotunTechnology(string targetType);

    // ═══════ Ancient Tongues (Passive — Tier 1) ═══════

    /// <summary>
    /// Determines if the specified script type can be read via Ancient Tongues.
    /// </summary>
    /// <param name="scriptType">The script type to check.</param>
    /// <returns><c>true</c> if the script is among the four unlocked types.</returns>
    bool CanReadScript(ScriptType scriptType);

    /// <summary>
    /// Gets all script types unlocked by the Ancient Tongues ability.
    /// </summary>
    /// <returns>Read-only list of unlocked <see cref="ScriptType"/> values.</returns>
    IReadOnlyList<ScriptType> GetUnlockedScripts();

    // ═══════ Prerequisite Helpers ═══════

    /// <summary>
    /// Checks whether Tier 2 abilities can be unlocked based on PP invested.
    /// Requires 8 PP invested in the Jötun-Reader tree.
    /// </summary>
    /// <param name="ppInvested">Total PP invested.</param>
    /// <returns><c>true</c> if threshold is met.</returns>
    bool CanUnlockTier2(int ppInvested);

    /// <summary>
    /// Calculates total PP invested from a list of unlocked Jötun-Reader abilities.
    /// </summary>
    /// <param name="unlockedAbilities">Currently unlocked abilities.</param>
    /// <returns>Total PP cost of all unlocked abilities.</returns>
    int CalculatePPInvested(IReadOnlyList<JotunReaderAbilityId> unlockedAbilities);

    /// <summary>
    /// Gets the PP cost for a specific Jötun-Reader ability based on its tier.
    /// </summary>
    /// <param name="abilityId">The ability to look up.</param>
    /// <returns>PP cost: 0 (Tier 1), 4 (Tier 2), 5 (Tier 3), 6 (Capstone).</returns>
    int GetAbilityPPCost(JotunReaderAbilityId abilityId);
}
