# Ability Rank Progression Redesign — Walkthrough

> **Implementation Complete:** Option E (Simplified Tier-Rank Hybrid)  
> **Date:** 2025-12-14

---

## Summary

The ability rank progression system has been redesigned from the confusing implicit tree-based triggers to an explicit PP-per-rank upgrade system.

### Before (Old System)
- Tier 1 abilities started at Rank 1, upgraded to Rank 2 when you bought 2× Tier 2 abilities
- Tier 2/3 abilities started at Rank 2 (players never saw Rank 1)
- All abilities upgraded to Rank 3 when you bought the Capstone
- PP costs in Saga System (5/10/15/25) didn't match specializations (3/4/5/6)

### After (New System)
- **All abilities start at Rank 1** when unlocked
- **Rank 2 costs +2 PP** (optional, player's choice)
- **Rank 3 costs +3 PP** (optional, requires Rank 2)
- **No tree dependencies** for rank upgrades
- **PP costs aligned** across all specs (3/4/5/6)

---

## Files Changed

### Core System Specs

| File | Changes |
|------|---------|
| [saga-system.md](../01-core/saga-system.md) | New Section 4.2 (Ability Rank Progression), fixed PP costs, updated UI mockup |

---

### Templates

| File | Changes |
|------|---------|
| [ability.md](../.templates/ability.md) | New rank section headers, updated Overview table |
| [_template.md](../03-character/specializations/_template.md) | New rank rules, removed confusing tree diagrams |

---

### Golden Standard (Berserkr)

| File | Changes |
|------|---------|
| [berserkr/berserkr-overview.md](../03-character/specializations/berserkr/berserkr-overview.md) | New PP Investment Table with per-ability costs |

---

### Example Ability

| File | Changes |
|------|---------|
| [wild-swing.md](../03-character/specializations/berserkr/abilities/wild-swing.md) | New rank headers with PP costs |

---

## Batch Updates

The following patterns were replaced across **all ability specs** in `docs/03-character/specializations/` and `docs/02-entities/specializations/`:

| Old Pattern | New Pattern |
|-------------|-------------|
| `### Rank 1 (Unlocked: When ability is learned)` | `### Rank 1 (Base — included with ability unlock)` |
| `### Rank 2 (Unlocked: When 2 Tier 2 abilities are trained)` | `### Rank 2 (Upgrade Cost: +2 PP)` |
| `### Rank 3 (Unlocked: When Capstone is trained)` | `### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)` |
| `*Unlocked: When ability is learned*` | `*Base — included with ability unlock*` |
| `*Unlocked: When 2 Tier 2 abilities trained*` | `*Upgrade Cost: +2 PP*` |
| `*Unlocked: When Capstone trained*` | `*Upgrade Cost: +3 PP, requires Rank 2*` |

**Files NOT modified:** `docs/99-legacy/` (reference-only)

---

## Verification

```bash
# Confirm no old patterns remain (excluding legacy)
grep -r "Unlocked: When" docs --include="*.md" | grep -v "99-legacy" | wc -l
# Result: 0
```

---

## New PP Economy

| Investment | PP Cost | Notes |
|------------|---------|-------|
| Unlock Spec | 3-10 PP | Varies by spec |
| Tier 1 Ability (R1) | 3 PP | Base unlock |
| Tier 1 Ability (R3) | 8 PP | 3 + 2 + 3 |
| Full Tree (R1 only) | ~47 PP | All 9 abilities |
| Full Tree (all R3) | ~92 PP | Maximum investment |

---

## Next Steps (Optional)

The following files still use the old implicit rank unlock rules in their **content** (not headers) and may benefit from review:

- [ ] Specialization overviews — Update the remaining 21 overview files to match `berserkr/berserkr-overview.md` format
- [ ] Ability specs with "Starting Rank: 2" — Now obsolete (all start at Rank 1)
- [ ] Hemorrhaging Strike — Remove "Starting Rank: 2" from overview table
- [ ] Corridor Maker — Remove "Starting Rank: 1" (now implicit)
