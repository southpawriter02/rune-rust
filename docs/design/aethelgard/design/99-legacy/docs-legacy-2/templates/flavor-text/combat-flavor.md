# Combat Flavor Text Template

This template guides the creation of dynamic combat narrative that brings fights to life. Combat flavor encompasses attacks, defenses, critical hits, fumbles, stance changes, and enemy-specific behavior.

---

## Overview

| Category | Purpose | Triggers |
|----------|---------|----------|
| **Defensive Actions** | Block, parry, dodge descriptions | Player defense |
| **Combat Stances** | Stance change narrative | Stance switching |
| **Critical Hits** | Exceptional success flavor | Rolling 6+ successes |
| **Fumbles** | Critical failure narrative | Rolling 0 successes |
| **Combat Maneuvers** | Special attack descriptions | Ability use |
| **Enemy Voice Profiles** | Enemy combat personality | Enemy actions/reactions |

---

## Template Structure

```
COMBAT CATEGORY: [DefensiveAction | Stance | CriticalHit | Fumble | Maneuver | EnemyVoice]
Action Type: [Specific action]
Outcome Type: [Success | PartialSuccess | Failure | CriticalSuccess | etc.]
Weapon Type: [If applicable]
Attack Intensity: [Light | Heavy | Overwhelming]
Environment Context: [Optional - terrain factor]
Weight: [0.5-2.0]
Tags: ["Tag1", "Tag2"]

DESCRIPTOR TEXT:
[Combat narrative - visceral, immediate, clear mechanical implications]
```

---

## DEFENSIVE ACTIONS

### Action Types

| Type | Attribute Used | Best Against |
|------|----------------|--------------|
| **Block** | STURDINESS | Heavy attacks |
| **Parry** | FINESSE | Precise attacks |
| **Dodge** | FINESSE | AOE, multiple attacks |

### Outcome Types

| Outcome | Meaning | Mechanical Effect |
|---------|---------|-------------------|
| Success | Full defense | No damage |
| CriticalSuccess | Perfect defense | Counter opportunity |
| PartialSuccess | Reduced effect | Half damage |
| Failure | Defense failed | Full damage |
| ShieldBroken | Equipment destroyed | Full damage + item loss |
| WeaponDamaged | Equipment degraded | Durability loss |

### Block Templates

#### Successful Blocks
```
DEFENSIVE ACTION: Block
Outcome: Success
Pattern: "You [BLOCK_ACTION] your [SHIELD_TYPE]. [IMPACT_DESCRIPTION]. [RESULT_STATEMENT]."

Variables:
- BLOCK_ACTION: raise, interpose, brace with, angle
- SHIELD_TYPE: shield, buckler, tower shield, guard
- IMPACT_DESCRIPTION: physical sensation of the block
- RESULT_STATEMENT: confirmation of success

Light Attack Examples:
- "You raise your shield. The blow cracks against it harmlessly."
- "Your shield absorbs the strike with ease. You barely felt that."
- "The attack bounces off your shield with a metallic clang."

Heavy Attack Examples:
- "The impact against your shield reverberates up your arm. That was a strong one."
- "You brace yourself as the heavy blow crashes into your shield. Your arm numbs from the force."
- "The shield groans under the impact but holds. You stagger back a step."
```

#### Critical Blocks
```
DEFENSIVE ACTION: Block
Outcome: CriticalSuccess
Pattern: "[TIMING_PHRASE]! [TECHNIQUE_DESCRIPTION]. [OPPORTUNITY_CREATED]."

Examples:
- "Perfect timing! The attack glances off your shield at the ideal angle, leaving your opponent off-balance!"
- "Your shield meets their weapon with precision, deflecting it completely. They're wide open! [Counter-attack opportunity]"
- "Flawless block! Not only do you stop the attack, you create an opening!"
```

#### Failed Blocks
```
DEFENSIVE ACTION: Block
Outcome: Failure
Pattern: "[FAILURE_REASON]—[CONSEQUENCE]!"

Examples:
- "You're too slow—the attack gets around your shield!"
- "The force of the blow batters past your defenses!"
- "Your shield arm buckles under the assault!"

Shield Destruction:
- "CRACK! Your shield splinters under the impact! [Shield destroyed]"
- "The blow shatters your shield completely! You're exposed!"
```

### Parry Templates

#### Successful Parries
```
DEFENSIVE ACTION: Parry
Outcome: Success
Pattern: "You [PARRY_ACTION] with your [WEAPON]. [SOUND/SENSATION]. [POSITION_RESULT]."

One-Handed:
- "You parry the strike with your blade, metal ringing on metal."
- "Your weapon intercepts theirs with a sharp clang, turning the blow aside."
- "Steel meets steel as you parry. The clash echoes through the chamber."

Two-Handed:
- "You bring your massive weapon around to parry, using its weight to your advantage."
- "Your greatsword catches their blade, stopping it cold."
- "You leverage your weapon's reach to intercept the attack before it reaches you."

Critical Parry (Riposte):
- "Perfect parry! You deflect the attack and throw your opponent off-balance! [Free riposte]"
- "Your parry is so precise, their weapon is knocked aside completely. They're vulnerable!"
```

#### Failed Parries
```
DEFENSIVE ACTION: Parry
Outcome: Failure
Pattern: "[MISTAKE_DESCRIPTION]. [HIT_RESULT]."

Examples:
- "You parry too late—the attack connects!"
- "Your weapon misses theirs entirely. The blow lands!"
- "You misjudge the angle. Their weapon slips past your guard!"

Weapon Damage:
- "The impact damages your weapon! The blade chips badly."
- "Your weapon notches from the force. It won't survive many more hits like that."
```

### Dodge Templates

#### Successful Dodges
```
DEFENSIVE ACTION: Dodge
Outcome: Success
Pattern: "You [DODGE_ACTION]. [RESULT_DESCRIPTION]."

Basic:
- "You sidestep the attack. It misses cleanly."
- "You duck under the blow. The weapon passes harmlessly overhead."
- "You twist aside at the last moment. Close, but you avoided it."

Acrobatic:
- "You roll beneath the strike, coming up behind your opponent!"
- "You flip backward, the attack missing by inches. Impressive!"
- "You weave through the attack with fluid grace, like water flowing around stone."

Critical Dodge:
- "You read their attack perfectly and evade with time to spare! [+1 AP this turn]"
- "Not only do you dodge, but you position yourself advantageously! [Advantage on next attack]"
```

#### Failed Dodges
```
DEFENSIVE ACTION: Dodge
Outcome: Failure
Pattern: "[DODGE_ATTEMPT]. [FAILURE_RESULT]."

Basic Failures:
- "You try to dodge but aren't fast enough. The attack connects!"
- "Your reflexes fail you. You take the hit!"
- "You stumble while trying to evade. The blow catches you off-guard!"

Environmental Failures:
- "You dodge the attack but back into the wall! [Stunned 1 turn]"
- "Evading the blow, you step on unstable ground! [Disadvantage next turn]"
- "You dodge right into the flames! [Avoid attack, take environmental damage]"
```

---

## COMBAT STANCES

### Stance Types

| Stance | Bonus | Penalty | Playstyle |
|--------|-------|---------|-----------|
| **Aggressive** | +2 attack dice | -2 defense | All-out offense |
| **Defensive** | +2 defense dice | -2 attack | Survival focus |
| **Balanced** | None | None | Default state |
| **Focused** | +crit chance | -to peripheral awareness | Precision strikes |

### Stance Templates

#### Entering Stance
```
COMBAT STANCE: [Stance Type]
Moment: Entering
Pattern: "You [SHIFT_ACTION] into [STANCE_DESCRIPTION]. [BODY_LANGUAGE]. [IMPLICATION]."

Aggressive:
- "You shift into an aggressive stance, weapon raised for maximum offense."
- "You abandon caution, focusing entirely on attack."
- "Your posture becomes predatory, ready to strike at any opening."

Defensive:
- "You raise your guard, prioritizing survival over aggression."
- "Your stance tightens, minimizing openings at the cost of reach."
- "You settle into a defensive posture. Let them come."

Balanced:
- "You center yourself, returning to a neutral stance."
- "You balance offense and defense equally."

Focused:
- "You narrow your focus, watching for the perfect opening."
- "Everything else fades. Only the target matters."
```

#### Maintaining Stance
```
COMBAT STANCE: [Stance Type]
Moment: Maintaining
Pattern: "[ONGOING_DESCRIPTION]. [TRADE-OFF_AWARENESS]."

Aggressive:
- "You press the attack relentlessly!"
- "Your aggressive stance leaves you open, but you strike with devastating force!"
- "Every swing is meant to kill. Defense is secondary."

Defensive:
- "You maintain your guard, watching, waiting."
- "Your defensive posture holds. They cannot find an opening."
- "You sacrifice offense for survival—for now."
```

---

## CRITICAL HITS

### Critical Hit Categories

| Category | Trigger | Effect |
|----------|---------|--------|
| **Devastating** | 6+ successes on attack | Massive damage bonus |
| **Precise** | Crit on vital target | Additional effect |
| **Brutal** | Crit with heavy weapon | Stagger/knockdown |
| **Elegant** | Crit with finesse weapon | Bleed/precision damage |

### Critical Hit Templates

#### General Critical Hits
```
CRITICAL HIT: General
Pattern: "[STRIKE_DESCRIPTION]! [IMPACT_DETAIL]. [MECHANICAL_EFFECT]."

Examples:
- "A perfect strike! Your weapon finds the gap in their defenses with surgical precision. [Critical damage]"
- "You put everything into that blow! The impact is devastating. [Double damage dice]"
- "Time seems to slow as you execute a flawless attack. [Critical damage + stagger]"
```

#### Weapon-Specific Critical Hits
```
CRITICAL HIT: Slashing
- "Your blade carves a deep, bleeding wound! Blood sprays across the stone. [Bleed applied]"
- "The edge bites deep, nearly severing their arm! [Critical damage + limb penalty]"

CRITICAL HIT: Crushing
- "Your weapon crashes into them with bone-shattering force! [Critical damage + stagger]"
- "The impact sends them flying! They land hard, stunned. [Knockdown + critical damage]"

CRITICAL HIT: Piercing
- "Your point finds the gap and drives home! A vital hit! [Critical damage + armor bypass]"
- "The thrust is perfect—deep into unprotected flesh. [Critical damage + bleed]"

CRITICAL HIT: Unarmed
- "Your strike lands with devastating precision on a nerve cluster! [Stun + critical damage]"
- "A perfect combination! Each blow lands exactly where it needs to. [Multi-hit critical]"
```

#### Enemy-Specific Critical Hits
```
CRITICAL HIT: vs Servitor
- "You find the gap in its chassis! Sparks fly as vital components shatter!"
- "Your strike severs a critical cable! The Servitor's movements become erratic!"

CRITICAL HIT: vs Forlorn
- "You strike true! The undead thing recoils, its animation faltering!"
- "Your weapon disrupts whatever dark energy holds it together! It staggers!"

CRITICAL HIT: vs Corrupted_Dvergr
- "You break through their maddened guard! Blood and lucidity flash in their eyes—briefly."
- "A devastating hit! Even their madness can't ignore that wound!"
```

---

## FUMBLES

### Fumble Categories

| Category | Effect | Recovery |
|----------|--------|----------|
| **Overextension** | Off-balance | -1 AP next turn |
| **Weapon Problem** | Stuck/dropped | Action to recover |
| **Self-Injury** | Hurt yourself | Take damage |
| **Environmental** | Terrain mishap | Varies |

### Fumble Templates

#### Attack Fumbles
```
FUMBLE: Attack
Pattern: "[FAILED_ACTION]. [CONSEQUENCE]. [MECHANICAL_EFFECT]."

Overextension:
- "You overcommit to the swing and stumble! [Off-balance: -2 defense this round]"
- "Your attack goes wide, momentum carrying you past your target! [Enemy gets free attack]"
- "You swing at nothing! The recovery leaves you exposed!"

Weapon Problems:
- "Your weapon catches on... something. You struggle to free it! [Weapon stuck]"
- "Your grip fails! The weapon flies from your hand! [Disarmed]"
- "The blow connects with stone instead of flesh. Your weapon chips! [Durability loss]"

Self-Injury:
- "You somehow manage to cut yourself! [Take 1 damage]"
- "The recoil is worse than expected. Pain shoots through your arm! [Injured arm: -1 to attacks]"
```

#### Defense Fumbles
```
FUMBLE: Defense
Pattern: "[FAILED_DEFENSE]. [BAD_RESULT]. [MECHANICAL_EFFECT]."

Block Fumbles:
- "You raise your shield too late and too wrong. The attack crashes through! [Full damage + stagger]"
- "Your shield catches the blow at the worst possible angle! [Shield damaged + full damage]"

Parry Fumbles:
- "Your weapon is knocked from your grip by your own failed parry! [Disarmed]"
- "You parry into empty air as the real attack lands elsewhere! [Full damage + vulnerability]"

Dodge Fumbles:
- "You dodge directly into their follow-up attack! [Double damage]"
- "You stumble and fall while trying to evade! [Prone + full damage]"
- "Your foot catches and you go down hard! [Fall damage + prone]"
```

---

## COMBAT MANEUVERS

### Maneuver Types

| Type | Purpose | Key Attributes |
|------|---------|----------------|
| **Offensive** | Deal damage/effects | MIGHT, FINESSE |
| **Tactical** | Reposition/control | FINESSE, WITS |
| **Defensive** | Protect/recover | STURDINESS, WILL |
| **Resource** | Build/spend resource | Varies by class |

### Maneuver Templates

#### Offensive Maneuvers
```
COMBAT MANEUVER: Offensive
Activation: "[PREPARATION_PHRASE]."
Execution: "[ACTION_DESCRIPTION]."
Impact: "[EFFECT_DESCRIPTION]. [MECHANICAL_NOTE]."

Example - Power Attack:
Activation: "You plant your feet and draw back for a devastating swing."
Execution: "You pour every ounce of strength into the blow!"
Impact: "The impact is tremendous! [+4 damage dice, -2 to hit]"

Example - Precision Strike:
Activation: "You watch, waiting for the perfect opening..."
Execution: "There! Your weapon darts forward with surgical precision!"
Impact: "The strike lands exactly where intended! [Ignore armor, critical range +2]"
```

#### Tactical Maneuvers
```
COMBAT MANEUVER: Tactical
Setup: "[POSITIONING_PHRASE]."
Action: "[MOVEMENT/MANIPULATION_DESCRIPTION]."
Result: "[NEW_STATE]. [MECHANICAL_NOTE]."

Example - Feint:
Setup: "You telegraph an obvious attack..."
Action: "Then shift at the last moment, striking from an unexpected angle!"
Result: "They bought it! Their guard is in the wrong place! [Target loses defense bonus]"

Example - Reposition:
Setup: "You look for better ground..."
Action: "A quick step, a pivot, and you've changed the geometry of the fight!"
Result: "Now you control the angles! [+1 defense, force enemy to reorient]"
```

#### Resource-Building Maneuvers
```
COMBAT MANEUVER: Resource
Trigger: "[CONDITION_MET]."
Buildup: "[RESOURCE_GAIN_DESCRIPTION]."
State: "[NEW_RESOURCE_LEVEL]. [AVAILABLE_OPTIONS]."

Example - Savagery Build (Skar-Horde):
Trigger: "Your attack draws blood!"
Buildup: "The scent of violence awakens something primal. Your heart pounds faster!"
State: "Savagery builds! [+1 Savagery point]"

Example - Fervor Build (Iron-Bane):
Trigger: "You strike a machine-cursed enemy!"
Buildup: "Righteous fury burns through you! This is what you were made for!"
State: "Righteous Fervor ignites! [Fervor resource gained]"
```

---

## ENEMY VOICE PROFILES

### Enemy Archetypes

| Archetype | Personality | Combat Style | Communication |
|-----------|-------------|--------------|---------------|
| **Servitor** | Emotionless, corrupted | Mechanical precision | Error messages, corrupted speech |
| **Forlorn** | Mournful, confused | Relentless, unfeeling | Whispers, moans, memory fragments |
| **Corrupted_Dvergr** | Maddened, brilliant | Erratic genius | Ravings, technical babble |
| **Blight_Touched_Beast** | Feral, diseased | Savage, unpredictable | Growls, unnatural sounds |
| **Aether_Wraith** | Alien, paradoxical | Reality-bending | Impossible sounds, reversed speech |

### Enemy Voice Templates

#### Servitor Voice Profile
```
ENEMY VOICE: Servitor
Context: [Idle | Alerted | Combat | Damaged | Destroyed]

Idle:
- "*Whirr-click* MAINTENANCE CYCLE: CONTINUE. DIRECTIVE: PATROL."
- "Scanning... scanning... anomaly not detected... continue patrol protocol..."

Alerted:
- "ALERT. UNAUTHORIZED BIOLOGICAL DETECTED. INITIATING RESPONSE PROTOCOL."
- "*Error chime* Intruder classification: THREAT. Engaging countermeasures."

Combat:
- "TERMINATE. TERMINATE. SYSTEM OVERRIDE: LETHAL FORCE AUTHORIZED."
- "*Servo whine* Target acquired. Primary directive: ELIMINATE."

Damaged:
- "*Sparks* DAMAGE DETECTED—SYSTem error—CONTINUE DIRECTIVE—error error—"
- "Structural integrity: CRITICAL. Mission priority: UNCHANGED. Must... eliminate..."

Destroyed:
- "*Grinding halt* Directive... incomplete... systems... failing..."
- "ERROR. ERROR. ERRO—" *The lights die*
```

#### Forlorn Voice Profile
```
ENEMY VOICE: Forlorn
Context: [Idle | Alerted | Combat | Damaged | Destroyed]

Idle:
- "*Mournful moan* ...so cold... so long..."
- "*Whispered* Where... where did everyone go..."

Alerted:
- "*Head snaps toward you* You... you're... alive? ALIVE?!"
- "*Confused growl* Not... supposed to be here... no one comes here anymore..."

Combat:
- "*Wailing shriek* WHY WON'T YOU STAY?! Everyone leaves! EVERYONE!"
- "*Desperate grab* Be like me... be with me... forever..."

Damaged:
- "*Plaintive cry* It hurts... I remember pain... why do I remember..."
- "*Sobbing* I was... I was someone... once..."

Destroyed:
- "*Fading whisper* Thank... you..." *Collapse*
- "*Final exhale* Rest... at last..." *Stillness*
```

#### Corrupted Dvergr Voice Profile
```
ENEMY VOICE: Corrupted_Dvergr
Context: [Idle | Alerted | Combat | Damaged | Destroyed]

Idle:
- "*Manic muttering* The numbers... the NUMBERS are wrong! Fix it fix it fix it!"
- "*Laughing at nothing* They said I was mad but I SEE THE PATTERN NOW!"

Alerted:
- "*Wild eyes lock onto you* YOU! You've come to steal my work!"
- "*Paranoid shriek* SABOTEUR! I knew they'd send someone! I KNEW IT!"

Combat:
- "*Cackling* Let me show you my IMPROVEMENTS! *swings modified weapon*"
- "*Technical ravings* Strike vector compensated! Adjusting attack parameters! DIE!"

Damaged:
- "*Moment of clarity* What... what am I doing... *madness returns* NO! The work must continue!"
- "*Pain mixed with laughter* GOOD! Pain means it's WORKING! Everything is working AS DESIGNED!"

Destroyed:
- "*Final clarity* The machines... they got inside my head... I couldn't..."
- "*Dying laugh* Tell them... tell them I was RIGHT... the numbers... are..."
```

---

## Environmental Combat Flavor

### Terrain Interaction
```
ENVIRONMENT: Combat Terrain
Pattern: "[TERRAIN_USE]. [COMBAT_EFFECT]."

Cover:
- "You duck behind the rusted machinery. Partial cover gained."
- "The pillar absorbs the attack meant for you. [Full cover benefit]"

Hazards:
- "Your opponent stumbles into the steam vent! [Environmental damage to enemy]"
- "You force them toward the unstable flooring! [Terrain trap triggered]"

Verticality:
- "You gain the high ground! [+1 to ranged attacks]"
- "They're above you—a tactical disadvantage. [Enemy gains +1 attack]"
```

---

## Writing Guidelines

### Combat Flavor Principles
1. **Visceral and immediate** - readers should feel the impact
2. **Clear mechanical implications** - [brackets] for game effects
3. **Varied vocabulary** - don't repeat the same verbs
4. **Appropriate length** - longer for criticals, shorter for basic actions
5. **Character perspective** - write from the player's viewpoint

### Violence Level
- Describe impact without gratuitous gore
- Focus on combat effectiveness, not suffering
- Enemy damage can be more descriptive than player damage
- Death descriptions should be meaningful, not gratuitous

### Pacing Words
- Quick actions: "dart, snap, flash, strike"
- Heavy actions: "crash, thunder, crush, slam"
- Precise actions: "slip, slide, pierce, find"
- Failed actions: "stumble, miss, fumble, catch"

---

## Quality Checklist

- [ ] Mechanical effect clearly indicated in [brackets]
- [ ] Appropriate to weapon/action type
- [ ] Visceral but not gratuitously violent
- [ ] Varied from similar descriptors
- [ ] Correct length for importance
- [ ] Active voice throughout
- [ ] Clear who is doing what to whom
- [ ] Enemy voice fits archetype personality
