using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.Entities;

/// <summary>
/// Represents a persistent environmental hazard zone in a room.
/// </summary>
/// <remarks>
/// HazardZone entities represent dangerous areas that deal damage or apply status effects
/// to players who remain in or pass through them. Unlike traps which trigger once,
/// hazard zones persist and affect players each turn they remain in the zone.
/// <para>
/// Key features:
/// <list type="bullet">
///   <item><description>Entry damage when first entering the zone</description></item>
///   <item><description>Per-turn damage while remaining in the zone</description></item>
///   <item><description>Saving throws to reduce or negate effects</description></item>
///   <item><description>Status effect application on failed saves</description></item>
///   <item><description>Duration tracking for temporary hazards</description></item>
/// </list>
/// </para>
/// </remarks>
/// <seealso cref="HazardType"/>
/// <seealso cref="SavingThrow"/>
/// <seealso cref="HazardDefinition"/>
public class HazardZone : IEntity
{
    // ===== Core Properties =====

    /// <summary>Gets the unique identifier for this hazard zone.</summary>
    public Guid Id { get; private set; }

    /// <summary>Gets the definition ID from configuration.</summary>
    public string DefinitionId { get; private set; } = string.Empty;

    /// <summary>Gets the display name of this hazard.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Gets the description shown when examining the hazard.</summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>Gets the type of environmental hazard.</summary>
    public HazardType HazardType { get; private set; }

    /// <summary>Gets whether this hazard zone is currently active.</summary>
    public bool IsActive { get; private set; } = true;

    // ===== Damage Properties =====

    /// <summary>Gets the dice notation for per-turn damage (e.g., "1d6"), or null if no per-turn damage.</summary>
    public string? DamagePerTurnDice { get; private set; }

    /// <summary>Gets the dice notation for entry damage (e.g., "2d6"), or null if no entry damage.</summary>
    public string? EntryDamageDice { get; private set; }

    /// <summary>Gets the damage type identifier (e.g., "fire", "poison").</summary>
    public string? DamageType { get; private set; }

    // ===== Status Effect Properties =====

    /// <summary>Gets the status effects applied by this hazard on failed saves.</summary>
    public IReadOnlyList<string> StatusEffects { get; private set; } = Array.Empty<string>();

    /// <summary>Gets the duration of applied status effects in turns.</summary>
    public int StatusDuration { get; private set; } = 3;

    // ===== Saving Throw Properties =====

    /// <summary>Gets the saving throw configuration for this hazard, or null if no save allowed.</summary>
    public SavingThrow? Save { get; private set; }

    // ===== Zone Properties =====

    /// <summary>Gets whether this hazard affects the entire room.</summary>
    public bool AffectsWholeRoom { get; private set; } = true;

    /// <summary>Gets the remaining duration in turns (-1 = permanent).</summary>
    public int Duration { get; private set; } = -1;

    /// <summary>Gets the keywords used to identify this hazard in commands.</summary>
    public IReadOnlyList<string> Keywords { get; private set; } = Array.Empty<string>();

    /// <summary>Gets the custom message displayed when the hazard effect triggers.</summary>
    public string? EffectMessage { get; private set; }

    // ===== Computed Properties =====

    /// <summary>Gets whether this hazard is permanent (never expires).</summary>
    public bool IsPermanent => Duration < 0;

    /// <summary>Gets whether this hazard has expired (non-permanent with duration 0 or less).</summary>
    public bool IsExpired => !IsPermanent && Duration <= 0;

    /// <summary>Gets whether this hazard deals damage on entry.</summary>
    public bool DamageOnEntry => !string.IsNullOrEmpty(EntryDamageDice);

    /// <summary>Gets whether this hazard deals damage per turn.</summary>
    public bool DamagePerTurn => !string.IsNullOrEmpty(DamagePerTurnDice);

    /// <summary>Gets whether this hazard applies status effects.</summary>
    public bool AppliesStatus => StatusEffects.Count > 0;

    /// <summary>Gets whether this hazard allows a saving throw.</summary>
    public bool HasSave => Save.HasValue;

    // ===== Constructors =====

    /// <summary>
    /// Private parameterless constructor for Entity Framework Core.
    /// </summary>
    private HazardZone()
    {
    }

    // ===== Factory Methods =====

    /// <summary>
    /// Creates a new hazard zone from a definition.
    /// </summary>
    /// <param name="definition">The hazard definition to create from.</param>
    /// <returns>A new HazardZone instance configured according to the definition.</returns>
    /// <exception cref="ArgumentNullException">Thrown when definition is null.</exception>
    public static HazardZone FromDefinition(HazardDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentException.ThrowIfNullOrWhiteSpace(definition.Id);
        ArgumentException.ThrowIfNullOrWhiteSpace(definition.Name);

        var hazard = new HazardZone
        {
            Id = Guid.NewGuid(),
            DefinitionId = definition.Id.ToLowerInvariant(),
            Name = definition.Name,
            Description = definition.Description,
            HazardType = definition.HazardType,
            AffectsWholeRoom = definition.AffectsWholeRoom,
            Duration = definition.Duration,
            StatusDuration = definition.StatusDuration,
            EffectMessage = definition.EffectMessage,
            Keywords = definition.Keywords.Select(k => k.ToLowerInvariant()).ToList(),
            StatusEffects = definition.StatusEffects.ToList()
        };

        // Configure damage
        if (definition.DamagePerTurn != null)
        {
            hazard.DamagePerTurnDice = definition.DamagePerTurn.Dice;
            hazard.DamageType = definition.DamagePerTurn.DamageType;
        }

        if (definition.EntryDamage != null)
        {
            hazard.EntryDamageDice = definition.EntryDamage.Dice;
            // Use entry damage type if no per-turn type set
            hazard.DamageType ??= definition.EntryDamage.DamageType;
        }

        // Configure saving throw
        if (definition.Save != null)
        {
            hazard.Save = SavingThrow.Create(
                definition.Save.Attribute,
                definition.Save.DC,
                definition.Save.Negates);
        }

        return hazard;
    }

    /// <summary>
    /// Creates a new hazard zone with explicit parameters.
    /// </summary>
    public static HazardZone Create(
        string definitionId,
        string name,
        string description,
        HazardType hazardType,
        string? damagePerTurnDice = null,
        string? entryDamageDice = null,
        string? damageType = null,
        IEnumerable<string>? statusEffects = null,
        int statusDuration = 3,
        SavingThrow? save = null,
        bool affectsWholeRoom = true,
        int duration = -1,
        IEnumerable<string>? keywords = null,
        string? effectMessage = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(definitionId);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new HazardZone
        {
            Id = Guid.NewGuid(),
            DefinitionId = definitionId.ToLowerInvariant(),
            Name = name,
            Description = description ?? string.Empty,
            HazardType = hazardType,
            DamagePerTurnDice = damagePerTurnDice,
            EntryDamageDice = entryDamageDice,
            DamageType = damageType,
            StatusEffects = (statusEffects?.ToList() as IReadOnlyList<string>) ?? Array.Empty<string>(),
            StatusDuration = statusDuration,
            Save = save,
            AffectsWholeRoom = affectsWholeRoom,
            Duration = duration,
            Keywords = (keywords?.Select(k => k.ToLowerInvariant()).ToList() as IReadOnlyList<string>) ?? Array.Empty<string>(),
            EffectMessage = effectMessage
        };
    }

    // ===== Domain Methods =====

    /// <summary>
    /// Processes a turn tick, decrementing duration for temporary hazards.
    /// </summary>
    /// <returns>True if the hazard expired this tick; false otherwise.</returns>
    /// <remarks>
    /// Permanent hazards (Duration &lt; 0) are never affected by this method.
    /// When a temporary hazard's duration reaches 0, it is deactivated.
    /// </remarks>
    public bool ProcessTurnTick()
    {
        // Permanent hazards don't tick
        if (IsPermanent)
            return false;

        // Already inactive
        if (!IsActive)
            return false;

        Duration--;

        if (Duration <= 0)
        {
            IsActive = false;
            return true; // Hazard expired
        }

        return false;
    }

    /// <summary>
    /// Deactivates this hazard zone.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }

    /// <summary>
    /// Checks if this hazard matches the specified keyword (case-insensitive).
    /// </summary>
    /// <param name="keyword">The keyword to match.</param>
    /// <returns>True if the hazard matches the keyword.</returns>
    public bool MatchesKeyword(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return false;

        var lowerKeyword = keyword.ToLowerInvariant();

        // Check explicit keywords
        if (Keywords.Any(k => k.Equals(lowerKeyword, StringComparison.OrdinalIgnoreCase)))
            return true;

        // Check name contains keyword
        if (Name.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            return true;

        // Check definition ID
        if (DefinitionId.Contains(lowerKeyword, StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
    }

    /// <summary>
    /// Gets a thematic description of the hazard type.
    /// </summary>
    /// <returns>A descriptive string for the hazard type.</returns>
    public string GetHazardTypeDescription() => HazardType switch
    {
        HazardType.PoisonGas => "Poisonous gas fills the area",
        HazardType.Fire => "Flames burn throughout the area",
        HazardType.Ice => "Bitter cold permeates the area",
        HazardType.Spikes => "Sharp spikes cover the ground",
        HazardType.AcidPool => "Corrosive acid pools on the floor",
        HazardType.Darkness => "Magical darkness obscures vision",
        HazardType.Electricity => "Electrical energy crackles through the air",
        HazardType.Radiant => "Radiant energy fills the area",
        HazardType.Necrotic => "Necrotic energy drains life force",
        _ => "An unknown hazard affects this area"
    };

    /// <summary>
    /// Returns a string representation of this hazard zone.
    /// </summary>
    public override string ToString() =>
        $"{Name} ({HazardType}, {(IsPermanent ? "permanent" : $"{Duration} turns")})";
}
