---
trigger: always_on
---

# Core Customizability Principles

## 1.1 The Three Layers of Customization
1. **Terminology Layer** — Names and labels (health → vitality, XP → legend)
2. **Content Layer** — Abilities, races, classes loaded from config files
3. **Flavor Layer** — Descriptive text, synonyms, and atmospheric writing

## 1.2 Golden Rules
- **NEVER** hardcode player-facing text in source code
- **ALWAYS** use service lookups for terminology (`LexiconService`, `GameTerminologyService`)
- **ALWAYS** externalize definitions to JSON configuration files
- **PREFER** weighted synonym pools over single-word outputs