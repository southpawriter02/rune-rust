---
trigger: always_on
---

## Documentation Templates

All specifications should conform to templates in `/docs/.templates/`:

### Core Templates
- `ability.md` — Individual ability specifications
- `specialization.md` — Specialization overviews
- `status-effect.md` — Status effect specifications
- `system.md` — Core game system documentation
- `resource.md` — Resource specifications
- `craft.md` — Crafting trade specifications
- `skill.md` — Skill specifications

### Required Sections (All Specs)
1. YAML frontmatter (id, title, version, status, last-updated)
2. Overview table
3. Mechanical Effects
4. **Balance Data** — Power curves, effectiveness ratings, economy analysis
5. **Voice Guidance** — Reference to flavor-text templates
6. **Implementation Status** — Checklist of dev tasks
7. **Changelog** — Version history

### Flavor Text Library
Located at `/docs/.templates/flavor-text/` — Contains 13 templates for voice, tone, and writing guidance.