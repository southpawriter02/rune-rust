# Myr-Stalker (Entropic Predator)

Type: Specialization
Priority: Should-Have
Status: In Design
Archetype Foundation: Skirmisher
Balance Validated: No
Document ID: AAM-SPEC-SPEC-MYRSTALKER-v5.0
Mechanical Role: Damage Dealer, Environmental Specialist
Primary Creed Affiliation: Independents
Proof-of-Concept Flag: Yes
Resource System: Stamina
Sub-item: Tier 1 Ability: Blighted Symbiosis (Tier%201%20Ability%20Blighted%20Symbiosis%208381ad7f25034ae4bbde625d8d7a779a.md), Tier 1 Ability: Venom-Laced Shiv (Tier%201%20Ability%20Venom-Laced%20Shiv%20bbf29023cee84bbb815a39d4fd8fe8c7.md), Tier 1 Ability: Create Toxin Trap (Tier%201%20Ability%20Create%20Toxin%20Trap%200345ee54c70c4fd4a5d5f93e57e1673c.md), Tier 2 Ability: Corrosive Haze (Tier%202%20Ability%20Corrosive%20Haze%2099aba76b2cc64eee98b8f8371321c9fb.md), Tier 2 Ability: Miasmic Shroud (Tier%202%20Ability%20Miasmic%20Shroud%20955b4af9bef8486ba91f9aac73e8494f.md), Tier 2 Ability: Corruption Catalyst (Tier%202%20Ability%20Corruption%20Catalyst%20c54ed317c0414d4fb40af11cf6683741.md), Tier 3 Ability: Systemic Collapse (Tier%203%20Ability%20Systemic%20Collapse%20cb42a2929ea14ff0bc957556fe0d6af4.md), Tier 3 Ability: One with the Rot (Tier%203%20Ability%20One%20with%20the%20Rot%200e08b6fcb9d2410d9c802c46dfdd2a35.md), Capstone Ability: Pandemic Bloom (Capstone%20Ability%20Pandemic%20Bloom%20463ce3d7ff55449a832479ae0300e047.md)
Template Compliance: v5.0 Three-Tier Template
Template Validated: No
Trauma Economy Risk: High
Voice Validated: No

# Myr-Stalker (Entropic Predator) — Specialization Specification v5.0

## I. Core Philosophy: Symbiosis with Decay

The Myr-Stalker is the **Ranger specialization that has achieved heretical symbiosis with the Runic Blight's physical corruption**. Where others flee from toxic mires and blighted swamps, the Myr-Stalker thrives. They do not fight the world's decay—they weaponize it.

They are masters of **damage-over-time warfare**, patient hunters who coat their blades in distilled entropy, lay traps of weaponized blight, and watch their enemies rot from within. To choose the Myr-Stalker is to embrace the philosophy that the fastest way to defeat a corrupted system is to make it crash faster.

Your saga is written in the language of rot. You are the antibody of a dying world, proving that the strongest survivors are those who adapt to—and exploit—the very forces that killed everything else.

---

## II. Player-Facing Presentation

**Role:** Entropic Assassin, Poison Specialist, Swamp Master

**Primary Attribute:** FINESSE (for accuracy with poisoned strikes and trap placement)

**Secondary Attribute:** WITS (for alchemical knowledge and environmental awareness)

**Tertiary Attribute:** STURDINESS (for surviving toxic environments)

**Resource:** Stamina (universal Ranger resource)

**Gameplay Feel:**

The Myr-Stalker is a **methodical, attrition-focused damage dealer**. Your combat loop is:

1. **Apply:** Use poisoned strikes and traps to apply [Poisoned] and [Corroded] status effects
2. **Multiply:** Stack multiple DoT effects on priority targets
3. **Endure:** Use environmental immunity to fight in zones that harm enemies
4. **Harvest:** Convert your own Corruption into increased poison potency

You're not a burst damage dealer—you're a patient predator. You strike, apply your toxins, then fade back while your enemies decay. In blighted swamps and toxic ruins, you're nearly unkillable, turning hazards into sanctuaries.

**The Trade-Off:** Your power comes from willingly accumulating Runic Blight Corruption. Every poison resisted, every hour spent in toxic zones, every rest in a corrupted biome advances your own decay. You are powerful **because** you are rotting, not in spite of it.

---

## III. Mechanical Implementation: Blighted Alchemy

### A. Core Mechanic: Blighted Symbiosis

**Feature:** The Myr-Stalker has achieved symbiotic adaptation to physical corruption.

**Specification:**

- **Immunity:** Immune to [Poisoned] and [Disease] status effects
- **Environmental Mastery:** Can rest safely in [Metaphysical Corruption] zones
- **Hazard Healing:** Toxic environmental hazards restore 5 HP per turn instead of damaging
- **Wasteland Survival +2:** Automatic bonus (swamp/ruin specialist)

**The Price (Trauma Economy Integration):**

- When you resist a Poison/Disease effect: Gain +1 Runic Blight Corruption
- When you rest in a [Corrupted] biome: Gain +2 Corruption per rest
- Your body constantly filters the world's decay, fueling your power at the cost of your soul

### B. Core Mechanic: Weaponized Entropy

**Feature:** The Myr-Stalker specializes in applying stacking DoT effects.

**Specification:**

- All Myr-Stalker abilities that apply [Poisoned] or [Corroded] can stack
- Maximum stacks per target: 5
- Each stack increases damage per turn
- DoT effects scale with Myr-Stalker's Corruption level (see Corruption Catalyst)

**Status Effects:**

- **[Poisoned]:** 1d6 damage per turn, stacks up to 5 times (5d6 max)
- **[Corroded]:** Reduces Armor Value by 1 per stack, 1d4 damage per turn

---

## IV. The Skill Tree: The Alchemist's Arsenal

### Tier 1 (Foundational Toxicity)

**Blighted Symbiosis (Passive)**

- **Description:** Your body has undergone heretical adaptation. Poisons no longer harm you—they sustain you.
- **Mechanics:** Grants core Blighted Symbiosis mechanic (immunity + Corruption cost)

**Venom-Laced Shiv (Active - Standard Action)**

- **Cost:** 15 Stamina
- **Description:** Strike with a blade coated in distilled entropy.
- **Mechanics:**
    - FINESSE attack: Weapon damage + apply 1 stack [Poisoned]
    - Can be used repeatedly to stack [Poisoned] up to 5 times
    - This is your primary DoT applicator

**Create Toxin Trap (Active - Standard Action)**

- **Cost:** 20 Stamina
- **Description:** Place a concealed pressure-plate trap that releases debilitating toxins.
- **Mechanics:**
    - Place trap on target tile within 2 tiles
    - First enemy to step on it: Apply 2 stacks [Poisoned]
    - Physical Resolve DC 14 or also [Disoriented] for 2 turns
    - Trap remains until triggered or combat ends

---

### Tier 2 (Advanced Alchemy)

**Corrosive Haze (Active - Standard Action)**

- **Cost:** 25 Stamina
- **Description:** Hurl a glass orb that shatters, releasing weaponized entropy.
- **Mechanics:**
    - Creates 2-tile damaging zone for 3 turns
    - Enemies starting turn in haze: 2d6 damage + 1 stack [Corroded]
    - Allies immune (Myr-Stalker can filter the toxin for party members)
- **Prerequisite:** Any 2 Tier 1 abilities

**Miasmic Shroud (Active - Free Action)**

- **Cost:** 15 Stamina
- **Description:** Release disorienting vapor, becoming a ghost in your own swamp.
- **Mechanics:**
    - Gain [Concealed] for 2 turns (+4d10 to Defense)
    - Enemies who melee attack you while active: Gain 1 stack [Poisoned]
    - Can move while [Concealed] without breaking it
- **Prerequisite:** Venom-Laced Shiv + 1 other Tier 1

**Corruption Catalyst (Passive)**

- **Description:** Your inner corruption fuels your alchemy. The more Blight infects your soul, the deadlier your poisons.
- **Mechanics:**
    - [Poisoned] and [Corroded] damage increased based on your Corruption:
        - 0-25 Corruption: +0 damage
        - 26-50 Corruption: +1 damage per stack
        - 51-75 Corruption: +2 damage per stack
        - 76-100 Corruption: +3 damage per stack
    - This converts your Trauma into raw power
- **Prerequisite:** Blighted Symbiosis + 1 other Tier 1

---

### Tier 3 (Mastery of Decay)

**Systemic Collapse (Active - Standard Action)**

- **Cost:** 35 Stamina
- **Description:** Deliver a toxin that attacks the target's source code, causing catastrophic system failure.
- **Mechanics:**
    - FINESSE attack: 2d8 + FINESSE damage
    - Apply unique debuff [Systemic Collapse]: 3d6 damage per turn for 4 turns
    - Each time [Systemic Collapse] deals damage: 20% chance to trigger random Glitch effect (1 turn)
    - [Systemic Collapse] cannot be cleansed by normal means
- **Prerequisite:** Corrosive Haze + Corruption Catalyst + 1 Tier 2

**One with the Rot (Passive)**

- **Description:** Your symbiosis is complete. You are nourished by decay, finding peace in the rot.
- **Mechanics:**
    - When you resist Poison/Disease: Heal 10 HP (in addition to gaining +1 Corruption)
    - Once per rest: Can absorb toxic environmental hazard, removing it from battlefield and reducing your Psychic Stress by 10
    - Resting in [Metaphysical Corruption] zones now reduces Psychic Stress by 10 (instead of increasing it)
- **Prerequisite:** Blighted Symbiosis + Miasmic Shroud + 1 Tier 2

---

### Capstone: Pandemic Bloom (20 PP)

**Type:** Active (Standard Action)

**Cost:** 50 Stamina, once per combat

**Prerequisite:** Any 2 Tier 3 abilities

**Description:** You become the epicenter of a catastrophic alchemical reaction, unleashing the corruption stored within your body as a life-leeching bloom of weaponized entropy.

**Mechanics:**

- **Area:** All other combatants (allies and enemies)
- **Damage:** 4d10 + FINESSE damage (cannot be reduced by Armor)
- **Effect:** Apply 3 stacks [Poisoned] to all targets
- **Lifesteal:** You heal for 50% of total damage dealt
- **The Cost:** Gain +15 Runic Blight Corruption (permanent)

**Thematic:** This is the ultimate expression of your philosophy: you have become a walking biohazard, a pure conduit for the world's decay. For one devastating moment, you prove that the strongest weapon in a dying world is entropy itself.

---

## V. Systemic Integration

### Core Role & Fantasy Fulfillment

**Role:** Sustained DoT damage dealer, environmental specialist

**Fantasy Delivered:** You are the master of toxic environments. In swamps, blighted ruins, and corrupted zones where others struggle to survive, you thrive. Your enemies don't die to a single devastating blow—they rot slowly, inexorably, as your poisons unmake them from within.

### Trauma Economy Interaction

**Heretical Specialization:**

Myr-Stalker is a **high Trauma Economy cost** specialization.

**Corruption Sources:**

- Passive: +1 Corruption every time you resist Poison/Disease (frequent in toxic zones)
- Resting: +2 Corruption per rest in [Corrupted] biomes
- Capstone: +15 Corruption when using Pandemic Bloom

**Typical Campaign Arc:**

- Early game (0-25 Corruption): Poisons are effective, but you're not yet corrupted
- Mid game (26-50 Corruption): Corruption Catalyst kicks in, poisons deal +1 damage per stack
- Late game (51-75 Corruption): You're visibly corrupted but devastatingly powerful (+2 damage per stack)
- End game (76-100 Corruption): You're a walking biohazard, your poisons are lethal (+3 damage per stack), but you're one failed roll away from becoming Forlorn

**Trade-Off:** You gain unparalleled damage and survivability in the environments that kill others, but you pay for it with your soul. You **will** accumulate Corruption faster than any Coherent specialization.

### Situational Power Profile

**Optimal Conditions:**

- Blighted swamps ([Helheim] biome)
- Toxic ruins with [Metaphysical Corruption]
- Extended combat (DoTs shine over time)
- High-HP, high-Armor enemies (DoTs bypass mitigation)

**Weakness Conditions:**

- Clean, non-toxic environments (no hazard healing)
- Burst damage races (DoTs take time)
- Enemies immune to poison (rare but devastating)
- Short combats (need time to stack poisons)

**Hard Counters:**

- Undying/Mechanical enemies with Poison Immunity
- Enemies with Cleanse abilities (remove your stacks)

**Hard Countered By:**

- Undying Purifiers (cleanse DoTs)

### Synergies

**Positive Synergies:**

- **Controllers** (Hlekkr-master): Root enemies in your Corrosive Haze for guaranteed damage
- **Debuffers** (Jötun-Reader): Stack your DoTs on [Vulnerable] targets for massive damage
- **Tanks** (Skjaldmær): They hold aggro while your poisons work
- **Other Myr-Stalkers:** Stack poisons collaboratively for overwhelming DoT damage

**Negative Synergies:**

- **Burst Damage Dealers** (Berserkr): Combat ends before your DoTs can shine
- **Cleansers** (Bone-Setter): Your Corruption accumulation is a healing tax

### Archetype Fulfillment

**Archetype:** Ranger (Environmental Specialist)

**Sub-Role:** Sustained DPS, Area Denial, Environmental Master

**Distinguishing Feature:** Only specialization with Poison/Disease immunity and the ability to weaponize Blight-saturated environments.

---

*Parent Archetype:* Ranger

*Related Specializations:* Einbúi (environmental adaptation), Ruin-Stalker (hazard navigation), Veiðimaðr (hunter tactics)