# System Bypass — Skill Specification v5.0

Type: Mechanic
Status: Review
Balance Validated: No
Document ID: AAM-SPEC-SKILL-SYSTEMBYPASS-v5.0
Parent item: Skills System — Core System Specification v5.0 (Skills%20System%20%E2%80%94%20Core%20System%20Specification%20v5%200%20f074e9ec58e64ae6865c52dca47e733d.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

## Core Philosophy

System Bypass resolves: **Can this character manipulate, jury-rig, or exploit corrupted Old World technology?** This is not elegant programming—it is the desperate craft of tinkering with incomprehensible machinery through physical manipulation, pattern recognition, and exploiting obvious glitches.

**Governing Attribute:** WITS

---

## Trigger Events

- **Lockpicking:** Opening mechanical or electronic locks
- **Terminal Interaction:** Coaxing Old World terminals into function
- **Trap Disarmament:** Disabling physical or electronic traps
- **Security Override:** Bypassing alarms, gates, access controls
- **Glitch Exploitation:** Exploiting obvious system malfunctions

---

## DC Tables

### Lockpicking

| Type | DC | Example |
| --- | --- | --- |
| Improvised Latch | 6 | Scrap metal bar |
| Simple Lock | 10 | Residential door |
| Standard Lock | 14 | Security gate |
| Complex Lock | 18 | High-security bunker |
| Master Lock | 22 | Jötun-Forged vault |
| [Glitched] | +2 DC | Unpredictable mechanism |
| [Blighted] | +4 DC | Anti-logical behavior |

### Terminal Interaction

| Type | DC | Example |
| --- | --- | --- |
| Unprotected | 8 | Emergency panel |
| Basic | 12 | Storage manifest |
| Secured | 16 | Research lab |
| Encrypted | 20 | Military console |
| Jötun-Reader | 24 | Ancient archive |

---

## Dice Pool Calculation

```
Pool = WITS + Rank + Tool Mod + Situational
```

**Tool Modifiers:**

- Proper Lockpicks: +1d10
- Improvised: +0
- Bare Hands: -2d10

---

## Trauma Economy Costs

| Surface Type | Stress |
| --- | --- |
| [Normal] | 0 |
| [Glitched] | 3-8 |
| [Blighted] | 5-10 |
| [Psychic Resonance] | 8-15 |

---

## Master Rank Benefits (Rank 5)

- **Auto-Bypass:** Auto-succeed DC ≤ 10 checks
- **Salvage Expertise:** Always salvage trap components
- **Silent Bypass:** All successful checks are silent
- **Glitch Reader:** Reduce [Glitched] penalty by -2 DC

---

## Integration Points

**Dependencies:** Attributes (WITS), Dice Pool System, Trauma Economy, Equipment System

**Referenced By:** Exploration System, Scrap-Tinker Specialization, Gantry-Runner Specialization