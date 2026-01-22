---
id: SPEC-REPO-001
title: Repository Pattern
version: 1.0.1
status: Implemented
related_specs: [SPEC-SAVE-001, SPEC-SEED-001, SPEC-MIGRATE-001]
last_updated: 2025-12-24
---

# SPEC-REPO-001: Repository Pattern

> **Version:** 1.0.1
> **Status:** Implemented
> **Services:** IRepository<T>, GenericRepository<T>, 13 Specialized Repositories
> **Location:** `RuneAndRust.Core/Interfaces/`, `RuneAndRust.Persistence/Repositories/`

---

## Overview

The **Repository Pattern** provides a clean abstraction layer between domain services and database operations in Rune & Rust. The system uses **Entity Framework Core** with **PostgreSQL 16** as the backing store, implementing a generic base repository for common CRUD operations and 13 specialized repositories for domain-specific queries.

### Key Responsibilities

1. **Data Access Abstraction**: Decouple services from EF Core implementation details
2. **CRUD Operations**: Provide standard Create, Read, Update, Delete operations for all entities
3. **Specialized Queries**: Offer domain-specific query methods (spatial queries, filtered lookups, aggregations)
4. **Change Tracking**: Leverage EF Core DbContext for implicit Unit of Work pattern
5. **Async-First Design**: All database operations are async (`Task<T>`) for scalability
6. **Logging Integration**: Debug-level logging for all database operations via Serilog

### Architecture Pattern

```
Service Layer → IXxxRepository → XxxRepository → DbContext → PostgreSQL
                      ↓
               GenericRepository<T> (Base CRUD)
                      ↓
                   DbSet<T>
```

**Key Design Decision**: The system uses a **Generic + Specialized** repository approach:
- `IRepository<T>` → `GenericRepository<T>` provides base CRUD
- Specialized interfaces (e.g., `IRoomRepository`) extend with domain-specific queries
- Specialized implementations inherit `GenericRepository<T>` and add custom methods

**Technology Stack**:
- **ORM**: Entity Framework Core (EF Core)
- **Database**: PostgreSQL 16 via Npgsql provider
- **JSON Storage**: PostgreSQL JSONB columns for complex data
- **Lifetime**: Scoped (one DbContext per request/game loop iteration)

---

## Core Concepts

### 1. Generic Repository Interface

**Definition**: Base interface providing standard CRUD operations for any entity type.

**Interface** (`IRepository<T>`):
```csharp
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(Guid id);
    Task SaveChangesAsync();
}
```

**Location**: `/Users/ryan/Documents/GitHub/rune-rust/RuneAndRust.Core/Interfaces/IRepository.cs`

**Characteristics**:
- **Generic Constraint**: `where T : class` (reference types only, required for EF Core)
- **Nullable Returns**: `GetByIdAsync` returns `T?` (null if not found)
- **Async Methods**: All operations return `Task` for non-blocking I/O
- **Explicit SaveChanges**: Changes are not persisted until `SaveChangesAsync()` is called

**Benefits**:
- **Consistency**: All entities use same CRUD interface
- **Testability**: Easy to mock for unit tests
- **Flexibility**: Can swap EF Core implementation without changing services

---

### 2. Generic Repository Implementation

**Definition**: Base implementation wrapping EF Core `DbSet<T>` operations.

**Implementation** (`GenericRepository<T>`):
```csharp
public class GenericRepository<T> : IRepository<T> where T : class
{
    protected readonly RuneAndRustDbContext _context;
    protected readonly DbSet<T> _dbSet;
    protected readonly ILogger<GenericRepository<T>> _logger;

    public GenericRepository(RuneAndRustDbContext context, ILogger<GenericRepository<T>> logger)
    {
        _context = context;
        _dbSet = context.Set<T>();
        _logger = logger;
    }

    public async Task<T?> GetByIdAsync(Guid id)
    {
        _logger.LogDebug("GetByIdAsync: {Type} with Id {Id}", typeof(T).Name, id);
        return await _dbSet.FindAsync(id);
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        _logger.LogDebug("GetAllAsync: {Type}", typeof(T).Name);
        return await _dbSet.ToListAsync();
    }

    public async Task AddAsync(T entity)
    {
        _logger.LogDebug("AddAsync: {Type}", typeof(T).Name);
        await _dbSet.AddAsync(entity);
    }

    public async Task UpdateAsync(T entity)
    {
        _logger.LogDebug("UpdateAsync: {Type}", typeof(T).Name);
        _dbSet.Update(entity);
        await Task.CompletedTask; // Update is synchronous in EF Core
    }

    public async Task DeleteAsync(Guid id)
    {
        _logger.LogDebug("DeleteAsync: {Type} with Id {Id}", typeof(T).Name, id);
        var entity = await GetByIdAsync(id);
        if (entity != null)
            _dbSet.Remove(entity);
    }

    public async Task SaveChangesAsync()
    {
        _logger.LogDebug("SaveChangesAsync: Persisting changes");
        await _context.SaveChangesAsync();
    }
}
```

**Location**: `/Users/ryan/Documents/GitHub/rune-rust/RuneAndRust.Persistence/Repositories/GenericRepository.cs`

**Protected Members**:
- `_context` - RuneAndRustDbContext (shared across repositories in same scope)
- `_dbSet` - EF Core DbSet<T> for entity type
- `_logger` - Serilog logger for debug output

**Key Behaviors**:
- **FindAsync**: Uses primary key lookup (optimal performance)
- **ToListAsync**: Materializes entire table (use specialized queries for large tables)
- **Update**: Marks entity as modified in change tracker
- **SaveChangesAsync**: Flushes all pending changes to database in single transaction

---

### 3. Specialized Repository Interfaces

**Purpose**: Extend base CRUD with domain-specific query methods.

**13 Specialized Repositories**:

| Interface | Purpose | Key Methods |
|-----------|---------|-------------|
| `ICharacterRepository` | Player character management | `GetByNameAsync`, `NameExistsAsync`, `GetAllOrderedByCreationAsync`, `GetMostRecentAsync` |
| `IRoomRepository` | Spatial navigation | `GetByPositionAsync`, `GetStartingRoomAsync`, `PositionExistsAsync`, `GetRoomsInGridAsync`, `AddRangeAsync`, `GetAllRoomsAsync`, `ClearAllRoomsAsync` |
| `IInventoryRepository` | Equipment/item management | `GetByCharacterIdAsync`, `GetEquippedItemsAsync`, `GetEquippedInSlotAsync`, `GetTotalWeightAsync`, `FindByTagAsync`, `GetByCharacterAndItemAsync`, `FindByItemNameAsync`, `ClearInventoryAsync` |
| `ISaveGameRepository` | Save slot management | `GetBySlotAsync`, `SlotExistsAsync`, `GetAllOrderedByLastPlayedAsync` |
| `IDataCaptureRepository` | Journal fragments | `GetByCharacterIdAsync`, `GetByEntryIdAsync`, `GetFragmentCountAsync`, `GetUnassignedAsync`, `GetDiscoveredEntryIdsAsync` |
| `IItemRepository` | Item queries | `GetByQualityAsync`, `GetByTypeAsync`, `GetEquipmentBySlotAsync`, `GetByNameAsync`, `GetAllEquipmentAsync` |
| `ICodexEntryRepository` | Lore entries | `GetByCategoryAsync` (extends IRepository) |
| `IInteractableObjectRepository` | Room objects | `AddRangeAsync`, `ClearRoomObjectsAsync` (extends IRepository) |
| `IActiveAbilityRepository` | Combat abilities | `GetByArchetypeAsync`, `GetByTierAsync` |
| `IRoomTemplateRepository` | Dynamic room generation | `GetByIdAsync(string templateId)`, `GetByBiomeAsync`, `UpsertAsync` |
| `IBiomeDefinitionRepository` | Biome configuration | `GetByIdAsync(string biomeId)`, `GetAllAsync`, `UpsertAsync`, `GetElementsForBiomeAsync` |
| `IBiomeElementRepository` | Biome spawn elements | `GetByBiomeIdAsync`, `GetByElementTypeAsync` |

**Pattern**:
```csharp
public interface ICharacterRepository : IRepository<Character>
{
    Task<Character?> GetByNameAsync(string name);
    Task<bool> NameExistsAsync(string name);
    Task<IEnumerable<Character>> GetAllOrderedByCreationAsync();
    Task<Character?> GetMostRecentAsync();
}
```

**Benefits**:
- **Discoverability**: Domain-specific methods are explicitly named
- **Optimization**: Specialized queries use LINQ for efficient SQL generation
- **Encapsulation**: Query logic lives in repository, not scattered across services

---

### 4. DbContext Architecture

**Definition**: EF Core context managing all entity sets and configuration.

**Implementation** (`RuneAndRustDbContext`):
```csharp
public class RuneAndRustDbContext : DbContext
{
    // Entity Sets (15+)
    public DbSet<SaveGame> SaveGames { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<Character> Characters { get; set; }
    public DbSet<InteractableObject> InteractableObjects { get; set; }
    public DbSet<DynamicHazard> DynamicHazards { get; set; }
    public DbSet<Item> Items { get; set; }
    public DbSet<Equipment> Equipment { get; set; }
    public DbSet<InventoryItem> InventoryItems { get; set; }
    public DbSet<CodexEntry> CodexEntries { get; set; }
    public DbSet<DataCapture> DataCaptures { get; set; }
    public DbSet<ActiveAbility> ActiveAbilities { get; set; }
    public DbSet<ItemProperty> ItemProperties { get; set; }
    public DbSet<AmbientCondition> AmbientConditions { get; set; }
    public DbSet<HazardTemplate> HazardTemplates { get; set; }
    public DbSet<RoomTemplate> RoomTemplates { get; set; }
    public DbSet<BiomeDefinition> BiomeDefinitions { get; set; }
    public DbSet<BiomeElement> BiomeElements { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // TPH (Table-per-Hierarchy) inheritance
        modelBuilder.Entity<InteractableObject>()
            .HasDiscriminator<string>("Discriminator")
            .HasValue<InteractableObject>("InteractableObject")
            .HasValue<DynamicHazard>("DynamicHazard");

        modelBuilder.Entity<Item>()
            .HasDiscriminator<string>("Discriminator")
            .HasValue<Item>("Item")
            .HasValue<Equipment>("Equipment");

        // JSONB columns for complex data
        modelBuilder.Entity<Room>()
            .Property(r => r.Exits)
            .HasColumnType("jsonb");

        modelBuilder.Entity<Item>()
            .Property(i => i.Tags)
            .HasColumnType("jsonb");

        modelBuilder.Entity<Equipment>()
            .Property(e => e.Bonuses)
            .HasColumnType("jsonb");

        // Owned types (value objects)
        modelBuilder.Entity<Room>()
            .OwnsOne(r => r.Position);

        // Indexes for query optimization
        modelBuilder.Entity<SaveGame>()
            .HasIndex(s => s.SlotNumber)
            .IsUnique();

        modelBuilder.Entity<Character>()
            .HasIndex(c => c.Name);

        // Foreign key cascades
        modelBuilder.Entity<InventoryItem>()
            .HasOne(ii => ii.Character)
            .WithMany()
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

**Location**: `/Users/ryan/Documents/GitHub/rune-rust/RuneAndRust.Persistence/Data/RuneAndRustDbContext.cs`

**Key Configurations**:
- **TPH Inheritance**: Item/Equipment and InteractableObject/DynamicHazard share tables with discriminator columns
- **JSONB Columns**: Dictionaries and lists stored as PostgreSQL JSONB for flexible schemas
- **Owned Types**: Value objects (Coordinate with X, Y, Z) embedded in owning entity's table
- **Unique Indexes**: SaveGame.SlotNumber, Character.Name for fast lookups
- **Cascade Deletes**: InventoryItem deleted when Character deleted

---

### 5. Implicit Unit of Work

**Definition**: DbContext acts as implicit Unit of Work, tracking all entity changes until `SaveChangesAsync()` is called.

**Pattern**:
```csharp
// Service method using multiple repositories (same DbContext scope)
public async Task TransferItemAsync(Guid characterId, Guid itemId, string targetSlot)
{
    var character = await _characterRepository.GetByIdAsync(characterId);
    var inventoryItem = await _inventoryRepository.GetByIdAsync(itemId);

    // Make changes through repositories
    inventoryItem.EquippedSlot = targetSlot;
    inventoryItem.IsEquipped = true;
    await _inventoryRepository.UpdateAsync(inventoryItem);

    character.LastModified = DateTime.UtcNow;
    await _characterRepository.UpdateAsync(character);

    // Single SaveChanges persists both changes atomically
    await _inventoryRepository.SaveChangesAsync();
}
```

**Characteristics**:
- **No Explicit UoW Class**: DbContext already implements pattern
- **Shared Context**: All repositories in same scope share one DbContext instance
- **Atomic Transactions**: Single `SaveChangesAsync()` commits all changes in one transaction
- **Rollback on Exception**: If exception occurs before SaveChanges, no changes are persisted

**Trade-Offs**:
- **Simplicity**: No additional UoW abstraction layer
- **Coupling**: Repositories must be in same DI scope for shared transactions
- **Control**: Less explicit transaction boundaries (mitigated by scoped lifetime)

---

### 6. Query Patterns

**Purpose**: Common patterns for building efficient queries in specialized repositories.

#### Filtering by Field
```csharp
public async Task<Character?> GetByNameAsync(string name)
{
    return await _dbSet
        .FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower());
}
```

#### Ordering
```csharp
public async Task<IEnumerable<Character>> GetAllOrderedByCreationAsync()
{
    return await _dbSet
        .OrderByDescending(c => c.CreatedAt)
        .ToListAsync();
}
```

#### Spatial Queries (Coordinate-based)
```csharp
public async Task<Room?> GetByPositionAsync(Coordinate position)
{
    return await _dbSet
        .FirstOrDefaultAsync(r =>
            r.Position.X == position.X &&
            r.Position.Y == position.Y &&
            r.Position.Z == position.Z);
}

public async Task<IEnumerable<Room>> GetRoomsInGridAsync(
    int z, int minX, int maxX, int minY, int maxY)
{
    return await _dbSet
        .Where(r =>
            r.Position.Z == z &&
            r.Position.X >= minX && r.Position.X <= maxX &&
            r.Position.Y >= minY && r.Position.Y <= maxY)
        .ToListAsync();
}
```

#### Aggregation
```csharp
public async Task<int> GetTotalWeightAsync(Guid characterId)
{
    return await _dbSet
        .Where(ii => ii.CharacterId == characterId)
        .SumAsync(ii => ii.Item.Weight * ii.Quantity);
}

public async Task<int> GetFragmentCountAsync(Guid characterId, Guid entryId)
{
    return await _dbSet
        .Where(dc => dc.CharacterId == characterId && dc.CodexEntryId == entryId)
        .CountAsync();
}
```

#### Eager Loading (Include)
```csharp
public async Task<IEnumerable<InventoryItem>> GetByCharacterIdAsync(Guid characterId)
{
    return await _dbSet
        .Include(ii => ii.Item) // Eager load related Item entity
        .Where(ii => ii.CharacterId == characterId)
        .ToListAsync();
}
```

#### Existence Checks (AnyAsync)
```csharp
public async Task<bool> NameExistsAsync(string name)
{
    return await _dbSet
        .AnyAsync(c => c.Name.ToLower() == name.ToLower());
}

public async Task<bool> PositionExistsAsync(Coordinate position)
{
    return await _dbSet
        .AnyAsync(r =>
            r.Position.X == position.X &&
            r.Position.Y == position.Y &&
            r.Position.Z == position.Z);
}
```

#### JSONB Filtering (In-Memory)
```csharp
// Tags stored as JSONB array - filter in memory after materialization
public async Task<IEnumerable<Item>> FindByTagAsync(string tag)
{
    var items = await _dbSet.ToListAsync();
    return items.Where(i => i.Tags?.Contains(tag) == true);
}
```

**Note**: JSONB filtering is done in-memory due to EF Core JSONB query limitations. For performance-critical paths, consider denormalizing or using raw SQL.

---

## Behaviors

### B-1: GetByIdAsync - Primary Key Lookup

**Signature**: `Task<T?> GetByIdAsync(Guid id)`

**Purpose**: Retrieve single entity by its primary key (Guid).

**Sequence**:
```
1. Log debug message with entity type and ID
2. Call DbSet.FindAsync(id)
3. EF Core checks:
   a. Is entity already tracked in context? → Return tracked entity
   b. Not tracked? → Execute SELECT query
4. Return entity or null if not found
```

**Code**:
```csharp
public async Task<T?> GetByIdAsync(Guid id)
{
    _logger.LogDebug("GetByIdAsync: {Type} with Id {Id}", typeof(T).Name, id);
    return await _dbSet.FindAsync(id);
}
```

**Performance**: O(1) lookup when entity is tracked, O(log n) database index lookup otherwise.

**SQL Generated**:
```sql
SELECT c."Id", c."Name", c."Archetype", c."Level", ...
FROM "Characters" AS c
WHERE c."Id" = @p0
LIMIT 1
```

---

### B-2: GetAllAsync - Full Table Retrieval

**Signature**: `Task<IEnumerable<T>> GetAllAsync()`

**Purpose**: Retrieve all entities of type T.

**Sequence**:
```
1. Log debug message with entity type
2. Call DbSet.ToListAsync()
3. EF Core executes SELECT * query
4. Materialize all rows into entity list
5. Return list
```

**Code**:
```csharp
public async Task<IEnumerable<T>> GetAllAsync()
{
    _logger.LogDebug("GetAllAsync: {Type}", typeof(T).Name);
    return await _dbSet.ToListAsync();
}
```

**Warning**: Avoid for large tables. Use specialized query methods with filtering/pagination.

**SQL Generated**:
```sql
SELECT c."Id", c."Name", c."Archetype", c."Level", ...
FROM "Characters" AS c
```

---

### B-3: AddAsync - Insert New Entity

**Signature**: `Task AddAsync(T entity)`

**Purpose**: Stage new entity for insertion (not persisted until SaveChangesAsync).

**Sequence**:
```
1. Log debug message with entity type
2. Call DbSet.AddAsync(entity)
3. EF Core marks entity as "Added" in change tracker
4. Entity will be INSERTed on next SaveChangesAsync
```

**Code**:
```csharp
public async Task AddAsync(T entity)
{
    _logger.LogDebug("AddAsync: {Type}", typeof(T).Name);
    await _dbSet.AddAsync(entity);
}
```

**Note**: Entity must have valid Id (Guid.NewGuid()) or use database-generated ID.

---

### B-4: UpdateAsync - Modify Existing Entity

**Signature**: `Task UpdateAsync(T entity)`

**Purpose**: Mark entity as modified (not persisted until SaveChangesAsync).

**Sequence**:
```
1. Log debug message with entity type
2. Call DbSet.Update(entity)
3. EF Core marks entity as "Modified" in change tracker
4. All properties will be UPDATEd on next SaveChangesAsync
```

**Code**:
```csharp
public async Task UpdateAsync(T entity)
{
    _logger.LogDebug("UpdateAsync: {Type}", typeof(T).Name);
    _dbSet.Update(entity);
    await Task.CompletedTask; // Synchronous operation wrapped in Task
}
```

**Alternative**: If entity is already tracked, simply modify properties and call SaveChangesAsync (EF Core auto-detects changes).

---

### B-5: DeleteAsync - Remove Entity

**Signature**: `Task DeleteAsync(Guid id)`

**Purpose**: Stage entity for deletion (not persisted until SaveChangesAsync).

**Sequence**:
```
1. Log debug message with entity type and ID
2. Lookup entity by ID (GetByIdAsync)
3. If entity found, call DbSet.Remove(entity)
4. EF Core marks entity as "Deleted" in change tracker
5. Entity will be DELETEd on next SaveChangesAsync
```

**Code**:
```csharp
public async Task DeleteAsync(Guid id)
{
    _logger.LogDebug("DeleteAsync: {Type} with Id {Id}", typeof(T).Name, id);
    var entity = await GetByIdAsync(id);
    if (entity != null)
        _dbSet.Remove(entity);
}
```

**Cascade Behavior**: Depends on FK configuration (DeleteBehavior.Cascade, Restrict, etc.)

---

### B-6: SaveChangesAsync - Persist All Changes

**Signature**: `Task SaveChangesAsync()`

**Purpose**: Commit all pending changes (Add, Update, Delete) to database in single transaction.

**Sequence**:
```
1. Log debug message
2. Call DbContext.SaveChangesAsync()
3. EF Core:
   a. Opens database transaction
   b. Generates SQL for all tracked changes
   c. Executes INSERT/UPDATE/DELETE statements
   d. Commits transaction
4. All tracked entities updated with database-generated values (if any)
5. Change tracker reset to "Unchanged" state
```

**Code**:
```csharp
public async Task SaveChangesAsync()
{
    _logger.LogDebug("SaveChangesAsync: Persisting changes");
    await _context.SaveChangesAsync();
}
```

**Transactional**: All changes succeed or all fail (atomic).

**Performance**: Single round-trip to database for all pending changes.

---

## Restrictions

### R-1: Scoped Lifetime
- **Rule**: All repositories MUST be registered as Scoped in DI container.
- **Rationale**: Ensures single DbContext instance per request, enabling shared transactions.
- **Enforcement**: DI registration in Program.cs uses `AddScoped<>()`.

### R-2: Guid Primary Keys
- **Rule**: All entities MUST use `Guid Id` as primary key.
- **Rationale**: Consistent interface for `IRepository<T>.GetByIdAsync(Guid)`.
- **Exception**: Some lookup tables (RoomTemplate, BiomeDefinition) use string IDs.

### R-3: Async-Only Operations
- **Rule**: All repository methods MUST be async (return `Task<T>`).
- **Rationale**: Non-blocking I/O for database operations.
- **Enforcement**: Interface definitions use `Task<T>` return types.

### R-4: SaveChangesAsync Required
- **Rule**: Changes are NOT persisted until `SaveChangesAsync()` is explicitly called.
- **Rationale**: Allows batching multiple operations in single transaction.
- **Common Mistake**: Forgetting to call SaveChanges after Add/Update/Delete.

---

## Limitations

### L-1: No Cross-Scope Transactions
- **Issue**: Repositories in different DI scopes cannot share transactions.
- **Workaround**: Ensure all related operations occur within same scope.
- **Future Enhancement**: Explicit transaction scope wrapper.

### L-2: JSONB Query Limitations
- **Issue**: EF Core has limited support for querying JSONB array contents.
- **Impact**: `FindByTagAsync` loads all items to memory, then filters.
- **Workaround**: Use raw SQL for complex JSONB queries, or denormalize.

### L-3: No Pagination
- **Issue**: Base `GetAllAsync` retrieves entire table.
- **Impact**: Performance issues for large datasets.
- **Workaround**: Use specialized methods with `Skip/Take` for pagination.

### L-4: No Bulk Operations
- **Issue**: `AddRangeAsync` implemented only in RoomRepository, not in base.
- **Impact**: Inserting many entities requires N calls to AddAsync.
- **Future Enhancement**: Add `AddRangeAsync` to base IRepository<T>.

---

## Use Cases

### UC-1: Character Creation with Validation

**Scenario**: Player creates new character with unique name.

**Actors**: CharacterService, ICharacterRepository

**Sequence**:
```
1. CharacterService receives character creation request
2. Call _characterRepository.NameExistsAsync(name)
3. If name exists, return validation error
4. If name unique:
   a. Create Character entity with Guid.NewGuid()
   b. Call _characterRepository.AddAsync(character)
   c. Call _characterRepository.SaveChangesAsync()
5. Return created character
```

**Code**:
```csharp
public async Task<Character?> CreateCharacterAsync(string name, Archetype archetype)
{
    // Validate unique name
    if (await _characterRepository.NameExistsAsync(name))
    {
        _logger.LogWarning("Character name '{Name}' already exists", name);
        return null;
    }

    // Create entity
    var character = new Character
    {
        Id = Guid.NewGuid(),
        Name = name,
        Archetype = archetype,
        Level = 1,
        CreatedAt = DateTime.UtcNow
    };

    // Persist
    await _characterRepository.AddAsync(character);
    await _characterRepository.SaveChangesAsync();

    _logger.LogInformation("Created character '{Name}' with Id {Id}", name, character.Id);
    return character;
}
```

**SQL Generated**:
```sql
-- Existence check
SELECT EXISTS (SELECT 1 FROM "Characters" WHERE LOWER("Name") = LOWER(@p0))

-- Insert
INSERT INTO "Characters" ("Id", "Name", "Archetype", "Level", "CreatedAt")
VALUES (@p0, @p1, @p2, @p3, @p4)
```

---

### UC-2: Room Spatial Query for Minimap

**Scenario**: Exploration screen requests 3×3 grid of rooms around player for minimap rendering.

**Actors**: ExplorationService, IRoomRepository

**Sequence**:
```
1. ExplorationService has current room position (x=5, y=3, z=0)
2. Calculate grid bounds: minX=4, maxX=6, minY=2, maxY=4
3. Call _roomRepository.GetRoomsInGridAsync(0, 4, 6, 2, 4)
4. RoomRepository executes WHERE clause with bounds
5. Return 0-9 rooms within grid
6. ExplorationService builds minimap from returned rooms
```

**Code**:
```csharp
public async Task<IEnumerable<Room>> GetRoomsInGridAsync(
    int z, int minX, int maxX, int minY, int maxY)
{
    _logger.LogDebug("GetRoomsInGridAsync: z={Z}, x=[{MinX}-{MaxX}], y=[{MinY}-{MaxY}]",
        z, minX, maxX, minY, maxY);

    return await _dbSet
        .Where(r =>
            r.Position.Z == z &&
            r.Position.X >= minX && r.Position.X <= maxX &&
            r.Position.Y >= minY && r.Position.Y <= maxY)
        .ToListAsync();
}
```

**SQL Generated**:
```sql
SELECT r."Id", r."Name", r."Description", r."Position_X", r."Position_Y", r."Position_Z", r."Exits"
FROM "Rooms" AS r
WHERE r."Position_Z" = @p0
  AND r."Position_X" >= @p1 AND r."Position_X" <= @p2
  AND r."Position_Y" >= @p3 AND r."Position_Y" <= @p4
```

**Performance**: Uses composite index on (Z, X, Y) for efficient range query.

---

### UC-3: Inventory Weight Calculation

**Scenario**: Encumbrance system needs total weight of player's inventory.

**Actors**: InventoryService, IInventoryRepository

**Sequence**:
```
1. InventoryService receives weight check request
2. Call _inventoryRepository.GetTotalWeightAsync(characterId)
3. InventoryRepository executes SUM aggregation with JOIN
4. Return total weight (int)
5. InventoryService compares to character's MaxCarryWeight
```

**Code**:
```csharp
public async Task<int> GetTotalWeightAsync(Guid characterId)
{
    _logger.LogDebug("Calculating total inventory weight for character {CharacterId}", characterId);

    return await _dbSet
        .Include(ii => ii.Item)
        .Where(ii => ii.CharacterId == characterId)
        .SumAsync(ii => ii.Item.Weight * ii.Quantity);
}
```

**SQL Generated**:
```sql
SELECT COALESCE(SUM(i."Weight" * ii."Quantity"), 0)
FROM "InventoryItems" AS ii
INNER JOIN "Items" AS i ON ii."ItemId" = i."Id"
WHERE ii."CharacterId" = @p0
```

**Performance**: Database computes SUM, no data transfer to application.

---

### UC-4: Equipped Items by Slot

**Scenario**: Combat system needs to know what weapon is equipped in MainHand slot.

**Actors**: CombatService, IInventoryRepository

**Sequence**:
```
1. CombatService needs attack calculation
2. Call _inventoryRepository.GetEquippedInSlotAsync(characterId, EquipmentSlot.MainHand)
3. InventoryRepository filters by IsEquipped and EquippedSlot
4. Return InventoryItem with included Item/Equipment data
5. CombatService extracts weapon stats for damage calculation
```

**Code**:
```csharp
public async Task<InventoryItem?> GetEquippedInSlotAsync(Guid characterId, EquipmentSlot slot)
{
    _logger.LogDebug("GetEquippedInSlotAsync: Character {CharacterId}, Slot {Slot}",
        characterId, slot);

    return await _dbSet
        .Include(ii => ii.Item)
        .FirstOrDefaultAsync(ii =>
            ii.CharacterId == characterId &&
            ii.IsEquipped &&
            ii.EquippedSlot == slot);
}
```

**SQL Generated**:
```sql
SELECT ii."Id", ii."CharacterId", ii."ItemId", ii."Quantity", ii."IsEquipped", ii."EquippedSlot",
       i."Id", i."Name", i."Weight", i."Discriminator", ...
FROM "InventoryItems" AS ii
INNER JOIN "Items" AS i ON ii."ItemId" = i."Id"
WHERE ii."CharacterId" = @p0 AND ii."IsEquipped" = TRUE AND ii."EquippedSlot" = @p1
LIMIT 1
```

---

### UC-5: Save Game Slot Management

**Scenario**: Title screen needs to display all save slots with character names.

**Actors**: TitleScreenService, ISaveGameRepository

**Sequence**:
```
1. TitleScreenService requests save slot list
2. Call _saveGameRepository.GetAllOrderedByLastPlayedAsync()
3. SaveGameRepository orders by LastPlayed descending
4. Return list of SaveGame entities
5. TitleScreenService builds slot summaries for UI
```

**Code**:
```csharp
public async Task<IEnumerable<SaveGame>> GetAllOrderedByLastPlayedAsync()
{
    _logger.LogDebug("GetAllOrderedByLastPlayedAsync");

    return await _dbSet
        .OrderByDescending(s => s.LastPlayed)
        .ToListAsync();
}
```

**SQL Generated**:
```sql
SELECT s."Id", s."SlotNumber", s."CharacterName", s."CreatedAt", s."LastPlayed", s."SerializedState"
FROM "SaveGames" AS s
ORDER BY s."LastPlayed" DESC
```

---

### UC-6: Biome Element Spawn Query (v0.4.0)

**Scenario**: Dynamic room generator needs enemy spawn configurations for "the_roots" biome.

**Actors**: RoomGeneratorService, IBiomeElementRepository

**Sequence**:
```
1. RoomGeneratorService generating room in "the_roots" biome
2. Call _biomeElementRepository.GetByBiomeIdAsync("the_roots")
3. BiomeElementRepository filters by BiomeId
4. Return list of BiomeElement entities (enemies, hazards, terrain, loot)
5. RoomGeneratorService applies spawn weights and rules
```

**Code**:
```csharp
public async Task<IEnumerable<BiomeElement>> GetByBiomeIdAsync(string biomeId)
{
    _logger.LogDebug("GetByBiomeIdAsync: Biome {BiomeId}", biomeId);

    return await _dbSet
        .Where(e => e.BiomeId == biomeId)
        .ToListAsync();
}

public async Task<IEnumerable<BiomeElement>> GetByElementTypeAsync(string biomeId, ElementType type)
{
    return await _dbSet
        .Where(e => e.BiomeId == biomeId && e.ElementType == type)
        .ToListAsync();
}
```

**SQL Generated**:
```sql
SELECT e."Id", e."BiomeId", e."ElementName", e."ElementType", e."Weight", e."SpawnRules"
FROM "BiomeElements" AS e
WHERE e."BiomeId" = @p0
```

---

## Decision Trees

### DT-1: Repository Selection

**Trigger**: Service needs to access entity data

```
Entity Type?
│
├─ Standard Entity (Character, Room, Item, etc.)
│  ├─ Need specialized query?
│  │  ├─ YES → Use specialized repository (ICharacterRepository, IRoomRepository, etc.)
│  │  │  └─ Query methods available?
│  │  │     ├─ YES → Call specialized method
│  │  │     └─ NO → Add method to specialized interface
│  │  └─ NO → Use base IRepository<T> methods (GetById, GetAll, Add, Update, Delete)
│
├─ InventoryItem (Join Table)
│  └─ Use IInventoryRepository (special handling for many-to-many)
│     └─ Always Include(ii => ii.Item) for full data
│
├─ Dynamic Room Entities (v0.4.0)
│  ├─ RoomTemplate → IRoomTemplateRepository (string TemplateId)
│  ├─ BiomeDefinition → IBiomeDefinitionRepository (string BiomeId)
│  └─ BiomeElement → IBiomeElementRepository (compound queries)
│
└─ SaveGame
   └─ Use ISaveGameRepository
      └─ Slot-based operations (GetBySlotAsync, SlotExistsAsync)
```

---

### DT-2: Query Building

**Trigger**: Building a repository query method

```
Query Requirements?
│
├─ Single Entity Lookup
│  ├─ By Guid Id → FindAsync(id) (optimal primary key lookup)
│  ├─ By Other Field → FirstOrDefaultAsync(predicate)
│  └─ By Composite Key → Where(predicate).FirstOrDefaultAsync()
│
├─ Multiple Entities
│  ├─ All Entities → ToListAsync() (caution: large tables)
│  ├─ Filtered → Where(predicate).ToListAsync()
│  ├─ Ordered → OrderBy().ToListAsync()
│  └─ Paginated → Skip(n).Take(m).ToListAsync()
│
├─ Aggregation
│  ├─ Count → CountAsync() or Where().CountAsync()
│  ├─ Sum → SumAsync(selector)
│  ├─ Exists → AnyAsync(predicate)
│  └─ Distinct → Select().Distinct().ToListAsync()
│
├─ Related Entities
│  ├─ Eager Load → Include(navigation).ThenInclude(nested)
│  ├─ Projection → Select(x => new Dto { ... })
│  └─ Split Query → AsSplitQuery() for multiple collections
│
└─ JSONB Data
   ├─ Simple Access → Works via EF Core property access
   ├─ Array Contains → Load to memory, filter in C#
   └─ Complex Query → Use raw SQL via FromSqlRaw()
```

---

### DT-3: Transaction Management

**Trigger**: Service method modifying multiple entities

```
Operation Type?
│
├─ Single Entity CRUD
│  └─ Standard pattern:
│     1. Get/Add/Update/Delete
│     2. SaveChangesAsync()
│
├─ Multiple Entities (Same Repository)
│  └─ Batch pattern:
│     1. Multiple Add/Update/Delete calls
│     2. Single SaveChangesAsync() (atomic)
│
├─ Multiple Entities (Different Repositories)
│  ├─ Same DI Scope?
│  │  ├─ YES → Shared DbContext
│  │  │  1. Operations on RepoA
│  │  │  2. Operations on RepoB
│  │  │  3. Single SaveChangesAsync() on either repo
│  │  └─ NO → Separate transactions (potential inconsistency)
│  │     └─ Refactor to same scope
│
└─ Rollback Required?
   ├─ Exception before SaveChanges → No changes persisted (automatic)
   ├─ Exception during SaveChanges → Transaction rolled back (automatic)
   └─ Manual rollback → Don't call SaveChangesAsync()
```

---

## Cross-Links

### Dependencies (Systems SPEC-REPO-001 relies on)

1. **Entity Framework Core**
   - **Relationship**: All repositories built on EF Core DbContext/DbSet
   - **Integration Point**: GenericRepository wraps DbSet operations
   - **Version**: EF Core 8.0+

2. **PostgreSQL / Npgsql**
   - **Relationship**: Database provider for EF Core
   - **Integration Point**: Connection string in Program.cs, JSONB column types
   - **Version**: PostgreSQL 16, Npgsql 8.0+

3. **SPEC-MIGRATE-001 (Migration System)**
   - **Relationship**: Migrations create database schema that repositories query
   - **Integration Point**: Entity configurations in OnModelCreating match migration schema

---

### Dependents (Systems that rely on SPEC-REPO-001)

1. **SPEC-SAVE-001 (Save System)**
   - **Relationship**: SaveManager uses ISaveGameRepository for persistence
   - **Integration Point**: `GetBySlotAsync`, `AddAsync`, `UpdateAsync`, `SaveChangesAsync`

2. **SPEC-SEED-001 (Database Seeding)**
   - **Relationship**: Seeders use repositories/DbContext to insert seed data
   - **Integration Point**: `AddAsync`, `AddRangeAsync`, `SaveChangesAsync`

3. **All Game Services**
   - **Relationship**: Services inject repositories for data access
   - **Integration Point**: Constructor injection via DI container

---

### Related Systems

1. **Dependency Injection (Program.cs)**
   - **Relationship**: Repositories registered as Scoped services
   - **Integration Point**: `services.AddScoped<IXxxRepository, XxxRepository>()`

2. **Logging (Serilog)**
   - **Relationship**: Repositories log debug messages for all operations
   - **Integration Point**: `ILogger<T>` injected via constructor

---

## Related Services

### Base Repository

1. **IRepository<T>** (RuneAndRust.Core/Interfaces/IRepository.cs)
   - Generic CRUD interface

2. **GenericRepository<T>** (RuneAndRust.Persistence/Repositories/GenericRepository.cs)
   - Base implementation wrapping EF Core DbSet

### Specialized Repositories (13)

3. **ICharacterRepository / CharacterRepository**
4. **IRoomRepository / RoomRepository**
5. **IInventoryRepository / InventoryRepository** (special: no base inheritance)
6. **ISaveGameRepository / SaveGameRepository**
7. **IDataCaptureRepository / DataCaptureRepository**
8. **IItemRepository / ItemRepository**
9. **ICodexEntryRepository / CodexEntryRepository**
10. **IInteractableObjectRepository / InteractableObjectRepository**
11. **IActiveAbilityRepository / ActiveAbilityRepository**
12. **IRoomTemplateRepository / RoomTemplateRepository** (v0.4.0)
13. **IBiomeDefinitionRepository / BiomeDefinitionRepository** (v0.4.0)
14. **IBiomeElementRepository / BiomeElementRepository** (v0.4.0)

### DbContext

15. **RuneAndRustDbContext** (RuneAndRust.Persistence/Data/RuneAndRustDbContext.cs)
    - Central EF Core context with all DbSet properties

---

## Data Models

### Entity Categories

**Core Game Entities**:
- `Character` - Player character with stats, archetype, level
- `Room` - Exploration location with position, exits, description
- `Item` / `Equipment` - Items with optional equipment bonuses (TPH)
- `InventoryItem` - Many-to-many join table (Character ↔ Item)
- `InteractableObject` / `DynamicHazard` - Room objects (TPH)

**Save System**:
- `SaveGame` - Slot-based save with serialized GameState

**Journal System**:
- `CodexEntry` - Lore entry with unlock fragments
- `DataCapture` - Discovered journal fragments

**Combat System**:
- `ActiveAbility` - Player/enemy combat abilities

**Environment System (v0.3.5)**:
- `AmbientCondition` - Room-wide environmental effects
- `HazardTemplate` - Prototype hazard definitions

**Dynamic Room System (v0.4.0)**:
- `RoomTemplate` - Room generation template (string ID)
- `BiomeDefinition` - Biome configuration (string ID)
- `BiomeElement` - Spawn configuration within biome

### JSONB Columns

| Entity | Property | Content |
|--------|----------|---------|
| Room | Exits | Dictionary<Direction, RoomExit> |
| Item | Tags | List<string> |
| Equipment | Bonuses | Dictionary<StatType, int> |
| AmbientCondition | BiomeTags | List<string> |
| HazardTemplate | BiomeTags | List<string> |
| BiomeDefinition | DescriptorCategories | Dictionary<string, List<string>> |
| BiomeElement | SpawnRules | SpawnRuleConfig |

### TPH Inheritance

**Item Hierarchy**:
```
Item (base)
└── Equipment (derived, adds Slot, Bonuses)
```

**InteractableObject Hierarchy**:
```
InteractableObject (base)
└── DynamicHazard (derived, adds HazardType, TriggerScript)
```

**Discriminator**: String column with class name ("Item", "Equipment", "InteractableObject", "DynamicHazard")

---

## Configuration

### DI Registration (Program.cs)

```csharp
// DbContext
services.AddDbContext<RuneAndRustDbContext>(options =>
    options.UseNpgsql(connectionString));

// Generic Repository (open generic registration)
services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));

// Specialized Repositories
services.AddScoped<ICharacterRepository, CharacterRepository>();
services.AddScoped<IRoomRepository, RoomRepository>();
services.AddScoped<IInventoryRepository, InventoryRepository>();
services.AddScoped<ISaveGameRepository, SaveGameRepository>();
services.AddScoped<IDataCaptureRepository, DataCaptureRepository>();
services.AddScoped<IItemRepository, ItemRepository>();
services.AddScoped<ICodexEntryRepository, CodexEntryRepository>();
services.AddScoped<IInteractableObjectRepository, InteractableObjectRepository>();
services.AddScoped<IActiveAbilityRepository, ActiveAbilityRepository>();
services.AddScoped<IRoomTemplateRepository, RoomTemplateRepository>();
services.AddScoped<IBiomeDefinitionRepository, BiomeDefinitionRepository>();
services.AddScoped<IBiomeElementRepository, BiomeElementRepository>();
```

**Location**: `/Users/ryan/Documents/GitHub/rune-rust/RuneAndRust.Terminal/Program.cs` (lines 44-58)

### Connection String

```csharp
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? "Host=localhost;Database=runeandrust;Username=postgres;Password=postgres";
```

**Docker Override**: `Host=db` when running in Docker Compose.

---

## Testing

### Unit Testing Strategy

**Test Coverage**: No dedicated repository unit tests. Repositories are indirectly tested via:
- Integration tests using `PostgreSqlTestFixture`
- Service-level tests that exercise repository methods
- Journey tests that validate end-to-end data flows

**Mocking Approach**:
- Mock `IXxxRepository` interfaces in service tests
- Use InMemory provider or SQLite for repository integration tests
- NSubstitute for interface mocking

### Example Test Pattern: CharacterRepository

**File**: RuneAndRust.Tests/Persistence/CharacterRepositoryTests.cs (example pattern, not implemented)

```csharp
public class CharacterRepositoryTests : IDisposable
{
    private readonly RuneAndRustDbContext _context;
    private readonly CharacterRepository _repository;

    public CharacterRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<RuneAndRustDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new RuneAndRustDbContext(options);
        var logger = Substitute.For<ILogger<GenericRepository<Character>>>();
        _repository = new CharacterRepository(_context, logger);
    }

    [Fact]
    public async Task GetByNameAsync_ExistingCharacter_ReturnsCharacter()
    {
        // Arrange
        var character = new Character { Id = Guid.NewGuid(), Name = "TestHero" };
        await _context.Characters.AddAsync(character);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByNameAsync("testhero");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("TestHero", result.Name);
    }

    [Fact]
    public async Task GetByNameAsync_NonExistentCharacter_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByNameAsync("nonexistent");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task NameExistsAsync_ExistingName_ReturnsTrue()
    {
        // Arrange
        var character = new Character { Id = Guid.NewGuid(), Name = "TestHero" };
        await _context.Characters.AddAsync(character);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.NameExistsAsync("TestHero");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task NameExistsAsync_CaseInsensitive_ReturnsTrue()
    {
        // Arrange
        var character = new Character { Id = Guid.NewGuid(), Name = "TestHero" };
        await _context.Characters.AddAsync(character);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.NameExistsAsync("TESTHERO");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task GetAllOrderedByCreationAsync_MultipleCharacters_ReturnsOrderedList()
    {
        // Arrange
        var older = new Character { Id = Guid.NewGuid(), Name = "Older", CreatedAt = DateTime.UtcNow.AddDays(-1) };
        var newer = new Character { Id = Guid.NewGuid(), Name = "Newer", CreatedAt = DateTime.UtcNow };
        await _context.Characters.AddRangeAsync(older, newer);
        await _context.SaveChangesAsync();

        // Act
        var result = (await _repository.GetAllOrderedByCreationAsync()).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Newer", result[0].Name); // Descending order
        Assert.Equal("Older", result[1].Name);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
```

### Integration Test with Real Database

```csharp
[Collection("DatabaseTests")]
public class RoomRepositoryIntegrationTests
{
    private readonly RuneAndRustDbContext _context;
    private readonly RoomRepository _repository;

    public RoomRepositoryIntegrationTests(DatabaseFixture fixture)
    {
        _context = fixture.CreateContext();
        var logger = Substitute.For<ILogger<GenericRepository<Room>>>();
        _repository = new RoomRepository(_context, logger);
    }

    [Fact]
    public async Task GetRoomsInGridAsync_ReturnsCorrectRooms()
    {
        // Arrange - rooms seeded in test database

        // Act
        var result = await _repository.GetRoomsInGridAsync(0, 0, 2, 0, 2);

        // Assert
        Assert.All(result, r =>
        {
            Assert.Equal(0, r.Position.Z);
            Assert.InRange(r.Position.X, 0, 2);
            Assert.InRange(r.Position.Y, 0, 2);
        });
    }
}
```

---

## Design Rationale

### DR-1: Why Generic + Specialized Repositories?

**Decision**: Use generic base repository (IRepository<T>) plus specialized interfaces for domain-specific queries.

**Alternatives Considered**:
1. **Only Generic Repository**: Single IRepository<T> for all entities
2. **Only Specialized Repositories**: Each entity has unique interface with all methods
3. **Query Objects**: Separate query classes for each query type

**Rationale for Generic + Specialized**:
- **DRY**: Base CRUD operations defined once in GenericRepository
- **Discoverability**: Domain-specific methods explicitly named in specialized interfaces
- **Testability**: Can mock specific repository interface for unit tests
- **Flexibility**: Add specialized methods without modifying base

**Trade-Offs**:
- **Complexity**: More interfaces/classes than single generic approach
- **Consistency**: Must ensure specialized repos inherit from GenericRepository

---

### DR-2: Why No Explicit Unit of Work?

**Decision**: Use DbContext as implicit Unit of Work (no separate IUnitOfWork interface).

**Alternatives Considered**:
1. **Explicit IUnitOfWork**: Separate interface wrapping DbContext.SaveChangesAsync()
2. **Repository SaveChanges**: Each repository has SaveChangesAsync() (current approach)
3. **Service SaveChanges**: Services call DbContext directly for SaveChanges

**Rationale for Implicit UoW**:
- **Simplicity**: DbContext already implements UoW pattern
- **Less Boilerplate**: No additional wrapper class
- **Scoped Lifetime**: DI ensures single DbContext per request (transaction boundary)

**Trade-Offs**:
- **Coupling**: Repositories must be in same scope for shared transactions
- **Explicitness**: Less obvious when transaction boundary occurs

---

### DR-3: Why PostgreSQL JSONB?

**Decision**: Store complex data (exits, tags, bonuses) as JSONB columns.

**Alternatives Considered**:
1. **Separate Tables**: Normalize all data (Exits table, Tags table, etc.)
2. **XML Columns**: Store as PostgreSQL XML type
3. **Serialized Blobs**: Store as binary blobs

**Rationale for JSONB**:
- **Flexibility**: Schema-less storage for varying structures
- **Query Support**: PostgreSQL JSONB operators for filtering
- **Performance**: Binary format with indexing support
- **Readability**: Human-readable in database tools

**Trade-Offs**:
- **EF Core Limitations**: Complex JSONB queries require raw SQL
- **Migration Complexity**: JSONB schema changes harder to track
- **Type Safety**: Less compile-time checking than strongly-typed columns

---

## Changelog

### Version 1.0.0 (2025-12-22) - Initial Specification

**Added**:
- Comprehensive repository pattern documentation
- Generic repository interface and implementation
- 13 specialized repository specifications
- DbContext architecture with TPH inheritance, JSONB, owned types
- Implicit Unit of Work pattern documentation
- 6 query patterns (filtering, ordering, spatial, aggregation, eager loading, existence)
- 6 detailed use cases (character creation, minimap, inventory weight, equipped items, save management, biome elements)
- 3 decision trees (repository selection, query building, transaction management)
- Testing strategy with example unit/integration tests
- Design rationale (generic + specialized, no explicit UoW, JSONB choice)

---

## Future Enhancements

### FE-1: Pagination Support

**Problem**: Base `GetAllAsync` retrieves entire table.

**Proposed Solution**:
```csharp
public interface IRepository<T>
{
    Task<PagedResult<T>> GetPagedAsync(int page, int pageSize);
}

public record PagedResult<T>(
    IEnumerable<T> Items,
    int TotalCount,
    int PageNumber,
    int PageSize
);
```

---

### FE-2: Bulk Operations

**Problem**: Inserting many entities requires N AddAsync calls.

**Proposed Solution**:
```csharp
public interface IRepository<T>
{
    Task AddRangeAsync(IEnumerable<T> entities);
    Task UpdateRangeAsync(IEnumerable<T> entities);
    Task DeleteRangeAsync(IEnumerable<Guid> ids);
}
```

---

### FE-3: Specification Pattern

**Problem**: Complex queries scattered across repository methods.

**Proposed Solution**:
```csharp
public interface ISpecification<T>
{
    Expression<Func<T, bool>> Criteria { get; }
    List<Expression<Func<T, object>>> Includes { get; }
}

public interface IRepository<T>
{
    Task<IEnumerable<T>> FindAsync(ISpecification<T> spec);
}
```

---

### FE-4: Read-Only Repository

**Problem**: Some services only need read access.

**Proposed Solution**:
```csharp
public interface IReadOnlyRepository<T>
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
}
```

---

## AAM-VOICE Compliance

### Layer Classification: **Layer 3 (Technical Specification)**

**Rationale**: This document is a system architecture specification for developers, not in-game content. Layer 3 applies to technical documentation written POST-Glitch with modern precision language.

### Domain 4 Compliance: **NOT APPLICABLE**

**Rationale**: Domain 4 (Technology Constraints) applies to **in-game lore content** (item descriptions, bestiary entries, NPC dialogue). This specification is **out-of-game technical documentation** and may use precision measurements (e.g., "13 repositories," "70% coverage").

### Voice Discipline: **Technical Authority**

**Characteristics**:
- **Precision**: Exact interface signatures, SQL examples, file paths
- **Definitive Statements**: "All repositories MUST be Scoped"
- **Code Examples**: C# implementations with expected SQL output
- **Quantifiable Metrics**: "70% test coverage," "15+ DbSet properties"

---

## Changelog

### v1.0.1 (2025-12-24)
**Documentation Updates:**
- Added `last_updated` field to frontmatter
- Fixed `GetTotalWeightAsync` return type: `decimal` → `int` (matches implementation)
- Documented 11 additional repository methods:
  - `IRoomRepository`: `GetAllRoomsAsync`, `ClearAllRoomsAsync`
  - `IInventoryRepository`: `GetByCharacterAndItemAsync`, `FindByItemNameAsync`, `ClearInventoryAsync`
  - `IDataCaptureRepository`: `GetDiscoveredEntryIdsAsync`
  - `ICodexEntryRepository`: `GetByCategoryAsync`
  - `IInteractableObjectRepository`: `AddRangeAsync`, `ClearRoomObjectsAsync`
  - `IRoomTemplateRepository`: `UpsertAsync`
  - `IBiomeDefinitionRepository`: `UpsertAsync`, `GetElementsForBiomeAsync`
- Updated test coverage section (no dedicated tests, indirect coverage via integration tests)
- Added code traceability remarks to 6 key repository files

### v1.0.0 (2025-12-22)
**Initial Release:**
- Repository pattern documentation
- 13 specialized repositories
- GenericRepository base class
- DI registration patterns
- Query pattern examples (spatial, filtering, aggregation)

---

**END OF SPECIFICATION**
