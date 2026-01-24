// ------------------------------------------------------------------------------
// <copyright file="TerminalInfiltrationState.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Tracks the state of a multi-layer terminal infiltration attempt.
// Part of v0.15.4b Terminal Hacking System implementation.
// Updated in v0.15.4c to add ICE encounter tracking.
// </summary>
// ------------------------------------------------------------------------------

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.Entities;

/// <summary>
/// Tracks the state of a multi-layer terminal infiltration attempt.
/// </summary>
/// <remarks>
/// <para>
/// Maintains the progression of a hacking attempt across all three layers,
/// tracking results, access level achieved, alert status, and overall outcome.
/// </para>
/// <para>
/// Infiltration follows a state machine pattern:
/// <code>
/// Created → Layer1 → Layer2 → Layer3 → Completed
///                ↓        ↓        ↓
///              Failed   Failed   Failed
///                ↓        ↓        ↓
///              Locked  Locked   Locked (fumble only)
/// </code>
/// </para>
/// </remarks>
public class TerminalInfiltrationState
{
    // -------------------------------------------------------------------------
    // Backing Fields
    // -------------------------------------------------------------------------

    private readonly List<LayerResult> _layerResults = new();
    private readonly List<IceEncounter> _iceEncounters = new();

    // -------------------------------------------------------------------------
    // Core Properties
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the unique identifier for this infiltration attempt.
    /// </summary>
    public string InfiltrationId { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the character attempting the infiltration.
    /// </summary>
    public string CharacterId { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the type of terminal being hacked.
    /// </summary>
    public TerminalType TerminalType { get; private set; }

    /// <summary>
    /// Gets the unique identifier of the terminal entity.
    /// </summary>
    public string TerminalId { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the current layer being attempted (1-3).
    /// </summary>
    public int CurrentLayer { get; private set; }

    /// <summary>
    /// Gets the results of each layer attempt.
    /// </summary>
    public IReadOnlyList<LayerResult> LayerResults => _layerResults.AsReadOnly();

    /// <summary>
    /// Gets the current access level achieved.
    /// </summary>
    public AccessLevel AccessLevel { get; private set; }

    /// <summary>
    /// Gets the accumulated alert level (affects security response).
    /// </summary>
    /// <remarks>
    /// Alert level increases on failed checks and certain outcomes.
    /// Higher alert levels may trigger faster security response or
    /// more aggressive ICE behavior.
    /// </remarks>
    public int AlertLevel { get; private set; }

    /// <summary>
    /// Gets whether the terminal has been locked out (fumble).
    /// </summary>
    public bool IsLockedOut { get; private set; }

    /// <summary>
    /// Gets the overall status of the infiltration attempt.
    /// </summary>
    public InfiltrationStatus Status { get; private set; }

    /// <summary>
    /// Gets whether tracks have been covered (logs wiped).
    /// </summary>
    public bool TracksCovered { get; private set; }

    // -------------------------------------------------------------------------
    // ICE Encounter Properties (v0.15.4c)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the collection of ICE encounters during this infiltration attempt.
    /// </summary>
    /// <remarks>
    /// ICE encounters are added when defensive programs activate, typically on
    /// Layer 2 authentication failure. Each encounter tracks its type, rating,
    /// and resolution outcome.
    /// </remarks>
    public IReadOnlyList<IceEncounter> IceEncounters => _iceEncounters.AsReadOnly();

    /// <summary>
    /// Gets the time when the terminal lockout expires.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Lockout occurs on ICE-induced disconnect (Active or Lethal ICE failure)
    /// or permanent lockout (Lethal ICE save failure or fumble).
    /// </para>
    /// <para>
    /// <c>null</c>: No lockout active.
    /// <c>DateTime.MaxValue</c>: Permanent lockout (terminal can never be accessed).
    /// Other values: Temporary lockout until that time.
    /// </para>
    /// </remarks>
    public DateTime? LockoutUntil { get; private set; }

    // -------------------------------------------------------------------------
    // Computed Properties
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the current layer enum value.
    /// </summary>
    public InfiltrationLayer CurrentLayerEnum => CurrentLayer switch
    {
        1 => InfiltrationLayer.Layer1_Access,
        2 => InfiltrationLayer.Layer2_Authentication,
        3 => InfiltrationLayer.Layer3_Navigation,
        _ => InfiltrationLayer.Layer1_Access
    };

    /// <summary>
    /// Gets whether the infiltration is complete (success or failure).
    /// </summary>
    public bool IsComplete => Status is InfiltrationStatus.Completed
        or InfiltrationStatus.LockedOut
        or InfiltrationStatus.Disconnected;

    /// <summary>
    /// Gets whether the infiltration was successful.
    /// </summary>
    public bool IsSuccessful => Status == InfiltrationStatus.Completed
        && AccessLevel >= AccessLevel.UserLevel;

    /// <summary>
    /// Gets whether admin access was achieved.
    /// </summary>
    public bool HasAdminAccess => AccessLevel == AccessLevel.AdminLevel;

    // -------------------------------------------------------------------------
    // ICE Computed Properties (v0.15.4c)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Whether any ICE encounter resulted in the character's location being revealed.
    /// </summary>
    /// <remarks>
    /// Location reveal occurs when Passive (Trace) ICE wins the contested check.
    /// When location is revealed, security may dispatch guards to the hacker's position.
    /// </remarks>
    public bool IsLocationRevealed => _iceEncounters.Any(e =>
        e.IceType == IceType.Passive &&
        e.EncounterResult == IceResolutionOutcome.IceWon);

    /// <summary>
    /// Whether the character was disconnected by ICE.
    /// </summary>
    /// <remarks>
    /// Forced disconnect occurs when Active or Lethal ICE wins. The character
    /// must wait for the lockout to expire before retrying (or cannot retry
    /// if lockout is permanent).
    /// </remarks>
    public bool WasDisconnectedByIce => _iceEncounters.Any(e =>
        e.EncounterResult == IceResolutionOutcome.IceWon &&
        e.IceType is IceType.Active or IceType.Lethal);

    /// <summary>
    /// Whether any ICE was successfully defeated.
    /// </summary>
    /// <remarks>
    /// Defeating ICE (particularly Active ICE) may grant bonuses to subsequent checks.
    /// </remarks>
    public bool HasDefeatedIce => _iceEncounters.Any(e =>
        e.EncounterResult == IceResolutionOutcome.CharacterWon);

    /// <summary>
    /// Gets the total bonus dice earned from defeating Active ICE.
    /// </summary>
    /// <remarks>
    /// Each defeated Active ICE grants +1d10 to the next layer check.
    /// </remarks>
    public int IceBonusDice => _iceEncounters.Count(e =>
        e.IceType == IceType.Active &&
        e.EncounterResult == IceResolutionOutcome.CharacterWon);

    /// <summary>
    /// Whether the terminal is currently in lockout state.
    /// </summary>
    public bool IsInLockout => LockoutUntil.HasValue && DateTime.UtcNow < LockoutUntil.Value;

    /// <summary>
    /// Whether the terminal is permanently locked out.
    /// </summary>
    public bool IsPermanentlyLockedOut => LockoutUntil == DateTime.MaxValue;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    /// <summary>
    /// Private constructor for EF Core and factory methods.
    /// </summary>
    private TerminalInfiltrationState() { }

    // -------------------------------------------------------------------------
    // Factory Method
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a new infiltration state for a terminal hacking attempt.
    /// </summary>
    /// <param name="infiltrationId">Unique identifier for this attempt.</param>
    /// <param name="characterId">The character attempting the hack.</param>
    /// <param name="terminalType">The type of terminal being hacked.</param>
    /// <param name="terminalId">The terminal entity identifier.</param>
    /// <returns>A new TerminalInfiltrationState in initial state.</returns>
    /// <exception cref="ArgumentException">Thrown when required parameters are null or whitespace.</exception>
    public static TerminalInfiltrationState Create(
        string infiltrationId,
        string characterId,
        TerminalType terminalType,
        string terminalId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(infiltrationId);
        ArgumentException.ThrowIfNullOrWhiteSpace(characterId);
        ArgumentException.ThrowIfNullOrWhiteSpace(terminalId);

        return new TerminalInfiltrationState
        {
            InfiltrationId = infiltrationId,
            CharacterId = characterId,
            TerminalType = terminalType,
            TerminalId = terminalId,
            CurrentLayer = 1,
            AccessLevel = AccessLevel.None,
            AlertLevel = 0,
            IsLockedOut = false,
            Status = InfiltrationStatus.InProgress,
            TracksCovered = false
        };
    }

    // -------------------------------------------------------------------------
    // State Mutation Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Records a layer result and advances the state machine.
    /// </summary>
    /// <param name="result">The result of the layer attempt.</param>
    /// <exception cref="InvalidOperationException">If infiltration is already complete.</exception>
    public void RecordLayerResult(LayerResult result)
    {
        if (IsComplete)
        {
            throw new InvalidOperationException("Cannot record result for completed infiltration.");
        }

        if (result.Layer != CurrentLayerEnum)
        {
            throw new InvalidOperationException(
                $"Expected result for {CurrentLayerEnum}, got {result.Layer}.");
        }

        _layerResults.Add(result);

        // Process result based on outcome
        if (result.IsFumble)
        {
            ProcessFumble();
        }
        else if (result.IsSuccess)
        {
            ProcessSuccess(result);
        }
        else
        {
            ProcessFailure(result);
        }
    }

    /// <summary>
    /// Marks tracks as covered after a successful stealth check.
    /// </summary>
    /// <exception cref="InvalidOperationException">If infiltration was not successful.</exception>
    public void MarkTracksCovered()
    {
        if (!IsSuccessful)
        {
            throw new InvalidOperationException("Can only cover tracks after successful infiltration.");
        }

        TracksCovered = true;
    }

    /// <summary>
    /// Marks the infiltration as disconnected (voluntary or ICE).
    /// </summary>
    public void MarkDisconnected()
    {
        if (Status == InfiltrationStatus.LockedOut)
        {
            return; // Already in terminal state
        }

        Status = InfiltrationStatus.Disconnected;
    }

    /// <summary>
    /// Increases the alert level by the specified amount.
    /// </summary>
    /// <param name="amount">The amount to increase (default 1).</param>
    public void IncreaseAlertLevel(int amount = 1)
    {
        AlertLevel += Math.Max(0, amount);
    }

    // -------------------------------------------------------------------------
    // ICE Encounter Methods (v0.15.4c)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Adds a resolved ICE encounter to the state.
    /// </summary>
    /// <param name="encounter">The resolved encounter to add.</param>
    /// <remarks>
    /// Call this method after resolving an ICE encounter to track the outcome.
    /// The encounter should have a non-Pending resolution outcome.
    /// </remarks>
    public void AddIceEncounter(IceEncounter encounter)
    {
        _iceEncounters.Add(encounter);
    }

    /// <summary>
    /// Sets the terminal to a disconnected state with a lockout duration.
    /// </summary>
    /// <param name="lockoutMinutes">
    /// Minutes until reconnection allowed.
    /// Use -1 for permanent lockout (terminal can never be accessed again).
    /// Use 0 for immediate availability (no lockout).
    /// </param>
    /// <remarks>
    /// <para>
    /// Called when ICE forces a disconnect:
    /// <list type="bullet">
    ///   <item><description>Active ICE failure: 1 minute lockout</description></item>
    ///   <item><description>Lethal ICE save success: 1 minute lockout</description></item>
    ///   <item><description>Lethal ICE save failure: Permanent lockout</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public void SetDisconnected(int lockoutMinutes)
    {
        if (Status == InfiltrationStatus.LockedOut)
        {
            return; // Already in terminal state from fumble
        }

        Status = InfiltrationStatus.Disconnected;

        // Set lockout duration
        LockoutUntil = lockoutMinutes switch
        {
            -1 => DateTime.MaxValue,           // Permanent lockout
            0 => null,                          // No lockout
            _ => DateTime.UtcNow.AddMinutes(lockoutMinutes)  // Temporary lockout
        };
    }

    /// <summary>
    /// Marks the terminal as permanently locked out.
    /// </summary>
    /// <remarks>
    /// Called when Lethal ICE save fails or on terminal fumble.
    /// The terminal can never be accessed again by any character.
    /// </remarks>
    public void MarkAsLockedOut()
    {
        IsLockedOut = true;
        AccessLevel = AccessLevel.Lockout;
        Status = InfiltrationStatus.LockedOut;
        LockoutUntil = DateTime.MaxValue;
    }

    /// <summary>
    /// Clears the lockout if the lockout period has expired.
    /// </summary>
    /// <returns>True if lockout was cleared; false if still active or permanent.</returns>
    /// <remarks>
    /// Call this before allowing retry attempts to check if lockout has expired.
    /// Permanent lockouts (DateTime.MaxValue) can never be cleared.
    /// </remarks>
    public bool TryClearExpiredLockout()
    {
        if (!LockoutUntil.HasValue)
        {
            return true; // No lockout
        }

        if (LockoutUntil == DateTime.MaxValue)
        {
            return false; // Permanent lockout
        }

        if (DateTime.UtcNow >= LockoutUntil.Value)
        {
            LockoutUntil = null;
            Status = InfiltrationStatus.InProgress;
            return true;
        }

        return false; // Still locked out
    }

    // -------------------------------------------------------------------------
    // Private Processing Methods
    // -------------------------------------------------------------------------

    private void ProcessFumble()
    {
        IsLockedOut = true;
        AccessLevel = AccessLevel.Lockout;
        Status = InfiltrationStatus.LockedOut;
        AlertLevel += 5; // Major alert on fumble
    }

    private void ProcessSuccess(LayerResult result)
    {
        switch (CurrentLayer)
        {
            case 1:
                // Layer 1 success: Proceed to Layer 2
                CurrentLayer = 2;
                if (result.IsCriticalSuccess)
                {
                    // Critical on Layer 1 grants Admin immediately
                    AccessLevel = AccessLevel.AdminLevel;
                }
                break;

            case 2:
                // Layer 2 success: Grant access, proceed to Layer 3
                CurrentLayer = 3;
                if (result.IsCriticalSuccess || AccessLevel == AccessLevel.AdminLevel)
                {
                    AccessLevel = AccessLevel.AdminLevel;
                }
                else
                {
                    AccessLevel = AccessLevel.UserLevel;
                }
                break;

            case 3:
                // Layer 3 success: Infiltration complete
                Status = InfiltrationStatus.Completed;
                break;
        }
    }

    private void ProcessFailure(LayerResult result)
    {
        AlertLevel += 1; // Increase alert on failure

        switch (CurrentLayer)
        {
            case 1:
                // Layer 1 failure: Temporary lockout, can retry
                // State remains at Layer 1, DC increase tracked elsewhere
                Status = InfiltrationStatus.TemporaryLockout;
                break;

            case 2:
                // Layer 2 failure: Alert triggered, ICE may activate
                // Note: ICE activation handled by v0.15.4c
                AlertLevel += 2; // Additional alert for Layer 2 failure
                Status = InfiltrationStatus.AlertTriggered;
                break;

            case 3:
                // Layer 3 failure: Partial access only
                Status = InfiltrationStatus.Completed; // Still "completed" but with reduced results
                break;
        }
    }

    // -------------------------------------------------------------------------
    // Display Methods
    // -------------------------------------------------------------------------

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"Infiltration {InfiltrationId}: {TerminalType} " +
               $"Layer {CurrentLayer}, Access: {AccessLevel}, " +
               $"Status: {Status}, Alert: {AlertLevel}";
    }

    /// <summary>
    /// Returns a compact summary for logging.
    /// </summary>
    /// <returns>A log-friendly string representation.</returns>
    public string ToLogString()
    {
        return $"Infiltration[{InfiltrationId}] " +
               $"Terminal={TerminalId} Layer={CurrentLayer} " +
               $"Access={AccessLevel} Status={Status} Alert={AlertLevel}";
    }
}
