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
/// Manages crafting system for Field Medicine and other craftables
/// </summary>
public class CraftingService
{
    private readonly DiceService _diceService;
    private readonly List<CraftingRecipe> _recipes = new();

    public CraftingService(DiceService? diceService = null)
    {
        _diceService = diceService ?? new DiceService();
        InitializeRecipes();
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
}
