# Legend System — Core System Specification v5.0

Status: Proposed
Balance Validated: No
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

## I. Core Philosophy: The Measure of a Saga

**Legend** is the in-world term for experience points in Aethelgard. It is not an abstract measure of "power" or "kills," but a direct representation of the **narrative weight of a character's saga**—the sum of their heroic deeds, traumatic discoveries, masterful creations, and victories against the encroaching chaos of a broken world.

**Design Pillars:**

- **Narrative Diegesis:** Legend is understood in-world as the worthiness of a character's story
- **Thematic Reward:** System rewards engagement with core themes (coherence, Trauma Economy, saga-worthy acts)
- **Anti-Grinding:** Trivial encounters award zero Legend
- **Cooperative Focus:** Shared Legend for party activities encourages cooperation
- **Trauma Integration:** Higher psychic risk yields higher Legend rewards

**The Legend → Milestone → PP Loop:**

Legend serves as the **passive accumulation currency** in Aethelgard's progression system:

1. Characters perform saga-worthy deeds → Earn Legend (passive resource)
2. Legend fills progression bar → Reach Milestone threshold
3. Milestone event → Award 1 Progression Point (active spendable currency)
4. PP spent during Sanctuary Rest → Character upgrades (attributes, abilities)

**Transformation Narrative:**

The accumulation of Legend is the process of a character transforming from a simple survivor into a figure of legend—someone whose deeds are worthy of being chronicled, remembered, and sung about in the halls of the Last Stronghold.

---

*Migration in progress from v2.0 Legend System specification. Remaining sections to be added incrementally.*

The accumulation of Legend is the process of a character transforming from a simple survivor into a figure of legend—someone whose deeds are worthy of being chronicled, remembered, and sung about in the halls of the Last Stronghold.

## II. Legend Accrual: The Universal Formula

Legend is a passive resource that is automatically awarded to characters upon completion of a "saga-worthy" action. All Legend awards are calculated using a single, flexible formula that ensures fairness, balance, and thematic consistency.

### **The Universal Legend Formula**

```csharp
Legend Awarded = (Base Legend Value × Difficulty Modifier) × Trauma Modifier
```

This three-component formula scales rewards appropriately based on:

1. The inherent narrative importance of the action
2. The challenge level relative to party power
3. The psychic/spiritual risk taken

---

### **Component 1: Base Legend Value (BLV)**

**Purpose:** Represents the inherent narrative weight and importance of the action itself

**Value Categories:**

**Minor Act (BLV: 10-20)**

- Small but notable moments
- Examples:
    - Critical hit in combat
    - Standard successful skill check
    - Discovering a hidden cache
    - Completing a minor environmental puzzle

**Significant Act (BLV: 50-100)**

- Standard, meaningful accomplishments
- Examples:
    - Defeating a Champion enemy
    - Completing a minor Faction Quest
    - Crafting a [Masterwork] item
    - Successfully negotiating a diplomatic resolution
    - Solving a complex multi-stage puzzle

**Major Deed (BLV: 200-500+)**

- Truly legendary, saga-defining moments
- Examples:
    - Defeating a Boss enemy
    - Completing a Saga Quest
    - Discovering a new realm or major location
    - Creating a [Legendary] artifact
    - Preventing a catastrophic Blight incursion

**Implementation:**

- BLV for every rewardable action stored in dedicated database table (`LegendValues`)
- Organized by action type (combat, quest, crafting, exploration, social)
- Regularly tuned based on playtesting data

**Design Philosophy:**

- BLV represents the "this is worth telling a story about" factor
- Trivial or routine actions have BLV of 0
- Acts must have narrative or mechanical significance to award Legend

---

### **Component 2: Difficulty Modifier (DM)**

**Purpose:** Scales reward based on challenge relative to party's current power level

**Integration:** Directly tied to the **Target Difficulty Rating (TDR)** system

**Modifier Values:**

**Trivial Challenge (DM: 0×)**

- TDR significantly below party's Power Score
- **No Legend awarded** (anti-grinding mechanic)
- Example: Level 10 party fighting level 3 enemies
- Prevents farming low-level content for progression

**Standard Challenge (DM: 1.0×)**

- TDR matched to party's Power Score
- Fair, appropriate challenge for party level
- Baseline reward expectation

**Difficult Challenge (DM: 1.25×)**

- TDR moderately above party's Power Score
- Challenging boss fight or high-DC puzzle
- Rewards taking calculated risks

**Overwhelming Challenge (DM: 1.5× - 2.0×)**

- TDR significantly above party's Power Score
- Epic, multi-stage boss or Saga Quest finale
- Highest rewards for most dangerous encounters
- Maximum 2.0× cap prevents exploit incentives

**Implementation:**

- `LegendService.cs` queries `DifficultyService.cs` for encounter TDR
- Compares TDR against party's current Power Score
- Applies appropriate multiplier automatically
- DM calculated at time of reward, not encounter start (prevents power-leveling exploits)

**Edge Cases:**

- If party composition changes mid-encounter, use average Power Score at reward time
- Solo players receive full group TDR calculation (no penalty for playing alone)
- If TDR cannot be determined (scripted events), defaults to 1.0×

---

### **Component 3: Trauma Modifier (TM)**

**Purpose:** Directly rewards players for engaging with the Trauma Economy and taking psychic/spiritual risks

**Core Design Principle:** Higher risk of mental/spiritual harm = higher Legend reward

**Modifier Values:**

**Coherent Act (TM: 1.0×)**

- Action involved no direct interface with the Blight
- Standard combat, exploration, crafting in safe areas
- No psychic risk taken
- Examples:
    - Fighting corrupted wildlife (physical threat only)
    - Crafting in sanctuary workshop
    - Exploring non-Blighted ruins

**Taxing Act (TM: 1.25×)**

- Action involved minor but necessary interface with the Blight
- Moderate psychic risk
- Examples:
    - Fighting in [Psychic Resonance] zone
    - Using abilities with Psychic Stress costs
    - Interacting with mildly corrupted artifacts
    - Exploring actively glitching environments

**Traumatic Act (TM: 1.5×)**

- Significant, high-risk psychic/spiritual interface
- Examples:
    - Seiðkona's Forlorn Communion ability (commune with Forlorn)
    - Jötun-Reader's deep analysis of major reality glitch
    - Voluntary exposure to [Trauma Surge] effects
    - Completing quests requiring direct Blight interaction

**Heretical Act (TM: 2.0×)**

- Acts of profound self-corruption for the "greater good"
- **Permanent** consequences (Runic Blight Corruption gain, permanent Trauma)
- Examples:
    - Quest solutions requiring permanent Corruption gain
    - Forbidden rituals that permanently alter character
    - Sacrificing coherence to achieve critical objectives
    - Using Rust-Witch's most dangerous heretical spells

**Implementation:**

- `LegendService.cs` receives `ETraumaContext` enum parameter with each award
- Trauma context determined by:
    - Environmental zone properties
    - Ability/spell metadata (Stress costs, Corruption risks)
    - Quest design flags
    - GM discretion for edge cases
- Multiple trauma sources in single encounter: Use highest applicable TM

**Balancing Note:**

- TM creates incentive to engage with dangerous content
- Encourages risk-taking while maintaining meaningful consequences
- Players can avoid high TM content but progress slower
- Creates strategic tension: "Is the extra Legend worth the Stress/Corruption?"

---

### **Formula Examples**

**Example 1: Standard Boss Fight**

- Base Legend Value: 300 (Major Deed - Boss)
- Difficulty Modifier: 1.0× (Standard Challenge)
- Trauma Modifier: 1.25× (Taxing - fought in Psychic Resonance zone)
- **Legend Awarded: 300 × 1.0 × 1.25 = 375 Legend**

**Example 2: Overwhelming Heretical Quest**

- Base Legend Value: 500 (Major Deed - Saga Quest completion)
- Difficulty Modifier: 1.5× (Overwhelming Challenge)
- Trauma Modifier: 2.0× (Heretical - permanent Corruption gain required)
- **Legend Awarded: 500 × 1.5 × 2.0 = 1,500 Legend**

**Example 3: Trivial Encounter (Grinding Attempt)**

- Base Legend Value: 50 (Significant Act - Champion enemy)
- Difficulty Modifier: 0× (Trivial - party overpowered)
- Trauma Modifier: 1.0× (Coherent)
- **Legend Awarded: 50 × 0 × 1.0 = 0 Legend**

**Example 4: Masterwork Crafting**

- Base Legend Value: 75 (Significant Act - Masterwork craft)
- Difficulty Modifier: 1.0× (Standard Challenge - appropriate difficulty check)
- Trauma Modifier: 1.0× (Coherent - safe workshop)
- **Legend Awarded: 75 × 1.0 × 1.0 = 75 Legend**
- **Legend Awarded: 75 × 1.0 × 1.0 = 75 Legend**

## III. Progression Curve: The Milestone System

Legend is a **passive resource** that cannot be spent directly. Instead, it accumulates until reaching a **Milestone threshold**, at which point it converts into a **Progression Point (PP)**, the active spendable currency.

### **Milestone Formula**

```csharp
LegendToNextMilestone = (CurrentMilestoneLevel × 100) + 500
```

**Variable Definitions:**

- **CurrentMilestoneLevel:** Integer count of Milestones reached (starts at 0)
- **LegendToNextMilestone:** Legend threshold required to reach next Milestone
- **Base Value:** 500 (ensures meaningful first milestone)
- **Scaling Factor:** 100 per milestone level (linear progression)

### **Progression Examples**

| Current Milestone | Next Milestone | Legend Required |
| --- | --- | --- |
| 0 → 1 | 1 | 500 |
| 1 → 2 | 2 | 600 |
| 5 → 6 | 6 | 1,000 |
| 10 → 11 | 11 | 1,500 |
| 20 → 21 | 21 | 2,500 |
| 50 → 51 | 51 | 5,500 |

### **Legend Overflow Handling**

**Critical Implementation Requirement:** When Legend exceeds threshold, the **overflow** must be preserved.

**Correct Implementation:**

```csharp
if (character.legend_points >= character.legend_to_next_milestone)
{
  int overflow = character.legend_points - character.legend_to_next_milestone;
  character.legend_points = overflow; // Preserve excess Legend
  character.progression_points += 1;
  character.current_milestone_level += 1;
  character.legend_to_next_milestone = CalculateNextMilestone(character.current_milestone_level);
}
```

**Incorrect Implementation:**

```csharp
// WRONG - Do not zero out Legend entirely
character.legend_points = 0; // Loses overflow
```

**Example:**

- Character at Milestone 5 (needs 1,000 Legend for Milestone 6)
- Currently has 850 Legend
- Earns 300 Legend from boss fight
- Total: 1,150 Legend
- **Correct:** Milestone 6 reached, 150 Legend carried forward
- **Wrong:** Milestone 6 reached, 0 Legend (player loses 150 Legend)

### **Multiple Milestone Triggers**

**Scenario:** Player earns enough Legend to cross multiple Milestone thresholds in one reward

**Implementation:**

```csharp
while (character.legend_points >= character.legend_to_next_milestone)
{
  TriggerMilestone(character);
}
```

**Example:**

- Character at Milestone 0 (needs 500 for Milestone 1)
- Has 200 Legend
- Completes overwhelming heretical quest: 1,500 Legend awarded
- Total: 1,700 Legend
- **Result:**
    - Milestone 1 reached: 1,700 - 500 = 1,200 remaining
    - Milestone 2 reached: 1,200 - 600 = 600 remaining
    - Milestone 3 reached: 600 - 700 = Not enough (stops)
    - Final state: Milestone 2, 600 Legend toward Milestone 3 (needs 700 total)

**Design Intent:**

- Large Legend awards feel immediately rewarding
- No "wasted" Legend from overflow
- Encourages high-risk, high-reward gameplay
- Prevents feel-bad moments where players "just miss" multiple milestones

### **Milestone Event**

When a Milestone is reached:

**Immediate Effects:**

1. Award **1 Progression Point (PP)**
2. Subtract Legend threshold from current Legend
3. Increment Milestone level
4. Recalculate next Milestone threshold
5. Trigger UI celebration notification
6. Save all progression data to database

**No Immediate Power Gain:**

- Milestone grants PP but does NOT automatically improve character
- PP must be spent during Sanctuary Rest via Saga Menu
- This creates deliberate, meaningful progression moments
- Reinforces the "rest and reflect" narrative theme

---

## IV. Party Dynamics & Legend Distribution

### **Shared Saga Philosophy**

**Core Principle:** When Legend is awarded for group activities, **every party member present receives the full amount**.

**Rationale:**

- Encourages cooperative play
- Prevents "kill-stealing" behavior
- Rewards support roles equally (healers, buffers, crowd control)
- Simplifies bookkeeping (no complex contribution calculations)
- Thematically appropriate: Shared deeds become shared saga

### **Group Activity Awards**

**Full Shared Legend for:**

- Defeating enemies in party combat
- Completing quests as a party
- Solving puzzles collaboratively
- Group exploration discoveries
- Party-wide diplomatic successes

**Example:**

- 4-person party defeats Boss (300 BLV × 1.0 DM × 1.25 TM = 375 Legend)
- **Each** party member receives 375 Legend
- Total Legend distributed: 1,500 (375 × 4)
- No splitting, no dilution

### **Individual Activity Awards**

**Solo Legend for:**

- Critical success on solo skill check (player separated from party)
- Individual crafting projects
- Solo exploration discoveries (while party is elsewhere)
- Personal character moment achievements

**Example:**

- Scrap-Tinker crafting Masterwork item alone in workshop (75 Legend)
- Only Scrap-Tinker receives 75 Legend
- Party members elsewhere receive nothing

### **Partial Party Presence**

**Scenario:** Not all party members present for Legend-awarding event

**Rule:** Only characters present at time of reward receive Legend

**Example:**

- 4-person party splits up
- 2 members defeat Champion enemy (50 BLV = 50 Legend)
- Only those 2 members receive 50 Legend each
- Other 2 members receive nothing

**Rationale:**

- Encourages staying together for major encounters
- Rewards tactical splitting when appropriate
- Simple "present = reward" rule prevents disputes

### **Death & Legend Awards**

**Dead Character During Reward:**

- If character dies during encounter but party wins, character still receives Legend
- Dead characters participated in the saga-worthy deed
- Prevents feel-bad moment of missing reward due to death

**Dead Character Before Reward:**

- If character already dead when party begins encounter, no Legend
- Character did not participate in deed

**Dead Character Before Reward:**

- If character already dead when party begins encounter, no Legend
- Character did not participate in deed

## V. Implementation Details

### **Service Architecture**

**LegendService.cs (Primary Service)**

**Responsibilities:**

- Sole authority for awarding Legend
- Calculates final Legend amount using Universal Formula
- Distributes Legend to appropriate party members
- Interfaces with SagaService for Milestone triggers
- Logs all Legend awards for debugging and analytics

**Key Methods:**

**`AwardLegend(Party party, ERewardType rewardType, EDifficulty difficulty, ETraumaContext traumaContext)`**

- **Inputs:**
    - `party` (Party): Party object containing all characters present
    - `rewardType` (ERewardType): Type of action (combat, quest, crafting, exploration, social)
    - `difficulty` (EDifficulty): Difficulty rating (Trivial, Standard, Difficult, Overwhelming)
    - `traumaContext` (ETraumaContext): Trauma context (Coherent, Taxing, Traumatic, Heretical)
- **Process:**
    1. Query LegendValues database table for Base Legend Value (BLV) based on rewardType
    2. Query DifficultyService for Difficulty Modifier (DM) based on difficulty vs. party Power Score
    3. Determine Trauma Modifier (TM) from traumaContext enum
    4. Calculate: `finalLegend = (BLV × DM) × TM`
    5. If finalLegend > 0, distribute to all present party members
    6. For each character receiving Legend: call SagaService.AddLegend(character, finalLegend)
    7. Log award to LegendAwards table
    8. Trigger UI notification
- **Outputs:** void (side effects: updates character Legend, triggers UI)
- **Validation:**
    - If DM = 0 (Trivial), short-circuit and award 0 Legend
    - If party is empty, log error and return
    - If BLV lookup fails, log error and use default value

**`AwardIndividualLegend(PlayerCharacter character, ERewardType rewardType, EDifficulty difficulty, ETraumaContext traumaContext)`**

- **Inputs:** Same as AwardLegend but for single character
- **Process:** Same calculation logic, awards to single character only
- **Usage:** Solo skill checks, individual crafting, personal achievements

**`GetBaseLegendValue(ERewardType rewardType)`**

- **Inputs:** `rewardType` (ERewardType)
- **Process:** Query LegendValues database table
- **Outputs:** int (Base Legend Value)
- **Caching:** Results cached for performance (rarely changes)

### **Database Schema**

**LegendValues Table (Configuration)**

```sql
CREATE TABLE LegendValues (
  reward_id INT PRIMARY KEY AUTO_INCREMENT,
  reward_type ENUM('combat_minor', 'combat_champion', 'combat_boss', 
                   'quest_minor', 'quest_saga', 'crafting_masterwork', 
                   'crafting_legendary', 'exploration_discovery', 
                   'social_resolution', 'puzzle_complex') NOT NULL,
  base_legend_value INT NOT NULL,
  description VARCHAR(255),
  
  UNIQUE KEY (reward_type)
);
```

**Purpose:** Stores BLV for each rewardable action type

**LegendAwards Table (History/Analytics)**

```sql
CREATE TABLE LegendAwards (
  award_id INT PRIMARY KEY AUTO_INCREMENT,
  character_id INT NOT NULL,
  reward_type VARCHAR(50) NOT NULL,
  base_legend_value INT NOT NULL,
  difficulty_modifier DECIMAL(3,2) NOT NULL,
  trauma_modifier DECIMAL(3,2) NOT NULL,
  final_legend_awarded INT NOT NULL,
  awarded_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  
  FOREIGN KEY (character_id) REFERENCES Characters(character_id)
    ON DELETE CASCADE,
  
  INDEX idx_character_awards (character_id, awarded_at)
);
```

**Purpose:** Logs every Legend award for debugging, analytics, and anti-cheat detection

### **Integration with SagaService**

**Flow:**

1. LegendService calculates and awards Legend to character
2. LegendService calls `SagaService.AddLegend(character, amount)`
3. SagaService adds Legend to character's legend_points
4. SagaService checks if legend_points >= legend_to_next_milestone
5. If yes: SagaService.TriggerMilestone(character) (see Saga System spec)
6. Milestone awards PP, recalculates threshold, triggers UI celebration

**Clear Separation of Concerns:**

- LegendService: Calculates and distributes Legend based on deeds
- SagaService: Manages Legend accumulation and Milestone conversion

### **Enums**

**ERewardType:**

```csharp
public enum ERewardType
{
  CombatMinor,          // Minor enemy, critical hit
  CombatChampion,       // Champion enemy
  CombatBoss,           // Boss enemy
  QuestMinor,           // Minor faction quest
  QuestSaga,            // Major saga quest
  CraftingMasterwork,   // Masterwork item
  CraftingLegendary,    // Legendary artifact
  ExplorationDiscovery, // Major discovery
  SocialResolution,     // Diplomatic success
  PuzzleComplex         // Multi-stage puzzle
}
```

**EDifficulty:**

```csharp
public enum EDifficulty
{
  Trivial,      // DM: 0×
  Standard,     // DM: 1.0×
  Difficult,    // DM: 1.25×
  Overwhelming  // DM: 1.5× - 2.0×
}
```

**ETraumaContext:**

```csharp
public enum ETraumaContext
{
  Coherent,   // TM: 1.0×
  Taxing,     // TM: 1.25×
  Traumatic,  // TM: 1.5×
  Heretical   // TM: 2.0×
}
```

---

## VI. UI Requirements

### **Legend Progress Display**

**Location:** Character sheet or Saga menu screen

**Components:**

- Progress bar visualization showing Legend / Next Milestone
- Numeric display: "750 / 1,500 Legend"
- Current Milestone level displayed
- Real-time updates when Legend awarded

**Example Display:**

```
━━━━━━━━━━━░░░░░░░░░░ 750 / 1,500 Legend
Milestone 10 → 11
```

**Behavior:**

- Smooth fill animation when Legend added
- Glow/pulse effect when approaching Milestone (>90% filled)
- Satisfying visual feedback for progression

### **Legend Award Notifications**

**Location:** Command & Log Window (CSI)

**Standard Legend Award:**

- Clear, thematic message
- Display exact Legend amount
- Brief description of source

**Examples:**

```
> Your victory over the Corrupted Warden is a tale worth telling. 
  You have earned 375 Legend.

> The mechanism yields to your expertise. 
  You have earned 75 Legend.

> You have stared into the abyss of a corrupted mind and emerged with knowledge. 
  The saga grows darker. You have earned 187 Legend.
```

**Message Tone:**

- Thematic and narrative
- Varies by Trauma Context (Heretical acts have darker, more ominous messages)
- Celebrates achievement while maintaining world tone

### **Milestone Notification**

**Display:**

- More significant, celebratory message
- Prominent visual effect (screen flash, particle effects)
- Clear indication of PP awarded
- Reminder about Saga Menu access

**Example:**

```
>> A NEW CHAPTER OF YOUR SAGA IS WRITTEN <<

You have reached MILESTONE 11 and earned a Progression Point!

Available PP: 3
Spend PP during Sanctuary Rest in the Saga Menu.
```

**Behavior:**

- Modal or prominent banner (requires dismissal or auto-dismiss after 5 seconds)
- Golden/bronze color scheme (saga/chronicle aesthetic)
- Sound effect (epic chord, fanfare)
- Brief pause in gameplay for celebration moment

### **Party-Wide Legend Awards**

**Display:**

- Single notification for party awards
- Lists all characters receiving Legend
- Indicates shared reward

**Example:**

```
> The party's triumph over the Forlorn Horror echoes through the ruins.
  Each party member has earned 500 Legend.
  
  - Thorvald: 500 Legend
  - Sigrun: 500 Legend
  - Bjorn: 500 Legend
  - Astrid: 500 Legend
```

---

## VII. System Integration

### **Integration with Core Gameplay Loop**

**The Adventure → Rest → Progression Cycle:**

```
1. Adventure Phase
   ↓
2. Perform saga-worthy deeds
   ↓
3. LegendService awards Legend
   ↓
4. SagaService accumulates Legend
   ↓
5. Milestone threshold reached
   ↓
6. Award Progression Point (PP)
   ↓
7. Return to Sanctuary Rest
   ↓
8. Spend PP in Saga Menu
   ↓
9. Character improves (attributes, abilities)
   ↓
10. Resume Adventure Phase
```

**Key Design Principle:** Legend → Milestone → PP → Sanctuary Rest → Saga Menu spending

This enforces deliberate, meaningful progression moments rather than constant incremental drip.

---

### **Integration with Character Creation**

**Starting State:**

- All characters begin with **0 Legend**
- All characters begin with **0 Progression Points**
- All characters begin at **Milestone 0**
- LegendToNextMilestone: 500 (first milestone threshold)

**Narrative:** Character's saga is an unwritten page, ready to be filled with deeds.

---

### **Integration with Death & Resurrection System**

**[Frayed Echo] Debuff:**

- Applied when character is resurrected
- **Effect:** -50% Legend gain for 24 hours (in-game time)
- **Implementation:** LegendService multiplies final Legend by 0.5 if debuff present
- **Rationale:** Death is a significant setback to character progression

**Formula with Frayed Echo:**

```csharp
finalLegend = ((BLV × DM) × TM) × 0.5  // If [Frayed Echo] active
```

**Example:**

- Boss defeated: 375 Legend normally
- With [Frayed Echo]: 375 × 0.5 = 187 Legend
- Player loses 188 Legend due to recent death

---

### **Integration with Trauma Economy**

**[Systemic Apathy] Trauma:**

- Permanent Trauma that can be gained at 100 Psychic Stress
- **Effect:** -25% Legend gain (permanent until cured)
- **Implementation:** LegendService multiplies final Legend by 0.75 if Trauma present
- **Rationale:** Character's mental state affects their ability to write a compelling saga

**Stacking with Frayed Echo:**

```csharp
float modifier = 1.0f;
if (hasSystemicApathy) modifier *= 0.75f;
if (hasFrayedEcho) modifier *= 0.5f;
finalLegend = ((BLV × DM) × TM) × modifier;
```

**Example:**

- Boss defeated: 375 Legend normally
- With both debuffs: 375 × 0.75 × 0.5 = 140 Legend
- Player loses 235 Legend due to death + mental trauma

**Thematic Resonance:**

- System reinforces Trauma Economy consequences
- Mental health directly impacts character growth
- Creates strategic tension around risk management

---

### **Integration with Difficulty System**

**Target Difficulty Rating (TDR):**

- Every encounter, puzzle, and challenge has a TDR value
- TDR represents the "level" or power requirement of the content
- LegendService compares TDR to party's Power Score

**Power Score Calculation:**

- Average of party's Milestone levels + attribute totals + gear quality
- Calculated by DifficultyService
- Used to determine appropriate challenges

**Dynamic Difficulty Modifier:**

```csharp
float CalculateDifficultyModifier(int tdr, int partyPowerScore)
{
  int difference = tdr - partyPowerScore;
  
  if (difference < -5) return 0.0f;    // Trivial
  if (difference >= -5 && difference <= 0) return 1.0f;  // Standard
  if (difference >= 1 && difference <= 3) return 1.25f;  // Difficult
  if (difference >= 4) return 1.5f + (difference - 4) * 0.1f;  // Overwhelming (caps at 2.0)
}
```

---

### **Integration with Quest System**

**Quest Completion Awards:**

- Each quest has defined Legend award
- Quest designers set BLV in quest metadata
- Trauma Context determined by quest content
- Difficulty Modifier based on quest TDR vs. party level

**Quest Types:**

- **Minor Faction Quests:** BLV 50-100, typically Standard difficulty
- **Major Saga Quests:** BLV 300-500+, typically Difficult/Overwhelming
- **Heretical Quests:** High BLV with Heretical Trauma Context (TM: 2.0×)

**Implementation:**

- QuestService calls `LegendService.AwardLegend()` on quest completion
- Passes quest metadata (BLV, difficulty, trauma context)
- All party members present at quest completion receive full Legend

---

### **Integration with Crafting System**

**Crafting Awards:**

- Masterwork items: BLV 75, Standard difficulty, Coherent context
- Legendary items: BLV 200+, Difficult, potentially Traumatic/Heretical
- Individual award (only crafter receives Legend)

**Quality Tiers:**

- Standard craft: 0 Legend (routine work)
- Masterwork: 75 Legend
- Legendary: 200-500 Legend

**Heretical Crafting:**

- Using corrupted components: Traumatic or Heretical context
- Higher Legend reward but risk Runic Blight Corruption
- Risk/reward decision for crafters

---

## VIII. Balancing Considerations

### **Anti-Grinding Mechanics**

**Trivial Challenge = 0 Legend:**

- Primary anti-grinding mechanic
- Players cannot farm low-level content
- Encourages engaging with appropriately challenging content

**Implementation:**

- Difficulty Modifier of 0× nullifies entire reward
- Even high BLV × 0 = 0 Legend
- Clear feedback to player: "No saga-worthy deed performed"

**Benefits:**

- Prevents level-farming exploits
- Maintains challenge/reward balance
- Encourages forward progression

---

### **Trauma Modifier Risk/Reward**

**Design Tension:**

- Higher TM = Higher Legend gain
- Higher TM = Higher Psychic Stress/Corruption risk
- Players must decide: "Is 2× Legend worth permanent Corruption?"

**Balancing:**

- Coherent content (TM: 1.0×) provides viable progression path
- Players can avoid high-TM content and still progress (slower)
- High-TM content offers faster progression at significant cost
- No "correct" choice - strategic preference

**Prevents:**

- Mandatory heretical acts (always an alternative)
- Trauma Economy trivialization (higher rewards justify risks)

---

### **Progression Pacing**

**Early Game (Milestones 0-10):**

- Low Legend requirements (500-1,500)
- Players reach milestones frequently
- Feels fast and rewarding
- Encourages engagement

**Mid Game (Milestones 10-30):**

- Moderate Legend requirements (1,500-3,500)
- Steady progression with occasional milestones
- Balanced pacing

**Late Game (Milestones 30+):**

- High Legend requirements (3,500+)
- Milestones become long-term goals
- Each milestone feels significant
- Prevents infinite power scaling

**Tuning Knobs:**

- Base Value (currently 500)
- Scaling Factor (currently 100)
- Can adjust if playtesting reveals pacing issues

---

### **Party Size Considerations**

**Shared Legend (No Split):**

- 4-person party receives same Legend as solo player
- Total Legend "generated" is higher for parties
- Rationale:
    - Parties face more difficult encounters (higher DM)
    - Cooperation is core theme
    - Prevents "solo is optimal" meta
    - Simpler to implement and understand

**Solo Play:**

- Solo players progress at comparable rate
- Encounter difficulty scales down for solo (via TDR system)
- No penalty for playing alone
- No bonus for playing in party (reward is cooperation itself)

---

## IX. Implementation Notes

### **Performance Considerations**

**Database Queries:**

- BLV lookups: Cached in memory (rarely changes)
- LegendAwards inserts: Batched if multiple awards in quick succession
- Index on character_id for fast award history lookups

**Calculation Performance:**

- Universal Formula is simple arithmetic (very fast)
- No complex loops or recursive calculations
- Can handle hundreds of awards per second

**UI Updates:**

- Legend progress bar updates smoothly (interpolation)
- Notifications queued to prevent spam
- Milestone notifications take priority

---

### **Testing Checklist**

**Unit Tests:**

- [ ]  Universal Formula calculates correctly for all modifier combinations
- [ ]  Overflow handling preserves excess Legend
- [ ]  Multiple milestone triggers work correctly
- [ ]  Trivial difficulty awards 0 Legend
- [ ]  Party distribution awards full Legend to each member
- [ ]  Individual awards only affect single character

**Integration Tests:**

- [ ]  LegendService → SagaService integration (Milestone trigger)
- [ ]  Death debuffs apply Legend penalties correctly
- [ ]  Trauma debuffs apply Legend penalties correctly
- [ ]  Quest completion awards Legend to party
- [ ]  Crafting awards Legend to individual

**Balancing Tests:**

- [ ]  Early game pacing feels fast
- [ ]  Late game pacing feels meaningful
- [ ]  Trauma Modifier incentivizes risk appropriately
- [ ]  Anti-grinding prevents low-level farming

---

## X. Design Philosophy Summary

**Legend as Narrative Currency:**

Legend is not simply "XP"—it is the in-world measure of a character's saga. Every point represents a moment worth remembering, a deed worth chronicling, a story worth telling. This diegetic approach reinforces Aethelgard's core theme: You are not just getting stronger; you are **writing your saga**.

**The Three-Component Philosophy:**

1. **Base Legend Value:** What was done?
2. **Difficulty Modifier:** How challenging was it?
3. **Trauma Modifier:** What was risked?

This elegant formula captures the essence of saga-worthy deeds while remaining simple to understand and implement.

**Anti-Grinding as Design Pillar:**

The Trivial Challenge = 0 Legend rule is fundamental. It prevents exploitation, maintains challenge/reward balance, and reinforces the narrative theme: Routine acts are not saga-worthy.

**Cooperative by Design:**

Shared Legend for party activities is a deliberate choice to encourage cooperation over competition. The party's saga is a shared chronicle, and all who participate share the glory.

**Trauma Integration:**

The Trauma Modifier creates a unique risk/reward dynamic. Players who engage more deeply with the dangerous, corrupted world progress faster—but at the cost of their characters' mental and spiritual health. This is Aethelgard's core tension made mechanical.

---

## Migration Complete

This specification has been fully migrated from v2.0 Legend System specification[[1]](https://www.notion.so/Feature-Specification-The-Legend-System-2a355eb312da809f9d9fd809adfae1c4?pvs=21) and enhanced with DB10 structure requirements. All sections now reflect Aethelgard's unique Legend accrual mechanics, progression curve, party dynamics, and thematic integration.

**Status:** Draft → Ready for Review

**Next Steps:**

1. Review for consistency with Saga System integration points
2. Validate formulas match Saga System Milestone calculations
3. Populate LegendValues database table with initial BLV values
4. Update status to Final after review

---

---