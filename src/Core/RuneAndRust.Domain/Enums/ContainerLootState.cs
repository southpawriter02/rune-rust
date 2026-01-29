namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Defines the lifecycle states of a container for loot tracking.
/// </summary>
/// <remarks>
/// <para>
/// Containers progress through states from undiscovered (for hidden containers)
/// or discovered (for visible containers) through to looted. Not all containers
/// pass through every state (e.g., visible chests skip Undiscovered, unlocked
/// chests skip Locked).
/// </para>
/// <para>
/// State transitions are enforced by the <see cref="Entities.ContainerLootTable"/>
/// entity methods. Once a container reaches the Looted state, it cannot be
/// looted again within the same game run.
/// </para>
/// <para>
/// Note: This enum tracks loot lifecycle states, distinct from the
/// <c>ContainerState</c> enum in ExaminationEnums.cs which tracks
/// physical container conditions (Intact, Damaged, etc.).
/// </para>
/// </remarks>
public enum ContainerLootState
{
    /// <summary>
    /// Container has not been discovered by the player.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Applies to hidden containers (e.g., HiddenCache) requiring a Perception check.
    /// Regular containers skip this state and start as Discovered.
    /// </para>
    /// <para>
    /// Transition: Undiscovered → Discovered via <c>Discover()</c> when perception
    /// check succeeds.
    /// </para>
    /// </remarks>
    Undiscovered = 0,

    /// <summary>
    /// Container is visible but has not been interacted with.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Initial state for most containers. The player can see the container
    /// but has not opened or unlocked it.
    /// </para>
    /// <para>
    /// Transitions:
    /// <list type="bullet">
    /// <item>Discovered → Locked (initial assignment for locked containers)</item>
    /// <item>Discovered → Open via <c>Open()</c> for unlocked containers</item>
    /// </list>
    /// </para>
    /// </remarks>
    Discovered = 1,

    /// <summary>
    /// Container requires unlocking before it can be opened.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Applies to locked containers requiring a key item or successful lockpick
    /// skill check. Not all containers have this state.
    /// </para>
    /// <para>
    /// Transition: Locked → Open via <c>Unlock()</c> when key is used or
    /// lockpicking succeeds.
    /// </para>
    /// </remarks>
    Locked = 2,

    /// <summary>
    /// Container is open and contents are visible.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Contents have been revealed but not yet taken. The player can see
    /// items and currency inside. This is the only state from which looting
    /// is permitted.
    /// </para>
    /// <para>
    /// Transition: Open → Looted via <c>Loot()</c> when player takes contents.
    /// </para>
    /// </remarks>
    Open = 3,

    /// <summary>
    /// Container has been emptied of all contents.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Terminal state. Container cannot be looted again within this game run.
    /// Subsequent access returns empty contents. The original contents are
    /// preserved in the entity for reference/logging purposes.
    /// </para>
    /// </remarks>
    Looted = 4
}
