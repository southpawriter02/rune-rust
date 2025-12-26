# v0.33.3: Faction Quests & Rewards

Type: Feature
Description: 25+ faction-specific quests with reputation requirements, 15+ faction rewards (equipment, consumables, abilities), reputation-gated unlocks. 7-11 hours.
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.33.1 (Database), v0.33.2 (Reputation mechanics)
Implementation Difficulty: Medium
Balance Validated: No
Parent item: v0.33: Faction System & Reputation (v0%2033%20Faction%20System%20&%20Reputation%20161115b505034a2fa3ad8288e2513b36.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Document ID:** RR-SPEC-v0.33.3-CONTENT

**Parent Specification:** v0.33 Faction System & Reputation

**Status:** Design Complete — Ready for Implementation

**Timeline:** 7-11 hours

**Prerequisites:** v0.33.1 (Database), v0.33.2 (Reputation mechanics)

---

## I. Overview

Faction-specific quests and rewards that unlock at reputation thresholds. 25+ quests (5 per faction) and 15+ exclusive rewards.

### Core Deliverables

- **25+ Faction Quests** with reputation requirements
- **15+ Faction Rewards** (equipment, consumables, abilities)
- **Reputation-Gated Unlocks**
- **Integration with Quest System**

---

## II. Faction Quest Examples

### Iron-Banes Quests

**Quest 1: "Purge the Rust" (Neutral, 0 reputation)**

- Kill 10 Undying enemies
- Reward: +20 Iron-Bane reputation

**Quest 2: "Corrupted Forge" (Friendly, +25 reputation)**

- Clear Undying from Muspelheim forge
- Reward: +30 reputation, Purification Sigil

**Quest 3: "Destroy the Sleeper" (Allied, +50 reputation)**

- Kill dormant Jötun-Forged (hostile to God-Sleeper Cultists)
- Reward: +40 reputation, access to Zealot's Blade

---

### God-Sleeper Cultist Quests

**Quest 1: "Offerings to the Sleeper" (Neutral, 0 reputation)**

- Donate 5 mechanical components at Jötunheim temple
- Reward: +15 reputation

**Quest 2: "Defend the Sacred" (Friendly, +25 reputation)**

- Protect dormant Jötun from Iron-Bane attack
- Reward: +30 reputation, Cultist's Blessing

---

## III. Faction Rewards

See parent specification v0.33 Section VI for complete reward lists per faction.

**Reward Types:**

- Equipment (weapons, armor)
- Consumables (buffs, healing)
- Services (merchant discounts, cache access)
- Abilities (unique powers)

---

## IV. Integration with Quest System

Faction quests use existing v0.14 Quest System architecture:

- Stored in Quests table
- Linked via Faction_Quests table
- Reputation requirement checked before display
- Reputation reward applied on completion

---

**Faction content complete. Proceed to service implementation (v0.33.4).**