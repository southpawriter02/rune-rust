# Milestone 2: The Explorer (World & Interaction)

This phase transforms the application from a generic engine into a playable RPG by adding character identity, object interaction, an economy, and a lore progression system. All versions in this milestone have been implemented.

## v0.1.0: The Survivor's Beginning

Scope: Implemented the Character Creation Wizard using a Factory pattern. This includes Lineage, Background, and Archetype selection, along with the StatCalculationService for derived stats (HP, Stamina, AP).

Key Deliverables: CharacterCreationController, CharacterFactory, DerivedStats logic.

## v0.1.1: The Interaction Layer

Scope: Established the "Three-Tier Composition" engine for procedural descriptions and the core verb system (Search, Open, Unlock, Examine). This transformed static rooms into interactive environments.

Key Deliverables: InteractableObject entity, DescriptorEngine, InteractionService.

## v0.1.2: The Burden of Survival

Scope: Implemented the Inventory System, Equipment management, and Loot Generation. 

Divided into three sub-versions:

    ### v0.1.2a: Foundation

Data models for Items, Equipment, and Quality Tiers.

    ### v0.1.2b: Player Interaction

InventoryService logic, Burden (encumbrance) mechanics, and attribute bonuses from gear.

    ### v0.1.2c: World Integration

LootService for procedural drops based on Biome/Danger level and container searching.

## v0.1.3: Data Captures (The Codex)

Scope: Implemented the "Scavenger's Journal," a non-inventory progression system for collecting lore fragments and unlocking mechanics. 

Divided into three sub-versions:

    ### v0.1.3a: Data Foundation
    Database schema for CodexEntry and DataCapture.

    ### v0.1.3b: Logic
    DataCaptureService for probabilistic discovery and auto-assignment logic.

    ### v0.1.3c: UI
    The Terminal UI for the Journal, including the TextRedactor for progressive lore revealing.
