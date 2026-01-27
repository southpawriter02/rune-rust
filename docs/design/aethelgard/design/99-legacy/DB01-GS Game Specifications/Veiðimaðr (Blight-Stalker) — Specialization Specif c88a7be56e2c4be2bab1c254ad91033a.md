# Veiðimaðr (Blight-Stalker) — Specialization Specification v5.0

Type: Specialization
Priority: Should-Have
Status: In Design
Archetype Foundation: Skirmisher
Balance Validated: No
Document ID: AAM-SPEC-SPEC-VEIDIMADR-v5.0
Mechanical Role: Damage Dealer, Sustained Damage
Primary Creed Affiliation: Independents
Proof-of-Concept Flag: Yes
Resource System: Stamina
Sub-Type: Combat
Sub-item: Tier 1 Ability: Aimed Shot (Tier%201%20Ability%20Aimed%20Shot%205e76b46dfade4813a6a974312a045db6.md), Tier 1 Ability: Set Snare (Tier%201%20Ability%20Set%20Snare%205dec2c3c719d4e5e8bb9192e9c50a55c.md), Tier 1 Ability: Wilderness Acclimation I (Tier%201%20Ability%20Wilderness%20Acclimation%20I%20d3bb6d7be58a4798bdd911f580a6a145.md), Tier 2 Ability: Blight-Tipped Arrow (Tier%202%20Ability%20Blight-Tipped%20Arrow%209265eb2ec80448a885a9d3505dbd3e86.md), Tier 2 Ability: Mark for Death (Tier%202%20Ability%20Mark%20for%20Death%20d513814f735e4cee9ba5fcab7c5219bd.md), Tier 3 Ability: Exploit Corruption (Tier%203%20Ability%20Exploit%20Corruption%2020d247ed73684713a2d543be16c6d6ae.md), Capstone Ability: Stalker of the Unseen (Capstone%20Ability%20Stalker%20of%20the%20Unseen%2088f103696a084763b9d9f7b7ecacca4e.md), Tier 3 Ability: Heartseeker Shot (Tier%203%20Ability%20Heartseeker%20Shot%20657006684a6e4a7d9d44c0bb7523f34d.md), Tier 2 Ability: Predator's Focus (Tier%202%20Ability%20Predator's%20Focus%20ec89b2628ac144569e9c88e33794a4b4.md)
Template Compliance: v5.0 Three-Tier Template
Template Validated: No
Trauma Economy Risk: Low
Voice Validated: No

# Veiðimaðr (Blight-Stalker) — Specialization Specification v5.0

## I. Core Philosophy: The Patient Predator

The **Veiðimaðr** is the Skirmisher specialization that has learned to read the invisible signs of the Runic Blight. Where others see corrupted beasts and glitching horrors, you see spoor to track, weaknesses to exploit, and corruption levels to measure. You are the patient predator who culls the sick before the infection spreads.

To choose the Veiðimaðr is to embrace the philosophy that **knowledge precedes violence**. Your saga is written in clean kills, marked targets, and corruption purged from the world one precisely-aimed arrow at a time. You don't fight the Blight—you hunt it.

---

## II. Player-Facing Presentation

| Attribute | Value |
| --- | --- |
| **Role** | Ranged DPS, Corruption Tracker, Blight-Stalker |
| **Primary Attribute** | FINESSE (for precision archery and trap placement) |
| **Secondary Attribute** | WITS (for tracking, perception, and corruption reading) |
| **Resource** | Stamina + Focus |
| **Trauma Risk** | Medium (Mark for Death inflicts Psychic Stress) |

### Gameplay Feel

The Veiðimaðr is a **methodical, single-target damage dealer** who excels against corrupted enemies. Your combat loop is:

1. **Scout**: Use Wilderness Acclimation to assess corruption levels before engagement
2. **Mark**: Designate priority targets with Mark for Death for bonus damage
3. **Exploit**: Gain massive crit bonuses against corrupted targets via Exploit Corruption
4. **Execute**: Deliver Heartseeker Shot to purge corruption and deal devastating burst

You operate from the **back row**, maintaining distance while your precision arrows eliminate high-value targets. Your damage scales with enemy corruption—the more Blighted the foe, the deadlier you become.

### The Trade-Off

Your power comes from **focusing on the Blight**. Each time you Mark for Death, you expose yourself to minor Psychic Stress as you attune to a target's corruption signature. You are powerful because you understand decay intimately, not in spite of it.

---

## III. Mechanical Implementation

### A. Core Mechanic: Corruption Tracking

| Corruption Level | Range | Exploit Corruption Crit Bonus |
| --- | --- | --- |
| None | 0 | +0% |
| Low | 1-29 | +5% → +10% (Rank 2) |
| Medium | 30-59 | +10% → +20% (Rank 2) |
| High | 60-89 | +15% → +30% (Rank 2) |
| Extreme | 90+ | +20% → +40% (Rank 2) |

**Wilderness Acclimation** allows you to perceive these levels before combat, enabling tactical target selection.

### B. Core Mechanic: Mark for Death

Mark for Death designates a priority target for 3-4 turns:

- Your attacks deal +8/+12/+15 bonus damage vs marked target
- At Rank 3, allies also gain +5 damage vs marked target
- **Cost**: 5/3/2 Psychic Stress (focusing on Blight is taxing)
- **Stalker of the Unseen** passive reveals vulnerabilities when you mark

---

## IV. The Skill Tree: The Hunter's Path

### Tier 1 — Foundational Marksmanship (3 PP each)

**Wilderness Acclimation I** *(Passive)*

> *"Your senses are honed to a razor's edge. You distinguish unnatural spoor from healthy, reading the subtle signs of a corrupted landscape."*
> 
- +1d10 bonus to WITS checks for tracking, foraging, and perception
- Can identify Blighted creatures by spoor
- **Rank 2**: +2d10; estimate corruption level (Low/Medium/High/Extreme)
- **Rank 3**: +3d10; automatically detect [Blighted] items without touching them

**Aimed Shot** *(Active — Standard Action)*

> *"You steady your breath, your focus absolute, and release a single, perfectly aimed shot."*
> 
- Cost: 40 Stamina (35 at Rank 2)
- FINESSE-based ranged attack dealing weapon damage
- **Rank 2**: +1d6 damage
- **Rank 3**: +2d6 damage; critical hits apply [Bleeding] for 2 turns

**Set Snare** *(Active — Standard Action)*

> *"You quickly assemble a simple but effective snare, concealing it beneath corrupted earth and rubble."*
> 
- Cost: 35 Stamina + 1 Trap Component
- Place trap on target tile; first enemy to step on it becomes [Rooted] for 1 turn
- **Rank 2**: [Rooted] 2 turns; can place up to 2 active traps
- **Rank 3**: [Rooted] 3 turns; +2d6 Physical damage on trigger
- Cooldown: 2 turns

---

### Tier 2 — Advanced Tracking (4 PP each, requires 8 PP in tree)

**Mark for Death** *(Active — Bonus Action)*

> *"You focus your intent on a single target, observing the subtle tells of its Blighted nature. You mark it as the primary corruption to be cleansed."*
> 
- Cost: 30 Stamina
- Apply [Marked] for 3 turns; your attacks deal +8 bonus damage vs marked target
- **You gain 5 Psychic Stress** (focusing on Blight)
- **Rank 2**: +12 damage; 4 turn duration; 3 Stress
- **Rank 3**: +15 your damage; +5 ally damage; 2 Stress
- Cooldown: 3 turns

**Blight-Tipped Arrow** *(Active — Standard Action)*

> *"You draw an arrow tipped with toxin harvested from Blighted flora. The shot introduces a sliver of the world's own sickness into your foe."*
> 
- Cost: 45 Stamina + 1 Alchemical Component
- Deal 3d6 Physical damage + apply [Blighted Toxin] (2d6/turn for 3 turns)
- If target has 30+ Corruption: 40% chance to inflict [Glitch] (skip next action)
- **Rank 2**: 4d6 damage; 4 turn toxin; 60% Glitch chance
- **Rank 3**: 3d6/turn toxin; 80% Glitch chance
- Cooldown: 3 turns

**Predator's Focus** *(Passive)*

> *"Your mind is a fortress of calm focus. The familiar rhythm of the hunt filters out the maddening psychic noise."*
> 
- While in back row and not adjacent to enemies: +1d10 to Resolve checks vs Psychic Stress
- **Rank 2**: +2d10 Resolve; +1d10 Perception in back row
- **Rank 3**: +3d10 Resolve; regenerate 5 Stamina/turn (out of combat only)

---

### Tier 3 — Mastery of the Hunt (5 PP each, requires 16 PP in tree)

**Exploit Corruption** *(Passive)*

> *"You have studied the Blight so long you can predict its chaotic influence. A corrupted creature is unstable—more susceptible to system-shocking blows."*
> 
- Gain increased critical hit chance vs corrupted targets (see Corruption Tracking table)
- **Rank 2**: Double all crit bonuses; critical hits vs High/Extreme apply [Staggered] (1 turn)
- **Rank 3**: Crits vs corrupted deal +50% damage; refund 20 Stamina on kill

**Heartseeker Shot** *(Active — Full Turn Charge)*

> *"You take a full turn to aim not for a vital organ, but for the metaphysical core of the target's corruption. Your stable arrow corrects unstable code."*
> 
- Cost: 60 Stamina + 30 Focus
- **Turn 1**: Declare and charge (cannot move or act; vulnerable to interruption)
- **Turn 2**: Release shot dealing 6d10 Physical damage
- If target is [Marked]: Purge 10 Corruption, dealing +2 bonus damage per Corruption purged (max +20)
- **Rank 2**: 8d10 damage; purge 15 Corruption (max +30 bonus)
- **Rank 3**: 10d10 damage; purge 20 Corruption (max +40 bonus); refund 30 Stamina + 15 Focus on kill
- Cooldown: 4 turns

---

### Capstone — Stalker of the Unseen (6 PP, requires 24 PP + any Tier 3)

> *"You have become the perfect predator. Your senses transcend the physical, perceiving the invisible threads of Blight itself. To be marked by you is to have your corruption laid bare."*
> 

**Passive Component**:

- When you use Mark for Death, automatically learn target's Vulnerabilities and precise Corruption level
- **Rank 2**: Reveal 2 vulnerabilities
- **Rank 3**: Reveal ALL weaknesses

**Active Component** *(Bonus Action toggle)*:

- Enter "Blight-Stalker's Stance" (20 Stamina/turn upkeep)
- While active: Immune to visual impairment; 50% chance to inflict [Staggered] on Aimed Shots vs High/Extreme Corruption
- When stance ends: Gain 10 Psychic Stress
- **Rank 2**: 70% Stagger; 15 Stamina/turn
- **Rank 3**: 90% Stagger; +2d10 to attacks vs corrupted; only 5 Stress when stance ends

---

## V. Systemic Integration

### Trauma Economy

- **Coherent Specialization**: No heretical mechanics; Psychic Stress from Mark for Death is the only self-inflicted cost
- **Predator's Focus** provides stress resistance while in optimal back-row position
- **Moderate Risk**: Mark spam accumulates Stress; Stalker Stance has exit penalty

### Situational Power Profile

**Optimal Conditions**:

- Heavily Blighted areas with High/Extreme Corruption enemies
- Single-target priority elimination (bosses, casters, elites)
- Party compositions that benefit from marked target focus fire

**Weakness Conditions**:

- Swarm encounters with many low-Corruption enemies
- Close-quarters combat that forces front-row positioning
- Corruption-immune enemies (constructs, Coherent foes)

### Synergies

**Positive**:

- **Skjaldmær**: Held front grants safe back-row lanes
- **Controllers** (Hlekkr-master, Thul): Fear, stillness, confusion widen clean-hit windows
- **Jötun-Reader**: Analyze Weakness + Mark for Death = devastating focus fire

**Negative**:

- **AoE-focused parties**: May not provide single-target opportunities
- **Aggressive melee comps**: May push into close quarters where Veiðimaðr is vulnerable

---

## VI. v5.0 Setting Compliance

✅ **Technology, Not Magic**: Arrows, toxins, and traps—no supernatural elements

✅ **Layer 2 Voice**: "Corruption tracking," "system instability," "code correction"

✅ **Norse-Inspired**: Veiðimaðr (hunter), traditional archery and trapping

✅ **Blight Integration**: Corruption exploitation is the core mechanical identity