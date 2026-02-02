// ═══════════════════════════════════════════════════════════════════════════════
// PartyContext.cs
// Record providing party state context for unified rest processing.
// Contains party member IDs and identifies any Berserker in FrenzyBeyondReason
// whose rage bonus applies to party rest outcomes.
// Version: 0.18.5c
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.Records;

/// <summary>
/// Provides party state context for unified rest processing.
/// </summary>
/// <remarks>
/// <para>
/// Used by UnifiedRestHandler to apply
/// party-wide effects during rest, such as the Berserker's FrenzyBeyondReason bonus
/// (−10 stress to nearby allies on Long/Sanctuary rest).
/// </para>
/// </remarks>
/// <param name="PartyMemberIds">All party member character IDs, including the resting character.</param>
/// <param name="BerserkerId">ID of a Berserker in FrenzyBeyondReason state, or null if none.</param>
/// <example>
/// <code>
/// var partyContext = new PartyContext(
///     PartyMemberIds: new[] { player1Id, player2Id, berserkerId },
///     BerserkerId: berserkerId);
///
/// var result = restHandler.ProcessRest(player1Id, RestType.Long, partyContext);
/// </code>
/// </example>
public sealed record PartyContext(
    IReadOnlyList<Guid> PartyMemberIds,
    Guid? BerserkerId = null)
{
    /// <summary>
    /// Gets whether a Berserker in FrenzyBeyondReason is present in the party.
    /// </summary>
    public bool HasBerserkerInFrenzy => BerserkerId.HasValue;

    /// <summary>
    /// Gets the party size.
    /// </summary>
    public int PartySize => PartyMemberIds.Count;

    /// <summary>
    /// Creates an empty party context (solo adventurer).
    /// </summary>
    /// <param name="characterId">The solo character's ID.</param>
    /// <returns>A party context with just the solo character.</returns>
    public static PartyContext Solo(Guid characterId) => new(
        PartyMemberIds: new[] { characterId });

    /// <summary>
    /// Creates a party context without any Berserker bonus.
    /// </summary>
    /// <param name="memberIds">The party member IDs.</param>
    /// <returns>A party context without rage bonus effects.</returns>
    public static PartyContext WithMembers(params Guid[] memberIds) => new(
        PartyMemberIds: memberIds);
}
