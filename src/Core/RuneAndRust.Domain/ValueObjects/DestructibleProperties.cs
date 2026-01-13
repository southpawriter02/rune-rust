namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Properties for objects that can be destroyed through damage.
/// </summary>
/// <remarks>
/// DestructibleProperties defines how an interactive object responds to damage.
/// This includes hit points, defense, and damage type modifiers (vulnerabilities,
/// resistances, immunities) that mirror the monster damage system.
/// 
/// <example>
/// Example: Create a fragile wooden crate vulnerable to fire:
/// <code>
/// var props = DestructibleProperties.Create(
///     maxHP: 10,
///     defense: 0,
///     vulnerabilities: new[] { "fire", "slashing" },
///     resistances: new[] { "piercing" },
///     immunities: new[] { "poison" });
/// crate.SetDestructible(props);
/// </code>
/// </example>
/// </remarks>
public class DestructibleProperties
{
    /// <summary>
    /// Gets the maximum hit points of this object.
    /// </summary>
    public int MaxHP { get; private set; }

    /// <summary>
    /// Gets the current hit points.
    /// </summary>
    public int CurrentHP { get; private set; }

    /// <summary>
    /// Gets whether this object is destroyed (HP is 0 or less).
    /// </summary>
    public bool IsDestroyed => CurrentHP <= 0;

    /// <summary>
    /// Gets the defense value that reduces incoming damage.
    /// </summary>
    /// <remarks>
    /// Defense is subtracted from damage after vulnerability/resistance modifiers.
    /// Minimum of 1 damage is always dealt if attack would deal any damage.
    /// </remarks>
    public int Defense { get; private set; }

    /// <summary>
    /// Gets damage types that deal double damage.
    /// </summary>
    /// <remarks>
    /// Comparison is case-insensitive.
    /// Vulnerability is checked after immunity but before resistance.
    /// </remarks>
    public IReadOnlyList<string> Vulnerabilities { get; private set; } = Array.Empty<string>();

    /// <summary>
    /// Gets damage types that deal half damage.
    /// </summary>
    /// <remarks>
    /// Comparison is case-insensitive.
    /// Resistance is applied after immunity check.
    /// </remarks>
    public IReadOnlyList<string> Resistances { get; private set; } = Array.Empty<string>();

    /// <summary>
    /// Gets damage types that deal no damage.
    /// </summary>
    /// <remarks>
    /// Comparison is case-insensitive.
    /// Immunity is checked first, before vulnerability or resistance.
    /// </remarks>
    public IReadOnlyList<string> Immunities { get; private set; } = Array.Empty<string>();

    /// <summary>
    /// Gets the loot table ID for items dropped when destroyed.
    /// </summary>
    /// <remarks>
    /// If null, no loot is dropped. The loot table is resolved by the game's
    /// loot generation system.
    /// </remarks>
    public string? LootTableId { get; private set; }

    /// <summary>
    /// Gets the percentage of HP remaining (0-100).
    /// </summary>
    public double HealthPercentage => MaxHP > 0 ? (double)CurrentHP / MaxHP * 100 : 0;

    /// <summary>
    /// Private constructor for factory methods.
    /// </summary>
    private DestructibleProperties() { }

    /// <summary>
    /// Creates destructible properties with full configuration.
    /// </summary>
    /// <param name="maxHP">Maximum hit points (must be positive).</param>
    /// <param name="defense">Defense value that reduces damage (default 0).</param>
    /// <param name="vulnerabilities">Damage types that deal double damage.</param>
    /// <param name="resistances">Damage types that deal half damage.</param>
    /// <param name="immunities">Damage types that deal no damage.</param>
    /// <param name="lootTableId">Optional loot table for drops on destruction.</param>
    /// <returns>A new DestructibleProperties instance.</returns>
    /// <exception cref="ArgumentException">Thrown when maxHP is not positive.</exception>
    public static DestructibleProperties Create(
        int maxHP,
        int defense = 0,
        IEnumerable<string>? vulnerabilities = null,
        IEnumerable<string>? resistances = null,
        IEnumerable<string>? immunities = null,
        string? lootTableId = null)
    {
        if (maxHP <= 0)
            throw new ArgumentException("MaxHP must be positive.", nameof(maxHP));

        return new DestructibleProperties
        {
            MaxHP = maxHP,
            CurrentHP = maxHP,
            Defense = Math.Max(0, defense),
            Vulnerabilities = vulnerabilities?.Select(v => v.ToLowerInvariant()).ToList() ?? [],
            Resistances = resistances?.Select(r => r.ToLowerInvariant()).ToList() ?? [],
            Immunities = immunities?.Select(i => i.ToLowerInvariant()).ToList() ?? [],
            LootTableId = lootTableId
        };
    }

    /// <summary>
    /// Creates weak destructible properties (low HP, no defenses).
    /// </summary>
    /// <param name="maxHP">Maximum hit points (default 10).</param>
    /// <param name="lootTableId">Optional loot table for drops.</param>
    /// <returns>A new DestructibleProperties for weak objects.</returns>
    /// <remarks>
    /// Use for: wooden crates, spider webs, thin barriers.
    /// </remarks>
    public static DestructibleProperties Weak(int maxHP = 10, string? lootTableId = null) =>
        Create(maxHP, defense: 0, lootTableId: lootTableId);

    /// <summary>
    /// Creates sturdy destructible properties (higher HP, some defense).
    /// </summary>
    /// <param name="maxHP">Maximum hit points (default 25).</param>
    /// <param name="defense">Defense value (default 2).</param>
    /// <param name="lootTableId">Optional loot table for drops.</param>
    /// <returns>A new DestructibleProperties for sturdy objects.</returns>
    /// <remarks>
    /// Use for: reinforced crates, barricades, heavy furniture.
    /// </remarks>
    public static DestructibleProperties Sturdy(int maxHP = 25, int defense = 2, string? lootTableId = null) =>
        Create(maxHP, defense, lootTableId: lootTableId);

    /// <summary>
    /// Creates heavily armored destructible properties.
    /// </summary>
    /// <param name="maxHP">Maximum hit points (default 50).</param>
    /// <param name="defense">Defense value (default 5).</param>
    /// <param name="lootTableId">Optional loot table for drops.</param>
    /// <returns>A new DestructibleProperties for armored objects.</returns>
    /// <remarks>
    /// Use for: stone barriers, metal doors, fortified obstacles.
    /// </remarks>
    public static DestructibleProperties Armored(int maxHP = 50, int defense = 5, string? lootTableId = null) =>
        Create(maxHP, defense, lootTableId: lootTableId);

    /// <summary>
    /// Applies damage to this object.
    /// </summary>
    /// <param name="amount">The base damage amount.</param>
    /// <param name="damageType">Optional damage type for modifier checks.</param>
    /// <returns>The actual damage dealt after all modifications.</returns>
    /// <remarks>
    /// Damage calculation order:
    /// 1. Check immunity (returns 0 if immune)
    /// 2. Check resistance (halves damage, rounded down)
    /// 3. Check vulnerability (doubles damage)
    /// 4. Apply defense reduction
    /// 5. Apply minimum 1 damage rule (if base > 0 and not immune)
    /// </remarks>
    public int TakeDamage(int amount, string? damageType = null)
    {
        if (IsDestroyed) return 0;
        if (amount <= 0) return 0;

        var finalDamage = amount;
        var normalizedType = damageType?.ToLowerInvariant();

        // Apply damage type modifiers
        if (!string.IsNullOrEmpty(normalizedType))
        {
            // Check immunity first (no damage)
            if (Immunities.Contains(normalizedType, StringComparer.OrdinalIgnoreCase))
                return 0;

            // Check vulnerability (double damage) - applied before resistance
            if (Vulnerabilities.Contains(normalizedType, StringComparer.OrdinalIgnoreCase))
            {
                finalDamage = amount * 2;
            }
            // Check resistance (half damage, rounded down)
            else if (Resistances.Contains(normalizedType, StringComparer.OrdinalIgnoreCase))
            {
                finalDamage = amount / 2;
            }
        }

        // Apply defense reduction
        finalDamage = Math.Max(0, finalDamage - Defense);

        // Minimum 1 damage if attack would deal any (and not immune)
        if (finalDamage == 0 && amount > 0)
            finalDamage = 1;

        CurrentHP = Math.Max(0, CurrentHP - finalDamage);

        return finalDamage;
    }

    /// <summary>
    /// Checks if this object is immune to a damage type.
    /// </summary>
    /// <param name="damageType">The damage type to check.</param>
    /// <returns>True if immune; otherwise false.</returns>
    public bool IsImmuneTo(string damageType)
    {
        if (string.IsNullOrEmpty(damageType)) return false;
        return Immunities.Contains(damageType, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks if this object is resistant to a damage type.
    /// </summary>
    /// <param name="damageType">The damage type to check.</param>
    /// <returns>True if resistant; otherwise false.</returns>
    public bool IsResistantTo(string damageType)
    {
        if (string.IsNullOrEmpty(damageType)) return false;
        return Resistances.Contains(damageType, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks if this object is vulnerable to a damage type.
    /// </summary>
    /// <param name="damageType">The damage type to check.</param>
    /// <returns>True if vulnerable; otherwise false.</returns>
    public bool IsVulnerableTo(string damageType)
    {
        if (string.IsNullOrEmpty(damageType)) return false;
        return Vulnerabilities.Contains(damageType, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Resets HP to maximum (for respawning objects).
    /// </summary>
    public void Repair()
    {
        CurrentHP = MaxHP;
    }

    /// <summary>
    /// Gets a description of the object's condition based on HP percentage.
    /// </summary>
    /// <returns>A string describing the current health state.</returns>
    public string GetConditionDescription()
    {
        if (IsDestroyed)
            return "destroyed";

        var percent = HealthPercentage;
        return percent switch
        {
            >= 90 => "pristine",
            >= 75 => "slightly damaged",
            >= 50 => "damaged",
            >= 25 => "heavily damaged",
            > 0 => "nearly destroyed",
            _ => "destroyed"
        };
    }

    /// <summary>
    /// Returns a string representation of these properties.
    /// </summary>
    /// <returns>A string describing the destructible properties.</returns>
    public override string ToString() =>
        $"HP: {CurrentHP}/{MaxHP} ({GetConditionDescription()}), Defense: {Defense}";
}
