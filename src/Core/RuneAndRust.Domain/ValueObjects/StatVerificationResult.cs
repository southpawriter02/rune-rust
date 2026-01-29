namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Contains the result of stat verification for an item.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="StatVerificationResult"/> aggregates all violations found during item
/// stat verification. It provides an overall validity flag and detailed violation
/// information for logging and debugging.
/// </para>
/// <para>
/// <strong>Example Usage:</strong>
/// <code>
/// // Valid result
/// var validResult = StatVerificationResult.Valid("sword-001", QualityTier.Scavenged);
/// Console.WriteLine(validResult.IsValid);  // true
/// 
/// // Invalid result with violations
/// var violations = new[] { StatViolation.Damage(expected, 15) };
/// var invalidResult = StatVerificationResult.Invalid("sword-002", QualityTier.Scavenged, violations);
/// Console.WriteLine(invalidResult.GetViolationSummary());
/// // "Damage value 15 outside expected range 1-6 (1d6)"
/// </code>
/// </para>
/// </remarks>
/// <param name="IsValid">Whether all stats passed validation.</param>
/// <param name="Violations">Collection of stat violations (empty if valid).</param>
/// <param name="ItemId">Identifier of the item that was verified.</param>
/// <param name="QualityTier">The quality tier the item was verified against.</param>
/// <seealso cref="StatViolation"/>
/// <seealso cref="StatRange"/>
public readonly record struct StatVerificationResult(
    bool IsValid,
    IReadOnlyList<StatViolation> Violations,
    string ItemId,
    QualityTier QualityTier)
{
    #region Factory Methods

    /// <summary>
    /// Creates a valid verification result (no violations).
    /// </summary>
    /// <param name="itemId">Identifier of the verified item.</param>
    /// <param name="tier">Quality tier the item was verified against.</param>
    /// <returns>A new valid <see cref="StatVerificationResult"/>.</returns>
    /// <remarks>
    /// <code>
    /// var result = StatVerificationResult.Valid("axe-007", QualityTier.RuneEtched);
    /// result.IsValid.Should().BeTrue();
    /// result.Violations.Should().BeEmpty();
    /// </code>
    /// </remarks>
    public static StatVerificationResult Valid(string itemId, QualityTier tier) =>
        new(true, Array.Empty<StatViolation>(), itemId, tier);

    /// <summary>
    /// Creates an invalid verification result with violations.
    /// </summary>
    /// <param name="itemId">Identifier of the verified item.</param>
    /// <param name="tier">Quality tier the item was verified against.</param>
    /// <param name="violations">Collection of stat violations found.</param>
    /// <returns>A new invalid <see cref="StatVerificationResult"/>.</returns>
    /// <remarks>
    /// <code>
    /// var violations = new[] 
    /// { 
    ///     StatViolation.Damage(expected, 15),
    ///     StatViolation.Attribute("Might", attrRange, 7)
    /// };
    /// var result = StatVerificationResult.Invalid("sword-002", QualityTier.Scavenged, violations);
    /// result.IsValid.Should().BeFalse();
    /// result.Violations.Should().HaveCount(2);
    /// </code>
    /// </remarks>
    public static StatVerificationResult Invalid(
        string itemId,
        QualityTier tier,
        IReadOnlyList<StatViolation> violations) =>
        new(false, violations, itemId, tier);

    #endregion

    #region Computed Properties

    /// <summary>
    /// Gets the count of violations found.
    /// </summary>
    public int ViolationCount => Violations.Count;

    /// <summary>
    /// Gets whether any damage violations were found.
    /// </summary>
    public bool HasDamageViolation =>
        Violations.Any(v => v.StatType == StatViolationType.Damage);

    /// <summary>
    /// Gets whether any defense violations were found.
    /// </summary>
    public bool HasDefenseViolation =>
        Violations.Any(v => v.StatType == StatViolationType.Defense);

    /// <summary>
    /// Gets whether any attribute violations were found.
    /// </summary>
    public bool HasAttributeViolation =>
        Violations.Any(v => v.StatType == StatViolationType.Attribute);

    #endregion

    #region Methods

    /// <summary>
    /// Gets a summary of all violations.
    /// </summary>
    /// <returns>
    /// "All stats valid" if valid, otherwise semicolon-separated violation messages.
    /// </returns>
    /// <remarks>
    /// <code>
    /// var result = StatVerificationResult.Invalid("sword-001", tier, violations);
    /// Console.WriteLine(result.GetViolationSummary());
    /// // "Damage value 15 outside expected range 1-6 (1d6); Might value 7 outside expected range 4-4 (+4)"
    /// </code>
    /// </remarks>
    public string GetViolationSummary() =>
        IsValid
            ? "All stats valid"
            : string.Join("; ", Violations.Select(v => v.Message));

    /// <summary>
    /// Gets violations of a specific type.
    /// </summary>
    /// <param name="type">The violation type to filter by.</param>
    /// <returns>Collection of matching violations.</returns>
    public IEnumerable<StatViolation> GetViolationsOfType(StatViolationType type) =>
        Violations.Where(v => v.StatType == type);

    #endregion

    /// <summary>
    /// Returns a summary string for logging/debugging.
    /// </summary>
    public override string ToString() =>
        IsValid
            ? $"Item {ItemId} valid for {QualityTier}"
            : $"Item {ItemId} invalid for {QualityTier}: {ViolationCount} violation(s)";
}
