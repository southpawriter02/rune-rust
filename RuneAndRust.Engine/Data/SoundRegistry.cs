using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Models;

namespace RuneAndRust.Engine.Data;

/// <summary>
/// Static registry mapping game events to appropriate sound cues (v0.3.19b).
/// Provides centralized cue selection logic for the audio event system.
/// </summary>
/// <remarks>See: SPEC-AUDIO-001 for Audio System design.</remarks>
public static class SoundRegistry
{
    /// <summary>
    /// Damage threshold for distinguishing heavy vs light hits.
    /// </summary>
    private const int HeavyDamageThreshold = 10;

    /// <summary>
    /// Value threshold for valuable loot (triggers special cue).
    /// </summary>
    private const int ValuableLootThreshold = 50;

    /// <summary>
    /// Gets the appropriate sound cue for a damage event.
    /// </summary>
    /// <param name="isCritical">Whether the hit was critical.</param>
    /// <param name="damageAmount">The amount of damage dealt.</param>
    /// <returns>The appropriate combat sound cue.</returns>
    public static SoundCue GetDamageCue(bool isCritical, int damageAmount)
    {
        // Critical hits always get the critical cue
        if (isCritical)
        {
            return SoundCue.CombatCritical;
        }

        // Heavy vs light hit based on damage threshold
        return damageAmount > HeavyDamageThreshold
            ? SoundCue.CombatHitHeavy
            : SoundCue.CombatHitLight;
    }

    /// <summary>
    /// Gets the appropriate sound cue for a death event.
    /// </summary>
    /// <param name="isPlayer">Whether the deceased was a player character.</param>
    /// <returns>The appropriate death sound cue.</returns>
    public static SoundCue GetDeathCue(bool isPlayer)
    {
        // Player death is more impactful - use error cue for now
        // Future: Add dedicated death cues (SoundCue.DeathPlayer, SoundCue.DeathEnemy)
        return isPlayer
            ? SoundCue.UiError
            : SoundCue.CombatHitHeavy;
    }

    /// <summary>
    /// Gets the appropriate sound cue for a loot pickup event.
    /// </summary>
    /// <param name="itemValue">The Scrip value of the looted item.</param>
    /// <returns>The appropriate loot sound cue.</returns>
    public static SoundCue GetLootCue(int itemValue)
    {
        // Valuable loot gets a more prominent cue
        return itemValue > ValuableLootThreshold
            ? SoundCue.UiSelect
            : SoundCue.UiClick;
    }

    /// <summary>
    /// Gets the appropriate sound cue for a UI action.
    /// </summary>
    /// <param name="actionType">The type of UI action.</param>
    /// <returns>The appropriate UI sound cue.</returns>
    public static SoundCue GetUiCue(UiActionType actionType)
    {
        return actionType switch
        {
            UiActionType.Click => SoundCue.UiClick,
            UiActionType.Select => SoundCue.UiSelect,
            UiActionType.Error => SoundCue.UiError,
            UiActionType.Confirm => SoundCue.UiSelect,
            UiActionType.Cancel => SoundCue.UiClick,
            _ => SoundCue.UiClick
        };
    }
}

/// <summary>
/// Types of UI actions for sound cue selection.
/// </summary>
public enum UiActionType
{
    /// <summary>Standard button click or menu navigation.</summary>
    Click,

    /// <summary>Selection confirmation or item pickup.</summary>
    Select,

    /// <summary>Error or invalid action.</summary>
    Error,

    /// <summary>Confirming an important action.</summary>
    Confirm,

    /// <summary>Canceling or backing out of a menu.</summary>
    Cancel
}
