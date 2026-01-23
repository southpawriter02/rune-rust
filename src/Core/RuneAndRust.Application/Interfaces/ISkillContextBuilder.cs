using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Builder interface for constructing <see cref="SkillContext"/> objects.
/// </summary>
/// <remarks>
/// <para>
/// Provides a fluent API for adding modifiers from various sources:
/// <list type="bullet">
///   <item><description>Equipment modifiers via <see cref="WithEquipment(EquipmentModifier)"/></description></item>
///   <item><description>Situational modifiers via <see cref="WithSituation(SituationalModifier)"/></description></item>
///   <item><description>Environment modifiers via <see cref="WithEnvironment(EnvironmentModifier)"/></description></item>
///   <item><description>Target modifiers via <see cref="WithTarget(TargetModifier)"/></description></item>
/// </list>
/// </para>
/// <para>
/// Call <see cref="Build"/> to create the immutable <see cref="SkillContext"/>.
/// </para>
/// </remarks>
public interface ISkillContextBuilder
{
    /// <summary>
    /// Adds an equipment modifier.
    /// </summary>
    /// <param name="equipmentId">Equipment item identifier.</param>
    /// <param name="name">Equipment display name.</param>
    /// <param name="diceModifier">Dice pool bonus/penalty.</param>
    /// <param name="dcModifier">DC bonus/penalty.</param>
    /// <param name="category">Equipment category.</param>
    /// <param name="required">Whether required to attempt check.</param>
    /// <returns>The builder for chaining.</returns>
    ISkillContextBuilder WithEquipment(
        string equipmentId,
        string name,
        int diceModifier = 0,
        int dcModifier = 0,
        EquipmentCategory category = EquipmentCategory.Tool,
        bool required = false);

    /// <summary>
    /// Adds an equipment modifier from an existing value object.
    /// </summary>
    /// <param name="modifier">The equipment modifier to add.</param>
    /// <returns>The builder for chaining.</returns>
    ISkillContextBuilder WithEquipment(EquipmentModifier modifier);

    /// <summary>
    /// Adds a situational modifier.
    /// </summary>
    /// <param name="modifierId">Modifier identifier.</param>
    /// <param name="name">Display name.</param>
    /// <param name="diceModifier">Dice pool bonus/penalty.</param>
    /// <param name="dcModifier">DC bonus/penalty.</param>
    /// <param name="source">Source of the modifier.</param>
    /// <param name="duration">How long modifier persists.</param>
    /// <returns>The builder for chaining.</returns>
    ISkillContextBuilder WithSituation(
        string modifierId,
        string name,
        int diceModifier = 0,
        int dcModifier = 0,
        string? source = null,
        ModifierDuration duration = ModifierDuration.Instant);

    /// <summary>
    /// Adds a situational modifier from an existing value object.
    /// </summary>
    /// <param name="modifier">The situational modifier to add.</param>
    /// <returns>The builder for chaining.</returns>
    ISkillContextBuilder WithSituation(SituationalModifier modifier);

    /// <summary>
    /// Adds environment modifiers for surface and lighting.
    /// </summary>
    /// <param name="surface">Surface type (for climbing, stealth).</param>
    /// <param name="lighting">Lighting level (for visual tasks).</param>
    /// <returns>The builder for chaining.</returns>
    ISkillContextBuilder WithEnvironment(
        SurfaceType? surface = null,
        LightingLevel? lighting = null);

    /// <summary>
    /// Adds a corruption modifier.
    /// </summary>
    /// <param name="tier">Corruption tier of the area.</param>
    /// <returns>The builder for chaining.</returns>
    ISkillContextBuilder WithCorruption(CorruptionTier tier);

    /// <summary>
    /// Adds an environment modifier from an existing value object.
    /// </summary>
    /// <param name="modifier">The environment modifier to add.</param>
    /// <returns>The builder for chaining.</returns>
    ISkillContextBuilder WithEnvironment(EnvironmentModifier modifier);

    /// <summary>
    /// Adds a target disposition modifier.
    /// </summary>
    /// <param name="disposition">Target's disposition.</param>
    /// <param name="targetId">Optional target ID.</param>
    /// <returns>The builder for chaining.</returns>
    ISkillContextBuilder WithTargetDisposition(Disposition disposition, string? targetId = null);

    /// <summary>
    /// Adds a target suspicion modifier.
    /// </summary>
    /// <param name="suspicionLevel">Suspicion level (0-10).</param>
    /// <param name="targetId">Optional target ID.</param>
    /// <returns>The builder for chaining.</returns>
    ISkillContextBuilder WithTargetSuspicion(int suspicionLevel, string? targetId = null);

    /// <summary>
    /// Adds a target modifier from an existing value object.
    /// </summary>
    /// <param name="modifier">The target modifier to add.</param>
    /// <returns>The builder for chaining.</returns>
    ISkillContextBuilder WithTarget(TargetModifier modifier);

    /// <summary>
    /// Adds a status effect to be applied based on check outcome.
    /// </summary>
    /// <param name="statusId">Status effect identifier.</param>
    /// <returns>The builder for chaining.</returns>
    ISkillContextBuilder WithAppliedStatus(string statusId);

    /// <summary>
    /// Builds the immutable skill context.
    /// </summary>
    /// <returns>A new <see cref="SkillContext"/> with all added modifiers.</returns>
    SkillContext Build();

    /// <summary>
    /// Resets the builder to its initial empty state.
    /// </summary>
    /// <returns>The builder for chaining.</returns>
    ISkillContextBuilder Reset();
}
