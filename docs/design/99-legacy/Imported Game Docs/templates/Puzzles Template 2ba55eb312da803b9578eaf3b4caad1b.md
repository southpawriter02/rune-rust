# Puzzles Template

Parent item: Rune and Rust: Flavor Text Template Library (Rune%20and%20Rust%20Flavor%20Text%20Template%20Library%202ba55eb312da808ea34ef7299503f783.md)

This template guides the creation of flavor text for puzzles—environmental challenges that require observation, logic, or skill checks to solve. Puzzles provide non-combat engagement and reward exploration.

---

## Overview

| Puzzle Type | Primary Challenge | Key Skills |
| --- | --- | --- |
| **Mechanical** | Understand and operate machinery | WITS, FINESSE |
| **Environmental** | Navigate hazardous terrain | FINESSE, Observation |
| **Logic** | Decode patterns, sequences | WITS, Lore |
| **Combination** | Multiple locks/safes | WITS, Search |
| **Ritual** | Follow specific procedures | WITS, Lore |

---

## Template Structure

```
PUZZLE: [Mechanical | Environmental | Logic | Combination | Ritual]
Name: [Descriptive name]
Biome: [The_Roots | Muspelheim | Niflheim | Alfheim | Jotunheim | Universal]
Difficulty: [Easy | Moderate | Hard | Expert]
Success Threshold: [Number of successes required]
Time Pressure: [None | Soft (consequences) | Hard (failure)]
Failure Damage: [None | Minor | Moderate | Severe]
Tags: ["Tag1", "Tag2"]

SETUP TEXT: [Initial puzzle presentation]
HINT TEXT: [Clues available to observant players]
ATTEMPT TEXT: [During puzzle solving]
SUCCESS TEXT: [Puzzle solved]
PARTIAL SUCCESS TEXT: [Progress made but not complete]
FAILURE TEXT: [Failed attempt]
CONSEQUENCE TEXT: [What happens on failure with damage]

```

---

## MECHANICAL PUZZLES

### Mechanical Puzzle Types

| Type | Challenge | Biome Affinity |
| --- | --- | --- |
| Gear Alignment | Rotate gears to connect power flow | The_Roots |
| Pressure Regulation | Balance multiple valves | The_Roots |
| Circuit Routing | Connect power paths | The_Roots |
| Lock Mechanism | Complex multi-stage lock | Universal |
| Ancient Device | Operate unknown technology | Jotunheim |

### Mechanical Puzzle Templates

### Gear Alignment Puzzle

```
PUZZLE: Mechanical - Gear Alignment
Difficulty: Moderate
Success Threshold: 3 successes (WITS DC 12)
Time Pressure: None
Failure Damage: None (just doesn't work)

SETUP:
"A mechanism dominates the wall—interlocking gears of various sizes,
some frozen with rust, others still capable of movement. Power flows
through this system, but the gears are misaligned. Nothing connects."

"The goal is clear: align the gears so power can flow from the input
shaft to the output mechanism. But which gears move? Which are fixed?"

HINTS:
- "Examining closely: Some gears have wear patterns showing their
  original positions."
- "The rust on certain gears is superficial—they can still turn."
- "A schematic is etched into the panel below, though damaged.
  You can make out part of the intended configuration."

ATTEMPT:
- "You grip a gear and attempt to rotate it... [Roll WITS]"
- "Analyzing the mechanism, you try to visualize the correct arrangement..."

SUCCESS (per attempt):
- "The gear clicks into place. You hear something engage deeper in the mechanism."
- "Yes! That's the correct position. The power flow extends further."
- "The final gear aligns. The mechanism hums to life!"

PARTIAL:
- "Progress. Some gears are aligned, but the circuit isn't complete yet."

FAILURE (attempt):
- "The gear won't move that way. You've misread the mechanism."
- "That configuration is wrong. The gears grind against each other."

FULL SUCCESS:
"With the final gear in place, the mechanism engages! Power flows
through the system—lights flicker on, a door grinds open, and
somewhere deep in the structure, something awakens."

```

### Pressure Regulation Puzzle

```
PUZZLE: Mechanical - Pressure Regulation
Difficulty: Hard
Success Threshold: 4 successes (WITS DC 14)
Time Pressure: Soft (pressure building)
Failure Damage: Minor (steam scalds)

SETUP:
"Three massive valves control the pressure system here. Steam hisses
from overpressured pipes—the system is critical. You need to balance
the pressure across all three sections to open the blast door.

Gauges show: Section A - RED (too high), Section B - YELLOW (borderline),
Section C - GREEN (optimal). You need all three in the green."

HINTS:
- "The valves are interconnected. Adjusting one affects the others."
- "Section A feeds into B, B feeds into C. Relieve A first?"
- "A faded diagram shows the original pressure levels."

ATTEMPT:
- "You begin adjusting the valves, watching the gauges respond..."
- "Each turn of the valve changes the readings across all sections..."

SUCCESS (per attempt):
- "The pressures shift. You're getting closer to equilibrium."
- "Section A drops to yellow. Progress."

PRESSURE WARNING (soft failure):
- "The pressure spikes! Steam vents dangerously close!"
- "You've destabilized the system. Work faster!"

FAILURE (damage):
- "A pipe bursts! Steam scalds your arm! [1d6 fire damage]"
- "The pressure releases violently! [Minor damage to all nearby]"

FULL SUCCESS:
"All three gauges settle into the green. The system stabilizes,
and with a heavy clunk, the blast door's locks disengage.
The way is open."

```

---

## ENVIRONMENTAL PUZZLES

### Environmental Puzzle Types

| Type | Challenge | Biome Affinity |
| --- | --- | --- |
| Timed Crossing | Navigate before hazard reactivates | Muspelheim |
| Platforming | Navigate unstable/elevated terrain | Universal |
| Hazard Navigation | Find safe path through danger | All |
| Temperature Management | Survive extreme conditions | Muspelheim, Niflheim |

### Environmental Puzzle Templates

### Timed Hazard Crossing

```
PUZZLE: Environmental - Timed Crossing
Difficulty: Moderate
Success Threshold: 2 successes (FINESSE DC 12)
Time Pressure: Hard (6 seconds between eruptions)
Failure Damage: Moderate (caught in eruption)

SETUP:
"Steam vents line the corridor in a deadly grid pattern. They erupt
in sequence—a wave of scalding death that sweeps through every few
seconds. You can see the pattern: left vents fire, then center, then
right. A gap exists between cycles.

The safe zone on the other side is [X] meters away. You have roughly
six seconds to cross before the next eruption."

HINTS:
- "Watch the pattern. Left, center, right. Then a pause."
- "The third vent from the left seems clogged—it might not fire."
- "If you're caught mid-crossing, there's a depression that might
  offer shelter."

ATTEMPT:
- "You time the eruption and sprint! [Roll FINESSE]"
- "Watching the vents, you prepare to move the moment the cycle ends..."

SUCCESS (first roll):
- "You bolt forward, steam erupting behind you! Halfway there!"
- "Perfect timing! The vents fire just after you pass!"

SUCCESS (second roll - completion):
- "You dive clear as the next cycle begins! Safe on the other side!"
- "The steam roars behind you but you're through! The crossing is complete!"

FAILURE (caught):
- "You mistimed it! Steam engulfs you! [2d6 fire damage, thrown back]"
- "The eruption catches you mid-stride! [Damage + restart required]"

PARTIAL (shelter found):
- "You throw yourself into the depression as steam roars overhead!
  Safe for the moment, but you'll need to try again."

```

### Unstable Platform Navigation

```
PUZZLE: Environmental - Platforming
Difficulty: Hard
Success Threshold: 3 successes (FINESSE DC 14)
Time Pressure: Soft (platforms degrading)
Failure Damage: Severe (fall damage)

SETUP:
"The floor has collapsed, leaving only scattered platforms of debris
suspended over a deep shaft. Some look stable. Others... less so.
The far side is reachable, but you'll need to cross from platform
to platform.

Each jump is a gamble. And the platforms creak ominously under
even your gaze."

HINTS:
- "That larger platform in the center looks stable—a good waypoint."
- "Some platforms are connected by bent rebar—could be used as handholds."
- "The rust patterns show which surfaces have held weight before."

ATTEMPT:
- "You leap to the first platform... [Roll FINESSE]"
- "Testing your weight carefully, you prepare to jump..."

SUCCESS (per platform):
- "You land solidly! The platform holds! Two more to go."
- "A perfect leap! You catch the edge and pull yourself up!"

PLATFORM DEGRADATION (soft failure):
- "The platform shudders! Cracks spread! It won't survive another landing!"
- "Your platform begins to tilt! Move now or fall with it!"

FAILURE (fall):
- "The platform gives way! You plummet! [Fall damage based on depth]"
- "You misjudge the distance! Your fingers scrape the edge but can't
  hold! [Fall damage + restart from bottom]"

FULL SUCCESS:
"With a final, desperate leap, you reach solid ground! Behind you,
the last platform finally surrenders to gravity, crashing into the
darkness below. You made it—barely."

```

---

## LOGIC PUZZLES

### Logic Puzzle Types

| Type | Challenge | Skill Focus |
| --- | --- | --- |
| Sequence | Identify and continue patterns | WITS, Observation |
| Code Decryption | Break coded messages | WITS, Lore |
| Symbol Matching | Associate symbols with meanings | WITS, Lore |
| Order/Arrangement | Place objects in correct order | WITS |

### Logic Puzzle Templates

### Sequence Puzzle

```
PUZZLE: Logic - Sequence
Difficulty: Moderate
Success Threshold: 2 successes (WITS DC 12)
Time Pressure: None
Failure Damage: None (reset to try again)

SETUP:
"Four panels line the wall, each displaying a symbol. Three are
illuminated in sequence: RUNE-GEAR-FLAME. A fourth panel awaits
input. Clearly, you must determine what comes next.

Beneath the panels, Dvergr script reads: 'Speak the language of
creation. Begin with the word. End with the world.'"

HINTS:
- "RUNE (word/concept) -> GEAR (mechanism) -> FLAME (energy)...
  What logically follows?"
- "Creation: idea, then tool, then power, then...?"
- "The final panel shows four options: STONE, LIFE, WATER, VOID"

ATTEMPT:
- "You study the sequence, trying to find the pattern..."
- "What follows flame in the cycle of creation?"

SUCCESS:
- "LIFE! Word becomes tool, tool harnesses power, power creates life!
  The panel illuminates as you press it!"
- "The pattern resolves. You understand the Dvergr philosophy now."

FAILURE:
- "The panels flash red and reset. That wasn't the answer."
- "Wrong sequence. The puzzle resets to its initial state."

FULL SUCCESS:
"All four panels glow in harmony. The mechanism recognizes the
complete sequence, and a hidden panel slides open, revealing
[reward/passage]."

```

### Code Decryption Puzzle

```
PUZZLE: Logic - Code Decryption
Difficulty: Hard
Success Threshold: 3 successes (WITS DC 14)
Time Pressure: None
Failure Damage: None (but limited attempts: 3)

SETUP:
"The terminal demands a passcode. A note nearby reads:

'Remember: The First Forge-Master
The year of the Great Seal
The number of founding clans
Combined. Reversed. Entered.'

Someone didn't trust their memory. Lucky for you."

HINTS:
- "Forge-Master Balder founded the Roots. Historical records give
  dates for the Great Seal."
- "A plaque in the previous room mentioned 'seven clans united.'"
- "Combined might mean concatenated. Reversed means... backwards?"

ATTEMPT:
- "You piece together the historical references..."
- "Working through the cipher logic..."

SUCCESS (per step):
- "The Great Seal was Year 247. That's one piece."
- "Seven founding clans. Combined with the year... 2477."
- "Reversed: 7742. You enter the code..."

FAILURE:
- "ACCESS DENIED. [X] attempts remaining."
- "The terminal beeps angrily. That combination was wrong."

LOCKOUT:
- "TERMINAL LOCKED. Maximum attempts exceeded. A backup access
  point may exist elsewhere."

FULL SUCCESS:
"ACCESS GRANTED. Welcome, Archivist.

The terminal unlocks, revealing [information/control/passage]."

```

---

## COMBINATION PUZZLES

### Combination Puzzle Types

| Type | Challenge | Notes |
| --- | --- | --- |
| Safe Combination | Find/deduce combination | Often spans multiple rooms |
| Multi-Lock | Multiple locks, multiple keys/solutions | Sequence matters |
| Distributed | Parts scattered across area | Exploration required |

### Combination Puzzle Templates

### Safe Puzzle

```
PUZZLE: Combination - Safe
Difficulty: Moderate-Hard (depends on hint availability)
Success Threshold: 3 successes (WITS DC 12) OR correct combination
Time Pressure: None
Failure Damage: None (but may trigger alarm after X attempts)

SETUP:
"A heavy safe is built into the wall, its combination dial worn smooth
from use. Whatever is inside was important enough to protect with
Dvergr engineering—but not important enough for the original owner
to remember the combination.

Hints exist somewhere in this facility. They always do."

HINTS (found elsewhere):
- Painting: "A portrait of a Dvergr family. Inscription: 'Born in
  different seasons: Winter-3, Summer-12, Autumn-7.'"
- Journal: "The safe combination? Easy. Our anniversary, doubled."
- Engineering Note: "Reset code: 24-12-36. For emergency use only."

ATTEMPT (brute force):
- "You try to crack the combination by feel alone... [Roll WITS]"
- "Without the combination, you resort to methodical attempts..."

ATTEMPT (correct combination):
- "You dial in the numbers: [X]-[Y]-[Z]..."
- "The combination clicks into place, number by number..."

SUCCESS (brute force):
- "You feel the tumbler click. First number found."
- "Another click! Two down, one to go."
- "The final tumbler aligns! The safe opens!"

SUCCESS (correct combo):
- "The mechanism recognizes the combination. The safe opens with
  a satisfying clunk. Inside: [valuable contents]"

FAILURE:
- "The dial spins freely. Wrong combination."

ALARM (after 5 failures):
- "The safe begins to emit a shrill alarm! You've triggered security!"

```

---

## RITUAL PUZZLES

### Ritual Puzzle Types

| Type | Challenge | Biome Affinity |
| --- | --- | --- |
| Rune Activation | Activate runes in correct order/manner | Universal |
| Offering | Provide correct items/actions | Alfheim, Jotunheim |
| Repetition | Mimic observed pattern | Universal |

### Ritual Puzzle Templates

### Rune Activation Puzzle

```
PUZZLE: Ritual - Rune Activation
Difficulty: Moderate
Success Threshold: 3 successes (WITS DC 12)
Time Pressure: None
Failure Damage: Minor (magical backlash)

SETUP:
"Five runes are carved into the floor in a pentagonal arrangement,
each dark and dormant. A central rune pulses faintly, awaiting
activation of the others.

Wall inscriptions read: 'The cycle of elements, as the sun travels.
Begin where fire meets earth. End where water becomes air.'"

HINTS:
- "The elements: Fire, Earth, Metal, Water, Air. The classical cycle."
- "Where fire meets earth—the forge. Where water becomes air—evaporation."
- "Each rune is marked with an elemental symbol."

ATTEMPT:
- "You approach the first rune and channel your intent..."
- "Following the elemental cycle, you begin the activation..."

SUCCESS (per rune):
- "The Fire rune flares to life! One of five."
- "Earth awakens. The pattern grows stronger."
- "Metal, Water, Air—each rune joins the pattern!"

FAILURE (wrong order):
- "The rune flashes angrily and goes dark! Magical backlash! [1d4 damage]"
- "The pattern breaks. You must begin again."

FULL SUCCESS:
"All five runes blaze with elemental light, feeding into the central
glyph. The combined energies unlock [door/chest/secret/power]."

```

---

## ALFHEIM PUZZLES (Reality Bending)

### Paradox Puzzles

```
PUZZLE: Paradox - Impossible Geometry
Difficulty: Expert
Success Threshold: 4 successes (WITS DC 16, WILL DC 14 to stay sane)
Time Pressure: Soft (sanity damage)
Failure Damage: Stress damage

SETUP:
"The room has four doors. Each door leads back to this room—but
different. The furniture moves. The light changes. You are everywhere
and nowhere.

To escape, you must find the door that leads forward. But forward
doesn't exist here in the normal sense."

HINTS:
- "One detail remains constant in every iteration. Find it."
- "The Cursed Choir's pitch changes near the correct door."
- "Your own footprints exist before you make them. Follow the ones
  that lead somewhere new."

ATTEMPT:
- "You try to impose logic on the impossible... [Roll WITS]"
- "You choose a door and step through, hoping..."
- "Fighting the disorientation, you search for consistency..."

SUCCESS:
- "There! That crack in the wall appears in every iteration! It's real!"
- "The Choir pitch shifts—this door is different!"

FAILURE:
- "You loop again. The room resets. Your mind strains. [+1 Stress]"
- "Was this room always like this? You're losing track..."

SANITY DAMAGE:
- "The loops are eroding your sense of reality. [Stress accumulation]"
- "You've been here before. Haven't you? Haven't you? [Roll WILL]"

FULL SUCCESS:
"You find the thread of consistency in the chaos—the one true path.
Stepping through the correct door, reality solidifies. You've escaped
the paradox. For now."

```

---

## WRITING GUIDELINES

### Puzzle Design Principles

1. **Observable solution** - players can solve by observation/logic
2. **Skill backup** - dice rolls as alternative to player skill
3. **Progressive hints** - more clues available on examination
4. **Meaningful failure** - failures have interesting consequences
5. **World integration** - puzzles fit the setting

### Difficulty Calibration

| Difficulty | Target Rolls | Hint Availability | Penalty |
| --- | --- | --- | --- |
| Easy | 1-2 | Obvious | None |
| Moderate | 2-3 | Available | Minor |
| Hard | 3-4 | Hidden | Moderate |
| Expert | 4+ | Obscure | Severe |

### Avoid

- Single-solution with no alternatives
- Arbitrary solutions (moon logic)
- Instant failure states
- Player knowledge requirements (genre savvy)
- Tedious repetition

---

## Quality Checklist

- [ ]  Clear initial presentation
- [ ]  Observable hints exist
- [ ]  Skill check alternative available
- [ ]  Partial progress acknowledged
- [ ]  Failure consequences defined
- [ ]  Success reward specified
- [ ]  Biome-appropriate flavor
- [ ]  Time pressure clearly communicated (if any)
- [ ]  Multiple solution approaches (ideally)