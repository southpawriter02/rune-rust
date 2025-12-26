# Hólmgangr (Master Duelist) — Specialization Specification v5.0

Type: Specialization
Priority: Should-Have
Status: In Design
Archetype Foundation: Warrior
Balance Validated: No
Document ID: AAM-SPEC-CLASS-HOLMGANGR-v5.0
Mechanical Role: Burst Damage, Damage Dealer
Primary Creed Affiliation: Independents
Proof-of-Concept Flag: Yes
Resource System: Action Points (AP), Stamina
Sub-Type: Combat
Sub-item: Unencumbered Speed (Unencumbered%20Speed%20107a88acd8c94a7880f41fc161374555.md), Precise Thrust (Precise%20Thrust%2073d49b258d354403908d27058dad9597.md), Challenge of Honour (Challenge%20of%20Honour%20bac00c3ad67243399e630b5c9b2a37f9.md), Reactive Parry (Reactive%20Parry%207df4adc93316451780a23daada1266d9.md), Crippling Cut (Crippling%20Cut%20bebcd40b822a4dfabb1ccda51353142d.md), Singular Focus (Singular%20Focus%20f7a534e645d1484aabdf2c66be4a6796.md), Exploit Opening (Exploit%20Opening%200da00ec958344e60a653411ad14c0665.md), Blade Dance (Blade%20Dance%207e74bdffbdda49bd8b5d9c002f0a8afb.md), Finishing Lesson (Finishing%20Lesson%20bdff8aa694b74f90abd5f9016c939464.md)
Template Compliance: v5.0 Three-Tier Template
Template Validated: No
Trauma Economy Risk: None
Voice Layer: Layer 2 (Diagnostic)
Voice Validated: No

# Hólmgangr (Master Duelist) — Specialization Specification v5.0

## I. Core Philosophy: The Blade as Arbiter

The Hólmgangr is the Warrior specialization that embodies the ancient art of the duel—the sacred circle drawn for two. They are elite duelists who settle worth through precision, timing, and clean execution. Where others fight wars, the Hólmgangr removes a single problem at the stem.

**One foe, one finish. Grace before noise; precision before fury.**

### Core Identity

| Attribute | Value |
| --- | --- |
| Archetype | Warrior |
| Path Type | Coherent |
| Mechanical Role | Single-Target Burst Damage / Duelist |
| Primary Attribute | FINESSE |
| Resource System | Stamina + Focus |
| Trauma Economy Risk | None |

### Core Fantasy Delivered

You are the blade that cuts the head from the serpent. In a world of chaos and overwhelming numbers, you choose your battles—isolating the most dangerous threat and eliminating them with surgical precision. You dance at the edge, drawing breath from speed and clean timing. Every duel is a lesson. Every finish is proof of your worth.

---

## II. Design Pillars

### A. The Art of the Duel

The Hólmgangr operates on a tempo system—**invite, turn, answer**. Make the opponent move first, then punish them for it.

**Focus Mechanics:**

- **Challenge of Honour** marks a single enemy as your [Dueling Target]
- Attacks against your Dueling Target gain bonuses
- Each successful hit on your Dueling Target builds Focus
- Focus enables devastating finisher abilities

### B. Isolation Strategy

The Hólmgangr excels at single combat but requires setup:

- Separate the mark from the pack through positioning, feints, or ally coordination
- Light armor enables evasion but punishes being surrounded
- Clear line of sight is critical

### C. Precision Over Power

Kit habit: light harness, quick hand, clear lines. Leave shields and plate to the wall-keepers.

**Preferred Terrain:**

- Bridge spine, stair throat, alley run, ring of cords
- Any chokepoint that forces one-on-one engagement

---

## III. The Skill Tree: The Duelist's Codex

### Tier 1 (Foundational Footwork)

*Prerequisite: Unlock Hólmgangr Specialization (10 PP)*

**Unencumbered Speed (Passive)**

> *"Shun weight that dulls the edge of motion; train breath and steps until evasion is a habit."*
> 
- **Cost:** 3 PP
- **Effect:** While wearing Light armor or no armor, gain +2 Defense and +1 Movement.
- **Rank 2 (20 PP):** +3 Defense, +2 Movement
- **Rank 3 (Capstone):** First attack each combat round has +1 bonus die to hit

**Precise Thrust (Active)**

> *"A spare motion to a named point; tests guard and trims accuracy of the foe that answers poorly."*
> 
- **Cost:** 3 PP | 30 Stamina | Standard Action
- **Target:** Single enemy (melee range)
- **Effect:** FINESSE-based attack dealing 2d8 Physical damage. If target is your [Dueling Target], generate 10 Focus.
- **Rank 2 (20 PP):** 3d8 damage, generates 15 Focus vs Dueling Target
- **Rank 3 (Capstone):** On hit, target suffers -1 die to their next attack against you

**Challenge of Honour (Active)**

> *"Name the opponent and draw the line; for a time, the mind and blade attend to that one above all others."*
> 
- **Cost:** 3 PP | 20 Stamina | Bonus Action
- **Target:** Single enemy within line of sight
- **Duration:** Combat (until target dies or you choose new target)
- **Effect:** Mark target as your [Dueling Target]. You gain +1 die to all attacks against them. They gain +1 die to attacks against you (the duel cuts both ways).
- **Rank 2 (20 PP):** Your bonus increases to +2 dice; their bonus remains +1
- **Rank 3 (Capstone):** When Dueling Target dies, immediately heal 15 HP and gain 20 Focus

### Tier 2 (Advanced Blade Work)

*Prerequisite: 8 PP spent in Hólmgangr tree*

**Reactive Parry (Passive)**

> *"Turn the strike aside with timing so clean it leaves a gap to answer into."*
> 
- **Cost:** 4 PP
- **Effect:** Once per round, when your [Dueling Target] misses you with a melee attack, you may make an immediate counterattack (Free Action, no Stamina cost).
- **Rank 2 (20 PP):** Counterattack deals +1d6 bonus damage
- **Rank 3 (Capstone):** Counterattack generates 15 Focus

**Crippling Cut (Active)**

> *"A low sweep that robs stance and step; the foe moves as if in mud for a brief span."*
> 
- **Cost:** 4 PP | 40 Stamina | Standard Action
- **Target:** Single enemy (melee range)
- **Effect:** FINESSE attack dealing 3d6 Physical damage. Target gains [Slowed] for 2 rounds (half movement, -2 Defense).
- **Rank 2 (20 PP):** 4d6 damage, [Slowed] lasts 3 rounds
- **Rank 3 (Capstone):** Also applies [Hobbled] (cannot use Reactions)

**Singular Focus (Passive)**

> *"Each clean touch on the named opponent sharpens the next; the eye learns their rhythm."*
> 
- **Cost:** 4 PP
- **Effect:** Each consecutive hit on your [Dueling Target] grants a cumulative +1 damage bonus (stacks up to +5). Resets if you attack a different enemy or miss.
- **Rank 2 (20 PP):** Maximum stacks increased to +8
- **Rank 3 (Capstone):** At max stacks, your critical hit range expands by +10%

### Tier 3 (Mastery of the Duel)

*Prerequisite: 16 PP spent in Hólmgangr tree*

**Exploit Opening (Active)**

> *"When the foe is off-balance or hindered, drive the lesson home; a placed strike does more than a loud one."*
> 
- **Cost:** 5 PP | 50 Stamina + 30 Focus | Standard Action
- **Target:** Single enemy (melee range)
- **Requirement:** Target must have a negative status effect ([Slowed], [Staggered], [Feared], etc.)
- **Effect:** FINESSE attack dealing 5d10 Physical damage. Ignores 50% of target's Soak.
- **Rank 2 (20 PP):** 6d10 damage, ignores 75% Soak
- **Rank 3 (Capstone):** If target is your Dueling Target, this attack automatically crits

**Blade Dance (Active)**

> *"A flurry that flows past the guard in three clean lines—only for the named opponent."*
> 
- **Cost:** 5 PP | 55 Stamina + 40 Focus | Standard Action
- **Target:** Your [Dueling Target] only (melee range)
- **Effect:** Make three separate FINESSE attacks, each dealing 2d8 Physical damage. Each hit that lands generates 5 Focus.
- **Rank 2 (20 PP):** Each attack deals 3d8 damage
- **Rank 3 (Capstone):** If all three attacks hit, target is [Staggered] for 1 round

### Capstone (Ultimate Expression)

*Prerequisite: 24 PP in tree + any Tier 3 ability*

**Finishing Lesson (Active)**

> *"When an opponent's strength is plainly failing and their rhythm broken, end it with a single, exacting stroke."*
> 
- **Cost:** 6 PP | 40 Stamina + 60 Focus | Standard Action
- **Target:** Your [Dueling Target] only (melee range)
- **Requirement:** Target must be below 40% HP
- **Effect:** FINESSE attack dealing 8d12 Physical damage. If this attack kills the target, you may immediately designate a new [Dueling Target] and gain 40 Focus.
- **Rank 2 (20 PP):** Execute threshold raised to 50% HP, 10d12 damage
- **Rank 3 (Capstone):** On kill, all cooldowns reset; you gain [Empowered] for 2 rounds (+25% damage)

---

## IV. Systemic Integration

### Strategic Role

**The Anti-Champion:** The Hólmgangr answers the great single threat with craft rather than weight; frees the formation fighters to mind the many while you eliminate leadership.

### Situational Power Profile

| Condition | Rating |
| --- | --- |
| **1v1 Duel** | Excellent |
| **Boss with Adds** | Good (if allies control adds) |
| **Swarm Fights** | Poor |
| **Flanked/Surrounded** | Very Poor |

### Party Synergies

| Specialization | Synergy |
| --- | --- |
| **Skjaldmær** | Creates space and safety for your duels |
| **Skald** | Tempo manipulation and courage buffs |
| **Atgeir-wielder** | Keeps enemies off you while you duel |
| **Vargr-Born** | Both single-target specialists create "predator pack" |

### Vulnerabilities

- **A ring for two is a gift—crowds are not.** Partners must peel the rest away.
- **Light armor:** High evasion but punished hard when hit
- **Setup dependent:** Challenge of Honour must be active for full effectiveness

---

## V. Tactical Applications

### The Dueling Protocol

1. **Chalk the circle:** Identify highest-priority single target
2. **Challenge of Honour:** Mark them, commit to the duel
3. **Invite, turn, answer:** Let them swing first, punish mistakes
4. **Build Focus:** Precise Thrust and Reactive Parry generate resources
5. **Finish cleanly:** When they falter, deliver the Finishing Lesson

### Field Notes

- Do not boast a challenge the crew cannot afford; a dead duelist is a cost.
- Honour the ring even when the foe will not; bring a partner who can keep the circle clear.
- Withdraw when the hand trembles—reset breath, reset stance, then draw the line again.

---

## VI. Related Documents

- **Source Material:** A Guide to the Hólmgangr: The Discipline of the Duel (Layer 2)
- **Archetype:** Warrior Archetype Foundation
- **Related Specializations:** Vargr-Born (single-target predator), Atgeir-wielder (formation complement)