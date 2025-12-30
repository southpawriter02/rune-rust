using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Events;

/// <summary>
/// Published whenever a character's reputation with a faction changes.
/// Consumed by UI listeners for notifications and combat AI for aggression updates.
/// </summary>
/// <remarks>See: v0.4.2a (The Repute) for Faction System implementation.</remarks>
/// <param name="CharacterId">The character whose reputation changed.</param>
/// <param name="CharacterName">The character's display name.</param>
/// <param name="Faction">The faction affected.</param>
/// <param name="OldValue">Previous reputation value.</param>
/// <param name="NewValue">New reputation value.</param>
/// <param name="Delta">The amount changed (positive or negative).</param>
/// <param name="Source">Optional source of the change (e.g., "Quest: Iron Bane Initiation").</param>
public record ReputationChangedEvent(
    Guid CharacterId,
    string CharacterName,
    FactionType Faction,
    int OldValue,
    int NewValue,
    int Delta,
    string? Source = null);
