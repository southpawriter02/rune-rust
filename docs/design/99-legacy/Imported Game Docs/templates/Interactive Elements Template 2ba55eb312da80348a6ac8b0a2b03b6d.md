# Interactive Elements Template

Parent item: Rune and Rust: Flavor Text Template Library (Rune%20and%20Rust%20Flavor%20Text%20Template%20Library%202ba55eb312da808ea34ef7299503f783.md)

This template guides the creation of flavor text for interactive objects—mechanisms, containers, investigatable items, and barriers that players can manipulate during exploration.

---

## Overview

| Element Type | Purpose | Common Interactions |
| --- | --- | --- |
| **Mechanism** | Trigger effects | Pull, Activate, Hack |
| **Container** | Provide loot | Open, Search, Force |
| **Investigatable** | Reveal information | Examine, Read, Search |
| **Barrier** | Block passage | Open, Force, Hack, Key |

---

## Template Structure

```
INTERACTIVE ELEMENT: [Mechanism | Container | Investigatable | Barrier]
Object Type: [Specific type]
Biome: [The_Roots | Muspelheim | Niflheim | Alfheim | Jotunheim | Universal]
Interaction Type: [Pull | Open | Search | Read | Hack | Automatic | Examine]
Skill Check: [WITS | MIGHT | FINESSE | None]
Difficulty: [DC value]
Consequence Type: [Unlock | Trigger | Spawn | Reveal | Loot | Trap | None]
Loot Tier: [Common | Uncommon | Rare | Random]
Tags: ["Tag1", "Tag2"]

APPEARANCE TEXT: [Initial description]
INTERACTION TEXT: [During interaction]
SUCCESS TEXT: [Successful interaction]
FAILURE TEXT: [Failed interaction]
CONSEQUENCE TEXT: [What happens after]

```

---

## MECHANISMS

### Mechanism Types

| Type | Primary Interaction | Common Effects |
| --- | --- | --- |
| Lever | Pull | Door open, bridge extend, hazard toggle |
| Switch | Activate | Power routing, alarm, mechanism |
| Console | Hack | System access, information, control |
| Pressure Plate | Step/Weight | Door, trap, alarm |
| Valve | Turn | Steam, fluid, pressure |
| Crank | Turn | Mechanical, elevation, gate |

### Mechanism Templates

### Levers

```
MECHANISM: Lever
Interaction: Pull
Skill: None (or MIGHT if rusted/stuck)
DC: 0 (or DC 12 if stuck)

APPEARANCE:
- "A lever protrudes from the wall, its purpose unclear."
- "The control lever is marked with faded Dvergr runes. Could mean anything."
- "A massive lever—built for giant hands—has been jury-rigged for smaller users."

INTERACTION (standard):
- "You grasp the lever and pull. It moves with a satisfying clunk."
- "The lever resists for a moment, then gives way."

INTERACTION (stuck):
- "The lever is rusted in place. You'll need to force it."
- "Years of corrosion have locked the mechanism. Put your back into it."

SUCCESS:
- "The lever engages! Something mechanical responds in the walls."
- "CLUNK. The lever locks into the new position. Whatever it controls has changed."

FAILURE (stuck):
- "The lever won't budge. It's completely seized."
- "Despite your effort, the mechanism remains frozen."

CONSEQUENCE EXAMPLES:
- Door: "A grinding sound—somewhere nearby, a door is opening."
- Bridge: "Metal on metal. A bridge extends across the gap."
- Hazard: "The steam vents suddenly stop. The path is clear."
- Trap: "You hear clicking. That wasn't supposed to happen."

```

### Consoles/Terminals

```
MECHANISM: Console
Interaction: Hack
Skill: WITS
DC: Varies (12-18)

APPEARANCE:
- "A control console flickers to life as you approach. Some systems still function."
- "The terminal screen displays corrupted text. It might still be accessible."
- "Dvergr engineering—a masterwork console that's somehow still operational."

INTERACTION:
- "You begin working the console, navigating corrupted menus..."
- "Your fingers dance across the interface, trying to bypass security..."
- "The system resists intrusion. Layers of encryption, decaying but present."

SUCCESS (minor):
- "You're in. Basic access granted. You see: [Information/options]"

SUCCESS (major):
- "Full system access! You've cracked the security entirely."
- "The console yields its secrets. Administrative control achieved."

FAILURE:
- "Access denied. The security protocols hold."
- "The console locks you out. [X] attempts remaining before lockdown."

FAILURE (critical):
- "LOCKDOWN INITIATED. You've triggered the security response!"
- "The console sparks and dies. You've burned out the interface."

INFORMATION REVEALED (examples):
- "The console displays a map of this level. New areas marked."
- "Personnel logs. The last entry is [X] years old. It describes [event]."
- "System status: [relevant information about area/enemies/hazards]."

```

### Valves

```
MECHANISM: Valve
Interaction: Turn
Skill: MIGHT
DC: 10-14

APPEARANCE:
- "A large valve controls the flow of [substance] through these pipes."
- "The emergency shutoff valve is marked with faded warning symbols."
- "Corrosion has nearly welded this valve shut. It will take effort."

INTERACTION:
- "You grip the valve wheel and strain against centuries of corrosion..."
- "The valve groans as you force it to turn..."

SUCCESS:
- "With a final effort, the valve turns! You hear the flow change."
- "The valve opens. [Substance] begins to flow / stops flowing."

EFFECTS:
- Steam: "Steam vents seal shut. The scalding hazard is neutralized."
- Coolant: "Coolant floods the area, extinguishing fires."
- Toxic: "Warning! The valve releases toxic chemicals!"
- Pressure: "Pressure equalizes. A sealed door can now be opened."

```

---

## CONTAINERS

### Container Types

| Type | Loot Tier | Special Considerations |
| --- | --- | --- |
| Crate | Common | Easy to open |
| Locker | Common-Uncommon | May be locked |
| Chest | Uncommon-Rare | Often locked/trapped |
| Safe | Rare | Always locked, high security |
| Corpse | Random | May have contextual loot |
| Cache | Varies | Hidden, valuable |

### Container Templates

### Standard Containers (Crates/Lockers)

```
CONTAINER: Crate
Interaction: Open / Force
Skill: None (or MIGHT DC 8 if sealed)
Loot Tier: Common

APPEARANCE:
- "Supply crates are stacked against the wall. Some might still hold useful items."
- "A storage locker stands half-open. Worth checking."
- "Shipping containers bear faded labels. Whatever was inside has been here a long time."

OPENING (unlocked):
- "You pry open the crate. Inside: [contents]."
- "The locker opens easily. You find: [contents]."

OPENING (forced):
- "You put your weight into it. The seal breaks!"
- "With a grunt of effort, you force the container open."

CONTENTS (examples):
- "[Common consumable] x2, [Crafting material]"
- "Mostly rotted supplies. But you salvage [item]."
- "Empty. Someone got here first."

```

### Locked Containers

```
CONTAINER: Locked Chest
Interaction: Unlock / Force / Hack
Skill: FINESSE (lockpick) DC 14 / MIGHT (force) DC 16 / WITS (hack) DC 12
Loot Tier: Uncommon-Rare

APPEARANCE:
- "A reinforced chest sits here, lock still engaged after all these years."
- "The container's electronic lock glows faintly. Security still active."
- "Dvergr craftsmanship—this chest was built to protect its contents."

LOCKPICKING:
- "You insert your picks and feel for the mechanism..."
- "The lock is complex but not impossible. Delicate work..."

FORCING:
- "You set your shoulder against the chest and heave!"
- "Brute force isn't elegant, but sometimes it works."

HACKING:
- "You interface with the electronic lock, seeking exploits..."
- "The security system is old but sophisticated."

SUCCESS:
- "Click! The lock yields. Inside you find: [valuable contents]."
- "The chest opens! Your patience is rewarded: [loot]."

FAILURE (lockpick):
- "Your pick breaks. The lock remains engaged."
- "The mechanism defeats you. Perhaps another approach?"

FAILURE (force):
- "It won't budge. This thing was built to resist exactly this."
- "Your efforts only dent the casing. The lock holds."

FAILURE (hack):
- "Access denied. [X] attempts before complete lockout."
- "The system rejects your intrusion."

```

### Corpse Containers

```
CONTAINER: Corpse
Interaction: Search
Skill: None
Loot Tier: Random (contextual)

APPEARANCE:
- "A body lies here, long dead. What remains might tell you something."
- "The corpse wears [faction/role] equipment. They died [manner]."
- "Skeletal remains sprawl across the floor. Personal effects might remain."

SEARCHING:
- "You search the remains with respectful efficiency..."
- "You check the body for anything useful..."

CONTENTS (examples):
- "A journal fragment. The last entry reads: '[contextual lore]'."
- "They carried [equipment appropriate to role]. And [currency]."
- "Nothing useful remains. Time and scavengers have claimed everything."
- "Hidden in their boot: [valuable item]. They died protecting this."

CONTEXTUAL DISCOVERIES:
- "The wound that killed them suggests [enemy type]."
- "Their expression is peaceful. They didn't suffer."
- "Whatever happened here, they fought hard first."

```

---

## INVESTIGATABLES

### Investigatable Types

| Type | Information Provided | Skill Required |
| --- | --- | --- |
| Data Slate | Direct text | Read |
| Terminal Entry | System logs | Hack/Read |
| Inscription | Environmental lore | Examine/Interpret |
| Evidence | Forensic details | Search/WITS |
| Artifact | Historical context | Examine |

### Investigatable Templates

### Data Slates / Documents

```
INVESTIGATABLE: Data Slate
Interaction: Read
Skill: None (or WITS DC 10 for encrypted/damaged)

APPEARANCE:
- "A data slate lies among the debris, screen cracked but readable."
- "Personal logs, scattered when their owner fled—or died."
- "Official documentation, stamped with [faction] seals."

READING:
- "You activate the slate. Text scrolls across the damaged display..."
- "The handwriting is cramped, hurried. Someone wrote this in fear."

CONTENT TYPES:
- Personal: "A diary entry. '[Emotional/personal content that humanizes the setting]'"
- Technical: "Maintenance logs describe [mechanical/facility information]."
- Warning: "An official notice: 'DANGER - [specific threat] in [area]'"
- Lore: "Historical record of [event/location/faction]."

CONTENT TEMPLATE:
"[DATE/CONTEXT HEADER]
[BODY TEXT - 2-4 sentences of contextual information]
[SIGNATURE/ENDING - who wrote this and their fate implied]"

EXAMPLE:
"Day 847 of the Silence. The Servitors have gone wrong. They're 'correcting' anything that moves.
We're sealing ourselves in Storage Bay 7. If you find this, we're probably still there.
Still waiting.
- Foreman Keld"

```

### Environmental Inscriptions

```
INVESTIGATABLE: Inscription
Interaction: Examine
Skill: WITS DC 12 (to interpret meaning)

APPEARANCE:
- "Runes are carved into the wall here, worn but legible."
- "Someone scratched words into the metal. Recent, by the look of it."
- "Dvergr script covers this surface—a historical record, perhaps."

EXAMINING:
- "You trace the inscriptions, trying to parse their meaning..."
- "The runes are old but recognizable. They speak of..."

CONTENT TYPES:
- Warning: "These runes are a warning. '[Ancient danger described]'"
- History: "A record of events: '[Historical information]'"
- Direction: "Instructions for [ritual/mechanism/navigation]."
- Prophecy: "Cryptic verses that may refer to [current events]."

EXAMPLE:
"The runes translate roughly as: 'Here fell the last defenders of [place]. May their sacrifice
never be forgotten. May their failure never be repeated. The [threat] sleeps below—
let none wake it.'"

```

### Physical Evidence

```
INVESTIGATABLE: Evidence
Interaction: Search / Examine
Skill: WITS DC 14 (to draw conclusions)

APPEARANCE:
- "Something happened here. The evidence tells a story."
- "Scorch marks, blood, scattered equipment—a battle was fought."
- "The scene raises questions. Careful examination might provide answers."

EXAMINING:
- "You study the evidence, piecing together what occurred..."
- "The details paint a picture. You begin to understand..."

FINDINGS (examples):
- "The blast pattern suggests [weapon type]. [Faction] uses those."
- "The blood trail leads [direction]. Someone survived—barely."
- "These claw marks match [creature type]. It came from [direction]."
- "The equipment was deliberately sabotaged. This wasn't accident—it was murder."

MECHANICAL BENEFIT:
- "[Knowledge gained]: +2 to relevant checks against [enemy/hazard]."
- "You now know [enemy/hazard] is present in this area."

```

---

## BARRIERS

### Barrier Types

| Type | Primary Bypass | Alternatives |
| --- | --- | --- |
| Door | Key / Mechanism | Force, Hack |
| Gate | Mechanism | Force (high DC) |
| Hatch | Open / Key | Force |
| Debris | Clear | Explosives, Find Route |
| Energy Field | Disable Source | None |

### Barrier Templates

### Standard Doors

```
BARRIER: Door
Interaction: Open / Unlock / Force / Hack
Skill: Varies

APPEARANCE (unlocked):
- "A door blocks the passage. It looks operable."
- "Standard bulkhead door. Should open."

APPEARANCE (locked):
- "The door is sealed. A lock mechanism glows red."
- "Locked. Whatever's behind this, someone wanted it to stay there."

APPEARANCE (jammed):
- "The door is stuck—warped by heat/damage/corrosion."
- "This door has fused with its frame. Brute force might work."

OPENING (success):
- "The door grinds open, revealing [what's beyond]."
- "With a pneumatic hiss, the door slides aside."

FORCING (success):
- "You put everything into it. The door gives way with a screech!"
- "The lock mechanism breaks. The door swings open."

HACKING (success):
- "Override accepted. The door's security disengages."
- "You bypass the lock. The door recognizes you as authorized."

FAILURE:
- "The door refuses to budge. Another approach is needed."
- "Access denied. The security holds firm."

```

### Debris/Collapsed Passage

```
BARRIER: Debris
Interaction: Clear / Bypass
Skill: MIGHT DC 14 (clear) or Search for alternate route

APPEARANCE:
- "The corridor is blocked by collapsed debris. Getting through won't be easy."
- "Cave-in. Tons of rubble fill the passage."
- "Structural failure has sealed this route. There might be another way."

CLEARING (attempt):
- "You start moving rubble, piece by piece..."
- "You set your back to the debris and push..."

CLEARING (success):
- "After considerable effort, you create a passage through the debris."
- "The rubble shifts enough to squeeze through."

CLEARING (failure):
- "The debris is too heavy, too unstable. You'll need another approach."
- "Every piece you move sends more cascading down. It's not safe."

ALTERNATE ROUTE:
- "There might be another way around. [Perception check to find]"
- "The ventilation shaft above could bypass this blockage."

```

### Energy Barriers

```
BARRIER: Energy Field
Interaction: Disable Source
Skill: WITS DC 16 to locate generator, then destroy/hack it

APPEARANCE:
- "An energy field crackles across the doorway, barring passage."
- "Blue-white energy fills the arch. Touching it would be unwise."
- "A barrier of pure force blocks the way. There must be a power source."

EXAMINING:
- "The field is generated from somewhere nearby. Find the source."
- "Cables run from the barrier toward [direction]. The generator must be there."

INTERACTION (touch):
- "The field repels you! Shock damage lances through your body!"
- "Pain! The barrier is very much active!"

DISABLING:
- "You locate the generator and [destroy/hack] it. The field collapses."
- "Power flow interrupted! The barrier flickers and dies."

```

---

## SKILL CHECK GUIDELINES

### Difficulty Scale

| DC | Description | Success Rate (avg skill) |
| --- | --- | --- |
| 8 | Trivial | Almost guaranteed |
| 10 | Easy | Likely |
| 12 | Moderate | 50/50 |
| 14 | Challenging | Requires effort |
| 16 | Hard | Needs specialization |
| 18 | Very Hard | Expert level |
| 20+ | Extreme | Near impossible |

### Skill Application

| Skill | Used For |
| --- | --- |
| MIGHT | Forcing, breaking, heavy lifting |
| FINESSE | Lockpicking, delicate manipulation, precision |
| WITS | Hacking, solving puzzles, finding patterns |
| WILL | Resisting mental effects, intimidation |
| STURDINESS | Enduring consequences of failure |

---

## Writing Guidelines

### Interactive Element Principles

1. **Clear affordance** - what can I do here?
2. **Meaningful choice** - multiple approaches when possible
3. **Interesting failure** - failures should have consequences or alternatives
4. **Narrative reward** - success should feel earned
5. **World-building** - elements should reinforce setting

### Description Priorities

1. What is it?
2. What state is it in?
3. What might it do?
4. How might I interact with it?

### Avoid

- Elements with no purpose
- Single-solution puzzles without alternatives
- Instant failure states
- Breaking immersion with game terms in descriptions

---

## Quality Checklist

- [ ]  Clear appearance description
- [ ]  Interaction options specified
- [ ]  Skill check and DC defined
- [ ]  Success outcome described
- [ ]  Failure outcome described
- [ ]  Consequence/reward specified
- [ ]  Biome-appropriate flavor
- [ ]  World-building integrated
- [ ]  Multiple approaches where appropriate