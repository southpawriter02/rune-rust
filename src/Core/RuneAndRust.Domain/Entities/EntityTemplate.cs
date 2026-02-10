using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.Entities;

/// <summary>
/// A template for spawning entities (monsters) via the threat budget system.
/// </summary>
public class EntityTemplate : IEntity
{
    public Guid Id { get; private set; }
    public string EntityId { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string FactionId { get; private set; }
    public Biome Biome { get; private set; }
    public int Cost { get; private set; }
    public EntityRole Role { get; private set; }
    public Stats BaseStats { get; private set; }

    private readonly List<string> _tags = [];

    public IReadOnlyList<string> Tags => _tags.AsReadOnly();

    private EntityTemplate()
    {
        EntityId = null!;
        Name = null!;
        Description = null!;
        FactionId = null!;
    } // For EF Core

    public EntityTemplate(
        string entityId,
        string name,
        string description,
        string factionId,
        Biome biome,
        int cost,
        EntityRole role,
        Stats baseStats)
    {
        if (string.IsNullOrWhiteSpace(entityId))
            throw new ArgumentException("Entity ID cannot be empty", nameof(entityId));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty", nameof(description));
        if (string.IsNullOrWhiteSpace(factionId))
            throw new ArgumentException("Faction ID cannot be empty", nameof(factionId));
        if (cost < 1)
            throw new ArgumentOutOfRangeException(nameof(cost), "Cost must be at least 1");

        Id = Guid.NewGuid();
        EntityId = entityId;
        Name = name;
        Description = description;
        FactionId = factionId;
        Biome = biome;
        Cost = cost;
        Role = role;
        BaseStats = baseStats;
    }

    /// <summary>
    /// Adds a tag to this entity template.
    /// </summary>
    public void AddTag(string tag)
    {
        if (!string.IsNullOrWhiteSpace(tag) && !_tags.Contains(tag))
            _tags.Add(tag);
    }

    /// <summary>
    /// Adds multiple tags to this entity template.
    /// </summary>
    public void AddTags(IEnumerable<string> tags)
    {
        foreach (var tag in tags)
            AddTag(tag);
    }

    /// <summary>
    /// Checks if this entity can be afforded with the given budget.
    /// </summary>
    public bool CanAfford(int budget) => Cost <= budget;

    /// <summary>
    /// Checks if this entity belongs to a specific faction.
    /// </summary>
    public bool BelongsToFaction(string factionId) =>
        FactionId.Equals(factionId, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Checks if this entity is compatible with the given biome.
    /// </summary>
    public bool IsCompatibleWithBiome(Biome biome) => Biome == biome;

    /// <summary>
    /// Creates a Monster instance from this template.
    /// </summary>
    public Monster CreateMonster()
    {
        return new Monster(Name, Description, BaseStats.MaxHealth, BaseStats);
    }

    /// <summary>
    /// Factory method for creating swarm units.
    /// </summary>
    public static EntityTemplate CreateSwarm(
        string entityId,
        string name,
        string description,
        string factionId,
        Biome biome,
        int cost,
        Stats stats)
    {
        if (cost > 5)
            throw new ArgumentOutOfRangeException(nameof(cost), "Swarm units should cost 1-5");

        var template = new EntityTemplate(entityId, name, description, factionId, biome, cost, EntityRole.Swarm, stats);
        template.AddTag("Swarm");
        return template;
    }

    /// <summary>
    /// Factory method for creating grunt units.
    /// </summary>
    public static EntityTemplate CreateGrunt(
        string entityId,
        string name,
        string description,
        string factionId,
        Biome biome,
        int cost,
        EntityRole role,
        Stats stats)
    {
        if (role == EntityRole.Swarm || role == EntityRole.Elite || role == EntityRole.Boss)
            throw new ArgumentException("Use specific factory for Swarm, Elite, or Boss units", nameof(role));

        var template = new EntityTemplate(entityId, name, description, factionId, biome, cost, role, stats);
        template.AddTag("Grunt");
        return template;
    }

    /// <summary>
    /// Factory method for creating elite units.
    /// </summary>
    public static EntityTemplate CreateElite(
        string entityId,
        string name,
        string description,
        string factionId,
        Biome biome,
        int cost,
        Stats stats)
    {
        if (cost < 40)
            throw new ArgumentOutOfRangeException(nameof(cost), "Elite units should cost 40+");

        var template = new EntityTemplate(entityId, name, description, factionId, biome, cost, EntityRole.Elite, stats);
        template.AddTag("Elite");
        return template;
    }

    /// <summary>
    /// Factory method for creating boss units.
    /// </summary>
    public static EntityTemplate CreateBoss(
        string entityId,
        string name,
        string description,
        string factionId,
        Biome biome,
        int cost,
        Stats stats)
    {
        if (cost < 100)
            throw new ArgumentOutOfRangeException(nameof(cost), "Boss units should cost 100+");

        var template = new EntityTemplate(entityId, name, description, factionId, biome, cost, EntityRole.Boss, stats);
        template.AddTag("Boss");
        return template;
    }

    public override string ToString() => $"{Name} ({FactionId}, Cost: {Cost}, {Role})";
}
