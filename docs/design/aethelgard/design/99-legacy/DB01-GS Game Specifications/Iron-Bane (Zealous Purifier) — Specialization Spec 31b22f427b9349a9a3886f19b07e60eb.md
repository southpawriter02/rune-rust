# Iron-Bane (Zealous Purifier) — Specialization Specification v5.0

Type: Specialization
Priority: Must-Have
Status: In Design
Archetype Foundation: Warrior
Balance Validated: Yes
Document ID: AAM-SPEC-CLASS-IRONBANE-v5.0
Mechanical Role: Damage Dealer, Tank/Durability
Primary Creed Affiliation: Iron-Banes
Proof-of-Concept Flag: Yes
Resource System: Charges/Uses, Stamina
Sub-Type: Combat
Template Compliance: v5.0 Three-Tier Template
Template Validated: Yes
Trauma Economy Risk: Low
Voice Layer: Layer 1 (Mythic)
Voice Validated: Yes

### **I. Core Philosophy: The Purge of the Corrupted Code**

The **Iron-Bane** is the Warrior specialization that embodies the philosophy of the **fanatical, righteous purge**. They are not mere warriors; they are scholar-slayers, a grim order dedicated to the utter extermination of the **Undying**. Theirs is a crusade fueled by loss and honed by an encyclopedic, obsessive knowledge of their hated foe—from the mechanics of the "Iron Heart" to the alchemical compounds that can turn machine to rust.

To choose the Iron-Bane is to embrace the fantasy of the ultimate monster hunter and the holy inquisitor. Your saga is a personal crusade, a relentless and focused war against the greatest technological blasphemy of the Age of Echoes. Every battle is a righteous act of purification, and every fallen Undying is a verse in your grim hymn of vengeance.

### **II. Player-Facing Presentation**

- **Role:** Specialist Hunter, Anti-Armor Tank, Psychic Sentinel
- **Primary Attribute:** MIGHT (for the power to shatter steel and break automata)
- **Secondary Attribute:** WILL (for the iron will needed to resist the psychic hum of their foes)
- **Gameplay Feel:** The Iron-Bane is a highly specialized, methodical, and incredibly powerful "hard counter" class. Their gameplay is a focused loop of identifying their favored prey, systematically stripping their defenses with corrosive assaults, and then executing them with overwhelming force. They feel like an unstoppable, righteous juggernaut against their chosen enemy but may feel like a more standard warrior against other foes.

### **III. Mechanical Implementation: The Art of the Hunt**

The Iron-Bane's power is derived from their specialist knowledge and a unique resource fueled by their righteous crusade.

**A. Core Mechanic: Vengeance (Unique Resource)**

- **Feature:** The Iron-Bane is fueled by **`\[Vengeance\]`**.
- **Specification:**
    - **Resource Bar:** A unique resource bar from 0 to 100.
    - **Generation:** Vengeance is gained in two primary ways:
        1. Successfully hitting an **Undying** or mechanical enemy.
        2. Successfully **resisting a psychic or fear-based effect** with their WILL.
    - **Spending:** Vengeance is the required resource for their most powerful, anti-machine abilities.
    - **Decay:** Vengeance does not decay during combat but is lost upon a **Sanctuary Rest**.

**B. Core Mechanic: Specialist Knowledge**

- **Feature:** The Iron-Bane's power comes from exploiting unique, lore-driven weaknesses.
- **Specification:** They are the only specialization that can, through analysis, discover the deepest "critical flaws" in an Undying's code, which are required for their capstone ability.

### **IV. The Skill Tree: The Inquisitor's Doctrine**

The Iron-Bane's skill tree is a toolkit for hunting and exterminating technological horrors.

**Tier 1 (Foundational Vows)**

*Prerequisite: Unlock the Iron-Bane Specialization (10 PP)*

- **`Undying Insight I`** (Passive)
    - **Description:** The Iron-Bane has spent countless hours studying the anatomy of the Undying. They see not a monster, but a machine.
    - **Mechanics:** Provides a **bonus die (+1d10)** to all **`investigate` (WITS)** checks made to identify the weaknesses of Undying or mechanical enemies.
- **`Sanctified Steel`** (Active)
    - **Description:** The Iron-Bane channels their righteous fury into their weapon, a potent strike meant to test the defenses of their unholy foe.
    - **Mechanics:**
        - **Cost:** 40 Stamina.
        - **Effect:** A standard MIGHT-based melee attack. Generates **15 Vengeance** on a successful hit against an **Undying** or mechanical enemy.
- **`Indomitable Will I`** (Passive)
    - **Description:** The Iron-Bane's mind is a fortress, hardened by an unwavering vow of vengeance. The psychic hum of the Jötun-Forged is a familiar annoyance.
    - **Mechanics:** Provides a **bonus die (+1d10)** to all **WILL-based Resolve Checks** against `\[Fear\]` and `\[Psychic Static\]` effects.

**Tier 2 (Advanced Extermination)**

*Prerequisite: 8 PP spent in the Iron-Bane tree*

- **`Corrosive Strike`** (Active)
    - **Description:** The Iron-Bane's weapon strikes with the intent to not just break, but to unmake, coating the blow with an entropic agent.
    - **Mechanics:**
        - **Cost:** 45 Stamina, 25 Vengeance.
        - **Effect:** A high-damage melee attack. If the target is Undying or mechanical, it also applies the **`\[Corroded\]`** status effect.
- **`Righteous Retribution`** (Passive)
    - **Description:** The Iron-Bane's will is not merely a shield but a mirror, capable of hurling a foe's psychic assault back at them.
    - **Mechanics:** When an Undying enemy targets the Iron-Bane with a `\[Fear\]` or `\[Psychic Static\]` effect and the Iron-Bane **succeeds on their Resolve Check**, there is a 50% chance for the effect to be reflected back onto the caster.
- **`Purging Alacrity`** (Passive)
    - **Description:** The act of purification invigorates the Iron-Bane. As their foe begins to decay, the Iron-Bane is filled with a righteous speed.
    - **Mechanics:** Whenever the Iron-Bane successfully applies the `\[Corroded\]` or `\[Bleeding\]` effect to an Undying enemy, they gain a temporary bonus to their **Defense Score** for 1 round.

**Tier 3 (Mastery of the Hunt)**

*Prerequisite: 20 PP spent in the Iron-Bane tree*

- **`Chains of Decay`** (Active)
    - **Description:** The Iron-Bane slams their weapon into the ground, unleashing a shockwave of corrosive energy that erupts in ethereal, rusting chains.
    - **Mechanics:**
        - **Cost:** 50 Stamina, 40 Vengeance.
        - **Effect:** Deals moderate Physical damage to all enemies in the Front Row. If the targets are Undying or mechanical, it also applies the **`\[Corroded\]`** debuff to all of them.
- **`Heart of Iron`** (Passive)
    - **Description:** The Iron-Bane's resolve becomes absolute, making them an unshakeable bastion against the terror their enemies wield.
    - **Mechanics:** The Iron-Bane gains permanent **Immunity to the `\[Fear\]` effect**.

**Capstone (Ultimate Expression)**

*Prerequisite: 40 PP spent in the Iron-Bane tree*

- **`Annihilate Iron Heart`** (Active)
    - **Description:** The ultimate expression of the Iron-Bane's purpose. After studying their foe, they unleash a single, perfect strike designed to shatter its profane power source.
    - **Mechanics:**
        - **Cost:** 60 Stamina, 75 Vengeance. Once per combat.
        - **Requirement:** Can only be used on an **Undying** enemy that is `\[Bloodied\]` (below 50% HP) and has had its weaknesses successfully analyzed via a **Critical Success** on an `investigate` check.
        - **Effect:** A single-target finishing move. If the attack hits, it deals **massive, irresistible damage** (damage that cannot be reduced by Soak or Resistance), typically destroying the target instantly.

### **V. Systemic Integration**

- **The Ultimate Hard Counter:** The Iron-Bane is the ultimate "hard counter" specialization. They are designed from the ground up to be the solution to the **Undying** faction. In the **Jotunheim** biome, they are a god-tier, indispensable member of any party.
- **The Coherent Warrior:** The Iron-Bane is a "coherent" warrior. Their power comes from study, discipline, and righteous fury, not heresy. They have positive interactions with the **Trauma Economy**, gaining their unique resource by *resisting* its effects, making them a powerful psychic sentinel.
- **Situational Power:** Their greatest strength is also their greatest weakness. Against non-mechanical, non-Undying foes (like Blighted Beasts or rival Humanoid clans), their kit is far less effective. They lose their Vengeance generation and the bonus effects of many abilities, making them feel like a more standard, if very durable, Warrior.
- **Synergy:**
    - They synergize powerfully with the **Jötun-Reader**, whose superior analysis can help meet the stringent requirements for `Annihilate Iron Heart`.
    - Their ability to resist psychic effects makes them a natural protector for the party's more mentally fragile members.