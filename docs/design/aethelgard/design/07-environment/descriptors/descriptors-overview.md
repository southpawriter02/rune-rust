---
id: SPEC-DESCRIPTORS-OVERVIEW
title: "Descriptor System — Master Framework"
version: 1.0
status: draft
last-updated: 2025-12-14
related-files:
  - path: "docs/07-environment/room-engine/descriptors.md"
    status: Reference (Room-specific implementation)
  - path: "docs/07-environment/descriptors/entity.md"
    status: Active
  - path: "docs/07-environment/descriptors/sensory.md"
    status: Active
  - path: "docs/07-environment/descriptors/psychological.md"
    status: Active
  - path: "docs/07-environment/descriptors/combat.md"
    status: Active
  - path: "docs/07-environment/descriptors/interaction.md"
    status: Active
---

# Descriptor System — Master Framework

> *"The world speaks in rust and whispers. Learn to hear it."*

---

## 1. Overview

The **Descriptor System** generates unique, contextually-appropriate text for all game elements — rooms, creatures, characters, combat, and psychological states. This document serves as the master framework; individual categories have dedicated specifications.

### 1.1 Identity Table

| Property | Value |
|----------|-------|
| Spec ID | `SPEC-DESCRIPTORS-OVERVIEW` |
| Category | Core System |
| Total Fragments | 1,500+ |
| Categories | 7 |

### 1.2 Design Philosophy

| Principle | Application |
|-----------|-------------|
| **Sensory First** | Describe what characters sense, not what they know |
| **Gritty & Visceral** | Concrete, immediate, physical details |
| **Medieval Vocabulary** | Inhabitants describe effects, not mechanisms |
| **Biome Coherence** | Each realm has distinct sensory signature |
| **Combinatorial Variety** | Mix-and-match for millions of unique outputs |

---

## 2. Descriptor Categories

| Spec | Path | Contents |
|------|------|----------|
| **Entity** | [entity.md](entity.md) | Races, NPCs, creatures, factions, specializations |
| **Sensory** | [sensory.md](sensory.md) | Sound, smell, sight, touch, taste |
| **Environment** | [../room-engine/descriptors.md](../room-engine/descriptors.md) | Rooms, biomes, hazards, features |
| **Psychological** | [psychological.md](psychological.md) | Stress, trauma, corruption manifestations |
| **Combat** | [combat.md](combat.md) | Actions, damage, effects, outcomes |
| **Interaction** | [interaction.md](interaction.md) | Objects, examination, discovery |

---

## 3. Three-Tier Composition Model

All descriptors follow a three-tier composition:

```
┌─────────────────────────────────────────────────────────────┐
│ TIER 1: Base Templates                                      │
│   Category-agnostic archetypes with placeholder tokens      │
│   Example: "{Race}_Appearance_Base"                         │
└─────────────────────────────────────────────────────────────┘
                          +
┌─────────────────────────────────────────────────────────────┐
│ TIER 2: Thematic Modifiers                                  │
│   Context-specific variations (biome, faction, condition)   │
│   Example: "[Frost-Touched]", "[Battle-Scarred]"            │
└─────────────────────────────────────────────────────────────┘
                          =
┌─────────────────────────────────────────────────────────────┐
│ TIER 3: Composite Descriptors                               │
│   Final generated text with placeholders filled             │
│   Example: "A frost-touched Dvergr with crystalline veins"  │
└─────────────────────────────────────────────────────────────┘
```

---

## 4. Placeholder Token System

### 4.1 Universal Tokens

| Token | Category | Example |
|-------|----------|---------|
| `{Subject}` | Target of description | "the Dvergr", "the corridor" |
| `{Modifier}` | Thematic modifier name | "Rusted", "Frost-Touched" |
| `{Modifier_Adj}` | Modifier as adjective | "corroded", "ice-veined" |
| `{Modifier_Detail}` | Modifier detail fragment | "shows centuries of decay" |
| `{Biome}` | Current biome | "Niflheim", "Muspelheim" |

### 4.2 Sensory Tokens

| Token | Sense | Example |
|-------|-------|---------|
| `{Sound}` | Auditory | "grinding gears", "distant thunder" |
| `{Smell}` | Olfactory | "ozone and copper", "rot" |
| `{Sight}` | Visual | "flickering shadows", "rust streaks" |
| `{Touch}` | Tactile | "cold iron", "slick condensation" |
| `{Taste}` | Gustatory | "ash on the wind", "copper blood" |

### 4.3 Entity Tokens

| Token | Category | Example |
|-------|----------|---------|
| `{Race}` | Race name | "Dvergr", "Dökkálfar" |
| `{Faction}` | Faction affiliation | "Rust-Clan", "Iron-Bane" |
| `{Specialization}` | Character spec | "Berserkr", "Bone-Setter" |
| `{Condition}` | Physical state | "wounded", "exhausted" |

---

## 5. Selection Algorithm

### 5.1 Weighted Random Selection

```csharp
public string SelectDescriptor(
    string category,
    string[] contextTags,
    Random rng)
{
    var candidates = _descriptors
        .Where(d => d.Category == category)
        .Where(d => d.Tags.Intersect(contextTags).Any())
        .ToList();
    
    var totalWeight = candidates.Sum(d => d.Weight);
    var roll = rng.NextDouble() * totalWeight;
    
    foreach (var descriptor in candidates)
    {
        roll -= descriptor.Weight;
        if (roll <= 0) return descriptor.Text;
    }
    
    return candidates.LastOrDefault()?.Text ?? "";
}
```

### 5.2 Context Filtering

Descriptors are filtered by:

| Context | Filter |
|---------|--------|
| **Biome** | `biome_affinity` matches current realm |
| **Danger Level** | `intensity` matches danger tier |
| **Time of Day** | `time_of_day` if applicable |
| **Visibility** | `light_level` for visual descriptors |
| **Tags** | `required_tags` for specific contexts |

---

## 6. Database Schema

### 6.1 Core Tables

| Table | Purpose | Est. Records |
|-------|---------|--------------|
| `descriptors` | Master descriptor storage | 1,500+ |
| `descriptor_templates` | Tier 1 base templates | 100+ |
| `descriptor_modifiers` | Tier 2 thematic modifiers | 50+ |
| `descriptor_composites` | Tier 3 cached composites | Generated |

### 6.2 Descriptor Record

```sql
CREATE TABLE descriptors (
    id UUID PRIMARY KEY,
    category TEXT NOT NULL,        -- 'Entity', 'Sensory', 'Combat', etc.
    subcategory TEXT NOT NULL,     -- 'Race', 'Sound', 'Hit', etc.
    text TEXT NOT NULL,            -- The descriptor text
    weight REAL DEFAULT 1.0,       -- Selection weight
    intensity TEXT,                -- 'Subtle', 'Moderate', 'Oppressive'
    biome_affinity TEXT[],         -- Applicable biomes
    required_tags TEXT[],          -- Must match context
    excluded_tags TEXT[],          -- Cannot match context
    time_of_day TEXT,              -- 'Day', 'Night', NULL
    created_at TIMESTAMP DEFAULT NOW()
);

CREATE INDEX idx_descriptors_category ON descriptors(category, subcategory);
CREATE INDEX idx_descriptors_biome ON descriptors USING GIN(biome_affinity);
```

---

## 7. IDescriptorService Interface

```csharp
public interface IDescriptorService
{
    // Single descriptor selection
    string GetDescriptor(string category, string subcategory, DescriptorContext context);
    
    // Multiple descriptors for composition
    IReadOnlyList<string> GetDescriptors(
        string category, 
        int count, 
        DescriptorContext context);
    
    // Templated composition
    string ComposeDescription(
        string templateName,
        Dictionary<string, string> tokens,
        DescriptorContext context);
    
    // Category queries
    IReadOnlyList<DescriptorTemplate> GetTemplates(string category);
    IReadOnlyList<ThematicModifier> GetModifiers(string biome);
}

public record DescriptorContext(
    string Biome,
    int DangerLevel,
    LightLevel Light,
    string[] Tags
);
```

---

## 8. Writing Guidelines

### 8.1 Voice & Tone

| Do | Don't |
|----|-------|
| "Rust streaks like old blood" | "Oxidation is visible" |
| "The cold bites at your fingers" | "It's cold" |
| "Something large moves in shadow" | "An enemy is detected" |
| "The oracle-box shudders" | "The computer displays" |

### 8.2 Sensory Hierarchy

1. **Sight** — What do they see first?
2. **Sound** — What breaks the silence?
3. **Smell** — What does the air carry?
4. **Touch** — What do surfaces feel like?
5. **Taste** — Reserved for extreme conditions

### 8.3 Length Guidelines

| Context | Length | Example |
|---------|--------|---------|
| **Brief** | 5-15 words | Status effect tooltip |
| **Standard** | 15-35 words | Room feature description |
| **Extended** | 35-75 words | Full room description |
| **Narrative** | 75+ words | Story moments |

---

## 9. Implementation Status

| Spec | Status |
|------|--------|
| Overview (this doc) | ✅ Created |
| Entity Descriptors | 🔄 In Progress |
| Sensory Descriptors | ❌ Planned |
| Psychological Descriptors | ❌ Planned |
| Combat Descriptors | ❌ Planned |
| Interaction Descriptors | ❌ Planned |

---

## 10. Related Specifications

| Spec | Relationship |
|------|--------------|
| [Room Engine Descriptors](../room-engine/descriptors.md) | Room-specific implementation |
| [Biome Specifications](../biomes/) | Biome-specific content |
| [Trauma Economy](../../01-core/trauma-economy.md) | Psychological descriptor triggers |

---

## 11. Changelog

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-14 | Initial specification |
