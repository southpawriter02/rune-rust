# Rune & Rust - Comprehensive Documentation (v0.17)

**Version:** v0.17 Documentation Suite
**Status:** 🚧 In Progress
**Purpose:** Complete technical reference for all game systems (v0.1-v0.16)

---

## Documentation Structure

This documentation follows a **6-layer architecture** for complete coverage:

### Layer 0: Specifications (Design Intent - WHAT & WHY)
**NEW** - High-level feature/system specifications defining WHAT systems do and WHY they exist
- Design-focused (non-technical, implementation-agnostic)
- Functional requirements, acceptance criteria, design philosophy
- Player experience goals, balance targets, success metrics
- Serves as contract between design intent and implementation
- **Location**: `docs/00-specifications/` (organized by domain: combat, progression, world, narrative, economy)

### Layer 1: Functional (What It Does - HOW)
Player-facing descriptions, system overviews, feature lists, UI/UX documentation
- Technical implementation details
- System integration guides
- **Location**: `docs/01-systems/`

### Layer 2: Statistical (The Numbers)
All formulas, stat values, scaling curves, probability tables
- **Location**: `docs/02-statistical-registry/`

### Layer 3: Technical (How It Works)
Database schemas, service APIs, class hierarchies, integration patterns
- **Location**: `docs/03-technical-reference/`

### Layer 4: Testing (How We Verify)
Test coverage reports, test specifications, QA checklists, regression tests
- **Location**: `docs/04-testing-qa/`

### Layer 5: Balance (Why These Numbers)
Design intent, power budget analysis, role effectiveness matrices, difficulty tuning
- **Location**: `docs/05-balance-reference/`

---

## Documentation Sections

### 📐 [00 - Feature & System Specifications](./00-specifications/)
**NEW** - High-level design specifications organized by domain:

**Combat Domain:**
- Combat Resolution System (initiative, turn order, combat flow)
- Status Effects System (buffs, debuffs, DoT mechanics)
- Boss Encounter System (multi-phase mechanics, special abilities)

**Progression Domain:**
- Character Progression System (Legend/XP, Milestones, Progression Points)
- Archetype & Specialization System (class mechanics, skill trees)
- Ability Advancement System (ability ranks, costs, upgrades)

**World Domain:**
- Procedural Generation System (Wave Function Collapse, room design)
- Biome System (environmental themes, hazards)
- Room & Hazard System (environmental mechanics)

**Narrative Domain:**
- Descriptor Framework (flavor text, atmospheric descriptions)
- Dialogue System (branching conversations, skill checks)
- Faction & Reputation System (territory, faction interactions)
- Quest System (quest generation, objectives, rewards)

**Economy Domain:**
- Loot & Equipment System (quality tiers, drop tables)
- Crafting System (recipes, resources, components)
- Trauma Economy System (Psychic Stress, Corruption, Breaking Points)

**See**:
- **[START_HERE.md](./00-specifications/START_HERE.md)** - Quick start for AI sessions drafting new specs
- **[SPEC_BACKLOG.md](./00-specifications/SPEC_BACKLOG.md)** - Registry of all specs (3 completed, 34 planned, 37 total)
- **[Specification Writing Guide](./00-specifications/README.md)** - Complete governance, standards, and templates

### 📖 [01 - System Documentation](./01-systems/)
Complete technical documentation of all game systems using the 5-layer structure:

- **Combat Systems** - Combat resolution, damage, accuracy, status effects
- **Progression & Resources** - Legend/XP, PP, attributes, HP/Stamina
- **Trauma Economy** - Stress, Corruption, Breaking Points, Traumas
- **Specializations** - All 6 specializations and ability trees
- **Equipment & Items** - Equipment slots, quality tiers, weapons, armor
- **Enemies & Encounters** - Enemy types, stat blocks, AI behavior
- **Procedural Generation** - Wave Function Collapse, room templates, biomes
- **World State & Persistence** - State changes, destructible environments, save/load
- **Quest System** - Quest types, objectives, generation, rewards
- **Dialogue System** - Dialogue trees, skill checks, outcomes

### 📊 [02 - Statistical Registry](./02-statistical-registry/)
Queryable database of all game stats and formulas:

- **Abilities Registry** - All 45+ abilities with exact values
- **Equipment Registry** - All 60+ equipment pieces with stats
- **Enemy Registry** - All 20+ enemies with stat blocks
- **Status Effects Registry** - All 12+ status effects with formulas
- **Formulas Registry** - All calculation formulas with variables
- **Resource Costs** - Stamina, Stress, Corruption costs for all actions

### 🔧 [03 - Technical Reference](./03-technical-reference/)
Developer-focused technical documentation:

- **Database Schema** - ERD, table definitions, relationships
- **Service Architecture** - All services, APIs, dependencies
- **Code Architecture** - Class hierarchies, design patterns, data flow
- **Integration Patterns** - How systems interact and integrate

### ✅ [04 - Testing & QA](./04-testing-qa/)
Quality assurance framework and test documentation:

- **Test Coverage Report** - Coverage by system, gaps analysis
- **Test Specifications** - Unit test examples, integration tests
- **QA Checklists** - System-specific QA procedures
- **Bug Templates** - Bug tracking and regression test templates

### ⚖️ [05 - Balance Reference](./05-balance-reference/)
Balance analysis materials for tuning:

- **Damage Analysis** - DPS calculations, TTK analysis
- **Survivability Analysis** - HP pools, effective HP, mitigation
- **Resource Economy** - Stamina/Stress/Corruption rates
- **Build Viability** - Specialization effectiveness matrix
- **Difficulty Curves** - Enemy scaling, room difficulty

### 📋 [Templates](./templates/)
Reusable documentation templates:

- System Documentation Template (5-layer structure)
- Ability Registry Entry Template
- Enemy Bestiary Entry Template
- Equipment Entry Template
- Test Specification Template

### ⚡ [Quick Reference](./quick-reference/)
At-a-glance reference materials:

- Combat Resolution Flowchart
- Damage Calculation Cheat Sheet
- Ability Cost Reference Table
- Enemy Weakness Matrix
- Equipment Comparison Tables
- Formula Quick Reference

---

## How to Use This Documentation

### For Developers (Human & AI)
1. **Start with Specifications** (`00-specifications/`) to understand WHAT the system should do and WHY
2. Reference **System Documentation** (`01-systems/`) for HOW it's currently implemented
3. Use **Technical Reference** (`03-technical-reference/`) for code architecture details
4. Verify with **Testing & QA** (`04-testing-qa/`) to ensure correctness
5. Check **Statistical Registry** (`02-statistical-registry/`) for exact values

### For Balance Designers
1. Start with **Specifications** (`00-specifications/`) for design intent and balance targets
2. Reference **Balance Reference** (`05-balance-reference/`) to understand current state
3. Use **Statistical Registry** (`02-statistical-registry/`) to find outliers
4. Reference **System Documentation** (`01-systems/`) for implementation constraints
5. Test changes against **QA Checklists** (`04-testing-qa/`)

### For Content Creators
1. Start with **Statistical Registry** for templates
2. Reference **System Documentation** for constraints
3. Use **Enemy/Equipment Registry** for examples
4. Follow patterns from existing content

### For Testers
1. Start with **QA Checklists** for test plans
2. Reference **Test Coverage Report** for gaps
3. Use **Bug Templates** for reporting
4. Cross-reference **System Documentation** for expected behavior

---

## Documentation Standards

All documentation follows these standards:

### Numerical Precision
✅ Use exact numbers (e.g., "2d10+7 damage")
❌ Avoid vague terms (e.g., "moderate damage")

### Implementation-Ready
✅ Formulas are executable code
✅ Variable ranges defined
✅ Edge cases documented

### v5.0 Aethelgard Compliance
✅ All lore respects 800-year post-Glitch setting
✅ No fantasy/magic terminology
✅ Layer 2 diagnostic voice for technical elements
✅ Post-Glitch aesthetic maintained

### Cross-Referencing
✅ Link between related documents
✅ Reference file paths and line numbers
✅ Maintain consistent terminology

---

## Documentation Progress

- [x] Phase 1: Documentation Infrastructure (this file)
- [ ] Phase 2: Core Combat Documentation
- [ ] Phase 3: Progression & Resources Documentation
- [ ] Phase 4: Abilities & Specializations Registry
- [ ] Phase 5: Equipment & Enemy Documentation
- [ ] Phase 6: Technical Reference Documentation
- [ ] Phase 7: Testing & QA Documentation
- [ ] Phase 8: Balance Reference & Analysis
- [ ] Phase 9: Documentation Review & Polish

**Target Completion:** 35-50 hours (5-7 weeks part-time)

---

## Contributing to Documentation

When adding or updating documentation:

1. **Choose the right layer** - Functional, Statistical, Technical, Testing, or Balance
2. **Use templates** - See `/templates/` for standard formats
3. **Cross-reference** - Link to related systems and documents
4. **Verify accuracy** - Cross-check with code and database
5. **Update registry** - Add entries to statistical registry
6. **Run validation** - Ensure all links work and formulas are valid

---

## Next Steps

👉 **NEW: Read Specifications First**: [Specification Writing Guide](./00-specifications/README.md)
👉 **Browse Core Specifications**: [Combat Resolution](./00-specifications/combat/combat-resolution-spec.md) | [Character Progression](./00-specifications/progression/character-progression-spec.md) | [Trauma Economy](./00-specifications/economy/trauma-economy-spec.md)
👉 **Then Reference Implementation**: [System Documentation](./01-systems/)
👉 **Browse Ability Registry**: [Abilities](./02-statistical-registry/)
👉 **Check Test Coverage**: [Testing & QA](./04-testing-qa/)

---

**v0.17: Document before you balance — establish ground truth for all systems.**

*Generated: 2025-11-12*
*Status: 🚧 In Progress*
