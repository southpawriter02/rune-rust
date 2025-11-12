# Rune & Rust - Comprehensive Documentation (v0.17)

**Version:** v0.17 Documentation Suite
**Status:** 🚧 In Progress
**Purpose:** Complete technical reference for all game systems (v0.1-v0.16)

---

## Documentation Structure

This documentation follows a **5-layer architecture** for complete coverage:

### Layer 1: Functional (What It Does)
Player-facing descriptions, system overviews, feature lists, UI/UX documentation

### Layer 2: Statistical (The Numbers)
All formulas, stat values, scaling curves, probability tables

### Layer 3: Technical (How It Works)
Database schemas, service APIs, class hierarchies, integration patterns

### Layer 4: Testing (How We Verify)
Test coverage reports, test specifications, QA checklists, regression tests

### Layer 5: Balance (Why These Numbers)
Design intent, power budget analysis, role effectiveness matrices, difficulty tuning

---

## Documentation Sections

### 📖 [01 - System Documentation](./01-systems/)
Complete documentation of all game systems using the 5-layer structure:

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

### For Developers
1. Start with **System Documentation** to understand how systems work
2. Reference **Technical Reference** for implementation details
3. Use **Testing & QA** to verify your changes
4. Check **Statistical Registry** for exact values

### For Balance Designers
1. Start with **Balance Reference** to understand current state
2. Use **Statistical Registry** to find outliers
3. Reference **System Documentation** for design intent
4. Test changes against **QA Checklists**

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

👉 **Start with System Documentation**: [Combat Systems](./01-systems/combat-systems.md)
👉 **Browse Ability Registry**: [Abilities](./02-statistical-registry/abilities-registry.md)
👉 **Check Test Coverage**: [Testing & QA](./04-testing-qa/test-coverage-report.md)

---

**v0.17: Document before you balance — establish ground truth for all systems.**

*Generated: 2025-11-12*
*Status: 🚧 In Progress*
