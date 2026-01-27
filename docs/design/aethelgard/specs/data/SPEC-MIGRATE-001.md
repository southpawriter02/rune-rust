---
id: SPEC-MIGRATE-001
title: Migration System
version: 1.0.1
status: Implemented
priority: P0
owner: Backend Team
related_specs: [SPEC-REPO-001, SPEC-SEED-001]
created: 2025-12-22
last_updated: 2025-12-24
---

# SPEC-MIGRATE-001: Migration System

## 1. Overview

### 1.1 Purpose

The **Migration System** provides version-controlled schema evolution for the Rune & Rust PostgreSQL database using Entity Framework Core Code-First migrations. This specification documents the migration architecture, schema versioning strategy, and execution patterns that enable safe, bidirectional database changes.

### 1.2 Scope

This specification covers:

- EF Core Code-First migration architecture
- Up/Down method bidirectional execution
- Design-time factory configuration
- Schema versioning and naming conventions
- PostgreSQL-specific features (JSONB, arrays, GUIDs)
- Migration execution via CLI tooling
- Rollback and recovery procedures

### 1.3 Design Goals

| Goal | Description |
|------|-------------|
| **Version Control** | Track all schema changes in source control with descriptive names |
| **Bidirectionality** | Every Up() operation has a corresponding Down() rollback |
| **Atomicity** | Each migration succeeds or fails as a unit |
| **Idempotency** | MigrateAsync() safely applies only pending migrations |
| **Development UX** | Design-time factory enables CLI tooling without running the application |

### 1.4 Key Terminology

| Term | Definition |
|------|------------|
| **Migration** | A C# class that defines schema changes via Up() and Down() methods |
| **MigrationBuilder** | EF Core fluent API for defining DDL operations |
| **Design-Time Factory** | Factory class that creates DbContext for CLI operations |
| **Snapshot** | EF Core model snapshot reflecting current schema state |
| **__EFMigrationsHistory** | PostgreSQL table tracking applied migrations |

---

## 2. Core Concepts

### 2.1 Migration Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                     MIGRATION SYSTEM                             │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌─────────────────┐    ┌──────────────────┐                    │
│  │ Design-Time     │    │ Test Fixture     │                    │
│  │ (dotnet ef)     │    │ (MigrateAsync)   │                    │
│  └────────┬────────┘    └────────┬─────────┘                    │
│           │                      │                               │
│           ▼                      ▼                               │
│  ┌─────────────────────────────────────────┐                    │
│  │        DesignTimeDbContextFactory       │                    │
│  │  - Creates DbContext without host       │                    │
│  │  - Uses env var or default connection   │                    │
│  └────────────────────┬────────────────────┘                    │
│                       │                                          │
│                       ▼                                          │
│  ┌─────────────────────────────────────────┐                    │
│  │           RuneAndRustDbContext          │                    │
│  │  - Entity configurations                │                    │
│  │  - JSONB value converters               │                    │
│  │  - TPH inheritance mappings             │                    │
│  └────────────────────┬────────────────────┘                    │
│                       │                                          │
│                       ▼                                          │
│  ┌─────────────────────────────────────────┐                    │
│  │           PostgreSQL (Npgsql)           │                    │
│  │  - __EFMigrationsHistory table          │                    │
│  │  - Applied migrations tracking          │                    │
│  └─────────────────────────────────────────┘                    │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

### 2.2 Migration File Structure

Each migration consists of three generated files:

```
RuneAndRust.Persistence/Migrations/
├── 20251220231703_InitialCreate.cs              # Up/Down methods
├── 20251220231703_InitialCreate.Designer.cs     # Model snapshot (auto-generated)
├── 20251221014933_AddRestSystemColumns.cs
├── 20251221014933_AddRestSystemColumns.Designer.cs
├── 20251222025251_AddEnvironmentEcosystemTables.cs
├── 20251222025251_AddEnvironmentEcosystemTables.Designer.cs
├── 20251223022458_MakeActiveAbilityArchetypeNullable.cs
├── 20251223022458_MakeActiveAbilityArchetypeNullable.Designer.cs
└── RuneAndRustDbContextModelSnapshot.cs         # Current model state
```

### 2.3 Naming Convention

```
[Timestamp]_[DescriptiveName].cs
    │              │
    │              └── PascalCase description of changes
    │
    └── Format: yyyyMMddHHmmss (UTC)
```

**Examples:**
- `20251220231703_InitialCreate` - Initial schema creation
- `20251221014933_AddRestSystemColumns` - Adding JSONB columns
- `20251222025251_AddEnvironmentEcosystemTables` - New tables for environment system
- `20251223022458_MakeActiveAbilityArchetypeNullable` - Column modification

---

## 3. Schema Evolution History

### 3.1 Migration Timeline

```
┌──────────────────────────────────────────────────────────────────────┐
│  MIGRATION 1: InitialCreate (20251220231703)                         │
│  Base schema: 12 tables, 20+ indexes, JSONB columns                  │
├──────────────────────────────────────────────────────────────────────┤
│  Tables Created:                                                     │
│  • ActiveAbilities     • InteractableObjects   • SaveGames           │
│  • Characters          • Items (TPH)           • Trauma              │
│  • CodexEntries        • Rooms                 • DataCaptures        │
│  • InventoryItems      • ItemProperties                              │
│                                                                      │
│  JSONB Columns:                                                      │
│  • Characters.EquipmentBonuses                                       │
│  • CodexEntries.UnlockThresholds                                     │
│  • Items.AttributeBonuses, Items.Requirements                        │
│  • Rooms.Exits                                                       │
│  • ItemProperties.StatModifiers                                      │
│  • SaveGames.SerializedState                                         │
└──────────────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌──────────────────────────────────────────────────────────────────────┐
│  MIGRATION 2: AddRestSystemColumns (20251221014933)                  │
│  Rest system support: status effects, item tags                      │
├──────────────────────────────────────────────────────────────────────┤
│  Columns Added:                                                      │
│  • Items.Tags (JSONB, default: [])                                   │
│  • Characters.ActiveStatusEffects (JSONB, default: [])               │
└──────────────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌──────────────────────────────────────────────────────────────────────┐
│  MIGRATION 3: AddEnvironmentEcosystemTables (20251222025251)         │
│  Ambient conditions, hazards, dynamic hazard TPH extension           │
├──────────────────────────────────────────────────────────────────────┤
│  Tables Created:                                                     │
│  • AmbientConditions (8 condition types)                             │
│  • HazardTemplates (13 hazard templates)                             │
│                                                                      │
│  Columns Added (Rooms):                                              │
│  • ConditionId (FK → AmbientConditions)                              │
│  • Features (integer[] array)                                        │
│                                                                      │
│  TPH Extension (InteractableObjects → DynamicHazard):                │
│  • HazardType, Trigger, State                                        │
│  • EffectScript, TriggerMessage                                      │
│  • MaxCooldown, CooldownRemaining                                    │
│  • OneTimeUse, DamageThreshold, RequiredDamageType                   │
│                                                                      │
│  Character Additions:                                                │
│  • Background (enum, default: 0)                                     │
└──────────────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌──────────────────────────────────────────────────────────────────────┐
│  MIGRATION 4: MakeActiveAbilityArchetypeNullable (20251223022458)    │
│  Biome system tables, ability archetype flexibility                  │
├──────────────────────────────────────────────────────────────────────┤
│  Tables Created:                                                     │
│  • BiomeDefinitions (procedural generation config)                   │
│  • RoomTemplates (20 templates from JSON)                            │
│  • BiomeElements (27 spawn elements)                                 │
│                                                                      │
│  Column Modified:                                                    │
│  • ActiveAbilities.Archetype: integer → integer (nullable)           │
│                                                                      │
│  JSONB Columns Added:                                                │
│  • BiomeDefinitions.AvailableTemplates, DescriptorCategories         │
│  • RoomTemplates.NameTemplates, Adjectives, DescriptionTemplates     │
│  • RoomTemplates.Details, ValidConnections, Tags                     │
│  • BiomeElements.SpawnRules                                          │
└──────────────────────────────────────────────────────────────────────┘
```

### 3.2 Complete Table Registry

| Table | Migration | Primary Features |
|-------|-----------|------------------|
| ActiveAbilities | 1 | Combat abilities, cooldowns, archetype filtering |
| Characters | 1, 2, 3 | Player stats, JSONB bonuses, status effects |
| CodexEntries | 1 | Lore system, fragment unlocks |
| DataCaptures | 1 | Journal fragments, FK to CodexEntries |
| InteractableObjects | 1, 3 | Base + DynamicHazard TPH extension |
| InventoryItems | 1 | Many-to-many with cascading delete |
| Items | 1, 2 | Base + Equipment TPH, JSONB tags |
| ItemProperties | 1 | Item modifiers, JSONB stat modifiers |
| Rooms | 1, 3 | Spatial grid, JSONB exits, FK to conditions |
| SaveGames | 1 | Slot-based saves, full JSONB state |
| Trauma | 1 | Character trauma tracking |
| AmbientConditions | 3 | Environment tick effects |
| HazardTemplates | 3 | Procedural hazard spawning |
| BiomeDefinitions | 4 | Biome generation parameters |
| RoomTemplates | 4 | Dynamic room generation |
| BiomeElements | 4 | Spawn weights, rules |

---

## 4. Behaviors

### 4.1 Up Method Patterns

The `Up()` method applies forward schema changes.

**Creating Tables:**

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.CreateTable(
        name: "Characters",
        columns: table => new
        {
            Id = table.Column<Guid>(type: "uuid", nullable: false),
            Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
            EquipmentBonuses = table.Column<string>(type: "jsonb", nullable: false),
            CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
        },
        constraints: table =>
        {
            table.PrimaryKey("PK_Characters", x => x.Id);
        });

    migrationBuilder.CreateIndex(
        name: "IX_Characters_Name",
        table: "Characters",
        column: "Name",
        unique: true);
}
```

**Adding Columns with Defaults:**

```csharp
migrationBuilder.AddColumn<string>(
    name: "Tags",
    table: "Items",
    type: "jsonb",
    nullable: false,
    defaultValue: "[]");  // Empty JSON array default
```

**Modifying Column Nullability:**

```csharp
migrationBuilder.AlterColumn<int>(
    name: "Archetype",
    table: "ActiveAbilities",
    type: "integer",
    nullable: true,       // Now nullable
    oldClrType: typeof(int),
    oldType: "integer");  // Was non-nullable
```

**Creating Foreign Keys:**

```csharp
migrationBuilder.AddForeignKey(
    name: "FK_Rooms_AmbientConditions_ConditionId",
    table: "Rooms",
    column: "ConditionId",
    principalTable: "AmbientConditions",
    principalColumn: "Id");  // No cascade - allows NULL
```

### 4.2 Down Method Patterns

The `Down()` method reverses the Up() changes for rollback.

**Dropping Tables (reverse order of dependencies):**

```csharp
protected override void Down(MigrationBuilder migrationBuilder)
{
    // Drop dependent table first
    migrationBuilder.DropTable(name: "BiomeElements");

    // Then parent tables
    migrationBuilder.DropTable(name: "RoomTemplates");
    migrationBuilder.DropTable(name: "BiomeDefinitions");
}
```

**Removing Columns:**

```csharp
migrationBuilder.DropColumn(name: "Tags", table: "Items");
migrationBuilder.DropColumn(name: "ActiveStatusEffects", table: "Characters");
```

**Reverting Column Modifications:**

```csharp
migrationBuilder.AlterColumn<int>(
    name: "Archetype",
    table: "ActiveAbilities",
    type: "integer",
    nullable: false,      // Restore non-nullable
    defaultValue: 0,      // Need default for existing NULL values
    oldClrType: typeof(int),
    oldType: "integer",
    oldNullable: true);
```

### 4.3 Migration Execution

Migrations are applied via CLI tooling, **not at application startup**:

```bash
# Apply all pending migrations
dotnet ef database update --project RuneAndRust.Persistence

# Apply migrations up to a specific one
dotnet ef database update AddEnvironmentEcosystemTables --project RuneAndRust.Persistence
```

**Note:** The application assumes the database schema is already migrated. `Program.cs` only runs seeders after database connection—it does **not** call `MigrateAsync()`. This design separates schema deployment from application execution for clearer operational control.

**Test Environment:** For integration tests, `PostgreSqlTestFixture` does call `MigrateAsync()` to ensure a clean test database:

```csharp
// PostgreSqlTestFixture.cs
await migrationContext.Database.MigrateAsync();
```

**MigrateAsync Behavior (when invoked):**
1. Connects to PostgreSQL
2. Creates `__EFMigrationsHistory` if not exists
3. Queries applied migrations
4. Compares against code migrations
5. Executes pending Up() methods in order
6. Records each migration in history table

---

## 5. Design-Time Factory

### 5.1 Purpose

The `DesignTimeDbContextFactory` enables EF Core CLI tools to create a DbContext instance without starting the full application host.

### 5.2 Implementation

```csharp
// DesignTimeDbContextFactory.cs
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<RuneAndRustDbContext>
{
    public RuneAndRustDbContext CreateDbContext(string[] args)
    {
        // Environment variable override for CI/CD
        var connectionString = Environment.GetEnvironmentVariable("RUNEANDRUST_CONNECTION_STRING")
            ?? "Host=localhost;Port=5433;Database=RuneAndRust;Username=postgres;Password=password";

        var optionsBuilder = new DbContextOptionsBuilder<RuneAndRustDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new RuneAndRustDbContext(optionsBuilder.Options);
    }
}
```

### 5.3 CLI Commands

```bash
# Navigate to Persistence project
cd RuneAndRust.Persistence

# Add new migration
dotnet ef migrations add <MigrationName> --context RuneAndRustDbContext

# Generate SQL script (for review)
dotnet ef migrations script --idempotent

# Apply migrations manually
dotnet ef database update

# Rollback to specific migration
dotnet ef database update <TargetMigration>

# Remove last migration (if not applied)
dotnet ef migrations remove
```

---

## 6. PostgreSQL-Specific Features

### 6.1 JSONB Columns

PostgreSQL's JSONB type is used extensively for flexible, queryable JSON storage:

```csharp
// DDL Generation
table.Column<string>(type: "jsonb", nullable: false)

// With default value
table.Column<string>(
    name: "Tags",
    type: "jsonb",
    nullable: false,
    defaultValue: "[]")
```

**JSONB Usage in Schema:**

| Table | Column | Content Type |
|-------|--------|--------------|
| Characters | EquipmentBonuses | `Dictionary<string, int>` |
| Characters | ActiveStatusEffects | `List<StatusEffect>` |
| Items | Tags | `List<string>` |
| Items | AttributeBonuses | `Dictionary<Attribute, int>` |
| Items | Requirements | `Dictionary<string, int>` |
| Rooms | Exits | `Dictionary<Direction, Guid>` |
| SaveGames | SerializedState | Complete game state |
| CodexEntries | UnlockThresholds | `List<int>` |
| ItemProperties | StatModifiers | `Dictionary<string, int>` |
| BiomeDefinitions | AvailableTemplates | `List<string>` |
| RoomTemplates | NameTemplates | `List<string>` |
| BiomeElements | SpawnRules | Complex spawn config |

### 6.2 UUID Primary Keys

All tables use PostgreSQL native UUID type:

```csharp
Id = table.Column<Guid>(type: "uuid", nullable: false)
```

### 6.3 Integer Arrays

PostgreSQL-specific integer arrays for room features:

```csharp
table.Column<int[]>(
    name: "Features",
    type: "integer[]",
    nullable: false,
    defaultValue: new int[0])
```

### 6.4 Timestamp with Time Zone

All datetime columns use timezone-aware timestamps:

```csharp
table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
```

---

## 7. Restrictions

### 7.1 Migration Safety Rules

| Rule | Rationale |
|------|-----------|
| **Never delete Down() logic** | Required for rollback capability |
| **Always provide defaults for new non-nullable columns** | Prevents migration failure on existing data |
| **Drop FKs before tables** | PostgreSQL enforces referential integrity |
| **Order drops by dependency** | Child tables must drop before parents |

### 7.2 Data Preservation Requirements

- **Column type changes**: Must preserve existing data or provide migration script
- **Nullability changes**: Non-nullable → nullable is safe; reverse requires default
- **Index changes**: Dropping indexes doesn't affect data but impacts performance

### 7.3 JSONB Considerations

- JSONB structure changes don't require migrations
- Schema validation happens at application layer
- Empty array `[]` or empty object `{}` defaults for JSONB columns

---

## 8. Use Cases

### 8.1 Use Case: Adding a New Table

**Scenario:** Add a new `QuestLog` table to track player quests.

```csharp
// Generated migration: 20251224000000_AddQuestLogTable.cs
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.CreateTable(
        name: "QuestLogs",
        columns: table => new
        {
            Id = table.Column<Guid>(type: "uuid", nullable: false),
            CharacterId = table.Column<Guid>(type: "uuid", nullable: false),
            QuestId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
            State = table.Column<int>(type: "integer", nullable: false),
            Progress = table.Column<string>(type: "jsonb", nullable: false),
            StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
        },
        constraints: table =>
        {
            table.PrimaryKey("PK_QuestLogs", x => x.Id);
            table.ForeignKey(
                name: "FK_QuestLogs_Characters_CharacterId",
                column: x => x.CharacterId,
                principalTable: "Characters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        });

    migrationBuilder.CreateIndex(
        name: "IX_QuestLogs_CharacterId",
        table: "QuestLogs",
        column: "CharacterId");
}

protected override void Down(MigrationBuilder migrationBuilder)
{
    migrationBuilder.DropTable(name: "QuestLogs");
}
```

### 8.2 Use Case: Adding JSONB Column to Existing Table

**Scenario:** Add a `Modifiers` JSONB column to track dynamic item modifications.

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.AddColumn<string>(
        name: "Modifiers",
        table: "Items",
        type: "jsonb",
        nullable: false,
        defaultValue: "{}");  // Empty object for existing rows
}

protected override void Down(MigrationBuilder migrationBuilder)
{
    migrationBuilder.DropColumn(name: "Modifiers", table: "Items");
}
```

### 8.3 Use Case: Modifying Column Constraints

**Scenario:** Make an existing required column optional.

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.AlterColumn<string>(
        name: "DetailedDescription",
        table: "Items",
        type: "character varying(1000)",
        maxLength: 1000,
        nullable: true,  // Was false
        oldClrType: typeof(string),
        oldType: "character varying(1000)",
        oldMaxLength: 1000);
}

protected override void Down(MigrationBuilder migrationBuilder)
{
    // Provide default for NULL values before making non-nullable
    migrationBuilder.Sql(
        "UPDATE \"Items\" SET \"DetailedDescription\" = 'No description' WHERE \"DetailedDescription\" IS NULL");

    migrationBuilder.AlterColumn<string>(
        name: "DetailedDescription",
        table: "Items",
        type: "character varying(1000)",
        maxLength: 1000,
        nullable: false,
        oldClrType: typeof(string),
        oldType: "character varying(1000)",
        oldMaxLength: 1000,
        oldNullable: true);
}
```

### 8.4 Use Case: Adding TPH Discriminator Columns

**Scenario:** Extend InteractableObjects with DynamicHazard subtype (as done in Migration 3).

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // Add discriminator columns for TPH
    migrationBuilder.AddColumn<int>(
        name: "HazardType",
        table: "InteractableObjects",
        type: "integer",
        nullable: true);  // Nullable because not all InteractableObjects are hazards

    migrationBuilder.AddColumn<string>(
        name: "EffectScript",
        table: "InteractableObjects",
        type: "character varying(500)",
        maxLength: 500,
        nullable: true);

    // ... additional DynamicHazard columns
}
```

### 8.5 Use Case: Adding Foreign Key Relationship

**Scenario:** Link Rooms to AmbientConditions.

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // Add FK column first
    migrationBuilder.AddColumn<Guid>(
        name: "ConditionId",
        table: "Rooms",
        type: "uuid",
        nullable: true);

    // Create index for query performance
    migrationBuilder.CreateIndex(
        name: "IX_Rooms_ConditionId",
        table: "Rooms",
        column: "ConditionId");

    // Add foreign key constraint
    migrationBuilder.AddForeignKey(
        name: "FK_Rooms_AmbientConditions_ConditionId",
        table: "Rooms",
        column: "ConditionId",
        principalTable: "AmbientConditions",
        principalColumn: "Id");  // No cascade - allows orphan rooms
}

protected override void Down(MigrationBuilder migrationBuilder)
{
    // Remove FK first
    migrationBuilder.DropForeignKey(
        name: "FK_Rooms_AmbientConditions_ConditionId",
        table: "Rooms");

    migrationBuilder.DropIndex(
        name: "IX_Rooms_ConditionId",
        table: "Rooms");

    migrationBuilder.DropColumn(
        name: "ConditionId",
        table: "Rooms");
}
```

### 8.6 Use Case: Rolling Back a Migration

**Scenario:** Production issue requires rollback of last migration.

```bash
# Check current migration state
dotnet ef migrations list

# Rollback to previous migration
dotnet ef database update AddEnvironmentEcosystemTables

# Or programmatically in C#:
await context.Database.MigrateAsync("AddEnvironmentEcosystemTables");
```

**Rollback executes:**
1. Identifies current migration (MakeActiveAbilityArchetypeNullable)
2. Executes Down() method
3. Updates __EFMigrationsHistory
4. Schema now matches AddEnvironmentEcosystemTables state

---

## 9. Decision Trees

### 9.1 Migration Creation Flow

```
                    ┌─────────────────────┐
                    │ Model Class Changed │
                    └──────────┬──────────┘
                               │
                               ▼
                    ┌─────────────────────┐
                    │ Change Type?        │
                    └──────────┬──────────┘
                               │
          ┌────────────────────┼────────────────────┐
          │                    │                    │
          ▼                    ▼                    ▼
    ┌───────────┐       ┌───────────┐       ┌───────────┐
    │ New Table │       │ New Column│       │ Modify    │
    └─────┬─────┘       └─────┬─────┘       │ Column    │
          │                   │             └─────┬─────┘
          │                   │                   │
          ▼                   ▼                   ▼
    ┌───────────────────────────────────────────────────┐
    │     dotnet ef migrations add <DescriptiveName>    │
    └───────────────────────────────────────────────────┘
                               │
                               ▼
                    ┌─────────────────────┐
                    │ Review Generated    │
                    │ Up() and Down()     │
                    └──────────┬──────────┘
                               │
                               ▼
                    ┌─────────────────────┐
                    │ Test Migration      │
                    │ Locally             │
                    └──────────┬──────────┘
                               │
                               ▼
                    ┌─────────────────────┐
                    │ Commit Migration    │
                    │ Files to Git        │
                    └─────────────────────┘
```

### 9.2 Rollback Decision Flow

```
                    ┌─────────────────────┐
                    │ Schema Issue        │
                    │ Detected            │
                    └──────────┬──────────┘
                               │
                               ▼
                    ┌─────────────────────┐
                    │ Migration Applied   │
                    │ to Production?      │
                    └──────────┬──────────┘
                               │
             ┌─────────────────┴─────────────────┐
             │ NO                                │ YES
             ▼                                   ▼
    ┌─────────────────────┐            ┌─────────────────────┐
    │ dotnet ef migrations│            │ Data Loss Risk?     │
    │ remove              │            └──────────┬──────────┘
    │ (removes last       │                       │
    │ unapplied migration)│         ┌─────────────┴─────────────┐
    └─────────────────────┘         │ NO                        │ YES
                                    ▼                           ▼
                           ┌─────────────────────┐    ┌─────────────────────┐
                           │ dotnet ef database  │    │ Create FORWARD      │
                           │ update <Previous>   │    │ migration to fix    │
                           │ (runs Down() method)│    │ (never rollback     │
                           └─────────────────────┘    │ with data loss)     │
                                                      └─────────────────────┘
```

### 9.3 PostgreSQL Type Selection

```
                    ┌─────────────────────┐
                    │ C# Type to Map      │
                    └──────────┬──────────┘
                               │
        ┌──────────────────────┼──────────────────────┐
        │                      │                      │
        ▼                      ▼                      ▼
  ┌───────────┐          ┌───────────┐          ┌───────────┐
  │ Primitive │          │ Collection│          │ Complex   │
  │ Type      │          │ Type      │          │ Object    │
  └─────┬─────┘          └─────┬─────┘          └─────┬─────┘
        │                      │                      │
        ▼                      ▼                      ▼
  ┌───────────────┐      ┌───────────────┐      ┌───────────────┐
  │ Guid → uuid   │      │ List<T>?      │      │ Use JSONB     │
  │ int → integer │      └───────┬───────┘      │ with Value    │
  │ string → varchar     │       │              │ Converter     │
  │ DateTime → timestamp │       │              └───────────────┘
  │ bool → boolean│      │       │
  └───────────────┘      ▼       ▼
                   ┌─────────┐ ┌─────────┐
                   │ int[]?  │ │ Other   │
                   │ Use     │ │ Use     │
                   │ integer[]│ │ JSONB   │
                   └─────────┘ └─────────┘
```

---

## 10. Testing Strategy

### 10.1 Migration Testing

```csharp
public class MigrationTests
{
    [Fact]
    public async Task MigrationsApplyCleanly()
    {
        // Arrange - fresh database
        var options = new DbContextOptionsBuilder<RuneAndRustDbContext>()
            .UseNpgsql("Host=localhost;Port=5433;Database=RuneAndRust_Test;...")
            .Options;

        using var context = new RuneAndRustDbContext(options);

        // Act - apply all migrations
        await context.Database.EnsureDeletedAsync();
        await context.Database.MigrateAsync();

        // Assert - schema is valid
        var tables = await context.Database.SqlQuery<string>(
            $"SELECT tablename FROM pg_tables WHERE schemaname = 'public'")
            .ToListAsync();

        Assert.Contains("Characters", tables);
        Assert.Contains("Items", tables);
        Assert.Contains("Rooms", tables);
    }

    [Fact]
    public async Task DownMigrationsExecuteCleanly()
    {
        var options = new DbContextOptionsBuilder<RuneAndRustDbContext>()
            .UseNpgsql("Host=localhost;Port=5433;Database=RuneAndRust_Test;...")
            .Options;

        using var context = new RuneAndRustDbContext(options);

        // Apply all migrations
        await context.Database.MigrateAsync();

        // Rollback to initial state
        await context.Database.MigrateAsync("0");  // Empty string = initial state

        // Verify clean state
        var tables = await context.Database.SqlQuery<string>(
            $"SELECT tablename FROM pg_tables WHERE schemaname = 'public'")
            .ToListAsync();

        Assert.DoesNotContain("Characters", tables);
    }
}
```

### 10.2 Schema Validation

```csharp
[Fact]
public void ModelMatchesDatabaseSchema()
{
    var options = new DbContextOptionsBuilder<RuneAndRustDbContext>()
        .UseNpgsql(TestConnectionString)
        .Options;

    using var context = new RuneAndRustDbContext(options);

    // This throws if model doesn't match database
    var pendingMigrations = context.Database.GetPendingMigrations();

    Assert.Empty(pendingMigrations);
}
```

---

## 11. Configuration

### 11.1 Connection Strings

**Development (default):**
```
Host=localhost;Port=5433;Database=RuneAndRust;Username=postgres;Password=password
```

**Environment Variable Override:**
```bash
export RUNEANDRUST_CONNECTION_STRING="Host=prod-server;Port=5432;Database=RuneAndRust;..."
```

### 11.2 appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5433;Database=RuneAndRust;Username=postgres;Password=password"
  }
}
```

### 11.3 Program.cs DI Registration

```csharp
// Line 44-58 in Program.cs
builder.Services.AddDbContext<RuneAndRustDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
```

---

## 12. Cross-References

| Specification | Relationship |
|---------------|--------------|
| [SPEC-REPO-001](./SPEC-REPO-001.md) | Repositories depend on migrated schema |
| [SPEC-SEED-001](./SPEC-SEED-001.md) | Seeding runs after migrations complete |
| [SPEC-SAVE-001](./SPEC-SAVE-001.md) | SaveGames table created by InitialCreate |

---

## 13. Design Rationale

### 13.1 Why EF Core Code-First?

| Alternative | Rejected Because |
|-------------|------------------|
| Database-First | Schema changes hard to track in source control |
| Raw SQL Scripts | No automatic rollback generation |
| Dapper Migrations | Manual effort, no model synchronization |
| FluentMigrator | Additional dependency, less EF Core integration |

**Benefits of Code-First:**
- Migrations generated from C# model changes
- Automatic Up()/Down() method generation
- Model snapshot tracks expected schema state
- CLI tools for development workflow
- MigrateAsync() for runtime execution

### 13.2 Why PostgreSQL JSONB?

| Alternative | Rejected Because |
|-------------|------------------|
| Separate Tables | Excessive JOINs for nested data |
| XML Columns | Less query capability, larger storage |
| VARCHAR JSON | No indexing, no query operators |

**Benefits of JSONB:**
- Query JSON paths with GIN indexes
- Schema flexibility without migrations
- Compact binary storage
- Native PostgreSQL operators (@>, ->, ?)

### 13.3 Why CLI Migration (Not Startup)?

| Alternative | Status |
|-------------|--------|
| **CLI Migration** | **Adopted** - Explicit control, clear separation of concerns |
| Automatic Startup | Rejected - Mixes deployment concerns with application execution |
| CI/CD Only | Considered - Good for production, but CLI also supports local dev |
| Migration Service | Rejected - Unnecessary infrastructure complexity |

**Benefits of CLI Migration:**
- Explicit control over when migrations run
- Cleaner separation: deployment vs. runtime
- Easier debugging when migrations fail
- Test infrastructure uses `MigrateAsync()` for isolated databases
- Works for local development (`dotnet ef database update`) and CI/CD pipelines

---

## 14. Future Considerations

### 14.1 Planned Enhancements

- **Migration Health Checks**: Add migration status to health endpoint
- **Seeding Migrations**: Consider EF Core HasData() for static data
- **Migration Bundles**: Pre-compiled migration bundles for deployment
- **Schema Comparison**: Automated drift detection between environments

### 14.2 Scaling Considerations

- **Large Table Migrations**: Use batched updates for data migrations
- **Zero-Downtime Deployments**: Consider rolling migrations with backward compatibility
- **Read Replicas**: Migration timing coordination with replica lag

---

## 15. AAM-VOICE Compliance

### 15.1 Migration System Narrative Context

The Migration System operates at the **infrastructure layer** and does not directly generate player-facing content. However, migrations that seed data must ensure all inserted content complies with Domain 4 constraints.

### 15.2 Domain 4 Enforcement in Migrations

When migrations insert seed data (rare, prefer Seeder classes), all narrative content must avoid:

- ❌ Precision measurements ("45 Hz", "200 meters", "1,200 PSI")
- ❌ Technical terminology ("API", "Bug", "Glitch", "Debug")
- ❌ Exact percentages ("95% chance", "89% nocturnal")

Use instead:

- ✅ Qualitative descriptions ("low-frequency rumble", "a spear's throw")
- ✅ Archaic equivalents ("Anomaly", "Phenomenon", "Corruption")
- ✅ Epistemic uncertainty ("appears to", "suggests", "reportedly")

---

## Appendix A: Migration File Template

```csharp
using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RuneAndRust.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MigrationName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Forward changes here
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reverse changes here (must fully undo Up)
        }
    }
}
```

---

## Appendix B: __EFMigrationsHistory Schema

```sql
CREATE TABLE "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

-- Current contents:
-- MigrationId                                    | ProductVersion
-- ----------------------------------------------|---------------
-- 20251220231703_InitialCreate                  | 8.0.0
-- 20251221014933_AddRestSystemColumns           | 8.0.0
-- 20251222025251_AddEnvironmentEcosystemTables  | 8.0.0
-- 20251223022458_MakeActiveAbilityArchetypeNullable | 8.0.0
```

---

## Appendix C: Complete Index Registry

| Index Name | Table | Column(s) | Unique |
|------------|-------|-----------|--------|
| IX_ActiveAbilities_Archetype_Tier | ActiveAbilities | Archetype, Tier | No |
| IX_ActiveAbilities_Name | ActiveAbilities | Name | Yes |
| IX_Characters_Name | Characters | Name | Yes |
| IX_CodexEntries_Category | CodexEntries | Category | No |
| IX_CodexEntries_Title | CodexEntries | Title | Yes |
| IX_DataCaptures_CharacterId | DataCaptures | CharacterId | No |
| IX_DataCaptures_CodexEntryId | DataCaptures | CodexEntryId | No |
| IX_InteractableObjects_RoomId | InteractableObjects | RoomId | No |
| IX_InventoryItems_CharacterId | InventoryItems | CharacterId | No |
| IX_InventoryItems_IsEquipped | InventoryItems | IsEquipped | No |
| IX_InventoryItems_ItemId | InventoryItems | ItemId | No |
| IX_ItemProperties_ItemId | ItemProperties | ItemId | No |
| IX_Items_ItemType | Items | ItemType | No |
| IX_Items_Name | Items | Name | No |
| IX_Items_Quality | Items | Quality | No |
| IX_Items_Slot | Items | Slot | No |
| IX_SaveGames_SlotNumber | SaveGames | SlotNumber | Yes |
| IX_Trauma_CharacterId | Trauma | CharacterId | No |
| IX_AmbientConditions_Type | AmbientConditions | Type | Yes |
| IX_HazardTemplates_Name | HazardTemplates | Name | Yes |
| IX_Rooms_ConditionId | Rooms | ConditionId | No |
| IX_BiomeDefinitions_BiomeId | BiomeDefinitions | BiomeId | Yes |
| IX_BiomeElements_BiomeId | BiomeElements | BiomeId | No |
| IX_BiomeElements_ElementType | BiomeElements | ElementType | No |
| IX_RoomTemplates_Archetype | RoomTemplates | Archetype | No |
| IX_RoomTemplates_BiomeId | RoomTemplates | BiomeId | No |
| IX_RoomTemplates_TemplateId | RoomTemplates | TemplateId | Yes |

---

## Changelog

### v1.0.1 (2025-12-24)
**Critical Correction:**
- Fixed section 4.3: Migrations are applied via CLI (`dotnet ef database update`), **not at application startup**
- `Program.cs` does not call `MigrateAsync()`—only seeders run after database connection
- `PostgreSqlTestFixture` uses `MigrateAsync()` for integration test databases
- Updated section 13.3 to reflect CLI migration rationale
- Updated architecture diagram (Runtime → Test Fixture)
- Standardized frontmatter: `updated` → `last_updated`
- Added code traceability remark to DesignTimeDbContextFactory

### v1.0.0 (2025-12-22)
**Initial Release:**
- EF Core Code-First migration documentation
- 4 migrations: InitialCreate, AddRestSystemColumns, AddEnvironmentEcosystemTables, MakeActiveAbilityArchetypeNullable
- DesignTimeDbContextFactory for CLI tooling
- PostgreSQL-specific features (JSONB, arrays, GUIDs)
- Complete index registry (21 indexes)
