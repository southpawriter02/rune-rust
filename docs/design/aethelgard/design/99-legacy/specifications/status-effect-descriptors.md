# v0.38.8: Status Effects & Condition Descriptors

Description: 50+ status application descriptors, 40+ active status, 30+ end descriptors, severity levels
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.38, v0.22
Implementation Difficulty: Medium
Balance Validated: No
Parent item: v0.38: Descriptor Library & Content Database (v0%2038%20Descriptor%20Library%20&%20Content%20Database%200a9293f3a9b44c968a36c0a429ab841d.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Parent Specification:** [v0.38: Descriptor Library & Content Database](v0%2038%20Descriptor%20Library%20&%20Content%20Database%200a9293f3a9b44c968a36c0a429ab841d.md)

**Status:** Design Phase

**Timeline:** 8-10 hours

**Goal:** Build comprehensive status effect and condition descriptor library

**Philosophy:** Every ailment tells a story of how the world harms you

---

## I. Purpose

v0.38.8 creates **Status Effect & Condition Descriptors**, making afflictions visceral:

- **50+ Status Application Descriptors** (how you get afflicted)
- **40+ Active Status Descriptors** (ongoing effect messages)
- **30+ Status End Descriptors** (recovery/expiration)
- **Severity Levels** (minor, moderate, severe)

**Strategic Function:**

Currently, status effects are mechanical:

- ❌ "You are Poisoned. Take 2 damage per turn."
- ❌ No description of how poison manifests
- ❌ No variation by source or severity

**v0.38.8 Solution:**

- Visceral application descriptions
- Ongoing suffering narratives
- Source-specific variations (beast bite vs. toxic haze)
- Severity-scaled descriptions
- Recovery moments

---

## II. Status Effect Taxonomy

### Core Status Effects (From v0.22)

**Damage-Over-Time:**

- Burning
- Bleeding
- Poisoned
- Corroding (acid)
- Freezing

**Debuffs:**

- Stunned
- Slowed
- Weakened
- Blinded
- Confused (Blight)

**Buffs:**

- Haste
- Strengthened
- Protected
- Regenerating

**Special:**

- Blight Corruption (paradox buildup)
- Cursed (Forlorn touch)

---

## III. Status Application Descriptors

### Burning

**Source: Enemy Attack (Fire)**

- "The {Enemy}'s flames catch on your clothing—you're burning!"
- "Fire clings to you, eating through leather and flesh alike!"
- "You're engulfed in flames—the heat is excruciating!"

**Source: Environmental Hazard**

- "You step too close to the lava flow—your boots ignite!"
- "A gout of flame from the broken pipe washes over you!"
- "Burning embers land on you from the collapsing ceiling!"

**Source: Galdr Backfire**

- "Your miscast Fehu turns inward—you're burning from your own magic!"
- "Paradoxical flame erupts from your hands, searing your flesh!"

### Bleeding

**Source: Weapon (Slashing)**

- "The {Enemy}'s blade opens a deep gash in your {Location}—blood flows freely!"
- "You feel the cut—warm blood soaking through your armor!"
- "The wound is deep—you're bleeding heavily!"

**Source: Weapon (Piercing)**

- "The arrow punches through—when you pull it free, blood pulses from the wound!"
- "The {Enemy}'s strike pierces clean through—you begin bleeding profusely!"

**Source: Environmental**

- "Jagged metal tears your flesh as you stumble—you're bleeding!"
- "Broken glass from shattered crystals slashes you open!"

### Poisoned

**Source: Beast Bite**

- "The beast's fangs sink deep—you feel venom burning through your veins!"
- "Poison seeps from the creature's bite, spreading cold numbness!"
- "The Blight-touched animal's saliva carries corruption—you're poisoned!"

**Source: Environmental (Toxic Haze)**

- "The toxic fumes sear your lungs—you've inhaled poison!"
- "Chemical vapors burn your throat and nose!"
- "You breathe the caustic air—immediately you feel sick!"

**Source: Weapon (Coated)**

- "The blade was poisoned! You feel it spreading from the wound!"
- "Venom from the coated weapon enters your bloodstream!"

### Stunned

**Source: Lightning**

- "Electricity courses through you—your muscles lock up!"
- "The lightning bolt leaves you convulsing, unable to act!"
- "Your nervous system rebels—you're paralyzed by the shock!"

**Source: Concussive Force**

- "The impact rattles your brain—everything spins!"
- "Your head rings from the blow—you stagger, dazed!"
- "The explosion sends you reeling, ears ringing, vision swimming!"

### Blight Corruption

**Source: Alfheim Reality Tear**

- "Reality fractures—your mind struggles to process the paradox!"
- "The Blight's corruption seeps into you—thoughts become contradictory!"
- "You feel the All-Rune's influence—is this real or memory or future?"

**Source: Forlorn Touch**

- "The Forlorn's touch is ice and emptiness—something vital drains from you!"
- "Psychic anguish washes over you, the Forlorn sharing its eternal torment!"

---

## IV. Active Status Descriptors

### Burning (Each Turn)

**Minor (1-2 damage/turn):**

- "Small flames lick at your clothing—the burns are painful but manageable."
- "You're still smoking, small fires smoldering in your gear."

**Moderate (3-5 damage/turn):**

- "Fire consumes your armor—the heat is intense!"
- "You're engulfed in flames—every moment is agony!"
- "The burning spreads—you desperately try to extinguish yourself!"

**Severe (6+ damage/turn):**

- "The flames are all-consuming—your flesh blackens and chars!"
- "You're a pillar of fire—the pain is beyond comprehension!"
- "Immolation is certain if you don't stop the burning NOW!"

### Bleeding (Each Turn)

**Minor:**

- "Blood seeps steadily from your wound."
- "The cut throbs—you're losing blood."

**Moderate:**

- "You're bleeding heavily—crimson spreads across your clothing!"
- "Blood pulses from the wound with each heartbeat!"

**Severe:**

- "You're hemorrhaging—the world swims as blood loss takes its toll!"
- "So much blood—you're growing faint!"
- "The bleeding is catastrophic—you need to stanch this NOW!"

### Poisoned

**Minor:**

- "Nausea grips you—the poison works through your system."
- "You feel weak and dizzy from the toxin."

**Moderate:**

- "The poison burns through your veins like liquid fire!"
- "You retch violently—the toxin is potent!"
- "Your vision blurs as poison clouds your mind!"

**Severe:**

- "The venom ravages your body—every breath is agony!"
- "You convulse as poison shuts down your organs!"
- "Death seems certain if you don't find an antidote!"

### Blight Corruption (Paradox Buildup)

**Low (1-2 stacks):**

- "Reality feels slightly wrong—thoughts become slippery."
- "You catch glimpses of things that aren't there... or are they?"

**Moderate (3-4 stacks):**

- "The world flickers—was that door always there?"
- "Your memories contradict themselves. Did this happen? Will it happen?"
- "Paradox builds in your mind—coherence becomes difficult!"

**High (5+ stacks, near threshold):**

- "Reality fractures around you—cause and effect lose meaning!"
- "You exist in multiple states simultaneously—which is the real you?"
- "The All-Rune's influence is overwhelming—sanity slips away!"

---

## V. Status End Descriptors

### Burning (Extinguished)

**Natural Expiration:**

- "The flames finally gutter out, leaving you singed but alive."
- "You manage to smother the fire—the burns will heal."

**Active Removal (Roll in water, etc.):**

- "You roll frantically—the flames are extinguished!"
- "You plunge into the water—blessed relief as the fire dies!"

### Bleeding (Stopped)

**Natural Clotting:**

- "The wound finally clots—the bleeding stops."
- "Your body's defenses kick in—the blood flow slows to a trickle."

**Bandaged:**

- "You bind the wound tightly—the bleeding stops."
- "The bandage holds—crisis averted."

### Poisoned (Purged)

**Natural Expiration:**

- "Your body fights off the toxin—the nausea fades."
- "The poison works its way through your system—you survive."

**Antidote:**

- "The antidote burns its way down—immediately the poison's effects diminish!"
- "You feel the cure taking effect—strength returns!"

### Blight Corruption (Stabilized)

**Below Threshold:**

- "The paradox subsides—reality reasserts itself."
- "Your mind clears—coherence returns."
- "The All-Rune's influence fades—you remain yourself."

**At Threshold (Paradox Cascade):**

- "TOO MUCH. Reality fractures. You see all possible yous. Which one survives?"
- "The Blight wins. You exist and don't exist. Were you ever real?"

---

*Continued with database schema and integration...*