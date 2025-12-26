# Hazards & Environmental Features Template

This template guides the creation of flavor text for environmental hazards, terrain features, cover systems, and dynamic environmental elements that affect gameplay.

---

## Overview

| Category | Purpose | Player Interaction |
|----------|---------|-------------------|
| **Hazards** | Damage/status threats | Avoidance, mitigation |
| **Static Terrain** | Navigation obstacles | Pathfinding, cover |
| **Dynamic Features** | Changing elements | Timing, exploitation |
| **Cover** | Combat protection | Tactical positioning |

---

## Template Structure

```
ENVIRONMENTAL: [Hazard | StaticTerrain | DynamicFeature | Cover]
Feature Type: [Specific type]
Biome: [The_Roots | Muspelheim | Niflheim | Alfheim | Jotunheim | Universal]
Activation: [Periodic | Proximity | Triggered | Persistent | Movement]
Damage Type: [If applicable]
Severity: [Minor | Moderate | Severe | Lethal]
Tags: ["Tag1", "Tag2"]

APPEARANCE TEXT: [What the player sees]
ACTIVATION TEXT: [When hazard triggers]
EFFECT TEXT: [During ongoing effect]
RESOLUTION TEXT: [When effect ends/avoided]
```

---

## HAZARD TYPES

### Hazard Registry

| Hazard | Biomes | Damage Type | Typical Activation |
|--------|--------|-------------|-------------------|
| ToxicFumes | The_Roots | Poison + Stress | Proximity/Periodic |
| ToxicSludge | The_Roots | Acid | Movement |
| UnstableFlooring | The_Roots, Jotunheim | Fall | Movement |
| ElectricalHazard | The_Roots | Electrical | Proximity/Triggered |
| Radiation | The_Roots | Radiation (cumulative) | Proximity |
| Fire | Muspelheim, The_Roots | Fire | Persistent/Movement |
| Ice | Niflheim | Cold + Difficult Terrain | Persistent |
| Darkness | All | Stress + Perception penalty | Area |
| Vacuum | The_Roots (sealed areas) | Suffocation | Area/Persistent |

### Hazard Templates

#### ToxicFumes
```
HAZARD: ToxicFumes
Biome: The_Roots
Activation: Proximity / Periodic
Damage: 1d6 poison + 1d4 Stress per round
Save: STURDINESS vs DC 12

APPEARANCE:
- "A sickly yellow-green mist hangs in the air here, clinging to the floor."
- "Corroded pipes leak something foul. The vapors shimmer and swirl."
- "Warning signs—faded, unreadable—line the walls. The smell tells you why."

ACTIVATION:
- "The fumes catch in your throat. Your eyes water, lungs burning."
- "You breathe it in before you realize the danger. It tastes of chemicals and death."
- "The toxic cloud envelops you. Every breath is agony."

EFFECT (ongoing):
- "The poison burns through your system. Your vision blurs."
- "You can't stop coughing. Each breath draws more toxins into your lungs."
- "Your body fights the contamination, but the fumes are relentless."

RESOLUTION:
- "You stumble free of the fumes, gasping clean air. Your lungs ache."
- "The ventilation here clears the toxins. You can breathe again."
- "Your antidote kicks in, neutralizing the worst of the poison."
```

#### ToxicSludge
```
HAZARD: ToxicSludge
Biome: The_Roots
Activation: Movement
Damage: 2d6 acid on contact, difficult terrain
Save: FINESSE vs DC 14 to cross safely

APPEARANCE:
- "A pool of iridescent sludge blocks the path. Things dissolve slowly in its depths."
- "Industrial waste has pooled here—chemicals that even rust fears."
- "The sludge bubbles occasionally. Bones of small creatures protrude from its surface."

ACTIVATION:
- "The sludge splashes up onto your legs. It burns immediately!"
- "You miscalculate the depth. The chemicals eat through your boots!"
- "A misstep plunges your foot into the muck. Pain, immediate and intense!"

EFFECT (ongoing):
- "The acid continues to burn even after you're clear. Your gear smokes and hisses."
- "Your skin blisters where the sludge touched. This will scar."

RESOLUTION:
- "You scrape off the last of the sludge. The burning slowly fades."
- "Water neutralizes the acid. You'll live, but your equipment has seen better days."
```

#### UnstableFlooring
```
HAZARD: UnstableFlooring
Biome: The_Roots, Jotunheim
Activation: Movement
Damage: Fall damage (varies by height)
Save: FINESSE vs DC 12 to avoid, MIGHT vs DC 14 to catch yourself

APPEARANCE:
- "The floor grating here is buckled and rusted. It groans under your weight."
- "Sections of flooring have already given way. Darkness yawns below."
- "Cracks spider-web across the floor. This section could go at any moment."

ACTIVATION:
- "The floor gives way beneath you with a shriek of tortured metal!"
- "Your foot punches through rusted decking! You plunge into darkness!"
- "CRACK! The support finally fails! Everything tilts!"

EFFECT (falling):
- "You fall—wind rushing past—the floor above shrinking—"
- "Time stretches as you drop. You have a moment to prepare for impact."

RESOLUTION (caught yourself):
- "You catch the edge! Your arms scream in protest but you hold!"
- "You grab a pipe as you fall—swinging, but alive!"

RESOLUTION (fell):
- "You hit the ground hard. [Fall damage applied]"
- "The landing is brutal. Everything hurts. But you're alive."
```

#### ElectricalHazard
```
HAZARD: ElectricalHazard
Biome: The_Roots
Activation: Proximity / Triggered
Damage: 2d8 electrical + stun
Save: FINESSE vs DC 13 to avoid, STURDINESS vs DC 12 to resist stun

APPEARANCE:
- "Sparks arc between exposed cables. The air smells of ozone."
- "A junction box crackles with barely-contained power. Blue-white light flickers within."
- "Water has pooled beneath damaged wiring. The surface ripples with electrical discharge."

ACTIVATION:
- "Lightning leaps from the cables! Your muscles seize as current flows through you!"
- "The electrical discharge catches you! Every nerve screams!"
- "ZAP! The shock is instantaneous and excruciating!"

EFFECT (stunned):
- "Your muscles won't respond. The electricity has scrambled your nervous system."
- "You can't move. Everything twitches uncontrollably."

RESOLUTION:
- "The current stops. You collapse, smoking slightly, but alive."
- "Your nerves slowly remember how to function. Movement returns."
- "You jump clear just as the discharge erupts. Too close."
```

#### Fire Hazard
```
HAZARD: Fire
Biome: Muspelheim (common), The_Roots (localized)
Activation: Persistent / Movement
Damage: 1d8 per round in flames, 1d4 on adjacent squares
Save: FINESSE vs DC 11 to navigate safely

APPEARANCE (Muspelheim):
- "Fire crawls across every surface. The heat is suffocating."
- "Walls of flame block the corridor. There's no way around—only through."
- "The ground itself burns. Cracks in the floor reveal molten rock beneath."

APPEARANCE (The_Roots):
- "Something is burning ahead. Chemical fire, by the color of the flames."
- "A ruptured fuel line has ignited. Orange flames lick at the ceiling."

ACTIVATION:
- "The flames engulf you! Your world becomes heat and pain!"
- "Fire catches your clothing! You're burning!"
- "The heat is beyond what flesh can endure!"

EFFECT (burning):
- "You burn! Stop, drop, and roll or keep taking damage!"
- "Your gear smolders. Your skin blisters. The fire doesn't care about your screams."

RESOLUTION:
- "You smother the flames. Burned, but the fire is out."
- "You roll through the cold water beyond. The fire hisses and dies."
```

#### Ice Hazard
```
HAZARD: Ice
Biome: Niflheim
Activation: Persistent (difficult terrain) / Movement (slip)
Effect: Difficult terrain, slip chance on fast movement
Save: FINESSE vs DC 10 to maintain footing

APPEARANCE:
- "The floor is a sheet of black ice—perfectly smooth, treacherously slick."
- "Ice covers everything. Each step is a gamble."
- "Frost makes every surface a trap. Move carefully or not at all."

ACTIVATION (slip):
- "Your feet go out from under you! You crash down onto the ice!"
- "You lose traction and slide—directly toward the edge!"
- "The ice betrays you. You fall hard."

EFFECT (ongoing cold):
- "The cold from the ice seeps through your boots. Your feet are going numb."
- "Prolonged contact with the ice drains your warmth. [Ongoing cold damage]"

RESOLUTION:
- "You find footing on a rough patch. Movement is possible here."
- "The ice ends. Solid ground beneath your feet again."
```

#### Radiation
```
HAZARD: Radiation
Biome: The_Roots (old power cores, medical bays)
Activation: Proximity (cumulative exposure)
Damage: 1 cumulative damage per turn, effects worsen with exposure
Save: STURDINESS vs DC 14 to reduce accumulation

APPEARANCE:
- "Warning symbols—the three-bladed circle—mark this area. Radiation hazard."
- "Your skin prickles. There's no visible threat, but something is very wrong here."
- "A faint blue glow emanates from damaged equipment. Cherenkov radiation."

ACTIVATION (entering):
- "You feel it immediately—a wrongness in your cells. Radiation."
- "The exposure begins. Time here is borrowed time."

EFFECT (accumulating):
- "Radiation Level 1: A persistent headache. Nausea creeps in."
- "Radiation Level 2: Your skin reddens. Movement becomes difficult."
- "Radiation Level 3: You're vomiting blood. Get out or die here."

RESOLUTION:
- "You clear the irradiated zone. The damage is done, but it won't get worse."
- "Anti-radiation medication slows the cellular damage. You'll survive this dose."
```

#### Darkness
```
HAZARD: Darkness
Biome: Universal
Activation: Area (persistent)
Effect: -4 to perception, -2 to attacks, +2 Stress accumulation
Mitigation: Light sources, Darkvision

APPEARANCE:
- "Darkness absolute. Your light source is the only thing between you and blindness."
- "The shadows here are hungry. They seem to swallow light."
- "Your torch flickers, struggling against the dark. It feels alive."

ACTIVATION:
- "The darkness closes in as your light fails."
- "You step beyond the reach of illumination. Blind."

EFFECT (in darkness):
- "Every sound is amplified in the dark. Every sound could be danger."
- "You move by touch and instinct. The darkness presses close."
- "The dark has weight here. It pushes against your sanity."

RESOLUTION:
- "Light returns! The darkness retreats, but doesn't go far."
- "Your new torch catches. The world exists again."
```

---

## STATIC TERRAIN FEATURES

### Feature Types

| Feature | Effect | Interaction |
|---------|--------|-------------|
| Pillar | Cover, blocks movement | Navigation, tactical |
| Chasm | Impassable, fall hazard | Bridge, jump, rope |
| Platform | Elevation change | Climbing, advantage |
| Debris | Difficult terrain | Slow movement |
| Barrier | Blocks movement/sight | Destruction, bypass |

### Static Terrain Templates

#### Pillars/Columns
```
STATIC TERRAIN: Pillar
Cover: Heavy (-4 dice to attackers, blocks line of sight)

APPEARANCE:
- "Stone columns rise here—structural supports that offer solid cover."
- "Rust-eaten pillars provide concealment. They might even stop a blow."
- "Massive support struts—if you need to hide, these will serve."

TACTICAL USE:
- "You duck behind the pillar. Solid cover from that direction."
- "The column blocks their line of sight. They'll have to reposition."
- "Using the pillars, you advance while staying protected."
```

#### Chasms/Pits
```
STATIC TERRAIN: Chasm
Type: Impassable, fall hazard
Crossing: Jump (FINESSE DC varies by width), bridge, rope

APPEARANCE:
- "A gaping chasm splits the floor. Darkness below. No visible bottom."
- "The pit opens before you—a wound in the structure, depth unknown."
- "The floor ends. The gap is [X] meters. Too far to jump safely."

CROSSING ATTEMPT:
- "You take a running start and leap... [Roll result]"
- "The rope stretches across the gap. It holds your weight—barely."
- "The makeshift bridge sways but supports you."

FAILURE:
- "You don't make it! You scramble for the edge—[Roll to catch yourself]"
- "The rope snaps! You fall!"
```

#### Elevation/Platforms
```
STATIC TERRAIN: Platform
Effect: Elevated position, climbing required
Tactical: +1 to ranged attacks from height, -1 to melee vs elevated

APPEARANCE:
- "A raised platform offers a vantage point over the area."
- "Walkways run along the upper level—better sightlines up there."
- "The gantry above would give you a significant tactical advantage."

CLIMBING:
- "You pull yourself up onto the platform. [X] AP spent."
- "The climb is tricky but manageable. You reach the elevated position."

TACTICAL:
- "From up here, you have clear shots across the entire room."
- "They're below you—an advantageous position."
```

---

## COVER SYSTEM

### Cover Qualities

| Quality | Defense Bonus | Notes |
|---------|--------------|-------|
| None | 0 | Fully exposed |
| Light | -2 dice to attackers | Partial concealment |
| Heavy | -4 dice, blocks LOS | Solid protection |

### Cover Templates

#### Light Cover
```
COVER: Light
Effect: Attackers suffer -2 dice penalty
Examples: Crates, thin walls, vegetation, smoke

APPEARANCE:
- "A stack of crates provides partial concealment."
- "Thin sheet metal offers some cover—better than nothing."
- "Smoke and debris partially obscure your position."

IN USE:
- "You crouch behind the crate. It won't stop everything, but it helps."
- "The partial cover makes you a harder target."
```

#### Heavy Cover
```
COVER: Heavy
Effect: Attackers suffer -4 dice, line of sight blocked
Examples: Stone pillars, thick walls, heavy machinery

APPEARANCE:
- "The stone pillar is solid—real protection."
- "Industrial machinery offers heavy cover. Thick metal between you and them."
- "The wall is thick enough to stop anything short of siege weapons."

IN USE:
- "You're completely hidden behind the barrier. They can't target you directly."
- "Heavy cover protects you from ranged attacks. They'll have to come around."
- "From behind the machinery, you're invisible to their line of sight."
```

---

## DYNAMIC FEATURES

### Dynamic Feature Types

| Type | Behavior | Interaction |
|------|----------|-------------|
| Steam Vent | Periodic blast | Timing, exploitation |
| Pressure Plate | Triggered | Avoidance, use against enemies |
| Unstable Structure | Triggered/timed | Escape, controlled collapse |
| Moving Platform | Periodic | Timing, navigation |

### Dynamic Feature Templates

#### Steam Vents
```
DYNAMIC FEATURE: Steam Vent
Activation: Periodic (every X rounds)
Damage: 1d6 fire on eruption
Warning: Visual/audio cue 1 round before

APPEARANCE:
- "Vents line the floor, steam occasionally hissing from their depths."
- "The grating here covers pressurized steam lines. They vent on a cycle."

WARNING:
- "The vent begins to whistle—pressure building!"
- "You hear the buildup. The steam is about to blow!"

ACTIVATION:
- "WHOOSH! Superheated steam erupts from the vent!"
- "The steam blast catches [target]! Scalding damage!"

TACTICAL:
- "Time it right and you can force enemies through the steam."
- "The vent cycle creates a pattern—wait for it to discharge, then move."
```

#### Pressure Plates
```
DYNAMIC FEATURE: Pressure Plate
Activation: Triggered by weight
Effect: Varies (trap, alarm, door, other mechanism)

APPEARANCE:
- "A section of floor sits slightly lower than its surroundings. Pressure plate."
- "The tile here is different—newer, or at least less worn. Suspicious."

TRIGGERING:
- "CLICK. You feel the plate depress beneath your foot."
- "The pressure plate activates! [Effect triggers]"

TYPES:
- Trap: "The plate triggers a dart barrage from the walls!"
- Alarm: "A klaxon sounds! Something knows you're here now!"
- Door: "The pressure plate causes a hidden door to grind open."
```

#### Collapsing Structure
```
DYNAMIC FEATURE: Unstable Structure
Activation: Triggered (damage/weight) or Timed
Effect: Area damage, terrain change

APPEARANCE:
- "The ceiling here is compromised. Cracks spider through the stonework."
- "Support beams have partially failed. This section could come down."
- "Structural damage is extensive. One wrong move and..."

WARNING:
- "The ceiling groans! Dust rains down! It's coming down!"
- "Cracks spread across the supports! This place is collapsing!"

COLLAPSE:
- "With a thunderous roar, the ceiling gives way!"
- "The structure finally fails! Tons of debris crash down!"
- "The collapse blocks the passage. [New terrain created]"
```

---

## BIOME-SPECIFIC HAZARD PACKAGES

### The Roots Hazard Ensemble
```
BIOME PACKAGE: The_Roots
Common Hazards: ToxicFumes, ElectricalHazard, UnstableFlooring, Darkness
Rare Hazards: Radiation, Vacuum (sealed sections)
Terrain: Debris, corroded platforms, failing grating
Dynamic: Steam vents, pressure plates, collapsing ceilings

Thematic Notes:
- Everything is failing, stressed, on the edge of catastrophe
- Hazards feel like neglect rather than malice
- Industrial danger—chemicals, electricity, structural failure
```

### Muspelheim Hazard Ensemble
```
BIOME PACKAGE: Muspelheim
Common Hazards: Fire, extreme heat, lava proximity
Rare Hazards: Volcanic gas, pyroclastic surge
Terrain: Obsidian edges, unstable basalt, ash drifts
Dynamic: Lava surges, eruptions, thermal vents

Thematic Notes:
- The environment itself is hostile and alive
- Hazards feel aggressive, primal, inevitable
- Survival is the constant challenge
```

### Niflheim Hazard Ensemble
```
BIOME PACKAGE: Niflheim
Common Hazards: Ice (slipping), extreme cold, frostbite
Rare Hazards: Avalanche, ice collapse, blizzard
Terrain: Slick ice, deep snow, frozen surfaces
Dynamic: Ice cracking, frost accumulation, wind chill

Thematic Notes:
- Hazards are patient, cumulative, inexorable
- Cold that drains rather than strikes
- Environment waits to claim the unwary
```

### Alfheim Hazard Ensemble
```
BIOME PACKAGE: Alfheim
Common Hazards: Reality distortion, temporal paradox, sensory overload
Rare Hazards: Spatial collapse, existence failure
Terrain: Non-Euclidean geometry, shifting layouts
Dynamic: Reality fluctuations, time loops, existence glitches

Thematic Notes:
- Hazards that shouldn't be possible
- The environment follows wrong rules
- Sanity damage as environmental effect
```

### Jotunheim Hazard Ensemble
```
BIOME PACKAGE: Jotunheim
Common Hazards: Scale hazards (giant steps), ancient traps
Rare Hazards: Guardian activation, gravitational anomaly
Terrain: Giant-scale architecture, oversized obstacles
Dynamic: Ancient mechanisms, guardian patrols

Thematic Notes:
- Hazards designed for beings much larger than you
- Ancient, deliberate, purposeful danger
- Scale itself is the challenge
```

---

## Writing Guidelines

### Hazard Flavor Principles
1. **Warning first** - players should usually see danger coming
2. **Clear consequences** - what happens if they fail
3. **Mechanical clarity** - include [bracketed effects]
4. **Biome consistency** - hazards should feel native to their environment
5. **Escalation potential** - some hazards should compound

### Sensory Engagement
- **Visual**: What does the hazard look like?
- **Audio**: What sounds warn of danger?
- **Tactile**: What does the effect feel like?
- **Temporal**: Is there a rhythm or pattern?

### Avoid
- Instant-death without warning
- Unavoidable hazards (there should always be a way)
- Generic descriptions ("it hurts")
- Inconsistent danger levels

---

## Quality Checklist

- [ ] Clear visual/audio warning
- [ ] Mechanical effect in [brackets]
- [ ] Save DC and attribute specified
- [ ] Biome-appropriate
- [ ] Escape/mitigation described
- [ ] Escalation path if relevant
- [ ] Varied from similar hazards
- [ ] Sensory detail included
