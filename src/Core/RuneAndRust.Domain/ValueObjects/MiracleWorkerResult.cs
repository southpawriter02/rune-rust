namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Immutable result record from the Miracle Worker capstone ability execution.
/// Records the full HP restoration and condition clearing of a target.
/// </summary>
/// <remarks>
/// <para>Introduced in v0.20.6c as the Bone-Setter Capstone (Ultimate) ability result.</para>
/// <para>Miracle Worker is the pinnacle of the Bone-Setter's medical art:</para>
/// <list type="bullet">
/// <item>Fully restores target to maximum HP</item>
/// <item>Clears all negative conditions (poison, disease, blinded, etc.)</item>
/// <item>Costs 5 AP, no Medical Supplies required</item>
/// <item>Usable once per long rest (cooldown resets on rest)</item>
/// </list>
/// <para>Key computed properties:</para>
/// <list type="bullet">
/// <item><see cref="HpAfter"/>: Always equals <see cref="MaxHp"/> (full restoration)</item>
/// <item><see cref="TotalHealing"/>: MaxHp - HpBefore</item>
/// <item><see cref="ConditionsCleared"/>: Count of conditions removed</item>
/// </list>
/// <para>No Corruption risk — Miracle Worker follows the Coherent path.</para>
/// </remarks>
public sealed record MiracleWorkerResult
{
    /// <summary>
    /// Unique identifier of the healed target.
    /// </summary>
    public Guid TargetId { get; init; }

    /// <summary>
    /// Display name of the healed target.
    /// </summary>
    public string TargetName { get; init; } = string.Empty;

    /// <summary>
    /// HP value before Miracle Worker was applied.
    /// </summary>
    public int HpBefore { get; init; }

    /// <summary>
    /// Target's maximum HP. Miracle Worker restores to this value.
    /// </summary>
    public int MaxHp { get; init; }

    /// <summary>
    /// HP value after healing. Always equals <see cref="MaxHp"/> (full restoration).
    /// </summary>
    public int HpAfter => MaxHp;

    /// <summary>
    /// Total HP healed by Miracle Worker. Computed as MaxHp - HpBefore.
    /// </summary>
    public int TotalHealing => MaxHp - HpBefore;

    /// <summary>
    /// Collection of condition names that were cleared by Miracle Worker.
    /// Includes poison, disease, blinded, stunned, and all other negative effects.
    /// Does NOT include Corruption (metaphysical, not medical).
    /// </summary>
    public IReadOnlyList<string> ClearedConditions { get; init; } = [];

    /// <summary>
    /// Number of conditions that were removed. Computed from <see cref="ClearedConditions"/> count.
    /// </summary>
    public int ConditionsCleared => ClearedConditions.Count;

    /// <summary>
    /// Narrative message describing the miraculous intervention.
    /// </summary>
    public string MiracleMessage { get; init; } = string.Empty;

    /// <summary>
    /// Returns a human-readable status message summarizing the miracle's outcome.
    /// Includes HP restoration and condition count if any were cleared.
    /// </summary>
    /// <returns>
    /// A formatted string showing the target name, HP transition, and condition count.
    /// </returns>
    public string GetStatusMessage() =>
        $"MIRACLE WORKER: {TargetName} FULLY RESTORED! " +
        $"({HpBefore} -> {HpAfter} HP" +
        $"{(ConditionsCleared > 0 ? $", {ConditionsCleared} conditions cleared" : "")})";

    /// <summary>
    /// Returns a detailed multi-line breakdown of the Miracle Worker's effects.
    /// Includes HP restoration, condition list, and cooldown information.
    /// </summary>
    /// <returns>
    /// A multi-line formatted string showing the complete miracle breakdown.
    /// </returns>
    public string GetFullBreakdown()
    {
        var lines = new List<string>
        {
            "=== MIRACLE WORKER ===",
            $"Target: {TargetName}",
            $"HP Restored: {HpBefore} -> {HpAfter} (+{TotalHealing})",
            $"Conditions Cleared: {ConditionsCleared}"
        };

        if (ClearedConditions.Count > 0)
            lines.Add($"  Removed: {string.Join(", ", ClearedConditions)}");

        lines.Add("Next Available: After long rest");
        lines.Add("=====================");

        return string.Join("\n", lines);
    }
}
