using RuneAndRust.Core;

namespace RuneAndRust.Engine;

/// <summary>
/// Result of a crafting attempt
/// </summary>
public class CraftingResult
{
    public bool Success { get; set; }
    public Consumable? CraftedItem { get; set; }
    public CraftQuality Quality { get; set; }
    public int RollTotal { get; set; }
    public int DC { get; set; }
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// v0.19.10: Result of a runeforging attempt
/// </summary>
public class RuneforgingResult
{
    public bool Success { get; set; }
    public Equipment? EnchantedItem { get; set; }
    public bool IsCriticalSuccess { get; set; } = false; // DC+5 = Masterwork
    public int ChargesApplied { get; set; } = 0;
    public int RollTotal { get; set; }
    public int DC { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? SagaProperty { get; set; } = null; // Legendary effect if Masterwork + Architect of Stability
}

/// <summary>
/// Manages crafting system for Field Medicine and Runeforging (v0.19.10)
/// </summary>
public class CraftingService
{
    private readonly DiceService _diceService;
    private readonly List<CraftingRecipe> _recipes = new();
    private readonly List<RuneforgeRecipe> _runeforgeRecipes = new(); // v0.19.10

    public CraftingService(DiceService? diceService = null)
    {
        _diceService = diceService ?? new DiceService();
        InitializeRecipes();
        InitializeRuneforgeRecipes(); // v0.19.10
    }

    /// <summary>
    /// Get all available recipes (optionally filtered by specialization)
    /// </summary>
    public List<CraftingRecipe> GetAvailableRecipes(PlayerCharacter character)
    {
        return _recipes.Where(r =>
            !r.RequiresBoneSetterSpecialization ||
            character.Specialization == Specialization.BoneSetter
        ).ToList();
    }

    /// <summary>
    /// Attempt to craft an item using a recipe
    /// </summary>
    public CraftingResult CraftItem(PlayerCharacter character, CraftingRecipe recipe)
    {
        // Verify specialization requirements
        if (recipe.RequiresBoneSetterSpecialization && character.Specialization != Specialization.BoneSetter)
        {
            return new CraftingResult
            {
                Success = false,
                Message = "You lack the medical training to craft this item. (Requires Bone-Setter specialization)"
            };
        }

        // Make skill check (WITS + Field Medic I bonus if applicable)
        int attributeValue = character.GetAttributeValue(recipe.SkillAttribute);
        int bonus = GetCraftingBonus(character);
        var skillCheck = _diceService.Roll(attributeValue + bonus);

        int rollTotal = skillCheck.TotalValue;
        int dc = recipe.SkillCheckDC;

        // Determine result
        if (rollTotal < dc)
        {
            // Failure - components are wasted
            return new CraftingResult
            {
                Success = false,
                RollTotal = rollTotal,
                DC = dc,
                Message = $"Crafting failed! (Rolled {rollTotal} vs DC {dc})\nComponents were consumed in the failed attempt."
            };
        }

        // Success - determine quality
        CraftQuality quality = (rollTotal >= dc + 5) ? CraftQuality.Masterwork : CraftQuality.Standard;
        Consumable craftedItem = recipe.CreateResult(quality);

        string qualityText = quality == CraftQuality.Masterwork ? "Masterwork!" : "Standard";

        return new CraftingResult
        {
            Success = true,
            CraftedItem = craftedItem,
            Quality = quality,
            RollTotal = rollTotal,
            DC = dc,
            Message = $"Crafting successful! ({qualityText})\nRolled {rollTotal} vs DC {dc}"
        };
    }

    /// <summary>
    /// Get crafting bonus from abilities
    /// </summary>
    private int GetCraftingBonus(PlayerCharacter character)
    {
        int bonus = 0;

        // Field Medic I: +2 to crafting checks
        var fieldMedic = character.Abilities.FirstOrDefault(a => a.Name == "Field Medic I");
        if (fieldMedic != null)
        {
            bonus += 2;
        }

        // Field Medic II: +3 to crafting checks
        var fieldMedic2 = character.Abilities.FirstOrDefault(a => a.Name == "Field Medic II");
        if (fieldMedic2 != null)
        {
            bonus += 3;
        }

        return bonus;
    }

    /// <summary>
    /// Initialize all Field Medicine recipes
    /// </summary>
    private void InitializeRecipes()
    {
        // Healing Poultice - Basic healing item
        _recipes.Add(new CraftingRecipe
        {
            Name = "Healing Poultice",
            Description = "A basic medicinal compress that restores HP.",
            RequiredComponents = new Dictionary<ComponentType, int>
            {
                { ComponentType.CommonHerb, 2 },
                { ComponentType.CleanCloth, 1 }
            },
            SkillCheckDC = 10,
            SkillAttribute = "WITS",
            RequiresBoneSetterSpecialization = true,
            ResultItemName = "Healing Poultice",
            CreateResult = (quality) => new Consumable
            {
                Name = "Healing Poultice",
                Description = "A compress of medicinal herbs that restores vitality.",
                Type = ConsumableType.Medicine,
                Quality = quality,
                HPRestore = 15,
                MasterworkBonusHP = 5, // Masterwork restores 20 HP total
            }
        });

        // Antidote - Cures poison
        _recipes.Add(new CraftingRecipe
        {
            Name = "Antidote",
            Description = "A mixture that neutralizes toxins and cures poison.",
            RequiredComponents = new Dictionary<ComponentType, int>
            {
                { ComponentType.CommonHerb, 2 },
                { ComponentType.Antiseptic, 1 }
            },
            SkillCheckDC = 12,
            SkillAttribute = "WITS",
            RequiresBoneSetterSpecialization = true,
            ResultItemName = "Antidote",
            CreateResult = (quality) => new Consumable
            {
                Name = "Antidote",
                Description = "Neutralizes toxins and cures poison status.",
                Type = ConsumableType.Medicine,
                Quality = quality,
                HPRestore = 5,
                ClearsPoison = true,
                MasterworkBonusHP = 5, // Masterwork also restores 10 HP
            }
        });

        // Stabilizing Draught - Emergency medicine with temp HP
        _recipes.Add(new CraftingRecipe
        {
            Name = "Stabilizing Draught",
            Description = "An emergency restorative that grants temporary fortitude.",
            RequiredComponents = new Dictionary<ComponentType, int>
            {
                { ComponentType.CommonHerb, 1 },
                { ComponentType.Antiseptic, 1 },
                { ComponentType.Stimulant, 1 }
            },
            SkillCheckDC = 14,
            SkillAttribute = "WITS",
            RequiresBoneSetterSpecialization = true,
            ResultItemName = "Stabilizing Draught",
            CreateResult = (quality) => new Consumable
            {
                Name = "Stabilizing Draught",
                Description = "Emergency medicine that restores HP and grants temporary resilience.",
                Type = ConsumableType.Medicine,
                Quality = quality,
                HPRestore = 10,
                TempHPGrant = 10,
                MasterworkBonusHP = 5, // Masterwork grants 15 HP + 10 Temp HP
            }
        });

        // Trauma Salve - Reduces Psychic Stress
        _recipes.Add(new CraftingRecipe
        {
            Name = "Trauma Salve",
            Description = "A soothing mixture that calms the mind and reduces stress.",
            RequiredComponents = new Dictionary<ComponentType, int>
            {
                { ComponentType.CommonHerb, 3 },
                { ComponentType.Antiseptic, 1 }
            },
            SkillCheckDC = 13,
            SkillAttribute = "WITS",
            RequiresBoneSetterSpecialization = true,
            ResultItemName = "Trauma Salve",
            CreateResult = (quality) => new Consumable
            {
                Name = "Trauma Salve",
                Description = "Soothes trauma and reduces Psychic Stress.",
                Type = ConsumableType.Medicine,
                Quality = quality,
                HPRestore = 5,
                StressRestore = 15,
                MasterworkBonusHP = 5, // Masterwork restores 10 HP + reduces 15 Stress
            }
        });

        // Field Dressing - Stops bleeding
        _recipes.Add(new CraftingRecipe
        {
            Name = "Field Dressing",
            Description = "Emergency bandaging that stops bleeding wounds.",
            RequiredComponents = new Dictionary<ComponentType, int>
            {
                { ComponentType.CleanCloth, 2 },
                { ComponentType.Antiseptic, 1 }
            },
            SkillCheckDC = 11,
            SkillAttribute = "WITS",
            RequiresBoneSetterSpecialization = true,
            ResultItemName = "Field Dressing",
            CreateResult = (quality) => new Consumable
            {
                Name = "Field Dressing",
                Description = "Bandages that stop bleeding and restore some HP.",
                Type = ConsumableType.Medicine,
                Quality = quality,
                HPRestore = 8,
                ClearsBleeding = true,
                MasterworkBonusHP = 7, // Masterwork restores 15 HP + stops bleeding
            }
        });

        // Combat Stimulant - Restores Stamina
        _recipes.Add(new CraftingRecipe
        {
            Name = "Combat Stimulant",
            Description = "A powerful stimulant that restores stamina in combat.",
            RequiredComponents = new Dictionary<ComponentType, int>
            {
                { ComponentType.Stimulant, 2 },
                { ComponentType.CommonHerb, 1 }
            },
            SkillCheckDC = 15,
            SkillAttribute = "WITS",
            RequiresBoneSetterSpecialization = true,
            ResultItemName = "Combat Stimulant",
            CreateResult = (quality) => new Consumable
            {
                Name = "Combat Stimulant",
                Description = "Adrenaline compound that restores stamina and vigor.",
                Type = ConsumableType.Medicine,
                Quality = quality,
                StaminaRestore = 30,
                MasterworkBonusStamina = 10, // Masterwork restores 40 Stamina
            }
        });
    }

    /// <summary>
    /// Get recipe by name
    /// </summary>
    public CraftingRecipe? GetRecipeByName(string name)
    {
        return _recipes.FirstOrDefault(r => r.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Get list of all recipe names
    /// </summary>
    public List<string> GetAllRecipeNames()
    {
        return _recipes.Select(r => r.Name).ToList();
    }

    // ==================== v0.19.10: RUNEFORGING SYSTEM ====================

    /// <summary>
    /// Get all available runeforge recipes (Rúnasmiðr only)
    /// </summary>
    public List<RuneforgeRecipe> GetAvailableRuneforgeRecipes(PlayerCharacter character)
    {
        return _runeforgeRecipes.Where(r =>
            !r.RequiresRunasmidrSpecialization ||
            character.Specialization == Specialization.Runasmidr
        ).ToList();
    }

    /// <summary>
    /// Attempt to runeforge (enchant) an item
    /// </summary>
    public RuneforgingResult RuneforgeItem(PlayerCharacter character, RuneforgeRecipe recipe, Equipment targetItem)
    {
        // Verify specialization requirements
        if (recipe.RequiresRunasmidrSpecialization && character.Specialization != Specialization.Runasmidr)
        {
            return new RuneforgingResult
            {
                Success = false,
                Message = "You lack the runic knowledge to perform this enchantment. (Requires Rúnasmiðr specialization)"
            };
        }

        // Verify equipment type matches
        if (targetItem.Type != recipe.TargetType)
        {
            string expectedType = recipe.TargetType == EquipmentType.Weapon ? "weapon" : "armor";
            return new RuneforgingResult
            {
                Success = false,
                Message = $"This enchantment can only be applied to {expectedType}s."
            };
        }

        // Make Forging Pool check (WITS + WILL + Master Carver bonus)
        int witsValue = character.GetAttributeValue("WITS");
        int willValue = character.GetAttributeValue("WILL");
        int masterCarverBonus = GetMasterCarverBonus(character);
        var forgingCheck = _diceService.Roll(witsValue + willValue + masterCarverBonus);

        int rollTotal = forgingCheck.TotalValue;
        int dc = recipe.ForgingDC;

        // Determine result
        if (rollTotal < dc)
        {
            // Failure - components are wasted, item unharmed
            return new RuneforgingResult
            {
                Success = false,
                RollTotal = rollTotal,
                DC = dc,
                Message = $"Runeforging failed! (Rolled {rollTotal} vs DC {dc})\nThe runes flicker and fade. Components were consumed."
            };
        }

        // Success - determine if Masterwork (critical success)
        bool isMasterwork = (rollTotal >= dc + 5);
        int chargesApplied = isMasterwork ? recipe.MasterworkCharges : recipe.BaseCharges;

        // Apply enchantment to item
        targetItem.RunicEnchantment = recipe.EnchantmentName;
        targetItem.RunicCharges = chargesApplied;
        targetItem.RunicEffect = recipe.ChargeEffect;

        // Check for Architect of Stability (Capstone) - adds Saga Property on Masterwork
        bool hasArchitect = character.Abilities.Any(a => a.Name == "Architect of Stability");
        string? sagaProperty = null;

        if (isMasterwork && hasArchitect)
        {
            // Roll on Saga Property Table
            sagaProperty = RollSagaProperty();
            targetItem.SagaProperty = sagaProperty;
        }

        string qualityText = isMasterwork ? "Masterwork!" : "Standard";
        string sagaText = sagaProperty != null ? $"\nSaga Property: [{sagaProperty}]" : "";

        return new RuneforgingResult
        {
            Success = true,
            EnchantedItem = targetItem,
            IsCriticalSuccess = isMasterwork,
            ChargesApplied = chargesApplied,
            RollTotal = rollTotal,
            DC = dc,
            SagaProperty = sagaProperty,
            Message = $"Runeforging successful! ({qualityText})\nRolled {rollTotal} vs DC {dc}\n[{targetItem.Name}] now has {chargesApplied} charges of [{recipe.EnchantmentName}]{sagaText}"
        };
    }

    /// <summary>
    /// Get Master Carver bonus from abilities
    /// </summary>
    private int GetMasterCarverBonus(PlayerCharacter character)
    {
        int bonus = 0;

        // Master Carver I: +1d10
        var masterCarver1 = character.Abilities.FirstOrDefault(a => a.Name == "Master Carver I");
        if (masterCarver1 != null)
        {
            bonus += 1;
        }

        // Master Carver II: +2d10
        var masterCarver2 = character.Abilities.FirstOrDefault(a => a.Name == "Master Carver II");
        if (masterCarver2 != null)
        {
            bonus += 2;
        }

        // Master Carver III: +3d10
        var masterCarver3 = character.Abilities.FirstOrDefault(a => a.Name == "Master Carver III");
        if (masterCarver3 != null)
        {
            bonus += 3;
        }

        return bonus;
    }

    /// <summary>
    /// Roll on Saga Property Table for Masterwork items (Architect of Stability capstone)
    /// </summary>
    private string RollSagaProperty()
    {
        var random = new Random();
        int roll = random.Next(1, 16); // 1-15

        return roll switch
        {
            1 => "Unbreakable (never loses durability)",
            2 => "Thirsty Blade (lifesteal on kill: restore 1d6 HP)",
            3 => "Swift Foot (+2 Vigilance bonus)",
            4 => "Ironward (+1 Soak vs Physical damage)",
            5 => "Aetherward (+1 Soak vs Arcane damage)",
            6 => "Flameward (+1 Soak vs Fire damage)",
            7 => "Frostward (+1 Soak vs Ice damage)",
            8 => "Lucky Strike (reroll 1s on attack rolls)",
            9 => "Vengeful (deal +1d6 damage when below 50% HP)",
            10 => "Steadfast (immune to [Feared] status)",
            11 => "Cleansing (remove 1 Stress per combat won)",
            12 => "Inspiring (allies within 2 tiles gain +1 to checks)",
            13 => "Tireless (+5 Max Stamina)",
            14 => "Resilient (+10 Max HP)",
            15 => "Eternal Charge (50% chance to not consume runic charges)",
            _ => "Blessed (unknown benefit)"
        };
    }

    /// <summary>
    /// Initialize all Runeforging recipes (v0.19.10)
    /// </summary>
    private void InitializeRuneforgeRecipes()
    {
        // Algiz Imbuement - Defensive enchantment for armor (Tier 1)
        _runeforgeRecipes.Add(new RuneforgeRecipe
        {
            Name = "Algiz Imbuement",
            Description = "Imbue armor with protective wards that can be activated for defense.",
            EnchantmentName = "Warding Rune",
            RequiredComponents = new Dictionary<ComponentType, int>
            {
                { ComponentType.AlgizTablet, 1 },
                { ComponentType.AetherDust, 2 }
            },
            ForgingDC = 12,
            TargetType = EquipmentType.Armor,
            BaseCharges = 2,
            MasterworkCharges = 3,
            ChargeEffect = "React when hit: Gain +5 Soak vs that attack",
            RequiresRunasmidrSpecialization = true
        });

        // Uruz Imbuement - Offensive enchantment for weapons (Tier 2)
        _runeforgeRecipes.Add(new RuneforgeRecipe
        {
            Name = "Uruz Imbuement",
            Description = "Imbue weapon with primal strength that can be unleashed in battle.",
            EnchantmentName = "Bull's Strength",
            RequiredComponents = new Dictionary<ComponentType, int>
            {
                { ComponentType.UruzStone, 1 },
                { ComponentType.AetherDust, 2 }
            },
            ForgingDC = 13,
            TargetType = EquipmentType.Weapon,
            BaseCharges = 3,
            MasterworkCharges = 4,
            ChargeEffect = "Free Action: Add +2d10 damage to your next attack",
            RequiresRunasmidrSpecialization = true
        });
    }

    /// <summary>
    /// Get runeforge recipe by name
    /// </summary>
    public RuneforgeRecipe? GetRuneforgeRecipeByName(string name)
    {
        return _runeforgeRecipes.FirstOrDefault(r => r.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Get list of all runeforge recipe names
    /// </summary>
    public List<string> GetAllRuneforgeRecipeNames()
    {
        return _runeforgeRecipes.Select(r => r.Name).ToList();
    }
}
