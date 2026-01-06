using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.Entities;

public class Player : IEntity
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public int Health { get; private set; }
    public Stats Stats { get; private set; }
    public Position Position { get; private set; }
    public Inventory Inventory { get; private set; }

    public bool IsAlive => Health > 0;
    public bool IsDead => Health <= 0;

    private Player() { } // For EF Core

    public Player(string name, Stats? stats = null)
    {
        Id = Guid.NewGuid();
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Stats = stats ?? Stats.Default;
        Health = Stats.MaxHealth;
        Position = Position.Origin;
        Inventory = new Inventory();
    }

    public void MoveTo(Position newPosition)
    {
        Position = newPosition;
    }

    public int TakeDamage(int damage)
    {
        if (damage < 0)
            throw new ArgumentOutOfRangeException(nameof(damage), "Damage cannot be negative");

        var actualDamage = Math.Max(0, damage - Stats.Defense);
        Health = Math.Max(0, Health - actualDamage);
        return actualDamage;
    }

    public int Heal(int amount)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Heal amount cannot be negative");

        var previousHealth = Health;
        Health = Math.Min(Stats.MaxHealth, Health + amount);
        return Health - previousHealth;
    }

    public bool TryPickUpItem(Item item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));

        return Inventory.TryAdd(item);
    }

    public override string ToString() => $"{Name} (HP: {Health}/{Stats.MaxHealth})";
}
