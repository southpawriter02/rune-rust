namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents a set of damage type resistances for an entity.
/// </summary>
/// <remarks>
/// <para>Resistances are stored as percentages from -100 to +100:</para>
/// <list type="bullet">
/// <item>+100 = Immune (0% damage taken)</item>
/// <item>+50 = Resistant (50% damage taken)</item>
/// <item>0 = Normal (100% damage taken)</item>
/// <item>-50 = Vulnerable (150% damage taken)</item>
/// <item>-100 = Extremely Vulnerable (200% damage taken)</item>
/// </list>
/// </remarks>
public readonly record struct DamageResistances
{
    private readonly Dictionary<string, int>? _resistances;

    /// <summary>
    /// Gets the resistance values as a read-only dictionary.
    /// </summary>
    public IReadOnlyDictionary<string, int> Values =>
        _resistances ?? new Dictionary<string, int>();

    /// <summary>
    /// Gets an empty resistance set (no resistances or vulnerabilities).
    /// </summary>
    public static DamageResistances None => new();

    /// <summary>
    /// Creates a new DamageResistances from a dictionary.
    /// </summary>
    /// <param name="resistances">Dictionary of damage type ID to resistance percentage.</param>
    /// <remarks>
    /// Values are automatically clamped to the -100 to +100 range.
    /// Keys are normalized to lowercase.
    /// </remarks>
    public DamageResistances(IReadOnlyDictionary<string, int>? resistances = null)
    {
        if (resistances == null || resistances.Count == 0)
        {
            _resistances = null;
        }
        else
        {
            _resistances = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var kvp in resistances)
            {
                if (!string.IsNullOrWhiteSpace(kvp.Key))
                {
                    _resistances[kvp.Key.ToLowerInvariant()] = Math.Clamp(kvp.Value, -100, 100);
                }
            }
        }
    }

    /// <summary>
    /// Gets the resistance percentage for a specific damage type.
    /// </summary>
    /// <param name="damageTypeId">The damage type identifier.</param>
    /// <returns>The resistance percentage (-100 to +100), or 0 if not found.</returns>
    public int GetResistance(string damageTypeId)
    {
        if (string.IsNullOrWhiteSpace(damageTypeId) || _resistances == null)
            return 0;

        return _resistances.TryGetValue(damageTypeId.ToLowerInvariant(), out var val) ? val : 0;
    }

    /// <summary>
    /// Gets the damage multiplier for a specific damage type.
    /// </summary>
    /// <param name="damageTypeId">The damage type identifier.</param>
    /// <returns>The multiplier (0.0 to 2.0, where 1.0 = normal damage).</returns>
    /// <example>
    /// +50 resistance = 0.5 multiplier (half damage)
    /// -50 resistance = 1.5 multiplier (50% extra damage)
    /// </example>
    public float GetMultiplier(string damageTypeId)
    {
        var resistance = GetResistance(damageTypeId);
        return 1.0f - (resistance / 100.0f);
    }

    /// <summary>
    /// Checks if the entity is vulnerable to a damage type.
    /// </summary>
    /// <param name="damageTypeId">The damage type identifier.</param>
    /// <returns>True if resistance is negative (vulnerable).</returns>
    public bool IsVulnerable(string damageTypeId) => GetResistance(damageTypeId) < 0;

    /// <summary>
    /// Checks if the entity is resistant (but not immune) to a damage type.
    /// </summary>
    /// <param name="damageTypeId">The damage type identifier.</param>
    /// <returns>True if resistance is positive but less than 100.</returns>
    public bool IsResistant(string damageTypeId)
    {
        var resistance = GetResistance(damageTypeId);
        return resistance > 0 && resistance < 100;
    }

    /// <summary>
    /// Checks if the entity is immune to a damage type.
    /// </summary>
    /// <param name="damageTypeId">The damage type identifier.</param>
    /// <returns>True if resistance is 100% (immune).</returns>
    public bool IsImmune(string damageTypeId) => GetResistance(damageTypeId) >= 100;

    /// <summary>
    /// Gets all damage types this entity has non-zero resistance to.
    /// </summary>
    /// <returns>Enumerable of damage type IDs with non-zero resistance, ordered by absolute value.</returns>
    public IEnumerable<string> GetSignificantResistances()
    {
        if (_resistances == null)
            return Enumerable.Empty<string>();

        return _resistances
            .Where(kvp => kvp.Value != 0)
            .OrderByDescending(kvp => Math.Abs(kvp.Value))
            .Select(kvp => kvp.Key);
    }

    /// <summary>
    /// Combines this resistance set with another, taking the higher resistance for each type.
    /// </summary>
    /// <param name="other">The other resistance set to combine with.</param>
    /// <returns>A new DamageResistances with combined values.</returns>
    public DamageResistances CombineWith(DamageResistances other)
    {
        var combined = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var kvp in Values)
        {
            combined[kvp.Key] = kvp.Value;
        }

        foreach (var kvp in other.Values)
        {
            if (combined.TryGetValue(kvp.Key, out var existing))
            {
                combined[kvp.Key] = Math.Max(existing, kvp.Value);
            }
            else
            {
                combined[kvp.Key] = kvp.Value;
            }
        }

        return new DamageResistances(combined);
    }

    /// <summary>
    /// Creates a DamageResistances from a simple dictionary.
    /// </summary>
    /// <param name="resistances">Dictionary of damage type ID to resistance percentage.</param>
    /// <returns>A new DamageResistances instance.</returns>
    public static DamageResistances FromDictionary(Dictionary<string, int> resistances)
    {
        return new DamageResistances(resistances);
    }
}
