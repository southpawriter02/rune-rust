using RuneAndRust.Core;

namespace RuneAndRust.Engine;

public class GameWorld
{
    public Dictionary<string, Room> Rooms { get; private set; }
    public string StartRoomName { get; private set; }

    public GameWorld()
    {
        Rooms = new Dictionary<string, Room>();
        StartRoomName = "Entrance";
        InitializeRooms();
    }

    private void InitializeRooms()
    {
        // ====== ENTRANCE ZONE (Linear Tutorial) ======

        // Room 1: Entrance (Safe Zone)
        var entrance = new Room
        {
            Id = 1,
            Name = "Entrance",
            Description =
                "You stand at the shattered threshold of a pre-Glitch facility. Twisted metal frames " +
                "what was once a grand entrance. The air hums with residual energy. Ahead, a corridor " +
                "leads deeper into darkness.",
            Exits = new Dictionary<string, string>
            {
                { "north", "Corridor" }
            },
            IsStartRoom = true,
            HasBeenCleared = true, // Safe zone, no combat
            IsSanctuary = true // [v0.5] Safe location for Sanctuary Rest
        };

        // Room 2: Corridor (First Combat)
        var corridor = new Room
        {
            Id = 2,
            Name = "Corridor",
            Description =
                "A long corridor stretches before you. Flickering lights cast erratic shadows. " +
                "You hear the grinding of metal on metal—something moves in the darkness ahead.",
            Exits = new Dictionary<string, string>
            {
                { "south", "Entrance" },
                { "north", "Salvage Bay" }
            },
            Enemies = new List<Enemy>
            {
                EnemyFactory.CreateEnemy(EnemyType.CorruptedServitor),
                EnemyFactory.CreateEnemy(EnemyType.CorruptedServitor)
            }
        };

        // Room 3: Salvage Bay (NEW)
        var salvageBay = new Room
        {
            Id = 3,
            Name = "Salvage Bay",
            Description =
                "Rows of empty storage racks stretch into darkness. Something scuttles in the shadows. " +
                "As you advance, a corrupted servitor emerges from behind a collapsed shelf, accompanied " +
                "by a swift, hound-like scavenger drone.",
            Exits = new Dictionary<string, string>
            {
                { "south", "Corridor" },
                { "north", "Operations Center" }
            },
            Enemies = new List<Enemy>
            {
                EnemyFactory.CreateEnemy(EnemyType.CorruptedServitor),
                EnemyFactory.CreateEnemy(EnemyType.ScrapHound) // NEW enemy type
            }
        };

        // ====== CENTRAL HUB (Safe Zone + Branching Choice) ======

        // Room 4: Operations Center (NEW - Replaces old Room 3)
        var operationsCenter = new Room
        {
            Id = 4,
            Name = "Operations Center",
            Description =
                "A vast command center, dormant terminals line the walls. Two corridors branch off—one " +
                "leads east toward the arsenal, the other west toward the research wing. The room is quiet, " +
                "a rare moment of safety in the corrupted facility.",
            Exits = new Dictionary<string, string>
            {
                { "south", "Salvage Bay" },
                { "east", "Arsenal" },
                { "west", "Research Archives" }
            },
            HasBeenCleared = true, // Safe zone - rest point before choosing path
            IsSanctuary = true // [v0.5] Safe location for Sanctuary Rest
        };

        // ====== EAST WING (Combat Path) ======

        // Room 5: Arsenal (NEW)
        var arsenal = new Room
        {
            Id = 5,
            Name = "Arsenal",
            Description =
                "The armory. Weapon racks stand empty, their contents long since scavenged or corrupted. " +
                "Three blight-drones hover among the ruins, scanning for intruders.",
            Exits = new Dictionary<string, string>
            {
                { "west", "Operations Center" },
                { "east", "Training Chamber" }
            },
            Enemies = new List<Enemy>
            {
                EnemyFactory.CreateEnemy(EnemyType.BlightDrone),
                EnemyFactory.CreateEnemy(EnemyType.BlightDrone),
                EnemyFactory.CreateEnemy(EnemyType.BlightDrone)
            }
        };

        // Room 6: Training Chamber (NEW)
        var trainingChamber = new Room
        {
            Id = 6,
            Name = "Training Chamber",
            Description =
                "A cavernous training hall. Ancient combat drones still patrol their designated zones, " +
                "locked in an endless drill. A massive War-Frame—a military combat construct—blocks " +
                "the eastern exit.",
            Exits = new Dictionary<string, string>
            {
                { "west", "Arsenal" },
                { "east", "Ammunition Forge" }
            },
            Enemies = new List<Enemy>
            {
                EnemyFactory.CreateEnemy(EnemyType.WarFrame) // NEW enemy type (mini-boss)
            }
        };

        // Room 7: Ammunition Forge (NEW - Environmental Hazard)
        var ammunitionForge = new Room
        {
            Id = 7,
            Name = "Ammunition Forge",
            Description =
                "An ammunition manufacturing facility. Unstable reactors pulse with dangerous energy, " +
                "filling the room with crackling radiation. Two blight-drones patrol the area. You'll need " +
                "to disable the reactors or fight through the hazardous conditions.",
            Exits = new Dictionary<string, string>
            {
                { "west", "Training Chamber" },
                { "north", "Vault Antechamber" }
            },
            HasPuzzle = true,
            PuzzleDescription =
                "Unstable reactors are overloading. You can attempt to disable them with a WITS check, " +
                "or fight while taking damage from the radiation.",
            PuzzleSuccessThreshold = 3, // DC 3 WITS check
            PuzzleFailureDamage = 0, // Doesn't damage on failed check, but hazard activates in combat
            Enemies = new List<Enemy>
            {
                EnemyFactory.CreateEnemy(EnemyType.BlightDrone),
                EnemyFactory.CreateEnemy(EnemyType.BlightDrone)
            },
            // Environmental Hazard
            HasEnvironmentalHazard = true,
            IsHazardActive = true,
            HazardDamagePerTurn = 6, // 1d6 damage per turn
            HazardDescription = "Unstable reactors crackle with dangerous energy, filling the room with radiation.",
            // Trauma Economy (v0.5)
            PsychicResonance = PsychicResonanceLevel.Light // +5 Stress per turn
        };

        // ====== WEST WING (Exploration Path) ======

        // Room 8: Research Archives (NEW - No Combat, Puzzle)
        var researchArchives = new Room
        {
            Id = 8,
            Name = "Research Archives",
            Description =
                "Rows of data terminals, most dead. One still flickers with a faint signal. " +
                "The air is thick with dust. No enemies here—just the ghosts of knowledge long forgotten.",
            Exits = new Dictionary<string, string>
            {
                { "east", "Operations Center" },
                { "west", "Specimen Containment" }
            },
            HasPuzzle = true,
            PuzzleDescription =
                "A secure terminal still has power. With sufficient technical knowledge, you might " +
                "be able to access valuable data—or equipment schematics.",
            PuzzleSuccessThreshold = 4, // DC 4 WITS check
            PuzzleFailureDamage = 0 // No damage, just can't access loot
        };

        // Room 9: Specimen Containment (NEW)
        var specimenContainment = new Room
        {
            Id = 9,
            Name = "Specimen Containment",
            Description =
                "Shattered glass and twisted metal. Whatever was held here escaped long ago—or did it? " +
                "Two malformed test subjects lurch from the shadows, remnants of failed experiments.",
            Exits = new Dictionary<string, string>
            {
                { "east", "Research Archives" },
                { "west", "Observation Deck" }
            },
            Enemies = new List<Enemy>
            {
                EnemyFactory.CreateEnemy(EnemyType.TestSubject), // NEW enemy type
                EnemyFactory.CreateEnemy(EnemyType.TestSubject)
            }
        };

        // Room 10: Observation Deck (NEW - Talk or Fight)
        var observationDeck = new Room
        {
            Id = 10,
            Name = "Observation Deck",
            Description =
                "An observation chamber overlooking the containment labs. A lone figure stands at the " +
                "viewing window—a Forlorn Scholar, a data-ghost of a researcher. It turns toward you, " +
                "its form flickering. Does it recognize you as friend or foe?",
            Exits = new Dictionary<string, string>
            {
                { "east", "Specimen Containment" },
                { "north", "Vault Antechamber" }
            },
            Enemies = new List<Enemy>
            {
                EnemyFactory.CreateEnemy(EnemyType.ForlornScholar) // NEW enemy type (can be talked to)
            },
            HasTalkableNPC = true, // [v0.4] Player can negotiate instead of fighting
            PsychicResonance = PsychicResonanceLevel.Moderate // [v0.5] +10 Stress per turn
        };

        // ====== DEEP VAULT (Convergence - High Difficulty) ======

        // Room 11: Vault Antechamber (NEW - Paths Converge)
        var vaultAntechamber = new Room
        {
            Id = 11,
            Name = "Vault Antechamber",
            Description =
                "The paths converge before a massive sealed vault door. The air hums with residual energy. " +
                "A mixed group of corrupted machines guards the entrance—blight-drones and a scrap-hound " +
                "working in concert.",
            Exits = new Dictionary<string, string>
            {
                { "south", "Ammunition Forge" }, // From East Wing
                // Note: "south" to Observation Deck would conflict, handled in code
                { "north", "Vault Corridor" }
            },
            Enemies = new List<Enemy>
            {
                EnemyFactory.CreateEnemy(EnemyType.BlightDrone),
                EnemyFactory.CreateEnemy(EnemyType.BlightDrone),
                EnemyFactory.CreateEnemy(EnemyType.BlightDrone),
                EnemyFactory.CreateEnemy(EnemyType.ScrapHound)
            }
        };

        // Room 12: Vault Corridor (NEW - Boss Choice, replaces old Room 4)
        var vaultCorridor = new Room
        {
            Id = 12,
            Name = "Vault Corridor",
            Description =
                "The vault corridor splits. To the west, a door marked with weapon sigils pulses with " +
                "martial energy. To the east, a door crackles with unstable aetheric power. Both lead " +
                "to certain danger. Choose your battle.",
            Exits = new Dictionary<string, string>
            {
                { "south", "Vault Antechamber" },
                { "west", "Arsenal Vault" },
                { "east", "Energy Core" }
            },
            HasBeenCleared = true, // No combat here, just boss choice
            PsychicResonance = PsychicResonanceLevel.Heavy // [v0.5] +15 Stress per turn
        };

        // Room 13: Supply Cache (NEW - Secret Room)
        var supplyCache = new Room
        {
            Id = 13,
            Name = "Supply Cache",
            Description =
                "A hidden supply cache, overlooked by scavengers. Pristine equipment rests in secure " +
                "containers. This room offers a moment of respite before the final confrontation.",
            Exits = new Dictionary<string, string>
            {
                { "north", "Vault Corridor" }
            },
            HasBeenCleared = true // Safe zone, no combat
        };

        // ====== BOSS SANCTUMS (Player Choice) ======

        // Room 14: Arsenal Vault (Boss A - Existing Ruin-Warden, moved here)
        var arsenalVault = new Room
        {
            Id = 14,
            Name = "Arsenal Vault",
            Description =
                "You step into a cathedral of corrupted machinery. At the center stands a towering " +
                "construct—a Ruin-Warden, its frame warped by Blight, its weapon arms still functional. " +
                "It turns toward you. There is no escape.",
            Exits = new Dictionary<string, string>
            {
                { "east", "Vault Corridor" }
            },
            IsBossRoom = true,
            Enemies = new List<Enemy>
            {
                EnemyFactory.CreateEnemy(EnemyType.RuinWarden)
            }
        };

        // Room 15: Energy Core (Boss B - NEW Aetheric Aberration)
        var energyCore = new Room
        {
            Id = 15,
            Name = "Energy Core",
            Description =
                "The room crackles with unstable aetheric energy. At its center, a writhing mass of " +
                "corrupted power—an Aetheric Aberration, a being of pure chaotic energy. Reality bends " +
                "around it. This will be a battle of wills.",
            Exits = new Dictionary<string, string>
            {
                { "west", "Vault Corridor" }
            },
            IsBossRoom = true,
            PsychicResonance = PsychicResonanceLevel.Heavy, // [v0.5] +15 Stress per turn
            Enemies = new List<Enemy>
            {
                EnemyFactory.CreateEnemy(EnemyType.AethericAberration) // NEW boss enemy type
            }
        };

        // Add all rooms to dictionary
        Rooms.Add(entrance.Name, entrance);
        Rooms.Add(corridor.Name, corridor);
        Rooms.Add(salvageBay.Name, salvageBay);
        Rooms.Add(operationsCenter.Name, operationsCenter);
        Rooms.Add(arsenal.Name, arsenal);
        Rooms.Add(trainingChamber.Name, trainingChamber);
        Rooms.Add(ammunitionForge.Name, ammunitionForge);
        Rooms.Add(researchArchives.Name, researchArchives);
        Rooms.Add(specimenContainment.Name, specimenContainment);
        Rooms.Add(observationDeck.Name, observationDeck);
        Rooms.Add(vaultAntechamber.Name, vaultAntechamber);
        Rooms.Add(vaultCorridor.Name, vaultCorridor);
        Rooms.Add(supplyCache.Name, supplyCache);
        Rooms.Add(arsenalVault.Name, arsenalVault);
        Rooms.Add(energyCore.Name, energyCore);
    }

    public Room GetRoom(string roomName)
    {
        if (Rooms.TryGetValue(roomName, out var room))
        {
            return room;
        }
        throw new ArgumentException($"Room '{roomName}' does not exist.");
    }

    public Room GetStartRoom()
    {
        return GetRoom(StartRoomName);
    }

    /// <summary>
    /// [v0.4] Unlock secret room (Room 13: Supply Cache)
    /// Called when player succeeds WITS check in Vault Corridor
    /// </summary>
    public void UnlockSecretRoom()
    {
        var vaultCorridor = GetRoom("Vault Corridor");
        if (!vaultCorridor.Exits.ContainsKey("south"))
        {
            vaultCorridor.Exits.Add("south", "Supply Cache");
        }
    }

    /// <summary>
    /// Add starting loot to rooms (v0.3 Equipment System)
    /// Should be called after character creation
    /// </summary>
    public void AddStartingLoot(PlayerCharacter player)
    {
        var lootService = new LootService();

        // Room 1 (Entrance): Scavenged weapon upgrade for player's class
        var entrance = GetRoom("Entrance");
        var startingWeapon = lootService.CreateStartingWeapon(player.Class);
        if (startingWeapon != null)
        {
            lootService.PlaceStartingLoot(entrance, startingWeapon);
        }

        // [v0.4] Operations Center (Room 4): Clan-Forged equipment cache (2 items)
        AddOperationsCenterLoot(player);
    }

    /// <summary>
    /// [v0.4] Add Clan-Forged equipment cache to Operations Center
    /// Called during world initialization after character creation
    /// </summary>
    public void AddOperationsCenterLoot(PlayerCharacter player)
    {
        var lootService = new LootService();
        var operationsCenter = GetRoom("Operations Center");

        // Generate 2x Clan-Forged items appropriate for player's class
        // One weapon, one armor to provide balanced rewards
        var clanWeapon = EquipmentDatabase.GetRandomWeaponForClass(player.Class, QualityTier.ClanForged);
        var clanArmor = EquipmentDatabase.GetRandomArmor(QualityTier.ClanForged);

        if (clanWeapon != null)
        {
            lootService.PlaceStartingLoot(operationsCenter, clanWeapon);
        }

        if (clanArmor != null)
        {
            lootService.PlaceStartingLoot(operationsCenter, clanArmor);
        }
    }

    /// <summary>
    /// [v0.4] Add Myth-Forged loot to Secret Room
    /// Called when secret room is first discovered
    /// </summary>
    public void AddSecretRoomLoot(PlayerCharacter player)
    {
        var lootService = new LootService();
        var supplyCache = GetRoom("Supply Cache");

        // Check if loot already placed (don't add multiple times)
        if (supplyCache.ItemsOnGround.Count > 0)
        {
            return;
        }

        // Generate 3x Myth-Forged items for player to choose from
        // This gives player agency and increases replay value
        var mythWeapon1 = EquipmentDatabase.GetRandomWeaponForClass(player.Class, QualityTier.MythForged);
        var mythWeapon2 = EquipmentDatabase.GetRandomWeaponForClass(player.Class, QualityTier.MythForged);
        var mythArmor = EquipmentDatabase.GetRandomArmor(QualityTier.MythForged);

        if (mythWeapon1 != null)
        {
            lootService.PlaceStartingLoot(supplyCache, mythWeapon1);
        }

        if (mythWeapon2 != null)
        {
            lootService.PlaceStartingLoot(supplyCache, mythWeapon2);
        }

        if (mythArmor != null)
        {
            lootService.PlaceStartingLoot(supplyCache, mythArmor);
        }
    }

    /// <summary>
    /// Add puzzle reward loot (v0.3 Equipment System, expanded in v0.4)
    /// Called when puzzle is successfully solved
    /// </summary>
    public void AddPuzzleReward(string roomName, PlayerCharacter player)
    {
        var lootService = new LootService();
        var room = GetRoom(roomName);

        // Different rewards for different puzzle rooms
        Equipment? puzzleReward = null;

        switch (roomName)
        {
            case "Research Archives": // Room 8 - West Wing
                puzzleReward = lootService.CreatePuzzleReward(player.Class);
                break;

            case "Ammunition Forge": // Room 7 - East Wing (rewards Optimized armor)
                puzzleReward = lootService.CreatePuzzleReward(player.Class);
                break;

            default:
                puzzleReward = lootService.CreatePuzzleReward(player.Class);
                break;
        }

        if (puzzleReward != null)
        {
            lootService.PlaceStartingLoot(room, puzzleReward);
        }
    }
}
