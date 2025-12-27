---
id: ABILITY-ECHO-CALLER-28010
title: "Echo Attunement"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Echo Attunement

**Type:** Passive | **Tier:** 1 | **PP Cost:** 3

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Free Action (always active) |
| **Target** | Self |
| **Resource Cost** | None |
| **Ranks** | 1 → 2 → 3 |

---

## Description

Your mind has become attuned to the psychic frequencies of the Great Silence. The screaming static is your element.

---

## Rank Progression

### Rank 1 (Starting Rank - When ability is learned)

**Effect:**
- All [Echo]-tagged abilities cost -5 Aether
- +1d10 to WILL checks to resist psychic attacks

**Formula:**
```
For all EchoAbilities:
    AetherCost -= 5
WILLResolve_Psychic += 1d10
```

**Tooltip:** "Echo Attunement (Rank 1): -5 Aether to [Echo] abilities. +1d10 vs psychic attacks."

---

### Rank 2 (Upgrade Cost: +2 PP)

**Effect:**
- -10 Aether cost to [Echo] abilities
- +2d10 to resist psychic attacks

**Formula:**
```
For all EchoAbilities:
    AetherCost -= 10
WILLResolve_Psychic += 2d10
```

**Tooltip:** "Echo Attunement (Rank 2): -10 Aether to [Echo] abilities. +2d10 vs psychic attacks."

---

### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)

**Effect:**
- -15 Aether cost to [Echo] abilities
- +2d10 to resist psychic attacks
- **NEW:** Echo Chain range increased by 1 tile (affects all [Echo] abilities)

**Formula:**
```
For all EchoAbilities:
    AetherCost -= 15
    EchoChainRange += 1
WILLResolve_Psychic += 2d10
```

**Tooltip:** "Echo Attunement (Rank 3): -15 Aether. +2d10 psychic resist. +1 Echo Chain range."

---

## Combat Log Examples

- "Echo Attunement: Scream of Silence costs 25 Aether (reduced from 35)"
- "Echo Attunement: +2d10 to resist [Psychic Attack]"
- "Echo Attunement (Rank 3): Echo Chain range increased to 3 tiles"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Echo-Caller Overview](../echo-caller-overview.md) | Parent specialization |
