using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Entities;

/// <summary>
/// An interactive environmental hazard that triggers based on conditions (v0.3.3a).
/// Extends InteractableObject with lifecycle state, trigger configuration, and effect scripts.
/// Examples: steam vents, pressure plates, volatile spore pods.
/// </summary>
public class DynamicHazard : InteractableObject
{
    #region Classification

    /// <summary>
    /// Gets or sets the thematic classification of this hazard.
    /// Used for rendering and AI behavior hints.
    /// </summary>
    public HazardType HazardType { get; set; } = HazardType.Environmental;

    #endregion

    #region Lifecycle State

    /// <summary>
    /// Gets or sets the current lifecycle state of the hazard.
    /// Controls whether the hazard can trigger.
    /// </summary>
    public HazardState State { get; set; } = HazardState.Dormant;

    /// <summary>
    /// Gets or sets the remaining turns until the hazard resets from Cooldown to Dormant.
    /// Decremented by HazardService.TickCooldownsAsync at end of round.
    /// </summary>
    public int CooldownRemaining { get; set; } = 0;

    /// <summary>
    /// Gets or sets the number of turns required to recharge after activation.
    /// Set to hazard's MaxCooldown when entering Cooldown state.
    /// </summary>
    public int MaxCooldown { get; set; } = 2;

    /// <summary>
    /// Gets or sets whether this hazard is destroyed after a single use.
    /// If true, transitions to Destroyed state instead of Cooldown after triggering.
    /// </summary>
    public bool OneTimeUse { get; set; } = false;

    #endregion

    #region Trigger Configuration

    /// <summary>
    /// Gets or sets what condition activates this hazard.
    /// </summary>
    public TriggerType Trigger { get; set; } = TriggerType.Movement;

    /// <summary>
    /// Gets or sets an optional damage type filter for DamageTaken triggers.
    /// If null, any damage type will trigger the hazard.
    /// </summary>
    public DamageType? RequiredDamageType { get; set; }

    /// <summary>
    /// Gets or sets the minimum damage required to activate a DamageTaken trigger.
    /// Damage below this threshold is ignored.
    /// </summary>
    public int DamageThreshold { get; set; } = 0;

    #endregion

    #region Effect Execution

    /// <summary>
    /// Gets or sets the effect script executed when this hazard triggers.
    /// Uses semicolon-delimited command format: "DAMAGE:Fire:2d6;STATUS:Burning:2".
    /// Parsed by EffectScriptExecutor.
    /// </summary>
    public string EffectScript { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets an optional narrative message displayed when the hazard triggers.
    /// If empty, a default message based on HazardType is used.
    /// </summary>
    public string? TriggerMessage { get; set; }

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicHazard"/> class.
    /// Sets ObjectType to Hazard automatically.
    /// </summary>
    public DynamicHazard()
    {
        ObjectType = ObjectType.Hazard;
    }
}
