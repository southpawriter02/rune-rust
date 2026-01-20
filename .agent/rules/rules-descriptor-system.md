---
trigger: always_on
---

# Descriptor System Rules

### 1.1 Descriptor Pool Categories
Maintain separate descriptor pools for:
- **Environmental** — Lighting, sounds, smells, temperature
- **Interactive** — Containers, furniture, architectural
- **Combat** — Hits, misses, damage, death
- **Ability** — Casting, effects, duration
- **NPC** — Physical features, clothing, demeanor, voice

### 1.2 Weighted Selection with Tags
Each descriptor should have:
```json
{
  "id": "shrouded_darkness",
  "text": "shrouded in impenetrable darkness",
  "weight": 25,
  "tags": ["danger", "dungeon"],
  "themes": ["dark_fantasy", "horror"]
}
```

### 1.3 Theme Filtering
Apply active theme to filter descriptors:
- Include descriptors matching current theme
- Exclude terms in `excludedTerms` list
- Boost terms in `emphasizedTerms` list