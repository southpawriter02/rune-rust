---
id: ABILITY-SKJALDMAER-26025
title: "Implacable Defense"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Implacable Defense

**Type:** Active | **Tier:** 3 | **PP Cost:** 5

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Self |
| **Resource Cost** | 40 Stamina |
| **Ranks** | None (full power when unlocked) |

---

## Description

Achieve state of perfect focus—immovable against physical and mental assault. For a brief time, the Skjaldmær becomes an unshakeable pillar.

---

## Mechanical Effect

- For 3 turns, Skjaldmær is IMMUNE to:
  - [Stun]
  - [Staggered]
  - [Knocked Down]
  - [Fear]
  - [Disoriented]
- Additionally: +2 Soak for 3 turns
- Aura effect: Adjacent allies are immune to [Fear] for 3 turns

**Formula:**
```
Skjaldmaer.AddImmunity(["Stun", "Staggered", "Knocked Down", "Fear", "Disoriented"], Duration: 3)
Skjaldmaer.Soak += 2 (for 3 turns)

For each AdjacentAlly:
    AdjacentAlly.AddImmunity(["Fear"], Duration: 3)
```

**Tooltip:** "Implacable Defense: Become immune to Stun, Stagger, Knockdown, Fear, Disoriented for 3 turns. +2 Soak. Adjacent allies immune to Fear. Cost: 40 Stamina"

---

## Combat Log Examples

- "Implacable Defense activated! [Skjaldmær] is immune to control effects for 3 turns"
- "[Enemy] attempts to Stun [Skjaldmær] - IMMUNE!"
- "[Ally] is protected from Fear by Implacable Defense aura"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Skjaldmær Overview](../skjaldmaer-overview.md) | Parent specialization |
| [Feared](../../../../04-systems/status-effects/feared.md) | Immunity provided |
| [Stunned](../../../../04-systems/status-effects/stunned.md) | Immunity provided |
