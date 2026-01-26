namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Defines the type of perception system an ability affects.
/// </summary>
public enum PerceptionAbilityType
{
    /// <summary>
    /// Affects passive perception calculations (IPassivePerceptionService).
    /// </summary>
    PassivePerception,

    /// <summary>
    /// Affects examination checks and results (IExaminationService).
    /// </summary>
    Examination,

    /// <summary>
    /// Affects investigation checks and clue discovery (IInvestigationService).
    /// </summary>
    Investigation,

    /// <summary>
    /// Affects search operations (ISearchService).
    /// </summary>
    Search
}

/// <summary>
/// Defines the type of condition check for ability activation.
/// </summary>
public enum ConditionType
{
    /// <summary>
    /// Ability activates based on object category being examined.
    /// </summary>
    ObjectCategory,

    /// <summary>
    /// Ability activates based on specific object type.
    /// </summary>
    ObjectType,

    /// <summary>
    /// Ability activates based on hidden element type.
    /// </summary>
    ElementType,

    /// <summary>
    /// Ability activates based on investigation target type.
    /// </summary>
    InvestigationTarget,

    /// <summary>
    /// Ability activates based on proximity to elements.
    /// </summary>
    Proximity,

    /// <summary>
    /// Ability is always active regardless of conditions.
    /// </summary>
    Always,

    /// <summary>
    /// Ability activates based on object tags.
    /// </summary>
    ObjectTags
}

/// <summary>
/// Defines the type of effect a perception ability applies.
/// </summary>
public enum PerceptionEffectType
{
    /// <summary>
    /// Adds bonus dice to the check pool.
    /// </summary>
    DiceBonus,

    /// <summary>
    /// Adds a flat bonus to passive perception.
    /// </summary>
    PassiveBonus,

    /// <summary>
    /// Automatically succeeds an examination layer.
    /// </summary>
    AutoSuccessLayer,

    /// <summary>
    /// Automatically detects elements without a check.
    /// </summary>
    AutoDetect,

    /// <summary>
    /// Reveals additional lore or information.
    /// </summary>
    BonusLore,

    /// <summary>
    /// Provides a bonus to investigation checks.
    /// </summary>
    InvestigationBonus
}
