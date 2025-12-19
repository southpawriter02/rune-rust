using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models.Combat;
using CharacterAttribute = RuneAndRust.Core.Enums.Attribute;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// Handles initiative calculation and turn order sorting for combat encounters.
/// Formula: d10 + FINESSE + WITS (Vigilance).
/// </summary>
public class InitiativeService : IInitiativeService
{
    private readonly IDiceService _dice;
    private readonly ILogger<InitiativeService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="InitiativeService"/> class.
    /// </summary>
    /// <param name="dice">The dice service for rolling initiative.</param>
    /// <param name="logger">The logger for traceability.</param>
    public InitiativeService(IDiceService dice, ILogger<InitiativeService> logger)
    {
        _dice = dice;
        _logger = logger;
    }

    /// <inheritdoc/>
    public void RollInitiative(Combatant combatant)
    {
        _logger.LogTrace("Rolling initiative for {Name}", combatant.Name);

        // Formula: d10 + FINESSE + WITS (Vigilance)
        var roll = _dice.RollSingle(10, $"Initiative:{combatant.Name}");
        var finesse = combatant.GetAttribute(CharacterAttribute.Finesse);
        var wits = combatant.GetAttribute(CharacterAttribute.Wits);
        var vigilance = finesse + wits;

        combatant.Initiative = roll + vigilance;

        _logger.LogDebug("{Name} rolled initiative: {Total} (d10:{Roll} + Vigilance:{Vig})",
            combatant.Name, combatant.Initiative, roll, vigilance);
    }

    /// <inheritdoc/>
    public List<Combatant> SortTurnOrder(IEnumerable<Combatant> combatants)
    {
        var combatantList = combatants.ToList();
        _logger.LogTrace("Sorting turn order for {Count} combatants", combatantList.Count);

        var sorted = combatantList
            .OrderByDescending(c => c.Initiative)
            .ThenByDescending(c => c.GetAttribute(CharacterAttribute.Finesse))
            .ThenBy(_ => Guid.NewGuid()) // Random tie-breaker
            .ToList();

        _logger.LogDebug("Turn order: {Order}",
            string.Join(" → ", sorted.Select(c => $"{c.Name}({c.Initiative})")));

        return sorted;
    }
}
