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
        // Room 1: Entrance (Safe Zone)
        var entrance = new Room
        {
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
            HasBeenCleared = true // Safe zone, no combat
        };

        // Room 2: Corridor (First Combat)
        var corridor = new Room
        {
            Name = "Corridor",
            Description =
                "A long corridor stretches before you. Flickering lights cast erratic shadows. " +
                "You hear the grinding of metal on metal—something moves in the darkness ahead.",
            Exits = new Dictionary<string, string>
            {
                { "south", "Entrance" },
                { "north", "Combat Arena" }
            },
            Enemies = new List<Enemy>
            {
                EnemyFactory.CreateEnemy(EnemyType.CorruptedServitor),
                EnemyFactory.CreateEnemy(EnemyType.CorruptedServitor)
            }
        };

        // Room 3: Combat Arena (Main Combat)
        var combatArena = new Room
        {
            Name = "Combat Arena",
            Description =
                "You enter a vast chamber, once a testing facility. Scorch marks scar the walls. " +
                "Three corrupted machines activate as you enter, their optical sensors glowing red.",
            Exits = new Dictionary<string, string>
            {
                { "south", "Corridor" },
                { "east", "Puzzle Chamber" }
            },
            Enemies = new List<Enemy>
            {
                EnemyFactory.CreateEnemy(EnemyType.BlightDrone),
                EnemyFactory.CreateEnemy(EnemyType.BlightDrone),
                EnemyFactory.CreateEnemy(EnemyType.BlightDrone)
            }
        };

        // Room 4: Puzzle Chamber (The One Puzzle)
        var puzzleChamber = new Room
        {
            Name = "Puzzle Chamber",
            Description =
                "A sealed door blocks your path. Three power conduits are embedded in the wall, " +
                "each humming at different frequencies. A corroded control panel flickers with ancient " +
                "runes. You must route power in the correct sequence to proceed.",
            Exits = new Dictionary<string, string>
            {
                { "west", "Combat Arena" }
                // "north" to Boss Sanctum will be added when puzzle is solved
            },
            HasPuzzle = true,
            PuzzleDescription =
                "The conduits pulse with erratic energy. You need to analyze the power flow patterns " +
                "and determine the correct activation sequence.",
            PuzzleSuccessThreshold = 3,
            PuzzleFailureDamage = 6
        };

        // Room 5: Boss Sanctum (Final Fight)
        var bossSanctum = new Room
        {
            Name = "Boss Sanctum",
            Description =
                "You step into a cathedral of corrupted machinery. At the center stands a towering " +
                "construct—a Ruin-Warden, its frame warped by Blight, its weapon arms still functional. " +
                "It turns toward you. There is no escape.",
            Exits = new Dictionary<string, string>
            {
                { "south", "Puzzle Chamber" }
            },
            IsBossRoom = true,
            Enemies = new List<Enemy>
            {
                EnemyFactory.CreateEnemy(EnemyType.RuinWarden)
            }
        };

        // Add all rooms to dictionary
        Rooms.Add(entrance.Name, entrance);
        Rooms.Add(corridor.Name, corridor);
        Rooms.Add(combatArena.Name, combatArena);
        Rooms.Add(puzzleChamber.Name, puzzleChamber);
        Rooms.Add(bossSanctum.Name, bossSanctum);
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

    public void UnlockPuzzleDoor()
    {
        // Add the north exit to Boss Sanctum after puzzle is solved
        var puzzleChamber = GetRoom("Puzzle Chamber");
        if (!puzzleChamber.Exits.ContainsKey("north"))
        {
            puzzleChamber.Exits.Add("north", "Boss Sanctum");
        }
    }
}
