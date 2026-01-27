# Data Access Patterns

> **Version:** 0.41+
> **Last Updated:** November 2024
> **Location:** `RuneAndRust.Persistence/`

## Overview

Rune & Rust uses the **Repository Pattern** for all database access. Repositories provide a clean abstraction over SQLite operations, handle connection management, and encapsulate query logic. All repositories use **parameterized queries** to prevent SQL injection.

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                      Game Services                           │
│  (CombatEngine, DungeonGenerator, TraumaEconomyService)     │
└────────────────────────────┬────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────┐
│                      Repositories                            │
│  (SaveRepository, WorldStateRepository, AbilityRepository)   │
└────────────────────────────┬────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────┐
│                    Microsoft.Data.Sqlite                     │
└────────────────────────────┬────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────┐
│                      runeandrust.db                          │
└─────────────────────────────────────────────────────────────┘
```

## Repository List

| Repository | Purpose | Key Methods |
|------------|---------|-------------|
| `SaveRepository` | Character saves | `SaveGame`, `LoadGame`, `ListSaves`, `DeleteSave` |
| `WorldStateRepository` | Delta world changes | `RecordChange`, `GetChangesForRoom`, `GetChangesForSave` |
| `AbilityRepository` | Ability definitions | `GetAbilitiesForSpecialization`, `GetAbilityById` |
| `SpecializationRepository` | Spec definitions | `GetAllSpecializations`, `GetSpecializationById` |
| `AccountProgressionRepository` | Account unlocks | `CreateAccount`, `UpdateProgression`, `GetUnlocks` |
| `AchievementRepository` | Achievement tracking | `GetAchievements`, `UnlockAchievement` |
| `CosmeticRepository` | Cosmetic items | `GetCosmetics`, `UnlockCosmetic` |
| `MuspelheimDataRepository` | Muspelheim biome | `GetBiomeData`, `GetRoomTemplates` |
| `NiflheimDataRepository` | Niflheim biome | `GetBiomeData`, `GetRoomTemplates` |
| `JotunheimDataRepository` | Jotunheim biome | `GetBiomeData`, `GetRoomTemplates` |
| `AlfheimDataRepository` | Alfheim biome | `GetBiomeData`, `GetRoomTemplates` |
| `BossEncounterRepository` | Boss definitions | `GetBossEncounter`, `GetBossForBiome` |
| `CraftingRepository` | Crafting recipes | `GetRecipes`, `GetRecipeById` |
| `DatabaseService` | Generic utilities | `ExecuteQuery`, `ExecuteNonQuery`, `ExecuteScalar` |

## Standard Repository Pattern

### Constructor Pattern

All repositories follow a consistent constructor pattern:

```csharp
public class ExampleRepository
{
    private static readonly ILogger _log = Log.ForContext<ExampleRepository>();
    private readonly string _connectionString;
    private const string DatabaseName = "runeandrust.db";

    public ExampleRepository(string? dataDirectory = null)
    {
        var dbPath = Path.Combine(
            dataDirectory ?? Environment.CurrentDirectory,
            DatabaseName
        );
        _connectionString = $"Data Source={dbPath}";

        _log.Debug("ExampleRepository initialized with path: {DbPath}", dbPath);

        InitializeTable();
    }

    private void InitializeTable()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS example_table (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                name TEXT NOT NULL,
                created_at TEXT NOT NULL
            )
        ";
        command.ExecuteNonQuery();
    }
}
```

### Connection Management

Connections are created per-operation using `using` statements:

```csharp
public List<Example> GetAll()
{
    var results = new List<Example>();

    using var connection = new SqliteConnection(_connectionString);
    connection.Open();

    using var command = connection.CreateCommand();
    command.CommandText = "SELECT * FROM example_table ORDER BY name";

    using var reader = command.ExecuteReader();
    while (reader.Read())
    {
        results.Add(MapFromReader(reader));
    }

    return results;
}
```

## Query Patterns

### Parameterized Queries

**Always use parameters** to prevent SQL injection:

```csharp
// CORRECT: Parameterized query
command.CommandText = "SELECT * FROM saves WHERE character_name = $name";
command.Parameters.AddWithValue("$name", characterName);

// WRONG: String concatenation (vulnerable to SQL injection)
command.CommandText = $"SELECT * FROM saves WHERE character_name = '{characterName}'";
```

### Parameter Naming Conventions

Use `$` prefix for SQLite parameters:

```csharp
command.Parameters.AddWithValue("$saveId", saveId);
command.Parameters.AddWithValue("$roomId", roomId);
command.Parameters.AddWithValue("$changeType", changeType.ToString());
command.Parameters.AddWithValue("$timestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
```

### Handling Nulls

Use `DBNull.Value` for null parameters:

```csharp
command.Parameters.AddWithValue("$optionalField",
    (object?)optionalValue ?? DBNull.Value);
```

### INSERT OR REPLACE (Upsert)

For save operations that should update existing records:

```csharp
command.CommandText = @"
    INSERT OR REPLACE INTO saves (
        character_name, class, current_hp, max_hp, last_saved
    ) VALUES (
        $name, $class, $hp, $maxhp, $saved
    )
";
```

### INSERT OR IGNORE

For idempotent inserts (seeding data):

```csharp
command.CommandText = @"
    INSERT OR IGNORE INTO Biomes (
        biome_id, biome_name, biome_description
    ) VALUES (
        4, 'Muspelheim', 'Catastrophic geothermal failure zone...'
    )
";
```

### Batch Inserts

For seeding multiple rows efficiently:

```csharp
var roomTemplates = new[]
{
    ("Geothermal Control Chamber", "Large", 3, 5),
    ("Lava Flow Corridor", "Small", 2, 2),
    ("Collapsed Forge Floor", "Medium", 2, 3),
};

foreach (var (name, size, minConn, maxConn) in roomTemplates)
{
    var insertCommand = connection.CreateCommand();
    insertCommand.CommandText = @"
        INSERT OR IGNORE INTO Biome_RoomTemplates
        (biome_id, template_name, room_size_category, min_connections, max_connections)
        VALUES ($biomeId, $name, $size, $min, $max)
    ";
    insertCommand.Parameters.AddWithValue("$biomeId", biomeId);
    insertCommand.Parameters.AddWithValue("$name", name);
    insertCommand.Parameters.AddWithValue("$size", size);
    insertCommand.Parameters.AddWithValue("$min", minConn);
    insertCommand.Parameters.AddWithValue("$max", maxConn);
    insertCommand.ExecuteNonQuery();
}
```

## JSON Serialization

### Serializing to JSON Columns

Use `System.Text.Json.JsonSerializer`:

```csharp
// Serialize objects to JSON strings for storage
var equippedWeaponJson = player.EquippedWeapon != null
    ? JsonSerializer.Serialize(player.EquippedWeapon)
    : null;

var inventoryJson = JsonSerializer.Serialize(player.Inventory);
var consumablesJson = JsonSerializer.Serialize(player.Consumables);
var factionReputationsJson = JsonSerializer.Serialize(player.FactionReputations.Reputations);
```

### Deserializing from JSON Columns

```csharp
// Deserialize JSON strings back to objects
var inventoryOrdinal = reader.GetOrdinal("inventory_json");
if (!reader.IsDBNull(inventoryOrdinal))
{
    var inventoryJson = reader.GetString(inventoryOrdinal);
    player.Inventory = JsonSerializer.Deserialize<List<Equipment>>(inventoryJson)
        ?? new List<Equipment>();
}

// Dictionary deserialization
var componentsJson = reader.GetString(reader.GetOrdinal("crafting_components_json"));
player.CraftingComponents = JsonSerializer.Deserialize<Dictionary<ComponentType, int>>(componentsJson)
    ?? new Dictionary<ComponentType, int>();
```

### Handling Missing/Null JSON

```csharp
try
{
    var traumasOrdinal = reader.GetOrdinal("traumas_json");
    saveData.TraumasJson = reader.IsDBNull(traumasOrdinal)
        ? "[]"
        : reader.GetString(traumasOrdinal);
}
catch
{
    saveData.TraumasJson = "[]";
}
```

## Backward Compatibility

### Column Migration Pattern

Handle new columns gracefully for existing databases:

```csharp
// Attempt to add column, silently ignore if exists
var alterCommands = new[]
{
    "ALTER TABLE saves ADD COLUMN new_feature_json TEXT DEFAULT '[]'",
    "ALTER TABLE saves ADD COLUMN new_counter INTEGER DEFAULT 0",
};

foreach (var alterSql in alterCommands)
{
    try
    {
        var alterCommand = connection.CreateCommand();
        alterCommand.CommandText = alterSql;
        alterCommand.ExecuteNonQuery();
    }
    catch (SqliteException)
    {
        // Column already exists, ignore
    }
}
```

### Reading with Fallbacks

Handle missing columns when loading old saves:

```csharp
// Try to read new column, fallback to default if missing
try
{
    saveData.Currency = reader.GetInt32(reader.GetOrdinal("currency"));
}
catch
{
    saveData.Currency = 0;  // Default for old saves
}

// Enum parsing with fallback
try
{
    saveData.Specialization = Enum.Parse<Specialization>(
        reader.GetString(reader.GetOrdinal("specialization"))
    );
}
catch
{
    saveData.Specialization = Specialization.None;
}
```

## Delta-Based Storage

### Recording World Changes

The `WorldStateRepository` uses delta storage for efficient state tracking:

```csharp
public void RecordChange(WorldStateChange change)
{
    using var connection = new SqliteConnection(_connectionString);
    connection.Open();

    var command = connection.CreateCommand();
    command.CommandText = @"
        INSERT INTO world_state_changes (
            save_id, sector_seed, room_id, change_type, target_id, change_data,
            timestamp, turn_number, schema_version
        ) VALUES (
            $saveId, $sectorSeed, $roomId, $changeType, $targetId, $changeData,
            $timestamp, $turnNumber, $schemaVersion
        )
    ";

    command.Parameters.AddWithValue("$saveId", change.SaveId);
    command.Parameters.AddWithValue("$sectorSeed", change.SectorSeed);
    command.Parameters.AddWithValue("$roomId", change.RoomId);
    command.Parameters.AddWithValue("$changeType", change.ChangeType.ToString());
    command.Parameters.AddWithValue("$targetId", change.TargetId);
    command.Parameters.AddWithValue("$changeData", change.ChangeData);
    command.Parameters.AddWithValue("$timestamp",
        change.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff"));
    command.Parameters.AddWithValue("$turnNumber", change.TurnNumber);
    command.Parameters.AddWithValue("$schemaVersion", change.SchemaVersion);

    command.ExecuteNonQuery();
}
```

### Reconstructing Room State

Query all changes and replay them:

```csharp
public List<WorldStateChange> GetChangesForRoom(int saveId, string sectorSeed, string roomId)
{
    var changes = new List<WorldStateChange>();

    using var connection = new SqliteConnection(_connectionString);
    connection.Open();

    var command = connection.CreateCommand();
    command.CommandText = @"
        SELECT id, save_id, sector_seed, room_id, change_type, target_id, change_data,
               timestamp, turn_number, schema_version
        FROM world_state_changes
        WHERE save_id = $saveId AND sector_seed = $sectorSeed AND room_id = $roomId
        ORDER BY timestamp ASC
    ";
    command.Parameters.AddWithValue("$saveId", saveId);
    command.Parameters.AddWithValue("$sectorSeed", sectorSeed);
    command.Parameters.AddWithValue("$roomId", roomId);

    using var reader = command.ExecuteReader();
    while (reader.Read())
    {
        changes.Add(MapWorldStateChange(reader));
    }

    return changes;
}
```

## DatabaseService Utility

For simple database operations, use `DatabaseService`:

```csharp
public class DatabaseService
{
    private readonly string _connectionString;

    public SqliteConnection GetConnection()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        return connection;
    }

    public int ExecuteNonQuery(string sql, params (string name, object value)[] parameters)
    {
        using var connection = GetConnection();
        using var command = connection.CreateCommand();
        command.CommandText = sql;

        foreach (var (name, value) in parameters)
        {
            command.Parameters.AddWithValue(name, value ?? DBNull.Value);
        }

        return command.ExecuteNonQuery();
    }

    public object? ExecuteScalar(string sql, params (string name, object value)[] parameters)
    {
        using var connection = GetConnection();
        using var command = connection.CreateCommand();
        command.CommandText = sql;

        foreach (var (name, value) in parameters)
        {
            command.Parameters.AddWithValue(name, value ?? DBNull.Value);
        }

        return command.ExecuteScalar();
    }

    public List<T> ExecuteQuery<T>(
        string sql,
        Func<SqliteDataReader, T> mapper,
        params (string name, object value)[] parameters)
    {
        var results = new List<T>();

        using var connection = GetConnection();
        using var command = connection.CreateCommand();
        command.CommandText = sql;

        foreach (var (name, value) in parameters)
        {
            command.Parameters.AddWithValue(name, value ?? DBNull.Value);
        }

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            results.Add(mapper(reader));
        }

        return results;
    }
}
```

**Usage:**

```csharp
var dbService = new DatabaseService(connectionString);

// Simple count
var count = (long)dbService.ExecuteScalar(
    "SELECT COUNT(*) FROM saves WHERE class = $class",
    ("$class", "Warrior")
);

// Query with mapper
var names = dbService.ExecuteQuery(
    "SELECT character_name FROM saves ORDER BY last_saved DESC",
    reader => reader.GetString(0)
);
```

## Logging

All repositories use Serilog for structured logging:

```csharp
private static readonly ILogger _log = Log.ForContext<SaveRepository>();

public void SaveGame(PlayerCharacter player, WorldState worldState)
{
    _log.Information("Saving game: Character={CharacterName}, Class={Class}",
        player.Name, player.Class);

    var startTime = DateTime.Now;

    try
    {
        // ... save logic ...

        var duration = (DateTime.Now - startTime).TotalMilliseconds;
        _log.Information("Game saved: Character={CharacterName}, Duration={Duration}ms",
            player.Name, duration);
    }
    catch (Exception ex)
    {
        _log.Error(ex, "Failed to save: Character={CharacterName}", player.Name);
        throw;
    }
}
```

## Transaction Patterns

For operations requiring atomicity:

```csharp
public void TransferItems(int fromSaveId, int toSaveId, List<int> itemIds)
{
    using var connection = new SqliteConnection(_connectionString);
    connection.Open();

    using var transaction = connection.BeginTransaction();

    try
    {
        foreach (var itemId in itemIds)
        {
            // Remove from source
            var removeCmd = connection.CreateCommand();
            removeCmd.Transaction = transaction;
            removeCmd.CommandText = "DELETE FROM inventory WHERE save_id = $from AND item_id = $item";
            removeCmd.Parameters.AddWithValue("$from", fromSaveId);
            removeCmd.Parameters.AddWithValue("$item", itemId);
            removeCmd.ExecuteNonQuery();

            // Add to destination
            var addCmd = connection.CreateCommand();
            addCmd.Transaction = transaction;
            addCmd.CommandText = "INSERT INTO inventory (save_id, item_id) VALUES ($to, $item)";
            addCmd.Parameters.AddWithValue("$to", toSaveId);
            addCmd.Parameters.AddWithValue("$item", itemId);
            addCmd.ExecuteNonQuery();
        }

        transaction.Commit();
    }
    catch
    {
        transaction.Rollback();
        throw;
    }
}
```

## Best Practices

### Do

- Use parameterized queries for all user input
- Handle null values with `DBNull.Value`
- Use `using` statements for connections and commands
- Log operations with timing for performance monitoring
- Handle backward compatibility with try-catch for missing columns
- Use `INSERT OR REPLACE` for upsert operations
- Create indices for frequently-queried columns

### Don't

- Concatenate strings into SQL queries
- Keep connections open longer than necessary
- Ignore exceptions silently (log them)
- Store complex objects without JSON serialization
- Assume columns exist (handle migrations gracefully)
- Use `SELECT *` in production code (specify columns)

## Related Documentation

- [Database Schema](database-schema.md) - Table definitions and relationships
- [Service Architecture](service-architecture.md) - How services use repositories
- [Save/Load System](services/save-repository.md) - Save/load implementation details
