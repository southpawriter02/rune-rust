---
id: ABILITY-RUST-WITCH-25001
title: "Philosopher of Dust"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Philosopher of Dust

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

You understand entropy intimately. The decay that others fear, you have studied and embraced.

---

## Rank Progression

### Rank 1 (Starting Rank - When ability is learned)

**Effect:**
- +1d10 bonus to analysis/identification checks vs targets with Corruption > 0 or [Corroded] status

**Formula:**
```
If Target.Corruption > 0 OR Target.HasStatus("Corroded"):
    AnalysisCheckPool += 1d10
```

**Tooltip:** "Philosopher of Dust (Rank 1): +1d10 to analysis checks vs corrupted targets."

---

### Rank 2 (Upgrade Cost: +2 PP)

**Effect:**
- +2d10 bonus to analysis checks
- Analysis reveals target's Corruption percentage

**Formula:**
```
If Target.Corruption > 0 OR Target.HasStatus("Corroded"):
    AnalysisCheckPool += 2d10
    Reveal(Target.CorruptionPercentage)
```

**Tooltip:** "Philosopher of Dust (Rank 2): +2d10 analysis vs corrupted. Reveals Corruption %."

---

### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)

**Effect:**
- +3d10 bonus to analysis checks
- Analysis reveals target's vulnerabilities to entropy effects
- Identifies if target is near execution threshold

**Formula:**
```
If Target.Corruption > 0 OR Target.HasStatus("Corroded"):
    AnalysisCheckPool += 3d10
    Reveal(Target.CorruptionPercentage)
    Reveal(Target.EntropyVulnerabilities)
    Reveal(Target.NearExecutionThreshold)
```

**Tooltip:** "Philosopher of Dust (Rank 3): +3d10 analysis. Reveals Corruption %, vulnerabilities, and execution threshold status."

---

## Combat Log Examples

- "Philosopher of Dust: +1d10 to analyze corrupted target"
- "Philosopher of Dust: Target Corruption revealed - 67%"
- "Philosopher of Dust: Target near execution threshold!"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Rust-Witch Overview](../rust-witch-overview.md) | Parent specialization |
| [Corruption System](../../../../01-core/resources/coherence.md) | Corruption mechanics |
