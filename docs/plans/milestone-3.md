# Milestone 3: The Warrior (Combat & AI)

This phase transitions the application from an exploration engine into a tactical turn-based RPG, implementing the "Engine of Violence," enemy AI, and the Ability system.

## v0.2.0: Combat Resolution Core

    ### v0.2.0a: The Arena (State & Initiative)
    Established the CombatState, Combatant adapter, and InitiativeService to manage turn order.

    ### v0.2.0b: The Exchange (Actions & Resolution)
    Implemented the AttackResolutionService to handle hit/miss logic, damage calculations, and the action economy.

    ### v0.2.0c: The Interface (Combat UI)
    Created the TUI CombatScreenRenderer to visualize the turn order, health bars, and scrolling combat log.

## v0.2.1: The Armory & The Affliction

    ### v0.2.1a: The Gear (Equipment Integration)
    Connected the Inventory System to Combat, replacing hardcoded damage values with stats from equipped weapons and armor.

    ### v0.2.1b: The Affliction (Status Effects)
    Implemented the StatusEffectService to handle Buffs, Debuffs (e.g., Bleeding, Stunned), and DoT ticking.

    ### v0.2.1c: The Visuals (UI Updates)
    Updated the UI to display status icons in the turn order and added a VictoryScreen for post-combat loot.

## v0.2.2: The Adversary

    ### v0.2.2a: The Bestiary (Data)
    Established EnemyTemplate records and the EnemyFactory to spawn scalable enemies like Draugr and Servitors.

    ### v0.2.2b: The Mind (AI)
    Implemented EnemyAIService with archetype-based behavior trees (e.g., Tank, Glass Cannon) to drive enemy decision-making.

    ### v0.2.2c: The Elite (Traits)
    Added the CreatureTraitService to generate procedural Elites with modifiers like Explosive or Vampiric.

## v0.2.3: The Hero's Toolkit

    ### v0.2.3a: The Fuel (Resources)
    Implemented ResourceService for Stamina regeneration and the Mystic's HP-to-Aether "Overcast" mechanic.

    ### v0.2.3b: The Engine (Ability Logic)
    Built the AbilityService and EffectScript parser to execute complex commands (e.g., `DAMAGE:Fire:2d6;STATUS:Burn:2`).

    ### v0.2.3c: The Arsenal (Kits)
    Seeded the database with Tier 1 abilities for all archetypes and wired the use command in the UI.

## v0.2.4: The Adversary's Arsenal

    ### v0.2.4a: The Loadout (Data)
    Hydrates enemies with specific ability sets defined in their templates.

    ### v0.2.4b: The Tactician (Logic)
    Upgrades Enemy AI to evaluate and use Active Abilities based on utility scoring.

    ### v0.2.4c: The Omen (Telegraphs)
    Implements multi-turn "Chant" attacks and UI telegraphs for powerful enemy moves.