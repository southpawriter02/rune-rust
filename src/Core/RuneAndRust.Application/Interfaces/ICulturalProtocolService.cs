// ------------------------------------------------------------------------------
// <copyright file="ICulturalProtocolService.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Defines the contract for managing cultural protocols and cant-based modifiers
// for social interactions. Enables culture-specific social mechanics including
// Cant fluency, Veil-Speech, and protocol violation tracking.
// Part of v0.15.3g Cultural Protocols implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Manages cultural protocols and cant-based modifiers for social interactions.
/// </summary>
/// <remarks>
/// <para>
/// This service provides the core functionality for the Cultural Protocol System,
/// enabling culture-specific social interaction mechanics. It handles:
/// </para>
/// <list type="bullet">
///   <item><description>Protocol retrieval and management for each culture</description></item>
///   <item><description>Cant fluency modifiers (None -1d10, Basic +0, Fluent +1d10)</description></item>
///   <item><description>Veil-Speech state tracking for Utgard interactions</description></item>
///   <item><description>Protocol compliance checking with violation detection</description></item>
///   <item><description>Protocol violation recording and tracking</description></item>
/// </list>
/// <para>
/// The five major cultures each have unique protocols:
/// </para>
/// <list type="table">
///   <listheader>
///     <term>Culture</term>
///     <description>Protocol Characteristics</description>
///   </listheader>
///   <item>
///     <term>Dvergr (DC 18)</term>
///     <description>Trade-Tongue cant, Logic-Chain protocol requiring consistent arguments</description>
///   </item>
///   <item>
///     <term>Utgard (DC 16)</term>
///     <description>Veil-Speech where deception is respect and truth offends</description>
///   </item>
///   <item>
///     <term>Gorge-Maw (DC 14)</term>
///     <description>Hospitality Rite requiring extended greeting ceremonies</description>
///   </item>
///   <item>
///     <term>Rune-Lupin (DC 12)</term>
///     <description>Pack-Speech telepathy requiring mental openness</description>
///   </item>
///   <item>
///     <term>Iron-Bane (DC 16)</term>
///     <description>Battle-Tongue with Blood Oath and Martial Tribute protocols</description>
///   </item>
/// </list>
/// <para>
/// Implementation classes should use <c>ILogger&lt;T&gt;</c> for comprehensive logging
/// and <c>IOptions&lt;CulturalProtocolOptions&gt;</c> for configuration.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Get a culture's protocol
/// var dvergrProtocol = protocolService.GetProtocol("dvergr");
/// Console.WriteLine($"Base DC: {dvergrProtocol?.BaseDc}"); // 18
///
/// // Calculate cant modifier for a fluent speaker
/// var cantModifier = protocolService.GetCantModifier(CantFluency.Fluent, "dvergr");
/// Console.WriteLine(cantModifier.DiceModifier); // 1
///
/// // Check protocol compliance
/// var result = protocolService.CheckProtocolCompliance(characterId, "utgard", context);
/// if (result.BlocksInteraction)
/// {
///     Console.WriteLine("Interaction blocked due to protocol violation");
/// }
///
/// // Apply Veil-Speech logic for Utgard
/// var veilContext = protocolService.ApplyVeilSpeechLogic(npcId, isDeception: true, isDirectTruth: false, isProperVeilSpeech: false);
/// Console.WriteLine($"DC adjustment: {veilContext.GetDcAdjustment()}"); // -4 for deception
/// </code>
/// </example>
public interface ICulturalProtocolService
{
    // =========================================================================
    // Protocol Retrieval Methods
    // =========================================================================

    /// <summary>
    /// Gets the protocol definition for a specific culture.
    /// </summary>
    /// <param name="cultureId">
    /// The culture identifier (e.g., "dvergr", "utgard", "gorge-maw", "rune-lupin", "iron-bane").
    /// Case-insensitive comparison is used.
    /// </param>
    /// <returns>
    /// The <see cref="CultureProtocol"/> if found; otherwise, <c>null</c>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method retrieves the complete protocol definition for a culture, including:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Base DC for social interactions</description></item>
    ///   <item><description>Protocol requirements (mandatory and optional)</description></item>
    ///   <item><description>Special protocol types (Veil-Speech, Logic-Chain, etc.)</description></item>
    ///   <item><description>Cant name and requirements</description></item>
    /// </list>
    /// <para>
    /// Returns <c>null</c> if the culture is not recognized, allowing callers to handle
    /// unknown cultures gracefully by falling back to default interaction rules.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var protocol = protocolService.GetProtocol("dvergr");
    /// if (protocol != null)
    /// {
    ///     Console.WriteLine($"Protocol: {protocol.ProtocolName}");
    ///     Console.WriteLine($"Base DC: {protocol.BaseDc}");
    ///     Console.WriteLine($"Special Rules: {protocol.SpecialRuleType}");
    /// }
    /// else
    /// {
    ///     Console.WriteLine("Unknown culture, using default interaction rules");
    /// }
    /// </code>
    /// </example>
    CultureProtocol? GetProtocol(string cultureId);

    /// <summary>
    /// Gets all defined cultural protocols.
    /// </summary>
    /// <returns>
    /// A read-only list of all <see cref="CultureProtocol"/> definitions.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Returns all protocols currently loaded in the service. This is useful for:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Displaying available cultures to the player</description></item>
    ///   <item><description>Validating culture IDs during game initialization</description></item>
    ///   <item><description>Building UI elements that show cultural information</description></item>
    /// </list>
    /// <para>
    /// The returned list is read-only to prevent external modification of the protocol definitions.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var allProtocols = protocolService.GetAllProtocols();
    /// Console.WriteLine($"Loaded {allProtocols.Count} cultural protocols:");
    /// foreach (var protocol in allProtocols)
    /// {
    ///     Console.WriteLine($"  - {protocol.ProtocolName} (DC {protocol.BaseDc})");
    /// }
    /// </code>
    /// </example>
    IReadOnlyList<CultureProtocol> GetAllProtocols();

    // =========================================================================
    // Cant Modifier Methods
    // =========================================================================

    /// <summary>
    /// Calculates the cant modifier for a character's fluency level with a specific culture.
    /// </summary>
    /// <param name="fluency">
    /// The character's fluency level in the culture's cant.
    /// </param>
    /// <param name="cultureId">
    /// The target culture identifier.
    /// </param>
    /// <returns>
    /// A <see cref="CantModifier"/> containing the dice pool modifier and related information.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Cants are specialized dialects or coded languages used by different cultures.
    /// A character's fluency in a culture's cant affects their social interaction dice pool:
    /// </para>
    /// <list type="table">
    ///   <listheader>
    ///     <term>Fluency Level</term>
    ///     <description>Dice Modifier</description>
    ///   </listheader>
    ///   <item>
    ///     <term><see cref="CantFluency.None"/></term>
    ///     <description>-1d10 (cannot understand or speak the cant)</description>
    ///   </item>
    ///   <item>
    ///     <term><see cref="CantFluency.Basic"/></term>
    ///     <description>+0 (can communicate but lacks nuance)</description>
    ///   </item>
    ///   <item>
    ///     <term><see cref="CantFluency.Fluent"/></term>
    ///     <description>+1d10 (native-level proficiency)</description>
    ///   </item>
    /// </list>
    /// <para>
    /// The returned <see cref="CantModifier"/> includes the culture's cant name for display purposes
    /// and can be converted to a <see cref="SocialModifier"/> for integration with the social interaction system.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // A fluent speaker of Dvergr Trade-Tongue
    /// var modifier = protocolService.GetCantModifier(CantFluency.Fluent, "dvergr");
    /// Console.WriteLine(modifier.ToDisplayString()); // "Dvergr Trade-Tongue: +1d10 (Fluent)"
    /// Console.WriteLine(modifier.DiceModifier);       // 1
    ///
    /// // A character with no knowledge of Utgard Veil-Speech
    /// var penalty = protocolService.GetCantModifier(CantFluency.None, "utgard");
    /// Console.WriteLine(penalty.ToDisplayString());   // "Utgard Veil-Speech: -1d10 (Unfamiliar)"
    /// </code>
    /// </example>
    CantModifier GetCantModifier(CantFluency fluency, string cultureId);

    // =========================================================================
    // Protocol Compliance Methods
    // =========================================================================

    /// <summary>
    /// Checks protocol compliance for a social interaction and determines any violations.
    /// </summary>
    /// <param name="characterId">
    /// The unique identifier of the character attempting the interaction.
    /// </param>
    /// <param name="cultureId">
    /// The target culture identifier.
    /// </param>
    /// <param name="context">
    /// The social context containing interaction details, target information, and modifiers.
    /// </param>
    /// <returns>
    /// A <see cref="ProtocolCheckResult"/> indicating compliance status, violations, and applicable modifiers.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method evaluates whether a character has met the cultural protocol requirements
    /// for the interaction. It checks:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Mandatory requirements (must be fulfilled to avoid violation)</description></item>
    ///   <item><description>Optional requirements (provide bonuses when fulfilled)</description></item>
    ///   <item><description>Special protocol rules (Veil-Speech, Logic-Chain, etc.)</description></item>
    ///   <item><description>Cant fluency modifiers</description></item>
    /// </list>
    /// <para>
    /// The result includes:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Compliance status (compliant, violation, blocked)</description></item>
    ///   <item><description>Violation type if applicable (Minor, Moderate, Severe, Unforgivable)</description></item>
    ///   <item><description>Total DC modifier (sum of all DC adjustments)</description></item>
    ///   <item><description>Total dice modifier (sum of all dice pool adjustments)</description></item>
    ///   <item><description>Whether the interaction is blocked entirely</description></item>
    /// </list>
    /// <para>
    /// If no protocol exists for the culture, a neutral result is returned with no modifiers.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var context = SocialContext.Create(
    ///     characterId: "player-1",
    ///     targetId: "npc-dvergr-merchant",
    ///     cultureId: "dvergr",
    ///     interactionType: SocialInteractionType.Persuasion);
    ///
    /// var result = protocolService.CheckProtocolCompliance("player-1", "dvergr", context);
    ///
    /// if (result.BlocksInteraction)
    /// {
    ///     Console.WriteLine("Cannot interact: Unforgivable protocol violation");
    /// }
    /// else if (result.ViolationType != ProtocolViolationType.None)
    /// {
    ///     Console.WriteLine($"Violation: {result.ViolationType} (+{result.TotalDcModifier} DC)");
    /// }
    /// else
    /// {
    ///     Console.WriteLine($"Compliant. Dice modifier: {result.TotalDiceModifier}");
    /// }
    /// </code>
    /// </example>
    ProtocolCheckResult CheckProtocolCompliance(string characterId, string cultureId, SocialContext context);

    // =========================================================================
    // Veil-Speech Methods (Utgard-specific)
    // =========================================================================

    /// <summary>
    /// Applies Veil-Speech logic for Utgard interactions, calculating modifiers based on communication style.
    /// </summary>
    /// <param name="npcId">
    /// The unique identifier of the Utgard NPC being interacted with.
    /// </param>
    /// <param name="isDeception">
    /// <c>true</c> if the character is using deception; otherwise, <c>false</c>.
    /// When <c>true</c>, the DC is reduced by 4 as deception is culturally respected.
    /// </param>
    /// <param name="isDirectTruth">
    /// <c>true</c> if the character is telling direct, unvarnished truth; otherwise, <c>false</c>.
    /// When <c>true</c>, a -2d10 penalty is applied as this offends Utgard sensibilities.
    /// </param>
    /// <param name="isProperVeilSpeech">
    /// <c>true</c> if the character is using proper Veil-Speech (layered, indirect communication);
    /// otherwise, <c>false</c>. When <c>true</c>, a +1d10 bonus is applied.
    /// </param>
    /// <returns>
    /// A <see cref="VeilSpeechContext"/> containing the calculated modifiers and state information.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Veil-Speech is the Utgard cultural protocol where direct truth-telling is considered offensive
    /// and deception is a sign of respect. The Utgard believe that only fools and children speak plainly.
    /// </para>
    /// <para>
    /// Modifier values:
    /// </para>
    /// <list type="table">
    ///   <listheader>
    ///     <term>Communication Style</term>
    ///     <description>Effect</description>
    ///   </listheader>
    ///   <item>
    ///     <term>Deception</term>
    ///     <description>DC reduced by 4 (lies within veil-speech are expected)</description>
    ///   </item>
    ///   <item>
    ///     <term>Direct Truth</term>
    ///     <description>-2d10 penalty (offends Utgard sensibilities)</description>
    ///   </item>
    ///   <item>
    ///     <term>Proper Veil-Speech</term>
    ///     <description>+1d10 bonus (demonstrates cultural sophistication)</description>
    ///   </item>
    /// </list>
    /// <para>
    /// The returned context tracks the current Veil-Speech state with the NPC and can
    /// determine the resulting state after the interaction.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Using deception with an Utgard noble
    /// var deceptionContext = protocolService.ApplyVeilSpeechLogic(
    ///     npcId: "utgard-noble-1",
    ///     isDeception: true,
    ///     isDirectTruth: false,
    ///     isProperVeilSpeech: false);
    /// Console.WriteLine($"DC adjustment: {deceptionContext.GetDcAdjustment()}"); // -4
    ///
    /// // Speaking direct truth (offensive to Utgard)
    /// var truthContext = protocolService.ApplyVeilSpeechLogic(
    ///     npcId: "utgard-merchant-1",
    ///     isDeception: false,
    ///     isDirectTruth: true,
    ///     isProperVeilSpeech: false);
    /// Console.WriteLine($"Dice modifier: {truthContext.GetDiceModifier()}"); // -2
    ///
    /// // Using proper Veil-Speech
    /// var veilContext = protocolService.ApplyVeilSpeechLogic(
    ///     npcId: "utgard-elder-1",
    ///     isDeception: false,
    ///     isDirectTruth: false,
    ///     isProperVeilSpeech: true);
    /// Console.WriteLine($"Dice modifier: {veilContext.GetDiceModifier()}"); // +1
    /// </code>
    /// </example>
    VeilSpeechContext ApplyVeilSpeechLogic(string npcId, bool isDeception, bool isDirectTruth, bool isProperVeilSpeech);

    /// <summary>
    /// Gets the character's current Veil-Speech state with a specific Utgard NPC.
    /// </summary>
    /// <param name="characterId">
    /// The unique identifier of the character.
    /// </param>
    /// <param name="npcId">
    /// The unique identifier of the Utgard NPC.
    /// </param>
    /// <returns>
    /// The current <see cref="VeilSpeechState"/> for this character-NPC pair.
    /// Returns <see cref="VeilSpeechState.Neutral"/> if no prior interactions exist.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Veil-Speech state persists across interactions with the same NPC and affects
    /// future dice pool modifiers:
    /// </para>
    /// <list type="table">
    ///   <listheader>
    ///     <term>State</term>
    ///     <description>Effect</description>
    ///   </listheader>
    ///   <item>
    ///     <term><see cref="VeilSpeechState.Neutral"/></term>
    ///     <description>No modifier (default starting state)</description>
    ///   </item>
    ///   <item>
    ///     <term><see cref="VeilSpeechState.Respected"/></term>
    ///     <description>+1d10 bonus (proper Veil-Speech used previously)</description>
    ///   </item>
    ///   <item>
    ///     <term><see cref="VeilSpeechState.Offended"/></term>
    ///     <description>-2d10 penalty (direct truth told previously)</description>
    ///   </item>
    ///   <item>
    ///     <term><see cref="VeilSpeechState.DeepOffense"/></term>
    ///     <description>-4d10 penalty, interaction may be blocked</description>
    ///   </item>
    /// </list>
    /// <para>
    /// State transitions occur based on communication choices and can be recovered
    /// through apologies or time (except for DeepOffense which may be permanent).
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var state = protocolService.GetVeilSpeechState("player-1", "utgard-merchant-1");
    /// switch (state)
    /// {
    ///     case VeilSpeechState.DeepOffense:
    ///         Console.WriteLine("This NPC refuses to speak with you.");
    ///         break;
    ///     case VeilSpeechState.Offended:
    ///         Console.WriteLine("The merchant regards you coldly.");
    ///         break;
    ///     case VeilSpeechState.Respected:
    ///         Console.WriteLine("The merchant nods approvingly.");
    ///         break;
    ///     default:
    ///         Console.WriteLine("The merchant awaits your words.");
    ///         break;
    /// }
    /// </code>
    /// </example>
    VeilSpeechState GetVeilSpeechState(string characterId, string npcId);

    // =========================================================================
    // Violation Recording Methods
    // =========================================================================

    /// <summary>
    /// Records a protocol violation for tracking and consequence application.
    /// </summary>
    /// <param name="characterId">
    /// The unique identifier of the character who committed the violation.
    /// </param>
    /// <param name="cultureId">
    /// The culture whose protocol was violated.
    /// </param>
    /// <param name="type">
    /// The severity of the violation.
    /// </param>
    /// <remarks>
    /// <para>
    /// This method records a protocol violation for future reference. Violations affect:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Future interaction DCs with NPCs of this culture</description></item>
    ///   <item><description>Disposition levels with affected NPCs</description></item>
    ///   <item><description>Faction reputation (for Unforgivable violations)</description></item>
    /// </list>
    /// <para>
    /// Violation consequences by severity:
    /// </para>
    /// <list type="table">
    ///   <listheader>
    ///     <term>Severity</term>
    ///     <description>Consequences</description>
    ///   </listheader>
    ///   <item>
    ///     <term><see cref="ProtocolViolationType.Minor"/></term>
    ///     <description>+2 DC, -5 disposition, auto-recovers after success</description>
    ///   </item>
    ///   <item>
    ///     <term><see cref="ProtocolViolationType.Moderate"/></term>
    ///     <description>+4 DC, -1d10, -15 disposition, requires apology</description>
    ///   </item>
    ///   <item>
    ///     <term><see cref="ProtocolViolationType.Severe"/></term>
    ///     <description>+6 DC, -2d10, -30 disposition, may trigger hostility</description>
    ///   </item>
    ///   <item>
    ///     <term><see cref="ProtocolViolationType.Unforgivable"/></term>
    ///     <description>Blocks interaction, -100 disposition, faction reputation loss</description>
    ///   </item>
    /// </list>
    /// <para>
    /// Implementations should persist this information to game state for retrieval
    /// during future interactions.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Record a minor faux pas with the Dvergr
    /// protocolService.RecordProtocolViolation("player-1", "dvergr", ProtocolViolationType.Minor);
    ///
    /// // Record a severe offense with the Iron-Bane (broke a blood oath)
    /// protocolService.RecordProtocolViolation("player-1", "iron-bane", ProtocolViolationType.Severe);
    /// </code>
    /// </example>
    void RecordProtocolViolation(string characterId, string cultureId, ProtocolViolationType type);

    // =========================================================================
    // Protocol Requirement Methods
    // =========================================================================

    /// <summary>
    /// Gets all protocol requirements for a specific culture.
    /// </summary>
    /// <param name="cultureId">
    /// The culture identifier.
    /// </param>
    /// <returns>
    /// A read-only list of <see cref="ProtocolRequirement"/> for the culture,
    /// or an empty list if the culture is not found.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Returns both mandatory and optional requirements for the culture's protocol.
    /// Use <see cref="ProtocolRequirement.IsMandatory"/> to filter as needed.
    /// </para>
    /// <para>
    /// Common requirement types include:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Greeting rituals (Gorge-Maw Hospitality Rite)</description></item>
    ///   <item><description>Status acknowledgment (Dvergr guild ranks)</description></item>
    ///   <item><description>Gift exchange (Iron-Bane Martial Tribute)</description></item>
    ///   <item><description>Communication style (Utgard Veil-Speech)</description></item>
    ///   <item><description>Mental openness (Rune-Lupin Telepathy)</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var requirements = protocolService.GetRequirements("dvergr");
    /// var mandatory = requirements.Where(r => r.IsMandatory).ToList();
    /// Console.WriteLine($"Dvergr protocol has {mandatory.Count} mandatory requirements:");
    /// foreach (var req in mandatory)
    /// {
    ///     Console.WriteLine($"  - {req.Description} (Violation: {req.ViolationSeverity})");
    /// }
    /// </code>
    /// </example>
    IReadOnlyList<ProtocolRequirement> GetRequirements(string cultureId);

    /// <summary>
    /// Checks if a culture has a specific special protocol type.
    /// </summary>
    /// <param name="cultureId">
    /// The culture identifier.
    /// </param>
    /// <param name="type">
    /// The special protocol type to check for.
    /// </param>
    /// <returns>
    /// <c>true</c> if the culture uses this special protocol type; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Special protocols modify standard social interaction mechanics:
    /// </para>
    /// <list type="bullet">
    ///   <item><description><see cref="SpecialProtocolType.VeilSpeech"/>: Utgard - deception respected, truth offends</description></item>
    ///   <item><description><see cref="SpecialProtocolType.LogicChain"/>: Dvergr - arguments must be logically consistent</description></item>
    ///   <item><description><see cref="SpecialProtocolType.Telepathy"/>: Rune-Lupin - mental openness required</description></item>
    ///   <item><description><see cref="SpecialProtocolType.BloodOath"/>: Iron-Bane - binding agreements with consequences</description></item>
    ///   <item><description><see cref="SpecialProtocolType.HospitalityRite"/>: Gorge-Maw - extended greeting ceremony required</description></item>
    ///   <item><description><see cref="SpecialProtocolType.MartialTribute"/>: Iron-Bane - martial status demonstration required</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// if (protocolService.HasSpecialProtocol("utgard", SpecialProtocolType.VeilSpeech))
    /// {
    ///     Console.WriteLine("Apply Veil-Speech rules for this interaction");
    /// }
    ///
    /// if (protocolService.HasSpecialProtocol("iron-bane", SpecialProtocolType.BloodOath))
    /// {
    ///     Console.WriteLine("Blood Oath may be required for serious agreements");
    /// }
    /// </code>
    /// </example>
    bool HasSpecialProtocol(string cultureId, SpecialProtocolType type);

    // =========================================================================
    // DC and Dice Pool Calculation Methods
    // =========================================================================

    /// <summary>
    /// Calculates the effective DC for a social interaction, applying all protocol-based modifiers.
    /// </summary>
    /// <param name="cultureId">
    /// The target culture identifier.
    /// </param>
    /// <param name="compliance">
    /// The result of the protocol compliance check.
    /// </param>
    /// <param name="cantModifier">
    /// Optional cant modifier (cants affect dice pool, not DC, but included for completeness).
    /// </param>
    /// <param name="veilSpeechContext">
    /// Optional Veil-Speech context for Utgard interactions.
    /// </param>
    /// <returns>
    /// The effective DC after applying all cultural modifiers.
    /// </returns>
    /// <remarks>
    /// <para>
    /// The effective DC is calculated as:
    /// </para>
    /// <code>
    /// Effective DC = Base DC + Protocol Violation DC + Veil-Speech DC Adjustment
    /// </code>
    /// <para>
    /// DC modifiers:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Base DC varies by culture (12-18)</description></item>
    ///   <item><description>Protocol violations add +2/+4/+6 DC</description></item>
    ///   <item><description>Veil-Speech deception reduces DC by 4 (Utgard only)</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var compliance = protocolService.CheckProtocolCompliance(characterId, "dvergr", context);
    /// var cantMod = protocolService.GetCantModifier(CantFluency.Fluent, "dvergr");
    ///
    /// var effectiveDc = protocolService.CalculateEffectiveDc("dvergr", compliance, cantMod, null);
    /// Console.WriteLine($"Effective DC: {effectiveDc}"); // 18 (base) + violation modifiers
    /// </code>
    /// </example>
    int CalculateEffectiveDc(
        string cultureId,
        ProtocolCheckResult compliance,
        CantModifier? cantModifier,
        VeilSpeechContext? veilSpeechContext);

    /// <summary>
    /// Calculates the total dice pool modifier from all cultural protocol sources.
    /// </summary>
    /// <param name="cantModifier">
    /// Optional cant modifier (-1 to +1 based on fluency).
    /// </param>
    /// <param name="veilSpeechContext">
    /// Optional Veil-Speech context (-2 to +1 based on communication style).
    /// </param>
    /// <param name="compliance">
    /// The result of the protocol compliance check (may include dice penalties).
    /// </param>
    /// <returns>
    /// The total dice pool modifier (positive for bonuses, negative for penalties).
    /// </returns>
    /// <remarks>
    /// <para>
    /// The total dice modifier is the sum of:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Cant fluency: -1 (None), 0 (Basic), +1 (Fluent)</description></item>
    ///   <item><description>Veil-Speech: -2 (Direct Truth), 0 (Neutral), +1 (Proper)</description></item>
    ///   <item><description>Protocol violations: 0 (Minor), -1 (Moderate), -2 (Severe)</description></item>
    /// </list>
    /// <para>
    /// This modifier is applied to the Rhetoric skill dice pool for social interaction checks.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var cantMod = protocolService.GetCantModifier(CantFluency.Fluent, "dvergr"); // +1
    /// var compliance = ProtocolCheckResult.Compliant(protocol, modifiers);          // 0
    ///
    /// var totalDiceMod = protocolService.CalculateDiceModifier(cantMod, null, compliance);
    /// Console.WriteLine($"Total dice modifier: {totalDiceMod}"); // +1
    /// </code>
    /// </example>
    int CalculateDiceModifier(
        CantModifier? cantModifier,
        VeilSpeechContext? veilSpeechContext,
        ProtocolCheckResult compliance);

    // =========================================================================
    // State Management Methods
    // =========================================================================

    /// <summary>
    /// Updates the Veil-Speech state for a character-NPC pair after an interaction.
    /// </summary>
    /// <param name="characterId">
    /// The unique identifier of the character.
    /// </param>
    /// <param name="npcId">
    /// The unique identifier of the Utgard NPC.
    /// </param>
    /// <param name="newState">
    /// The new Veil-Speech state to set.
    /// </param>
    /// <remarks>
    /// <para>
    /// This method updates the persistent Veil-Speech state between a character and an Utgard NPC.
    /// State transitions typically occur as follows:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Neutral → Respected: Used proper Veil-Speech</description></item>
    ///   <item><description>Neutral → Offended: Told direct truth</description></item>
    ///   <item><description>Offended → DeepOffense: Repeated offense</description></item>
    ///   <item><description>Offended → Neutral: Apologized or time passed</description></item>
    ///   <item><description>Respected → Neutral: Time passed without reinforcement</description></item>
    /// </list>
    /// <para>
    /// DeepOffense is typically permanent unless extraordinary measures are taken.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Character used proper Veil-Speech
    /// protocolService.UpdateVeilSpeechState("player-1", "utgard-noble-1", VeilSpeechState.Respected);
    ///
    /// // Character told direct truth to already-offended NPC
    /// var currentState = protocolService.GetVeilSpeechState("player-1", "utgard-merchant-1");
    /// if (currentState == VeilSpeechState.Offended)
    /// {
    ///     protocolService.UpdateVeilSpeechState("player-1", "utgard-merchant-1", VeilSpeechState.DeepOffense);
    /// }
    /// </code>
    /// </example>
    void UpdateVeilSpeechState(string characterId, string npcId, VeilSpeechState newState);

    /// <summary>
    /// Clears all protocol-related state for a character (used for game reset or testing).
    /// </summary>
    /// <param name="characterId">
    /// The unique identifier of the character whose state should be cleared.
    /// </param>
    /// <remarks>
    /// <para>
    /// This method removes all tracked protocol information for a character, including:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Veil-Speech states with all NPCs</description></item>
    ///   <item><description>Recorded protocol violations</description></item>
    ///   <item><description>Any other per-character protocol state</description></item>
    /// </list>
    /// <para>
    /// This is primarily used for testing or when the game state needs to be reset.
    /// Use with caution in production as it removes all cultural relationship history.
    /// </para>
    /// </remarks>
    void ClearCharacterState(string characterId);
}
