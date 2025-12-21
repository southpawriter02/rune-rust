using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// Spawns interactable objects in rooms during dungeon generation.
/// Generates 2-3 objects per room with AAM-VOICE compliant descriptions.
/// </summary>
public class ObjectSpawner
{
    private readonly IInteractableObjectRepository _objectRepository;
    private readonly ILogger<ObjectSpawner> _logger;

    /// <summary>
    /// Minimum objects to spawn per room.
    /// </summary>
    private const int MinObjectsPerRoom = 2;

    /// <summary>
    /// Maximum objects to spawn per room.
    /// </summary>
    private const int MaxObjectsPerRoom = 3;

    /// <summary>
    /// Base descriptions for each object type. AAM-VOICE compliant.
    /// </summary>
    private static readonly Dictionary<ObjectType, ObjectTemplate[]> ObjectTemplates = new()
    {
        [ObjectType.Furniture] = new[]
        {
            new ObjectTemplate(
                "Collapsed Table",
                "A metal table lies overturned, its surface scarred by ancient violence.",
                "Scratch marks suggest something was dragged across it long ago.",
                "The markings form a crude map, perhaps scratched by a desperate survivor."),
            new ObjectTemplate(
                "Shattered Cabinet",
                "A storage cabinet stands with its doors hanging open, contents long plundered.",
                "Traces of dried organic material cling to the interior shelves.",
                "Hidden behind a false panel, you notice a small compartment."),
            new ObjectTemplate(
                "Debris Pile",
                "Rubble and twisted metal form an impassable mound.",
                "Among the debris, fragments of bone and rusted equipment are visible.",
                "Something glints beneath the surface, partially buried.")
        },
        [ObjectType.Container] = new[]
        {
            new ObjectTemplate(
                "Rusted Chest",
                "A corroded metal chest sits against the wall, its lock mechanism visible.",
                "Despite the rust, the hinges still seem functional.",
                "Faint inscriptions on the lock suggest it requires a specific key."),
            new ObjectTemplate(
                "Corroded Locker",
                "A tall locker stands against the wall, its door slightly ajar.",
                "The interior is dark; something shifts as you approach.",
                "Personal effects inside bear ranger insignia from the old patrols."),
            new ObjectTemplate(
                "Supply Crate",
                "A military-style crate rests near the passage entrance.",
                "The sealing mechanism shows signs of previous tampering.",
                "Markings identify this as pre-Glitch salvage, potentially valuable.")
        },
        [ObjectType.Device] = new[]
        {
            new ObjectTemplate(
                "Silent Terminal",
                "A data terminal sits dormant, its screen dark and lifeless.",
                "Pressing the activation panel produces no response, but warmth radiates from within.",
                "Faded text on the casing reads: 'J.T.N. COMMAND INTERFACE - RESTRICTED'"),
            new ObjectTemplate(
                "Dormant Console",
                "A control console protrudes from the wall, covered in dust and corrosion.",
                "Several indicator lights flicker weakly, suggesting residual power.",
                "The interface appears configured for security protocols long abandoned."),
            new ObjectTemplate(
                "Corroded Switch",
                "A large mechanical switch extends from a rusted panel.",
                "The lever resists movement, seized by age and neglect.",
                "Cables running from the switch disappear into the walls toward an unknown destination.")
        },
        [ObjectType.Inscription] = new[]
        {
            new ObjectTemplate(
                "Faded Runes",
                "Strange symbols are carved into the wall, glowing faintly with residual energy.",
                "The characters resemble old Dvergr script, though corrupted by time.",
                "Translation reveals a warning: 'The Sleeper stirs in depths below.'"),
            new ObjectTemplate(
                "Etched Symbols",
                "Geometric patterns cover a section of the wall in precise arrangements.",
                "Closer inspection reveals the patterns form a larger design, perhaps a map.",
                "These are navigation markers from the old ranger network."),
            new ObjectTemplate(
                "Warning Sign",
                "A metal placard displays text in multiple languages, all warning of danger.",
                "Most text has corroded away, but the intent remains clear.",
                "Emergency protocols listed here suggest evacuation routes still exist.")
        },
        [ObjectType.Corpse] = new[]
        {
            new ObjectTemplate(
                "Fallen Ranger",
                "The remains of a ranger lie slumped against the wall, equipment scattered nearby.",
                "The uniform suggests this was once a patrol member. Personal effects remain.",
                "A datapad clutched in skeletal hands contains encrypted coordinates."),
            new ObjectTemplate(
                "Ancient Bones",
                "Skeletal remains rest in a corner, disturbed only by time and vermin.",
                "The arrangement suggests this individual died while attempting to hide.",
                "Among the bones, a corroded badge identifies them as a facility technician."),
            new ObjectTemplate(
                "Creature Remains",
                "The twisted carcass of something not quite natural lies in decay.",
                "Corruption has warped the flesh, making identification impossible.",
                "The wounds suggest this creature was killed by something even more dangerous.")
        }
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="ObjectSpawner"/> class.
    /// </summary>
    /// <param name="objectRepository">The repository for persisting objects.</param>
    /// <param name="logger">The logger for traceability.</param>
    public ObjectSpawner(
        IInteractableObjectRepository objectRepository,
        ILogger<ObjectSpawner> logger)
    {
        _objectRepository = objectRepository;
        _logger = logger;
    }

    /// <summary>
    /// Spawns interactable objects in a room.
    /// </summary>
    /// <param name="roomId">The room to populate with objects.</param>
    /// <param name="biome">The room's biome type for themed objects.</param>
    /// <returns>The number of objects spawned.</returns>
    public async Task<int> SpawnObjectsInRoomAsync(Guid roomId, BiomeType biome = BiomeType.Ruin)
    {
        _logger.LogInformation("Spawning objects in room {RoomId} with biome {Biome}", roomId, biome);

        // Clear any existing objects
        await _objectRepository.ClearRoomObjectsAsync(roomId);

        // Determine how many objects to spawn
        var objectCount = Random.Shared.Next(MinObjectsPerRoom, MaxObjectsPerRoom + 1);

        _logger.LogDebug("Spawning {Count} objects in room {RoomId}", objectCount, roomId);

        // Select object types to spawn (ensure variety)
        var objectTypes = SelectObjectTypes(objectCount, biome);
        var spawnedObjects = new List<InteractableObject>();

        foreach (var objectType in objectTypes)
        {
            var obj = CreateObject(roomId, objectType, biome);
            spawnedObjects.Add(obj);

            _logger.LogDebug("Created object '{ObjectName}' ({ObjectType}) in room {RoomId}",
                obj.Name, obj.ObjectType, roomId);
        }

        // Persist all objects
        await _objectRepository.AddRangeAsync(spawnedObjects);
        await _objectRepository.SaveChangesAsync();

        _logger.LogInformation("Spawned {Count} objects in room {RoomId}", spawnedObjects.Count, roomId);

        return spawnedObjects.Count;
    }

    /// <summary>
    /// Spawns objects in all provided rooms.
    /// </summary>
    /// <param name="roomIds">The rooms to populate.</param>
    /// <returns>The total number of objects spawned.</returns>
    public async Task<int> SpawnObjectsInRoomsAsync(IEnumerable<Guid> roomIds)
    {
        var totalSpawned = 0;

        foreach (var roomId in roomIds)
        {
            // Assign a random biome for variety
            var biome = (BiomeType)Random.Shared.Next(0, 4);
            totalSpawned += await SpawnObjectsInRoomAsync(roomId, biome);
        }

        _logger.LogInformation("Total objects spawned across all rooms: {Total}", totalSpawned);

        return totalSpawned;
    }

    /// <summary>
    /// Selects which object types to spawn, ensuring variety.
    /// Hazards are excluded as they should be placed intentionally, not spawned randomly.
    /// </summary>
    private List<ObjectType> SelectObjectTypes(int count, BiomeType biome)
    {
        var types = new List<ObjectType>();
        // Exclude Hazard - hazards are placed intentionally via HazardService, not spawned randomly
        var availableTypes = Enum.GetValues<ObjectType>()
            .Where(t => t != ObjectType.Hazard)
            .ToList();

        // Bias toward certain types based on biome
        var biasedTypes = GetBiomeBias(biome);

        for (int i = 0; i < count; i++)
        {
            ObjectType selected;

            // 50% chance to use biome-biased type if available
            if (biasedTypes.Any() && Random.Shared.NextDouble() < 0.5)
            {
                selected = biasedTypes[Random.Shared.Next(biasedTypes.Count)];
            }
            else
            {
                selected = availableTypes[Random.Shared.Next(availableTypes.Count)];
            }

            types.Add(selected);
        }

        return types;
    }

    /// <summary>
    /// Gets object types that are more common in a given biome.
    /// </summary>
    private static List<ObjectType> GetBiomeBias(BiomeType biome)
    {
        return biome switch
        {
            BiomeType.Ruin => new List<ObjectType> { ObjectType.Furniture, ObjectType.Inscription },
            BiomeType.Industrial => new List<ObjectType> { ObjectType.Device, ObjectType.Container },
            BiomeType.Organic => new List<ObjectType> { ObjectType.Corpse, ObjectType.Furniture },
            BiomeType.Void => new List<ObjectType> { ObjectType.Inscription, ObjectType.Corpse },
            _ => new List<ObjectType>()
        };
    }

    /// <summary>
    /// Creates an interactable object from a template.
    /// </summary>
    private InteractableObject CreateObject(Guid roomId, ObjectType objectType, BiomeType biome)
    {
        var templates = ObjectTemplates[objectType];
        var template = templates[Random.Shared.Next(templates.Length)];

        var obj = new InteractableObject
        {
            RoomId = roomId,
            Name = template.Name,
            ObjectType = objectType,
            Description = template.BaseDescription,
            DetailedDescription = template.DetailedDescription,
            ExpertDescription = template.ExpertDescription,
            IsContainer = objectType == ObjectType.Container,
            IsOpen = false,
            IsLocked = objectType == ObjectType.Container && Random.Shared.NextDouble() < 0.3,
            LockDifficulty = 0
        };

        // Set lock difficulty if locked
        if (obj.IsLocked)
        {
            obj.LockDifficulty = Random.Shared.Next(1, 4); // 1-3 net successes required
        }

        return obj;
    }

    /// <summary>
    /// Template for object descriptions at all three tiers.
    /// </summary>
    private record ObjectTemplate(
        string Name,
        string BaseDescription,
        string DetailedDescription,
        string ExpertDescription
    );
}
