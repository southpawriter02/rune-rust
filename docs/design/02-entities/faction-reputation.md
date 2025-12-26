---
id: SPEC-FACTIONS
title: "Faction System & Reputation"
version: 1.0
status: design-complete
last-updated: 2025-12-07
related-files:
  - path: "docs/02-entities/npcs/"
    status: Active
---

# Faction System & Reputation

---

## 1. Overview

The Faction System brings **social consequences and political depth** to Aethelgard. Every action has witnesses. Every choice has factions that approve or condemn.

This is not a simple good/evil morality system. It's a **web of competing interests** where logical choices create enemies.

> [!TIP]
> Help the Iron-Banes purge Undying? The God-Sleeper Cultists mark you as a heretic. Share technology with Jötun-Readers? Rust-Clans see you as a hoarder.

---

## 2. Reputation Scale

| Tier | Range | Effects |
|------|-------|---------|
| Hated | -100 to -76 | Kill on sight, +50% prices |
| Hostile | -75 to -26 | Attack if provoked, +25% prices |
| Neutral | -25 to +24 | Standard interactions |
| Friendly | +25 to +49 | -10% prices, some quests |
| Allied | +50 to +74 | -20% prices, most content |
| Exalted | +75 to +100 | -30% prices, all rewards |

---

## 3. The Five Major Factions

### 3.1 Iron-Banes (Anti-Undying Zealots)

**Philosophy:** "The Undying are corrupted processes that must be purged."

| Aspect | Details |
|--------|---------|
| Location | Trunk/Roots patrols |
| Allies | Rust-Clans |
| Enemies | God-Sleeper Cultists |

**Reputation:**
- +10: Kill Undying
- +30: Destroy corrupted Jötun-Forged
- -30: Attack Iron-Bane patrol

**Rewards:**
- Friendly: 10% discount
- Allied: Purification Sigils
- Exalted: Zealot's Blade (+2d6 vs Undying)

---

### 3.2 God-Sleeper Cultists (Jötun-Forged Worshippers)

**Philosophy:** "The Jötun-Forged are sleeping gods awaiting the signal to awaken."

| Aspect | Details |
|--------|---------|
| Location | Jötunheim temples |
| Allies | Independents (tolerated) |
| Enemies | Iron-Banes |

**Reputation:**
- +30: Donate resources to temple
- +50: Protect dormant Jötun
- -20: Kill Jötun-Forged

**Rewards:**
- Friendly: Cultist's Blessing (+4 near Jötun)
- Allied: Jötun-Touched Robe
- Exalted: God-Sleeper's Grimoire

---

### 3.3 Jötun-Readers (Pre-Glitch Scholars)

**Philosophy:** "Knowledge is the only path to understanding the Glitch."

| Aspect | Details |
|--------|---------|
| Location | Alfheim, terminals |
| Allies | Rust-Clans (trade) |
| Enemies | None |

**Reputation:**
- +10: Recover data-log
- +30: Donate rare knowledge
- -20: Destroy data without reading

**Rewards:**
- Friendly: Archive access
- Allied: Scholar's Focus (+2 WITS)
- Exalted: Decryption Protocols

---

### 3.4 Rust-Clans (Midgard Survivors)

**Philosophy:** "Survival first. No ideology, no worship, no grand theories."

| Aspect | Details |
|--------|---------|
| Location | Midgard, trade outposts |
| Allies | Iron-Banes, Jötun-Readers |
| Enemies | Raiders |

**Reputation:**
- +10: Trade resources
- +25: Complete quest
- -15: Steal from merchants

**Rewards:**
- Friendly: 15% discount
- Allied: Scavenger's Kit
- Exalted: Clan Sigil (hidden caches)

---

### 3.5 Independents (Unaffiliated)

**Philosophy:** "Factions are chains. We walk our own path."

| Aspect | Details |
|--------|---------|
| Location | Anywhere |
| Allies | None |
| Enemies | None |

**Rewards:**
- Exalted (+100): Lone Wolf (+10% all stats solo)

---

## 4. Faction Dynamics

### 4.1 Mutual Exclusivity

Cannot maximize all factions simultaneously:

| Pair | Relationship |
|------|--------------|
| Iron-Banes ↔ God-Sleepers | Hostile |
| Rust-Clans ↔ Jötun-Readers | Friendly |
| Independents ↔ Any | Neutral |

### 4.2 Witness System

Actions affect reputation when observed by faction members:
- Direct action: Full reputation change
- Witnessed action: 75% reputation change
- Unwitnessed: No change

---

## 5. Balance Data

### 5.1 Reputation Economy
| Source | Gain | Frequency | Note |
|--------|------|-----------|------|
| Minor Task | +5 | Repeatable | Fetch quests, donations |
| Major Quest | +25 | Once | Plot points, big favors |
| Ideological Kill | +10 | Grindable | Slow grind (Iron-Banes vs Undying) |
| Betrayal | -30 | Contextual | Attacking members, stealing |

### 5.2 Reward Value Analysis
- **Discount (-10% to -30%):** Significant economy sink reduction over time.
- **Exclusive Gear:** Mid-tier uniques (Tier 2/3 equivalent). Not game-breaking but distinct.
- **Access:** Additional content (quests/areas) is the primary driver, not just stats.

---

## 6. Phased Implementation Guide

### Phase 1: Core Systems
- [ ] **Data Structure**: Create `Faction`, `ReputationEntry` classes.
- [ ] **Manager**: Implement `FactionService` with `ModifyReputation` logic.
- [ ] **Persistence**: Ensure reputation state saves/loads correctly.

### Phase 2: Logic Integration
- [ ] **Witnessing**: Implement `WitnessSystem` to scale reputation changes (100% vs 75%).
- [ ] **Thresholds**: Hook events when `GetTier()` changes (e.g. Hostile -> Neutral).
- [ ] **Prices**: Hook `MerchantService` to apply discounts based on Tier.

### Phase 3: Content & UI
- [ ] **UI**: "Reputation Sheet" showing bars/tiers for 5 factions.
- [ ] **Notifications**: `"+10 Iron-Bane Reputation"` floaters.
- [ ] **Quests**: Gate quest availability via `FactionRequirement`.

---

## 7. Testing Requirements

### 7.1 Unit Tests
- [ ] **Math**: `ModifyReputation(Faction, +10)` -> CurrentRep increases by 10.
- [ ] **Clamping**: Max +100, Min -100.
- [ ] **Tiers**: -26 is Hostile, -25 is Neutral. Boundary verification.
- [ ] **Witness**: `ModifyReputation(..., witnessed: false)` -> No change (if spec says Unwitnessed = No Change). Wait, spec says "Unwitnessed: No change". Correct.

### 7.2 Integration Tests
- [ ] **Merchants**: Friendly = 10% discount applied to Item Cost.
- [ ] **Combat**: Attack Iron-Bane -> Reputation drops -> Become Hostile -> They attack back.
- [ ] **Exclusivity**: Max Iron-Banes -> God-Sleepers become Hostile (due to Opposing Faction logic if implemented, or just natural negative rep gains from Iron-Bane quests).

### 7.3 Manual QA
- [ ] **UI**: Verify bar fills correctly and color changes (Red/Yellow/Green).

---

## 8. Logging Requirements

**Reference:** [logging.md](../../../00-project/logging.md)

### 8.1 Log Events
| Event | Level | Message Template | Properties |
|-------|-------|------------------|------------|
| RepChange | Info | "Reputation with {Faction}: {Change} (Total: {NewTotal})" | `Faction`, `Change`, `NewTotal` |
| TierChange | Info | "Your standing with {Faction} is now {NewTier}!" | `Faction`, `NewTier` |
| Conflict | Warn | "Action witnessed by {Faction}! Reputation impacted." | `Faction` |

---

## 9. Settlement-Specific Effects

### 9.1 Settlement Access

Faction reputation determines settlement access:

| Reputation | Access Level |
|------------|--------------|
| **Hated** | Barred entry; attacked on sight |
| **Hostile** | Barred entry; guards hostile |
| **Unfriendly** | Entry permitted; services restricted |
| **Neutral** | Full entry; standard services |
| **Friendly** | Full access; advanced services |
| **Honored** | VIP access; faction quarters open |
| **Exalted** | Leadership access; all areas open |

### 9.2 Service Gating by Reputation

Settlement services require minimum reputation with the controlling faction:

| Service | Minimum Reputation | Notes |
|---------|-------------------|-------|
| **Basic Commerce** | Unfriendly | Limited stock, +50% prices |
| **Full Commerce** | Neutral | Standard stock and prices |
| **Basic Crafting** | Neutral | Repair, simple crafting |
| **Advanced Crafting** | Friendly | Modifications, rare materials |
| **Rest (Short)** | Unfriendly | Public spaces only |
| **Rest (Full)** | Neutral | Tavern rooms |
| **Healing** | Neutral | Temple services |
| **Corruption Removal** | Friendly | Major temple services |
| **Training** | Friendly | Skill advancement |
| **Faction Quests** | Friendly | Faction-specific content |
| **Rare Items** | Honored | Unique inventory access |
| **Faction Enhancements** | Exalted | Exclusive crafting options |

### 9.3 Multi-Faction Settlements

Large settlements may have multiple faction presences. Each faction zone uses its own reputation:

```
CROSSROADS HOLD:
├── Gate District [Midgard Combine] - Uses Combine reputation
├── Market District [Neutral Ground] - Best of all reputations
├── Dvergr Imports [Dvergr Trade Mission] - Uses Dvergr reputation
└── Rangers Post [Rangers Guild] - Uses Rangers reputation
```

**Neutral Ground Rules:**
- Service availability: Use best reputation among present factions
- Pricing: Use best faction modifier
- Quest access: Each faction's quests require that faction's reputation

### 9.4 Faction-Controlled Settlements

Each faction controls specific settlement types with unique benefits:

| Faction | Settlement Types | Unique Access |
|---------|------------------|---------------|
| **Midgard Combine** | Holds, trade hubs | Toll road exemption at Honored |
| **Rangers Guild** | Outposts, waypoints | Free guide services at Friendly |
| **Rust-Clans** | Rig-stops, salvage camps | Rig berth access at Friendly |
| **Dvergr Hegemony** | Nidavellir, Deep Gates | Deep Gate passage at Honored |
| **Scavenger Barons** | Ice stations | Dreadnought berth at Friendly |
| **Hearth-Clans** | Muspelheim settlements | Thermal gear discounts at Friendly |
| **Jötun-Readers** | Archives, scriptoria | Archive full access at Honored |

---

## 10. Related Specifications
| Document | Purpose |
|----------|---------|
| [NPC Companions](npc-companions.md) | Faction-gated recruitment |
| [Quest System](../01-core/saga-system.md) | Faction quests |
| [Economy](../01-core/resources/money.md) | Discounts |
| [Settlements](../07-environment/settlements.md) | Settlement service mechanics |

---

## 11. Changelog
| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-07 | Initial specification |
| 1.1 | 2025-12-14 | Standardized with Balance, Phased Guide, Testing, Logging |
| 1.2 | 2025-12-14 | Added Settlement-Specific Effects (Section 9) |
