# Skald (Chronicler of Coherence) — Specialization Specification v5.0

Type: Specialization
Priority: Must-Have
Status: In Design
Archetype Foundation: Adept
Balance Validated: No
Document ID: AAM-SPEC-CLASS-SKALD-v5.0
Mechanical Role: Controller/Debuffer, Support/Healer, Utility/Versatility
Primary Creed Affiliation: Independents, Multiple/Neutral
Proof-of-Concept Flag: Yes
Resource System: Stamina
Sub-Type: Support
Template Compliance: v5.0 Three-Tier Template
Template Validated: No
Trauma Economy Risk: Low
Voice Layer: Layer 2 (Diagnostic)
Voice Validated: No

### **I. Core Philosophy: The Anchor of a Broken Narrative**

The **Skald** is the Adept specialization that embodies the philosophy that in a world whose own story has been shattered, **a coherent narrative is a weapon and a shield**. They are not mystics, but coherence-keepers—the warrior-poets who understand that stories are a source of immense, tangible power. Through structured verse and rousing chants, they create a temporary "narrative firewall," a pocket of logic, heroism, and meaning that can fortify an ally's mind against the psychic static of the Great Silence or break an enemy's morale with the weight of a foreseen doom.

To choose the Skald is to embrace the fantasy of the inspirational leader and the keeper of legends. Your saga is written in the courage you instill in your allies and the despair you inflict upon your foes. You are the heart of the clan, the voice of your people, and the living memory of a shattered world.

### **II. Player-Facing Presentation**

- **Role:** Morale Officer, Combat Buffer/Debuffer, Living Encyclopedia
- **Primary Attribute:** WILL (for the force of personality and conviction of their performance)
- **Secondary Attribute:** WITS (for their vast repository of lore and sagas)
- **Gameplay Feel:** The Skald is a pure, dedicated support class whose power is entirely auditory. They are a "non-magical bard." In combat, they are a back-row performer who spends their turns initiating and maintaining powerful, party-wide buffs (Sagas) or debilitating enemy-wide debuffs (Dirges). Playing a Skald is a game of tempo and choosing the right "song" for the right moment in the battle. The Skald's strength lies in their ability to shape the psychological battlefield—creating pockets of coherence in a world of chaos.

### **III. Mechanical Implementation: The Art of the Performance**

The Skald's power is expressed through a unique channeling mechanic called a Performance.

**A. Core Mechanic: Performance**

- **Feature:** The Skald's primary abilities are **Performances** (Sagas and Dirges).
- **Specification:**
    1. Initiating a Performance is a **Standard Action** and costs **Stamina**.
    2. Once initiated, the Skald enters the **[Performing]** status—a channeled state.
    3. The Saga or Dirge's effect is an **aura** that remains active as long as the Skald maintains their Performance.
    4. Duration: The Skald can maintain a Performance for a number of rounds equal to their **WILL** score.
    5. While [Performing], the Skald can move and use simple items, but cannot use another Standard Action (like a weapon attack) without ending their Performance.
- **Interruption:** A Skald who is [Stunned] or [Silenced] has their Performance immediately interrupted.
- **Rationale:** This creates a tactical choice—the Skald trades personal combat actions for sustained party-wide effects, embodying the support class fantasy while requiring strategic positioning and protection from allies.

**B. Core Mechanic: Circle-Craft (Narrative Containment)**

- **Feature:** The Skald's performances create pockets of coherence—structured narrative zones that resist the psychic entropy of the Blight.
- **Specification:** Performances are auditory auras. All allies "within earshot" (same battlefield) benefit. The Skald becomes a mobile anchor point of stability, using structured verse and cadence to synchronize their allies' mental states.
- **Rationale:** Mechanically straightforward (battlefield-wide buffs/debuffs), but thematically rich. The Skald doesn't cast spells—they perform structured narratives that impose order on chaos.

### **IV. The Skill Tree: The Repertoire of a Legend**

The Skald's skill tree is a repertoire of songs and sagas that manipulate the morale and psychological state of the battlefield.

### **Tier 1 (Foundational Performance)**

*Prerequisite: Unlock the Skald Specialization (10 PP)*

- **`Oral Tradition I`** (Passive)
    - **Description:** The Skald's life is one of constant recitation and learning. The great sagas are a part of their very being, carried in verse and cadence.
    - **Mechanics:**
        - **Effect:** Provides a permanent **bonus die (+1d10)** to all **Rhetoric** skill checks and to `investigate` checks made to decipher historical lore.
        - **Rationale:** Establishes the Skald as the party's social face and lorekeeper.
- **`Saga of Courage`** (Active Performance)
    - **Description:** The Skald begins a rousing chant—a tale of a hero who stood against overwhelming odds and did not falter. The structured verse creates a pocket of coherence, steadying allies' breath and resolve.
    - **Mechanics:**
        - **Cost:** 40 Stamina
        - **Effect:** While performing, all allies within earshot are **Immune to [Feared]** and gain a **+1 die bonus** to all WILL-based Resolve Checks to resist Psychic Stress.
        - **Duration:** Maintained for rounds equal to WILL score (can be extended by Enduring Performance passive).
        - **Rationale:** Core anti-Trauma Economy tool. Proactive defense against mental assault.
- **`Dirge of Defeat`** (Active Performance)
    - **Description:** The Skald's voice drops to a low, sorrowful dirge—recounting a tale of a great army that marched to its doom. The narrative weight unnerves intelligent foes.
    - **Mechanics:**
        - **Cost:** 40 Stamina
        - **Effect:** While performing, all intelligent enemies suffer a **1 die penalty** to their Accuracy and damage rolls.
        - **Targeting Restriction:** Does not affect mindless beasts or Undying (requires intelligence to comprehend narrative).
        - **Duration:** Maintained for rounds equal to WILL score.
        - **Rationale:** Area-of-effect debuff that rewards understanding of enemy types.

### **Tier 2 (Advanced Composition)**

*Prerequisite: 8 PP spent in the Skald tree*

- **`Rousing Verse`** (Active)
    - **Description:** The Skald speaks a quick, sharp verse from a saga about a tireless warrior, banishing fatigue through structured recollection.
    - **Mechanics:**
        - **Cost:** Standard Action, 35 Stamina (This is NOT a Performance)
        - **Effect:** Instantly restores a moderate amount of **Stamina** to a single ally.
        - **Formula:** Base restore + (Skald's WILL × 2)
        - **Rationale:** Non-Performance utility. Gives Skald a direct action option when not performing.
- **`Song of Silence`** (Active)
    - **Description:** The Skald begins a complex, counter-resonant chant—a structured pattern designed to disrupt hostile vocalizations and choke a spellcaster's words in their throat.
    - **Mechanics:**
        - **Cost:** Standard Action, 45 Stamina
        - **Effect:** An opposed **WILL + Rhetoric** check against a single intelligent enemy. If successful, inflicts **[Silenced]** for 2 rounds.
        - **Targeting Restriction:** Must target intelligent enemy capable of vocalization.
        - **Rationale:** Anti-caster tool. Creates tactical choice between maintaining performance or interrupting enemy casters.
- **`Enduring Performance`** (Passive)
    - **Description:** The Skald has honed their vocal endurance through rigorous practice, able to maintain their powerful performances for longer.
    - **Mechanics:**
        - **Effect:** The maximum duration of all of the Skald's Performances is **increased by 2 rounds**.
        - **Rationale:** Core passive that extends the Skald's impact. Essential for longer encounters.

### **Tier 3 (Mastery of the Saga)**

*Prerequisite: 20 PP spent in the Skald tree*

- **`Lay of the Iron Wall`** (Active Performance)
    - **Description:** The Skald tells the story of the unbreakable shield wall at the Battle of the Black Pass, inspiring their allies to hold the line as one. The narrative imposes structural coherence on their formation.
    - **Mechanics:**
        - **Cost:** 55 Stamina
        - **Effect:** While performing, all allies in the **Front Row** gain a significant **+2 bonus to their Soak value**.
        - **Duration:** Maintained for rounds equal to WILL score.
        - **Rationale:** Powerful defensive performance. Rewards positional play and formation tactics.
- **`Heart of the Clan`** (Passive)
    - **Description:** The Skald is the living heart of their clan. Their very presence is a source of unshakeable resolve—a constant recitation that steadies those who stand beside them.
    - **Mechanics:**
        - **Effect:** All allies standing in the same row as the Skald gain a permanent, small bonus (**+1 die**) to all their defensive Resolve Checks (to resist Fear, Disoriented, etc.).
        - **Condition:** Skald must not be [Stunned], [Feared], or [Silenced].
        - **Rationale:** Formation anchor. Encourages positional play and rewards protecting the Skald.

### **Capstone (Ultimate Expression)**

*Prerequisite: 40 PP spent in the Skald tree*

- **`Saga of the Einherjar`** (Active Performance)
    - **Description:** The Skald unleashes their masterpiece—a booming, epic saga of the greatest heroes of old. Their voice is filled with such power that their allies believe themselves to be those very heroes, temporarily elevated to a legendary state.
    - **Mechanics:**
        - **Cost:** 75 Stamina. Once per combat.
        - **Effect:** While performing, all allies gain:
            1. **[Inspired]** status effect (+3 bonus to damage dice pools)
            2. **Large amount of temporary HP** (removed when saga ends)
        - **Duration:** Maintained for rounds equal to WILL score.
        - **Psychic Cost:** When the performance ends, all allies who were affected suffer a **moderate amount of Psychic Stress** (representing the traumatic "comedown" from their temporary apotheosis).
        - **Rationale:** Ultimate "burn phase" cooldown. Massive power spike balanced by once-per-combat limit and Trauma Economy cost.

### **V. Systemic Integration**

**1. Core Role & Fantasy Fulfillment**

- The Skald is the ultimate party-focused support specialist. They have minimal personal damage output but dramatically increase party effectiveness through sustained buffs, debuffs, and morale management.
- Fantasy delivered: Inspirational leader, keeper of legends, "heart of the clan."

**2. Trauma Economy Interaction**

- **Coherent Specialization:** The Skald has no self-inflicted Psychic Stress or Corruption mechanics (except the intentional cost of Saga of the Einherjar).
- **Proactive Defense:** Saga of Courage provides **immunity to [Feared]** and bonus dice to resist Psychic Stress—making the Skald the party's primary proactive defense against the Trauma Economy.
- **Complementary to Bone-Setter:** The Skald prevents Stress; the Bone-Setter heals it. Together they form complete Trauma Economy management.

**3. Situational Power Profile**

- **Optimal Conditions:**
    - Long encounters where sustained buffs/debuffs provide maximum value
    - Facing intelligent humanoid enemies (Dirge of Defeat, Song of Silence are maximally effective)
    - Party with high-damage dealers who benefit from [Inspired] and accuracy buffs
- **Weakness Conditions:**
    - Short encounters (no time to leverage sustained performances)
    - Facing mindless beasts or Undying (Dirge of Defeat and Song of Silence lose effectiveness)
    - Solo play (all abilities are party-focused)
- **Hard Counters:** Enemy Galdr-casters, rival Skald-Screamers (Song of Silence is premier counter)
- **Hard Countered By:** Enemies with [Silenced] or [Stunned] applications (interrupts performances)

**4. Synergies**

- **Positive Synergies:**
    - **High-damage dealers** (Berserkr, Strandhögg, Vargr-Born): Maximize value from [Inspired] buffs
    - **Fragile casters/supports** (Bone-Setter, Jötun-Reader, Echo-Caller): Benefit from Saga of Courage's Psychic Stress resistance
    - **Formation-based Warriors** (Skjaldmær, Hólmgangr): Benefit from Heart of the Clan and Lay of the Iron Wall
    - **Bone-Setter**: Complete Trauma Economy management (Skald prevents, Bone-Setter heals)
- **Negative Synergies:**
    - Other buff-focused supports may create redundancy
    - Solo-focused specializations (Einbúi, Ruin-Stalker) gain less from party-wide buffs

**5. Archetype Fulfillment**

- **Archetype:** Adept
- **Sub-Role:** Pure Support (Buffer/Debuffer), with secondary roles as Face (Oral Tradition) and Lorekeeper
- **Adept Identity:** Non-magical, skill-based, requires preparation (choosing right performance for situation) and tactical awareness (positioning, timing)