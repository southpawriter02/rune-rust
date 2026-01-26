using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Context for executing a combined skill check.
/// </summary>
/// <remarks>
/// <para>
/// CombinedCheckContext provides all information needed to execute a multi-skill
/// exploration action:
/// <list type="bullet">
///   <item><description>Identifies the synergy type being attempted</description></item>
///   <item><description>Contains context for both primary and secondary checks</description></item>
///   <item><description>Tracks the player initiating the action</description></item>
/// </list>
/// </para>
/// <para>
/// The context object is passed to the <c>ICombinedCheckService.ExecuteCombinedCheck</c>
/// method and provides typed access to the specific context objects required by
/// each synergy type through helper properties.
/// </para>
/// </remarks>
/// <param name="PlayerId">Identifier for the player making the check.</param>
/// <param name="SynergyType">The type of skill synergy being executed.</param>
/// <param name="PrimaryContext">Context for the primary skill check (type depends on synergy).</param>
/// <param name="SecondaryContext">Context for the secondary skill check (type depends on synergy).</param>
/// <param name="PrimaryDc">Optional DC override for the primary check.</param>
/// <param name="SecondaryDc">Optional DC override for the secondary check.</param>
/// <seealso cref="SynergyType"/>
/// <seealso cref="CombinedCheckResult"/>
public readonly record struct CombinedCheckContext(
    string PlayerId,
    SynergyType SynergyType,
    object? PrimaryContext,
    object? SecondaryContext,
    int? PrimaryDc = null,
    int? SecondaryDc = null)
{
    // ═══════════════════════════════════════════════════════════════════════════
    // TYPED CONTEXT ACCESSORS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the primary context as NavigationContext.
    /// </summary>
    /// <remarks>
    /// Use this property when handling <see cref="Enums.SynergyType.FindHiddenPath"/> synergy.
    /// Returns null if the context is not a NavigationContext.
    /// </remarks>
    public NavigationContext? AsNavigationContext =>
        PrimaryContext as NavigationContext?;

    /// <summary>
    /// Gets the primary context as ForagingContext.
    /// </summary>
    /// <remarks>
    /// Use this property when handling <see cref="Enums.SynergyType.FindAndLoot"/> synergy.
    /// Returns null if the context is not a ForagingContext.
    /// </remarks>
    public ForagingContext? AsForagingContext =>
        PrimaryContext as ForagingContext?;

    // ═══════════════════════════════════════════════════════════════════════════
    // VALIDATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether the primary context is provided.
    /// </summary>
    public bool HasPrimaryContext => PrimaryContext is not null;

    /// <summary>
    /// Gets whether the secondary context is provided.
    /// </summary>
    public bool HasSecondaryContext => SecondaryContext is not null;

    /// <summary>
    /// Gets whether a primary DC override is specified.
    /// </summary>
    public bool HasPrimaryDc => PrimaryDc.HasValue;

    /// <summary>
    /// Gets whether a secondary DC override is specified.
    /// </summary>
    public bool HasSecondaryDc => SecondaryDc.HasValue;

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a context for Find Hidden Path synergy.
    /// </summary>
    /// <param name="playerId">The player initiating the check.</param>
    /// <param name="navigationContext">Context for the navigation check.</param>
    /// <param name="traversalContext">Context for the traversal check (optional).</param>
    /// <param name="primaryDc">DC override for navigation (optional).</param>
    /// <param name="secondaryDc">DC override for traversal (optional).</param>
    /// <returns>A combined check context for Find Hidden Path.</returns>
    public static CombinedCheckContext ForFindHiddenPath(
        string playerId,
        NavigationContext navigationContext,
        object? traversalContext = null,
        int? primaryDc = null,
        int? secondaryDc = null) =>
        new(playerId, Enums.SynergyType.FindHiddenPath, navigationContext, traversalContext, primaryDc, secondaryDc);

    /// <summary>
    /// Creates a context for Track to Lair synergy.
    /// </summary>
    /// <param name="playerId">The player initiating the check.</param>
    /// <param name="trackingContext">Context for the tracking check (can be any object).</param>
    /// <param name="entryContext">Context for the lair entry check (optional).</param>
    /// <param name="primaryDc">DC override for tracking (optional).</param>
    /// <param name="secondaryDc">DC override for entry (optional).</param>
    /// <returns>A combined check context for Track to Lair.</returns>
    public static CombinedCheckContext ForTrackToLair(
        string playerId,
        object? trackingContext,
        object? entryContext = null,
        int? primaryDc = null,
        int? secondaryDc = null) =>
        new(playerId, Enums.SynergyType.TrackToLair, trackingContext, entryContext, primaryDc, secondaryDc);

    /// <summary>
    /// Creates a context for Avoid Patrol synergy.
    /// </summary>
    /// <param name="playerId">The player initiating the check.</param>
    /// <param name="detectionContext">Context for the hazard detection check (can be any object).</param>
    /// <param name="stealthContext">Context for the stealth check (optional).</param>
    /// <param name="primaryDc">DC override for detection (optional).</param>
    /// <param name="secondaryDc">DC override for stealth (optional).</param>
    /// <returns>A combined check context for Avoid Patrol.</returns>
    public static CombinedCheckContext ForAvoidPatrol(
        string playerId,
        object? detectionContext,
        object? stealthContext = null,
        int? primaryDc = null,
        int? secondaryDc = null) =>
        new(playerId, Enums.SynergyType.AvoidPatrol, detectionContext, stealthContext, primaryDc, secondaryDc);

    /// <summary>
    /// Creates a context for Find and Loot synergy.
    /// </summary>
    /// <param name="playerId">The player initiating the check.</param>
    /// <param name="foragingContext">Context for the foraging check.</param>
    /// <param name="lockpickContext">Context for the lockpick check (optional).</param>
    /// <param name="primaryDc">DC override for foraging (optional).</param>
    /// <param name="secondaryDc">DC override for lockpicking (optional).</param>
    /// <returns>A combined check context for Find and Loot.</returns>
    public static CombinedCheckContext ForFindAndLoot(
        string playerId,
        ForagingContext foragingContext,
        object? lockpickContext = null,
        int? primaryDc = null,
        int? secondaryDc = null) =>
        new(playerId, Enums.SynergyType.FindAndLoot, foragingContext, lockpickContext, primaryDc, secondaryDc);

    /// <summary>
    /// Creates a simple context with DC overrides only (no detailed context objects).
    /// </summary>
    /// <param name="playerId">The player initiating the check.</param>
    /// <param name="synergyType">The synergy type to execute.</param>
    /// <param name="primaryDc">DC for the primary check.</param>
    /// <param name="secondaryDc">DC for the secondary check.</param>
    /// <returns>A combined check context with DC values only.</returns>
    public static CombinedCheckContext WithDcs(
        string playerId,
        SynergyType synergyType,
        int primaryDc,
        int secondaryDc) =>
        new(playerId, synergyType, null, null, primaryDc, secondaryDc);

    // ═══════════════════════════════════════════════════════════════════════════
    // STRING FORMATTING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a display string for this context.
    /// </summary>
    /// <returns>A string describing the combined check context.</returns>
    public override string ToString() =>
        $"Combined Check: {SynergyType} for player {PlayerId}";
}
