namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Context for determining available exploration synergies.
/// </summary>
/// <remarks>
/// <para>
/// ExplorationContext captures the current exploration state to determine which
/// synergy actions are available to the player:
/// <list type="bullet">
///   <item><description><see cref="AllowsNavigation"/>: Enables Find Hidden Path synergy</description></item>
///   <item><description><see cref="HasActiveTracking"/>: Enables Track to Lair synergy</description></item>
///   <item><description><see cref="HasPatrols"/>: Enables Avoid Patrol synergy</description></item>
///   <item><description><see cref="AllowsForaging"/>: Enables Find and Loot synergy</description></item>
/// </list>
/// </para>
/// <para>
/// This context is typically built from the current game state, including room
/// type, active tracking sessions, and known enemy presence.
/// </para>
/// </remarks>
/// <param name="LocationId">Current location identifier.</param>
/// <param name="AllowsNavigation">Whether the location permits navigation exploration.</param>
/// <param name="HasActiveTracking">Whether the player has an active tracking session.</param>
/// <param name="HasPatrols">Whether enemy patrols are present in the area.</param>
/// <param name="AllowsForaging">Whether the location permits foraging.</param>
/// <seealso cref="Enums.SynergyType"/>
public readonly record struct ExplorationContext(
    string LocationId,
    bool AllowsNavigation,
    bool HasActiveTracking,
    bool HasPatrols,
    bool AllowsForaging)
{
    // ═══════════════════════════════════════════════════════════════════════════
    // DERIVED PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether any synergies are available in this context.
    /// </summary>
    public bool HasAnySynergies =>
        AllowsNavigation || HasActiveTracking || HasPatrols || AllowsForaging;

    /// <summary>
    /// Gets the count of available synergies.
    /// </summary>
    public int AvailableSynergyCount =>
        (AllowsNavigation ? 1 : 0) +
        (HasActiveTracking ? 1 : 0) +
        (HasPatrols ? 1 : 0) +
        (AllowsForaging ? 1 : 0);

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates an empty context with no synergies available.
    /// </summary>
    /// <param name="locationId">The location identifier.</param>
    /// <returns>An exploration context with all synergies disabled.</returns>
    public static ExplorationContext Empty(string locationId) =>
        new(locationId, false, false, false, false);

    /// <summary>
    /// Creates a context with all synergies available.
    /// </summary>
    /// <param name="locationId">The location identifier.</param>
    /// <returns>An exploration context with all synergies enabled.</returns>
    public static ExplorationContext All(string locationId) =>
        new(locationId, true, true, true, true);

    /// <summary>
    /// Creates a context for standard exploration (navigation and foraging only).
    /// </summary>
    /// <param name="locationId">The location identifier.</param>
    /// <returns>An exploration context for standard exploration.</returns>
    public static ExplorationContext Standard(string locationId) =>
        new(locationId, true, false, false, true);

    /// <summary>
    /// Creates a context for dangerous areas (includes patrol detection).
    /// </summary>
    /// <param name="locationId">The location identifier.</param>
    /// <returns>An exploration context for dangerous areas.</returns>
    public static ExplorationContext Dangerous(string locationId) =>
        new(locationId, true, false, true, true);

    /// <summary>
    /// Creates a context for tracking scenarios.
    /// </summary>
    /// <param name="locationId">The location identifier.</param>
    /// <returns>An exploration context with tracking enabled.</returns>
    public static ExplorationContext WithTracking(string locationId) =>
        new(locationId, true, true, false, true);

    // ═══════════════════════════════════════════════════════════════════════════
    // BUILDER METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new context with navigation enabled/disabled.
    /// </summary>
    /// <param name="allows">Whether navigation is allowed.</param>
    /// <returns>A new context with updated navigation state.</returns>
    public ExplorationContext WithNavigation(bool allows) =>
        this with { AllowsNavigation = allows };

    /// <summary>
    /// Creates a new context with tracking enabled/disabled.
    /// </summary>
    /// <param name="hasTracking">Whether tracking is active.</param>
    /// <returns>A new context with updated tracking state.</returns>
    public ExplorationContext WithTracking(bool hasTracking) =>
        this with { HasActiveTracking = hasTracking };

    /// <summary>
    /// Creates a new context with patrols enabled/disabled.
    /// </summary>
    /// <param name="hasPatrols">Whether patrols are present.</param>
    /// <returns>A new context with updated patrol state.</returns>
    public ExplorationContext WithPatrols(bool hasPatrols) =>
        this with { HasPatrols = hasPatrols };

    /// <summary>
    /// Creates a new context with foraging enabled/disabled.
    /// </summary>
    /// <param name="allows">Whether foraging is allowed.</param>
    /// <returns>A new context with updated foraging state.</returns>
    public ExplorationContext WithForaging(bool allows) =>
        this with { AllowsForaging = allows };

    // ═══════════════════════════════════════════════════════════════════════════
    // STRING FORMATTING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a display string for this context.
    /// </summary>
    /// <returns>A string describing available synergies.</returns>
    public override string ToString()
    {
        var synergies = new List<string>();
        if (AllowsNavigation) synergies.Add("Navigation");
        if (HasActiveTracking) synergies.Add("Tracking");
        if (HasPatrols) synergies.Add("Patrol Detection");
        if (AllowsForaging) synergies.Add("Foraging");

        var synergyList = synergies.Count > 0
            ? string.Join(", ", synergies)
            : "None";

        return $"ExplorationContext [{LocationId}]: {synergyList}";
    }
}
