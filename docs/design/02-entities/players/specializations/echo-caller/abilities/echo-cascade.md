---
id: ABILITY-ECHO-CALLER-28013
title: "Echo Cascade"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Echo Cascade

**Type:** Passive | **Tier:** 2 | **PP Cost:** 4

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Free Action (always active) |
| **Target** | Self (affects [Echo] abilities) |
| **Resource Cost** | None |
| **Ranks** | 2 → 3 |

---

## Description

Your echoes grow stronger, reaching further, cascading through enemy formations with terrifying efficiency.

---

## Rank Progression

### Rank 2 (Starting Rank - When ability is learned)

**Effect:**
- All [Echo Chain] effects have range 2 tiles (up from 1)
- Chain damage increased from 50% to 70%

**Formula:**
```
For all EchoAbilities:
    EchoChainRange = 2
    EchoChainDamage = 0.70
```

**Tooltip:** "Echo Cascade (Rank 2): Echo Chain range 2 tiles, damage 70%."

---

### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)

**Effect:**
- Echo Chain range: 3 tiles
- Chain damage: 80%
- **NEW:** Chains can hit 2 targets instead of 1

**Formula:**
```
For all EchoAbilities:
    EchoChainRange = 3
    EchoChainDamage = 0.80
    EchoChainTargets = 2
```

**Tooltip:** "Echo Cascade (Rank 3): Range 3 tiles, 80% damage, chains to 2 targets."

---

## Echo Chain Comparison

| Property | Base | Rank 2 | Rank 3 |
|----------|------|--------|--------|
| Range | 1 tile | 2 tiles | 3 tiles |
| Damage % | 50% | 70% | 80% |
| Targets | 1 | 1 | 2 |

---

## Combat Log Examples

- "Echo Cascade: Chain range increased to 2 tiles"
- "Echo Cascade: Chain damage at 70% efficiency"
- "Echo Cascade (Rank 3): Chaining to 2 targets at 80% damage!"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Echo-Caller Overview](../echo-caller-overview.md) | Parent specialization |
| [Scream of Silence](scream-of-silence.md) | Affected ability |
