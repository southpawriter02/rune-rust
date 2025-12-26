# Progression Points (PP) System — Mechanic Specification v5.0

Status: Proposed
Balance Validated: No
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

## I. Core Philosophy: The Ink of a Living Saga

Progression Points (PP) are the **active, spendable currency of character growth** in Aethelgard. They are not simply "skill points," but a diegetic representation of the focused insight, profound learning, and moments of breakthrough a character achieves after reflecting upon their hard-won experiences.

**The Two-Currency Model:**

- **Legend** is the *story* of the saga (passive accumulation)
- **Progression Points** are the *ink* used to write the next chapter (active spending)

**Design Pillars:**

- **Deliberate Growth:** PP spending creates meaningful, momentous decisions
- **Sanctuary Requirement:** Growth requires safety and reflection (thematic enforcement)
- **Player Agency:** Every upgrade is a conscious choice, not automatic leveling
- **Permanent Choices:** All PP spending is final (no easy respec)
- **Discrete Awards:** PP granted in powerful chunks (Milestones), not incremental drips

**The Core Tension:**

By decoupling the *earning* of experience (Legend) from the *spending* of it (PP), the system creates periods of anticipation and moments of power. Players accumulate potential (Legend) continuously but convert it to growth (PP spending) only during deliberate reflection periods.

**Narrative Integration:**

A character cannot achieve saga-defining growth while their mind is battered by psychic static. They require the "clean boot" and mental clarity of a safe, coherent environment (a settlement's Runic Anchor) to process experiences and compile them into new, functional capabilities.

---

*Migration in progress from v2.0 PP System specification. Remaining sections to be added incrementally.*

A character cannot achieve saga-defining growth while their mind is battered by psychic static. They require the "clean boot" and mental clarity of a safe, coherent environment (a settlement's Runic Anchor) to process experiences and compile them into new, functional capabilities.

## II. PP Acquisition: The Milestone Event

### **The Trigger**

**Specification:** A character is awarded **exactly 1 Progression Point** each time their Legend bar fills completely.

**Event Name:** Reaching a **Milestone**

**Automatic Award:** PP is granted automatically when threshold reached (no player action required)

### **Milestone Mechanics**

**Process (Handled by SagaService.cs):**

1. Monitor `LegendPoints` and `LegendToNextMilestone` values on PlayerCharacter
2. When `LegendPoints >= LegendToNextMilestone`:
    - Increment `ProgressionPoints` by 1
    - Subtract `LegendToNextMilestone` from `LegendPoints` (preserve overflow)
    - Increment `CurrentMilestoneLevel` by 1
    - Recalculate `LegendToNextMilestone` using formula: `(CurrentMilestoneLevel × 100) + 500`
    - Trigger UI celebration notification
    - Save to database

**For detailed Milestone mechanics, see:** Saga System — Core System Specification v5.0

### **UI Notification**

**Display:** Distinct, visually impressive message in Command & Log Window

**Example Message:**

```
*************************************************
* A NEW VERSE IS WRITTEN IN YOUR SAGA!        **
* You have reached a MILESTONE.               **
* 1 Progression Point earned.                 **
*************************************************

Available PP: 3
Spend PP during Sanctuary Rest via the 'saga' command.
```

**Behavior:**

- Interrupts standard flow (prominent display)
- Golden/bronze color scheme (saga aesthetic)
- Auto-dismiss after 5 seconds or player dismissal
- Sound effect (epic chord/fanfare)
- Brief pause in gameplay for celebration moment

### **PP Storage**

**Persistence:** PP is stored permanently on character

**No Expiration:** PP never expires or degrades

**No Cap:** Characters can accumulate unlimited unspent PP

**Carries Forward:** PP persists across sessions, deaths, and resurrections

---

## III. The Golden Rule: Sanctuary Rest Requirement

### **Core Restriction**

**Specification:** Progression Points can **ONLY** be spent during a Sanctuary Rest.

**No Exceptions:** This rule is absolute and cannot be bypassed

**Validation:** System validates Sanctuary Rest state before allowing ANY PP expenditure

### **Rationale**

**Thematic Justification:**

- Character's mind must be coherent to reflect on saga
- Psychic static of ruins fragments consciousness
- Safe havens (Runic Anchors) provide "clean boot" environment
- Processing experiences requires mental clarity

**Gameplay Benefits:**

- Creates natural pacing (prevents constant micro-optimization)
- Rewards preparation (plan upgrades before expeditions)
- Makes returning to settlements exciting and rewarding
- Adds strategic depth (when to return vs. press forward)
- Prevents mid-combat respec abuse

### **Implementation**

**Access Control:**

- Saga Menu (`saga` command) only opens during Sanctuary Rest
- If player attempts to open menu outside Sanctuary: Error message displayed
- If Sanctuary Rest ends mid-menu: Spending disabled, warning displayed

**Error Message:**

```
> You cannot access the Saga Menu outside of a Sanctuary Rest.
  Your mind is too fragmented by the static to reflect on your saga.
  Find sanctuary first.
```

**Mid-Menu Warning:**

```
> WARNING: Sanctuary Rest has ended.
  You can still view the menu, but cannot spend PP until you rest again.
```

---

## IV. PP Spending Categories

Progression Points can be spent on three primary categories of upgrades, each accessible via the Saga Menu.

### **Category 1: Attributes**

**Function:** Increase the five Core Attributes (MIGHT, FINESSE, STURDINESS, WITS, WILL)

**Cost Formula:**

```csharp
PP Cost = New Attribute Level × 2
```

**Cost Examples:**

| Current Level | New Level | PP Cost |
| --- | --- | --- |
| 5 | 6 | 12 PP |
| 8 | 9 | 18 PP |
| 10 | 11 | 22 PP |
| 15 | 16 | 32 PP |
| 20 | 21 | 42 PP |

**Prerequisites:** None (attributes always available for purchase)

**Permanent:** All attribute increases are permanent

**Impact:** Increases derived stats (HP, AP, Defense, etc.) immediately upon purchase

**Design Philosophy:**

- Linear cost scaling prevents exponential power curve
- Higher costs at high levels make late-game progression meaningful
- No hard cap allows indefinite specialization
- Attributes are the primary long-term PP investment

**For detailed attribute mechanics, see:** Attributes System — Core System Specification v5.0

---

### **Category 2: Skills**

**Function:** Purchase and increase rank of four non-combat Skills

**Available Skills:**

- **System Bypass** (WITS/FINESSE-based)
- **Wasteland Survival** (WITS-based)
- **Rhetoric** (WILL-based)
- **Acrobatics** (FINESSE-based)

**Rank Progression:**

| Rank | Title | PP Cost | Total PP Invested |
| --- | --- | --- | --- |
| 0 | Untrained | 0 PP | 0 PP |
| 1 | Novice | 2 PP | 2 PP |
| 2 | Apprentice | 3 PP | 5 PP |
| 3 | Journeyman | 4 PP | 9 PP |
| 4 | Expert | 5 PP | 14 PP |
| 5 | Master | 6 PP | 20 PP |

**Prerequisites:**

- Must have previous rank to purchase next rank
- Cannot skip ranks
- Rank 0 (Untrained) available to all characters at creation

**Starting Bonus:**

- Character's chosen Background at creation provides +1 Rank bonus to one thematically appropriate skill
- This bonus does not cost PP (free starting expertise)

**Impact:**

- Each rank adds +1 die to skill check dice pools
- Higher ranks unlock more complex interactions and applications
- Rank 5 (Master) may unlock unique, skill-based abilities or options

**Design Philosophy:**

- Increasing cost rewards specialization
- Maximum 20 PP to master a single skill
- Players must choose which skills to prioritize
- Total cost to master all four skills: 80 PP (significant investment)

**For detailed skill mechanics, see:** Skill System specification (to be migrated)

---

### **Category 3: Specializations & Abilities**

**Function:** Unlock new Specializations and purchase active/passive abilities within their skill trees

**Two-Tier Structure:**

1. **Unlock Specialization** (one-time major cost)
2. **Purchase Abilities** (individual costs per ability)

### **Unlocking Specializations**

**Cost:** 10 PP (standard cost for all specializations)

**Prerequisites:**

- May require specific attribute thresholds
- May require specific skills at minimum ranks
- May require Specialization Catalyst item (for some specializations)

**Effect:**

- Grants access to specialization's skill tree
- Enables purchasing abilities from that tree
- May grant passive bonuses or unique mechanics

**One-Time Cost:** Unlocking is permanent; never need to unlock again

### **Purchasing Abilities**

**Cost Range:** 1-5 PP per ability (defined in each ability's specification)

**Cost Factors:**

- Power level of ability
- Tier within specialization tree
- Complexity of mechanics
- Frequency of use (passive vs. active)

**Typical Cost Distribution:**

- **1-2 PP:** Basic abilities, foundational skills
- **3-4 PP:** Intermediate abilities, significant power
- **5+ PP:** Advanced abilities, capstones, transformative powers

**Prerequisites:**

- Specialization must be unlocked
- May require previous abilities in tree
- May require minimum PP investment in tree (tier gates)
- May require specific attribute/skill levels

**Ability Milestones:**

- **Rank 2 (Expert):** Unlocked automatically when 20 PP spent in specialization tree
- **Rank 3 (Mastery):** Unlocked automatically when Capstone ability purchased
- All previously purchased abilities automatically upgrade (no additional PP cost)

**Design Philosophy:**

- 10 PP unlock cost creates meaningful commitment decision
- Individual ability costs allow granular customization
- Tier gates encourage depth over breadth
- Automatic ability rank upgrades reward specialization
- Total investment to fully complete one specialization: ~50-60 PP

**For detailed specialization mechanics, see:** Individual Specialization specifications

---

## V. Spending Workflow

### **Opening the Saga Menu**

**Command:** `saga` (opens Saga Menu TUI interface)

**Access Requirements:**

- Must be in Sanctuary Rest state
- If not in Sanctuary Rest: Error message, menu does not open

**Menu Structure:**

```
╔═══════════════════════════════════════════════╗
║        SAGA MENU - Character Progression      ║
╠═══════════════════════════════════════════════╣
║ Available Progression Points: 12              ║
╠═══════════════════════════════════════════════╣
║ [1] Attributes                                ║
║ [2] Skills                                    ║
║ [3] Specializations                           ║
║                                               ║
║ [Q] Exit Menu                                 ║
╚═══════════════════════════════════════════════╝
```

**Navigation:**

- Number keys to select category
- Arrow keys to navigate within category
- Enter to select upgrade
- Q to exit menu

---

### **Attribute Upgrade Flow**

**Steps:**

1. Player selects "Attributes" from main menu
2. System displays all five attributes with current values and upgrade costs
3. Player selects attribute to increase
4. System displays confirmation dialog:
    
    ```
    Increase MIGHT from 12 to 13?
    Cost: 26 PP
    You have: 12 PP available
    
    Impact:
    - +1 MIGHT
    - Damage with heavy weapons increases
    
    Confirm? [Y/N]
    ```
    
5. Player confirms
6. System validates:
    - Character has sufficient PP
    - Character is in Sanctuary Rest (re-check)
7. If valid:
    - Deduct PP cost
    - Apply attribute increase
    - Update derived stats
    - Log to Saga_Upgrades table
    - Save to database
    - Display success message
    - Refresh menu to show new values
8. If invalid:
    - Display error message
    - No changes made
    - Return to attribute selection

---

### **Skill Upgrade Flow**

**Steps:**

1. Player selects "Skills" from main menu
2. System displays four skills with current ranks and costs for next rank
3. Player selects skill to rank up
4. System displays confirmation dialog:
    
    ```
    Increase System Bypass from Rank 2 (Apprentice) to Rank 3 (Journeyman)?
    Cost: 4 PP
    You have: 12 PP available
    
    Impact:
    - +1 die to System Bypass checks
    - Unlocks Journeyman-level interactions
    
    Confirm? [Y/N]
    ```
    
5. Validation and application same as attributes

---

### **Specialization Unlock Flow**

**Steps:**

1. Player selects "Specializations" from main menu
2. System displays:
    - Unlocked specializations (expanded tree view)
    - Locked specializations (grayed out with unlock requirements)
3. Player selects locked specialization to unlock
4. System displays confirmation dialog:
    
    ```
    Unlock Specialization: Berserker?
    Cost: 10 PP
    You have: 12 PP available
    
    Prerequisites:
    ✓ MIGHT 8 or higher (you have 12)
    ✓ Warrior Archetype
    
    This grants access to the Berserker skill tree and all its abilities.
    
    Confirm? [Y/N]
    ```
    
5. Validation and application same as attributes

---

### **Ability Purchase Flow**

**Steps:**

1. Player navigates to unlocked specialization in tree view
2. System displays all abilities in tree:
    - Purchased abilities (highlighted, show current rank)
    - Available abilities (show cost and prerequisites)
    - Locked abilities (grayed out with requirements)
3. Player selects available ability to purchase
4. System displays confirmation dialog:
    
    ```
    Purchase Ability: Reckless Abandon (Berserker)?
    Cost: 3 PP
    You have: 2 PP remaining
    
    Prerequisites:
    ✓ Berserker specialization unlocked
    ✓ Fury resource system active
    
    Effect:
    [Ability description here]
    
    Confirm? [Y/N]
    ```
    
5. Validation and application same as attributes

---

### **Transaction Validation**

**All PP spending transactions must validate:**

**1. Sanctuary Rest State**

- Re-check rest state at transaction time (not just menu open)
- If rest ended: Block transaction, display warning

**2. Sufficient PP**

- `character.progression_points >= upgrade_cost`
- If insufficient: Display error with amount needed

**3. Prerequisites Met**

- Attribute requirements
- Skill requirements
- Previous abilities in tree
- Specialization unlocked
- Any special requirements
- If not met: Display list of missing prerequisites

**4. No Duplicate Purchases**

- Cannot purchase same upgrade twice
- Cannot unlock same specialization twice

**If ALL validations pass:**

- Execute transaction
- Display success message
- Remain in Saga Menu for additional purchases

**If ANY validation fails:**

- Block transaction
- Display specific error message
- No state changes
- Remain in Saga Menu

**If ANY validation fails:**

- Block transaction
- Display specific error message
- No state changes
- Remain in Saga Menu

## VI. System Integration

### **Integration with Saga System**

**Parent System:** PP System is a child component of Saga System

**Milestone Conversion:**

- Saga System monitors Legend accumulation
- At Milestone threshold: Awards 1 PP
- PP stored on PlayerCharacter
- Saga System validates PP spending requests

**For detailed integration, see:** Saga System — Core System Specification v5.0

---

### **Integration with Attributes System**

**PP → Attribute Increases:**

- SagaService.SpendProgressionPoint() validates and deducts PP
- AttributeService.IncreaseAttribute() applies increase
- StatCalculationService recalculates all derived stats
- Changes persist to database

**Cost Retrieval:**

- Saga Menu queries AttributeService for upgrade costs
- Formula: `New Level × 2 PP`
- Displays cost before confirmation

---

### **Integration with Skills System**

**PP → Skill Rank Increases:**

- SagaService validates PP and prerequisites
- SkillService.IncreaseRank() applies rank increase
- Skill check dice pools immediately reflect change

**Prerequisites:**

- Must have previous rank
- Cannot skip ranks
- Starting Background bonus applied at character creation (doesn't cost PP)

---

### **Integration with Specialization System**

**PP → Specialization Unlock:**

- 10 PP cost (standard)
- Validates attribute/skill prerequisites
- Validates Catalyst item (if required)
- Grants access to ability tree

**PP → Ability Purchase:**

- Individual PP costs per ability
- Validates tree prerequisites
- Validates tier requirements (minimum PP investment in tree)
- Adds ability to character's available abilities

**Automatic Ability Rank Upgrades:**

- System tracks total PP spent in each specialization tree
- At 20 PP: All abilities in tree upgrade to Rank 2 (Expert) automatically
- At Capstone purchase: All abilities upgrade to Rank 3 (Mastery) automatically
- No additional PP cost for rank upgrades

---

### **Integration with Sanctuary Rest System**

**Golden Rule Enforcement:**

- Sanctuary Rest System provides `IsSanctuaryRest()` method
- Saga Menu queries this before opening
- SagaService re-validates before each PP spend
- If rest ends mid-menu: Disable spending, display warning

**Rest State Changes:**

- Sanctuary Rest starts: Saga Menu becomes accessible
- Sanctuary Rest ends: Saga Menu becomes view-only (no spending)

---

### **Integration with Character Creation**

**Starting State:**

- All characters start with **0 PP**
- All characters start at **Milestone 0**
- Background grants +1 Rank to one skill (free, doesn't cost PP)
- No other starting PP bonuses

---

### **Integration with Death & Resurrection**

**PP Persistence:**

- PP is NOT lost on death
- PP carries forward through resurrection
- Unspent PP remains available after [Frayed Echo] debuff expires

**Rationale:**

- PP represents internalized learning, not temporary state
- Death already has significant penalties (Legend gain reduction)
- Losing PP would be doubly punishing and feel unfair

---

### **Integration with Trauma Economy**

**No Direct Integration:**

- PP spending is not affected by Psychic Stress
- PP spending is not affected by Runic Blight Corruption
- PP is not a resource that can be corrupted or degraded

**Indirect Integration:**

- [Systemic Apathy] Trauma reduces Legend gain (-25%), slowing PP acquisition
- High Psychic Stress reduces combat effectiveness, slowing Legend gain
- Trauma Economy affects the *rate* of PP gain, not the spending mechanics

---

## VII. Balancing Considerations

### **Player Agency vs. Optimization**

**Design Goal:** Every choice should feel meaningful and permanent

**Mechanisms:**

- No easy respec option (choices are who you become)
- Diverse viable builds (no single "correct" path)
- All three spending categories valuable (attributes, skills, specializations)

**Preventing Analysis Paralysis:**

- Clear cost formulas (predictable progression)
- Ability previews (know what you're buying)
- No trap choices (all options viable)

---

### **Pacing**

**Early Game (Milestones 0-10):**

- PP comes frequently (500-1,500 Legend per Milestone)
- Players unlock first specialization quickly (~Milestone 10)
- Rapid power growth feels rewarding

**Mid Game (Milestones 10-30):**

- PP rate slows (1,500-3,500 Legend per Milestone)
- Players balance attribute increases with ability purchases
- Specialization trees deepen

**Late Game (Milestones 30+):**

- PP rate very slow (3,500+ Legend per Milestone)
- Each PP is precious, choices feel critical
- Attributes become primary PP sink

**Tuning Knobs:**

- Milestone formula (currently 100n + 500)
- Attribute cost formula (currently 2n)
- Skill rank costs (currently 2, 3, 4, 5, 6)
- Specialization unlock cost (currently 10 PP)

---

### **Breadth vs. Depth**

**Design Incentives:**

- Specialization automatic upgrades reward depth (20 PP → Rank 2, Capstone → Rank 3)
- High attribute costs at high levels reward spreading points
- Total cost to master one specialization: ~50-60 PP
- Total cost to master all attributes to 20: Prohibitively expensive (~800+ PP)

**Expected Player Behavior:**

- Early: Unlock 1-2 specializations, purchase key abilities
- Mid: Balance specialization depth with attribute increases
- Late: Primarily invest in attributes for incremental power

**Prevents:**

- "One-point wonder" builds (spreading too thin)
- "God build" syndrome (maxing everything impossible)
- Stagnant progression (always something valuable to buy)

---

### **Scarcity**

**Design Philosophy:** PP should always feel valuable

**Mechanisms:**

- 1 PP per Milestone (no other sources)
- Milestone requirements increase linearly
- Late-game Milestones take substantial real-world time
- Creates meaningful decisions (opportunity cost)

**Prevents:**

- PP inflation (devaluing the currency)
- Trivial choices (everything feels free)
- Maxed-out characters too early

---

## VIII. The Respec Question

### **Current Design: No Easy Respec**

**Philosophy:** Choices define who you become

**Rationale:**

- Permanent choices create meaningful character identity
- Respec trivializes progression decisions
- "Optimal build" mentality undermines roleplay
- Players who commit to a path should be rewarded, not penalized

**Player Concerns:**

- "What if I make a mistake?"
- "What if I want to try a different build?"
- "What if the meta changes?"

---

### **Potential Future Respec Options**

If player demand is overwhelming, respec could be added with significant constraints:

**Option 1: Saga Rewrite Consumable**

- Ultra-rare, expensive consumable item
- Obtained through legendary Saga Quests
- One-time use
- Refunds ALL spent PP, resets character to Milestone 0 equivalent
- Narrative: Character "rewrites their saga" through profound ritual

**Option 2: Partial Respec (Last N Purchases)**

- Allow refunding last 3-5 PP purchases only
- High cost (e.g., 10 PP to refund 5 PP of purchases)
- Represents "recent learning" being unlearned
- Cannot refund purchases made more than X milestones ago

**Option 3: Specialization-Specific Reset**

- Reset one specialization tree only
- Cost: 20 PP
- Refunds all PP spent in that tree
- Locks tree for X milestones (cooldown)
- Narrative: "Unlearning" a combat style to master another

**If Implemented:**

- Must be costly (preserve weight of decisions)
- Must have narrative justification (not just mechanical convenience)
- Should not trivialize progression
- Consider limiting uses (e.g., once per character)

**Current Recommendation:** Wait for post-launch player feedback before implementing any respec

---

## IX. Implementation Notes

### **Database Schema**

**Characters Table (PP Fields):**

```sql
ALTER TABLE Characters ADD COLUMN (
  progression_points INT NOT NULL DEFAULT 0,
  
  CHECK (progression_points >= 0)
);
```

**Saga_Upgrades Table (Transaction History):**

```sql
CREATE TABLE Saga_Upgrades (
  upgrade_id INT PRIMARY KEY AUTO_INCREMENT,
  character_id INT NOT NULL,
  upgrade_type ENUM('attribute', 'skill', 'specialization', 'ability') NOT NULL,
  upgrade_target VARCHAR(100) NOT NULL,
  pp_cost INT NOT NULL,
  milestone_level_at_purchase INT NOT NULL,
  purchased_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  
  FOREIGN KEY (character_id) REFERENCES Characters(character_id)
    ON DELETE CASCADE,
  
  INDEX idx_character_history (character_id, purchased_at)
);
```

**Purpose:** Tracks every PP spending transaction for debugging, analytics, and potential future respec

---

### **Service Methods**

**SagaService.cs:**

**`SpendProgressionPoint(PlayerCharacter character, IUpgradeable target)`**

- **Inputs:** Character and upgrade target
- **Validations:**
    - Sanctuary Rest state
    - Sufficient PP
    - Prerequisites met
    - No duplicate purchases
- **Process:**
    - Deduct PP from character.progression_points
    - Apply upgrade via target.ApplyUpgrade(character)
    - Log to Saga_Upgrades table
    - Save to database
- **Outputs:** Boolean success, updated character
- **Error Handling:** Throws specific exceptions for each validation failure

**`CanAfford(PlayerCharacter character, int cost)`**

- **Inputs:** Character and PP cost
- **Outputs:** Boolean (can afford?)
- **Usage:** UI pre-validation (gray out unaffordable options)

**`GetSpendingHistory(PlayerCharacter character)`**

- **Inputs:** Character
- **Outputs:** List of Saga_Upgrades records
- **Usage:** Player can review past purchases

---

### **UI Display Requirements**

**Available PP Counter:**

- Prominently displayed in Saga Menu header
- Updates in real-time after each purchase
- Shows current PP / total PP earned (optional)

**Cost Display:**

- Always show PP cost before purchase
- Color-code affordability (green = can afford, red = cannot)
- Show PP remaining after purchase

**Confirmation Dialogs:**

- Required for ALL PP spending
- Show upgrade details
- Show cost and impact
- Require explicit confirmation
- Prevent accidental purchases

**Transaction Feedback:**

- Success message with upgrade name
- Updated PP counter
- Visual effect (flash, glow) on upgraded element
- Remain in menu for additional purchases

---

### **Testing Checklist**

**Unit Tests:**

- [ ]  PP awarded correctly at Milestone
- [ ]  PP persists across sessions
- [ ]  PP NOT lost on death
- [ ]  Sanctuary Rest validation blocks spending outside rest
- [ ]  Attribute cost calculation correct for all levels
- [ ]  Skill rank cost calculation correct for all ranks
- [ ]  Specialization unlock costs 10 PP
- [ ]  Ability purchase validates prerequisites
- [ ]  Automatic ability rank upgrades at 20 PP and Capstone
- [ ]  Cannot spend PP on duplicate purchases

**Integration Tests:**

- [ ]  Milestone → PP award → Database persistence
- [ ]  PP spend → Attribute increase → Derived stat recalculation
- [ ]  PP spend → Skill rank increase → Dice pool update
- [ ]  PP spend → Specialization unlock → Ability tree accessible
- [ ]  PP spend → Ability purchase → Ability usable in combat
- [ ]  Saga_Upgrades table correctly logs all transactions

**UI Tests:**

- [ ]  Saga Menu blocked outside Sanctuary Rest
- [ ]  Available PP displayed correctly
- [ ]  Upgrade costs displayed correctly
- [ ]  Affordable upgrades color-coded green
- [ ]  Unaffordable upgrades color-coded red
- [ ]  Confirmation dialog prevents accidental purchases
- [ ]  Success message displays after purchase
- [ ]  Menu refreshes to show new values

**Balancing Tests:**

- [ ]  Early game PP rate feels rewarding
- [ ]  Late game PP rate feels meaningful
- [ ]  Attribute costs scale appropriately
- [ ]  Specialization depth incentivized
- [ ]  No dominant "must-buy" upgrades

---

## X. Design Philosophy Summary

**PP as Earned Power:**

Progression Points represent the *power* earned through *experience*. The separation of Legend (earning) and PP (spending) creates a progression loop with natural highs and lows:

**Low Points:** Grinding through challenges, slowly filling Legend bar

**High Points:** Milestone reached! 1 PP earned!

**Anticipation:** Returning to sanctuary to spend PP

**Satisfaction:** Choosing and applying new power

This rhythm prevents "always leveling" fatigue and makes each Milestone feel like a genuine achievement.

**The Sanctuary Constraint:**

Requiring Sanctuary Rest for spending is not just a thematic choice—it's a **pacing mechanism** that:

- Prevents constant reoptimization
- Creates strategic tension (when to return?)
- Makes settlements feel rewarding and safe
- Reinforces core theme: coherence enables growth

**The Permanence Philosophy:**

No easy respec reinforces that **your choices are who you become**. In a game about coherence vs. chaos, maintaining a coherent character identity matters. Players should not be able to constantly rewrite their saga based on the latest optimal build guide.

**Three Spending Paths:**

The Attributes / Skills / Specializations split ensures:

- **Immediate power:** Specialization abilities provide new mechanics
- **Long-term scaling:** Attributes provide incremental power
- **Utility growth:** Skills provide non-combat capabilities

All three categories remain valuable throughout the entire game, preventing "solved" progression where one path dominates.

---

## Migration Complete

This specification has been fully migrated from v2.0 PP System specification[[1]](https://www.notion.so/Feature-Specification-The-Progression-Point-PP-System-2a355eb312da80dd849ecc1fe7fc8e13?pvs=21) and enhanced with DB10 structure requirements. All sections now reflect Aethelgard's unique PP spending mechanics, the Golden Rule (Sanctuary Rest requirement), spending categories and costs, transaction workflows, and design philosophy.

**Status:** Draft → Ready for Review

**Next Steps:**

1. Review for consistency with Saga System integration points
2. Validate cost formulas match balance goals
3. Implement Saga Menu UI (see UI Requirements section)
4. Update status to Final after review

**Related Specifications:**

- Saga System — Core System Specification v5.0 (parent system)
- Legend System — Core System Specification v5.0 (PP acquisition via Milestones)
- Attributes System — Core System Specification v5.0 (PP spending category 1)
- Skills System specification (PP spending category 2, to be migrated)
- Individual Specialization specifications (PP spending category 3)

---