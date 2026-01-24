using RuneAndRust.Domain.Entities;

namespace RuneAndRust.Domain.Interfaces;

/// <summary>
/// Repository interface for persisting and retrieving tracking state.
/// </summary>
/// <remarks>
/// <para>
/// Provides persistence operations for <see cref="TrackingState"/> entities.
/// Implementations should handle:
/// <list type="bullet">
///   <item><description>In-memory storage for session-based tracking</description></item>
///   <item><description>Database persistence for long-running pursuits</description></item>
///   <item><description>Concurrent access from multiple service calls</description></item>
/// </list>
/// </para>
/// <para>
/// Each player can only have one active tracking pursuit at a time.
/// The repository enforces this constraint through the GetActiveByTracker method.
/// </para>
/// </remarks>
public interface ITrackingStateRepository
{
    /// <summary>
    /// Saves or updates a tracking state.
    /// </summary>
    /// <param name="state">The tracking state to save.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when state is null.</exception>
    /// <remarks>
    /// <para>
    /// If the tracking state already exists (same TrackingId), it will be updated.
    /// If it's a new state, it will be created.
    /// </para>
    /// <para>
    /// Implementations should ensure thread-safe operations when multiple
    /// service calls attempt concurrent updates.
    /// </para>
    /// </remarks>
    Task SaveAsync(TrackingState state, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a tracking state by its unique identifier.
    /// </summary>
    /// <param name="trackingId">The unique identifier of the tracking state.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>The tracking state if found, or null if not found.</returns>
    /// <exception cref="ArgumentNullException">Thrown when trackingId is null or empty.</exception>
    /// <remarks>
    /// Returns the complete tracking state including check history and terrain history.
    /// </remarks>
    Task<TrackingState?> GetByIdAsync(string trackingId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the active tracking for a specific tracker.
    /// </summary>
    /// <param name="trackerId">The identifier of the tracking character.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>The active tracking state, or null if none exists.</returns>
    /// <exception cref="ArgumentNullException">Thrown when trackerId is null or empty.</exception>
    /// <remarks>
    /// <para>
    /// A tracker can only have one active tracking pursuit at a time.
    /// Active means Status == TrackingStatus.Active.
    /// </para>
    /// <para>
    /// Returns null if the tracker has no active tracking or if all their
    /// tracking attempts have been completed (TargetFound, TrailCold, or Abandoned).
    /// </para>
    /// </remarks>
    Task<TrackingState?> GetActiveByTrackerAsync(string trackerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all tracking states for a specific tracker (active and completed).
    /// </summary>
    /// <param name="trackerId">The identifier of the tracking character.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>A list of all tracking states for the tracker.</returns>
    /// <exception cref="ArgumentNullException">Thrown when trackerId is null or empty.</exception>
    /// <remarks>
    /// Returns all tracking states regardless of status, ordered by StartedAt descending
    /// (most recent first). Useful for history display and analytics.
    /// </remarks>
    Task<IReadOnlyList<TrackingState>> GetAllByTrackerAsync(string trackerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a tracking state.
    /// </summary>
    /// <param name="trackingId">The unique identifier of the tracking state to delete.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>True if the state was deleted, false if not found.</returns>
    /// <exception cref="ArgumentNullException">Thrown when trackingId is null or empty.</exception>
    /// <remarks>
    /// <para>
    /// Permanently removes the tracking state and all associated history.
    /// Use with caution - consider using Abandon() on the state instead
    /// to preserve history.
    /// </para>
    /// </remarks>
    Task<bool> DeleteAsync(string trackingId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a tracker has an active tracking pursuit.
    /// </summary>
    /// <param name="trackerId">The identifier of the tracking character.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>True if the tracker has an active pursuit, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when trackerId is null or empty.</exception>
    /// <remarks>
    /// More efficient than GetActiveByTrackerAsync when only checking existence.
    /// </remarks>
    Task<bool> HasActiveTrackingAsync(string trackerId, CancellationToken cancellationToken = default);
}
