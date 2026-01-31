// ═══════════════════════════════════════════════════════════════════════════════
// ICharacterFactory.cs
// Interface defining the contract for creating Player entities from a completed
// CharacterCreationState. The factory validates that the creation state is complete
// and internally consistent, then assembles all character components (lineage,
// background, attributes, archetype, specialization, name) into a fully
// initialized Player entity ready for persistence.
// Version: 0.17.5e
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Creates <see cref="Player"/> entities from completed character creation state.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="ICharacterFactory"/> is the application-layer service contract for
/// assembling a new <see cref="Player"/> entity from the selections captured in
/// <see cref="CharacterCreationState"/> during the character creation workflow.
/// </para>
/// <para>
/// <strong>Responsibilities:</strong>
/// </para>
/// <list type="bullet">
///   <item><description>Validate that the creation state is complete and internally consistent via <see cref="ValidateState"/></description></item>
///   <item><description>Construct the <see cref="Player"/> entity with base attributes from point allocation</description></item>
///   <item><description>Apply lineage attribute modifiers (including Clan-Born flexible +1 bonus)</description></item>
///   <item><description>Apply lineage passive bonuses (Max HP, Max AP, Soak, Movement, Skills)</description></item>
///   <item><description>Register lineage trait and set trauma baseline (Corruption/Stress)</description></item>
///   <item><description>Grant background skill bonuses</description></item>
///   <item><description>Calculate and apply derived stats (HP, Stamina, Aether Pool) via IDerivedStatCalculator</description></item>
///   <item><description>Initialize resource pools (Stamina, Aether Pool) at maximum values</description></item>
///   <item><description>Grant 3 archetype starting abilities and 3 specialization Tier 1 abilities</description></item>
///   <item><description>Initialize special resource if applicable (e.g., Rage for Berserkr)</description></item>
///   <item><description>Set saga progression to initial values (Legend: 0, PP: 0, Rank: 1)</description></item>
/// </list>
/// <para>
/// <strong>Dependencies:</strong> The implementation coordinates with
/// <c>ILineageProvider</c>, <c>IBackgroundProvider</c>, <c>IArchetypeProvider</c>,
/// <c>ISpecializationProvider</c>, and <c>IDerivedStatCalculator</c> to resolve
/// definitions and calculate derived values.
/// </para>
/// <para>
/// <strong>Usage:</strong> Called by <c>CharacterCreationController.ConfirmCharacterAsync()</c>
/// after name validation passes and the creation state is complete. The controller
/// accepts <c>ICharacterFactory?</c> as an optional constructor parameter.
/// </para>
/// </remarks>
/// <seealso cref="CharacterCreationState"/>
/// <seealso cref="Player"/>
/// <seealso cref="FactoryValidationResult"/>
public interface ICharacterFactory
{
    /// <summary>
    /// Validates that the creation state is complete and ready for character creation.
    /// </summary>
    /// <param name="state">
    /// The <see cref="CharacterCreationState"/> to validate. All required selections
    /// must be present: lineage, background, attributes (complete allocation),
    /// archetype, specialization, and character name.
    /// </param>
    /// <returns>
    /// A <see cref="FactoryValidationResult"/> indicating whether the state is valid.
    /// When invalid, the <see cref="FactoryValidationResult.Errors"/> list describes
    /// each validation failure.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Validation checks include:
    /// </para>
    /// <list type="number">
    ///   <item><description>Lineage is selected</description></item>
    ///   <item><description>Background is selected</description></item>
    ///   <item><description>Attributes are allocated and complete (<c>IsComplete == true</c>)</description></item>
    ///   <item><description>Archetype is selected</description></item>
    ///   <item><description>Specialization is selected</description></item>
    ///   <item><description>Character name is not null or whitespace</description></item>
    ///   <item><description>Clan-Born lineage has flexible attribute bonus selected</description></item>
    ///   <item><description>Selected specialization belongs to selected archetype</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var result = factory.ValidateState(state);
    /// if (!result.IsValid)
    ///     throw new InvalidOperationException(string.Join(", ", result.Errors));
    /// </code>
    /// </example>
    FactoryValidationResult ValidateState(CharacterCreationState state);

    /// <summary>
    /// Creates a fully initialized <see cref="Player"/> entity from the completed
    /// creation state using the 13-step initialization sequence.
    /// </summary>
    /// <param name="state">
    /// The completed <see cref="CharacterCreationState"/> containing all player
    /// selections (lineage, background, attributes, archetype, specialization, name).
    /// Must pass <see cref="ValidateState"/> validation.
    /// </param>
    /// <param name="ct">Cancellation token for async operation support.</param>
    /// <returns>
    /// A task that resolves to the newly created <see cref="Player"/> entity
    /// with all properties initialized, modifiers applied, abilities granted,
    /// and resources set to maximum values.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the state fails validation (incomplete selections, missing
    /// definitions, or specialization-archetype mismatch).
    /// </exception>
    /// <example>
    /// <code>
    /// var player = await factory.CreateCharacterAsync(state);
    /// // player.Name == state.CharacterName
    /// // player.SelectedLineage == state.SelectedLineage
    /// // player.Health == player.Stats.MaxHealth (resources at max)
    /// </code>
    /// </example>
    Task<Player> CreateCharacterAsync(
        CharacterCreationState state,
        CancellationToken ct = default);
}
