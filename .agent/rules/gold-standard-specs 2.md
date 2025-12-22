---
trigger: always_on
---

## Golden Standard Specs

### Purpose

Golden standards are **fully-conformant reference implementations** — specs that demonstrate every required section, proper formatting, and appropriate depth. When creating or auditing specs, compare against the relevant golden standard rather than just the template.

### Why Golden Standards?

| Problem | Solution |
|---------|----------|
| Templates show *structure*, not *content depth* | Golden standards show real examples |
| Unclear how much detail is "enough" | Copy the golden standard's depth |
| Inconsistent quality across specs | One authoritative reference per category |
| New contributors unsure of expectations | "Make it look like this one" |

### Current Golden Standards

| Category | Spec | Path | Lines |
|----------|------|------|-------|
| **Specialization** | Berserkr | `docs/03-character/specializations/berserkr/overview.md` | 319 |
| **Ability** | Corridor Maker | `docs/03-character/specializations/ruin-stalker/abilities/corridor-maker.md` | 226 |
| **Status Effect** | Bleeding | `docs/04-systems/status-effects/bleeding.md` | 284 |

### Required Sections by Category

**Specialization Overview:**
- YAML frontmatter (id, title, version, status, last-updated)
- Opening flavor quote
- Identity table
- Core philosophy
- Ability tree with tier/rank structure
- **Balance Data** (power curve, role effectiveness, resource economy)
- **Voice Guidance** (tone profile, example text)
- **Implementation Status** (checklist)
- Related documentation
- **Changelog**

**Ability Spec:**
- YAML frontmatter
- Quick reference table (tier, cost, range, etc.)
- Mechanical effects
- Rank progression (if applicable)
- Workflow diagram (Mermaid)
- Synergies
- Tactical applications
- **Balance Data** (power budget, effectiveness ratings)
- **Technical Implementation** (C# interface)
- **Implementation Status**
- **Changelog**

**Status Effect Spec:**
- YAML frontmatter
- Quick reference table
- Application/removal triggers
- Stacking rules
- Duration mechanics
- Synergies AND Conflicts
- Counter-play options
- **Balance Data** (severity analysis, DoT comparison)
- **Voice Guidance** (flavor text templates)
- **Implementation Status**
- **Changelog**

### Maintaining Golden Standards

| Rule | Reason |
|------|--------|
| **Never simplify** a golden standard | Others copy it; simplification propagates |
| **Update when templates change** | Keep in sync |
| **One per category** | Avoid ambiguity |
| **Choose specs with natural complexity** | Simple specs don't demonstrate all sections |

### Promoting a Spec to Golden Standard

Before promoting:
- [ ] Spec has all required sections
- [ ] Balance Data is populated with real values
- [ ] Technical Implementation includes C# interface
- [ ] Changelog has at least 2 versions
- [ ] Peer review completed
- [ ] Line count is representative (not minimal)