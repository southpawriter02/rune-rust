using RuneAndRust.Domain.Entities;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Repository interface for persisting container loot table states.
/// </summary>
/// <remarks>
/// <para>
/// This repository manages container state within a game run. Container states
/// (discovered, looted, contents) persist until explicitly reset. Typically,
/// reset occurs when starting a new game or run.
/// </para>
/// <para>
/// Implementations should maintain an index by room ID for efficient
/// room-based queries. The repository supports both single and batch
/// operations for flexibility and performance optimization.
/// </para>
/// <para>
/// Key lifecycle scenarios:
/// <list type="bullet">
/// <item>Player enters room → <see cref="GetContainersByRoomAsync"/> retrieves existing states</item>
/// <item>Container opened/looted → <see cref="SaveContainerAsync"/> persists the new state</item>
/// <item>New game/run started → <see cref="ResetAllContainersAsync"/> clears all states</item>
/// </list>
/// </para>
/// </remarks>
/// <seealso cref="ContainerLootTable"/>
public interface IContainerStateRepository
{
    /// <summary>
    /// Gets a container by its unique identifier.
    /// </summary>
    /// <param name="containerId">The container's unique identifier.</param>
    /// <param name="ct">Cancellation token for operation cancellation.</param>
    /// <returns>
    /// The container if found, or <c>null</c> if no container exists with that ID.
    /// </returns>
    /// <remarks>
    /// Use this method when you need to retrieve a specific container by ID,
    /// such as when processing a player's interaction with a known container.
    /// </remarks>
    /// <example>
    /// <code>
    /// var container = await repository.GetContainerAsync(containerId);
    /// if (container is not null)
    /// {
    ///     // Process the container
    /// }
    /// </code>
    /// </example>
    Task<ContainerLootTable?> GetContainerAsync(
        Guid containerId,
        CancellationToken ct = default);

    /// <summary>
    /// Gets all containers in a specific room.
    /// </summary>
    /// <param name="roomId">The room's unique identifier.</param>
    /// <param name="ct">Cancellation token for operation cancellation.</param>
    /// <returns>
    /// A read-only list of containers in the room, or an empty list if none exist.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method is typically called when a player enters a room to retrieve
    /// all container states for that room. Previously looted containers will
    /// be returned with their <see cref="Domain.Enums.ContainerLootState.Looted"/> state.
    /// </para>
    /// <para>
    /// Implementations should maintain an index by room ID for efficient
    /// lookups in rooms with multiple containers.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var containers = await repository.GetContainersByRoomAsync(currentRoomId);
    /// foreach (var container in containers)
    /// {
    ///     // Display container based on its state
    /// }
    /// </code>
    /// </example>
    Task<IReadOnlyList<ContainerLootTable>> GetContainersByRoomAsync(
        Guid roomId,
        CancellationToken ct = default);

    /// <summary>
    /// Saves or updates a container's state.
    /// </summary>
    /// <param name="container">The container to save.</param>
    /// <param name="ct">Cancellation token for operation cancellation.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="container"/> is null.</exception>
    /// <remarks>
    /// <para>
    /// If a container with the same ID exists, it will be updated (upsert semantics).
    /// Otherwise, a new container record is created.
    /// </para>
    /// <para>
    /// This method should be called after any state change:
    /// <list type="bullet">
    /// <item>Container discovered (hidden cache found)</item>
    /// <item>Container opened</item>
    /// <item>Container looted</item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// container.Open();
    /// await repository.SaveContainerAsync(container);
    /// </code>
    /// </example>
    Task SaveContainerAsync(
        ContainerLootTable container,
        CancellationToken ct = default);

    /// <summary>
    /// Saves or updates multiple containers in a batch operation.
    /// </summary>
    /// <param name="containers">The containers to save.</param>
    /// <param name="ct">Cancellation token for operation cancellation.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="containers"/> is null.</exception>
    /// <remarks>
    /// <para>
    /// More efficient than multiple individual <see cref="SaveContainerAsync"/> calls.
    /// Useful when initializing room containers or performing bulk updates.
    /// </para>
    /// <para>
    /// Each container in the collection follows the same upsert semantics as
    /// <see cref="SaveContainerAsync"/>.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var roomContainers = new[]
    /// {
    ///     ContainerLootTable.Create(ContainerType.SmallChest, roomId: roomId),
    ///     ContainerLootTable.Create(ContainerType.MediumChest, roomId: roomId)
    /// };
    /// await repository.SaveContainersAsync(roomContainers);
    /// </code>
    /// </example>
    Task SaveContainersAsync(
        IEnumerable<ContainerLootTable> containers,
        CancellationToken ct = default);

    /// <summary>
    /// Deletes a container by its unique identifier.
    /// </summary>
    /// <param name="containerId">The container's unique identifier.</param>
    /// <param name="ct">Cancellation token for operation cancellation.</param>
    /// <returns>
    /// <c>true</c> if the container was deleted;
    /// <c>false</c> if no container was found with that ID.
    /// </returns>
    /// <remarks>
    /// Use sparingly. Typically containers are reset via <see cref="ResetAllContainersAsync"/>
    /// rather than deleted individually. Individual deletion may be needed for
    /// special scenarios like removing a container destroyed by an explosion.
    /// </remarks>
    Task<bool> DeleteContainerAsync(
        Guid containerId,
        CancellationToken ct = default);

    /// <summary>
    /// Resets all container states, clearing the repository.
    /// </summary>
    /// <param name="ct">Cancellation token for operation cancellation.</param>
    /// <returns>The number of containers that were cleared.</returns>
    /// <remarks>
    /// <para>
    /// Called when starting a new game or run. All container states
    /// (discovered, looted, contents) are cleared, allowing containers
    /// to be looted again.
    /// </para>
    /// <para>
    /// The return value indicates how many containers were in the repository
    /// before clearing, useful for logging and analytics.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var clearedCount = await repository.ResetAllContainersAsync();
    /// logger.LogInformation("Reset {Count} containers for new run", clearedCount);
    /// </code>
    /// </example>
    Task<int> ResetAllContainersAsync(CancellationToken ct = default);

    /// <summary>
    /// Checks if a container exists in the repository.
    /// </summary>
    /// <param name="containerId">The container's unique identifier.</param>
    /// <param name="ct">Cancellation token for operation cancellation.</param>
    /// <returns>
    /// <c>true</c> if the container exists; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// More efficient than <see cref="GetContainerAsync"/> when you only need
    /// to check existence without retrieving the full entity.
    /// </remarks>
    Task<bool> ExistsAsync(
        Guid containerId,
        CancellationToken ct = default);

    /// <summary>
    /// Gets the count of containers currently in the repository.
    /// </summary>
    /// <param name="ct">Cancellation token for operation cancellation.</param>
    /// <returns>The total number of persisted containers.</returns>
    /// <remarks>
    /// Useful for analytics, debugging, and verifying reset operations.
    /// </remarks>
    Task<int> GetCountAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets all containers that have been looted.
    /// </summary>
    /// <param name="ct">Cancellation token for operation cancellation.</param>
    /// <returns>A read-only list of looted containers.</returns>
    /// <remarks>
    /// <para>
    /// Returns containers with <see cref="Domain.Enums.ContainerLootState.Looted"/> state.
    /// Useful for:
    /// <list type="bullet">
    /// <item>Analytics (tracking player loot behavior)</item>
    /// <item>Achievement checking (e.g., "Loot 100 containers")</item>
    /// <item>Save game generation (persisting loot history)</item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var lootedContainers = await repository.GetLootedContainersAsync();
    /// achievementService.CheckLootCount(lootedContainers.Count);
    /// </code>
    /// </example>
    Task<IReadOnlyList<ContainerLootTable>> GetLootedContainersAsync(
        CancellationToken ct = default);
}
