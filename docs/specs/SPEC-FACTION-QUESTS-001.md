# Faction Quest Chains: Design Specification

## Document Control

| Field               | Value                                                                   |
| :------------------ | :---------------------------------------------------------------------- |
| **Document ID**     | RR-DES-FACTION-QUESTS-001                                               |
| **Feature Name**    | Faction Quest Chain Infrastructure                                      |
| **Module Scope**    | Application (Services/Interfaces) + Config (quests.json expansion)      |
| **Status**          | Draft                                                                   |
| **Author**          | Claude (AI Assistant)                                                   |
| **Date**            | 2026-02-28                                                              |
| **Reviewers**       | Ryan (Project Owner)                                                    |
| **Est. Hours**      | 6-8 (implementation + tests)                                            |
| **Parent Document** | `docs/design/aethelgard/design/02-entities/faction-reputation.md` (v1.2)|

---

## Problem Statement

The quest system has the **data contracts** for faction chains (`QuestDefinitionDto.ChainId`, `.Faction`, `.PrerequisiteQuestIds`, `.Rewards.UnlockedQuestIds`, `.Rewards.ReputationChanges`) but lacks the **service logic** to:

1. **Gate quest availability by reputation.** `IQuestDefinitionProvider.GetAvailableQuests()` filters by legend level and completed prerequisites, but it completely ignores faction reputation. A player at Hated (-100) with Iron-Banes can still accept Iron-Bane faction quests.

2. **Present chain-level progression.** The quest journal shows individual quests but has no concept of "you're on step 3 of 5 in the Iron-Bane Initiation chain." Players can't see the overall faction quest arc.

3. **Track which chains exist and their state.** There's no service that knows about quest chains as a first-class concept. You can look up quests by chain ID, but there's no `GetChainProgress()`, `GetNextQuestInChain()`, or `GetAvailableChains()`.

The config data is partially populated: 5 chains exist across the 5 factions (ironbane_initiation, rustclan_ascent, jotunreader_path, independent_way, godsleeper_revelation) with 7 faction quests total. Most chains only have 1-2 quests of a planned 3-5. The chains need to be fleshed out with additional quest definitions.

**Why now?** The Reputation System (SPEC-REPUTATION-001) provides the gating mechanism. Without the chain service and reputation checks, quest rewards that grant reputation have no effect on quest availability — defeating the core narrative loop where "doing quests for a faction → improves standing → unlocks harder faction quests."

---

## Proposed Solution

Build on the existing quest infrastructure with two additions:

1. **Reputation-Aware Quest Filtering** — Extend `IQuestDefinitionProvider` (or add a wrapper service) to filter quests by faction reputation tier in addition to legend level and prerequisites.

2. **Quest Chain Service** — A new `IQuestChainService` that provides chain-level views: chain progression, next available quest, chain completion status, and a journal-friendly chain summary.

3. **Quest Data Expansion** — Complete the 5 faction chains in `config/quests.json` with 3-5 quests per chain, each with escalating reputation requirements, prerequisites, and rewards.

### Design Decisions

- **New service, not modifying `IQuestDefinitionProvider`.** The provider is a data-access concern (load from JSON, index, return DTOs). Quest availability logic involving reputation is business logic that belongs in a service layer. Adding a `IQuestChainService` with a `GetAvailableQuestsForPlayer()` method that wraps the provider and applies reputation filters follows clean architecture.

- **Reputation gating via a new `MinimumReputation` field on `QuestDefinitionDto`.** Adding a field to the DTO is simpler and more explicit than deriving reputation requirements from the faction + some hardcoded tier mapping. Quest designers can set exactly what reputation is needed per quest.

- **Chain ordering via `ChainOrder` field.** Rather than inferring order from prerequisites (which could form non-linear graphs), add an explicit `ChainOrder` integer to each quest. This makes ordering deterministic and simple.

- **No new domain entities.** Quest chains are a presentation/orchestration concept, not a domain concept. The domain already has `Quest`, `QuestObjective`, and `QuestCategory`. Chains are just a grouping of existing quests — the service provides the view, not a new entity.

---

## Architecture

### Component Diagram

```
┌─────────────────────────────────────────────────────────────┐
│ Application Layer (RuneAndRust.Application)                 │
│                                                             │
│  Interfaces/                                                │
│    IQuestChainService.cs      ← New: chain management      │
│                                                             │
│  DTOs/                                                      │
│    QuestDefinitionDto.cs      ← Modified: +MinReputation,   │
│                                  +ChainOrder                │
│    QuestChainSummaryDto.cs    ← New: chain-level view       │
│                                                             │
│  Services/                                                  │
│    QuestChainService.cs       ← New: chain logic            │
│                                                             │
├─────────────────────────────────────────────────────────────┤
│ Configuration                                               │
│                                                             │
│  config/quests.json           ← Expanded: complete chains   │
└─────────────────────────────────────────────────────────────┘
```

### File Location Map

| New File | Layer | Purpose |
|----------|-------|---------|
| `src/Core/RuneAndRust.Application/Interfaces/IQuestChainService.cs` | Application | Chain service contract |
| `src/Core/RuneAndRust.Application/DTOs/QuestChainSummaryDto.cs` | Application | Chain-level view DTO |
| `src/Core/RuneAndRust.Application/Services/QuestChainService.cs` | Application | Chain business logic |

### Modified Files

| Existing File | Change |
|---------------|--------|
| `src/Core/RuneAndRust.Application/DTOs/QuestDefinitionDto.cs` | Add `MinimumReputation` and `ChainOrder` fields |
| `config/quests.json` | Expand chains to 3-5 quests each, add `minimumReputation` and `chainOrder` fields |

---

## Data Contract / API

### QuestDefinitionDto Extensions

```csharp
// Added to QuestDefinitionDto:

/// <summary>
/// Minimum reputation value required with the quest's Faction to accept this quest.
/// 0 = no reputation requirement (default). Requires Faction to be set.
/// </summary>
/// <remarks>
/// Examples: 0 (Neutral, chain opener), 25 (Friendly, mid-chain), 50 (Allied, late-chain).
/// Used by QuestChainService to filter available quests based on player reputation.
/// </remarks>
public int MinimumReputation { get; init; } = 0;

/// <summary>
/// Order within the quest chain (1-based). Null if not part of a chain.
/// </summary>
/// <remarks>
/// Used to present quests in sequence in the quest journal and to determine
/// the "next quest" in a chain. Quests with the same ChainId are ordered
/// by this field.
/// </remarks>
public int? ChainOrder { get; init; }
```

### QuestChainSummaryDto

```csharp
/// <summary>
/// Summary of a faction quest chain's state for a specific player.
/// </summary>
public record QuestChainSummaryDto
{
    /// <summary>The chain ID (e.g., "ironbane_initiation").</summary>
    public string ChainId { get; init; } = string.Empty;

    /// <summary>The faction ID this chain belongs to.</summary>
    public string FactionId { get; init; } = string.Empty;

    /// <summary>The faction display name.</summary>
    public string FactionName { get; init; } = string.Empty;

    /// <summary>Total quests in the chain.</summary>
    public int TotalQuests { get; init; }

    /// <summary>Number of quests completed by the player.</summary>
    public int CompletedQuests { get; init; }

    /// <summary>Whether all quests in the chain are complete.</summary>
    public bool IsComplete => CompletedQuests >= TotalQuests;

    /// <summary>Completion percentage (0-100).</summary>
    public int CompletionPercent => TotalQuests == 0 ? 0 : (CompletedQuests * 100 / TotalQuests);

    /// <summary>The next available quest in the chain, or null if complete or locked.</summary>
    public string? NextQuestId { get; init; }

    /// <summary>The next quest's display name.</summary>
    public string? NextQuestName { get; init; }

    /// <summary>Whether the next quest is locked behind a reputation gate.</summary>
    public bool IsNextQuestReputationLocked { get; init; }

    /// <summary>The reputation required for the next quest (if locked).</summary>
    public int? NextQuestReputationRequired { get; init; }

    /// <summary>The player's current reputation with this faction.</summary>
    public int PlayerReputation { get; init; }
}
```

### IQuestChainService Interface

```csharp
/// <summary>
/// Service for managing faction quest chain progression and reputation-gated availability.
/// </summary>
public interface IQuestChainService
{
    /// <summary>
    /// Gets all quests available to a player, including reputation-based filtering.
    /// Extends the base provider's legend/prerequisite filtering with faction reputation checks.
    /// </summary>
    IReadOnlyList<QuestDefinitionDto> GetAvailableQuestsForPlayer(
        Player player,
        int legendLevel,
        IReadOnlySet<string> completedQuestIds);

    /// <summary>
    /// Gets a summary of all quest chains and their progress for a player.
    /// </summary>
    IReadOnlyList<QuestChainSummaryDto> GetAllChainSummaries(
        Player player,
        IReadOnlySet<string> completedQuestIds);

    /// <summary>
    /// Gets the summary for a specific quest chain.
    /// </summary>
    QuestChainSummaryDto? GetChainSummary(
        string chainId,
        Player player,
        IReadOnlySet<string> completedQuestIds);

    /// <summary>
    /// Gets the next quest in a chain for a player, or null if complete/locked.
    /// </summary>
    QuestDefinitionDto? GetNextQuestInChain(
        string chainId,
        IReadOnlySet<string> completedQuestIds);

    /// <summary>
    /// Gets all quests in a chain, ordered by ChainOrder.
    /// </summary>
    IReadOnlyList<QuestDefinitionDto> GetQuestsInChain(string chainId);

    /// <summary>
    /// Checks if a specific quest is available to a player (prerequisites + reputation).
    /// </summary>
    bool IsQuestAvailable(
        QuestDefinitionDto quest,
        Player player,
        int legendLevel,
        IReadOnlySet<string> completedQuestIds);
}
```

### Quest Chain Data — Expanded config/quests.json

Each of the 5 factions will have a complete chain of 3-5 quests with escalating reputation requirements:

**Iron-Bane Initiation (3 quests):**
| # | Quest ID | Min Rep | Reward Rep | Unlocks |
|---|----------|---------|------------|---------|
| 1 | `faction_ironbanes_purge_rust` | 0 | +20 | Quest #2 |
| 2 | `faction_ironbanes_corrupted_forge` | 0 | +25 | Quest #3 |
| 3 | `faction_ironbanes_undying_commander` | 25 | +30 | — |

**Rust-Clan Ascent (3 quests):**
| # | Quest ID | Min Rep | Reward Rep | Unlocks |
|---|----------|---------|------------|---------|
| 1 | `faction_rustclans_scavenge_run` | 0 | +15 | Quest #2 |
| 2 | `faction_rustclans_trade_route` | 0 | +20 | Quest #3 |
| 3 | `faction_rustclans_defense_duty` | 25 | +25 | — |

**Jötun-Reader Path (3 quests):**
| # | Quest ID | Min Rep | Reward Rep | Unlocks |
|---|----------|---------|------------|---------|
| 1 | `faction_jotunreaders_data_recovery` | 0 | +15 | Quest #2 |
| 2 | `faction_jotunreaders_glitch_analysis` | 0 | +20 | Quest #3 |
| 3 | `faction_jotunreaders_archive_restoration` | 25 | +25 | — |

**God-Sleeper Revelation (3 quests):**
| # | Quest ID | Min Rep | Reward Rep | Unlocks |
|---|----------|---------|------------|---------|
| 1 | `faction_godsleeper_defend_sacred` | 0 | +20 | Quest #2 |
| 2 | `faction_godsleeper_awakening_ritual` | 0 | +40 | Quest #3 |
| 3 | `faction_godsleeper_sacred_pilgrimage` | 25 | +30 | — |

**Independent Way (3 quests):**
| # | Quest ID | Min Rep | Reward Rep | Unlocks |
|---|----------|---------|------------|---------|
| 1 | `faction_independents_own_path` | 0 | +10 | Quest #2 |
| 2 | `faction_independents_solo_survival` | 0 | +15 | Quest #3 |
| 3 | `faction_independents_lone_wolf_trial` | 20 | +25 | — |

---

## Constraints

1. **Backward compatibility with existing quests.** The 12 quests already in `config/quests.json` must continue to work. `MinimumReputation` defaults to 0 and `ChainOrder` defaults to null, so existing non-chain quests are unaffected.

2. **No breaking changes to `IQuestDefinitionProvider`.** The new `IQuestChainService` wraps the existing provider rather than modifying its interface. Existing callers of `GetAvailableQuests()` continue to work (they just don't get reputation filtering).

3. **Faction ID consistency.** Quest faction IDs (e.g., "IronBanes" in quests.json) must match the faction IDs in factions.json (e.g., "iron-banes"). We need to either normalize the IDs or add a mapping. The config currently uses mixed conventions: quests use "IronBanes" while factions.json uses "iron-banes". We'll normalize quests.json to use the same kebab-case IDs as factions.json.

4. **Quest data needs mapping for existing quests.** The existing faction quests use different faction ID formats ("IronBanes" vs "iron-banes"). These need to be updated in quests.json to match the normalized IDs in factions.json.

---

## Alternatives Considered

### Alternative 1: Modify IQuestDefinitionProvider.GetAvailableQuests()

Add reputation parameter directly to the existing provider method.

**Pros:** Single call for all quest availability, no new service.

**Cons:** Violates single responsibility — the provider is a data-access layer, not business logic. Would require the provider to depend on `IReputationService` (infrastructure depending on application — wrong direction in clean architecture). Also would be a breaking change to an existing interface.

**Verdict:** Rejected. Clean architecture violation.

### Alternative 2: Reputation as a Prerequisite Type

Encode reputation requirements as entries in `PrerequisiteQuestIds` with a special prefix (e.g., "rep:iron-banes:25").

**Pros:** No new fields on the DTO.

**Cons:** Stringly-typed, error-prone, requires special parsing, mixes two distinct concepts (quest completion prerequisites and reputation gates), makes it impossible to give clear UI feedback about why a quest is locked.

**Verdict:** Rejected. Explicit fields are clearer and more type-safe.

### Alternative 3: Chosen Approach (New QuestChainService + DTO Fields)

New service that wraps the provider and applies reputation logic. New explicit fields on the DTO for reputation requirements and chain ordering.

**Pros:** Clean separation of concerns, explicit and type-safe, no breaking changes, chain-level views are a natural fit for a service.

**Cons:** One more service in the DI container.

**Verdict:** Selected.

---

## Error Handling

| Error Case | Handling Strategy | What the Caller Sees |
|------------|-------------------|---------------------|
| Quest has Faction set but faction not in factions.json | Log warning, treat MinimumReputation as 0 (always available) | Quest appears available — no silent blocking |
| Chain has gaps in ChainOrder | Log warning, order by ChainOrder ignoring gaps | Quests still ordered correctly, just non-contiguous |
| Chain has duplicate ChainOrder values | Log warning, use quest ID as tiebreaker | Deterministic ordering even with bad data |
| Player null | `ArgumentNullException` from guard clause | Standard .NET exception |
| Chain ID not found | Return null from `GetChainSummary`, empty list from `GetQuestsInChain` | Graceful empty results |
| Quest's MinimumReputation set but Faction is null | Log warning, ignore reputation check | Quest available if other prerequisites met |

---

## Performance Considerations

With 12-25 quests and 5 chains, performance is irrelevant. All operations are O(n) where n < 30. No caching needed.

---

## Success Criteria

1. `GetAvailableQuestsForPlayer()` excludes quests where the player doesn't meet `MinimumReputation` with the quest's faction.
2. `GetAllChainSummaries()` returns 5 chain summaries with correct completion counts.
3. `GetNextQuestInChain()` returns the correct next quest based on completed prerequisites and chain order.
4. Config quests.json has 5 complete chains with 3 quests each (15 faction quests total).
5. Faction IDs in quests.json are normalized to match factions.json kebab-case format.
6. All existing non-chain quests continue to be available (no regressions).
7. Chain summary correctly reports `IsNextQuestReputationLocked` when the player's reputation is too low.

---

## Acceptance Criteria

| #   | Category | Criterion | Verification |
| --- | -------- | --------- | ------------ |
| 1   | Service  | `GetAvailableQuestsForPlayer` returns quest when reputation meets minimum | Unit test |
| 2   | Service  | `GetAvailableQuestsForPlayer` excludes quest when reputation below minimum | Unit test |
| 3   | Service  | `GetAvailableQuestsForPlayer` includes non-faction quests (no reputation check) | Unit test |
| 4   | Service  | `GetAllChainSummaries` returns 5 chains | Integration test |
| 5   | Service  | `GetChainSummary` reports correct completion count | Unit test |
| 6   | Service  | `GetChainSummary` identifies reputation-locked next quest | Unit test |
| 7   | Service  | `GetNextQuestInChain` returns next uncompleted quest in order | Unit test |
| 8   | Service  | `GetNextQuestInChain` returns null when chain is complete | Unit test |
| 9   | Service  | `GetQuestsInChain` returns quests ordered by ChainOrder | Unit test |
| 10  | Service  | `IsQuestAvailable` checks legend level + prerequisites + reputation | Unit test |
| 11  | Config   | Each chain has exactly 3 quests with sequential ChainOrder | Integration test |
| 12  | Config   | All faction IDs match factions.json format (kebab-case) | Integration test |
| 13  | Config   | Chain final quests require MinimumReputation ≥ 20 | Integration test |
| 14  | Build    | Solution builds with zero warnings | `dotnet build` |
| 15  | Tests    | All new tests pass, all existing tests still pass | `dotnet test` |

---

## Open Questions

| # | Question | Owner | Resolution Deadline |
|---|----------|-------|---------------------|
| 1 | Should quest chain completion grant a special chain-completion reward (beyond individual quest rewards)? | Ryan | Before content expansion |
| 2 | How many quests per chain is the target? The spec uses 3 as a minimum. Should some chains be 4-5? | Ryan | During implementation |
| 3 | Should the Iron-Banes vs God-Sleeper Cultists antagonism be reflected in quests? (e.g., completing Iron-Bane quests gives negative God-Sleeper rep) | Ryan | Before content expansion (relates to SPEC-REPUTATION-001 Open Question #1) |

---

## Dependencies

### This Feature Depends On

| Dependency | Status | Notes |
|------------|--------|-------|
| `IQuestDefinitionProvider` | ✅ Exists | Read-only provider, wrapped by chain service |
| `QuestDefinitionDto` | ✅ Exists | Adding MinimumReputation and ChainOrder |
| `IReputationService` (SPEC-REPUTATION-001) | ✅ Implementing | Needed for reputation checks |
| `Player.GetFactionReputation()` | ✅ Implementing | Needed for reputation value lookups |
| `config/quests.json` | ✅ Exists (partial) | Needs quest data expansion |

### Depends on This Feature

| Downstream | Status | Notes |
|------------|--------|-------|
| Quest Journal Chain View | Future | Needs `GetAllChainSummaries()` |
| Settlement Quest Gating | Future (Tier 4+) | Needs `IsQuestAvailable()` |

---

## Development Standards

### Changelog Requirements

- One entry per new file, one per modified file.
- Format: `- [Added] ...` / `- [Changed] ...`.

### Logging Standards

| Level | Use For | Example |
|-------|---------|---------|
| DEBUG | Quest availability evaluation details | `"Quest {QuestId}: rep check {PlayerRep} >= {MinRep} = {Result}"` |
| INFO | Chain progression events | `"Chain {ChainId}: {Completed}/{Total} quests complete"` |
| WARN | Data issues | `"Quest {QuestId} has Faction but faction not found in config"` |

### Unit Testing Expectations

- **Must test:** Reputation gating, chain ordering, chain progress calculation, next-quest determination, mixed chain/non-chain availability.
- **Can skip:** DTO property defaults.
- **Naming:** `[MethodUnderTest]_[Scenario]_[ExpectedResult]`.
- **Framework:** NUnit + FluentAssertions + Moq.

---

## Deliverable Checklist

| #  | Deliverable | Status |
| -- | ----------- | ------ |
| 1  | `QuestDefinitionDto` extensions (+MinimumReputation, +ChainOrder) | Pending |
| 2  | `QuestChainSummaryDto.cs` DTO | Pending |
| 3  | `IQuestChainService.cs` interface | Pending |
| 4  | `QuestChainService.cs` implementation | Pending |
| 5  | Normalize faction IDs in quests.json to kebab-case | Pending |
| 6  | Add missing quest data for incomplete chains | Pending |
| 7  | Add MinimumReputation and ChainOrder to existing quests | Pending |
| 8  | Unit tests: `QuestChainServiceTests.cs` | Pending |
| 9  | Integration tests: chain data validation | Pending |

---

## Document History

| Version | Date       | Author | Changes                     |
| ------- | ---------- | ------ | --------------------------- |
| 1.0     | 2026-02-28 | Claude | Initial design specification |
