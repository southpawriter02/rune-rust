---
id: SPEC-SPECIALIZATION-BONE-SETTER
title: "Bone-Setter (Restorer of Coherence)"
version: 1.0
status: implemented
last-updated: 2025-12-07
---

# Bone-Setter (Restorer of Coherence)

---

## 1. Identity

| Property | Value |
|----------|-------|
| **Display Name** | Bone-Setter |
| **Translation** | "Restorer of Coherence" |
| **Archetype** | Adept |
| **Path Type** | Coherent |
| **Mechanical Role** | Support / Healer |
| **Primary Attribute** | WITS |
| **Secondary Attribute** | FINESSE |
| **Resource System** | Stamina + Consumables |
| **Trauma Risk** | None (Coherent path) |
| **Icon** | ⚕️ |

---

## 2. Unlock Requirements

| Requirement | Value | Notes |
|-------------|-------|-------|
| **PP Cost to Unlock** | 10 PP | Higher cost (critical role) |
| **Minimum Legend** | 2 | Early-game available |
| **Maximum Corruption** | 100 | No restriction |
| **Required Quest** | None | Skill-based unlock |

---

## 3. Design Philosophy

**Tagline:** "Your saga is not written in the enemies you defeat, but in the lives you save."

**Core Fantasy:** You are the indispensable combat medic and anchor of sanity. Where others break bodies, you mend them. Where minds shatter, you restore coherence. You are the quiet, unassuming hero who ensures the party survives long enough to write their own sagas.

**Mechanical Identity:**
1. **Non-Magical Healer** — Skill-based, pragmatic medicine
2. **Resource Dependent** — Effectiveness requires crafted supplies
3. **Reactive Play** — Respond to damage and status effects
4. **Trauma Economy Manager** — Remove Stress, cleanse mental effects
5. **Preparation Loop** — Craft during downtime, deploy during crisis

**Gameplay Feel:** Playing Bone-Setter is a game of **triage, resource management, and keeping a cool head in crisis.** Success requires preparation — the Bone-Setter who enters the field without a well-stocked medical kit cannot fulfill their role.

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

| Milestone | PP Spent | Tier 1 Rank | Notes |
|-----------|----------|-------------|-------|
| Unlock Specialization | 10 PP | - | Higher cost |
| All Tier 1 | 19 PP | Rank 1 | 3 abilities |
| 2× Tier 2 | 27 PP | **Rank 2** | Rank 2 unlocks |
| All Tier 2 | 31 PP | Rank 2 | |
| All Tier 3 | 41 PP | Rank 2 | |
| Capstone | 47 PP | **Rank 3** | Full tree |

---

## 5. Ability Tree

### 5.1 Visual Structure

```
                    TIER 1: FOUNDATIONAL MEDICINE (3 PP each)
    ┌─────────────────────┼─────────────────────┐
    │                     │                     │
 [Field Medic I]     [Mend Wound]        [Apply
   (Passive)           (Active)        Tourniquet]
    │                     │                (Active)
    └─────────────────────┴─────────────────────┘
                          │
                          ▼
                TIER 2: ADVANCED TREATMENT (4 PP each)
    ┌─────────────────────┼─────────────────────┐
    │                     │                     │
[Anatomical        [Administer         [Triage]
  Insight]          Antidote]          (Passive)
  (Active)           (Active)
    └─────────────────────┴─────────────────────┘
                          │
                          ▼
                TIER 3: MASTERY (5 PP each)
          ┌───────────────┴───────────────┐
          │                               │
  [Cognitive         ["First, Do
  Realignment]         No Harm"]
    (Active)            (Passive)
          └───────────────┬───────────────┘
                          │
                          ▼
              TIER 4: CAPSTONE (6 PP)
                          │
                [Miracle Worker]
                    (Active)
```

### 5.2 Ability Index

| ID | Ability | Tier | Type | PP | Spec Document |
|----|---------|------|------|-----|---------------|
| 2001 | Field Medic I | 1 | Passive | 3 | [field-medic-i.md](abilities/field-medic-i.md) |
| 2002 | Mend Wound | 1 | Active | 3 | [mend-wound.md](abilities/mend-wound.md) |
| 2003 | Apply Tourniquet | 1 | Active | 3 | [apply-tourniquet.md](abilities/apply-tourniquet.md) |
| 2004 | Anatomical Insight | 2 | Active | 4 | [anatomical-insight.md](abilities/anatomical-insight.md) |
| 2005 | Administer Antidote | 2 | Active | 4 | [administer-antidote.md](abilities/administer-antidote.md) |
| 2006 | Triage | 2 | Passive | 4 | [triage.md](abilities/triage.md) |
| 2007 | Cognitive Realignment | 3 | Active | 5 | [cognitive-realignment.md](abilities/cognitive-realignment.md) |
| 2008 | First, Do No Harm | 3 | Passive | 5 | [first-do-no-harm.md](abilities/first-do-no-harm.md) |
| 2009 | Miracle Worker | 4 | Active | 6 | [miracle-worker.md](abilities/miracle-worker.md) |

---

## 6. Field Medicine Integration

### 6.1 Crafting Bonuses

| Source | Bonus |
|--------|-------|
| Bone-Setter specialization | -2 DC, +1d10 |
| Field Medic I (Rank 1) | +1d10 to crafting |
| Field Medic I (Rank 2) | +2d10 to crafting |
| Field Medic I (Rank 3) | +3d10 to crafting |

### 6.2 Starting Supplies

Each expedition, Bone-Setter begins with:
- 3× Healing Poultice (Standard quality)
- 1× Soot-Stained Bandage

See: [Field Medicine](../../04-systems/crafting/field-medicine.md)

---

## 7. Situational Power Profile

### 7.1 Optimal Conditions

| Situation | Why Strong |
|-----------|------------|
| Long attrition encounters | Sustained healing value |
| Bleed/poison enemies | Direct counters |
| High-stress expeditions | Cognitive Realignment critical |
| High-risk party members | Enable risky strategies |

### 7.2 Weakness Conditions

| Situation | Why Weak |
|-----------|----------|
| Short burst fights | Insufficient time |
| No preparation time | Enters without supplies |
| Magical/energy threats | Anatomical Insight ineffective |

---

## 8. Party Synergies

### 8.1 Positive Synergies

| Partner | Synergy |
|---------|---------|
| **Berserkr** | Enables self-harm strategies |
| **Skald** | Complete Trauma Economy coverage |
| **Skjaldmær** | Tank survives longer with healing |
| **Vargr-Born** | Aggressive builds need healing |

### 8.2 Negative Synergies

| Partner | Issue |
|---------|-------|
| Other healers | Redundancy |
| Fast strikers | May outpace positioning |

---

## 9. Integration Points

| System | Integration |
|--------|-------------|
| **Field Medicine** | Primary crafting trade |
| **Status Effects** | Cleansing specialist |
| **Trauma Economy** | Stress removal |
| **Combat** | Reactive healing |

---

## 10. Balance Data

### 10.1 Power Curve

| Legend | Healing Output | Survivability | Utility |
|--------|----------------|---------------|---------|
| 1-3 | Medium | Medium | Low |
| 4-6 | High | High | Medium |
| 7-10 | Very High | High | High |

### 10.2 Role Effectiveness

| Role | Rating (1-5) | Notes |
|------|--------------|-------|
| Single Target DPS | ★☆☆☆☆ | weak combatant |
| AoE DPS | ☆☆☆☆☆ | None |
| Tanking | ★★☆☆☆ | Can self-sustain but lacks mitigation |
| Healing | ★★★★★ | Best in class |
| Utility | ★★★☆☆ | Status removal is critical |

---

## 11. Voice Guidance

**Reference:** [npc-flavor.md](../../../.templates/flavor-text/npc-flavor.md)

### 11.1 Tone Profile

| Property | Value |
|----------|-------|
| **Tone** | Clinical, reassuring, weary but determined |
| **Key Words** | Stitch, mend, breathe, stabilize, focus |
| **Sentence Style** | Imperative instructions, calm statements |

### 11.2 Example Voice

> **Activation:** "Hold still. I can fix this."
> **Effect:** "The flesh knits. You're not finished yet."
> **Failure:** "Too much damage... I can't stop the bleeding!"

---

## 12. Phased Implementation Guide

### Phase 1: Registration & Logic
- [ ] **Factory**: Register `BoneSetterSpecialization`.
- [ ] **Modifiers**: Implement `HealingBonus` property support in `Character`.

### Phase 2: Core Abilities
- [ ] **Mend Wound**: Implement basic heal logic.
- [ ] **Field Medic Passives**: Implement Crafting Bonus modifiers.

### Phase 3: Status Integration
- [ ] **Cleansing**: Implement `RemoveStatus` logic for Tourniquet/Antidote.
- [ ] **Triage**: Implement conditional passive (Low HP detection).

### Phase 4: UI & Feedback
- [ ] **Visuals**: Green healing number popups.
- [ ] **Crafting UI**: Show "Specialization Bonus: +1d10" in crafting menu.

---

## 13. Testing Requirements

### 13.1 Unit Tests
- [ ] **Heal**: Mend Wound -> Target HP increases.
- [ ] **Cleanse**: Apply Tourniquet -> Bleeding removed.
- [ ] **Bonus**: Crafting Roll with Bone-Setter -> Has +1d10 bonus.
- [ ] **Triage**: Active when ally < 50% HP.

### 13.2 Integration Tests
- [ ] **Field Medicine**: Verify Bone-Setter starts with Medkits.
- [ ] **Stress**: Cognitive Realignment reduces Stress correctly.

### 13.3 Manual QA
- [ ] **UI**: Verify healing numbers appear in combat log.
- [ ] **Crafting**: Check simple vs complex crafting DCs with bonus.

---

## 14. Logging Requirements

**Reference:** [logging.md](../../../00-project/logging.md)

### 14.1 Log Events

| Event | Level | Message Template | Properties |
|-------|-------|------------------|------------|
| Spec Unlock | Info | "Unlocked Specialization: Bone-Setter for {Character}." | `Character` |
| Heal | Info | "{Character} healed {Target} for {Amount} HP." | `Character`, `Target`, `Amount` |
| Cleanse | Info | "{Character} removed {Status} from {Target}." | `Character`, `Status`, `Target` |
| Craft Bonus | Debug | "Bone-Setter bonus applied to {Trade} check." | `Trade` |

---

## 15. Related Documentation

| Document | Purpose |
|----------|---------|
| [Field Medicine](../../04-systems/crafting/field-medicine.md) | Crafting trade |
| [Status Effects](../../04-systems/status-effects/status-effects-overview.md) | Effects cleansed |
| [Bleeding](../../04-systems/status-effects/bleeding.md) | Apply Tourniquet target |
| [Poisoned](../../04-systems/status-effects/poisoned.md) | Administer Antidote target |

---

## 16. Changelog

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-07 | Initial specification |
| 1.1 | 2025-12-13 | Standardized with Phased Guide, Logging, and Testing sections |
