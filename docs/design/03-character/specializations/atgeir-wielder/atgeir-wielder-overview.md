---
id: SPEC-SPECIALIZATION-ATGEIR-WIELDER
title: "Atgeir-Wielder (The Iron Anchor)"
version: 1.1
status: draft
last-updated: 2025-05-27
related-files:
  - path: "docs/99-legacy/atgeir-wielder-v1.md"
    status: Deprecated
---

# Atgeir-Wielder (The Iron Anchor)

---

## 1. Identity

| Property | Value |
|----------|-------|
| **Display Name** | Atgeir-Wielder |
| **Translation** | "Healer of the Breach" (Irony) / "The Iron Anchor" |
| **Archetype** | Warrior |
| **Path Type** | Coherent |
| **Mechanical Role** | Battlefield Controller / Formation Anchor |
| **Primary Attribute** | MIGHT |
| **Secondary Attribute** | WITS |
| **Resource System** | Stamina |
| **Trauma Risk** | Low (Disciplined Mind) |
| **Icon** | ⚔️ |

---

## 2. Unlock Requirements

| Requirement | Value | Notes |
|-------------|-------|-------|
| **PP Cost to Unlock** | 3 PP | Standard cost |
| **Minimum Legend** | 3 | Early-game specialization |
| **Maximum Corruption** | 100 | No restriction |
| **Required Quest** | None | No quest prerequisite |

---

## 3. Design Philosophy

**Tagline:** "Tactical discipline — control the battlefield."

**Core Fantasy:** You are the disciplined hoplite, the master of formation warfare. Wielding a long polearm, you command the space around you with tactical precision. Your [Reach] allows you to strike from safety while your Push and Pull effects shatter enemy formations. You are the immovable anchor that holds the line, the thinking warrior who controls where battles happen. You don't fight with rage—you fight with discipline, leverage, and perfect positioning.

**Mechanical Identity:**
1. **[Reach] Keyword**: Can attack front row from back row (tactical safety).
2. **Forced Movement**: Push enemies back, Pull enemies forward to disrupt formations.
3. **Formation Warfare**: Auras and bonuses that benefit adjacent allies.
4. **Defensive Anchor**: Stance abilities that make you immovable and punish attackers.

**Gameplay Feel:** Calculated, methodical, and immovable. You dictate the flow of combat, forcing enemies into disadvantageous positions while protecting your allies with your mere presence.

---

## 4. Mechanics & Keywords

### 4.1 Keywords

| Keyword | Description | Opposed Check |
|---------|-------------|---------------|
| `[Reach]` | Can attack Front Row enemies while positioned in Back Row. | None |
| `[Push]` | Move enemy from Front → Back Row. | MIGHT vs STURDINESS |
| `[Pull]` | Move enemy from Back → Front Row. | MIGHT vs STURDINESS |

### 4.2 Targeting Rules

| Your Position | Normal Attack Range | With [Reach] |
|---------------|---------------------|--------------|
| Front Row | Front Row enemies | Front + Back Row enemies |
| Back Row | Cannot melee | Front Row enemies |

---

## 5. Rank Progression

### 5.1 Rank Unlock Rules

| Tier | Starting Rank | Progresses To | Rank 3 Trigger |
|------|---------------|---------------|----------------|
| **Tier 1** | Rank 1 | Rank 2 (2× Tier 2) | Capstone trained |
| **Tier 2** | Rank 2 | Rank 3 (Capstone) | Capstone trained |
| **Tier 3** | Rank 2 | Rank 3 (Capstone) | Capstone trained |
| **Capstone** | Rank 1 | Rank 2→3 (tree) | Full tree |

### 5.2 Total PP Investment

| Milestone | PP Spent | Tier 1 Rank | Notes |
|-----------|----------|-------------|-------|
| Unlock Specialization | 3 PP | - | |
| All Tier 1 | 12 PP | Rank 1 | 3 abilities |
| 2× Tier 2 | 20 PP | **Rank 2** | Unlocks Rank 2 for Tier 1 |
| All Tier 2 | 24 PP | Rank 2 | |
| All Tier 3 | 34 PP | Rank 2 | |
| Capstone | 40 PP | **Rank 3** | Full tree mastery |

---

## 6. Ability Tree

### 6.1 Visual Structure

```
TIER 1: FOUNDATION (3 PP each, Ranks 1-3)
┌─────────────────────┼─────────────────────┐
│                     │                     │
[Formal Training]   [Skewer]        [Disciplined Stance]
   (Passive)        (Active)              (Active)
│                     │                     │
└─────────────────────┴─────────────────────┘
                      │
                      ▼
            RANK 2 UNLOCKS HERE
         (when 2 Tier 2 trained)
                      │
                      ▼
TIER 2: ADVANCED (4 PP each)
┌─────────────────────┼─────────────────────┐
│                     │                     │
[Hook and Drag]   [Line Breaker]  [Guarding Presence]
   (Active)          (Active)           (Passive)
│                     │                     │
└─────────────────────┴─────────────────────┘
                      │
                      ▼
TIER 3: MASTERY (5 PP each)
┌───────────────┴───────────────┐
│                               │
[Brace for Charge]      [Unstoppable Phalanx]
     (Active)                  (Active)
│                               │
└───────────────┬───────────────┘
                │
                ▼
            RANK 3 UNLOCKS HERE
         (when Capstone trained)
                │
                ▼
TIER 4: CAPSTONE (6 PP)
                │
        [Living Fortress]
           (Passive)
```

### 6.2 Ability Summary

| ID | Ability Name | Tier | Type | Resource | Key Effect |
|----|--------------|------|------|----------|------------|
| 1201 | Formal Training | 1 | Passive | None | +Stamina regen, resist disruption. |
| 1202 | Skewer | 1 | Active | 40 Stam | [Reach] attack. |
| 1203 | Disciplined Stance | 1 | Active | 30 Stam | +Soak, resist Push/Pull. |
| 1204 | Hook and Drag | 2 | Active | 45 Stam | [Pull] enemy to front. |
| 1205 | Line Breaker | 2 | Active | 50 Stam | AoE [Push] enemies back. |
| 1206 | Guarding Presence | 2 | Passive | None | Aura: +Soak for allies. |
| 1207 | Brace for Charge | 3 | Active | 40 Stam | Counter-attack stance. |
| 1208 | Unstoppable Phalanx | 3 | Active | 60 Stam | Line-piercing attack. |
| 1209 | Living Fortress | 4 | Passive | None | Forced movement immunity, Zone of Control. |

---

## 7. Ability Details

### 7.1 Tier 1: Foundation

#### Formal Training (ID: 1201)
*Passive. Your formal training instills deep physical and mental discipline.*

| Rank | Effect |
|------|--------|
| **1** | +5 Stamina regen/turn. +1d10 bonus dice to Resolve vs [Stagger]. |
| **2** | +7 Stamina regen/turn. +1d10 bonus dice vs [Stagger] AND [Disoriented]. |
| **3** | +10 Stamina regen/turn. +2d10 vs [Stagger]/[Disoriented]. **+1 WITS (Permanent).** |

#### Skewer (ID: 1202)
*Active. MIGHT vs DEFENSE. A precise, powerful thrust designed to exploit your weapon's length.*

| Rank | Cost | Effect |
|------|------|--------|
| **1** | 40 Stam | 2d8 Physical damage. **[Reach]:** Attack Front Row from Back Row. |
| **2** | 35 Stam | 2d8 + 1d10 Physical damage. |
| **3** | 35 Stam | 2d8 + 2d10 Physical damage. **Crit:** Apply [Bleeding] (2 turns). |

#### Disciplined Stance (ID: 1203)
*Active (Bonus Action). You plant your feet, becoming an anchor of stability.*

| Rank | Cost | Effect |
|------|------|--------|
| **1** | 30 Stam | 2 turns: +4 Soak, +3 dice to resist Push/Pull. Cannot move. |
| **2** | 30 Stam | 3 turns: +6 Soak, +4 dice to resist Push/Pull. Cannot move. |
| **3** | 30 Stam | **Status:** +8 Soak. **Immune** to Push/Pull. +5 Stamina regen while in stance. |

---

### 7.2 Tier 2: Advanced

#### Hook and Drag (ID: 1204)
*Active. MIGHT vs STURDINESS. Using your weapon's hooked blade, you violently yank a priority target out of position.*

| Rank | Cost | Effect |
|------|------|--------|
| **2** | 45 Stam | 3d8 Physical. **[Pull]** Back → Front (+2 bonus). If Pulled: [Slowed] 1 turn. |
| **3** | 45 Stam | 4d8 Physical. **[Pull]** (+3 bonus). If Pulled: [Slowed] + [Stunned] 1 turn. |

#### Line Breaker (ID: 1205)
*Active (AoE). A wide, sweeping strike that shatters enemy formations.*

| Rank | Cost | Effect |
|------|------|--------|
| **2** | 50 Stam | Target **All Front Row Enemies**: 4d10 Physical. **[Push]** Front → Back (+1 bonus). +1d10 damage on Push. |
| **3** | 50 Stam | 5d10 Physical. **[Push]** (+2 bonus). Pushed enemies are [Off-Balance] (-2 Hit) for 1 turn. |

#### Guarding Presence (ID: 1206)
*Passive Aura. Your disciplined presence inspires fortitude in those around you.*

| Rank | Effect |
|------|--------|
| **2** | While in Front Row: Adjacent allies gain +2 Soak, +1 die vs [Fear]. |
| **3** | While in Front Row: Adjacent allies gain +3 Soak, +1 die vs [Fear]. **Allies regen +3 Stamina/turn.** |

---

### 7.3 Tier 3: Mastery

#### Brace for Charge (ID: 1207)
*Active (Reaction/Stance). You set your weapon with expert precision. They will run onto your spear and break.*

| Rank | Cost | Effect |
|------|------|--------|
| **2** | 40 Stam | 1 turn: +10 Soak, Immune [Knockdown]. **On Melee Hit:** Deal 5d8 Physical, Attacker WILL save (DC 15) or [Stunned]. |
| **3** | 40 Stam | Counter damage 6d8, Stun DC 18. **Mechanical/Undying attackers are auto-Stunned.** |

#### Unstoppable Phalanx (ID: 1208)
*Active. Your polearm punches through armor and flesh, impaling one target and striking another.*

| Rank | Cost | Effect |
|------|------|--------|
| **2** | 60 Stam | Primary: 7d10 Physical. If hit: Secondary (behind) takes 5d10 Physical. Both [Off-Balance] 1 turn. |
| **3** | 60 Stam | Primary: 8d10. Secondary: 6d10. **If primary dies: Secondary takes 2× damage.** |

---

### 7.4 Tier 4: Capstone

#### Living Fortress (ID: 1209)
*Passive. You have become the absolute master of your domain. A living fortress around which battles are won.*

| Rank | Effect |
|------|--------|
| **1** | In Front Row: **Immune** to [Push]/[Pull]. Brace for Charge can be used as **Reaction** (1×/combat). |
| **2** | Aura: Adjacent allies +3 dice vs Push/Pull. Skewer range +1 row (can hit Back Row from Front). |
| **3** | **Zone of Control:** Enemies in opposing Front Row suffer -1 Hit and restricted movement. Brace as Reaction 2×/combat. |

---

## 8. Status Effect Definitions

| Effect | Duration | Icon | Color | Effects |
|--------|----------|------|-------|---------|
| `[Slowed]` | 1 turn | ⚓ | Blue | Movement costs doubled, Cannot use movement abilities. |
| `[Off-Balance]` | 1 turn | 💫 | Yellow | -2 to attack rolls, -1 Defense. |
| `[Stunned]` | 1 turn | ✨ | Yellow | Cannot take actions, -4 Defense. |
| `[Bleeding]` | 2 turns | 🩸 | Red | 1d10 Physical damage at start of turn. |
| `[Disciplined]` | 2-3 turns | 🛡️ | Gray | +Soak, Bonus dice vs Push/Pull, Cannot move. |

---

## 9. Party Synergies

### 9.1 Positive Synergies

| Partner | Synergy |
|---------|---------|
| **Berserkr** | You hold the line so they can focus on pure damage. Your [Stun] sets up their crits. |
| **Skald** | Their songs buff your Soak/Stamina, making you truly unkillable. |
| **Ranged DPS** | You keep enemies in the Back Row or hold them in Front Row, controlling line of sight. |

### 9.2 Negative Synergies

| Partner | Issue |
|---------|-------|
| **Mobile Skirmishers** | Your stationary playstyle ("Cannot move") clashes with high-mobility teams. |
| **Stealth** | You are a beacon of aggro; hard to be subtle with a "Zone of Control". |

---

## 10. Voice Guidance

**Reference:** Narrator Persona

### 10.1 Tone Profile

| Property | Value |
|----------|-------|
| **Tone** | Stoic, professional, weary but unbreakable. |
| **Key Words** | Position, leverage, discipline, anchor, wall. |
| **Sentence Style** | Precise, instructional, understated confidence. |

### 10.2 Example Voice

> **Activation:** "Hold the line. No one passes."
> **Effect:** "Leverage is superior to rage. They break themselves upon you."
> **Failure:** "Structure... compromised."

---

## 11. Implementation Status

- [x] Data Seeding (`RuneAndRust.Persistence/DataSeeder.cs`)
- [x] [Reach] Keyword Logic
- [x] [Push]/[Pull] Mechanics
- [ ] Visual Indicator for Zone of Control (GUI)
- [ ] TUI Representation of Stance (Status Line)

---

## 12. Changelog

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-11-27 | Initial specification from DataSeeder implementation. |
| 1.1 | 2025-05-27 | Migrated to Golden Standard (Worldbuilder Persona). Updated format. |
