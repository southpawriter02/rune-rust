# Specialization & Abilities Flavor Text Template

This template guides the creation of flavor text for character specialization abilities, resource mechanics, and class-specific actions. Each specialization has unique themes, mechanics, and narrative voice.

---

## Overview

### Specialization Registry

| Class | Specialization | Resource | Theme |
|-------|----------------|----------|-------|
| **Warrior** | Skar-Horde Aspirant | Savagery | Berserker fury, primal violence |
| **Warrior** | Iron-Bane | Righteous Fervor | Anti-machine crusader |
| **Warrior** | Atgeir-Wielder | Momentum | Reach weapon master, tactical control |
| **Adept** | Bone-Setter | Supplies / Triage | Field medic, support specialist |
| **Adept** | Scrap-Tinker | Scrap / Gadgets | Engineer, improvised tech |
| **Adept** | Jotun-Reader | Insight | Analyst, utility, forbidden knowledge |
| **Mystic** | Vard-Warden | Runic Power | Defensive caster, barriers |
| **Mystic** | Rust-Witch | Corruption | Debuffer, decay magic |

---

## Template Structure

```
ABILITY FLAVOR: [Specialization Name]
Ability Name: [Specific ability]
Ability Type: [Active | Passive | Resource | Reaction]
Resource: [Resource type used/generated]
Trigger: [When does this activate]
Tags: ["Tag1", "Tag2"]

ACTIVATION TEXT: [Initiating the ability]
EXECUTION TEXT: [The ability in action]
IMPACT TEXT: [Effect on target/environment]
RESOURCE TEXT: [Resource gain/expenditure narrative]
FAILURE TEXT: [If ability fails/resisted]
```

---

## WARRIOR SPECIALIZATIONS

### Skar-Horde Aspirant

**Theme**: Berserker fury, channeling primal violence, Skar-Horde barbarian traditions
**Resource**: Savagery (builds in combat, spent on powerful abilities)
**Playstyle**: Aggressive, escalating damage, risk/reward

#### Resource: Savagery

```
RESOURCE: Savagery
Max: 5 points
Build: +1 when dealing/taking damage, +2 on kill
Decay: -1 per round out of combat

BUILD TRIGGERS:
- Dealing Damage: "Blood spills! Savagery stirs in your heart. [+1 Savagery]"
- Taking Damage: "Pain awakens the beast within! Savagery rises! [+1 Savagery]"
- Killing Blow: "THE KILL! Savagery surges through you like fire! [+2 Savagery]"
- Max Reached: "SAVAGERY UNLEASHED! You've embraced the beast completely! [Max Savagery]"

DECAY:
- "The fury ebbs without violence to feed it. [-1 Savagery]"
- "Your blood cools. The beast retreats. For now. [-1 Savagery]"
```

#### Abilities

##### Blood Frenzy (Passive)
```
ABILITY: Blood Frenzy
Type: Passive
Effect: +1 damage per Savagery point

ACTIVATION (threshold reached):
- "The taste of blood sharpens your strikes. [Blood Frenzy active]"
- "Violence begets violence. Each wound you deal cuts deeper. [+X damage from Savagery]"

AT MAXIMUM:
- "You are death incarnate! Every blow lands with devastating fury! [+5 damage]"
```

##### Berserker Charge (Active - 3 Savagery)
```
ABILITY: Berserker Charge
Type: Active
Cost: 3 Savagery
Effect: Rush enemy, attack with +3 dice, -2 defense until next turn

ACTIVATION:
- "With a primal howl, you CHARGE!"
- "Savagery propels you forward like a spear of flesh and fury!"

EXECUTION:
- "You crash into them with unstoppable momentum!"
- "The distance vanishes—you're on them before they can react!"

IMPACT:
- "Your weapon descends with all the force of your rage! [Attack with +3 dice]"
- "The impact is devastating! They stagger from the sheer violence!"

VULNERABILITY:
- "Your defenses are wide open—but who needs defense? [-2 defense]"
```

##### Savage Roar (Active - 2 Savagery)
```
ABILITY: Savage Roar
Type: Active
Cost: 2 Savagery
Effect: Fear check for enemies in range, allies gain +1 to next attack

ACTIVATION:
- "You throw back your head and ROAR!"
- "A cry of pure, bestial fury erupts from your throat!"

EFFECT ON ENEMIES:
- "The sound freezes them in primal terror! [Enemies: Fear check DC 12]"
- "Lesser creatures cower! Even the bold hesitate!"

EFFECT ON ALLIES:
- "Your allies feel the surge! Your fury is contagious! [Allies: +1 to next attack]"
```

---

### Iron-Bane

**Theme**: Holy warrior against corrupted machines, righteous fury, Servitor hunters
**Resource**: Righteous Fervor (builds against machine enemies)
**Playstyle**: Anti-machine specialist, sustainable damage

#### Resource: Righteous Fervor

```
RESOURCE: Righteous Fervor
Max: 5 points
Build: +1 when damaging machine-type enemies
Decay: None (persistent until spent)

BUILD TRIGGERS:
- Damaging Machine: "The machine sparks! Your crusade continues! [+1 Fervor]"
- Destroying Machine: "Another abomination falls! Righteousness burns bright! [+2 Fervor]"
- Max Reached: "DIVINE PURPOSE ACHIEVED! You are the instrument of destruction! [Max Fervor]"

FLAVOR:
- "The Iron-Bane's creed burns in your heart: 'Break the chains. Smash the gears.'"
- "Every corrupted Servitor destroyed brings the world closer to salvation."
```

#### Abilities

##### Machine Bane (Passive)
```
ABILITY: Machine Bane
Type: Passive
Effect: +2 damage vs machine-type enemies

IN EFFECT:
- "Your strikes find the weak points in their construction! [+2 vs machines]"
- "You know these abominations intimately—where they're strong, where they break."
- "The Iron-Bane's training shows: optimal force applied to critical systems."
```

##### Smite the Unclean (Active - 3 Fervor)
```
ABILITY: Smite the Unclean
Type: Active
Cost: 3 Righteous Fervor
Effect: Powerful attack vs machine, chance to disable

ACTIVATION:
- "You raise your weapon high, Fervor blazing in your eyes!"
- "In the name of those these machines have wronged—SMITE!"

EXECUTION:
- "Your weapon descends with holy purpose!"
- "The blow is precise, powerful, perfect!"

IMPACT:
- "The machine reels! Critical systems spark and fail! [Attack + disable chance]"
- "You've struck true! The abomination staggers, mechanisms seizing!"

CRITICAL:
- "DEVASTATING SMITE! The machine's core ruptures! [Critical + guaranteed disable]"
```

##### Resolve of Iron (Active - 2 Fervor)
```
ABILITY: Resolve of Iron
Type: Active
Cost: 2 Righteous Fervor
Effect: Ignore next source of damage, cannot be staggered for 1 round

ACTIVATION:
- "Your faith is your armor! [Resolve of Iron active]"
- "You steel yourself, Fervor hardening your will to iron!"

IN EFFECT:
- "The blow lands but you don't falter! [Damage ignored]"
- "Nothing can move you! Your purpose is absolute! [Stagger immunity]"
```

---

### Atgeir-Wielder

**Theme**: Reach weapon mastery, battlefield control, tactical superiority
**Resource**: Momentum (builds through successful attacks and positioning)
**Playstyle**: Control, area denial, tactical manipulation

#### Resource: Momentum

```
RESOURCE: Momentum
Max: 5 points
Build: +1 on successful attack, +1 on successful control effect
Decay: -1 if you don't attack or use control ability

BUILD TRIGGERS:
- Successful Attack: "The flow of battle favors you! [+1 Momentum]"
- Control Effect: "You dictate their movements! [+1 Momentum]"
- Combo: "Perfect! Attack and control in one motion! [+2 Momentum]"

DECAY:
- "The tempo slips. You need to keep pressing! [-1 Momentum]"
```

#### Abilities

##### Reach Advantage (Passive)
```
ABILITY: Reach Advantage
Type: Passive
Effect: Attack of opportunity when enemies enter your range

TRIGGER:
- "They enter your striking range—a mistake!"
- "Your atgeir sweeps out to punish their approach!"

EFFECT:
- "The reach of your weapon turns approach into danger! [Free attack]"
```

##### Sweeping Strike (Active - 2 Momentum)
```
ABILITY: Sweeping Strike
Type: Active
Cost: 2 Momentum
Effect: Attack all enemies in a 180° arc

ACTIVATION:
- "You plant your feet and SWEEP!"
- "The atgeir arcs through the air in a devastating circle!"

EXECUTION:
- "Your weapon traces a lethal arc, catching everything in its path!"
- "The sweep catches [X] enemies! Each one feels the edge!"

IMPACT:
- "Multiple targets struck! The crowd reels from your area control!"
```

##### Impaling Thrust (Active - 3 Momentum)
```
ABILITY: Impaling Thrust
Type: Active
Cost: 3 Momentum
Effect: Pin enemy in place, preventing movement for 2 rounds

ACTIVATION:
- "You draw back and THRUST with precision!"
- "The atgeir becomes a spear of inevitability!"

IMPACT:
- "PINNED! Your weapon catches them, holding them in place!"
- "They struggle against the shaft but cannot escape! [2 rounds immobilized]"

MAINTAINED:
- "They remain impaled, helpless before you!"
- "Twist the weapon to maintain the pin!"
```

---

## ADEPT SPECIALIZATIONS

### Bone-Setter

**Theme**: Combat medic, field surgeon, keeping allies alive
**Resource**: Supplies (consumable) / Triage (tactical resource)
**Playstyle**: Support, healing, condition removal

#### Resource: Supplies / Triage

```
RESOURCE: Supplies
Type: Consumable (restocked at safe zones)
Max: 10
Use: Powers healing abilities

RESOURCE: Triage
Type: Tactical (builds through healing)
Max: 5
Build: +1 when healing an ally

SUPPLY USE:
- "You reach into your kit... [X Supplies remaining]"
- "Medical supplies dwindling. Use them wisely."

TRIAGE BUILD:
- "Your experience in the field grows! [+1 Triage]"
- "Another life saved, another lesson learned!"
```

#### Abilities

##### Field Medicine (Active - 2 Supplies)
```
ABILITY: Field Medicine
Type: Active
Cost: 2 Supplies
Effect: Heal ally for 2d6 + WITS modifier

ACTIVATION:
- "You pull out bandages and salves, moving with practiced efficiency."
- "Hold still—this will help!"

EXECUTION:
- "Your hands work with the speed of experience!"
- "Clean, bind, stabilize. The fundamentals save lives."

EFFECT:
- "[Ally] is patched up! [Heal for X]"
- "The bleeding stops. They'll fight another day."

CRITICAL (double 6s):
- "Masterful treatment! You've exceeded expectations! [Max heal + bonus]"
```

##### Triage Protocols (Passive)
```
ABILITY: Triage Protocols
Type: Passive
Effect: +1 to all healing per Triage point

IN EFFECT:
- "Your accumulated experience enhances every treatment. [+X to healing]"
- "You know wounds now. You know what works."

AT MAXIMUM:
- "Master healer! Your treatments are legendary! [+5 to all healing]"
```

##### Emergency Stabilization (Reaction - 3 Triage)
```
ABILITY: Emergency Stabilization
Type: Reaction
Trigger: Ally reduced to 0 HP within range
Cost: 3 Triage
Effect: Ally stabilizes at 1 HP, cannot act next round

TRIGGER:
- "[Ally] goes down! You move without thinking!"
- "NO! You won't lose this one!"

EXECUTION:
- "You're at their side before they hit the ground!"
- "Emergency protocols—you've done this a hundred times!"

EFFECT:
- "Stabilized! They're down but not out! [Ally at 1 HP, incapacitated 1 round]"
- "They breathe. That's enough. That's everything."
```

---

### Scrap-Tinker

**Theme**: Engineer, improviser, gadgets and turrets
**Resource**: Scrap (gathered from environment) / Gadgets (built items)
**Playstyle**: Technical, preparation, deployables

#### Resource: Scrap / Gadgets

```
RESOURCE: Scrap
Type: Gathered (found in environment)
Max: 20
Use: Building gadgets

RESOURCE: Gadgets
Type: Built items (temporary/uses)
Types: Turret, Trap, Tool, Explosive

SCRAP GATHERING:
- "Useful parts! [+X Scrap gathered]"
- "Junk to most. Parts to you."

GADGET BUILDING:
- "You assemble the components with practiced skill..."
- "The device takes shape under your hands."
```

#### Abilities

##### Build Turret (Active - 5 Scrap)
```
ABILITY: Build Turret
Type: Active
Cost: 5 Scrap, 2 rounds to deploy
Effect: Creates automated turret (HP 15, attacks each round)

BUILDING:
- "You begin assembling the turret... [Round 1 of 2]"
- "Connections made, calibrations set... almost ready!"

DEPLOYMENT:
- "The turret whirs to life! [Turret deployed: HP 15]"
- "Your creation tracks the nearest enemy, ready to fire!"

TURRET ATTACKS:
- "The turret opens fire! [2d6 damage to nearest enemy]"
- "Whirr-BANG! Your turret provides covering fire!"

TURRET DESTROYED:
- "The turret sparks and dies! [Destroyed]"
- "They've taken out your creation! [Scrap salvageable: 2]"
```

##### Improvised Explosive (Active - 3 Scrap)
```
ABILITY: Improvised Explosive
Type: Active
Cost: 3 Scrap
Effect: Thrown explosive, 3d6 damage in area

BUILDING:
- "You quickly wire together something... volatile."
- "This won't win any engineering awards, but it'll go BOOM."

THROWING:
- "Fire in the hole!"
- "You hurl the improvised device!"

DETONATION:
- "BOOM! The explosion catches [X] enemies! [3d6 area damage]"
- "Shrapnel flies! Everything in the blast radius takes damage!"
```

---

### Jotun-Reader

**Theme**: Forbidden analyst, giant lore specialist, knowledge-seeker
**Resource**: Insight (builds through observation and analysis)
**Playstyle**: Utility, information, unique perspective (with madness risk)

#### Resource: Insight

```
RESOURCE: Insight
Max: 5 points
Build: +1 when examining enemies/environment, +1 on successful analysis
Risk: High Insight may trigger corruption/madness effects

BUILD TRIGGERS:
- Examining: "You perceive patterns others miss... [+1 Insight]"
- Analysis: "The truth reveals itself to those who look! [+1 Insight]"

HIGH INSIGHT WARNING:
- "The knowledge weighs heavily. Some truths are dangerous. [Insight 4+]"
- "You're seeing too much. The veil thins. [Corruption risk at Max]"
```

#### Abilities

##### Analytical Gaze (Active - 1 Insight)
```
ABILITY: Analytical Gaze
Type: Active
Cost: 1 Insight
Effect: Reveal enemy weaknesses and resistances

ACTIVATION:
- "You study the enemy with uncanny focus..."
- "The patterns emerge. You see what others cannot."

REVELATION:
- "Analysis complete: [Enemy] is weak to [damage type], resistant to [damage type]."
- "You perceive their vulnerabilities! [Weakness revealed]"

DEEP ANALYSIS (3+ successes):
- "You see EVERYTHING. HP, abilities, hidden traits—all laid bare."
```

##### Forbidden Tongue (Active - 3 Insight)
```
ABILITY: Forbidden Tongue
Type: Active
Cost: 3 Insight
Effect: Communicate with giant-constructs or read Jotun inscriptions
Risk: WILL save or gain 1 Corruption

ACTIVATION:
- "You speak words that should not be known by mortals..."
- "The Forbidden Tongue flows from your lips—alien, ancient, WRONG."

EFFECT:
- "The giant-construct responds! It understands! [Communication established]"
- "The inscriptions translate themselves in your mind! [Lore revealed]"

CORRUPTION RISK:
- "The knowledge burns! [WILL save vs Corruption]"
- "Too deep! You feel the madness at the edges! [+1 Corruption on failed save]"
```

---

## MYSTIC SPECIALIZATIONS

### Vard-Warden

**Theme**: Defensive caster, protective barriers, runic guardian
**Resource**: Runic Power (standard Galdr resource)
**Playstyle**: Protection, shields, damage mitigation

#### Abilities

##### Runic Ward (Active - 2 Runic Power)
```
ABILITY: Runic Ward
Type: Active
Cost: 2 Runic Power
Effect: Create barrier absorbing 10 damage for 1 ally

CASTING:
- "You trace protective runes in the air, power gathering..."
- "Ancient words of warding flow from your lips!"

MANIFESTATION:
- "A shimmering barrier of runic energy surrounds [ally]!"
- "The ward blazes to life—blue-white protection!"

ABSORPTION:
- "The ward absorbs the attack! [X damage blocked, Y remaining]"
- "Runes flare and absorb the blow! [Target protected]"

EXPIRATION:
- "The ward has taken its limit! It shatters! [Ward depleted]"
- "The runic energy dissipates, its protection spent."
```

##### Sanctuary (Active - 4 Runic Power)
```
ABILITY: Sanctuary
Type: Active
Cost: 4 Runic Power
Effect: Create zone that heals allies and damages enemies

CASTING:
- "You channel everything into the sacred geometry..."
- "SANCTUARY!"

MANIFESTATION:
- "A dome of protective energy encompasses the area!"
- "The runes of the Vard-Wardens blaze beneath your feet!"

EFFECTS:
- "Allies within feel vitality return! [+2 HP/round to allies]"
- "Enemies burn! The sanctuary rejects their presence! [2d6/round to enemies in zone]"

MAINTENANCE:
- "You strain to maintain the sanctuary... [Concentration required]"
```

---

### Rust-Witch

**Theme**: Decay magic, entropy, debuffs and deterioration
**Resource**: Corruption (both power source and risk)
**Playstyle**: Debuffer, damage over time, risky power

#### Resource: Corruption

```
RESOURCE: Corruption
Max: 10 points
Build: Generated by using decay magic
Use: Powers abilities
Risk: At 10 Corruption, suffer major negative effect

BUILD:
- "Decay feeds decay. Corruption grows. [+X Corruption]"
- "You feel the entropy taking hold... [Corruption rising]"

HIGH CORRUPTION:
- "The decay spreads to you now... [Corruption 7+: -1 to healing received]"
- "You're becoming what you wield... [Corruption 10: Major negative effect]"
```

#### Abilities

##### Rust Touch (Active - 2 Corruption generated)
```
ABILITY: Rust Touch
Type: Active
Generate: +2 Corruption
Effect: Decay enemy equipment, -2 to their defense

ACTIVATION:
- "Entropy flows through your fingers..."
- "You reach out with the touch of ages..."

EFFECT:
- "Their armor corrodes before your eyes! [-2 defense]"
- "Metal rusts, leather rots, everything falls apart!"
- "The decay spreads across their equipment!"

DURATION:
- "The rust continues to spread... [Effect persists X rounds]"
```

##### Wither (Active - 3 Corruption generated)
```
ABILITY: Wither
Type: Active
Generate: +3 Corruption
Effect: Target loses HP over time (2d4/round for 3 rounds)

CASTING:
- "You speak words of ending, of entropy, of death..."
- "WITHER!"

EFFECT:
- "Their flesh begins to decay! [2d4 damage, 3 rounds]"
- "The curse takes hold—they rot from within!"

ONGOING:
- Round 1: "The withering spreads... [2d4 damage]"
- Round 2: "Decay accelerates... [2d4 damage]"
- Round 3: "The curse reaches its peak! [2d4 damage, effect ends]"
```

---

## WRITING GUIDELINES

### Ability Flavor Principles
1. **Character voice** - each specialization has distinct personality
2. **Resource narrative** - building/spending resources should feel meaningful
3. **Visual impact** - describe what abilities look like
4. **Mechanical clarity** - [brackets] for game effects
5. **Escalation** - more powerful abilities feel more powerful

### Specialization Voice Guidelines

| Specialization | Tone | Key Words |
|----------------|------|-----------|
| Skar-Horde | Primal, violent, hungry | Blood, fury, beast, kill |
| Iron-Bane | Righteous, crusading, purposeful | Smite, purge, righteousness, abomination |
| Atgeir-Wielder | Tactical, controlled, flowing | Momentum, sweep, control, reach |
| Bone-Setter | Professional, caring, efficient | Stabilize, heal, treat, save |
| Scrap-Tinker | Technical, creative, practical | Build, wire, deploy, improvise |
| Jotun-Reader | Mysterious, dangerous, knowing | See, perceive, truth, forbidden |
| Vard-Warden | Protective, stoic, guardian | Ward, shield, protect, sanctuary |
| Rust-Witch | Entropic, decay, dangerous | Rust, wither, decay, entropy |

---

## Quality Checklist

- [ ] Ability matches specialization theme
- [ ] Resource interaction clearly described
- [ ] Mechanical effect in [brackets]
- [ ] Activation, execution, and impact all covered
- [ ] Failure state included where relevant
- [ ] Voice consistent with specialization
- [ ] Visual description present
- [ ] Escalation clear for more powerful abilities
