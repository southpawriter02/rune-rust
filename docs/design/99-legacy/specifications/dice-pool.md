# Dice Pool System — Core System Specification v5.0

Status: Proposed
Balance Validated: No
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

## I. Core Philosophy: The Probability of Coherence

The Dice Pool System is the **mathematical engine that drives every uncertain action in Aethelgard**. Its core philosophy is to model the probability of a character successfully imposing a **coherent outcome upon a chaotic, glitching reality**.

**Design Pillars:**

- **Granular Outcomes:** Moves beyond binary pass/fail to create degrees of success
- **Bell Curve Distribution:** Natural probability curve feels organic, not "swingy"
- **Universal Application:** Single consistent mechanic for all resolution
- **Transparent Results:** Players see individual die results, not black-box calculations
- **Meaningful Progression:** More dice = higher probability of success

**The Conceptual Model:**

Every roll of the dice is a tangible representation of a character gathering their:

- **Innate Talent** (Attributes)
- **Learned Expertise** (Skills)
- **Situational Advantages** (Modifiers)

The **number of dice** in the pool represents the character's total capability. The **results** represent the degree to which they successfully impose order on chaos.

**Why d10 Pools?**

**Single Die Type:**

- Entire game uses d10 for all resolution
- Ensures ease of understanding
- Consistent probability model
- No need to track multiple die types

**Success Counting:**

- More intuitive than summing
- Creates natural bell curve
- Allows for dramatic swings (heroic luck, tragic failure)
- Botch mechanic represents Blight interference

**Scalability:**

- Works for low-power (2-3 dice) and high-power (15+ dice) characters
- Organic power cap (even 20 dice can fail)
- Progressive improvement feels meaningful

---

*Migration in progress from v2.0 Dice Pool System specification. Remaining sections to be added incrementally.*

- Organic power cap (even 20 dice can fail)
- Progressive improvement feels meaningful

## II. The Anatomy of a Roll

### **The Die Type**

**Specification:** The standard die for **ALL resolution checks** in Aethelgard is the **ten-sided die (d10)**.

**No Exceptions:** Every dice pool, regardless of context, uses d10

**Rationale:**

- Single die type simplifies gameplay
- Consistent probability model across all systems
- Easier for players to learn and master
- No need to track multiple die types
- Probability calculations remain consistent

---

### **The Dice Pool**

**Definition:** A collection of d10s where the **number** of dice represents the character's total capability for that action

**Assembly Formula:**

```csharp
Dice Pool Size = Governing Attribute + Skill Rank + Situational Modifiers
```

**Component Breakdown:**

**1. Governing Attribute (Base Capability)**

- Primary contributor to dice pool
- Represents innate talent
- Examples:
    - Attack accuracy: FINESSE attribute
    - Damage with heavy weapons: MIGHT attribute
    - Resisting fear: WILL attribute
    - Hacking check: WITS attribute

**2. Skill Rank (Learned Expertise)**

- Adds +1 die per rank
- Represents training and practice
- Range: 0 (Untrained) to 5 (Master)
- Example: System Bypass Rank 3 adds +3 dice to hacking checks

**3. Situational Modifiers (Temporary Factors)**

- Buffs: Add dice (e.g., [Coherent] status)
- Debuffs: Remove dice (e.g., [Disoriented] status)
- Equipment: Quality tools add dice
- Environmental: Lighting, terrain, etc.
- Ability bonuses: Specialization passives add dice

**Pool Size Examples:**

**Low-Power Character (Early Game):**

- FINESSE 6 + System Bypass Rank 1 + Masterwork Tools (+1) = **8d10**

**Mid-Power Character:**

- WITS 10 + System Bypass Rank 3 + Specialization Passive (+2) = **15d10**

**High-Power Character (Late Game):**

- WITS 15 + System Bypass Rank 5 + Specialization Passive (+2) + Buff (+1) = **23d10**

---

### **The Roll**

**Execution:** Player (or system) rolls all dice in the pool simultaneously

**Individual Reading:** Each die is read individually (not summed)

**Result Recording:** All individual results are displayed to player

**Example Roll:**

```
Pool: 7d10
Results: [9, 8, 5, 2, 1, 10, 7]
```

---

## III. Interpreting Results: Success Counting

The result of a roll is determined by **counting** the number of Successes and Botches on individual dice, not by summing the total.

### **The Core Values**

**Success:**

- **Values:** 8, 9, or 10 on a single d10
- **Count:** Each die showing 8+ counts as **one Success**
- **Probability:** 30% chance per die (3 out of 10 values)

**Botch:**

- **Value:** 1 on a single d10
- **Count:** Each die showing 1 counts as **one Botch**
- **Probability:** 10% chance per die (1 out of 10 values)
- **Narrative:** Represents Blight interference, cognitive glitch, reality warp

**Neutral:**

- **Values:** 2, 3, 4, 5, 6, or 7
- **Count:** No effect whatsoever
- **Probability:** 60% chance per die (6 out of 10 values)
- **Purpose:** Creates probability buffer between success and failure

---

### **Calculating the Final Result**

**Three-Step Process:**

**Step 1: Count Successes**

- Count total number of dice showing 8, 9, or 10

**Step 2: Count Botches**

- Count total number of dice showing 1

**Step 3: Calculate Net Successes**

```csharp
Net Successes = Total Successes - Total Botches
```

**Result can be:**

- **Positive:** More successes than botches (success of some degree)
- **Zero:** Equal successes and botches, or all neutral (failure)
- **Negative:** Not possible (result floors at 0, see Fumble rules)

---

### **Worked Examples**

**Example 1: Mixed Result**

```
Pool: 5d10
Roll: [9, 8, 5, 2, 1]

Step 1: Count Successes = 2 (the 9 and 8)
Step 2: Count Botches = 1 (the 1)
Step 3: Net Successes = 2 - 1 = 1

Final Result: 1 Net Success
```

**Example 2: Clean Success**

```
Pool: 6d10
Roll: [10, 9, 8, 7, 6, 4]

Step 1: Count Successes = 3 (10, 9, 8)
Step 2: Count Botches = 0
Step 3: Net Successes = 3 - 0 = 3

Final Result: 3 Net Successes
```

**Example 3: Total Failure**

```
Pool: 4d10
Roll: [7, 5, 3, 2]

Step 1: Count Successes = 0
Step 2: Count Botches = 0
Step 3: Net Successes = 0 - 0 = 0

Final Result: 0 Net Successes (Failure)
```

**Example 4: Fumble**

```
Pool: 3d10
Roll: [1, 1, 4]

Step 1: Count Successes = 0
Step 2: Count Botches = 2
Step 3: Condition Check: 0 Successes AND at least 1 Botch

Final Result: FUMBLE (Critical Failure)
```

---

### **Special Results: Criticals**

**Critical Success:**

- **Trigger:** Roll results in **5 or more Net Successes**
- **Effect:** Powerful bonus effect (system-specific)
- **Examples:**
    - Combat: Critical Hit (double damage dice)
    - Skill Check: Masterful result with bonus outcome
    - Crafting: Create item with bonus quality tier
- **Probability:** Higher dice pools significantly increase chance

**Fumble (Critical Failure):**

- **Trigger:** Roll results in **zero Successes AND at least one Botch**
- **Effect:** Catastrophic failure with negative consequence
- **Examples:**
    - Combat: Weapon jams, lose turn
    - Skill Check: Trigger trap, alert guards
    - Crafting: Destroy materials, waste resources
- **Narrative:** Direct manifestation of Runic Blight's influence
- **Flavor:** "Cognitive glitch," reality warp, paradox moment
- **Probability:** Low dice pools have higher Fumble risk

**Critical Boundaries:**

- **Minimum Net Successes:** 0 (cannot go negative due to Fumble rule)
- **Maximum Net Successes:** Theoretically unlimited (all dice roll 10)
- **Critical Success Threshold:** 5+ Net Successes
- **Fumble Condition:** 0 Successes + ≥1 Botch

---

## IV. Bonus Die & Penalty Die System

Situational advantages and disadvantages are handled by **adding or removing dice from the pool BEFORE the roll**.

### **Bonus Die**

**Definition:** Temporary advantage that grants **one extra d10** to the dice pool

**Sources:**

- **Status Effects:** [Coherent] buff, [Focused] buff
- **Equipment:** High-quality tools ([Masterwork], [Legendary])
- **Environmental:** Favorable conditions, advantageous terrain
- **Party Support:** Ally abilities that grant bonuses
- **Consumables:** Temporary enhancement items

**Stacking:** Multiple bonus dice can stack

**Example:**

```
Base Pool: WITS 8 + System Bypass 3 = 11d10
Masterwork Tools: +1d10
[Coherent] Buff: +1d10
Final Pool: 13d10
```

---

### **Penalty Die**

**Definition:** Temporary disadvantage that **removes one d10** from the dice pool

**Sources:**

- **Status Effects:** [Disoriented], [Exhausted], [Wounded]
- **Environmental:** [Dim Light], [Difficult Terrain], [Psychic Resonance]
- **Equipment:** Damaged or improvised tools
- **Corruption:** High Runic Blight Corruption may impose penalties
- **Enemy Abilities:** Debuffs applied by foes

**Stacking:** Multiple penalty dice can stack

**Example:**

```
Base Pool: FINESSE 7 + No Skill = 7d10
[Disoriented] Debuff: -1d10
[Dim Light] Environment: -1d10
Final Pool: 5d10
```

---

### **Net Modifier Calculation**

**Formula:**

```csharp
Final Pool = Base Pool + Total Bonus Dice - Total Penalty Dice
```

**Minimum Pool:** 1d10 (penalties cannot reduce pool below 1 die)

**Example: Mixed Modifiers**

```
Base Pool: MIGHT 9 = 9d10
Buff (+2): +2d10
Debuff (-1): -1d10
Net: 9 + 2 - 1 = 10d10 (Final Pool)
```

**Example: Extreme Penalties**

```
Base Pool: FINESSE 4 = 4d10
Multiple Debuffs: -5d10
Net: 4 - 5 = -1, floors to 1d10
Final Pool: 1d10 (minimum enforced)
```

---

## V. Probability Model

### **Expected Outcomes by Pool Size**

**Understanding:** Each die has 30% chance of Success, 10% chance of Botch

**Expected Net Successes:**

| Pool Size | Expected Net Successes | Critical Success Chance | Fumble Chance |
| --- | --- | --- | --- |
| 1d10 | 0.2 | 0% | 10% |
| 3d10 | 0.6 | <1% | ~7% |
| 5d10 | 1.0 | ~2% | ~5% |
| 7d10 | 1.4 | ~8% | ~3% |
| 10d10 | 2.0 | ~20% | ~1% |
| 15d10 | 3.0 | ~50% | <1% |
| 20d10 | 4.0 | ~75% | <0.1% |

**Key Insights:**

- **Low Pools (1-3d10):** High variance, Fumble risk significant
- **Mid Pools (5-10d10):** Reliable results, occasional criticals
- **High Pools (15+d10):** Consistent success, frequent criticals, Fumbles nearly impossible

---

### **The Bell Curve Effect**

**Natural Distribution:**

- Low dice pools: High variance, unpredictable
- High dice pools: Low variance, predictable
- Creates natural "power curve" as characters progress

**Anti-Swing Design:**

- Success counting creates smoother probability than single die
- Even bad rolls on high pools often produce some successes
- Even good rolls on low pools can be undone by Botches
- Feels more "fair" than d20 systems

---

## VI. Universal Application

The Dice Pool System is the **single resolution mechanic** for ALL uncertain actions in Aethelgard.

### **Combat Applications**

**Accuracy Checks:**

```csharp
Pool = FINESSE + Weapon Skill Bonus + Modifiers
```

- Determines if attack hits target
- Compared against target's Defense Score

**Damage Rolls:**

```csharp
Pool = Weapon Base Dice + Attribute Bonus
```

- Determines damage inflicted
- Net Successes = damage dealt (after Soak)

**Block/Parry Rolls:**

```csharp
Pool = STURDINESS/FINESSE + Shield/Weapon Bonus
```

- Opposed roll against attacker's Damage Pool
- Net Successes cancel attacker's damage

**Resolve Checks:**

```csharp
Pool = WILL or STURDINESS
```

- Resist status effects, fear, poison, etc.
- Success prevents or reduces effect

---

### **Skill Check Applications**

**System Bypass:**

```csharp
Pool = WITS + System Bypass Rank + Modifiers
```

- Hacking, lockpicking, trap disarming
- Net Successes compared against DC

**Wasteland Survival:**

```csharp
Pool = WITS + Wasteland Survival Rank + Modifiers
```

- Tracking, foraging, navigation
- Net Successes determine quality of outcome

**Rhetoric:**

```csharp
Pool = WILL + Rhetoric Rank + Modifiers
```

- Persuasion, deception, intimidation
- Often opposed roll vs. NPC's WILL

**Acrobatics:**

```csharp
Pool = FINESSE + Acrobatics Rank + Modifiers
```

- Climbing, balancing, stealth
- Net Successes vs. DC or opposed

---

### **Dialogue Check Applications**

**[MIGHT] Intimidation:**

```csharp
Pool = MIGHT + Modifiers
```

- Opposed roll vs. NPC's WILL
- Success: Compliance; Failure: Hostility

**[WITS] Insight:**

```csharp
Pool = WITS + Modifiers
```

- Detect lies, spot inconsistencies
- Success reveals hidden information

**[WILL] Persuasion:**

```csharp
Pool = WILL + Rhetoric Rank + Modifiers
```

- Convince, negotiate, inspire
- Success changes NPC disposition

---

### **Crafting Applications**

**Crafting Checks:**

```csharp
Pool = WITS + Crafting Skill Rank + Tool Bonus
```

- Determines quality of crafted item
- Net Successes determine tier:
    - 0-1: Standard quality
    - 2-4: Masterwork quality
    - 5+: Legendary quality (Critical Success)

---

## VII. Implementation Details

### **Service Architecture**

**DiceService.cs (Core Engine Service)**

**Responsibilities:**

- Handle ALL dice rolling logic
- Calculate Successes, Botches, Net Successes
- Determine Critical Success / Fumble
- Provide consistent, testable implementation

**Key Method:**

```csharp
public DiceResult Roll(int numberOfDice)
{
  // Validate input
  if (numberOfDice < 1) numberOfDice = 1; // Enforce minimum
  
  // Roll dice
  List<int> rolls = new List<int>();
  for (int i = 0; i < numberOfDice; i++)
  {
    rolls.Add([Random.Next](http://Random.Next)(1, 11)); // 1-10 inclusive
  }
  
  // Count Successes (8, 9, 10)
  int successes = rolls.Count(r => r >= 8);
  
  // Count Botches (1)
  int botches = rolls.Count(r => r == 1);
  
  // Calculate Net Successes
  int netSuccesses = successes - botches;
  
  // Check for Fumble
  bool isFumble = (successes == 0 && botches > 0);
  if (isFumble) netSuccesses = 0; // Fumble floors at 0
  
  // Check for Critical Success
  bool isCritical = (netSuccesses >= 5);
  
  // Return result object
  return new DiceResult
  {
    IndividualRolls = rolls,
    TotalSuccesses = successes,
    TotalBotches = botches,
    NetSuccesses = netSuccesses,
    IsCriticalSuccess = isCritical,
    IsFumble = isFumble
  };
}
```

---

### **DiceResult.cs (Result Object)**

**Purpose:** Encapsulates all information about a dice roll

**Properties:**

```csharp
public class DiceResult
{
  public List<int> IndividualRolls { get; set; }  // All die results
  public int TotalSuccesses { get; set; }         // Count of 8, 9, 10
  public int TotalBotches { get; set; }           // Count of 1
  public int NetSuccesses { get; set; }           // Successes - Botches
  public bool IsCriticalSuccess { get; set; }     // Net >= 5
  public bool IsFumble { get; set; }              // 0 Successes + ≥1 Botch
}
```

**Usage:**

- Returned by all DiceService.Roll() calls
- Consumed by combat, skill, dialogue, crafting systems
- Logged for analytics and debugging
- Displayed in UI for player transparency

---

## VIII. UI Display Requirements

### **Standard Roll Display**

**Format:** Clear, transparent communication of roll results

**Example 1: Success**

```
> You attack the Corrupted Servitor (Accuracy Pool: 5d10)...
  Roll: [9, 8, 5, 2, 1]
  → 2 Successes, 1 Botch
  Result: 1 Net Success
  The attack hits!
```

**Example 2: Failure**

```
> You attempt to hack the terminal (System Bypass Pool: 4d10)...
  Roll: [7, 5, 3, 2]
  → 0 Successes, 0 Botches
  Result: 0 Net Successes
  The terminal resists your intrusion.
```

---

### **Critical Success Display**

**Format:** Prominent, celebratory message

**Example:**

```
> You attack the Warden (Accuracy Pool: 12d10)...
  Roll: [10, 10, 9, 9, 8, 8, 7, 6, 4, 3, 2, 1]
  → 6 Successes, 1 Botch
  Result: 5 Net Successes
  
  *** CRITICAL SUCCESS! ***
  A perfect opening! You drive your blade past the Warden's guard
  and into a vulnerable seam in their armor!
  
  Damage dice doubled!
```

---

### **Fumble Display**

**Format:** Ominous, narrative-heavy message

**Example:**

```
> You attempt to hack the terminal (System Bypass Pool: 3d10)...
  Roll: [1, 1, 4]
  → 0 Successes, 2 Botches
  Result: FUMBLE
  
  [!] COGNITIVE GLITCH [!]
  Your thoughts fragment as the terminal's corrupted logic
  invades your consciousness. The interface flickers,
  unresolving into static.
  
  The alarm triggers. Guards alerted.
```

---

## IX. Integration with Other Systems

### **Combat System Integration**

**All combat rolls use Dice Pool System:**

- Accuracy Checks (hit/miss)
- Damage Rolls (damage dealt)
- Block/Parry Rolls (damage mitigation)
- Resolve Checks (resist status effects)

**For detailed combat integration, see:** Combat System specification

---

### **Skills System Integration**

**All skill checks use Dice Pool System:**

- Formula: `WITS/FINESSE/WILL + Skill Rank + Modifiers`
- Net Successes compared against DC
- Degrees of success: Fumble, Failure, Success, Critical Success

**For detailed skill mechanics, see:** Skills System specification

---

### **Attributes System Integration**

**Attributes are the primary pool contributor:**

- Every roll starts with attribute value as base dice
- Attribute increases directly improve all related rolls
- Attributes determine minimum competency

**For detailed attribute mechanics, see:** Attributes System — Core System Specification v5.0

---

### **Trauma Economy Integration**

**Psychic Stress affects dice pools:**

- High Stress may impose Penalty Dice
- Formula: `-1 die per 20 Psychic Stress` (or similar)
- Debuffs like [Disoriented] reduce pools

**Runic Blight Corruption affects dice pools:**

- High Corruption may impose Penalty Dice
- Corrupted characters more susceptible to mental effects

---

## X. Design Philosophy Summary

**Why Success Counting?**

The Dice Pool System's success counting mechanic creates:

1. **Granular Outcomes:** Not just pass/fail, but degrees of success
2. **Bell Curve Probability:** Natural, organic distribution
3. **Transparent Results:** Players see every die roll
4. **Meaningful Progression:** More dice = better odds
5. **Dramatic Moments:** Fumbles and Criticals create stories

**Why Botches?**

Botches serve multiple purposes:

1. **Thematic:** Represent Blight interference, reality glitches
2. **Mechanical:** Counter raw dice pool inflation
3. **Dramatic:** Create tension even in high-power scenarios
4. **Balancing:** Prevent "auto-win" with huge pools

**Why d10 Exclusively?**

Single die type ensures:

1. **Simplicity:** Players only need one type of die
2. **Consistency:** Same probability model everywhere
3. **Learnability:** Master one system, use it everywhere
4. **Implementation:** Easier to code and test

**The Organic Power Curve:**

The system provides natural scaling:

- **Early Game (3-6d10):** Uncertain, risky, Fumbles possible
- **Mid Game (7-12d10):** Reliable, occasional criticals
- **Late Game (15+d10):** Dominant, frequent criticals, Fumbles rare

Yet even at 20d10, failure remains theoretically possible—no "god mode."

---

## Migration Complete

This specification has been fully migrated from v2.0 Dice Pool System specification[[1]](https://www.notion.so/Feature-Specification-The-Dice-Pool-System-2a355eb312da80e4a757ded404ad97dd?pvs=21) and enhanced with DB10 structure requirements. All sections now reflect Aethelgard's foundational resolution mechanics, probability model, universal application, and implementation details.

**Status:** Draft → Ready for Review

**Next Steps:**

1. Review for consistency with Combat System integration
2. Validate probability model matches intended power curve
3. Test DiceService.cs implementation against specification
4. Update status to Final after review

**Referenced By:**

- Combat System (Accuracy, Damage, Block/Parry, Resolve)
- Skills System (all skill checks)
- Dialogue System (attribute-based checks)
- Crafting System (quality determination)
- Trauma Economy (Resolve Checks against Stress/Corruption)
- Every system that requires uncertain outcome resolution