using System.Text.Json.Serialization;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.ValueObjects;

namespace RuneAndRust.Core.Serialization;

/// <summary>
/// Source-generated JSON serialization context for GameState (v0.3.18c - The Snapshot).
/// Eliminates reflection-based serialization overhead for save/load operations.
/// </summary>
/// <remarks>
/// Uses compile-time source generation to:
/// - Reduce startup time by eliminating runtime reflection
/// - Decrease memory allocations during serialization
/// - Enable AOT compilation compatibility
/// See: SPEC-SAVE-001 for Save/Load System design.
/// Note: Uses fully qualified type names to avoid ambiguity between Entities.Character and Models.Character.
/// </remarks>
[JsonSourceGenerationOptions(
    WriteIndented = false,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
// Primary types (fully qualified to avoid ambiguity)
[JsonSerializable(typeof(Models.GameState))]
[JsonSerializable(typeof(Entities.Character))]
// Collections in Character
[JsonSerializable(typeof(List<Entities.InventoryItem>))]
[JsonSerializable(typeof(ICollection<Entities.InventoryItem>))]
[JsonSerializable(typeof(List<Entities.Trauma>))]
[JsonSerializable(typeof(List<StatusEffectType>))]
[JsonSerializable(typeof(Dictionary<Enums.Attribute, int>))]
// Entity types
[JsonSerializable(typeof(Entities.InventoryItem))]
[JsonSerializable(typeof(Entities.Trauma))]
[JsonSerializable(typeof(Entities.Item))]
[JsonSerializable(typeof(Entities.Equipment))]
[JsonSerializable(typeof(Entities.ItemProperty))]
[JsonSerializable(typeof(List<Entities.ItemProperty>))]
// GameState collections
[JsonSerializable(typeof(HashSet<Guid>))]
[JsonSerializable(typeof(Dictionary<Guid, string>))] // UserNotes (v0.3.20a)
// Value objects
[JsonSerializable(typeof(Coordinate))]
// Primitive collections used across entities
[JsonSerializable(typeof(List<string>))]
[JsonSerializable(typeof(Dictionary<string, int>))]
// Enum types that need serialization
[JsonSerializable(typeof(GamePhase))]
[JsonSerializable(typeof(LineageType))]
[JsonSerializable(typeof(ArchetypeType))]
[JsonSerializable(typeof(BackgroundType))]
[JsonSerializable(typeof(TraumaType))]
[JsonSerializable(typeof(StatusEffectType))]
[JsonSerializable(typeof(ItemType))]
[JsonSerializable(typeof(QualityTier))]
[JsonSerializable(typeof(EquipmentSlot))]
[JsonSerializable(typeof(DamageType))]
public partial class GameStateContext : JsonSerializerContext
{
}
