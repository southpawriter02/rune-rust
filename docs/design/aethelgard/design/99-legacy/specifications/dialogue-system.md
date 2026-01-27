# Dialogue System — Mechanic Specification v5.0

Type: Mechanic
Description: Branching dialogue trees with skill-gated options, faction reputation outcomes, and quest integration.
Priority: Should-Have
Status: Review
Balance Validated: No
Document ID: AAM-SPEC-MECH-DIALOGUE-v5.0
Proof-of-Concept Flag: No
Template Compliance: v5.0 Three-Tier Template
Template Validated: No
Voice Validated: No

## I. Core Philosophy

The Dialogue System provides branching, skill-gated conversations enabling narrative delivery, quest acquisition, faction reputation changes, and meaningful player choices through text-based interactions.

> Consolidated from SPEC-NARRATIVE-002 (Imported Game Docs / codebase reflection).
> 

**Design Pillars:**

- **Player Agency:** Every conversation presents meaningful choices reflecting character builds
- **Visible Consequences:** Reputation changes, quest updates display immediately
- **Character Builds Matter:** WITS, WILL, and specializations unlock unique dialogue paths

---

## II. Core Mechanics

### Hard Skill Check System

Dialogue options use **hard checks** — if player meets requirement, option is visible; otherwise completely hidden.

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

### Skill Check Display

- Attribute checks: `[WITS 4]`, `[WILL 5]`
- Specialization checks: `[Bone-Setter]`, `[Skald]`

---

## III. Dialogue Outcome Types

| Type | Purpose | Data Field | Side Effects |
| --- | --- | --- | --- |
| **Information** | Reveal lore/hints | Unused | None |
| **QuestGiven** | Add quest to log | Quest ID | Quest system notified |
| **QuestComplete** | Mark quest done | Quest ID | Quest system notified |
| **ReputationChange** | Modify standing | Reason text | Faction rep modified, NPC disposition updated |
| **ItemReceived** | Grant item | Item ID | Inventory modified |
| **ItemLost** | Remove item | Item ID | Inventory modified |
| **InitiateCombat** | Start fight | Unused | NPC.IsHostile = true |
| **EndConversation** | Clean exit | Unused | State cleared |

---

## IV. NPC Disposition System

### Disposition Tiers

| Tier | Range | Behavior |
| --- | --- | --- |
| **Friendly** | ≥50 | Extra helpful options, discounts |
| **Neutral-Positive** | 10-49 | Standard positive interactions |
| **Neutral** | -9 to +9 | Default behavior |
| **Unfriendly** | -49 to -10 | Reluctant, higher prices |
| **Hostile** | ≤-50 | May refuse to talk, attack on sight |

### Disposition Formula

```
CurrentDisposition = BaseDisposition + FactionReputation
```

---

## V. Database Schema

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

## VI. Service Architecture

### DialogueService

```csharp
public interface IDialogueService
{
    // Conversation Management
    DialogueNode StartConversation(NPC npc, PlayerCharacter player);
    (DialogueNode next, DialogueOutcome outcome) SelectOption(DialogueOption option, PlayerCharacter player);
    void EndConversation();
    
    // Query Methods
    bool IsInConversation();
    List<DialogueOption> GetAvailableOptions(DialogueNode node, PlayerCharacter player);
    bool MeetsRequirement(PlayerCharacter player, SkillCheckRequirement req);
    
    // Outcome Processing
    List<string> ProcessOutcome(DialogueOutcome outcome, PlayerCharacter player, NPC npc);
}
```

---

## VII. Data Format

### JSON Dialogue File Structure

```json
{
  "Id": "sigrun_greeting",
  "Text": "Another scavenger in these ruins? You look capable enough.",
  "Options": [
    {
      "Text": "Who are you?",
      "NextNodeId": "sigrun_about"
    },
    {
      "Text": "[WITS 4] These ruins are Jötun-Forged...",
      "SkillCheck": { "Attribute": "wits", "TargetValue": 4 },
      "NextNodeId": "sigrun_lore_branch",
      "Outcome": {
        "Type": "ReputationChange",
        "AffectedFaction": "MidgardCombine",
        "ReputationChange": 5
      }
    }
  ]
}
```

---

## VIII. Integration Points

**Dependencies:**

- PlayerCharacter → attribute values for skill checks
- Faction Reputation System → reputation modification
- Quest System → quest triggering via outcomes
- NPC System → disposition tracking

**Referenced By:**

- Quest Journal → quest acquisition
- Merchant System → trade context
- Companion System → recruitment conversations

---

*Consolidated from SPEC-NARRATIVE-002 (Dialogue System Specification) per Source Authority guidelines.*