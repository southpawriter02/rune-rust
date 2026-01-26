namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Contains all investigation modifiers from specialization abilities.
/// </summary>
public sealed record InvestigationModifiers
{
    /// <summary>
    /// Bonus dice to add to the investigation pool.
    /// </summary>
    public int BonusDice { get; init; }

    /// <summary>
    /// Flat bonus to add to the check result.
    /// </summary>
    public int FlatBonus { get; init; }

    /// <summary>
    /// Whether certain clue types are auto-discovered.
    /// </summary>
    public IReadOnlyList<string> AutoDiscoverClueTypes { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Additional narrative text to include in results.
    /// </summary>
    public string? BonusNarrativeKey { get; init; }

    /// <summary>
    /// The abilities that contributed to these modifiers.
    /// </summary>
    public IReadOnlyList<PerceptionAbility> ContributingAbilities { get; init; }
        = Array.Empty<PerceptionAbility>();

    /// <summary>
    /// Gets the total bonus (dice + flat).
    /// </summary>
    public int TotalBonus => BonusDice + FlatBonus;

    /// <summary>
    /// Gets whether there are any modifiers.
    /// </summary>
    public bool HasModifiers => BonusDice > 0 || FlatBonus > 0 || AutoDiscoverClueTypes.Count > 0;

    /// <summary>
    /// Gets whether any abilities contributed.
    /// </summary>
    public bool HasContributingAbilities => ContributingAbilities.Count > 0;

    /// <summary>
    /// Creates an empty investigation modifiers instance.
    /// </summary>
    public static InvestigationModifiers None => new();

    /// <summary>
    /// Creates modifiers with a dice bonus.
    /// </summary>
    public static InvestigationModifiers WithDiceBonus(int bonusDice, PerceptionAbility fromAbility) => new()
    {
        BonusDice = bonusDice,
        ContributingAbilities = new[] { fromAbility }
    };
}
