---
id: ABILITY-JOTUNREADER-205
title: "Navigational Bypass"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Navigational Bypass

**Type:** Active | **Tier:** 2 | **PP Cost:** 4

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Party (exploration buff) |
| **Resource Cost** | 30 Stamina (Rank 2), 20 Stamina (Rank 3) |
| **Ranks** | 2 → 3 |

---

## Description

"The trigger mechanism is corroded on the western edge. Distribute weight evenly—sensor won't register threshold pressure." Your analysis guides the party through hazards.

---

## Rank Progression

### Rank 2 (Starting Rank - When ability is learned)

**Effect:**
- Grant entire party +1d10 to next bypass check (trap avoidance/disarm)
- Single use per activation
- Non-combat utility ability

**Formula:**
```
StaminaCost = 30

Party.NextBypassCheck.BonusDice += 1d10
UsesPerActivation = 1
```

**Tooltip:** "Navigational Bypass (Rank 2): Party gains +1d10 to next trap bypass check. Cost: 30 Stamina"

---

### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)

**Effect:**
- Grant entire party +3d10 to next 2 bypass checks
- **Critical Success:** Permanently disables the hazard
- **NEW:** Can be used in combat (1/combat)
- **Reduced Stamina Cost:** 20

**Formula:**
```
StaminaCost = 20  // Reduced from 30

Party.NextBypassCheck.BonusDice += 3d10  // Increased from +1d10
UsesPerActivation = 2  // Covers 2 checks

If CriticalSuccess:
    Hazard.Disabled = true  // Permanent

CombatUsesPerCombat = 1  // NEW: Can use in combat
```

**Tooltip:** "Navigational Bypass (Rank 3): Party gains +3d10 to next 2 bypass checks. Critical = permanent disable. Usable in combat 1/fight. Cost: 20 Stamina"

---

## Exploration/Combat Examples

- "Navigational Bypass: Party gains +1d10 to bypass the pressure plate"
- "Following the Jötun-Reader's guidance, the party avoids the trap"
- "CRITICAL! The trap mechanism is permanently disabled!"
- "[IN COMBAT] Navigational Bypass guides allies past the environmental hazard"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Jötun-Reader Overview](../jotun-reader-overview.md) | Parent specialization |
| [Room Engine: Core](../../../../07-environment/room-engine/core.md) | Trap system integration |
