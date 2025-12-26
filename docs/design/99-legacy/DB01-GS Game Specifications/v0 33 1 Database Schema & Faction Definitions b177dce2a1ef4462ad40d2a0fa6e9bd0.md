# v0.33.1: Database Schema & Faction Definitions

Type: Technical
Description: Database foundation for Faction System: 4 new tables (Factions, Characters_FactionReputations, Faction_Quests, Faction_Rewards), 5 major faction definitions, SQL migration scripts. 8-12 hours.
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.14.1 (Quest database), v0.8 (NPC database)
Implementation Difficulty: Medium
Balance Validated: No
Parent item: v0.33: Faction System & Reputation (v0%2033%20Faction%20System%20&%20Reputation%20161115b505034a2fa3ad8288e2513b36.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Document ID:** RR-SPEC-v0.33.1-DATABASE

**Parent Specification:** v0.33 Faction System & Reputation

**Status:** Design Complete — Ready for Implementation

**Timeline:** 8-12 hours

**Prerequisites:** v0.14.1 (Quest database), v0.8 (NPC database)

---

## I. Overview

Complete database foundation for the Faction System: faction definitions, reputation tracking, faction quests, and faction rewards.

### Core Deliverables

- **4 New Tables:** Factions, Characters_FactionReputations, Faction_Quests, Faction_Rewards
- **5 Major Faction Definitions**
- **Complete SQL Migration Scripts**
- **Reputation Threshold Definitions**

---

## II. Database Schema

See parent specification v0.33 for complete faction definitions and philosophies. This spec provides technical schema only.

### A. Factions Table

```sql
CREATE TABLE IF NOT EXISTS Factions (
    faction_id INTEGER PRIMARY KEY,
    faction_name TEXT NOT NULL UNIQUE,
    display_name TEXT NOT NULL,
    philosophy TEXT,
    description TEXT,
    primary_location TEXT,
    allied_factions TEXT,
    enemy_factions TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
```

**5 Factions:** Iron-Banes, God-Sleeper Cultists, Jötun-Readers, Rust-Clans, Independents

---

### B. Characters_FactionReputations Table

```sql
CREATE TABLE IF NOT EXISTS Characters_FactionReputations (
    reputation_id INTEGER PRIMARY KEY AUTOINCREMENT,
    character_id INTEGER NOT NULL,
    faction_id INTEGER NOT NULL,
    reputation_value INTEGER DEFAULT 0 CHECK(reputation_value BETWEEN -100 AND 100),
    reputation_tier TEXT,
    last_modified TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (character_id) REFERENCES Characters(character_id) ON DELETE CASCADE,
    FOREIGN KEY (faction_id) REFERENCES Factions(faction_id),
    UNIQUE(character_id, faction_id)
);
```

**Reputation Tiers:**

- Hated: -100 to -76
- Hostile: -75 to -26
- Neutral: -25 to +24
- Friendly: +25 to +49
- Allied: +50 to +74
- Exalted: +75 to +100

---

### C. Faction_Quests Table

```sql
CREATE TABLE IF NOT EXISTS Faction_Quests (
    faction_quest_id INTEGER PRIMARY KEY AUTOINCREMENT,
    quest_id INTEGER NOT NULL,
    faction_id INTEGER NOT NULL,
    required_reputation INTEGER DEFAULT 0,
    reputation_reward INTEGER DEFAULT 0,
    is_repeatable BOOLEAN DEFAULT 0,
    FOREIGN KEY (quest_id) REFERENCES Quests(quest_id),
    FOREIGN KEY (faction_id) REFERENCES Factions(faction_id)
);
```

---

### D. Faction_Rewards Table

```sql
CREATE TABLE IF NOT EXISTS Faction_Rewards (
    reward_id INTEGER PRIMARY KEY AUTOINCREMENT,
    faction_id INTEGER NOT NULL,
    reward_type TEXT CHECK(reward_type IN ('Equipment', 'Consumable', 'Service', 'Ability', 'Discount')),
    reward_name TEXT NOT NULL,
    reward_description TEXT,
    required_reputation INTEGER DEFAULT 0,
    FOREIGN KEY (faction_id) REFERENCES Factions(faction_id)
);
```

---

## III. Success Criteria

- [ ]  All 4 tables created
- [ ]  5 factions seeded
- [ ]  Reputation triggers functional
- [ ]  Foreign keys validated
- [ ]  Migration script executes without errors

---

**Database foundation complete. Proceed to reputation mechanics (v0.33.2).**