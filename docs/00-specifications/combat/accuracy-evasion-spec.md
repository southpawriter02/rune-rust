# Accuracy & Evasion System Specification

> **Template Version**: 1.0
> **Last Updated**: 2025-11-20
> **Status**: Draft
> **Specification ID**: SPEC-COMBAT-004

---

## Document Control

### Version History
| Version | Date | Author | Changes | Reviewers |
|---------|------|--------|---------|-----------|
| 1.0 | 2025-11-20 | AI | Initial specification | - |

### Approval Status
- [x] **Draft**: Initial authoring in progress
- [ ] **Review**: Ready for stakeholder review
- [ ] **Approved**: Approved for implementation
- [ ] **Active**: Currently implemented and maintained
- [ ] **Deprecated**: Superseded by [SPEC-ID]

### Stakeholders
- **Primary Owner**: Combat Designer
- **Design**: Hit/miss determination, opposed roll balance
- **Balance**: Hit chance targets, accuracy bonus economy, STURDINESS scaling
- **Implementation**: CombatEngine.cs, DiceService.cs
- **QA/Testing**: Probability verification, edge case validation

---

## Executive Summary

### Purpose Statement
The Accuracy & Evasion System determines whether attacks hit or miss using opposed dice pool rolls. Attackers roll dice based on weapon attributes (MIGHT/FINESSE/WILL) plus accuracy bonuses, defenders roll STURDINESS dice, and the net successes determine hit success. This creates tactical uncertainty while rewarding attribute investment and equipment upgrades.

### Scope
**In Scope**:
- Opposed roll mechanics (attack roll vs defense roll)
- Attack dice pool calculation (base attribute + accuracy bonuses)
- Defense dice pool calculation (STURDINESS attribute)
- Net success determination (attack successes - defense successes)
- Hit/miss resolution (net > 0 = hit, ≤ 0 = miss)
- Accuracy bonus sources (equipment, abilities, status effects)
- Tie-breaking rule (defender wins ties)
- Combat log integration for roll display
- Probability balancing and hit chance targets

**Out of Scope**:
- Damage calculation after hit lands → `SPEC-COMBAT-002`
- Status effect application mechanics → `SPEC-COMBAT-003`
- Critical hit damage multipliers → Future enhancement
- Flanking position calculation → `SPEC-COMBAT-008`
- Environmental accuracy modifiers → `SPEC-WORLD-003`
- Dodge/parry active abilities → Individual ability specs
- Cover and concealment → Future feature

### Success Criteria
- **Player Experience**: Hits feel earned not guaranteed; misses feel explainable; tactical choices (accuracy bonuses, STURDINESS) matter
- **Technical**: Roll calculations deterministic; dice pool size limits enforced; probability distributions verified
- **Design**: Balanced builds (offense vs defense) have ~50-60% hit chance; glass cannons hit ~80%; tanks hit ~30%
- **Balance**: Average 30-40% miss rate prevents "rocket tag"; STURDINESS investment provides measurable survivability improvement

---
