using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.44.3: Room feature interaction service for dungeon exploration.
/// Handles searching, resting, puzzles, and hazard interactions.
/// Uses Aethelgard terminology: Player = "Survivor", Dungeon = "Sector".
/// </summary>
public class RoomFeatureService : IRoomFeatureService
{
    private readonly ILogger _logger;
    private readonly IEncounterService _encounterService;
    private readonly DiceService _diceService;
    private readonly Random _random;

    // Search loot configuration
    private const float SearchFindItemChance = 0.35f;
    private const float SearchFindSecretChance = 0.25f;
    private const float SearchFindScrapChance = 0.45f;

    // Rest configuration
    private const int SanctuaryHPRecoveryPercent = 100;
    private const int FieldRestHPRecoveryPercent = 25;
    private const int SanctuaryStaminaRecoveryPercent = 100;
    private const int FieldRestStaminaRecoveryPercent = 50;
    private const int FieldRestStressCost = 5;

    public RoomFeatureService(ILogger logger, IEncounterService encounterService, DiceService diceService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _encounterService = encounterService ?? throw new ArgumentNullException(nameof(encounterService));
        _diceService = diceService ?? throw new ArgumentNullException(nameof(diceService));
        _random = new Random();
    }

    /// <inheritdoc/>
    public async Task<SearchResult> SearchRoomAsync(Room room)
    {
        _logger.Debug("Survivor searching room {RoomId}: {RoomName}", room.RoomId, room.Name);

        // Check for search encounter first (10% chance)
        var searchEncounter = _encounterService.GenerateSearchEncounter(room, 1); // Level will be adjusted by caller
        if (searchEncounter.EncounterTriggered)
        {
            _logger.Warning("Search disturbed dormant Undying in room {RoomId}", room.RoomId);
            return SearchResult.WithEncounterTriggered(searchEncounter,
                "Your searching disturbs something lurking in the shadows...");
        }

        // Generate loot
        var items = new List<Equipment>();
        var secrets = new List<string>();
        var scrap = 0;

        // Roll for items
        if (_random.NextDouble() < SearchFindItemChance)
        {
            items.AddRange(GenerateSearchLoot(room));
        }

        // Roll for scrap
        if (_random.NextDouble() < SearchFindScrapChance)
        {
            scrap = _random.Next(5, 25);
        }

        // Roll for secrets
        if (_random.NextDouble() < SearchFindSecretChance)
        {
            secrets.AddRange(GenerateSecrets(room));
        }

        // Also include items already on ground
        if (room.ItemsOnGround.Count > 0)
        {
            items.AddRange(room.ItemsOnGround);
        }

        // Generate appropriate message
        string message;
        if (items.Count > 0 || scrap > 0)
        {
            message = items.Count > 0
                ? $"Your careful search reveals {items.Count} item(s)!"
                : $"You find {scrap} Scrap hidden among the debris.";
        }
        else if (secrets.Count > 0)
        {
            message = "You discover something interesting about this place...";
        }
        else
        {
            message = "Your thorough search reveals nothing of value.";
        }

        _logger.Information("Search complete in {RoomId}: {ItemCount} items, {Scrap} scrap, {SecretCount} secrets",
            room.RoomId, items.Count, scrap, secrets.Count);

        return SearchResult.WithLoot(items, scrap, secrets, message);
    }

    /// <inheritdoc/>
    public async Task<RestResult> PerformFieldRestAsync(Core.GameState gameState)
    {
        if (gameState.Player == null || gameState.CurrentRoom == null)
        {
            throw new InvalidOperationException("Cannot rest without active player and room");
        }

        var player = gameState.Player;
        var room = gameState.CurrentRoom;

        _logger.Information("Survivor field rest initiated: HP={HP}/{MaxHP}, Stress={Stress}",
            player.HP, player.MaxHP, player.PsychicStress);

        // Sanctuary rest - full recovery, no interruption risk
        if (room.IsSanctuary)
        {
            var hpRecovery = player.MaxHP - player.HP;
            var staminaRecovery = player.MaxStamina - player.Stamina;

            _logger.Information("Sanctuary rest completed - full recovery");
            return RestResult.SanctuaryRest(hpRecovery, staminaRecovery);
        }

        // Field rest - check for interruption first
        var interruptEncounter = _encounterService.GenerateRestInterruptEncounter(room, false, player.Level);
        if (interruptEncounter.EncounterTriggered)
        {
            _logger.Warning("Field rest interrupted by Undying!");
            return RestResult.Interrupted(interruptEncounter);
        }

        // Calculate partial recovery
        var fieldHpRecovery = (int)(player.MaxHP * FieldRestHPRecoveryPercent / 100.0);
        var fieldStaminaRecovery = (int)(player.MaxStamina * FieldRestStaminaRecoveryPercent / 100.0);

        // Cap recovery to actual missing values
        fieldHpRecovery = Math.Min(fieldHpRecovery, player.MaxHP - player.HP);
        fieldStaminaRecovery = Math.Min(fieldStaminaRecovery, player.MaxStamina - player.Stamina);

        _logger.Information("Field rest completed: +{HP} HP, +{Stamina} Stamina, +{Stress} Stress",
            fieldHpRecovery, fieldStaminaRecovery, FieldRestStressCost);

        return RestResult.FieldRest(fieldHpRecovery, fieldStaminaRecovery, FieldRestStressCost);
    }

    /// <inheritdoc/>
    public async Task<(bool Success, string Message, int? DamageTaken)> AttemptPuzzleAsync(Room room, PlayerCharacter player, string attributeUsed)
    {
        if (!room.HasPuzzle)
        {
            return (false, "There is no puzzle here to solve.", null);
        }

        if (room.IsPuzzleSolved)
        {
            return (false, "This puzzle has already been solved.", null);
        }

        _logger.Debug("Attempting puzzle in {RoomId} using {Attribute}", room.RoomId, attributeUsed);

        // Get attribute value
        var attributeValue = player.GetAttributeValue(attributeUsed);
        if (attributeValue <= 0)
        {
            attributeValue = 10; // Default if attribute not found
        }

        // Roll attribute check: 2d6 + attribute vs DC
        var roll = _diceService.Roll(2, 6);
        var total = roll + attributeValue;
        var dc = room.PuzzleSuccessThreshold > 0 ? room.PuzzleSuccessThreshold + 10 : 12; // Default DC 12

        _logger.Debug("Puzzle check: 2d6={Roll} + {Attr}={Total} vs DC {DC}", roll, attributeValue, total, dc);

        if (total >= dc)
        {
            room.IsPuzzleSolved = true;
            _logger.Information("Puzzle solved in {RoomId}!", room.RoomId);
            return (true, "Success! The puzzle mechanism clicks into place, revealing its secrets.", null);
        }
        else
        {
            // Failure - take damage
            var damage = _diceService.Roll(room.PuzzleFailureDamage > 0 ? room.PuzzleFailureDamage : 1, 6);
            _logger.Warning("Puzzle failed - survivor takes {Damage} damage", damage);
            return (false, $"You fail to solve the puzzle. A trap triggers, dealing {damage} damage!", damage);
        }
    }

    /// <inheritdoc/>
    public async Task<(bool Success, string Message)> DisableHazardAsync(Room room, PlayerCharacter player)
    {
        if (!room.HasEnvironmentalHazard)
        {
            return (false, "There is no hazard here to disable.");
        }

        if (!room.IsHazardActive)
        {
            return (false, "This hazard has already been disabled.");
        }

        _logger.Debug("Attempting to disable hazard in {RoomId}: {HazardType}", room.RoomId, room.HazardType);

        // Hazard disable check: FINESSE or WITS check
        var attributeValue = Math.Max(player.GetAttributeValue("FINESSE"), player.GetAttributeValue("WITS"));
        if (attributeValue <= 0) attributeValue = 10;

        var roll = _diceService.Roll(2, 6);
        var total = roll + attributeValue;
        var dc = room.HazardCheckDC > 0 ? room.HazardCheckDC : 12;

        _logger.Debug("Hazard disable check: 2d6={Roll} + {Attr}={Total} vs DC {DC}", roll, attributeValue, total, dc);

        if (total >= dc)
        {
            room.IsHazardActive = false;
            _logger.Information("Hazard disabled in {RoomId}", room.RoomId);
            return (true, "You carefully disable the environmental hazard. The area is now safe to traverse.");
        }
        else
        {
            _logger.Warning("Failed to disable hazard in {RoomId}", room.RoomId);
            return (false, "You fail to disable the hazard. It remains active and dangerous.");
        }
    }

    /// <inheritdoc/>
    public List<Equipment> CollectGroundItems(Room room, PlayerCharacter player)
    {
        var collected = new List<Equipment>(room.ItemsOnGround);
        room.ItemsOnGround.Clear();

        _logger.Information("Collected {Count} items from ground in {RoomId}", collected.Count, room.RoomId);
        return collected;
    }

    #region Private Helper Methods

    private List<Equipment> GenerateSearchLoot(Room room)
    {
        var items = new List<Equipment>();
        var numItems = _random.Next(1, 4); // 1-3 items

        for (int i = 0; i < numItems; i++)
        {
            items.Add(GenerateRandomItem(room));
        }

        return items;
    }

    private Equipment GenerateRandomItem(Room room)
    {
        var itemTypes = new[] { EquipmentType.Weapon, EquipmentType.Armor, EquipmentType.Accessory };
        var qualities = new[] { QualityTier.Scavenged, QualityTier.Scavenged, QualityTier.ClanForged }; // Weighted toward basic items

        var type = itemTypes[_random.Next(itemTypes.Length)];
        var quality = qualities[_random.Next(qualities.Length)];

        return new Equipment
        {
            Name = GenerateItemName(type, room.PrimaryBiome),
            Type = type,
            Quality = quality,
            Description = GetItemDescription(type, quality),
            DamageDice = type == EquipmentType.Weapon ? _random.Next(1, 3) : 0,
            DamageDieSize = 6,
            DefenseBonus = type == EquipmentType.Armor ? _random.Next(1, 5) : 0
        };
    }

    private string GenerateItemName(EquipmentType type, string? biome)
    {
        var prefixes = new[] { "Corroded", "Salvaged", "Ancient", "Damaged", "Modified", "Reinforced" };
        var prefix = prefixes[_random.Next(prefixes.Length)];

        var suffix = type switch
        {
            EquipmentType.Weapon => new[] { "Blade", "Pipe", "Wrench", "Shiv", "Maul" }[_random.Next(5)],
            EquipmentType.Armor => new[] { "Plate", "Vest", "Guard", "Shell", "Carapace" }[_random.Next(5)],
            EquipmentType.Accessory => new[] { "Ring", "Charm", "Module", "Implant", "Core" }[_random.Next(5)],
            _ => "Item"
        };

        return $"{prefix} {suffix}";
    }

    private string GetItemDescription(EquipmentType type, QualityTier quality)
    {
        var qualityDesc = quality switch
        {
            QualityTier.JuryRigged => "Scrap held together with hope and wire. It works... mostly.",
            QualityTier.Scavenged => "A piece of salvaged equipment in acceptable condition.",
            QualityTier.ClanForged => "A well-crafted piece, forged by survivor communities.",
            QualityTier.Optimized => "Pre-Glitch tech, carefully maintained and still functional.",
            QualityTier.MythForged => "A legendary artifact of immense power, touched by ancient craft.",
            _ => "A piece of equipment."
        };

        return qualityDesc;
    }

    private List<string> GenerateSecrets(Room room)
    {
        var allSecrets = new[]
        {
            "Faded runes on the wall seem to depict an ancient ritual.",
            "You find scratch marks suggesting something large passed through here.",
            "A hidden compartment contains old documents, mostly illegible.",
            "The temperature here is noticeably different than nearby areas.",
            "You notice strange stains on the floor that shimmer faintly.",
            "An old terminal flickers with corrupted data logs.",
            "The walls bear evidence of a past battle.",
            "You discover a hidden maintenance passage (now collapsed).",
            "Echoes of old machinery can still be heard in the walls.",
            "Strange symbols are etched into the floor in a precise pattern."
        };

        var numSecrets = _random.Next(1, 3);
        return allSecrets.OrderBy(_ => _random.Next()).Take(numSecrets).ToList();
    }

    #endregion
}
