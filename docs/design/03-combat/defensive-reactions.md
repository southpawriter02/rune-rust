---
id: SPEC-COMBAT-DEFENSE
title: "Defensive Reactions — Complete Specification"
version: 1.0
status: draft
last-updated: 2025-12-07
related-files:
  - path: "RuneAndRust.Engine/CombatEngine.cs"
    status: Exists
---

# Defensive Reactions — Evasion, Blocking, and Parrying

> *"To survive is not merely to endure blows—it is to turn an enemy's strength against them."*

---

## Document Control

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-07 | Initial specification |

---

## 1. Overview

### 1.1 Identity Table

| Property | Value |
|----------|-------|
| Spec ID | `SPEC-COMBAT-DEFENSE` |
| Category | Combat System |
| Type | Mechanics |
| Dependencies | Combat Resolution |

### 1.2 Core Philosophy

Defense in Rune & Rust is **active, not passive**. Every defensive option consumes resources and requires decisions. Tanks invest in STURDINESS and shields; agile fighters invest in FINESSE and evasion; specialists parry and riposte.

---

## 2. Defense Types

### 2.1 Defense Comparison

| Defense Type | Attribute | Trigger | Effect |
|--------------|-----------|---------|--------|
| **Passive Defense** | STURDINESS | Automatic | Dice pool vs attacks |
| **Defend Action** | STURDINESS | Action spent | +% damage reduction |
| **Evasion** | FINESSE | Reaction (Skirmisher) | Reduce incoming hit |
| **Block** | STURDINESS | Reaction (Shield) | Absorb damage |
| **Parry** | FINESSE | Reaction (Weapon) | Negate + riposte |

---

## 3. Passive Defense (STURDINESS Roll)

### 3.1 How It Works

Every attack triggers a passive defense roll:

```
Defense Dice = STURDINESS
Defense Successes = Count 8-10 on d10

Net Successes = Attack Successes - Defense Successes
```

### 3.2 STURDINESS Defense Table

| STURDINESS | Dice Pool | Avg Successes | Notes |
|------------|-----------|---------------|-------|
| 1 | 1d10 | 0.30 | Glass cannon |
| 3 | 3d10 | 0.90 | Below average |
| 5 | 5d10 | 1.50 | Average |
| 7 | 7d10 | 2.10 | Competent |
| 10 | 10d10 | 3.00 | Tank peak |

### 3.3 Hit Chance by Matchup

| Attack vs Defense | Approx Hit Chance |
|-------------------|-------------------|
| 3d10 vs 3d10 | 50% |
| 5d10 vs 5d10 | 50% |
| 7d10 vs 5d10 | 68% |
| 10d10 vs 5d10 | 82% |
| 5d10 vs 10d10 | 18% |

---

## 4. Defend Action

### 4.1 Mechanics

| Property | Value |
|----------|-------|
| Cost | 5 Stamina |
| Action | Standard Action |
| Duration | Until next hit or 1 turn |
| Effect | Roll STURDINESS, gain Defense Bonus |

### 4.2 Defense Bonus Calculation

```
Defense Bonus = STURDINESS Successes × 25%
Maximum: 75% (3 successes)
```

| Successes | Defense Bonus |
|-----------|---------------|
| 0 | 0% |
| 1 | 25% |
| 2 | 50% |
| 3+ | 75% (capped) |

### 4.3 Defend Properties

- **Consumed on First Hit**: Bonus applies, then expires
- **Stacks with Armor**: Defense Bonus (%) applied before Soak (flat)
- **One Per Turn**: Cannot Defend multiple times per turn

---

## 5. Evasion (Skirmisher)

### 5.1 Evasion Mechanic

Skirmisher archetype gains access to **Evasion**:

| Property | Value |
|----------|-------|
| Type | Reaction |
| Cost | 5 Stamina |
| Limit | 1 per round (2 at high rank) |
| Trigger | When targeted by attack |

### 5.2 Evasion Resolution

```
Evasion Roll = FINESSE dice pool
On 2+ successes: Attack automatically misses
On 1 success: -2 to attacker's net successes
On 0 successes: No effect, Stamina wasted
```

### 5.3 Evasion vs Block

| Evasion (Skirmisher) | Block (Shield-user) |
|----------------------|---------------------|
| Uses FINESSE | Uses STURDINESS |
| Avoids damage entirely | Absorbs damage |
| Light armor only | Any armor |
| No counter-attack | No counter-attack |

---

## 6. Block (Shield)

### 6.1 Shield Block Mechanic

Characters with shields can **Block**:

| Property | Value |
|----------|-------|
| Type | Reaction |
| Cost | 3 Stamina |
| Requires | Equipped shield |
| Limit | Varies by shield type |

### 6.2 Block Resolution

```
Block Roll = STURDINESS dice pool + Shield Bonus
Each success absorbs 2 damage
Remaining damage applies to HP
```

### 6.3 Shield Types

| Shield | Block Bonus | Max Blocks/Round |
|--------|-------------|------------------|
| Buckler | +1d10 | 3 |
| Round Shield | +2d10 | 2 |
| Kite Shield | +3d10 | 2 |
| Tower Shield | +4d10 | 1 |

### 6.4 Shield Durability

- Shields have **Durability** (10-30)
- Each block reduces Durability by 1
- At Durability 0, shield breaks

---

## 7. Parry (Counter-Attack System)

### 7.1 Parry Mechanic

Characters can **Parry** melee attacks:

| Property | Value |
|----------|-------|
| Type | Reaction |
| Cost | 5 Stamina |
| Requires | Melee weapon |
| Limit | 1 per round (2 for specialists) |

### 7.2 Parry Pool Calculation

```
Parry Pool = FINESSE + Weapon Skill + Modifiers

Modifiers:
  Defensive Stance: +1d10
  Hólmgangr (Rank 1): +1d10
  Hólmgangr (Rank 2-3): +2d10
  Atgeir-wielder: +1d10
```

### 7.3 Parry Outcomes

| Condition | Outcome | Effect |
|-----------|---------|--------|
| Parry < Attack | **Failed** | Attack hits normally |
| Parry = Attack | **Standard** | Attack negated |
| Parry > Attack | **Superior** | Attack negated, specialists riposte |
| Parry > Attack by 5+ | **Critical** | Attack negated, ALL can riposte |

### 7.4 Riposte

When parry triggers riposte:

```
Riposte = Free melee attack
Damage = Weapon + MIGHT + Rank Bonus

Cannot be parried by enemy.
```

| Rank | Riposte Bonus |
|------|---------------|
| Base (Critical Parry) | Standard damage |
| Hólmgangr Rank 1 | Superior triggers riposte |
| Hólmgangr Rank 2 | +1d10 damage |
| Hólmgangr Rank 3 | +1d10 damage, inflict [Staggered] |

---

## 8. Defensive Stance

### 8.1 Stance Properties

| Property | Value |
|----------|-------|
| Activation | Free action |
| Duration | Until changed |
| Bonus | +25% damage reduction |
| Penalty | -25% damage dealt |

### 8.2 Stance Synergies

- **With Defend**: Stack for up to 100% reduction (capped at 75% effective)
- **With Block**: Block absorbs, stance reduces remaining
- **With Parry**: +1d10 to parry pool

---

## 9. Defense Priority

### 9.1 Resolution Order

When multiple defenses available:

```
1. Evasion (if declared) → May avoid attack entirely
2. Parry (if declared) → May negate and riposte
3. Block (if declared) → Absorbs partial damage
4. Passive Defense → STURDINESS roll always applies
5. Defend Bonus → Applied if active
6. Soak → Flat armor reduction
```

### 9.2 Reaction Limits

| Defense Type | Reactions/Round |
|--------------|-----------------|
| Evasion | 1 (Skirmisher: 2 at Rank 3) |
| Block | 1-3 (by shield type) |
| Parry | 1 (Hólmgangr Rank 3: 2) |

---

## 10. Trauma Integration

### 10.1 Defense Stress

| Event | Stress Change |
|-------|---------------|
| Failed Parry | +5-8 |
| Block breaks shield | +10 |
| 3+ consecutive failed defenses | +10 |
| Superior Parry | -3 |
| Critical Parry | -5 |
| Riposte kill | -8 |

---

## 11. Technical Implementation

### 11.1 Data Model

```csharp
public enum DefenseType { Passive, Defend, Evasion, Block, Parry }
public enum ParryOutcome { Failed, Standard, Superior, Critical }

public class DefenseResult
{
    public DefenseType Type { get; set; }
    public int DiceRolled { get; set; }
    public int Successes { get; set; }
    public int DamageReduced { get; set; }
    public bool TriggeredRiposte { get; set; }
}
```

### 11.2 Service Interface

```csharp
public interface IDefenseService
{
    // Passive defense
    DefenseResult RollPassiveDefense(Character defender);
    
    // Defend action
    int CalculateDefendBonus(Character defender);
    
    // Reactions
    EvasionResult AttemptEvasion(Character defender, Attack attack);
    BlockResult AttemptBlock(Character defender, Attack attack, Shield shield);
    ParryResult AttemptParry(Character defender, Attack attack, Weapon weapon);
    
    // Riposte
    AttackResult ExecuteRiposte(Character parrier, Character attacker);
}
```

---

## 12. Phased Implementation Guide

### Phase 1: Core Systems
- [ ] **Data Model**: Implement `DefenseType`, `DefenseResult`.
- [ ] **Passive**: Implement `RollPassiveDefense` (Sturdiness based).
- [ ] **Defend**: Implement `CalculateDefendBonus` logic.

### Phase 2: Reactions
- [ ] **Block**: Implement Shield Block absorption logic.
- [ ] **Evasion**: Implement Skirmisher Evasion logic.
- [ ] **Parry**: Implement Parry vs Attack comparison logic.

### Phase 3: Advanced Logic
- [ ] **Riposte**: Implement "Free Attack" trigger on Critical Parry.
- [ ] **Durability**: Implement Shield Durability reduction on block.
- [ ] **Limits**: Enforce "Reactions Per Round" limits.

### Phase 4: UI & Feedback
- [ ] **Reaction UI**: "Use Reaction?" Prompt when attacked.
- [ ] **Visuals**: Shield icon for Block, Spark for Parry.
- [ ] **Log**: "Parried!" or "Blocked (5 dmg)" messages.

---

## 13. Testing Requirements

### 13.1 Unit Tests
- [ ] **Passive**: 5 Sturdiness -> 5d10 rolled.
- [ ] **Defend**: 2 Successes -> 50% reduction.
- [ ] **Evasion**: 2 Successes -> Attack Misses.
- [ ] **Block**: 3 Successes -> 6 Damage Absorbed.
- [ ] **Parry**: Parry > Attack -> Negated.

### 13.2 Integration Tests
- [ ] **Shield Break**: Block until Durability 0 -> Item Lost/Broken.
- [ ] **Riposte** : Crit Parry -> Free Attack Triggered -> Damage Dealt.
- [ ] **Order**: Evasion Checked -> then Block -> then Passive.

### 13.3 Manual QA
- [ ] **UI**: Verify "Reaction Used" decreases counter.
- [ ] **Log**: Verify "Buckler absorbs 4 damage" message.

---

## 14. Logging Requirements

**Reference:** [logging.md](../logging.md)

### 14.1 Log Events

| Event | Level | Message Template | Properties |
|-------|-------|------------------|------------|
| Defense Roll | Info | "{Defender} rolled {Type}: {Successes} successes." | `Defender`, `Type`, `Successes` |
| Reaction Used | Info | "{Defender} used {Reaction} against {Attacker}." | `Defender`, `Reaction`, `Attacker` |
| Shield Break | Warning | "{Defender}'s shield SHATTERED!" | `Defender` |
| Riposte | Info | "COUNTER! {Defender} ripostes!" | `Defender` |

---

## 15. Related Specifications

| Spec ID | Relationship |
|---------|--------------|
| [Combat Resolution](combat-resolution.md) | Parent combat loop |
| [Active Abilities](active-abilities.md) | Reaction ability definitions |
| [Attack Outcomes](attack-outcomes.md) | Attack resolution |
| [Combat Stances](combat-stances.md) | Stance bonuses |
| [Attributes](../01-core/attributes.md) | STURDINESS, FINESSE |
