---
id: SPEC-COMPANIONS
title: "NPC Companion System"
version: 1.0
status: design-complete
last-updated: 2025-12-07
related-files:
  - path: "docs/02-entities/faction-reputation.md"
    status: Active
---

# NPC Companion System

---

## 1. Overview

The Companion System transforms Rune & Rust from a solo experience into a **tactical squad-based game** where you recruit, equip, command, and develop AI-controlled party members.

Companions are not disposable summons—they are persistent characters with progression, personality, and mechanical depth.

**Party Size:** 4 total (1 player + 3 companions max)

---

## 2. The Six Companions

### 2.1 Kára Ironbreaker (Warrior — Iron-Bane)

| Aspect | Details |
|--------|---------|
| Requirement | Iron-Bane Friendly (+25) |
| Location | Iron-Bane enclave (Trunk) |
| Role | Tank / Anti-Undying |

**Abilities:** Shield Bash, Taunt, Purification Strike (+2d6 vs Undying)

**Personal Quest:** "The Last Protocol" — Recover her squad's final mission data

---

### 2.2 Finnr the Rust-Sage (Mystic — Jötun-Reader)

| Aspect | Details |
|--------|---------|
| Requirement | Jötun-Reader Friendly (+25) |
| Location | Alfheim archives |
| Role | Support / Knowledge |

**Abilities:** Aetheric Bolt, Data Analysis (reveal weaknesses), Runic Shield

**Personal Quest:** "The Forlorn Archive" — Access restricted database

---

### 2.3 Bjorn Scrap-Hand (Adept — Rust-Clan)

| Aspect | Details |
|--------|---------|
| Requirement | Rust-Clan Neutral (0) |
| Location | Midgard outpost |
| Role | Utility / Crafting |

**Abilities:** Improvised Repair, Scrap Grenade, Resourceful (extra loot)

**Personal Quest:** "The Old Workshop" — Reclaim family workshop

---

### 2.4 Valdis the Forlorn-Touched (Mystic — Independent)

| Aspect | Details |
|--------|---------|
| Requirement | None (rescue) |
| Location | Niflheim ruins |
| Role | Glass Cannon / Psychic |

**Abilities:** Spirit Bolt, Forlorn Whisper (fear), Fragile Mind (high damage/low HP)

**Personal Quest:** "Breaking the Voices" — Confront the Forlorn

---

### 2.5 Runa Shield-Sister (Warrior — Independent)

| Aspect | Details |
|--------|---------|
| Requirement | Complete "Defend Caravan" |
| Location | Jötunheim yards |
| Role | Tank / Bodyguard |

**Abilities:** Defensive Stance, Interpose, Shield Wall

**Personal Quest:** "The Broken Oath" — Track betrayer

---

### 2.6 Einar the God-Touched (Warrior — God-Sleeper)

| Aspect | Details |
|--------|---------|
| Requirement | God-Sleeper Friendly (+25) |
| Location | Jötunheim temple |
| Role | DPS / Conditional Powerhouse |

**Abilities:** Berserker Rage, Jötun Attunement (+4 near corpses), Reckless Strike

**Personal Quest:** "Awaken the Sleeper" — Reactivate dormant Jötun

---

## 3. AI Stance System

| Stance | Behavior |
|--------|----------|
| Aggressive | Prioritize damage, attack player's target |
| Defensive | Stay near player, protect low-HP allies |
| Passive | No action unless commanded |

**Commands:**
```
command [companion] [ability] [target]
stance [companion] aggressive|defensive|passive
```

---

## 4. Progression

### 4.1 Leveling

- Companions gain Legend (XP) alongside player
- Stat increases per level (MIGHT/FINESSE/STURDINESS/WITS/WILL)
- New abilities at levels 3, 5, 7

### 4.2 Equipment

- Weapons, armor, accessories
- Same quality tier restrictions as player

---

## 5. System Crash & Recovery

When companion reaches 0 HP:

1. **Removed from combat** (not permanent death)
2. **+10 Psychic Stress** to player
3. **Recovery required** — cannot rejoin this encounter

**Recovery:**
- After Combat: 50% HP
- At Sanctuary: Full HP
- Bone-Setter abilities: Mid-dungeon revive

---

## 6. Balance Data

### 6.1 Companion vs Player Strength
| Stat | Ratio | Limit |
|------|-------|-------|
| HP | 120% | Tanks are beefier than players (Meat Shield role). |
| Damage | 60% | Less burst than Players. Consistent DPS but not MVPs. |
| Util | 100% | Debuffs/Heals same as player effectiveness. |

### 6.2 Party Economy
- **Stamina:** Companions manage their own pool OR use a simplified Cooldown system? (Spec says "Passive/Aggressive Stance" implies AI. Usually AI doesn't manage strict Stamina unless tactical.) *Assumption: Companions have Stamina but regenerate faster or have lower costs.*
- **Equipment:** Need to craft/find 4x sets of gear. Massive economy sink.

---

## 7. Phased Implementation Guide

### Phase 1: Core Entity
- [ ] **Class**: Create `CompanionEntity` (Inherits from `Character`).
- [ ] **State**: Implement `IsRecruited`, `CurrentStance`, `PersonalQuestStep`.

### Phase 2: AI & Combat
- [ ] **AI**: Implement `CompanionAIController` (State Machine: Idle/Follow/Attack).
- [ ] **Commands**: Implement `CommandCompanion()` interface.
- [ ] **Revive**: Implement "Downed" state and recovery logic.

### Phase 3: Progression
- [ ] **Leveling**: Hook into `Victory` event to award XP to Party Members.
- [ ] **UI**: "Party Screen" for equipping/leveling companions.

---

## 8. Testing Requirements

### 8.1 Unit Tests
- [ ] **Recruit**: `Recruit("Kára")` -> Adds to Party List.
- [ ] **Dismiss**: Removes from active party (stays in Reserve).
- [ ] **Command**: `Command("Attack", Target)` -> Overrides current AI state.
- [ ] **Death**: HP 0 -> Removed from combat, `RecoveryRequired` set.

### 8.2 Integration Tests
- [ ] **Combat**: 3 Companions + 1 Player vs 4 Enemies.
- [ ] **Pathfinding**: Companions follow player through Room transitions.
- [ ] **Loot**: Equipping item on Companion updates their Stats.

### 8.3 Manual QA
- [ ] **Banther**: Verify companions trigger "Chatter" lines while exploring.

---

## 9. Logging Requirements

**Reference:** [logging.md](../../../00-project/logging.md)

### 9.1 Log Events
| Event | Level | Message Template | Properties |
|-------|-------|------------------|------------|
| Recruit | Info | "{Name} joined the party." | `Name` |
| Command | Info | "You ordered {Name} to use {Ability} on {Target}." | `Name`, `Ability`, `Target` |
| Downed | Warn | "{Name} has fallen!" | `Name` |
| Recovery | Info | "{Name} has recovered and returned to duty." | `Name` |

---

## 10. Related Specifications
| Document | Purpose |
|----------|---------|
| [Faction Reputation](faction-reputation.md) | Recruitment requirements |
| [Trauma Economy](../01-core/trauma-economy.md) | Pyschic Stress |

---

## 11. Changelog
| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-07 | Initial specification |
| 1.1 | 2025-12-14 | Standardized with Balance, Phased Guide, Testing, Logging |
