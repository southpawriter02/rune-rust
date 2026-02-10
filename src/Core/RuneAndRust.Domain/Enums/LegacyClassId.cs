// ═══════════════════════════════════════════════════════════════════════════════
// LegacyClassId.cs
// Enum defining the six legacy character classes that are being removed and
// migrated to the Aethelgard archetype/specialization system in v0.20.0.
// Version: 0.20.0a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Identifies the six legacy character classes that are being removed and
/// migrated to the Aethelgard archetype/specialization system.
/// </summary>
/// <remarks>
/// <para>
/// These classes were the original character archetypes before the introduction
/// of the Aethelgard specialization system in v0.17.3+. Characters still
/// assigned to one of these classes require migration via
/// <see cref="RuneAndRust.Domain.Entities.CharacterMigration"/>.
/// </para>
/// <para>
/// Each legacy class maps to exactly one target <see cref="Archetype"/>:
/// </para>
/// <list type="bullet">
///   <item><description><see cref="Rogue"/> → <see cref="Archetype.Skirmisher"/></description></item>
///   <item><description><see cref="Fighter"/> → <see cref="Archetype.Warrior"/></description></item>
///   <item><description><see cref="Mage"/> → <see cref="Archetype.Mystic"/></description></item>
///   <item><description><see cref="Healer"/> → <see cref="Archetype.Adept"/></description></item>
///   <item><description><see cref="Scholar"/> → <see cref="Archetype.Adept"/></description></item>
///   <item><description><see cref="Crafter"/> → <see cref="Archetype.Adept"/></description></item>
/// </list>
/// <para>
/// Enum values are explicitly assigned (0-5) to ensure stable serialization
/// and database storage.
/// </para>
/// </remarks>
/// <seealso cref="Archetype"/>
/// <seealso cref="MigrationStatus"/>
public enum LegacyClassId
{
    /// <summary>
    /// Legacy Rogue class — stealth and precision combat.
    /// Migrates to <see cref="Archetype.Skirmisher"/>.
    /// </summary>
    /// <remarks>
    /// Suggested specializations: Myrk-gengr (shadow manipulation),
    /// Veiðimaðr (traditional hunter).
    /// </remarks>
    Rogue = 0,

    /// <summary>
    /// Legacy Fighter class — frontline melee combat.
    /// Migrates to <see cref="Archetype.Warrior"/>.
    /// </summary>
    /// <remarks>
    /// Suggested specializations: Skjaldmær (shield protector),
    /// Berserkr (fury-powered damage).
    /// </remarks>
    Fighter = 1,

    /// <summary>
    /// Legacy Mage class — arcane spellcasting.
    /// Migrates to <see cref="Archetype.Mystic"/>.
    /// </summary>
    /// <remarks>
    /// Suggested specialization: Seiðkona (traditional Aether manipulation).
    /// </remarks>
    Mage = 2,

    /// <summary>
    /// Legacy Healer class — restoration and support.
    /// Migrates to <see cref="Archetype.Adept"/>.
    /// </summary>
    /// <remarks>
    /// Suggested specialization: Bone-Setter (traditional medicine).
    /// </remarks>
    Healer = 3,

    /// <summary>
    /// Legacy Scholar class — knowledge and lore.
    /// Migrates to <see cref="Archetype.Adept"/>.
    /// </summary>
    /// <remarks>
    /// Suggested specialization: Jötun-Reader (ancient knowledge).
    /// </remarks>
    Scholar = 4,

    /// <summary>
    /// Legacy Crafter class — item creation and engineering.
    /// Migrates to <see cref="Archetype.Adept"/>.
    /// </summary>
    /// <remarks>
    /// Suggested specializations: Rúnasmiðr (rune crafting),
    /// Scrap-Tinker (gadget engineering).
    /// </remarks>
    Crafter = 5
}
