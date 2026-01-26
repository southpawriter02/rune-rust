namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Examination depth layers requiring progressively higher WITS checks.
/// </summary>
public enum ExaminationLayer
{
    /// <summary>No check required - basic visual description.</summary>
    Cursory = 1,

    /// <summary>DC 12 - Functional details, condition, hints.</summary>
    Detailed = 2,

    /// <summary>DC 18 - Historical context, technical details, secrets.</summary>
    Expert = 3
}

/// <summary>
/// Categories of objects that can be examined.
/// </summary>
/// <remarks>
/// <para>
/// Categories provide the primary filter for descriptor lookup. Each category
/// contains specific object types with layered examination text.
/// </para>
/// <para>
/// Added in v0.15.6c: Furniture, Environmental, Creature categories.
/// </para>
/// </remarks>
public enum ObjectCategory
{
    /// <summary>Entry points and barriers (LockedDoor, BlastDoor, SecurityDoor).</summary>
    Door,

    /// <summary>Technical equipment (Console, Terminal, Servitor, PowerUnit).</summary>
    Machinery,

    /// <summary>Artistic and historical objects (Inscription, Mural, Statue).</summary>
    Decorative,

    /// <summary>Storage objects (Chest, Locker, Crate, Safe).</summary>
    Container,

    /// <summary>Structural elements (Wall, Floor, Pillar). Legacy alias for Environmental.</summary>
    Structural,

    /// <summary>Plant life and fungi.</summary>
    Flora,

    /// <summary>Animal life and creatures.</summary>
    Fauna,

    /// <summary>Deceased creatures and remains.</summary>
    Corpse,

    /// <summary>Written text and carvings.</summary>
    Inscription,

    /// <summary>Portable items and equipment.</summary>
    Item,

    /// <summary>Functional furniture (Table, Chair, Bed, Workbench, Shelf). Added in v0.15.6c.</summary>
    Furniture = 11,

    /// <summary>Room structural features (Wall, Floor, Ceiling, Vent, Pipe). Added in v0.15.6c.</summary>
    Environmental = 12,

    /// <summary>Living or deceased creatures and remains. Added in v0.15.6c.</summary>
    Creature = 13
}

/// <summary>
/// Environmental biomes affecting flora, fauna, and descriptor selection.
/// </summary>
public enum Biome
{
    /// <summary>Lower levels with fungal growth and humid conditions.</summary>
    TheRoots,

    /// <summary>Volcanic areas with extreme heat.</summary>
    Muspelheim,

    /// <summary>Frozen regions with sub-zero temperatures.</summary>
    Niflheim,

    /// <summary>Blight-corrupted areas with paradoxical phenomena.</summary>
    Alfheim,

    /// <summary>Standard dungeon areas of the citadel.</summary>
    Citadel,

    /// <summary>Outside surface areas.</summary>
    Surface,

    /// <summary>Giant ruins with massive architecture and runic power.</summary>
    Jotunheim
}

/// <summary>
/// Success levels for perception checks.
/// </summary>
public enum PerceptionSuccessLevel
{
    /// <summary>Standard success - basic detection.</summary>
    Standard,

    /// <summary>Expert success - additional context and details.</summary>
    Expert
}

/// <summary>
/// Categories for flora and fauna descriptors.
/// </summary>
public enum FloraFaunaCategory
{
    Flora,
    Fauna
}

/// <summary>
/// Categories for interaction descriptors.
/// </summary>
public enum InteractionCategory
{
    MechanicalObject,
    Container,
    WitsSuccess,
    WitsFailure,
    Discovery,
    ContainerInteraction,
    SkillSpecific,
    Environmental
}

/// <summary>
/// Success margins for WITS checks.
/// </summary>
public enum WitsCheckMargin
{
    Low,
    Medium,
    High,
    Critical
}

/// <summary>
/// Failure severity for WITS checks.
/// </summary>
public enum WitsFailureMargin
{
    NearMiss,
    Failure,
    Bad,
    CriticalFail
}

/// <summary>
/// Quality tiers for loot discovery.
/// </summary>
public enum LootQuality
{
    Poor,
    Average,
    Good,
    Excellent,
    Jackpot
}

/// <summary>
/// Age/state of corpses for examination.
/// </summary>
public enum CorpseAge
{
    Fresh,
    Recent,
    Old,
    Ancient,
    Forlorn,
    Stripped
}

/// <summary>
/// States for containers.
/// </summary>
public enum ContainerState
{
    Intact,
    Damaged,
    Opened,
    Trapped
}

/// <summary>
/// States for doors.
/// </summary>
public enum DoorState
{
    Open,
    Closed,
    Locked,
    Destroyed,
    Rusted
}

/// <summary>
/// States for levers.
/// </summary>
public enum LeverState
{
    Active,
    Inactive,
    Stuck,
    Unknown
}

/// <summary>
/// States for terminals/oracle-boxes.
/// </summary>
public enum TerminalState
{
    Active,
    Dormant,
    Corrupted,
    Dead,
    Glitched
}

/// <summary>
/// Crafting trade skills.
/// </summary>
public enum TradeSkill
{
    Bodging,
    FieldMedicine,
    Runeforging
}

/// <summary>
/// Character specializations.
/// </summary>
public enum Specialization
{
    JotunReader,
    RuinStalker
}

/// <summary>
/// Types of discovery for perception checks.
/// </summary>
public enum DiscoveryType
{
    Secret,
    Lore,
    Danger
}

/// <summary>
/// Intensity levels for secret discoveries.
/// </summary>
public enum SecretIntensity
{
    Minor,
    Moderate,
    Major
}

/// <summary>
/// Types of lore discoveries.
/// </summary>
public enum LoreType
{
    DataSlate,
    Inscription,
    Art,
    Note
}

/// <summary>
/// Types of danger discoveries.
/// </summary>
public enum DangerType
{
    Trap,
    Ambush,
    Hazard
}

/// <summary>
/// Types of container opening actions.
/// </summary>
public enum OpeningAction
{
    Unlock,
    Force,
    Bypass,
    Fail,
    TrapTrigger
}

/// <summary>
/// Types of resource nodes.
/// </summary>
public enum ResourceNodeType
{
    OreVein,
    SalvagePile,
    ScrapCluster,
    RareFind
}

/// <summary>
/// Types of environmental traversal.
/// </summary>
public enum TraversalResult
{
    Easy,
    Standard,
    Difficult,
    Fail
}

/// <summary>
/// Types of repair/disable actions.
/// </summary>
public enum RepairAction
{
    DisableTrap,
    Repair,
    Sabotage,
    Catastrophic
}

/// <summary>
/// Types of object-specific examination discoveries.
/// </summary>
public enum TechExamination
{
    Function,
    Status,
    Origin,
    Danger
}

/// <summary>
/// Types of body examination discoveries.
/// </summary>
public enum BodyExamination
{
    Cause,
    Identity,
    Loot,
    Warning
}

/// <summary>
/// Types of environment examination discoveries.
/// </summary>
public enum EnvironmentExamination
{
    Hidden,
    Trap,
    Path,
    Hazard
}
