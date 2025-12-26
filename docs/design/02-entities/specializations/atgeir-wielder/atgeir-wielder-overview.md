---
id: SPEC-SPECIALIZATION-ATGEIR-WIELDER
title: "Atgeir-Wielder Specialization — Overview"
version: 2.0
status: approved
last-updated: 2025-12-07
---

# Atgeir-Wielder — Overview

> *"Tactical discipline — control the battlefield"*

---

## Identity

| Property | Value |
|----------|-------|
| Internal Name | `AtgeirWielder` |
| Display Name | Atgeir-Wielder |
| Specialization ID | 12 |
| Archetype | Warrior |
| Path Type | Coherent |
| Role | Battlefield Controller / Formation Anchor |
| Primary Attribute | MIGHT |
| Secondary Attribute | WITS |
| Resource | Stamina |

---

## Unlock Requirements

| Requirement | Value |
|-------------|-------|
| PP Cost | 3 PP |
| Minimum Legend | 3 |
| Corruption | 0–100 |
| Quest | None |

---

## Design Philosophy

**Core Fantasy**: The disciplined hoplite, master of formation warfare. Your [Reach] allows tactical safety while Push/Pull effects shatter enemy formations. You are the immovable anchor.

**Mechanical Identity**:
1. **[Reach]**: Attack front row from back row
2. **Forced Movement**: Push/Pull to disrupt formations
3. **Formation Warfare**: Auras benefiting allies
4. **Defensive Anchor**: Stances that punish attackers

---

## Keywords

| Keyword | Effect | Check |
|---------|--------|-------|
| `[Reach]` | Attack front row from back row | — |
| `[Push]` | Move enemy Front → Back | MIGHT vs STURDINESS |
| `[Pull]` | Move enemy Back → Front | MIGHT vs STURDINESS |

---

## Rank Display Convention

Ability ranks use **Roman numerals** in display:

| Rank | Display |
|------|---------|
| 1 | `Skewer I` |
| 2 | `Skewer II` |
| 3 | `Skewer III` |

---

## Ability Tree

```
TIER 1: FOUNDATION (3 PP each)
├── Formal Training (Passive)
├── Skewer (Active)
└── Disciplined Stance (Active)

TIER 2: ADVANCED (4 PP each)
├── Hook and Drag (Active)
├── Line Breaker (Active)
└── Guarding Presence (Passive)

TIER 3: MASTERY (5 PP each)
├── Brace for Charge (Active)
└── Unstoppable Phalanx (Active)

TIER 4: CAPSTONE (6 PP)
└── Living Fortress (Passive)
```

---

## Ability Summary

| ID | Ability | Tier | Type | Ranks | Cost | Key Effect |
|----|---------|------|------|-------|------|------------|
| 1201 | [Formal Training](abilities/formal-training.md) | 1 | Passive | I–III | — | +Stamina regen |
| 1202 | [Skewer](abilities/skewer.md) | 1 | Active | I–III | 40 | [Reach] attack |
| 1203 | [Disciplined Stance](abilities/disciplined-stance.md) | 1 | Active | I–III | 30 | +Soak stance |
| 1204 | [Hook and Drag](abilities/hook-and-drag.md) | 2 | Active | II–III | 45 | [Pull] enemy |
| 1205 | [Line Breaker](abilities/line-breaker.md) | 2 | Active | II–III | 50 | AoE [Push] |
| 1206 | [Guarding Presence](abilities/guarding-presence.md) | 2 | Passive | II–III | — | Ally aura |
| 1207 | [Brace for Charge](abilities/brace-for-charge.md) | 3 | Active | II–III | 40 | Counter stance |
| 1208 | [Unstoppable Phalanx](abilities/unstoppable-phalanx.md) | 3 | Active | II–III | 60 | Line-pierce |
| 1209 | [Living Fortress](abilities/living-fortress.md) | 4 | Passive | I–III | — | Capstone |

---

## 8. Rank Progression

| Tier | Starting | → Rank II | → Rank III |
|------|----------|-----------|------------|
| 1 | I | 2× Tier 2 trained | Capstone trained |
| 2 | II | — | Capstone trained |
| 3 | II | — | Capstone trained |
| 4 | I | Tree-based | Full tree |

---

## 9. Total PP Investment

| Milestone | PP | Abilities |
|-----------|-----|-----------|
| Unlock | 3 | 0 |
| All Tier 1 | 12 | 3 |
| All Tier 2 | 24 | 6 |
| All Tier 3 | 34 | 8 |
| Capstone | 40 | 9 (all Rank III) |

---

## 10. Balance Data

### 10.1 Power Curve

| Legend | Control | Survivability | Damage |
|--------|---------|---------------|--------|
| 1-3 | High | Medium | Medium |
| 4-6 | Very High | High | Medium |
| 7-10 | Maximum | High | High |

### 10.2 Role Effectiveness

| Role | Rating (1-5) | Notes |
|------|--------------|-------|
| Single Target DPS | ★★★☆☆ | Reach allows safe uptime |
| AoE DPS | ★★☆☆☆ | Line Breaker only |
| Tanking | ★★★★☆ | Anchored but relies on spacing |
| Control | ★★★★★ | Best physics manipulation |
| Utility | ★★☆☆☆ | Formation buffs |

---

## 11. Voice Guidance

**Reference:** [npc-flavor.md](../../../.templates/flavor-text/npc-flavor.md)

### 11.1 Tone Profile

| Property | Value |
|----------|-------|
| **Tone** | Disciplined, military, precise |
| **Key Words** | Hold, brace, range, anchor, formation |
| **Sentence Style** | Commands, tactical assessments |

### 11.2 Example Voice

> **Activation:** "Shields lock! Spears up!"
> **Effect:** "They break against the wall."
> **Failure:** "Line breached! Fall back!"

---

## 12. Phased Implementation Guide

### Phase 1: Mechanics
- [ ] **Reach**: Implement `Reach` property (Attack range 2, ignoring Front Row penalty).
- [ ] **Physics**: Implement `Push` and `Pull` combat maneuvers.

### Phase 2: Core Abilities
- [ ] **Stances**: Implement `DisciplinedStance` (Soak bonus).
- [ ] **Skewer**: Implement basic Reach attack.

### Phase 3: Advanced Tactics
- [ ] **Line Breaker**: Implement AoE Push logic.
- [ ] **Passive**: Implement `GuardingPresence` aura.

### Phase 4: UI & Feedback
- [ ] **Visuals**: Spear thrust animation.
- [ ] **Grid**: Highlight valid Push destinations.

---

## 13. Testing Requirements

### 13.1 Unit Tests
- [ ] **Reach**: Attack from Back Row -> No Penalty.
- [ ] **Push**: Push Enemy -> Enemy moves 1 tile away.
- [ ] **Pull**: Pull Enemy -> Enemy moves 1 tile closer.
- [ ] **Stance**: Activate Stance -> Soak increases.

### 13.2 Integration Tests
- [ ] **Formation**: Guarding Presence -> Adjacent ally takes less damage.
- [ ] **Collision**: Push Enemy into Wall -> Extra damage (if applicable).

### 13.3 Manual QA
- [ ] **UI**: Valid targets for Reach attack highlighted correctly.
- [ ] **Log**: "Pushed Target 1 tile" message.

---

## 14. Logging Requirements

**Reference:** [logging.md](../../../00-project/logging.md)

### 14.1 Log Events

| Event | Level | Message Template | Properties |
|-------|-------|------------------|------------|
| Spec Unlock | Info | "Unlocked Specialization: Atgeir-Wielder for {Character}." | `Character` |
| Stance Change | Info | "{Character} entered {Stance}." | `Character`, `Stance` |
| Forced Move | Info | "{Character} pushed/pulled {Target} to {Position}." | `Character`, `Target`, `Position` |

---

## 15. Related Documentation

| Document | Purpose |
|----------|---------|
| [Combat Resolution](../../../03-combat/combat-resolution.md) | Movement rules |
| [MIGHT Attribute](../../../01-core/attributes/might.md) | Push/Pull checks |

---

## 16. Changelog

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-07 | Initial specification |
| 1.1 | 2025-12-13 | Standardized with Phased Guide, Logging, and Testing sections |
