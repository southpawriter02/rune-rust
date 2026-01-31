// ═══════════════════════════════════════════════════════════════════════════════
// TierUnlockResult.cs
// Value object capturing the result of unlocking a specialization ability tier,
// including the unlocked tier definition, granted abilities, PP cost, and any
// failure reasons.
// Version: 0.17.4e
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.ValueObjects;

using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the result of unlocking a specialization ability tier.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="TierUnlockResult"/> captures the outcome of a tier unlock operation,
/// including the abilities granted and the PP cost paid. Tier unlocks occur during
/// character progression after the initial specialization selection.
/// </para>
/// <para>
/// Use the factory methods <see cref="Succeeded"/> and <see cref="Failed"/> to create
/// instances rather than constructing directly.
/// </para>
/// <para>
/// <strong>Tier Unlock Costs and Requirements:</strong>
/// <list type="bullet">
///   <item><description>Tier 1: Free, unlocked automatically with specialization selection</description></item>
///   <item><description>Tier 2: 2 PP, requires Tier 1 unlocked + Rank 2</description></item>
///   <item><description>Tier 3: 3 PP, requires Tier 2 unlocked + Rank 3</description></item>
/// </list>
/// </para>
/// </remarks>
/// <seealso cref="SpecializationAbilityTier"/>
/// <seealso cref="SpecializationAbility"/>
/// <seealso cref="SpecializationApplicationResult"/>
public sealed class TierUnlockResult
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether the tier was unlocked successfully.
    /// </summary>
    /// <value>
    /// <c>true</c> if the tier was unlocked and abilities were granted;
    /// <c>false</c> if the unlock failed (e.g., missing previous tier,
    /// insufficient PP, insufficient rank, tier already unlocked).
    /// </value>
    public bool Success { get; }

    /// <summary>
    /// Gets the tier that was unlocked, if successful.
    /// </summary>
    /// <value>
    /// The <see cref="SpecializationAbilityTier"/> containing the tier number,
    /// display name, and abilities; or <c>null</c> if the unlock failed.
    /// </value>
    public SpecializationAbilityTier? UnlockedTier { get; }

    /// <summary>
    /// Gets the abilities that were granted by unlocking the tier.
    /// </summary>
    /// <value>
    /// A read-only list of <see cref="SpecializationAbility"/> records describing
    /// each ability granted. Typically 2-4 abilities per tier. Empty on failure.
    /// </value>
    public IReadOnlyList<SpecializationAbility> GrantedAbilities { get; }

    /// <summary>
    /// Gets the Progression Point cost that was deducted for this unlock.
    /// </summary>
    /// <value>
    /// 2 PP for Tier 2, 3 PP for Tier 3. 0 on failure.
    /// </value>
    public int PpCost { get; }

    /// <summary>
    /// Gets the failure reason, if the unlock was unsuccessful.
    /// </summary>
    /// <value>
    /// A human-readable description of why the tier unlock failed;
    /// or <c>null</c> if the unlock was successful.
    /// </value>
    public string? FailureReason { get; }

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Private constructor enforcing use of factory methods.
    /// </summary>
    /// <param name="success">Whether the unlock succeeded.</param>
    /// <param name="unlockedTier">The tier that was unlocked, if successful.</param>
    /// <param name="grantedAbilities">Abilities granted by the unlock.</param>
    /// <param name="ppCost">The PP cost deducted.</param>
    /// <param name="failureReason">Failure reason, if unsuccessful.</param>
    private TierUnlockResult(
        bool success,
        SpecializationAbilityTier? unlockedTier,
        IReadOnlyList<SpecializationAbility> grantedAbilities,
        int ppCost,
        string? failureReason)
    {
        Success = success;
        UnlockedTier = unlockedTier;
        GrantedAbilities = grantedAbilities;
        PpCost = ppCost;
        FailureReason = failureReason;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a successful tier unlock result.
    /// </summary>
    /// <param name="tier">The tier that was unlocked.</param>
    /// <returns>
    /// A <see cref="TierUnlockResult"/> with <see cref="Success"/> set to <c>true</c>
    /// and all unlock details populated.
    /// </returns>
    /// <example>
    /// <code>
    /// var result = TierUnlockResult.Succeeded(tier2);
    /// // result.Success == true
    /// // result.PpCost == 2
    /// // result.GrantedAbilities.Count == 3
    /// </code>
    /// </example>
    public static TierUnlockResult Succeeded(SpecializationAbilityTier tier) =>
        new(
            success: true,
            unlockedTier: tier,
            grantedAbilities: tier.Abilities,
            ppCost: tier.UnlockCost,
            failureReason: null);

    /// <summary>
    /// Creates a failed tier unlock result with a reason.
    /// </summary>
    /// <param name="reason">A human-readable description of why the unlock failed.</param>
    /// <returns>
    /// A <see cref="TierUnlockResult"/> with <see cref="Success"/> set to <c>false</c>
    /// and empty grant details.
    /// </returns>
    /// <example>
    /// <code>
    /// var result = TierUnlockResult.Failed("Tier 1 must be unlocked first");
    /// // result.Success == false
    /// // result.FailureReason == "Tier 1 must be unlocked first"
    /// </code>
    /// </example>
    public static TierUnlockResult Failed(string reason) =>
        new(
            success: false,
            unlockedTier: null,
            grantedAbilities: Array.Empty<SpecializationAbility>(),
            ppCost: 0,
            failureReason: reason);

    // ═══════════════════════════════════════════════════════════════════════════
    // DISPLAY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets a display-friendly summary of the tier unlock result.
    /// </summary>
    /// <returns>
    /// A formatted summary string. On success, includes the tier number, name,
    /// ability count, and PP cost. On failure, includes the failure reason.
    /// </returns>
    /// <example>
    /// <code>
    /// // Success:
    /// // "Unlocked Tier 2: Unleashed Beast
    /// //   Abilities Granted: 3
    /// //   PP Cost: 2"
    /// //
    /// // Failure:
    /// // "Failed: Tier 1 must be unlocked first"
    /// </code>
    /// </example>
    public string GetSummary()
    {
        if (!Success)
            return $"Failed: {FailureReason}";

        return $"Unlocked Tier {UnlockedTier?.Tier}: {UnlockedTier?.DisplayName}\n" +
               $"  Abilities Granted: {GrantedAbilities.Count}\n" +
               $"  PP Cost: {PpCost}";
    }
}
