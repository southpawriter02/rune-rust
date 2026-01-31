// ═══════════════════════════════════════════════════════════════════════════════
// FactoryValidationResult.cs
// Represents the result of validating a CharacterCreationState before character
// creation. Contains either a successful validation (all fields present, all
// definitions resolvable, specialization-archetype compatible) or a list of
// validation errors explaining why the state is not ready for character creation.
// Returned by ICharacterFactory.ValidateState().
// Version: 0.17.5e
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Result of validating a <see cref="Entities.CharacterCreationState"/> before
/// character creation via <c>ICharacterFactory.CreateCharacterAsync()</c>.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="FactoryValidationResult"/> is returned by
/// <c>ICharacterFactory.ValidateState()</c> to indicate whether a
/// <see cref="Entities.CharacterCreationState"/> is complete and valid for
/// character creation. When valid, the factory can proceed with the 13-step
/// initialization sequence. When invalid, the <see cref="Errors"/> list
/// describes each missing or inconsistent field.
/// </para>
/// <para>
/// <strong>Validation checks include:</strong>
/// </para>
/// <list type="bullet">
///   <item><description>All required selections present (lineage, background, archetype, specialization, name)</description></item>
///   <item><description>Attribute allocation complete (<c>IsComplete == true</c>)</description></item>
///   <item><description>Clan-Born lineage has flexible attribute bonus selected</description></item>
///   <item><description>Provider definitions exist for all selected options</description></item>
///   <item><description>Selected specialization belongs to selected archetype</description></item>
/// </list>
/// <para>
/// Instances are created exclusively through the <see cref="Valid"/> and
/// <see cref="Invalid(string[])"/> factory methods. Use <see cref="IsValid"/>
/// to check the result before proceeding with character creation.
/// </para>
/// </remarks>
/// <seealso cref="Entities.CharacterCreationState"/>
public readonly record struct FactoryValidationResult
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether the state is valid for character creation.
    /// </summary>
    /// <value>
    /// <c>true</c> if all required selections are present, all definitions
    /// resolve successfully, and all business rules pass. <c>false</c> if
    /// any validation check failed.
    /// </value>
    public bool IsValid { get; init; }

    /// <summary>
    /// Gets validation errors if state is invalid. Empty if successful.
    /// </summary>
    /// <value>
    /// A read-only list of user-friendly error messages describing each
    /// validation failure. Empty (not null) when <see cref="IsValid"/>
    /// is <c>true</c>.
    /// </value>
    public IReadOnlyList<string> Errors { get; init; }

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a valid result indicating the state is ready for character creation.
    /// </summary>
    /// <returns>
    /// A <see cref="FactoryValidationResult"/> with <see cref="IsValid"/> set
    /// to <c>true</c> and an empty <see cref="Errors"/> list.
    /// </returns>
    /// <example>
    /// <code>
    /// var result = FactoryValidationResult.Valid();
    /// // result.IsValid == true
    /// // result.Errors.Count == 0
    /// </code>
    /// </example>
    public static FactoryValidationResult Valid() => new()
    {
        IsValid = true,
        Errors = Array.Empty<string>()
    };

    /// <summary>
    /// Creates an invalid result with one or more validation errors.
    /// </summary>
    /// <param name="errors">One or more validation error messages.</param>
    /// <returns>
    /// A <see cref="FactoryValidationResult"/> with <see cref="IsValid"/> set
    /// to <c>false</c> and the provided errors in <see cref="Errors"/>.
    /// </returns>
    /// <example>
    /// <code>
    /// var result = FactoryValidationResult.Invalid("Lineage is required.");
    /// // result.IsValid == false
    /// // result.Errors[0] == "Lineage is required."
    /// </code>
    /// </example>
    public static FactoryValidationResult Invalid(params string[] errors) => new()
    {
        IsValid = false,
        Errors = errors
    };

    /// <summary>
    /// Creates an invalid result from an existing error list.
    /// </summary>
    /// <param name="errors">A read-only list of validation error messages.</param>
    /// <returns>
    /// A <see cref="FactoryValidationResult"/> with <see cref="IsValid"/> set
    /// to <c>false</c> and the provided error list in <see cref="Errors"/>.
    /// </returns>
    /// <example>
    /// <code>
    /// var errors = new List&lt;string&gt; { "Lineage is required.", "Name is required." };
    /// var result = FactoryValidationResult.Invalid(errors);
    /// // result.IsValid == false
    /// // result.Errors.Count == 2
    /// </code>
    /// </example>
    public static FactoryValidationResult Invalid(IReadOnlyList<string> errors) => new()
    {
        IsValid = false,
        Errors = errors
    };

    // ═══════════════════════════════════════════════════════════════════════════
    // DEBUGGING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a string representation for debugging.
    /// </summary>
    /// <returns>
    /// "Valid" when validation passed, or "Invalid: {count} error(s)" when failed.
    /// </returns>
    public override string ToString() =>
        IsValid
            ? "Valid"
            : $"Invalid: {Errors.Count} error(s)";
}
