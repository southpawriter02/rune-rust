using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Events;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;
using Character = RuneAndRust.Core.Entities.Character;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// Service implementation for managing faction reputation and disposition.
/// </summary>
/// <remarks>See: v0.4.2a (The Repute) for Faction System implementation.</remarks>
public class FactionService : IFactionService
{
    private readonly IFactionRepository _repository;
    private readonly IEventBus _eventBus;
    private readonly ILogger<FactionService> _logger;

    /// <summary>
    /// Minimum reputation value.
    /// </summary>
    public const int MinReputation = -100;

    /// <summary>
    /// Maximum reputation value.
    /// </summary>
    public const int MaxReputation = 100;

    /// <summary>
    /// Initializes a new instance of the <see cref="FactionService"/> class.
    /// </summary>
    public FactionService(
        IFactionRepository repository,
        IEventBus eventBus,
        ILogger<FactionService> logger)
    {
        _repository = repository;
        _eventBus = eventBus;
        _logger = logger;
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Reputation Modification
    // ═══════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public async Task<ReputationChangeResult> ModifyReputationAsync(
        Character character,
        FactionType faction,
        int amount,
        string? source = null)
    {
        _logger.LogTrace(
            "[Faction] ModifyReputationAsync: {CharName}, {Faction}, Amount={Amount}, Source={Source}",
            character.Name, faction, amount, source ?? "unspecified");

        // Get current standing
        var standing = await _repository.GetStandingAsync(character.Id, faction);
        var currentValue = standing?.Reputation ?? await GetDefaultReputationAsync(faction);
        var oldDisposition = GetDisposition(currentValue);

        // Check for no-op
        if (amount == 0)
        {
            _logger.LogTrace("[Faction] Amount is 0, no change");
            return ReputationChangeResult.NoChange(faction, currentValue, oldDisposition);
        }

        // Calculate new value with clamping
        var rawValue = currentValue + amount;
        var newValue = Math.Clamp(rawValue, MinReputation, MaxReputation);

        if (rawValue != newValue)
        {
            _logger.LogDebug(
                "[Faction] Reputation clamped: {RawValue} → {ClampedValue}",
                rawValue, newValue);
        }

        // Check if actually changed after clamping
        if (newValue == currentValue)
        {
            _logger.LogTrace("[Faction] No effective change after clamping");
            return ReputationChangeResult.NoChange(faction, currentValue, oldDisposition);
        }

        var newDisposition = GetDisposition(newValue);

        // Update or create standing
        if (standing != null)
        {
            standing.Reputation = newValue;
            standing.LastModifiedAt = DateTime.UtcNow;
            await _repository.UpdateStandingAsync(standing);
        }
        else
        {
            standing = new CharacterFactionStanding
            {
                CharacterId = character.Id,
                FactionType = faction,
                Reputation = newValue,
                CreatedAt = DateTime.UtcNow,
                LastModifiedAt = DateTime.UtcNow
            };
            await _repository.AddStandingAsync(standing);
        }

        await _repository.SaveChangesAsync();

        // Log the change
        _logger.LogInformation(
            "[Faction] {CharName} rep with {Faction}: {OldVal} → {NewVal} ({Delta:+#;-#;0})",
            character.Name, faction, currentValue, newValue, amount);

        // Publish reputation changed event
        var repEvent = new ReputationChangedEvent(
            character.Id,
            character.Name,
            faction,
            currentValue,
            newValue,
            newValue - currentValue,
            source);
        await _eventBus.PublishAsync(repEvent);

        // Check for disposition change
        if (oldDisposition != newDisposition)
        {
            _logger.LogInformation(
                "[Faction] {CharName} now {NewDisp} with {Faction} (was {OldDisp})",
                character.Name, newDisposition, faction, oldDisposition);

            var direction = newDisposition > oldDisposition
                ? DispositionChangeDirection.Improved
                : DispositionChangeDirection.Degraded;

            var dispEvent = new DispositionChangedEvent(
                character.Id,
                character.Name,
                faction,
                oldDisposition,
                newDisposition,
                direction);
            await _eventBus.PublishAsync(dispEvent);

            _logger.LogDebug(
                "[Faction] DispositionChangedEvent published: {CharName}, {Faction}, {Direction}",
                character.Name, faction, direction);
        }

        return ReputationChangeResult.Ok(
            faction, currentValue, newValue, oldDisposition, newDisposition, source);
    }

    /// <inheritdoc/>
    public async Task<ReputationChangeResult> SetReputationAsync(
        Character character,
        FactionType faction,
        int value,
        string? source = null)
    {
        var currentRep = await GetReputationAsync(character, faction);
        var delta = value - currentRep;
        return await ModifyReputationAsync(character, faction, delta, source);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Reputation Queries
    // ═══════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public async Task<int> GetReputationAsync(Character character, FactionType faction)
    {
        var standing = await _repository.GetStandingAsync(character.Id, faction);
        var value = standing?.Reputation ?? await GetDefaultReputationAsync(faction);

        _logger.LogTrace(
            "[Faction] GetReputation({CharName}, {Faction}) = {Value}",
            character.Name, faction, value);

        return value;
    }

    /// <inheritdoc/>
    public Disposition GetDisposition(int reputation)
    {
        var disposition = reputation switch
        {
            <= -50 => Disposition.Hated,
            <= -10 => Disposition.Hostile,
            <= 9 => Disposition.Neutral,
            <= 49 => Disposition.Friendly,
            _ => Disposition.Exalted
        };

        _logger.LogTrace("[Faction] GetDisposition({Reputation}) = {Disposition}",
            reputation, disposition);

        return disposition;
    }

    /// <inheritdoc/>
    public async Task<FactionStandingInfo> GetFactionStandingAsync(
        Character character,
        FactionType faction)
    {
        var reputation = await GetReputationAsync(character, faction);
        var disposition = GetDisposition(reputation);
        var factionDef = await _repository.GetFactionAsync(faction);
        var factionName = factionDef?.Name ?? faction.ToString();

        return new FactionStandingInfo(faction, factionName, reputation, disposition);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyDictionary<FactionType, FactionStandingInfo>> GetAllStandingsAsync(
        Character character)
    {
        var result = new Dictionary<FactionType, FactionStandingInfo>();

        foreach (FactionType faction in Enum.GetValues<FactionType>())
        {
            result[faction] = await GetFactionStandingAsync(character, faction);
        }

        return result;
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Convenience Checks
    // ═══════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public async Task<bool> IsHostileAsync(Character character, FactionType faction)
    {
        var rep = await GetReputationAsync(character, faction);
        return GetDisposition(rep) <= Disposition.Hostile;
    }

    /// <inheritdoc/>
    public async Task<bool> IsFriendlyAsync(Character character, FactionType faction)
    {
        var rep = await GetReputationAsync(character, faction);
        return GetDisposition(rep) >= Disposition.Friendly;
    }

    /// <inheritdoc/>
    public async Task<bool> MeetsDispositionRequirementAsync(
        Character character,
        FactionType faction,
        Disposition minDisposition)
    {
        var rep = await GetReputationAsync(character, faction);
        var currentDisposition = GetDisposition(rep);
        return currentDisposition >= minDisposition;
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Faction Metadata
    // ═══════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public async Task<Faction?> GetFactionAsync(FactionType faction)
    {
        return await _repository.GetFactionAsync(faction);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Faction>> GetAllFactionsAsync()
    {
        return await _repository.GetAllFactionsAsync();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Private Helpers
    // ═══════════════════════════════════════════════════════════════════════

    private async Task<int> GetDefaultReputationAsync(FactionType faction)
    {
        var factionDef = await _repository.GetFactionAsync(faction);
        return factionDef?.DefaultReputation ?? 0;
    }
}
