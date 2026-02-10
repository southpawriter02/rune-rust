# The Worldbuilder's Journal

## 2024-05-23 - Legacy Migration Constraints
**Design Constraint:** Legacy content in seeders uses raw integers (e.g., `HP: 80`) and lacks narrative "Voice" or "Identity" fields required for TUI/GUI duality.
**Solution:** Adopted a "Migrate & Augment" pattern: Extract raw data to `docs/99-legacy`, then map to Tiered d10 tables (e.g., HP 80 -> Tier 3 Resilience) in `docs/06-bestiary` with added Voice Guidance.
