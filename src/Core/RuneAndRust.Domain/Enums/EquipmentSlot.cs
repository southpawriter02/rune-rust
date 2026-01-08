namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Defines the equipment slots available for equipping items.
/// </summary>
/// <remarks>
/// <para>Each slot can hold exactly one item at a time. Items must have a matching
/// EquipmentSlot property to be equipped to that slot.</para>
/// <para>The slot determines where on the character the item is worn/wielded:</para>
/// <list type="bullet">
/// <item>Weapon - Primary hand for offensive equipment</item>
/// <item>Armor - Body protection</item>
/// <item>Shield - Off-hand defensive equipment</item>
/// <item>Helmet - Head protection</item>
/// <item>Boots - Foot protection</item>
/// <item>Ring - Finger accessory</item>
/// <item>Amulet - Neck accessory</item>
/// </list>
/// </remarks>
public enum EquipmentSlot
{
    /// <summary>Primary weapon slot for swords, axes, daggers, staffs, etc.</summary>
    Weapon,

    /// <summary>Body armor slot for chest protection.</summary>
    Armor,

    /// <summary>Off-hand slot for shields or secondary items.</summary>
    Shield,

    /// <summary>Head protection slot for helmets, hoods, etc.</summary>
    Helmet,

    /// <summary>Footwear slot for boots, shoes, etc.</summary>
    Boots,

    /// <summary>Finger accessory slot for rings.</summary>
    Ring,

    /// <summary>Neck accessory slot for amulets, necklaces, etc.</summary>
    Amulet
}
