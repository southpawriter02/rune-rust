using RuneAndRust.Core;
using RuneAndRust.Core.NewGamePlus;
using Serilog;
using System.Text.Json;

namespace RuneAndRust.Engine.NewGamePlus;

/// <summary>
/// v0.40.1: Carryover Service
/// Handles creation and application of progression snapshots for New Game+
/// </summary>
public class CarryoverService
{
    private static readonly ILogger _log = Log.ForContext<CarryoverService>();

    // ═════════════════════════════════════════════════════════════
    // SNAPSHOT CREATION
    // ═════════════════════════════════════════════════════════════

    /// <summary>
    /// Create a carryover snapshot from current character state
    /// </summary>
    public CarryoverSnapshot CreateSnapshot(PlayerCharacter character, int ngPlusTier)
    {
        using var operation = _log.BeginOperation(
            "CreateSnapshot for CharacterId={CharacterId}, Tier={Tier}",
            character.CharacterID, ngPlusTier);

        var snapshot = new CarryoverSnapshot
        {
            CharacterId = character.CharacterID,
            NGPlusTier = ngPlusTier,
            TimestampUtc = DateTime.UtcNow,

            // Character progression
            CharacterLevel = character.CurrentMilestone,
            LegendPoints = character.CurrentLegend,
            ProgressionPoints = character.ProgressionPoints,
            UnspentProgressionPoints = 0, // TODO: Add unspent PP tracking to PlayerCharacter

            // Attributes
            Attributes = new Dictionary<string, int>
            {
                { "MIGHT", character.Attributes.Might },
                { "FINESSE", character.Attributes.Finesse },
                { "WITS", character.Attributes.Wits },
                { "WILL", character.Attributes.Will },
                { "STURDINESS", character.Attributes.Sturdiness }
            },

            // Specializations and abilities
            UnlockedSpecializations = GetUnlockedSpecializations(character),
            LearnedAbilities = GetLearnedAbilities(character),

            // Equipment
            EquippedItems = GetEquippedItems(character),
            InventoryItems = GetInventoryItems(character),

            // Crafting
            CraftingMaterials = GetCraftingMaterials(character),
            UnlockedRecipes = new List<string>(), // TODO: Add recipe tracking

            // Currency
            Scrap = character.Currency,

            // Pre-reset snapshots (for debugging/verification)
            QuestStateSnapshot = JsonSerializer.Serialize(new
            {
                ActiveQuests = character.ActiveQuests.Select(q => q.Id).ToList(),
                CompletedQuests = character.CompletedQuests.Select(q => q.Id).ToList()
            }),
            WorldStateSnapshot = JsonSerializer.Serialize(new
            {
                CurrentSector = character.CurrentSectorId
            })
        };

        _log.Information(
            "Carryover snapshot created: CharacterId={CharacterId}, Level={Level}, PP={PP}, Equipment={EquipCount}",
            character.CharacterID, snapshot.CharacterLevel, snapshot.ProgressionPoints,
            snapshot.EquippedItems.Count + snapshot.InventoryItems.Count);

        operation.Complete();
        return snapshot;
    }

    private List<string> GetUnlockedSpecializations(PlayerCharacter character)
    {
        var specializations = new List<string>();

        // Add current specialization if set
        if (character.Specialization != Specialization.None)
        {
            specializations.Add(character.Specialization.ToString());
        }

        return specializations;
    }

    private List<string> GetLearnedAbilities(PlayerCharacter character)
    {
        return character.Abilities.Select(a => a.Name).ToList();
    }

    private List<Equipment> GetEquippedItems(PlayerCharacter character)
    {
        var equipped = new List<Equipment>();

        if (character.EquippedWeapon != null)
        {
            equipped.Add(character.EquippedWeapon);
        }

        if (character.EquippedArmor != null)
        {
            equipped.Add(character.EquippedArmor);
        }

        return equipped;
    }

    private List<Equipment> GetInventoryItems(PlayerCharacter character)
    {
        return character.Inventory.ToList();
    }

    private Dictionary<string, int> GetCraftingMaterials(PlayerCharacter character)
    {
        var materials = new Dictionary<string, int>();

        foreach (var component in character.CraftingComponents)
        {
            materials[component.Key.ToString()] = component.Value;
        }

        return materials;
    }

    // ═════════════════════════════════════════════════════════════
    // SNAPSHOT APPLICATION
    // ═════════════════════════════════════════════════════════════

    /// <summary>
    /// Apply a carryover snapshot to a character
    /// </summary>
    public void ApplySnapshot(PlayerCharacter character, CarryoverSnapshot snapshot)
    {
        using var operation = _log.BeginOperation(
            "ApplySnapshot for CharacterId={CharacterId}, SnapshotId={SnapshotId}",
            character.CharacterID, snapshot.CarryoverId);

        // Validate snapshot integrity
        if (!ValidateSnapshotIntegrity(snapshot))
        {
            _log.Error("Snapshot failed integrity validation: SnapshotId={SnapshotId}", snapshot.CarryoverId);
            throw new InvalidOperationException("Snapshot failed integrity validation");
        }

        // Apply character progression
        character.CurrentMilestone = snapshot.CharacterLevel;
        character.CurrentLegend = snapshot.LegendPoints;
        character.ProgressionPoints = snapshot.ProgressionPoints;

        // Apply attributes
        character.Attributes.Might = snapshot.Attributes["MIGHT"];
        character.Attributes.Finesse = snapshot.Attributes["FINESSE"];
        character.Attributes.Wits = snapshot.Attributes["WITS"];
        character.Attributes.Will = snapshot.Attributes["WILL"];
        character.Attributes.Sturdiness = snapshot.Attributes["STURDINESS"];

        // Apply currency
        character.Currency = snapshot.Scrap;

        // Apply specializations (simplified - would need proper restoration)
        if (snapshot.UnlockedSpecializations.Count > 0 &&
            Enum.TryParse<Specialization>(snapshot.UnlockedSpecializations[0], out var spec))
        {
            character.Specialization = spec;
        }

        // Apply abilities (simplified - would need proper restoration)
        // Note: Full ability restoration would require looking up ability definitions
        // and recreating ability instances. For now, we just log it.
        _log.Information("Abilities to restore: {AbilityCount}", snapshot.LearnedAbilities.Count);

        // Apply equipment (simplified)
        if (snapshot.EquippedItems.Count > 0)
        {
            foreach (var item in snapshot.EquippedItems)
            {
                if (item.Type == EquipmentType.Weapon)
                {
                    character.EquippedWeapon = item;
                }
                else if (item.Type == EquipmentType.Armor)
                {
                    character.EquippedArmor = item;
                }
            }
        }

        // Apply inventory
        character.Inventory.Clear();
        character.Inventory.AddRange(snapshot.InventoryItems);

        // Apply crafting materials (simplified)
        character.CraftingComponents.Clear();
        foreach (var material in snapshot.CraftingMaterials)
        {
            if (Enum.TryParse<ComponentType>(material.Key, out var componentType))
            {
                character.CraftingComponents[componentType] = material.Value;
            }
        }

        _log.Information(
            "Carryover snapshot applied successfully: CharacterId={CharacterId}, Level={Level}, PP={PP}",
            character.CharacterID, snapshot.CharacterLevel, snapshot.ProgressionPoints);

        operation.Complete();
    }

    // ═════════════════════════════════════════════════════════════
    // SNAPSHOT VALIDATION
    // ═════════════════════════════════════════════════════════════

    /// <summary>
    /// Validate snapshot integrity (prevent cheating/impossible values)
    /// </summary>
    public bool ValidateSnapshotIntegrity(CarryoverSnapshot snapshot)
    {
        // Check for impossible character level
        if (snapshot.CharacterLevel < 0 || snapshot.CharacterLevel > 50)
        {
            _log.Warning("Invalid character level in snapshot: {Level}", snapshot.CharacterLevel);
            return false;
        }

        // Check for negative progression points
        if (snapshot.ProgressionPoints < 0 || snapshot.UnspentProgressionPoints < 0)
        {
            _log.Warning("Negative PP in snapshot: Spent={Spent}, Unspent={Unspent}",
                snapshot.ProgressionPoints, snapshot.UnspentProgressionPoints);
            return false;
        }

        // Check for impossible attribute values
        foreach (var attr in snapshot.Attributes)
        {
            if (attr.Value < 8 || attr.Value > 30)
            {
                _log.Warning("Invalid attribute value in snapshot: {Attr}={Value}",
                    attr.Key, attr.Value);
                return false;
            }
        }

        // Check for negative currency
        if (snapshot.Scrap < 0)
        {
            _log.Warning("Negative currency in snapshot: Scrap={Scrap}", snapshot.Scrap);
            return false;
        }

        // All checks passed
        return true;
    }

    // ═════════════════════════════════════════════════════════════
    // HELPER METHODS
    // ═════════════════════════════════════════════════════════════

    /// <summary>
    /// Compare two snapshots for debugging
    /// </summary>
    public string CompareSnapshots(CarryoverSnapshot before, CarryoverSnapshot after)
    {
        var differences = new List<string>();

        if (before.CharacterLevel != after.CharacterLevel)
        {
            differences.Add($"Level: {before.CharacterLevel} → {after.CharacterLevel}");
        }

        if (before.LegendPoints != after.LegendPoints)
        {
            differences.Add($"Legend: {before.LegendPoints} → {after.LegendPoints}");
        }

        if (before.ProgressionPoints != after.ProgressionPoints)
        {
            differences.Add($"PP: {before.ProgressionPoints} → {after.ProgressionPoints}");
        }

        return differences.Count > 0
            ? string.Join(", ", differences)
            : "No differences";
    }
}
