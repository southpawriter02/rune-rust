// ═══════════════════════════════════════════════════════════════════════════════
// AttributeAllocationState.cs
// Value object tracking the complete state of attribute allocation during
// character creation, including current attribute values, points spent/remaining,
// allocation mode, and archetype selection for Simple mode.
// Version: 0.17.2b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

using Microsoft.Extensions.Logging;
using RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the complete state of attribute allocation during character creation.
/// </summary>
/// <remarks>
/// <para>
/// AttributeAllocationState is an immutable value object that captures all information
/// about a character's attribute allocation progress. It tracks the current mode,
/// individual attribute values, and point expenditure.
/// </para>
/// <para>
/// Two allocation modes are supported:
/// </para>
/// <list type="bullet">
///   <item>
///     <description>
///       <see cref="AttributeAllocationMode.Simple"/>: Attributes are auto-set from an
///       archetype's recommended build. Manual adjustment is disabled.
///     </description>
///   </item>
///   <item>
///     <description>
///       <see cref="AttributeAllocationMode.Advanced"/>: All attributes start at 1 and
///       the player manually allocates points via the point-buy system.
///     </description>
///   </item>
/// </list>
/// <para>
/// State transitions are performed via immutable factory methods and <c>with</c> expressions,
/// ensuring thread safety and predictable behavior. All mutation methods return new instances.
/// </para>
/// <para>
/// The state tracks five core attributes (Might, Finesse, Wits, Will, Sturdiness),
/// each with a valid range of 1-10, plus point accounting (spent, remaining, total).
/// </para>
/// </remarks>
/// <seealso cref="AttributeAllocationMode"/>
/// <seealso cref="CoreAttribute"/>
public readonly record struct AttributeAllocationState
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE FIELDS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Logger for detailed diagnostic output during state creation and transitions.
    /// </summary>
    private static ILogger<AttributeAllocationState>? _logger;

    // ═══════════════════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the current allocation mode (Simple or Advanced).
    /// </summary>
    /// <value>
    /// <see cref="AttributeAllocationMode.Simple"/> when using archetype-recommended builds,
    /// or <see cref="AttributeAllocationMode.Advanced"/> when using point-buy customization.
    /// </value>
    public AttributeAllocationMode Mode { get; init; }

    /// <summary>
    /// Gets the current Might attribute value.
    /// </summary>
    /// <value>An integer between 1 and 10 representing physical power and raw strength.</value>
    /// <remarks>
    /// Might affects melee damage, carrying capacity, and physical feats.
    /// Associated skills: Combat, Athletics, Intimidation.
    /// </remarks>
    public int CurrentMight { get; init; }

    /// <summary>
    /// Gets the current Finesse attribute value.
    /// </summary>
    /// <value>An integer between 1 and 10 representing agility and precision.</value>
    /// <remarks>
    /// Finesse affects ranged accuracy, initiative, and evasion.
    /// Associated skills: Stealth, Acrobatics, Lockpicking.
    /// </remarks>
    public int CurrentFinesse { get; init; }

    /// <summary>
    /// Gets the current Wits attribute value.
    /// </summary>
    /// <value>An integer between 1 and 10 representing perception and knowledge.</value>
    /// <remarks>
    /// Wits affects perception, crafting quality, and provides secondary Aether capacity.
    /// Associated skills: Lore, Craft, Investigation.
    /// </remarks>
    public int CurrentWits { get; init; }

    /// <summary>
    /// Gets the current Will attribute value.
    /// </summary>
    /// <value>An integer between 1 and 10 representing mental fortitude and magical affinity.</value>
    /// <remarks>
    /// Will is the primary determinant of Aether Pool and mental resistance.
    /// Associated skills: Rhetoric, Concentration, Willpower.
    /// </remarks>
    public int CurrentWill { get; init; }

    /// <summary>
    /// Gets the current Sturdiness attribute value.
    /// </summary>
    /// <value>An integer between 1 and 10 representing endurance and physical resilience.</value>
    /// <remarks>
    /// Sturdiness is the primary determinant of maximum HP and damage resistance (Soak).
    /// Associated skills: Endurance, Survival, Fortitude.
    /// </remarks>
    public int CurrentSturdiness { get; init; }

    /// <summary>
    /// Gets the total points spent on attribute allocation.
    /// </summary>
    /// <value>
    /// The cumulative cost of all attribute increases from the base value of 1.
    /// In Simple mode, this equals the archetype's total point cost.
    /// </value>
    public int PointsSpent { get; init; }

    /// <summary>
    /// Gets the points remaining to spend on attributes.
    /// </summary>
    /// <value>
    /// The difference between <see cref="TotalPoints"/> and <see cref="PointsSpent"/>.
    /// When zero, the allocation is complete.
    /// </value>
    public int PointsRemaining { get; init; }

    /// <summary>
    /// Gets the total point pool available for attribute allocation.
    /// </summary>
    /// <value>
    /// Typically 15 points for most archetypes, or 14 for the Adept archetype.
    /// </value>
    public int TotalPoints { get; init; }

    /// <summary>
    /// Gets the selected archetype ID for Simple mode.
    /// </summary>
    /// <value>
    /// A lowercase archetype identifier (e.g., "warrior", "mystic") when in Simple mode,
    /// or <c>null</c> when in Advanced mode or when no archetype has been selected.
    /// </value>
    /// <remarks>
    /// This property is only meaningful in <see cref="AttributeAllocationMode.Simple"/> mode.
    /// In Advanced mode, it is always <c>null</c>.
    /// </remarks>
    public string? SelectedArchetypeId { get; init; }

    // ═══════════════════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether all available points have been allocated.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="PointsRemaining"/> equals zero;
    /// otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// A complete allocation is required before the player can proceed to
    /// the next step in character creation. In Simple mode, allocation is
    /// always complete after selecting an archetype.
    /// </remarks>
    public bool IsComplete => PointsRemaining == 0;

    /// <summary>
    /// Gets whether manual attribute adjustment is allowed in the current mode.
    /// </summary>
    /// <value>
    /// <c>true</c> if the mode is <see cref="AttributeAllocationMode.Advanced"/>;
    /// otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// In Simple mode, attributes are set by the archetype's recommended build
    /// and the player cannot manually adjust individual values. The +/- controls
    /// should be hidden or disabled in the UI when this returns <c>false</c>.
    /// </remarks>
    public bool AllowsManualAdjustment => Mode == AttributeAllocationMode.Advanced;

    /// <summary>
    /// Gets whether an archetype has been selected (relevant in Simple mode).
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="SelectedArchetypeId"/> is not null or whitespace;
    /// otherwise, <c>false</c>.
    /// </value>
    public bool HasSelectedArchetype => !string.IsNullOrWhiteSpace(SelectedArchetypeId);

    // ═══════════════════════════════════════════════════════════════════════════
    // ATTRIBUTE ACCESS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the current value for a specific core attribute.
    /// </summary>
    /// <param name="attribute">The core attribute to retrieve.</param>
    /// <returns>The current integer value (1-10) for the specified attribute.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="attribute"/> is not a valid <see cref="CoreAttribute"/> value.
    /// </exception>
    /// <example>
    /// <code>
    /// var state = AttributeAllocationState.CreateAdvancedDefault(15);
    /// var might = state.GetAttributeValue(CoreAttribute.Might); // returns 1
    /// </code>
    /// </example>
    public int GetAttributeValue(CoreAttribute attribute) => attribute switch
    {
        CoreAttribute.Might => CurrentMight,
        CoreAttribute.Finesse => CurrentFinesse,
        CoreAttribute.Wits => CurrentWits,
        CoreAttribute.Will => CurrentWill,
        CoreAttribute.Sturdiness => CurrentSturdiness,
        _ => throw new ArgumentOutOfRangeException(
            nameof(attribute),
            attribute,
            $"Unknown CoreAttribute value: {attribute}")
    };

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates the default initial state for Advanced (point-buy) mode.
    /// </summary>
    /// <param name="totalPoints">
    /// The total point pool available. Defaults to 15.
    /// Use 14 for the Adept archetype.
    /// </param>
    /// <param name="logger">Optional logger for diagnostic output during creation.</param>
    /// <returns>
    /// A new <see cref="AttributeAllocationState"/> with all attributes set to 1
    /// and all points available for spending.
    /// </returns>
    /// <example>
    /// <code>
    /// // Standard 15-point allocation
    /// var state = AttributeAllocationState.CreateAdvancedDefault(15);
    /// // state.CurrentMight == 1, state.PointsRemaining == 15
    ///
    /// // Adept allocation with 14 points
    /// var adeptState = AttributeAllocationState.CreateAdvancedDefault(14);
    /// // adeptState.PointsRemaining == 14
    /// </code>
    /// </example>
    /// <remarks>
    /// <para>
    /// This factory method creates the starting state for Advanced mode where
    /// all five attributes are at their minimum value (1) and the full point
    /// pool is available for allocation.
    /// </para>
    /// </remarks>
    public static AttributeAllocationState CreateAdvancedDefault(
        int totalPoints = 15,
        ILogger<AttributeAllocationState>? logger = null)
    {
        _logger = logger;

        _logger?.LogDebug(
            "Creating Advanced mode default state. TotalPoints={TotalPoints}",
            totalPoints);

        var state = new AttributeAllocationState
        {
            Mode = AttributeAllocationMode.Advanced,
            CurrentMight = 1,
            CurrentFinesse = 1,
            CurrentWits = 1,
            CurrentWill = 1,
            CurrentSturdiness = 1,
            PointsSpent = 0,
            PointsRemaining = totalPoints,
            TotalPoints = totalPoints,
            SelectedArchetypeId = null
        };

        _logger?.LogInformation(
            "Created Advanced mode allocation state. TotalPoints={TotalPoints}, " +
            "AllAttributesAt=1, PointsRemaining={PointsRemaining}",
            totalPoints,
            totalPoints);

        return state;
    }

    /// <summary>
    /// Creates a state from an archetype's recommended build (Simple mode).
    /// </summary>
    /// <param name="archetypeId">
    /// The archetype identifier (e.g., "warrior", "mystic").
    /// Will be normalized to lowercase.
    /// </param>
    /// <param name="might">The recommended Might value (1-10).</param>
    /// <param name="finesse">The recommended Finesse value (1-10).</param>
    /// <param name="wits">The recommended Wits value (1-10).</param>
    /// <param name="will">The recommended Will value (1-10).</param>
    /// <param name="sturdiness">The recommended Sturdiness value (1-10).</param>
    /// <param name="totalPoints">The total point cost of this build (typically 14-15).</param>
    /// <param name="logger">Optional logger for diagnostic output during creation.</param>
    /// <returns>
    /// A new <see cref="AttributeAllocationState"/> in Simple mode with attributes
    /// set to the archetype's recommended values and all points spent.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="archetypeId"/> is null, empty, or whitespace.
    /// </exception>
    /// <example>
    /// <code>
    /// // Create Warrior recommended build
    /// var warrior = AttributeAllocationState.CreateFromRecommendedBuild(
    ///     "warrior", might: 4, finesse: 3, wits: 2, will: 2, sturdiness: 4, totalPoints: 15);
    /// // warrior.CurrentMight == 4, warrior.IsComplete == true
    ///
    /// // Create Adept recommended build (14 points)
    /// var adept = AttributeAllocationState.CreateFromRecommendedBuild(
    ///     "adept", might: 3, finesse: 3, wits: 3, will: 2, sturdiness: 3, totalPoints: 14);
    /// </code>
    /// </example>
    /// <remarks>
    /// <para>
    /// This factory method creates a state where attributes are pre-set to an archetype's
    /// recommended values. All points are considered spent, and manual adjustment is disabled.
    /// The archetype ID is normalized to lowercase for consistent matching.
    /// </para>
    /// </remarks>
    public static AttributeAllocationState CreateFromRecommendedBuild(
        string archetypeId,
        int might,
        int finesse,
        int wits,
        int will,
        int sturdiness,
        int totalPoints,
        ILogger<AttributeAllocationState>? logger = null)
    {
        _logger = logger;

        _logger?.LogDebug(
            "Creating Simple mode state from recommended build. ArchetypeId='{ArchetypeId}', " +
            "Might={Might}, Finesse={Finesse}, Wits={Wits}, Will={Will}, Sturdiness={Sturdiness}, " +
            "TotalPoints={TotalPoints}",
            archetypeId,
            might,
            finesse,
            wits,
            will,
            sturdiness,
            totalPoints);

        // Validate archetype ID is not null or whitespace
        ArgumentException.ThrowIfNullOrWhiteSpace(archetypeId, nameof(archetypeId));

        // Normalize archetype ID to lowercase for consistent matching
        var normalizedArchetypeId = archetypeId.ToLowerInvariant();

        _logger?.LogDebug(
            "Validation passed for recommended build. " +
            "NormalizedArchetypeId='{NormalizedArchetypeId}'",
            normalizedArchetypeId);

        var state = new AttributeAllocationState
        {
            Mode = AttributeAllocationMode.Simple,
            CurrentMight = might,
            CurrentFinesse = finesse,
            CurrentWits = wits,
            CurrentWill = will,
            CurrentSturdiness = sturdiness,
            PointsSpent = totalPoints,
            PointsRemaining = 0,
            TotalPoints = totalPoints,
            SelectedArchetypeId = normalizedArchetypeId
        };

        _logger?.LogInformation(
            "Created Simple mode allocation state from recommended build. " +
            "ArchetypeId='{ArchetypeId}', M:{Might} F:{Finesse} Wi:{Wits} Wl:{Will} S:{Sturdiness}, " +
            "TotalPoints={TotalPoints}, IsComplete={IsComplete}",
            normalizedArchetypeId,
            might,
            finesse,
            wits,
            will,
            sturdiness,
            totalPoints,
            state.IsComplete);

        return state;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // STATE TRANSITION METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new state with an updated attribute value (Advanced mode only).
    /// </summary>
    /// <param name="attribute">The core attribute to modify.</param>
    /// <param name="newValue">The new value for the attribute (1-10).</param>
    /// <param name="pointDelta">
    /// The change in points spent (positive for increases, negative for decreases).
    /// </param>
    /// <returns>
    /// A new <see cref="AttributeAllocationState"/> with the specified attribute
    /// updated and points recalculated.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when attempting to modify attributes in
    /// <see cref="AttributeAllocationMode.Simple"/> mode.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="attribute"/> is not a valid <see cref="CoreAttribute"/> value.
    /// </exception>
    /// <example>
    /// <code>
    /// var state = AttributeAllocationState.CreateAdvancedDefault(15);
    /// // Increase Might from 1 to 4 (costs 3 points)
    /// var updated = state.WithAttributeValue(CoreAttribute.Might, 4, 3);
    /// // updated.CurrentMight == 4, updated.PointsRemaining == 12
    /// </code>
    /// </example>
    /// <remarks>
    /// <para>
    /// This method enforces the mode restriction: attributes can only be modified
    /// in Advanced mode. In Simple mode, attributes are locked to the archetype's
    /// recommended build. The caller is responsible for calculating the correct
    /// point delta based on the point-buy cost system (v0.17.2c).
    /// </para>
    /// </remarks>
    public AttributeAllocationState WithAttributeValue(
        CoreAttribute attribute,
        int newValue,
        int pointDelta)
    {
        _logger?.LogDebug(
            "Attempting to modify attribute. Attribute={Attribute}, " +
            "NewValue={NewValue}, PointDelta={PointDelta}, CurrentMode={Mode}",
            attribute,
            newValue,
            pointDelta,
            Mode);

        // Enforce mode restriction: only Advanced mode allows manual modification
        if (Mode != AttributeAllocationMode.Advanced)
        {
            _logger?.LogWarning(
                "Cannot modify attributes in Simple mode. " +
                "Attribute={Attribute}, CurrentMode={Mode}. " +
                "Switch to Advanced mode first.",
                attribute,
                Mode);

            throw new InvalidOperationException(
                "Cannot modify attributes in Simple mode. " +
                "Switch to Advanced mode to manually adjust attribute values.");
        }

        var oldValue = GetAttributeValue(attribute);

        var result = attribute switch
        {
            CoreAttribute.Might => this with
            {
                CurrentMight = newValue,
                PointsSpent = PointsSpent + pointDelta,
                PointsRemaining = PointsRemaining - pointDelta
            },
            CoreAttribute.Finesse => this with
            {
                CurrentFinesse = newValue,
                PointsSpent = PointsSpent + pointDelta,
                PointsRemaining = PointsRemaining - pointDelta
            },
            CoreAttribute.Wits => this with
            {
                CurrentWits = newValue,
                PointsSpent = PointsSpent + pointDelta,
                PointsRemaining = PointsRemaining - pointDelta
            },
            CoreAttribute.Will => this with
            {
                CurrentWill = newValue,
                PointsSpent = PointsSpent + pointDelta,
                PointsRemaining = PointsRemaining - pointDelta
            },
            CoreAttribute.Sturdiness => this with
            {
                CurrentSturdiness = newValue,
                PointsSpent = PointsSpent + pointDelta,
                PointsRemaining = PointsRemaining - pointDelta
            },
            _ => throw new ArgumentOutOfRangeException(
                nameof(attribute),
                attribute,
                $"Unknown CoreAttribute value: {attribute}")
        };

        _logger?.LogDebug(
            "Modified attribute. Attribute={Attribute}, " +
            "OldValue={OldValue}, NewValue={NewValue}, PointDelta={PointDelta}, " +
            "PointsSpent={PointsSpent}, PointsRemaining={PointsRemaining}",
            attribute,
            oldValue,
            newValue,
            pointDelta,
            result.PointsSpent,
            result.PointsRemaining);

        return result;
    }

    /// <summary>
    /// Switches the current state to Advanced mode, preserving current attribute values.
    /// </summary>
    /// <param name="pointsSpent">
    /// The total points spent on the current attribute configuration.
    /// This should be calculated from the point-buy cost table.
    /// </param>
    /// <returns>
    /// A new <see cref="AttributeAllocationState"/> in Advanced mode with
    /// current attribute values preserved and manual adjustment enabled.
    /// </returns>
    /// <example>
    /// <code>
    /// // Start with Warrior recommended build
    /// var simple = AttributeAllocationState.CreateFromRecommendedBuild(
    ///     "warrior", 4, 3, 2, 2, 4, 15);
    ///
    /// // Switch to Advanced mode, keeping current values
    /// var advanced = simple.SwitchToAdvanced(15);
    /// // advanced.Mode == Advanced, advanced.CurrentMight == 4
    /// // advanced.SelectedArchetypeId == null, advanced.AllowsManualAdjustment == true
    /// </code>
    /// </example>
    /// <remarks>
    /// <para>
    /// When switching from Simple to Advanced mode:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Current attribute values are preserved</description></item>
    ///   <item><description>Mode changes to Advanced</description></item>
    ///   <item><description>Selected archetype ID is cleared (set to null)</description></item>
    ///   <item><description>Points are recalculated based on the provided pointsSpent value</description></item>
    ///   <item><description>Manual adjustment becomes enabled</description></item>
    /// </list>
    /// </remarks>
    public AttributeAllocationState SwitchToAdvanced(int pointsSpent)
    {
        _logger?.LogDebug(
            "Switching to Advanced mode. CurrentMode={CurrentMode}, " +
            "CurrentArchetype='{CurrentArchetype}', PointsSpent={PointsSpent}",
            Mode,
            SelectedArchetypeId,
            pointsSpent);

        var result = this with
        {
            Mode = AttributeAllocationMode.Advanced,
            PointsSpent = pointsSpent,
            PointsRemaining = TotalPoints - pointsSpent,
            SelectedArchetypeId = null
        };

        _logger?.LogInformation(
            "Switched to Advanced mode. " +
            "M:{Might} F:{Finesse} Wi:{Wits} Wl:{Will} S:{Sturdiness}, " +
            "PointsSpent={PointsSpent}, PointsRemaining={PointsRemaining}",
            result.CurrentMight,
            result.CurrentFinesse,
            result.CurrentWits,
            result.CurrentWill,
            result.CurrentSturdiness,
            result.PointsSpent,
            result.PointsRemaining);

        return result;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DEBUGGING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a formatted string representation of the allocation state.
    /// </summary>
    /// <returns>
    /// A string in the format "[Mode] M:x F:x Wi:x Wl:x S:x (remaining/total remaining)"
    /// (e.g., "[Advanced] M:4 F:3 Wi:2 Wl:2 S:4 (0/15 remaining)").
    /// </returns>
    public override string ToString() =>
        $"[{Mode}] M:{CurrentMight} F:{CurrentFinesse} Wi:{CurrentWits} " +
        $"Wl:{CurrentWill} S:{CurrentSturdiness} ({PointsRemaining}/{TotalPoints} remaining)";
}
