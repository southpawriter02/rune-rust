using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Models;
using RuneAndRust.Core.ValueObjects;
using CharacterAttribute = RuneAndRust.Core.Enums.Attribute;
using CharacterEntity = RuneAndRust.Core.Entities.Character;

namespace RuneAndRust.Persistence.Data;

/// <summary>
/// Entity Framework Core database context for Rune &amp; Rust.
/// Manages entity mappings and database configuration.
/// </summary>
public class RuneAndRustDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RuneAndRustDbContext"/> class.
    /// </summary>
    /// <param name="options">The database context options.</param>
    public RuneAndRustDbContext(DbContextOptions<RuneAndRustDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Gets or sets the SaveGames table.
    /// </summary>
    public DbSet<SaveGame> SaveGames { get; set; } = null!;

    /// <summary>
    /// Gets or sets the Rooms table.
    /// </summary>
    public DbSet<Room> Rooms { get; set; } = null!;

    /// <summary>
    /// Gets or sets the Characters table.
    /// </summary>
    public DbSet<CharacterEntity> Characters { get; set; } = null!;

    /// <summary>
    /// Gets or sets the InteractableObjects table (TPH - includes DynamicHazard).
    /// </summary>
    public DbSet<InteractableObject> InteractableObjects { get; set; } = null!;

    /// <summary>
    /// Gets or sets the DynamicHazards table view (subset of InteractableObjects via TPH, v0.3.3a).
    /// </summary>
    public DbSet<DynamicHazard> DynamicHazards { get; set; } = null!;

    /// <summary>
    /// Gets or sets the Items table (TPH - includes Equipment).
    /// </summary>
    public DbSet<Item> Items { get; set; } = null!;

    /// <summary>
    /// Gets or sets the Equipment table view (subset of Items via TPH).
    /// </summary>
    public DbSet<Equipment> Equipment { get; set; } = null!;

    /// <summary>
    /// Gets or sets the InventoryItems join table.
    /// </summary>
    public DbSet<InventoryItem> InventoryItems { get; set; } = null!;

    /// <summary>
    /// Gets or sets the CodexEntries table (Scavenger's Journal lore entries).
    /// </summary>
    public DbSet<CodexEntry> CodexEntries { get; set; } = null!;

    /// <summary>
    /// Gets or sets the DataCaptures table (player-discovered lore fragments).
    /// </summary>
    public DbSet<DataCapture> DataCaptures { get; set; } = null!;

    /// <summary>
    /// Gets or sets the ActiveAbilities table (combat abilities by archetype).
    /// </summary>
    public DbSet<ActiveAbility> ActiveAbilities { get; set; } = null!;

    /// <summary>
    /// Gets or sets the ItemProperties table (runic enchantments on items, v0.3.1c).
    /// </summary>
    public DbSet<ItemProperty> ItemProperties { get; set; } = null!;

    /// <summary>
    /// Gets or sets the AmbientConditions table (room environmental effects, v0.3.3b).
    /// </summary>
    public DbSet<AmbientCondition> AmbientConditions { get; set; } = null!;

    /// <summary>
    /// Gets or sets the HazardTemplates table (hazard template definitions, v0.3.3c).
    /// </summary>
    public DbSet<HazardTemplate> HazardTemplates { get; set; } = null!;

    /// <summary>
    /// Configures the entity mappings and relationships.
    /// </summary>
    /// <param name="modelBuilder">The model builder instance.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<SaveGame>(entity =>
        {
            entity.ToTable("SaveGames");

            entity.HasKey(s => s.Id);

            entity.HasIndex(s => s.SlotNumber)
                .IsUnique();

            entity.Property(s => s.SlotNumber)
                .IsRequired();

            entity.Property(s => s.CharacterName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(s => s.CreatedAt)
                .IsRequired();

            entity.Property(s => s.LastPlayed)
                .IsRequired();

            entity.Property(s => s.SerializedState)
                .HasColumnType("jsonb")
                .IsRequired();
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.ToTable("Rooms");

            entity.HasKey(r => r.Id);

            entity.Property(r => r.Name)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(r => r.Description)
                .HasMaxLength(2000)
                .IsRequired();

            entity.Property(r => r.IsStartingRoom)
                .IsRequired();

            // Map Coordinate as owned type (creates PositionX, PositionY, PositionZ columns)
            entity.OwnsOne(r => r.Position, position =>
            {
                position.Property(p => p.X).HasColumnName("PositionX").IsRequired();
                position.Property(p => p.Y).HasColumnName("PositionY").IsRequired();
                position.Property(p => p.Z).HasColumnName("PositionZ").IsRequired();
            });

            // Note: Unique index on position is handled at the application level
            // InMemory provider doesn't support unique indexes on owned types well

            // Store Exits dictionary as JSONB
            entity.Property(r => r.Exits)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<Dictionary<Direction, Guid>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<Direction, Guid>()
                )
                .IsRequired();
        });

        modelBuilder.Entity<CharacterEntity>(entity =>
        {
            entity.ToTable("Characters");

            entity.HasKey(c => c.Id);

            entity.HasIndex(c => c.Name)
                .IsUnique();

            entity.Property(c => c.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(c => c.Lineage)
                .IsRequired();

            entity.Property(c => c.Archetype)
                .IsRequired();

            // Core attributes
            entity.Property(c => c.Sturdiness).IsRequired();
            entity.Property(c => c.Might).IsRequired();
            entity.Property(c => c.Wits).IsRequired();
            entity.Property(c => c.Will).IsRequired();
            entity.Property(c => c.Finesse).IsRequired();

            // Derived stats
            entity.Property(c => c.MaxHP).IsRequired();
            entity.Property(c => c.CurrentHP).IsRequired();
            entity.Property(c => c.MaxStamina).IsRequired();
            entity.Property(c => c.CurrentStamina).IsRequired();
            entity.Property(c => c.ActionPoints).IsRequired();

            // Progression
            entity.Property(c => c.ExperiencePoints).IsRequired();
            entity.Property(c => c.Level).IsRequired();

            // Timestamps
            entity.Property(c => c.CreatedAt).IsRequired();
            entity.Property(c => c.LastModified).IsRequired();

            // Equipment bonuses stored as JSON
            entity.Property(c => c.EquipmentBonuses)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<Dictionary<CharacterAttribute, int>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<CharacterAttribute, int>()
                )
                .IsRequired();

            // ActiveStatusEffects stored as JSONB (v0.3.2a - Rest System)
            entity.Property(c => c.ActiveStatusEffects)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<StatusEffectType>>(v, (JsonSerializerOptions?)null) ?? new List<StatusEffectType>()
                )
                .IsRequired();

            // Inventory navigation property
            entity.HasMany(c => c.Inventory)
                .WithOne(i => i.Character)
                .HasForeignKey(i => i.CharacterId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // TPH (Table-per-Hierarchy) configuration for InteractableObject/DynamicHazard (v0.3.3a)
        modelBuilder.Entity<InteractableObject>(entity =>
        {
            entity.ToTable("InteractableObjects");

            entity.HasKey(o => o.Id);

            // TPH discriminator column based on ObjectType
            entity.HasDiscriminator(o => o.ObjectType)
                .HasValue<InteractableObject>(ObjectType.Furniture)
                .HasValue<InteractableObject>(ObjectType.Container)
                .HasValue<InteractableObject>(ObjectType.Device)
                .HasValue<InteractableObject>(ObjectType.Inscription)
                .HasValue<InteractableObject>(ObjectType.Corpse)
                .HasValue<DynamicHazard>(ObjectType.Hazard);

            // Index on RoomId for efficient room-based queries
            entity.HasIndex(o => o.RoomId);

            entity.Property(o => o.RoomId)
                .IsRequired();

            entity.Property(o => o.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(o => o.ObjectType)
                .IsRequired();

            // Description tiers
            entity.Property(o => o.Description)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(o => o.DetailedDescription)
                .HasMaxLength(1000);

            entity.Property(o => o.ExpertDescription)
                .HasMaxLength(1000);

            // Container properties
            entity.Property(o => o.IsContainer).IsRequired();
            entity.Property(o => o.IsOpen).IsRequired();
            entity.Property(o => o.IsLocked).IsRequired();
            entity.Property(o => o.LockDifficulty).IsRequired();

            // Examination state
            entity.Property(o => o.HasBeenExamined).IsRequired();
            entity.Property(o => o.HighestExaminationTier).IsRequired();

            // Loot properties
            entity.Property(o => o.HasBeenSearched).IsRequired();
            entity.Property(o => o.LootTier);

            // Timestamps
            entity.Property(o => o.CreatedAt).IsRequired();
            entity.Property(o => o.LastModified).IsRequired();
        });

        // DynamicHazard-specific columns (v0.3.3a)
        modelBuilder.Entity<DynamicHazard>(entity =>
        {
            // Inherits from InteractableObject via TPH

            entity.Property(h => h.HazardType)
                .HasConversion<int>()
                .IsRequired();

            entity.Property(h => h.State)
                .HasConversion<int>()
                .IsRequired();

            entity.Property(h => h.CooldownRemaining)
                .IsRequired();

            entity.Property(h => h.MaxCooldown)
                .IsRequired();

            entity.Property(h => h.OneTimeUse)
                .IsRequired();

            entity.Property(h => h.Trigger)
                .HasConversion<int>()
                .IsRequired();

            entity.Property(h => h.RequiredDamageType)
                .HasConversion<int?>();

            entity.Property(h => h.DamageThreshold)
                .IsRequired();

            entity.Property(h => h.EffectScript)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(h => h.TriggerMessage)
                .HasMaxLength(500);
        });

        // TPH (Table-per-Hierarchy) configuration for Item/Equipment
        modelBuilder.Entity<Item>(entity =>
        {
            entity.ToTable("Items");

            entity.HasKey(i => i.Id);

            // TPH discriminator column
            entity.HasDiscriminator<string>("ItemDiscriminator")
                .HasValue<Item>("Item")
                .HasValue<Equipment>("Equipment");

            entity.Property(i => i.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(i => i.ItemType)
                .IsRequired();

            entity.Property(i => i.Description)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(i => i.DetailedDescription)
                .HasMaxLength(1000);

            entity.Property(i => i.Weight).IsRequired();
            entity.Property(i => i.Value).IsRequired();
            entity.Property(i => i.Quality).IsRequired();
            entity.Property(i => i.IsStackable).IsRequired();
            entity.Property(i => i.MaxStackSize).IsRequired();

            entity.Property(i => i.CreatedAt).IsRequired();
            entity.Property(i => i.LastModified).IsRequired();

            // Tags stored as JSONB (v0.3.2a - Rest System)
            entity.Property(i => i.Tags)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
                )
                .IsRequired();

            // Indexes
            entity.HasIndex(i => i.Name);
            entity.HasIndex(i => i.Quality);
            entity.HasIndex(i => i.ItemType);

            // Item.Properties relationship (v0.3.1c - Runeforging)
            entity.HasMany(i => i.Properties)
                .WithOne()
                .HasForeignKey("ItemId")
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ItemProperty configuration (v0.3.1c - Runeforging enchantments)
        modelBuilder.Entity<ItemProperty>(entity =>
        {
            entity.ToTable("ItemProperties");

            entity.HasKey(p => p.Id);

            entity.Property(p => p.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(p => p.Description)
                .HasMaxLength(500);

            // Store StatModifiers dictionary as JSONB
            entity.Property(p => p.StatModifiers)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<Dictionary<string, int>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, int>()
                )
                .IsRequired();

            entity.Property(p => p.AppliedAt).IsRequired();
        });

        modelBuilder.Entity<Equipment>(entity =>
        {
            // Inherits from Item via TPH

            entity.Property(e => e.Slot).IsRequired();
            entity.Property(e => e.SoakBonus).IsRequired();
            entity.Property(e => e.DamageDie).IsRequired();

            // Store dictionaries as JSON
            entity.Property(e => e.AttributeBonuses)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<Dictionary<CharacterAttribute, int>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<CharacterAttribute, int>()
                )
                .IsRequired();

            entity.Property(e => e.Requirements)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<Dictionary<CharacterAttribute, int>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<CharacterAttribute, int>()
                )
                .IsRequired();

            entity.HasIndex(e => e.Slot);
        });

        modelBuilder.Entity<InventoryItem>(entity =>
        {
            entity.ToTable("InventoryItems");

            // Composite primary key
            entity.HasKey(ii => new { ii.CharacterId, ii.ItemId });

            entity.Property(ii => ii.Quantity).IsRequired();
            entity.Property(ii => ii.SlotPosition).IsRequired();
            entity.Property(ii => ii.IsEquipped).IsRequired();

            entity.Property(ii => ii.AddedAt).IsRequired();
            entity.Property(ii => ii.LastModified).IsRequired();

            // Relationships
            entity.HasOne(ii => ii.Character)
                .WithMany(c => c.Inventory)
                .HasForeignKey(ii => ii.CharacterId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ii => ii.Item)
                .WithMany()
                .HasForeignKey(ii => ii.ItemId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            entity.HasIndex(ii => ii.CharacterId);
            entity.HasIndex(ii => ii.IsEquipped);
        });

        modelBuilder.Entity<CodexEntry>(entity =>
        {
            entity.ToTable("CodexEntries");

            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.Title)
                .IsUnique();

            entity.Property(e => e.Title)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.Category)
                .IsRequired();

            entity.Property(e => e.FullText)
                .HasMaxLength(5000)
                .IsRequired();

            entity.Property(e => e.TotalFragments)
                .IsRequired();

            // Store UnlockThresholds dictionary as JSONB
            entity.Property(e => e.UnlockThresholds)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<Dictionary<int, string>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<int, string>()
                )
                .IsRequired();

            // Timestamps
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.LastModified).IsRequired();

            // One-to-many relationship with DataCaptures
            entity.HasMany(e => e.Fragments)
                .WithOne(d => d.CodexEntry)
                .HasForeignKey(d => d.CodexEntryId)
                .OnDelete(DeleteBehavior.SetNull);

            // Index for category-based queries
            entity.HasIndex(e => e.Category);
        });

        modelBuilder.Entity<DataCapture>(entity =>
        {
            entity.ToTable("DataCaptures");

            entity.HasKey(d => d.Id);

            // Index on CharacterId for fast journal lookup
            entity.HasIndex(d => d.CharacterId);

            // Index on CodexEntryId for fragment aggregation
            entity.HasIndex(d => d.CodexEntryId);

            entity.Property(d => d.CharacterId)
                .IsRequired();

            entity.Property(d => d.CodexEntryId);

            entity.Property(d => d.Type)
                .IsRequired();

            entity.Property(d => d.FragmentContent)
                .HasMaxLength(2000)
                .IsRequired();

            entity.Property(d => d.Source)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(d => d.Quality)
                .IsRequired();

            entity.Property(d => d.IsAnalyzed)
                .IsRequired();

            entity.Property(d => d.DiscoveredAt)
                .IsRequired();

            // Relationship to CodexEntry (many-to-one, optional)
            entity.HasOne(d => d.CodexEntry)
                .WithMany(e => e.Fragments)
                .HasForeignKey(d => d.CodexEntryId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ActiveAbility>(entity =>
        {
            entity.ToTable("ActiveAbilities");

            entity.HasKey(a => a.Id);

            entity.HasIndex(a => a.Name)
                .IsUnique();

            // Composite index for archetype + tier queries
            entity.HasIndex(a => new { a.Archetype, a.Tier });

            entity.Property(a => a.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(a => a.Description)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(a => a.EffectScript)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(a => a.StaminaCost)
                .IsRequired();

            entity.Property(a => a.AetherCost)
                .IsRequired();

            entity.Property(a => a.CooldownTurns)
                .IsRequired();

            entity.Property(a => a.Range)
                .IsRequired();

            entity.Property(a => a.Archetype)
                .IsRequired();

            entity.Property(a => a.Tier)
                .IsRequired();
        });

        // AmbientCondition configuration (v0.3.3b - Ambient Conditions)
        modelBuilder.Entity<AmbientCondition>(entity =>
        {
            entity.ToTable("AmbientConditions");

            entity.HasKey(c => c.Id);

            entity.HasIndex(c => c.Type)
                .IsUnique();

            entity.Property(c => c.Type)
                .HasConversion<int>()
                .IsRequired();

            entity.Property(c => c.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(c => c.Description)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(c => c.Color)
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(c => c.TickScript)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(c => c.TickChance)
                .IsRequired();

            // BiomeTags stored as JSONB (v0.3.3c - Environment Ecosystem)
            entity.Property(c => c.BiomeTags)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<BiomeType>>(v, (JsonSerializerOptions?)null) ?? new List<BiomeType>()
                )
                .IsRequired();
        });

        // Room -> Condition relationship (optional FK, v0.3.3b)
        modelBuilder.Entity<Room>()
            .HasOne<AmbientCondition>()
            .WithMany()
            .HasForeignKey(r => r.ConditionId)
            .IsRequired(false);

        // HazardTemplate configuration (v0.3.3c - Environment Ecosystem)
        modelBuilder.Entity<HazardTemplate>(entity =>
        {
            entity.ToTable("HazardTemplates");

            entity.HasKey(t => t.Id);

            entity.HasIndex(t => t.Name)
                .IsUnique();

            entity.Property(t => t.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(t => t.Description)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(t => t.HazardType)
                .HasConversion<int>()
                .IsRequired();

            entity.Property(t => t.Trigger)
                .HasConversion<int>()
                .IsRequired();

            entity.Property(t => t.EffectScript)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(t => t.MaxCooldown)
                .IsRequired();

            entity.Property(t => t.OneTimeUse)
                .IsRequired();

            // BiomeTags stored as JSONB
            entity.Property(t => t.BiomeTags)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<BiomeType>>(v, (JsonSerializerOptions?)null) ?? new List<BiomeType>()
                )
                .IsRequired();
        });
    }
}
