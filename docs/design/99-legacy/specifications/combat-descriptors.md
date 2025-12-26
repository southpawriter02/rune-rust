# v0.38.12: Advanced Combat Mechanics Descriptors

Type: Mechanic
Description: Build comprehensive advanced combat mechanic descriptor library including 40+ defensive action descriptors, 30+ stance descriptors, 25+ critical hit descriptors, 25+ fumble descriptors, and 20+ combat maneuver descriptors covering blocking, parrying, dodging, stances, critical hits, and fumbles.
Priority: Should-Have
Status: In Design
Target Version: Beta
Dependencies: v0.38: Descriptor Library & Content Database
Implementation Difficulty: Hard
Balance Validated: No
Parent item: v0.38: Descriptor Library & Content Database (v0%2038%20Descriptor%20Library%20&%20Content%20Database%200a9293f3a9b44c968a36c0a429ab841d.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Parent Specification:** [v0.38: Descriptor Library & Content Database](v0%2038%20Descriptor%20Library%20&%20Content%20Database%200a9293f3a9b44c968a36c0a429ab841d.md)

**Status:** Design Phase

**Timeline:** 10-12 hours

**Goal:** Build comprehensive advanced combat mechanic descriptor library

**Philosophy:** Combat depth through detailed defensive options, critical moments, and catastrophic failures

---

## I. Purpose

v0.38.12 creates **Advanced Combat Mechanics Descriptors**, adding tactical depth:

- **40+ Defensive Action Descriptors** (block, parry, dodge, counter)
- **30+ Stance Descriptors** (aggressive, defensive, balanced)
- **25+ Critical Hit Descriptors** (devastating outcomes)
- **25+ Fumble Descriptors** (catastrophic failures)
- **20+ Combat Maneuver Descriptors** (special techniques)

**Strategic Function:**

Currently, defensive actions lack flavor:

- ❌ "You block. Damage reduced."
- ❌ No sense of tactical positioning
- ❌ Fumbles are just numbers

**v0.38.12 Solution:**

- Weapon-specific defensive descriptions
- Stance changes with tactical implications
- Critical hits that feel devastating
- Fumbles with narrative consequences
- Counter-attack opportunities

---

## II. Defensive Actions

### Blocking (Shield)

### Successful Block

**Light Attack Blocked:**

- "You raise your shield. The blow cracks against it harmlessly."
- "Your shield absorbs the strike. You barely felt that."
- "The attack bounces off your shield with a metallic *clang*."
- "You intercept the blow with your shield. Easy."

**Heavy Attack Blocked:**

- "The impact against your shield reverberates up your arm. That was a strong one."
- "You brace yourself as the heavy blow crashes into your shield. Your arm numbs from the force."
- "The shield groans under the impact but holds. You stagger back a step."
- "That hit *hard*. Your shield arm throbs, but you kept the blow at bay."

**Critical Block (Perfect Timing):**

- "You time it perfectly! The attack glances off your shield at just the right angle, leaving your opponent off-balance!"
- "Your shield meets their weapon with precision, deflecting it completely. They're wide open! [Counter-attack opportunity]"
- "Flawless block! Not only do you stop the attack, but you create an opening!"

### Failed Block

**Partial Block:**

- "Your shield catches most of it, but the blow still finds its mark. [Half damage]"
- "You're too slow—the attack gets around your shield! [Full damage]"
- "The force of the blow batters past your defenses!"

**Shield Break:**

- "The impact is too much! Your shield splinters under the assault! [Shield destroyed]"
- "CRACK! Your shield fractures. It won't take many more hits like that. [Shield durability critical]"
- "The blow shatters your shield completely! You're exposed!"

### Parrying (Weapon)

### Successful Parry

**One-Handed Weapon:**

- "You parry the strike with your blade, metal ringing on metal."
- "Your weapon intercepts theirs with a sharp *clang*, turning the blow aside."
- "You deflect their attack with a precise counter-motion."
- "Steel meets steel as you parry. The clash echoes through the chamber."

**Two-Handed Weapon:**

- "You bring your massive weapon around to parry, using its weight to your advantage."
- "Your greatsword catches their blade, stopping it cold."
- "You leverage your weapon's reach to intercept the attack before it reaches you."

**Critical Parry (Riposte Opportunity):**

- "Perfect parry! You not only deflect the attack but throw your opponent off-balance! [Free riposte attack]"
- "You turn their blade aside with masterful technique, creating an opening for a counter-strike!"
- "Your parry is so precise, their weapon is knocked aside completely. They're vulnerable!"

### Failed Parry

**Timing Off:**

- "You parry too late—the attack connects! [Full damage]"
- "Your weapon misses theirs entirely. The blow lands!"
- "You misjudge the angle. Their weapon slips past your guard!"

**Weapon Damaged:**

- "The impact damages your weapon! [Durability loss]"
- "Your blade chips from the force of the parry. [Weapon condition worsens]"
- "The clash notches your weapon badly. It won't hold up to many more hits like that."

### Dodging

### Successful Dodge

**Basic Dodge:**

- "You sidestep the attack. It misses cleanly."
- "You duck under the blow. The weapon passes harmlessly overhead."
- "You twist aside at the last moment. Close, but you avoided it."
- "Agile footwork carries you out of harm's way."

**Acrobatic Dodge:**

- "You roll beneath the strike, coming up behind your opponent!"
- "You flip backward, the attack missing by inches. Impressive!"
- "You weave through the attack with fluid grace, like water flowing around stone."

**Critical Dodge (Perfect Evasion):**

- "You read their attack perfectly and evade with time to spare! [+1 AP this turn]"
- "Not only do you dodge, but you position yourself advantageously! [Advantage on next attack]"
- "You make dodging look effortless. They're wide open now!"

### Failed Dodge

**Slow Reaction:**

- "You try to dodge but aren't fast enough. The attack connects! [Full damage]"
- "Your reflexes fail you. You take the hit!"
- "You stumble while trying to evade. The blow catches you off-guard!"

**Dodge into Hazard:**

- "You dodge the attack but back into the wall! [No damage from attack, but stunned 1 turn]"
- "Evading the blow, you step on unstable ground and nearly fall! [Disadvantage next turn]"
- "You dodge right into the flames! [Avoid attack, take environmental damage]"

---

## III. Combat Stances

### Aggressive Stance

**Entering Stance:**

- "You shift into an aggressive stance, weapon raised for maximum offense."
- "You abandon caution, focusing entirely on attack."
- "Your posture becomes predatory, ready to strike at any opening."
- "You commit to the offense, defenses be damned."

**While in Aggressive Stance:**

- "You press the attack relentlessly!"
- "Your aggressive stance leaves you open, but you strike with devastating force!"
- "Every swing is meant to kill. Defense is secondary."

**Mechanical Note:** [+2 to attack rolls, -2 to defense]

### Defensive Stance

**Entering Stance:**

- "You settle into a defensive posture, weapon ready, guard unbreakable."
- "You prioritize protection, your stance low and stable."
- "You become a wall. Let them come."
- "You shift to a defensive stance, sacrificing offense for survival."

**While in Defensive Stance:**

- "You weather their attacks, shield and armor taking the brunt."
- "Your defensive stance holds firm against the onslaught."
- "You give no ground. Every attack is met with stalwart defense."

**Mechanical Note:** [+2 to defense, -2 to attack rolls]

### Balanced Stance

**Entering Stance:**

- "You adopt a balanced stance, ready to attack or defend as needed."
- "You center yourself, finding equilibrium between offense and defense."
- "Your stance is neutral, adaptable to any situation."

**While in Balanced Stance:**

- "You maintain your balance, attacking and defending with equal measure."
- "Your versatile stance allows you to adapt to the flow of combat."

**Mechanical Note:** [No modifiers, default stance]

### Reckless Stance

**Entering Stance:**

- "You throw caution to the wind, committing to all-out assault!"
- "Survival be damned—you're going for maximum damage!"
- "You abandon all defense in favor of overwhelming offense!"

**While in Reckless Stance:**

- "You trade blows without regard for your own safety!"
- "Your attacks are devastating but leave you completely exposed!"
- "You're a whirlwind of violence, heedless of danger!"

**Mechanical Note:** [+4 to attack, -4 to defense, increased fumble chance]

---

## IV. Critical Hits

### Melee Weapon Critical Hits

**Slashing Weapons (Swords, Axes):**

- "Your blade finds the perfect angle—it carves through armor and flesh like butter! [Maximum damage + bleeding]"
- "A devastating slash! Your weapon opens a grievous wound! [Double damage]"
- "You strike with surgical precision, finding a gap in their defenses! [Critical damage + weakened status]"
- "Your blade bites *deep*. They stagger, blood pouring from the wound!"

**Crushing Weapons (Hammers, Maces):**

- "The impact is catastrophic! Bones shatter under your weapon's weight! [Maximum damage + stunned]"
- "You pulverize their defenses! The crunch of breaking armor echoes! [Armor destroyed, massive damage]"
- "A crushing blow that would fell an ox! They crumple! [Double damage + prone]"
- "You cave in their chassis/armor with devastating force!"

**Piercing Weapons (Spears, Rapiers):**

- "You drive your weapon through a vital point! [Maximum damage + critical bleeding]"
- "Perfect thrust! Your weapon finds the heart! [Instant death on weak enemies, massive damage otherwise]"
- "You punch through their defenses completely! [Ignore armor, critical damage]"
- "Your weapon slides between armor plates with deadly accuracy!"

### Ranged Weapon Critical Hits

**Bows/Crossbows:**

- "The arrow strikes true—right through the eye! [Instant kill on weak enemies]"
- "Your shot pierces a vital organ! They collapse immediately! [Maximum damage + dying status]"
- "Perfect marksmanship! The bolt finds its mark with lethal precision! [Triple damage]"
- "Your arrow penetrates completely, emerging from the other side!"

### Magic Critical Hits

**Fire Magic:**

- "Your Galdr resonates perfectly! The target ignites like a torch! [Maximum damage + severe burning]"
- "The fire doesn't just burn—it *consumes*! Nothing remains but ash!"
- "Fehu answers with terrible power! Immolation is instant!"

**Ice Magic:**

- "Your frost magic crystallizes them instantly! They shatter like glass! [Instant kill]"
- "Perfect cold! Their body freezes solid! [Maximum damage + frozen status]"
- "Þurisaz's wrath incarnate! They're encased in ice!"

**Lightning Magic:**

- "The lightning doesn't just strike—it *chains*! Multiple targets convulse! [Damage to all nearby enemies]"
- "Ansuz's fury courses through them! Their nervous system fries! [Maximum damage + paralyzed]"
- "The electrical discharge is so powerful, their heart stops!"

---

## V. Fumbles & Critical Failures

### Attack Fumbles

**Weapon Fumbles:**

- "Your swing goes wide—you overextend and stumble! [Lose 1 AP, enemy gets advantage]"
- "Your weapon slips from your grip! [Weapon drops, must use action to retrieve]"
- "You strike at empty air, throwing yourself completely off-balance! [Prone, defense reduced]"
- "Your attack is so poorly executed, you leave yourself wide open! [Enemy gets free attack]"

**Weapon Breaks:**

- "Your weapon strikes at a bad angle and SNAPS! [Weapon destroyed]"
- "The strain of combat proves too much—your weapon shatters! [Disarmed, weapon unusable]"
- "Your blade breaks against their armor! You're left holding a hilt!"

**Self-Injury:**

- "You fumble your attack and cut yourself! [1d6 self-damage]"
- "Your weapon rebounds off their armor and strikes your own leg! [1d8 damage, slowed]"
- "Catastrophic failure! Your weapon's backswing hits you! [2d6 self-damage]"

### Magic Fumbles (Miscast)

**Blight Corruption Surge:**

- "Your Galdr falters—the Blight warps the spell! Paradoxical energy erupts! [+10 Psychic Stress, random effect]"
- "The rune inverts! The spell backfires catastrophically! [Take spell damage yourself]"
- "Reality rejects your magic! The All-Rune interferes! [+15 Psychic Stress, spell fails, Corruption +5]"

**Wild Magic Surge (Alfheim):**

- "The spell goes wrong in impossible ways! [Random magical effect from table]"
- "Your Galdr succeeds... but also creates a Reality Tear! [Spell works + Glitch spawns]"
- "Magic spirals out of control! Everyone nearby is affected!"

**Spell Burnout:**

- "The magical strain is too much! Your mind reels! [Stunned 2 turns, +8 Psychic Stress]"
- "You push too hard—the Galdr sears your consciousness! [Cannot cast for 3 turns, +12 Stress]"

### Defensive Fumbles

**Shield Fumbles:**

- "You raise your shield at the wrong moment—it's knocked aside! [Take double damage]"
- "You lose your grip on your shield! [Shield drops, must retrieve]"
- "Your shield gets stuck in the environment! [Shield unusable until freed]"

**Dodge Fumbles:**

- "You dodge directly into their follow-up attack! [Take extra damage]"
- "You trip while trying to evade! [Prone, defense severely reduced]"
- "You dodge backward and fall! [Prone, lose next turn]"

**Parry Fumbles:**

- "Your parry is mistimed—their weapon disarms you! [Weapon knocked away]"
- "You parry so badly, you expose yourself completely! [Enemy gets critical hit opportunity]"
- "Your weapon is knocked from your hands! [Disarmed]"

---

## VI. Special Combat Maneuvers

### Riposte (Counter-Attack)

**Successful Riposte:**

- "You parry and immediately counter-strike! Your blade finds its mark!"
- "In one fluid motion, you deflect their attack and strike back!"
- "Perfect timing! Your riposte catches them completely off-guard!"

**Failed Riposte:**

- "You attempt a riposte but they're too quick! Your counter misses!"
- "Your counter-attack is predictable. They evade easily."

### Disarm Attempt

**Successful Disarm:**

- "You strike their weapon hand! Their weapon flies away! [Enemy disarmed]"
- "A precise blow knocks their weapon loose! It clatters to the ground!"
- "You twist their weapon from their grip! They're unarmed!"

**Failed Disarm:**

- "They hold onto their weapon despite your attempt!"
- "Your disarm fails—their grip is too strong!"

### Trip/Knockdown

**Successful Trip:**

- "You sweep their legs! They crash to the ground! [Enemy prone]"
- "Your attack takes out their footing! They fall hard!"
- "They topple! You've knocked them down!"

**Failed Trip:**

- "They maintain their balance despite your attempt!"
- "You try to trip them but they sidestep!"

### Grapple

**Successful Grapple:**

- "You seize them in a hold! They struggle but can't break free! [Enemy grappled]"
- "You wrestle them to the ground! They're pinned!"
- "You lock them in a chokehold! [Grappled, taking suffocation damage]"

**Failed Grapple:**

- "They slip out of your grasp!"
- "You can't get a hold on them!"
- "They're too quick—your grapple fails!"

---

**v0.38.12 Complete.**