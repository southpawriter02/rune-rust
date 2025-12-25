using System.Text.Json;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// Handles emergency game state preservation during crashes (v0.3.16b).
/// Writes to a file-based backup since the database may be unavailable.
/// Uses synchronous I/O to ensure completion before process death.
/// Part of "The Black Box" crash recovery system.
/// </summary>
/// <remarks>See: SPEC-CRASH-001 for Crash Handling System design.</remarks>
public class EmergencySaveService : IEmergencySaveService
{
    /// <summary>
    /// Path to the emergency save file.
    /// </summary>
    private const string EmergencyPath = "data/saves/emergency.json";

    /// <summary>
    /// JSON serialization options matching the standard SaveManager format.
    /// </summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <inheritdoc/>
    public bool TryEmergencySave(GameState state)
    {
        try
        {
            // Viability check: Only save if there's an active session with a character
            if (state?.CurrentCharacter == null || !state.IsSessionActive)
            {
                return false;
            }

            // Ensure the saves directory exists
            var directory = Path.GetDirectoryName(EmergencyPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Synchronous serialization and write for crash reliability
            var json = JsonSerializer.Serialize(state, JsonOptions);
            File.WriteAllText(EmergencyPath, json);

            Console.WriteLine($"[EMERGENCY] Game state saved to {EmergencyPath}");
            return true;
        }
        catch (Exception ex)
        {
            // Log to console since Serilog may be unavailable during crash
            Console.WriteLine($"[EMERGENCY] Save failed: {ex.Message}");
            return false;
        }
    }

    /// <inheritdoc/>
    public bool EmergencySaveExists()
    {
        return File.Exists(EmergencyPath);
    }

    /// <inheritdoc/>
    public GameState? LoadEmergencySave()
    {
        try
        {
            if (!File.Exists(EmergencyPath))
            {
                return null;
            }

            var json = File.ReadAllText(EmergencyPath);
            return JsonSerializer.Deserialize<GameState>(json, JsonOptions);
        }
        catch
        {
            // Return null on any error (corrupt file, IO issue, etc.)
            return null;
        }
    }

    /// <inheritdoc/>
    public void ClearEmergencySave()
    {
        try
        {
            if (File.Exists(EmergencyPath))
            {
                File.Delete(EmergencyPath);
            }
        }
        catch
        {
            // Ignore cleanup failures - not critical
        }
    }
}
