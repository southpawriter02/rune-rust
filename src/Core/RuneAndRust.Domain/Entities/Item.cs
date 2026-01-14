using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.Entities;

/// <summary>
/// Represents an item that can be found in the dungeon and collected by players.
/// </summary>
/// <remarks>
/// Items can be of various types (weapons, consumables, quest items, etc.) and may have
/// different values representing their worth or effectiveness.
/// </remarks>
public class Item : IEntity
{
    /// <summary>
    /// Gets the unique identifier for this item.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the display name of this item.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the description of this item shown to players.
    /// </summary>
    public string Description { get; private set; }

    /// <summary>
    /// Gets the type category of this item.
    /// </summary>
    public ItemType Type { get; private set; }

    /// <summary>
    /// Gets the value of this item (e.g., damage for weapons, healing for potions).
    /// </summary>
    public int Value { get; private set; }

    /// <summary>
    /// Gets the effect this item provides when used.
    /// </summary>
    public ItemEffect Effect { get; private set; }

    /// <summary>
    /// Gets the magnitude of the effect (healing amount, damage, buff value).
    /// </summary>
    public int EffectValue { get; private set; }

    /// <summary>
    /// Gets the duration of the effect in turns (0 for instant effects).
    /// </summary>
    public int EffectDuration { get; private set; }

    /// <summary>
    /// Gets the equipment slot this item can be equipped to, or null if not equippable.
    /// </summary>
    /// <remarks>
    /// Items without an equipment slot (consumables, quest items, misc) cannot be equipped.
    /// The slot determines which equipment position the item occupies when equipped.
    /// </remarks>
    public EquipmentSlot? EquipmentSlot { get; private set; }

    /// <summary>
    /// Gets whether this item can be equipped.
    /// </summary>
    public bool IsEquippable => EquipmentSlot.HasValue;

    /// <summary>
    /// Gets the damage dice notation for this weapon (e.g., "1d8", "2d6").
    /// </summary>
    /// <remarks>
    /// Only applicable to weapon items. Uses the DicePool.Parse() format from v0.0.5.
    /// Returns null for non-weapon items.
    /// </remarks>
    public string? DamageDice { get; private set; }

    /// <summary>
    /// Gets the type of weapon this item is, or null if not a weapon.
    /// </summary>
    /// <remarks>
    /// Weapon type affects combat characteristics and available bonuses.
    /// Items with WeaponType must also have EquipmentSlot.Weapon.
    /// </remarks>
    public WeaponType? WeaponType { get; private set; }

    /// <summary>
    /// Gets the stat bonuses provided by this weapon when equipped.
    /// </summary>
    /// <remarks>
    /// Bonuses are applied while the weapon is equipped and removed when unequipped.
    /// </remarks>
    public WeaponBonuses WeaponBonuses { get; private set; } = WeaponBonuses.None;

    /// <summary>
    /// Gets whether this item is a weapon.
    /// </summary>
    public bool IsWeapon => WeaponType.HasValue && EquipmentSlot == Enums.EquipmentSlot.Weapon;

    /// <summary>
    /// Gets the type of armor this item is, or null if not armor.
    /// </summary>
    /// <remarks>
    /// Armor type affects defense bonus amounts and any associated penalties.
    /// Items with ArmorType should have appropriate EquipmentSlot (Armor, Shield, Helmet, Boots).
    /// </remarks>
    public ArmorType? ArmorType { get; private set; }

    /// <summary>
    /// Gets the defense bonus provided by this item when equipped.
    /// </summary>
    /// <remarks>
    /// Defense bonus is added to the player's base defense. Stacks with other
    /// equipped items' defense bonuses.
    /// </remarks>
    public int DefenseBonus { get; private set; }

    /// <summary>
    /// Gets the stat modifiers provided by this item when equipped.
    /// </summary>
    /// <remarks>
    /// General-purpose stat modifiers that apply while equipped.
    /// Separate from weapon-specific bonuses (WeaponBonuses).
    /// </remarks>
    public StatModifiers StatModifiers { get; private set; } = StatModifiers.None;

    /// <summary>
    /// Gets the initiative penalty from this item (typically heavy armor).
    /// </summary>
    /// <remarks>
    /// Penalty is subtracted from initiative rolls, making the wearer act later in combat.
    /// Should be 0 or negative.
    /// </remarks>
    public int InitiativePenalty { get; private set; }

    /// <summary>
    /// Gets the requirements to equip this item.
    /// </summary>
    /// <remarks>
    /// If requirements are set, player must meet all conditions to equip the item.
    /// Null or empty requirements mean anyone can equip.
    /// </remarks>
    public EquipmentRequirements Requirements { get; private set; } = EquipmentRequirements.None;

    /// <summary>
    /// Gets whether this item is armor.
    /// </summary>
    public bool IsArmor => ArmorType.HasValue;

    /// <summary>
    /// Gets whether this item has any requirements.
    /// </summary>
    public bool HasRequirements => Requirements.HasRequirements;

    // ===== Key Properties (v0.4.0b) =====

    /// <summary>
    /// Gets the lock ID this key opens (for key items only).
    /// </summary>
    /// <remarks>
    /// Only applicable when Type is ItemType.Key.
    /// The KeyId must match a LockDefinition.RequiredKeyId to unlock.
    /// </remarks>
    public string? KeyId { get; private set; }

    /// <summary>
    /// Gets whether this item is a key.
    /// </summary>
    public bool IsKey => Type == ItemType.Key;

    /// <summary>
    /// Gets whether this key is consumed when used.
    /// </summary>
    /// <remarks>
    /// Single-use keys are removed from inventory after unlocking.
    /// Only applicable when Type is ItemType.Key.
    /// </remarks>
    public bool IsKeyConsumedOnUse { get; private set; }

    // ===== Range Properties (v0.5.1a) =====

    /// <summary>
    /// Gets the attack range of this weapon.
    /// </summary>
    /// <remarks>
    /// Default is 1 for melee weapons. Reach weapons have range 2,
    /// ranged weapons have configurable range (e.g., bow range 12).
    /// </remarks>
    public int Range { get; private set; } = 1;

    /// <summary>
    /// Gets the range type of this weapon.
    /// </summary>
    public RangeType RangeType { get; private set; } = RangeType.Melee;

    /// <summary>
    /// Sets the range properties for this weapon.
    /// </summary>
    /// <param name="range">The maximum attack range.</param>
    /// <param name="rangeType">The type of range validation.</param>
    public void SetRange(int range, RangeType rangeType)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(range, 1);
        Range = range;
        RangeType = rangeType;
    }

    /// <summary>
    /// Gets the effective range based on range type.
    /// </summary>
    /// <returns>1 for Melee, 2 for Reach, or Range for Ranged.</returns>
    public int GetEffectiveRange() => RangeType switch
    {
        RangeType.Melee => 1,
        RangeType.Reach => 2,
        RangeType.Ranged => Range,
        _ => 1
    };

    /// <summary>
    /// Checks if a target at the given distance is in range.
    /// </summary>
    /// <param name="distance">The distance to the target.</param>
    /// <returns>True if the target is in range.</returns>
    public bool IsInRange(int distance) => RangeType switch
    {
        RangeType.Melee => distance == 1,
        RangeType.Reach => distance >= 1 && distance <= 2,
        RangeType.Ranged => distance >= 1 && distance <= Range,
        _ => distance == 1
    };

    /// <summary>
    /// Gets the parsed damage dice pool for combat calculations.
    /// </summary>
    /// <returns>The DicePool for this weapon, or null if not a weapon or no damage dice.</returns>
    public DicePool? GetDamageDicePool()
    {
        if (string.IsNullOrWhiteSpace(DamageDice))
            return null;

        return DicePool.Parse(DamageDice);
    }

    /// <summary>
    /// Private parameterless constructor for Entity Framework Core.
    /// </summary>
    private Item()
    {
        Name = null!;
        Description = null!;
    }

    /// <summary>
    /// Creates a new item with the specified properties.
    /// </summary>
    /// <param name="name">The display name of the item.</param>
    /// <param name="description">The description shown to players.</param>
    /// <param name="type">The type category of the item.</param>
    /// <param name="value">The value of the item (default is 0).</param>
    /// <param name="effect">The effect this item provides when used (default is None).</param>
    /// <param name="effectValue">The magnitude of the effect (default is 0).</param>
    /// <param name="effectDuration">The duration of the effect in turns (default is 0).</param>
    /// <param name="equipmentSlot">The equipment slot this item can be equipped to, or null if not equippable.</param>
    /// <param name="damageDice">The damage dice notation for weapons (e.g., "1d8").</param>
    /// <param name="weaponType">The type of weapon, or null if not a weapon.</param>
    /// <param name="weaponBonuses">The stat bonuses provided by this weapon.</param>
    /// <param name="armorType">The type of armor, or null if not armor.</param>
    /// <param name="defenseBonus">Defense bonus when equipped.</param>
    /// <param name="statModifiers">Stat modifiers when equipped.</param>
    /// <param name="initiativePenalty">Initiative penalty when equipped.</param>
    /// <param name="requirements">Requirements to equip this item.</param>
    /// <param name="keyId">The lock ID this key opens (for key items only).</param>
    /// <param name="isKeyConsumedOnUse">Whether the key is consumed when used.</param>
    /// <exception cref="ArgumentNullException">Thrown when name or description is null.</exception>
    public Item(string name, string description, ItemType type, int value = 0,
                ItemEffect effect = ItemEffect.None, int effectValue = 0, int effectDuration = 0,
                EquipmentSlot? equipmentSlot = null,
                string? damageDice = null,
                WeaponType? weaponType = null,
                WeaponBonuses? weaponBonuses = null,
                ArmorType? armorType = null,
                int defenseBonus = 0,
                StatModifiers? statModifiers = null,
                int initiativePenalty = 0,
                EquipmentRequirements? requirements = null,
                string? keyId = null,
                bool isKeyConsumedOnUse = false)
    {
        Id = Guid.NewGuid();
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        Type = type;
        Value = value;
        Effect = effect;
        EffectValue = effectValue;
        EffectDuration = effectDuration;
        EquipmentSlot = equipmentSlot;
        DamageDice = damageDice;
        WeaponType = weaponType;
        WeaponBonuses = weaponBonuses ?? WeaponBonuses.None;
        ArmorType = armorType;
        DefenseBonus = defenseBonus;
        StatModifiers = statModifiers ?? StatModifiers.None;
        InitiativePenalty = initiativePenalty;
        Requirements = requirements ?? EquipmentRequirements.None;
        KeyId = keyId;
        IsKeyConsumedOnUse = isKeyConsumedOnUse;
    }

    /// <summary>
    /// Factory method to create a basic sword weapon.
    /// </summary>
    /// <returns>A new sword item.</returns>
    public static Item CreateSword() => new(
        "Rusty Sword",
        "An old sword covered in rust. Still sharp enough to cut.",
        ItemType.Weapon,
        value: 5,
        equipmentSlot: Enums.EquipmentSlot.Weapon,
        damageDice: "1d8",
        weaponType: Enums.WeaponType.Sword
    );

    /// <summary>
    /// Factory method to create an iron sword weapon.
    /// </summary>
    /// <returns>A new iron sword item.</returns>
    public static Item CreateIronSword() => new(
        "Iron Sword",
        "A standard iron sword. Reliable and balanced.",
        ItemType.Weapon,
        value: 50,
        equipmentSlot: Enums.EquipmentSlot.Weapon,
        damageDice: "1d8",
        weaponType: Enums.WeaponType.Sword
    );

    /// <summary>
    /// Factory method to create a battle axe weapon.
    /// </summary>
    /// <returns>A new battle axe item with attack penalty.</returns>
    public static Item CreateBattleAxe() => new(
        "Battle Axe",
        "A heavy two-handed axe. Hits hard but swings slow.",
        ItemType.Weapon,
        value: 75,
        equipmentSlot: Enums.EquipmentSlot.Weapon,
        damageDice: "1d10",
        weaponType: Enums.WeaponType.Axe,
        weaponBonuses: WeaponBonuses.ForAttack(-1)
    );

    /// <summary>
    /// Factory method to create a steel dagger weapon.
    /// </summary>
    /// <returns>A new steel dagger item with Finesse bonus.</returns>
    public static Item CreateSteelDagger() => new(
        "Steel Dagger",
        "A quick, precise blade favored by rogues.",
        ItemType.Weapon,
        value: 40,
        equipmentSlot: Enums.EquipmentSlot.Weapon,
        damageDice: "1d4",
        weaponType: Enums.WeaponType.Dagger,
        weaponBonuses: WeaponBonuses.ForFinesse(2)
    );

    /// <summary>
    /// Factory method to create an oak staff weapon.
    /// </summary>
    /// <returns>A new oak staff item with Will bonus.</returns>
    public static Item CreateOakStaff() => new(
        "Oak Staff",
        "A sturdy wooden staff imbued with minor magic.",
        ItemType.Weapon,
        value: 45,
        equipmentSlot: Enums.EquipmentSlot.Weapon,
        damageDice: "1d6",
        weaponType: Enums.WeaponType.Staff,
        weaponBonuses: WeaponBonuses.ForWill(2)
    );

    /// <summary>
    /// Factory method to create a quest scroll item.
    /// </summary>
    /// <returns>A new scroll item.</returns>
    public static Item CreateScroll() => new(
        "Ancient Scroll",
        "A scroll with mysterious runes. You can't quite make out what it says.",
        ItemType.Quest,
        0
    );

    /// <summary>
    /// Factory method to create a health potion consumable.
    /// </summary>
    /// <returns>A new health potion item.</returns>
    public static Item CreateHealthPotion() => new(
        "Health Potion",
        "A vial of red liquid that restores health when consumed.",
        ItemType.Consumable,
        value: 25,
        effect: ItemEffect.Heal,
        effectValue: 25
    );

    /// <summary>
    /// Factory method to create basic leather armor.
    /// </summary>
    /// <returns>A new light armor item with defense bonus.</returns>
    public static Item CreateLeatherArmor() => new(
        "Leather Armor",
        "Simple armor made of tanned leather. Provides basic protection.",
        ItemType.Armor,
        value: 30,
        equipmentSlot: Enums.EquipmentSlot.Armor,
        armorType: Enums.ArmorType.Light,
        defenseBonus: 2
    );

    /// <summary>
    /// Factory method to create chain mail armor.
    /// </summary>
    /// <returns>A new medium armor item with defense bonus and initiative penalty.</returns>
    public static Item CreateChainMail() => new(
        "Chain Mail",
        "Interlocking metal rings providing solid protection.",
        ItemType.Armor,
        value: 100,
        equipmentSlot: Enums.EquipmentSlot.Armor,
        armorType: Enums.ArmorType.Medium,
        defenseBonus: 4,
        initiativePenalty: -1,
        requirements: EquipmentRequirements.ForFortitude(12)
    );

    /// <summary>
    /// Factory method to create plate armor.
    /// </summary>
    /// <returns>A new heavy armor item with high defense and requirements.</returns>
    public static Item CreatePlateArmor() => new(
        "Plate Armor",
        "Full plate armor offering maximum protection at the cost of mobility.",
        ItemType.Armor,
        value: 500,
        equipmentSlot: Enums.EquipmentSlot.Armor,
        armorType: Enums.ArmorType.Heavy,
        defenseBonus: 6,
        initiativePenalty: -3,
        requirements: new EquipmentRequirements { MinFortitude = 14, MinMight = 12 }
    );

    /// <summary>
    /// Factory method to create a basic wooden shield.
    /// </summary>
    /// <returns>A new shield item with defense bonus.</returns>
    public static Item CreateWoodenShield() => new(
        "Wooden Shield",
        "A basic wooden shield. Better than nothing.",
        ItemType.Armor,
        value: 15,
        equipmentSlot: Enums.EquipmentSlot.Shield,
        armorType: Enums.ArmorType.Light,
        defenseBonus: 1
    );

    /// <summary>
    /// Factory method to create a basic iron helmet.
    /// </summary>
    /// <returns>A new helmet item with defense bonus.</returns>
    public static Item CreateIronHelmet() => new(
        "Iron Helmet",
        "A sturdy iron helmet that protects your head.",
        ItemType.Armor,
        value: 40,
        equipmentSlot: Enums.EquipmentSlot.Helmet,
        armorType: Enums.ArmorType.Medium,
        defenseBonus: 2
    );

    /// <summary>
    /// Factory method to create a ring of strength.
    /// </summary>
    /// <returns>A new ring item with Might bonus.</returns>
    public static Item CreateRingOfStrength() => new(
        "Ring of Strength",
        "A silver ring that enhances the wearer's physical power.",
        ItemType.Misc,
        value: 200,
        equipmentSlot: Enums.EquipmentSlot.Ring,
        statModifiers: new StatModifiers { Might = 2 }
    );

    /// <summary>
    /// Factory method to create an amulet of vitality.
    /// </summary>
    /// <returns>A new amulet item with Fortitude and MaxHealth bonus.</returns>
    public static Item CreateAmuletOfVitality() => new(
        "Amulet of Vitality",
        "A ruby amulet that bolsters the wearer's constitution.",
        ItemType.Misc,
        value: 250,
        equipmentSlot: Enums.EquipmentSlot.Amulet,
        statModifiers: new StatModifiers { Fortitude = 2, MaxHealth = 10 }
    );

    // ===== Key Factory Methods (v0.4.0b) =====

    /// <summary>
    /// Factory method to create a key item.
    /// </summary>
    /// <param name="name">The display name of the key.</param>
    /// <param name="description">The description of the key.</param>
    /// <param name="keyId">The lock ID this key opens.</param>
    /// <param name="consumeOnUse">Whether the key is consumed when used.</param>
    /// <returns>A new key item.</returns>
    public static Item CreateKey(
        string name,
        string description,
        string keyId,
        bool consumeOnUse = false) => new(
        name,
        description,
        ItemType.Key,
        value: 0,
        keyId: keyId,
        isKeyConsumedOnUse: consumeOnUse
    );

    /// <summary>
    /// Factory method to create a basic iron key.
    /// </summary>
    /// <param name="keyId">The lock ID this key opens.</param>
    /// <returns>A new iron key item.</returns>
    public static Item CreateIronKey(string keyId) => CreateKey(
        "Iron Key",
        "A simple iron key. It must open something.",
        keyId
    );

    /// <summary>
    /// Factory method to create an ornate key.
    /// </summary>
    /// <param name="keyId">The lock ID this key opens.</param>
    /// <returns>A new ornate key item.</returns>
    public static Item CreateOrnateKey(string keyId) => CreateKey(
        "Ornate Key",
        "An elaborate key decorated with intricate patterns.",
        keyId
    );

    /// <summary>
    /// Returns the name of this item.
    /// </summary>
    /// <returns>The item name.</returns>
    public override string ToString() => Name;
}
