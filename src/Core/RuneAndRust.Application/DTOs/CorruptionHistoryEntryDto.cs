// ═══════════════════════════════════════════════════════════════════════════════
// CorruptionHistoryEntryDto.cs
// DTO representing a corruption history entry for external consumption.
// Maps from the CorruptionHistoryEntry domain entity, providing a serializable
// representation of a single corruption change event for API responses,
// analytics export, and admin panel display.
// Version: 0.18.1e
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.DTOs;

using System.Text.Json.Serialization;
using RuneAndRust.Domain.Enums;

/// <summary>
/// DTO representing a corruption history entry for external consumption.
/// </summary>
/// <remarks>
/// <para>
/// Provides a serializable representation of a <c>CorruptionHistoryEntry</c> domain entity
/// for API responses, analytics export, and admin panel display. Contains all fields
/// from the domain entity in a format suitable for JSON serialization.
/// </para>
/// <para>
/// <strong>Property Mapping from Domain Entity:</strong>
/// </para>
/// <list type="table">
/// <listheader>
/// <term>DTO Property</term>
/// <description>Entity Property</description>
/// </listheader>
/// <item><term><see cref="Id"/></term><description><c>CorruptionHistoryEntry.Id</c></description></item>
/// <item><term><see cref="CharacterId"/></term><description><c>CorruptionHistoryEntry.CharacterId</c></description></item>
/// <item><term><see cref="Amount"/></term><description><c>CorruptionHistoryEntry.Amount</c></description></item>
/// <item><term><see cref="Source"/></term><description><c>CorruptionHistoryEntry.Source</c> (enum → string)</description></item>
/// <item><term><see cref="NewTotal"/></term><description><c>CorruptionHistoryEntry.NewTotal</c></description></item>
/// <item><term><see cref="ThresholdCrossed"/></term><description><c>CorruptionHistoryEntry.ThresholdCrossed</c></description></item>
/// <item><term><see cref="IsTransfer"/></term><description><c>CorruptionHistoryEntry.IsTransfer</c></description></item>
/// <item><term><see cref="TransferTargetId"/></term><description><c>CorruptionHistoryEntry.TransferTargetId</c></description></item>
/// <item><term><see cref="CreatedAt"/></term><description><c>CorruptionHistoryEntry.CreatedAt</c></description></item>
/// </list>
/// </remarks>
/// <seealso cref="CorruptionConfigurationDto"/>
public sealed record CorruptionHistoryEntryDto
{
    // ═══════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the unique identifier for this history entry.
    /// </summary>
    /// <value>A <see cref="Guid"/> that uniquely identifies this corruption event record.</value>
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the identifier of the character this corruption event applies to.
    /// </summary>
    /// <value>The character's unique identifier.</value>
    [JsonPropertyName("characterId")]
    public Guid CharacterId { get; init; }

    /// <summary>
    /// Gets the corruption amount changed in this event.
    /// </summary>
    /// <remarks>
    /// Positive values indicate corruption gained; negative values indicate
    /// corruption removed or transferred away.
    /// </remarks>
    /// <value>The corruption change delta.</value>
    [JsonPropertyName("amount")]
    public int Amount { get; init; }

    /// <summary>
    /// Gets the source category of the corruption event.
    /// </summary>
    /// <value>The <see cref="CorruptionSource"/> enum value serialized as a string.</value>
    [JsonPropertyName("source")]
    public CorruptionSource Source { get; init; }

    /// <summary>
    /// Gets the corruption total after this event was applied.
    /// </summary>
    /// <value>The corruption value after the change, clamped to [0, 100].</value>
    [JsonPropertyName("newTotal")]
    public int NewTotal { get; init; }

    /// <summary>
    /// Gets the corruption threshold that was crossed, if any.
    /// </summary>
    /// <value>The threshold value (25, 50, 75, or 100), or <c>null</c> if none crossed.</value>
    [JsonPropertyName("thresholdCrossed")]
    public int? ThresholdCrossed { get; init; }

    /// <summary>
    /// Gets whether this event was a Blot-Priest corruption transfer.
    /// </summary>
    /// <value><c>true</c> for transfer events; <c>false</c> otherwise.</value>
    [JsonPropertyName("isTransfer")]
    public bool IsTransfer { get; init; }

    /// <summary>
    /// Gets the target character ID for transfer events.
    /// </summary>
    /// <value>The transfer target's identifier, or <c>null</c> for non-transfer events.</value>
    [JsonPropertyName("transferTargetId")]
    public Guid? TransferTargetId { get; init; }

    /// <summary>
    /// Gets the UTC timestamp when this corruption event occurred.
    /// </summary>
    /// <value>The event timestamp in UTC.</value>
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; init; }
}
