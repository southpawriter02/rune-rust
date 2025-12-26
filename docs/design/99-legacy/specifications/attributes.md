# Attributes System — Core System Specification v5.0

Status: Proposed
Balance Validated: No
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

## I. Core Philosophy: The Core Parameters of a Survivor

The five Core Attributes in Aethelgard are the **fundamental parameters of a character's "personal operating system."** They represent the raw, innate potential of a survivor's mind, body, and spirit—the baseline code that determines how a character interfaces with the broken world.

**Design Pillars:**

- **Meaningful Specialization:** Every attribute must be compelling for at least one Archetype
- **Balanced Distribution:** No single attribute should dominate all aspects of play
- **Clear Identity:** Each attribute solves problems in a fundamentally different way
- **Universal Relevance:** Every character needs at least two attributes to survive
- **Progression Foundation:** Attributes are the primary long-term investment in the Saga System

**The Five Parameters:**

1. **MIGHT** — The brute-force processor: Raw physical power
2. **FINESSE** — The agile co-processor: Speed, precision, evasion
3. **STURDINESS** — The resilient hardware: Endurance, toughness, capacity
4. **WITS** — The analytical processor: Intelligence, perception, logic
5. **WILL** — The psychic firewall: Mental fortitude, spiritual resilience

Every point invested in an attribute is a meaningful choice that shapes a character's capabilities, defines their role, and dictates how they interface with the physical and metaphysical challenges of a reality that no longer obeys its original rules.

---

*Migration in progress from v2.0 Attributes System specification. Remaining sections to be added incrementally.*

Every point invested in an attribute is a meaningful choice that shapes a character's capabilities, defines their role, and dictates how they interface with the physical and metaphysical challenges of a reality that no longer obeys its original rules.

## II. Attribute Definitions

Each of the five Core Attributes represents a distinct approach to problem-solving in Aethelgard's broken reality. This section defines their mechanical functions, thematic roles, and integration points.

---

### **1. MIGHT — The Brute-Force Processor**

**Core Philosophy:**

MIGHT represents raw physical power, muscular force, and the ability to impose one's will upon the physical world through brute strength. It is the parameter for the **brute-force solution**—applying a simpler, more direct, and brutally undeniable logic to a world of complex, paradoxical problems.

A high-MIGHT character does not debug a locked door; they smash it. They do not exploit a glitch in an enemy's code; they shatter the hardware it's running on.

**Player-Facing Description:**

> **[MIGHT]:** *"Raw, physical power. The strength of your arm and the force of your blows. MIGHT governs your effectiveness with heavy melee weapons, your ability to smash through obstacles, and your capacity to intimidate foes through sheer physical presence."*
> 

**Primary Functions:**

**Melee Damage (Primary Driver):**

- MIGHT is the primary damage modifier for heavy melee weapons (two-handed weapons, strength-based one-handed weapons)
- Formula: `Damage Dice Pool = (Weapon's Base Dice) + MIGHT`
- Example: Great Axe (4d6 base) wielded by character with 8 MIGHT = 12d6 damage pool

**Physical Force Skill Checks:**

- Governs checks requiring raw physical power
- Formula: `Dice Pool = MIGHT + (Relevant Skill Rank)`
- Applications: Smashing doors, lifting rubble, breaking objects, forcing mechanisms

**Dialogue: Intimidation Path:**

- Provides high-risk social interaction option
- Triggers MIGHT-based opposed roll vs. NPC's WILL
- Success: NPC compliance; Failure: Hostility, combat, faction reputation loss

**Equipment Requirements:**

- Heavy weapons and armor may require minimum MIGHT
- Below threshold: Increased Botch range (weapons), Defense penalty (armor)

**Thematic Role:**

- Primary attribute for **Warriors** (Berserker, Atgeir-wielder, Gorge-Maw Ascetic, Vargr-Born)
- Secondary attribute for Skjaldmær (shield bash damage), Strandhögg (charge damage)
- Dump stat for most Mystics and Adepts

**Flavor:** Attacks described as "crushing," "brutal," "shattering." Feats of strength are "effortless" or "mighty."

---

### **2. FINESSE — The Agile Co-Processor**

**Core Philosophy:**

FINESSE represents agility, reflexes, precision, and manual dexterity. It is the parameter for the **agile co-processor**—the ability to navigate the world's physical glitches with grace, exploit the "lag" in an enemy's corrupted combat script, and execute actions with speed and accuracy that seems to defy chaos.

A high-FINESSE character does not overpower a system error; they avoid it. They do not smash through a defense; they find the single, hair-thin gap.

**Player-Facing Description:**

> **[FINESSE]:** *"Agility, reflexes, and precision. The speed of your hand and the certainty of your step. FINESSE governs your ability to evade attacks, your accuracy with ranged and light weapons, and your proficiency in the subtle arts of stealth, acrobatics, and disarming delicate, ancient mechanisms."*
> 

**Primary Functions:**

**Defense Score (Primary Driver):**

- FINESSE is the most important contributor to passive evasion
- Formula: `Defense Score = (Base Defense) + (FINESSE - Psychic Stress Penalty) + Armor Modifiers`
- Every point makes character exponentially harder to hit

**Accuracy (Universal for Physical Attacks):**

- Governs accuracy for most physical attacks (melee and ranged)
- Formula: `Accuracy Dice Pool = FINESSE + (Weapon Skill Bonus)`
- Separates precision from raw power

**Damage (Finesse Weapons & Ranged):**

- Damage modifier for specific weapon types
- Applies to: Daggers, rapiers, bows, thrown weapons
- Formula: `Damage Dice Pool = (Weapon's Base Dice) + FINESSE`

**Initiative (Vigilance):**

- Key component of turn order calculation
- Formula: `Vigilance = FINESSE + WITS`
- High-FINESSE characters act first

**Skill Checks:**

- Governs Acrobatics (climbing, balancing, leaping, stealth)
- Governs precision-based System Bypass (disarming traps, picking locks)
- Formula: `Dice Pool = FINESSE + (Skill Rank)`

**Thematic Role:**

- Primary attribute for **Skirmishers** (Myrk-gengr, Veiðimaðr, Gantry-Runner, Hlekkr-master)
- Secondary attribute for Hólmgangr (parrying), Strandhögg (mobility), Scrap-Tinker (delicate repairs)
- Dump stat for heavy Warriors and non-combat Mystics

**Flavor:** Attacks described as "swift," "precise," "elegant." Movements are "deft," "ghostly," or a "blur of motion."

---

### **3. STURDINESS — The Resilient Hardware**

**Core Philosophy:**

STURDINESS represents physical toughness, endurance, and resilience. It is the parameter for **resilient hardware**—the measure of a character's ability to withstand physical damage, disease, environmental hardship, and prolonged exertion without system failure.

A high-STURDINESS character does not avoid damage; they absorb it. They are the stable, well-maintained system that refuses to crash, the immovable object that endures where others would fall.

**Player-Facing Description:**

> **[STURDINESS]:** *"Physical toughness, endurance, and resilience. The capacity of your body to withstand punishment and keep functioning. STURDINESS governs your maximum health, your ability to resist poison and disease, your endurance in prolonged physical exertion, and your capacity to block incoming blows."*
> 

**Primary Functions:**

**Health Pool (Primary Driver):**

- STURDINESS is the most significant contributor to maximum HP
- Formula: `Max HP = (Base HP) + (STURDINESS × 10) + Gear/Ability Bonuses - Corruption Penalty`
- Every point grants 10 additional HP

**Stamina Pool (Major Contributor):**

- Affects maximum Stamina for physical actions
- Formula: `Max Stamina = (Base Stamina) + (STURDINESS × 5) + Gear/Ability Bonuses`
- Sustains longer combat engagement

**Physical Resolve Checks (Resistance):**

- Dice pool for resisting physical effects
- Formula: `Physical Resolve Pool = STURDINESS (dice)`
- Resists: Poison, Disease, Knocked Down, Exhaustion, Environmental hazards

**Block Pool (Defensive Action):**

- Component of the `block` command dice pool
- Formula: `Block Pool = STURDINESS + (Shield Rating) + (Relevant Skill)`
- Enables active damage mitigation

**Carrying Capacity:**

- Determines encumbrance thresholds
- Formula: `Carrying Capacity = (Base Capacity) + (STURDINESS × 5) kg`

**Environmental Resistance:**

- Reduces damage from environmental hazards
- Extends survival time in hostile conditions (extreme cold, heat, toxic atmosphere)

**Thematic Role:**

- Primary attribute for **Tanks** (Skjaldmær, Vargr-Born resilience builds)
- Secondary attribute for all Warriors (survivability), Strandhögg (charging durability)
- Universal secondary stat (every character needs some HP)
- Lower priority for glass-cannon Skirmishers and backline Mystics

**Flavor:** Character described as "hardy," "unyielding," "resilient." Injuries are "shrugged off" or "endured." Constitution is "iron" or "inexhaustible."

---

### **4. WITS — The Analytical Processor**

**Core Philosophy:**

WITS represents intelligence, logic, perception, and analytical reasoning. It is the parameter for the **analytical processor**—a character's ability to parse the corrupted data of a glitching world, identify the signal in the static, read the error logs of a failing system, and debug the paradoxes that govern reality.

A high-WITS character does not overpower or avoid a threat; they deconstruct it. They are the scholars, detectives, and engineers of a new age, believing that knowledge is the only tool that can truly fix a broken world.

**Player-Facing Description:**

> **[WITS]:** *"Intelligence, perception, and analytical reasoning. The sharpness of your eye and the speed of your thought. WITS governs your ability to notice hidden details, to decipher ancient lore, to analyze your foes' weaknesses, and to master the complex arts of crafting and survival in a world that no longer makes sense."*
> 

**Primary Functions:**

**Perception (Primary Driver):**

- Most important attribute for the Perception System
- Passive Physical Perception: `Score = 10 + WITS`
- Active Perception: `Dice Pool = WITS + (Skill Rank)`
- Determines what characters notice automatically and what they can discover when searching

**Skill Checks (Knowledge & Logic):**

- Governs System Bypass (hacking, analyzing schematics, paradox logic)
- Governs Wasteland Survival (tracking, foraging, ecological knowledge)
- Formula: `Dice Pool = WITS + (Skill Rank)`

**Crafting (Primary Attribute):**

- WITS is primary attribute for all crafting disciplines
- Applies to: Tinkering, Alchemy, Field Medicine
- Formula: `Crafting Dice Pool = WITS + (Crafting Skill Rank) + Tool Bonus`
- Represents technical knowledge to create stable items from corrupted components

**Initiative (Vigilance - Tactical Component):**

- Represents tactical awareness and battlefield processing speed
- Formula: `Vigilance = FINESSE + WITS`
- High-WITS characters act first due to faster analysis

**Combat Utility: Analysis Abilities:**

- Fuels analysis abilities across multiple specializations
- Examples: Jötun-Reader's Analyze Weakness, Scrap-Tinker's Analyze Mechanism, Bone-Setter's Anatomical Insight
- Provides force-multiplying combat advantage through information

**Thematic Role:**

- Primary attribute for **Adepts** (Jötun-Reader, Scrap-Tinker, Ruin-Stalker, Brewmaster)
- Secondary attribute for Veiðimaðr (tracking), Seiðkona (interpreting echoes), any crafter
- Dump stat for pure brute-force builds (Berserker instinct over analysis)

**Flavor:** Character receives richer, more detailed information. Dialogue options are clever, logical, insightful. Descriptions include technical details and hidden patterns.

---

### **5. WILL — The Psychic Firewall**

**Core Philosophy:**

WILL represents mental fortitude, force of personality, and spiritual resilience. It is the parameter for the **psychic firewall**—the strength of a character's coherent sense of self, their ability to maintain a stable "personal operating system" against the constant, invasive barrage of the Great Silence's screaming, paradoxical static.

A high-WILL character solves problems by **enduring the world's madness**. They are the anchor in the psychic storm, the unshakeable voice of reason, and the conduit for the world's most dangerous and powerful energies. **WILL is the single most critical stat for long-term survival in the Trauma Economy.**

**Player-Facing Description:**

> **[WILL]:** *"Mental fortitude, personal conviction, and spiritual resilience. The strength of your mind and the integrity of your soul. WILL governs your ability to resist the terrifying psychic horrors of the ruins, to persuade others through the force of your personality, and to channel the raw, chaotic power of the tainted Aether without being consumed by it."*
> 

**Primary Functions:**

**Trauma Economy Defense (Primary Driver - CRITICAL):**

- WILL is the single most important attribute for surviving mental/metaphysical threats
- Formula: `Mental Resolve Dice Pool = WILL`
- Resists:
    - Gaining Psychic Stress (environmental, enemy attacks, self-inflicted abilities)
    - Gaining permanent Trauma (critical check at 100 Psychic Stress)
    - Gaining Runic Blight Corruption (heretical magic, glitched artifacts, metaphysical exposure)
    - Mental status effects (Fear, Disoriented, Charm)

**Mystic Power (Primary Driver):**

- WILL is the "engine" for all Mystic specializations

**Maximum Aether Pool:**

- Formula: `Max AP = (Base AP) + (WILL × 10) + Gear/Ability Bonuses`
- Every point grants 10 additional AP

**Mystic Potency:**

- Primary modifier for spell/ability effectiveness
- Applications:
    - Increased damage for Galdr-caster spells
    - Increased healing for Grove-Warden balms
    - Higher Difficulty Class (DC) for enemies resisting curses/effects

**Perception & Social (Secondary Functions):**

- Passive Psychic Perception: `Score = 10 + WILL`
- Governs Rhetoric skill (persuasion, deception, intimidation)
- Formula: `Dialogue Dice Pool = WILL + Rhetoric Skill Rank`
- Allows character to sense metaphysical phenomena and project personality forcefully

**Thematic Role:**

- Primary attribute for ALL **Mystic** specializations (universal)
- Secondary attribute for Skald/Thul (oratory), Iron-Hearted (resisting core corruption)
- **Universal critical secondary stat:** Even pure MIGHT/STURDINESS Warriors cannot neglect WILL—low WILL means the first Forlorn will break them psychologically

**Flavor:** Resolve described as "unflinching," "iron-clad," "serene." Character is "calm center of the storm." Dialogue is persuasive, commanding, or deceptively serene.

**Flavor:** Resolve described as "unflinching," "iron-clad," "serene." Character is "calm center of the storm." Dialogue is persuasive, commanding, or deceptively serene.

---

## III. Character Creation & Progression

### **Character Creation: Point-Buy System**

**Starting Allocation:**

- All five attributes start at base value of **5**
- Players receive **15 points** to distribute
- Scaling cost encourages specialization

**Cost Scaling:**

| Attribute Level | Cost per Level |
| --- | --- |
| 5 → 8 | 1 point |
| 8 → 10 | 2 points |

**Example Distributions:**

**Balanced Warrior (5 points remaining):**

- MIGHT: 8 (3 points spent)
- FINESSE: 7 (2 points spent)
- STURDINESS: 8 (3 points spent)
- WITS: 5 (0 points spent)
- WILL: 7 (2 points spent)

**Specialized Mystic (0 points remaining):**

- MIGHT: 5 (0 points spent)
- FINESSE: 6 (1 point spent)
- STURDINESS: 6 (1 point spent)
- WITS: 7 (2 points spent)
- WILL: 10 (7 points spent: 3 to reach 8, then 4 more to reach 10)

**Implementation Requirements:**

- CharacterCreationService.cs handles point-buy logic
- UI must clearly display:
    - Remaining points
    - Cost for next level
    - Derived stat previews (HP, AP, Defense, etc.)
    - Warning if attempting "dump stat" critical attributes (e.g., WILL below 6)

### **Progression: Saga System Integration**

**Primary PP Expenditure:**

- Attributes are the primary long-term investment for Progression Points
- After unlocking desired abilities, most PP goes to attribute increases

**Cost Formula:**

```
PP Cost = New Attribute Level × 2
```

**Cost Examples:**

- Increase from 8 → 9: 18 PP
- Increase from 9 → 10: 20 PP
- Increase from 10 → 11: 22 PP
- Increase from 15 → 16: 32 PP

**Rationale:**

- Linear scaling prevents exponential power curve explosion
- High costs at high levels ensure progression remains meaningful
- No hard cap allows long-term character growth
- "Organic cap" is the finite number of PP earnable in the game

**Progression Philosophy:**

- Early game: Players focus on unlocking key abilities (PP → Specializations)
- Mid game: Players balance attribute increases with ability unlocks
- Late game: Players primarily increase attributes for incremental power gains

**No Hard Caps:**

- Attributes can theoretically increase indefinitely
- Practical limit is total PP earned during campaign
- Prevents absurd values while allowing deep specialization
- At 50+ milestones, a character might reach 20+ in primary attribute

### **Respec Policy**

**Current Design: No Respec**

- Attribute point allocation is permanent
- Encourages meaningful, deliberate choices
- Prevents constant reoptimization based on encounter type

**Future Consideration:**

- If player demand is high, may add costly respec mechanic
- Potential implementation: "Saga Rewrite" costs significant PP investment
- Would maintain weight of decisions while allowing course correction

**Future Consideration:**

- If player demand is high, may add costly respec mechanic
- Potential implementation: "Saga Rewrite" costs significant PP investment
- Would maintain weight of decisions while allowing course correction

## IV. Derived Stats & Calculation Formulas

Attributes serve as the input values for calculating a character's derived statistics. These formulas are evaluated by StatCalculationService.cs whenever attributes, gear, or relevant modifiers change.

### **Health Points (HP)**

**Formula:**

```csharp
Max HP = (Base HP) + (STURDINESS × 10) + Gear/Ability Bonuses - Corruption Penalty
```

**Variable Definitions:**

- **Base HP:** 50 (constant for all characters)
- **STURDINESS × 10:** Primary driver (e.g., STURDINESS 8 = +80 HP)
- **Gear/Ability Bonuses:** Armor, trinkets, specialization abilities
- **Corruption Penalty:** Runic Blight Corruption reduces max HP

**Example Calculations:**

- Character with STURDINESS 5, no bonuses: 50 + 50 = 100 HP
- Character with STURDINESS 8, +20 from gear: 50 + 80 + 20 = 150 HP
- Character with STURDINESS 10, +30 from gear, -15 from corruption: 50 + 100 + 30 - 15 = 165 HP

**Recalculation Triggers:**

- STURDINESS increase via Saga System
- Equipping/unequipping armor or trinkets
- Gaining Runic Blight Corruption
- Activating/deactivating HP-modifying abilities

---

### **Aether Pool (AP)**

**Formula:**

```csharp
Max AP = (Base AP) + (WILL × 10) + Gear/Ability Bonuses
```

**Variable Definitions:**

- **Base AP:** 0 (only characters with Mystic specializations have base AP)
- **WILL × 10:** Primary driver (e.g., WILL 8 = +80 AP)
- **Gear/Ability Bonuses:** Mystic equipment, specialization passives

**Example Calculations:**

- Non-Mystic with WILL 7: 0 + 70 = 70 AP (unusable without Mystic abilities)
- Mystic with WILL 10, no bonuses: 0 + 100 = 100 AP
- Mystic with WILL 12, +30 from gear: 0 + 120 + 30 = 150 AP

**Recalculation Triggers:**

- WILL increase via Saga System
- Equipping/unequipping Mystic gear
- Unlocking Mystic specialization abilities with AP bonuses

---

### **Stamina Pool**

**Formula:**

```csharp
Max Stamina = (Base Stamina) + (STURDINESS × 5) + Gear/Ability Bonuses
```

**Variable Definitions:**

- **Base Stamina:** 50 (constant for all characters)
- **STURDINESS × 5:** Major contributor (e.g., STURDINESS 8 = +40 Stamina)
- **Gear/Ability Bonuses:** Certain equipment, Warrior specialization passives

**Example Calculations:**

- Character with STURDINESS 5, no bonuses: 50 + 25 = 75 Stamina
- Character with STURDINESS 8, +15 from gear: 50 + 40 + 15 = 105 Stamina

**Usage:**

- Powers physical actions (sprinting, dodging, blocking, power attacks)
- Regenerates between encounters or via rest actions

---

### **Defense Score**

**Formula:**

```csharp
Defense Score = (Base Defense) + (FINESSE - Psychic Stress Penalty) + Armor Modifiers
```

**Variable Definitions:**

- **Base Defense:** 10 (constant for all characters)
- **FINESSE:** Primary driver of evasion (e.g., FINESSE 8 = +8 Defense)
- **Psychic Stress Penalty:** Direct reduction from current Psychic Stress level
    - Formula: `Penalty = floor(Psychic Stress / 20)`
    - At 40 Stress: -2 Defense
    - At 80 Stress: -4 Defense
- **Armor Modifiers:** Light armor adds minor Defense; heavy armor may reduce Defense (cumbersome)

**Example Calculations:**

- Character with FINESSE 8, 0 Stress, no armor: 10 + 8 = 18 Defense
- Character with FINESSE 10, 40 Stress, +2 from light armor: 10 + 10 - 2 + 2 = 20 Defense
- Character with FINESSE 6, 80 Stress, -1 from heavy armor: 10 + 6 - 4 - 1 = 11 Defense

**Critical Balancing Note:**

- Psychic Stress Penalty is the **primary counter** to high-FINESSE "dodge tanks"
- A FINESSE 15 character at 100 Stress loses 5 Defense, making them vulnerable
- This enforces the Trauma Economy as universal threat

**Recalculation Triggers:**

- FINESSE increase
- Psychic Stress gain/reduction
- Equipping/unequipping armor

---

### **Vigilance (Initiative)**

**Formula:**

```csharp
Vigilance = FINESSE + WITS
```

**Variable Definitions:**

- **FINESSE:** Represents reflexes and physical reaction speed
- **WITS:** Represents tactical awareness and threat assessment speed

**Example Calculations:**

- Warrior (FINESSE 8, WITS 5): Vigilance 13
- Skirmisher (FINESSE 10, WITS 7): Vigilance 17
- Adept (FINESSE 6, WITS 10): Vigilance 16

**Usage:**

- At combat start, all combatants roll initiative
- Vigilance is added to initiative roll
- Higher total acts first in turn order

**Design Intent:**

- Both "fast characters" (high FINESSE) and "smart characters" (high WITS) can act early
- Creates multiple paths to controlling turn order

---

### **Resolve Pools**

**Physical Resolve Pool:**

```csharp
Physical Resolve Dice Pool = STURDINESS
```

**Mental Resolve Pool:**

```csharp
Mental Resolve Dice Pool = WILL
```

**Usage:**

- Resolve checks are resistance rolls against harmful effects
- Physical Resolve: Resists Poison, Disease, Knocked Down, Exhaustion
- Mental Resolve: Resists Psychic Stress, Fear, Charm, Corruption

**Example:**

- Character with STURDINESS 8 rolls 8d6 to resist Poison
- Character with WILL 10 rolls 10d6 to resist Fear effect

---

### **Carrying Capacity**

**Formula:**

```csharp
Carrying Capacity = (Base Capacity) + (STURDINESS × 5) kg
```

**Variable Definitions:**

- **Base Capacity:** 25 kg (constant)
- **STURDINESS × 5:** (e.g., STURDINESS 8 = +40 kg capacity)

**Example Calculations:**

- Character with STURDINESS 5: 25 + 25 = 50 kg capacity
- Character with STURDINESS 10: 25 + 50 = 75 kg capacity

**Encumbrance Thresholds:**

- 0-100% capacity: No penalty
- 100-150% capacity: -2 Defense, movement speed reduced
- 150%+ capacity: Cannot move without dropping items

---

### **Perception Scores**

**Passive Physical Perception:**

```csharp
Score = 10 + WITS
```

**Passive Psychic Perception:**

```csharp
Score = 10 + WILL
```

**Usage:**

- Determines what character notices automatically without active searching
- GM compares against hidden object/trap Difficulty Class (DC)
- If Perception Score ≥ DC, character automatically notices

**Example:**

- Character with WITS 9: Passive Physical Perception 19 (notices DC 19 or lower automatically)
- Character with WILL 7: Passive Psychic Perception 17 (senses DC 17 or lower metaphysical phenomena)

**Example:**

- Character with WITS 9: Passive Physical Perception 19 (notices DC 19 or lower automatically)
- Character with WILL 7: Passive Psychic Perception 17 (senses DC 17 or lower metaphysical phenomena)

---

## V. Integration with Other Systems

The Attribute System serves as the foundational layer upon which all other game mechanics are built. This section documents critical integration points and dependencies.

### **Skills System**

**Dependency:** Every skill check combines an Attribute + Skill Rank

**Formula:** `Skill Check Dice Pool = (Governing Attribute) + (Skill Rank) + Situational Modifiers`

**Attribute-to-Skill Mapping:**

- **MIGHT:** Physical force checks (contextual, not a dedicated skill)
- **FINESSE:** Acrobatics, precision System Bypass
- **STURDINESS:** Resistance checks (not skill-based)
- **WITS:** System Bypass (hacking/analysis), Wasteland Survival, all Crafting
- **WILL:** Rhetoric

**Implementation:** SkillCheckService.cs queries attributes before calculating dice pools

---

### **Combat System**

**Attack Accuracy:**

- Physical attacks: `FINESSE + (Weapon Skill Bonus)`

**Damage Calculation:**

- Heavy/Strength weapons: `(Weapon Base) + MIGHT`
- Finesse/Ranged weapons: `(Weapon Base) + FINESSE`

**Defense:**

- Passive evasion: `10 + (FINESSE - Stress Penalty) + Armor`
- Active blocking: `STURDINESS + (Shield Rating) + (Skill)`

**Resistance:**

- Physical status effects: `STURDINESS (dice pool)`
- Mental status effects: `WILL (dice pool)`

**Initiative:** `FINESSE + WITS`

---

### **Trauma Economy**

**Critical Dependency:** WILL is the primary defense against all mental/metaphysical threats

**Psychic Stress Resistance:**

- Environmental exposure: WILL-based Resolve Check
- Enemy psychic attacks: WILL-based Resolve Check
- Self-inflicted costs: WILL-based Resolve Check

**Permanent Trauma Check:**

- At 100 Psychic Stress: Critical WILL-based Resolve Check
- Failure: Gain permanent Trauma

**Runic Blight Corruption:**

- Heretical magic exposure: WILL-based Resolve Check
- Corrupted artifact interaction: WILL-based Resolve Check

**Reverse Integration:**

- Psychic Stress directly penalizes FINESSE (Defense Score reduction)
- Creates feedback loop: Low WILL → High Stress → Low Defense → More damage → More stress

---

### **Dialogue System**

**Attribute-Based Dialogue Options:**

**[MIGHT] Options:**

- Intimidation through physical presence
- Opposed roll: Character's MIGHT vs. NPC's WILL
- High risk: Failure often leads to combat or faction reputation loss

**[WITS] Options:**

- Logical deduction, spotting inconsistencies, recalling lore
- Unlocks deeper quest information and alternative solutions
- Often leads to non-violent resolutions

**[WILL] Options:**

- Persuasion, force of personality, commanding presence
- Uses Rhetoric skill: `WILL + Rhetoric Rank`
- Deception and intimidation through psychological pressure

---

### **Specialization System**

**Attribute Requirements:**

Each specialization has a **primary attribute** that determines its effectiveness:

**MIGHT-Primary Specializations:**

- Berserker, Atgeir-wielder, Gorge-Maw Ascetic, Vargr-Born (damage builds)

**FINESSE-Primary Specializations:**

- Myrk-gengr, Veiðimaðr, Gantry-Runner, Hlekkr-master

**STURDINESS-Primary Specializations:**

- Skjaldmær (primary), Vargr-Born (tank builds)

**WITS-Primary Specializations:**

- Jötun-Reader, Scrap-Tinker, Ruin-Stalker, Brewmaster

**WILL-Primary Specializations:**

- ALL Mystic specializations (Galdr-caster, Seiðkona, Grove-Warden, Rust-Witch, etc.)

**Ability Scaling:**

- Most specialization abilities scale with their primary attribute
- Some abilities have minimum attribute requirements to unlock

---

### **Puzzle System**

**Attribute-Based Solutions:**

**MIGHT Solutions:**

- Brute-force approach: Smash doors, break mechanisms, force open gates
- Bypasses complex puzzles at cost of noise/attention
- Example: Smashing through locked door instead of finding key

**FINESSE Solutions:**

- Precision approach: Disarm traps, pick locks, exploit physical glitches
- Quiet, elegant solutions
- Example: Carefully navigating tripwire maze

**STURDINESS Solutions:**

- Endurance approach: Surviving environmental hazards to reach solution
- Tanking damage to access otherwise deadly areas
- Example: Wading through acid to pull lever

**WITS Solutions:**

- Analytical approach: Solve logic puzzles, decode mechanisms, hack systems
- Most efficient and comprehensive solutions
- Example: Debugging Data Corruption Puzzle

**WILL Solutions:**

- Metaphysical approach: Interface with Blight-corrupted systems, resist psychic traps
- Required for certain metaphysical puzzles
- Example: Maintaining coherence while interfacing with paradox engine

---

### **Saga System**

**Integration:** Attributes are the primary long-term PP sink

**PP Cost to Increase Attribute:**

```csharp
PP Cost = New Attribute Level × 2
```

**Progression Priority:**

- Early game: PP → Specialization abilities
- Mid game: PP → Balance of attributes and abilities
- Late game: PP → Primarily attributes for incremental power

**No Hard Caps:**

- Attributes can increase indefinitely
- Organic cap is finite PP available in game

---

## VI. Balancing Considerations

### **The "Single Attribute Dominance" Problem**

**Challenge:** Prevent any one attribute from becoming universally superior

**Solutions Implemented:**

**MIGHT Limitations:**

- Does not improve accuracy (FINESSE does)
- Does not improve survivability (STURDINESS/WILL do)
- Forces Warriors to invest in 2-3 attributes for effectiveness

**FINESSE Limitations:**

- Hard-countered by AoE attacks (no evasion)
- Directly penalized by Psychic Stress (Defense reduction)
- Low WILL + high FINESSE = glass cannon that shatters mentally

**STURDINESS Limitations:**

- Only provides defensive value
- No offensive contribution
- Cannot survive alone without damage output or evasion

**WITS Limitations:**

- No direct combat damage contribution
- Requires party to capitalize on analysis information
- Utility-focused, not power-focused

**WILL Limitations:**

- Only offensive value is for Mystics
- Warriors investing heavily in WILL sacrifice damage output
- Must be balanced with gear/consumable alternatives for Trauma Economy

---

### **The "WILL as God Stat" Problem**

**Challenge:** WILL's universal importance for Trauma Economy survival risks making it mandatory for all builds

**Mitigation Strategies:**

**1. Specialization Dependency:**

- Only Mystics gain offensive benefits from WILL
- Warriors investing heavily in WILL become very sane but weak

**2. Alternative Defenses:**

- Gear with [Psychic Baffler] mods provides Stress resistance
- Consumables (Stabilizing Draughts) provide Stress management
- Bone-Setter specialization provides party-wide Stress healing

**3. Minimum Viable WILL:**

- WILL 6-7 with gear/party support is viable for non-Mystics
- WILL 5 (base) is extremely risky but theoretically survivable with heavy support
- Creates meaningful choice: Invest in WILL OR rely on gear/party

**4. Strategic Play:**

- Low-WILL characters must play cautiously around psychic threats
- Sanctuary Rest becomes critical for Stress management
- Party composition matters (Bone-Setter becomes invaluable)

---

### **The "FINESSE Dodge Tank" Problem**

**Challenge:** High FINESSE can make characters nearly unhittable

**Balancing Mechanisms:**

**1. Psychic Stress Penalty (Primary Counter):**

- Every 20 Stress = -1 Defense
- At 100 Stress: -5 Defense
- FINESSE 15 character drops from 25 Defense to 20 Defense at high stress
- Enforces Trauma Economy as universal threat

**2. Hard Counters:**

- AoE attacks ignore Defense (hit automatically)
- Environmental hazards ignore Defense
- Psychic attacks bypass physical evasion entirely

**3. Accuracy Scaling:**

- Enemy accuracy increases at higher difficulty tiers
- Late-game enemies have high enough accuracy to threaten even high-Defense characters

---

## VII. Implementation Notes

### **Service Architecture**

**StatCalculationService.cs (Core Service):**

- Responsible for calculating all derived stats
- Must be queried by all other services that need attribute values
- Recalculates derived stats whenever:
    - Attributes change (Saga System increase)
    - Gear changes (equip/unequip)
    - Status effects apply/expire
    - Psychic Stress changes (for Defense recalculation)

**Integration Pattern:**

```csharp
// Other services should never directly access character.attributes
// Always go through StatCalculationService
var defense = StatCalculationService.CalculateDefenseScore(character);
var vigilance = StatCalculationService.CalculateVigilance(character);
```

### **Database Schema**

**Characters Table (Attribute Fields):**

```sql
ALTER TABLE Characters ADD COLUMN (
  might INT NOT NULL DEFAULT 5,
  finesse INT NOT NULL DEFAULT 5,
  sturdiness INT NOT NULL DEFAULT 5,
  wits INT NOT NULL DEFAULT 5,
  will INT NOT NULL DEFAULT 5,
  
  CHECK (might >= 5),
  CHECK (finesse >= 5),
  CHECK (sturdiness >= 5),
  CHECK (wits >= 5),
  CHECK (will >= 5)
);
```

**Constraints:**

- All attributes have minimum value of 5 (starting base)
- No maximum constraint (organic cap via finite PP)

### **UI Display Requirements**

**Character Sheet:**

- Display all five attributes prominently
- Show derived stats with attribute contributions clearly labeled
- Update in real-time when hovering over attribute increase options
- Preview derived stat changes before confirming PP spend

**Saga Menu (Attribute Increase Tab):**

- Display current attribute values
- Display PP cost for next level (New Level × 2)
- Show derived stat impact (e.g., "+10 HP, +1 Defense")
- Confirmation dialog before spending PP
- Disable if insufficient PP

---

## VIII. Design Philosophy Summary

**The Five-Attribute Philosophy:**

Aethelgard's attribute system is built on the principle that **every approach to survival must be mechanically viable and thematically distinct.**

**Why Five Attributes?**

- Fewer would lack mechanical depth
- More would create analysis paralysis and dilute identity
- Five allows clear role definition: Power (MIGHT), Speed (FINESSE), Endurance (STURDINESS), Knowledge (WITS), Willpower (WILL)

**Why No Dump Stats?**

- MIGHT: Warriors need damage
- FINESSE: Everyone needs Defense and accuracy
- STURDINESS: Everyone needs HP to survive
- WITS: Everyone benefits from Perception and crafting
- WILL: Everyone needs Trauma Economy defense

**But specialization is encouraged:**

- Point-buy costs incentivize focusing on 2-3 attributes
- PP costs at high levels make maxing all five impossible
- Each archetype has clear primary/secondary attributes

**The Trauma Economy Integration:**

- WILL's universal importance creates the central tension
- Every character must choose: Invest in WILL OR rely on external support
- This decision shapes playstyle and party dynamics

---

## Migration Complete

This specification has been fully migrated from v2.0 Attributes System specification[[1]](https://www.notion.so/Feature-Specification-The-Attributes-System-2a355eb312da8017bbe9d1b3d5551b2d?pvs=21) and individual attribute specs (MIGHT[[2]](https://www.notion.so/Feature-Specification-The-MIGHT-Attribute-2a355eb312da80ba9ba7cdba0c15018d?pvs=21), FINESSE[[3]](https://www.notion.so/Feature-Specification-The-FINESSE-Attribute-2a355eb312da80509fdadfc5d2a0799f?pvs=21), WILL[[4]](https://www.notion.so/Feature-Specification-The-WILL-Attribute-2a355eb312da80169ed4f0e6d1e35bc5?pvs=21), WITS[[5]](https://www.notion.so/Feature-Specification-The-WITS-Attribute-2a355eb312da8086a691d3fdb419b3f1?pvs=21)), with STURDINESS extrapolated from the main system spec. All sections now reflect Aethelgard's unique attribute design and integration requirements.

**Status:** Draft → Ready for Review

**Next Steps:**

1. Review for consistency with Saga System progression costs
2. Validate derived stat formulas with other system specs (Combat, Trauma Economy)
3. Update status to Final after review

---

---

---