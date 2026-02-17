---
id: SPEC-LEGEND-001
title: Legend Point System
version: 1.0.0
status: Planned
last_updated: 2025-12-23
related_specs: [SPEC-CODEX-001, SPEC-CHAR-001, SPEC-XP-001]
---

# SPEC-LEGEND-001: Legend Point System

> **Version:** 1.0.0
> **Status:** Planned (v0.1.4+ roadmap)
> **Service:** None (planned: `ILegendService`)
> **Location:** Planned: `RuneAndRust.Engine/Services/LegendService.cs`

---

> **WARNING: DESIGN SPECIFICATION ONLY**
>
> This specification describes a **planned feature** that has **NOT been implemented**.
> No C# services, entities, or tests exist for this system yet.
>
> **What Exists:** Architectural hook in `DataCapture.Quality` field (15/30 LP values)
>
> **Target Version:** v0.1.4+

---

## Table of Contents

- [Overview](#overview)
- [Core Concepts](#core-concepts)
- [Behaviors](#behaviors)
- [Restrictions](#restrictions)
- [Limitations](#limitations)
- [Use Cases](#use-cases)
- [Decision Trees](#decision-trees)
- [Cross-Links](#cross-links)
- [Related Services](#related-services)
- [Data Models](#data-models)
- [Configuration](#configuration)
- [Testing](#testing)
- [Design Rationale](#design-rationale)
- [Future Enhancements](#future-enhancements)
- [AAM-VOICE Compliance](#aam-voice-compliance)
- [Changelog](#changelog)

---

## Overview

The Legend Point System provides meta-progression across character runs through permanent account-wide upgrades earned by completing Codex entries and achieving milestones. Legend Points persist beyond character death, allowing players to unlock starting bonuses, equipment access, and quality-of-life improvements for future runs.

This system rewards lore discovery and long-term mastery, providing meaningful progression even after character permadeath.

---

## Core Concepts

### Legend Points (LP)

**Definition:** Meta-currency earned through Codex entry completion and milestone achievements that persists across all character deaths.

**Current Implementation:** **NOT IMPLEMENTED**
- No `LegendPoints` field on Character or Account entity
- No Legend Point award mechanics
- No Legend Point spending system
- Architectural hooks exist in `DataCapture.Quality` field (15/30 points)

**Future Sources:**
| Source | LP Award | Status |
|--------|----------|--------|
| Standard Codex Fragment | 15 LP | Planned (quality field exists) |
| Specialist Codex Fragment | 30 LP | Planned (quality field exists) |
| Complete Codex Entry (100%) | Sum of fragment quality | Planned |
| Boss Defeats (First Time) | 50 LP | Planned |
| Milestones (Depth 10/20/30) | 25/50/100 LP | Planned |
| Achievements | Varies | Planned |

---

### Legend Purchases

**Definition:** Permanent unlocks purchased with Legend Points that benefit all future characters.

**Categories (Planned):**

#### 1. Starting Equipment Unlocks
| Purchase | Cost | Effect |
|----------|------|--------|
| **Clan-Forged Starter Weapon** | 150 LP | New characters start with Clan-Forged quality weapon |
| **Optimized Starter Armor** | 200 LP | New characters start with Optimized quality armor |
| **Tool Kit** | 100 LP | Start with 3 random tools (lockpick, crowbar, etc.) |

#### 2. Attribute Bonuses
| Purchase | Cost | Effect |
|----------|------|--------|
| **Lineage Affinity I** | 100 LP | +1 to primary lineage attribute for new characters |
| **Lineage Affinity II** | 250 LP | +2 to primary lineage attribute (requires Affinity I) |
| **Archetype Mastery I** | 150 LP | +1 to primary archetype attribute |

#### 3. Progression Bonuses
| Purchase | Cost | Effect |
|----------|------|--------|
| **Quick Learner** | 300 LP | +10% XP gain (all sources) |
| **Lore Hunter** | 200 LP | Codex fragments reveal 20% more text per discovery |
| **Lucky Scavenger** | 250 LP | +10% chance to upgrade loot quality tier |

#### 4. Quality-of-Life Unlocks
| Purchase | Cost | Effect |
|----------|------|--------|
| **Expanded Inventory** | 150 LP | +5 inventory slots for all characters |
| **Fast Travel I** | 200 LP | Unlock fast travel between discovered sanctuaries |
| **Corruption Resistance** | 400 LP | Corruption builds 10% slower |

---

### Meta-Progression Philosophy

**Permanent vs. Temporary:**
- **Legend Points:** PERMANENT (never lost, even on character death)
- **Purchases:** PERMANENT (once unlocked, benefits all future characters)
- **Character XP/Levels:** TEMPORARY (lost on death, starts at Level 1)

**Design Goal:** Reward long-term mastery and discovery without trivializing runs. Purchases provide meaningful advantage but don't eliminate challenge.

---

## Behaviors

> **Note:** All behaviors in this section are **PLANNED** and not yet implemented.
> Method signatures and sequences represent the intended design.

### Primary Behaviors (Planned)

#### 1. Grant Legend Points (`GrantLegendPointsAsync`) - NOT IMPLEMENTED

```csharp
Task GrantLegendPointsAsync(Guid accountId, int amount, string source)
```

**Planned Sequence:**
1. Load Account entity
2. Add `amount` to `Account.LegendPoints`
3. Log award event with source
4. Persist account state
5. Trigger UI notification

**Example:**
```csharp
// Player completes Codex entry "Blight-Rat Bestiary"
var entry = await _codexRepo.GetByIdAsync(entryId);
var lpAwarded = entry.DataCaptures.Sum(dc => dc.Quality);  // Sum fragment quality
await _legendService.GrantLegendPointsAsync(accountId, lpAwarded, "Codex: Blight-Rat");
// Account.LegendPoints: 150 → 195 (entry had 3 fragments: 15+15+15)
```

---

#### 2. Purchase Upgrade (`PurchaseUpgradeAsync`) - NOT IMPLEMENTED

```csharp
Task<bool> PurchaseUpgradeAsync(Guid accountId, string upgradeId)
```

**Planned Sequence:**
1. Load Account entity
2. Validate upgrade exists in catalog
3. Check if already purchased (no double-buying)
4. Verify sufficient LP balance
5. Deduct cost from `Account.LegendPoints`
6. Add upgrade to `Account.LegendPurchases` collection
7. Persist account state
8. Return success/failure

**Example:**
```csharp
// Player attempts to buy "Quick Learner" (300 LP)
var success = await _legendService.PurchaseUpgradeAsync(accountId, "UPGRADE_QUICK_LEARNER");
if (success)
{
    // Account.LegendPoints: 450 → 150
    // Account.LegendPurchases: [..., "UPGRADE_QUICK_LEARNER"]
    // Future characters gain +10% XP
}
else
{
    // Insufficient LP or already purchased
}
```

---

#### 3. Get Available Legend Points (`GetAvailableLegendPointsAsync`) - NOT IMPLEMENTED

```csharp
Task<int> GetAvailableLegendPointsAsync(Guid accountId)
```

**Returns:** Current unspent Legend Point balance for account.

**Example:**
```csharp
var lp = await _legendService.GetAvailableLegendPointsAsync(accountId);
// Returns: 450 (total earned: 1200, total spent: 750)
```

---

#### 4. Get Purchase History (`GetPurchaseHistoryAsync`) - NOT IMPLEMENTED

```csharp
Task<List<LegendPurchase>> GetPurchaseHistoryAsync(Guid accountId)
```

**Returns:** List of all purchased upgrades with purchase dates.

**Example:**
```csharp
var purchases = await _legendService.GetPurchaseHistoryAsync(accountId);
// Returns:
// [
//   { UpgradeId: "UPGRADE_QUICK_LEARNER", PurchasedAt: 2025-12-01, Cost: 300 },
//   { UpgradeId: "UPGRADE_CLAN_WEAPON", PurchasedAt: 2025-12-15, Cost: 150 }
// ]
```

---

#### 5. Apply Upgrades to New Character (`ApplyLegendBonuses`) - NOT IMPLEMENTED

```csharp
void ApplyLegendBonuses(Character newCharacter, List<string> purchasedUpgrades)
```

**Planned Sequence:**
1. Called by `CharacterFactory.Create()` when new character created
2. Iterate purchased upgrades
3. Apply effects based on upgrade type:
   - Equipment unlocks → Add items to inventory
   - Attribute bonuses → Increase base attributes
   - Progression bonuses → Set multiplier flags
4. Log applied bonuses

**Example:**
```csharp
// Player creates new Ranger after purchasing:
// - "UPGRADE_CLAN_WEAPON" (150 LP)
// - "UPGRADE_LINEAGE_AFFINITY_I" (100 LP)
// - "UPGRADE_QUICK_LEARNER" (300 LP)

var character = _characterFactory.Create(Archetype.Ranger, Lineage.Human);
_legendService.ApplyLegendBonuses(character, account.LegendPurchases);

// Effects:
// - Inventory contains Clan-Forged quality bow (instead of Scavenged)
// - STU +1 (Human lineage primary attribute)
// - XpMultiplier = 1.1f (10% bonus)
```

---

## Restrictions

### Purchase Requirements

**Prerequisite Chains:**
- Some upgrades require earlier upgrades (e.g., Affinity II requires Affinity I)
- No skipping tiers

**One-Time Purchases:**
- Each upgrade can only be purchased once per account
- No refunds or resets

---

### Legend Point Caps

**Earning Cap:** NONE
- Players can accumulate unlimited LP
- No "wasted" progress if all upgrades purchased

**Spending Cap:** Catalog-Defined
- Total spendable LP = sum of all upgrade costs
- Current planned catalog: ~3500 LP total

---

## Limitations

### Numerical Bounds

| Constraint | Value | Notes |
|------------|-------|-------|
| Min LP | 0 | Cannot go negative |
| Max LP | Unbounded | No earning cap |
| Standard Fragment LP | 15 | Per fragment |
| Specialist Fragment LP | 30 | Rare/hidden fragments |
| Min Purchase Cost | 100 LP | Cheapest upgrades |
| Max Purchase Cost | 400 LP | Most expensive upgrades |

---

### System Gaps (Current)

- **No Account entity** - Would need to store LP and purchase history
- **No Legend Service** - No award or spending logic
- **No UI for Legend Shop** - Purchase interface not designed
- **No Codex → Legend integration** - Completion hooks don't trigger LP awards
- **No CharacterFactory integration** - New characters don't receive bonuses
- **No Achievement system** - Alternative LP sources not implemented

---

## Use Cases

### UC-1: First Codex Completion

```csharp
// Player completes "Blight-Rat Bestiary" Codex entry
var entry = await _codexRepo.GetByIdAsync(entryId);
var totalCaptures = entry.DataCaptures.Count;  // 3 fragments
var completionPercent = entry.GetCompletionPercentage();  // 100%

if (completionPercent >= 100.0f)
{
    var lpAwarded = entry.DataCaptures.Sum(dc => dc.Quality);
    // 3 standard fragments: 15 + 15 + 15 = 45 LP

    await _legendService.GrantLegendPointsAsync(accountId, lpAwarded, $"Codex: {entry.Name}");

    // UI Notification:
    // "Codex entry completed! +45 Legend Points earned."
    // "You now have 45 LP. Visit the Legend Shop to spend them."
}
```

**Narrative Impact:** Lore discovery has tangible meta-progression reward.

---

### UC-2: First Purchase (Starting Equipment)

```csharp
// Player has 200 LP, wants Clan-Forged weapon (150 LP cost)
var accountLp = await _legendService.GetAvailableLegendPointsAsync(accountId);
// Returns: 200

var canAfford = accountLp >= 150;
if (canAfford)
{
    var success = await _legendService.PurchaseUpgradeAsync(accountId, "UPGRADE_CLAN_WEAPON");
    if (success)
    {
        // Account.LegendPoints: 200 → 50
        // Account.LegendPurchases: ["UPGRADE_CLAN_WEAPON"]

        // Next character creation:
        var newCharacter = _characterFactory.Create(Archetype.Ranger, Lineage.Human);
        _legendService.ApplyLegendBonuses(newCharacter, account.LegendPurchases);

        // newCharacter.Inventory contains:
        // - Clan-Forged Recurve Bow (Quality: ClanForged, Value: 225 Scrip)
        // Instead of:
        // - Scavenged Shortbow (Quality: Scavenged, Value: 150 Scrip)
    }
}
```

**Narrative Impact:** Early game is less punishing for subsequent runs, rewarding veteran players.

---

### UC-3: Progression Stacking (Multiple Bonuses)

```csharp
// Veteran player with 1500 LP spent purchases:
// - UPGRADE_QUICK_LEARNER (300 LP) → +10% XP
// - UPGRADE_LINEAGE_AFFINITY_I (100 LP) → +1 STU (Human)
// - UPGRADE_LINEAGE_AFFINITY_II (250 LP) → +2 STU (cumulative +3)
// - UPGRADE_LUCKY_SCAVENGER (250 LP) → +10% loot quality upgrade chance
// - UPGRADE_CLAN_WEAPON (150 LP) → Start with Clan-Forged weapon
// - UPGRADE_EXPANDED_INVENTORY (150 LP) → +5 inventory slots

var newCharacter = _characterFactory.Create(Archetype.Warrior, Lineage.Human);
_legendService.ApplyLegendBonuses(newCharacter, account.LegendPurchases);

// Applied Bonuses:
// - BaseStu: 7 → 10 (Human +1, Warrior +2, Affinity +3)
// - MaxInventorySlots: 15 → 20 (+5 from Expanded Inventory)
// - XpMultiplier: 1.1f (Quick Learner)
// - LootQualityUpgradeChance: 0.10f (Lucky Scavenger)
// - Starting Weapon: Clan-Forged Longsword (instead of Scavenged)

// Character is significantly stronger than fresh account character,
// but still starts at Level 1 with normal HP/Stamina
```

**Narrative Impact:** Veterans have meaningful edge without trivializing challenge. Still vulnerable to early-game deaths.

---

### UC-4: Boss First-Kill Bonus

```csharp
// Player defeats "Ancient Guardian" boss for first time
var bossTemplate = enemy.Template;
var isBoss = bossTemplate.Archetype == EnemyArchetype.Boss;
var isFirstKill = !await _achievementService.HasAchievementAsync(accountId, $"BOSS_{bossTemplate.Id}");

if (isBoss && isFirstKill)
{
    await _legendService.GrantLegendPointsAsync(accountId, 50, $"First Victory: {bossTemplate.Name}");
    await _achievementService.GrantAchievementAsync(accountId, $"BOSS_{bossTemplate.Id}");

    // UI Notification:
    // "First victory against Ancient Guardian! +50 Legend Points."
}
```

**Narrative Impact:** Incentivizes exploration and boss attempts, rewards risk-taking.

---

### UC-5: Prerequisite Chain Purchase

```csharp
// Player wants Lineage Affinity II but hasn't bought Affinity I
var upgradeConfig = _legendService.GetUpgradeConfig("UPGRADE_LINEAGE_AFFINITY_II");
// { Prerequisites: ["UPGRADE_LINEAGE_AFFINITY_I"], Cost: 250 }

var hasPurchased = await _legendService.HasPurchasedAsync(accountId, "UPGRADE_LINEAGE_AFFINITY_I");
if (!hasPurchased)
{
    var canPurchase = await _legendService.PurchaseUpgradeAsync(accountId, "UPGRADE_LINEAGE_AFFINITY_II");
    // Returns: false (prerequisite not met)

    // UI Message:
    // "Requires: Lineage Affinity I (100 LP)"
}
else
{
    // Prerequisites met, can purchase if LP sufficient
}
```

**Narrative Impact:** Encourages strategic spending, prevents overpowered early rushes.

---

## Decision Trees

### Legend Point Award Flow

```
┌─────────────────────────────────┐
│  Codex Entry Completed          │
│  (100% fragments discovered)    │
└────────────┬────────────────────┘
             │
    ┌────────┴────────┐
    │ Sum Fragment    │
    │ Quality Values  │
    └────────┬────────┘
             │
    ┌────────┴────────────┐
    │ Standard: 15 LP ea  │
    │ Specialist: 30 LP ea│
    └────────┬────────────┘
             │
    ┌────────┴────────────┐
    │ Grant LP to Account │
    └────────┬────────────┘
             │
    ┌────────┴────────────┐
    │ Persist Account DB  │
    └────────┬────────────┘
             │
    ┌────────┴────────────┐
    │ Show UI Notification│
    └─────────────────────┘
```

---

### Purchase Validation Flow

```
┌─────────────────────────────────┐
│  Player Selects Upgrade         │
└────────────┬────────────────────┘
             │
    ┌────────┴────────────┐
    │ Already Purchased?  │
    └────────┬────────────┘
         ┌───┴───┐
         │       │
        YES     NO
         │       │
         │       └───> Check Prerequisites
         │                    │
         └─────────────> Return "Already Owned"
                              │
                    ┌─────────┴─────────┐
                    │ Prerequisites Met?│
                    └─────────┬─────────┘
                          ┌───┴───┐
                          │       │
                         YES     NO
                          │       │
                          │       └─> Return "Requires: [List]"
                          │
                    ┌─────┴──────┐
                    │ LP >= Cost?│
                    └─────┬──────┘
                      ┌───┴───┐
                      │       │
                     YES     NO
                      │       │
                      │       └─> Return "Insufficient LP"
                      │
                      ▼
                ┌──────────┐
                │ Deduct LP│
                │ Add to   │
                │ Purchases│
                │ Persist  │
                └──────────┘
```

---

## Cross-Links

### Dependencies (Consumes)

| Service | Specification | Usage |
|---------|---------------|-------|
| `IRepository<Account>` | Infrastructure | Persist LP balance and purchase history |
| `ICodexEntryRepository` | [SPEC-CODEX-001](SPEC-CODEX-001.md) | Calculate LP from completed entries |
| `IAchievementService` | Planned | Track first-kill boss bonuses, milestones |
| `ILogger` | Infrastructure | LP award and purchase event tracing |

### Dependents (Provides To)

| Service | Specification | Usage |
|---------|---------------|-------|
| `CharacterFactory` | [SPEC-CHAR-001](SPEC-CHAR-001.md) | Apply Legend bonuses to new characters |
| `ProgressionService` | [SPEC-XP-001](SPEC-XP-001.md) | Apply XP multiplier from Quick Learner upgrade |
| `LootService` | [SPEC-LOOT-001](SPEC-LOOT-001.md) | Apply quality upgrade bonus from Lucky Scavenger |

### Related Systems

- [SPEC-CODEX-001](SPEC-CODEX-001.md) - **Codex System**: Primary LP source via entry completion
- [SPEC-CHAR-001](SPEC-CHAR-001.md) - **Character System**: Receives Legend bonus application
- [SPEC-XP-001](SPEC-XP-001.md) - **XP System**: Modified by Quick Learner upgrade

---

## Related Services

> **Note:** All files listed below are **NOT YET CREATED**. These represent the planned implementation structure.

### Primary Implementation (Planned)

| File | Purpose | Status |
|------|----------|--------|
| `LegendService.cs` | LP award, purchase validation, bonus application | NOT CREATED |

### Supporting Types (Planned)

| File | Purpose | Status |
|------|----------|--------|
| `Account.cs` | LP balance and purchase history storage | NOT CREATED |
| `LegendPurchase.cs` | Purchase record entity | NOT CREATED |
| `LegendUpgradeConfig.cs` | Upgrade catalog definitions | NOT CREATED |

### Existing Hooks

| File | Purpose | Status |
|------|----------|--------|
| `DataCapture.cs` | `Quality` field (15/30 LP values) | EXISTS |

---

## Data Models (Planned)

> **Note:** All data models below are **proposed designs** and do not exist in the codebase yet.

### Account Entity

```csharp
public class Account
{
    public Guid Id { get; set; }
    public string Username { get; set; }

    /// <summary>
    /// Total unspent Legend Points.
    /// </summary>
    public int LegendPoints { get; set; } = 0;

    /// <summary>
    /// Total Legend Points earned across all runs (for statistics).
    /// </summary>
    public int TotalLegendPointsEarned { get; set; } = 0;

    /// <summary>
    /// List of purchased upgrade IDs.
    /// </summary>
    public List<string> LegendPurchases { get; set; } = new();

    public DateTime CreatedAt { get; set; }
    public DateTime LastPlayedAt { get; set; }
}
```

---

### LegendPurchase Record

```csharp
public record LegendPurchase(
    string UpgradeId,
    int Cost,
    DateTime PurchasedAt
);
```

---

### LegendUpgradeConfig

```csharp
public class LegendUpgradeConfig
{
    public string Id { get; set; }  // "UPGRADE_QUICK_LEARNER"
    public string Name { get; set; }  // "Quick Learner"
    public string Description { get; set; }  // "+10% XP from all sources"
    public int Cost { get; set; }  // 300
    public List<string> Prerequisites { get; set; } = new();  // ["UPGRADE_FOO"]
    public UpgradeCategory Category { get; set; }  // Equipment, Attribute, Progression, QOL
}

public enum UpgradeCategory
{
    Equipment,
    Attribute,
    Progression,
    QualityOfLife
}
```

---

## Configuration

### Upgrade Catalog (Planned)

**JSON Configuration (LegendUpgrades.json):**
```json
{
  "upgrades": [
    {
      "id": "UPGRADE_QUICK_LEARNER",
      "name": "Quick Learner",
      "description": "+10% XP from all sources for all future characters",
      "cost": 300,
      "category": "Progression",
      "prerequisites": []
    },
    {
      "id": "UPGRADE_LINEAGE_AFFINITY_I",
      "name": "Lineage Affinity I",
      "description": "+1 to primary lineage attribute for new characters",
      "cost": 100,
      "category": "Attribute",
      "prerequisites": []
    },
    {
      "id": "UPGRADE_LINEAGE_AFFINITY_II",
      "name": "Lineage Affinity II",
      "description": "+2 to primary lineage attribute (cumulative with Affinity I)",
      "cost": 250,
      "category": "Attribute",
      "prerequisites": ["UPGRADE_LINEAGE_AFFINITY_I"]
    },
    {
      "id": "UPGRADE_CLAN_WEAPON",
      "name": "Clan-Forged Starter Weapon",
      "description": "New characters start with a Clan-Forged quality weapon",
      "cost": 150,
      "category": "Equipment",
      "prerequisites": []
    }
  ]
}
```

---

### Fragment Quality → LP Mapping

**Current Implementation (DataCapture.cs:80-83):**
```csharp
/// Affects Legend reward when the parent entry is completed.
/// Standard captures: 15 points. Specialist captures: 30 points.
public int Quality { get; set; } = 15;
```

**Calculation:**
```csharp
public async Task<int> CalculateEntryLegendPoints(Guid entryId)
{
    var entry = await _codexRepo.GetByIdAsync(entryId);
    return entry.DataCaptures.Sum(dc => dc.Quality);
}
```

**Example Entry LP Values:**
| Entry Type | Fragments | Standard/Specialist | Total LP |
|------------|-----------|---------------------|----------|
| Basic Bestiary | 3 | 3 Standard | 45 LP |
| Advanced Bestiary | 4 | 3 Standard, 1 Specialist | 75 LP |
| Lore Archive | 6 | 4 Standard, 2 Specialist | 120 LP |

---

## Testing (Planned)

> **Note:** No tests exist for this system yet. The test scenarios below define the **required coverage** when implementation begins.

### Test Coverage

`LegendServiceTests.cs` (NOT YET CREATED) should cover:

1. **LP Award Tests** (5 tests)
   - Grant LP to account
   - Grant LP from Codex completion
   - Grant LP from boss first-kill
   - Grant LP with maximum int value (overflow check)
   - Grant negative LP (should fail)

2. **Purchase Tests** (8 tests)
   - Purchase upgrade with sufficient LP
   - Purchase upgrade with insufficient LP (fails)
   - Purchase already-owned upgrade (fails)
   - Purchase upgrade without prerequisites (fails)
   - Purchase upgrade with prerequisites met (succeeds)
   - Deduct correct LP amount on purchase
   - Add upgrade to purchase history
   - Persist account after purchase

3. **Bonus Application Tests** (6 tests)
   - Apply equipment upgrade (Clan-Forged weapon in inventory)
   - Apply attribute bonus (STU +1)
   - Apply progression bonus (XP multiplier)
   - Apply multiple upgrades (stacking bonuses)
   - Apply upgrades to different archetypes (Warrior vs. Mystic)
   - Apply upgrades to different lineages (Human vs. Dvergr)

4. **Catalog Tests** (3 tests)
   - Load upgrade catalog from JSON
   - Validate all prerequisite chains (no circular dependencies)
   - Calculate total catalog LP cost

---

## Design Rationale

### Why Account-Level (Not Character)?

**Decision:** Legend Points tied to Account entity, not Character.

**Rationale:**
- **Permadeath Preservation:** LP survives character death
- **Alt Character Benefit:** All characters on account share bonuses
- **Progression Continuity:** Feels like "player" progression, not "character" progression

**Alternative Considered:** Character-specific LP. Rejected because:
- Defeats purpose of meta-progression (lost on death)
- Punishes experimentation with new archetypes/lineages

---

### Why Codex Completion Rewards LP?

**Decision:** Primary LP source is completing Codex entries (lore discovery).

**Rationale:**
- **Encourages Exploration:** Lore is optional, LP makes it rewarding
- **Thematic Fit:** "Legend" = stories and knowledge passed down
- **Non-Combat Progression:** Rewards playstyle diversity beyond grinding
- **Integration:** Codex system already tracks completion %

**Alternative Considered:** LP from combat only. Rejected because:
- Encourages mindless grinding
- Ignores existing Codex system investment

---

### Why Permanent Unlocks (Not Temporary Buffs)?

**Decision:** Purchases are permanent account upgrades, not consumable boosts.

**Rationale:**
- **Meaningful Investment:** Players don't fear "wasting" LP on wrong choice
- **Strategic Planning:** Can save LP for expensive upgrades
- **No Pressure:** Can stop playing and return without losing progress

**Alternative Considered:** Consumable buffs (e.g., "Next run has +20% XP"). Rejected because:
- Creates FOMO (fear of wasting buff)
- Pressure to play immediately after purchase

---

### Why Prerequisite Chains?

**Decision:** Some upgrades require earlier purchases (e.g., Affinity II requires Affinity I).

**Rationale:**
- **Pacing:** Prevents rushing to most powerful upgrades immediately
- **Progression Arc:** Creates sense of build-up over multiple runs
- **Balance:** Forces investment in lower-tier bonuses before unlocking game-changers

**Alternative Considered:** All upgrades independent. Rejected because:
- No strategic spending decisions
- Veterans could skip straight to "best" upgrades

---

## Changelog

### v1.0.0 (2025-12-23) - Design Specification

**Status:** Planned (NOT Implemented)

**Design Specification Only:**
- Account entity with LP tracking
- Legend service with award/purchase logic
- Upgrade catalog configuration
- Character creation integration
- Codex completion → LP award hooks

**Architectural Hooks (Exist):**
- `DataCapture.Quality` field (15/30 LP values) in Codex system
- Codex completion tracking (can trigger LP award when service implemented)

**Design Decisions:**
- Account-level progression (survives character death)
- Codex completion as primary LP source
- Permanent unlocks (not consumable buffs)
- Prerequisite chains for high-tier upgrades
- Four upgrade categories (Equipment, Attribute, Progression, QOL)

**Planned Roadmap:**
- v0.1.4: Initial Legend Service implementation
- v0.1.5: Upgrade catalog and purchase UI
- v0.1.6: Bonus application to new characters
- v0.2.0: Achievement system integration (alternative LP sources)

---

## Future Enhancements

### Prestige System

**Concept:** "Reset" all Legend Purchases for massive LP refund + prestige bonus.

**Implementation:**
```csharp
public async Task<int> PrestigeAsync(Guid accountId)
{
    var account = await _accountRepo.GetByIdAsync(accountId);

    var refundLp = account.LegendPurchases.Count * 50;  // 50 LP per purchase refunded
    account.LegendPoints += refundLp;
    account.LegendPurchases.Clear();
    account.PrestigeLevel++;  // Cosmetic badge

    await _accountRepo.SaveChangesAsync();
    return refundLp;
}
```

**Benefit:** Allows experimentation with different upgrade paths.

---

### Seasonal Legend Events

**Concept:** Time-limited bonus LP sources or discounted upgrades.

**Examples:**
```csharp
// Winter Event: +50% LP from all sources
if (IsSeasonalEvent("WINTER_FESTIVAL"))
{
    lpAwarded = (int)(lpAwarded * 1.5f);
}

// Halloween Event: Corruption Resistance upgrade 50% off
if (IsSeasonalEvent("HALLOWEEN"))
{
    var upgradeConfig = GetUpgradeConfig("UPGRADE_CORRUPTION_RESIST");
    upgradeConfig.Cost = 200;  // Normally 400 LP
}
```

**Benefit:** Drives player retention through limited-time incentives.

---

### LP Gifting (Co-op)

**Concept:** Transfer LP between accounts (friend system).

**Implementation:**
```csharp
public async Task<bool> GiftLegendPointsAsync(Guid fromAccountId, Guid toAccountId, int amount)
{
    var fromAccount = await _accountRepo.GetByIdAsync(fromAccountId);
    if (fromAccount.LegendPoints < amount) return false;

    fromAccount.LegendPoints -= amount;
    var toAccount = await _accountRepo.GetByIdAsync(toAccountId);
    toAccount.LegendPoints += amount;

    await _accountRepo.SaveChangesAsync();
    return true;
}
```

**Benefit:** Encourages community interaction, helps new players.

---

## AAM-VOICE Compliance

This specification describes mechanical systems and is exempt from Domain 4 constraints. In-game Legend Point award narration must follow AAM-VOICE guidelines:

**Compliant Example:**
```
Your understanding of the Blight-Rats deepens. Tales of their resilience will
echo in future scavenger camps. [+45 Legend Points]
```

**Non-Compliant Example:**
```
Achievement unlocked: Blight-Rat Codex 100% complete. XP: +0. LP: +45. [Layer 4 technical bleed]
```
