// ═══════════════════════════════════════════════════════════════════════════════
// ICharacterFactory.cs
// Interface defining the contract for creating Player entities from a completed
// CharacterCreationState. The factory assembles all character components (lineage,
// background, attributes, archetype, specialization, name) into a fully
// initialized Player entity ready for persistence.
// Version: 0.17.5d (stub — full implementation in v0.17.5e)
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.Entities;

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
/// <strong>Lifecycle:</strong> This interface is defined in v0.17.5d as a dependency
/// of <c>CharacterCreationController</c>, but the full implementation is deferred
/// to v0.17.5e. The controller accepts <c>ICharacterFactory?</c> as an optional
/// constructor parameter and handles the null case gracefully.
/// </para>
/// <para>
/// <strong>Responsibilities:</strong>
/// </para>
/// <list type="bullet">
///   <item><description>Validate that the creation state is complete</description></item>
///   <item><description>Map lineage, background, archetype, and specialization to Player properties</description></item>
///   <item><description>Apply attribute modifiers from lineage and background</description></item>
///   <item><description>Initialize starting abilities from archetype</description></item>
///   <item><description>Return a fully initialized Player entity</description></item>
/// </list>
/// </remarks>
/// <seealso cref="CharacterCreationState"/>
/// <seealso cref="Player"/>
public interface ICharacterFactory
{
    /// <summary>
    /// Creates a new <see cref="Player"/> entity from the completed creation state.
    /// </summary>
    /// <param name="state">
    /// The completed <see cref="CharacterCreationState"/> containing all player
    /// selections (lineage, background, attributes, archetype, specialization, name).
    /// Must have <c>IsComplete</c> equal to <c>true</c>.
    /// </param>
    /// <returns>
    /// A task that resolves to the newly created <see cref="Player"/> entity
    /// with all properties initialized from the creation state.
    /// </returns>
    Task<Player> CreateCharacterAsync(CharacterCreationState state);
}
