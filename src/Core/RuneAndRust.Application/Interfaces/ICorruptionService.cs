// ═══════════════════════════════════════════════════════════════════════════════
// ICorruptionService.cs
// Interface defining the contract for managing character Runic Blight Corruption,
// including corruption state queries, penalty calculations, corruption application
// from heretical sources, Blot-Priest corruption transfer, rare corruption removal,
// and Terminal Error survival checks.
// Version: 0.18.1c
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Application.Exceptions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Service interface for managing character Runic Blight Corruption.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Overview:</strong>
/// The corruption service handles all aspects of the Runic Blight system —
/// the near-permanent negative resource that represents spiritual and physical
/// contamination from exposure to corrupted runic energies. Unlike Psychic Stress
/// (which recovers through rest), corruption accumulates over a character's
/// lifetime with almost no recovery options, creating permanent mechanical
/// consequences that shape character identity.
/// </para>
/// <para>
/// <strong>Responsibilities:</strong>
/// </para>
/// <list type="bullet">
///   <item>
///     <description>
///       <strong>Querying:</strong> Read current corruption state and derived penalties
///       (max HP/AP percentage penalty, Resolve dice penalty, stage-based skill
///       modifiers) without modifying character state.
///     </description>
///   </item>
///   <item>
///     <description>
///       <strong>Adding Corruption:</strong> Apply corruption from various sources
///       (MysticMagic, HereticalAbility, Artifact, Environmental, Consumable,
///       Ritual, ForlornContact, BlightTransfer) with threshold crossing detection
///       and Terminal Error triggering.
///     </description>
///   </item>
///   <item>
///     <description>
///       <strong>Transferring:</strong> Transfer corruption between characters via the
///       Blot-Priest specialization mechanic — the Blot-Priest absorbs an ally's
///       Blight at personal cost.
///     </description>
///   </item>
///   <item>
///     <description>
///       <strong>Removing:</strong> Remove corruption through extremely rare narrative
///       events such as divine purification rituals, Pure Essence artifacts, or
///       story-specific resolutions. There is NO natural corruption recovery.
///     </description>
///   </item>
///   <item>
///     <description>
///       <strong>Terminal Error:</strong> Perform WILL-based survival checks when
///       corruption reaches 100. Failure transforms the character into a Forlorn
///       NPC — the ultimate consequence of corruption accumulation.
///     </description>
///   </item>
/// </list>
/// <para>
/// <strong>Corruption Mechanics:</strong>
/// Corruption is a near-permanent negative resource in the range 0-100. As
/// corruption accumulates, characters suffer escalating penalties across six
/// stage tiers:
/// </para>
/// <list type="table">
///   <listheader>
///     <term>Stage</term>
///     <description>Effect</description>
///   </listheader>
///   <item>
///     <term>Uncorrupted (0-19)</term>
///     <description>No penalties</description>
///   </item>
///   <item>
///     <term>Tainted (20-39)</term>
///     <description>Tech +1, Social -1</description>
///   </item>
///   <item>
///     <term>Infected (40-59)</term>
///     <description>Tech +2, Social -2, Faction locked</description>
///   </item>
///   <item>
///     <term>Blighted (60-79)</term>
///     <description>Tech +2, Social -2, Machine Affinity</description>
///   </item>
///   <item>
///     <term>Corrupted (80-99)</term>
///     <description>Tech +2, Social -2, NPCs fear character</description>
///   </item>
///   <item>
///     <term>Consumed (100)</term>
///     <description>Terminal Error — character lost</description>
///   </item>
/// </list>
/// <para>
/// <strong>Resource Penalties:</strong>
/// In addition to stage-based modifiers, corruption imposes escalating resource
/// penalties calculated from the raw corruption value:
/// </para>
/// <list type="bullet">
///   <item>
///     <description>
///       <strong>Max HP Penalty:</strong> <c>floor(corruption / 10) * 5</c>% (0-50%)
///     </description>
///   </item>
///   <item>
///     <description>
///       <strong>Max AP Penalty:</strong> <c>floor(corruption / 10) * 5</c>% (0-50%)
///     </description>
///   </item>
///   <item>
///     <description>
///       <strong>Resolve Dice Penalty:</strong> <c>floor(corruption / 20)</c> dice (0-5)
///     </description>
///   </item>
/// </list>
/// <para>
/// <strong>Consumers:</strong> CombatService (HP/AP penalties), SkillCheckService
/// (tech bonus, social penalty), AbilityService (heretical corruption costs),
/// Blot-Priest Mechanics (corruption transfer), RestService (no natural recovery),
/// UI Layer (corruption bar display).
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Query corruption state and penalties
/// var state = corruptionService.GetCorruptionState(characterId);
/// var hpPenalty = corruptionService.GetMaxHpPenaltyPercent(characterId);
/// var resolvePenalty = corruptionService.GetResolveDicePenalty(characterId);
///
/// // Add corruption from heretical ability
/// var result = corruptionService.AddCorruption(
///     characterId,
///     amount: 15,
///     source: CorruptionSource.HereticalAbility);
///
/// if (result.IsTerminalError)
///     InitiateTerminalErrorCheck(characterId);
///
/// // Blot-Priest cleanses ally (at personal cost)
/// var transfer = corruptionService.TransferCorruption(
///     fromCharacterId: targetAllyId,
///     toCharacterId: blotPriestId,
///     amount: 10);
///
/// if (transfer.TargetTerminalError)
///     InitiateTerminalErrorCheck(blotPriestId);
///
/// // Query skill modifiers for check resolution
/// var modifiers = corruptionService.GetSkillModifiers(characterId);
/// if (isTechCheck)
///     dicePool += modifiers.TechBonus;
/// </code>
/// </example>
/// <seealso cref="CorruptionState"/>
/// <seealso cref="CorruptionAddResult"/>
/// <seealso cref="CorruptionTransferResult"/>
/// <seealso cref="CorruptionSkillModifiers"/>
/// <seealso cref="TerminalErrorResult"/>
/// <seealso cref="CorruptionStage"/>
/// <seealso cref="CorruptionSource"/>
public interface ICorruptionService
{
    #region Query Methods

    /// <summary>
    /// Gets the current corruption state for a character.
    /// </summary>
    /// <param name="characterId">
    /// The character's unique identifier. Must correspond to an existing character
    /// in the repository.
    /// </param>
    /// <returns>
    /// A <see cref="CorruptionState"/> snapshot containing the current corruption value,
    /// computed stage tier, percentage to consumption, mutation risk flags, and
    /// consumed/uncorrupted convenience properties.
    /// </returns>
    /// <exception cref="CharacterNotFoundException">
    /// Thrown when no character exists with the specified <paramref name="characterId"/>.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This is a read-only query with no side effects. The returned
    /// <see cref="CorruptionState"/> is an immutable value object — modifying it
    /// does not affect the character's persisted corruption.
    /// </para>
    /// <para>
    /// Use the convenience methods <see cref="GetMaxHpPenaltyPercent"/>,
    /// <see cref="GetMaxApPenaltyPercent"/>, <see cref="GetResolveDicePenalty"/>,
    /// and <see cref="GetSkillModifiers"/> when only a single derived value or
    /// modifier set is needed.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var state = corruptionService.GetCorruptionState(characterId);
    /// Console.WriteLine($"Corruption: {state.CurrentCorruption}/{CorruptionState.MaxCorruption}");
    /// Console.WriteLine($"Stage: {state.Stage}");
    /// Console.WriteLine($"Mutation Risk: {state.HasMutationRisk}");
    /// </code>
    /// </example>
    CorruptionState GetCorruptionState(Guid characterId);

    /// <summary>
    /// Gets the maximum HP percentage penalty from the character's current corruption level.
    /// </summary>
    /// <param name="characterId">
    /// The character's unique identifier. Must correspond to an existing character
    /// in the repository.
    /// </param>
    /// <returns>
    /// The HP penalty percentage as an integer in the range 0-50, calculated as
    /// <c>floor(corruption / 10) * 5</c>. Returns 0 for corruption 0-9 and 50 for
    /// corruption 100.
    /// </returns>
    /// <exception cref="CharacterNotFoundException">
    /// Thrown when no character exists with the specified <paramref name="characterId"/>.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This is a convenience method equivalent to:
    /// <c>GetCorruptionTracker(characterId).MaxHpPenaltyPercent</c>
    /// </para>
    /// <para>
    /// Primarily consumed by the combat system when calculating effective maximum HP.
    /// The effective max HP is: <c>BaseMaxHP * (1 - penalty / 100)</c>.
    /// </para>
    /// <para>
    /// <strong>Penalty Table:</strong>
    /// </para>
    /// <list type="table">
    ///   <listheader>
    ///     <term>Corruption</term>
    ///     <description>Max HP Penalty</description>
    ///   </listheader>
    ///   <item>
    ///     <term>0-9</term>
    ///     <description>0%</description>
    ///   </item>
    ///   <item>
    ///     <term>10-19</term>
    ///     <description>5%</description>
    ///   </item>
    ///   <item>
    ///     <term>20-29</term>
    ///     <description>10%</description>
    ///   </item>
    ///   <item>
    ///     <term>30-39</term>
    ///     <description>15%</description>
    ///   </item>
    ///   <item>
    ///     <term>40-49</term>
    ///     <description>20%</description>
    ///   </item>
    ///   <item>
    ///     <term>50-59</term>
    ///     <description>25%</description>
    ///   </item>
    ///   <item>
    ///     <term>60-69</term>
    ///     <description>30%</description>
    ///   </item>
    ///   <item>
    ///     <term>70-79</term>
    ///     <description>35%</description>
    ///   </item>
    ///   <item>
    ///     <term>80-89</term>
    ///     <description>40%</description>
    ///   </item>
    ///   <item>
    ///     <term>90-99</term>
    ///     <description>45%</description>
    ///   </item>
    ///   <item>
    ///     <term>100</term>
    ///     <description>50%</description>
    ///   </item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // During combat HP calculation
    /// int hpPenaltyPercent = corruptionService.GetMaxHpPenaltyPercent(characterId);
    /// int effectiveMaxHp = baseMaxHp * (100 - hpPenaltyPercent) / 100;
    /// </code>
    /// </example>
    int GetMaxHpPenaltyPercent(Guid characterId);

    /// <summary>
    /// Gets the maximum AP percentage penalty from the character's current corruption level.
    /// </summary>
    /// <param name="characterId">
    /// The character's unique identifier. Must correspond to an existing character
    /// in the repository.
    /// </param>
    /// <returns>
    /// The AP penalty percentage as an integer in the range 0-50, calculated as
    /// <c>floor(corruption / 10) * 5</c>. Same formula as the HP penalty.
    /// </returns>
    /// <exception cref="CharacterNotFoundException">
    /// Thrown when no character exists with the specified <paramref name="characterId"/>.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This is a convenience method equivalent to:
    /// <c>GetCorruptionTracker(characterId).MaxApPenaltyPercent</c>
    /// </para>
    /// <para>
    /// Primarily consumed by the combat system when calculating effective maximum AP.
    /// The effective max AP is: <c>BaseMaxAP * (1 - penalty / 100)</c>.
    /// </para>
    /// <para>
    /// The AP penalty uses the same formula as the HP penalty, representing the
    /// character's diminished vitality and spiritual energy as corruption erodes
    /// their connection to the runic substrate.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // During combat AP calculation
    /// int apPenaltyPercent = corruptionService.GetMaxApPenaltyPercent(characterId);
    /// int effectiveMaxAp = baseMaxAp * (100 - apPenaltyPercent) / 100;
    /// </code>
    /// </example>
    int GetMaxApPenaltyPercent(Guid characterId);

    /// <summary>
    /// Gets the Resolve dice pool penalty from the character's current corruption level.
    /// </summary>
    /// <param name="characterId">
    /// The character's unique identifier. Must correspond to an existing character
    /// in the repository.
    /// </param>
    /// <returns>
    /// The Resolve penalty as an integer in the range 0-5, calculated as
    /// <c>floor(corruption / 20)</c>. Returns 0 for corruption 0-19 and 5 for
    /// corruption 100.
    /// </returns>
    /// <exception cref="CharacterNotFoundException">
    /// Thrown when no character exists with the specified <paramref name="characterId"/>.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This is a convenience method equivalent to:
    /// <c>GetCorruptionTracker(characterId).ResolveDicePenalty</c>
    /// </para>
    /// <para>
    /// The Resolve penalty affects all WILL-based checks including stress resistance
    /// checks and Trauma Checks, creating a dangerous feedback loop:
    /// </para>
    /// <para>
    /// High Corruption -> Resolve Penalty -> Weaker Stress Resistance -> More Stress
    /// -> Trauma Check with Fewer Dice -> More Traumas
    /// </para>
    /// <para>
    /// This cross-system interaction between corruption and stress (v0.18.5 integration)
    /// is one of the core horror mechanics of the Trauma Economy.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // During WILL-based check resolution
    /// int resolvePenalty = corruptionService.GetResolveDicePenalty(characterId);
    /// int effectiveWillPool = baseWill - resolvePenalty;
    /// // Minimum 1 die to avoid auto-fail
    /// effectiveWillPool = Math.Max(1, effectiveWillPool);
    /// </code>
    /// </example>
    int GetResolveDicePenalty(Guid characterId);

    /// <summary>
    /// Gets the skill check modifiers from the character's current corruption stage.
    /// </summary>
    /// <param name="characterId">
    /// The character's unique identifier. Must correspond to an existing character
    /// in the repository.
    /// </param>
    /// <returns>
    /// A <see cref="CorruptionSkillModifiers"/> containing the stage-based TechBonus,
    /// SocialPenalty, and FactionLocked status as a single composite object.
    /// </returns>
    /// <exception cref="CharacterNotFoundException">
    /// Thrown when no character exists with the specified <paramref name="characterId"/>.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Returns a composite object rather than individual values to reduce query overhead
    /// when multiple modifiers are needed simultaneously (common during skill check resolution).
    /// </para>
    /// <para>
    /// Stage-based modifiers:
    /// <list type="bullet">
    ///   <item><description>Uncorrupted: +0 Tech, -0 Social, no faction lock</description></item>
    ///   <item><description>Tainted: +1 Tech, -1 Social, no faction lock</description></item>
    ///   <item><description>Infected/Blighted/Corrupted: +2 Tech, -2 Social, faction locked</description></item>
    ///   <item><description>Consumed: N/A — character is lost to Terminal Error</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var modifiers = corruptionService.GetSkillModifiers(characterId);
    ///
    /// if (isTechCheck)
    ///     dicePool += modifiers.TechBonus;    // +1 or +2
    /// if (isSocialCheck)
    ///     dicePool += modifiers.SocialPenalty; // -1 or -2
    /// if (modifiers.FactionLocked)
    ///     blockFactionReputationGain();
    /// </code>
    /// </example>
    CorruptionSkillModifiers GetSkillModifiers(Guid characterId);

    #endregion

    #region Command Methods

    /// <summary>
    /// Adds corruption to a character from a specified source.
    /// </summary>
    /// <param name="characterId">
    /// The character's unique identifier. Must correspond to an existing character
    /// in the repository.
    /// </param>
    /// <param name="amount">
    /// The corruption amount to add. Must be a positive integer. Typical values
    /// range from 1 (minor Blight exposure) to 15 (overcasting powerful spells).
    /// </param>
    /// <param name="source">
    /// The category of corruption source (MysticMagic, HereticalAbility,
    /// Environmental, etc.). Used for history tracking, analytics, and contextual
    /// narrative events.
    /// </param>
    /// <returns>
    /// A <see cref="CorruptionAddResult"/> containing:
    /// <list type="bullet">
    ///   <item><description>Previous and new corruption values with computed delta.</description></item>
    ///   <item><description>The corruption source for history tracking.</description></item>
    ///   <item><description>Threshold crossing (25, 50, 75, or null) — each fires once per character.</description></item>
    ///   <item><description>Stage transition detection (e.g., Tainted to Infected).</description></item>
    ///   <item><description>Terminal Error flag when corruption reaches 100.</description></item>
    ///   <item><description>Faction lock detection when corruption first crosses 50.</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="CharacterNotFoundException">
    /// Thrown when no character exists with the specified <paramref name="characterId"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown if <paramref name="amount"/> is negative.
    /// </exception>
    /// <remarks>
    /// <para>
    /// <strong>Corruption Sources:</strong> Each source has narrative significance and
    /// may trigger different UI presentation:
    /// <list type="bullet">
    ///   <item><description>MysticMagic: Standard spellcasting side-effect (0-2 per spell).</description></item>
    ///   <item><description>HereticalAbility: Voluntary use of forbidden power (2-5 per use).</description></item>
    ///   <item><description>Artifact: Interaction with glitched runic artifacts (1-5).</description></item>
    ///   <item><description>Environmental: Exposure to Blight zones (1-3 per encounter).</description></item>
    ///   <item><description>Consumable: Usage of corrupted substances (2-5).</description></item>
    ///   <item><description>Ritual: Participation in forbidden rituals (variable).</description></item>
    ///   <item><description>ForlornContact: Direct contact with Forlorn entities (5-10).</description></item>
    ///   <item><description>BlightTransfer: Receiving corruption from a Blot-Priest transfer.</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Clamping:</strong> The resulting corruption is clamped to the range 0-100.
    /// Corruption cannot exceed 100.
    /// </para>
    /// <para>
    /// <strong>Terminal Error:</strong> After adding corruption, check
    /// <see cref="CorruptionAddResult.IsTerminalError"/> to determine if a Terminal
    /// Error check should be initiated via <see cref="PerformTerminalErrorCheck"/>.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Add corruption from heretical ability use
    /// var result = corruptionService.AddCorruption(
    ///     characterId,
    ///     amount: 5,
    ///     source: CorruptionSource.HereticalAbility);
    ///
    /// if (result.ThresholdCrossed.HasValue)
    ///     ShowThresholdNotification(result.ThresholdCrossed.Value);
    ///
    /// if (result.StageCrossed)
    ///     ShowStageTransition(result.PreviousStage, result.NewStage);
    ///
    /// if (result.IsTerminalError)
    ///     InitiateTerminalErrorCheck(characterId);
    ///
    /// // Add environmental corruption from Blight zone
    /// var envResult = corruptionService.AddCorruption(
    ///     characterId,
    ///     amount: 3,
    ///     source: CorruptionSource.Environmental);
    /// </code>
    /// </example>
    CorruptionAddResult AddCorruption(Guid characterId, int amount, CorruptionSource source);

    /// <summary>
    /// Transfers corruption from one character to another.
    /// </summary>
    /// <param name="fromCharacterId">
    /// The source character (being cleansed). Their corruption will be reduced
    /// by the transfer amount.
    /// </param>
    /// <param name="toCharacterId">
    /// The target character (absorbing corruption). Their corruption will be
    /// increased by the transfer amount. Typically a Blot-Priest.
    /// </param>
    /// <param name="amount">
    /// The amount of corruption to transfer. Must be a positive integer and must
    /// not exceed the source character's current corruption.
    /// </param>
    /// <returns>
    /// A <see cref="CorruptionTransferResult"/> containing:
    /// <list type="bullet">
    ///   <item><description>Whether the transfer succeeded.</description></item>
    ///   <item><description>The actual amount of corruption transferred.</description></item>
    ///   <item><description>Both characters' new corruption values.</description></item>
    ///   <item><description>Whether the target reached Terminal Error (corruption 100).</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="CharacterNotFoundException">
    /// Thrown when either character does not exist.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown if <paramref name="amount"/> is negative or exceeds the source
    /// character's current corruption.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="fromCharacterId"/> equals <paramref name="toCharacterId"/>
    /// (self-transfer is not permitted).
    /// </exception>
    /// <remarks>
    /// <para>
    /// <strong>Blot-Priest Mechanic:</strong>
    /// This is the signature ability of the Blot-Priest specialization. The
    /// Blot-Priest willingly absorbs Blight from others, sacrificing their own
    /// purity for the good of their allies. This represents one of the most
    /// selfless acts in the setting — knowingly taking near-permanent corruption
    /// to save another.
    /// </para>
    /// <para>
    /// <strong>Transfer Rules:</strong>
    /// <list type="bullet">
    ///   <item><description>Cannot transfer more corruption than the source has.</description></item>
    ///   <item><description>Source's corruption reduced by the transfer amount.</description></item>
    ///   <item><description>Target's corruption increased by the transfer amount.</description></item>
    ///   <item><description>Both updates are applied atomically.</description></item>
    ///   <item><description>Self-transfer is not permitted.</description></item>
    ///   <item><description>If target reaches 100, Terminal Error is flagged in the result.</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Terminal Error on Transfer:</strong>
    /// If the transfer causes the target's corruption to reach 100, check
    /// <see cref="CorruptionTransferResult.TargetTerminalError"/> and initiate a
    /// Terminal Error check via <see cref="PerformTerminalErrorCheck"/> for the
    /// target character.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Blot-Priest cleanses ally (absorbs 10 corruption)
    /// var transfer = corruptionService.TransferCorruption(
    ///     fromCharacterId: allyId,
    ///     toCharacterId: blotPriestId,
    ///     amount: 10);
    ///
    /// if (transfer.Success)
    /// {
    ///     Console.WriteLine($"Transferred {transfer.AmountTransferred} corruption");
    ///     Console.WriteLine($"Ally: {transfer.SourceNewCorruption}, Priest: {transfer.TargetNewCorruption}");
    /// }
    ///
    /// if (transfer.TargetTerminalError)
    ///     InitiateTerminalErrorCheck(blotPriestId);
    /// </code>
    /// </example>
    CorruptionTransferResult TransferCorruption(
        Guid fromCharacterId,
        Guid toCharacterId,
        int amount);

    /// <summary>
    /// Removes corruption from a character (extremely rare).
    /// </summary>
    /// <param name="characterId">
    /// The character's unique identifier. Must correspond to an existing character
    /// in the repository.
    /// </param>
    /// <param name="amount">
    /// The amount of corruption to remove. Must be a positive integer.
    /// </param>
    /// <param name="reason">
    /// The narrative reason for corruption removal. Must be a significant story
    /// event or ritual. This is logged for history tracking and auditing.
    /// </param>
    /// <returns>
    /// <c>true</c> if corruption was successfully removed; <c>false</c> if the
    /// request was invalid (e.g., amount exceeds current corruption, or the
    /// character has no corruption to remove).
    /// </returns>
    /// <exception cref="CharacterNotFoundException">
    /// Thrown when no character exists with the specified <paramref name="characterId"/>.
    /// </exception>
    /// <remarks>
    /// <para>
    /// <strong>IMPORTANT:</strong> Corruption removal is EXTREMELY rare in the
    /// Aethelgard setting. Unlike stress recovery (which occurs through rest),
    /// there is NO natural corruption reduction. This method should be called
    /// sparingly and only for significant narrative events.
    /// </para>
    /// <para>
    /// Valid reasons for corruption removal:
    /// <list type="bullet">
    ///   <item><description>Divine purification ritual (major quest reward).</description></item>
    ///   <item><description>Consuming a Pure Essence (consumable artifact).</description></item>
    ///   <item><description>Story-specific narrative event (GM discretion).</description></item>
    ///   <item><description>Divine intervention (extreme rarity).</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// The <paramref name="reason"/> string is logged at Warning level because
    /// corruption removal is a significant event that should be tracked and
    /// auditable. Any corruption removal outside the documented reasons should
    /// be investigated.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Divine purification ritual after completing major quest
    /// bool removed = corruptionService.RemoveCorruption(
    ///     characterId,
    ///     amount: 15,
    ///     reason: "Divine Purification Ritual - Temple of the Architects");
    ///
    /// if (removed)
    ///     Console.WriteLine("The Blight recedes... for now.");
    /// else
    ///     Console.WriteLine("The purification failed to take hold.");
    /// </code>
    /// </example>
    bool RemoveCorruption(Guid characterId, int amount, string reason);

    #endregion

    #region Terminal Error Methods

    /// <summary>
    /// Performs a Terminal Error survival check when corruption reaches 100.
    /// </summary>
    /// <param name="characterId">
    /// The character's unique identifier. Must correspond to an existing character
    /// whose corruption is at exactly 100.
    /// </param>
    /// <returns>
    /// A <see cref="TerminalErrorResult"/> containing:
    /// <list type="bullet">
    ///   <item><description>Whether the character survived (Survived flag).</description></item>
    ///   <item><description>Final corruption value (99 if survived, 100 if Forlorn).</description></item>
    ///   <item><description>Whether the character became Forlorn (BecameForlorn flag).</description></item>
    ///   <item><description>Roll details (SuccessesRolled vs RequiredDc).</description></item>
    ///   <item><description>Critical success detection (successes >= 2x DC).</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="CharacterNotFoundException">
    /// Thrown when no character exists with the specified <paramref name="characterId"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the character's corruption is not at 100 (Terminal Error check
    /// is only valid when corruption has reached the maximum).
    /// </exception>
    /// <remarks>
    /// <para>
    /// <strong>Terminal Error Check Mechanics:</strong>
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       <strong>Trigger:</strong> Character's corruption reaches exactly 100.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <strong>Roll:</strong> WILL dice pool (affected by Resolve dice penalty
    ///       of <c>floor(corruption / 20)</c> = 5 dice at corruption 100) vs DC 3.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <strong>Success (successes >= DC):</strong> Character survives. Corruption
    ///       is set to 99, leaving them one point from another Terminal Error. The
    ///       character is alive but deeply marked by the Blight.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <strong>Critical Success (successes >= 2x DC):</strong> Character survives
    ///       with additional narrative benefit — potential insight into the Blight's
    ///       nature or temporary resistance to further corruption.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <strong>Failure (successes &lt; DC):</strong> Character becomes Forlorn —
    ///       an unplayable NPC controlled by the GM, consumed by the Blight. The
    ///       character may appear as an antagonist in future encounters.
    ///     </description>
    ///   </item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Resolve Penalty at Terminal Error:</strong>
    /// At corruption 100, the Resolve dice penalty is <c>floor(100/20) = 5</c>,
    /// which severely hampers the survival check. A character with WILL 6 would
    /// roll only 1 die against DC 3, making survival extremely difficult. This
    /// is an intentional design choice — reaching Terminal Error should feel
    /// genuinely dangerous.
    /// </para>
    /// <para>
    /// <strong>Post-Check State:</strong>
    /// <list type="bullet">
    ///   <item><description>Survived: Corruption set to 99, character continues play.</description></item>
    ///   <item><description>Failed: Character is Forlorn — removed from active play, becomes NPC.</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // After AddCorruption indicates Terminal Error
    /// if (addResult.IsTerminalError)
    /// {
    ///     var checkResult = corruptionService.PerformTerminalErrorCheck(characterId);
    ///
    ///     if (checkResult.Survived)
    ///     {
    ///         Console.WriteLine($"Survived! Corruption reduced to {checkResult.FinalCorruption}");
    ///         if (checkResult.WasCriticalSuccess)
    ///             Console.WriteLine("Critical success — gained insight into the Blight!");
    ///     }
    ///     else
    ///     {
    ///         Console.WriteLine("The character has been lost to the Blight...");
    ///         RetireCharacterAsForlorn(characterId);
    ///     }
    /// }
    /// </code>
    /// </example>
    TerminalErrorResult PerformTerminalErrorCheck(Guid characterId);

    #endregion
}
