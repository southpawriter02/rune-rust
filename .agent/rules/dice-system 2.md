---
trigger: always_on
---

## Dice System

**CRITICAL**: This project uses **d10 for resolution** and a **tiered d4-d10 hierarchy for damage**.

### Resolution Rolls (d10 Only)

All skill checks, attribute checks, and resolution rolls use **d10 exclusively**:

```
Resolution Roll = [Attribute] d10 vs DC
Example: WITS 6 → Roll 6d10, count successes ≥ DC
```

### Damage Dice (Tiered Hierarchy)

Damage rolls use a **tier-based die system** (d4–d10) for mechanical differentiation:

| Tier | Die | Avg | Use Case |
|------|-----|-----|----------|
| **Minor** | d4 | 2.5 | DoT ticks, trinkets, improvised weapons |
| **Light** | d6 | 3.5 | Daggers, unarmed, Tier 1 abilities |
| **Medium** | d8 | 4.5 | Swords, axes, polearms, Tier 2 abilities |
| **Heavy** | d10 | 5.5 | Greatswords, Tier 3+, elite abilities |

### Examples

**Resolution (always d10):**
```
WITS check DC 3 → Roll 6d10, count successes
Resist Fear → Roll WILL d10 vs DC
```

**Damage (tiered):**
```
Dagger: 2d6 + 2
Longsword: 2d8 + 3
Greatsword: 2d10 + 5
DoT tick: 1d4 per stack
```

### When Auditing

| Context | Correct Die |
|---------|-------------|
| Skill/attribute checks | d10 |
| Resistance rolls | d10 |
| Weapon damage | d4/d6/d8/d10 by weapon tier |
| Ability damage | d4/d6/d8/d10 by ability tier |
| DoT/trinket damage | d4 |

### Invalid Notation (Flag These)

- ❌ `Roll 6d8 for your WITS check` — Resolution should be d10
- ❌ `Greatsword deals 2d6` — Heavy weapons use d10
- ❌ `All damage is d10` — Ignores tier hierarchy