using RuneAndRust.Core;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.16 Ability Database
/// Manages the expanded ability library for Warrior archetype
/// Abilities unlock based on Legend tier
/// </summary>
public class AbilityDatabase
{
    private readonly Dictionary<string, Ability> _abilities = new();
    private readonly Dictionary<int, List<string>> _abilitiesByLegendTier = new();

    public AbilityDatabase()
    {
        InitializeAbilities();
    }

    private void InitializeAbilities()
    {
        // Tier 1 abilities (Legend 1-2)
        AddAbility(CreateCrushingBlow(), legendTier: 1);
        AddAbility(CreateRallyCry(), legendTier: 1);

        // Tier 2 abilities (Legend 3-4)
        AddAbility(CreateWhirlwindStrike(), legendTier: 2);
        AddAbility(CreateSecondWind(), legendTier: 2);
        AddAbility(CreateArmorBreaker(), legendTier: 2);
        AddAbility(CreateIntimidatingPresence(), legendTier: 2);

        // Tier 3 abilities (Legend 5-6)
        AddAbility(CreateUnstoppable(), legendTier: 3);
        AddAbility(CreateExecute(), legendTier: 3);
        AddAbility(CreateBulwark(), legendTier: 3);

        // Tier 4 abilities (Legend 7+)
        AddAbility(CreateTitansStrength(), legendTier: 4);
        AddAbility(CreateLastStand(), legendTier: 4);

        // Heretical abilities (Corruption-based, available at any level if Corruption threshold met)
        AddAbility(CreateEmbraceTheMachine(), legendTier: 2);
        AddAbility(CreateJotunReadersGift(), legendTier: 3);
        AddAbility(CreateSymbioticRegeneration(), legendTier: 2);
    }

    private void AddAbility(Ability ability, int legendTier)
    {
        _abilities[ability.Name] = ability;

        if (!_abilitiesByLegendTier.ContainsKey(legendTier))
        {
            _abilitiesByLegendTier[legendTier] = new List<string>();
        }
        _abilitiesByLegendTier[legendTier].Add(ability.Name);
    }

    /// <summary>
    /// Get all abilities available at a specific Legend tier
    /// </summary>
    public List<Ability> GetAbilitiesForLegendTier(int legendTier)
    {
        if (!_abilitiesByLegendTier.ContainsKey(legendTier))
            return new List<Ability>();

        return _abilitiesByLegendTier[legendTier]
            .Select(name => _abilities[name])
            .ToList();
    }

    /// <summary>
    /// Get a specific ability by name
    /// </summary>
    public Ability? GetAbility(string abilityName)
    {
        return _abilities.GetValueOrDefault(abilityName);
    }

    /// <summary>
    /// Get all abilities in the database
    /// </summary>
    public List<Ability> GetAllAbilities()
    {
        return _abilities.Values.ToList();
    }

    #region Tier 1 Abilities (Legend 1-2)

    private Ability CreateCrushingBlow()
    {
        return new Ability
        {
            Name = "Crushing Blow",
            Description = "A devastating overhead strike that sends foes crashing to the ground. Knocks target prone.",
            StaminaCost = 15,
            Type = AbilityType.Attack,
            AttributeUsed = "might",
            BonusDice = 1,
            SuccessThreshold = 2,
            DamageDice = 2, // 2d6 + MIGHT
            MaxRank = 3,
            CostToRank2 = 5,
            CostToRank3 = 0
        };
    }

    private Ability CreateRallyCry()
    {
        return new Ability
        {
            Name = "Rally Cry",
            Description = "Your commanding shout inspires your companions to fight on. All allies within 15ft heal 1d8 HP, gain +1 to next attack, and reduce Stress by 5.",
            StaminaCost = 20,
            Type = AbilityType.Utility,
            AttributeUsed = "will",
            BonusDice = 0,
            SuccessThreshold = 2,
            MaxRank = 3,
            CostToRank2 = 5,
            CostToRank3 = 0
        };
    }

    #endregion

    #region Tier 2 Abilities (Legend 3-4)

    private Ability CreateWhirlwindStrike()
    {
        return new Ability
        {
            Name = "Whirlwind Strike",
            Description = "Spin in a devastating arc, your blade finding every nearby foe. Attack all enemies within 5ft for 1d8+MIGHT damage each.",
            StaminaCost = 25,
            Type = AbilityType.Attack,
            AttributeUsed = "might",
            BonusDice = 0,
            SuccessThreshold = 2,
            DamageDice = 1, // 1d8 per target (≈ 1d6+1)
            MaxRank = 3,
            CostToRank2 = 5,
            CostToRank3 = 0
        };
    }

    private Ability CreateSecondWind()
    {
        return new Ability
        {
            Name = "Second Wind",
            Description = "Dig deep. You're not done yet. Heal 2d10+WILL HP, remove one [Wounded] condition. Once per combat.",
            StaminaCost = 20,
            Type = AbilityType.Utility,
            AttributeUsed = "will",
            BonusDice = 0,
            SuccessThreshold = 0, // Auto-success
            MaxRank = 3,
            CostToRank2 = 5,
            CostToRank3 = 0
        };
    }

    private Ability CreateArmorBreaker()
    {
        return new Ability
        {
            Name = "Armor Breaker",
            Description = "Strike at the weak points in your enemy's defenses. Attack ignores target's armor, target's Defense reduced by 3 for 2 turns.",
            StaminaCost = 20,
            Type = AbilityType.Attack,
            AttributeUsed = "might",
            BonusDice = 1,
            SuccessThreshold = 2,
            DamageDice = 2, // 2d6 + MIGHT, ignores armor
            IgnoresArmor = true,
            MaxRank = 3,
            CostToRank2 = 5,
            CostToRank3 = 0
        };
    }

    private Ability CreateIntimidatingPresence()
    {
        return new Ability
        {
            Name = "Intimidating Presence",
            Description = "Your mere presence radiates menace. All enemies within 10ft must pass WILL save or become [Frightened] (disadvantage on attacks).",
            StaminaCost = 15,
            Type = AbilityType.Control,
            AttributeUsed = "will",
            BonusDice = 2,
            SuccessThreshold = 2,
            MaxRank = 3,
            CostToRank2 = 5,
            CostToRank3 = 0
        };
    }

    #endregion

    #region Tier 3 Abilities (Legend 5-6)

    private Ability CreateUnstoppable()
    {
        return new Ability
        {
            Name = "Unstoppable",
            Description = "Nothing will stop you. Nothing. For 3 turns: immune to crowd control, take half damage, +2 to all attacks. Once per combat.",
            StaminaCost = 30,
            Type = AbilityType.Utility,
            AttributeUsed = "",
            BonusDice = 0,
            SuccessThreshold = 0, // Auto-success
            DefenseDuration = 3,
            MaxRank = 1, // Ultimate abilities don't rank up
            CostToRank2 = 0,
            CostToRank3 = 0
        };
    }

    private Ability CreateExecute()
    {
        return new Ability
        {
            Name = "Execute",
            Description = "A killing blow for weakened foes. If target below 30% HP, instant kill. Otherwise, 2d10+MIGHT damage.",
            StaminaCost = 25,
            Type = AbilityType.Attack,
            AttributeUsed = "might",
            BonusDice = 2,
            SuccessThreshold = 2,
            DamageDice = 2, // 2d10 (represented as 2d6+2)
            MaxRank = 3,
            CostToRank2 = 5,
            CostToRank3 = 0
        };
    }

    private Ability CreateBulwark()
    {
        return new Ability
        {
            Name = "Bulwark",
            Description = "You are the wall. They will break against you. Stance - All enemies within 15ft must target you. +5 Defense.",
            StaminaCost = 0, // Stances don't cost stamina
            Type = AbilityType.Defense,
            AttributeUsed = "",
            BonusDice = 0,
            SuccessThreshold = 0, // Auto-success
            MaxRank = 1, // Stances don't rank up
            CostToRank2 = 0,
            CostToRank3 = 0
        };
    }

    #endregion

    #region Tier 4 Abilities (Legend 7+)

    private Ability CreateTitansStrength()
    {
        return new Ability
        {
            Name = "Titan's Strength",
            Description = "Channel the strength of the ancient colossi. For 4 turns: double MIGHT attribute, all attacks deal +10 damage, cannot be moved. Once per combat.",
            StaminaCost = 40,
            Type = AbilityType.Utility,
            AttributeUsed = "",
            BonusDice = 0,
            SuccessThreshold = 0, // Auto-success
            DefenseDuration = 4,
            MaxRank = 1, // Ultimate abilities don't rank up
            CostToRank2 = 0,
            CostToRank3 = 0
        };
    }

    private Ability CreateLastStand()
    {
        return new Ability
        {
            Name = "Last Stand",
            Description = "You will NOT fall. Not here. Not now. When HP drops to 0, instead drop to 1 HP and gain 50 temp HP. Lasts 5 turns. Once per day.",
            StaminaCost = 35,
            Type = AbilityType.Defense,
            AttributeUsed = "",
            BonusDice = 0,
            SuccessThreshold = 0, // Auto-success
            DefenseDuration = 5,
            MaxRank = 1, // Ultimate abilities don't rank up
            CostToRank2 = 0,
            CostToRank3 = 0
        };
    }

    #endregion

    #region Heretical Abilities (Corruption-based)

    private Ability CreateEmbraceTheMachine()
    {
        return new Ability
        {
            Name = "Embrace the Machine",
            Description = "Let the machine logic flow through you. Efficient. Optimal. +3 to all Tech checks, +2 Defense (cold logic), lasts 1 hour. Cost: 5 Corruption (permanent).",
            StaminaCost = 20,
            Type = AbilityType.Utility,
            AttributeUsed = "",
            BonusDice = 0,
            SuccessThreshold = 0, // Auto-success
            MaxRank = 1, // Heretical abilities don't rank up
            CostToRank2 = 0,
            CostToRank3 = 0
        };
    }

    private Ability CreateJotunReadersGift()
    {
        return new Ability
        {
            Name = "Jötun-Reader's Gift",
            Description = "Touch their mind with alien thought. Watch them break. Ranged psychic attack, 3d8 damage, ignores armor, target gains 5 Corruption. Cost: 8 Corruption.",
            StaminaCost = 30,
            Type = AbilityType.Attack,
            AttributeUsed = "will",
            BonusDice = 3,
            SuccessThreshold = 2,
            DamageDice = 3, // 3d8 (represented as 3d6+3)
            IgnoresArmor = true,
            MaxRank = 1, // Heretical abilities don't rank up
            CostToRank2 = 0,
            CostToRank3 = 0
        };
    }

    private Ability CreateSymbioticRegeneration()
    {
        return new Ability
        {
            Name = "Symbiotic Regeneration",
            Description = "Let the fungus in. It will repair you. It always repairs you. Heal 3d10 HP, gain [Infected] status for 3 turns (immune to healing but +2 MIGHT). Cost: 6 Corruption.",
            StaminaCost = 25,
            Type = AbilityType.Utility,
            AttributeUsed = "",
            BonusDice = 0,
            SuccessThreshold = 0, // Auto-success
            MaxRank = 1, // Heretical abilities don't rank up
            CostToRank2 = 0,
            CostToRank3 = 0
        };
    }

    #endregion
}
