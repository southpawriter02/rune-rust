using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Application.UnitTests.Fakes;

/// <summary>
/// In-memory implementation of <see cref="IContainerStateRepository"/> for testing.
/// </summary>
/// <remarks>
/// <para>
/// This fake implementation stores containers in memory using dictionaries for
/// fast lookup by container ID and room ID. It is intended for unit and
/// integration testing scenarios only.
/// </para>
/// <para>
/// The implementation is thread-safe for basic operations but should not be
/// used in concurrent production scenarios.
/// </para>
/// </remarks>
public class InMemoryContainerStateRepository : IContainerStateRepository
{
    /// <summary>
    /// Primary storage: container ID → container entity.
    /// </summary>
    private readonly Dictionary<Guid, ContainerLootTable> _containers = new();

    /// <summary>
    /// Room index: room ID → set of container IDs in that room.
    /// </summary>
    private readonly Dictionary<Guid, HashSet<Guid>> _roomIndex = new();

    /// <inheritdoc/>
    public Task<ContainerLootTable?> GetContainerAsync(
        Guid containerId,
        CancellationToken ct = default)
    {
        _containers.TryGetValue(containerId, out var container);
        return Task.FromResult(container);
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<ContainerLootTable>> GetContainersByRoomAsync(
        Guid roomId,
        CancellationToken ct = default)
    {
        if (!_roomIndex.TryGetValue(roomId, out var containerIds))
        {
            return Task.FromResult<IReadOnlyList<ContainerLootTable>>(
                Array.Empty<ContainerLootTable>());
        }

        var containers = containerIds
            .Where(id => _containers.ContainsKey(id))
            .Select(id => _containers[id])
            .ToList();

        return Task.FromResult<IReadOnlyList<ContainerLootTable>>(containers);
    }

    /// <inheritdoc/>
    public Task SaveContainerAsync(
        ContainerLootTable container,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(container);

        // Check if container already exists with a different room
        if (_containers.TryGetValue(container.Id, out var existing) && existing.RoomId.HasValue)
        {
            // Remove from old room index if room changed
            if (existing.RoomId != container.RoomId &&
                _roomIndex.TryGetValue(existing.RoomId.Value, out var oldRoomIds))
            {
                oldRoomIds.Remove(container.Id);
            }
        }

        // Store container
        _containers[container.Id] = container;

        // Update room index
        if (container.RoomId.HasValue)
        {
            if (!_roomIndex.TryGetValue(container.RoomId.Value, out var containerIds))
            {
                containerIds = new HashSet<Guid>();
                _roomIndex[container.RoomId.Value] = containerIds;
            }

            containerIds.Add(container.Id);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async Task SaveContainersAsync(
        IEnumerable<ContainerLootTable> containers,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(containers);

        foreach (var container in containers)
        {
            await SaveContainerAsync(container, ct);
        }
    }

    /// <inheritdoc/>
    public Task<bool> DeleteContainerAsync(
        Guid containerId,
        CancellationToken ct = default)
    {
        if (!_containers.TryGetValue(containerId, out var container))
        {
            return Task.FromResult(false);
        }

        _containers.Remove(containerId);

        // Remove from room index
        if (container.RoomId.HasValue &&
            _roomIndex.TryGetValue(container.RoomId.Value, out var containerIds))
        {
            containerIds.Remove(containerId);
        }

        return Task.FromResult(true);
    }

    /// <inheritdoc/>
    public Task<int> ResetAllContainersAsync(CancellationToken ct = default)
    {
        var count = _containers.Count;
        _containers.Clear();
        _roomIndex.Clear();
        return Task.FromResult(count);
    }

    /// <inheritdoc/>
    public Task<bool> ExistsAsync(
        Guid containerId,
        CancellationToken ct = default)
    {
        return Task.FromResult(_containers.ContainsKey(containerId));
    }

    /// <inheritdoc/>
    public Task<int> GetCountAsync(CancellationToken ct = default)
    {
        return Task.FromResult(_containers.Count);
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<ContainerLootTable>> GetLootedContainersAsync(
        CancellationToken ct = default)
    {
        var looted = _containers.Values
            .Where(c => c.State == ContainerLootState.Looted)
            .ToList();

        return Task.FromResult<IReadOnlyList<ContainerLootTable>>(looted);
    }
}
