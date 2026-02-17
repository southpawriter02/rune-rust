# Stance System — Mechanic Specification v5.0

Type: Mechanic
Description: Universal combat stance system with three postures (Aggressive/Defensive/Calculated) that modify damage, defense, and trauma checks.
Priority: Must-Have
Status: In Design
Balance Validated: No
Document ID: AAM-SPEC-MECHANIC-STANCES-v5.0
Proof-of-Concept Flag: No
Sub-Type: Combat
Template Compliance: v5.0 Three-Tier Template
Template Validated: No
Trauma Economy Risk: Low
Voice Validated: No

## Core Philosophy

The Stance System transforms combat into deliberate tactical positioning where characters declare their **metaphysical intent**—how they interface with Aethelgard's broken reality each turn.

**Three Stances:**

- **Aggressive:** High damage (+4), low defense (-3), vulnerable to trauma (-2 WILL)
- **Defensive:** High defense (+2 Soak, +2 WILL), reduced damage (75%)
- **Calculated:** Balanced baseline (no modifiers)

---

## Key Mechanics

### Stance Modifiers

| Stance | Damage | Defense | Soak | WILL Checks | Stamina Regen |
| --- | --- | --- | --- | --- | --- |
| **Aggressive** | +4 | -3 | — | -2 | Normal |
| **Defensive** | ×0.75 | — | +2 | +2 | -5/turn |
| **Calculated** | — | — | — | — | Normal |

### Action Economy

- **Stance Change:** Free Action (once per turn)
- **Restrictions:** Cannot change stance while [Stunned], [Seized], [Feared], or [Disoriented]

---

## Trauma Economy Integration

**Stress Vectors:**

- Attacked in Aggressive Stance: +8-10 stress (exposed weakness)
- Missing attacks in Defensive Stance: +5 stress (wasted opportunity)
- Enemy forces stance change: +5 stress (loss of control)

**Stress Relief:**

- Maintaining optimal stance 3+ turns: -5 stress (tactical confidence)
- Blocking major attack in Defensive: -3 stress (validation)

---

## Database Schema

```sql
CREATE TABLE CharacterStances (
    CharacterID INTEGER PRIMARY KEY,
    CurrentStance TEXT NOT NULL DEFAULT 'Calculated',
    StanceDuration INTEGER DEFAULT 0,
    LastStanceChange DATETIME,
    FOREIGN KEY (CharacterID) REFERENCES Characters(ID)
);
```

---

## Service Architecture

```csharp
public class StanceService
{
    public async Task<StanceChangeResult> ChangeStance(int characterId, CombatStance newStance);
    public CombatStance GetCurrentStance(int characterId);
    public bool CanChangeStance(int characterId);
    
    public int CalculateDamageModifier(int characterId);
    public int CalculateDefenseModifier(int characterId);
    public int CalculateSoakModifier(int characterId);
    public int CalculateTraumaCheckModifier(int characterId);
}
```

---

**Estimated Implementation:** 8-12 hours

**Migration Source:** v2.0 Combat Stance System specifications