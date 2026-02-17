# Bone-Setter (Restorer of Coherence) — Specialization Specification v5.0

Type: Specialization
Priority: Must-Have
Status: In Design
Archetype Foundation: Adept
Balance Validated: No
Document ID: AAM-SPEC-CLASS-BONESETTER-v5.0
Mechanical Role: Controller/Debuffer, Support/Healer
Primary Creed Affiliation: Independents, Multiple/Neutral
Proof-of-Concept Flag: Yes
Resource System: Stamina
Sub-Type: Support
Sub-item: Tier 1 Ability: Field Medic I (Tier%201%20Ability%20Field%20Medic%20I%202871d5eb988148a086876d740ea24eb5.md), Tier 1 Ability: Mend Wound (Tier%201%20Ability%20Mend%20Wound%20c202e04ca6dd4a3f981f4a324d6dd390.md), Tier 1 Ability: Apply Tourniquet (Tier%201%20Ability%20Apply%20Tourniquet%20b55f947da0e04d0d9667ec3b836d7c55.md), Tier 2 Ability: Anatomical Insight (Tier%202%20Ability%20Anatomical%20Insight%20f429a0ab39694a7fb5519532c26364d2.md), Tier 2 Ability: Administer Antidote (Tier%202%20Ability%20Administer%20Antidote%2025b71325df724611b647c5343966969f.md), Tier 2 Ability: Triage (Tier%202%20Ability%20Triage%203c51b1d7336b449d97b6e1ac4c12678b.md), Tier 3 Ability: Cognitive Realignment (Tier%203%20Ability%20Cognitive%20Realignment%209bf71b4c150640f1a4e75db56258acc1.md), Tier 3 Ability: First, Do No Harm (Tier%203%20Ability%20First,%20Do%20No%20Harm%205d362002508d4fc8bfcd734d78bf062d.md), Capstone Ability: Miracle Worker (Capstone%20Ability%20Miracle%20Worker%209cd5e4716a984336a247a248606d42a6.md)
Template Compliance: v5.0 Three-Tier Template
Template Validated: No
Trauma Economy Risk: None
Voice Layer: Layer 2 (Diagnostic)
Voice Validated: No

### **I. Core Philosophy: The Restorer of Coherence**

The **Bone-Setter** is the Adept specialization that embodies the principle of **restoring physical and mental coherence**. They are not mystics who channel the world's broken code, but pragmatic, non-magical healers who rely on a deep, scientific understanding of anatomy, herbalism, and psychological first aid. In a world that relentlessly assaults the body and shatters the mind, the Bone-Setter is the calm, steady hand that stitches the wounds, sets the bones, and anchors the fraying sanity of their comrades.

To choose the Bone-Setter is to embrace the fantasy of the indispensable combat medic and the anchor of sanity. Your saga is not written in the enemies you defeat, but in the lives you save and the minds you keep from breaking. You are the quiet, unassuming hero who ensures the party is coherent enough to continue their own sagas.

### **II. Player-Facing Presentation**

- **Role:** Non-Magical Healer, Sanity Anchor, Alchemical Support
- **Primary Attribute:** WITS (for anatomical knowledge, crafting, and analysis)
- **Secondary Attribute:** FINESSE (for delicate procedures and quick application)
- **Gameplay Feel:** The Bone-Setter is a pure, dedicated support class. Their actions are preparatory (crafting), reactive (healing damage), and preventative (managing Stress). In combat, they are a high-priority target for enemies, as their ability to reverse the party's attrition is a massive threat. Playing a Bone-Setter is a game of triage, resource management, and keeping a cool head in a crisis. Success requires preparation during downtime—the Bone-Setter who enters the field without a well-stocked medical kit will find themselves unable to fulfill their role.

### **III. Mechanical Implementation: The Art of Triage**

The Bone-Setter's power comes from their mastery of the **Field Medicine** crafting discipline and their unique, skill-based abilities.

**A. Core Mechanic: Anatomical & Psychic Triage**

- **Feature:** The Bone-Setter is the master of non-magical restoration for both the body and the mind.
- **Specification:** They are the only specialization that can craft and effectively use the most potent healing and sanity-restoring consumables in the game. Their abilities are not spells; they are `use item` actions that are often enhanced by their passive skills.
- **Rationale:** Creates a clear preparation loop—craft during downtime, deploy during crisis. The Bone-Setter's effectiveness is directly proportional to their preparation, reinforcing the Adept's planning-focused identity.

**B. Core Mechanic: Field Medicine (Crafting)**

- **Feature:** The Bone-Setter is the master of the **Field Medicine** non-artisan crafting discipline.
- **Specification:** They can use a **Campfire** or **Alchemist's Lab** to craft a wide array of medical supplies. Their high **WITS** and passives from their skill tree grant them bonus dice on these checks, allowing them to create more potent **[Masterwork]** versions of their consumables.
- **Rationale:** Establishes clear gameplay loop and resource economy. The Bone-Setter must invest time in crafting to be effective, creating interesting downtime choices.

### **IV. The Skill Tree: The Medic's Toolkit**

The Bone-Setter's skill tree is focused on enhancing their healing, their ability to cleanse debilitating effects, and their unique role in managing the Trauma Economy.

### **Tier 1 (Foundational Medicine)**

*Prerequisite: Unlock the Bone-Setter Specialization (10 PP)*

- **`Field Medic I`** (Passive)
    - **Description:** The Bone-Setter is an expert at preparing medical supplies, ensuring their kit is always ready and effective.
    - **Mechanics:**
        - **Effect:** Provides a permanent **bonus die (+1d10)** to all Field Medicine crafting checks.
        - **Starting Bonus:** The Bone-Setter also starts every expedition with 3 extra **[Standard Healing Poultices]**.
        - **Rationale:** Establishes crafting superiority and provides baseline resources.
- **`Mend Wound`** (Active)
    - **Description:** The Bone-Setter quickly and efficiently dresses a wound, applying a prepared poultice to begin the healing process.
    - **Mechanics:**
        - **Cost:** Standard Action, consumes one **[Healing Poultice]** from inventory.
        - **Effect:** A standard, single-target healing ability that restores a moderate amount of **HP**.
        - **Formula:** Base healing (determined by poultice quality) + (Bone-Setter's WITS score).
        - **Rationale:** Core healing ability. Consumes resources but is efficient and reliable.
- **`Apply Tourniquet`** (Active)
    - **Description:** Seeing a grievous, bleeding wound, the Bone-Setter acts with speed and precision to stop the life-threatening blood loss.
    - **Mechanics:**
        - **Cost:** Standard Action.
        - **Effect:** Immediately removes the **[Bleeding]** status effect from a single ally.
        - **Rationale:** Zero-cost emergency response. Situationally critical against bleed-heavy enemies.

### **Tier 2 (Advanced Treatment)**

*Prerequisite: 8 PP spent in the Bone-Setter tree*

- **`Anatomical Insight`** (Active)
    - **Description:** The Bone-Setter's knowledge is not limited to healing. They can observe a living creature and instantly recognize its anatomical weaknesses.
    - **Mechanics:**
        - **Cost:** Standard Action.
        - **Effect:** The Bone-Setter makes a **WITS** check against a single organic (non-mechanical) enemy. Success applies the **[Vulnerable]** debuff for 2 turns, causing the target to take bonus damage from all Physical attacks.
        - **Targeting Restriction:** Only affects organic creatures (not Undying or mechanical constructs).
        - **Rationale:** Gives Bone-Setter offensive utility. Rewards anatomical expertise thematically.
- **`Administer Antidote`** (Active)
    - **Description:** The Bone-Setter administers a carefully prepared antidote for the most common toxins.
    - **Mechanics:**
        - **Cost:** Standard Action, consumes one **[Common Antidote]** from inventory.
        - **Effect:** Removes one **[Poisoned]** or **[Disease]** effect from a single ally.
        - **Rationale:** Situational cleanse. Critical against poison-focused enemies.
- **`Triage`** (Passive)
    - **Description:** The Bone-Setter has a grim but necessary understanding of battlefield medicine: treat the most grievous wounds first.
    - **Mechanics:**
        - **Effect:** All of the Bone-Setter's healing abilities restore **25% more HP** when used on an ally who is **[Bloodied]** (below 50% HP).
        - **Rationale:** Rewards reactive play and efficient triage decisions.

### **Tier 3 (Mastery of Anatomy)**

*Prerequisite: 20 PP spent in the Bone-Setter tree*

- **`Cognitive Realignment`** (Active)
    - **Description:** The Bone-Setter uses a combination of calming techniques, pressure points, and alchemical smelling salts to "reboot" a panicked or disoriented mind, forcing a moment of clarity.
    - **Mechanics:**
        - **Cost:** Standard Action, consumes one **[Stabilizing Draught]**.
        - **Effect:** Immediately removes the **[Feared]** or **[Disoriented]** status effect from a single ally. Additionally, it removes a **large amount of Psychic Stress**.
        - **Rationale:** Premier sanity-restoring ability. Makes Bone-Setter essential for Trauma Economy management.
- **`"First, Do No Harm"`** (Passive)
    - **Description:** When focused on the act of healing, the Bone-Setter enters a state of heightened awareness, ensuring they don't become a casualty while saving another.
    - **Mechanics:**
        - **Effect:** For the rest of the combat round after using any single-target healing ability on an **ally**, the Bone-Setter gains a significant bonus to their **Defense Score** (+2).
        - **Duration:** Until end of current round.
        - **Rationale:** Survivability passive. Protects Bone-Setter while performing their role.

### **Capstone (Ultimate Expression)**

*Prerequisite: 40 PP spent in the Bone-Setter tree*

- **`Miracle Worker`** (Active)
    - **Description:** The Bone-Setter performs a complex and exhausting procedure—a combination of potent stimulants, field surgery, and sheer force of will—that can bring an ally back from the very brink of a fatal system crash.
    - **Mechanics:**
        - **Cost:** Standard Action, consumes one **[Miracle Tincture]** (a rare, crafted item).
        - **Requirement:** Can only be used on an ally who is **[Bloodied]** (below 50% HP).
        - **Effect:** Restores a **massive amount of HP** to the target and **removes all negative physical status effects** ([Bleeding], [Poisoned], [Crippled], [Staggered], etc.).
        - **Formula:** Massive base healing + (Bone-Setter's WITS × 3).
        - **Rationale:** Ultimate save ability. Expensive but can turn the tide of battle.

### **V. Systemic Integration**

**1. Core Role & Fantasy Fulfillment**

- The Bone-Setter is the party's primary non-magical healer and the anchor of the Trauma Economy management system.
- Fantasy delivered: Indispensable combat medic, master of triage, keeper of sanity.

**2. Trauma Economy Interaction**

- **Coherent Specialization:** The Bone-Setter has no self-inflicted Psychic Stress or Corruption mechanics.
- **Reactive Healer:** Unlike the Skald (who prevents Stress), the Bone-Setter actively removes Stress with Cognitive Realignment.
- **Complete Management:** A party with both a Skald (prevention) and Bone-Setter (treatment) has full Trauma Economy coverage.
- **Resource Dependency:** Effectiveness requires crafted consumables—Stabilizing Draughts are not infinitely available.

**3. Situational Power Profile**

- **Optimal Conditions:**
    - Long, attrition-based encounters where healing creates sustained value
    - Facing enemies with bleed/poison effects (Apply Tourniquet, Administer Antidote)
    - Expeditions with high Psychic Stress (Cognitive Realignment is critical)
    - Party with high-risk specializations (Berserkr, Echo-Caller who self-harm)
- **Weakness Conditions:**
    - Short burst encounters (insufficient time to leverage healing)
    - Lack of preparation time (enters field without crafted supplies)
    - Facing purely magical/energy-based threats (Anatomical Insight ineffective)
- **Hard Counters:** Enemies that apply [Bleeding], [Poisoned], [Feared] heavily
- **Hard Countered By:** Burst damage that kills before healing can respond; enemies that prevent item use

**4. Synergies**

- **Positive Synergies:**
    - **High-risk specializations** (Berserkr, Echo-Caller, Runecarver, Galdr-Weaver): Enable aggressive self-harm strategies through reliable healing
    - **Skald**: Complete Trauma Economy management (Skald prevents, Bone-Setter heals)
    - **Tank Warriors** (Skjaldmær, Hólmgangr): Maximize value from sustained healing
    - **Jötun-Reader**: Both are Adept supports with complementary roles (information vs restoration)
- **Negative Synergies:**
    - Other healing-focused supports create redundancy
    - Fast, mobile strikers (Vargr-Born, Strandhögg) may outpace Bone-Setter's positioning

**5. Archetype Fulfillment**

- **Archetype:** Adept
- **Sub-Role:** Pure Support (Healer/Cleanser), with secondary offensive utility (Anatomical Insight)
- **Adept Identity:** Non-magical, skill-based, heavily dependent on preparation (Field Medicine crafting during downtime is essential)