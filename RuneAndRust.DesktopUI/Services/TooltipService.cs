using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace RuneAndRust.DesktopUI.Services;

/// <summary>
/// v0.43.20: Implementation of ITooltipService.
/// Provides contextual tooltips and searchable help documentation.
/// </summary>
public class TooltipService : ITooltipService
{
    private static readonly ILogger _log = Log.ForContext<TooltipService>();
    private readonly ConcurrentDictionary<string, TooltipData> _tooltips = new();

    /// <summary>
    /// Creates a new TooltipService with default tooltips registered.
    /// </summary>
    public TooltipService()
    {
        InitializeDefaultTooltips();
        _log.Information("TooltipService initialized with {Count} tooltips", _tooltips.Count);
    }

    /// <inheritdoc/>
    public TooltipData GetTooltip(string key)
    {
        if (_tooltips.TryGetValue(key, out var tooltip))
        {
            return tooltip;
        }

        _log.Debug("Tooltip not found: {Key}", key);
        return new TooltipData
        {
            Key = key,
            Title = key,
            Description = "No help available for this item.",
            Category = "Unknown"
        };
    }

    /// <inheritdoc/>
    public void RegisterTooltip(string key, string title, string description,
        string? detailedHelp = null, string category = "General")
    {
        RegisterTooltip(new TooltipData
        {
            Key = key,
            Title = title,
            Description = description,
            DetailedHelp = detailedHelp,
            Category = category
        });
    }

    /// <inheritdoc/>
    public void RegisterTooltip(TooltipData tooltip)
    {
        _tooltips[tooltip.Key] = tooltip;
    }

    /// <inheritdoc/>
    public IEnumerable<TooltipData> SearchTooltips(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return _tooltips.Values.OrderBy(t => t.Category).ThenBy(t => t.Title);
        }

        var lowerQuery = query.ToLowerInvariant();
        return _tooltips.Values
            .Where(t =>
                t.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                t.Description.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                (t.DetailedHelp?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false) ||
                t.Tags.Any(tag => tag.Contains(query, StringComparison.OrdinalIgnoreCase)))
            .OrderByDescending(t => t.Title.StartsWith(query, StringComparison.OrdinalIgnoreCase))
            .ThenBy(t => t.Title);
    }

    /// <inheritdoc/>
    public IEnumerable<TooltipData> GetByCategory(string category)
    {
        return _tooltips.Values
            .Where(t => t.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
            .OrderBy(t => t.Title);
    }

    /// <inheritdoc/>
    public IEnumerable<string> GetCategories()
    {
        return _tooltips.Values
            .Select(t => t.Category)
            .Distinct()
            .OrderBy(c => c);
    }

    /// <inheritdoc/>
    public IEnumerable<TooltipData> GetAllTooltips()
    {
        return _tooltips.Values.OrderBy(t => t.Category).ThenBy(t => t.Title);
    }

    /// <inheritdoc/>
    public bool HasTooltip(string key)
    {
        return _tooltips.ContainsKey(key);
    }

    /// <summary>
    /// Initializes all default tooltips for the game.
    /// </summary>
    private void InitializeDefaultTooltips()
    {
        // Combat Actions
        InitializeCombatTooltips();

        // Character Stats
        InitializeStatTooltips();

        // Status Effects
        InitializeStatusEffectTooltips();

        // Items & Equipment
        InitializeItemTooltips();

        // Abilities & Specializations
        InitializeAbilityTooltips();

        // Dungeon & Exploration
        InitializeDungeonTooltips();

        // Progression & Meta
        InitializeProgressionTooltips();

        // UI & Controls
        InitializeUITooltips();
    }

    private void InitializeCombatTooltips()
    {
        RegisterTooltip(new TooltipData
        {
            Key = "combat.attack",
            Title = "Attack",
            Description = "Deal damage to a target enemy within range.",
            DetailedHelp = "Choose an enemy within your weapon's range and attempt to hit them. " +
                "Success depends on your Accuracy vs their Evasion. Critical hits deal double damage. " +
                "Melee attacks use MIGHT, ranged attacks use FINESSE.",
            Category = "Combat",
            Icon = "\u2694",
            Shortcut = "1",
            Tags = new List<string> { "damage", "melee", "ranged", "weapon" }
        });

        RegisterTooltip(new TooltipData
        {
            Key = "combat.defend",
            Title = "Defend",
            Description = "Take a defensive stance to reduce incoming damage.",
            DetailedHelp = "Until your next turn, gain +50% Physical Defense and +50% Metaphysical Defense. " +
                "You cannot perform other actions while defending. Useful when surrounded or low on HP.",
            Category = "Combat",
            Icon = "\U0001F6E1",
            Shortcut = "2",
            Tags = new List<string> { "defense", "block", "protect", "tank" }
        });

        RegisterTooltip(new TooltipData
        {
            Key = "combat.ability",
            Title = "Use Ability",
            Description = "Activate a special ability from your specialization.",
            DetailedHelp = "Abilities are powerful skills unlocked through your specialization tree. " +
                "Each ability has a Stamina cost and may have cooldowns. Rank up abilities using " +
                "Progression Points (PP) to increase their effectiveness.",
            Category = "Combat",
            Icon = "\u2728",
            Shortcut = "3",
            Tags = new List<string> { "skill", "special", "magic", "stamina" }
        });

        RegisterTooltip(new TooltipData
        {
            Key = "combat.item",
            Title = "Use Item",
            Description = "Use a consumable item from your inventory.",
            DetailedHelp = "Consumables include healing potions, antidotes, buff items, and throwables. " +
                "Using an item consumes your action for the turn. Keep healing items ready for emergencies.",
            Category = "Combat",
            Icon = "\U0001F9EA",
            Shortcut = "4",
            Tags = new List<string> { "potion", "consumable", "heal", "buff" }
        });

        RegisterTooltip(new TooltipData
        {
            Key = "combat.move",
            Title = "Move",
            Description = "Reposition on the battlefield.",
            DetailedHelp = "Move to any valid adjacent cell. Moving through hazards will trigger their effects. " +
                "Positioning is crucial: flanking grants +25% damage, cover reduces incoming damage, " +
                "and elevation provides accuracy bonuses.",
            Category = "Combat",
            Icon = "\u27A1",
            Shortcut = "5",
            Tags = new List<string> { "position", "tactical", "grid", "movement" }
        });

        RegisterTooltip(new TooltipData
        {
            Key = "combat.flee",
            Title = "Flee",
            Description = "Attempt to escape from combat.",
            DetailedHelp = "Fleeing has a chance to fail based on enemy speed. If successful, you return to " +
                "the previous room but lose any loot from defeated enemies. Fleeing from boss fights is not possible.",
            Category = "Combat",
            Icon = "\U0001F3C3",
            Shortcut = "F",
            Tags = new List<string> { "escape", "retreat", "run" }
        });

        RegisterTooltip(new TooltipData
        {
            Key = "combat.endturn",
            Title = "End Turn",
            Description = "End your turn and pass to the next combatant.",
            DetailedHelp = "If you have actions remaining, ending your turn early allows enemies to act sooner. " +
                "Useful when you want to wait for buffs to expire or enemies to move into range.",
            Category = "Combat",
            Icon = "\u23ED",
            Shortcut = "Space",
            Tags = new List<string> { "pass", "wait", "skip" }
        });

        RegisterTooltip(new TooltipData
        {
            Key = "combat.cover",
            Title = "Cover",
            Description = "Terrain that provides protection from attacks.",
            DetailedHelp = "Physical cover (solid lines) reduces physical damage by 25%. " +
                "Metaphysical cover (dashed lines) reduces magic damage by 25%. " +
                "Full cover blocks line of sight entirely. Use cover to survive against ranged enemies.",
            Category = "Combat",
            Icon = "\u2588",
            Tags = new List<string> { "terrain", "defense", "protection" }
        });

        RegisterTooltip(new TooltipData
        {
            Key = "combat.flanking",
            Title = "Flanking",
            Description = "Attacking an enemy from multiple angles.",
            DetailedHelp = "When an enemy is adjacent to two or more of your allies on opposite sides, " +
                "they are flanked. Attacks against flanked enemies deal +25% damage and have +10% critical chance.",
            Category = "Combat",
            Icon = "\u2694",
            Tags = new List<string> { "positioning", "tactical", "bonus" }
        });

        RegisterTooltip(new TooltipData
        {
            Key = "combat.turnorder",
            Title = "Turn Order",
            Description = "The sequence in which combatants act.",
            DetailedHelp = "Turn order is determined by Initiative (based on WITS). Higher Initiative acts first. " +
                "Some abilities can modify turn order or grant extra actions.",
            Category = "Combat",
            Tags = new List<string> { "initiative", "speed", "order" }
        });
    }

    private void InitializeStatTooltips()
    {
        RegisterTooltip(new TooltipData
        {
            Key = "stat.might",
            Title = "MIGHT",
            Description = "Physical power and melee damage.",
            DetailedHelp = "MIGHT increases your base melee damage, carrying capacity, and physical skill effectiveness. " +
                "Each point grants:\n• +2 Melee Damage\n• +10 Carrying Capacity\n• +1 Physical Skill Power",
            Category = "Stats",
            Icon = "\U0001F4AA",
            Tags = new List<string> { "strength", "melee", "physical", "power" }
        });

        RegisterTooltip(new TooltipData
        {
            Key = "stat.finesse",
            Title = "FINESSE",
            Description = "Dexterity and ranged accuracy.",
            DetailedHelp = "FINESSE increases ranged damage, accuracy, and evasion. " +
                "Each point grants:\n• +2 Ranged Damage\n• +1 Accuracy\n• +1 Evasion",
            Category = "Stats",
            Icon = "\U0001F3AF",
            Tags = new List<string> { "dexterity", "ranged", "accuracy", "dodge" }
        });

        RegisterTooltip(new TooltipData
        {
            Key = "stat.wits",
            Title = "WITS",
            Description = "Perception and initiative.",
            DetailedHelp = "WITS increases your initiative (turn order), critical chance, and detection radius. " +
                "Each point grants:\n• +2 Initiative\n• +1% Critical Chance\n• +1 Detection Radius",
            Category = "Stats",
            Icon = "\U0001F9E0",
            Tags = new List<string> { "intelligence", "speed", "critical", "perception" }
        });

        RegisterTooltip(new TooltipData
        {
            Key = "stat.will",
            Title = "WILL",
            Description = "Mental fortitude and magic power.",
            DetailedHelp = "WILL increases magic damage, Psychic Stress resistance, and maximum Stamina. " +
                "Each point grants:\n• +2 Magic Damage\n• +5 Max Stamina\n• +2 Max Psychic Stress Threshold",
            Category = "Stats",
            Icon = "\U0001F52E",
            Tags = new List<string> { "magic", "mental", "stamina", "willpower" }
        });

        RegisterTooltip(new TooltipData
        {
            Key = "stat.sturdiness",
            Title = "STURDINESS",
            Description = "HP and defense.",
            DetailedHelp = "STURDINESS increases maximum HP and both types of defense. " +
                "Each point grants:\n• +10 Max HP\n• +1 Physical Defense\n• +1 Metaphysical Defense",
            Category = "Stats",
            Icon = "\u2764",
            Tags = new List<string> { "health", "defense", "tank", "constitution" }
        });

        RegisterTooltip(new TooltipData
        {
            Key = "stat.hp",
            Title = "HP (Hit Points)",
            Description = "Your current health. Reach 0 and you're defeated.",
            DetailedHelp = "HP represents your physical well-being. When HP reaches 0, you are defeated. " +
                "Restore HP with healing items, rest, or certain abilities. Max HP is based on STURDINESS.",
            Category = "Stats",
            Icon = "\u2764",
            Tags = new List<string> { "health", "life", "hitpoints" }
        });

        RegisterTooltip(new TooltipData
        {
            Key = "stat.stamina",
            Title = "Stamina",
            Description = "Resource for using abilities.",
            DetailedHelp = "Stamina is consumed when using abilities. It regenerates slowly each turn or can be " +
                "restored with items and rest. Max Stamina is based on WILL. Manage stamina carefully in long fights.",
            Category = "Stats",
            Icon = "\u26A1",
            Tags = new List<string> { "energy", "mana", "resource" }
        });

        RegisterTooltip(new TooltipData
        {
            Key = "stat.psychicstress",
            Title = "Psychic Stress",
            Description = "Mental strain from dungeon exploration.",
            DetailedHelp = "Psychic Stress accumulates from resting in the dungeon and witnessing horrific events. " +
                "High Psychic Stress causes penalties and may trigger panic. " +
                "Reduce it by completing floors or using specific items. Threshold is based on WILL.",
            Category = "Stats",
            Icon = "\U0001F9E0",
            Tags = new List<string> { "sanity", "stress", "mental", "horror" }
        });

        RegisterTooltip(new TooltipData
        {
            Key = "stat.corruption",
            Title = "Corruption",
            Description = "Dark influence from metaphysical sources.",
            DetailedHelp = "Corruption is gained from dark artifacts, cursed items, and certain abilities. " +
                "At high levels, it grants power but causes permanent changes. " +
                "Very high corruption may transform your character permanently.",
            Category = "Stats",
            Icon = "\U0001F480",
            Tags = new List<string> { "dark", "curse", "mutation" }
        });
    }

    private void InitializeStatusEffectTooltips()
    {
        RegisterTooltip(new TooltipData
        {
            Key = "status.bleeding",
            Title = "Bleeding",
            Description = "Taking damage over time.",
            DetailedHelp = "Each turn at the start, lose HP equal to the bleed magnitude. " +
                "Can stack multiple times. Cured by healing items, bandages, or resting. " +
                "Movement may worsen bleeding.",
            Category = "Status Effects",
            Icon = "\U0001FA78",
            Tags = new List<string> { "dot", "damage", "wound" }
        });

        RegisterTooltip(new TooltipData
        {
            Key = "status.burning",
            Title = "Burning",
            Description = "On fire! Taking fire damage over time.",
            DetailedHelp = "Burning deals fire damage each turn. Can spread to adjacent allies. " +
                "Extinguish by entering water, using abilities, or waiting it out.",
            Category = "Status Effects",
            Icon = "\U0001F525",
            Tags = new List<string> { "fire", "dot", "damage" }
        });

        RegisterTooltip(new TooltipData
        {
            Key = "status.poisoned",
            Title = "Poisoned",
            Description = "Toxins dealing damage over time.",
            DetailedHelp = "Poison deals damage each turn and may reduce healing effectiveness. " +
                "Cure with antidotes or cleansing abilities. Some poisons have additional effects.",
            Category = "Status Effects",
            Icon = "\u2620",
            Tags = new List<string> { "toxic", "dot", "damage" }
        });

        RegisterTooltip(new TooltipData
        {
            Key = "status.stunned",
            Title = "Stunned",
            Description = "Cannot take actions.",
            DetailedHelp = "While stunned, you skip your turn entirely. " +
                "Stun typically lasts 1 turn. Some abilities grant stun immunity.",
            Category = "Status Effects",
            Icon = "\U0001F4AB",
            Tags = new List<string> { "disable", "skip", "crowd control" }
        });

        RegisterTooltip(new TooltipData
        {
            Key = "status.blessed",
            Title = "Blessed",
            Description = "Divine protection increasing success chances.",
            DetailedHelp = "Increases all success chances (accuracy, ability effects) by the magnitude percentage. " +
                "Cannot coexist with Cursed - one will override the other.",
            Category = "Status Effects",
            Icon = "\u2728",
            Tags = new List<string> { "buff", "holy", "bonus" }
        });

        RegisterTooltip(new TooltipData
        {
            Key = "status.cursed",
            Title = "Cursed",
            Description = "Dark affliction reducing success chances.",
            DetailedHelp = "Decreases all success chances by the magnitude percentage. " +
                "Cannot coexist with Blessed. Remove with holy items or cleansing abilities.",
            Category = "Status Effects",
            Icon = "\U0001F480",
            Tags = new List<string> { "debuff", "dark", "penalty" }
        });

        RegisterTooltip(new TooltipData
        {
            Key = "status.vulnerable",
            Title = "Vulnerable",
            Description = "Taking increased damage.",
            DetailedHelp = "All damage taken is increased by the magnitude percentage. " +
                "Focus vulnerable enemies to maximize damage output.",
            Category = "Status Effects",
            Icon = "\U0001F534",
            Tags = new List<string> { "debuff", "damage", "weakness" }
        });

        RegisterTooltip(new TooltipData
        {
            Key = "status.strengthened",
            Title = "Strengthened",
            Description = "Dealing increased damage.",
            DetailedHelp = "All damage dealt is increased by the magnitude percentage. " +
                "Stack with critical hits for massive damage.",
            Category = "Status Effects",
            Icon = "\U0001F4AA",
            Tags = new List<string> { "buff", "damage", "power" }
        });
    }

    private void InitializeItemTooltips()
    {
        RegisterTooltip(new TooltipData
        {
            Key = "inventory.equipment",
            Title = "Equipment",
            Description = "Gear that provides stat bonuses when equipped.",
            DetailedHelp = "You have equipment slots for: Weapon, Armor, and Accessory. " +
                "Equipped items provide stat bonuses and may grant special abilities. " +
                "Drag items from inventory to equip, or click to auto-equip.",
            Category = "Items",
            Icon = "\u2694",
            Shortcut = "I",
            Tags = new List<string> { "gear", "weapon", "armor" }
        });

        RegisterTooltip(new TooltipData
        {
            Key = "inventory.consumables",
            Title = "Consumables",
            Description = "Single-use items that provide immediate effects.",
            DetailedHelp = "Consumables include healing potions, status cure items, buff potions, and throwables. " +
                "Use in combat or exploration. Stock up before difficult encounters.",
            Category = "Items",
            Icon = "\U0001F9EA",
            Tags = new List<string> { "potion", "usable", "temporary" }
        });

        RegisterTooltip(new TooltipData
        {
            Key = "inventory.currency",
            Title = "Currency",
            Description = "Gold coins for purchasing items.",
            DetailedHelp = "Earn currency from defeated enemies, found treasure, and quest rewards. " +
                "Spend at merchants for equipment, consumables, and services.",
            Category = "Items",
            Icon = "\U0001FA99",
            Tags = new List<string> { "gold", "money", "coins" }
        });

        RegisterTooltip(new TooltipData
        {
            Key = "item.rarity.common",
            Title = "Common Item",
            Description = "Basic quality equipment.",
            DetailedHelp = "Common items have standard stats with no special effects. " +
                "Good for starting out, but replace when better gear is found.",
            Category = "Items",
            Tags = new List<string> { "quality", "gray", "basic" }
        });

        RegisterTooltip(new TooltipData
        {
            Key = "item.rarity.uncommon",
            Title = "Uncommon Item",
            Description = "Improved quality with minor bonuses.",
            DetailedHelp = "Uncommon items have improved base stats and may have one minor bonus effect. " +
                "Green-bordered items in the inventory.",
            Category = "Items",
            Tags = new List<string> { "quality", "green", "improved" }
        });

        RegisterTooltip(new TooltipData
        {
            Key = "item.rarity.rare",
            Title = "Rare Item",
            Description = "High quality with significant bonuses.",
            DetailedHelp = "Rare items have substantially improved stats and one or more special effects. " +
                "Blue-bordered items. Worth keeping and upgrading.",
            Category = "Items",
            Tags = new List<string> { "quality", "blue", "special" }
        });

        RegisterTooltip(new TooltipData
        {
            Key = "item.rarity.epic",
            Title = "Epic Item",
            Description = "Exceptional quality with powerful effects.",
            DetailedHelp = "Epic items have excellent stats and multiple powerful special effects. " +
                "Purple-bordered items. Often build-defining gear.",
            Category = "Items",
            Tags = new List<string> { "quality", "purple", "powerful" }
        });

        RegisterTooltip(new TooltipData
        {
            Key = "item.rarity.legendary",
            Title = "Legendary Item",
            Description = "The finest equipment with unique abilities.",
            DetailedHelp = "Legendary items have maximum stats and unique special abilities not found elsewhere. " +
                "Gold-bordered items. Extremely rare and valuable.",
            Category = "Items",
            Tags = new List<string> { "quality", "gold", "unique" }
        });
    }

    private void InitializeAbilityTooltips()
    {
        RegisterTooltip(new TooltipData
        {
            Key = "ability.unlock",
            Title = "Unlock Ability",
            Description = "Spend PP to learn a new ability.",
            DetailedHelp = "Each ability has a PP cost to unlock. Once unlocked, abilities can be ranked up " +
                "for increased effectiveness. Some abilities require prerequisite abilities.",
            Category = "Abilities",
            Icon = "\U0001F513",
            Tags = new List<string> { "skill", "learn", "progression" }
        });

        RegisterTooltip(new TooltipData
        {
            Key = "ability.rankup",
            Title = "Rank Up Ability",
            Description = "Spend PP to improve an ability.",
            DetailedHelp = "Each rank increases the ability's power, reduces cost, or adds new effects. " +
                "Maximum rank is typically 5. Higher ranks cost more PP.",
            Category = "Abilities",
            Icon = "\u2B06",
            Tags = new List<string> { "upgrade", "improve", "enhance" }
        });

        RegisterTooltip(new TooltipData
        {
            Key = "specialization.shieldmaiden",
            Title = "Shieldmaiden",
            Description = "Defensive warrior specialization.",
            DetailedHelp = "Shieldmaidens excel at protecting allies and controlling the battlefield. " +
                "Key abilities: Shield Wall, Guardian's Oath, Bulwark Stance. " +
                "Best for: Tank roles, protecting squishier allies.",
            Category = "Abilities",
            Icon = "\U0001F6E1",
            Tags = new List<string> { "tank", "defense", "protection" }
        });

        RegisterTooltip(new TooltipData
        {
            Key = "specialization.berserker",
            Title = "Berserker",
            Description = "Aggressive melee damage dealer.",
            DetailedHelp = "Berserkers trade defense for overwhelming offensive power. " +
                "Key abilities: Rage, Cleave, Bloodlust. " +
                "Best for: High damage output, clearing multiple enemies.",
            Category = "Abilities",
            Icon = "\u2694",
            Tags = new List<string> { "damage", "melee", "aggressive" }
        });

        RegisterTooltip(new TooltipData
        {
            Key = "specialization.skald",
            Title = "Skald",
            Description = "Support through songs and inspiration.",
            DetailedHelp = "Skalds buff allies and debuff enemies through performances. " +
                "Key abilities: Battle Hymn, Song of Courage, Discordant Note. " +
                "Best for: Group buffs, versatile support.",
            Category = "Abilities",
            Icon = "\U0001F3B5",
            Tags = new List<string> { "support", "buff", "music" }
        });

        RegisterTooltip(new TooltipData
        {
            Key = "specialization.seer",
            Title = "Seer",
            Description = "Mystical caster with foresight.",
            DetailedHelp = "Seers harness metaphysical power for damage and utility. " +
                "Key abilities: Foresight, Mind Blast, Psychic Shield. " +
                "Best for: Ranged magic damage, battlefield control.",
            Category = "Abilities",
            Icon = "\U0001F441",
            Tags = new List<string> { "magic", "ranged", "caster" }
        });
    }

    private void InitializeDungeonTooltips()
    {
        RegisterTooltip(new TooltipData
        {
            Key = "dungeon.explore",
            Title = "Explore Room",
            Description = "Search the current room for items and secrets.",
            DetailedHelp = "Exploring may reveal hidden items, trigger encounters, or find secret passages. " +
                "Higher WITS increases discovery chance. Some rooms have multiple searchable areas.",
            Category = "Dungeon",
            Icon = "\U0001F50D",
            Shortcut = "S",
            Tags = new List<string> { "search", "loot", "discover" }
        });

        RegisterTooltip(new TooltipData
        {
            Key = "dungeon.rest",
            Title = "Rest",
            Description = "Recover HP and Stamina at the cost of Psychic Stress.",
            DetailedHelp = "Resting in the dungeon restores HP and Stamina but increases Psychic Stress. " +
                "The longer you rest, the more you recover but the more stress you gain. " +
                "Rest strategically before difficult encounters.",
            Category = "Dungeon",
            Icon = "\U0001F6CF",
            Shortcut = "R",
            Tags = new List<string> { "heal", "recover", "camp" }
        });

        RegisterTooltip(new TooltipData
        {
            Key = "dungeon.minimap",
            Title = "Minimap",
            Description = "Shows explored areas of the dungeon.",
            DetailedHelp = "The minimap displays rooms you've visited and their connections. " +
                "Unexplored areas are hidden. Special rooms (boss, treasure) are marked when discovered.",
            Category = "Dungeon",
            Icon = "\U0001F5FA",
            Shortcut = "M",
            Tags = new List<string> { "map", "navigation", "explored" }
        });

        RegisterTooltip(new TooltipData
        {
            Key = "dungeon.hazard.fire",
            Title = "Fire Hazard",
            Description = "Burning ground that damages anyone standing in it.",
            DetailedHelp = "Fire hazards deal fire damage to any unit that starts their turn in the cell. " +
                "Can spread to adjacent cells. Avoid or use to your advantage against enemies.",
            Category = "Dungeon",
            Icon = "\U0001F525",
            Tags = new List<string> { "terrain", "damage", "environmental" }
        });

        RegisterTooltip(new TooltipData
        {
            Key = "dungeon.hazard.poison",
            Title = "Poison Hazard",
            Description = "Toxic ground that poisons those who enter.",
            DetailedHelp = "Poison hazards apply the Poisoned status effect to anyone who enters. " +
                "The poison continues to damage over time even after leaving the area.",
            Category = "Dungeon",
            Icon = "\u2620",
            Tags = new List<string> { "terrain", "status", "environmental" }
        });

        RegisterTooltip(new TooltipData
        {
            Key = "dungeon.floor",
            Title = "Floor / Milestone",
            Description = "Current depth in the dungeon.",
            DetailedHelp = "Each floor represents progress through the dungeon. Deeper floors have stronger enemies " +
                "but better rewards. Complete all rooms and defeat the floor boss to advance.",
            Category = "Dungeon",
            Tags = new List<string> { "depth", "level", "progress" }
        });
    }

    private void InitializeProgressionTooltips()
    {
        RegisterTooltip(new TooltipData
        {
            Key = "progression.legend",
            Title = "Legend",
            Description = "Your character's power level and renown.",
            DetailedHelp = "Legend increases through defeating powerful enemies and completing floors. " +
                "Each Legend level grants:\n• 2 Attribute Points\n• 1 Progression Point (PP)\n" +
                "Higher Legend unlocks more powerful abilities and equipment.",
            Category = "Progression",
            Icon = "\u2B50",
            Tags = new List<string> { "level", "experience", "power" }
        });

        RegisterTooltip(new TooltipData
        {
            Key = "progression.pp",
            Title = "Progression Points (PP)",
            Description = "Currency for unlocking and upgrading abilities.",
            DetailedHelp = "Spend PP in your specialization tree to:\n" +
                "• Unlock new abilities\n• Rank up existing abilities\n• Unlock passive bonuses\n\n" +
                "Earned from leveling Legend and completing achievements.",
            Category = "Progression",
            Icon = "\U0001F4A0",
            Tags = new List<string> { "points", "skill", "unlock" }
        });

        RegisterTooltip(new TooltipData
        {
            Key = "progression.ngplus",
            Title = "New Game+",
            Description = "Replay the game with increased difficulty and rewards.",
            DetailedHelp = "After completing the game, unlock NG+ tiers with:\n" +
                "• Stronger enemies\n• Better loot\n• New modifiers\n• Exclusive rewards\n\n" +
                "Progress through NG+1 to NG+5 for the ultimate challenge.",
            Category = "Progression",
            Icon = "\u2795",
            Tags = new List<string> { "endgame", "difficulty", "replay" }
        });

        RegisterTooltip(new TooltipData
        {
            Key = "progression.achievements",
            Title = "Achievements",
            Description = "Account-wide accomplishments.",
            DetailedHelp = "Achievements track your accomplishments across all characters. " +
                "Completing achievements earns meta-progression rewards including:\n" +
                "• Cosmetic unlocks\n• Starting bonuses\n• New game modes",
            Category = "Progression",
            Icon = "\U0001F3C6",
            Tags = new List<string> { "goals", "rewards", "meta" }
        });

        RegisterTooltip(new TooltipData
        {
            Key = "progression.metaprogression",
            Title = "Meta-Progression",
            Description = "Permanent account-wide unlocks.",
            DetailedHelp = "Meta-progression provides permanent benefits that apply to all future characters:\n" +
                "• Cosmetic skins\n• Alternative starting options\n• Permanent stat bonuses\n" +
                "Progress is never lost, even when characters die.",
            Category = "Progression",
            Tags = new List<string> { "permanent", "unlock", "account" }
        });
    }

    private void InitializeUITooltips()
    {
        RegisterTooltip(new TooltipData
        {
            Key = "ui.newgame",
            Title = "New Game",
            Description = "Start a new adventure.",
            DetailedHelp = "Create a new character and begin your journey into the dungeon. " +
                "Choose your class and customize your starting attributes.",
            Category = "UI",
            Icon = "\u2795",
            Tags = new List<string> { "start", "begin", "create" }
        });

        RegisterTooltip(new TooltipData
        {
            Key = "ui.continue",
            Title = "Continue",
            Description = "Resume your most recent save.",
            DetailedHelp = "Quickly load your most recently saved game and continue where you left off.",
            Category = "UI",
            Tags = new List<string> { "resume", "load", "recent" }
        });

        RegisterTooltip(new TooltipData
        {
            Key = "ui.loadgame",
            Title = "Load Game",
            Description = "Browse and load saved games.",
            DetailedHelp = "View all your saved games with details including character name, level, and progress. " +
                "Select a save to load or manage your save files.",
            Category = "UI",
            Icon = "\U0001F4C2",
            Shortcut = "L",
            Tags = new List<string> { "save", "browser", "files" }
        });

        RegisterTooltip(new TooltipData
        {
            Key = "ui.settings",
            Title = "Settings",
            Description = "Configure game options.",
            DetailedHelp = "Adjust graphics, audio, gameplay, accessibility, and control settings. " +
                "Changes are saved automatically.",
            Category = "UI",
            Icon = "\u2699",
            Tags = new List<string> { "options", "config", "preferences" }
        });

        RegisterTooltip(new TooltipData
        {
            Key = "ui.quicksave",
            Title = "Quick Save",
            Description = "Instantly save your current progress.",
            DetailedHelp = "Creates a quick save slot that can be loaded with Quick Load (F9). " +
                "Only one quick save is stored per character.",
            Category = "UI",
            Icon = "\U0001F4BE",
            Shortcut = "F5",
            Tags = new List<string> { "save", "instant", "shortcut" }
        });

        RegisterTooltip(new TooltipData
        {
            Key = "ui.quickload",
            Title = "Quick Load",
            Description = "Instantly load your quick save.",
            DetailedHelp = "Loads the most recent quick save. Warning: current progress will be lost.",
            Category = "UI",
            Icon = "\U0001F4C2",
            Shortcut = "F9",
            Tags = new List<string> { "load", "instant", "shortcut" }
        });

        RegisterTooltip(new TooltipData
        {
            Key = "ui.help",
            Title = "Help",
            Description = "Open the help browser.",
            DetailedHelp = "Browse all help topics or search for specific information. " +
                "The help system contains detailed explanations of all game mechanics.",
            Category = "UI",
            Icon = "\u2753",
            Shortcut = "F1",
            Tags = new List<string> { "documentation", "guide", "info" }
        });
    }
}
