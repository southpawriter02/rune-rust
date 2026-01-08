using Microsoft.Extensions.Logging;
using RuneAndRust.Domain.Entities;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Resolves target specifications to actual combatants in combat.
/// </summary>
/// <remarks>
/// <para>Supports multiple targeting formats:</para>
/// <list type="bullet">
/// <item>Empty/null: Target first active monster</item>
/// <item>Number only: Target monster by position (1-based)</item>
/// <item>Name only: Target first monster matching name</item>
/// <item>Name + Number: Target specific monster of that type</item>
/// </list>
/// </remarks>
public class TargetResolver
{
    private readonly ILogger<TargetResolver> _logger;

    /// <summary>
    /// Creates a new TargetResolver instance.
    /// </summary>
    /// <param name="logger">The logger for diagnostics.</param>
    public TargetResolver(ILogger<TargetResolver> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Result of target resolution.
    /// </summary>
    /// <param name="Target">The resolved target, or null if not found.</param>
    /// <param name="IsAmbiguous">True if multiple targets matched (returned first).</param>
    /// <param name="ErrorMessage">Error message if target not found.</param>
    public record TargetResolutionResult(
        Combatant? Target,
        bool IsAmbiguous = false,
        string? ErrorMessage = null)
    {
        /// <summary>
        /// Gets whether the resolution was successful.
        /// </summary>
        public bool Success => Target != null;

        /// <summary>
        /// Creates a successful result.
        /// </summary>
        public static TargetResolutionResult Found(Combatant target, bool isAmbiguous = false) =>
            new(target, isAmbiguous);

        /// <summary>
        /// Creates a not-found result.
        /// </summary>
        public static TargetResolutionResult NotFound(string errorMessage) =>
            new(null, false, errorMessage);
    }

    /// <summary>
    /// Resolves a target specification to a monster combatant.
    /// </summary>
    /// <param name="encounter">The active combat encounter.</param>
    /// <param name="targetSpec">The target specification (name, number, or name+number).</param>
    /// <returns>The resolution result containing the target or error information.</returns>
    /// <exception cref="ArgumentNullException">Thrown if encounter is null.</exception>
    public TargetResolutionResult ResolveMonsterTarget(CombatEncounter encounter, string? targetSpec)
    {
        ArgumentNullException.ThrowIfNull(encounter);

        var activeMonsters = encounter.GetActiveMonsters().ToList();

        if (activeMonsters.Count == 0)
        {
            _logger.LogDebug("No active monsters in encounter");
            return TargetResolutionResult.NotFound("There are no enemies to attack.");
        }

        // No target specified - return first monster
        if (string.IsNullOrWhiteSpace(targetSpec))
        {
            _logger.LogDebug("No target specified, returning first active monster: {Name}",
                activeMonsters[0].DisplayName);
            return TargetResolutionResult.Found(activeMonsters[0]);
        }

        targetSpec = targetSpec.Trim();

        // Try parsing as number only ("attack 2")
        if (int.TryParse(targetSpec, out var number))
        {
            var target = encounter.GetMonsterByNumber(number);
            if (target != null)
            {
                _logger.LogDebug("Resolved number {Number} to {Name}", number, target.DisplayName);
                return TargetResolutionResult.Found(target);
            }
            else
            {
                _logger.LogDebug("No monster at position {Number}", number);
                return TargetResolutionResult.NotFound($"There is no enemy #{number}.");
            }
        }

        // Try parsing as "name number" ("attack goblin 2")
        var parts = targetSpec.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2 && int.TryParse(parts[^1], out var nameNumber))
        {
            var name = string.Join(' ', parts[..^1]);
            var matchingMonsters = encounter.GetMonstersByName(name).ToList();

            if (matchingMonsters.Count == 0)
            {
                _logger.LogDebug("No monsters matching name '{Name}'", name);
                return TargetResolutionResult.NotFound($"There is no enemy named '{name}'.");
            }

            var target = matchingMonsters.ElementAtOrDefault(nameNumber - 1);
            if (target != null)
            {
                _logger.LogDebug("Resolved '{Name} {Number}' to {DisplayName}",
                    name, nameNumber, target.DisplayName);
                return TargetResolutionResult.Found(target);
            }
            else
            {
                _logger.LogDebug("No {Name} #{Number}", name, nameNumber);
                return TargetResolutionResult.NotFound($"There is no {name} #{nameNumber}.");
            }
        }

        // Try matching by name only ("attack goblin")
        var byName = encounter.GetMonstersByName(targetSpec).ToList();
        if (byName.Count == 1)
        {
            _logger.LogDebug("Resolved name '{Name}' to {DisplayName}",
                targetSpec, byName[0].DisplayName);
            return TargetResolutionResult.Found(byName[0]);
        }
        else if (byName.Count > 1)
        {
            // Ambiguous - return first match but mark as ambiguous
            _logger.LogDebug("Ambiguous target '{Name}' matched {Count} monsters, using first",
                targetSpec, byName.Count);
            return TargetResolutionResult.Found(byName[0], isAmbiguous: true);
        }

        _logger.LogDebug("No match for target specification '{TargetSpec}'", targetSpec);
        return TargetResolutionResult.NotFound($"No enemy matches '{targetSpec}'.");
    }

    /// <summary>
    /// Gets a formatted list of valid targets for display.
    /// </summary>
    /// <param name="encounter">The active combat encounter.</param>
    /// <returns>List of target descriptions.</returns>
    public IReadOnlyList<string> GetValidTargets(CombatEncounter encounter)
    {
        ArgumentNullException.ThrowIfNull(encounter);

        var monsters = encounter.GetActiveMonsters().ToList();
        var targets = new List<string>();

        for (var i = 0; i < monsters.Count; i++)
        {
            var m = monsters[i];
            targets.Add($"[{i + 1}] {m.DisplayName} ({m.CurrentHealth}/{m.MaxHealth} HP)");
        }

        return targets.AsReadOnly();
    }
}
