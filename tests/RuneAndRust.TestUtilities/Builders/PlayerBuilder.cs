using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.TestUtilities.Builders;

/// <summary>
/// Fluent builder for creating test Player instances.
/// </summary>
/// <remarks>
/// Provides sensible defaults for all properties to minimize test setup.
/// Use method chaining to customize the player as needed.
/// </remarks>
public class PlayerBuilder
{
    private string _name = "TestPlayer";
    private Stats _stats = Stats.Default;
    private PlayerAttributes _attributes = PlayerAttributes.Default;
    private string _raceId = "human";
    private string _backgroundId = "soldier";
    private int? _currentHealth;
    private Position? _position;
    private readonly List<Item> _inventoryItems = [];
    private string? _classId;
    private string? _archetypeId;
    private readonly Dictionary<string, (int maximum, int? current, bool startAtZero)> _resources = new();
    private readonly List<(string abilityId, bool isUnlocked, int cooldown)> _abilities = [];
    private int _level = 1;
    private int _experience = 0;

    /// <summary>
    /// Creates a new PlayerBuilder with default values.
    /// </summary>
    public static PlayerBuilder Create() => new();

    /// <summary>
    /// Sets the player name.
    /// </summary>
    public PlayerBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    /// <summary>
    /// Sets the player stats.
    /// </summary>
    public PlayerBuilder WithStats(Stats stats)
    {
        _stats = stats;
        return this;
    }

    /// <summary>
    /// Sets the player stats using individual values.
    /// </summary>
    public PlayerBuilder WithStats(int maxHealth, int attack, int defense)
    {
        _stats = new Stats(maxHealth, attack, defense);
        return this;
    }

    /// <summary>
    /// Sets the player attributes.
    /// </summary>
    public PlayerBuilder WithAttributes(PlayerAttributes attributes)
    {
        _attributes = attributes;
        return this;
    }

    /// <summary>
    /// Sets the player attributes using individual values.
    /// </summary>
    public PlayerBuilder WithAttributes(int might, int fortitude, int will, int wits, int finesse)
    {
        _attributes = new PlayerAttributes(might, fortitude, will, wits, finesse);
        return this;
    }

    /// <summary>
    /// Sets the player's race ID.
    /// </summary>
    public PlayerBuilder WithRace(string raceId)
    {
        _raceId = raceId;
        return this;
    }

    /// <summary>
    /// Sets the player's background ID.
    /// </summary>
    public PlayerBuilder WithBackground(string backgroundId)
    {
        _backgroundId = backgroundId;
        return this;
    }

    /// <summary>
    /// Sets the current health (player will take damage to reach this value if below max).
    /// </summary>
    public PlayerBuilder WithCurrentHealth(int health)
    {
        _currentHealth = health;
        return this;
    }

    /// <summary>
    /// Sets the player position.
    /// </summary>
    public PlayerBuilder AtPosition(int x, int y)
    {
        _position = new Position(x, y);
        return this;
    }

    /// <summary>
    /// Adds an item to the player's inventory.
    /// </summary>
    public PlayerBuilder WithItem(Item item)
    {
        _inventoryItems.Add(item);
        return this;
    }

    /// <summary>
    /// Sets the player's class and archetype.
    /// </summary>
    public PlayerBuilder WithClass(string archetypeId, string classId)
    {
        _archetypeId = archetypeId;
        _classId = classId;
        return this;
    }

    /// <summary>
    /// Adds a resource pool to the player.
    /// </summary>
    public PlayerBuilder WithResource(string resourceTypeId, int maximum, int? current = null, bool startAtZero = false)
    {
        _resources[resourceTypeId] = (maximum, current, startAtZero);
        return this;
    }

    /// <summary>
    /// Adds an ability to the player.
    /// </summary>
    public PlayerBuilder WithAbility(string abilityId, bool isUnlocked = true, int cooldown = 0)
    {
        _abilities.Add((abilityId, isUnlocked, cooldown));
        return this;
    }

    /// <summary>
    /// Sets the player's level.
    /// </summary>
    public PlayerBuilder WithLevel(int level)
    {
        _level = level;
        return this;
    }

    /// <summary>
    /// Sets the player's experience points.
    /// </summary>
    public PlayerBuilder WithExperience(int experience)
    {
        _experience = experience;
        return this;
    }

    /// <summary>
    /// Builds the Player instance.
    /// </summary>
    public Player Build()
    {
        var player = new Player(_name, _raceId, _backgroundId, _attributes, stats: _stats);

        if (_classId != null && _archetypeId != null)
        {
            player.SetClass(_archetypeId, _classId);
        }

        if (_position.HasValue)
        {
            player.MoveTo(_position.Value);
        }

        foreach (var item in _inventoryItems)
        {
            player.TryPickUpItem(item);
        }

        foreach (var (resourceTypeId, (maximum, current, startAtZero)) in _resources)
        {
            player.InitializeResource(resourceTypeId, maximum, startAtZero);
            if (current.HasValue)
            {
                var pool = player.GetResource(resourceTypeId);
                if (pool != null)
                {
                    // Calculate how much to spend to get to current value
                    var amountToSpend = pool.Current - current.Value;
                    if (amountToSpend > 0)
                    {
                        pool.Spend(amountToSpend);
                    }
                }
            }
        }

        foreach (var (abilityId, isUnlocked, cooldown) in _abilities)
        {
            var ability = PlayerAbility.Create(abilityId, isUnlocked);
            if (cooldown > 0)
            {
                ability.Use(cooldown);
            }
            player.AddAbility(ability);
        }

        if (_level > 1)
        {
            player.SetLevel(_level);
        }

        if (_experience > 0)
        {
            player.AddExperience(_experience);
        }

        // Apply damage to reach target health
        if (_currentHealth.HasValue && _currentHealth.Value < _stats.MaxHealth)
        {
            var damageNeeded = _stats.MaxHealth - _currentHealth.Value;
            // Apply raw damage (bypassing defense) to get to target health
            // We need to account for defense in the damage calculation
            player.TakeDamage(damageNeeded + _stats.Defense);
        }

        return player;
    }
}
