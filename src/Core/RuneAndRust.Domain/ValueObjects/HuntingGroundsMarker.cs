namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents a marked hunting grounds area for the Veioimaor specialization.
/// </summary>
/// <remarks>
/// <para>
/// The Hunting Grounds ability allows a Veioimaor to mark an area as their
/// territory. While active, all Wasteland Survival checks made within the
/// marked area receive +2d10 bonus dice.
/// </para>
/// <para>
/// <b>Marker Lifecycle:</b>
/// <list type="bullet">
///   <item><description><b>Creation:</b> When the character activates Hunting Grounds ability</description></item>
///   <item><description><b>Active:</b> From creation until expiration time or character rests</description></item>
///   <item><description><b>Expired:</b> When current time exceeds <see cref="ExpiresAt"/></description></item>
///   <item><description><b>Cleared:</b> When character rests or manually clears the marker</description></item>
/// </list>
/// </para>
/// <para>
/// Only one hunting grounds marker can be active per character at a time.
/// Marking a new area automatically replaces any existing marker.
/// </para>
/// </remarks>
/// <param name="PlayerId">The unique identifier of the character who marked this area.</param>
/// <param name="AreaId">The unique identifier of the marked area (location, zone, or room).</param>
/// <param name="AreaName">The display name of the marked area.</param>
/// <param name="MarkedAt">The UTC timestamp when the area was marked.</param>
/// <param name="ExpiresAt">The UTC timestamp when the marker expires (null = until rest).</param>
/// <seealso cref="SpecializationBonus"/>
/// <seealso cref="AbilityActivation"/>
public readonly record struct HuntingGroundsMarker(
    string PlayerId,
    string AreaId,
    string AreaName,
    DateTime MarkedAt,
    DateTime? ExpiresAt)
{
    // =========================================================================
    // CONSTANTS
    // =========================================================================

    /// <summary>
    /// The bonus dice granted by an active hunting grounds marker.
    /// </summary>
    /// <value>2 (representing +2d10).</value>
    public const int HuntingGroundsBonusDice = 2;

    /// <summary>
    /// The ability ID for the hunting grounds ability.
    /// </summary>
    public const string AbilityId = "hunting-grounds";

    // =========================================================================
    // COMPUTED PROPERTIES
    // =========================================================================

    /// <summary>
    /// Gets a value indicating whether this marker is currently active.
    /// </summary>
    /// <value>True if the marker has not expired.</value>
    /// <remarks>
    /// A marker is active if:
    /// <list type="bullet">
    ///   <item><description><see cref="ExpiresAt"/> is null (persists until rest), or</description></item>
    ///   <item><description>Current UTC time is before <see cref="ExpiresAt"/></description></item>
    /// </list>
    /// </remarks>
    public bool IsActive => ExpiresAt is null || DateTime.UtcNow < ExpiresAt.Value;

    /// <summary>
    /// Gets a value indicating whether this marker has expired.
    /// </summary>
    /// <value>True if the current time is past the expiration time.</value>
    public bool IsExpired => ExpiresAt.HasValue && DateTime.UtcNow >= ExpiresAt.Value;

    /// <summary>
    /// Gets a value indicating whether this marker is valid (has required data).
    /// </summary>
    /// <value>True if player ID and area ID are not null or whitespace.</value>
    public bool IsValid => !string.IsNullOrWhiteSpace(PlayerId) &&
                           !string.IsNullOrWhiteSpace(AreaId);

    /// <summary>
    /// Gets the duration the marker has been active.
    /// </summary>
    /// <value>The time elapsed since the marker was created.</value>
    public TimeSpan ActiveDuration => DateTime.UtcNow - MarkedAt;

    /// <summary>
    /// Gets the remaining time until the marker expires, if applicable.
    /// </summary>
    /// <value>
    /// The time remaining until expiration, or null if the marker persists until rest.
    /// </value>
    public TimeSpan? RemainingTime => ExpiresAt.HasValue
        ? ExpiresAt.Value - DateTime.UtcNow
        : null;

    // =========================================================================
    // BONUS METHODS
    // =========================================================================

    /// <summary>
    /// Gets the specialization bonus for this hunting grounds marker.
    /// </summary>
    /// <returns>
    /// A <see cref="SpecializationBonus"/> granting +2d10, or an empty bonus if expired.
    /// </returns>
    /// <remarks>
    /// Returns an empty bonus if the marker has expired. The calling service
    /// should check <see cref="IsActive"/> or the bonus's <see cref="SpecializationBonus.IsValid"/>
    /// property before applying the bonus.
    /// </remarks>
    /// <example>
    /// <code>
    /// var marker = HuntingGroundsMarker.Create("player-1", "area-12", "The Rusted Valley");
    /// if (marker.IsActive)
    /// {
    ///     var bonus = marker.GetBonus();
    ///     // bonus.BonusDice == 2
    /// }
    /// </code>
    /// </example>
    public SpecializationBonus GetBonus()
    {
        if (!IsActive || !IsValid)
        {
            return SpecializationBonus.Empty();
        }

        var displayName = string.IsNullOrWhiteSpace(AreaName) ? "hunting grounds" : AreaName;

        return SpecializationBonus.DiceBonus(
            abilityId: AbilityId,
            bonusDice: HuntingGroundsBonusDice,
            description: $"Hunting Grounds ({displayName})");
    }

    // =========================================================================
    // AREA MATCHING METHODS
    // =========================================================================

    /// <summary>
    /// Checks if this marker applies to a specific area.
    /// </summary>
    /// <param name="areaId">The area ID to check.</param>
    /// <returns>True if this marker is for the specified area and is active.</returns>
    /// <remarks>
    /// Comparison is case-insensitive.
    /// </remarks>
    public bool AppliesToArea(string areaId)
    {
        if (string.IsNullOrWhiteSpace(areaId))
        {
            return false;
        }

        return IsActive &&
               IsValid &&
               AreaId.Equals(areaId, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks if this marker belongs to a specific player.
    /// </summary>
    /// <param name="playerId">The player ID to check.</param>
    /// <returns>True if this marker was created by the specified player.</returns>
    /// <remarks>
    /// Comparison is case-insensitive.
    /// </remarks>
    public bool BelongsToPlayer(string playerId)
    {
        if (string.IsNullOrWhiteSpace(playerId))
        {
            return false;
        }

        return PlayerId.Equals(playerId, StringComparison.OrdinalIgnoreCase);
    }

    // =========================================================================
    // FACTORY METHODS
    // =========================================================================

    /// <summary>
    /// Creates a new hunting grounds marker that persists until rest.
    /// </summary>
    /// <param name="playerId">The ID of the character marking the area.</param>
    /// <param name="areaId">The ID of the area being marked.</param>
    /// <param name="areaName">The display name of the area.</param>
    /// <returns>A new active <see cref="HuntingGroundsMarker"/>.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="playerId"/> or <paramref name="areaId"/> is null, empty, or whitespace.
    /// </exception>
    /// <example>
    /// <code>
    /// // Mark an area as hunting grounds (persists until rest)
    /// var marker = HuntingGroundsMarker.Create("player-1", "area-12", "The Rusted Valley");
    /// </code>
    /// </example>
    public static HuntingGroundsMarker Create(string playerId, string areaId, string areaName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(playerId, nameof(playerId));
        ArgumentException.ThrowIfNullOrWhiteSpace(areaId, nameof(areaId));

        return new HuntingGroundsMarker(
            PlayerId: playerId,
            AreaId: areaId,
            AreaName: areaName ?? string.Empty,
            MarkedAt: DateTime.UtcNow,
            ExpiresAt: null);
    }

    /// <summary>
    /// Creates a new hunting grounds marker with a specific duration.
    /// </summary>
    /// <param name="playerId">The ID of the character marking the area.</param>
    /// <param name="areaId">The ID of the area being marked.</param>
    /// <param name="areaName">The display name of the area.</param>
    /// <param name="duration">How long the marker should last.</param>
    /// <returns>A new <see cref="HuntingGroundsMarker"/> that expires after the duration.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="playerId"/> or <paramref name="areaId"/> is null, empty, or whitespace.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="duration"/> is negative or zero.
    /// </exception>
    /// <example>
    /// <code>
    /// // Mark an area for 1 hour
    /// var marker = HuntingGroundsMarker.CreateWithDuration(
    ///     "player-1",
    ///     "area-12",
    ///     "The Rusted Valley",
    ///     TimeSpan.FromHours(1));
    /// </code>
    /// </example>
    public static HuntingGroundsMarker CreateWithDuration(
        string playerId,
        string areaId,
        string areaName,
        TimeSpan duration)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(playerId, nameof(playerId));
        ArgumentException.ThrowIfNullOrWhiteSpace(areaId, nameof(areaId));

        if (duration <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(duration), "Duration must be positive.");
        }

        var now = DateTime.UtcNow;

        return new HuntingGroundsMarker(
            PlayerId: playerId,
            AreaId: areaId,
            AreaName: areaName ?? string.Empty,
            MarkedAt: now,
            ExpiresAt: now + duration);
    }

    /// <summary>
    /// Creates an empty, invalid marker.
    /// </summary>
    /// <returns>An empty <see cref="HuntingGroundsMarker"/>.</returns>
    /// <remarks>
    /// Used as a default return value when no marker exists.
    /// Check <see cref="IsValid"/> before using the marker.
    /// </remarks>
    public static HuntingGroundsMarker Empty() => new(
        PlayerId: string.Empty,
        AreaId: string.Empty,
        AreaName: string.Empty,
        MarkedAt: DateTime.MinValue,
        ExpiresAt: DateTime.MinValue);

    // =========================================================================
    // DISPLAY METHODS
    // =========================================================================

    /// <summary>
    /// Returns a formatted display string for this marker.
    /// </summary>
    /// <returns>A string suitable for display in the UI.</returns>
    public string ToDisplayString()
    {
        if (!IsValid)
        {
            return "No hunting grounds marked";
        }

        var displayName = string.IsNullOrWhiteSpace(AreaName) ? AreaId : AreaName;
        var status = IsActive ? "Active" : "Expired";

        if (RemainingTime.HasValue && IsActive)
        {
            var remaining = RemainingTime.Value;
            var timeStr = remaining.TotalMinutes < 1
                ? $"{remaining.Seconds}s remaining"
                : $"{(int)remaining.TotalMinutes}m remaining";
            return $"Hunting Grounds: {displayName} ({status}, {timeStr})";
        }

        return $"Hunting Grounds: {displayName} ({status}, until rest)";
    }

    /// <summary>
    /// Returns a detailed string representation of this marker.
    /// </summary>
    /// <returns>A detailed string representation.</returns>
    public string ToDetailedString() =>
        $"HuntingGroundsMarker {{ PlayerId = {PlayerId}, AreaId = {AreaId}, " +
        $"AreaName = {AreaName}, MarkedAt = {MarkedAt:O}, ExpiresAt = {ExpiresAt?.ToString("O") ?? "null"}, " +
        $"IsActive = {IsActive} }}";

    /// <summary>
    /// Returns a string representation of this marker.
    /// </summary>
    /// <returns>A string representation.</returns>
    public override string ToString() =>
        $"HuntingGroundsMarker {{ AreaId = {AreaId}, IsActive = {IsActive} }}";
}
