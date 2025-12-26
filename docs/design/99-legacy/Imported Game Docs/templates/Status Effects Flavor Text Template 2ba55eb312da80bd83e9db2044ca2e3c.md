# Status Effects Flavor Text Template

Parent item: Rune and Rust: Flavor Text Template Library (Rune%20and%20Rust%20Flavor%20Text%20Template%20Library%202ba55eb312da808ea34ef7299503f783.md)

This template guides the creation of flavor text for status effects—temporary conditions that affect characters in combat and exploration. Status effects communicate mechanical changes through narrative description.

---

## Overview

| Category | Purpose | Duration Type |
| --- | --- | --- |
| **Damage Over Time** | Ongoing damage | Rounds/Turns |
| **Debuffs** | Stat/ability penalties | Rounds/Condition |
| **Buffs** | Stat/ability bonuses | Rounds/Condition |
| **Control** | Movement/action restriction | Rounds/Condition |
| **Environmental** | Biome-specific effects | While in area |

---

## Template Structure

```
STATUS EFFECT: [Effect Name]
Category: [DOT | Debuff | Buff | Control | Environmental]
Source: [What causes this - attack, spell, environment, etc.]
Duration: [X rounds | Until condition | While in area]
Severity: [Minor | Moderate | Severe | Critical]
Interaction: [How it interacts with other effects]
Tags: ["Tag1", "Tag2"]

APPLICATION TEXT: [When effect is first applied]
ACTIVE TEXT: [Ongoing effect description]
TICK TEXT: [When periodic damage/effect occurs]
EXPIRATION TEXT: [When effect ends naturally]
REMOVAL TEXT: [When effect is cleansed/cured]

```

---

## DAMAGE OVER TIME EFFECTS

### DOT Registry

| Effect | Damage Type | Source | Duration |
| --- | --- | --- | --- |
| **Burning** | Fire | Fire attacks, lava, spells | 3 rounds |
| **Bleeding** | Physical | Slashing, piercing crits | Until healed |
| **Poisoned** | Poison | Toxic attacks, fumes | 5 rounds |
| **Frostbitten** | Cold | Ice attacks, extreme cold | 3 rounds |
| **Corroding** | Acid | Acid attacks, Rust-Witch | 3 rounds |
| **Electrocuted** | Lightning | Lightning, electrical hazards | 2 rounds |

### DOT Templates

### Burning

```
STATUS EFFECT: Burning
Category: DOT
Damage: 1d6 fire per round
Duration: 3 rounds (or until extinguished)
Severity: Moderate

APPLICATION:
- "Flames catch! Fire spreads across [target], hungry and hot!"
- "You're ON FIRE! Orange flames lick at your flesh!"

ACTIVE:
- "[Target] burns! The flames consume, relentless!"
- "Fire continues to spread, feeding on cloth and flesh alike!"

TICK (each round):
- "The flames BURN! [1d6 fire damage]"
- "Fire sears flesh! [Damage applied]"
- "The burning intensifies! [Damage]"

EXPIRATION:
- "The flames finally die, leaving charred wounds behind."
- "The fire burns itself out. The damage is done."

REMOVAL (extinguished):
- "Water/sand/effort smothers the flames! The burning stops!"
- "The fire is out! Relief floods through the scorched areas."

```

### Bleeding

```
STATUS EFFECT: Bleeding
Category: DOT
Damage: 1d4 per round
Duration: Until healed or bandaged (WITS DC 10)
Severity: Moderate (escalates if untreated)

APPLICATION:
- "Blood flows! The wound is deep and won't stop bleeding!"
- "A crimson stain spreads from the injury. Bleeding badly!"

ACTIVE:
- "[Target] is losing blood steadily. The wound weeps red."
- "Blood drips, pools, spreads. This needs attention."

TICK (each round):
- "Blood loss continues! [1d4 damage]"
- "The bleeding doesn't stop! [Damage applied]"
- "Growing weaker from blood loss... [Damage]"

EXPIRATION:
- "The bleeding finally clots naturally. You're pale but stable."

REMOVAL (healed/bandaged):
- "The wound is bound! Bleeding controlled!"
- "Healing energy closes the wound. The bleeding stops."
- "Pressure and bandaging stem the flow. You'll live."

```

### Poisoned

```
STATUS EFFECT: Poisoned
Category: DOT
Damage: 1d6 poison per round
Duration: 5 rounds (STURDINESS save each round to end early)
Severity: Moderate-Severe

APPLICATION:
- "Poison courses through your veins! Your vision swims!"
- "The toxin takes hold! Nausea and weakness spread rapidly!"

ACTIVE:
- "[Target] is poisoned! The toxin works through their system."
- "Poison burns through [target]'s blood. They grow paler."

TICK (each round):
- "The poison BURNS internally! [1d6 poison damage]"
- "Another wave of toxic pain! [Damage applied]"
- "The venom continues its work! [Damage]"

SAVE ATTEMPT:
- "Fighting off the poison... [STURDINESS check]"
- Success: "Your body purges the toxin! The poison ends!"
- Failure: "The poison holds on! Another round of suffering!"

EXPIRATION:
- "The poison runs its course. You're weakened but clear."
- "Finally, the toxin dissipates. You survive."

REMOVAL (antidote):
- "The antidote works! You feel the poison neutralize!"
- "Relief floods through you as the cure takes effect!"

```

### Corroding (Rust-Witch)

```
STATUS EFFECT: Corroding
Category: DOT (equipment damage)
Damage: 1d4 acid + equipment degradation
Duration: 3 rounds
Severity: Moderate (severe for equipment)

APPLICATION:
- "Entropy magic takes hold! Metal rusts, leather rots!"
- "Corrosion spreads across your equipment like a disease!"

ACTIVE:
- "[Target]'s equipment degrades before their eyes!"
- "Rust blooms. Leather cracks. Everything falls apart."

TICK (each round):
- "The corrosion continues! [1d4 damage + 1 equipment durability]"
- "More of your gear succumbs to decay! [Damage + durability loss]"

EQUIPMENT EFFECT:
- "Your [weapon/armor] grows brittle! [-1 to effectiveness]"
- "The rust has weakened your equipment significantly!"

EXPIRATION:
- "The entropic magic fades. The damage remains."

REMOVAL:
- "Purifying magic halts the decay! Equipment is damaged but stable."

```

---

## DEBUFF EFFECTS

### Debuff Registry

| Effect | Penalty | Duration |
| --- | --- | --- |
| **Slowed** | -2 movement, -1 FINESSE | 3 rounds |
| **Weakened** | -2 MIGHT, -1 damage | 3 rounds |
| **Blinded** | -4 perception, -2 attack | Until cleared |
| **Stunned** | Cannot act | 1-2 rounds |
| **Frightened** | Must flee or -2 all | Until escape/end |
| **Marked** | Enemies have +2 vs you | Until removed |

### Debuff Templates

### Slowed

```
STATUS EFFECT: Slowed
Category: Debuff
Penalty: -2 movement, -1 FINESSE checks
Duration: 3 rounds
Source: Ice magic, injuries, fatigue

APPLICATION:
- "Your movements become sluggish! Everything feels heavy!"
- "Cold/injury/exhaustion weighs you down! You're slowed!"

ACTIVE:
- "[Target] moves like they're wading through mud."
- "Every motion is an effort. Speed is a memory."

MECHANICAL:
- "Movement reduced! [-2 movement, -1 FINESSE]"
- "Actions come slowly, painfully."

EXPIRATION:
- "The effect lifts! You can move normally again!"
- "Your limbs remember their purpose. Full speed restored!"

```

### Stunned

```
STATUS EFFECT: Stunned
Category: Debuff/Control
Penalty: Cannot take actions
Duration: 1-2 rounds
Source: Critical hits, lightning, concussion

APPLICATION:
- "The blow staggers you! Stars explode across your vision!"
- "STUNNED! Your body refuses to respond!"

ACTIVE:
- "[Target] stands dazed, unable to act!"
- "They're trying to shake it off, but their mind won't cooperate."

MECHANICAL:
- "Cannot take actions this round! [Stunned]"
- "Helpless until the effect clears!"

RECOVERY (each round):
- "Fighting through the daze... [STURDINESS check if applicable]"

EXPIRATION:
- "The world snaps back into focus! You can act again!"
- "Your head clears. The stun fades."

```

### Frightened

```
STATUS EFFECT: Frightened
Category: Debuff/Control
Penalty: Must flee OR -2 to all actions if cannot flee
Duration: Until source escaped or effect ends
Source: Trauma triggers, fear effects, enemy abilities

APPLICATION:
- "Terror grips you! Every instinct screams to RUN!"
- "Fear takes hold! [Source] is more than you can face!"

ACTIVE (fleeing possible):
- "[Target] flees from [fear source]!"
- "They run! Survival instinct overrides everything!"

ACTIVE (cannot flee):
- "[Target] cowers, trembling! Fear undermines every action!"
- "Trapped with the terror! [-2 to all actions]"

MECHANICAL:
- "Must move away from [source] OR suffer -2 to all checks!"
- "Fear dominates decision-making!"

EXPIRATION:
- "The fear releases its grip. You can think clearly again."
- "Whatever held you in terror... its power fades."

```

---

## BUFF EFFECTS

### Buff Registry

| Effect | Bonus | Duration |
| --- | --- | --- |
| **Hasted** | +2 movement, +1 action | 3 rounds |
| **Strengthened** | +2 MIGHT, +1 damage | 3 rounds |
| **Protected** | +2 defense, damage resistance | 3 rounds |
| **Inspired** | +2 to all checks | 3 rounds |
| **Focused** | +2 to next attack/skill check | Until used |

### Buff Templates

### Strengthened

```
STATUS EFFECT: Strengthened
Category: Buff
Bonus: +2 MIGHT, +1 damage per hit
Duration: 3 rounds
Source: Spells, consumables, abilities

APPLICATION:
- "Power surges through your muscles! You feel STRONG!"
- "Magical/chemical enhancement kicks in! [Strengthened!]"

ACTIVE:
- "[Target] moves with enhanced power! Every blow hits harder!"
- "Strength beyond natural limits flows through them!"

MECHANICAL:
- "+2 to MIGHT checks, +1 damage on all attacks!"
- "Physical capabilities enhanced!"

EXPIRATION:
- "The enhanced strength fades. You return to normal."
- "The power boost ends, leaving you feeling almost weak by comparison."

```

### Inspired

```
STATUS EFFECT: Inspired
Category: Buff
Bonus: +2 to all checks
Duration: 3 rounds
Source: Leadership, morale, abilities

APPLICATION:
- "Inspiration fills you! You can do this!"
- "Courage and clarity surge! [Inspired!]"

ACTIVE:
- "[Target] acts with confidence and precision!"
- "They're in the zone! Everything clicks!"

MECHANICAL:
- "+2 to ALL checks while inspired!"
- "Confidence translates to capability!"

EXPIRATION:
- "The inspiration fades, but the confidence lingers."
- "You return to normal, but you remember what peak felt like."

```

---

## CONTROL EFFECTS

### Control Registry

| Effect | Restriction | Duration |
| --- | --- | --- |
| **Immobilized** | Cannot move | Until freed |
| **Grappled** | Cannot move, -2 to actions | Until escape |
| **Prone** | -2 defense vs melee, +2 vs ranged | Until standing |
| **Silenced** | Cannot speak/cast verbal spells | Until removed |

### Control Templates

### Immobilized

```
STATUS EFFECT: Immobilized
Category: Control
Restriction: Cannot move from current position
Duration: Until freed (usually requires action or help)
Source: Ice, webs, restraints, pinning attacks

APPLICATION:
- "You're stuck! Ice/webs/restraints hold you in place!"
- "Movement is impossible! You're immobilized!"

ACTIVE:
- "[Target] struggles against their bonds!"
- "They can act, but they cannot move from that spot!"

ESCAPE ATTEMPT:
- "Straining against the restraint... [MIGHT/FINESSE check]"
- Success: "You break free! Movement restored!"
- Failure: "Still stuck! The restraint holds!"

MECHANICAL:
- "Cannot use movement! Can still attack/cast/etc."
- "Enemies know exactly where you'll be!"

REMOVAL:
- "The restraint breaks/melts/releases! You're free!"

```

### Prone

```
STATUS EFFECT: Prone
Category: Control
Restriction: On the ground
Penalty: -2 defense vs melee, +2 defense vs ranged
Duration: Until standing (costs movement)
Source: Knockdown, trips, voluntary drop

APPLICATION:
- "You're knocked down! The ground rushes up to meet you!"
- "You hit the floor! [Prone]"

ACTIVE:
- "[Target] is on the ground, vulnerable to close attacks!"
- "They scramble, trying to regain their feet!"

MECHANICAL:
- "Melee attackers have advantage! Ranged attackers have disadvantage!"
- "Standing up will cost movement action!"

STANDING:
- "You push yourself up! Back on your feet!"
- "Rising takes effort, but you're standing again!"

```

---

## ENVIRONMENTAL EFFECTS

### Environmental Effect Registry

| Effect | Source | Duration |
| --- | --- | --- |
| **Extreme Heat** | Muspelheim | While in area |
| **Extreme Cold** | Niflheim | While in area |
| **Reality Distortion** | Alfheim | While in area |
| **Toxic Atmosphere** | The_Roots | While in area |
| **Darkness** | Any | While in area |

### Environmental Templates

### Extreme Heat (Muspelheim)

```
STATUS EFFECT: Extreme Heat
Category: Environmental
Effect: 1d4 fire damage per round, -1 to STURDINESS checks
Duration: While in heated area
Mitigation: Heat resistance, protective gear

APPLICATION:
- "The heat here is INTENSE. You feel yourself cooking."
- "This temperature is beyond safe. Damage is inevitable."

ACTIVE:
- "The heat saps your strength and burns your lungs."
- "Sweat evaporates instantly. You're dehydrating rapidly."

TICK:
- "The heat damages you! [1d4 fire damage]"
- "Another moment in this furnace! [Damage]"

LEAVING AREA:
- "The temperature drops as you leave the heat zone. Relief!"

```

### Reality Distortion (Alfheim)

```
STATUS EFFECT: Reality Distortion
Category: Environmental
Effect: -2 to perception, +10% miscast chance, +1 Stress/hour
Duration: While in Alfheim regions
Mitigation: WILL saves, reality anchors

APPLICATION:
- "Reality bends around you. What you see cannot be trusted."
- "The Cursed Choir grows louder. The world grows uncertain."

ACTIVE:
- "Is that wall closer or further? You can't tell anymore."
- "Colors have sounds. Sounds have textures. Nothing makes sense."

STRESS ACCUMULATION:
- "The distortion wears at your sanity. [+1 Stress]"
- "Your mind strains against the impossible. [Stress accumulated]"

LEAVING AREA:
- "Reality snaps back into place. The relief is immense."
- "The world makes sense again. The Choir fades to background."

```

---

## EFFECT INTERACTIONS

### Stacking Rules

| Interaction | Rule |
| --- | --- |
| Same effect, same source | Does not stack (refresh duration) |
| Same effect, different source | Higher value applies |
| Related buffs | Stack unless noted |
| Opposing effects | Cancel or reduce each other |

### Interaction Templates

```
EFFECT INTERACTION: Same Effect Refresh
- "The [effect] refreshes! Duration extended!"
- "Another application of [effect] resets the timer!"

EFFECT INTERACTION: Effect Override
- "[Stronger effect] overwrites [weaker effect]!"
- "The more powerful version takes precedence."

EFFECT INTERACTION: Opposing Effects
- "[Effect A] and [Effect B] clash! Both are reduced!"
- "Fire meets ice! Both effects are partially neutralized!"

EFFECT INTERACTION: Synergy
- "[Effect A] combines with [Effect B]! Enhanced result!"
- "The effects stack—this is getting dangerous!"

```

---

## SEVERITY SCALING

### Severity Levels

| Severity | Damage/Penalty | Duration | Removal Difficulty |
| --- | --- | --- | --- |
| Minor | 1d4 / -1 | 2 rounds | Easy (DC 8) |
| Moderate | 1d6 / -2 | 3 rounds | Standard (DC 12) |
| Severe | 2d6 / -3 | 5 rounds | Hard (DC 16) |
| Critical | 3d6 / -4 | 10 rounds | Very Hard (DC 20) |

### Severity Templates

```
SEVERITY: Minor
- "A mild [effect]. Uncomfortable but manageable."
- "[Effect] is present but not severe. [-1 penalty / 1d4 damage]"

SEVERITY: Moderate
- "[Effect] is significant. This needs attention."
- "The [effect] hampers your effectiveness. [-2 penalty / 1d6 damage]"

SEVERITY: Severe
- "[Effect] is SERIOUS. You're in trouble."
- "This [effect] is debilitating! [-3 penalty / 2d6 damage]"

SEVERITY: Critical
- "[Effect] is CRITICAL. Life-threatening!"
- "Extreme [effect]! Immediate attention required! [-4 penalty / 3d6 damage]"

```

---

## WRITING GUIDELINES

### Status Effect Principles

1. **Clear mechanical communication** - reader understands the game effect
2. **Narrative immersion** - description feels in-world
3. **Escalation language** - severity reflected in word choice
4. **Source acknowledgment** - what caused this is clear
5. **Duration awareness** - temporal aspect communicated

### Word Choices by Severity

| Severity | Verbs | Adjectives |
| --- | --- | --- |
| Minor | affects, touches, bothers | mild, slight, minor |
| Moderate | grips, hampers, weakens | significant, notable, clear |
| Severe | ravages, cripples, overwhelms | serious, severe, dangerous |
| Critical | devastates, annihilates, destroys | critical, extreme, lethal |

### Avoid

- Vague descriptions ("you feel bad")
- Missing mechanical information
- Inconsistent severity language
- Breaking immersion with pure mechanics
- Ignoring the source of the effect

---

## Quality Checklist

- [ ]  Application text is clear and dramatic
- [ ]  Active text describes ongoing state
- [ ]  Tick text varies (not repetitive)
- [ ]  Expiration/removal both covered
- [ ]  Mechanical effects in [brackets]
- [ ]  Severity reflected in language
- [ ]  Source acknowledged
- [ ]  Duration communicated
- [ ]  Interactions considered