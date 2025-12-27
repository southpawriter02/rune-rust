---
id: ABILITY-JOTUNREADER-201
title: "Scholarly Acumen I"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Scholarly Acumen I

**Type:** Passive | **Tier:** 1 | **PP Cost:** 3

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Free Action (always active) |
| **Target** | Self |
| **Resource Cost** | None |
| **Attribute** | WITS |
| **Ranks** | 1 → 2 → 3 |

---

## Description

Your mind is a finely honed instrument, constantly processing layers of forgotten history. Years of study have made pattern recognition instinctive.

---

## Rank Progression

### Rank 1 (Base — included with ability unlock)

**Effect:**
- +2 bonus dice (d10) to all WITS-based Investigate checks
- +2 bonus dice (d10) to all System Bypass checks

**Formula:**
```
InvestigateCheckPool = WITS + 2d10
SystemBypassCheckPool = WITS + 2d10
```

**Tooltip:** "Scholarly Acumen I (Rank 1): +2d10 to Investigate and System Bypass checks"

---

### Rank 2 (Upgrade Cost: +2 PP)

**Effect:**
- +4 bonus dice (d10) to all WITS-based Investigate checks
- +4 bonus dice (d10) to all System Bypass checks
- Translation time reduced by 25%

**Formula:**
```
InvestigateCheckPool = WITS + 4d10
SystemBypassCheckPool = WITS + 4d10
TranslationTime = BaseTime * 0.75
```

**Tooltip:** "Scholarly Acumen I (Rank 2): +4d10 to Investigate and System Bypass checks. Translation -25% time."

---

### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)

**Effect:**
- +4 bonus dice (d10) to all WITS-based Investigate checks
- +4 bonus dice (d10) to all System Bypass checks
- Translation time reduced by 25%
- **NEW:** Auto-upgrade Success → Critical Success on Investigation checks

**Formula:**
```
InvestigateCheckPool = WITS + 4d10
If InvestigateCheck == Success:
    InvestigateCheck = CriticalSuccess  // Auto-upgrade
SystemBypassCheckPool = WITS + 4d10
TranslationTime = BaseTime * 0.75
```

**Tooltip:** "Scholarly Acumen I (Rank 3): +4d10 to Investigate and System Bypass checks. Translation -25% time. Investigation Success → Critical Success."

---

## Combat Log Examples

- "Scholarly Acumen grants +2d10 to Investigation"
- "Scholarly Acumen (Rank 2) grants +4d10 to Investigation"
- "Investigation Success auto-upgraded to Critical Success! (Scholarly Acumen)"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Jötun-Reader Overview](../jotun-reader-overview.md) | Parent specialization |
| [Skills: System Bypass](../../../../01-core/skills/system-bypass.md) | Skill integration |
