# v0.38.9: Perception & Examination Descriptors

Description: 100+ object examination, 50+ perception success, 30+ flora/fauna descriptors, layered detail levels
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.38, v0.37.1
Implementation Difficulty: Medium
Balance Validated: No
Parent item: v0.38: Descriptor Library & Content Database (v0%2038%20Descriptor%20Library%20&%20Content%20Database%200a9293f3a9b44c968a36c0a429ab841d.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Parent Specification:** [v0.38: Descriptor Library & Content Database](v0%2038%20Descriptor%20Library%20&%20Content%20Database%200a9293f3a9b44c968a36c0a429ab841d.md)

**Status:** Design Phase

**Timeline:** 10-12 hours

**Goal:** Build comprehensive examination and perception descriptor library

**Philosophy:** Reward player curiosity with layered environmental storytelling

---

## I. Purpose

v0.38.9 creates **Perception & Examination Descriptors**, adding depth to world exploration:

- **100+ Object Examination Descriptors** (detailed object inspection)
- **50+ Perception Success Descriptors** (notice hidden elements)
- **30+ Flora Descriptors** (plants, fungi, growths)
- **30+ Fauna Descriptors** (living creatures, non-hostile)
- **Layered Detail Levels** (cursory → thorough → expert)

**Strategic Function:**

Currently, examination is minimal:

- ❌ "A rusty door. It's locked."
- ❌ No reward for high Perception/WITS
- ❌ Missed environmental storytelling

**v0.38.9 Solution:**

- Multi-layer examination (more detail with better checks)
- Perception reveals hidden elements
- Flora/fauna add living world feeling
- Biome-specific details
- Lore integration through examination

---

## II. The Rule: What's In vs. Out

### ✅ In Scope

- **Object Examination:** Interactive objects, room features, decorative elements
- **Perception Checks:** Hidden elements, traps, secrets
- **Flora Descriptors:** Plants, fungi, mosses, crystalline growths
- **Fauna Descriptors:** Ambient creatures, critters, non-combat animals
- **Detail Layering:** Basic → Detailed → Expert examination
- **Lore Integration:** Examine reveals world history
- **Biome-specific variants**

### ❌ Out of Scope

- Enemy examination (covered in combat spec)
- Item identification (separate system)
- Quest-specific examination triggers (v0.40)
- NPC descriptions (separate system)
- UI/rendering changes

---

## III. Examination Detail Layers

### Layer 1: Cursory (No Check)

**Basic visual description, obvious features**

- "A corroded support pillar."
- "An ancient control console."
- "A pile of machinery debris."

### Layer 2: Detailed (WITS Check DC 12)

**Functional details, condition assessment, hints**

- "A corroded support pillar, rust eating through the iron. Structural integrity is compromised—it might collapse under stress."
- "An ancient Jötun control console. The display is cracked but still flickers with runic power. Several access ports remain functional."
- "A pile of machinery debris. Closer inspection reveals Servitor components—damaged power cells, fractured limbs, corrupted data cores."

### Layer 3: Expert (WITS Check DC 18)

**Historical context, technical details, secrets**

- "A corroded support pillar bearing the maker's mark of Clan Ironforge. The degradation pattern suggests 800 years of neglect. Hidden within the rust, you spot a concealed maintenance panel."
- "An ancient Jötun control console, Model KR-7844. Pre-Blight manufacture, designed for citadel-wide systems control. The active ports could interface with modern equipment. Warning indicators suggest emergency shutdown protocols were never completed."
- "A pile of machinery debris that tells a story. These Servitors were destroyed recently—within days. The damage patterns suggest concentrated fire from above. Someone or something ambushed them here."

---

## IV. Object Examination by Category

### Category 1: Doors

### Basic Locked Door

**Cursory:**

- "A heavy iron door, currently locked."

**Detailed (DC 12):**

- "A heavy iron door reinforced with Jötun metalwork. The lock mechanism is complex—Jötun engineering, designed to resist forced entry. No visible signs of recent use; dust coats the handle."

**Expert (DC 18):**

- "A heavy iron door bearing the seal of Level 7 Security Clearance. The lock mechanism uses a combination of physical tumblers and runic authentication. Age has corrupted the rune-lock—a skilled lockpicker might bypass the physical mechanism, or someone with Galdr knowledge could attempt to restore the runic component."

### Blast Door

**Cursory:**

- "A massive sealed blast door."

**Detailed (DC 12):**

- "A massive blast door, thirty centimeters of reinforced alloy. Emergency seals were activated—the door was closed during the Blight's arrival. The control panel is dark but intact."

**Expert (DC 18):**

- "A Jötun-class emergency blast door. The emergency protocols locked this sector off 800 years ago during the evacuation. The door can only be opened from the inside or with citadel-level override codes. However, you notice the power conduits feeding the door have degraded—cutting power might disengage the magnetic locks."

---

### Category 2: Machinery

### Servitor Corpse

**Cursory:**

- "A destroyed Servitor, its chassis crumpled."

**Detailed (DC 12):**

- "A destroyed Servitor, Model M-12 maintenance drone. Its chassis shows signs of corrupted runic energy—the Blight turned it hostile. Death was recent; the power core is still warm. Salvageable components include damaged actuators and a partially intact data core."

**Expert (DC 18):**

- "A destroyed Servitor, Model M-12, serial number suggests manufacture circa 780 years pre-Blight. The corruption pattern is unusual—this drone was exposed to concentrated Blight energy, likely from Alfheim's expansion. The data core, if recoverable, might contain pre-corruption logs showing what it witnessed. The actuators could be repaired with proper tools."

### Ancient Console

**Cursory:**

- "An ancient control console, still flickering with power."

**Detailed (DC 12):**

- "An ancient Jötun control console designed for environmental systems management. The display shows a corrupted schematic of this sector. Three access ports remain functional—you could interface a Data-Slate to attempt data recovery. Warning indicators suggest hazardous atmosphere in connected areas."

**Expert (DC 18):**

- "A Jötun Environmental Control Console, Designation ENV-4422. It still has partial connection to ancient systems. The schematic reveals this level connected to the main geothermal plant—heat regulation failed 800 years ago. The console could potentially be used to divert power, vent atmospheres, or even access locked-down emergency protocols. However, any commands sent will echo through the entire network—everything connected will know you're here."

---

### Category 3: Decorative/Narrative Elements

### Wall Inscription

**Cursory:**

- "Faded writing on the wall."

**Detailed (DC 12):**

- "An inscription in Dvergr runic script, faded but legible: 'Seek not the deep places. The All-Rune waits below.' The warning was carved hastily—whoever wrote this was in a hurry."

**Expert (DC 18):**

- "An inscription in Classical Dvergr, the formal dialect used before the Blight. The phrasing suggests a Runecaster's warning. 'Seek not the deep places' refers specifically to Alfheim's expansion—the lower levels. 'The All-Rune waits below' is a reference to the Blight's epicenter. The carver's chisel-work reveals they were a master crafts-dwarf, yet their hand shook with fear. This was carved during the evacuation, a final warning to any who might return."

### Skeleton

**Cursory:**

- "A skeleton slumped against the wall."

**Detailed (DC 12):**

- "A human skeleton, clothing rotted to rags. They died here 800 years ago during the evacuation. The right hand still clutches a broken blade. The skull shows signs of trauma—death was violent."

**Expert (DC 18):**

- "A human skeleton, likely a citadel guard based on the remnants of uniform. They died defending this position during the final evacuation. The blade they wielded broke mid-combat—the fracture pattern suggests it struck something impossibly hard. The positioning of the bones tells a story: they backed into this corner, wounded, and made their last stand here. Scratch marks on the floor show something dragged itself toward them. They died buying time for others to escape."

---

## V. Perception Check Descriptors

### Hidden Trap Detection

**Success (DC 15):**

- "Your trained eye catches a discrepancy—a pressure plate, barely visible beneath the dust!"
- "Something's wrong with this floor tile. It's newer than the others, carefully placed. A trap."
- "You notice thin wires at ankle height, almost invisible. A tripwire trap!"

**Expert Success (DC 20):**

- "You spot the pressure plate and trace its mechanism—it's connected to a ceiling collapse system. More concerning: the trap is recent. Someone has been here within the past week."

### Secret Door Detection

**Success (DC 16):**

- "The wall here looks slightly different. You run your hands along it and find a concealed seam—a hidden door!"
- "Air currents suggest a space behind this wall. You search and discover a hidden passage!"

**Expert Success (DC 22):**

- "You find the hidden door and immediately understand its purpose: this is an emergency escape route, Jötun construction. The mechanism is mechanical, not runic—it would still work even during power failures. The door opens to a service tunnel that bypasses the main corridors."

### Hidden Cache Detection

**Success (DC 14):**

- "Something's hidden here—you spot a loose floor panel. Prying it up reveals a hidden cache!"
- "Your eye catches a disturbed dust pattern. Investigating reveals a concealed compartment!"

---

## VI. Flora Descriptors by Biome

### The Roots

### Luminous Shelf Fungus

**Cursory:**

- "Large shelf fungus growing from the wall, glowing faintly."

**Detailed (DC 12):**

- "Massive shelf fungus, bioluminescent—a common sight in the lower levels. The glow is natural, produced by symbiotic bacteria. The fungus is edible but bitter. Alchemically useful for light-source potions."

**Expert (DC 18):**

- "Luminous Shelf Fungus, Fungus lucidus. It thrives in high-humidity, low-light environments—exactly what the Roots became after the cooling systems failed. The bioluminescence evolved as a survival mechanism, attracting insects that spread spores. Harvesting it requires care; damaging the cap releases toxin-laden spores. Properly prepared, it's a potent alchemical reagent for light, vision, and consciousness-altering potions."

### Rust-Eater Moss

**Cursory:**

- "Orange moss coating the metal surfaces."

**Detailed (DC 12):**

- "Rust-Eater Moss, feeding on oxidized iron. It's accelerated the corrosion in this area—weakening structural supports. The moss itself is harmless but indicates severe decay."

---

### Muspelheim

### Ember Moss

**Cursory:**

- "Red moss growing near heat sources, pulsing like coals."

**Detailed (DC 12):**

- "Ember Moss, a thermophilic organism that thrives in extreme heat. The pulsing glow is real—the moss generates heat through chemical reactions. Touching it will burn you. Alchemically, it's a key component in fire resistance potions."

**Expert (DC 18):**

- "Ember Moss, one of the few organisms that survived Muspelheim's volcanic transformation. It doesn't just tolerate heat—it requires temperatures above 80°C to survive. The moss stores thermal energy in specialized cells. Harvesting requires heat-resistant gloves and proper timing (during its 'dormant' phase, which lasts mere seconds). Master alchemists use it for fire resistance, heat generation, and even experimental explosive compounds."

---

### Niflheim

### Frost Lichen

**Cursory:**

- "Pale blue lichen coating frozen surfaces."

**Detailed (DC 12):**

- "Frost Lichen, adapted to sub-zero temperatures. It grows slowly, spreading across any frozen surface. The blue coloration comes from ice crystals integrated into its cellular structure. Alchemically useful for cold resistance potions."

---

### Alfheim

### Paradox Spore Clusters

**Cursory:**

- "Strange crystalline growths that seem to shift when you look away."

**Detailed (DC 12):**

- "Paradox Spore Clusters—if they can even be called organic. They exist in multiple states simultaneously, both fungus and crystal, living and not. The Blight created them. Approach with extreme caution; they're unpredictable."

**Expert (DC 18):**

- "Paradox Spore Clusters, a true impossibility made manifest by the Blight. They violate biological laws—reproducing backward through time, existing as both spore and mature colony. Harvesting them is dangerous; they may infect the harvester with Blight Corruption. However, they're the most potent source of paradoxical energy known—essential for reality-bending Galdr and experimental runecraft. Handle only with proper containment protocols."

---

## VII. Fauna Descriptors

### Non-Hostile Ambient Creatures

### Cave Rat

**Observation:**

- "A rat scurries across the floor, disappearing into a crack in the wall."
- "You hear the scratch of tiny claws—rats, living in the walls."

**Expert Observation (DC 15):**

- "Rats thrive here despite everything. Their presence is actually reassuring—rats flee before serious threats. If the rats are calm, the immediate area is relatively safe."

### Rust Beetles

**Observation:**

- "Small metallic beetles skitter across corroded surfaces, feeding on rust."

**Expert Observation (DC 15):**

- "Rust Beetles, Ferrum scarabaeus. They feed exclusively on oxidized metals. Their presence indicates this area has been undisturbed for years—they're shy creatures that flee from activity. Harvesting them is tricky but worthwhile; alchemists use their shells for metal-strengthening compounds."

### Blight-Moths

**Observation:**

- "Pale moths flutter through the air, drawn to runic light."

**Expert Observation (DC 15):**

- "Blight-Moths, creatures born from paradox. They shouldn't exist but do, feeding on runic energy. They're harmless and actually useful—they're attracted to active rune-magic. Seiðkona use them to detect magical signatures."

---

*Continued with database schema and implementation...*