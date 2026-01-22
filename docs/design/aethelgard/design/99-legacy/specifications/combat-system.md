# Combat System — Core System Specification v5.0

Status: Proposed
Balance Validated: No
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

## I. Core Philosophy: The Tactical System Crash

Combat in Aethelgard is a **tactical system crash**—a structured, turn-based encounter where two or more groups of "coherent processes" (the party) and "corrupted processes" (enemies) attempt to terminate each other.

**Design Pillars:**

- **Tactical Depth:** Emphasizes positioning, resource management, and clever ability application over simple damage races
- **Meaningful Positioning:** Grid location dictates targeting options, vulnerability, and environmental interaction
- **Resource Tension:** Action economy, Stamina, and HP create strategic trade-offs
- **Defensive Options:** Three distinct defensive approaches (Evasion, Block, Parry) reward tactical preparation
- **Trauma Integration:** Combat is a primary source of Psychic Stress, reinforcing theme of mental cost

**The Conceptual Model:**

Every action is a **command executed within an unstable subsystem**. Every outcome is the direct result of:

- Character's build (attributes, specializations, equipment)
- Strategic choices (positioning, ability selection, resource management)
- Chaotic, glitching nature of the broken world (dice pool resolution, status effects)

Combat is not a simple exchange of damage—it is a **deep, tactical puzzle** where positioning, timing, and resource allocation determine victory or defeat.

**Why Turn-Based Grid Combat?**

**Thematic Justification:**

- Reflects the structured, logical nature of "coherent processes" imposing order on chaos
- Turn order (Vigilance) represents processing speed and threat detection
- Grid positioning represents physical and metaphysical proximity in unstable reality

**Gameplay Benefits:**

- Allows for deep tactical planning (no twitch reflexes required)
- Clear communication of all combat state (HP, status effects, positioning)
- Enables complex ability interactions and combos
- Accessible to text-based interface (CSI)

---

*Migration in progress from v2.0 Combat System specification. Remaining sections to be added incrementally.*

## II. The Anatomy of an Encounter

### Combat Initiation

**Trigger Conditions:**

Combat is initiated when any of the following occurs:

- Party enters a room/zone with hostile entities
- Ambush during a rest period (failed security, random encounter)
- Failed dialogue/negotiation check turns NPC hostile
- Scripted story encounter begins

**The Transition Sequence:**

When combat is triggered, the `GameEngine` transitions from `ExplorationState` to `CombatState`. This involves a specific initialization sequence:

**1. Grid Finalization**

The battlefield grid is locked in with all environmental elements:

- **Rows:** Player Zone (Front/Back Row) and Enemy Zone (Front/Back Row)
- **Sectors:** Distinct positioning within rows (if applicable)
- **Cover:** Environmental objects providing defensive bonuses
- **Hazards:** Environmental dangers (fire, corrupted zones, unstable terrain)

**2. Vigilance Calculation**

The `CombatEngine` calculates the **Vigilance** score for every combatant:

```
Vigilance = FINESSE + WITS
```

**What Vigilance Represents:**

- **Processing Speed:** How quickly a process (character/enemy) detects and responds to threats
- **Threat Awareness:** The ability to anticipate danger and act preemptively
- **Initiative Priority:** Higher Vigilance = earlier position in Turn Order

**3. Turn Order Creation**

All combatants are placed into a **static initiative list**, ordered from highest Vigilance to lowest. This order **will not change** for the duration of combat (no dynamic initiative rerolls).

**Tie-Breaking:** If two combatants have identical Vigilance scores:

1. Player characters act before NPCs/enemies
2. Among PCs: alphabetical by character name
3. Among NPCs: GM/system determines order

**4. UI Reconfiguration**

The CSI (Core System Interface) dynamically switches to its **Combat Layout**:

- **Turn Order Display:** Shows initiative sequence with current actor highlighted
- **Expanded Party Vitals:** HP, Stamina, AP (Action Points), active status effects for all visible combatants
- **Grid Visualization:** Abstract representation of positioning (Front/Back Row, sectors)
- **Combat Log:** Main narrative window displays all combat events with thematic flavor

**5. Combat Initiation Message**

```
> Hostile processes detected! Combat initiated! <
```

---

### The Combat Grid

**Specification Reference:**

Combat takes place on an **abstracted positioning grid** consisting of:

- **Player Zone:** Front Row + Back Row
- **Enemy Zone:** Front Row + Back Row

**Why Abstracted Grid?**

**Thematic Justification:**

- Reflects the "logical" nature of system processes organizing into attack/defend formations
- Avoids granular hex/square positioning that would be cumbersome in text interface
- Emphasizes tactical role (tank, ranged DPS, support) over precise measurement

**Gameplay Benefits:**

- Simple to communicate in text: "You are in the Front Row facing 3 enemies in their Front Row"
- Clear targeting rules: Front Row enemies block access to Back Row (usually)
- Enables meaningful positioning choices without overwhelming complexity

**Positioning Impact:**

A character's position on the grid is the **single most important tactical element**, dictating:

- **Targeting Options:** Who can you attack? Who can attack you?
- **Vulnerability:** Front Row = higher danger, Back Row = safer (but limited targeting)
- **Environmental Interaction:** Some hazards/cover only affect specific rows
- **Ability Requirements:** Some abilities require Front Row position, others work from Back Row

---

*Migration in progress from v2.0 Combat System specification. Remaining sections to be added incrementally.*

- **Ability Requirements:** Some abilities require Front Row position, others work from Back Row

---

## III. The Turn Cycle: The Flow of Execution

Combat proceeds in a **round-based, sequential order** based on the established Turn Order.

### The Round

A **round** is complete once every combatant in the Turn Order has taken their turn. After the last combatant acts, a new round begins with the first combatant in the Turn Order.

**Round Tracking:**

- Important for duration-based effects (e.g., "lasts 3 rounds")
- Some abilities have cooldowns measured in rounds
- Environmental effects may change each round

---

### The Turn

**Action Pool Specification:**

On a character's turn, they have a pool of resources to perform actions. By default, a character can perform:

- **One (1) Standard Action**
- **Variable number of Free Actions** (typically unlimited, but some may have restrictions)

**The Sequence of a Turn:**

**1. Start of Turn Phase**

Before the character can act, automatic processes occur:

**Passive Resource Regeneration:**

- **Stamina:** Regenerates by base amount (typically 10-20 per turn, build-dependent)
- **Unique Resources:** Some abilities/specializations have per-turn regeneration (e.g., "Regain 1 Runic Charge at start of turn")

**Status Effect Timers Tick Down:**

- Duration-based effects decrement by 1 turn
- Example: A 2-turn `[Stun]` becomes a 1-turn `[Stun]`
- Effects that reach 0 duration expire at the end of this phase

**Damage-Over-Time Effects Deal Damage:**

- `[Poison]`, `[Bleeding]`, `[Corroded]`, and similar effects deal their specified damage
- This damage is applied BEFORE the character can act
- **Critical:** DOT damage can potentially reduce a character to 0 HP before they can act on their turn

**2. Action Phase**

The character (or AI controlling the character) selects and performs their chosen action:

- **Attack:** Use a weapon to strike an enemy
- **Use Ability:** Activate a specialized combat ability (e.g., `Aether Dart`, `Shield Bash`, `Glitch-Dash`)
- **Defensive Action:** Use `block` (Standard Action)
- **Change Stance:** Switch between combat stances (typically Free Action)
- **Use Item:** Consume a potion, activate a Runic Charge item (action cost varies)
- **Move:** Change position on the grid (action cost varies by distance/circumstances)
- **Special Actions:** Context-dependent actions (e.g., interact with environmental object)

**3. End of Turn Phase**

After the character completes their action:

- **"End-of-Turn" Effects Trigger:** Some abilities/status effects have "at the end of your turn" triggers
- **Turn Passes:** The next combatant in the Turn Order becomes the active character

---

### The Action Economy

Understanding the action economy is critical to tactical play. Each action has a specific **action cost** that determines what can be done on a single turn.

**Standard Actions:**

The primary action for a turn. Most combat activities are Standard Actions:

- **Attacks:** Using a weapon to strike (`attack` command)
- **Most Abilities:** `Aether Dart`, `Shield Bash`, `Glitch-Dash`, `Hamstring Strike`
- **Active Defense:** `block` (requires shield)
- **Movement:** Moving between rows (typically)
- **Complex Interactions:** Using environmental objects, hacking terminals, etc.

**Rule:** You can perform **one (1) Standard Action** per turn (unless an ability/effect grants additional actions).

---

**Free Actions:**

Actions that do not consume the Standard Action for the turn. Free Actions are typically:

- **Quick:** Take minimal time/effort
- **Non-Aggressive:** Don't involve attacking or complex maneuvering
- **Self-Targeted:** Affect only the character performing the action

**Examples of Free Actions:**

- **Changing Stance:** Switch between Aggressive/Defensive/Balanced stance
- **Using Runic Charge Items:** Consuming a pre-charged item (the charging was done outside combat)
- **Riposte from Parry:** A successful `parry` (Reaction) may grant a Free Action counter-attack
- **Communication:** Speaking to allies (brief tactical commands)

**Restriction:** While technically "unlimited," Free Actions should be reasonable. A character cannot take 50 Free Actions in a single turn—the GM/system enforces narrative sense.

---

**Reactions:**

Actions that take place **outside of a character's turn** in response to a specific trigger. Reactions are the exception to the "wait for your turn" rule.

**Key Properties:**

- **Triggered:** Only usable when a specific event occurs (e.g., "when an enemy attacks you")
- **Limited:** A character typically has **one (1) Reaction per round** (resets at the start of their next turn)
- **Interrupting:** Reactions resolve immediately when triggered, interrupting the active character's turn

**Examples of Reactions:**

- **Parry:** When an enemy attacks you, attempt to deflect with weapon (`FINESSE` + bonuses vs. enemy's Accuracy)
- **Interposing Shield:** When an ally is attacked, jump in front to take the hit (requires positioning + shield)
- **Attack of Opportunity:** When an enemy attempts to flee/move past you (if specialization grants this)
- **Counter-Spell:** When an enemy casts a spell, attempt to disrupt it (requires specific ability)

**Timing:** Reactions are declared BEFORE the triggering event resolves. For example:

1. Enemy declares attack on you
2. You declare `parry` Reaction
3. Parry roll is resolved
4. If parry succeeds, attack is negated; if parry fails, attack proceeds normally

---

*Migration in progress from v2.0 Combat System specification. Remaining sections to be added incrementally.*

1. If parry succeeds, attack is negated; if parry fails, attack proceeds normally

---

## IV. Core Combat Mechanics & Resolution

### Action Resolution

**Specification:**

Every action with an **uncertain outcome** is resolved using the **Dice Pool system**.

The Dice Pool system is the universal resolution mechanic for all contested actions in Aethelgard. In combat, it is used for:

- **Hitting a Target:** Does your attack connect?
- **Dealing Damage:** How much damage does a successful hit inflict?
- **Resisting Effects:** Can you shrug off a debuff or status effect?

**See Also:** [AI Session Handoff — DB10 Specifications Migration](https://www.notion.so/AI-Session-Handoff-DB10-Specifications-Migration-065ef630b24048129cc560a393bd2547?pvs=21) for complete resolution rules.

---

### Combat-Specific Dice Pool Applications

**1. Hitting a Target: The Accuracy Roll**

When a character attempts to attack an enemy, they make an **Accuracy Roll** against the target's **Defense Score**.

**Accuracy Pool:**

```
Accuracy Pool = FINESSE + Bonuses
```

**Bonuses Include:**

- Weapon bonuses (e.g., +1 from enchanted weapon)
- Stance bonuses (e.g., +1 from Aggressive Stance)
- Status effects (e.g., +2 from `[Flanking]`)
- Specialization bonuses

**Target Number:**

```
Target Number = Target's Defense Score
```

**Resolution:**

- Roll the Accuracy Pool
- Count successes (dice showing 5 or 6)
- **If Successes ≥ Target Number:** Attack hits, proceed to Damage Roll
- **If Successes < Target Number:** Attack misses, no damage dealt

**See Also:** The Accuracy Check System specification (child mechanic) provides detailed rules for accuracy rolls, critical hits, and special attack types.

---

**2. Dealing Damage: The Damage Roll**

When an attack successfully hits, the attacker makes a **Damage Roll** to determine how much damage is inflicted.

**Damage Pool:**

```
Damage Pool = Weapon Dice + Attribute Bonus
```

**Components:**

- **Weapon Dice:** The weapon's base damage dice (e.g., 4d6 for a longsword)
- **Attribute Bonus:** Additional dice based on governing attribute:
    - **Melee Weapons:** +MIGHT dice
    - **Ranged Weapons:** +FINESSE dice
    - **Magic Weapons:** Varies by weapon/spell

**Resolution:**

- Roll the Damage Pool
- Count successes (dice showing 5 or 6)
- **Net Successes = Total Successes - Target's Soak**
- **Final Damage = Net Successes**

**Soak:**

Most enemies and armored characters have a **Soak** value that reduces incoming damage. Soak represents armor, natural toughness, or magical protection.

**Critical Rule:** Soak reduces successes, not dice. If an attack rolls 8 successes and the target has 3 Soak, the final damage is 5.

**See Also:** The Damage System specification (child mechanic) provides detailed rules for damage types, critical damage, and special damage effects.

---

**3. Resisting Effects: The Resolve Check**

When a character is targeted by a debuff, status effect, or other harmful effect, they may make a **Resolve Check** to resist it.

**Resolve Pool:**

The pool depends on the nature of the effect:

- **Physical Effects** (Poison, Bleeding, Knockdown): `STURDINESS + bonuses`
- **Mental Effects** (Fear, Charm, Confusion): `WILL + bonuses`

**Target Number:**

```
Target Number = Effect's Potency Pool
```

**Resolution:**

- Roll the Resolve Pool
- Count successes (dice showing 5 or 6)
- **If Successes ≥ Potency:** Effect is resisted (no effect applied)
- **If Successes < Potency:** Effect is applied with full duration/intensity

**Partial Resistance:**

Some effects allow **partial resistance**, where even if you fail the check, the number of successes you rolled reduces the effect's severity:

- **Duration Reduction:** Each success reduces duration by 1 turn
- **Intensity Reduction:** Each success reduces the effect's numerical impact

**See Also:** The Resolve Check System specification provides detailed rules for resistance, saving throws, and ongoing resistance checks.

---

## V. The Defensive Trinity

A character has **three primary methods** of mitigating incoming physical damage. Each has distinct properties, tactical applications, and trade-offs.

### 1. Evasion (Passive Defense)

**Type:** Passive, always active

**Mechanic:** Governed by the **Defense Score**, which serves as the Target Number for enemy Accuracy Rolls.

**Formula:**

```
Defense Score = 10 + FINESSE + Armor Bonuses + Status Effects
```

**What Evasion Represents:**

- **Not Being Hit:** The character's ability to not be where the attack lands
- **Agility & Reflexes:** Quick footwork, dodging, weaving
- **Spatial Awareness:** Anticipating attacks and moving preemptively

**Strengths:**

- **Always Active:** Doesn't require an action or Stamina expenditure
- **Protects Against All Attacks:** Every incoming attack must beat your Defense Score
- **Scales with FINESSE:** High-FINESSE characters are inherently harder to hit

**Weaknesses:**

- **Passive Only:** You cannot "choose" to dodge; it's automatic
- **No Counter-Attack:** Dodging doesn't create offensive opportunities
- **Bypassed by Area Effects:** Some attacks (AOE, environmental hazards) ignore Defense Score

**Tactical Application:**

- Primary defense for light-armor, high-FINESSE builds (rogues, duelists)
- Stacking Defense Score bonuses (stance, cover, buffs) creates "unhittable" characters
- Vulnerable to "sure-hit" abilities that bypass Accuracy Rolls

---

### 2. Blocking (Active Defense)

**Type:** Active, requires Standard Action

**Mechanic:** The character uses a **shield** and their **Block Pool** to negate incoming damage via an opposed roll.

**Requirements:**

- Character must have a shield equipped
- Character must use their Standard Action to `block`
- Shield must be available (not broken, not disarmed)

**Block Pool:**

```
Block Pool = STURDINESS + Shield Dice + Bonuses
```

**How Blocking Works:**

1. On your turn, declare `block` as your Standard Action
2. Until your next turn, you are in a "blocking stance"
3. When an enemy attacks you:
    - Enemy rolls Accuracy vs. your Defense Score (as normal)
    - If attack hits, **opposed Block Roll** occurs:
        - You roll your Block Pool
        - Enemy rolls their Damage Pool
        - **If Block Successes ≥ Damage Successes:** Damage is completely negated
        - **If Block Successes < Damage Successes:** You take (Damage - Block) damage

**What Blocking Represents:**

- **Interposing Shield:** Physically placing a barrier between you and harm
- **Bracing for Impact:** Using strength and shield weight to absorb force
- **Defensive Positioning:** Holding a strong defensive posture

**Strengths:**

- **Reliable Damage Reduction:** Even partial blocks reduce damage significantly
- **High STURDINESS Synergy:** Tanks with high STURDINESS can block enormous damage
- **Protects Allies:** Some abilities allow blocking attacks targeting allies

**Weaknesses:**

- **Consumes Standard Action:** You cannot attack and block in the same turn
- **Requires Shield:** Not available to all builds
- **Stamina Cost:** Blocking (especially multiple attacks) drains Stamina
- **Vulnerable to Bypass:** Some attacks cannot be blocked (psychic, environmental)

**Tactical Application:**

- Primary defense for tank builds (high STURDINESS, heavy armor, shields)
- Blocking enables "hold the line" tactics (protect allies by tanking damage)
- Must balance offensive pressure with defensive necessity

---

### 3. Parrying (Reactive Defense)

**Type:** Reaction, requires weapon

**Mechanic:** The character uses their **weapon** and their **Parry Pool** to deflect an incoming attack via an opposed Accuracy Roll, with the potential for a **Riposte** (counter-attack).

**Requirements:**

- Character must have a weapon equipped (that can parry—not all can)
- Character must have their Reaction available (1 per round)
- Must be targeted by a melee attack (ranged attacks typically cannot be parried)

**Parry Pool:**

```
Parry Pool = FINESSE + Weapon Bonuses + Specialization Bonuses
```

**How Parrying Works:**

1. Enemy declares a melee attack against you
2. You declare `parry` as your Reaction (before enemy rolls)
3. **Opposed Accuracy Roll** occurs:
    - Enemy rolls their Accuracy Pool
    - You roll your Parry Pool
    - **If Parry Successes ≥ Enemy Accuracy Successes:** Attack is deflected (no damage)
    - **If Parry Successes < Enemy Accuracy Successes:** Parry fails, attack proceeds normally (enemy's excess successes may even increase damage)
4. **If Parry Succeeds:** You may immediately make a **Riposte** (Free Action attack against the enemy)

**What Parrying Represents:**

- **Weapon Deflection:** Using your weapon to redirect the enemy's weapon
- **Martial Skill:** Precision timing and technique to counter attacks
- **Creating Openings:** Turning defense into offense

**Strengths:**

- **Offensive Potential:** Successful parry grants a Free Action counter-attack (Riposte)
- **Doesn't Consume Standard Action:** Uses Reaction, so you can still attack on your turn
- **High Skill Expression:** Rewards skilled players who anticipate enemy attacks

**Weaknesses:**

- **Risky:** Failed parry means attack proceeds normally (unlike blocking, which reduces damage even on failure)
- **Limited Uses:** Only 1 Reaction per round (must choose which attack to parry)
- **Requires High FINESSE:** Low FINESSE characters have low success rates
- **Restricted Target Types:** Cannot parry ranged attacks, magic, or area effects

**Tactical Application:**

- Primary defense for duelists, fencers, and high-FINESSE melee builds
- Parrying is a "high risk, high reward" option: succeed and gain offensive advantage; fail and take full damage
- Must choose which attacks to parry (limited to 1 per round)

---

## VI. The Trauma Economy in Combat

**Specification:**

Combat is a **primary source of Psychic Stress**. The Trauma Economy system tracks the mental cost of violence, death, and exposure to the chaotic/heretical nature of the world.

**Why Combat Causes Stress:**

**Thematic Justification:**

- Combat is **traumatic**, even for trained combatants
- Witnessing death, mutilation, and the glitching horrors of corrupted entities erodes mental stability
- The "coherent vs. chaos" theme: combat exposes characters to the chaotic nature of the broken world

**Gameplay Benefits:**

- Creates long-term consequences for combat encounters (not just HP/resource drain)
- Incentivizes avoiding unnecessary combat
- Reinforces the "horror" aspect of the setting

---

### Stress-Inducing Combat Events

**Triggers:**

The following events cause Psychic Stress during combat:

**Witnessing Critical Hits on Allies:**

- **Stress:** 5-10 points (severity depends on overkill damage)
- **Why:** Seeing a companion suffer catastrophic injury is deeply disturbing

**Companion Being Defeated (0 HP):**

- **Stress:** 10-20 points (higher if it's a close companion)
- **Why:** Watching a friend fall in battle is traumatic

**Being Targeted by `[Fear]` Effect:**

- **Stress:** Varies by effect (typically 5-15 points)
- **Why:** Supernatural fear/terror directly assaults the mind

**Being in a `[Psychic Resonance]` Zone:**

- **Stress:** 5 points per turn in the zone
- **Why:** Prolonged exposure to chaotic/corrupted areas erodes sanity

**Using Powerful Heretical Abilities:**

- **Stress:** Varies by ability (typically 10-30 points)
- **Why:** Channeling chaotic energies damages the caster's coherence

**Killing Sentient Beings (Humanoids):**

- **Stress:** 5-20 points (higher for first kill, innocents, etc.)
- **Why:** Taking a life, especially a humanoid life, is morally/psychologically heavy

---

### Implementation

**Event Broadcasting:**

The `CombatEngine` broadcasts events for stress-inducing triggers:

```
CombatEngine.OnCriticalHitWitnessed(ally, damage)
CombatEngine.OnCompanionDefeated(companion)
CombatEngine.OnFearEffectApplied(target, intensity)
```

**Trauma Service Listener:**

The `TraumaService` listens for these events and applies the appropriate Stress:

```
TraumaService.ApplyStress(character, amount, reason)
```

**Player Visibility:**

Stress gains should be communicated to the player in the combat log:

```
> [Trauma] Witnessing Kael's critical injury has shaken you. +10 Psychic Stress.
```

**See Also:** [**Feature Specification: The Fury Resource System**](https://www.notion.so/Feature-Specification-The-Fury-Resource-System-2a355eb312da80768e66db52bdd7cf19?pvs=21) for complete rules on Stress, Coherence, and mental state.

---

*Migration in progress from v2.0 Combat System specification. Remaining sections to be added incrementally.*

**See Also:** [**Feature Specification: The Fury Resource System**](https://www.notion.so/Feature-Specification-The-Fury-Resource-System-2a355eb312da80768e66db52bdd7cf19?pvs=21) for complete rules on Stress, Coherence, and mental state.

---

## VII. Combat End Conditions

Combat encounters end when one of three conditions is met: **Victory**, **Defeat**, or **Fleeing**.

### Victory

**Condition:** Combat ends when **all hostile entities have been reduced to 0 HP**.

**Resolution Sequence:**

**1. Victory Announcement:**

```
> All hostile processes terminated. Combat victory! <
```

**2. Legend Award:**

- The `CombatEngine` calculates Legend rewards based on:
    - Enemy difficulty/level
    - Combat performance (turns taken, damage dealt, allies surviving)
    - Trauma Modifier (if character has high Stress, Legend gain may be modified)
- Legend is awarded to all surviving party members
- Display: `> +250 Legend earned. <`

**3. Loot Generation:**

- The `LootService` generates loot based on:
    - Enemy type (humanoids drop equipment, constructs drop materials)
    - Loot tables (randomized or scripted drops)
    - Player choices (e.g., "Search the bodies" vs. "Leave immediately")
- Loot is added to party inventory or displayed for manual pickup

**4. State Transition:**

- The `GameEngine` transitions from `CombatState` back to `ExplorationState`
- The combat grid is cleared
- The CSI returns to Exploration Layout
- Party remains in current location (the room where combat occurred)

**5. Post-Combat Recovery:**

- Characters may use items, rest briefly, or continue exploring
- Status effects persist unless explicitly removed
- HP and Stamina do not automatically regenerate (requires rest or items)

---

### Defeat

**Condition:** Combat ends when **all player characters are in the `[System Crashing]` (0 HP) or `[Dead]` state**.

**Resolution Sequence:**

**1. Defeat Announcement:**

```
> System crash imminent. All processes terminated. <
```

**2. Death & Resurrection System Trigger:**

- The `DeathService` takes over and executes the Death & Resurrection protocol
- **The saga is not over**—death is not permanent in Aethelgard, but it carries significant penalties

**3. Respawn at Runic Anchor:**

- The party is returned to their **last visited Runic Anchor** (checkpoint/respawn point)
- This represents the party's "coherence" being restored at a stable anchor point in reality

**4. Penalties:**

**Durability Loss:**

- All equipped items suffer **massive Durability loss** (typically 20-40% of max)
- Items at 0 Durability are broken and non-functional until repaired
- This creates a significant economic penalty for death

**`[Frayed Echo]` Debuff:**

- All party members gain **one stack** of the `[Frayed Echo]` debuff
- **Effect:** Reduces max HP by 10% per stack, reduces Psychic Stress threshold by 5 per stack
- **Duration:** Persists until removed by special rest or ritual
- **Stacking:** Multiple deaths = multiple stacks = compounding penalties
- **Thematic Justification:** Each death "frays" the character's coherence, making them more vulnerable

**Psychic Stress:**

- Death typically awards 30-50 Psychic Stress (traumatic experience)
- Combined with `[Frayed Echo]`, this pushes characters toward mental instability

**5. State Transition:**

- The `GameEngine` transitions to `ExplorationState` at the Runic Anchor location
- Party must decide: Return to the location where they died? Rest and recover? Retreat to town?

---

### Fleeing

**Condition:** A player uses the `flee` command during combat, attempting to **escape the encounter**.

**Mechanic:**

**1. Flee Declaration:**

- On their turn, a character (or the party collectively) declares `flee`
- This initiates a **group skill check**

**2. Flee Check:**

**Party Flee Pool:**

```
Flee Pool = Average of (FINESSE + WITS) for all party members
```

**Enemy Vigilance Pool:**

```
Vigilance Pool = Highest Vigilance among enemies (or average, GM discretion)
```

**Resolution:**

- Roll Party Flee Pool vs. Enemy Vigilance Pool (opposed roll)
- **If Party Successes ≥ Enemy Successes:** Flee succeeds
- **If Party Successes < Enemy Successes:** Flee fails

**3A. Flee Success:**

- The party successfully escapes combat
- **Movement:** Party is moved back to the **previous room/zone** they were in (before entering combat)
- **Penalties:** Minor Stamina drain, potential morale/stress penalty
- **State Transition:** `GameEngine` transitions to `ExplorationState`
- **Display:**

```
> The party successfully flees from combat! You retreat to the previous area. <
```

**Tactical Implications:**

- Fleeing is a valid strategic choice when overwhelmed
- Allows party to regroup, heal, and prepare before re-engaging
- Some story encounters may prevent fleeing (scripted "you cannot escape" situations)

**3B. Flee Failure:**

- The flee attempt fails—the enemies are too fast, too numerous, or have blocked escape routes
- **Penalty:** All characters who attempted to flee **lose their turn** for the current round
- **Tactical Disaster:** This effectively gives enemies multiple free attacks
- **Display:**

```
> Escape attempt failed! The enemies block your retreat! <
```

**Tactical Implications:**

- Fleeing is risky—failed flee attempts can turn a difficult fight into a certain defeat
- Party must weigh: "Can we win this fight, or is fleeing worth the risk?"
- High Vigilance enemies are harder to flee from

---

## VIII. Integration & Implementation

### System Dependencies

The Combat System is an **orchestrator system** that integrates multiple child mechanics and services:

**Core Dependencies:**

**Foundation Systems:**

- [AI Session Handoff — DB10 Specifications Migration](https://www.notion.so/AI-Session-Handoff-DB10-Specifications-Migration-065ef630b24048129cc560a393bd2547?pvs=21): Universal resolution mechanic
- [AI Session Handoff — DB10 Specifications Migration](https://www.notion.so/AI-Session-Handoff-DB10-Specifications-Migration-065ef630b24048129cc560a393bd2547?pvs=21): Provides FINESSE, STURDINESS, MIGHT, WILL, WITS

**Child Mechanics (Combat Domain):**

- **Accuracy Check System:** Governs hit/miss resolution, critical hits, accuracy modifiers
- **Damage System:** Governs damage calculation, damage types, Soak, critical damage
- **Status Effect System:** Governs debuffs, buffs, duration tracking, effect stacking

**Referenced Systems:**

- [**Feature Specification: The Fury Resource System**](https://www.notion.so/Feature-Specification-The-Fury-Resource-System-2a355eb312da80768e66db52bdd7cf19?pvs=21): Combat triggers Psychic Stress
- **Equipment System:** Weapons, armor, shields provide dice pools and bonuses
- **Positioning System:** Grid layout, targeting rules, movement mechanics
- **AI System:** Enemy behavior, threat assessment, action selection
- **Death & Resurrection System:** Handles defeat condition

---

### Service Architecture

**`CombatEngine.cs` (Primary Service)**

**Responsibilities:**

- Manage the `CombatState` state machine
- Initialize combat encounters (grid, turn order, UI)
- Execute the turn cycle (Start of Turn → Action Phase → End of Turn)
- Resolve actions (attacks, abilities, movement)
- Track all combatants (HP, Stamina, status effects, positioning)
- Broadcast combat events (for Trauma, AI, UI)
- Determine combat end conditions (Victory, Defeat, Flee)

**State Management:**

The `CombatEngine` maintains a `CombatState` object containing:

- **Combatants List:** All active characters and enemies
- **Turn Order:** Static initiative list
- **Current Turn Index:** Which combatant is currently acting
- **Round Counter:** Current round number
- **Grid State:** Positioning of all combatants, environmental hazards
- **Environmental Conditions:** Weather, lighting, special effects
- **Combat Log:** History of all actions/events (for UI display)

**Key Methods:**

```
CombatEngine.InitializeCombat(enemies, environment)
CombatEngine.StartTurn(combatant)
CombatEngine.ExecuteAction(combatant, action)
CombatEngine.EndTurn(combatant)
CombatEngine.ApplyDamage(target, damage)
CombatEngine.ApplyStatusEffect(target, effect)
CombatEngine.CheckVictoryCondition()
CombatEngine.CheckDefeatCondition()
CombatEngine.AttemptFlee()
```

---

**`AI_Service.cs` (Enemy AI)**

**Responsibilities:**

- On an NPC's turn, the `CombatEngine` calls the `AI_Service`
- The `AI_Service` receives the current `CombatState`
- AI evaluates threat, positioning, abilities, and chooses optimal action
- Returns the chosen action to `CombatEngine` for execution

**AI Complexity Levels:**

- **Basic:** Simple targeting (attack nearest/weakest enemy)
- **Tactical:** Considers positioning, HP thresholds, ability synergies
- **Advanced:** Adapts to player strategy, uses environmental hazards, coordinates with allies

---

### UI Requirements (CSI - Core System Interface)

The CSI must clearly communicate **all aspects of combat** to the player. Combat is complex, and the interface must be both informative and readable.

**Combat Layout Elements:**

**1. Turn Order Display:**

- Shows initiative sequence (top to bottom: highest Vigilance → lowest)
- Current actor is highlighted
- Shows character names, HP bars (visual), active status effects (icons)

**2. Expanded Party Vitals:**

- For all visible combatants (player party + enemies in line of sight):
    - **HP:** Current/Max (e.g., "85/120 HP")
    - **Stamina:** Current/Max (e.g., "40/60 Stamina")
    - **Status Effects:** Icons/text for active debuffs/buffs (e.g., `[Poisoned]`, `[Blessed]`)
- Position on grid (e.g., "Front Row, Sector 2")

**3. Grid Visualization:**

- Abstract representation of positioning:

```
[PLAYER ZONE]
Front Row: [Kael] [Lyra] [Empty]
Back Row: [Theron] [Empty] [Empty]

[ENEMY ZONE]
Front Row: [Corrupted Guard] [Glitch Beast] [Corrupted Guard]
Back Row: [Heretic Mage]
```

**4. Combat Log (Main Narrative Window):**

- Displays all combat events with **thematic flavor**:

```
> Round 2 - Kael's turn <
> Kael attacks Corrupted Guard with [Runed Longsword]! <
> Accuracy Roll: 6 successes vs. Defense 5. Hit! <
> Damage Roll: 4 successes - 2 Soak = 2 damage dealt! <
> Corrupted Guard: 18/20 HP remaining. <
```

**5. Available Actions Panel:**

- Lists all available actions for the current character:
    - **Standard Actions:** Attack, Use Ability, Block, Move
    - **Free Actions:** Change Stance, Use Item
    - **Reactions:** Parry (if available)
- Shows Stamina costs and requirements

---

### Implementation Notes

**Performance Considerations:**

- Combat encounters may involve 10+ combatants (4 players + 6+ enemies)
- Each turn involves multiple dice rolls, status effect checks, and UI updates
- The `CombatEngine` must be optimized for frequent state updates

**Testing Requirements:**

- **Unit Tests:** Individual combat mechanics (accuracy, damage, status effects)
- **Integration Tests:** Full combat scenarios (party vs. enemies, various builds)
- **Edge Cases:** Flee attempts, simultaneous defeats, environmental hazards

**Extensibility:**

- Combat system must support future additions:
    - New status effects (easily pluggable)
    - New enemy types (AI behavior variants)
    - Environmental interactions (destructible terrain, dynamic hazards)
    - Multiplayer support (if applicable)

---

## Migration Status: COMPLETE

**Date Migrated:** 2025-11-08

**Source:** v2.0 Combat System Feature Specification

**Target:** DB10 Combat System — Core System Specification v5.0

**Status:** ✅ Draft Complete

**All sections migrated:**

- ✅ I. Core Philosophy
- ✅ II. Anatomy of an Encounter
- ✅ III. Turn Cycle
- ✅ IV. Core Combat Mechanics & Resolution
- ✅ V. Defensive Trinity
- ✅ VI. Trauma Economy in Combat
- ✅ VII. Combat End Conditions
- ✅ VIII. Integration & Implementation

**Next Steps:**

1. Create child mechanic specs (Accuracy Check, Damage, Status Effect)
2. Review for completeness and consistency
3. Submit for validation against v2.0 source