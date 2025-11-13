using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// Service for managing equipment, inventory, and stat calculations
/// </summary>
public class EquipmentService
{
    private static readonly ILogger _log = Log.ForContext<EquipmentService>();

    /// <summary>
    /// Equip a weapon from inventory
    /// </summary>
    public bool EquipWeapon(PlayerCharacter player, Equipment weapon)
    {
        if (weapon.Type != EquipmentType.Weapon)
        {
            _log.Warning("Attempted to equip non-weapon: Character={CharacterName}, Item={ItemName}, Type={ItemType}",
                player.Name, weapon.Name, weapon.Type);
            return false;
        }

        var oldWeapon = player.EquippedWeapon;

        // Remove from inventory if present
        player.Inventory.Remove(weapon);

        // Unequip current weapon to inventory (if any and if there's space)
        if (player.EquippedWeapon != null)
        {
            if (player.Inventory.Count < player.MaxInventorySize)
            {
                player.Inventory.Add(player.EquippedWeapon);
            }
            // If inventory is full, the old weapon is dropped (handled by caller)
        }

        // Equip new weapon
        player.EquippedWeapon = weapon;

        // Recalculate stats
        RecalculatePlayerStats(player);

        _log.Information("Weapon equipped: Character={CharacterName}, NewWeapon={NewWeapon}, OldWeapon={OldWeapon}",
            player.Name, weapon.Name, oldWeapon?.Name ?? "None");

        return true;
    }

    /// <summary>
    /// Equip armor from inventory
    /// </summary>
    public bool EquipArmor(PlayerCharacter player, Equipment armor)
    {
        if (armor.Type != EquipmentType.Armor)
        {
            _log.Warning("Attempted to equip non-armor: Character={CharacterName}, Item={ItemName}, Type={ItemType}",
                player.Name, armor.Name, armor.Type);
            return false;
        }

        var oldArmor = player.EquippedArmor;

        // Remove from inventory if present
        player.Inventory.Remove(armor);

        // Unequip current armor to inventory (if any and if there's space)
        if (player.EquippedArmor != null)
        {
            if (player.Inventory.Count < player.MaxInventorySize)
            {
                player.Inventory.Add(player.EquippedArmor);
            }
            // If inventory is full, the old armor is dropped (handled by caller)
        }

        // Equip new armor
        player.EquippedArmor = armor;

        // Recalculate stats
        RecalculatePlayerStats(player);

        _log.Information("Armor equipped: Character={CharacterName}, NewArmor={NewArmor}, OldArmor={OldArmor}",
            player.Name, armor.Name, oldArmor?.Name ?? "None");

        return true;
    }

    /// <summary>
    /// Unequip weapon to inventory
    /// </summary>
    public bool UnequipWeapon(PlayerCharacter player)
    {
        if (player.EquippedWeapon == null)
        {
            return false;
        }

        if (player.Inventory.Count >= player.MaxInventorySize)
        {
            return false; // Inventory full
        }

        player.Inventory.Add(player.EquippedWeapon);
        player.EquippedWeapon = null;

        RecalculatePlayerStats(player);
        return true;
    }

    /// <summary>
    /// Unequip armor to inventory
    /// </summary>
    public bool UnequipArmor(PlayerCharacter player)
    {
        if (player.EquippedArmor == null)
        {
            return false;
        }

        if (player.Inventory.Count >= player.MaxInventorySize)
        {
            return false; // Inventory full
        }

        player.Inventory.Add(player.EquippedArmor);
        player.EquippedArmor = null;

        RecalculatePlayerStats(player);
        return true;
    }

    /// <summary>
    /// Add item to player inventory
    /// </summary>
    public bool AddToInventory(PlayerCharacter player, Equipment item)
    {
        if (player.Inventory.Count >= player.MaxInventorySize)
        {
            return false; // Inventory full
        }

        player.Inventory.Add(item);
        return true;
    }

    /// <summary>
    /// Remove item from player inventory
    /// </summary>
    public bool RemoveFromInventory(PlayerCharacter player, Equipment item)
    {
        return player.Inventory.Remove(item);
    }

    /// <summary>
    /// Pick up item from room ground to inventory
    /// </summary>
    public bool PickupItem(PlayerCharacter player, Room room, Equipment item)
    {
        if (!room.ItemsOnGround.Contains(item))
        {
            return false;
        }

        if (player.Inventory.Count >= player.MaxInventorySize)
        {
            return false; // Inventory full
        }

        room.ItemsOnGround.Remove(item);
        player.Inventory.Add(item);
        return true;
    }

    /// <summary>
    /// Drop item from inventory to room ground
    /// </summary>
    public bool DropItem(PlayerCharacter player, Room room, Equipment item)
    {
        if (!player.Inventory.Contains(item))
        {
            return false;
        }

        player.Inventory.Remove(item);
        room.ItemsOnGround.Add(item);
        return true;
    }

    /// <summary>
    /// Find item in inventory by name (case-insensitive, partial match)
    /// </summary>
    public Equipment? FindInInventory(PlayerCharacter player, string itemName)
    {
        return player.Inventory.FirstOrDefault(e =>
            e.Name.Contains(itemName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Find item on ground by name (case-insensitive, partial match)
    /// </summary>
    public Equipment? FindOnGround(Room room, string itemName)
    {
        return room.ItemsOnGround.FirstOrDefault(e =>
            e.Name.Contains(itemName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Recalculate player stats based on equipped items
    /// This updates MaxHP and attribute bonuses from equipment
    /// </summary>
    public void RecalculatePlayerStats(PlayerCharacter player)
    {
        // Store current HP ratio to maintain percentage after equipment change
        float hpRatio = player.MaxHP > 0 ? (float)player.HP / player.MaxHP : 1.0f;

        // Reset MaxHP to base (will be recalculated from equipment)
        // Base MaxHP depends on class and level
        int baseMaxHP = GetBaseMaxHP(player);
        player.MaxHP = baseMaxHP;

        // v0.7.1: Apply Warrior's Vigor passive (+10% Max HP)
        if (player.Abilities.Any(a => a.Name == "Warrior's Vigor"))
        {
            player.MaxHP = (int)(player.MaxHP * 1.10f);
        }

        // Apply armor HP bonus
        if (player.EquippedArmor != null)
        {
            player.MaxHP += player.EquippedArmor.HPBonus;
        }

        // Restore HP ratio (but don't exceed new MaxHP)
        player.HP = Math.Min((int)(player.MaxHP * hpRatio), player.MaxHP);

        // v0.19.8: Recalculate Aether Pool for Mystics
        if (player.Class == CharacterClass.Mystic)
        {
            // Store current AP ratio to maintain percentage after equipment change
            float apRatio = player.MaxAP > 0 ? (float)player.AP / player.MaxAP : 1.0f;

            // Base MaxAP = (WILL × 10) + 50
            int baseMaxAP = (player.Attributes.Will * 10) + 50;
            player.MaxAP = baseMaxAP;

            // v0.19.8: Apply Aetheric Attunement passive (+10% Max AP)
            if (player.Abilities.Any(a => a.Name == "Aetheric Attunement"))
            {
                player.MaxAP = (int)(player.MaxAP * 1.10f);
            }

            // Restore AP ratio (but don't exceed new MaxAP)
            player.AP = Math.Min((int)(player.MaxAP * apRatio), player.MaxAP);
        }

        // Note: Attribute bonuses are calculated dynamically in GetEffectiveAttributeValue
        // rather than modifying the base Attributes object
    }

    /// <summary>
    /// Get base MaxHP for character (class base + milestone bonuses)
    /// </summary>
    private int GetBaseMaxHP(PlayerCharacter player)
    {
        // Base HP from character creation (from CharacterFactory)
        int baseHP = player.Class switch
        {
            CharacterClass.Warrior => 50,
            CharacterClass.Scavenger => 40,
            CharacterClass.Mystic => 30,
            _ => 40
        };

        // Add +10 HP per milestone (from v0.2 progression system)
        baseHP += player.CurrentMilestone * 10;

        return baseHP;
    }

    /// <summary>
    /// Get effective attribute value including equipment bonuses
    /// </summary>
    public int GetEffectiveAttributeValue(PlayerCharacter player, string attributeName)
    {
        int baseValue = player.GetAttributeValue(attributeName);
        int bonus = 0;

        // Add bonuses from equipped weapon
        if (player.EquippedWeapon != null)
        {
            foreach (var equipBonus in player.EquippedWeapon.Bonuses)
            {
                if (equipBonus.AttributeName.Equals(attributeName, StringComparison.OrdinalIgnoreCase))
                {
                    bonus += equipBonus.BonusValue;
                }
            }
        }

        // Add bonuses from equipped armor
        if (player.EquippedArmor != null)
        {
            foreach (var equipBonus in player.EquippedArmor.Bonuses)
            {
                if (equipBonus.AttributeName.Equals(attributeName, StringComparison.OrdinalIgnoreCase))
                {
                    bonus += equipBonus.BonusValue;
                }
            }
        }

        return baseValue + bonus;
    }

    /// <summary>
    /// Get total defense bonus from equipped armor
    /// </summary>
    public int GetTotalDefenseBonus(PlayerCharacter player)
    {
        int defenseBonus = 0;

        if (player.EquippedArmor != null)
        {
            defenseBonus += player.EquippedArmor.DefenseBonus;
        }

        return defenseBonus;
    }

    /// <summary>
    /// Get total accuracy bonus from equipped weapon
    /// </summary>
    public int GetTotalAccuracyBonus(PlayerCharacter player)
    {
        int accuracyBonus = 0;

        if (player.EquippedWeapon != null)
        {
            accuracyBonus += player.EquippedWeapon.AccuracyBonus;
        }

        return accuracyBonus;
    }

    /// <summary>
    /// Get weapon damage info (dice and bonus) for combat
    /// </summary>
    public (int damageDice, int damageBonus, int staminaCost) GetWeaponDamage(PlayerCharacter player)
    {
        if (player.EquippedWeapon != null)
        {
            return (
                player.EquippedWeapon.DamageDice,
                player.EquippedWeapon.DamageBonus,
                player.EquippedWeapon.StaminaCost
            );
        }

        // Fallback to unarmed (weak)
        return (1, -2, 5); // 1d6-2, 5 stamina (equivalent to d4)
    }

    /// <summary>
    /// Compare two equipment items for upgrade/downgrade analysis
    /// </summary>
    public EquipmentComparison CompareEquipment(Equipment? current, Equipment proposed)
    {
        if (current == null)
        {
            return new EquipmentComparison
            {
                Current = null,
                Proposed = proposed,
                IsUpgrade = true,
                Differences = new List<string> { "Currently unequipped" }
            };
        }

        var comparison = new EquipmentComparison
        {
            Current = current,
            Proposed = proposed,
            Differences = new List<string>()
        };

        if (current.Type == EquipmentType.Weapon && proposed.Type == EquipmentType.Weapon)
        {
            CompareWeapons(current, proposed, comparison);
        }
        else if (current.Type == EquipmentType.Armor && proposed.Type == EquipmentType.Armor)
        {
            CompareArmor(current, proposed, comparison);
        }

        // Determine if it's an upgrade based on quality tier as tiebreaker
        if (comparison.Differences.Count == 0)
        {
            comparison.IsUpgrade = proposed.Quality >= current.Quality;
        }

        return comparison;
    }

    private void CompareWeapons(Equipment current, Equipment proposed, EquipmentComparison comparison)
    {
        int upgradeScore = 0;

        // Calculate dice and bonus differences
        comparison.DamageDiceDiff = proposed.DamageDice - current.DamageDice;
        comparison.DamageBonusDiff = proposed.DamageBonus - current.DamageBonus;

        // Compare damage
        int currentAvgDamage = current.DamageDice * 3 + current.DamageBonus; // Avg of d6 is 3.5, we use 3 for simplicity
        int proposedAvgDamage = proposed.DamageDice * 3 + proposed.DamageBonus;
        if (proposedAvgDamage > currentAvgDamage)
        {
            comparison.Differences.Add($"Damage: +{proposedAvgDamage - currentAvgDamage} avg");
            upgradeScore++;
        }
        else if (proposedAvgDamage < currentAvgDamage)
        {
            comparison.Differences.Add($"Damage: {proposedAvgDamage - currentAvgDamage} avg");
            upgradeScore--;
        }

        // Compare stamina cost
        if (proposed.StaminaCost < current.StaminaCost)
        {
            comparison.Differences.Add($"Stamina: -{current.StaminaCost - proposed.StaminaCost}");
            upgradeScore++;
        }
        else if (proposed.StaminaCost > current.StaminaCost)
        {
            comparison.Differences.Add($"Stamina: +{proposed.StaminaCost - current.StaminaCost}");
            upgradeScore--;
        }

        // Compare accuracy
        if (proposed.AccuracyBonus > current.AccuracyBonus)
        {
            comparison.Differences.Add($"Accuracy: +{proposed.AccuracyBonus - current.AccuracyBonus}");
            upgradeScore++;
        }
        else if (proposed.AccuracyBonus < current.AccuracyBonus)
        {
            comparison.Differences.Add($"Accuracy: {proposed.AccuracyBonus - current.AccuracyBonus}");
            upgradeScore--;
        }

        // Compare bonuses
        CompareEquipmentBonuses(current, proposed, comparison, ref upgradeScore);

        comparison.IsUpgrade = upgradeScore > 0;
    }

    private void CompareArmor(Equipment current, Equipment proposed, EquipmentComparison comparison)
    {
        int upgradeScore = 0;

        // Calculate bonus differences
        comparison.HPBonusDiff = proposed.HPBonus - current.HPBonus;
        comparison.DefenseBonusDiff = proposed.DefenseBonus - current.DefenseBonus;

        // Compare HP bonus
        if (proposed.HPBonus > current.HPBonus)
        {
            comparison.Differences.Add($"HP: +{proposed.HPBonus - current.HPBonus}");
            upgradeScore++;
        }
        else if (proposed.HPBonus < current.HPBonus)
        {
            comparison.Differences.Add($"HP: {proposed.HPBonus - current.HPBonus}");
            upgradeScore--;
        }

        // Compare defense bonus
        if (proposed.DefenseBonus > current.DefenseBonus)
        {
            comparison.Differences.Add($"Defense: +{proposed.DefenseBonus - current.DefenseBonus}");
            upgradeScore++;
        }
        else if (proposed.DefenseBonus < current.DefenseBonus)
        {
            comparison.Differences.Add($"Defense: {proposed.DefenseBonus - current.DefenseBonus}");
            upgradeScore--;
        }

        // Compare bonuses
        CompareEquipmentBonuses(current, proposed, comparison, ref upgradeScore);

        comparison.IsUpgrade = upgradeScore > 0;
    }

    private void CompareEquipmentBonuses(Equipment current, Equipment proposed, EquipmentComparison comparison, ref int upgradeScore)
    {
        // Check for new bonuses in proposed
        foreach (var bonus in proposed.Bonuses)
        {
            var existingBonus = current.Bonuses.FirstOrDefault(b =>
                b.AttributeName.Equals(bonus.AttributeName, StringComparison.OrdinalIgnoreCase));

            if (existingBonus == null)
            {
                comparison.Differences.Add($"NEW: {bonus.Description}");
                upgradeScore++;
            }
            else if (bonus.BonusValue > existingBonus.BonusValue)
            {
                comparison.Differences.Add($"{bonus.AttributeName}: +{bonus.BonusValue - existingBonus.BonusValue}");
                upgradeScore++;
            }
            else if (bonus.BonusValue < existingBonus.BonusValue)
            {
                comparison.Differences.Add($"{bonus.AttributeName}: {bonus.BonusValue - existingBonus.BonusValue}");
                upgradeScore--;
            }
        }

        // Check for lost bonuses
        foreach (var bonus in current.Bonuses)
        {
            var proposedBonus = proposed.Bonuses.FirstOrDefault(b =>
                b.AttributeName.Equals(bonus.AttributeName, StringComparison.OrdinalIgnoreCase));

            if (proposedBonus == null)
            {
                comparison.Differences.Add($"LOST: {bonus.Description}");
                upgradeScore--;
            }
        }

        // Check for special effects
        if (!string.IsNullOrEmpty(proposed.SpecialEffect) && string.IsNullOrEmpty(current.SpecialEffect))
        {
            comparison.Differences.Add($"NEW: {proposed.SpecialEffect}");
            upgradeScore++;
        }
        else if (!string.IsNullOrEmpty(current.SpecialEffect) && string.IsNullOrEmpty(proposed.SpecialEffect))
        {
            comparison.Differences.Add($"LOST: {current.SpecialEffect}");
            upgradeScore--;
        }
    }
}

/// <summary>
/// Result of comparing two equipment items
/// </summary>
public class EquipmentComparison
{
    public Equipment? Current { get; set; }
    public Equipment Proposed { get; set; } = null!;
    public bool IsUpgrade { get; set; }
    public List<string> Differences { get; set; } = new();

    // Weapon comparison properties
    public int DamageDiceDiff { get; set; }
    public int DamageBonusDiff { get; set; }

    // Armor comparison properties
    public int HPBonusDiff { get; set; }
    public int DefenseBonusDiff { get; set; }
}
