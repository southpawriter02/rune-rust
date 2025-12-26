# Rhetoric — Skill Specification v5.0

Type: Mechanic
Status: Review
Balance Validated: No
Document ID: AAM-SPEC-SKILL-RHETORIC-v5.0
Parent item: Skills System — Core System Specification v5.0 (Skills%20System%20%E2%80%94%20Core%20System%20Specification%20v5%200%20f074e9ec58e64ae6865c52dca47e733d.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

## Core Philosophy

Rhetoric resolves: **Can this character navigate the complex social landscape of Aethelgard's fractured cultures?** This is survival through words in a world where every faction has different rules of speech, and a wrong phrase can mean death.

**Governing Attribute:** WILL

---

## Trigger Events

- **Persuasion:** Convincing NPCs to take action
- **Deception:** Lying, misleading, concealing truth
- **Intimidation:** Coercing through threat or display
- **Negotiation:** Brokering deals or truces
- **Cultural Protocol:** Navigating formal interactions

---

## DC Tables

### Persuasion

| Request | DC | Example |
| --- | --- | --- |
| Trivial | 8 | "Tell me the time" |
| Simple | 12 | "Share your rations" |
| Moderate | 16 | "Help me fight" |
| Major | 20 | "Attack your faction's enemies" |
| Extreme | 24 | "Betray your clan leader" |

### Cultural Protocols

| Culture | DC | Protocol |
| --- | --- | --- |
| Dvergr Logic-Chain | 18 | Precise, non-contradictory sequences |
| Utgard Veil-Speech | 16 | Layer truth in deception |
| Gorge-Maw Patience | 14 | Listen to full 10+ min rumble |
| Rune-Lupin Telepathy | 12 | Open mind, suppress hostility |
| Iron-Bane Tribute | 16 | Offer martial tribute |

---

## Dice Pool Calculation

```
Pool = WILL + Rank + Cultural Mod + Situational + Cant Mod
```

**Cultural Modifiers:**

- Native culture: +2d10
- Familiar culture: +1d10
- Alien/hostile culture: -2d10

**Cant Modifier:** Speaking target's cant fluently: +1d10

---

## Utgard Veil-Speech

In Utgard culture, **honesty is insult** (implies target too stupid for complexity). Deception DC reduced by -4 when speaking to Utgard-folk. Direct truth telling offends.

---

## Master Rank Benefits (Rank 5)

- **Polyglot:** Know all common cants (DC ≤ 14)
- **Cultural Diplomat:** Auto-succeed protocol DC ≤ 14
- **Master Negotiator:** Re-roll failed checks once/conversation
- **Silver Tongue:** Auto-succeed persuasion DC ≤ 12
- **Fearsome Reputation:** +2d10 to intimidation

---

## Integration Points

**Dependencies:** Attributes (WILL), Dice Pool System, Faction System

**Referenced By:** Dialogue System, Quest System, Skald Specialization