---
id: SPEC-DESCRIPTORS-SENSORY
title: "Sensory Descriptors — Sound, Smell, Sight, Touch, Taste"
version: 1.0
status: draft
last-updated: 2025-12-14
related-files:
  - path: "docs/07-environment/descriptors/descriptors-overview.md"
    status: Parent Specification
  - path: "docs/99-legacy/Imported Game Docs/templates/Sensory & Atmospheric Descriptor Template"
    status: Legacy Reference
---

# Sensory Descriptors — Sound, Smell, Sight, Touch, Taste

> *"Trust your senses. When they start lying to you, it's already too late."*

---

## 1. Overview

This specification defines sensory descriptors that create immersive atmospheric experiences through the five senses, biome-specific and context-aware.

### 1.1 Identity Table

| Property | Value |
|----------|-------|
| Spec ID | `SPEC-DESCRIPTORS-SENSORY` |
| Category | Descriptor System |
| Subcategories | Sound, Smell, Sight, Touch, Taste |
| Est. Fragments | 300+ |

### 1.2 Sense Categories

| Sense | Frequency | Purpose |
|-------|-----------|---------|
| **Sound** | High | Audio atmosphere, warnings |
| **Smell** | Medium | Olfactory immersion, danger cues |
| **Sight** | High | Visual details, lighting, particles |
| **Touch** | Medium | Temperature, texture, moisture |
| **Taste** | Low | Extreme conditions, air quality |

### 1.3 Intensity Levels

| Level | Description | Frequency |
|-------|-------------|-----------|
| **Subtle** | Background, noticed if paying attention | 60% |
| **Moderate** | Clearly noticeable, active atmosphere | 30% |
| **Oppressive** | Demands attention, immediate impact | 10% |

---

## 2. Sound Descriptors

### 2.1 Sound Categories by Biome

#### The Roots

| Subcategory | Examples |
|-------------|----------|
| ActiveMachinery | Pumps, generators, hydraulics still functioning |
| DecayingSystems | Structural failure, dripping, rust falling |
| OppressiveSilence | Unnatural quiet, isolation, heartbeat |
| SmallCreatures | Rats, insects, vermin |
| DistantThreats | Forlorn moans, Servitor movement |

#### Muspelheim

| Subcategory | Examples |
|-------------|----------|
| Lava | Bubbling, flowing, erupting |
| ThermalStress | Cracking stone, expanding metal |
| Flames | Crackling, roaring, consuming |
| HeatWind | Screaming heat-wind, pressure changes |
| AmbientRoar | Constant background fury |

#### Niflheim

| Subcategory | Examples |
|-------------|----------|
| IceCreaking | Ice cracking, glaciers shifting |
| WindHowling | Wind across frozen expanses |
| FrozenSilence | Sound frozen, breath crystallizing |
| Shattering | Ice breaking, crystalline collapse |

#### Svartalfheim

| Subcategory | Examples |
|-------------|----------|
| Echoes | Endless returning sounds |
| Clicking | Echo-location, Silent Folk |
| Dripping | Water in darkness |
| AbsoluteQuiet | Silence so deep it hurts |

#### Jötunheim

| Subcategory | Examples |
|-------------|----------|
| EmptySpaces | Echoes in vast chambers |
| AncientMachinery | Giant-scale mechanisms |
| Wind | Air movement in massive spaces |
| Groaning | Structure under impossible stress |

### 2.2 Sound Templates

#### Mechanical Sounds

| Intensity | Descriptor |
|-----------|------------|
| Subtle | "Machinery thrums somewhere in the walls — systems still running after all these years" |
| Subtle | "A rhythmic clank-clank-clank echoes from below. Something still pumps down there" |
| Moderate | "Hydraulics hiss and groan behind the panels, stressed beyond design limits" |
| Moderate | "Gears grind with the complaint of age and neglect" |
| Oppressive | "The machinery screams — metal against metal, bearings failing, systems dying" |

#### Decay Sounds

| Intensity | Descriptor |
|-----------|------------|
| Subtle | "Water drips steadily from corroded pipes. Plink. Plink. Plink" |
| Subtle | "Rust flakes fall from above with a soft, papery whisper" |
| Moderate | "Metal creaks ominously overhead — this structure is failing" |
| Moderate | "A distant crash echoes through the corridors. Something structural just gave up" |
| Oppressive | "The ceiling groans. Rivets pop. The walls are having a conversation about collapse" |

#### Creature Sounds

| Intensity | Descriptor |
|-----------|------------|
| Subtle | "Tiny claws skitter across metal somewhere in the walls. Rats, probably" |
| Subtle | "Something chitinous scrapes in the darkness — insects, or worse" |
| Moderate | "A hollow moan echoes from somewhere deep below. Forlorn" |
| Moderate | "Footsteps that aren't yours. Heavy. Metallic. Getting closer" |
| Oppressive | "Something large shuffles in the darkness ahead. It has noticed you" |
| Oppressive | "A scream cuts through the silence — human once, now something else" |

#### Elemental Sounds

| Intensity | Descriptor |
|-----------|------------|
| Subtle | "Fire crackles in the distance, comfortable and deadly" |
| Moderate | "Wind howls across the frozen expanse like tortured souls seeking release" |
| Moderate | "Ice groans with the patient voice of glaciers — a sound like the end of time" |
| Oppressive | "The storm ROARS. Nothing can be heard above the fury" |
| Oppressive | "Lava bubbles and spits. The mountain is speaking in fire" |

#### Silence

| Intensity | Descriptor |
|-----------|------------|
| Subtle | "The silence is noticeable. Too quiet for a place this size" |
| Moderate | "The silence is oppressive. Even your footsteps seem too loud here" |
| Moderate | "Nothing. Just the sound of your own breathing and the thud of your heartbeat" |
| Oppressive | "Sound itself seems frozen here. Your voice dies the moment it leaves your lips" |
| Oppressive | "The quiet has weight. It presses against your ears, demanding attention" |

---

## 3. Smell Descriptors

### 3.1 Smell Categories by Biome

#### The Roots

| Subcategory | Examples |
|-------------|----------|
| Industrial | Oil, coolant, ozone, lubricant |
| Decay | Rust, rot, mold, stagnant water |
| Chemical | Toxic fumes, leaking chemicals |
| Organic | Death, fungus, vermin |

#### Muspelheim

| Subcategory | Examples |
|-------------|----------|
| Sulfur | Volcanic gases, brimstone |
| Burning | Charred material, smoke, ash |
| Metal | Superheated metal, melting slag |

#### Niflheim

| Subcategory | Examples |
|-------------|----------|
| Clean | Crisp, pure, sterile cold |
| Absence | Nothing, frozen, preserved |
| Ancient | Preserved decay, time capsule |

#### Vanaheim

| Subcategory | Examples |
|-------------|----------|
| Growth | Vegetation, pollen, sap |
| Rot | Decomposition, mulch, fungal |
| Toxic | Poisonous blooms, chemical defense |

### 3.2 Smell Templates

#### Industrial Smells

| Intensity | Descriptor |
|-----------|------------|
| Subtle | "A trace of ozone hangs in the air — electrical activity nearby" |
| Subtle | "The faint tang of machine oil, old but persistent" |
| Moderate | "The sharp smell of ozone fills your lungs — something electrical is active" |
| Moderate | "Machine oil and coolant create a thick, industrial stench" |
| Oppressive | "The acrid reek of burnt wiring warns of fire hazards ahead" |
| Oppressive | "Chemical fumes burn your eyes and throat. You need to move" |

#### Decay Smells

| Intensity | Descriptor |
|-----------|------------|
| Subtle | "The mustiness of age and neglect permeates the space" |
| Subtle | "A hint of mold beneath the dust" |
| Moderate | "The smell of rust and age fills every breath — the scent of entropy itself" |
| Moderate | "Rot permeates this place. Something organic has been decaying for a long time" |
| Oppressive | "The stench of corruption hits you like a wall. You gag reflexively" |
| Oppressive | "Death. Unmistakable. Recent enough to smell, old enough to worry" |

#### Elemental Smells

| Intensity | Descriptor |
|-----------|------------|
| Subtle | "Sulfur traces on the wind — volcanic activity somewhere distant" |
| Moderate | "The air reeks of sulfur and brimstone. Your eyes water" |
| Moderate | "Ash coats everything, including the inside of your mouth. You taste the fire" |
| Oppressive | "The stench of hell itself — sulfur, burning, and something worse" |
| Oppressive | "The cold has frozen smell itself. There is nothing — and that's terrifying" |

#### Absence of Smell

| Intensity | Descriptor |
|-----------|------------|
| Subtle | "The air here is clean. Unusually so" |
| Moderate | "There is no smell here. The cold has frozen everything, including scent" |
| Oppressive | "There is no smell here, and that's somehow worse. Death should have a scent" |

---

## 4. Sight Descriptors

### 4.1 Visual Categories

| Category | Focus |
|----------|-------|
| Lighting | Illumination quality, sources, shadows |
| Particles | Airborne matter, dust, ash, ice |
| Surfaces | Material state, textures, degradation |
| Movement | Things that move, that shouldn't |

### 4.2 Lighting Templates

| Intensity | Descriptor |
|-----------|------------|
| Subtle | "Dim Lume-strips provide just enough light to navigate" |
| Subtle | "Shadows pool in corners, but the path forward is visible" |
| Moderate | "Lume-strips flicker uncertainly. Shadows dance with each power surge" |
| Moderate | "Emergency lighting bathes everything in crimson. The world looks wounded" |
| Moderate | "The only light comes from distant magma flows — a hellish orange glow" |
| Oppressive | "Absolute darkness. Your light source is the only thing between you and blindness" |
| Oppressive | "The light strobes, seizure-fast. Reality stutters in and out" |

### 4.3 Particle Templates

| Intensity | Descriptor |
|-----------|------------|
| Subtle | "Dust motes drift lazily in the still air" |
| Subtle | "Something fine and powdery hangs suspended, barely visible" |
| Moderate | "Rust flakes drift through the stale air like orange snow" |
| Moderate | "Ash swirls in thermal currents, never settling, always moving" |
| Moderate | "Ice crystals hang suspended in the frigid air, catching what little light exists" |
| Oppressive | "The air is thick with particulates. You can barely see ten meters" |
| Oppressive | "Spores cloud the air. Every breath could be your last clear one" |

### 4.4 Surface Templates

| Intensity | Descriptor |
|-----------|------------|
| Subtle | "Signs of age mark every surface — patina rather than ruin" |
| Moderate | "Every surface is corroded. The walls are more rust than metal now" |
| Moderate | "Every surface is slick with condensation. Footing is treacherous here" |
| Moderate | "Every surface is frost-covered. Touch the walls and your skin might stick" |
| Oppressive | "The corruption has eaten through the structure itself. What remains, shouldn't" |
| Oppressive | "Organic growth covers everything — the walls pulse with alien life" |

### 4.5 Movement Templates

| Intensity | Descriptor |
|-----------|------------|
| Subtle | "Shadows shift at the edge of your vision — probably nothing" |
| Moderate | "Something moved in the darkness ahead. Gone before you could focus" |
| Moderate | "Cables twitch and swing, disturbed by something that passed" |
| Oppressive | "Things MOVE in the shadows. Not hiding — watching" |
| Oppressive | "The walls themselves seem to breathe. You're not imagining it" |

---

## 5. Touch Descriptors

### 5.1 Touch Categories

| Category | Focus |
|----------|-------|
| Temperature | Heat, cold, fluctuations |
| Texture | Surface quality, material state |
| Moisture | Humidity, wetness, ice |
| Vibration | Movement, machinery, seismic |

### 5.2 Temperature Templates

| Intensity | Descriptor |
|-----------|------------|
| Subtle | "The air is warmer here. Or maybe your gear is just failing" |
| Subtle | "A slight chill, nothing serious, but noticeable" |
| Moderate | "The heat radiates from every surface. Sweat evaporates before it can form" |
| Moderate | "The cold seeps through your gear, finding every gap, every weakness" |
| Moderate | "Temperature fluctuates wildly — freezing in shadows, burning in light" |
| Oppressive | "The heat is murder. Every breath scorches your lungs" |
| Oppressive | "The cold is absolute. Your fingers are going numb despite your gear" |

### 5.3 Texture Templates

| Intensity | Descriptor |
|-----------|------------|
| Subtle | "The metal is smooth, well-maintained despite its age" |
| Moderate | "Surfaces are rough with corrosion, catching at your gloves" |
| Moderate | "Everything feels greasy, coated with some industrial residue" |
| Oppressive | "The walls are wrong — organic, pulsing, warm to the touch" |
| Oppressive | "Ice has claimed every surface. Metal sticks to skin" |

### 5.4 Moisture Templates

| Intensity | Descriptor |
|-----------|------------|
| Subtle | "Humidity hangs heavy — somewhere, water moves" |
| Moderate | "Condensation beads on every surface, dripping constantly" |
| Moderate | "The air is so dry your lips crack, your throat scratches" |
| Oppressive | "Water streams down the walls. You're in a failing dam" |
| Oppressive | "Ice forms on your gear as you watch. The moisture is freezing" |

### 5.5 Vibration Templates

| Intensity | Descriptor |
|-----------|------------|
| Subtle | "A faint vibration through the floor — distant machinery, still running" |
| Moderate | "The floor trembles rhythmically. Something massive is moving nearby" |
| Moderate | "Vibration in your teeth — subsonic, just at the edge of perception" |
| Oppressive | "The ground SHAKES. Earthquake, or something far worse" |
| Oppressive | "Everything is vibrating. The structure is failing around you" |

---

## 6. Taste Descriptors

> [!NOTE]
> Taste descriptors are rare — reserved for extreme conditions, ambient air quality, or deliberate actions (eating, drinking, tasting blood).

### 6.1 Ambient Taste Templates

| Intensity | Descriptor |
|-----------|------------|
| Subtle | "A metallic tang in the air — iron, or blood, you can't tell" |
| Moderate | "Ash on your tongue with every breath. The fire isn't far" |
| Moderate | "The air tastes wrong — chemical, synthetic, dangerous" |
| Oppressive | "Copper floods your mouth. Your own blood" |
| Oppressive | "The taste of ozone burns your tongue. Lightning is imminent" |

### 6.2 Condition-Specific Tastes

| Condition | Descriptor |
|-----------|------------|
| Bleeding | "Blood fills your mouth, warm and copper" |
| Exhaustion | "Salt on your lips — dried sweat" |
| Fear | "Bile rises in your throat, bitter and urgent" |
| Corruption | "Something wrong on your tongue. Reality tastes of static" |
| Cold | "Your breath tastes of nothing. The cold has frozen even that" |

---

## 7. Biome Sensory Profiles

### 7.1 The Roots (Industrial Decay)

| Sense | Signature |
|-------|-----------|
| Sound | Dripping, grinding machinery, distant collapses |
| Smell | Rust, oil, mold, stagnant water |
| Sight | Flickering lights, rust-orange, cramped corridors |
| Touch | Cold metal, damp, rough corrosion |
| Taste | Metallic air, industrial residue |

### 7.2 Muspelheim (Volcanic Inferno)

| Sense | Signature |
|-------|-----------|
| Sound | Roaring flames, hissing steam, cracking stone |
| Smell | Sulfur, burning, superheated metal |
| Sight | Orange-red glow, heat shimmer, ash clouds |
| Touch | Oppressive heat, hot surfaces |
| Taste | Ash, sulfur, burnt air |

### 7.3 Niflheim (Frozen Waste)

| Sense | Signature |
|-------|-----------|
| Sound | Howling wind, creaking ice, oppressive silence |
| Smell | Clean nothing, preserved ancient air |
| Sight | White-blue, ice crystals, blinding glare |
| Touch | Deadly cold, ice on every surface |
| Taste | Nothing — frozen |

### 7.4 Svartalfheim (Lightless Deep)

| Sense | Signature |
|-------|-----------|
| Sound | Echoes, clicking, dripping, absolute silence |
| Smell | Wet stone, mineral, ancient stagnation |
| Sight | Darkness, bioluminescence, shadow |
| Touch | Cold stone, moisture, vibration |
| Taste | Mineral water, cave air |

### 7.5 Vanaheim (Overgrown Garden)

| Sense | Signature |
|-------|-----------|
| Sound | Rustling, growth sounds, animal calls |
| Smell | Vegetation, rot, pollen, toxic blooms |
| Sight | Green-filtered light, moving vines, spores |
| Touch | Humidity, organic textures, warmth |
| Taste | Pollen, sap, danger on the tongue |

---

## 8. Composite Sensory Templates

### 8.1 Full Atmospheric Template

```
{Sight_Lighting}. {Sound_Ambient}.

The air {Smell_Describe}. {Touch_Temperature}.

{Detail_Optional}.
```

### 8.2 Example Output (The Roots)

> "Lume-strips flicker uncertainly overhead, casting dancing shadows on corroded walls. Machinery thrums somewhere in the walls — systems still running after all these years.
>
> The air reeks of rust and age, filling every breath with the scent of entropy itself. Cold metal seeps through your gloves wherever you touch.
>
> Rust flakes drift through the stale air like orange snow."

---

## 9. Implementation Status

| Category | Subcategories | Fragments |
|----------|---------------|-----------|
| Sound | 5 types × 5 biomes | 75+ |
| Smell | 4 types × 5 biomes | 60+ |
| Sight | 4 types × 5 biomes | 60+ |
| Touch | 4 types × 5 biomes | 60+ |
| Taste | Rare/conditional | 25+ |
| **Total** | — | **280+** |

---

## 10. Related Specifications

| Spec | Relationship |
|------|--------------|
| [Descriptor Overview](overview.md) | Parent framework |
| [Room Engine Descriptors](../room-engine/descriptors.md) | Room-specific implementation |
| [Biome Specifications](../biomes/) | Biome-specific context |

---

## 11. Changelog

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-14 | Initial specification with 5 senses, biome profiles |
