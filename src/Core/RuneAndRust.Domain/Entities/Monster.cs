using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.Entities;

public class Monster : IEntity
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public int Health { get; private set; }
    public int MaxHealth { get; private set; }
    public Stats Stats { get; private set; }
    public bool IsDefeated => Health <= 0;
    public bool IsAlive => Health > 0;

    private Monster() { } // For EF Core

    public Monster(string name, string description, int maxHealth, Stats stats)
    {
        Id = Guid.NewGuid();
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        MaxHealth = maxHealth > 0 ? maxHealth : throw new ArgumentOutOfRangeException(nameof(maxHealth));
        Health = maxHealth;
        Stats = stats;
    }

    public int TakeDamage(int damage)
    {
        if (damage < 0)
            throw new ArgumentOutOfRangeException(nameof(damage), "Damage cannot be negative");

        var actualDamage = Math.Max(0, damage - Stats.Defense);
        Health = Math.Max(0, Health - actualDamage);
        return actualDamage;
    }

    public static Monster CreateGoblin() => new(
        "Goblin",
        "A small, green creature with sharp teeth and beady eyes. It looks hostile.",
        30,
        new Stats(30, 8, 2)
    );

    public override string ToString() => $"{Name} (HP: {Health}/{MaxHealth})";
}
