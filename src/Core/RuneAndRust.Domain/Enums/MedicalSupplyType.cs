namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Enumeration of all Medical Supply item types for the Bone-Setter specialization.
/// Supplies are consumable inventory items with quality-based healing bonuses.
/// </summary>
/// <remarks>
/// <para>Medical Supplies are the Bone-Setter's special resource — a consumable inventory
/// of medical materials acquired through salvage, purchase, or crafting. Each supply type
/// serves a different medical purpose, and all types can be consumed by healing abilities.</para>
/// <para>Quality rating (1–5) determines the healing bonus provided by each supply item,
/// calculated as Quality - 1 (range: 0–4 bonus HP).</para>
/// </remarks>
public enum MedicalSupplyType
{
    /// <summary>
    /// Bandage — Basic wound treatment wrappings.
    /// The most common and versatile medical supply.
    /// Used as the default supply type for Field Dressing.
    /// </summary>
    Bandage = 0,

    /// <summary>
    /// Salve — Topical healing cream for burns and abrasions.
    /// Effective for surface wounds and skin injuries.
    /// </summary>
    Salve = 1,

    /// <summary>
    /// Splint — Bone and joint stabilization materials.
    /// Used for structural injuries including fractures and dislocations.
    /// </summary>
    Splint = 2,

    /// <summary>
    /// Suture — Wound closure materials for deep lacerations.
    /// Used for surgical closures and deep wound treatment.
    /// </summary>
    Suture = 3,

    /// <summary>
    /// Herbs — Plant-based remedies with broad medicinal applications.
    /// Required component for Antidote Craft ability (v0.20.6b).
    /// </summary>
    Herbs = 4,

    /// <summary>
    /// Antidote — Poison and disease treatment compound.
    /// Crafted from Herbs + salvage via Antidote Craft (v0.20.6b).
    /// Cures one poison effect when applied to a target.
    /// </summary>
    Antidote = 5,

    /// <summary>
    /// Stimulant — Emergency combat stimulant for revival.
    /// Required by Resuscitate ability (v0.20.6c) to revive unconscious allies.
    /// Can be salvaged from World-Machine ruins or purchased.
    /// </summary>
    Stimulant = 6
}
