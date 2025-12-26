# Biome Descriptor Template

Parent item: Rune and Rust: Flavor Text Template Library (Rune%20and%20Rust%20Flavor%20Text%20Template%20Library%202ba55eb312da808ea34ef7299503f783.md)

This template provides guidance for creating rich, atmospheric flavor text for each of Rune and Rust's five biomes. Use this to maintain consistent tone while adding variety.

---

## Biome Overview

| Biome | Core Theme | Dominant Emotion | Color Palette |
| --- | --- | --- | --- |
| **The Roots** | Industrial decay, 800 years of rust | Oppressive dread, abandonment | Rust-orange, verdigris, sickly yellow |
| **Muspelheim** | Volcanic fury, primordial fire | Primal fear, awe | Molten orange, char-black, ember-red |
| **Niflheim** | Eternal frost, desolate cold | Isolation, creeping despair | Ice-blue, bone-white, grey-violet |
| **Alfheim** | Reality distortion, paradox | Disorientation, madness | Iridescent, shifting hues, impossible colors |
| **Jotunheim** | Giant-scale architecture | Insignificance, wonder | Stone-grey, iron-black, ancient bronze |

---

## Template Structure

Each biome descriptor should follow this structure:

```
[BIOME]: [CATEGORY]
Subcategory: [Specific type]
Intensity: [Subtle | Moderate | Oppressive]
Location Context: [Optional - where this occurs]
Weight: [0.5-2.0 - frequency weighting]
Tags: ["Tag1", "Tag2", "Tag3"]

DESCRIPTOR TEXT:
[Your flavor text here - 1-3 sentences, evocative and specific]

```

---

## THE ROOTS

### Design Principles

- **800 years of neglect**: Everything is corroded, failing, or barely functional
- **Dvergr engineering**: Once-magnificent infrastructure now in ruin
- **Living decay**: The environment feels alive in its deterioration
- **Servitor presence**: Corrupted maintenance machines still patrol

### Architectural Features

### Template: Corridors

```
THE_ROOTS: ARCHITECTURE
Subcategory: Corridor
Intensity: Moderate
Tags: ["Structural", "Decay", "Navigation"]

TEMPLATE:
"The corridor stretches ahead, {CONDITION_ADJECTIVE} walls bearing the {DECAY_MARKER} of {TIME_REFERENCE}. {ENVIRONMENTAL_DETAIL}. {OPTIONAL_HAZARD_HINT}."

```

**Variables:**

- `{CONDITION_ADJECTIVE}`: rust-streaked, corroded, pitted, buckling, sagging
- `{DECAY_MARKER}`: scars, weight, evidence, testament, memory
- `{TIME_REFERENCE}`: centuries of neglect, abandoned ages, the long silence, the machine-death
- `{ENVIRONMENTAL_DETAIL}`: Pipes leak a steady drip | Cables hang like dead vines | The floor grating has given way in patches
- `{OPTIONAL_HAZARD_HINT}`: The ceiling groans overhead | Something moves in the shadows ahead | The lights flicker uncertainly

**Example Outputs:**

1. "The corridor stretches ahead, rust-streaked walls bearing the weight of centuries of neglect. Pipes leak a steady drip that echoes off the metal. The ceiling groans overhead."
2. "The corridor stretches ahead, buckling walls bearing the scars of the long silence. Cables hang like dead vines. Something moves in the shadows ahead."

### Template: Chambers

```
THE_ROOTS: ARCHITECTURE
Subcategory: Chamber
Intensity: Moderate to Oppressive
Tags: ["Structural", "Scale", "Atmosphere"]

TEMPLATE:
"You enter a {SCALE_DESCRIPTOR} chamber. {ORIGINAL_PURPOSE_HINT}. Now, {CURRENT_STATE}. {ATMOSPHERIC_DETAIL}."

```

**Variables:**

- `{SCALE_DESCRIPTOR}`: vast, cavernous, cramped, circular, multi-level
- `{ORIGINAL_PURPOSE_HINT}`: Once a control center | This was a maintenance hub | Assembly lines once filled this space | Generators lined these walls
- `{CURRENT_STATE}`: rust claims everything | only shadows remain | machines stand silent and dead | debris chokes the floor
- `{ATMOSPHERIC_DETAIL}`: The air tastes of iron and time | Your footsteps echo endlessly | The darkness seems to breathe

**Example Outputs:**

1. "You enter a vast chamber. Once a control center, banks of consoles lined the walls. Now, rust claims everything. The air tastes of iron and time."
2. "You enter a cramped chamber. This was a maintenance hub where Servitors were repaired. Now, only shadows remain. Your footsteps echo endlessly."

### Lighting Conditions

```
THE_ROOTS: LIGHTING
Subcategory: [Functional | Failing | Dark | Emergency]
Intensity: [Subtle | Moderate | Oppressive]
Tags: ["Visual", "Atmosphere", "Mood"]

EXAMPLES:
- Functional: "Flickering lume-strips cast uncertain light, shadows dancing with each power fluctuation."
- Failing: "Half the lights are dead. The rest buzz and flicker, painting the corridor in strobing illumination."
- Dark: "Darkness absolute. Your light cuts a narrow swath through eight centuries of shadow."
- Emergency: "Red emergency lighting bathes everything in crimson—a bloodstain upon the world."

```

### Environmental Details

```
THE_ROOTS: ENVIRONMENTAL_DETAIL
Subcategory: [Water | Rust | Debris | Biological | Mechanical]
Intensity: Variable
Tags: ["Detail", "Sensory", "Immersion"]

WATER:
- "Water has pooled here, black and still, reflecting nothing."
- "Condensation weeps from the pipes, rust-stained tears."
- "A thin film of oily water covers the floor, rainbow sheen on darkness."

RUST:
- "Rust blooms across every surface like a disease."
- "The walls are more rust than metal now—centuries of oxidation made solid."
- "Flakes of rust rain down with each footstep, orange snow in dim light."

DEBRIS:
- "Collapsed machinery blocks half the passage."
- "Rubble crunches underfoot—the remnants of a ceiling that finally gave up."
- "Overturned containers spill their corroded contents across the floor."

BIOLOGICAL:
- "Fungal growth spreads across the walls in pale, fleshy patches."
- "Something has nested here—scraps of cloth and bone form crude shelters."
- "The smell of decay is stronger here. Something died nearby, and not recently."

MECHANICAL:
- "A Servitor lies crumpled against the wall, eye-lights dark but still somehow watching."
- "Gears and cogs litter the ground—the viscera of dead machines."
- "Control panels spark intermittently, systems trying to report failures to masters long dead."

```

---

## MUSPELHEIM

### Design Principles

- **Living fire**: The realm itself burns with primordial fury
- **Thermal extremes**: Heat that warps metal and blisters skin
- **Volcanic activity**: Rivers of magma, geysers of flame
- **Survival pressure**: Every moment tests endurance

### Terrain Features

```
MUSPELHEIM: TERRAIN
Subcategory: [Lava | Obsidian | Basalt | Ash]
Intensity: Moderate to Oppressive
Tags: ["Terrain", "Hazard", "Navigation"]

LAVA:
- "Molten rock flows sluggishly through channels carved into the floor, radiating lethal heat."
- "A river of fire cuts through the chamber, its surface crackling with cooling stone that's immediately remelted."
- "Lava pools in the depression, bubbling and popping, each burst sending sparks into the air."

OBSIDIAN:
- "Obsidian formations jut from the ground like frozen black flames."
- "The walls are volcanic glass, reflecting your light in sharp, fractured planes."
- "Razor-edged obsidian lines the passage—one wrong step means lacerated flesh."

BASALT:
- "Hexagonal basalt columns rise like the pillars of some giant's temple."
- "The basalt floor is cracked and buckled from thermal stress."
- "Ancient lava flows have solidified into twisted basalt sculptures of frozen motion."

ASH:
- "Ash drifts ankle-deep, soft and treacherous, hiding the terrain beneath."
- "Each step raises clouds of grey ash that coat your lungs and sting your eyes."
- "The air is thick with falling ash—volcanic snow that never ends."

```

### Heat Effects

```
MUSPELHEIM: HEAT_EFFECTS
Subcategory: [Ambient | Intense | Lethal]
Intensity: Variable
Tags: ["Environmental", "Damage", "Survival"]

AMBIENT:
- "The heat is constant—a pressure against your skin that never relents."
- "Sweat evaporates before it can form. You feel yourself drying out."
- "The air shimmers with heat haze, making distances impossible to judge."

INTENSE:
- "Metal surfaces are hot enough to burn on contact."
- "The heat steals your breath, each inhalation like breathing fire."
- "Your equipment grows uncomfortably warm. Much longer and it will be too hot to touch."

LETHAL:
- "The heat here is killing. Exposed skin blisters within moments."
- "The air itself seems to burn. You cannot stay here long."
- "This close to the lava, the temperature is beyond survivable—every second counts."

```

---

## NIFLHEIM

### Design Principles

- **Absolute cold**: Temperature that kills slowly and surely
- **Frozen time**: Everything preserved in eternal ice
- **Isolation**: Vast, empty expanses of white
- **Mist and fog**: Limited visibility, hidden dangers

### Ice Formations

```
NIFLHEIM: ICE_FORMATIONS
Subcategory: [Glacier | Icicle | Frozen_Structure | Permafrost]
Intensity: Variable
Tags: ["Terrain", "Visual", "Atmosphere"]

GLACIER:
- "The glacier groans beneath you—ancient ice under unimaginable pressure."
- "Blue-white ice stretches to the horizon, a frozen sea that never thaws."
- "Deep within the glacier, you see shapes—frozen shadows of things long dead."

ICICLE:
- "Icicles hang like crystal fangs from every overhang."
- "Massive ice formations descend from the ceiling, some thick as tree trunks."
- "The icicles chime softly as air currents disturb them—a frozen wind chime."

FROZEN_STRUCTURE:
- "A building lies encased in ice, preserved perfectly for eternity."
- "The doorway is frozen shut—inches of ice seal it closed."
- "Ice has invaded the structure, crawling across walls and floors like a living thing."

PERMAFROST:
- "The ground is frozen solid—harder than stone and twice as cold."
- "Permafrost has locked this place in an unchanging grip for millennia."
- "The frozen earth cracks beneath your weight, deep booming sounds that echo across the ice."

```

### Cold Effects

```
NIFLHEIM: COLD_EFFECTS
Subcategory: [Bitter | Numbing | Lethal]
Intensity: Variable
Tags: ["Environmental", "Damage", "Survival"]

BITTER:
- "The cold bites at exposed skin, a constant gnawing discomfort."
- "Your breath crystallizes instantly, each exhalation a small cloud of ice."
- "The cold is inescapable—it seeps through every layer, every gap."

NUMBING:
- "Your fingers are going numb. Fine manipulation becomes difficult."
- "The cold has reached your core. Movement becomes sluggish."
- "Thought itself seems to slow in this cold—your mind struggling against the freeze."

LETHAL:
- "Frostbite is setting in. Exposed flesh is turning white."
- "The cold is no longer uncomfortable—it's deadly. You feel yourself shutting down."
- "At these temperatures, death comes quietly—a slow drift into frozen sleep."

```

---

## ALFHEIM

### Design Principles

- **Reality breakdown**: The laws of physics are suggestions here
- **Paradox and impossibility**: Things that cannot be, are
- **The Cursed Choir**: Ever-present maddening harmonics
- **Temporal distortion**: Time flows strangely

### Reality Distortions

```
ALFHEIM: REALITY_DISTORTION
Subcategory: [Spatial | Temporal | Sensory | Paradox]
Intensity: Oppressive
Tags: ["Glitch", "Madness", "Impossible"]

SPATIAL:
- "The corridor ahead is longer than the building that contains it."
- "You turn a corner and find yourself back where you started—but the room has changed."
- "Up and down lose meaning. You walk on what should be the ceiling, and it feels natural."

TEMPORAL:
- "You see your own footprints ahead of you—you haven't been there yet."
- "Time stutters. You experience the same moment twice, slightly different each time."
- "The dust you disturbed settles upward, returning to where it was."

SENSORY:
- "Colors here have sounds. The blue of the walls hums with a low drone."
- "You smell the approach of something before you see it—a scent like burning mathematics."
- "Your shadow moves independently, gesturing at things you cannot see."

PARADOX:
- "A door stands open and closed simultaneously. You see both states layered upon each other."
- "The corpse speaks to you, has been dead for centuries, and was never alive—all true at once."
- "Water flows upward into a drain that is also a fountain that is also a wound in reality."

```

### The Cursed Choir

```
ALFHEIM: CURSED_CHOIR
Subcategory: [Distant | Present | Overwhelming]
Intensity: Oppressive
Tags: ["Glitch", "Sound", "Madness", "Persistent"]

DISTANT:
- "The Cursed Choir is faint here—a distant harmony that scratches at the edge of hearing."
- "You can almost ignore the singing. Almost. It's always there, under everything."
- "The Choir whispers from somewhere far away, words in languages that never existed."

PRESENT:
- "The Cursed Choir fills this space—harmonies that hurt, melodies that madden."
- "You cannot escape the singing. It resonates in your skull, in your teeth, in your soul."
- "The Choir is louder here. Their impossible song makes your vision blur."

OVERWHELMING:
- "THE CHOIR IS ALL YOU CAN HEAR. It drowns thought, drowns reason, drowns you."
- "The singing is inside you now—you cannot tell where it ends and your thoughts begin."
- "Every harmony cuts. Every note bleeds. The Cursed Choir is trying to unmake you."

```

---

## JOTUNHEIM

### Design Principles

- **Giant scale**: Everything built for beings far larger than you
- **Ancient engineering**: Dvergr work at its most ambitious
- **Emptiness**: Vast spaces, long abandoned
- **Insignificance**: The architecture diminishes mortals

### Scale and Architecture

```
JOTUNHEIM: ARCHITECTURE
Subcategory: [Doorway | Chamber | Stairway | Corridor]
Intensity: Moderate to Oppressive
Tags: ["Scale", "Architecture", "Atmosphere"]

DOORWAY:
- "The doorway is sized for giants—thirty feet tall, the door itself thick as castle walls."
- "You could march a column of soldiers through this entrance and still have room."
- "The door mechanism is visible but unreachable—meant for hands the size of boulders."

CHAMBER:
- "The chamber's ceiling is lost in darkness high above. Sounds take seconds to echo back."
- "This hall could hold armies. Instead, it holds only silence and shadow."
- "The scale is crushing. You are an insect in a titan's throne room."

STAIRWAY:
- "Each step is waist-high—stairs for giant feet that require climbing."
- "The stairway ascends into shadow, its upper reaches invisible from below."
- "You scramble up steps meant for beings ten times your size."

CORRIDOR:
- "The corridor is wide enough for three giants to walk abreast."
- "Your footsteps are insignificant sounds in this massive passage."
- "Walking here feels like traversing a canyon—walls of worked stone rising on either side."

```

### Remnants of Giants

```
JOTUNHEIM: REMNANTS
Subcategory: [Artifacts | Bodies | Machinery | Writing]
Intensity: Variable
Tags: ["Lore", "Discovery", "Atmosphere"]

ARTIFACTS:
- "A cup lies overturned—it would serve as a bathtub for you."
- "Tools of enormous size lean against the wall, their purpose unclear."
- "A chain lies coiled on the floor, each link the size of your torso."

BODIES:
- "Giant bones rest against the wall—a skeleton larger than a house."
- "The corpse has been here for ages, preserved by the cold. Even in death, it dwarfs you."
- "Skulls the size of small buildings line this passage—a giant's ossuary."

MACHINERY:
- "Gears taller than you stand frozen in place, part of some incomprehensible mechanism."
- "The machine is silent now, but was clearly built for giant operators."
- "Control surfaces the size of tabletops dot the walls—built for massive hands."

WRITING:
- "Runes the size of your entire body are carved into the wall."
- "The inscription would take hours to read—each character requires craning your neck."
- "Giant-script covers every surface, telling stories you can only glimpse."

```

---

## Cross-Biome Elements

Some elements appear across multiple biomes with thematic variations:

### Doorways

```
[BIOME]: DOORWAY
Structure: "The {DOOR_TYPE} {DOOR_STATE}. {DOOR_DETAIL}. {PASSAGE_HINT}."

THE_ROOTS: "The blast door stands half-open, mechanism long failed. Rust has welded it in place. Beyond lies deeper darkness."
MUSPELHEIM: "The stone door is cracked from thermal stress. Heat bleeds through the gap. Something burns on the other side."
NIFLHEIM: "The doorway is frozen shut, inches of ice sealing it closed. Cold radiates from beyond. This leads somewhere colder still."
ALFHEIM: "The door is open and closed. Both states exist simultaneously. Choosing which to believe may determine what lies beyond."
JOTUNHEIM: "The giant's door would take a team to move. A smaller passage has been cut through its base—for lesser beings."

```

### Lighting Transitions

```
[BIOME]: LIGHTING_TRANSITION
Structure: "The light {TRANSITION_VERB} as you {MOVEMENT}. {NEW_LIGHT_STATE}."

THE_ROOTS: "The light dies as you move forward. The next section has no working lume-strips. Darkness absolute."
MUSPELHEIM: "The light intensifies as you approach. Magma flow ahead provides illumination—and danger."
NIFLHEIM: "The light grows cold and blue as you descend. Ice refracts what little illumination exists into ghostly patterns."
ALFHEIM: "The light bends as you cross the threshold. Colors shift. What was bright becomes dark, what was dark becomes blindingly bright."
JOTUNHEIM: "The light seems insignificant as you enter the great hall. Your torch illuminates nothing but the nearest surfaces of an infinite space."

```

---

## Writing Guidelines

### Tone Consistency

- **The Roots**: Industrial horror, melancholy, oppressive
- **Muspelheim**: Primal danger, overwhelming power, survival urgency
- **Niflheim**: Isolation, despair, creeping death
- **Alfheim**: Cosmic horror, madness, unreality
- **Jotunheim**: Awe, insignificance, ancient mystery

### Sensory Engagement

Always engage at least two senses:

- Visual + Auditory: "Rust-red walls echo with dripping water."
- Auditory + Tactile: "The grinding machinery vibrates through the floor."
- Visual + Olfactory: "Black mold spreads across the walls, its decay thick in your lungs."

### Active Voice

Prefer active, immediate descriptions:

- **Good**: "Water drips from corroded pipes."
- **Avoid**: "The corroded pipes have water dripping from them."

### Second Person Sparingly

Use "you" to draw the player in during moments of impact:

- "You enter the chamber and the scale hits you—this place was not built for mortals."
- "The heat washes over you, stealing your breath."

### Word Count Guidelines

- **Brief descriptors**: 15-25 words
- **Standard descriptors**: 25-40 words
- **Elaborate descriptors**: 40-60 words (use sparingly)

---

## Metadata Requirements

Every descriptor should include:

1. **Biome**: Which biome this belongs to
2. **Category**: Major category (Architecture, Terrain, Lighting, etc.)
3. **Subcategory**: Specific type within category
4. **Intensity**: Subtle, Moderate, or Oppressive
5. **Tags**: Array of searchable tags
6. **Weight**: 0.5-2.0 (affects selection frequency)
7. **Location Context** (optional): Specific location type

---

## Quality Checklist

Before finalizing a descriptor:

- [ ]  Does it evoke at least two senses?
- [ ]  Is it specific to the biome's theme?
- [ ]  Does it maintain consistent tone?
- [ ]  Is it the appropriate length?
- [ ]  Could it work in multiple contexts?
- [ ]  Does it avoid clichés and generic descriptions?
- [ ]  Does it hint at the world's history/lore when appropriate?
- [ ]  Are all variable slots clearly defined?