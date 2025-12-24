---
description: How to effectively gain context about a system, component, or topic in this project
---

# Gaining Context for Rune & Rust Project

This workflow explains how to quickly and effectively gain context about any system, component, or topic in the Rune & Rust project.

## Step 1: Check Cross-References First

Start by consulting the Cross-References section in `docs/00-project/DOCUMENTATION_STANDARDS.md` (lines 61-126):

| Category | Description |
|----------|-------------|
| **Core Systems** | Dice, Game Loop, Death, Persistence, Character Creation, Trauma, Saga, Events |
| **Resources** | HP, Stamina, Stress, Fury, Corruption |
| **Attributes** | MIGHT, FINESSE, STURDINESS, WITS, WILL |
| **Templates** | 7 spec templates + flavor-text library |
| **Specializations** | Berserkr, Bone-Setter, Ruin Stalker, Rúnasmiðr |
| **Crafting Trades** | Field Medicine, Runeforging, Alchemy, Bodging |

## Step 2: Navigate the Directory Structure

Use the directory structure (lines 365-435) to find specific content:

| Content Type | Location |
|--------------|----------|
| Core systems | `docs/01-core/{system}.md` |
| Resources | `docs/01-core/resources/{resource}.md` |
| Attributes | `docs/01-core/attributes/{attribute}.md` |
| Specializations | `docs/03-character/specializations/{name}/overview.md` |
| Abilities | `docs/03-character/specializations/{spec}/abilities/{ability}.md` |
| Status effects | `docs/04-systems/status-effects/{effect}.md` |
| Crafting | `docs/04-systems/crafting/{trade}.md` |
| Templates | `docs/.templates/{category}.md` |
| Flavor text | `docs/.templates/flavor-text/` |

## Step 3: Review Golden Standards for Examples

For quality reference implementations, check these golden standards (lines 171-177):

| Category | Golden Standard |
|----------|-----------------|
| Specialization | `docs/03-character/specializations/berserkr/overview.md` |
| Ability | `docs/03-character/specializations/ruin-stalker/abilities/corridor-maker.md` |
| Status Effect | `docs/04-systems/status-effects/bleeding.md` |

## Step 4: Understand Core Mechanics

Before working on anything, internalize these critical rules:

### Dice System (lines 3-58)
- **Resolution rolls**: Always d10 (skill/attribute checks)
- **Damage dice**: Tiered hierarchy (d4 Minor → d6 Light → d8 Medium → d10 Heavy)

### Naming Conventions (lines 815-847)
- **Player-facing**: Full diacritics (Rúnasmiðr)
- **Code/files**: ASCII lowercase (runasmidr)

### Key Mechanics (lines 850-863)
- **Corruption**: Permanent, 100 = Forlorn transformation
- **Stress**: 0-100, triggers Trauma Check at 100
- **Stamina**: Universal action currency, 25%/turn regen

## Step 5: Understand the Setting Context

Key thematic elements (lines 679-708):

- **Lost Knowledge**: Inhabitants don't understand *why* tech works
- **Bodge Culture**: Trial-and-error tinkering, not engineering
- **Cargo Cult Technology**: Interacting with incomprehensible Old World tech
- **Nordic Fatalism**: Doom is coming; glory is in how you face it

### Magic System (lines 711-767)
- Runes = command syntax for the Aetheric Field (not spells)
- FUTHARK Protocol = corrupted operating system for reality
- Runic Blight = consequence of magic use

## Step 6: Check Legacy for Additional Context

If spec doesn't exist yet, check `docs/99-legacy/` (968 Notion exports):
- ⚠️ Reference only — do not edit or link to
- ✅ Extract patterns and content to create proper specs

## Step 7: Use Templates When Creating

Always use the appropriate template from `docs/.templates/`:

| Creating | Template |
|----------|----------|
| Ability | `ability.md` |
| Specialization | `specialization.md` |
| Status effect | `status-effect.md` |
| Core system | `system.md` |
| Resource | `resource.md` |
| Crafting trade | `craft.md` |
| Skill | `skill.md` |

## Quick Reference Commands

```bash
# Find a specific spec by name
fd "stamina" docs/

# Search for a mechanic across all specs
rg "Corruption" docs/ --include "*.md"

# List all status effects
ls docs/04-systems/status-effects/

# List all specialization abilities
ls docs/03-character/specializations/*/abilities/
```

## Checklist for Full Context

- [ ] Checked Cross-References section
- [ ] Located the relevant spec file
- [ ] Reviewed golden standard for the category
- [ ] Understood dice system (d10 resolution, tiered damage)
- [ ] Understood naming conventions (diacritics vs ASCII)
- [ ] Reviewed key mechanics (Corruption, Stress, Stamina)
- [ ] Understood setting context (Lost Knowledge, Bodge Culture)
- [ ] Checked legacy folder if spec doesn't exist
