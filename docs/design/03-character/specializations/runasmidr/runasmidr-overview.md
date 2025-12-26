---
id: SPEC-SPECIALIZATION-RUNASMIDR
title: "Rúnasmiðr (Runesmith)"
version: 1.0
status: implemented
last-updated: 2025-12-07
---

# Rúnasmiðr (Runesmith)

---

## 1. Identity

| Property | Value |
|----------|-------|
| **Display Name** | Rúnasmiðr |
| **Translation** | "Rune-Smith" |
| **Archetype** | Adept |
| **Path Type** | Coherent |
| **Mechanical Role** | Crafter / Enchanter |
| **Primary Attribute** | WITS |
| **Secondary Attribute** | WILL |
| **Resource System** | Stamina + Runeink |
| **Trauma Risk** | Moderate (runic failures) |
| **Icon** | ᚱ |

---

## 2. Unlock Requirements

| Requirement | Value | Notes |
|-------------|-------|-------|
| **PP Cost to Unlock** | 3 PP | Standard cost |
| **Minimum Legend** | 4 | Mid-game specialization |
| **Maximum Corruption** | 75 | Cannot be heavily corrupted |
| **Required Quest** | Find Elder Runestone | Discovery prerequisite |

---

## 3. Design Philosophy

**Tagline:** "Carve the ancient patterns. Channel power you cannot comprehend."

**Core Fantasy:** You are the keeper of symbols that predate the Glitch. Where others see scratches on metal, you see the echoes of Old World power. You carve runes into weapons and armor, channeling forces your ancestors understood but you can only practice through memorized patterns and inherited techniques.

Your craft is dangerous. The runes connect to something vast and incomprehensible — the All-Rune that lurks beneath reality. Every inscription is a gamble: power in exchange for the risk of corruption.

**Mechanical Identity:**
1. **Permanent Equipment Enhancement** — Your inscriptions last forever (if successful)
2. **Runic Combat Options** — Ward runes, binding glyphs, protective circles
3. **Corruption Risk** — Greater power at the cost of potential corruption
4. **Knowledge Economy** — Learn new runes through discovery and study

---

## 4. Rank Progression

### 4.1 Rank Unlock Rules

| Tier | Starting Rank | Progresses To | Rank 3 Trigger |
|------|---------------|---------------|----------------|
| **Tier 1** | Rank 1 | Rank 2 (2× Tier 2) | Capstone trained |
| **Tier 2** | Rank 2 | Rank 3 (Capstone) | Capstone trained |
| **Tier 3** | Rank 2 | Rank 3 (Capstone) | Capstone trained |
| **Capstone** | Rank 1 | Rank 2→3 (tree) | Full tree |

### 4.2 Total PP Investment

| Milestone | PP Spent | Tier 1 Rank | Tier 2 Rank |
|-----------|----------|-------------|-------------|
| Unlock Specialization | 3 PP | - | - |
| All Tier 1 | 12 PP | Rank 1 | - |
| 2× Tier 2 | 20 PP | **Rank 2** | Rank 2 |
| All Tier 2 | 24 PP | Rank 2 | Rank 2 |
| All Tier 3 | 34 PP | Rank 2 | Rank 2 |
| Capstone | 40 PP | **Rank 3** | **Rank 3** |

---

## 5. Ability Tree

### 5.1 Visual Structure

```
                    TIER 1: FOUNDATION (3 PP each)
    ┌─────────────────────┼─────────────────────┐
    │                     │                     │
 [Rune Reader]      [Carve Ward]      [Inscription
   (Passive)          (Active)         Expertise]
    │                     │                (Passive)
    └─────────────────────┴─────────────────────┘
                          │
                          ▼
                TIER 2: ADVANCED (4 PP each)
    ┌─────────────────────┼─────────────────────┐
    │                     │                     │
[Engrave Weapon]   [Sigil of        [Armor
    (Active)        Binding]      Inscription]
    │                (Active)         (Active)
    └─────────────────────┴─────────────────────┘
                          │
                          ▼
                TIER 3: MASTERY (5 PP each)
          ┌───────────────┴───────────────┐
          │                               │
   [Runelore           [Elder Patterns]
    Mastery]                (Passive)
    (Passive)
          └───────────────┬───────────────┘
                          │
                          ▼
              TIER 4: CAPSTONE (6 PP)
                          │
               [All-Rune Glimpse]
                    (Active)
```

### 5.2 Ability Index

| ID | Ability | Tier | Type | PP | Spec Document |
|----|---------|------|------|-----|---------------|
| 1501 | Rune Reader | 1 | Passive | 3 | [rune-reader.md](abilities/rune-reader.md) |
| 1502 | Carve Ward | 1 | Active | 3 | [carve-ward.md](abilities/carve-ward.md) |
| 1503 | Inscription Expertise | 1 | Passive | 3 | [inscription-expertise.md](abilities/inscription-expertise.md) |
| 1504 | Engrave Weapon | 2 | Active | 4 | [engrave-weapon.md](abilities/engrave-weapon.md) |
| 1505 | Sigil of Binding | 2 | Active | 4 | [sigil-of-binding.md](abilities/sigil-of-binding.md) |
| 1506 | Armor Inscription | 2 | Active | 4 | [armor-inscription.md](abilities/armor-inscription.md) |
| 1507 | Runelore Mastery | 3 | Passive | 5 | [runelore-mastery.md](abilities/runelore-mastery.md) |
| 1508 | Elder Patterns | 3 | Passive | 5 | [elder-patterns.md](abilities/elder-patterns.md) |
| 1509 | All-Rune Glimpse | 4 | Active | 6 | [all-rune-glimpse.md](abilities/all-rune-glimpse.md) |

---

## 6. The Runeink System

### 6.1 Runeink as Resource

| Source | Runeink Gained |
|--------|----------------|
| Crafting (WITS check) | 5 per batch |
| Looting ruined workshops | 3-10 |
| Trading | Variable |
| Scavenging [Blighted] areas | 1-5 (risky) |

### 6.2 Runeink Costs

| Action | Base Cost | With Mastery |
|--------|-----------|--------------|
| Simple rune | 10 | 5-6 |
| Standard rune | 20 | 10-12 |
| Complex rune | 35 | 17-21 |
| Elder rune | 50 | 25-30 |
| Combination rune | 40 | 20-24 |

---

## 7. Integration Points

| System | Integration |
|--------|-------------|
| **Runeforging Craft** | Primary crafting trade |
| **Equipment** | Permanent modifications |
| **Combat** | Ward runes, binding sigils |
| **Corruption** | Capstone and failure risks |
| **Trauma Economy** | Runic stress |

---

## 8. Situational Power Profile

### 8.1 Optimal Conditions

| Situation | Why Strong |
|-----------|------------|
| Preparation time available | Can inscribe specific counters |
| Undead/Magic enemies | Wards are highly effective |
| Long campaigns | Permanent gear upgrades accumulate value |
| Defending locations | Static wards and traps excel |

### 8.2 Weakness Conditions

| Situation | Why Weak |
|-----------|----------|
| Ambush scenarios | No time to prepare runes |
| Anti-magic zones | Runes fail or backfire |
| Fast-moving skirmishes | Static effects left behind |
| High Corruption zones | Increased risk of rune-rot |

---

## 9. Party Synergies

### 9.1 Positive Synergies

| Partner | Synergy |
|---------|---------|
| **Berserkr** | Buffs mitigate their reckless defense |
| **Skjaldmær** | Armor runes make them unkillable |
| **Ruin-Stalker** | Trap runes overlap with their kit |
| **Atgeir-Wielder** | Weapon runes enhance their reach |

### 9.2 Negative Synergies

| Partner | Issue |
|---------|-------|
| **Vargr-Born** | Bestial nature clashes with precision |
| **Heretical Casters** | Competition for Aetheric bandwidth |

---

## 10. Balance Data

### 10.1 Power Curve

| Legend | Crafting Power | Combat Utility | Direct Damage |
|--------|----------------|----------------|---------------|
| 1-3 | Medium | Medium | Low |
| 4-6 | High | High | Medium |
| 7-10 | Very High | Very High | High |

### 10.2 Role Effectiveness

| Role | Rating (1-5) | Notes |
|------|--------------|-------|
| Single Target DPS | ★★☆☆☆ | Weapon buffs help |
| AoE DPS | ★★★☆☆ | Exploding runes |
| Tanking | ★★☆☆☆ | Ward stones |
| Crafting | ★★★★★ | Best in class |
| Utility | ★★★★☆ | Permanent buffs are unique |

---

## 11. Voice Guidance

**Reference:** [npc-flavor.md](../../../.templates/flavor-text/npc-flavor.md)

### 11.1 Tone Profile

| Property | Value |
|----------|-------|
| **Tone** | Reverent, obsessive, slightly corrupted |
| **Key Words** | Etch, bind, pattern, logic, rot, stabilize |
| **Sentence Style** | Complex sentences, references to "The Pattern" |

### 11.2 Example Voice

> **Activation:** "The Pattern speaks. I must obey."
> **Effect:** "Bound in iron and will. It holds."
> **Failure:** "The ink bleeds! The geometry is wrong!"

---

## 12. Phased Implementation Guide

### Phase 1: Resource & Logic
- [ ] **Factory**: Register `RunasmidrSpecialization`.
- [ ] **Runeink**: Implement `Runeink` resource on Character.

### Phase 2: Core Abilities
- [ ] **Carve Ward**: Implement tile-based ward logic.
- [ ] **Engrave Weapon**: Implement Item Modification system via Runes.

### Phase 3: Corruption & Risks
- [ ] **Backfire**: Implement `RuneFailure` chance and `Corruption` gain.
- [ ] **All-Rune**: Implement Capstone visionary effects (reveal map/enemies).

### Phase 4: UI & Feedback
- [ ] **Crafting UI**: Dedicated Rune Inscription interface.
- [ ] **Visuals**: Glowing rune decals on weapons/armor in inventory.

---

## 13. Testing Requirements

### 13.1 Unit Tests
- [ ] **Inscribe**: Add Rune to Weapon -> Weapon gains +Stat.
- [ ] **Cost**: Use Ability -> Runeink removed.
- [ ] **Corruption**: Fail check -> Corruption increases.
- [ ] **Ward**: Enemy enters tile -> Ward triggers effect.

### 13.2 Integration Tests
- [ ] **Save/Load**: Inscribed items retain runes after load.
- [ ] **Combat**: Inscribed weapon deals extra damage in combat resolution.

### 13.3 Manual QA
- [ ] **Visual**: Weapon glow effect matches Rune type (Fire = Red).
- [ ] **Log**: "Inscribed Thurisaz Rune on Longsword" message.

---

## 14. Logging Requirements

**Reference:** [logging.md](../../../00-project/logging.md)

### 14.1 Log Events

| Event | Level | Message Template | Properties |
|-------|-------|------------------|------------|
| Spec Unlock | Info | "Unlocked Specialization: Rúnasmiðr for {Character}." | `Character` |
| Inscribe | Info | "{Character} inscribed {Rune} on {Item} (Success: {Result})." | `Character`, `Rune`, `Item`, `Result` |
| Ward Trigger | Info | "{Ward} triggered on {Target} dealing {Damage}." | `Ward`, `Target`, `Damage` |
| Corruption | Warning | "Runic failure! {Character} gained {Amount} Corruption." | `Character`, `Amount` |

---

## 15. Related Documentation

| Document | Purpose |
|----------|---------|
| [Runeforging](../../04-systems/crafting/runeforging.md) | Craft trade specification |
| [Corruption System](../../01-core/corruption.md) | Corruption mechanics |
| [Skills Overview](../../01-core/skills/skills-overview.md) | Dice pool reference |

---

## 16. Changelog

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-07 | Initial specification |
| 1.1 | 2025-12-13 | Standardized with Power Profile, Synergies, Balance, and Implementation sections |
