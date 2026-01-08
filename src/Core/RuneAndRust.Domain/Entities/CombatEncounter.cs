using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;

namespace RuneAndRust.Domain.Entities;

/// <summary>
/// Represents an active combat encounter with multiple combatants.
/// </summary>
/// <remarks>
/// <para>CombatEncounter is the aggregate root for all combat-related state.
/// It tracks turn order, round progression, and combat resolution.</para>
/// <para>State transitions:</para>
/// <code>
/// NotStarted -> (Start) -> Active -> (CheckForResolution) -> Victory | PlayerDefeated
///                                 -> (EndByFlee) -> Fled
/// </code>
/// </remarks>
public class CombatEncounter : IEntity
{
    // ===== Properties =====

    /// <summary>
    /// Gets the unique identifier for this encounter.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the ID of the room where this encounter is taking place.
    /// </summary>
    public Guid RoomId { get; private set; }

    /// <summary>
    /// Gets the ID of the room the player came from (for flee destination).
    /// </summary>
    public Guid? PreviousRoomId { get; private set; }

    /// <summary>
    /// Gets the current state of the encounter.
    /// </summary>
    public CombatState State { get; private set; } = CombatState.NotStarted;

    /// <summary>
    /// Gets the current round number (1-based).
    /// </summary>
    public int RoundNumber { get; private set; } = 0;

    /// <summary>
    /// Gets the index of the current combatant in turn order.
    /// </summary>
    public int CurrentTurnIndex { get; private set; } = 0;

    /// <summary>
    /// Gets the timestamp when combat started.
    /// </summary>
    public DateTime StartedAt { get; private set; }

    /// <summary>
    /// Gets the timestamp when combat ended (null if ongoing).
    /// </summary>
    public DateTime? EndedAt { get; private set; }

    /// <summary>
    /// All combatants participating in this encounter.
    /// </summary>
    private readonly List<Combatant> _combatants = new();

    /// <summary>
    /// Combat log entries for this encounter.
    /// </summary>
    private readonly List<CombatLogEntry> _combatLog = new();

    // ===== Computed Properties =====

    /// <summary>
    /// Gets a read-only list of all combatants in turn order.
    /// </summary>
    public IReadOnlyList<Combatant> Combatants => _combatants.AsReadOnly();

    /// <summary>
    /// Gets the combatant whose turn it currently is, or null if combat not active.
    /// </summary>
    public Combatant? CurrentCombatant =>
        State == CombatState.Active && CurrentTurnIndex < _combatants.Count
            ? _combatants[CurrentTurnIndex]
            : null;

    /// <summary>
    /// Gets whether combat is currently active.
    /// </summary>
    public bool IsActive => State == CombatState.Active;

    /// <summary>
    /// Gets whether combat has ended (any terminal state).
    /// </summary>
    public bool IsEnded => State is CombatState.Victory or CombatState.PlayerDefeated or CombatState.Fled;

    /// <summary>
    /// Gets whether it is currently the player's turn.
    /// </summary>
    public bool IsPlayerTurn => CurrentCombatant?.IsPlayer ?? false;

    /// <summary>
    /// Gets the total number of combatants.
    /// </summary>
    public int CombatantCount => _combatants.Count;

    /// <summary>
    /// Gets the number of active (non-defeated) monsters.
    /// </summary>
    public int ActiveMonsterCount => _combatants.Count(c => c.IsMonster && c.IsActive);

    // ===== Private Constructor =====

    /// <summary>
    /// Private constructor for factory method pattern and EF Core.
    /// </summary>
    private CombatEncounter() { }

    // ===== Factory Methods =====

    /// <summary>
    /// Creates a new combat encounter in the NotStarted state.
    /// </summary>
    /// <param name="roomId">The ID of the room where combat occurs.</param>
    /// <param name="previousRoomId">The ID of the previous room (for flee destination).</param>
    /// <returns>A new combat encounter ready for combatants to be added.</returns>
    public static CombatEncounter Create(Guid roomId, Guid? previousRoomId = null)
    {
        return new CombatEncounter
        {
            Id = Guid.NewGuid(),
            RoomId = roomId,
            PreviousRoomId = previousRoomId,
            State = CombatState.NotStarted
        };
    }

    // ===== Combatant Management =====

    /// <summary>
    /// Adds a combatant to the encounter.
    /// </summary>
    /// <param name="combatant">The combatant to add.</param>
    /// <exception cref="InvalidOperationException">Thrown if combat has already started.</exception>
    /// <exception cref="ArgumentNullException">Thrown if combatant is null.</exception>
    public void AddCombatant(Combatant combatant)
    {
        ArgumentNullException.ThrowIfNull(combatant);

        if (State != CombatState.NotStarted)
            throw new InvalidOperationException("Cannot add combatants after combat has started.");

        _combatants.Add(combatant);
    }

    // ===== State Transitions =====

    /// <summary>
    /// Starts the encounter, sorting combatants by initiative.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if no combatants or already started.</exception>
    /// <remarks>
    /// Combatants are sorted by initiative (descending), then by Finesse for ties.
    /// Higher values act first.
    /// </remarks>
    public void Start()
    {
        if (_combatants.Count == 0)
            throw new InvalidOperationException("Cannot start combat with no combatants.");
        if (State != CombatState.NotStarted)
            throw new InvalidOperationException("Combat has already started.");

        // Sort by initiative (descending), then by Finesse for ties (descending)
        _combatants.Sort((a, b) =>
        {
            var initiativeCompare = b.Initiative.CompareTo(a.Initiative);
            if (initiativeCompare != 0) return initiativeCompare;
            return b.Finesse.CompareTo(a.Finesse);
        });

        State = CombatState.Active;
        RoundNumber = 1;
        CurrentTurnIndex = 0;
        StartedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Advances to the next combatant's turn, potentially advancing the round.
    /// </summary>
    /// <returns>The new current combatant, or null if combat ended.</returns>
    /// <remarks>
    /// <para>Skips defeated combatants automatically.</para>
    /// <para>When all combatants have acted, advances to the next round and resets acted flags.</para>
    /// </remarks>
    public Combatant? AdvanceTurn()
    {
        if (State != CombatState.Active) return null;

        // Mark current combatant as having acted
        if (CurrentCombatant != null)
            CurrentCombatant.MarkActed();

        // Find next active combatant
        var startIndex = CurrentTurnIndex;
        do
        {
            CurrentTurnIndex++;

            // Check for round end
            if (CurrentTurnIndex >= _combatants.Count)
            {
                CurrentTurnIndex = 0;
                RoundNumber++;
                ResetActedFlags();
            }

            // Skip defeated combatants
            if (_combatants[CurrentTurnIndex].IsActive)
                return _combatants[CurrentTurnIndex];

        } while (CurrentTurnIndex != startIndex);

        // If we've looped completely with no active combatants, combat should be over
        CheckForResolution();
        return CurrentCombatant;
    }

    /// <summary>
    /// Checks if combat should end and updates state accordingly.
    /// </summary>
    /// <remarks>
    /// Call this after any action that could defeat a combatant.
    /// </remarks>
    public void CheckForResolution()
    {
        if (State != CombatState.Active) return;

        var activeMonsters = _combatants.Where(c => c.IsMonster && c.IsActive).ToList();
        var activePlayer = _combatants.FirstOrDefault(c => c.IsPlayer && c.IsActive);

        if (activePlayer == null)
        {
            State = CombatState.PlayerDefeated;
            EndedAt = DateTime.UtcNow;
        }
        else if (activeMonsters.Count == 0)
        {
            State = CombatState.Victory;
            EndedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Ends combat due to player fleeing.
    /// </summary>
    /// <remarks>
    /// Only valid when combat is Active.
    /// </remarks>
    public void EndByFlee()
    {
        if (State != CombatState.Active) return;
        State = CombatState.Fled;
        EndedAt = DateTime.UtcNow;
    }

    // ===== Monster Targeting =====

    /// <summary>
    /// Gets all active (non-defeated) monsters in the encounter.
    /// </summary>
    /// <returns>An enumerable of active monster combatants.</returns>
    public IEnumerable<Combatant> GetActiveMonsters() =>
        _combatants.Where(c => c.IsMonster && c.IsActive);

    /// <summary>
    /// Gets a monster by its display number (1-based indexing).
    /// </summary>
    /// <param name="number">The 1-based display number.</param>
    /// <returns>The monster combatant, or null if not found.</returns>
    /// <remarks>
    /// Numbers are based on the order of active monsters, not their display numbers.
    /// </remarks>
    public Combatant? GetMonsterByNumber(int number)
    {
        if (number < 1) return null;
        return GetActiveMonsters().ElementAtOrDefault(number - 1);
    }

    /// <summary>
    /// Gets all monsters matching a name (case-insensitive partial match).
    /// </summary>
    /// <param name="name">The name or partial name to match.</param>
    /// <returns>Enumerable of matching monster combatants.</returns>
    public IEnumerable<Combatant> GetMonstersByName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Enumerable.Empty<Combatant>();

        return GetActiveMonsters().Where(c =>
            c.DisplayName.Contains(name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets the player combatant.
    /// </summary>
    /// <returns>The player combatant, or null if not found.</returns>
    public Combatant? GetPlayerCombatant() =>
        _combatants.FirstOrDefault(c => c.IsPlayer);

    // ===== Combat Log (v0.0.6b) =====

    /// <summary>
    /// Gets the combat log for this encounter.
    /// </summary>
    public IReadOnlyList<CombatLogEntry> CombatLog => _combatLog.AsReadOnly();

    /// <summary>
    /// Adds an entry to the combat log.
    /// </summary>
    /// <param name="entry">The log entry to add.</param>
    public void AddLogEntry(CombatLogEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        _combatLog.Add(entry);
    }

    /// <summary>
    /// Gets the most recent log entries.
    /// </summary>
    /// <param name="count">Maximum number of entries to return.</param>
    /// <returns>The most recent entries, up to the specified count.</returns>
    public IEnumerable<CombatLogEntry> GetRecentLogEntries(int count = 10) =>
        _combatLog.TakeLast(count);

    /// <summary>
    /// Gets log entries for a specific round.
    /// </summary>
    /// <param name="round">The round number to filter by.</param>
    /// <returns>All entries from the specified round.</returns>
    public IEnumerable<CombatLogEntry> GetLogEntriesForRound(int round) =>
        _combatLog.Where(e => e.RoundNumber == round);

    // ===== AI Helpers (v0.0.6b) =====

    /// <summary>
    /// Gets active ally combatants for a monster (excluding self).
    /// </summary>
    /// <param name="self">The monster requesting allies.</param>
    /// <returns>List of active monster allies.</returns>
    public IReadOnlyList<Combatant> GetAlliesFor(Combatant self)
    {
        ArgumentNullException.ThrowIfNull(self);
        return _combatants
            .Where(c => c.IsMonster && c.IsActive && c.Id != self.Id)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Gets active enemy combatants for a monster (the player).
    /// </summary>
    /// <returns>List of active player combatants (usually just one).</returns>
    public IReadOnlyList<Combatant> GetEnemiesForMonster() =>
        _combatants
            .Where(c => c.IsPlayer && c.IsActive)
            .ToList()
            .AsReadOnly();

    /// <summary>
    /// Removes a monster from combat (when it flees).
    /// </summary>
    /// <param name="combatant">The monster combatant to remove.</param>
    public void RemoveMonster(Combatant combatant)
    {
        ArgumentNullException.ThrowIfNull(combatant);
        if (combatant.IsPlayer)
            throw new InvalidOperationException("Cannot remove player using this method");

        // Mark as defeated for removal from turn order
        combatant.Monster?.TakeDamage(combatant.MaxHealth + 1);
    }

    // ===== Private Helpers =====

    /// <summary>
    /// Resets the "has acted" flag for all active combatants.
    /// </summary>
    private void ResetActedFlags()
    {
        foreach (var combatant in _combatants.Where(c => c.IsActive))
            combatant.ResetActed();
    }

    /// <inheritdoc />
    public override string ToString() =>
        $"Encounter (Round {RoundNumber}, {State}, {ActiveMonsterCount} monsters active)";
}
