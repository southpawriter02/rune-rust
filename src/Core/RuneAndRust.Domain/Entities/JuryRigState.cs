// ------------------------------------------------------------------------------
// <copyright file="JuryRigState.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// State machine entity tracking the progress of a jury-rigging attempt
// through the five-step trial-and-error procedure.
// Part of v0.15.4e Jury-Rigging System implementation.
// </summary>
// ------------------------------------------------------------------------------

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Extensions;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.Entities;

/// <summary>
/// State machine entity tracking the progress of a jury-rigging attempt
/// through the five-step trial-and-error procedure.
/// </summary>
/// <remarks>
/// <para>
/// JuryRigState manages the full lifecycle of a jury-rigging attempt:
/// <list type="bullet">
///   <item><description>Observe (optional) → Study mechanism visually</description></item>
///   <item><description>Probe (automatic) → Try obvious buttons and levers</description></item>
///   <item><description>Pattern (optional) → Recognize mechanism type</description></item>
///   <item><description>MethodSelection → Choose bypass approach</description></item>
///   <item><description>Experiment → Attempt the bypass</description></item>
///   <item><description>Iterate → Learn from failure (DC -1)</description></item>
/// </list>
/// </para>
/// <para>
/// The state tracks DC reduction from iteration learning, familiarity with
/// mechanism types, and stores revealed hints from observation/probing.
/// </para>
/// </remarks>
public class JuryRigState
{
    // -------------------------------------------------------------------------
    // Constants
    // -------------------------------------------------------------------------

    /// <summary>
    /// The minimum DC for any jury-rigging attempt, regardless of iteration learning.
    /// </summary>
    public const int MinimumDc = 4;

    // -------------------------------------------------------------------------
    // Backing Fields
    // -------------------------------------------------------------------------

    private readonly HashSet<string> _familiarMechanismTypes;
    private readonly List<string> _revealedHints;
    private readonly List<JuryRigResult> _previousAttempts;

    // -------------------------------------------------------------------------
    // Identity Properties
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the unique identifier for this jury-rigging session.
    /// </summary>
    public string JuryRigId { get; private init; } = string.Empty;

    /// <summary>
    /// Gets the ID of the character attempting the bypass.
    /// </summary>
    public string CharacterId { get; private init; } = string.Empty;

    // -------------------------------------------------------------------------
    // Mechanism Properties
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the type of mechanism being bypassed.
    /// </summary>
    /// <remarks>
    /// Mechanism type is used for familiarity tracking. Successfully bypassing
    /// a mechanism marks its type as "familiar" for future encounters.
    /// </remarks>
    public string MechanismType { get; private init; } = string.Empty;

    /// <summary>
    /// Gets the display name of the mechanism.
    /// </summary>
    public string MechanismName { get; private init; } = string.Empty;

    /// <summary>
    /// Gets the base DC for bypassing this mechanism.
    /// </summary>
    public int BaseDc { get; private init; }

    /// <summary>
    /// Gets whether the mechanism is in a glitched state.
    /// </summary>
    /// <remarks>
    /// Glitched mechanisms enable the Glitch Exploitation bypass method (-4 DC).
    /// </remarks>
    public bool IsGlitched { get; private set; }

    // -------------------------------------------------------------------------
    // Progress Properties
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the current step in the jury-rigging procedure.
    /// </summary>
    public JuryRigStep CurrentStep { get; private set; } = JuryRigStep.Observe;

    /// <summary>
    /// Gets the currently selected bypass method, if any.
    /// </summary>
    public BypassMethod? SelectedMethod { get; private set; }

    /// <summary>
    /// Gets the number of iterations (failed attempts) on this mechanism.
    /// </summary>
    /// <remarks>
    /// Each iteration reduces the DC by 1, to a minimum of <see cref="MinimumDc"/>.
    /// </remarks>
    public int IterationCount { get; private set; }

    // -------------------------------------------------------------------------
    // Step Completion Flags
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets whether the Observe step has been completed.
    /// </summary>
    public bool ObservationComplete { get; private set; }

    /// <summary>
    /// Gets whether the Probe step has been completed.
    /// </summary>
    public bool ProbingComplete { get; private set; }

    /// <summary>
    /// Gets whether the Pattern Recognition step has been completed.
    /// </summary>
    public bool PatternRecognized { get; private set; }

    /// <summary>
    /// Gets whether the character is familiar with this mechanism type.
    /// </summary>
    /// <remarks>
    /// Familiarity is determined at session start or earned by successfully
    /// bypassing a mechanism. Familiar mechanisms grant +2d10 bonus dice.
    /// </remarks>
    public bool IsFamiliarMechanism { get; private set; }

    // -------------------------------------------------------------------------
    // Status Properties
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the current status of the jury-rigging attempt.
    /// </summary>
    public JuryRigStatus Status { get; private set; } = JuryRigStatus.InProgress;

    /// <summary>
    /// Gets whether an alarm has been triggered during this session.
    /// </summary>
    /// <remarks>
    /// Alarms trigger on complication roll 2-3. Once triggered, the alarm
    /// continues sounding regardless of further attempts.
    /// </remarks>
    public bool AlarmTriggered { get; private set; }

    /// <summary>
    /// Gets the total damage taken during this session.
    /// </summary>
    /// <remarks>
    /// Damage can come from electrocution (Wire Manipulation) or
    /// Sparks Fly complication.
    /// </remarks>
    public int TotalDamageTaken { get; private set; }

    // -------------------------------------------------------------------------
    // Collection Properties
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the hints revealed during Observe and Probe steps.
    /// </summary>
    public IReadOnlyList<string> RevealedHints => _revealedHints.AsReadOnly();

    /// <summary>
    /// Gets the results of previous experiment attempts.
    /// </summary>
    public IReadOnlyList<JuryRigResult> PreviousAttempts => _previousAttempts.AsReadOnly();

    /// <summary>
    /// Gets the salvaged components (from critical success or Brute Disassembly).
    /// </summary>
    public IReadOnlyList<string> SalvagedComponents { get; private set; } = Array.Empty<string>();

    // -------------------------------------------------------------------------
    // Computed Properties
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the current modified DC (base - iteration + method modifier).
    /// </summary>
    /// <remarks>
    /// <para>
    /// DC is calculated as: BaseDC - IterationCount + MethodModifier
    /// <list type="bullet">
    ///   <item><description>Iteration learning reduces DC by 1 per failed attempt</description></item>
    ///   <item><description>Method modifier ranges from -4 (Glitch Exploitation) to +2 (Brute Disassembly)</description></item>
    ///   <item><description>Final DC is clamped to minimum of <see cref="MinimumDc"/></description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public int GetModifiedDc()
    {
        var methodModifier = SelectedMethod?.GetDcModifier() ?? 0;
        var rawDc = BaseDc - IterationCount + methodModifier;
        return Math.Max(rawDc, MinimumDc);
    }

    /// <summary>
    /// Gets the DC reduction from iteration learning.
    /// </summary>
    public int IterationBonus => IterationCount;

    /// <summary>
    /// Gets whether the jury-rigging is in a terminal state.
    /// </summary>
    /// <remarks>
    /// Terminal states end the session: Bypassed, Destroyed, MechanismDestroyed,
    /// PermanentlyLocked, Abandoned.
    /// </remarks>
    public bool IsTerminal => Status != JuryRigStatus.InProgress;

    /// <summary>
    /// Gets whether the bypass was successful.
    /// </summary>
    public bool WasSuccessful => Status == JuryRigStatus.Bypassed ||
                                  Status == JuryRigStatus.Destroyed;

    /// <summary>
    /// Gets the number of dice bonus from familiarity.
    /// </summary>
    public int FamiliarityBonus => IsFamiliarMechanism ? 2 : 0;

    /// <summary>
    /// Gets whether the character can use Memorized Sequence.
    /// </summary>
    public bool CanUseMemorizedSequence => IsFamiliarMechanism;

    /// <summary>
    /// Gets whether the character can use Glitch Exploitation.
    /// </summary>
    public bool CanUseGlitchExploitation => IsGlitched;

    // -------------------------------------------------------------------------
    // Factory Method
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a new jury-rigging state for the specified character and mechanism.
    /// </summary>
    /// <param name="characterId">The ID of the character attempting bypass.</param>
    /// <param name="mechanismType">The type category of the mechanism.</param>
    /// <param name="mechanismName">The display name of the mechanism.</param>
    /// <param name="baseDc">The base DC for bypassing this mechanism.</param>
    /// <param name="isGlitched">Whether the mechanism is in a glitched state.</param>
    /// <param name="familiarMechanismTypes">Types the character is already familiar with.</param>
    /// <returns>A new JuryRigState in the initial (Observe) state.</returns>
    /// <exception cref="ArgumentException">Thrown when required parameters are invalid.</exception>
    public static JuryRigState Create(
        string characterId,
        string mechanismType,
        string mechanismName,
        int baseDc,
        bool isGlitched,
        IEnumerable<string>? familiarMechanismTypes = null)
    {
        if (string.IsNullOrWhiteSpace(characterId))
        {
            throw new ArgumentException("Character ID cannot be null or empty.", nameof(characterId));
        }

        if (string.IsNullOrWhiteSpace(mechanismType))
        {
            throw new ArgumentException("Mechanism type cannot be null or empty.", nameof(mechanismType));
        }

        if (baseDc < MinimumDc)
        {
            throw new ArgumentOutOfRangeException(
                nameof(baseDc),
                baseDc,
                $"Base DC must be at least {MinimumDc}.");
        }

        var familiarTypes = new HashSet<string>(
            familiarMechanismTypes ?? Enumerable.Empty<string>(),
            StringComparer.OrdinalIgnoreCase);

        var isFamiliar = familiarTypes.Contains(mechanismType);

        return new JuryRigState(familiarTypes)
        {
            JuryRigId = Guid.NewGuid().ToString(),
            CharacterId = characterId,
            MechanismType = mechanismType,
            MechanismName = string.IsNullOrWhiteSpace(mechanismName) ? mechanismType : mechanismName,
            BaseDc = baseDc,
            IsGlitched = isGlitched,
            CurrentStep = JuryRigStep.Observe,
            Status = JuryRigStatus.InProgress,
            IsFamiliarMechanism = isFamiliar,
            IterationCount = 0
        };
    }

    // -------------------------------------------------------------------------
    // Observation Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Records the completion of the Observe step.
    /// </summary>
    /// <param name="success">Whether observation succeeded.</param>
    /// <param name="hints">Hints revealed by successful observation.</param>
    /// <exception cref="InvalidOperationException">Thrown if observation already complete or invalid state.</exception>
    public void CompleteObservation(bool success, IEnumerable<string>? hints = null)
    {
        ValidateInProgress("complete observation");

        if (ObservationComplete)
        {
            throw new InvalidOperationException("Observation has already been completed.");
        }

        if (CurrentStep != JuryRigStep.Observe)
        {
            throw new InvalidOperationException(
                $"Cannot complete observation from step {CurrentStep}. Must be at Observe step.");
        }

        ObservationComplete = true;

        if (success && hints != null)
        {
            _revealedHints.AddRange(hints);
        }

        CurrentStep = JuryRigStep.Probe;
    }

    /// <summary>
    /// Skips the Observe step, proceeding directly to Probe.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if not at Observe step.</exception>
    public void SkipObservation()
    {
        ValidateInProgress("skip observation");

        if (CurrentStep != JuryRigStep.Observe)
        {
            throw new InvalidOperationException(
                $"Cannot skip observation from step {CurrentStep}. Must be at Observe step.");
        }

        ObservationComplete = true;
        CurrentStep = JuryRigStep.Probe;
    }

    // -------------------------------------------------------------------------
    // Probe Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Records the completion of the Probe step.
    /// </summary>
    /// <param name="hints">Hints revealed by probing.</param>
    /// <param name="detectedGlitched">Whether probing revealed glitched behavior.</param>
    /// <exception cref="InvalidOperationException">Thrown if not at Probe step.</exception>
    public void CompleteProbe(IEnumerable<string>? hints = null, bool? detectedGlitched = null)
    {
        ValidateInProgress("complete probe");

        if (CurrentStep != JuryRigStep.Probe)
        {
            throw new InvalidOperationException(
                $"Cannot complete probe from step {CurrentStep}. Must be at Probe step.");
        }

        ProbingComplete = true;

        if (hints != null)
        {
            _revealedHints.AddRange(hints);
        }

        if (detectedGlitched.HasValue)
        {
            IsGlitched = detectedGlitched.Value;
        }

        CurrentStep = JuryRigStep.Pattern;
    }

    // -------------------------------------------------------------------------
    // Pattern Recognition Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Records the completion of the Pattern Recognition step.
    /// </summary>
    /// <param name="success">Whether pattern recognition succeeded.</param>
    /// <exception cref="InvalidOperationException">Thrown if not at Pattern step.</exception>
    /// <remarks>
    /// If the character successfully recognizes a new mechanism type,
    /// it will be added to their familiar types upon successful bypass.
    /// </remarks>
    public void CompletePatternRecognition(bool success)
    {
        ValidateInProgress("complete pattern recognition");

        if (CurrentStep != JuryRigStep.Pattern)
        {
            throw new InvalidOperationException(
                $"Cannot complete pattern recognition from step {CurrentStep}. Must be at Pattern step.");
        }

        PatternRecognized = true;
        CurrentStep = JuryRigStep.MethodSelection;
    }

    /// <summary>
    /// Skips the Pattern Recognition step.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if not at Pattern step.</exception>
    public void SkipPatternRecognition()
    {
        ValidateInProgress("skip pattern recognition");

        if (CurrentStep != JuryRigStep.Pattern)
        {
            throw new InvalidOperationException(
                $"Cannot skip pattern recognition from step {CurrentStep}. Must be at Pattern step.");
        }

        PatternRecognized = true;
        CurrentStep = JuryRigStep.MethodSelection;
    }

    // -------------------------------------------------------------------------
    // Method Selection
    // -------------------------------------------------------------------------

    /// <summary>
    /// Selects a bypass method for the experiment.
    /// </summary>
    /// <param name="method">The bypass method to use.</param>
    /// <exception cref="InvalidOperationException">Thrown if not at MethodSelection step.</exception>
    /// <exception cref="ArgumentException">Thrown if method is not valid for current context.</exception>
    public void SelectMethod(BypassMethod method)
    {
        ValidateInProgress("select method");

        if (CurrentStep != JuryRigStep.MethodSelection)
        {
            throw new InvalidOperationException(
                $"Cannot select method from step {CurrentStep}. Must be at MethodSelection step.");
        }

        var invalidReason = method.GetInvalidReason(IsFamiliarMechanism, IsGlitched);
        if (invalidReason != null)
        {
            throw new ArgumentException($"Cannot use {method}: {invalidReason}", nameof(method));
        }

        SelectedMethod = method;
        CurrentStep = JuryRigStep.Experiment;
    }

    // -------------------------------------------------------------------------
    // Experiment Results
    // -------------------------------------------------------------------------

    /// <summary>
    /// Records the result of an experiment attempt.
    /// </summary>
    /// <param name="result">The experiment result.</param>
    /// <exception cref="InvalidOperationException">Thrown if not at Experiment step.</exception>
    public void RecordAttempt(JuryRigResult result)
    {
        ValidateInProgress("record attempt");

        if (CurrentStep != JuryRigStep.Experiment)
        {
            throw new InvalidOperationException(
                $"Cannot record attempt from step {CurrentStep}. Must be at Experiment step.");
        }

        _previousAttempts.Add(result);

        if (result.AlertTriggered)
        {
            AlarmTriggered = true;
        }

        if (result.TookDamage)
        {
            TotalDamageTaken += result.DamageDealt;
        }

        if (result.HasSalvage)
        {
            SalvagedComponents = result.SalvagedComponents;
        }

        // Update status based on outcome
        switch (result.Outcome)
        {
            case JuryRigOutcome.Success:
            case JuryRigOutcome.CriticalSuccess:
                HandleSuccess(result);
                break;

            case JuryRigOutcome.Fumble:
                Status = JuryRigStatus.MechanismDestroyed;
                break;

            case JuryRigOutcome.PermanentLock:
                Status = JuryRigStatus.PermanentlyLocked;
                break;

            case JuryRigOutcome.PartialSuccess:
            case JuryRigOutcome.Failure:
                // Remain in progress, can iterate
                CurrentStep = JuryRigStep.Iterate;
                break;
        }

        // Check for GlitchInFavor (auto-success from complication table)
        if (result.ComplicationEffect == ComplicationEffect.GlitchInFavor)
        {
            HandleSuccess(result);
        }
    }

    /// <summary>
    /// Handles successful bypass (including Brute Disassembly).
    /// </summary>
    private void HandleSuccess(JuryRigResult result)
    {
        // Mark mechanism type as familiar for future sessions
        if (!_familiarMechanismTypes.Contains(MechanismType))
        {
            _familiarMechanismTypes.Add(MechanismType);
        }

        IsFamiliarMechanism = true;

        // Determine final status based on method
        if (result.MethodUsed == BypassMethod.BruteDisassembly)
        {
            Status = JuryRigStatus.Destroyed;
        }
        else
        {
            Status = JuryRigStatus.Bypassed;
        }
    }

    // -------------------------------------------------------------------------
    // Iteration Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Applies iteration learning after a failed attempt.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if not at Iterate step.</exception>
    public void ApplyIteration()
    {
        ValidateInProgress("apply iteration");

        if (CurrentStep != JuryRigStep.Iterate)
        {
            throw new InvalidOperationException(
                $"Cannot apply iteration from step {CurrentStep}. Must be at Iterate step.");
        }

        IterationCount++;
        SelectedMethod = null;
        CurrentStep = JuryRigStep.MethodSelection;
    }

    // -------------------------------------------------------------------------
    // Abandon Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Abandons the jury-rigging attempt.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if already in terminal state.</exception>
    public void Abandon()
    {
        ValidateInProgress("abandon");
        Status = JuryRigStatus.Abandoned;
    }

    // -------------------------------------------------------------------------
    // Electrocution Damage
    // -------------------------------------------------------------------------

    /// <summary>
    /// Records electrocution damage from Wire Manipulation.
    /// </summary>
    /// <param name="damage">The damage taken.</param>
    public void RecordElectrocutionDamage(int damage)
    {
        ValidateInProgress("record electrocution damage");

        if (damage > 0)
        {
            TotalDamageTaken += damage;
        }
    }

    // -------------------------------------------------------------------------
    // Familiarity Access
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the set of mechanism types the character is familiar with.
    /// </summary>
    /// <returns>A copy of the familiar mechanism types set.</returns>
    /// <remarks>
    /// This can be used to persist familiarity between sessions.
    /// Successfully bypassing a mechanism adds its type to this set.
    /// </remarks>
    public IReadOnlySet<string> GetFamiliarMechanismTypes()
    {
        return new HashSet<string>(_familiarMechanismTypes, StringComparer.OrdinalIgnoreCase);
    }

    // -------------------------------------------------------------------------
    // Validation Helpers
    // -------------------------------------------------------------------------

    private void ValidateInProgress(string operation)
    {
        if (IsTerminal)
        {
            throw new InvalidOperationException(
                $"Cannot {operation}: jury-rigging is in terminal state {Status}.");
        }
    }

    // -------------------------------------------------------------------------
    // Display Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns a formatted display string for the state.
    /// </summary>
    /// <returns>A human-readable summary of the jury-rig state.</returns>
    public string ToDisplayString()
    {
        var methodStr = SelectedMethod.HasValue
            ? $" [{SelectedMethod.Value.GetDisplayName()}]"
            : "";

        var iterStr = IterationCount > 0 ? $" (-{IterationCount} DC iteration)" : "";
        var familiarStr = IsFamiliarMechanism ? " [Familiar +2d10]" : "";
        var glitchStr = IsGlitched ? " [Glitched]" : "";

        return $"{MechanismName}: {Status} @ {CurrentStep} - DC {GetModifiedDc()}{methodStr}{iterStr}{familiarStr}{glitchStr}";
    }

    /// <summary>
    /// Returns a compact string for logging purposes.
    /// </summary>
    /// <returns>A log-friendly string representation.</returns>
    public string ToLogString()
    {
        var shortId = JuryRigId.Length > 8 ? JuryRigId[..8] : JuryRigId;
        return $"JuryRigState[{shortId}] Type={MechanismType} Status={Status} Step={CurrentStep} " +
               $"DC={GetModifiedDc()} Iter={IterationCount} Familiar={IsFamiliarMechanism} " +
               $"Glitched={IsGlitched} Method={SelectedMethod}";
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return ToDisplayString();
    }

    // -------------------------------------------------------------------------
    // Private Constructor
    // -------------------------------------------------------------------------

    /// <summary>
    /// Private constructor - use Create() factory method.
    /// </summary>
    private JuryRigState(HashSet<string> familiarTypes)
    {
        _familiarMechanismTypes = familiarTypes;
        _revealedHints = new List<string>();
        _previousAttempts = new List<JuryRigResult>();
    }
}
