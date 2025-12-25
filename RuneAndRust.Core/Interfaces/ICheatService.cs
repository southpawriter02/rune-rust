namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Service for executing debug cheat commands (v0.3.17b).
/// Part of "The Toolbox" debug tools milestone.
/// </summary>
/// <remarks>See: SPEC-CHEAT-001 for Cheat Command System design.</remarks>
public interface ICheatService
{
    /// <summary>
    /// Toggles God Mode (invulnerability to damage/stress/corruption).
    /// </summary>
    /// <returns>The new God Mode state.</returns>
    bool ToggleGodMode();

    /// <summary>
    /// Fully restores HP, Stamina, AP and clears negative status effects.
    /// </summary>
    /// <returns>True if heal succeeded, false if no active character.</returns>
    bool FullHeal();

    /// <summary>
    /// Teleports the player to the specified room.
    /// </summary>
    /// <param name="roomIdOrName">Room GUID or partial name match.</param>
    /// <returns>Room name if found, null if not found.</returns>
    Task<string?> TeleportAsync(string roomIdOrName);

    /// <summary>
    /// Spawns an item into the player's inventory.
    /// </summary>
    /// <param name="itemId">The item template ID.</param>
    /// <param name="quantity">Number to spawn (default 1).</param>
    /// <returns>True if spawn succeeded.</returns>
    Task<bool> SpawnItemAsync(string itemId, int quantity = 1);

    /// <summary>
    /// Reveals all rooms on the map (clears fog of war).
    /// </summary>
    /// <returns>Number of rooms revealed.</returns>
    Task<int> RevealMapAsync();
}
