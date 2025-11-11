using RuneAndRust.Core;

namespace RuneAndRust.Engine;

/// <summary>
/// Service for generating and managing loot drops
/// </summary>
public class LootService
{
    private readonly Random _random;

    public LootService()
    {
        _random = new Random();
    }

    /// <summary>
    /// Generate loot from a defeated enemy
    /// </summary>
    public Equipment? GenerateLoot(Enemy enemy, PlayerCharacter? player = null)
    {
        return enemy.Type switch
        {
            EnemyType.CorruptedServitor => GenerateServitorLoot(player),
            EnemyType.BlightDrone => GenerateDroneLoot(player),
            EnemyType.RuinWarden => GenerateBossLoot(player),
            _ => null
        };
    }

    /// <summary>
    /// Generate loot from Corrupted Servitor (Tier 0 - Trash Mob)
    /// </summary>
    private Equipment? GenerateServitorLoot(PlayerCharacter? player)
    {
        // 60% chance: Jury-Rigged
        // 30% chance: Scavenged
        // 10% chance: Nothing
        int roll = _random.Next(100);

        QualityTier quality;
        if (roll < 10) return null; // No loot
        else if (roll < 70) quality = QualityTier.JuryRigged;
        else quality = QualityTier.Scavenged;

        return GenerateRandomItem(quality, player);
    }

    /// <summary>
    /// Generate loot from Blight-Drone (Tier 1 - Standard Enemy)
    /// </summary>
    private Equipment? GenerateDroneLoot(PlayerCharacter? player)
    {
        // 40% chance: Scavenged
        // 40% chance: Clan-Forged
        // 20% chance: Optimized
        int roll = _random.Next(100);

        QualityTier quality;
        if (roll < 40) quality = QualityTier.Scavenged;
        else if (roll < 80) quality = QualityTier.ClanForged;
        else quality = QualityTier.Optimized;

        return GenerateRandomItem(quality, player);
    }

    /// <summary>
    /// Generate loot from Ruin-Warden (Boss - Tier 2)
    /// </summary>
    private Equipment? GenerateBossLoot(PlayerCharacter? player)
    {
        // 30% chance: Optimized
        // 70% chance: Myth-Forged
        int roll = _random.Next(100);

        QualityTier quality;
        if (roll < 30) quality = QualityTier.Optimized;
        else quality = QualityTier.MythForged;

        // Boss always drops appropriate for player class
        return GenerateClassAppropriateItem(quality, player);
    }

    /// <summary>
    /// Generate a random item of a specific quality
    /// 50% chance weapon (appropriate for player class if possible)
    /// 50% chance armor
    /// </summary>
    private Equipment? GenerateRandomItem(QualityTier quality, PlayerCharacter? player)
    {
        // 50/50 weapon or armor
        bool isWeapon = _random.Next(2) == 0;

        if (isWeapon && player != null)
        {
            // 60% chance to drop weapon appropriate for player's class
            if (_random.Next(100) < 60)
            {
                var weapon = EquipmentDatabase.GetRandomWeaponForClass(player.Class, quality);
                if (weapon != null) return weapon;
            }
        }

        // Fallback: random item of this quality
        var allItems = EquipmentDatabase.GetAllEquipment()
            .Where(e => e.Quality == quality)
            .ToList();

        if (allItems.Count == 0) return null;

        // Filter to weapon or armor based on roll
        var filtered = allItems.Where(e =>
            isWeapon ? e.Type == EquipmentType.Weapon : e.Type == EquipmentType.Armor
        ).ToList();

        if (filtered.Count == 0) return allItems[_random.Next(allItems.Count)];

        return filtered[_random.Next(filtered.Count)];
    }

    /// <summary>
    /// Generate an item appropriate for the player's class
    /// Boss loot - always class-appropriate
    /// </summary>
    private Equipment? GenerateClassAppropriateItem(QualityTier quality, PlayerCharacter? player)
    {
        if (player == null)
        {
            return GenerateRandomItem(quality, null);
        }

        // 50/50 weapon or armor
        bool isWeapon = _random.Next(2) == 0;

        if (isWeapon)
        {
            var weapon = EquipmentDatabase.GetRandomWeaponForClass(player.Class, quality);
            if (weapon != null) return weapon;
        }
        else
        {
            var armor = EquipmentDatabase.GetRandomArmor(quality);
            if (armor != null) return armor;
        }

        // Fallback
        return GenerateRandomItem(quality, player);
    }

    /// <summary>
    /// Drop loot to room ground
    /// </summary>
    public void DropLootToRoom(Equipment? loot, Room room, List<string> combatLog)
    {
        if (loot == null) return;

        room.ItemsOnGround.Add(loot);
        combatLog.Add($"[yellow]💎 {loot.GetDisplayName()} dropped![/]");
    }

    /// <summary>
    /// Place starting loot in a room
    /// Used for initial room setup
    /// </summary>
    public void PlaceStartingLoot(Room room, Equipment item)
    {
        room.ItemsOnGround.Add(item);
    }

    /// <summary>
    /// Create a specific weapon for starting rooms
    /// </summary>
    public Equipment? CreateStartingWeapon(CharacterClass characterClass)
    {
        // Give player a Scavenged-tier weapon appropriate for their class
        return characterClass switch
        {
            CharacterClass.Warrior => EquipmentDatabase.GetByName("Scavenged Axe"),
            CharacterClass.Scavenger => EquipmentDatabase.GetByName("Scavenged Spear"),
            CharacterClass.Mystic => EquipmentDatabase.GetByName("Scavenged Staff"),
            _ => null
        };
    }

    /// <summary>
    /// Create puzzle reward (Optimized-tier weapon)
    /// </summary>
    public Equipment? CreatePuzzleReward(CharacterClass characterClass)
    {
        // Puzzle always rewards Optimized-tier class-appropriate weapon
        return EquipmentDatabase.GetRandomWeaponForClass(characterClass, QualityTier.Optimized);
    }
}
