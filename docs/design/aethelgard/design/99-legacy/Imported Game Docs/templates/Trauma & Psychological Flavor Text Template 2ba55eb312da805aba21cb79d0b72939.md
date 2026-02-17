# Trauma & Psychological Flavor Text Template

Parent item: Rune and Rust: Flavor Text Template Library (Rune%20and%20Rust%20Flavor%20Text%20Template%20Library%202ba55eb312da808ea34ef7299503f783.md)

This template guides the creation of flavor text for the trauma system—stress accumulation, breaking points, trauma manifestations, and recovery. The psychological system is core to Rune and Rust's horror atmosphere.

---

## Overview

| Category | Purpose | Trigger |
| --- | --- | --- |
| **Stress Accumulation** | Building tension | Stressful events |
| **Breaking Points** | Stress overflow | Reaching 100% stress |
| **Trauma Acquisition** | Gaining trauma | Breaking point failure |
| **Trauma Manifestation** | Ongoing effects | Trauma triggers |
| **Trauma Triggers** | Context activation | Environmental/situational |
| **Recovery** | Healing trauma | Rest, treatment, time |

---

## Template Structure

```
PSYCHOLOGICAL: [Stress | BreakingPoint | Acquisition | Manifestation | Trigger | Recovery]
Trauma Category: [Fear | Isolation | Paranoia | Obsession | Dissociation | Corruption]
Phase: [Warning | Breaking | Resolution]
Intensity: [Mild | Moderate | Strong | Extreme]
Trigger Context: [Environmental, Social, Combat, etc.]
Tags: ["Tag1", "Tag2"]

SYSTEM MESSAGE: [Mechanical notification to player]
INTERNAL TEXT: [Character's internal experience]
EXTERNAL TEXT: [Observable behavior/symptoms]
RESOLUTION TEXT: [How this phase ends/transitions]

```

---

## TRAUMA CATEGORIES

### Category Definitions

| Category | Core Fear | Common Triggers | Manifestation Type |
| --- | --- | --- | --- |
| **Fear** | Death, danger, horror | Combat, monsters, darkness | Panic, flight, paralysis |
| **Isolation** | Abandonment, loneliness | Separation, silence, vast spaces | Withdrawal, clinginess, despair |
| **Paranoia** | Betrayal, being watched | NPCs, confined spaces, Servitors | Suspicion, aggression, delusion |
| **Obsession** | Loss of control, incompleteness | Patterns, items, numbers | Compulsion, fixation, ritual |
| **Dissociation** | Reality, identity, existence | Stress overload, Alfheim, trauma | Detachment, depersonalization |
| **Corruption** | Jotun influence, forbidden knowledge | Jotun-Reader abilities, artifacts | Physical/mental changes |

---

## STRESS ACCUMULATION

### Stress Sources

| Source | Typical Stress | Example |
| --- | --- | --- |
| Combat start | +1d4 | Entering dangerous situation |
| Taking damage | +1 per 5 HP lost | Physical harm |
| Witnessing horror | +1d6 | Corpses, monsters, atrocities |
| Isolation | +1/hour | Separated from party |
| Environmental | +1d4 | Oppressive environments |
| Supernatural | +2d4 | Alfheim, Cursed Choir, paradox |

### Stress Accumulation Templates

### Combat Stress

```
STRESS: Combat Entry
Source: Entering combat
Amount: +1d4

MILD (+1-2):
- Internal: "Your heart rate spikes. Combat awareness sharpens."
- External: "You ready yourself, tension visible in your stance."

MODERATE (+3-4):
- Internal: "Fear and adrenaline war within you. Stay focused!"
- External: "Your breathing quickens. Your grip tightens on your weapon."

SYSTEM: "[Character] gains [X] Stress from combat. [Current: X/100]"

```

### Horror Stress

```
STRESS: Witnessing Horror
Source: Corpses, monsters, atrocities
Amount: +1d6

MILD (+1-2):
- Internal: "You've seen worse. Haven't you? You try not to think about it."
- External: "A slight grimace, quickly suppressed."

MODERATE (+3-4):
- Internal: "That image will haunt you. You know it already."
- External: "You turn away, bile rising. Deep breaths."

SEVERE (+5-6):
- Internal: "No. No no no. This is wrong. This is all wrong."
- External: "You stagger, hand over mouth. Someone catches your arm."

SYSTEM: "[Character] gains [X] Stress from horror. [Current: X/100]"

```

### Environmental Stress

```
STRESS: Oppressive Environment
Source: Sustained environmental pressure
Amount: +1d4/hour in oppressive areas

THE_ROOTS:
- Internal: "The weight of ages presses down. Everything is dying here."
- External: "Shoulders slump. The oppression is visible."

NIFLHEIM:
- Internal: "The cold isn't just temperature. It's despair made physical."
- External: "They're moving slower. The cold is in their spirit."

ALFHEIM:
- Internal: "Reality bends and you bend with it. What's real anymore?"
- External: "They're staring at nothing. The Choir has their attention."

SYSTEM: "[Character] gains [X] Stress from environment. [Current: X/100]"

```

### Stress Thresholds

| Threshold | Description | Effects |
| --- | --- | --- |
| 0-25% | Calm | No penalties |
| 26-50% | Stressed | Minor narrative effects |
| 51-75% | Anxious | -1 to WILL checks |
| 76-99% | Breaking | -2 to all checks, Warning phase begins |
| 100% | Breaking Point | Breaking Point triggered |

```
STRESS THRESHOLD: Warning (75-99%)
System: "WARNING: [Character]'s stress is critical! [X/100]"

Internal: "Everything is too much. You're fraying at the edges."
External: "They're barely holding it together. Something has to give."

Effects:
- "Your hands shake. Focus becomes difficult. [-2 to all checks]"
- "Every shadow seems to move. Every sound could be a threat."

```

---

## BREAKING POINTS

### Breaking Point Phases

| Phase | Duration | What Happens |
| --- | --- | --- |
| Warning | 75-99% stress | Impending collapse visible |
| Breaking | At 100% | Actual breakdown |
| Resolution | After roll | Success or trauma acquisition |

### Breaking Point Templates

### Warning Phase

```
BREAKING POINT: Warning Phase
Trigger: Stress reaches 75%+

SYSTEM MESSAGE:
"⚠️ WARNING: [Character] is approaching a breaking point!
Stress: [X]/100 | Danger Zone"

INTERNAL:
- "The walls are closing in. You can feel yourself cracking."
- "Too much. It's all too much. You can't—you won't—you—"
- "Something is wrong with you. You can feel it coming."

EXTERNAL:
- "Their eyes dart constantly. Every noise makes them flinch."
- "They're muttering to themselves. The words don't quite make sense."
- "Trembling hands. Rapid breathing. The signs of imminent collapse."

ALLY AWARENESS:
- "[Ally] notices [Character]'s distress. Are they going to break?"

```

### Breaking Phase

```
BREAKING POINT: Breaking
Trigger: Stress reaches 100%

SYSTEM MESSAGE:
"💔 BREAKING POINT: [Character] has reached their limit!
WILL check required (DC 14) or acquire trauma."

THE MOMENT:
- "The final straw. Something inside you snaps."
- "You can't take it anymore. The dam breaks."
- "Everything comes crashing down at once."

EXTERNAL COLLAPSE:
Fear: "They scream—raw, primal terror—and bolt!"
Isolation: "They crumple, hands over ears, rocking back and forth."
Paranoia: "They spin, weapon raised, pointing at everyone. 'WHICH ONE OF YOU?!'"
Obsession: "They freeze, then begin a frantic, repetitive action."
Dissociation: "They go blank. Lights on, nobody home."
Corruption: "Something twists in them. You can almost see it."

```

### Resolution Phase - Success

```
BREAKING POINT: Resolution (Success)
Result: WILL check succeeded

SYSTEM MESSAGE:
"✓ [Character] held it together! Stress reduced to 50%."

RECOVERY:
- "You master yourself. Barely. The moment passes."
- "Through sheer will, you pull back from the edge."
- "Not today. Not today. You refuse to break."

AFTERMATH:
- "You're exhausted. Emptied. But still functional."
- "The crisis passes, leaving you drained but intact."
- "Close. Too close. You need to be more careful."

```

### Resolution Phase - Failure

```
BREAKING POINT: Resolution (Failure)
Result: WILL check failed

SYSTEM MESSAGE:
"✗ [Character] has broken! Acquiring trauma...
Rolling trauma type based on context..."

COLLAPSE:
- "You couldn't hold it. The breaking is complete."
- "Something inside you tears. It won't heal the same."
- "The damage is done. You're different now."

TRANSITION TO TRAUMA:
- "In the aftermath, you realize something fundamental has changed."
- "When you finally come back to yourself, you know you've been marked."

```

---

## TRAUMA ACQUISITION

### Acquisition Templates by Category

### Fear Trauma

```
TRAUMA ACQUISITION: Fear
Context: Terror-based breaking point

SYSTEM MESSAGE:
"[Character] has acquired: [Specific Fear Trauma]
Trigger: [Trigger condition]"

ACQUISITION MOMENT:
- "The terror burned itself into you. You'll never forget. You'll never stop fearing."
- "Some fears go away. This one took root in your soul."
- "The memory crystallizes into something permanent. A new fear is born."

SPECIFIC FEARS:
- Scotophobia (Darkness): "The dark is alive now. It moves when you're not looking."
- Claustrophobia (Enclosed Spaces): "The walls. They're too close. Always too close."
- Necrophobia (The Dead): "The dead don't stay dead here. You know that now."

REVELATION:
- "[Character] now has a persistent fear of [trigger]. This is trauma."
- "Some part of them is forever changed. Fear has carved a new shape in their mind."

```

### Isolation Trauma

```
TRAUMA ACQUISITION: Isolation
Context: Abandonment-based breaking point

ACQUISITION MOMENT:
- "Alone. You were alone and now you'll never stop feeling alone."
- "They left you. Everyone leaves eventually. You learned that forever."
- "The silence got inside you. It lives there now."

SPECIFIC TRAUMAS:
- Abandonment Fear: "You cannot be alone. You won't survive it again."
- Codependency: "You need them. All of them. Without them you're nothing."
- Mutism: "Words failed you when you needed them. Now they fail you always."

REVELATION:
- "[Character] carries the wound of isolation now. It won't heal cleanly."

```

### Paranoia Trauma

```
TRAUMA ACQUISITION: Paranoia
Context: Betrayal/surveillance-based breaking point

ACQUISITION MOMENT:
- "Trust is a luxury you can no longer afford."
- "They were watching. They're always watching. You see that now."
- "Everyone is suspect. Everyone has secrets. Everyone is dangerous."

SPECIFIC TRAUMAS:
- Persecution Complex: "They're out to get you. All of them. Specifically you."
- Conspiracy Ideation: "It's all connected. You're the only one who sees the pattern."
- Hostile Attribution: "Every kindness is a trap. Every smile hides teeth."

REVELATION:
- "[Character]'s ability to trust has been fundamentally damaged."

```

### Obsession Trauma

```
TRAUMA ACQUISITION: Obsession
Context: Control/pattern-based breaking point

ACQUISITION MOMENT:
- "The ritual formed itself. You didn't choose it. It chose you."
- "This is the only way to be safe. You know that now."
- "The pattern must be maintained. Must. MUST."

SPECIFIC TRAUMAS:
- Compulsive Behavior: "Touch the doorway three times. Always three times."
- Hoarding: "Everything is valuable. Everything might be needed."
- Number Fixation: "The numbers matter. Count them. Always count them."

REVELATION:
- "[Character] has developed an obsessive coping mechanism."

```

### Dissociation Trauma

```
TRAUMA ACQUISITION: Dissociation
Context: Reality/identity-based breaking point

ACQUISITION MOMENT:
- "You watched yourself break from somewhere far away."
- "Reality and you have become... loosely affiliated."
- "Are you still you? You're not sure anymore."

SPECIFIC TRAUMAS:
- Depersonalization: "Your body is a vehicle you're piloting. Poorly."
- Derealization: "Is any of this real? Does it matter?"
- Fragmented Identity: "You're not sure which you is the real one anymore."

REVELATION:
- "[Character]'s grip on reality has been permanently loosened."

```

### Corruption Trauma

```
TRAUMA ACQUISITION: Corruption
Context: Jotun influence/forbidden knowledge

ACQUISITION MOMENT:
- "The knowledge carved something new inside you."
- "You touched something ancient. It touched back."
- "The corruption is inside you now. Growing."

SPECIFIC TRAUMAS:
- Whispers: "They speak to you. The ancient ones. They have such plans."
- Physical Mark: "Your body is changing. Slowly. Horribly. Wonderfully?"
- Forbidden Insight: "You see things now. Things no mortal should see."

REVELATION:
- "[Character] has been touched by forces beyond mortal understanding."

```

---

## TRAUMA MANIFESTATIONS

### Manifestation Types

| Type | When | Description |
| --- | --- | --- |
| Passive | Always | Constant background effect |
| Triggered | Context | Activates in specific situations |
| Active | By choice | Can be engaged with deliberately |

### Manifestation Templates

### Fear Manifestations

```
TRAUMA MANIFESTATION: Fear (Scotophobia)
Trigger: Darkness, light source extinguishing

PASSIVE:
- "You're always aware of light sources. Always."
- "Even in lit areas, you watch the shadows."

TRIGGERED:
Onset: "The light fails. Your breath stops. It's coming."
Escalation: "The darkness is alive. You know it. It's MOVING."
Peak: "RUN. HIDE. LIGHT. YOU NEED LIGHT NOW!"

MECHANICAL: "[Character] is panicking! -4 to all checks until light restored!"

RECOVERY: "Light returns. You gasp, trembling. Safe. For now."

```

### Paranoia Manifestations

```
TRAUMA MANIFESTATION: Paranoia (Persecution Complex)
Trigger: Strangers, enclosed spaces with others, being observed

PASSIVE:
- "Everyone you meet is evaluated. Friend? Foe? Probably foe."
- "Your back is never to a door. Never."

TRIGGERED:
Onset: "They're looking at you. Why are they looking at you?"
Escalation: "This is a setup. It has to be. They're all in on it."
Peak: "YOU WON'T GET ME! I KNOW WHAT YOU'RE DOING!"

MECHANICAL: "[Character] is paranoid! Cannot benefit from ally abilities!"

RECOVERY: "The threat passes. Or does it? You're watching them now."

```

### Obsession Manifestations

```
TRAUMA MANIFESTATION: Obsession (Compulsive Ritual)
Trigger: Specific situations, interrupted rituals

PASSIVE:
- "The ritual must be performed. Morning. Evening. Always."
- "Deviation from the pattern causes intense anxiety."

TRIGGERED (ritual interrupted):
Onset: "No. You didn't finish. You have to finish."
Escalation: "START OVER. IT WASN'T RIGHT."
Peak: "YOU CAN'T MOVE ON UNTIL IT'S DONE PROPERLY!"

MECHANICAL: "[Character] MUST complete ritual or suffer +2d6 Stress!"

RECOVERY: "The ritual completes. Order is restored. You can function again."

```

---

## TRAUMA TRIGGERS

### Trigger Categories

| Category | Examples |
| --- | --- |
| Environmental | Darkness, heights, water, fire, enclosed spaces |
| Social | Crowds, strangers, authority, abandonment |
| Sensory | Specific sounds, smells, textures |
| Situational | Combat, isolation, helplessness |
| Supernatural | Specific creatures, magic types, locations |

### Trigger Templates

```
TRAUMA TRIGGER: [Trigger Name]
Trauma: [Associated trauma]
Activation: [What causes it]

DETECTION:
- "[Character] enters [trigger zone/situation]."
- "The trigger is present: [specific element]."

RESPONSE REQUIRED:
- "WILL check DC [X] to resist manifestation."

SUCCESS:
- "You feel it pulling at you, but you master yourself. For now."
- "The trigger activates, but you maintain control."

FAILURE:
- "The trigger finds its mark. [Manifestation begins]."
- "You couldn't stop it. The trauma response activates."

```

---

## RECOVERY

### Recovery Methods

| Method | Effect | Requirements |
| --- | --- | --- |
| Rest | Reduce stress | Safe location, time |
| Treatment | Progress toward trauma cure | Healer, supplies |
| Breakthrough | Resolve trauma | Narrative milestone |
| Suppression | Temporary relief | WILL check, risky |

### Recovery Templates

### Stress Recovery

```
RECOVERY: Stress Reduction
Method: Rest
Effect: -2d6 Stress per full rest

RESTING:
- "You find a moment of peace. The tension begins to drain."
- "Sleep comes reluctantly, but it comes. Your mind begins to heal."
- "A meal, a rest, a moment without fear. Stress eases."

PARTIAL:
- "The rest helps, but the wounds are still fresh. [-X Stress]"

FULL:
- "You awaken feeling... better. Not good. But better. [Stress reduced significantly]"

```

### Trauma Treatment

```
RECOVERY: Trauma Treatment
Method: Professional care, time, specific conditions
Progress: Track progress toward resolution (usually 3-5 milestones)

TREATMENT SESSION:
- "You talk about it. For the first time, you really talk about it."
- "The healer guides you through the memory. It hurts, but differently."
- "Progress is slow, but real. [Treatment milestone X/5]"

SETBACK (failed treatment roll):
- "Today was hard. You couldn't face it. [No progress]"
- "The session triggered a manifestation. [-1 progress]"

BREAKTHROUGH (final milestone):
- "Something shifts inside you. The weight... lifts. Slightly. But noticeably."
- "The trauma isn't gone, but it's smaller now. Manageable. [Trauma severity reduced]"

```

### Suppression (Risky)

```
RECOVERY: Suppression
Method: Force of will
Effect: Temporary relief, risk of worse later

ATTEMPTING:
- "You push it down. Deep down. Where it can't reach you."
- "Not now. You don't have time for this. LATER."

SUCCESS:
- "The trauma retreats. For now. [Suppressed until next trigger]"
- "You've bought yourself time, but the debt remains."

FAILURE:
- "You can't suppress it. It's too strong. [Manifestation activates]"

CONSEQUENCE (later):
- "The suppressed trauma resurfaces with compound interest. [+50% stress on next trigger]"

```

---

## WRITING GUIDELINES

### Psychological Content Principles

1. **Respectful portrayal** - mental health is serious; handle with care
2. **Player agency** - traumas affect gameplay but don't remove control
3. **Hope exists** - recovery is always possible
4. **Gradual progression** - effects escalate, giving time to react
5. **Individual expression** - same trauma manifests differently

### Tone Guidelines

- Fear should feel genuinely frightening, not silly
- Isolation should feel achingly lonely
- Paranoia should feel uncomfortably rational
- Obsession should feel compellingly necessary
- Dissociation should feel disorienting but not confusing
- Corruption should feel insidiously attractive

### Avoid

- Trivializing mental health conditions
- Making trauma "cool" or desirable
- Removing player agency without consent
- Permanent, irrecoverable states
- Real-world mental illness terminology used carelessly

---

## Quality Checklist

- [ ]  System message clearly communicates mechanical effect
- [ ]  Internal experience is visceral and immersive
- [ ]  External manifestation is observable by others
- [ ]  Trauma category is consistent throughout
- [ ]  Recovery path is mentioned or implied
- [ ]  Tone is respectful and serious
- [ ]  Player agency is preserved
- [ ]  Escalation is gradual and clear