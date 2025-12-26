# Thul (Jötun-Reader Diagnostician) — Specialization Specification v5.0

Type: Specialization
Priority: Must-Have
Status: In Design
Archetype Foundation: Adept
Balance Validated: Yes
Document ID: AAM-SPEC-CLASS-THUL-v5.0
Mechanical Role: Controller/Debuffer, Utility/Versatility
Primary Creed Affiliation: Jötun-Readers
Proof-of-Concept Flag: Yes
Resource System: Action Points (AP), Stamina
Sub-Type: Support
Sub-item: Tier 1 Ability: Keeper of Sagas I (Tier%201%20Ability%20Keeper%20of%20Sagas%20I%20c5e76e7cb63f4db9b0459962ed6ecc6e.md), Tier 1 Ability: The Sage's Insight (Tier%201%20Ability%20The%20Sage's%20Insight%20d9a24ec6c38e4e0f9520125a7cd146e6.md), Tier 1 Ability: Inspiring Word (Tier%201%20Ability%20Inspiring%20Word%2039e9347284304c46b47bdc7643be277b.md), Tier 2 Ability: Demoralizing Diatribe (Tier%202%20Ability%20Demoralizing%20Diatribe%20bf98c251d4da45a4a07c2a0b21d1e217.md), Tier 2 Ability: Logic Trap (Tier%202%20Ability%20Logic%20Trap%20af961c673de44c0b93d0d65fbcafad76.md), Tier 2 Ability: Resolute Presence (Tier%202%20Ability%20Resolute%20Presence%206427268990524afea9b65482f1adffd5.md), Tier 3 Ability: Keeper of Oaths (Tier%203%20Ability%20Keeper%20of%20Oaths%206b2de630370146cb96794baf84a264b0.md), Tier 3 Ability: The Unspoken Truth (Tier%203%20Ability%20The%20Unspoken%20Truth%20837d57944b7a445ea36f03fb2f71e603.md), Capstone Ability: Saga of the Broken Will (Capstone%20Ability%20Saga%20of%20the%20Broken%20Will%2045c0e0b0bea846c688dffc0d947880ad.md)
Template Compliance: v5.0 Three-Tier Template
Template Validated: Yes
Trauma Economy Risk: Medium
Voice Layer: Layer 2 (Diagnostic)
Voice Validated: Yes

### **I. Core Philosophy: The Deconstruction of Narrative**

The **Thul** is the Adept specialization that embodies the philosophy that **a broken narrative is a weapon**. They are not the inspirational poets of the Skalds; they are the grim sages, the cynical orators, and the masters of psychological deconstruction. Where a Skald builds a heroic saga to inspire, a Thul dissects an enemy's saga to expose its flaws, its fears, and its logical fallacies, causing a catastrophic collapse in morale.

To choose the Thul is to embrace the fantasy of the master strategist and the psychological warrior. Your power is not in the stories you tell, but in the truths you expose. Your saga is written in the shattered confidence of your enemies and the flawless, logical execution of your allies' plans. You are not the party's heart; you are its cold, calculating mind.

### **II. Player-Facing Presentation**

- **Role:** Tactical Analyst, Psychological Warfare Specialist, Master Debuffer
- **Primary Attribute:** WITS (for analysis, logic, and spotting flaws)
- **Secondary Attribute:** WILL (for the force of personality to deliver their crushing rhetoric)
- **Gameplay Feel:** The Thul is a pure, dedicated support and debuff class. They are a "non-magical" controller who feels like a battlefield chess master. In combat, they spend their turns analyzing single targets, then systematically dismantling their will to fight with powerful, single-target debuffs. Playing a Thul is a game of identifying the highest threat and utterly neutralizing it through sheer, overwhelming logic and rhetoric.

### **III. Mechanical Implementation: The Art of Rhetoric**

The Thul's power is expressed through their mastery of the `Rhetoric` skill and a suite of active abilities that function as targeted, psychological assaults.

**A. Core Mechanic: Tactical Analysis**

- **Feature:** The Thul is a master of identifying and communicating enemy weaknesses.
- **Specification:** Their abilities are a direct and potent application of their analytical mind, turning information into a powerful, party-wide offensive tool.

**B. Core Mechanic: Psychological Debuffs**

- **Feature:** The Thul is the premier specialist in applying potent, single-target mental debuffs to **intelligent** enemies.
- **Specification:** Their abilities are `Rhetoric`-based skill checks that do not deal damage but can cripple an enemy's ability to function effectively. These abilities have **no effect on mindless entities** like Beasts or most Undying.

### **IV. The Skill Tree: The Orator's Arsenal**

The Thul's skill tree is a collection of logical weapons designed to deconstruct their opponents.

**Tier 1 (Foundational Rhetoric)**

*Prerequisite: Unlock the Thul Specialization (10 PP)*

- **`Keeper of Sagas I`** (Passive)
    - **Description:** The Thul's mind is a vast library of oral histories, laws, and, most importantly, the records of past failures and fatal flaws.
    - **Mechanics:** Provides a permanent **bonus die (+1d10)** to all **`Rhetoric`** skill checks and to `investigate` checks made to discover a creature's lore or weaknesses.
- **`The Sage's Insight`** (Active)
    - **Description:** The Thul focuses their analytical mind on a single foe, observing their stance and tactics to identify a critical flaw in their strategy.
    - **Mechanics:**
        - **Cost:** 30 Stamina.
        - **Effect:** A **WITS** check against a single target.
        - **Success:** Reveals one of the target's **Resistances** and one **Vulnerability**, and applies the **`\[Analyzed\]`** debuff for 2 rounds (allies gain +1 Accuracy die against the target).
        - **Psychic Cost:** This intense analysis inflicts a small amount of **Psychic Stress** on the Thul.
- **`Inspiring Word`** (Active)
    - **Description:** The Thul speaks a short, powerful word of logical encouragement to an ally, reminding them of their tactical purpose.
    - **Mechanics:**
        - **Cost:** 35 Stamina.
        - **Effect:** Applies a buff to a single ally for 2 rounds that grants them a **bonus die (+1d10)** to their next attack's Accuracy Pool.

**Tier 2 (Advanced Oration)**

*Prerequisite: 8 PP spent in the Thul tree*

- **`Demoralizing Diatribe`** (Active)
    - **Description:** The Thul unleashes a barrage of scathing, logical insults or recounts a past failure of their foe's clan with perfect clarity, shattering their confidence.
    - **Mechanics:**
        - **Cost:** 45 Stamina.
        - **Effect:** An opposed **WILL + Rhetoric** check against a single intelligent enemy's **WILL**. If successful, it inflicts the **`\[Disoriented\]`** status effect for 2 rounds.
- **`Logic Trap`** (Active)
    - **Description:** The Thul presents an opponent with a paradoxical question or a deeply flawed logical proposition that their mind struggles to process.
    - **Mechanics:**
        - **Cost:** 50 Stamina.
        - **Effect:** An opposed **WITS + Rhetoric** check against a single intelligent enemy's **WITS**. If successful, it inflicts the **`\[Stunned\]`** status effect for 1 round as the enemy is lost in thought.
- **`Resolute Presence`** (Passive)
    - **Description:** The Thul's calm, analytical presence is a source of stability, a coherent signal in the psychic static.
    - **Mechanics:** All allies in the same row as the Thul gain a small bonus (**+1 die**) to their **WILL-based Resolve Checks** against **`\[Fear\]`**.

**Tier 3 (Mastery of the Mind)**

*Prerequisite: 20 PP spent in the Thul tree*

- **`Keeper of Oaths`** (Active)
    - **Description:** The Thul invokes a powerful, ancient oath, reminding an ally of their sworn duties and filling them with an unshakeable sense of purpose.
    - **Mechanics:**
        - **Cost:** 55 Stamina.
        - **Effect:** Applies a powerful buff to a single ally for 3 rounds. The buffed ally gains the **`\[Inspired\]`** status effect (+2 damage dice) and becomes **Immune to `\[Fear\]`**.
- **`The Unspoken Truth`** (Passive)
    - **Description:** The Thul's insight is now so profound that they can perceive the deep-seated fears and insecurities of their opponents.
    - **Mechanics:** The **`Demoralizing Diatribe`** ability is upgraded. On a Critical Success, it now also inflicts the **`\[Feared\]`** status effect in addition to `\[Disoriented\]`.

**Capstone (Ultimate Expression)**

*Prerequisite: 40 PP spent in the Thul tree*

- **`Saga of the Broken Will`** (Active)
    - **Description:** The Thul does not recite an inspiring saga, but a devastating one: the perfectly logical, step-by-step prediction of their target's inevitable defeat. The argument is so flawless, so undeniable, that it shatters the enemy's will to exist.
    - **Mechanics:**
        - **Cost:** 70 Stamina. Once per combat.
        - **Effect:** An opposed **WITS + Rhetoric** check against a single intelligent enemy's **WILL**. The Thul gains a massive bonus to this roll.
        - **Success:** The target is afflicted with a unique **`\[Will Broken\]`** debuff for 3 rounds. While broken, the target's damage output is halved, and they are **Vulnerable to all damage types**.
        - **Psychic Cost:** This act of profound mental deconstruction inflicts a large amount of **Psychic Stress** on the Thul.

### **V. Systemic Integration**

- **The Anti-Leader:** The Thul is the ultimate "anti-leader" class. They excel at singling out the most dangerous intelligent enemy on the battlefield—the chieftain, the alpha, the rival spellcaster—and completely neutralizing them with a barrage of debilitating mental debuffs.
- **The Trauma Economy:** The Thul is a master of inflicting **Psychic Stress** on their enemies through their abilities. However, they are also vulnerable to it themselves, as their powerful analytical abilities require them to interface with the world's madness. A high **WILL** score is essential for their own defense.
- **Synergy with Analysts:** The Thul has a powerful synergy with the **Jötun-Reader**. The Jötun-Reader can identify the enemy, and the Thul can then use that information to psychologically dismantle them.
- **The Logical Counter:** They are the perfect counter to enemies that rely on morale and tactics, like **Humanoid Factions**. However, their entire toolkit is almost useless against non-sentient **Blighted Beasts** or mindless **Undying**, creating a clear and compelling weakness that must be covered by their party members.