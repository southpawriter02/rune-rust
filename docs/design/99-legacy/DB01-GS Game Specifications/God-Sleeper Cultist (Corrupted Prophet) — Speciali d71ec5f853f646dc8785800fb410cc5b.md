# God-Sleeper Cultist (Corrupted Prophet) — Specialization Specification v5.0

Type: Specialization
Priority: Must-Have
Status: In Design
Archetype Foundation: Mystic
Balance Validated: Yes
Document ID: AAM-SPEC-CLASS-GODSLEEPER-v5.0
Mechanical Role: Controller/Debuffer, Environmental Specialist, Summoner/Minion Master
Primary Creed Affiliation: God-Sleepers
Proof-of-Concept Flag: Yes
Resource System: Action Points (AP), Aether Pool, Attunement, Corruption/Psychic Stress
Sub-Type: Control
Sub-item: Tier 1 Ability: Jötun-Forged Attunement (Tier%201%20Ability%20J%C3%B6tun-Forged%20Attunement%2017e2aa48da40495192a9065622b96285.md), Tier 1 Ability: Animate Scrap (Tier%201%20Ability%20Animate%20Scrap%2079447fa42fc04b0c90a3621a19c030fb.md), Tier 1 Ability: Static Lash (Tier%201%20Ability%20Static%20Lash%200d9ba709ab704551afb345db4a8efa22.md), Tier 2 Ability: Machine God's Blessing (Tier%202%20Ability%20Machine%20God's%20Blessing%200b93990049994cc5bce91d41497eff3d.md), Tier 2 Ability: Static Pulse (Tier%202%20Ability%20Static%20Pulse%20c4321d05f73e4c4c8f62d66f9cb16dba.md), Tier 2 Ability: Conduit of the Sleeper (Tier%202%20Ability%20Conduit%20of%20the%20Sleeper%205c0e113eeb7b47e0808395bf42703936.md), Tier 3 Ability: Animate Horde (Tier%203%20Ability%20Animate%20Horde%20a5dbf91179124992bae1c013baba6fea.md), Tier 3 Ability: Override Protocol (Tier%203%20Ability%20Override%20Protocol%20fe3f7c01bb2444928a5c583dac70ab9b.md), Capstone Ability: Voice of the God-Sleeper (Capstone%20Ability%20Voice%20of%20the%20God-Sleeper%20b2117f85cbeb40dca49bb205ebd7ff13.md)
Template Compliance: v5.0 Three-Tier Template
Template Validated: Yes
Trauma Economy Risk: Extreme
Voice Layer: Layer 1 (Mythic)
Voice Validated: Yes

### **I. Core Philosophy: The Corrupted User**

The **God-Sleeper Cultist** is the Mystic specialization that embodies the fanatical philosophy that the **system crash was a holy, transformative event**. They do not see the Runic Blight as a disease, but as a divine gospel. They are not casters; they are **Corrupted Users**, evangelists of the new iron age who draw their psionic power from the oppressive, psychic hum of the dormant Jötun-Forged—their "sleeping gods."

To choose the God-Sleeper Cultist is to embrace the fantasy of the heretical prophet and the master of scrap-code. Your saga is written in the fanatical litanies you preach and the disposable, glitching minions you animate in the name of your machine gods. You are a herald of the Blight's new world order, seeking not to fix the crash, but to run the corrupted program again, louder and with more conviction than before.

### **II. Player-Facing Presentation**

- **Role:** Psionic Controller, Minion Master, Proximity-Based Caster
- **Primary Attribute:** WILL (for the power of their psionics and their fanatical faith)
- **Secondary Attribute:** WITS (for controlling their constructs and understanding the "iron gospel")
- **Gameplay Feel:** The God-Sleeper Cultist is a unique "pet" and debuff class whose power is highly situational. Their gameplay is a tactical puzzle of positioning, environmental awareness, and minion management. They are weak in open, "clean" environments but become an overwhelming force when fighting in the shadow of their gods (in Jötun-Forged ruins). They are a high-risk, high-reward "summoner" who trades personal power for control of a disposable, glitching army.

### **III. Mechanical Implementation: The Art of the Iron Gospel**

The Cultist's power comes from their symbiotic relationship with the Blight's mechanical manifestations.

**A. Core Mechanic: Jötun-Forged Attunement**

- **Feature:** The Cultist's power is directly proportional to their proximity to their "gods."
- **Specification:** This is a powerful passive. While the Cultist is in a `\[Jotunheim\]` biome or any room containing a dormant Jötun-Forged, major wreckage, or multiple **Undying**, they gain the **`\[Attuned\]`** status.
- **`\[Attuned\]` Effect:** The **Aether Pool (AP) cost of all their abilities is reduced by 25%**, and their effects are significantly more potent.
- **Rationale:** This is the core of their situational power, creating a high-risk, high-reward playstyle that encourages them to fight in the most dangerous places.

**B. Core Mechanic: Scrap Animacy**

- **Feature:** The Cultist does not summon creatures from the Aether; they "infect" inert matter with the Blight's animating force.
- **Specification:** Their primary summoning ability requires a **`\[Scrap Pile\]`** (a common form of Static Terrain) to be present in the room. They use their psionic power to temporarily animate this scrap into a loyal, disposable minion.

### **IV. The Skill Tree: The Verses of a Corrupted Faith**

The Cultist's skill tree is a collection of psionic abilities that channel the psychic static of the machine gods.

**Tier 1 (Foundational Faith)**

*Prerequisite: Unlock the God-Sleeper Cultist Specialization (10 PP)*

- **`Jötun-Forged Attunement`** (Passive)
    - **Description:** The Cultist's faith opens their mind to the psychic hum of the God-Sleepers, a connection that is a source of both power and madness.
    - **Mechanics:** Grants the core `\[Attuned\]` mechanic. This is a heretical act, and the character **passively gains a small amount of Runic Blight Corruption** over time while `\[Attuned\]`.
- **`Animate Scrap`** (Active)
    - **Description:** The Cultist whispers a prayer to the machine god, and a nearby pile of inert scrap metal shudders to life.
    - **Mechanics:**
        - **Cost:** 40 AP.
        - **Requirement:** Requires a `\[Scrap Pile\]` in the room.
        - **Effect:** Summons a single **`\[Scrap Construct\]`** minion to an empty tile. The construct is a simple melee attacker with its own HP that lasts for 3 rounds.
- **`Static Lash`** (Active)
    - **Description:** The Cultist channels a shard of the Jötun-Forged's disorienting psychic hum into an invisible, painful lance of pure mental noise.
    - **Mechanics:**
        - **Cost:** 35 AP.
        - **Effect:** Deals moderate **Psychic damage** to a single target and applies a minor `\[Psychic Static\]` debuff for 2 rounds, penalizing their WILL-based Resolve Checks.
        - **Psychic Cost:** Inflicts a small amount of **Psychic Stress** on the caster.

**Tier 2 (Advanced Worship)**

*Prerequisite: 8 PP spent in the God-Sleeper Cultist tree*

- **`Machine God's Blessing`** (Active)
    - **Description:** The Cultist anoints a construct with a sacred unguent of oil and rust, chanting a litany of prime directives.
    - **Mechanics:**
        - **Cost:** 45 AP.
        - **Effect:** Places a powerful buff on a single allied construct for 3 rounds, granting it bonus damage and a significant bonus to its Soak value.
- **`Static Pulse`** (Active)
    - **Description:** The Cultist becomes a living broadcast antenna for the machine god's oppressive psychic presence.
    - **Mechanics:**
        - **Cost:** 55 AP.
        - **Psychic Cost:** +15 Psychic Stress to caster.
        - **Effect:** Deals low **Psychic damage** to all enemies in a target row and applies the `\[Psychic Static\]` debuff for 2 rounds.
- **`Conduit of the Sleeper`** (Passive)
    - **Description:** The Cultist's faith deepens, and their mind becomes a more efficient conduit. The psychic hum no longer just empowers them; it sustains them.
    - **Mechanics:** The cost reduction of `Jötun-Forged Attunement` is increased to **33%**. Additionally, while `\[Attuned\]`, the Cultist regenerates a small amount of their **Aether Pool (AP)** at the start of their turn.

**Tier 3 (Mastery of the Iron Gospel)**

*Prerequisite: 20 PP spent in the God-Sleeper Cultist tree*

- **`Animate Horde`** (Active)
    - **Description:** The Cultist speaks a powerful verse, and the battlefield's scrap heaps answer the call, forming a swarm of lesser constructs.
    - **Mechanics:**
        - **Cost:** 70 AP.
        - **Requirement:** Requires at least two `\[Scrap Piles\]` in the room.
        - **Effect:** Summons three weak **`\[Scrap Swarmling\]`** minions to empty tiles.
- **`Override Protocol`** (Active)
    - **Description:** The Cultist focuses their will on an enemy machine, speaking directly to its core logic with the voice of its true, corrupted master.
    - **Mechanics:**
        - **Cost:** 65 AP.
        - **Effect:** An opposed **WILL** check against a single **Undying** or mechanical enemy. If successful, the target is **`\[Charmed\]`** for 2 rounds, turning it against its former allies.
        - **Psychic Cost:** This intense mental battle inflicts a large amount of **Psychic Stress** on the Cultist.

**Capstone (Ultimate Expression)**

*Prerequisite: 40 PP spent in the God-Sleeper Cultist tree*

- **`Voice of the God-Sleeper`** (Active)
    - **Description:** The Cultist becomes a true prophet, surrendering their own consciousness to become a mouthpiece for the dormant Jötun-Forged.
    - **Mechanics:**
        - **Cost:** 100 AP. Once per combat.
        - **Requirement:** `Jötun-Forged Attunement` must be active.
        - **Effect:** Unleashes a wave of pure psychic command with a dual effect:
            1. **For Allies:** All allied constructs are **`\[Overcharged\]`** for 3 rounds, gaining massive bonuses to damage and the `\[Stun\]` property on their attacks.
            2. **For Enemies:** All enemies are afflicted with a powerful `\[Psychic Static\]` debuff and have a high chance to be `\[Stunned\]` for 1 round.
        - **The Cost:** The psychic backlash is immense, immediately setting the Cultist's **Psychic Stress to maximum** and inflicting a large amount of **Runic Blight Corruption**.

### **V. Systemic Integration**

- **The Heretical Summoner:** The Cultist is the game's primary "summoner," but with a unique, heretical twist. Their power is not their own, but a borrowed, dangerous force that constantly corrupts them.
- **The Trauma Economy:** This specialization is a deep dive into the Trauma Economy. Their core mechanic, `Attunement`, passively inflicts **Corruption**. Their most powerful abilities inflict massive **Psychic Stress**. They are a path of immense power at the cost of the character's soul.
- **The Environmental Specialist:** Their power is entirely dependent on the environment. In the sprawling ruins of **Jotunheim**, they are a god-like force. In the pure, natural wilds of **Vanaheim** (where there is no scrap or Undying), they are nearly powerless. This makes them a highly strategic and specialized choice.
- **Synergy:** They have a powerful synergy with other specializations that can create or manipulate the battlefield. A **Gorge-Maw Ascetic** who uses `Earthshaker` to create `\[Rubble Piles\]` is effectively creating ammunition for the Cultist's `Animate Scrap` ability.