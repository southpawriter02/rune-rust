---
id: ABILITY-SKJALDMAER-26027
title: "Bastion of Sanity"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Bastion of Sanity

**Type:** Passive + Reaction | **Tier:** 4 (Capstone) | **PP Cost:** 6

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Passive / Reaction |
| **Target** | Aura (All Allies) / Single Ally in Crisis |
| **Resource Cost** | Reaction: 40 Psychic Stress + 1 Corruption |
| **Trigger** | Ally would gain permanent Trauma |
| **Limit** | Reaction: Once per combat |
| **Ranks** | None (full power when unlocked) |
| **Special** | Training upgrades all Tier 1 & 2 abilities to Rank 3 |

---

## Description

Become living Runic Anchor—a kernel of stable reality. The ultimate expression of the Skjaldmær's protective nature: she can absorb the psychic wounds that would permanently scar her allies, taking the madness into herself.

---

## Component 1: Passive Aura (Always Active While in Front Row)

**Effect:**
- While Skjaldmær is in Front Row position:
  - All allies in the same row gain +1 WILL
  - All allies in the same row reduce ambient Psychic Stress gain by 10%

**Formula:**
```
If Skjaldmaer.Position.Row == Front:
    For each Ally in FrontRow:
        Ally.WILL += 1 (temporary)
        Ally.AmbientStressModifier *= 0.90
```

---

## Component 2: Reaction (Once Per Combat)

**Trigger:** An ally would gain permanent Trauma from reaching Breaking Point

**Effect:**
- Skjaldmær absorbs the Trauma instead of the ally
- **Cost to Skjaldmær:**
  - Gain 40 Psychic Stress
  - Gain 1 Corruption
- **Result for Ally:** Avoids gaining the Trauma entirely

**Formula:**
```
Trigger: Ally.WouldGainTrauma(TraumaType)

If Player accepts:
    Ally.CancelTrauma()
    Skjaldmaer.PsychicStress += 40
    Skjaldmaer.Corruption += 1
    MarkBastionUsedThisCombat()
```

---

## Reaction Prompt

```
┌─────────────────────────────────────────────────────┐
│              BASTION OF SANITY                      │
├─────────────────────────────────────────────────────┤
│ [Ally Name] is about to gain permanent Trauma:      │
│                                                     │
│   ╔═══════════════════════════════════════════╗     │
│   ║  [TRAUMA NAME]                            ║     │
│   ║  [Trauma description/effect]              ║     │
│   ╚═══════════════════════════════════════════╝     │
│                                                     │
│ Absorb this Trauma to save [Ally]?                  │
│                                                     │
│ ┌─────────────────────────────────────────────┐     │
│ │ COST TO YOU:                                │     │
│ │   • +40 Psychic Stress                      │     │
│ │   • +1 Corruption (PERMANENT)               │     │
│ │                                             │     │
│ │ Your current Stress: [X]/100                │     │
│ │ Your current Corruption: [Y]                │     │
│ └─────────────────────────────────────────────┘     │
│                                                     │
│ ⚠ This can only be used ONCE per combat            │
│                                                     │
│    [ABSORB TRAUMA]        [LET IT PASS]             │
└─────────────────────────────────────────────────────┘
```

---

## Combat Log Examples

- "BASTION OF SANITY AURA: [Ally1], [Ally2] gain +1 WILL, -10% Stress gain"
- "[Ally] reaches Breaking Point! Would gain Trauma: [Shattered Confidence]"
- "BASTION OF SANITY TRIGGERED! [Skjaldmær] absorbs [Ally]'s Trauma!"
- "[Skjaldmær] gains 40 Psychic Stress and 1 Corruption"
- "[Ally] is saved from permanent Trauma!"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Skjaldmær Overview](../skjaldmaer-overview.md) | Parent specialization |
| [Trauma Economy](../../../../01-core/trauma-economy.md) | Breaking Point system |
| [Coherence](../../../../01-core/resources/coherence.md) | Corruption mechanics |
