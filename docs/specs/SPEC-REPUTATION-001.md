# Reputation System: Design Specification

## Document Control

| Field               | Value                                                                   |
| :------------------ | :---------------------------------------------------------------------- |
| **Document ID**     | RR-DES-REPUTATION-001                                                   |
| **Feature Name**    | Faction Reputation System                                               |
| **Module Scope**    | Domain (Entities/ValueObjects) + Application (Services/Interfaces) + Infrastructure (Providers) + Config |
| **Status**          | Draft                                                                   |
| **Author**          | Claude (AI Assistant)                                                   |
| **Date**            | 2026-02-28                                                              |
| **Reviewers**       | Ryan (Project Owner)                                                    |
| **Est. Hours**      | 8-12 (implementation + tests)                                           |
| **Parent Document** | `docs/design/aethelgard/design/02-entities/faction-reputation.md` (v1.2)|

---

## Problem Statement

Rune & Rust currently has **no faction reputation tracking**. The `Player` entity (2234 lines, 5 specializations, currency, skills, quests) lacks any field for faction standing, and no service exists to modify, query, or gate gameplay based on faction relationships. Meanwhile, the game's quest infrastructure already assumes reputation exists:

- `QuestRewardDto.ReputationChanges` carries `IReadOnlyDictionary<string, int>` but nothing consumes it.
- `FailureCheckContext.FactionReputations` passes `IReadOnlyDictionary<string, int>` to failure checks, but no caller populates it with real data.
- `FailureConditionDto` supports `Type = "ReputationDropped"` with a threshold, but no service evaluates it.
- Legacy quest files (31 total) already define `ReputationGains` like `{ "IronBanes": 40, "GodSleeperCultists": -60 }`.

Without the reputation system, quest rewards silently discard reputation changes, failure conditions can't trigger on reputation thresholds, and the planned settlement service gating (Section 9 of the design doc) is impossible.

**Why now?** Reputation is a dependency for Tier 3 Faction Quest Chains. Quest chains need to gate availability by faction standing, apply reputation deltas on completion, and trigger failure when reputation drops below thresholds. Building quest chains without reputation would mean stubbing out the most narratively impactful mechanic.

---

## Proposed Solution

Implement a three-layer reputation system that follows the project's existing Clean Architecture pattern:

1. **Domain Layer** — New value object `FactionReputation` and new enum `ReputationTier`. Add a reputation dictionary to `Player`.
2. **Application Layer** — New `IReputationService` interface and `ReputationService` implementation. New `IFactionDefinitionProvider` interface for loading faction metadata from config.
3. **Infrastructure Layer** — New `JsonFactionDefinitionProvider` to load `config/factions.json`. New `GameEvent` factory method for reputation events.
4. **Configuration** — New `config/factions.json` defining the 5 factions with their tier thresholds, ally/enemy relationships, and base modifiers.

### Data Flow

```
Quest Completion
    ↓
GameSessionService calls ReputationService.ApplyReputationChanges(playerId, changes, witnessContext)
    ↓
ReputationService:
  1. Loads faction definitions from IFactionDefinitionProvider
  2. Applies witness multiplier (100% / 75% / 0%)
  3. Clamps result to [-100, +100]
  4. Detects tier transitions
  5. Emits GameEvent for each change
  6. Returns ReputationChangeResult (old tier, new tier, delta, messages)
    ↓
GameSessionService renders narrative messages to player
```

### Design Decisions

- **Reputation stored on Player, not in a separate aggregate.** The Player entity already holds quests, skills, currency, equipment, and specialization data. Faction standing is a direct player attribute (you don't have "a reputation" independently — it belongs to the character). This follows the existing pattern.
- **Value object, not entity.** `FactionReputation` is an immutable value object (like `Stats`, `PlayerAttributes`). The full dictionary of reputations is stored as `Dictionary<string, FactionReputation>` on Player, exposed as `IReadOnlyDictionary`.
- **Witness system deferred to Phase 2.** The witness multiplier (100%/75%/0%) is architecturally supported in the interface (the `WitnessContext` parameter), but the initial implementation will default to `Direct` (100%) for all changes. Full witness detection requires NPC proximity/line-of-sight calculations that depend on the combat grid system.
- **Settlement service gating deferred.** Section 9 of the design doc describes service gating by reputation. This will be implemented when the Settlement system is built (Tier 4+). The reputation infrastructure supports it — we just don't have settlements yet.

---

## Architecture

### Component Diagram

```
┌─────────────────────────────────────────────────────────────┐
│ Domain Layer (RuneAndRust.Domain)                           │
│                                                             │
│  Enums/                                                     │
│    ReputationTier.cs        ← 6 tiers (Hated → Exalted)    │
│    EventCategory.cs         ← Add: Reputation               │
│                                                             │
│  ValueObjects/                                              │
│    FactionReputation.cs     ← Immutable (faction, value,    │
│                                tier, modifiers)              │
│    ReputationChangeResult.cs← Delta, old/new tier, message  │
│    WitnessContext.cs        ← Witness type enum + multiplier│
│                                                             │
│  Entities/                                                  │
│    Player.cs                ← Add _factionReputations dict  │
│                                + Get/Set/Modify methods     │
│                                                             │
│  Events/                                                    │
│    GameEvent.cs             ← Add Reputation() factory      │
│                                                             │
├─────────────────────────────────────────────────────────────┤
│ Application Layer (RuneAndRust.Application)                 │
│                                                             │
│  Interfaces/                                                │
│    IReputationService.cs    ← Core service interface        │
│    IFactionDefinitionProvider.cs ← Config loading           │
│                                                             │
│  DTOs/                                                      │
│    FactionDefinitionDto.cs  ← Faction metadata from config  │
│                                                             │
│  Services/                                                  │
│    ReputationService.cs     ← Business logic implementation │
│                                                             │
├─────────────────────────────────────────────────────────────┤
│ Infrastructure Layer (RuneAndRust.Infrastructure)           │
│                                                             │
│  Providers/                                                 │
│    JsonFactionDefinitionProvider.cs ← Loads factions.json   │
│                                                             │
├─────────────────────────────────────────────────────────────┤
│ Configuration                                               │
│                                                             │
│  config/factions.json       ← 5 faction definitions         │
└─────────────────────────────────────────────────────────────┘
```

### File Location Map

| New File | Layer | Purpose |
|----------|-------|---------|
| `src/Core/RuneAndRust.Domain/Enums/ReputationTier.cs` | Domain | 6-tier enum |
| `src/Core/RuneAndRust.Domain/ValueObjects/FactionReputation.cs` | Domain | Immutable reputation value |
| `src/Core/RuneAndRust.Domain/ValueObjects/ReputationChangeResult.cs` | Domain | Change result with tier transition |
| `src/Core/RuneAndRust.Domain/ValueObjects/WitnessContext.cs` | Domain | Witness type + multiplier |
| `src/Core/RuneAndRust.Application/Interfaces/IReputationService.cs` | Application | Service contract |
| `src/Core/RuneAndRust.Application/Interfaces/IFactionDefinitionProvider.cs` | Application | Faction data provider |
| `src/Core/RuneAndRust.Application/DTOs/FactionDefinitionDto.cs` | Application | Faction config DTO |
| `src/Core/RuneAndRust.Application/Services/ReputationService.cs` | Application | Core logic |
| `src/Infrastructure/RuneAndRust.Infrastructure/Providers/JsonFactionDefinitionProvider.cs` | Infra | JSON loader |
| `config/factions.json` | Config | Faction definitions |

### Modified Files

| Existing File | Change |
|---------------|--------|
| `src/Core/RuneAndRust.Domain/Entities/Player.cs` | Add `_factionReputations` dictionary + methods |
| `src/Core/RuneAndRust.Domain/Enums/EventCategory.cs` | Add `Reputation` member |
| `src/Core/RuneAndRust.Domain/Events/GameEvent.cs` | Add `Reputation()` factory method |

---

## Data Contract / API

### ReputationTier Enum

```csharp
/// <summary>
/// Represents the player's standing with a faction.
/// Tiers map to reputation value ranges per the faction-reputation design doc (v1.2).
/// </summary>
public enum ReputationTier
{
    /// <summary>Kill on sight, +50% prices. Range: -100 to -76.</summary>
    Hated = 0,

    /// <summary>Attack if provoked, +25% prices. Range: -75 to -26.</summary>
    Hostile = 1,

    /// <summary>Standard interactions. Range: -25 to +24.</summary>
    Neutral = 2,

    /// <summary>-10% prices, some quests available. Range: +25 to +49.</summary>
    Friendly = 3,

    /// <summary>-20% prices, most content available. Range: +50 to +74.</summary>
    Allied = 4,

    /// <summary>-30% prices, all rewards available. Range: +75 to +100.</summary>
    Exalted = 5
}
```

### FactionReputation Value Object

```csharp
/// <summary>
/// Immutable value object representing a player's reputation with a single faction.
/// </summary>
/// <remarks>
/// <para>Reputation is clamped to [-100, +100]. Tier is derived from the value.</para>
/// <para>Follows the immutable pattern used by Stats and PlayerAttributes.</para>
/// </remarks>
public readonly record struct FactionReputation
{
    public const int MinValue = -100;
    public const int MaxValue = 100;

    /// <summary>The faction ID this reputation is with.</summary>
    public string FactionId { get; }

    /// <summary>The current reputation value, clamped to [-100, +100].</summary>
    public int Value { get; }

    /// <summary>The current tier derived from Value.</summary>
    public ReputationTier Tier { get; }

    /// <summary>The price modifier for this tier (e.g., 1.5 for Hated, 0.7 for Exalted).</summary>
    public double PriceModifier { get; }

    /// <summary>Creates a new FactionReputation at Neutral (0).</summary>
    public static FactionReputation Neutral(string factionId);

    /// <summary>Creates a FactionReputation at a specific value.</summary>
    public static FactionReputation Create(string factionId, int value);

    /// <summary>Returns a new FactionReputation with the value adjusted by delta.</summary>
    public FactionReputation WithDelta(int delta);

    /// <summary>Derives the ReputationTier from a given value.</summary>
    public static ReputationTier GetTierForValue(int value);

    /// <summary>Gets the price modifier for a given tier.</summary>
    public static double GetPriceModifierForTier(ReputationTier tier);
}
```

**Tier boundary table (encoded in `GetTierForValue`):**

| Tier | Min | Max |
|------|-----|-----|
| Hated | -100 | -76 |
| Hostile | -75 | -26 |
| Neutral | -25 | +24 |
| Friendly | +25 | +49 |
| Allied | +50 | +74 |
| Exalted | +75 | +100 |

### WitnessContext Value Object

```csharp
/// <summary>
/// Context describing how a reputation-affecting action was observed.
/// </summary>
/// <remarks>
/// Phase 1: All actions default to Direct (100% reputation change).
/// Phase 2: Will integrate with NPC proximity/line-of-sight for Witnessed (75%) and Unwitnessed (0%).
/// </remarks>
public readonly record struct WitnessContext
{
    public WitnessType Type { get; }
    public double Multiplier { get; }

    public static WitnessContext Direct => new(WitnessType.Direct, 1.0);
    public static WitnessContext Witnessed => new(WitnessType.Witnessed, 0.75);
    public static WitnessContext Unwitnessed => new(WitnessType.Unwitnessed, 0.0);
}

public enum WitnessType
{
    /// <summary>Player directly interacted with the faction. 100% rep change.</summary>
    Direct,
    /// <summary>A faction member saw the action. 75% rep change.</summary>
    Witnessed,
    /// <summary>No faction members present. 0% rep change.</summary>
    Unwitnessed
}
```

### ReputationChangeResult Value Object

```csharp
/// <summary>
/// Result of applying a reputation change to a player.
/// </summary>
public record ReputationChangeResult
{
    /// <summary>The faction ID affected.</summary>
    public string FactionId { get; init; } = string.Empty;

    /// <summary>The faction display name.</summary>
    public string FactionName { get; init; } = string.Empty;

    /// <summary>The raw delta before witness multiplier.</summary>
    public int RawDelta { get; init; }

    /// <summary>The actual delta applied after witness multiplier and clamping.</summary>
    public int ActualDelta { get; init; }

    /// <summary>The new reputation value after the change.</summary>
    public int NewValue { get; init; }

    /// <summary>The tier before the change.</summary>
    public ReputationTier OldTier { get; init; }

    /// <summary>The tier after the change.</summary>
    public ReputationTier NewTier { get; init; }

    /// <summary>Whether a tier transition occurred.</summary>
    public bool TierChanged => OldTier != NewTier;

    /// <summary>Human-readable message for the player (e.g., "+10 Iron-Bane Reputation").</summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>Optional tier transition message (e.g., "Your standing with Iron-Banes is now Friendly!").</summary>
    public string? TierTransitionMessage { get; init; }
}
```

### Player Extensions (added to Player.cs)

```csharp
// ===== Faction Reputation Properties (SPEC-REPUTATION-001) =====

/// <summary>
/// Backing field for faction reputation tracking.
/// </summary>
private readonly Dictionary<string, FactionReputation> _factionReputations = new(StringComparer.OrdinalIgnoreCase);

/// <summary>
/// Gets a read-only view of the player's faction reputations.
/// </summary>
public IReadOnlyDictionary<string, FactionReputation> FactionReputations => _factionReputations;

// ===== Faction Reputation Methods (SPEC-REPUTATION-001) =====

/// <summary>
/// Gets the player's reputation with a specific faction.
/// Returns Neutral (0) if no reputation has been established.
/// </summary>
public FactionReputation GetFactionReputation(string factionId);

/// <summary>
/// Sets the reputation for a faction to a specific value object.
/// Used by ReputationService after calculating changes.
/// </summary>
public void SetFactionReputation(FactionReputation reputation);

/// <summary>
/// Gets the reputation tier for a specific faction.
/// Returns Neutral if no reputation exists.
/// </summary>
public ReputationTier GetFactionTier(string factionId);

/// <summary>
/// Gets faction reputations as a simple dictionary for FailureCheckContext.
/// </summary>
public IReadOnlyDictionary<string, int> GetFactionReputationValues();
```

### IReputationService Interface

```csharp
/// <summary>
/// Service for managing player faction reputation.
/// </summary>
public interface IReputationService
{
    /// <summary>
    /// Applies reputation changes from a quest reward or game action.
    /// </summary>
    /// <param name="player">The player whose reputation to modify.</param>
    /// <param name="reputationChanges">Dictionary of faction ID → delta.</param>
    /// <param name="witnessContext">How the action was observed.</param>
    /// <returns>List of change results, one per affected faction.</returns>
    IReadOnlyList<ReputationChangeResult> ApplyReputationChanges(
        Player player,
        IReadOnlyDictionary<string, int> reputationChanges,
        WitnessContext witnessContext);

    /// <summary>
    /// Gets the player's current tier with a faction.
    /// </summary>
    ReputationTier GetTier(Player player, string factionId);

    /// <summary>
    /// Checks whether the player meets a minimum reputation requirement.
    /// </summary>
    bool MeetsReputationRequirement(Player player, string factionId, int minimumValue);

    /// <summary>
    /// Checks whether the player meets a minimum tier requirement.
    /// </summary>
    bool MeetsTierRequirement(Player player, string factionId, ReputationTier minimumTier);

    /// <summary>
    /// Gets the display name for a faction.
    /// </summary>
    string GetFactionName(string factionId);

    /// <summary>
    /// Gets all known faction IDs.
    /// </summary>
    IReadOnlyList<string> GetAllFactionIds();
}
```

### IFactionDefinitionProvider Interface

```csharp
/// <summary>
/// Provider for faction definitions loaded from configuration.
/// </summary>
public interface IFactionDefinitionProvider
{
    /// <summary>Gets all faction definitions.</summary>
    IReadOnlyList<FactionDefinitionDto> GetAllFactions();

    /// <summary>Gets a faction by ID (case-insensitive).</summary>
    FactionDefinitionDto? GetFaction(string factionId);

    /// <summary>Checks if a faction exists.</summary>
    bool FactionExists(string factionId);

    /// <summary>Gets the IDs of factions allied with the specified faction.</summary>
    IReadOnlyList<string> GetAllies(string factionId);

    /// <summary>Gets the IDs of factions hostile to the specified faction.</summary>
    IReadOnlyList<string> GetEnemies(string factionId);
}
```

### FactionDefinitionDto

```csharp
/// <summary>
/// Data transfer object for faction definitions loaded from config/factions.json.
/// </summary>
public record FactionDefinitionDto
{
    /// <summary>Unique faction identifier (e.g., "iron-banes").</summary>
    public string FactionId { get; init; } = string.Empty;

    /// <summary>Display name (e.g., "Iron-Banes").</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Faction philosophy description.</summary>
    public string Philosophy { get; init; } = string.Empty;

    /// <summary>Primary location in the world.</summary>
    public string Location { get; init; } = string.Empty;

    /// <summary>IDs of allied factions.</summary>
    public IReadOnlyList<string> Allies { get; init; } = [];

    /// <summary>IDs of enemy factions.</summary>
    public IReadOnlyList<string> Enemies { get; init; } = [];

    /// <summary>Default starting reputation value for new players (typically 0).</summary>
    public int DefaultReputation { get; init; } = 0;
}
```

### config/factions.json Structure

```json
{
  "factions": [
    {
      "factionId": "iron-banes",
      "name": "Iron-Banes",
      "philosophy": "The Undying are corrupted processes that must be purged.",
      "location": "Trunk/Roots patrols",
      "allies": ["rust-clans"],
      "enemies": ["god-sleeper-cultists"],
      "defaultReputation": 0
    },
    {
      "factionId": "god-sleeper-cultists",
      "name": "God-Sleeper Cultists",
      "philosophy": "The Jotun-Forged are sleeping gods awaiting the signal to awaken.",
      "location": "Jotunheim temples",
      "allies": [],
      "enemies": ["iron-banes"],
      "defaultReputation": 0
    },
    {
      "factionId": "jotun-readers",
      "name": "Jotun-Readers",
      "philosophy": "Knowledge is the only path to understanding the Glitch.",
      "location": "Alfheim, terminals",
      "allies": ["rust-clans"],
      "enemies": [],
      "defaultReputation": 0
    },
    {
      "factionId": "rust-clans",
      "name": "Rust-Clans",
      "philosophy": "Survival first. No ideology, no worship, no grand theories.",
      "location": "Midgard, trade outposts",
      "allies": ["iron-banes", "jotun-readers"],
      "enemies": [],
      "defaultReputation": 0
    },
    {
      "factionId": "independents",
      "name": "Independents",
      "philosophy": "Factions are chains. We walk our own path.",
      "location": "Anywhere",
      "allies": [],
      "enemies": [],
      "defaultReputation": 0
    }
  ]
}
```

### GameEvent Extension

```csharp
// Added to GameEvent.cs
/// <summary>
/// Creates a reputation event.
/// </summary>
public static GameEvent Reputation(string eventType, string message, Guid? playerId = null) =>
    new() { Category = EventCategory.Reputation, EventType = eventType, Message = message, PlayerId = playerId };
```

---

## Constraints

1. **Player.cs is already 2234 lines.** The reputation additions (~60 lines: 1 dictionary field, 4 methods, XML docs) are modest compared to specialization sections (which average 100-200 lines each). No refactoring needed yet.
2. **Backward compatibility.** Existing save states won't have faction reputations. `GetFactionReputation()` returns Neutral (0) for unknown factions, so deserialization of old saves is safe.
3. **No persistence layer changes.** The project uses in-memory state during sessions. Reputation is persisted with the Player entity the same way quests and skills are — whatever serialization mechanism exists covers it.
4. **Quest reward integration must not break existing flow.** `QuestRewardDto.ReputationChanges` already exists but is ignored. We'll wire it up, but if the dictionary is empty (which it is for quests without reputation changes), no reputation logic triggers.
5. **Faction ID normalization.** All IDs are lowercased and use `StringComparer.OrdinalIgnoreCase` dictionaries, matching the existing patterns in Player (currency, skills, resources).

---

## Alternatives Considered

### Alternative 1: Separate ReputationTracker Entity

Store reputation in a standalone `ReputationTracker` entity with its own ID, separate from Player. The Player would hold a reference by ID.

**Pros:** Cleaner separation, Player.cs doesn't grow further, reusable for NPCs.

**Cons:** Adds entity management complexity (lifecycle, persistence, queries), breaks the established pattern where all player state lives on the Player entity, requires a new repository or lookup service. Every other player subsystem (quests, skills, currency, equipment, specializations) lives directly on Player. Extracting reputation alone would be inconsistent.

**Verdict:** Rejected. The consistency argument is strong — reputation is player state just like currency or skills.

### Alternative 2: Event-Sourced Reputation

Store reputation as a log of `ReputationEvent` records and derive current standing by replaying them.

**Pros:** Full audit trail, undo support, temporal queries ("what was my reputation 10 turns ago?").

**Cons:** Dramatically more complex than needed, performance overhead for frequent queries (every quest availability check, every merchant interaction), no existing event-sourcing infrastructure in the project, and the design doc doesn't require audit trails or temporal queries.

**Verdict:** Rejected. Over-engineered for the current requirements. Can always be added later if audit trails become a need.

### Alternative 3: Chosen Approach (Dictionary on Player + Service)

Reputation lives on Player as a simple dictionary. A `ReputationService` handles all business logic (clamping, tier derivation, witness multipliers, event emission).

**Pros:** Follows existing patterns exactly, minimal new infrastructure, easy to test, integrates with existing `FailureCheckContext` and `QuestRewardDto` contracts.

**Cons:** Player.cs grows slightly, but only by ~60 lines — far less than any specialization section.

**Verdict:** Selected. Best fit for the project's current architecture and complexity level.

---

## Error Handling

| Error Case | Handling Strategy | What the Caller Sees |
|------------|-------------------|---------------------|
| Unknown faction ID in reputation change | Log warning, skip that faction entry, continue with valid entries | `ReputationChangeResult` list excludes the unknown faction; no exception |
| Null or empty `reputationChanges` dictionary | Return empty list immediately | Empty `IReadOnlyList<ReputationChangeResult>` — no-op |
| Witness multiplier results in zero delta | Include in results with `ActualDelta = 0` | Result present but with zero change — caller can choose to display or not |
| Player is null | `ArgumentNullException` from guard clause | Standard .NET exception, caught by caller |
| FactionId is null/whitespace in `GetTier` | Return `ReputationTier.Neutral` | Graceful degradation — unknown faction = neutral standing |
| Config file missing or malformed | `JsonFactionDefinitionProvider` logs error, initializes with empty collection | `GetAllFactions()` returns empty list; all faction lookups return null; reputation changes are no-ops (unknown faction path) |
| Reputation value overflow (int arithmetic) | `Math.Clamp` to [-100, +100] applied after every delta | Always within valid range |

---

## Performance Considerations

- **Dictionary lookup per faction:** O(1) for case-insensitive `StringComparer.OrdinalIgnoreCase`. With 5 factions, performance is irrelevant.
- **Tier derivation:** A chain of 5 `if` comparisons — effectively O(1).
- **Quest availability checks querying reputation:** Same O(1) dictionary lookup. No performance concern even if called per quest per frame.
- **Config loading:** One-time on startup. 5 faction definitions — trivial.

No performance targets needed. This system handles <10 items and O(1) operations.

---

## Success Criteria

1. `Player.GetFactionReputation("iron-banes")` returns a `FactionReputation` with correct value and tier.
2. `ReputationService.ApplyReputationChanges()` correctly applies deltas with clamping, derives tiers, and detects transitions.
3. Quest completion wires through `QuestRewardDto.ReputationChanges` to `ReputationService` — reputation actually changes when a quest with reputation rewards is completed.
4. `FailureCheckContext.FactionReputations` is populated from real `Player.GetFactionReputationValues()` data.
5. `config/factions.json` loads without errors and exposes all 5 factions through the provider.
6. All 5 factions have correct ally/enemy relationships per the design doc.
7. Tier boundaries match the design doc table exactly (verified by unit tests at every boundary value).

---

## Acceptance Criteria

| #   | Category      | Criterion                                                                 | Verification      |
| --- | ------------- | ------------------------------------------------------------------------- | ----------------- |
| 1   | Domain        | `ReputationTier` enum has 6 values (Hated through Exalted)              | Unit test         |
| 2   | Domain        | `FactionReputation.Create("iron-banes", -76)` returns Tier = Hated      | Unit test         |
| 3   | Domain        | `FactionReputation.Create("iron-banes", -75)` returns Tier = Hostile    | Unit test         |
| 4   | Domain        | `FactionReputation.WithDelta()` clamps to [-100, +100]                  | Unit test         |
| 5   | Domain        | `Player.GetFactionReputation()` returns Neutral for unknown factions     | Unit test         |
| 6   | Domain        | `Player.SetFactionReputation()` stores and retrieves correctly           | Unit test         |
| 7   | Service       | `ApplyReputationChanges` with empty dict returns empty list              | Unit test         |
| 8   | Service       | `ApplyReputationChanges` applies correct delta and returns results       | Unit test         |
| 9   | Service       | `ApplyReputationChanges` detects tier transitions                        | Unit test         |
| 10  | Service       | `MeetsReputationRequirement` returns true/false correctly                | Unit test         |
| 11  | Service       | `MeetsTierRequirement` returns true/false correctly                      | Unit test         |
| 12  | Provider      | `JsonFactionDefinitionProvider` loads all 5 factions from config         | Integration test  |
| 13  | Provider      | `GetAllies("iron-banes")` returns `["rust-clans"]`                      | Integration test  |
| 14  | Provider      | `GetEnemies("iron-banes")` returns `["god-sleeper-cultists"]`           | Integration test  |
| 15  | Event         | `EventCategory.Reputation` exists in the enum                           | Unit test         |
| 16  | Event         | `GameEvent.Reputation()` creates event with correct category            | Unit test         |
| 17  | Integration   | Tier boundary values match design doc at every boundary (-100, -76, -75, -26, -25, +24, +25, +49, +50, +74, +75, +100) | Unit test |
| 18  | Build         | Solution builds with zero warnings                                      | `dotnet build`    |
| 19  | Tests         | All new tests pass, all existing tests still pass                       | `dotnet test`     |

---

## Open Questions

| # | Question | Owner | Resolution Deadline |
|---|----------|-------|---------------------|
| 1 | Should faction reputation changes cascade to allied/enemy factions automatically (e.g., +10 Iron-Banes also gives -5 God-Sleeper Cultists)? The design doc implies mutual exclusivity through quest content rather than automatic cascading. | Ryan | Before Phase 2 implementation |
| 2 | Should the Independents faction have a special mechanic (gaining rep by NOT joining other factions, per the "Lone Wolf" design)? Or is it just a standard faction with its own quest line? | Ryan | Before Faction Quest Chains spec |
| 3 | The design doc mentions 5 factions, but Section 9.4 references additional factions (Midgard Combine, Rangers Guild, Dvergr Hegemony, Scavenger Barons, Hearth-Clans). Are these future factions, or do they already exist? | Ryan | Before Settlement system |

---

## Dependencies

### This Feature Depends On

| Dependency | Status | Notes |
|------------|--------|-------|
| `Player.cs` entity | ✅ Exists | Adding reputation fields |
| `QuestRewardDto.ReputationChanges` | ✅ Exists | Already defined, currently unused |
| `FailureCheckContext.FactionReputations` | ✅ Exists | Already defined, currently stubbed |
| `GameEvent` event system | ✅ Exists | Adding factory method |
| `EventCategory` enum | ✅ Exists | Adding Reputation member |
| `System.Text.Json` | ✅ Available | For config loading (already used by other providers) |

### Depends on This Feature

| Downstream | Status | Notes |
|------------|--------|-------|
| Faction Quest Chains (SPEC-FACTION-QUESTS-001) | Pending | Needs `MeetsTierRequirement()` for quest gating |
| Settlement Service Gating | Future (Tier 4+) | Needs `GetTier()` and `PriceModifier` |
| Merchant Pricing | Future | Needs `FactionReputation.PriceModifier` |
| Quest Failure Detection | Current (to be wired) | Needs `GetFactionReputationValues()` for `FailureCheckContext` |

---

## Development Standards

### Changelog Requirements

- One entry per new file with brief purpose description.
- One entry for each modified file explaining what was added.
- Format: `- [Added] ...` / `- [Changed] ...` per existing project convention.
- Audience: Developer team.

### Logging Standards

| Level | Use For | Example |
|-------|---------|---------|
| DEBUG | Reputation calculation details | `"Applying {RawDelta} to {FactionId}, witness={WitnessType}, actual={ActualDelta}"` |
| INFO | Reputation changes applied | `"Reputation with {FactionName}: {Delta:+#;-#} (now {NewValue})"` |
| INFO | Tier transitions | `"Standing with {FactionName} changed: {OldTier} → {NewTier}"` |
| WARN | Unknown faction ID in change request | `"Unknown faction '{FactionId}' in reputation changes, skipping"` |
| ERROR | Config loading failure | `"Failed to load factions.json from {Path}: {Error}"` |

No sensitive data in logs. Structured logging with named placeholders per existing `ILogger<T>` patterns.

### Unit Testing Expectations

- **Must test:** All tier boundary values, clamping behavior, witness multiplier math, tier transition detection, Player getter/setter round-trips, empty/null input handling.
- **Can skip:** Private constructors, trivial property getters on DTOs.
- **Naming convention:** `[MethodUnderTest]_[Scenario]_[ExpectedResult]` (matches existing test naming).
- **Framework:** NUnit + FluentAssertions + Moq (matches project standard).
- **Coverage target:** 100% of public methods on value objects and service. Integration tests for provider.

### Dependency Tracking

| Dependency | Version | Reason |
|------------|---------|--------|
| NUnit | Existing | Test framework |
| FluentAssertions | Existing | Assertion library |
| Moq | Existing | Mock framework |
| System.Text.Json | .NET 9 built-in | Config deserialization |

No new package dependencies. All testing frameworks already referenced by existing test projects.

---

## Deliverable Checklist

| #  | Deliverable | Status |
| -- | ----------- | ------ |
| 1  | `ReputationTier.cs` enum | Pending |
| 2  | `FactionReputation.cs` value object | Pending |
| 3  | `ReputationChangeResult.cs` value object | Pending |
| 4  | `WitnessContext.cs` value object | Pending |
| 5  | `Player.cs` reputation extensions | Pending |
| 6  | `EventCategory.cs` + Reputation member | Pending |
| 7  | `GameEvent.cs` + Reputation() factory | Pending |
| 8  | `IReputationService.cs` interface | Pending |
| 9  | `IFactionDefinitionProvider.cs` interface | Pending |
| 10 | `FactionDefinitionDto.cs` DTO | Pending |
| 11 | `ReputationService.cs` implementation | Pending |
| 12 | `JsonFactionDefinitionProvider.cs` provider | Pending |
| 13 | `config/factions.json` configuration | Pending |
| 14 | Unit tests: `FactionReputationTests.cs` | Pending |
| 15 | Unit tests: `ReputationServiceTests.cs` | Pending |
| 16 | Integration tests: `JsonFactionDefinitionProviderTests.cs` | Pending |
| 17 | Unit tests: `ReputationTierTests.cs` | Pending |

---

## Document History

| Version | Date       | Author | Changes                     |
| ------- | ---------- | ------ | --------------------------- |
| 1.0     | 2026-02-28 | Claude | Initial design specification |
