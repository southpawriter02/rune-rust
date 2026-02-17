# v0.38.14: Trauma Descriptor Library Specification

**Document ID:** RR-SPEC-v0.38.14
**Parent Specification:** v0.38 Descriptor Library & Content Database
**Status:** Implementation Complete
**Timeline:** 10-12 hours
**Author:** Rune & Rust Development Team
**Date:** 2025-11-23

---

## I. Purpose

v0.38.14 creates the **Trauma Manifestation Descriptor Library**, transforming abstract psychological trauma into tangible, visceral gameplay experiences. This specification defines how mental scars manifest in moment-to-moment play through comprehensive narrative descriptors.

### Strategic Function

**Current Problem:**

- Traumas are abstract: *"You have [Paranoia]. -1 to social checks."*
- No sense of how trauma manifests during play
- Missed opportunity for psychological horror
- Mechanical effects without narrative weight

**v0.38.14 Solution:**

- Vivid trauma acquisition scenes (Breaking Point moments)
- Contextual trigger descriptions (environmental, combat, social)
- Mechanical effects reflected in narrative
- Recovery as emotional release (Cognitive Stabilizer, Saga Quests)

### Deliverables

1. **30+ Trauma Type Descriptors** - Acquisition and ongoing manifestation for 10 trauma types
2. **40+ Trauma Trigger Descriptors** - Contextual manifestations based on game state
3. **25+ Breaking Point Descriptors** - Stress warnings and mental breaking moments
4. **20+ Recovery Descriptors** - Trauma suppression and removal narratives
5. **Total: 180+ descriptors** providing comprehensive trauma narrative coverage

---

## II. Design Philosophy

### Trauma as Tangible Experience

Trauma in Rune & Rust is not merely a stat penalty—it is a lived experience that shapes how players interact with the world. The descriptor library embodies this philosophy:

1. **Visceral Manifestation**
    - Traumas have physical, mental, and behavioral components
    - Descriptions engage multiple senses (sight, sound, touch, proprioception)
    - Players *feel* the weight of psychological damage
2. **Contextual Awareness**
    - Traumas respond to environment (darkness, Blight, isolation)
    - Combat triggers differ from social triggers
    - Past experiences echo in similar situations
3. **Progressive Deterioration**
    - Level 1: Manageable but present
    - Level 2: Severe and intrusive
    - Level 3: Critical and debilitating
    - Descriptors scale with trauma progression
4. **Recovery as Journey**
    - Temporary relief (Cognitive Stabilizer) vs. permanent healing (Saga Quest)
    - Confronting trauma is painful but necessary
    - Emotional catharsis when trauma is removed

### Horror Through Intimacy

The most effective horror is personal and intimate. v0.38.14 achieves this through:

- **First-person perspective**: "Your mind shatters" not "The character's mind shatters"
- **Internal experience**: Focus on thoughts, sensations, perceptions
- **Loss of control**: Traumas act *on* the player, not *for* them
- **Uncertainty**: "Is this real?" captures dissociation and paranoia

---

## III. Trauma Type Coverage

### Combat Traumas

### Flashbacks

**Acquisition Context:** Near-death experiences, psychic overload
**Manifestation:** Random combat stuns as past trauma overwhelms present
**Mechanical Effect:** Stunned 1-3 turns (progression-dependent)
**Descriptor Count:** 7

**Example Descriptors:**

- *Acquisition:* "Your mind shatters. Suddenly you're back there—the moment when you thought you'd die."
- *Manifestation:* "Mid-combat, a flashback strikes. For a moment, you're not here—you're reliving past trauma."
- *Trigger:* "You're injured badly. Just like before. The fear is overwhelming."

### Battle Tremors

**Acquisition Context:** Repeated combat stress, nervous system damage
**Manifestation:** Hands shake, weapon fumbles, impaired precision
**Mechanical Effect:** -1 to -3 attack rolls (progression-dependent)
**Descriptor Count:** 5

**Example Descriptors:**

- *Acquisition:* "Your hands won't stop shaking. The adrenaline crash is too much."
- *Manifestation:* "Your hands shake as you try to strike. The blow is less precise than intended."
- *Trigger:* "Your trauma triggers at the sight of the Servitor. Your hands shake."

### Hypervigilance

**Acquisition Context:** Constant threat, repeated ambushes
**Manifestation:** Cannot relax, exhaustion, false threat detection
**Mechanical Effect:** 50% rest effectiveness, -1 to -3 WILL checks
**Descriptor Count:** 7

**Example Descriptors:**

- *Acquisition:* "You can never relax again. Every shadow might hide an enemy."
- *Manifestation:* "Sleep is impossible. Every time you close your eyes, you're certain something approaches."
- *Trigger:* "Too many enemies. Too many angles. You can't watch them all."

### Blight-Related Traumas

### Paradoxical Paranoia

**Acquisition Context:** Blight corruption affecting perception
**Manifestation:** Reality uncertainty, temporal confusion
**Mechanical Effect:** Disadvantage on Perception, 20% false threat detection
**Descriptor Count:** 5

**Example Descriptors:**

- *Acquisition:* "The Blight's corruption takes root in your mind. You start seeing patterns that shouldn't exist."
- *Manifestation:* "Is that person following you, or have they always been ahead of you? Time feels wrong."
- *Trigger:* "The Blight is near. You can feel it warping reality. Or are you warping it?"

### Auditory Hallucinations

**Acquisition Context:** Cursed Choir exposure (Alfheim)
**Manifestation:** Persistent voices, hostile whispers, confusion
**Mechanical Effect:** -2 to -3 concentration checks, threat location confusion
**Descriptor Count:** 7

**Example Descriptors:**

- *Acquisition:* "The Cursed Choir won't leave your mind. Even when you're far from Alfheim, you hear them singing."
- *Manifestation:* "Kill them. They're corrupted. You're corrupted. Everything is corruption. The voice won't stop."
- *Trigger:* "The Choir grows louder as the Blight intensifies. They're calling to you."

### Reality Dissociation

**Acquisition Context:** Reality barrier collapse (severe Blight)
**Manifestation:** Uncertainty about existence, body dissociation
**Mechanical Effect:** Random 1 AP loss per turn (20-35% chance)
**Descriptor Count:** 5

**Example Descriptors:**

- *Acquisition:* "The barrier between real and unreal dissolves. You're no longer sure which side you're on."
- *Manifestation:* "For a moment, you're not convinced you're real. Maybe you're someone else's hallucination."
- *Trigger:* "The Blight makes everything uncertain. What's real? What's corruption?"

### Social Traumas

### Corrupted Social Script

**Acquisition Context:** Blight-induced social cognition damage
**Manifestation:** Incomprehensible social cues, awkward interactions
**Mechanical Effect:** -3 to -5 Rhetoric checks, NPC discomfort
**Descriptor Count:** 5

**Example Descriptors:**

- *Acquisition:* "Your ability to interact normally is broken. The Blight has corrupted the part of you that understands people."
- *Manifestation:* "You say something. Their expression tells you it was wrong. You don't understand why."
- *Trigger:* "So many faces. So many incomprehensible expressions. You can't process them all."

### Trust Erosion

**Acquisition Context:** Betrayal, repeated deception
**Manifestation:** Cannot trust allies or NPCs, automatic refusal of help
**Mechanical Effect:** Disadvantage on cooperation, NPC help blocked
**Descriptor Count:** 5

**Example Descriptors:**

- *Acquisition:* "You've been betrayed too many times. You can't trust anyone anymore. Not even yourself."
- *Manifestation:* "They offer help. You refuse automatically. You can't trust them."
- *Trigger:* "They lie. Of course they lie. Everyone lies."

### Existential Traumas

### Systemic Apathy

**Acquisition Context:** Existential crisis, nihilistic revelation
**Manifestation:** Mechanical actions, no motivation, detachment
**Mechanical Effect:** -2 to -3 initiative, quest motivation penalties
**Descriptor Count:** 5

**Example Descriptors:**

- *Acquisition:* "What's the point? The world ended 800 years ago. Everything you do is meaningless."
- *Manifestation:* "You move mechanically, going through the motions. Nothing matters."
- *Trigger:* "Alone in the ruins. A fitting metaphor. You and the corpse of civilization, both meaningless."

### Existential Dread

**Acquisition Context:** Cosmic horror realization, abyss confrontation
**Manifestation:** Periodic stress accumulation, despair vulnerability
**Mechanical Effect:** +3-5 Psychic Stress periodically, despair weakness
**Descriptor Count:** 5

**Example Descriptors:**

- *Acquisition:* "The weight of existence crushes you. 800 years of horror. Why does anything continue?"
- *Manifestation:* "The meaninglessness of it all overwhelms you suddenly. [+3 Psychic Stress]"
- *Trigger:* "In the silence, the questions return. Why continue? Why exist? The void has no answers."

---

## IV. Breaking Point System

### Stress Threshold Phases

### Phase 1: Warning Signs (75-99% Stress)

**Purpose:** Telegraph impending breaking point
**Frequency:** 30% chance per turn above 75% stress
**Descriptor Count:** 13

**Categories:**

- Visual/perceptual warnings ("Your vision narrows. Reality feels thin, like paper.")
- Psychological warnings ("The pressure inside your skull is unbearable. Something has to give.")
- Physical warnings ("Your hands shake. Your breathing is rapid. You're losing control.")
- Existential warnings ("It's too much. All of it. The Blight, the death, the endless ruins.")

### Phase 2: Breaking Point (100% Stress)

**Purpose:** Describe mental fracture moment
**Descriptor Count:** 9

**Examples:**

- "Your mind can't take anymore. The psychic pressure exceeds your capacity. You BREAK."
- "Everything is too much. The Choir. The Blight. The horror. You shatter like glass."
- "Darkness swallows your consciousness. Not physical darkness—mental void."

### Phase 3: System Message

**Purpose:** Mechanical prompt for Resolve Check
**Format:** System-style message with DC, outcomes

```
[SYSTEM: BREAKING POINT REACHED]
Your mind fractures under the weight of the Cursed Choir.
Make a WILL-based Resolve Check (DC 16) to hold yourself together.

Success: Retain sanity, but remain shaken. (Stress resets to 75%, Disoriented 2 turns)
Failure: Gain permanent Trauma. (Stress resets to 50%, Stunned 1 turn)

```

### Phase 4: Resolve Check Success (Barely Holding)

**Purpose:** Describe successful resistance to trauma
**Descriptor Count:** 6

**Examples:**

- "You claw your way back from the brink. You're still here. Still sane. Barely."
- "With monumental effort, you force the fractures closed. You hold. For now."
- "No. You refuse. You will not break. Not here. Not now."

### Phase 5: Resolve Check Failure (Trauma Acquisition)

**Purpose:** Describe permanent trauma acquisition
**Descriptor Count:** 5

**Examples:**

- "You break. Something permanent damages inside you. A scar that will never fully heal."
- "Your mind fractures. When the pieces reform, something is missing. Something is wrong."
- "You lose a part of yourself in that moment. It's gone forever."

### Phase 6: Trauma Reveal

**Purpose:** System message revealing specific trauma acquired
**Descriptor Count:** 10 (one per trauma type)

**Format:**

```
[TRAUMA ACQUIRED: Flashbacks]

Combat will trigger vivid memories of your near-death experiences.
You'll randomly freeze mid-fight, trapped in past trauma.

This scar is permanent. Only a Saga Quest can remove it.

```

---

## V. Trauma Trigger System

### Trigger Categories

### Environmental Triggers

**Purpose:** Respond to location, atmosphere, conditions
**Descriptor Count:** 15+

**Trigger Conditions:**

- **Trauma Site Return** - Returning to where trauma was acquired (+5 Stress)
- **Darkness** - Dim/dark areas trigger hypervigilance (+2-3 Stress)
- **Confined Space** - Small rooms trigger claustrophobia (+2 Stress)
- **Isolation** - Being alone triggers trust erosion, apathy (+2-4 Stress)
- **Blight Presence** - Near Blight manifestations (+3-5 Stress, reality distortion)
- **Loud Noises** - Sudden sounds trigger hypervigilance (+2 Stress, startle)

**Examples:**

- *Trauma Site:* "You're back. Here. Where it happened. The memories flood back."
- *Darkness:* "The darkness hides threats. You know it does. Every shadow could be lethal."
- *Blight:* "The Blight is near. You can feel it warping reality. Or are you warping it?"

### Combat Triggers

**Purpose:** Respond to battle conditions, threats, damage
**Descriptor Count:** 20+

**Trigger Conditions:**

- **Similar Enemy Type** - Enemy that caused trauma (+5 Stress or Stunned)
- **Low Health** - Below 25% HP triggers flashbacks (Stunned 1 turn)
- **Outnumbered** - 2+ enemies per ally (+3-4 Stress)
- **Ambushed** - Surprise round triggers hypervigilance (+3-4 Stress)
- **Ally Down** - Ally unconscious triggers guilt, erosion (+3-4 Stress)
- **Critical Hit Received** - Major damage triggers flashbacks (40% chance)

**Examples:**

- *Similar Enemy:* "A Forlorn. Just like the one that nearly killed you. Panic seizes you."
- *Low Health:* "Death is close. Too close. Your past trauma erupts. [Stunned 1 turn]"
- *Outnumbered:* "Too many enemies. Too many angles. You can't watch them all. The panic is paralyzing."

### Social Triggers

**Purpose:** Respond to interpersonal situations
**Descriptor Count:** 8+

**Trigger Conditions:**

- **Betrayal/Deception** - Detected lies trigger trust erosion (+5 Stress)
- **Crowds** - 4+ NPCs trigger hypervigilance (+4 Stress)
- **Unexpected Touch** - Physical contact triggers flinch (+3 Stress)

**Examples:**

- *Betrayal:* "They lie. Of course they lie. Everyone lies. [Trust Erosion activates]"
- *Crowd:* "Too many people. Too much stimulation. Your trauma overwhelms you."
- *Touch:* "They touch your shoulder. You flinch violently, hand moving to your weapon."

---

## VI. Recovery System

### Temporary Suppression: Cognitive Stabilizer

### Application (Descriptor Count: 10)

**Purpose:** Describe chemical trauma suppression
**Duration:** 1 combat encounter

**Generic Application:**

- "You inject the Cognitive Stabilizer. The chemicals flood your system."
- "For a brief time, the trauma recedes. Your mind clears. The pain dulls."
- "Artificial calm settles over you. You know it won't last, but for now... relief."

**Trauma-Specific Application:**

- *Flashbacks:* "The Stabilizer dampens the memories. They're still there, but distant. Muted."
- *Battle Tremors:* "Your hands steady. The tremors fade. For one combat, you have control again."
- *Auditory Hallucinations:* "The Choir's volume decreases. The voices quiet. Silence. Beautiful silence."

### Duration (Descriptor Count: 4)

**Purpose:** Remind player of temporary nature

**Examples:**

- "The Stabilizer's effects last for one combat encounter."
- "You have temporary reprieve from your psychological scars."
- "For now, you're free. The trauma is suppressed, locked away behind chemical barriers."

### Wearing Off (Descriptor Count: 7)

**Purpose:** Describe trauma return with rebound stress

**Generic Wearing Off:**

- "The Stabilizer fades. Your trauma comes roaring back, worse than before. [+3 Psychic Stress]"
- "Reality crashes back in. The trauma was there all along, just hidden."

**Trauma-Specific Wearing Off:**

- *Flashbacks:* "The memories surge back. You're vulnerable to flashbacks again."
- *Battle Tremors:* "Your hands start shaking again. The tremors return, as persistent as ever."
- *Auditory Hallucinations:* "The Choir returns. Louder. Angrier. As if offended by your attempt to silence them."

### Permanent Removal: Saga Quests

### Quest Beginning (Descriptor Count: 5)

**Purpose:** Frame trauma removal as difficult journey

**Examples:**

- "You've decided to confront your trauma. This won't be easy."
- "To heal this wound, you must return to where it began."
- "The only way out is through. You must confront the source of your trauma."

### Quest During (Descriptor Count: 5)

**Purpose:** Describe painful confrontation process

**Examples:**

- "The memories are overwhelming, but you push forward."
- "You're reliving the trauma, but this time, you're in control."
- "Each step toward healing tears open old wounds. But they must be reopened to properly heal."

### Quest Completion (Descriptor Count: 6)

**Purpose:** System message confirming trauma removal

**Generic Completion:**

```
[TRAUMA REMOVED]

You've confronted your trauma. Faced it. Processed it.
The scar remains, but it no longer controls you.
You are free.

```

**Trauma-Specific Completion:**

```
[TRAUMA REMOVED: Flashbacks]

You've confronted the memories. They no longer have power over you.
The past is the past. It cannot trap you anymore.
You are free.

```

### Healing Moments (Descriptor Count: 8)

**Purpose:** Emotional catharsis after trauma removal

**Examples:**

- "The weight lifts. For the first time in forever, you breathe easy."
- "Tears stream down your face. Not from pain, but from relief. It's over."
- "You feel... lighter. Whole. The broken pieces have been mended."
- "You'll never forget what happened. But it no longer defines you."

---

## VII. Technical Implementation

### Database Schema

### Trauma_Descriptors Table

```sql
CREATE TABLE Trauma_Descriptors (
    descriptor_id INTEGER PRIMARY KEY,
    descriptor_name TEXT UNIQUE,
    trauma_id TEXT NOT NULL,           -- Links to Trauma.TraumaId
    trauma_name TEXT NOT NULL,         -- "[FLASHBACKS]"
    descriptor_type TEXT NOT NULL,     -- Acquisition, Manifestation, Trigger
    context_tag TEXT,                  -- Combat, Environmental, Social
    descriptor_text TEXT NOT NULL,
    progression_level INTEGER,         -- 1, 2, or 3
    mechanical_context TEXT,           -- JSON: {"effect": "stunned", "duration": 1}
    spawn_weight REAL DEFAULT 1.0,
    display_conditions TEXT,           -- JSON: {"min_stress": 50}
    tags TEXT                          -- JSON array
);

```

### Breaking_Point_Descriptors Table

```sql
CREATE TABLE Breaking_Point_Descriptors (
    descriptor_id INTEGER PRIMARY KEY,
    descriptor_name TEXT UNIQUE,
    stress_threshold_min INTEGER NOT NULL,
    stress_threshold_max INTEGER NOT NULL,
    phase TEXT NOT NULL,               -- Warning, Breaking, ResolveSuccess, etc.
    descriptor_text TEXT NOT NULL,
    spawn_weight REAL DEFAULT 1.0,
    tags TEXT
);

```

### Recovery_Descriptors Table

```sql
CREATE TABLE Recovery_Descriptors (
    descriptor_id INTEGER PRIMARY KEY,
    descriptor_name TEXT UNIQUE,
    recovery_type TEXT NOT NULL,       -- Stabilizer_Application, Quest_Completion, etc.
    trauma_id TEXT,                    -- Optional: trauma-specific
    descriptor_text TEXT NOT NULL,
    spawn_weight REAL DEFAULT 1.0,
    tags TEXT
);

```

### Trauma_Trigger_Conditions Table

```sql
CREATE TABLE Trauma_Trigger_Conditions (
    trigger_id INTEGER PRIMARY KEY,
    trigger_name TEXT UNIQUE,
    trigger_category TEXT NOT NULL,
    trigger_condition TEXT NOT NULL,
    applicable_trauma_ids TEXT NOT NULL,  -- JSON array
    thresholds TEXT,                      -- JSON: {"health_percent": 25}
    trigger_description TEXT NOT NULL,
    stress_impact TEXT,
    tags TEXT
);

```

### C# Models

### TraumaDescriptor.cs

```csharp
public class TraumaDescriptor
{
    public int DescriptorId { get; set; }
    public string DescriptorName { get; set; }
    public string TraumaId { get; set; }
    public string TraumaName { get; set; }
    public TraumaDescriptorType DescriptorType { get; set; }
    public string? ContextTag { get; set; }
    public string DescriptorText { get; set; }
    public int? ProgressionLevel { get; set; }
    public string? MechanicalContext { get; set; }  // JSON
    public float SpawnWeight { get; set; }
    public string? DisplayConditions { get; set; }  // JSON
    public string? Tags { get; set; }  // JSON

    public bool ShouldDisplay(int currentStress, int? currentProgression, List<string>? activeTraumas);
}

```

### BreakingPointDescriptor.cs

```csharp
public class BreakingPointDescriptor
{
    public int DescriptorId { get; set; }
    public string DescriptorName { get; set; }
    public int StressThresholdMin { get; set; }
    public int StressThresholdMax { get; set; }
    public BreakingPointPhase Phase { get; set; }
    public string DescriptorText { get; set; }
    public float SpawnWeight { get; set; }
    public string? Tags { get; set; }  // JSON
}

```

---

## VIII. Integration with Trauma Economy

### Stress → Breaking Point Flow

```
Player Stress: 0-74 → Normal gameplay
             ↓
Player Stress: 75-99 → Warning descriptors (30% per turn)
             ↓
Player Stress: 100 → BREAKING POINT
             ↓
Display Breaking descriptor → System Message → Resolve Check (DC 16)
             ↓                                      ↓
    Success (75% stress)                     Failure (50% stress)
    Disoriented 2 turns                      Acquire Trauma
    ResolveSuccess descriptor                ResolveFailure descriptor
                                             TraumaReveal descriptor

```

### Trauma Manifestation Flow

```
Each Turn/Action:
  ↓
For each active trauma:
  ↓
Check manifestation chance (trauma-dependent)
  ↓
If triggered:
  ↓
Query Trauma_Descriptors (trauma_id, Manifestation, context, progression_level)
  ↓
Select descriptor (weighted random by spawn_weight)
  ↓
Display descriptor text
  ↓
Apply mechanical effect from mechanical_context JSON
  ↓
Update game state (Stunned, stress gain, penalties, etc.)

```

### Trigger Evaluation Flow

```
Game Event (low health, darkness, enemy encounter, etc.):
  ↓
Query Trauma_Trigger_Conditions for matching trigger_condition
  ↓
Check player has applicable trauma (from applicable_trauma_ids)
  ↓
Evaluate thresholds (health_percent, light_level, etc.)
  ↓
If conditions met:
  ↓
Query Trauma_Descriptors (trauma_id, Trigger, context_tag)
  ↓
Display trigger descriptor
  ↓
Apply stress impact or status effect

```

---

## IX. Success Criteria

### Quantitative Metrics

- [x]  **55+ Trauma Type Descriptors** (acquisition + manifestation for 10 traumas)
- [x]  **40+ Trauma Trigger Descriptors** (environmental, combat, social contexts)
- [x]  **44+ Breaking Point Descriptors** (6 phases of breaking point experience)
- [x]  **45+ Recovery Descriptors** (suppression and removal narratives)
- [x]  **Total: 180+ descriptors** providing comprehensive coverage

### Qualitative Metrics

- [x]  **Visceral Language** - Descriptors engage multiple senses, focus on first-person experience
- [x]  **Contextual Awareness** - Triggers respond to environment, combat state, social situations
- [x]  **Progressive Scaling** - Descriptors intensify with trauma progression (levels 1-3)
- [x]  **Mechanical Integration** - All descriptors link to concrete mechanical effects
- [x]  **Emotional Impact** - Recovery descriptors provide catharsis and relief

### Technical Metrics

- [x]  **Database Schema** - 4 tables created (Trauma_Descriptors, Breaking_Point_Descriptors, Recovery_Descriptors, Trauma_Trigger_Conditions)
- [x]  **C# Models** - TraumaDescriptor, BreakingPointDescriptor classes implemented
- [x]  **SQL Population** - All 5 content SQL scripts executable without errors
- [x]  **Integration Guide** - Comprehensive documentation with code examples

---

## X. Future Enhancements

### v0.38.15: Trauma Combination Effects

- Descriptors for multiple concurrent traumas
- Synergy effects (e.g., Flashbacks + Hypervigilance)
- Trauma interaction narratives

### v0.38.16: Biome-Specific Trauma Descriptors

- The Roots: Industrial decay trauma variants
- Muspelheim: Fire and heat trauma variants
- Niflheim: Cold and isolation trauma variants
- Alfheim: Choir and Blight trauma variants
- Jotunheim: Scale and corruption trauma variants

### v0.38.17: NPC Reactions to Visible Traumas

- NPCs notice Battle Tremors, Hypervigilance
- Dialogue variations based on player traumas
- Social consequences of visible psychological damage

### v0.38.18: Trauma Progression Narratives

- Detailed descriptors for trauma worsening (Level 1 → 2 → 3)
- Inflection points where trauma becomes critical
- Warning signs before progression

---

## XI. Conclusion

v0.38.14 transforms the Trauma Economy System from mechanical penalties into lived psychological horror. With 180+ descriptors covering acquisition, manifestation, triggering, and recovery, players will **feel** their traumas as visceral, moment-to-moment gameplay experiences.

**Key Achievements:**

- Trauma is no longer abstract—it manifests tangibly in combat, exploration, and social interactions
- Breaking Points are dramatic, emotional moments rather than dry mechanical checks
- Recovery (temporary and permanent) provides hope and emotional catharsis
- Contextual triggers ensure traumas respond to player situation, not just random chance

The Trauma Descriptor Library makes psychological damage **real**. Players will carry their scars, fight through their flashbacks, and ultimately—through Saga Quests—find healing. This is trauma-informed design that respects the weight of psychological horror while providing paths to recovery.

**Trauma shapes the journey. Recovery completes it.**

---

**END SPECIFICATION v0.38.14**