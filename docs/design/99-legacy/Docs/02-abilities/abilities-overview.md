# Abilities & Specializations System

**Version**: v0.17 (Based on v0.16 implementation)
**Status**: Core System (Tier 3)
**Dependencies**: Attributes, Stamina, Saga Service (PP spending)
**Integration Points**: Combat Engine, Character Creation, Progression

---

## Table of Contents

1. [Functional Overview](#1-functional-overview)
2. [Ability Mechanics](#2-ability-mechanics)
3. [Specializations](#3-specializations)
4. [Ability Categories](#4-ability-categories)
5. [Balance Considerations](#5-balance-considerations)

---

## 1. Functional Overview

### 1.1 Purpose

**Abilities** are special actions that cost Stamina and provide powerful effects beyond basic attacks. The ability system creates:
- **Class Identity**: Each archetype has unique abilities
- **Tactical Depth**: Players choose when to spend Stamina
- **Progression**: Abilities can be ranked up with PP
- **Specialization**: 3 PP unlocks advanced ability trees **(v0.18: reduced from 10 PP)**

### 1.2 Ability Acquisition

**Starting Abilities** (Character Creation):
- **Warrior**: 3 abilities (Strike, Defensive Stance, Warrior's Vigor passive)
- **Scavenger**: 4 abilities (Exploit Weakness, Quick Dodge, Precision Strike, Survivalist)
- **Mystic**: 4 abilities (Aetheric Bolt, Disrupt, Aetheric Shield, Chain Lightning)
- **Adept**: 3 abilities (Improvised Strike, Analyze, Keen Eye passive)

**Heretical Abilities** (All classes):
- 9 forbidden abilities available to all classes
- Cost Corruption/Stress to use
- Unlock at character creation

**Specialization Abilities** (3 PP unlock, v0.18: reduced from 10 PP):
- 9 tiered abilities per specialization
- Tier 1 (3 abilities): Granted immediately on unlock
- Tier 2-3: Must be purchased with PP

---

## 2. Ability Mechanics

### 2.1 Ability Structure

```csharp
public class Ability
{
    string Name;              // "Power Strike"
    string Description;       // Flavor text
    int StaminaCost;         // 5-50 Stamina
    AbilityType Type;        // Attack/Defense/Utility/Control

    // Rank System
    int CurrentRank;         // 1-3
    int CostToRank2;         // PP cost to advance (typically 2-5 PP)

    // Dice Roll
    string AttributeUsed;    // "might", "finesse", "will", "wits"
    int BonusDice;          // +0 to +6 dice
    int SuccessThreshold;   // 2-4 successes needed

    // Effects
    int DamageDice;         // 0-5 damage dice
    bool IgnoresArmor;      // Bypasses defense
    int DefensePercent;     // 0-75% damage reduction
    bool SkipEnemyTurn;     // Control effect
    // ... many more effect types
}
```

### 2.2 Ability Usage Flow

```
1. Player selects ability
   ↓
2. Check Stamina >= StaminaCost
   ↓
3. Deduct Stamina
   ↓
4. Roll AttributeUsed + BonusDice (e.g., MIGHT 4 + 2 bonus = 6d6)
   ↓
5. Count successes (5-6 on d6)
   ↓
6. Check if successes >= SuccessThreshold
   ↓
7. IF YES: Apply effects (damage, defense, control, etc.)
   IF NO: Ability fails (Stamina still spent)
```

### 2.3 Rank System

**Ranks**: 1 → 2 → 3 (Rank 3 locked until v0.5+)

**Rank Advancement**:
- Cost: `ability.CostToRank2` PP (typically 2-5 PP)
- Method: `SagaService.AdvanceAbilityRank(player, ability)`

**Rank 2 Improvements** (varies by ability):
- +1-2 Bonus Dice (most common)
- -1-2 Stamina Cost (efficiency)
- +1 Damage Dice (power)
- +1 Duration (longer effects)
- -1 Success Threshold (easier to trigger)

**Example: Power Strike Rank Progression**:
```
Rank 1: 10 Stamina, +2 bonus dice, 2d6 damage
Rank 2: 8 Stamina, +3 bonus dice, 2d6 damage (costs 3 PP)
Rank 3: LOCKED (v0.5+)
```

---

## 3. Specializations

### 3.1 Specialization System

**Unlock Cost**: 3 PP (one-time) **(v0.18: reduced from 10 PP)**
**Limit**: One specialization per character
**Availability**: Adept archetype only (v0.7)

### 3.2 Available Specializations

#### **Bone-Setter** (Support/Healer)
**Theme**: Non-magical field medic
**Focus**: HP restoration, bleeding management, stress reduction

**Tier 1 Abilities** (granted on unlock):
1. **Mend Wound** (5 Stamina): Heal ally for 3d6 HP
2. **Apply Tourniquet** (10 Stamina): Stop bleeding effect
3. **Field Triage** (15 Stamina): Emergency stabilization

**Tier 2-3 Abilities** (must purchase with PP):
- Anatomical Insight (debuff enemy, +2 accuracy for allies) **[20 Stamina, v0.18: reduced from 25]**
- Administer Antidote (cure poison/disease)
- Cognitive Realignment (reduce Stress) **[25 Stamina, v0.18: reduced from 30]**
- Miracle Worker (massive heal) **[40 Stamina, v0.18: reduced from 60]**

---

#### **Jötun-Reader** (Utility/Analyst)
**Theme**: System diagnostician, machine whisperer
**Focus**: Enemy analysis, tech checks, bypassing locks

**Tier 1 Abilities** (granted on unlock):
1. **Analyze Weakness** **[25 Stamina, v0.18: reduced from 30]** (5 Stress): Reveal enemy stats
2. **System Diagnostic** (20 Stamina): Understand machine logic
3. **Bypass Lock** (25 Stamina): Open locked doors/chests

**Tier 2-3 Abilities** (must purchase with PP):
- Exploit Design Flaw (debuff enemy defense) **[28 Stamina, v0.18: reduced from 35]**
- Navigational Bypass (teleport through walls)
- The Unspoken Truth (psychic damage)
- Architect of the Silence (control enemy, 15 Stress cost)

---

#### **Skald** (Buffer/Debuffer)
**Theme**: Morale officer, performance artist
**Focus**: Party buffs, enemy debuffs, Stamina recovery

**Tier 1 Abilities** (granted on unlock):
1. **Saga of Courage** (40 Stamina): Party-wide attack buff
2. **Dirge of Defeat** (40 Stamina): Enemy-wide attack debuff
3. **Rousing Verse** (15 Stamina): Restore ally Stamina

**Tier 2-3 Abilities** (must purchase with PP):
- Song of Silence (silence enemy casters)
- Lay of the Iron Wall (massive defense buff)
- Saga of the Einherjar (grant Temp HP + Inspired status)

---

## 4. Ability Categories

### 4.1 Attack Abilities
**Purpose**: Deal damage to enemies
**Examples**: Power Strike, Aetheric Bolt, Whirlwind Strike
**Cost Range**: 10-30 Stamina
**Typical Roll**: MIGHT/FINESSE/WILL + bonus dice vs 2-3 success threshold

### 4.2 Defense Abilities
**Purpose**: Reduce incoming damage
**Examples**: Shield Wall, Quick Dodge, Aetheric Shield
**Cost Range**: 10-20 Stamina
**Typical Effect**: +50-75% defense for 2-3 turns, or negate next attack

### 4.3 Utility Abilities
**Purpose**: Support, healing, buffs
**Examples**: Exploit Weakness, Mend Wound, Rally Cry
**Cost Range**: 5-40 Stamina
**Typical Effect**: Grant bonuses, restore resources, reveal information

### 4.4 Control Abilities
**Purpose**: Disable or manipulate enemies
**Examples**: Disrupt, Song of Silence, Architect of the Silence
**Cost Range**: 12-50 Stamina
**Typical Effect**: Skip enemy turn, silence caster, reduce enemy accuracy

### 4.5 Heretical Abilities
**Purpose**: Powerful effects at cost of Corruption/Stress
**Examples**: Void Strike, Psychic Lash, Desperate Gambit
**Cost Range**: 20-50 Stamina + 2-15 Corruption/Stress
**Typical Effect**: Armor-piercing damage, AOE attacks, massive power spikes

---

## 5. Balance Considerations

### 5.1 Stamina Costs

**Design Goal**: Abilities should be powerful but limited by Stamina scarcity

**Cost Tiers**:
- **Basic** (5-15 Stamina): Bread-and-butter, 2-4 uses/combat
- **Moderate** (16-30 Stamina): Powerful, 1-2 uses/combat
- **Ultimate** (31-60 Stamina): Game-changers, once per combat

**Balance Check**: Warriors (30 base Stamina) can afford ~2 abilities/combat; Mystics (50 base) can afford ~4 abilities/combat.

### 5.2 Rank Advancement ROI

**Question**: Is ranking up abilities worth the PP cost?

**Example: Power Strike Rank 2**:
- Cost: 3 PP
- Benefit: -2 Stamina cost, +1 bonus die
- Result: ~10% better success rate, 20% cheaper usage
- **Value**: High for frequently-used abilities

**Alternative**: 3 PP = 3 attribute increases (+3 dice to ALL rolls)
- **Trade-off**: Broad vs focused improvement

**Conclusion**: Rank up signature abilities; spend PP on attributes for general power.

### 5.3 Specialization Value

**3 PP Cost (v0.18: reduced from 10 PP)**: Achievable by Milestone 2 (4 PP available)
**Benefit**: 9 unique abilities (3 granted immediately, 6 purchasable)

**Opportunity Cost**:
- 3 PP = 3 attribute increases (broad power boost)
- 3 PP = 1-2 ability rank advancements (focused improvements)

**Trade-off**: Specialization grants **unique utility** (healing, analysis, buffs) not available through attributes/ranks.

**Design (v0.18)**: Specializations are now viable within v0.1 scope (15-20 min playtime, 5 PP by Milestone 3). Players can unlock specialization by M2 and still have 1-2 PP for customization, enabling diverse build strategies including support-focused characters.

---

**End of Document**
*For detailed ability stats*: See ability registry (pending)
*For specialization tier details*: See specialization guides (pending)
