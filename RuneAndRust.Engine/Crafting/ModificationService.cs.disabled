using RuneAndRust.Core;
using RuneAndRust.Persistence;
using Serilog;
using System.Text.Json;

namespace RuneAndRust.Engine.Crafting;

/// <summary>
/// v0.36.3: Equipment Modification Service
/// Manages application and removal of runic inscriptions on equipment
/// </summary>
public class ModificationService
{
    private static readonly ILogger _log = Log.ForContext<ModificationService>();
    private readonly CraftingRepository _repository;

    private const int MAX_MODIFICATIONS_PER_ITEM = 3;

    public ModificationService(CraftingRepository repository)
    {
        _repository = repository;
        _log.Debug("ModificationService initialized");
    }

    /// <summary>
    /// Apply a runic inscription to equipment
    /// </summary>
    public ModificationResult ApplyModification(
        int characterId,
        int equipmentInventoryId,
        int inscriptionId)
    {
        _log.Information("Apply modification: Character={CharacterId}, Equipment={EquipmentId}, Inscription={InscriptionId}",
            characterId, equipmentInventoryId, inscriptionId);

        try
        {
            // Get inscription details
            var inscription = _repository.GetInscriptionById(inscriptionId);
            if (inscription == null)
            {
                _log.Warning("Inscription {InscriptionId} not found", inscriptionId);
                return ModificationResult.Failure("Inscription not found");
            }

            // Get equipment item
            var equipment = _repository.GetEquipmentItem(characterId, equipmentInventoryId);
            if (equipment == null)
            {
                _log.Warning("Equipment {EquipmentId} not found for character {CharacterId}",
                    equipmentInventoryId, characterId);
                return ModificationResult.Failure("Equipment not found");
            }

            // Validate equipment type matches inscription target
            if (!ValidateEquipmentType(equipment.ItemType, inscription.TargetEquipmentType))
            {
                _log.Information("Equipment type mismatch: {EquipmentType} cannot use {TargetType} inscription",
                    equipment.ItemType, inscription.TargetEquipmentType);
                return ModificationResult.Failure(
                    $"This inscription can only be applied to {inscription.TargetEquipmentType}");
            }

            // Check modification slot limit
            var existingMods = _repository.GetActiveModifications(equipmentInventoryId);
            if (existingMods.Count >= MAX_MODIFICATIONS_PER_ITEM)
            {
                _log.Information("Equipment {EquipmentId} has {Count} modifications (max {Max})",
                    equipmentInventoryId, existingMods.Count, MAX_MODIFICATIONS_PER_ITEM);
                return ModificationResult.Failure(
                    $"Equipment already has maximum {MAX_MODIFICATIONS_PER_ITEM} modifications");
            }

            // Parse component requirements
            var components = ParseComponentRequirements(inscription.ComponentRequirements);

            // Validate player has components
            var validation = ValidateComponentsInternal(characterId, components);
            if (!validation.Success)
            {
                _log.Information("Insufficient components for inscription {InscriptionId}: {Reason}",
                    inscriptionId, validation.Message);
                return ModificationResult.Failure(validation.Message);
            }

            // Check credit cost
            if (inscription.CraftingCostCredits > 0)
            {
                int playerCredits = _repository.GetCharacterCredits(characterId);
                if (playerCredits < inscription.CraftingCostCredits)
                {
                    return ModificationResult.Failure(
                        $"Insufficient credits: need {inscription.CraftingCostCredits}, have {playerCredits}");
                }
            }

            // Consume components
            bool consumed = _repository.ConsumeComponents(characterId, validation.ComponentsToConsume);
            if (!consumed)
            {
                _log.Error("Failed to consume components - rolling back modification attempt");
                return ModificationResult.Failure("Failed to consume components from inventory");
            }

            // Deduct credits
            if (inscription.CraftingCostCredits > 0)
            {
                _repository.DeductCredits(characterId, inscription.CraftingCostCredits);
            }

            // Apply modification
            int modificationId = _repository.CreateModification(equipmentInventoryId, inscription);

            _log.Information("Applied {InscriptionName} to equipment {EquipmentId} (mod {ModificationId})",
                inscription.InscriptionName, equipmentInventoryId, modificationId);

            return ModificationResult.Success(
                modificationId,
                inscription.InscriptionName,
                inscription.IsTemporary ? inscription.UsesIfTemporary : null);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to apply modification: Character={CharacterId}, Equipment={EquipmentId}",
                characterId, equipmentInventoryId);
            throw;
        }
    }

    /// <summary>
    /// Remove a modification from equipment
    /// </summary>
    public bool RemoveModification(int characterId, int modificationId)
    {
        _log.Information("Remove modification: Character={CharacterId}, Modification={ModificationId}",
            characterId, modificationId);

        try
        {
            // Verify modification belongs to character's equipment
            bool isValid = _repository.ValidateModificationOwnership(characterId, modificationId);
            if (!isValid)
            {
                _log.Warning("Modification {ModificationId} does not belong to character {CharacterId}",
                    modificationId, characterId);
                return false;
            }

            // Delete modification
            bool deleted = _repository.DeleteModification(modificationId);

            if (deleted)
            {
                _log.Information("Removed modification {ModificationId} for character {CharacterId}",
                    modificationId, characterId);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to remove modification: Character={CharacterId}, Modification={ModificationId}",
                characterId, modificationId);
            throw;
        }
    }

    /// <summary>
    /// Decrement uses on temporary modifications.
    /// Called after combat or equipment use.
    /// </summary>
    public void DecrementTemporaryModificationUses(int equipmentInventoryId)
    {
        _log.Debug("Decrement modification uses: Equipment={EquipmentId}", equipmentInventoryId);

        try
        {
            _repository.DecrementTemporaryModificationUses(equipmentInventoryId);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to decrement modification uses: Equipment={EquipmentId}",
                equipmentInventoryId);
            throw;
        }
    }

    /// <summary>
    /// Get all active modifications on equipment
    /// </summary>
    public List<EquipmentModification> GetActiveModifications(int equipmentInventoryId)
    {
        return _repository.GetActiveModifications(equipmentInventoryId);
    }

    /// <summary>
    /// Calculate total stat bonuses from all modifications on equipment
    /// </summary>
    public ModificationStats CalculateModificationStats(int equipmentInventoryId)
    {
        var modifications = _repository.GetActiveModifications(equipmentInventoryId);
        var stats = new ModificationStats();

        foreach (var mod in modifications)
        {
            try
            {
                var effectData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
                    mod.ModificationValue);

                if (effectData == null)
                    continue;

                switch (mod.ModificationType)
                {
                    case "Stat_Boost":
                        ApplyStatBoost(stats, effectData);
                        break;

                    case "Resistance":
                        ApplyResistance(stats, effectData);
                        break;

                    case "Elemental":
                        ApplyElementalEffect(stats, effectData);
                        break;

                    case "Status":
                        ApplyStatusEffect(stats, effectData);
                        break;

                    case "Special":
                        ApplySpecialEffect(stats, effectData);
                        break;

                    default:
                        _log.Warning("Unknown modification type: {Type}", mod.ModificationType);
                        break;
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Failed to parse modification {ModificationId}", mod.ModificationId);
            }
        }

        _log.Debug("Calculated stats for equipment {EquipmentId}: Damage+{Damage}, Mitigation+{Mitigation}, Elements={Elements}",
            equipmentInventoryId, stats.BonusDamage, stats.BonusMitigation, stats.ElementalDamage.Count);

        return stats;
    }

    #region Helper Methods

    /// <summary>
    /// Validate equipment type matches inscription target
    /// </summary>
    private bool ValidateEquipmentType(string equipmentType, string targetType)
    {
        if (targetType == "Both")
            return true;

        return equipmentType == targetType;
    }

    /// <summary>
    /// Parse component requirements from JSON
    /// </summary>
    private List<RecipeComponent> ParseComponentRequirements(string json)
    {
        try
        {
            var requirements = JsonSerializer.Deserialize<List<ComponentRequirement>>(json);
            if (requirements == null)
                return new List<RecipeComponent>();

            return requirements.Select(r => new RecipeComponent
            {
                ComponentItemId = r.ItemId,
                QuantityRequired = r.Quantity,
                MinimumQuality = r.MinQuality
            }).ToList();
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to parse component requirements: {Json}", json);
            return new List<RecipeComponent>();
        }
    }

    /// <summary>
    /// Validate player has required components (simplified version)
    /// </summary>
    private ComponentValidationResult ValidateComponentsInternal(
        int characterId,
        List<RecipeComponent> required)
    {
        var playerComponents = _repository.GetPlayerComponents(characterId);
        var componentsToConsume = new List<ConsumedComponent>();
        var missingComponents = new List<string>();

        foreach (var req in required)
        {
            // Find player components that match
            var availableComponents = playerComponents
                .Where(pc => pc.ItemId == req.ComponentItemId &&
                            pc.QualityTier >= req.MinimumQuality &&
                            pc.Quantity > 0)
                .OrderByDescending(pc => pc.QualityTier)
                .ToList();

            int totalAvailable = availableComponents.Sum(c => c.Quantity);

            if (totalAvailable < req.QuantityRequired)
            {
                string componentName = availableComponents.FirstOrDefault()?.ItemName ?? $"Item {req.ComponentItemId}";
                missingComponents.Add($"{componentName} (need {req.QuantityRequired}, have {totalAvailable})");
                continue;
            }

            // Allocate components to consume
            int remaining = req.QuantityRequired;
            foreach (var component in availableComponents)
            {
                if (remaining <= 0)
                    break;

                int toConsume = Math.Min(remaining, component.Quantity);
                componentsToConsume.Add(new ConsumedComponent
                {
                    ItemId = component.ItemId,
                    ItemName = component.ItemName,
                    Quantity = toConsume,
                    QualityTier = component.QualityTier
                });

                remaining -= toConsume;
            }
        }

        if (missingComponents.Any())
        {
            return new ComponentValidationResult
            {
                Success = false,
                Message = $"Insufficient components:\n{string.Join("\n", missingComponents)}",
                ComponentsToConsume = new List<ConsumedComponent>()
            };
        }

        return new ComponentValidationResult
        {
            Success = true,
            Message = string.Empty,
            ComponentsToConsume = componentsToConsume
        };
    }

    /// <summary>
    /// Apply stat boost modification to aggregate stats
    /// </summary>
    private void ApplyStatBoost(ModificationStats stats, Dictionary<string, JsonElement> effectData)
    {
        if (!effectData.ContainsKey("stat") || !effectData.ContainsKey("value"))
            return;

        string stat = effectData["stat"].GetString() ?? "";
        int value = effectData["value"].GetInt32();

        switch (stat.ToLower())
        {
            case "damage":
                stats.BonusDamage += value;
                break;
            case "mitigation":
                stats.BonusMitigation += value;
                break;
            case "accuracy":
                stats.BonusAccuracy += value;
                break;
            case "evasion":
                stats.BonusEvasion += value;
                break;
        }
    }

    /// <summary>
    /// Apply resistance modification to aggregate stats
    /// </summary>
    private void ApplyResistance(ModificationStats stats, Dictionary<string, JsonElement> effectData)
    {
        if (!effectData.ContainsKey("resistance_type") || !effectData.ContainsKey("value"))
            return;

        string resistanceType = effectData["resistance_type"].GetString() ?? "";
        int value = effectData["value"].GetInt32();

        if (!stats.Resistances.ContainsKey(resistanceType))
            stats.Resistances[resistanceType] = 0;

        stats.Resistances[resistanceType] += value;
    }

    /// <summary>
    /// Apply elemental effect modification to aggregate stats
    /// </summary>
    private void ApplyElementalEffect(ModificationStats stats, Dictionary<string, JsonElement> effectData)
    {
        if (!effectData.ContainsKey("element") || !effectData.ContainsKey("bonus_damage"))
            return;

        string element = effectData["element"].GetString() ?? "";
        int bonusDamage = effectData["bonus_damage"].GetInt32();
        double applicationChance = 0.0;

        // Check for optional application chance (burn_chance, freeze_chance, etc.)
        foreach (var key in effectData.Keys)
        {
            if (key.EndsWith("_chance"))
            {
                applicationChance = effectData[key].GetDouble();
                break;
            }
        }

        stats.ElementalDamage.Add(new ElementalEffect
        {
            Element = element,
            BonusDamage = bonusDamage,
            ApplicationChance = applicationChance
        });
    }

    /// <summary>
    /// Apply status effect modification to aggregate stats
    /// </summary>
    private void ApplyStatusEffect(ModificationStats stats, Dictionary<string, JsonElement> effectData)
    {
        if (!effectData.ContainsKey("status") ||
            !effectData.ContainsKey("application_chance") ||
            !effectData.ContainsKey("duration"))
            return;

        string statusName = effectData["status"].GetString() ?? "";
        double applicationChance = effectData["application_chance"].GetDouble();
        int duration = effectData["duration"].GetInt32();

        stats.StatusEffects.Add(new ModificationStatusEffect
        {
            StatusName = statusName,
            ApplicationChance = applicationChance,
            Duration = duration
        });
    }

    /// <summary>
    /// Apply special effect modification to aggregate stats
    /// </summary>
    private void ApplySpecialEffect(ModificationStats stats, Dictionary<string, JsonElement> effectData)
    {
        if (!effectData.ContainsKey("effect"))
            return;

        string effectType = effectData["effect"].GetString() ?? "";

        switch (effectType.ToLower())
        {
            case "regeneration":
                if (effectData.ContainsKey("hp_per_turn"))
                {
                    stats.RegenerationPerTurn = effectData["hp_per_turn"].GetInt32();
                }
                break;
            // Add more special effects as needed
        }
    }

    #endregion
}

/// <summary>
/// Internal validation result for component checking
/// </summary>
internal class ComponentValidationResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<ConsumedComponent> ComponentsToConsume { get; set; } = new();
}
