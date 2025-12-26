---
id: SPEC-DESCRIPTORS-INTERACTION
title: "Interaction Descriptors — Objects, Examination, & Discovery"
version: 1.0
status: draft
last-updated: 2025-12-14
related-files:
  - path: "docs/07-environment/descriptors/descriptors-overview.md"
    status: Parent Specification
---

# Interaction Descriptors — Objects, Examination, & Discovery

> *"Every piece of salvage has a story. Most of them end badly."*

---

## 1. Overview

This specification defines descriptors for interactive objects, examination results, discovery moments, and salvage encounters.

### 1.1 Identity Table

| Property | Value |
|----------|-------|
| Spec ID | `SPEC-DESCRIPTORS-INTERACTION` |
| Category | Descriptor System |
| Subcategories | Objects, Examination, Discovery, Containers |
| Est. Fragments | 150+ |

### 1.2 Interaction Categories

| Category | Purpose |
|----------|---------|
| **Objects** | Interactive object descriptions |
| **Examination** | Detailed inspection results |
| **Discovery** | Hidden/secret finding moments |
| **Containers** | Loot sources and salvage |
| **Terminals** | Oracle-boxes and data-slates |

---

## 2. Object Descriptors

### 2.1 Mechanical Objects

#### Levers

| State | Descriptor |
|-------|------------|
| Up/Active | "A lever locked in the up position, mechanism still engaged" |
| Down/Inactive | "A lever pulled down, whatever it controlled now dormant" |
| Rusted/Stuck | "A lever frozen by centuries of rust. It would take force to move" |
| Unknown | "A lever. No markings. No indication of what it controls" |

#### Doors

| State | Descriptor |
|-------|------------|
| Open | "A door left ajar. What passed through didn't bother to close it" |
| Closed | "A sealed door. The mechanism might still function" |
| Locked | "The door is sealed. Whatever lock controls it isn't visible" |
| Destroyed | "A door torn from its hinges. Something wanted through badly" |
| Rusted | "A door fused shut by rust. This portal hasn't moved in centuries" |

#### Terminals (Oracle-Boxes)

| State | Descriptor |
|-------|------------|
| Active | "The oracle-box hums with power, glass face swimming with ghost-light" |
| Dormant | "The screen is dark. But the box still ticks, waiting" |
| Corrupted | "Static crawls across the display. Symbols writhe, half-formed" |
| Dead | "Nothing. The box is cold, its purpose long since ended" |
| Glitched | "The oracle-box shows things that shouldn't be. Images from when?" |

### 2.2 Container Objects

#### Salvage Crates

| State | Descriptor |
|-------|------------|
| Intact | "A sealed crate, markings still visible. Someone thought the contents worth protecting" |
| Damaged | "The crate has been breached. Whatever was here may still remain" |
| Opened | "Already searched. What's left is what wasn't worth taking" |
| Trapped | "Something about the seal is wrong. This crate was secured for a reason" |

#### Corpses

| Age | Descriptor |
|-----|------------|
| Fresh | "Blood still wet. This happened recently. Whatever did this might still be here" |
| Recent | "Days old. The smell is starting. The useful items might remain" |
| Old | "Bones and dust. This one has been here longer than you've been alive" |
| Ancient | "Mummified remains. They've been here since before the Glitch" |
| Forlorn | "A Forlorn, finally still. Whatever humanity remained is gone now" |
| Stripped | "Already looted. Whoever got here first took what mattered" |

#### Resource Nodes

| Type | Descriptor |
|------|------------|
| Ore Vein | "Metal gleams in the stone. Someone has to dig it out" |
| Salvage Pile | "Loose components, probably functional. Worth sifting through" |
| Scrap Cluster | "Junk, mostly. But junk can become armor. Weapons. Life" |
| Rare Find | "Your heart skips. This is the good stuff. Finally" |

---

## 3. Examination Descriptors

### 3.1 Perception-Based Examination

#### WITS Check — Success

| Margin | Descriptor |
|--------|------------|
| Low | "You notice something. There's more here than appears at first glance" |
| Medium | "Details emerge. The mechanism here isn't quite what it seems" |
| High | "Everything becomes clear. You see how this works, what it hides, what it means" |
| Critical | "You understand this object in ways its makers might not have intended" |

#### WITS Check — Failure

| Severity | Descriptor |
|----------|------------|
| Near Miss | "Something... you're missing something. You can feel it" |
| Failure | "Nothing stands out. It's just what it appears to be. Or so you think" |
| Bad | "You stare at it. Nothing. Just an object" |
| Critical Fail | "You have no idea what you're looking at. Worse, you're confident you do" |

### 3.2 Object-Specific Examination

#### Examining Technology

| Discovery | Descriptor |
|-----------|------------|
| Function | "The mechanism reveals its purpose. Here is where you interact" |
| Status | "You assess its condition. Damaged, but repairable. Maybe" |
| Origin | "Dvergr-make. The quality is unmistakable, even corrupted" |
| Danger | "Something is wrong here. This object is trapped, or worse" |

#### Examining Bodies

| Discovery | Descriptor |
|-----------|------------|
| Cause | "The wound tells a story. They didn't die peacefully" |
| Identity | "Clan markings. You recognize the faction. This was one of yours" |
| Loot | "They carried something useful. Still do, technically" |
| Warning | "Whatever killed them left marks. Now you know what to fear" |

#### Examining Environments

| Discovery | Descriptor |
|-----------|------------|
| Hidden | "There — a seam where there shouldn't be one. Concealed" |
| Trap | "The dust pattern is wrong. Something has been placed here recently" |
| Path | "Air moves. Faintly. There's another way through somewhere" |
| Hazard | "The stains on the floor tell you where not to step" |

---

## 4. Discovery Descriptors

### 4.1 Secret Discovery

| Intensity | Descriptor |
|-----------|------------|
| Minor | "You find a hidden compartment. Someone wanted this overlooked" |
| Minor | "Behind the panel — a cache. Small, but valuable" |
| Moderate | "A concealed passage. Not on any map you've seen" |
| Moderate | "The wall is false. Beyond it... something worth hiding" |
| Major | "A sealed vault. Whatever they hid here, they MEANT to hide it" |
| Major | "No one has been here. No one. Until you" |

### 4.2 Lore Discovery

| Type | Descriptor |
|------|------------|
| Data-Slate | "A data-slate, still readable. Someone left a record" |
| Data-Slate | "Words from the past. The dead speak, if you're willing to listen" |
| Inscription | "Runes carved into the wall. Not decoration — instruction. Or warning" |
| Inscription | "Ancient words. You understand some of them. Wish you didn't" |
| Art | "A mural, faded but visible. This is what the world looked like, before" |
| Note | "Handwriting. Personal. This was meant for someone specific" |

### 4.3 Danger Discovery

| Type | Descriptor |
|------|------------|
| Trap | "Your instincts scream. Something here is waiting to kill you" |
| Trap | "The trigger mechanism is barely visible. One more step and..." |
| Ambush | "Movement. At the edge of vision. You're not alone" |
| Ambush | "Too quiet. This is a kill-box. You're in it" |
| Hazard | "The air is wrong. Toxic. You need to leave or stop breathing" |
| Hazard | "The floor is weak. One more person and it gives way" |

---

## 5. Container Interaction Descriptors

### 5.1 Opening Actions

| Type | Descriptor |
|------|------------|
| Unlock | "The lock yields. The contents are yours now" |
| Force | "Metal groans, then gives. Brute force has its uses" |
| Bypass | "The mechanism clicks. You're in without triggering anything" |
| Fail | "It won't budge. Whatever sealed this meant business" |
| Trap Trigger | "Click. That wasn't the lock. That was something else" |

### 5.2 Loot Discovery

| Quality | Descriptor |
|---------|------------|
| Poor | "Junk, mostly. But junk is better than nothing" |
| Poor | "Someone else got here first. Leftovers only" |
| Average | "Usable supplies. Nothing remarkable, but nothing worthless" |
| Average | "This will keep you alive a little longer. Good enough" |
| Good | "Quality goods. Someone lost this; you gained it" |
| Good | "Real resources. This trip just paid for itself" |
| Excellent | "Your breath catches. This is worth the bruises" |
| Excellent | "The good stuff. Finally. Someone is watching over you" |
| Jackpot | "You've found something IMPORTANT. This changes things" |

---

## 6. Skill-Specific Interactions

### 6.1 Crafting Examination

| Trade | Descriptor |
|-------|------------|
| Bodging | "You assess the salvage. With the right parts, this could be fixed" |
| Bodging | "Broken, but not useless. Your hands already know what to do" |
| Field Medicine | "Medical supplies. Some expired, some contaminated, some... usable" |
| Field Medicine | "You inventory what heals and what harms. The line is thin" |
| Runeforging | "The inscriptions are degraded but readable. Power remains here" |
| Runeforging | "Rune-material. The substrate accepts inscription — if you're careful" |

### 6.2 Specialization-Specific

| Spec | Descriptor |
|------|------------|
| Jötun-Reader | "The mechanism is Old World — pre-Glitch architecture. You recognize the patterns" |
| Jötun-Reader | "This is Giant-tech. Original. Unmolested since the Glitch" |
| Ruin Stalker | "You evaluate the obstacles. Routes of entry, points of exit. Professional assessment" |
| Ruin Stalker | "Trap-lines. Patrol patterns. This place was secured. By whom?" |

---

## 7. Environmental Interactions

### 7.1 Climbing/Traversal

| Success | Descriptor |
|---------|------------|
| Easy | "You pull yourself up. Simple" |
| Standard | "Handholds present themselves. You ascend with care" |
| Difficult | "The climb demands everything. But you make it" |
| Fail | "Your grip fails. Gravity has opinions" |

### 7.2 Disabling/Repair

| Action | Descriptor |
|--------|------------|
| Disable Trap | "The mechanism is neutralized. Safe, now" |
| Repair | "With improvised tools and stubbornness, you make it work again" |
| Sabotage | "It won't function for anyone else now. Your work here is done" |
| Catastrophic | "Something breaks. Spectacularly. Permanently" |

---

## 8. Implementation Status

| Category | Types | Fragments |
|----------|-------|-----------|
| Objects | 20+ object types | 50+ |
| Examination | 3 categories | 30+ |
| Discovery | 3 types | 25+ |
| Containers | Opening + loot | 25+ |
| Skill-Specific | 5 trades/specs | 20+ |
| **Total** | — | **150+** |

---

## 9. Related Specifications

| Spec | Relationship |
|------|--------------|
| [Descriptor Overview](overview.md) | Parent framework |
| [Smart Commands](../../08-ui/smart-commands.md) | Command integration |
| [Inventory UI](../../08-ui/inventory-ui.md) | Loot display |

---

## 10. Changelog

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-14 | Initial specification |
