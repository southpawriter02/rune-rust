# ADR-003: Entity Framework Core Integration

**Status:** Accepted
**Date:** 2026-01-06
**Deciders:** Development Team

## Context

Rune and Rust needs to persist game sessions to allow players to save and resume their progress. The persistence layer must:

- Store complete game state (player, dungeon, monsters, inventory)
- Support save/load operations without data loss
- Work with our Clean Architecture design
- Not pollute domain entities with persistence concerns
- Be testable with in-memory alternatives

## Decision

We will use Entity Framework Core for persistence with the following conventions:

### Entity Design

Domain entities will support EF Core by including:

1. **Private Parameterless Constructor**: Required by EF Core for materialization
2. **Public Constructor with Validation**: Used by application code
3. **Private Setters or init Properties**: Maintain encapsulation while allowing EF Core to set values

### Repository Pattern

- `IGameRepository` interface defined in Application layer
- `EfGameRepository` implementation in Infrastructure layer
- Repository returns domain entities, not DTOs

### Database Context

- `GameDbContext` in Infrastructure layer
- Entity configurations via `IEntityTypeConfiguration<T>`
- Owned types for value objects

### In-Memory Alternative

- `InMemoryGameRepository` for testing and development
- Implements same interface as EF Core repository

## Consequences

### Positive

- **ORM Benefits**: Automatic change tracking, migrations, LINQ support
- **Type Safety**: Compile-time checking of queries
- **Testability**: In-memory provider for unit tests
- **Encapsulation Preserved**: Private constructors keep domain logic intact
- **Clean Architecture**: Persistence details stay in Infrastructure layer

### Negative

- **Constructor Complexity**: Two constructors per entity (private for EF, public for app)
- **Configuration Overhead**: Entity configurations needed for complex types
- **EF Core Constraints**: Some patterns require workarounds (private collections, etc.)
- **Learning Curve**: Team must understand EF Core conventions

### Neutral

- Migrations managed via EF Core tooling
- SQLite used for development, can switch to other providers

## Implementation Details

### Entity Constructor Pattern

```csharp
public class Player
{
    // Private constructor for EF Core
    private Player() { }

    // Public constructor with validation for application use
    public Player(string name, int health, int attack, int defense)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name required", nameof(name));
        if (health <= 0)
            throw new ArgumentOutOfRangeException(nameof(health));

        Id = Guid.NewGuid();
        Name = name;
        MaxHealth = health;
        CurrentHealth = health;
        Attack = attack;
        Defense = defense;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = "";
    public int MaxHealth { get; private set; }
    public int CurrentHealth { get; private set; }
    public int Attack { get; private set; }
    public int Defense { get; private set; }
}
```

### Value Object Configuration

```csharp
public class PlayerConfiguration : IEntityTypeConfiguration<Player>
{
    public void Configure(EntityTypeBuilder<Player> builder)
    {
        builder.HasKey(p => p.Id);

        // Configure owned value object
        builder.OwnsOne(p => p.Position, pos =>
        {
            pos.Property(p => p.X).HasColumnName("PositionX");
            pos.Property(p => p.Y).HasColumnName("PositionY");
        });

        // Configure collection
        builder.HasMany(p => p.Inventory)
            .WithOne()
            .HasForeignKey("PlayerId");
    }
}
```

### Repository Interface

```csharp
// Application layer
public interface IGameRepository
{
    Task<GameSession?> GetSessionAsync(Guid id);
    Task SaveSessionAsync(GameSession session);
    Task<IEnumerable<GameSessionSummary>> GetSessionListAsync();
    Task DeleteSessionAsync(Guid id);
}
```

### DbContext Registration

```csharp
// Infrastructure layer
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<GameDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<IGameRepository, EfGameRepository>();

        return services;
    }
}
```

## Alternatives Considered

### Alternative 1: Dapper (Micro-ORM)

Use Dapper for raw SQL with mapping.

**Rejected because:**
- Manual SQL for every operation
- No change tracking
- More code for complex object graphs

### Alternative 2: JSON File Persistence

Serialize game state to JSON files.

**Rejected because:**
- No query capability
- Full load/save on every operation
- Concurrency issues with file access

### Alternative 3: LiteDB

Use LiteDB document database.

**Rejected because:**
- Less tooling support than EF Core
- Team more familiar with EF Core
- Document model less natural for relational game data

### Alternative 4: No Persistence (In-Memory Only)

Keep all state in memory, no save/load.

**Rejected because:**
- Poor user experience
- Players lose progress on exit
- Not viable for a game application

## Related

- [ADR-001](ADR-001-clean-architecture.md): Clean Architecture
- [ADR-006](ADR-006-definition-pattern.md): Definition Entity Pattern (configuration vs. persistence entities)
