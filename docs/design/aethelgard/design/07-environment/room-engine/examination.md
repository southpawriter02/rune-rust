---
id: SPEC-ROOMENGINE-EXAMINATION
title: "Examination & Perception System"
version: 1.0
status: draft
last-updated: 2025-12-14
related-files:
  - path: "docs/07-environment/room-engine/descriptors.md"
    status: Reference
  - path: "docs/07-environment/puzzle-system.md"
    status: Reference
  - path: "docs/08-ui/commands/interaction.md"
    status: Reference
  - path: "data/schemas/v0.38.9_examination_perception_descriptors_schema.sql"
    status: Active
  - path: "data/descriptors/v0.38.9_examination_perception_descriptors_data.sql"
    status: Active
---

# Examination & Perception System

> *"The citadel reveals its secrets to those who look carefully. Every scratch tells a story, every shadow hides a truth."*

---

## 1. Overview

The examination system provides **layered environmental detail** that rewards player curiosity. Objects, features, and the environment itself reveal progressively more information based on WITS checks, creating meaningful exploration and environmental storytelling.

### 1.1 Design Philosophy

| Principle | Description |
|-----------|-------------|
| **Reward Curiosity** | Players who examine objects learn more about the world |
| **Meaningful Checks** | WITS determines depth of understanding, not access |
| **Layered Discovery** | Three tiers of detail (Cursory → Detailed → Expert) |
| **No Gatekeeping** | Basic information always available; deeper knowledge requires skill |
| **Environmental Storytelling** | 800 years of history visible in every room |

### 1.2 Content Coverage

| Category | Descriptor Count | Purpose |
|----------|------------------|---------|
| Object Examination | 100+ | Interactive objects, doors, machinery |
| Perception Success | 50+ | Hidden elements, traps, secrets |
| Flora | 30+ | Plants, fungi, growths by biome |
| Fauna | 30+ | Ambient creatures, non-hostile animals |
| **Total** | ~210+ | Full examination library |

---

## 2. Three-Layer Detail System

### 2.1 Layer Definitions

| Layer | Name | Check Required | Information Depth |
|-------|------|----------------|-------------------|
| **Layer 1** | Cursory | None | Basic visual description, obvious features |
| **Layer 2** | Detailed | WITS DC 12 | Functional details, condition, hints |
| **Layer 3** | Expert | WITS DC 18 | Historical context, technical details, secrets |

### 2.2 Information Progression

```
┌─────────────────────────────────────────────────────────────┐
│ LAYER 1: CURSORY (No Check)                                 │
│   "What it looks like at a glance"                          │
│   • Basic physical description                              │
│   • Obvious state (open, closed, broken)                    │
│   • General material and size                               │
└─────────────────────────────────────────────────────────────┘
                          ↓ WITS DC 12
┌─────────────────────────────────────────────────────────────┐
│ LAYER 2: DETAILED                                           │
│   "What closer inspection reveals"                          │
│   • Functional assessment                                   │
│   • Condition and age                                       │
│   • Mechanical hints                                        │
│   • Salvageable components                                  │
└─────────────────────────────────────────────────────────────┘
                          ↓ WITS DC 18
┌─────────────────────────────────────────────────────────────┐
│ LAYER 3: EXPERT                                             │
│   "What a trained eye understands"                          │
│   • Historical context and lore                             │
│   • Technical specifications                                │
│   • Hidden features and secrets                             │
│   • Strategic implications                                  │
└─────────────────────────────────────────────────────────────┘
```

### 2.3 Automatic Layer Unlocking

Players automatically see all layers up to their check result:

| WITS Check Result | Layers Shown |
|-------------------|--------------|
| No check / Failed | Layer 1 only |
| DC 12-17 | Layers 1 + 2 |
| DC 18+ | Layers 1 + 2 + 3 |

---

## 3. Object Examination by Category

### 3.1 Doors

#### Basic Locked Door

**Cursory (No Check):**
> "A heavy iron door, currently locked."

**Detailed (DC 12):**
> "A heavy iron door reinforced with Jötun metalwork. The lock mechanism is complex—Jötun engineering, designed to resist forced entry. No visible signs of recent use; dust coats the handle."

**Expert (DC 18):**
> "A heavy iron door bearing the seal of Level 7 Security Clearance. The lock mechanism uses a combination of physical tumblers and runic authentication. Age has corrupted the rune-lock—a skilled lockpicker might bypass the physical mechanism, or someone with Galdr knowledge could attempt to restore the runic component."

#### Blast Door

**Cursory (No Check):**
> "A massive sealed blast door."

**Detailed (DC 12):**
> "A massive blast door, thirty centimeters of reinforced alloy. Emergency seals were activated—the door was closed during the Blight's arrival. The control panel is dark but intact."

**Expert (DC 18):**
> "A Jötun-class emergency blast door. The emergency protocols locked this sector off 800 years ago during the evacuation. The door can only be opened from the inside or with citadel-level override codes. However, you notice the power conduits feeding the door have degraded—cutting power might disengage the magnetic locks."

---

### 3.2 Machinery

#### Servitor Corpse

**Cursory (No Check):**
> "A destroyed Servitor, its chassis crumpled."

**Detailed (DC 12):**
> "A destroyed Servitor, Model M-12 maintenance drone. Its chassis shows signs of corrupted runic energy—the Blight turned it hostile. Death was recent; the power core is still warm. Salvageable components include damaged actuators and a partially intact data core."

**Expert (DC 18):**
> "A destroyed Servitor, Model M-12, serial number suggests manufacture circa 780 years pre-Blight. The corruption pattern is unusual—this drone was exposed to concentrated Blight energy, likely from Alfheim's expansion. The data core, if recoverable, might contain pre-corruption logs showing what it witnessed. The actuators could be repaired with proper tools."

#### Ancient Console

**Cursory (No Check):**
> "An ancient control console, still flickering with power."

**Detailed (DC 12):**
> "An ancient Jötun control console designed for environmental systems management. The display shows a corrupted schematic of this sector. Three access ports remain functional—you could interface a Data-Slate to attempt data recovery. Warning indicators suggest hazardous atmosphere in connected areas."

**Expert (DC 18):**
> "A Jötun Environmental Control Console, Designation ENV-4422. It still has partial connection to ancient systems. The schematic reveals this level connected to the main geothermal plant—heat regulation failed 800 years ago. The console could potentially be used to divert power, vent atmospheres, or even access locked-down emergency protocols. However, any commands sent will echo through the entire network—everything connected will know you're here."

---

### 3.3 Decorative/Narrative Elements

#### Wall Inscription

**Cursory (No Check):**
> "Faded writing on the wall."

**Detailed (DC 12):**
> "An inscription in Dvergr runic script, faded but legible: 'Seek not the deep places. The All-Rune waits below.' The warning was carved hastily—whoever wrote this was in a hurry."

**Expert (DC 18):**
> "An inscription in Classical Dvergr, the formal dialect used before the Blight. The phrasing suggests a Runecaster's warning. 'Seek not the deep places' refers specifically to Alfheim's expansion—the lower levels. 'The All-Rune waits below' is a reference to the Blight's epicenter. The carver's chisel-work reveals they were a master crafts-dwarf, yet their hand shook with fear. This was carved during the evacuation, a final warning to any who might return."

#### Skeleton

**Cursory (No Check):**
> "A skeleton slumped against the wall."

**Detailed (DC 12):**
> "A human skeleton, clothing rotted to rags. They died here 800 years ago during the evacuation. The right hand still clutches a broken blade. The skull shows signs of trauma—death was violent."

**Expert (DC 18):**
> "A human skeleton, likely a citadel guard based on the remnants of uniform. They died defending this position during the final evacuation. The blade they wielded broke mid-combat—the fracture pattern suggests it struck something impossibly hard. The positioning of the bones tells a story: they backed into this corner, wounded, and made their last stand here. Scratch marks on the floor show something dragged itself toward them. They died buying time for others to escape."

---

## 4. Perception Check Descriptors

### 4.1 Hidden Trap Detection

**Success (DC 15):**
> "Your trained eye catches a discrepancy—a pressure plate, barely visible beneath the dust!"

> "Something's wrong with this floor tile. It's newer than the others, carefully placed. A trap."

> "You notice thin wires at ankle height, almost invisible. A tripwire trap!"

**Expert Success (DC 20):**
> "You spot the pressure plate and trace its mechanism—it's connected to a ceiling collapse system. More concerning: the trap is recent. Someone has been here within the past week."

### 4.2 Secret Door Detection

**Success (DC 16):**
> "The wall here looks slightly different. You run your hands along it and find a concealed seam—a hidden door!"

> "Air currents suggest a space behind this wall. You search and discover a hidden passage!"

**Expert Success (DC 22):**
> "You find the hidden door and immediately understand its purpose: this is an emergency escape route, Jötun construction. The mechanism is mechanical, not runic—it would still work even during power failures. The door opens to a service tunnel that bypasses the main corridors."

### 4.3 Hidden Cache Detection

**Success (DC 14):**
> "Something's hidden here—you spot a loose floor panel. Prying it up reveals a hidden cache!"

> "Your eye catches a disturbed dust pattern. Investigating reveals a concealed compartment!"

---

## 5. Flora Descriptors by Biome

### 5.1 The Roots

#### Luminous Shelf Fungus

**Cursory (No Check):**
> "Large shelf fungus growing from the wall, glowing faintly."

**Detailed (DC 12):**
> "Massive shelf fungus, bioluminescent—a common sight in the lower levels. The glow is natural, produced by symbiotic bacteria. The fungus is edible but bitter. Alchemically useful for light-source potions."

**Expert (DC 18):**
> "Luminous Shelf Fungus, *Fungus lucidus*. It thrives in high-humidity, low-light environments—exactly what the Roots became after the cooling systems failed. The bioluminescence evolved as a survival mechanism, attracting insects that spread spores. Harvesting it requires care; damaging the cap releases toxin-laden spores. Properly prepared, it's a potent alchemical reagent for light, vision, and consciousness-altering potions."

#### Rust-Eater Moss

**Cursory (No Check):**
> "Orange moss coating the metal surfaces."

**Detailed (DC 12):**
> "Rust-Eater Moss, feeding on oxidized iron. It's accelerated the corrosion in this area—weakening structural supports. The moss itself is harmless but indicates severe decay."

---

### 5.2 Muspelheim

#### Ember Moss

**Cursory (No Check):**
> "Red moss growing near heat sources, pulsing like coals."

**Detailed (DC 12):**
> "Ember Moss, a thermophilic organism that thrives in extreme heat. The pulsing glow is real—the moss generates heat through chemical reactions. Touching it will burn you. Alchemically, it's a key component in fire resistance potions."

**Expert (DC 18):**
> "Ember Moss, one of the few organisms that survived Muspelheim's volcanic transformation. It doesn't just tolerate heat—it requires temperatures above 80°C to survive. The moss stores thermal energy in specialized cells. Harvesting requires heat-resistant gloves and proper timing (during its 'dormant' phase, which lasts mere seconds). Master alchemists use it for fire resistance, heat generation, and even experimental explosive compounds."

---

### 5.3 Niflheim

#### Frost Lichen

**Cursory (No Check):**
> "Pale blue lichen coating frozen surfaces."

**Detailed (DC 12):**
> "Frost Lichen, adapted to sub-zero temperatures. It grows slowly, spreading across any frozen surface. The blue coloration comes from ice crystals integrated into its cellular structure. Alchemically useful for cold resistance potions."

---

### 5.4 Alfheim

#### Paradox Spore Clusters

**Cursory (No Check):**
> "Strange crystalline growths that seem to shift when you look away."

**Detailed (DC 12):**
> "Paradox Spore Clusters—if they can even be called organic. They exist in multiple states simultaneously, both fungus and crystal, living and not. The Blight created them. Approach with extreme caution; they're unpredictable."

**Expert (DC 18):**
> "Paradox Spore Clusters, a true impossibility made manifest by the Blight. They violate biological laws—reproducing backward through time, existing as both spore and mature colony. Harvesting them is dangerous; they may infect the harvester with Blight Corruption. However, they're the most potent source of paradoxical energy known—essential for reality-bending Galdr and experimental runecraft. Handle only with proper containment protocols."

---

## 6. Fauna Descriptors

### 6.1 Non-Hostile Ambient Creatures

#### Cave Rat

**Observation:**
> "A rat scurries across the floor, disappearing into a crack in the wall."

> "You hear the scratch of tiny claws—rats, living in the walls."

**Expert Observation (DC 15):**
> "Rats thrive here despite everything. Their presence is actually reassuring—rats flee before serious threats. If the rats are calm, the immediate area is relatively safe."

#### Rust Beetles

**Observation:**
> "Small metallic beetles skitter across corroded surfaces, feeding on rust."

**Expert Observation (DC 15):**
> "Rust Beetles, *Ferrum scarabaeus*. They feed exclusively on oxidized metals. Their presence indicates this area has been undisturbed for years—they're shy creatures that flee from activity. Harvesting them is tricky but worthwhile; alchemists use their shells for metal-strengthening compounds."

#### Blight-Moths

**Observation:**
> "Pale moths flutter through the air, drawn to runic light."

**Expert Observation (DC 15):**
> "Blight-Moths, creatures born from paradox. They shouldn't exist but do, feeding on runic energy. They're harmless and actually useful—they're attracted to active rune-magic. Seiðkona use them to detect magical signatures."

---

## 7. Integration with Game Systems

### 7.1 Examine Command

The `examine` command triggers the examination system:

```
> examine door

  [WITS Check vs DC 12]
  Rolling... 2 successes (rolled 8, 9, 4, 6)

  A heavy iron door reinforced with Jötun metalwork. The lock mechanism
  is complex—Jötun engineering, designed to resist forced entry. No
  visible signs of recent use; dust coats the handle.

  [Detailed examination complete]
```

### 7.2 Passive Perception

Room entry triggers passive perception for hidden elements:

```
┌─────────────────────────────────────────────────────────────┐
│ On Room Entry:                                              │
│   1. Calculate Passive Perception (WITS ÷ 2, round up)      │
│   2. Compare to hidden element DCs                          │
│   3. Auto-reveal elements where Passive >= DC               │
│   4. Describe revealed elements in room description         │
└─────────────────────────────────────────────────────────────┘
```

### 7.3 Search vs. Examine

| Command | Target | Skill | Effect |
|---------|--------|-------|--------|
| `examine <object>` | Specific object | WITS | Layered detail based on check |
| `search` | Entire room | WITS | Reveal hidden objects/elements |
| `search <container>` | Container | — | List container contents |

### 7.4 Puzzle Integration

Expert examination can reveal puzzle solutions:

```
> examine console (Expert success, DC 18)

  "...The console could potentially be used to divert power, vent
  atmospheres, or even access locked-down emergency protocols."

  [New puzzle solution revealed: Console Override (WITS DC 4)]
```

---

## 8. Data Schema

### 8.1 Examination Descriptor Table

```sql
CREATE TABLE ExaminationDescriptors (
    Id                  UNIQUEIDENTIFIER PRIMARY KEY,
    ObjectCategory      VARCHAR(50) NOT NULL,      -- Door, Machinery, Decorative, etc.
    ObjectType          VARCHAR(100) NOT NULL,     -- LockedDoor, BlastDoor, Console, etc.
    Layer               INT NOT NULL,              -- 1 = Cursory, 2 = Detailed, 3 = Expert
    RequiredDC          INT NOT NULL DEFAULT 0,    -- 0 for Layer 1
    BiomeAffinity       VARCHAR(50) NULL,          -- NULL = universal
    DescriptorText      NVARCHAR(MAX) NOT NULL,
    RevealsHint         BIT DEFAULT 0,             -- Does this reveal puzzle hints?
    RevealsSolution     VARCHAR(100) NULL,         -- Solution ID revealed (if any)
    Weight              INT DEFAULT 1,             -- Selection weight for variants
    CreatedAt           DATETIME2 DEFAULT GETUTCDATE()
);

CREATE INDEX IX_ExamDesc_Category ON ExaminationDescriptors(ObjectCategory, ObjectType);
CREATE INDEX IX_ExamDesc_Layer ON ExaminationDescriptors(Layer, RequiredDC);
```

### 8.2 Perception Descriptor Table

```sql
CREATE TABLE PerceptionDescriptors (
    Id                  UNIQUEIDENTIFIER PRIMARY KEY,
    PerceptionType      VARCHAR(50) NOT NULL,      -- Trap, SecretDoor, Cache, etc.
    SuccessLevel        VARCHAR(20) NOT NULL,      -- Standard, Expert
    RequiredDC          INT NOT NULL,
    BiomeAffinity       VARCHAR(50) NULL,
    DescriptorText      NVARCHAR(MAX) NOT NULL,
    RevealedElementType VARCHAR(50) NULL,          -- What type of element is revealed
    Weight              INT DEFAULT 1,
    CreatedAt           DATETIME2 DEFAULT GETUTCDATE()
);
```

### 8.3 Flora/Fauna Descriptor Table

```sql
CREATE TABLE FloraFaunaDescriptors (
    Id                  UNIQUEIDENTIFIER PRIMARY KEY,
    Category            VARCHAR(20) NOT NULL,      -- Flora, Fauna
    SpeciesName         VARCHAR(100) NOT NULL,
    ScientificName      VARCHAR(100) NULL,
    Layer               INT NOT NULL,
    RequiredDC          INT NOT NULL DEFAULT 0,
    Biome               VARCHAR(50) NOT NULL,
    DescriptorText      NVARCHAR(MAX) NOT NULL,
    AlchemicalUse       NVARCHAR(500) NULL,
    HarvestDC           INT NULL,
    HarvestRisk         VARCHAR(200) NULL,
    Weight              INT DEFAULT 1,
    CreatedAt           DATETIME2 DEFAULT GETUTCDATE()
);

CREATE INDEX IX_FloraFauna_Biome ON FloraFaunaDescriptors(Biome, Category);
```

---

## 9. Service Interface

### 9.1 IExaminationService

```csharp
public interface IExaminationService
{
    /// <summary>
    /// Examines an object with WITS check, returning layered description.
    /// </summary>
    ExaminationResult ExamineObject(
        Guid objectId,
        int witsPool,
        Random rng);

    /// <summary>
    /// Gets passive perception result for room entry.
    /// </summary>
    PassivePerceptionResult CheckPassivePerception(
        Guid roomId,
        int passivePerception);

    /// <summary>
    /// Gets flora/fauna observation for biome.
    /// </summary>
    string GetAmbientObservation(
        string biome,
        int witsPool,
        Random rng);
}
```

### 9.2 DTOs

```csharp
public record ExaminationResult
{
    public Guid ObjectId { get; init; }
    public string ObjectName { get; init; } = string.Empty;
    public int WitsCheckResult { get; init; }
    public int HighestLayerUnlocked { get; init; }  // 1, 2, or 3
    public string CompositeDescription { get; init; } = string.Empty;
    public bool RevealedHint { get; init; }
    public string? RevealedSolutionId { get; init; }
    public IReadOnlyList<string> LayerTexts { get; init; } = [];
}

public record PassivePerceptionResult
{
    public Guid RoomId { get; init; }
    public int PassivePerception { get; init; }
    public IReadOnlyList<RevealedElement> RevealedElements { get; init; } = [];
}

public record RevealedElement
{
    public Guid ElementId { get; init; }
    public string ElementType { get; init; } = string.Empty;  // Trap, SecretDoor, Cache
    public string DiscoveryText { get; init; } = string.Empty;
    public int DC { get; init; }
}
```

---

## 10. TUI Presentation

### 10.1 Basic Examination

```
> examine door

  A heavy iron door, currently locked.
```

### 10.2 Detailed Examination (DC 12 Success)

```
> examine door

  [WITS Check vs DC 12]
  Rolling... 2 successes

  A heavy iron door reinforced with Jötun metalwork. The lock mechanism
  is complex—Jötun engineering, designed to resist forced entry. No
  visible signs of recent use; dust coats the handle.
```

### 10.3 Expert Examination (DC 18 Success)

```
> examine door

  [WITS Check vs DC 18]
  Rolling... 3 successes

  A heavy iron door bearing the seal of Level 7 Security Clearance.
  The lock mechanism uses a combination of physical tumblers and runic
  authentication. Age has corrupted the rune-lock—a skilled lockpicker
  might bypass the physical mechanism, or someone with Galdr knowledge
  could attempt to restore the runic component.

  [Hint revealed: Runic bypass possible with Galdr skill]
```

### 10.4 Perception Discovery

```
  As you enter the corridor, something catches your eye...

  [Passive Perception: 4 vs DC 15]

  Your trained eye catches a discrepancy—a pressure plate, barely
  visible beneath the dust!

  [Trap revealed: Pressure Plate (disarm DC 14)]
```

---

## 11. Phased Implementation Guide

### Phase 1: Core Examination
- [ ] Implement `ExaminationService.ExamineObject()`
- [ ] Create WITS check integration
- [ ] Build three-layer text composition
- [ ] Add `examine` command to parser

### Phase 2: Passive Perception
- [ ] Implement `CheckPassivePerception()` on room entry
- [ ] Add hidden element detection
- [ ] Integrate with room description generation

### Phase 3: Flora/Fauna
- [ ] Implement biome-specific flora descriptors
- [ ] Add fauna ambient observations
- [ ] Create harvesting mechanics integration

### Phase 4: Puzzle Integration
- [ ] Connect expert examination to puzzle hints
- [ ] Implement solution reveal on high WITS
- [ ] Add `RevealedSolutionId` tracking

---

## 12. Testing Requirements

### 12.1 Unit Tests
- [ ] Layer 1 always returns without check
- [ ] Layer 2 requires DC 12 success
- [ ] Layer 3 requires DC 18 success
- [ ] Composite description includes all unlocked layers

### 12.2 Integration Tests
- [ ] Room entry triggers passive perception
- [ ] Hidden elements reveal at correct DC
- [ ] Puzzle hints link to puzzle system

### 12.3 Manual QA
- [ ] Verify lore consistency across examination text
- [ ] Check biome-appropriate flora/fauna
- [ ] Validate DC progression feels fair

---

## 13. Related Documentation

| Document | Relationship |
|----------|--------------|
| [Descriptors](descriptors.md) | Three-tier composition model |
| [Descriptor Content](descriptor-content.md) | Full content index |
| [Puzzle System](../puzzle-system.md) | Puzzle hint integration |
| [Interaction Commands](../../08-ui/commands/interaction.md) | Search/examine commands |

---

## 14. Changelog

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-14 | Initial specification (migrated from legacy v0.38.9) |
