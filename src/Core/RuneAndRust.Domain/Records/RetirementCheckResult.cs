// ═══════════════════════════════════════════════════════════════════════════════
// RetirementCheckResult.cs
// Immutable record representing the evaluation of retirement conditions.
// Version: 0.18.3b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.Records;

/// <summary>
/// Represents the result of evaluating a character's retirement conditions.
/// </summary>
/// <remarks>
/// <para>
/// This record is returned when checking if a character must retire due to
/// accumulated trauma. It provides detailed information about which traumas
/// triggered the retirement and what options are available.
/// </para>
/// <para>
/// <b>Retirement Scenarios:</b>
/// <list type="bullet">
///   <item><description><b>Immediate</b>: Single retirement trauma acquired (MustRetire=true)</description></item>
///   <item><description><b>Stacking</b>: Critical stack threshold reached (MustRetire=true)</description></item>
///   <item><description><b>Accumulation</b>: Multiple moderate traumas (CanContinueWithPermission=true)</description></item>
///   <item><description><b>None</b>: No retirement conditions met</description></item>
/// </list>
/// </para>
/// <para>
/// <b>Retirement Conditions by Trauma:</b>
/// <list type="table">
///   <listheader>
///     <term>Trauma</term>
///     <description>Condition</description>
///   </listheader>
///   <item>
///     <term>Survivor's Guilt</term>
///     <description>First acquisition → Immediate retirement</description>
///   </item>
///   <item>
///     <term>Machine Affinity</term>
///     <description>First acquisition → Immediate retirement</description>
///   </item>
///   <item>
///     <term>Death Wish</term>
///     <description>First acquisition → Immediate retirement</description>
///   </item>
///   <item>
///     <term>Reality Doubt</term>
///     <description>5+ instances → Stacking retirement</description>
///   </item>
///   <item>
///     <term>Hollow Resonance</term>
///     <description>3+ instances → Stacking retirement</description>
///   </item>
///   <item>
///     <term>Multiple Moderate</term>
///     <description>3+ different traumas with StackCount > 1 → Optional</description>
///   </item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var result = RetirementCheckResult.CreateMustRetire(
///     characterId: characterId,
///     retirementReason: "Machine Affinity forces retirement",
///     traumasCausingRetirement: new[] { "machine-affinity" }
/// );
/// </code>
/// </example>
public record RetirementCheckResult
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Properties
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the character being evaluated.
    /// </summary>
    public Guid CharacterId { get; init; }

    /// <summary>
    /// Gets whether the character must retire.
    /// </summary>
    /// <remarks>
    /// <para>True: Character forced to retire immediately.</para>
    /// <para>False: Character can continue (but may be optional retirement).</para>
    /// </remarks>
    public bool MustRetire { get; init; }

    /// <summary>
    /// Gets the reason for forced retirement (if applicable).
    /// </summary>
    /// <remarks>
    /// <para>Null if MustRetire is false and no optional retirement applies.</para>
    /// <para>Example: "Machine Affinity forces immediate retirement"</para>
    /// </remarks>
    public string? RetirementReason { get; init; }

    /// <summary>
    /// Gets the traumas that triggered retirement conditions.
    /// </summary>
    /// <remarks>
    /// <para>List of trauma IDs that contributed to retirement evaluation.</para>
    /// <para>May be single trauma (immediate) or multiple (accumulation).</para>
    /// <para>Empty if no retirement conditions met.</para>
    /// </remarks>
    public IReadOnlyList<string> TraumasCausingRetirement { get; init; }

    /// <summary>
    /// Gets the total count of retirement-relevant traumas the character has.
    /// </summary>
    /// <remarks>
    /// Used to assess severity of accumulated trauma.
    /// </remarks>
    public int TotalRetirementTraumas { get; init; }

    /// <summary>
    /// Gets whether the character can continue playing with GM permission.
    /// </summary>
    /// <remarks>
    /// <para>True if retirement is optional (e.g., multiple moderate traumas).</para>
    /// <para>False if retirement is mandatory OR if no retirement conditions apply.</para>
    /// <para>Ignored if MustRetire is true.</para>
    /// </remarks>
    public bool CanContinueWithPermission { get; init; }

    // ═══════════════════════════════════════════════════════════════════════════
    // Constructors
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of the <see cref="RetirementCheckResult"/> record.
    /// </summary>
    /// <remarks>
    /// Use factory methods (<see cref="CreateMustRetire"/>, <see cref="CreateOptional"/>,
    /// <see cref="CreateNoRetirement"/>) instead of this constructor directly.
    /// </remarks>
    private RetirementCheckResult()
    {
        TraumasCausingRetirement = new List<string>();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Factory Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a result indicating mandatory retirement.
    /// </summary>
    /// <param name="characterId">The character being evaluated.</param>
    /// <param name="retirementReason">Why retirement is mandatory.</param>
    /// <param name="traumasCausingRetirement">Which traumas caused the retirement.</param>
    /// <returns>A new RetirementCheckResult with MustRetire=true.</returns>
    /// <example>
    /// <code>
    /// var result = RetirementCheckResult.CreateMustRetire(
    ///     characterId: playerId,
    ///     retirementReason: "Machine Affinity forces immediate retirement",
    ///     traumasCausingRetirement: new[] { "machine-affinity" }
    /// );
    /// // result.MustRetire == true
    /// // result.CanContinueWithPermission == false
    /// </code>
    /// </example>
    public static RetirementCheckResult CreateMustRetire(
        Guid characterId,
        string retirementReason,
        IReadOnlyList<string> traumasCausingRetirement)
    {
        return new RetirementCheckResult
        {
            CharacterId = characterId,
            MustRetire = true,
            RetirementReason = retirementReason,
            TraumasCausingRetirement = traumasCausingRetirement,
            TotalRetirementTraumas = traumasCausingRetirement.Count,
            CanContinueWithPermission = false
        };
    }

    /// <summary>
    /// Creates a result indicating optional retirement (GM can override).
    /// </summary>
    /// <param name="characterId">The character being evaluated.</param>
    /// <param name="traumasCausingRetirement">Which traumas contributed to the evaluation.</param>
    /// <returns>A new RetirementCheckResult with CanContinueWithPermission=true.</returns>
    /// <remarks>
    /// Typically used when a character has 3+ different moderate traumas with StackCount > 1.
    /// </remarks>
    /// <example>
    /// <code>
    /// var result = RetirementCheckResult.CreateOptional(
    ///     characterId: playerId,
    ///     traumasCausingRetirement: new[] { "combat-flashbacks", "night-terrors", "paranoid-ideation" }
    /// );
    /// // result.MustRetire == false
    /// // result.CanContinueWithPermission == true
    /// // result.TotalRetirementTraumas == 3
    /// </code>
    /// </example>
    public static RetirementCheckResult CreateOptional(
        Guid characterId,
        IReadOnlyList<string> traumasCausingRetirement)
    {
        return new RetirementCheckResult
        {
            CharacterId = characterId,
            MustRetire = false,
            RetirementReason = null,
            TraumasCausingRetirement = traumasCausingRetirement,
            TotalRetirementTraumas = traumasCausingRetirement.Count,
            CanContinueWithPermission = true
        };
    }

    /// <summary>
    /// Creates a result indicating no retirement conditions were met.
    /// </summary>
    /// <param name="characterId">The character being evaluated.</param>
    /// <returns>A new RetirementCheckResult with all retirement flags set to false.</returns>
    /// <example>
    /// <code>
    /// var result = RetirementCheckResult.CreateNoRetirement(playerId);
    /// // result.MustRetire == false
    /// // result.CanContinueWithPermission == false
    /// // result.TotalRetirementTraumas == 0
    /// // result.TraumasCausingRetirement is empty
    /// </code>
    /// </example>
    public static RetirementCheckResult CreateNoRetirement(Guid characterId)
    {
        return new RetirementCheckResult
        {
            CharacterId = characterId,
            MustRetire = false,
            RetirementReason = null,
            TraumasCausingRetirement = new List<string>(),
            TotalRetirementTraumas = 0,
            CanContinueWithPermission = false
        };
    }
}
