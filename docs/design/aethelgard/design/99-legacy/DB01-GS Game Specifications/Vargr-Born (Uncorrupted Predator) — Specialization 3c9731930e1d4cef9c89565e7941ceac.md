# Vargr-Born (Uncorrupted Predator) — Specialization Specification v5.0

Type: Specialization
Priority: Must-Have
Status: In Design
Archetype Foundation: Warrior
Balance Validated: No
Document ID: AAM-SPEC-CLASS-VARGRBORN-v5.0
Mechanical Role: Controller/Debuffer, Damage Dealer
Primary Creed Affiliation: Independents
Proof-of-Concept Flag: Yes
Resource System: Charges/Uses, Stamina
Sub-Type: Combat
Sub-item: Tier 1 Ability: Primal Senses (Tier%201%20Ability%20Primal%20Senses%20b5e961556ab04ce5ac6a090705a1fb2e.md), Tier 1 Ability: Savage Claws (Tier%201%20Ability%20Savage%20Claws%209493c91b66424aab8eda09d39fd8ec0c.md), Tier 1 Ability: Predatory Lunge (Tier%201%20Ability%20Predatory%20Lunge%200dff12d95c4c4b1a904271d17b35da0e.md), Tier 2 Ability: Terrifying Howl (Tier%202%20Ability%20Terrifying%20Howl%20e156baf18ff64afbb2ba8b30a2e93a4a.md), Tier 2 Ability: Go for the Throat (Tier%202%20Ability%20Go%20for%20the%20Throat%205db6b66561df4ad7a208dc0314c08565.md), Tier 2 Ability: Taste for Blood (Tier%202%20Ability%20Taste%20for%20Blood%203463cfe9420e4578b8fa05492f088976.md), Tier 3 Ability: Feral Maelstrom (Tier%203%20Ability%20Feral%20Maelstrom%20e5585b0cbd2c44aaae7c43a2e46d04c6.md), Tier 3 Ability: Wounded Animal's Ferocity (Tier%203%20Ability%20Wounded%20Animal's%20Ferocity%20494119323e7d4ae8a0be6b615bae186d.md), Capstone Ability: Aspect of the Great Wolf (Capstone%20Ability%20Aspect%20of%20the%20Great%20Wolf%2094a832e9b5e544a089a029d0ce58996b.md)
Template Compliance: v5.0 Three-Tier Template
Template Validated: No
Trauma Economy Risk: None
Voice Layer: Layer 2 (Diagnostic)
Voice Validated: No

### **I. Core Philosophy: The Uncorrupted Echo**

The **Vargr-Born** is the Warrior specialization that embodies the philosophy of the **uncorrupted, primal echo**. They are not heretics who channel the Blight, nor are they disciplined soldiers. Their bloodline has become fused with a sliver of the world's **original, stable source code**—an ancient, predatory "wolf" spirit that is pure, logical, and utterly savage. This makes them a living **counter-frequency** to the Great Silence. Their rage is not the chaotic feedback of the Blight, but the focused, primal fury of a stable system violently rejecting a virus.

To choose the Vargr-Born is to embrace the fantasy of the feral brawler and the psychological predator. Your saga is written in the bleeding wounds you inflict and the terror you sow with your howl. Your howl is not just sound; it is a blast of pure, coherent reality that can shatter a Blighted creature's fractured mind. You are the old world's antibody, a savage cure for a modern disease.

### **II. Player-Facing Presentation**

- **Role:** Feral Brawler, Primal Predator, Psychological Warfare Specialist
- **Primary Attribute:** MIGHT (for the raw, savage power of their unarmed strikes)
- **Secondary Attribute:** FINESSE (for their predatory speed and grace)
- **Gameplay Feel:** The Vargr-Born is an aggressive, relentless, and self-sufficient melee damage dealer. Their gameplay is a satisfying loop of building their unique **Feral Fury** resource by inflicting **[Bleeding]** wounds, and then spending that Fury to unleash savage, high-damage attacks or a terrifying, morale-breaking howl. They are a "predator" class, excelling at hunting down and finishing off a single, bleeding target. The Vargr-Born rewards patient stalking followed by explosive violence—identify the prey, open the wound, and pursue until the kill.

### **III. Mechanical Implementation: The Art of the Hunt**

The Vargr-Born's power is derived from their unique resource and their ability to weaponize their primal nature.

**A. Core Mechanic: Feral Fury (Unique Resource)**

- **Feature:** The Vargr-Born is fueled by **Feral Fury**.
- **Specification:**
    - **Resource Bar:** A unique resource bar from 0 to 100.
    - **Generation:** Fury is gained by:
        1. **Dealing Damage:** Successfully hitting with their abilities generates base Fury.
        2. **Inflicting Bleeds:** Gaining a significant bonus to Fury generation whenever they apply or deal damage with a [Bleeding] effect.
    - **Spending:** Fury is the required resource for their most powerful abilities.
    - **Decay:** Fury decays slowly outside of combat and is lost upon a **Sanctuary Rest**.
- **Rationale:** Creates a hunting pattern—apply bleed, generate Fury from bleed damage, spend Fury on finishers. Rewards focus on single targets.

**B. Core Mechanic: Primal Counter-Resonance**

- **Feature:** The Vargr-Born weaponizes their "coherent" nature to inflict psychological trauma on corrupted enemies.
- **Specification:** Their `Terrifying Howl` is a blast of pure, uncorrupted primal energy. To a mind already struggling with the Blight's static, this sudden introduction of a "clean" signal is a psychic shock that inflicts **[Fear]** and **Psychic Stress**.
- **Rationale:** Inverts the Trauma Economy—the Vargr-Born uses their coherence as a weapon against the corrupted, rather than being vulnerable to corruption themselves.

### **IV. The Skill Tree: The Predator's Path**

The Vargr-Born's skill tree is a toolkit for bleeding their prey and breaking their will.

### **Tier 1 (Foundational Ferocity)**

*Prerequisite: Unlock the Vargr-Born Specialization (10 PP)*

- **`Primal Senses`** (Passive)
    - **Description:** The Vargr-Born's senses are attuned to the pure, logical rhythms of the uncorrupted world. They can track by scent and instinct where others must rely on sight.
    - **Mechanics:**
        - **Effect:** Provides a **bonus die (+1d10)** to all **Wasteland Survival (Tracking)** checks.
        - **Mental Clarity:** Grants a bonus die to Resolve Checks to resist the **[Disoriented]** status effect.
        - **Rationale:** Establishes primal awareness and coherence. Makes Vargr-Born excellent scouts.
- **`Savage Claws`** (Active)
    - **Description:** The Vargr-Born lashes out with their claw-like hands in a blur of primal motion, tearing flesh and opening wounds.
    - **Mechanics:**
        - **Cost:** 40 Stamina.
        - **Attack Type:** MIGHT-based **unarmed** melee attack.
        - **Effect:** On a Solid or Critical Success, also applies the **[Bleeding]** status effect.
        - **Fury Generation:** Generates **15 Feral Fury** on a successful hit.
        - **Rationale:** Core ability. Establishes unarmed combat identity and bleed application.
- **`Predatory Lunge`** (Active)
    - **Description:** With a burst of animalistic speed, the Vargr-Born leaps across the battlefield to strike at vulnerable prey in the back lines.
    - **Mechanics:**
        - **Cost:** 35 Stamina.
        - **Effect:** The Vargr-Born can use this ability from the Front Row to make a single melee attack against an enemy in the **Back Row**.
        - **Fury Generation:** Generates **10 Feral Fury**.
        - **Rationale:** Mobility tool. Enables back-line pressure and target selection.

### **Tier 2 (Advanced Predation)**

*Prerequisite: 8 PP spent in the Vargr-Born tree*

- **`Terrifying Howl`** (Active)
    - **Description:** The Vargr-Born unleashes a soul-shaking howl of pure, uncorrupted primal dominance—a blast of coherent reality that shatters corrupted minds.
    - **Mechanics:**
        - **Cost:** 30 Stamina, 25 Feral Fury.
        - **Effect:** Attempts to inflict the **[Feared]** status effect on all enemies in a target row for 2 rounds.
        - **Psychic Damage:** This ability also inflicts a moderate amount of **Psychic Stress** on all enemies hit.
        - **Rationale:** AoE crowd control with Trauma Economy component. Weaponizes coherence.
- **`Go for the Throat`** (Active)
    - **Description:** The Vargr-Born identifies a momentary opening and lunges for their prey's most vital point—the killing strike of a patient hunter.
    - **Mechanics:**
        - **Cost:** 45 Stamina, 30 Feral Fury.
        - **Effect:** Deals high Physical damage to a single target.
        - **Conditional Bonus:** This attack has a **significantly increased critical hit chance** against targets that are currently **[Bleeding]** or **[Feared]**.
        - **Rationale:** Finisher ability. Rewards setup (bleed/fear) with massive burst.
- **`Taste for Blood`** (Passive)
    - **Description:** The scent and reality of blood in the air attunes the Vargr-Born more deeply to their primal nature, accelerating their fury.
    - **Mechanics:**
        - **Effect:** Whenever the Vargr-Born deals damage with a **[Bleeding]** effect (including DoT ticks), they instantly generate **5 bonus Feral Fury**.
        - **Rationale:** Core resource engine. Rewards maintaining bleeds on multiple targets.

### **Tier 3 (Mastery of the Hunt)**

*Prerequisite: 20 PP spent in the Vargr-Born tree*

- **`Feral Maelstrom`** (Active)
    - **Description:** The Vargr-Born explodes into a whirlwind of claws and fangs—a maelstrom of pure, focused violence that tears through the front line.
    - **Mechanics:**
        - **Cost:** 55 Stamina, 40 Feral Fury.
        - **Effect:** Deals moderate Physical damage to all enemies in the **Front Row**.
        - **Bleed Application:** This attack has a high chance to apply the **[Bleeding]** status effect to all targets hit.
        - **Rationale:** AoE damage with bleed spread. Enables multi-target Fury generation.
- **`Wounded Animal's Ferocity`** (Passive)
    - **Description:** A cornered wolf is at its most dangerous. When injured, the Vargr-Born's survival instincts take over, sharpening their senses and quickening their strikes.
    - **Mechanics:**
        - **Effect:** While the Vargr-Born is **[Bloodied]** (below 50% HP), all of their attacks **cost 10 less Stamina** and deal bonus damage.
        - **Rationale:** Survivability passive. Rewards aggressive play even when injured.

### **Capstone (Ultimate Expression)**

*Prerequisite: 40 PP spent in the Vargr-Born tree*

- **`Aspect of the Great Wolf`** (Active)
    - **Description:** The Vargr-Born achieves a perfect, terrifying synthesis with their primal spirit, becoming the ultimate predator—a living embodiment of the uncorrupted world's savage will.
    - **Mechanics:**
        - **Cost:** 20 Stamina, 75 Feral Fury. Once per combat.
        - **Effect:** Enters the **[Aspect of the Wolf]** state for 3 rounds. While in this state:
            1. Their `Savage Claws` ability costs no Stamina.
            2. Their `Terrifying Howl` costs no Fury and can **Stun** on a Critical Success.
            3. They are **Immune to [Fear]** and **[Disoriented]**, their mind a fortress of primal clarity.
        - **The Cost:** The strain of channeling such pure, primal code inflicts a moderate amount of **Psychic Stress** on the Vargr-Born when the effect ends.
        - **Rationale:** Ultimate transformation cooldown. Temporary god-mode balanced by high cost and Stress drawback.

### **V. Systemic Integration**

**1. Core Role & Fantasy Fulfillment**

- The Vargr-Born is the premier specialist in applying and capitalizing on the [Bleeding] status effect.
- Fantasy delivered: Feral brawler, primal predator, psychological warfare specialist who weaponizes coherence.

**2. Trauma Economy Interaction**

- **Coherent Specialization:** The Vargr-Born is a "coherent" warrior. Their power is a rejection of the Blight's chaos.
- **Offensive Use:** They weaponize their coherence to inflict Psychic Stress on corrupted foes (Terrifying Howl).
- **Defensive Stability:** They have positive interactions with the Trauma Economy—resistant to mental effects and no self-inflicted Stress (except minor cost of Aspect of the Great Wolf).
- **Thematic Inversion:** Unlike the Berserkr who channels trauma, the Vargr-Born rejects it, making them a stable, self-sufficient combatant.

**3. Situational Power Profile**

- **Optimal Conditions:**
    - Single-target focused encounters (maximize bleed value on priority target)
    - Facing corrupted/Blighted enemies (Terrifying Howl inflicts Psychic Stress)
    - Long encounters where bleeds provide sustained value
    - Solo play or small party (self-sufficient, doesn't need heavy support)
- **Weakness Conditions:**
    - Facing mechanical/Undying enemies immune to bleeding
    - Burst encounters (insufficient time for bleeds to generate Fury)
    - Enemies with heavy AoE (vulnerable due to front-line positioning)
- **Hard Counters:** Organic enemies vulnerable to bleeding; Blighted enemies vulnerable to Psychic Stress from Terrifying Howl
- **Hard Countered By:** Bleed-immune enemies (Undying, constructs, elementals); heavy armor negating unarmed strikes

**4. Synergies**

- **Positive Synergies:**
    - **Hólmgangr** (duelist): Both single-target specialists; create "predator pack" that isolates and eliminates priority targets
    - **Controllers** (Thul, Jötun-Reader): Lock down primary target so Vargr-Born can apply and maintain Bleed stacks
    - **Skald**: Saga of Courage's Psychic Stress resistance complements Terrifying Howl's offensive Stress application
    - **Bone-Setter**: Both benefit from positioning near tanks; Calculated Triage helps sustain Vargr-Born
- **Negative Synergies:**
    - AoE-focused parties may not provide good single-target opportunities
    - Heavy reliance on unarmed combat limits gear synergies with weapon-focused builds

**5. Archetype Fulfillment**

- **Archetype:** Warrior
- **Sub-Role:** Damage Dealer (Single-Target Specialist), with secondary role as Controller (fear application)
- **Warrior Identity:** Straightforward melee combat with unique unarmed focus. Clear hunting pattern: stalk, wound, finish.