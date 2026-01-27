---
id: SPEC-DIALOGUE
title: "Dialogue System"
version: 1.0
status: implemented
last-updated: 2025-12-07
related-files:
  - path: "data/dialogues/"
    status: Active
---

# Dialogue System

---

## 1. Overview

The Dialogue System provides **branching, skill-gated conversations** between the player and NPCs, enabling narrative delivery, quest acquisition, faction reputation changes, and meaningful player choices.

**Design Pillars:**
- **Player Agency** — Every conversation presents choices reflecting character builds
- **Visible Consequences** — Reputation changes, quest updates display immediately
- **Character Builds Matter** — WITS, WILL, and specializations unlock unique paths

---

## 2. Skill Check Mechanics

### 2.1 Hard Check System

The dialogue system uses **hard checks** — if the player meets the requirement, the option is visible; otherwise it's completely hidden.

```
PassesCheck = true

If SkillCheck.Attribute is specified:
    PlayerValue = Player.GetAttributeValue(SkillCheck.Attribute)
    If PlayerValue < SkillCheck.TargetValue:
        PassesCheck = false

If SkillCheck.Specialization is specified:
    If Player.Specialization != SkillCheck.Specialization:
        PassesCheck = false

Return PassesCheck
```

### 2.2 Display Format

| Type | Display | Example |
|------|---------|---------|
| Attribute | `[ATTR VALUE]` | `[WITS 4]` |
| Specialization | `[Name]` | `[Bone-Setter]` |
| Combined | Both shown | `[WILL 3] [Skald]` |

---

## 3. Outcome Types

When a player selects a dialogue option, an outcome may trigger:

| Type | Effect | Example |
|------|--------|---------|
| **Information** | No mechanical effect (lore only) | Learn about faction history |
| **QuestGiven** | Add quest to journal | Accept scrap collection |
| **QuestComplete** | Mark quest done | Turn in collected items |
| **ReputationChange** | Modify faction standing | +5 Midgard Combine |
| **ItemReceived** | Grant item to player | Receive Aetheric Stabilizer |
| **ItemLost** | Remove item from player | Surrender weapon |
| **InitiateCombat** | NPC becomes hostile | Provoke attack |
| **EndConversation** | Clean exit | Say goodbye |

---

## 4. NPC Disposition

### 4.1 Disposition Tiers

| Tier | Range | Behavior |
|------|-------|----------|
| Friendly | ≥50 | Extra options, discounts |
| Neutral-Positive | 10-49 | Standard positive |
| Neutral | -9 to +9 | Default behavior |
| Unfriendly | -49 to -10 | Reluctant, higher prices |
| Hostile | ≤-50 | May refuse/attack |

### 4.2 Calculation

```
CurrentDisposition = BaseDisposition + FactionReputation
```

NPCs update disposition when faction reputation changes.

---

## 5. Data Format

### 5.1 Dialogue Node Structure

```json
{
  "Id": "sigrun_greeting",
  "Text": "Another scavenger in these ruins? You look capable enough.",
  "Options": [
    {
      "Text": "Who are you?",
      "NextNodeId": "sigrun_about",
      "Outcome": null
    },
    {
      "Text": "[WITS 4] These ruins are Jötun-Forged...",
      "SkillCheck": {
        "Attribute": "wits",
        "TargetValue": 4
      },
      "NextNodeId": "sigrun_lore_branch",
      "Outcome": {
        "Type": "ReputationChange",
        "AffectedFaction": "MidgardCombine",
        "ReputationChange": 5
      }
    }
  ],
  "EndsConversation": false
}
```

### 5.2 Data Files

Location: `data/dialogues/`

| File | NPC | Nodes |
|------|-----|-------|
| `sigrun_dialogues.json` | Sigrun (Midgard Combine) | 8 |
| `astrid_dialogues.json` | Astrid | 6 |
| `bjorn_dialogues.json` | Bjorn | 5 |
| `eydis_dialogues.json` | Eydis | 5 |
| `gunnar_dialogues.json` | Gunnar | 5 |
| `kjartan_dialogues.json` | Kjartan | 6 |
| `rolf_dialogues.json` | Rolf | 7 |
| `thorvald_dialogues.json` | Thorvald | 5 |

---

## 6. Database Schema

```sql
CREATE TABLE DialogueNodes (
    node_id TEXT PRIMARY KEY,
    npc_id TEXT NOT NULL,
    text TEXT NOT NULL,
    options_json TEXT NOT NULL,
    ends_conversation BOOLEAN DEFAULT FALSE
);

CREATE TABLE DialogueOptions (
    option_id INTEGER PRIMARY KEY,
    node_id TEXT NOT NULL,
    text TEXT NOT NULL,
    next_node_id TEXT,
    skill_check_json TEXT,
    outcome_json TEXT,
    FOREIGN KEY (node_id) REFERENCES DialogueNodes(node_id)
);

CREATE TABLE NPCDialogueState (
    npc_id TEXT PRIMARY KEY,
    has_been_met BOOLEAN DEFAULT FALSE,
    encountered_topics_json TEXT DEFAULT '[]',
    current_disposition INTEGER DEFAULT 0
);
```

---

## 7. Balance Data

### 7.1 DC Analysis
| Tier | Attribute | Specialization | Consequence |
|------|-----------|----------------|-------------|
| Easy | 2-3 | None | Flavor text, minor info |
| Moderate | 4-5 | Tier 1/2 Spec | Minor reward, Quest shortcut |
| Hard | 6-7 | Tier 3 Spec | Major reward, Avoid Combat |
| Heroic | 8+ | Capstone Spec | Unique Artifact, Story Branch |

### 7.2 Economy Impact
- **Information:** Low economic value, high narrative value.
- **Quest Skip:** High time value, potential partial loot loss.
- **Diplomacy:** Avoids combat (Saves resources) vs Loot (Gains resources).

---

## 8. Phased Implementation Guide

### Phase 1: Core Systems
- [ ] **Data**: Implement `DialogueNode` and `DialogueOption` structs/classes.
- [ ] **Parser**: Create JSON Loader for `data/dialogues/`.
- [ ] **Service**: Build `DialogueService` with `StartConversation/SelectOption`.

### Phase 2: Logic Integration
- [ ] **Checks**: Implement `MeetsRequirement()` logic for Attributes and Specs.
- [ ] **Outcomes**: Hook `ProcessOutcome()` to Quest/Inventory/Faction systems.
- [ ] **State**: Persist `encountered_topics` to prevent repeating "First Time" lines.

### Phase 3: UI & Polish
- [ ] **UI**: Dialogue Box with typewriter text.
- [ ] **Options**: Rendering of unavailable options (Greyed out or Hidden? Spec says Hidden for Hard Checks).
- [ ] **Voice**: (Optional) Grunts/Typewriter audio.

---

## 9. Testing Requirements

### 9.1 Unit Tests
- [ ] **Traversal**: Start -> Option A -> Node B.
- [ ] **Check (Fail)**: WITS 3 needed, Player has 2 -> Option Hidden/Disabled.
- [ ] **Check (Pass)**: WITS 3 needed, Player has 4 -> Option Visible.
- [ ] **Outcome**: Option selected -> Reputation +5 applied.
- [ ] **Loop**: Node A -> Option -> Node A again (Investigation loop).

### 9.2 Integration Tests
- [ ] **Quest**: Acceptance choice -> Quest appears in Journal.
- [ ] **Combat**: Aggressive choice -> Dialogue Ends -> Combat Interaction starts.
- [ ] **Faction**: Wears Iron-Bane Sigil -> Special greeting node selected automatically (Start Node Override).

### 9.3 Manual QA
- [ ] **Layout**: Verify long text wraps correctly.
- [ ] **Input**: Mouse/Keyboard/Gamepad referencing correct ID.

---

## 10. Logging Requirements

**Reference:** [logging.md](../../../00-project/logging.md)

### 10.1 Log Events
| Event | Level | Message Template | Properties |
|-------|-------|------------------|------------|
| Start | Info | "Conversation started with {NPC}." | `NPC` |
| Choice | Info | "Player selected: '{OptionText}' (Pass: {IsPass})." | `OptionText`, `IsPass` |
| Outcome | debug | "Dialogue trigger: {Type} -> {Value}." | `Type`, `Value` |
| End | Info | "Conversation ended." | - |

---

## 11. Related Specifications
| Document | Purpose |
|----------|---------|
| [NPC Companions](npc-companions.md) | Companion recruitment dialogues |
| [Faction Reputation](faction-reputation.md) | Reputation outcomes |
| [Saga System](../01-core/saga-system.md) | Quest integration |

---

## 12. Changelog
| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-07 | Initial specification |
| 1.1 | 2025-12-14 | Standardized with Balance, Phased Guide, Testing, Logging |
