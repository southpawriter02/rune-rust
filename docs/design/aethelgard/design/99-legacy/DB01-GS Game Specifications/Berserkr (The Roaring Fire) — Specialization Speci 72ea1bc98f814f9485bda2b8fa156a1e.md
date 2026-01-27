# Berserkr (The Roaring Fire) — Specialization Specification v5.0

Type: Specialization
Priority: Must-Have
Status: In Design
Archetype Foundation: Warrior
Balance Validated: No
Document ID: AAM-SPEC-CLASS-BERSERKR-v5.0
Mechanical Role: Burst Damage, Damage Dealer
Primary Creed Affiliation: Independents
Proof-of-Concept Flag: Yes
Resource System: Charges/Uses, Stamina
Sub-Type: Combat
Template Compliance: v5.0 Three-Tier Template
Template Validated: No
Trauma Economy Risk: High
Voice Layer: Layer 2 (Diagnostic)
Voice Validated: No

### **I. Core Philosophy: The Roaring Fire**

The **Berserkr** is the Warrior specialization that embodies the heretical philosophy of **channeling the world's trauma into pure, untamed physical power**. They are not disciplined soldiers; they are **roaring fires** of destruction. They have learned to open their minds to the violent, screaming psychic static of the Great Silence, using that chaotic feedback to fuel a battle-lust that pushes their bodies far beyond their normal limits.

To choose the Berserkr is to embrace the fantasy of the unstoppable barbarian and the high-risk, high-reward damage engine. Your saga is written in the blood of your enemies and the scars on your own body. You are a terrifying whirlwind of pure, untamed destruction, a warrior who abandons tactical finesse and personal safety for the promise of overwhelming, glorious violence.

### **II. Player-Facing Presentation**

- **Role:** Melee Damage Engine, Shock Trooper, Crowd Clearer
- **Primary Attribute:** MIGHT (for raw, overwhelming damage)
- **Secondary Attribute:** STURDINESS (to provide the HP pool needed to survive their reckless style)
- **Gameplay Feel:** The Berserkr is a visceral, aggressive, and momentum-based damage dealer. Their gameplay is a satisfying loop of building their unique **Fury** resource by both dealing and taking damage, and then spending that Fury on spectacular, devastating abilities. They are the definition of a high-risk, high-reward specialist who trades defense for the highest sustained melee damage output in the game. Playing a Berserkr is about managing the razor's edge between overwhelming power and catastrophic collapse—the longer the fight, the more dangerous you become, but also the closer you teeter to mental breakdown.

### **III. Mechanical Implementation: The Art of Rage**

The Berserkr's power is entirely derived from their unique resource and their reckless combat style.

**A. Core Mechanic: Fury (Unique Resource)**

- **Feature:** The Berserkr is fueled by **Fury**.
- **Specification:**
    - **Resource Bar:** A unique resource bar from 0 to 100.
    - **Generation:** Fury is gained in two ways:
        1. **Dealing Damage:** Every successful hit with a Berserkr ability generates a small amount of Fury.
        2. **Taking Damage:** Every point of HP damage the Berserkr takes generates Fury. This is their primary generation method.
    - **Spending:** Fury is the required resource for their most powerful, high-impact abilities.
    - **Decay:** Fury does not decay during combat but is completely lost upon a **Sanctuary Rest**.
- **Rationale:** Creates a unique feedback loop where the Berserkr becomes more dangerous as combat intensifies. The dual generation method (dealing and taking damage) rewards aggressive positioning and accepting punishment.

**B. The Trauma Economy Interface**

- **Feature:** The Berserkr's Fury is a direct interface with the Trauma Economy—a heretical channeling of the world's psychic scream.
- **Specification:** While the Berserkr has **any amount of Fury**, they suffer a **2 dice penalty** to all **WILL-based Resolve Checks**.
- **Rationale:** The higher their Fury, the louder the scream in their head, but the more powerful they become. This makes them exceptionally vulnerable to psychic attacks and fear effects—a critical weakness that balances their overwhelming offensive power.

### **IV. The Skill Tree: The Path of Carnage**

The Berserkr's skill tree is a toolkit for generating Fury and unleashing carnage.

### **Tier 1 (Foundational Fury)**

*Prerequisite: Unlock the Berserkr Specialization (10 PP)*

- **`Primal Vigor I`** (Passive)
    - **Description:** The Berserkr's very physiology is tied to their rage. As their fury builds, their body surges with adrenaline, accelerating recovery.
    - **Mechanics:**
        - **Effect:** For every 25 **Fury** the Berserkr currently has, they gain a stacking bonus to their **Stamina Regeneration**.
        - **Breakpoints:** At 25/50/75/100 Fury, stamina regeneration increases progressively.
        - **Rationale:** Sustains combat presence during long encounters. Rewards Fury accumulation beyond immediate spending.
- **`Wild Swing`** (Active)
    - **Description:** The Berserkr unleashes a wide, reckless swing, caring little for precision and focusing only on widespread destruction.
    - **Mechanics:**
        - **Cost:** 40 Stamina.
        - **Effect:** Deals moderate Physical damage to all enemies in the **Front Row**.
        - **Fury Generation:** Generates **5 Fury** for each enemy hit.
        - **Rationale:** Primary AoE Fury builder. Scales with enemy count.
- **`Reckless Assault`** (Active)
    - **Description:** Lowering their guard completely, the Berserkr lunges forward to deliver a powerful, single-target attack.
    - **Mechanics:**
        - **Cost:** 35 Stamina.
        - **Effect:** Deals high Physical damage to a single target.
        - **Fury Generation:** Generates **15 Fury**.
        - **Drawback:** For 1 round, the Berserkr suffers from **[Vulnerable]**, increasing the damage they take.
        - **Rationale:** High Fury generation at the cost of survivability. Embodies reckless trade-off.

### **Tier 2 (Advanced Carnage)**

*Prerequisite: 8 PP spent in the Berserkr tree*

- **`Unleashed Roar`** (Active)
    - **Description:** The Berserkr lets out a terrifying, guttural war cry, challenging a single foe to face their wrath.
    - **Mechanics:**
        - **Cost:** 30 Stamina, 20 Fury.
        - **Effect:** **Taunts** a single enemy for 2 rounds. If the taunted enemy attacks the Berserkr, the Berserkr instantly generates **10 Fury**.
        - **Rationale:** Hybrid offensive/defensive ability. Converts incoming attacks into fuel.
- **`Whirlwind of Destruction`** (Active)
    - **Description:** An evolution of Wild Swing—a spinning vortex of pure destruction that reaches across the entire battlefield.
    - **Mechanics:**
        - **Cost:** 50 Stamina, 30 Fury.
        - **Effect:** Deals high Physical damage to all enemies in **both the Front and Back rows**.
        - **Rationale:** Ultimate AoE clear. Expensive but battlefield-wide impact.
- **`Blood-Fueled`** (Passive)
    - **Description:** Pain is a catalyst. Every wound is an invitation to greater violence. The Berserkr has learned to transform suffering into power.
    - **Mechanics:**
        - **Effect:** **Doubles the amount of Fury gained from taking damage**.
        - **Rationale:** Fundamentally alters the Berserkr's strategy. Rewards tanking hits and enables aggressive positioning in dangerous situations.

### **Tier 3 (Mastery of Rage)**

*Prerequisite: 20 PP spent in the Berserkr tree*

- **`Hemorrhaging Strike`** (Active)
    - **Description:** Focusing their rage into a single, savage blow, the Berserkr opens a grievous injury that will bleed the enemy dry.
    - **Mechanics:**
        - **Cost:** 45 Stamina, 40 Fury.
        - **Effect:** Deals very high Physical damage to a single target and applies a potent **[Bleeding]** effect.
        - **Duration:** Bleed lasts for 3 rounds.
        - **Rationale:** High single-target burst with sustained damage component.
- **`Death or Glory`** (Passive)
    - **Description:** The Berserkr fights with the greatest ferocity when on the brink of death. Desperation fuels transcendent rage.
    - **Mechanics:**
        - **Effect:** While the Berserkr is **[Bloodied]** (below 50% HP), all abilities that generate Fury now generate **50% more**.
        - **Rationale:** Rewards high-risk play. The closer to death, the faster Fury accumulates.

### **Capstone (Ultimate Expression)**

*Prerequisite: 40 PP spent in the Berserkr tree*

- **`Unstoppable Fury`** (Passive)
    - **Description:** The Berserkr's rage transcends mere emotion and becomes a force of nature, allowing them to defy death itself through sheer will and fury.
    - **Mechanics:**
        - **Effect:** This passive grants two powerful benefits:
            1. The Berserkr gains **Immunity to [Feared]** and **[Stunned]** effects.
            2. The first time the Berserkr would be reduced to 0 HP in combat, their **Health is instead set to 1**, and they instantly gain **100 Fury**. This can only occur once per combat.
        - **Rationale:** Ultimate survival tool balanced by once-per-combat limit. Creates dramatic "last stand" moments.

### **V. Systemic Integration**

**1. Core Role & Fantasy Fulfillment**

- The Berserkr is the premier sustained melee damage dealer in the game, especially against groups of enemies.
- Fantasy delivered: Unstoppable barbarian, terrifying whirlwind of destruction, warrior who channels trauma into power.

**2. Trauma Economy Interaction**

- **Heretical Specialization:** The Berserkr is a heretical Warrior who directly interfaces with the Trauma Economy.
- **Vulnerability:** Their constant Fury penalty (-2 dice to WILL Resolve Checks) is their greatest weakness.
- **Self-Inflicted Risk:** Unlike most Warriors, the Berserkr becomes more mentally vulnerable as they become more physically powerful.
- **Dependency:** Requires support from Bone-Setter or Skald to manage inevitable psychic attacks and mental debuffs.

**3. Situational Power Profile**

- **Optimal Conditions:**
    - Long, brutal encounters where Fury can accumulate
    - Facing multiple enemies (Wild Swing, Whirlwind of Destruction scale with count)
    - Attrition-based fights where taking damage is inevitable
    - Party with strong healers and tanks who can keep Berserkr alive
- **Weakness Conditions:**
    - Short burst encounters (insufficient time to build Fury)
    - Psychic-heavy enemies (vulnerable due to WILL penalty)
    - Solo play (no support to manage weaknesses)
    - Enemies with heavy crowd control (vulnerable to fear/stun without capstone)
- **Hard Counters:** Enemies applying [Bleeding] (turns Berserkr's HP pool into Fury generation resource)
- **Hard Countered By:** Psychic attackers, fear-focused enemies, burst damage that kills before Fury builds

**4. Synergies**

- **Positive Synergies:**
    - **Skjaldmær** (tank): Can Taunt enemies and absorb damage the Berserkr is ill-equipped to handle
    - **Bone-Setter** (healer): Keeps Berserkr alive during their reckless assaults and manages Psychic Stress
    - **Skald** (buffer): Saga of Courage provides immunity to [Feared], covering Berserkr's main weakness
    - **Controllers** (Thul, Jötun-Reader): Debuff enemies so Berserkr can focus on damage output
- **Negative Synergies:**
    - Other "selfish" damage dealers compete for healer attention
    - Fragile party compositions can't support Berserkr's reckless style

**5. Archetype Fulfillment**

- **Archetype:** Warrior
- **Sub-Role:** Pure Damage Dealer (Melee AoE Specialist), with secondary role as Shock Trooper
- **Warrior Identity:** Straightforward, visceral combat with clear risk/reward dynamics. Entire kit is a feedback loop: the more brutal the fight, the more powerful they become.