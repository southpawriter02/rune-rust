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
                // "south" exit to Maintenance Shaft added when boss is defeated
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
                // "south" exit to Maintenance Shaft added when boss is defeated
            },
            IsBossRoom = true,
            PsychicResonance = PsychicResonanceLevel.Heavy, // [v0.5] +15 Stress per turn
            Enemies = new List<Enemy>
            {
                EnemyFactory.CreateEnemy(EnemyType.AethericAberration) // NEW boss enemy type
            }
        };

        // ====== [v0.6] THE LOWER DEPTHS (NEW - 10 Rooms) ======
        // Unlocked after defeating boss in Room 14 or Room 15

        // ====== DESCENT (Transition Zone) ======

        // Room 16: Maintenance Shaft (NEW)
        var maintenanceShaft = new Room
        {
            Id = 16,
            Name = "Maintenance Shaft",
            Description =
                "A narrow maintenance shaft descends into darkness. Corroded grates creak underfoot, and the air " +
                "grows thick with moisture. The descent is claustrophobic and oppressive. Whatever lies below " +
                "was sealed for a reason.",
            Exits = new Dictionary<string, string>
            {
                // "north" exit dynamically added from whichever boss room player came from
                { "south", "Collapsed Junction" }
            },
            Enemies = new List<Enemy>
            {
                EnemyFactory.CreateEnemy(EnemyType.ScrapHound),
                EnemyFactory.CreateEnemy(EnemyType.ScrapHound),
                EnemyFactory.CreateEnemy(EnemyType.MaintenanceConstruct) // NEW enemy type
            },
            HasEnvironmentalHazard = true,
            IsHazardActive = true,
            HazardDamagePerTurn = 0, // Special: Movement restriction, not damage
            HazardDescription = "[Unstable Flooring] - Weak floor panels restrict movement.",
            PsychicResonance = PsychicResonanceLevel.Light // +5 Stress per turn
        };

        // Room 17: Collapsed Junction (NEW)
        var collapsedJunction = new Room
        {
            Id = 17,
            Name = "Collapsed Junction",
            Description =
                "The ceiling has caved in, leaving mountains of twisted metal and broken ferrocrete. Safe paths " +
                "through the debris are narrow and treacherous. Something moves in the shadows between the rubble piles.",
            Exits = new Dictionary<string, string>
            {
                { "north", "Maintenance Shaft" },
                { "south", "Airlock Transition" }
            },
            Enemies = new List<Enemy>
            {
                EnemyFactory.CreateEnemy(EnemyType.TestSubject),
                EnemyFactory.CreateEnemy(EnemyType.TestSubject),
                EnemyFactory.CreateEnemy(EnemyType.TestSubject),
                EnemyFactory.CreateEnemy(EnemyType.ScrapHound)
            },
            HasPuzzle = true,
            PuzzleDescription =
                "Navigate through the rubble without triggering a collapse. A FINESSE check can find the safe path.",
            PuzzleSuccessThreshold = 4, // DC 4 FINESSE check
            PuzzleFailureDamage = 6, // 1d6 falling debris damage on failure
            HasEnvironmentalHazard = true,
            IsHazardActive = true,
            HazardDamagePerTurn = 0, // Difficult terrain penalty, handled separately
            HazardDescription = "[Collapsed Ceiling] - Difficult terrain reduces movement effectiveness."
        };

        // Room 18: Airlock Transition (NEW - Puzzle Gate)
        var airlockTransition = new Room
        {
            Id = 18,
            Name = "Airlock Transition",
            Description =
                "A reinforced airlock sealed from the inside. Warning sigils in Old Dvergr script read: " +
                "'QUARANTINE ZONE - BIOLOGICAL HAZARD.' Beyond, you hear the drip of water and the hum of " +
                "failing pumps. Whatever lies ahead was deliberately contained.",
            Exits = new Dictionary<string, string>
            {
                { "north", "Collapsed Junction" }
                // "south" exit to Flooded Reservoir locked until puzzle solved
            },
            HasPuzzle = true,
            PuzzleDescription =
                "The airlock control panel is partially functional. You can attempt to override the locks " +
                "with a WITS check, but failure will trigger defensive systems.",
            PuzzleSuccessThreshold = 6, // DC 6 WITS check
            PuzzleFailureDamage = 12, // 2d6 electrical damage on failed attempt
            PsychicResonance = PsychicResonanceLevel.Moderate // +10 Stress per turn
        };

        // ====== THE SUMP (Toxic Industrial Zone) ======

        // Room 19: Flooded Reservoir (NEW - Hazard Arena)
        var floodedReservoir = new Room
        {
            Id = 19,
            Name = "Flooded Reservoir",
            Description =
                "A vast reservoir, half-filled with iridescent toxic sludge. The surface bubbles and hisses. " +
                "Movement in the depths suggests things have adapted to the poison. The air reeks of chemical decay.",
            Exits = new Dictionary<string, string>
            {
                { "north", "Airlock Transition" },
                { "east", "Pump Control Station" },
                { "west", "Filtration Array" }
            },
            Enemies = new List<Enemy>
            {
                EnemyFactory.CreateEnemy(EnemyType.SludgeCrawler), // NEW enemy type
                EnemyFactory.CreateEnemy(EnemyType.SludgeCrawler),
                EnemyFactory.CreateEnemy(EnemyType.SludgeCrawler),
                EnemyFactory.CreateEnemy(EnemyType.SludgeCrawler)
            },
            HasEnvironmentalHazard = true,
            IsHazardActive = true,
            HazardDamagePerTurn = 4, // 1d4 toxic sludge damage per turn
            HazardDescription = "[Toxic Sludge] - Corrosive waste fills half the room, dealing damage to anyone standing in it.",
            PsychicResonance = PsychicResonanceLevel.Moderate // +10 Stress per turn
        };

        // Room 20: Pump Control Station (NEW - Tactical Cover)
        var pumpControlStation = new Room
        {
            Id = 20,
            Name = "Pump Control Station",
            Description =
                "A control room overlooking the reservoir. Ancient pump machinery still functions, though its purpose " +
                "is unclear. Massive pipes and machinery provide tactical cover, and a corrupted engineer construct " +
                "oversees repairs to damaged equipment.",
            Exits = new Dictionary<string, string>
            {
                { "west", "Flooded Reservoir" }
            },
            Enemies = new List<Enemy>
            {
                EnemyFactory.CreateEnemy(EnemyType.MaintenanceConstruct),
                EnemyFactory.CreateEnemy(EnemyType.MaintenanceConstruct),
                EnemyFactory.CreateEnemy(EnemyType.CorruptedEngineer) // NEW enemy type
            },
            HasPuzzle = true,
            PuzzleDescription =
                "The pump control systems are still operational. With sufficient technical knowledge, you could " +
                "activate the drainage pumps and clear the toxic sludge from the reservoir.",
            PuzzleSuccessThreshold = 5, // DC 5 WITS check
            PuzzleFailureDamage = 0, // No damage on failure
            HasEnvironmentalHazard = true,
            IsHazardActive = true,
            HazardDamagePerTurn = 8, // 2d8 electrical damage if touching conduits
            HazardDescription = "[Live Power Conduit] - Exposed electrical systems crackle with lethal energy."
        };

        // Room 21: Filtration Array (NEW - Gauntlet Run)
        var filtrationArray = new Room
        {
            Id = 21,
            Name = "Filtration Array",
            Description =
                "Rows of filtration tanks line the walls. Most have ruptured, filling the air with acrid fumes. " +
                "Your eyes water and your lungs burn. Whatever lurks in this toxic corridor won't give you time to rest.",
            Exits = new Dictionary<string, string>
            {
                { "east", "Flooded Reservoir" },
                { "south", "Sump Floor" }
            },
            Enemies = new List<Enemy>
            {
                EnemyFactory.CreateEnemy(EnemyType.SludgeCrawler),
                EnemyFactory.CreateEnemy(EnemyType.SludgeCrawler),
                EnemyFactory.CreateEnemy(EnemyType.SludgeCrawler),
                EnemyFactory.CreateEnemy(EnemyType.ScrapHound),
                EnemyFactory.CreateEnemy(EnemyType.ScrapHound)
            },
            HasEnvironmentalHazard = true,
            IsHazardActive = true,
            HazardDamagePerTurn = 4, // 1d4 toxic fumes damage per turn (unavoidable)
            HazardDescription = "[Toxic Fumes] - Poisonous gas fills the room, damaging all combatants each turn.",
            PsychicResonance = PsychicResonanceLevel.Light // +5 Stress per turn
        };

        // Room 22: Sump Floor (NEW - Sanctuary/Safe Zone)
        var sumpFloor = new Room
        {
            Id = 22,
            Name = "Sump Floor",
            Description =
                "A rare dry platform, elevated above the toxic mire. Emergency lighting still functions here, " +
                "casting a sickly yellow glow. This is the last safe place before the deep vaults. Beyond lies " +
                "the final threshold.",
            Exits = new Dictionary<string, string>
            {
                { "north", "Filtration Array" },
                { "south", "Deep Vault Antechamber" }
            },
            HasBeenCleared = true, // Safe zone, no combat
            IsSanctuary = true // [v0.6] Last rest before final bosses
        };

        // ====== FINAL SANCTUM (End-Game Bosses) ======

        // Room 23: Deep Vault Antechamber (NEW - Elite Arena)
        var deepVaultAntechamber = new Room
        {
            Id = 23,
            Name = "Deep Vault Antechamber",
            Description =
                "A massive sealed vault door dominates the southern wall. Before it stands a towering construct, " +
                "its chassis scarred from centuries of faithful duty. It does not recognize you as authorized personnel. " +
                "Twin sealed vaults flank the guardian—both radiate dangerous energy.",
            Exits = new Dictionary<string, string>
            {
                { "north", "Sump Floor" }
                // "west" and "east" exits to boss rooms locked until Vault Custodian defeated
            },
            Enemies = new List<Enemy>
            {
                EnemyFactory.CreateEnemy(EnemyType.VaultCustodian) // NEW enemy type (mini-boss)
            },
            IsBossRoom = true, // Prevent fleeing from mini-boss
            PsychicResonance = PsychicResonanceLevel.Heavy // +15 Stress per turn
        };

        // Room 24: Core Vault A - The Sleeper's Tomb (NEW - Psychic Boss)
        var sleepersVault = new Room
        {
            Id = 24,
            Name = "Sleeper's Tomb",
            Description =
                "A spherical chamber, its walls covered in corrupted data terminals. At the center, suspended in " +
                "a failing stasis field, floats the desiccated corpse of a Jötun-Reader. Its eyes snap open. " +
                "It has been waiting. The psychic pressure is overwhelming.",
            Exits = new Dictionary<string, string>
            {
                { "east", "Deep Vault Antechamber" }
            },
            IsBossRoom = true,
            PsychicResonance = PsychicResonanceLevel.Heavy, // +15 Stress per turn
            Enemies = new List<Enemy>
            {
                EnemyFactory.CreateEnemy(EnemyType.ForlornArchivist) // NEW boss enemy type
            }
        };

        // Room 25: Core Vault B - The Engine Room (NEW - Physical Boss)
        var engineRoom = new Room
        {
            Id = 25,
            Name = "Engine Room",
            Description =
                "The heart of the facility. A massive power core still hums with ancient energy, and a colossus " +
                "of metal and fury has awoken to defend it. This is the Omega Sentinel, the final failsafe. " +
                "Live conduits crackle with lethal power across the multi-level arena.",
            Exits = new Dictionary<string, string>
            {
                { "west", "Deep Vault Antechamber" }
            },
            IsBossRoom = true,
            HasEnvironmentalHazard = true,
            IsHazardActive = true,
            HazardDamagePerTurn = 16, // 2d8 electrical damage if knocked into conduits
            HazardDescription = "[Live Power Conduit] - Exposed electrical systems cover the arena. Knockback attacks can slam you into them.",
            Enemies = new List<Enemy>
            {
                EnemyFactory.CreateEnemy(EnemyType.OmegaSentinel) // NEW boss enemy type
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

        // [v0.6] Add new rooms (The Lower Depths)
        Rooms.Add(maintenanceShaft.Name, maintenanceShaft);
        Rooms.Add(collapsedJunction.Name, collapsedJunction);
        Rooms.Add(airlockTransition.Name, airlockTransition);
        Rooms.Add(floodedReservoir.Name, floodedReservoir);
        Rooms.Add(pumpControlStation.Name, pumpControlStation);
        Rooms.Add(filtrationArray.Name, filtrationArray);
        Rooms.Add(sumpFloor.Name, sumpFloor);
        Rooms.Add(deepVaultAntechamber.Name, deepVaultAntechamber);
        Rooms.Add(sleepersVault.Name, sleepersVault);
        Rooms.Add(engineRoom.Name, engineRoom);
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
    /// [v0.6] Unlock The Lower Depths from Arsenal Vault (Room 14)
    /// Called when player defeats Ruin-Warden boss
    /// </summary>
    public void UnlockLowerDepthsFromArsenalVault()
    {
        var arsenalVault = GetRoom("Arsenal Vault");
        var maintenanceShaft = GetRoom("Maintenance Shaft");

        // Add exit from boss room to Maintenance Shaft
        if (!arsenalVault.Exits.ContainsKey("south"))
        {
            arsenalVault.Exits.Add("south", "Maintenance Shaft");
        }

        // Add return exit from Maintenance Shaft to boss room
        if (!maintenanceShaft.Exits.ContainsKey("north"))
        {
            maintenanceShaft.Exits.Add("north", "Arsenal Vault");
        }
    }

    /// <summary>
    /// [v0.6] Unlock The Lower Depths from Energy Core (Room 15)
    /// Called when player defeats Aetheric Aberration boss
    /// </summary>
    public void UnlockLowerDepthsFromEnergyCore()
    {
        var energyCore = GetRoom("Energy Core");
        var maintenanceShaft = GetRoom("Maintenance Shaft");

        // Add exit from boss room to Maintenance Shaft
        if (!energyCore.Exits.ContainsKey("south"))
        {
            energyCore.Exits.Add("south", "Maintenance Shaft");
        }

        // Add return exit from Maintenance Shaft to boss room
        if (!maintenanceShaft.Exits.ContainsKey("north"))
        {
            maintenanceShaft.Exits.Add("north", "Energy Core");
        }
    }

    /// <summary>
    /// [v0.6] Unlock Airlock Transition puzzle gate (Room 18)
    /// Called when player succeeds WITS check (DC 6) on airlock override
    /// </summary>
    public void UnlockAirlockTransition()
    {
        var airlockTransition = GetRoom("Airlock Transition");
        if (!airlockTransition.Exits.ContainsKey("south"))
        {
            airlockTransition.Exits.Add("south", "Flooded Reservoir");
        }
    }

    /// <summary>
    /// [v0.6] Drain toxic sludge from Flooded Reservoir (Room 19)
    /// Called when player succeeds WITS check (DC 5) at Pump Control Station (Room 20)
    /// </summary>
    public void DrainFloodedReservoir()
    {
        var floodedReservoir = GetRoom("Flooded Reservoir");
        if (floodedReservoir.HasEnvironmentalHazard && floodedReservoir.IsHazardActive)
        {
            floodedReservoir.IsHazardActive = false; // Disable toxic sludge hazard
        }
    }

    /// <summary>
    /// [v0.6] Unlock final boss rooms from Deep Vault Antechamber (Room 23)
    /// Called when player defeats Vault Custodian mini-boss
    /// </summary>
    public void UnlockFinalBossRooms()
    {
        var deepVaultAntechamber = GetRoom("Deep Vault Antechamber");

        // Add exits to both final boss rooms
        if (!deepVaultAntechamber.Exits.ContainsKey("west"))
        {
            deepVaultAntechamber.Exits.Add("west", "Sleeper's Tomb");
        }

        if (!deepVaultAntechamber.Exits.ContainsKey("east"))
        {
            deepVaultAntechamber.Exits.Add("east", "Engine Room");
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

            case "Collapsed Junction": // [v0.6] Room 17 - Hidden cache behind rubble
                puzzleReward = EquipmentDatabase.GetRandomWeaponForClass(player.Class, QualityTier.Optimized);
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

    /// <summary>
    /// [v0.6] Add Optimized equipment cache to Sump Floor (Room 22)
    /// Called when room is first entered (sanctuary rest before final bosses)
    /// </summary>
    public void AddSumpFloorLoot(PlayerCharacter player)
    {
        var lootService = new LootService();
        var sumpFloor = GetRoom("Sump Floor");

        // Check if loot already placed
        if (sumpFloor.ItemsOnGround.Count > 0)
        {
            return;
        }

        // Generate 2x Optimized items for final boss preparation
        var optimizedWeapon = EquipmentDatabase.GetRandomWeaponForClass(player.Class, QualityTier.Optimized);
        var optimizedArmor = EquipmentDatabase.GetRandomArmor(QualityTier.Optimized);

        if (optimizedWeapon != null)
        {
            lootService.PlaceStartingLoot(sumpFloor, optimizedWeapon);
        }

        if (optimizedArmor != null)
        {
            lootService.PlaceStartingLoot(sumpFloor, optimizedArmor);
        }
    }
}
