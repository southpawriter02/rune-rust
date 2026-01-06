namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Categories for descriptor fragments used in three-tier room description composition.
/// </summary>
public enum FragmentCategory
{
    /// <summary>Room size/feel descriptions (8+ fragments).</summary>
    Spatial,

    /// <summary>Wall/ceiling/floor features (12+ fragments).</summary>
    Architectural,

    /// <summary>Ambient details - decay, runes, activity, ominous, loot (28+ fragments).</summary>
    Detail,

    /// <summary>Sensory elements - smell, sound, light, temperature (155+ fragments).</summary>
    Atmospheric,

    /// <summary>Direction phrases (6+ fragments).</summary>
    Direction
}

/// <summary>
/// Subcategories for Detail fragments.
/// </summary>
public enum DetailSubcategory
{
    /// <summary>Decay and deterioration descriptions.</summary>
    Decay,

    /// <summary>Runic inscriptions and magical markings.</summary>
    Runes,

    /// <summary>Signs of activity or habitation.</summary>
    Activity,

    /// <summary>Ominous or foreboding elements.</summary>
    Ominous,

    /// <summary>Loot and treasure hints.</summary>
    Loot
}

/// <summary>
/// Subcategories for Atmospheric fragments.
/// </summary>
public enum AtmosphericSubcategory
{
    /// <summary>Olfactory descriptions.</summary>
    Smell,

    /// <summary>Auditory descriptions.</summary>
    Sound,

    /// <summary>Visual/lighting descriptions.</summary>
    Light,

    /// <summary>Temperature and tactile descriptions.</summary>
    Temperature
}

/// <summary>
/// Subcategories for Architectural fragments.
/// </summary>
public enum ArchitecturalSubcategory
{
    /// <summary>Wall descriptions.</summary>
    Wall,

    /// <summary>Ceiling descriptions.</summary>
    Ceiling,

    /// <summary>Floor descriptions.</summary>
    Floor
}

/// <summary>
/// Room size categories for base templates.
/// </summary>
public enum RoomSize
{
    /// <summary>Small rooms like corridors and secret rooms.</summary>
    Small,

    /// <summary>Medium rooms like junctions and maintenance hubs.</summary>
    Medium,

    /// <summary>Large rooms like chambers and storage bays.</summary>
    Large,

    /// <summary>Extra large rooms like boss arenas.</summary>
    XLarge
}
