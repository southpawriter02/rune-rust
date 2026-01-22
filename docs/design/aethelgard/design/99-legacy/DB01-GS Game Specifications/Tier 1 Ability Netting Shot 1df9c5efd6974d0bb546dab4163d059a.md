# Tier 1 Ability: Netting Shot

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Skirmisher
Balance Validated: No
Document ID: AAM-SPEC-ABILITY-HLEKKRMASTER-NETTINGSHOT-v5.0
Parent item: Hlekkr-master (Glitch Exploiter) — Specialization Specification v5.0 (Hlekkr-master%20(Glitch%20Exploiter)%20%E2%80%94%20Specialization%20%20e50dff9a1cb14a8fb79a7ba71da8f771.md)
Proof-of-Concept Flag: Yes
Sub-Type: Tier 1
Template Validated: No
Voice Validated: No

## Core Identity

**Netting Shot** is your primary crowd control applicator. You hurl a weighted net with practiced accuracy, designed to entangle erratic movements and exploit physical instability. Enemies caught in your net cannot escape your kill zone.

---

## Mechanics

### Cost

20 Stamina | **Standard Action** | **Range:** 10m | **Cooldown:** 2 turns

### Effect

- FINESSE attack dealing **1d6 Physical damage**
- Apply **[Rooted]** for 2 turns
- [Rooted]: Target cannot move from current position

### Rank Progression

| Rank | PP Cost | Effect |
| --- | --- | --- |
| 1 | 3 | 1d6 damage; [Rooted] 2 turns |
| 2 | 20 | [Rooted] 3 turns; can split net to target **2 enemies** |
| 3 | 35 | 15 Stamina cost; vs 60+ Corruption also apply [Slowed] 2 turns |

### Duration with Pragmatic Preparation

| Configuration | [Rooted] Duration |
| --- | --- |
| Base Rank 1 | 2 turns |
| + Pragmatic Prep R1 | 3 turns |
| Rank 2 + Pragmatic Prep R3 | 5 turns |

---

## Tactical Applications

**Lane Lock:**

```jsx
Enemy melee rushes your backline
Netting Shot → [Rooted] 2+ turns
Result: Enemy stuck in front row, cannot reach casters
```

**Split Net (Rank 2):**

```jsx
2 enemies in formation → Single Netting Shot
Both become [Rooted]
Result: 2-for-1 crowd control efficiency
```

**Synergies:**

- **Pragmatic Preparation:** Extended [Rooted] duration
- **Punish the Helpless:** +50-100% damage vs rooted targets
- **Grappling Hook Toss:** Root front line, then pull back row

---

## Failure Modes

<aside>
⚠️

**Low Damage** — 1d6 is minimal. This is a control ability, not a damage dealer.

</aside>

<aside>
⚠️

**Root Only** — [Rooted] prevents movement but not attacks. Ranged enemies remain dangerous.

</aside>

---

## Integration Notes

**Parent Specialization:** Hlekkr-master (Chain-Master)

**Archetype:** Skirmisher

**Role:** Primary Crowd Control, Lane Denial