# Artifact & Attunement System — Mechanic Specification v5.0

Type: Mechanic
Status: Review
Balance Validated: No
Document ID: AAM-SPEC-MECHANIC-ARTIFACT-v5.0
Parent item: Equipment System — Core System Specification v5.0 (Equipment%20System%20%E2%80%94%20Core%20System%20Specification%20v5%200%200ec604d185934907915e1ba9cd3e8800.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

## Core Philosophy

Artifacts are **Myth-Forged or specially designated items** requiring attunement. They represent Jötun-forged relics, Blighted items, Creed relics, or Old World tech—tremendously powerful but demanding commitment and often exacting costs.

**Fixed Limit:** 3 attunement slots (never increases)

---

## Attunement Mechanics

### Slot Usage

```
Max Attunement Slots = 3 (fixed, never changes)
```

### Attunement Requirements

- Must be at **Runic Anchor** (Sanctuary Rest location)
- Item in inventory or equipped
- Available slot (or willingness to unattune existing)

### Critical Rule

**Un-attuned Artifacts provide ZERO benefits.** Still count as equipped, still have weight—but no bonuses, abilities, or stat modifiers.

---

## Artifact Categories

| Category | Description |
| --- | --- |
| **Jötun-Forged Relics** | Pre-Glitch AI-crafted perfection |
| **Blighted Items** | Corrupted by Runic Blight |
| **Creed Relics** | Sacred faction items |
| **Old World Tech** | Functional Pre-Glitch devices |

---

## Artifact Drawbacks

### Corruption Accumulation

- **Per Combat:** +1-3 Corruption while wielded
- **Per Use:** +5-10 Corruption per ability activation

### Resource Costs

- HP sacrifice for abilities
- Increased Stamina costs

### Behavioral Restrictions

- Creed locks (faction-only)
- Sentient items may refuse commands

---

## Balance Considerations

**Power Level:** 3-5× more powerful than Scavenged equivalents

**Attunement Limit Rationale:**

- 3 slots = strategic choice (weapon + armor + accessory)
- Prevents "carry all artifacts" power stacking
- Forces meaningful build decisions

**Acquisition:** Rare (1-2 per major dungeon), quest rewards, boss drops

---

## Integration Points

**Dependencies:** Equipment System, Sanctuary Rest System, Loot System

**Referenced By:** Combat System, Trauma Economy, all Specializations