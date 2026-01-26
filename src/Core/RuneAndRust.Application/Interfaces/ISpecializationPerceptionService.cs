namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Provides specialization-based perception ability management and application.
/// Integrates with passive perception, examination, and investigation services.
/// </summary>
/// <remarks>
/// <para>
/// This service acts as a centralized registry for perception abilities
/// and provides methods to query and apply abilities based on character
/// specialization and current context.
/// </para>
/// </remarks>
public interface ISpecializationPerceptionService
{
    /// <summary>
    /// Gets all perception abilities for a character based on their specialization.
    /// </summary>
    /// <param name="characterId">The character to query.</param>
    /// <returns>All perception abilities available to this character.</returns>
    IReadOnlyList<PerceptionAbility> GetAbilitiesForCharacter(string characterId);

    /// <summary>
    /// Checks if any abilities apply to examining the specified object.
    /// </summary>
    /// <param name="characterId">The examining character.</param>
    /// <param name="objectCategory">The object's category.</param>
    /// <param name="objectType">The object's specific type.</param>
    /// <param name="objectTags">Tags associated with the object.</param>
    /// <returns>List of applicable abilities with their effects.</returns>
    IReadOnlyList<PerceptionAbility> GetExaminationAbilities(
        string characterId,
        string objectCategory,
        string objectType,
        IReadOnlyList<string> objectTags);

    /// <summary>
    /// Gets passive perception modifiers from specialization abilities.
    /// </summary>
    /// <param name="characterId">The character to query.</param>
    /// <param name="elementTypes">Optional: specific element types to check against.</param>
    /// <returns>Total passive perception bonus from abilities.</returns>
    int GetPassivePerceptionBonus(
        string characterId,
        IReadOnlyList<string>? elementTypes = null);

    /// <summary>
    /// Checks if a character auto-detects specific element types.
    /// </summary>
    /// <param name="characterId">The character to check.</param>
    /// <param name="elementType">The hidden element type.</param>
    /// <param name="distanceFeet">Distance to the element in feet.</param>
    /// <returns>True if auto-detection applies.</returns>
    bool CheckAutoDetection(
        string characterId,
        string elementType,
        int distanceFeet);

    /// <summary>
    /// Gets investigation bonuses for a specific target type.
    /// </summary>
    /// <param name="characterId">The investigating character.</param>
    /// <param name="targetType">The investigation target type.</param>
    /// <returns>Total bonus dice and other modifiers.</returns>
    InvestigationModifiers GetInvestigationModifiers(
        string characterId,
        string targetType);

    /// <summary>
    /// Checks if a character auto-succeeds an examination layer for the given object.
    /// </summary>
    /// <param name="characterId">The examining character.</param>
    /// <param name="objectCategory">The object's category.</param>
    /// <param name="objectType">The object's type.</param>
    /// <param name="objectTags">Tags on the object.</param>
    /// <param name="layer">The examination layer to check.</param>
    /// <returns>True if auto-success applies for this layer.</returns>
    bool CheckExaminationAutoSuccess(
        string characterId,
        string objectCategory,
        string objectType,
        IReadOnlyList<string> objectTags,
        int layer);

    /// <summary>
    /// Gets bonus lore keys that should be revealed for the given examination.
    /// </summary>
    /// <param name="characterId">The examining character.</param>
    /// <param name="objectId">The object being examined.</param>
    /// <param name="objectTags">Tags on the object.</param>
    /// <returns>List of bonus lore keys to reveal.</returns>
    IReadOnlyList<string> GetBonusLoreKeys(
        string characterId,
        string objectId,
        IReadOnlyList<string> objectTags);

    /// <summary>
    /// Records an ability activation for tracking purposes.
    /// </summary>
    /// <param name="activation">The activation to record.</param>
    /// <param name="ct">Cancellation token.</param>
    Task RecordActivationAsync(PerceptionAbilityActivation activation, CancellationToken ct = default);
}
