// ═══════════════════════════════════════════════════════════════════════════════
// CorruptionTracker.cs
// Domain entity that tracks per-character Runic Blight Corruption accumulation
// and effects. Maintains mutable corruption state including threshold trigger
// history, computed penalty values (Max HP, Max AP, Resolve Dice), stage-based
// bonuses (Tech, Social), faction lock status, and Terminal Error detection.
// Unlike the immutable CorruptionState value object (v0.18.1a), this entity
// persists state across operations and implements IEntity for repository support.
// Part of the Trauma Economy system — the second pillar (Creeping Doom).
// Version: 0.18.1b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.Entities;

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Entity that tracks per-character Runic Blight Corruption accumulation and effects.
/// </summary>
/// <remarks>
/// <para>
/// CorruptionTracker maintains the mutable corruption state for a single character,
/// including threshold trigger history and computed penalty values. Each character
/// has at most one CorruptionTracker, identified by <see cref="CharacterId"/>.
/// </para>
/// <para>
/// Unlike the immutable <see cref="CorruptionState"/> value object, this entity
/// persists state across operations and implements <see cref="IEntity"/> for
/// repository and persistence support.
/// </para>
/// <para>
/// Key Features:
/// <list type="bullet">
///   <item><description>Per-character corruption tracking via <see cref="CharacterId"/>.</description></item>
///   <item><description>One-time threshold crossing events at 25/50/75 corruption.</description></item>
///   <item><description>Computed penalties: Max HP (<see cref="MaxHpPenaltyPercent"/>), Max AP (<see cref="MaxApPenaltyPercent"/>), Resolve Dice (<see cref="ResolveDicePenalty"/>).</description></item>
///   <item><description>Stage-based bonuses: Tech (<see cref="TechBonus"/>), Social penalties (<see cref="SocialPenalty"/>).</description></item>
///   <item><description>Faction reputation lock at 50+ corruption (<see cref="IsFactionLocked"/>).</description></item>
///   <item><description>Terminal Error detection at 100 corruption (<see cref="IsTerminalError"/>).</description></item>
/// </list>
/// </para>
/// <para>
/// <strong>Penalty Formulas:</strong>
/// <list type="bullet">
///   <item><description>Max HP Penalty: floor(Corruption / 10) x 5% — ranges from 0% to 50%.</description></item>
///   <item><description>Max AP Penalty: floor(Corruption / 10) x 5% — ranges from 0% to 50%.</description></item>
///   <item><description>Resolve Dice Penalty: floor(Corruption / 20) — ranges from 0 to 5 dice.</description></item>
/// </list>
/// </para>
/// <para>
/// <strong>Threshold Crossings:</strong> At 25, 50, and 75 corruption, one-time
/// narrative events trigger. Each threshold fires exactly once per character
/// lifetime, tracked by <see cref="Threshold25Triggered"/>,
/// <see cref="Threshold50Triggered"/>, and <see cref="Threshold75Triggered"/> flags.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Create tracker for a character
/// var tracker = CorruptionTracker.Create(characterId);
///
/// // Add corruption from a heretical ability
/// var result = tracker.AddCorruption(15, CorruptionSource.HereticalAbility);
///
/// // Check computed penalties
/// Console.WriteLine($"HP Penalty: {tracker.MaxHpPenaltyPercent}%");
/// Console.WriteLine($"Resolve Penalty: -{tracker.ResolveDicePenalty} dice");
///
/// // Check threshold crossings
/// if (result.ThresholdCrossed.HasValue)
///     Console.WriteLine($"Crossed {result.ThresholdCrossed}% threshold!");
///
/// // Check for Terminal Error
/// if (result.IsTerminalError)
///     Console.WriteLine("TERMINAL ERROR - Survival check required!");
/// </code>
/// </example>
/// <seealso cref="CorruptionState"/>
/// <seealso cref="CorruptionStage"/>
/// <seealso cref="CorruptionAddResult"/>
/// <seealso cref="TerminalErrorResult"/>
/// <seealso cref="CorruptionSource"/>
public class CorruptionTracker : IEntity
{
    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTANTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Minimum corruption value.
    /// </summary>
    /// <value>0 — represents a character with no accumulated corruption.</value>
    public const int MinCorruption = 0;

    /// <summary>
    /// Maximum corruption value (Terminal Error).
    /// </summary>
    /// <value>100 — triggers a Terminal Error survival check.</value>
    public const int MaxCorruption = 100;

    /// <summary>
    /// First major threshold crossing value.
    /// </summary>
    /// <value>
    /// 25 — triggers a UI warning ("You feel the Blight's touch...").
    /// This threshold fires exactly once per character.
    /// </value>
    public const int Threshold25 = 25;

    /// <summary>
    /// Second major threshold crossing value (Faction Lock).
    /// </summary>
    /// <value>
    /// 50 — triggers faction reputation lock. Human faction reputation
    /// gains are permanently blocked at this corruption level.
    /// This threshold fires exactly once per character.
    /// </value>
    public const int Threshold50 = 50;

    /// <summary>
    /// Third major threshold crossing value.
    /// </summary>
    /// <value>
    /// 75 — triggers Machine Affinity trauma acquisition.
    /// This threshold fires exactly once per character.
    /// </value>
    public const int Threshold75 = 75;

    // ═══════════════════════════════════════════════════════════════════════════
    // PROPERTIES — Stored (persisted)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the unique identifier for this corruption tracker.
    /// </summary>
    /// <value>A <see cref="Guid"/> that uniquely identifies this tracker entity.</value>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the identifier of the character this tracker belongs to.
    /// </summary>
    /// <value>
    /// The <see cref="Guid"/> of the character (Player entity) whose corruption
    /// this tracker manages. Used as a foreign key for per-character queries.
    /// </value>
    public Guid CharacterId { get; private set; }

    /// <summary>
    /// Gets the current corruption value (0-100).
    /// </summary>
    /// <value>
    /// An integer in the range [<see cref="MinCorruption"/>, <see cref="MaxCorruption"/>].
    /// This is the single source of truth from which all computed properties are derived.
    /// </value>
    public int CurrentCorruption { get; private set; }

    /// <summary>
    /// Gets whether the 25% threshold has been triggered.
    /// </summary>
    /// <value>
    /// <c>true</c> if corruption has previously crossed the 25 threshold;
    /// <c>false</c> if not yet triggered.
    /// </value>
    /// <remarks>
    /// This flag is set exactly once when corruption first crosses 25.
    /// Used to trigger narrative events that should only occur once.
    /// Once set, it persists even if corruption is reduced below 25
    /// (which is extremely rare for corruption).
    /// </remarks>
    public bool Threshold25Triggered { get; private set; }

    /// <summary>
    /// Gets whether the 50% threshold has been triggered.
    /// </summary>
    /// <value>
    /// <c>true</c> if corruption has previously crossed the 50 threshold;
    /// <c>false</c> if not yet triggered.
    /// </value>
    /// <remarks>
    /// This threshold also triggers faction reputation lock. Once triggered,
    /// the character can no longer gain reputation with human-aligned factions
    /// through normal means.
    /// </remarks>
    public bool Threshold50Triggered { get; private set; }

    /// <summary>
    /// Gets whether the 75% threshold has been triggered.
    /// </summary>
    /// <value>
    /// <c>true</c> if corruption has previously crossed the 75 threshold;
    /// <c>false</c> if not yet triggered.
    /// </value>
    /// <remarks>
    /// This threshold triggers the acquisition of the Machine Affinity trauma,
    /// representing the character's body beginning to resonate with corrupted
    /// runic machinery.
    /// </remarks>
    public bool Threshold75Triggered { get; private set; }

    // ═══════════════════════════════════════════════════════════════════════════
    // PROPERTIES — Computed (derived from CurrentCorruption)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the current corruption stage based on <see cref="CurrentCorruption"/>.
    /// </summary>
    /// <value>
    /// The <see cref="CorruptionStage"/> determined by delegating to
    /// <see cref="CorruptionState.DetermineStage"/>. Changes automatically
    /// when <see cref="CurrentCorruption"/> changes.
    /// </value>
    public CorruptionStage Stage => CorruptionState.DetermineStage(CurrentCorruption);

    /// <summary>
    /// Gets the maximum HP percentage penalty.
    /// </summary>
    /// <value>
    /// An integer representing the percentage reduction to the character's
    /// maximum HP. Range: 0% (corruption 0-9) to 50% (corruption 100).
    /// </value>
    /// <remarks>
    /// Formula: floor(Corruption / 10) x 5.
    /// Integer division in C# performs floor automatically for positive values.
    /// Example: Corruption 45 -> floor(45/10) x 5 = 4 x 5 = 20%.
    /// </remarks>
    public int MaxHpPenaltyPercent => (CurrentCorruption / 10) * 5;

    /// <summary>
    /// Gets the maximum AP percentage penalty.
    /// </summary>
    /// <value>
    /// An integer representing the percentage reduction to the character's
    /// maximum Aether Points. Same formula as <see cref="MaxHpPenaltyPercent"/>.
    /// </value>
    /// <remarks>
    /// Formula: floor(Corruption / 10) x 5.
    /// Same calculation as HP penalty — both resources are equally affected
    /// by corruption's physical toll on the body.
    /// </remarks>
    public int MaxApPenaltyPercent => (CurrentCorruption / 10) * 5;

    /// <summary>
    /// Gets the Resolve dice pool penalty.
    /// </summary>
    /// <value>
    /// An integer representing the number of dice removed from Resolve checks.
    /// Range: 0 (corruption 0-19) to 5 (corruption 100).
    /// </value>
    /// <remarks>
    /// Formula: floor(Corruption / 20).
    /// Resolve represents mental fortitude — as corruption grows, the
    /// character's ability to resist further psychological effects weakens.
    /// </remarks>
    public int ResolveDicePenalty => CurrentCorruption / 20;

    /// <summary>
    /// Gets the technology interaction bonus based on corruption stage.
    /// </summary>
    /// <value>
    /// An integer bonus applied to technology-related skill checks.
    /// Corrupted characters have enhanced affinity with malfunctioning
    /// runic technology.
    /// </value>
    /// <remarks>
    /// Stage-based values:
    /// <list type="bullet">
    ///   <item><description>Uncorrupted: +0</description></item>
    ///   <item><description>Tainted: +1</description></item>
    ///   <item><description>Infected/Blighted/Corrupted: +2</description></item>
    ///   <item><description>Consumed: +0 (Terminal Error — bonuses irrelevant)</description></item>
    /// </list>
    /// The tech bonus caps at +2 and represents the character's growing
    /// resonance with the corrupted runic infrastructure of Aethelgard.
    /// </remarks>
    public int TechBonus => Stage switch
    {
        CorruptionStage.Uncorrupted => 0,
        CorruptionStage.Tainted => 1,
        CorruptionStage.Infected => 2,
        CorruptionStage.Blighted => 2,
        CorruptionStage.Corrupted => 2,
        CorruptionStage.Consumed => 0, // Terminal Error — bonuses irrelevant
        _ => 0
    };

    /// <summary>
    /// Gets the social interaction penalty based on corruption stage.
    /// </summary>
    /// <value>
    /// A negative integer penalty applied to social skill checks with
    /// uncorrupted entities. Pure entities and NPCs react negatively
    /// to visible corruption signs.
    /// </value>
    /// <remarks>
    /// Stage-based values:
    /// <list type="bullet">
    ///   <item><description>Uncorrupted: -0</description></item>
    ///   <item><description>Tainted: -1</description></item>
    ///   <item><description>Infected/Blighted/Corrupted: -2</description></item>
    ///   <item><description>Consumed: -0 (Terminal Error — penalties irrelevant)</description></item>
    /// </list>
    /// The social penalty caps at -2 because at higher corruption levels,
    /// NPCs already refuse to interact rather than applying further penalties.
    /// </remarks>
    public int SocialPenalty => Stage switch
    {
        CorruptionStage.Uncorrupted => 0,
        CorruptionStage.Tainted => -1,
        CorruptionStage.Infected => -2,
        CorruptionStage.Blighted => -2,
        CorruptionStage.Corrupted => -2,
        CorruptionStage.Consumed => 0, // Terminal Error — penalties irrelevant
        _ => 0
    };

    /// <summary>
    /// Gets whether faction reputation changes are locked.
    /// </summary>
    /// <value>
    /// <c>true</c> when <see cref="CurrentCorruption"/> is 50 or higher;
    /// <c>false</c> otherwise.
    /// </value>
    /// <remarks>
    /// At 50+ corruption, the character's reputation with human-aligned factions
    /// can no longer be improved through normal means. This represents the
    /// visible corruption making the character untrusted by organized society.
    /// Faction lock is based on current corruption, not the one-time threshold.
    /// </remarks>
    public bool IsFactionLocked => CurrentCorruption >= Threshold50;

    /// <summary>
    /// Gets whether corruption has reached Terminal Error (100).
    /// </summary>
    /// <value>
    /// <c>true</c> when <see cref="CurrentCorruption"/> equals
    /// <see cref="MaxCorruption"/>; <c>false</c> otherwise.
    /// </value>
    /// <remarks>
    /// At Terminal Error, the character must immediately resolve a
    /// Terminal Error check (WILL vs DC 3) or become Forlorn (unplayable NPC).
    /// See <see cref="TerminalErrorResult"/> for outcome details.
    /// </remarks>
    public bool IsTerminalError => CurrentCorruption >= MaxCorruption;

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTORS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Private parameterless constructor for Entity Framework Core materialization.
    /// </summary>
    /// <remarks>
    /// EF Core requires a parameterless constructor to create entity instances
    /// when loading from the database. Use <see cref="Create"/> factory method
    /// for application-level creation.
    /// </remarks>
    private CorruptionTracker()
    {
        Id = Guid.Empty;
        CharacterId = Guid.Empty;
    }

    /// <summary>
    /// Private constructor for factory method.
    /// </summary>
    /// <param name="characterId">The character to track corruption for.</param>
    private CorruptionTracker(Guid characterId)
    {
        Id = Guid.NewGuid();
        CharacterId = characterId;
        CurrentCorruption = MinCorruption;
        Threshold25Triggered = false;
        Threshold50Triggered = false;
        Threshold75Triggered = false;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new corruption tracker for a character.
    /// </summary>
    /// <param name="characterId">The character to track corruption for.</param>
    /// <returns>
    /// A new <see cref="CorruptionTracker"/> with zero corruption and no
    /// triggered thresholds.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="characterId"/> is <see cref="Guid.Empty"/>.
    /// </exception>
    /// <example>
    /// <code>
    /// var tracker = CorruptionTracker.Create(characterId);
    /// // tracker.CurrentCorruption == 0
    /// // tracker.Stage == CorruptionStage.Uncorrupted
    /// // tracker.MaxHpPenaltyPercent == 0
    /// </code>
    /// </example>
    public static CorruptionTracker Create(Guid characterId)
    {
        if (characterId == Guid.Empty)
        {
            throw new ArgumentException("Character ID cannot be empty.", nameof(characterId));
        }

        return new CorruptionTracker(characterId);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DOMAIN METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Adds corruption to the character.
    /// </summary>
    /// <param name="amount">
    /// The amount of corruption to add. Positive values increase corruption;
    /// negative values decrease corruption (rare removal operations).
    /// The final value is clamped to [0, 100].
    /// </param>
    /// <param name="source">The source category of the corruption.</param>
    /// <returns>
    /// A <see cref="CorruptionAddResult"/> describing the outcome, including
    /// threshold crossings, stage transitions, and Terminal Error status.
    /// </returns>
    /// <remarks>
    /// <para>
    /// The method performs the following steps:
    /// <list type="number">
    ///   <item><description>Record the previous corruption and stage.</description></item>
    ///   <item><description>Calculate new corruption, clamped to [0, 100].</description></item>
    ///   <item><description>Check for one-time threshold crossings (25/50/75).</description></item>
    ///   <item><description>Detect stage transitions.</description></item>
    ///   <item><description>Return a comprehensive result object.</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Threshold Crossing Logic:</strong> A threshold is crossed when the
    /// previous corruption was below the threshold, the new corruption is at or
    /// above it, AND the threshold has not been previously triggered. Each threshold
    /// fires at most once per character lifetime.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var result = tracker.AddCorruption(15, CorruptionSource.HereticalAbility);
    /// if (result.IsTerminalError)
    ///     InitiateTerminalErrorCheck();
    /// if (result.ThresholdCrossed.HasValue)
    ///     TriggerThresholdEvent(result.ThresholdCrossed.Value);
    /// </code>
    /// </example>
    public CorruptionAddResult AddCorruption(int amount, CorruptionSource source)
    {
        var previousCorruption = CurrentCorruption;
        var previousStage = Stage;

        // Calculate new corruption, clamped to valid range
        var newCorruption = Math.Clamp(CurrentCorruption + amount, MinCorruption, MaxCorruption);
        CurrentCorruption = newCorruption;

        // Detect threshold crossings (one-time triggers)
        int? thresholdCrossed = null;

        if (!Threshold25Triggered && previousCorruption < Threshold25 && newCorruption >= Threshold25)
        {
            Threshold25Triggered = true;
            thresholdCrossed = Threshold25;
        }
        else if (!Threshold50Triggered && previousCorruption < Threshold50 && newCorruption >= Threshold50)
        {
            Threshold50Triggered = true;
            thresholdCrossed = Threshold50;
        }
        else if (!Threshold75Triggered && previousCorruption < Threshold75 && newCorruption >= Threshold75)
        {
            Threshold75Triggered = true;
            thresholdCrossed = Threshold75;
        }

        var newStage = Stage;

        return CorruptionAddResult.Create(
            previousCorruption: previousCorruption,
            newCorruption: newCorruption,
            source: source,
            thresholdCrossed: thresholdCrossed,
            previousStage: previousStage,
            newStage: newStage);
    }

    /// <summary>
    /// Sets the corruption value directly (for deserialization/testing).
    /// </summary>
    /// <param name="value">The corruption value to set. Clamped to [0, 100].</param>
    /// <remarks>
    /// This method bypasses threshold trigger logic. Use only for
    /// persistence reconstitution or test setup. For gameplay corruption
    /// changes, use <see cref="AddCorruption"/> which properly tracks
    /// thresholds and returns result objects.
    /// </remarks>
    internal void SetCorruption(int value)
    {
        CurrentCorruption = Math.Clamp(value, MinCorruption, MaxCorruption);
    }

    /// <summary>
    /// Sets threshold trigger flags directly (for deserialization/testing).
    /// </summary>
    /// <param name="t25">Whether the 25% threshold has been triggered.</param>
    /// <param name="t50">Whether the 50% threshold has been triggered.</param>
    /// <param name="t75">Whether the 75% threshold has been triggered.</param>
    /// <remarks>
    /// This method is used for persistence reconstitution and test setup.
    /// In normal gameplay, threshold flags are set automatically by
    /// <see cref="AddCorruption"/>.
    /// </remarks>
    internal void SetThresholdTriggers(bool t25, bool t50, bool t75)
    {
        Threshold25Triggered = t25;
        Threshold50Triggered = t50;
        Threshold75Triggered = t75;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DEBUGGING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a string representation of this corruption tracker for debugging and logging.
    /// </summary>
    /// <returns>
    /// A formatted string showing the character ID, corruption level, stage,
    /// computed penalties, and status flags (Terminal Error, Faction Lock).
    /// </returns>
    /// <example>
    /// <code>
    /// var tracker = CorruptionTracker.Create(characterId);
    /// tracker.SetCorruption(55);
    /// var display = tracker.ToString();
    /// // Returns "CorruptionTracker[{guid}]: 55/100 [Infected] HP:-25% AP:-25% Resolve:-2 [FACTION LOCKED]"
    /// </code>
    /// </example>
    public override string ToString() =>
        $"CorruptionTracker[{CharacterId}]: {CurrentCorruption}/{MaxCorruption} [{Stage}]" +
        $" HP:-{MaxHpPenaltyPercent}% AP:-{MaxApPenaltyPercent}% Resolve:-{ResolveDicePenalty}" +
        (IsTerminalError ? " [TERMINAL ERROR]" : "") +
        (IsFactionLocked ? " [FACTION LOCKED]" : "");
}
