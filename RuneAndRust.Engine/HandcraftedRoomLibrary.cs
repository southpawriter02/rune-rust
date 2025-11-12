using RuneAndRust.Core;
using System.Text.Json;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.11 Handcrafted Room Library
/// Manages handcrafted room definitions for Quest Anchors
/// </summary>
public class HandcraftedRoomLibrary
{
    private static readonly ILogger _log = Log.ForContext<HandcraftedRoomLibrary>();
    private readonly Dictionary<string, HandcraftedRoom> _rooms = new();
    private readonly string _roomDataPath;

    public HandcraftedRoomLibrary(string dataPath = "Data/QuestAnchors")
    {
        _roomDataPath = dataPath;
    }

    /// <summary>
    /// Loads all handcrafted room definitions from JSON files
    /// </summary>
    public void LoadRooms()
    {
        _log.Debug("Loading handcrafted rooms from: {DataPath}", _roomDataPath);

        if (!Directory.Exists(_roomDataPath))
        {
            _log.Warning("Handcrafted room data path not found: {DataPath}", _roomDataPath);
            Console.WriteLine($"Warning: Handcrafted room data path not found: {_roomDataPath}");
            return;
        }

        var roomFiles = Directory.GetFiles(_roomDataPath, "*.json", SearchOption.AllDirectories);
        _log.Debug("Found {FileCount} handcrafted room files to load", roomFiles.Length);

        int loadedCount = 0;
        int invalidCount = 0;

        foreach (var file in roomFiles)
        {
            try
            {
                var json = File.ReadAllText(file);
                var room = JsonSerializer.Deserialize<HandcraftedRoom>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (room != null && !string.IsNullOrEmpty(room.RoomId))
                {
                    var (isValid, errors) = room.Validate();
                    if (isValid)
                    {
                        _rooms[room.RoomId] = room;
                        loadedCount++;

                        _log.Debug("Loaded handcrafted room: {RoomId} ({RoomName}, Enemies={EnemyCount}) from {FileName}",
                            room.RoomId, room.Name, room.EnemyIds.Count, Path.GetFileName(file));
                    }
                    else
                    {
                        invalidCount++;
                        _log.Warning("Handcrafted room validation failed for {RoomId}: {Errors}",
                            room.RoomId, string.Join(", ", errors));
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error loading handcrafted room from file: {FileName}", Path.GetFileName(file));
                Console.WriteLine($"Error loading handcrafted room from {file}: {ex.Message}");
            }
        }

        _log.Information("Loaded {LoadedCount} valid handcrafted rooms ({InvalidCount} invalid, {TotalFiles} total files)",
            loadedCount, invalidCount, roomFiles.Length);
        Console.WriteLine($"Loaded {loadedCount} handcrafted rooms");
    }

    /// <summary>
    /// Gets a handcrafted room by ID
    /// </summary>
    public HandcraftedRoom? GetRoom(string roomId)
    {
        return _rooms.GetValueOrDefault(roomId);
    }

    /// <summary>
    /// Gets all loaded rooms
    /// </summary>
    public List<HandcraftedRoom> GetAllRooms()
    {
        return _rooms.Values.ToList();
    }

    /// <summary>
    /// Gets the count of loaded rooms
    /// </summary>
    public int GetRoomCount()
    {
        return _rooms.Count;
    }

    /// <summary>
    /// Checks if a room exists
    /// </summary>
    public bool HasRoom(string roomId)
    {
        return _rooms.ContainsKey(roomId);
    }
}
