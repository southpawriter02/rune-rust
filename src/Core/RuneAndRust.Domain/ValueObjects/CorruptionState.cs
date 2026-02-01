// ═══════════════════════════════════════════════════════════════════════════════
// CorruptionState.cs
// Immutable value object representing a character's current Runic Blight
// Corruption state. Encapsulates the corruption value (0-100) and derives all
// dependent properties: corruption stage, percentage to consumption, mutation
// risk flags, and mutation check trigger. Provides factory methods for creation
// and a static DetermineStage method for threshold resolution.
// Parallels StressState from the Psychic Stress system (v0.18.0a).
// Version: 0.18.1a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Represents a character's current Runic Blight Corruption state as an immutable value object.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="CorruptionState"/> encapsulates the corruption value (clamped to 0-100) and
/// derives all dependent gameplay properties from it. The struct is immutable: factory
/// methods return new instances, preserving the value object contract.
/// </para>
/// <para>
/// <strong>Derived Properties:</strong>
/// </para>
/// <list type="bullet">
///   <item>
///     <description>
///       <see cref="Stage"/>: The <see cref="CorruptionStage"/> tier determined
///       by the current corruption value (e.g., 0-19 = Uncorrupted, 20-39 = Tainted).
///     </description>
///   </item>
///   <item>
///     <description>
///       <see cref="PercentageToConsumption"/>: Progress toward being Consumed (0.0-1.0).
///     </description>
///   </item>
///   <item>
///     <description>
///       <see cref="RequiresMutationCheck"/>: Whether a Mutation Check is required
///       (corruption reaches 100).
///     </description>
///   </item>
///   <item>
///     <description>
///       <see cref="HasMutationRisk"/>: Whether the character is at risk of mutation
///       (corruption &gt;= 80, Corrupted stage or higher).
///     </description>
///   </item>
/// </list>
/// <para>
/// <strong>Clamping Behavior:</strong> Corruption values passed to <see cref="Create"/>
/// are clamped to the [0, 100] range. Negative values become 0; values above 100
/// become 100. This ensures the corruption state is always in a valid range without
/// throwing exceptions for overflow/underflow from gameplay calculations.
/// </para>
/// <para>
/// <strong>Corruption vs Stress:</strong> While Psychic Stress represents mental strain
/// and is recoverable through rest, Corruption represents physical/spiritual contamination
/// from Runic Blight exposure. Corruption is near-permanent — recovery is extremely rare
/// and typically requires special rituals or quests. This creates long-term strategic stakes
/// distinct from the tactical pressure of the stress system.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Create corruption state from a value
/// var state = CorruptionState.Create(45);
///
/// // Query derived properties
/// Console.WriteLine($"Stage: {state.Stage}");                       // Infected
/// Console.WriteLine($"Progress: {state.PercentageToConsumption:P0}"); // 45%
/// Console.WriteLine($"Mutation Risk: {state.HasMutationRisk}");     // false
///
/// // Clamping behavior
/// var clamped = CorruptionState.Create(150);
/// // clamped.CurrentCorruption == 100 (clamped)
/// // clamped.RequiresMutationCheck == true
///
/// // Convenience property
/// var pure = CorruptionState.Uncorrupted;
/// // pure.CurrentCorruption == 0
/// // pure.IsUncorrupted == true
/// </code>
/// </example>
/// <seealso cref="CorruptionStage"/>
/// <seealso cref="StressState"/>
public readonly record struct CorruptionState
{
    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTANTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// The minimum valid corruption value (inclusive).
    /// </summary>
    /// <value>0 — represents a character with no accumulated corruption.</value>
    public const int MinCorruption = 0;

    /// <summary>
    /// The maximum valid corruption value (inclusive).
    /// </summary>
    /// <value>100 — represents maximum corruption, triggering a Mutation Check.</value>
    public const int MaxCorruption = 100;

    /// <summary>
    /// The corruption threshold for entering the Tainted stage (20+).
    /// </summary>
    /// <value>20 — minimum corruption for minor cosmetic signs.</value>
    public const int TaintedThreshold = 20;

    /// <summary>
    /// The corruption threshold for entering the Infected stage (40+).
    /// </summary>
    /// <value>40 — minimum corruption for visible corruption marks.</value>
    public const int InfectedThreshold = 40;

    /// <summary>
    /// The corruption threshold for entering the Blighted stage (60+).
    /// </summary>
    /// <value>60 — minimum corruption for physical mutations to manifest.</value>
    public const int BlightedThreshold = 60;

    /// <summary>
    /// The corruption threshold for entering the Corrupted stage (80+).
    /// </summary>
    /// <value>80 — minimum corruption for severe mutations and high mutation risk.</value>
    public const int CorruptedThreshold = 80;

    /// <summary>
    /// The corruption threshold for the Consumed stage (100).
    /// </summary>
    /// <value>100 — maximum corruption, triggering a mandatory Mutation Check.</value>
    public const int ConsumedThreshold = 100;

    // ═══════════════════════════════════════════════════════════════════════════
    // PROPERTIES — Stored
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the current corruption value, clamped to [0, 100].
    /// </summary>
    /// <value>
    /// An integer in the range [<see cref="MinCorruption"/>, <see cref="MaxCorruption"/>].
    /// This is the single source of truth from which all other properties are derived.
    /// </value>
    public int CurrentCorruption { get; }

    // ═══════════════════════════════════════════════════════════════════════════
    // PROPERTIES — Computed (set in constructor for performance)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the corruption stage for the current corruption value.
    /// </summary>
    /// <value>
    /// The <see cref="CorruptionStage"/> enum value corresponding to the current
    /// corruption range (e.g., Uncorrupted for 0-19, Tainted for 20-39, etc.).
    /// </value>
    public CorruptionStage Stage { get; }

    /// <summary>
    /// Gets the percentage of progress toward being Consumed (0.0 to 1.0).
    /// </summary>
    /// <value>
    /// A double in the range [0.0, 1.0] representing the corruption fill percentage.
    /// Returns 1.0 when corruption equals <see cref="MaxCorruption"/>.
    /// Useful for progress bar rendering in the TUI.
    /// </value>
    public double PercentageToConsumption { get; }

    /// <summary>
    /// Gets a value indicating whether a Mutation Check is required.
    /// </summary>
    /// <value>
    /// <c>true</c> when corruption has reached <see cref="MaxCorruption"/> (100), indicating
    /// the character must make a Mutation Check; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// <para>
    /// When <c>true</c>, the character must immediately resolve a Mutation Check
    /// before any other actions can be taken. Mutation Check outcomes:
    /// <list type="bullet">
    ///   <item><description>Success: Gain a permanent mutation, corruption resets to 75.</description></item>
    ///   <item><description>Failure: Gain a severe mutation, corruption resets to 50.</description></item>
    ///   <item><description>Critical Failure: Character may become an NPC (GM discretion).</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public bool RequiresMutationCheck { get; }

    /// <summary>
    /// Gets a value indicating whether the character is at risk of mutation.
    /// </summary>
    /// <value>
    /// <c>true</c> when corruption is at or above <see cref="CorruptedThreshold"/> (80),
    /// meaning the character is in the Corrupted or Consumed stage; otherwise, <c>false</c>.
    /// At this level, any additional corruption exposure could trigger a Mutation Check.
    /// Used for warning displays and risk-aware decision making.
    /// </value>
    public bool HasMutationRisk { get; }

    // ═══════════════════════════════════════════════════════════════════════════
    // PROPERTIES — Arrow-Expression (derived from stored/computed properties)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets a value indicating whether the character is in the Uncorrupted stage.
    /// </summary>
    /// <value>
    /// <c>true</c> when <see cref="Stage"/> is <see cref="CorruptionStage.Uncorrupted"/>
    /// (corruption 0-19); otherwise, <c>false</c>.
    /// </value>
    public bool IsUncorrupted => Stage == CorruptionStage.Uncorrupted;

    /// <summary>
    /// Gets a value indicating whether the character has been Consumed by corruption.
    /// </summary>
    /// <value>
    /// <c>true</c> when <see cref="Stage"/> is <see cref="CorruptionStage.Consumed"/>
    /// (corruption 100); otherwise, <c>false</c>.
    /// </value>
    public bool IsConsumed => Stage == CorruptionStage.Consumed;

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR (private — use factory methods)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of the <see cref="CorruptionState"/> struct.
    /// </summary>
    /// <param name="corruption">
    /// The raw corruption value. Clamped to [<see cref="MinCorruption"/>, <see cref="MaxCorruption"/>].
    /// </param>
    /// <remarks>
    /// <para>
    /// The constructor clamps the corruption value and computes all derived properties.
    /// This ensures the struct is always in a valid, consistent state regardless
    /// of the input value.
    /// </para>
    /// </remarks>
    private CorruptionState(int corruption)
    {
        // Clamp corruption to valid range [0, 100]
        CurrentCorruption = Math.Clamp(corruption, MinCorruption, MaxCorruption);

        // Compute derived properties from clamped corruption value
        Stage = DetermineStage(CurrentCorruption);
        PercentageToConsumption = CurrentCorruption / (double)MaxCorruption;
        RequiresMutationCheck = CurrentCorruption >= MaxCorruption;
        HasMutationRisk = CurrentCorruption >= CorruptedThreshold;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new <see cref="CorruptionState"/> with the specified corruption value.
    /// </summary>
    /// <param name="corruption">
    /// The corruption value. Clamped to [0, 100] — negative values become 0,
    /// values above 100 become 100.
    /// </param>
    /// <returns>A new <see cref="CorruptionState"/> instance with derived properties computed.</returns>
    /// <example>
    /// <code>
    /// var state = CorruptionState.Create(45);
    /// // state.CurrentCorruption == 45
    /// // state.Stage == CorruptionStage.Infected
    /// // state.HasMutationRisk == false
    ///
    /// var clamped = CorruptionState.Create(150);
    /// // clamped.CurrentCorruption == 100 (clamped)
    /// // clamped.RequiresMutationCheck == true
    /// </code>
    /// </example>
    public static CorruptionState Create(int corruption) => new(corruption);

    /// <summary>
    /// Gets a <see cref="CorruptionState"/> representing zero corruption (Uncorrupted stage).
    /// </summary>
    /// <value>
    /// A <see cref="CorruptionState"/> with <see cref="CurrentCorruption"/> = 0,
    /// <see cref="Stage"/> = <see cref="CorruptionStage.Uncorrupted"/>,
    /// and no mutation risk.
    /// </value>
    /// <example>
    /// <code>
    /// var pure = CorruptionState.Uncorrupted;
    /// // pure.CurrentCorruption == 0
    /// // pure.IsUncorrupted == true
    /// // pure.HasMutationRisk == false
    /// </code>
    /// </example>
    public static CorruptionState Uncorrupted => Create(MinCorruption);

    // ═══════════════════════════════════════════════════════════════════════════
    // STAGE DETERMINATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Determines the <see cref="CorruptionStage"/> for a given corruption value.
    /// </summary>
    /// <param name="corruption">The corruption value (0-100).</param>
    /// <returns>The corresponding <see cref="CorruptionStage"/>.</returns>
    /// <remarks>
    /// Stage thresholds:
    /// <list type="bullet">
    ///   <item><description>0-19: Uncorrupted</description></item>
    ///   <item><description>20-39: Tainted</description></item>
    ///   <item><description>40-59: Infected</description></item>
    ///   <item><description>60-79: Blighted</description></item>
    ///   <item><description>80-99: Corrupted</description></item>
    ///   <item><description>100: Consumed</description></item>
    /// </list>
    /// </remarks>
    public static CorruptionStage DetermineStage(int corruption) =>
        corruption switch
        {
            >= ConsumedThreshold => CorruptionStage.Consumed,
            >= CorruptedThreshold => CorruptionStage.Corrupted,
            >= BlightedThreshold => CorruptionStage.Blighted,
            >= InfectedThreshold => CorruptionStage.Infected,
            >= TaintedThreshold => CorruptionStage.Tainted,
            _ => CorruptionStage.Uncorrupted
        };

    // ═══════════════════════════════════════════════════════════════════════════
    // DEBUGGING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a string representation of the corruption state for debugging and logging.
    /// </summary>
    /// <returns>
    /// A formatted string in the format:
    /// <c>"Corruption: {CurrentCorruption}/{MaxCorruption} [{Stage}]"</c>,
    /// with optional <c>[MUTATION CHECK REQUIRED]</c> or <c>[Mutation Risk]</c> suffixes.
    /// </returns>
    /// <example>
    /// <code>
    /// var state = CorruptionState.Create(45);
    /// var display = state.ToString();
    /// // Returns "Corruption: 45/100 [Infected]"
    ///
    /// var danger = CorruptionState.Create(85);
    /// // Returns "Corruption: 85/100 [Corrupted] [Mutation Risk]"
    ///
    /// var consumed = CorruptionState.Create(100);
    /// // Returns "Corruption: 100/100 [Consumed] [MUTATION CHECK REQUIRED]"
    /// </code>
    /// </example>
    public override string ToString() =>
        $"Corruption: {CurrentCorruption}/{MaxCorruption} [{Stage}]" +
        (RequiresMutationCheck ? " [MUTATION CHECK REQUIRED]" : "") +
        (HasMutationRisk && !RequiresMutationCheck ? " [Mutation Risk]" : "");
}
