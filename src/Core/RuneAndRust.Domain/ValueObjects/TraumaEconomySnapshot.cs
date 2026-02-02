// ═══════════════════════════════════════════════════════════════════════════════
// TraumaEconomySnapshot.cs
// Immutable point-in-time capture of a character's trauma economy state.
// Used for logging, state comparison, and history tracking.
// Version: 0.18.5a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

/// <summary>
/// Represents an immutable point-in-time snapshot of a character's trauma economy state.
/// </summary>
/// <remarks>
/// <para>
/// TraumaEconomySnapshot captures all relevant trauma economy values at a specific moment.
/// Unlike <see cref="TraumaEconomyState"/>, which is a live aggregation of component states,
/// the snapshot is a frozen record designed for:
/// </para>
/// <list type="bullet">
///   <item><description>Logging and audit trails</description></item>
///   <item><description>State comparison (before/after combat, rest, etc.)</description></item>
///   <item><description>Serialization and persistence</description></item>
///   <item><description>UI delta display ("Stress increased by 15")</description></item>
/// </list>
/// <para>
/// <strong>Immutability:</strong> As a <c>readonly record struct</c>, TraumaEconomySnapshot
/// is fully immutable and has value-based equality. Two snapshots with identical values
/// are considered equal regardless of reference.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Capture state before combat
/// var before = TraumaEconomySnapshot.Create(traumaState);
/// 
/// // ... combat occurs ...
/// 
/// // Capture state after combat
/// var after = TraumaEconomySnapshot.Create(traumaState);
/// 
/// // Compare
/// int stressDelta = after.Stress - before.Stress;
/// Console.WriteLine($"Stress changed by {stressDelta}");
/// </code>
/// </example>
/// <seealso cref="TraumaEconomyState"/>
/// <seealso cref="WarningLevel"/>
public readonly record struct TraumaEconomySnapshot
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Properties — Identity
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the character ID this snapshot belongs to.
    /// </summary>
    public Guid CharacterId { get; init; }

    /// <summary>
    /// Gets the timestamp when this snapshot was captured.
    /// </summary>
    public DateTime CapturedAt { get; init; }

    // ═══════════════════════════════════════════════════════════════════════════
    // Properties — Stress System
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the stress value at time of capture (0-100).
    /// </summary>
    public int Stress { get; init; }

    /// <summary>
    /// Gets the stress threshold tier at time of capture.
    /// </summary>
    public StressThreshold StressThreshold { get; init; }

    // ═══════════════════════════════════════════════════════════════════════════
    // Properties — Corruption System
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the corruption value at time of capture (0-100).
    /// </summary>
    public int Corruption { get; init; }

    /// <summary>
    /// Gets the corruption stage at time of capture.
    /// </summary>
    public CorruptionStage CorruptionStage { get; init; }

    // ═══════════════════════════════════════════════════════════════════════════
    // Properties — CPS System
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the CPS stage at time of capture.
    /// </summary>
    public CpsStage CpsStage { get; init; }

    // ═══════════════════════════════════════════════════════════════════════════
    // Properties — Traumas
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the count of active traumas at time of capture.
    /// </summary>
    public int TraumaCount { get; init; }

    /// <summary>
    /// Gets the list of active trauma definition IDs at time of capture.
    /// </summary>
    /// <remarks>
    /// Captures the <see cref="CharacterTrauma.TraumaDefinitionId"/> for each active trauma.
    /// </remarks>
    public IReadOnlyList<string> TraumaIds { get; init; }

    // ═══════════════════════════════════════════════════════════════════════════
    // Properties — Specialization Resource
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the specialization type at time of capture.
    /// </summary>
    /// <remarks>
    /// Examples: "rage", "momentum", "coherence", or null if no specialization.
    /// </remarks>
    public string? SpecializationType { get; init; }

    /// <summary>
    /// Gets the specialization resource current value at time of capture.
    /// </summary>
    /// <remarks>
    /// The interpretation depends on <see cref="SpecializationType"/>:
    /// <list type="bullet">
    ///   <item><description>Rage: Current rage value (0-100)</description></item>
    ///   <item><description>Momentum: Current stacks (0-10)</description></item>
    ///   <item><description>Coherence: Current coherence (0-100)</description></item>
    /// </list>
    /// Null if no specialization is active.
    /// </remarks>
    public int? SpecializationValue { get; init; }

    /// <summary>
    /// Gets the specialization resource threshold/tier at time of capture.
    /// </summary>
    /// <remarks>
    /// The interpretation depends on <see cref="SpecializationType"/>:
    /// <list type="bullet">
    ///   <item><description>Rage: RageThreshold ordinal (0-4)</description></item>
    ///   <item><description>Momentum: MomentumThreshold ordinal (0-4)</description></item>
    ///   <item><description>Coherence: CoherenceThreshold ordinal (0-3)</description></item>
    /// </list>
    /// Null if no specialization is active.
    /// </remarks>
    public int? SpecializationThreshold { get; init; }

    // ═══════════════════════════════════════════════════════════════════════════
    // Properties — Derived/Computed (captured at snapshot time)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the warning level at time of capture.
    /// </summary>
    public WarningLevel WarningLevel { get; init; }

    /// <summary>
    /// Gets whether the state was critical (any system ≥80) at time of capture.
    /// </summary>
    public bool WasCritical { get; init; }

    /// <summary>
    /// Gets whether the state was terminal (any system at 100) at time of capture.
    /// </summary>
    public bool WasTerminal { get; init; }

    // ═══════════════════════════════════════════════════════════════════════════
    // Factory Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new snapshot from the current trauma economy state.
    /// </summary>
    /// <param name="state">The current trauma economy state to capture.</param>
    /// <param name="capturedAt">
    /// Optional timestamp. If null, uses <see cref="DateTime.UtcNow"/>.
    /// </param>
    /// <returns>A new immutable snapshot capturing all state values.</returns>
    /// <example>
    /// <code>
    /// var state = traumaEconomyService.GetState(characterId);
    /// var snapshot = TraumaEconomySnapshot.Create(state);
    /// 
    /// // With explicit timestamp
    /// var historicalSnapshot = TraumaEconomySnapshot.Create(state, eventTime);
    /// </code>
    /// </example>
    public static TraumaEconomySnapshot Create(
        TraumaEconomyState state,
        DateTime? capturedAt = null)
    {
        ArgumentNullException.ThrowIfNull(state);

        // Extract specialization values (polymorphic handling)
        var (specValue, specThreshold) = ExtractSpecializationValues(
            state.SpecializationResource,
            state.SpecializationType);

        return new TraumaEconomySnapshot
        {
            CharacterId = state.CharacterId,
            CapturedAt = capturedAt ?? DateTime.UtcNow,

            // Stress system
            Stress = state.StressState.CurrentStress,
            StressThreshold = state.StressState.Threshold,

            // Corruption system
            Corruption = state.CorruptionState.CurrentCorruption,
            CorruptionStage = state.CorruptionState.Stage,

            // CPS system
            CpsStage = state.CpsState.Stage,

            // Traumas
            TraumaCount = state.TraumaCount,
            TraumaIds = state.ActiveTraumaIds,

            // Specialization
            SpecializationType = state.SpecializationType,
            SpecializationValue = specValue,
            SpecializationThreshold = specThreshold,

            // Derived
            WarningLevel = state.GetWarningLevel(),
            WasCritical = state.IsCriticalState,
            WasTerminal = state.IsTerminalState
        };
    }

    /// <summary>
    /// Gets a default empty snapshot for comparison purposes.
    /// </summary>
    /// <remarks>
    /// All numeric values are 0, all thresholds at minimum, no traumas.
    /// Useful as a baseline for delta calculations.
    /// </remarks>
    public static TraumaEconomySnapshot Empty => new()
    {
        CharacterId = Guid.Empty,
        CapturedAt = DateTime.MinValue,
        Stress = 0,
        StressThreshold = Enums.StressThreshold.Calm,
        Corruption = 0,
        CorruptionStage = Enums.CorruptionStage.Uncorrupted,
        CpsStage = Enums.CpsStage.None,
        TraumaCount = 0,
        TraumaIds = new List<string>().AsReadOnly(),
        SpecializationType = null,
        SpecializationValue = null,
        SpecializationThreshold = null,
        WarningLevel = Enums.WarningLevel.None,
        WasCritical = false,
        WasTerminal = false
    };

    // ═══════════════════════════════════════════════════════════════════════════
    // Validation
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Validates that the snapshot contains consistent data.
    /// </summary>
    /// <returns>
    /// <c>true</c> if all values are within valid ranges; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Checks performed:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>CharacterId is not empty (unless Empty snapshot)</description></item>
    ///   <item><description>Stress is in range [0, 100]</description></item>
    ///   <item><description>Corruption is in range [0, 100]</description></item>
    ///   <item><description>TraumaCount matches TraumaIds length</description></item>
    ///   <item><description>TraumaIds is not null</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var snapshot = TraumaEconomySnapshot.Create(state);
    /// if (!snapshot.IsValid())
    /// {
    ///     logger.LogWarning("Invalid snapshot detected");
    /// }
    /// </code>
    /// </example>
    public bool IsValid()
    {
        // Allow Empty snapshot as valid
        if (CharacterId == Guid.Empty && CapturedAt == DateTime.MinValue)
        {
            return true;
        }

        // CharacterId must be set
        if (CharacterId == Guid.Empty)
        {
            return false;
        }

        // Stress in valid range
        if (Stress < 0 || Stress > 100)
        {
            return false;
        }

        // Corruption in valid range
        if (Corruption < 0 || Corruption > 100)
        {
            return false;
        }

        // TraumaIds must not be null
        if (TraumaIds == null)
        {
            return false;
        }

        // TraumaCount should match TraumaIds length
        if (TraumaCount != TraumaIds.Count)
        {
            return false;
        }

        return true;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Display
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a string representation of the snapshot for debugging and logging.
    /// </summary>
    /// <returns>
    /// A formatted string showing key snapshot values.
    /// </returns>
    /// <example>
    /// <code>
    /// var snapshot = TraumaEconomySnapshot.Create(state);
    /// Console.WriteLine(snapshot.ToString());
    /// // Output: "Snapshot[2026-02-02T08:30:00Z]: Stress=45 Corruption=30 CPS=None Traumas=2"
    /// </code>
    /// </example>
    public override string ToString() =>
        $"Snapshot[{CapturedAt:yyyy-MM-ddTHH:mm:ssZ}]: " +
        $"Stress={Stress} Corruption={Corruption} CPS={CpsStage} Traumas={TraumaCount}";

    // ═══════════════════════════════════════════════════════════════════════════
    // Private Helpers
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Extracts numeric values from polymorphic specialization resource.
    /// </summary>
    private static (int? Value, int? Threshold) ExtractSpecializationValues(
        object? resource,
        string? type)
    {
        if (resource == null || string.IsNullOrEmpty(type))
        {
            return (null, null);
        }

        // Type-based extraction using reflection-free pattern matching
        // The actual RageState/MomentumState/CoherenceState types have
        // CurrentX and Threshold properties. We use dynamic dispatch here
        // for simplicity; in production, use interface or visitor pattern.
        return type.ToLowerInvariant() switch
        {
            "rage" => ExtractFromDynamic(resource, "CurrentRage", "Threshold"),
            "momentum" => ExtractFromDynamic(resource, "CurrentStacks", "Threshold"),
            "coherence" => ExtractFromDynamic(resource, "CurrentCoherence", "Threshold"),
            _ => (null, null)
        };
    }

    /// <summary>
    /// Extracts values from a resource object using property names.
    /// </summary>
    private static (int? Value, int? Threshold) ExtractFromDynamic(
        object resource,
        string valueProperty,
        string thresholdProperty)
    {
        try
        {
            var type = resource.GetType();

            var valueProp = type.GetProperty(valueProperty);
            var thresholdProp = type.GetProperty(thresholdProperty);

            int? value = valueProp?.GetValue(resource) as int?
                ?? (valueProp?.GetValue(resource) is int v ? v : null);

            int? threshold = null;
            if (thresholdProp?.GetValue(resource) is Enum e)
            {
                threshold = Convert.ToInt32(e);
            }

            return (value, threshold);
        }
        catch
        {
            return (null, null);
        }
    }
}
