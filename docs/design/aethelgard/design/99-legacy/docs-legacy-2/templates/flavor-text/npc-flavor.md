# NPC Flavor Text Template

This template guides the creation of flavor text for non-player characters—their dialogue, reactions, descriptions, and combat barks. NPCs bring the world of Rune and Rust to life.

---

## Overview

| Category | Purpose | Usage |
|----------|---------|-------|
| **Descriptions** | Physical/personality intro | First encounter |
| **Reactions** | Emotional responses | Events/triggers |
| **Dialogue** | Conversations | Direct interaction |
| **Combat Barks** | Battle commentary | Combat actions |
| **Ambient Barks** | Idle comments | Background activity |

---

## Template Structure

```
NPC FLAVOR: [Description | Reaction | Dialogue | CombatBark | AmbientBark]
Archetype: [Dvergr | Seidkona | Bandit | Raider | Merchant | Guard | Citizen | Forlorn]
Disposition: [Hostile | Unfriendly | Neutral | Friendly | Allied]
Context: [Specific situation]
Emotion: [Current emotional state]
Tags: ["Tag1", "Tag2"]

TEXT: [The actual flavor content]
ACTION: [Physical actions accompanying speech]
TENDENCY: [Likely next action: Approach | Flee | Attack | Assist | Ignore]
```

---

## NPC ARCHETYPES

### Archetype Registry

| Archetype | Role | Speech Pattern | Typical Disposition |
|-----------|------|----------------|---------------------|
| **Dvergr** | Engineers, workers | Technical, practical | Neutral-Friendly |
| **Seidkona** | Mystics, seers | Cryptic, formal | Neutral |
| **Bandit** | Survivors, thieves | Crude, threatening | Hostile-Unfriendly |
| **Raider** | Organized criminals | Tactical, proud | Hostile |
| **Merchant** | Traders | Mercenary, shrewd | Neutral |
| **Guard** | Protectors | Authoritative, wary | Neutral-Unfriendly |
| **Citizen** | Common folk | Varied, emotional | Friendly-Neutral |
| **Forlorn** | Undead | Fragments, whispers | Hostile |

### Archetype Voice Patterns

#### Dvergr Voice
```
VOICE PATTERN: Dvergr
Characteristics: Technical vocabulary, practical mindset, clan pride
Speech Patterns: References to engineering, ancestors, the old ways

EXAMPLES:
- "The seals on this section haven't been calibrated in... by the First Forge, centuries."
- "Efficient work. Your clan would be proud. Mine certainly would be."
- "This isn't malfunction—it's entropy. The Roots are dying, seal by seal."
- "I could repair this, given time and proper tools. We have neither."
```

#### Seidkona Voice
```
VOICE PATTERN: Seidkona
Characteristics: Mystical, seeing-beyond, speaking in riddles
Speech Patterns: References to fate, the runes, what they've seen

EXAMPLES:
- "The runes speak of you. They have been... anticipating."
- "I see threads connecting you to this place. Cut some. Follow others."
- "What you seek lies where three paths become one. You'll understand when you must."
- "The spirits are restless today. Something approaches. Something hungry."
```

#### Bandit Voice
```
VOICE PATTERN: Bandit
Characteristics: Desperate, threatening, survival-focused
Speech Patterns: Crude threats, survival justifications, opportunism

EXAMPLES:
- "Drop your gear and walk away. Or don't walk away at all."
- "Nothing personal, friend. Just business. The desperate kind."
- "We've been watching you. Nice equipment. Too nice for you."
- "Don't make this harder than it has to be."
```

#### Merchant Voice
```
VOICE PATTERN: Merchant
Characteristics: Shrewd, calculating, profit-motivated
Speech Patterns: Value assessments, deals offered, pragmatic philosophy

EXAMPLES:
- "In these times, everything has value. Everything has a price. Let's discuss yours."
- "Quality goods, fair prices, no questions asked. That's the deal."
- "I could let this go for... hmm, let me reassess your ability to pay."
- "Survival is the economy now. I deal in survival."
```

---

## NPC DESCRIPTIONS

### Physical Description Templates

#### Dvergr Description
```
NPC DESCRIPTION: Dvergr
Pattern: "[BUILD] Dvergr with [DISTINCTIVE_FEATURE]. Their [EQUIPMENT/CLOTHING] suggests [ROLE/STATUS]. [PERSONALITY_HINT]."

Variables:
- BUILD: stocky, weathered, scarred, aged, young, broad-shouldered
- DISTINCTIVE_FEATURE: clan markings, mechanical prosthetic, runic tattoos
- EQUIPMENT: engineer's tools, ceremonial garb, warrior's armor
- ROLE/STATUS: craftsperson, clan elder, outcast, warrior
- PERSONALITY_HINT: observational detail about manner

Examples:
- "A weathered Dvergr with faded clan markings on their cheeks. Their tool belt is well-maintained despite the rust elsewhere—a professional. They assess you with the calculating gaze of someone who measures everything."

- "A young Dvergr, mechanical arm glinting dully in the low light. The prosthetic clicks and whirs with each movement. They seem eager—perhaps desperate—to prove themselves."
```

#### Seidkona Description
```
NPC DESCRIPTION: Seidkona
Pattern: "[BEARING] figure draped in [GARMENTS]. Their eyes [EYE_QUALITY]. [MYSTICAL_ELEMENT]. [IMPRESSION]."

Variables:
- BEARING: ethereal, hunched, commanding, serene, unsettling
- GARMENTS: rune-sewn robes, tattered mystic garb, bone-adorned cloak
- EYE_QUALITY: seem to look through you, are milky with second-sight, dart constantly
- MYSTICAL_ELEMENT: runes glow on skin, spirits whisper, the air feels charged
- IMPRESSION: overall feeling they evoke

Examples:
- "A serene figure draped in rune-sewn robes that seem to move without wind. Their eyes—milky, distant—look at something beyond you. When they speak, you feel your ancestors listening."

- "Hunched beneath a bone-adorned cloak, the Seidkona's eyes dart constantly to things you cannot see. The air around them feels thick, charged. They know things. Things that might be better unknown."
```

#### Hostile NPC Description
```
NPC DESCRIPTION: Hostile
Pattern: "[THREAT_POSTURE]. [WEAPON/EQUIPMENT] [WEAPON_STATE]. [GROUP_AFFILIATION]. [DANGER_ASSESSMENT]."

Examples:
- "They block the corridor, weapons drawn and ready. Six of them—bandits by the look of their mismatched gear. The leader smiles without humor. They've done this before."

- "The raider's eyes are cold above the face-cloth. Professional. Organized. The weapon in their hands is well-maintained and familiar to them. This is someone who kills for a living."
```

---

## NPC REACTIONS

### Reaction Triggers

| Trigger | Description |
|---------|-------------|
| PlayerApproaches | Player enters NPC awareness |
| PlayerAttacks | Combat initiated by player |
| PlayerHelps | Player provides aid |
| PlayerSteals | Caught taking from NPC/allies |
| AllyKilled | NPC's ally dies |
| TakingDamage | NPC is wounded |
| VictoryAchieved | Combat won |
| TreasureFound | Valuable discovery |
| SecretRevealed | Hidden truth uncovered |
| BetrayalDetected | Trust broken |

### Reaction Templates

#### Positive Reactions
```
NPC REACTION: Positive
Trigger: PlayerHelps
Archetype: Citizen

GRATEFUL:
- Text: "You... you helped us. I didn't think anyone would."
- Action: *tears forming, hands clasped*
- Tendency: Assist

IMPRESSED:
- Text: "Well fought! There's a warrior's heart in you."
- Action: *nods approvingly, tension easing*
- Tendency: Approach

RELIEVED:
- Text: "Thank the ancestors. I thought I was dead."
- Action: *slumps against wall, breathing hard*
- Tendency: Assist
```

#### Negative Reactions
```
NPC REACTION: Negative
Trigger: BetrayalDetected
Archetype: Merchant

ANGRY:
- Text: "You think you can cheat ME? I've dealt with worse than you!"
- Action: *face contorts with fury, hand moving to hidden weapon*
- Tendency: Attack

FEARFUL:
- Text: "Please... I have family. Take what you want, just—"
- Action: *backing away, hands raised defensively*
- Tendency: Flee

DISGUSTED:
- Text: "I gave you fair trade. And this is how you repay it."
- Action: *spits on ground, reaching for weapon*
- Tendency: Attack
```

#### Combat Reactions
```
NPC REACTION: Combat
Trigger: TakingDamage

WARRIOR TYPE:
- Mild: "That one got through. It won't happen again."
- Moderate: "You'll pay for that in blood!"
- Severe: "I'm... not done yet!"

CIVILIAN TYPE:
- Mild: "Ah! That—that hurt!"
- Moderate: "Please, stop! I yield!"
- Severe: *wordless whimper, trying to crawl away*

FORLORN TYPE:
- Any damage: "Pain... I remember pain..." *continues fighting*
```

---

## NPC DIALOGUE

### Conversation Categories

| Category | Purpose | Example Situations |
|----------|---------|-------------------|
| Greeting | Initial contact | First meeting |
| Information | Sharing knowledge | Lore, directions, warnings |
| Quest | Mission-related | Quest give/update/complete |
| Trade | Commerce | Buying/selling |
| Conflict | Confrontation | Arguments, threats |
| Farewell | Ending conversation | Parting words |

### Dialogue Templates

#### Greeting Dialogue
```
NPC DIALOGUE: Greeting
Disposition: [Varies]

FRIENDLY:
- "Ah, travelers! It's been too long since we've seen friendly faces."
- "Welcome, welcome! You look like you could use a rest."
- "The ancestors smile on this meeting. What brings you to these depths?"

NEUTRAL:
- "State your business. These are dangerous times."
- "Hmm. New faces. What do you want?"
- "I don't know you. Should I?"

UNFRIENDLY:
- "You're not welcome here. Say what you need to say and move on."
- "Another scavenger come to pick the bones. What do you want?"
- "Keep your hands where I can see them. What's your business?"

HOSTILE:
- "Bad luck for you, wandering into my territory."
- "You've made a mistake coming here. Your last one."
- "Look what walked right into our trap."
```

#### Information Dialogue
```
NPC DIALOGUE: Information
Type: Lore/Warning/Direction

LORE:
- "The old stories say this place was a power hub. Before the Silence, thousands worked here."
- "Servitors used to maintain everything. Now they... maintain other things."
- "My grandfather remembered when the lights never flickered. Can you imagine?"

WARNING:
- "Don't go that way. Something lives in those corridors now. Something wrong."
- "The lower levels are flooded with toxic runoff. Death in a breath."
- "Raiders have been hitting caravans on the main route. Find another path."

DIRECTION:
- "The safe house? Two levels down, past the old assembly line. Look for the blue door."
- "Follow the functioning lume-strips. Where there's light, there's usually someone maintaining it."
- "The marketplace is east. Just follow the noise. And keep your hand on your coin."
```

#### Trade Dialogue
```
NPC DIALOGUE: Trade
Context: Commerce interactions

OPENING:
- "Looking to trade? Let's see what you've got."
- "Currency or barter, I deal in both. What do you need?"
- "Quality goods at fair prices. Inspect anything you like."

HAGGLING:
- "That's my price. Take it or leave it."
- "Hmm, you drive a hard bargain. Fine, [new price]."
- "I could go lower, but then I'd be cheating myself. [Final offer]."

SUCCESSFUL TRADE:
- "Pleasure doing business. Come back when you need more."
- "A fair deal for both of us. May it serve you well."

NO DEAL:
- "Can't help you if you can't pay. Come back when you can."
- "A shame we couldn't agree. Perhaps next time."
```

---

## COMBAT BARKS

### Combat Bark Categories

| Category | When Used |
|----------|-----------|
| Battle Cry | Combat initiation |
| Attack | During attack actions |
| Hit Taken | Receiving damage |
| Kill | Defeating an enemy |
| Ally Down | Companion falls |
| Fleeing | Retreating |
| Surrender | Giving up |
| Dying | Final words |

### Combat Bark Templates

#### Battle Cries
```
COMBAT BARK: Battle Cry
Trigger: Combat initiation

WARRIOR TYPES:
- "FOR BLOOD AND GLORY!"
- "YOU PICKED THE WRONG FIGHT!"
- "LET'S SEE WHAT YOU'RE MADE OF!"

BANDIT TYPES:
- "GET THEM! TAKE EVERYTHING!"
- "No witnesses!"
- "Should've kept walking, fool!"

GUARD TYPES:
- "Halt! You're under arrest!"
- "Weapons down or we open fire!"
- "This is your only warning!"
```

#### Attack Barks
```
COMBAT BARK: Attack
Trigger: Making attack

AGGRESSIVE:
- "EAT THIS!"
- "HERE IT COMES!"
- "DIE!"

TACTICAL:
- "Flanking left!"
- "Cover me!"
- "Target the weak one!"

DEFENSIVE:
- "Stay back!"
- "You won't take us!"
- "I'll protect them!"
```

#### Damage Barks
```
COMBAT BARK: Hit Taken
Trigger: Receiving damage

MINOR:
- "Tch! Scratch!"
- "That all you got?"
- "Lucky hit!"

MODERATE:
- "Gah! That one hurt!"
- "You'll pay for that!"
- "I'm bleeding!"

SEVERE:
- "I can't... keep going..."
- "Ancestors... give me strength..."
- "Need... help!"
```

#### Fleeing Barks
```
COMBAT BARK: Fleeing
Trigger: Morale breaks

- "This isn't worth dying for!"
- "RETREAT! FALL BACK!"
- "You're crazy! I'm out!"
- "The pay isn't worth this!"
```

#### Dying Barks
```
COMBAT BARK: Dying
Trigger: NPC death

WARRIOR:
- "A... good death..."
- "I die... with honor..."
- "Tell my clan... I fought well..."

BANDIT:
- "Damn... you..."
- "Should've... stayed in the depths..."
- *wordless final breath*

FORLORN:
- "Free... at last..."
- "Rest... finally... rest..."
- *sighs with something like relief*
```

---

## AMBIENT BARKS

### Ambient Bark Categories

| Category | Purpose |
|----------|---------|
| Idle | Background commentary |
| Environmental | Reacting to surroundings |
| Social | Interacting with other NPCs |
| Alertness | Vigilance state |

### Ambient Bark Templates

#### Idle Barks
```
AMBIENT BARK: Idle
Context: No immediate activity

CITIZEN:
- *muttering* "Wonder if the traders will come this cycle..."
- "Same rust, different day."
- *humming an old tune, slightly off-key*

GUARD:
- "All clear, sector seven."
- *yawning* "Nothing ever happens on this shift."
- "Keep sharp. Could change any moment."

MERCHANT:
- *counting coins* "Three, four, five... need to diversify inventory."
- "The margins are thin, but the margins are everything."
```

#### Environmental Reaction Barks
```
AMBIENT BARK: Environmental
Context: Responding to surroundings

SOUNDS:
- "Did you hear that?" *pauses, listening*
- "Just the pipes. Probably. Hopefully."
- "That sound again. Getting closer."

VISUAL:
- "What's that over there?" *squinting*
- "The lights are dimmer today. Not a good sign."
- "Movement. No, just shadows. I think."

DANGER:
- "We shouldn't stay here long."
- "This area feels wrong. Stay alert."
- "Something died here. Recently."
```

#### Social Barks
```
AMBIENT BARK: Social
Context: NPC-to-NPC interaction

BETWEEN GUARDS:
- Guard 1: "Quiet night."
  Guard 2: "That's what worries me."

- Guard 1: "Shift change in an hour."
  Guard 2: "Not soon enough."

BETWEEN CITIZENS:
- Citizen 1: "Heard the market's running low on meds."
  Citizen 2: "What isn't running low these days?"

- Citizen 1: "My kid found a working datapad."
  Citizen 2: "Dangerous, showing tech like that around."
```

---

## DISPOSITION MODIFIERS

### Disposition Effects

| Disposition | Greeting | Information | Prices | Combat Likelihood |
|-------------|----------|-------------|--------|-------------------|
| Allied | Warm, eager | Full, free | Best rates | Will fight for you |
| Friendly | Positive | Helpful | Fair rates | Reluctant to fight |
| Neutral | Cautious | Transactional | Standard | Depends on situation |
| Unfriendly | Cold, suspicious | Minimal | Inflated | Easily provoked |
| Hostile | Threatening | None/lies | N/A | Immediate |

### Disposition Templates

```
DISPOSITION MODIFIER: Allied
Base Reaction: Positive across all triggers

GREETING: "Friend! It's good to see you. How can I help?"
INFORMATION: "I'll tell you everything I know. You've earned that trust."
TRADE: "For you? Special price. You've done right by us."
FAREWELL: "Take care out there. Come back safe."
```

```
DISPOSITION MODIFIER: Hostile
Base Reaction: Negative across all triggers

GREETING: "You. You dare show your face here?"
INFORMATION: "Why would I tell you anything? Get out."
TRADE: N/A - No trade with hostile NPCs
FAREWELL: "If I see you again, I won't be talking."
```

---

## WRITING GUIDELINES

### NPC Voice Principles
1. **Consistent archetype** - each NPC type sounds distinct
2. **Appropriate vocabulary** - Dvergr are technical, Seidkona mystical, etc.
3. **Emotional authenticity** - reactions match the situation
4. **World integration** - dialogue references setting naturally
5. **Useful information** - NPCs provide gameplay-relevant content

### Speech Pattern Rules

| Archetype | Vocabulary | Sentence Structure |
|-----------|------------|-------------------|
| Dvergr | Technical, clan-focused | Complex, precise |
| Seidkona | Mystical, archaic | Indirect, prophetic |
| Bandit | Crude, threatening | Short, aggressive |
| Merchant | Business, value | Transactional |
| Guard | Official, authoritative | Commands, warnings |
| Citizen | Common, emotional | Natural, varied |

### Avoid
- Modern slang or anachronisms
- Breaking character for exposition
- All NPCs sounding the same
- Excessive or unfunny humor
- Dialogue that doesn't serve gameplay

---

## Quality Checklist

- [ ] Archetype voice is consistent
- [ ] Disposition affects tone appropriately
- [ ] Reaction matches trigger
- [ ] Dialogue provides useful information
- [ ] Combat barks are distinct and memorable
- [ ] Physical actions accompany speech where appropriate
- [ ] World-building is integrated naturally
- [ ] Character feels alive and believable
