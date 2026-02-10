// ═══════════════════════════════════════════════════════════════════════════════
// SpecializationAbilityTier.cs
// Value object representing a tier of abilities within a specialization.
// Each specialization has up to four tiers (1-4) with escalating PP unlock costs
// and rank requirements. Tiers contain 2-4 abilities (mix of active and passive).
// Version: 0.17.4c
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

using Microsoft.Extensions.Logging;

/// <summary>
/// Represents a tier of abilities within a specialization.
/// </summary>
/// <remarks>
/// <para>
/// SpecializationAbilityTier groups abilities into progressive tiers that are
/// unlocked through Progression Points (PP) as a character advances in their
/// specialization. Each specialization has exactly three tiers with escalating
/// costs and requirements.
/// </para>
/// <para>
/// The three-tier structure follows a standard progression:
/// </para>
/// <list type="bullet">
///   <item>
///     <description>
///       <b>Tier 1 — Core Techniques</b>: Unlocked immediately when the specialization
///       is selected (0 PP). Represents foundational abilities that define the
///       specialization's identity. Requires Rank 1.
///     </description>
///   </item>
///   <item>
///     <description>
///       <b>Tier 2 — Advanced Mastery</b>: Unlocked at Rank 2 with 2 PP investment.
///       Requires Tier 1 to be unlocked. Represents more powerful or nuanced
///       abilities that build on the core foundation.
///     </description>
///   </item>
///   <item>
///     <description>
///       <b>Tier 3 — Ultimate Power</b>: Unlocked at Rank 3 with 3 PP investment.
///       Requires Tier 2 to be unlocked. Represents the pinnacle of specialization
///       mastery with the most powerful abilities.
///     </description>
///   </item>
/// </list>
/// <para>
/// Each tier contains 2-4 abilities, which can be a mix of active and passive types.
/// Ability IDs must be unique within a tier (cross-tier uniqueness is validated at
/// the <see cref="RuneAndRust.Domain.Entities.SpecializationDefinition"/> level).
/// </para>
/// <para>
/// The <see cref="CanUnlock"/> method provides runtime validation for tier unlock
/// eligibility, checking rank requirements, previous tier prerequisites, and
/// available Progression Points.
/// </para>
/// <para>
/// This is an immutable value object using the <c>readonly record struct</c> pattern.
/// All instances are created via the <see cref="Create"/> factory method or
/// convenience factories <see cref="CreateTier1"/>, <see cref="CreateTier2"/>,
/// and <see cref="CreateTier3"/>.
/// </para>
/// </remarks>
/// <seealso cref="SpecializationAbility"/>
/// <seealso cref="SpecialResourceDefinition"/>
/// <seealso cref="RuneAndRust.Domain.Entities.SpecializationDefinition"/>
/// <seealso cref="RuneAndRust.Domain.Enums.SpecializationId"/>
public readonly record struct SpecializationAbilityTier
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE FIELDS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Logger for detailed diagnostic output during tier creation and unlock checks.
    /// </summary>
    private static ILogger<SpecializationAbilityTier>? _logger;

    // ═══════════════════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the tier number (1, 2, 3, or 4).
    /// </summary>
    /// <value>
    /// An integer from 1 to 4 representing the tier level.
    /// Higher tiers require more PP and higher rank to unlock.
    /// </value>
    /// <remarks>
    /// <para>Tier progression:</para>
    /// <list type="bullet">
    ///   <item><description>Tier 1: Foundation — free on specialization selection</description></item>
    ///   <item><description>Tier 2: Intermediate — requires Tier 1, 2 PP, Rank 2</description></item>
    ///   <item><description>Tier 3: Pinnacle — requires Tier 2, 3 PP, Rank 3</description></item>
    /// </list>
    /// </remarks>
    public int Tier { get; init; }

    /// <summary>
    /// Gets the display name for this tier.
    /// </summary>
    /// <value>
    /// A player-friendly name shown in the UI (e.g., "Core Techniques",
    /// "Advanced Mastery", "Ultimate Power").
    /// </value>
    /// <example>"Core Techniques", "Advanced Mastery", "Ultimate Power"</example>
    public string DisplayName { get; init; }

    /// <summary>
    /// Gets the Progression Point cost to unlock this tier.
    /// </summary>
    /// <value>
    /// Non-negative integer. Standard costs: Tier 1 = 0, Tier 2 = 2, Tier 3 = 3.
    /// </value>
    /// <remarks>
    /// <para>
    /// Progression Points (PP) are earned through gameplay milestones and
    /// spent to advance specialization tiers. The total PP cost to fully
    /// unlock all three tiers is 5 PP (0 + 2 + 3).
    /// </para>
    /// <para>
    /// Tier 1 is always free (0 PP) as it represents the baseline abilities
    /// granted when a specialization is first selected during character creation.
    /// </para>
    /// </remarks>
    public int UnlockCost { get; init; }

    /// <summary>
    /// Gets whether unlocking this tier requires the previous tier to be unlocked.
    /// </summary>
    /// <value>
    /// <c>false</c> for Tier 1 (no previous tier exists);
    /// <c>true</c> for Tier 2 and Tier 3 (sequential unlock required).
    /// </value>
    /// <remarks>
    /// This enforces a strict sequential unlock order: players cannot skip
    /// tiers. Tier 2 requires Tier 1, and Tier 3 requires Tier 2.
    /// </remarks>
    public bool RequiresPreviousTier { get; init; }

    /// <summary>
    /// Gets the minimum progression rank required to unlock this tier.
    /// </summary>
    /// <value>
    /// Positive integer. Standard ranks: Tier 1 = Rank 1, Tier 2 = Rank 2, Tier 3 = Rank 3.
    /// </value>
    /// <remarks>
    /// Progression rank represents the character's overall advancement level.
    /// Characters start at Rank 1 upon selecting a specialization. Rank
    /// advancement gates access to higher-tier abilities.
    /// </remarks>
    public int RequiredRank { get; init; }

    /// <summary>
    /// Gets the abilities available in this tier.
    /// </summary>
    /// <value>
    /// A read-only list of 2-4 <see cref="SpecializationAbility"/> instances.
    /// Never null or empty after valid construction.
    /// </value>
    /// <remarks>
    /// <para>
    /// Abilities within a tier are unlocked simultaneously when the tier is
    /// unlocked. Each tier contains a mix of active and passive abilities
    /// that complement each other thematically and mechanically.
    /// </para>
    /// <para>
    /// Ability IDs must be unique within the tier. Cross-tier uniqueness
    /// is validated at the <see cref="RuneAndRust.Domain.Entities.SpecializationDefinition"/>
    /// level.
    /// </para>
    /// </remarks>
    public IReadOnlyList<SpecializationAbility> Abilities { get; init; }

    // ═══════════════════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the total number of abilities in this tier.
    /// </summary>
    /// <value>
    /// Non-negative integer. Typically 2-4 for a valid tier.
    /// Returns 0 if <see cref="Abilities"/> is null (default struct state).
    /// </value>
    public int AbilityCount => Abilities?.Count ?? 0;

    /// <summary>
    /// Gets the number of passive abilities in this tier.
    /// </summary>
    /// <value>
    /// Count of abilities where <see cref="SpecializationAbility.IsPassive"/> is <c>true</c>.
    /// Returns 0 if <see cref="Abilities"/> is null.
    /// </value>
    public int PassiveCount => Abilities?.Count(a => a.IsPassive) ?? 0;

    /// <summary>
    /// Gets the number of active abilities in this tier.
    /// </summary>
    /// <value>
    /// Count of abilities where <see cref="SpecializationAbility.IsActive"/> is <c>true</c>.
    /// Returns 0 if <see cref="Abilities"/> is null.
    /// </value>
    public int ActiveCount => Abilities?.Count(a => a.IsActive) ?? 0;

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new ability tier with comprehensive validation.
    /// </summary>
    /// <param name="tier">Tier number (1-4). Determines unlock requirements and position in progression.</param>
    /// <param name="displayName">UI display name (e.g., "Core Techniques"). Cannot be null or whitespace.</param>
    /// <param name="unlockCost">Progression Point cost to unlock. Must be non-negative.</param>
    /// <param name="requiresPreviousTier">
    /// Whether the previous tier must be unlocked first. Must be <c>false</c> for Tier 1,
    /// <c>true</c> for Tier 2 and 3.
    /// </param>
    /// <param name="requiredRank">Minimum progression rank required. Must be at least 1.</param>
    /// <param name="abilities">Abilities in this tier. Must contain at least one ability with unique IDs.</param>
    /// <param name="logger">Optional logger for diagnostic output during creation.</param>
    /// <returns>A validated and immutable <see cref="SpecializationAbilityTier"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="tier"/> is not 1-4, <paramref name="unlockCost"/> is negative,
    /// or <paramref name="requiredRank"/> is less than 1.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="displayName"/> is null or whitespace, <paramref name="abilities"/>
    /// is empty, contains duplicate ability IDs, or when <paramref name="requiresPreviousTier"/>
    /// does not match the tier number constraint.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="abilities"/> is null.
    /// </exception>
    /// <example>
    /// <code>
    /// // Create a Tier 1 with three abilities
    /// var tier1 = SpecializationAbilityTier.Create(
    ///     tier: 1,
    ///     displayName: "Core Techniques",
    ///     unlockCost: 0,
    ///     requiresPreviousTier: false,
    ///     requiredRank: 1,
    ///     abilities: new[]
    ///     {
    ///         SpecializationAbility.CreateActive("rage-strike", "Rage Strike", "A devastating blow.", 20, "rage"),
    ///         SpecializationAbility.CreatePassive("blood-frenzy", "Blood Frenzy", "+10% damage below 50% HP."),
    ///         SpecializationAbility.CreateActive("war-cry", "War Cry", "Intimidate nearby enemies.", 15, "rage")
    ///     });
    /// </code>
    /// </example>
    public static SpecializationAbilityTier Create(
        int tier,
        string displayName,
        int unlockCost,
        bool requiresPreviousTier,
        int requiredRank,
        IEnumerable<SpecializationAbility> abilities,
        ILogger<SpecializationAbilityTier>? logger = null)
    {
        // Store logger for this creation context
        _logger = logger;

        _logger?.LogDebug(
            "Creating SpecializationAbilityTier with tier={Tier}, displayName='{DisplayName}', " +
            "unlockCost={UnlockCost}, requiresPreviousTier={RequiresPreviousTier}, " +
            "requiredRank={RequiredRank}",
            tier,
            displayName,
            unlockCost,
            requiresPreviousTier,
            requiredRank);

        // Validate tier number range (1-4)
        ArgumentOutOfRangeException.ThrowIfLessThan(tier, 1, nameof(tier));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(tier, 4, nameof(tier));

        // Validate display name
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName, nameof(displayName));

        // Validate non-negative unlock cost
        ArgumentOutOfRangeException.ThrowIfNegative(unlockCost, nameof(unlockCost));

        // Validate required rank minimum
        ArgumentOutOfRangeException.ThrowIfLessThan(requiredRank, 1, nameof(requiredRank));

        // Validate abilities collection is not null
        ArgumentNullException.ThrowIfNull(abilities, nameof(abilities));

        // Materialize the abilities to a list for validation and storage
        var abilityList = abilities.ToList();

        // Validate at least one ability exists
        if (abilityList.Count == 0)
        {
            _logger?.LogWarning(
                "Validation failed for tier {Tier} '{DisplayName}': tier must contain at least one ability",
                tier,
                displayName);

            throw new ArgumentException(
                "Tier must contain at least one ability",
                nameof(abilities));
        }

        _logger?.LogDebug(
            "Tier {Tier} '{DisplayName}' contains {AbilityCount} abilities. " +
            "Validating tier-level constraints",
            tier,
            displayName,
            abilityList.Count);

        // Tier 1 cannot require a previous tier (there is no Tier 0)
        if (tier == 1 && requiresPreviousTier)
        {
            _logger?.LogWarning(
                "Validation failed for tier {Tier} '{DisplayName}': " +
                "Tier 1 cannot require a previous tier",
                tier,
                displayName);

            throw new ArgumentException(
                "Tier 1 cannot require a previous tier",
                nameof(requiresPreviousTier));
        }

        // Tier 2 and 3 must require the previous tier (sequential unlock)
        if (tier > 1 && !requiresPreviousTier)
        {
            _logger?.LogWarning(
                "Validation failed for tier {Tier} '{DisplayName}': " +
                "Tier {Tier} must require the previous tier",
                tier,
                displayName,
                tier);

            throw new ArgumentException(
                $"Tier {tier} must require the previous tier",
                nameof(requiresPreviousTier));
        }

        // Validate unique ability IDs within the tier
        var duplicateIds = abilityList
            .GroupBy(a => a.AbilityId)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicateIds.Count > 0)
        {
            _logger?.LogWarning(
                "Validation failed for tier {Tier} '{DisplayName}': " +
                "duplicate ability IDs found: {DuplicateIds}",
                tier,
                displayName,
                string.Join(", ", duplicateIds));

            throw new ArgumentException(
                $"Duplicate ability IDs in tier: {string.Join(", ", duplicateIds)}",
                nameof(abilities));
        }

        _logger?.LogDebug(
            "All validations passed for tier {Tier} '{DisplayName}'. " +
            "Tier range valid, display name non-empty, unlock cost non-negative, " +
            "previous tier constraint satisfied, ability IDs unique",
            tier,
            displayName);

        var tierInstance = new SpecializationAbilityTier
        {
            Tier = tier,
            DisplayName = displayName.Trim(),
            UnlockCost = unlockCost,
            RequiresPreviousTier = requiresPreviousTier,
            RequiredRank = requiredRank,
            Abilities = abilityList.AsReadOnly()
        };

        _logger?.LogInformation(
            "Created SpecializationAbilityTier {Tier}: '{DisplayName}'. " +
            "UnlockCost: {UnlockCost} PP, RequiredRank: {RequiredRank}, " +
            "RequiresPreviousTier: {RequiresPreviousTier}, " +
            "AbilityCount: {AbilityCount} (Active: {ActiveCount}, Passive: {PassiveCount})",
            tierInstance.Tier,
            tierInstance.DisplayName,
            tierInstance.UnlockCost,
            tierInstance.RequiredRank,
            tierInstance.RequiresPreviousTier,
            tierInstance.AbilityCount,
            tierInstance.ActiveCount,
            tierInstance.PassiveCount);

        // Log each ability in the tier for traceability
        foreach (var ability in tierInstance.Abilities)
        {
            _logger?.LogDebug(
                "  Tier {Tier} ability: '{AbilityId}' ({DisplayName}) — {Type}",
                tierInstance.Tier,
                ability.AbilityId,
                ability.DisplayName,
                ability.IsPassive ? "Passive" : "Active");
        }

        return tierInstance;
    }

    /// <summary>
    /// Creates a standard Tier 1 (free on specialization selection, Rank 1 required).
    /// </summary>
    /// <param name="displayName">UI display name for the tier (e.g., "Core Techniques").</param>
    /// <param name="abilities">Abilities in this tier.</param>
    /// <param name="logger">Optional logger for diagnostic output.</param>
    /// <returns>A validated Tier 1 <see cref="SpecializationAbilityTier"/>.</returns>
    /// <remarks>
    /// <para>
    /// Convenience factory that sets standard Tier 1 parameters:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Tier: 1</description></item>
    ///   <item><description>UnlockCost: 0 PP (free)</description></item>
    ///   <item><description>RequiresPreviousTier: false</description></item>
    ///   <item><description>RequiredRank: 1</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var tier1 = SpecializationAbilityTier.CreateTier1(
    ///     "Core Techniques",
    ///     new[]
    ///     {
    ///         SpecializationAbility.CreateActive("rage-strike", "Rage Strike", "A devastating blow.", 20, "rage"),
    ///         SpecializationAbility.CreatePassive("blood-frenzy", "Blood Frenzy", "+10% damage below 50% HP.")
    ///     });
    /// </code>
    /// </example>
    public static SpecializationAbilityTier CreateTier1(
        string displayName,
        IEnumerable<SpecializationAbility> abilities,
        ILogger<SpecializationAbilityTier>? logger = null)
    {
        return Create(
            tier: 1,
            displayName: displayName,
            unlockCost: 0,
            requiresPreviousTier: false,
            requiredRank: 1,
            abilities: abilities,
            logger: logger);
    }

    /// <summary>
    /// Creates a standard Tier 2 (2 PP cost, requires Tier 1, Rank 2 required).
    /// </summary>
    /// <param name="displayName">UI display name for the tier (e.g., "Advanced Mastery").</param>
    /// <param name="abilities">Abilities in this tier.</param>
    /// <param name="logger">Optional logger for diagnostic output.</param>
    /// <returns>A validated Tier 2 <see cref="SpecializationAbilityTier"/>.</returns>
    /// <remarks>
    /// <para>
    /// Convenience factory that sets standard Tier 2 parameters:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Tier: 2</description></item>
    ///   <item><description>UnlockCost: 2 PP</description></item>
    ///   <item><description>RequiresPreviousTier: true</description></item>
    ///   <item><description>RequiredRank: 2</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var tier2 = SpecializationAbilityTier.CreateTier2(
    ///     "Advanced Mastery",
    ///     new[]
    ///     {
    ///         SpecializationAbility.CreateActive("berserker-charge", "Berserker Charge", "Rush forward.", 30, "rage", 4, 10),
    ///         SpecializationAbility.CreatePassive("pain-is-power", "Pain is Power", "Damage converts to Rage.")
    ///     });
    /// </code>
    /// </example>
    public static SpecializationAbilityTier CreateTier2(
        string displayName,
        IEnumerable<SpecializationAbility> abilities,
        ILogger<SpecializationAbilityTier>? logger = null)
    {
        return Create(
            tier: 2,
            displayName: displayName,
            unlockCost: 2,
            requiresPreviousTier: true,
            requiredRank: 2,
            abilities: abilities,
            logger: logger);
    }

    /// <summary>
    /// Creates a standard Tier 3 (3 PP cost, requires Tier 2, Rank 3 required).
    /// </summary>
    /// <param name="displayName">UI display name for the tier (e.g., "Ultimate Power").</param>
    /// <param name="abilities">Abilities in this tier.</param>
    /// <param name="logger">Optional logger for diagnostic output.</param>
    /// <returns>A validated Tier 3 <see cref="SpecializationAbilityTier"/>.</returns>
    /// <remarks>
    /// <para>
    /// Convenience factory that sets standard Tier 3 parameters:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Tier: 3</description></item>
    ///   <item><description>UnlockCost: 3 PP</description></item>
    ///   <item><description>RequiresPreviousTier: true</description></item>
    ///   <item><description>RequiredRank: 3</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var tier3 = SpecializationAbilityTier.CreateTier3(
    ///     "Ultimate Power",
    ///     new[]
    ///     {
    ///         SpecializationAbility.CreateActive("blood-rampage", "Blood Rampage", "Unstoppable fury.", 50, "rage", 8, 15),
    ///         SpecializationAbility.CreatePassive("undying-rage", "Undying Rage", "Cannot die while Rage > 50.")
    ///     });
    /// </code>
    /// </example>
    public static SpecializationAbilityTier CreateTier3(
        string displayName,
        IEnumerable<SpecializationAbility> abilities,
        ILogger<SpecializationAbilityTier>? logger = null)
    {
        return Create(
            tier: 3,
            displayName: displayName,
            unlockCost: 3,
            requiresPreviousTier: true,
            requiredRank: 3,
            abilities: abilities,
            logger: logger);
    }

    /// <summary>
    /// Creates a standard Tier 4 / Capstone (6 PP cost, requires Tier 3, Rank 4 required).
    /// </summary>
    /// <param name="displayName">UI display name for the tier (e.g., "The Wall").</param>
    /// <param name="abilities">Abilities in this tier.</param>
    /// <param name="logger">Optional logger for diagnostic output.</param>
    /// <returns>A validated Tier 4 <see cref="SpecializationAbilityTier"/>.</returns>
    /// <remarks>
    /// <para>
    /// Convenience factory that sets standard Tier 4 (Capstone) parameters:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Tier: 4</description></item>
    ///   <item><description>UnlockCost: 6 PP</description></item>
    ///   <item><description>RequiresPreviousTier: true</description></item>
    ///   <item><description>RequiredRank: 4</description></item>
    /// </list>
    /// </remarks>
    public static SpecializationAbilityTier CreateTier4(
        string displayName,
        IEnumerable<SpecializationAbility> abilities,
        ILogger<SpecializationAbilityTier>? logger = null)
    {
        return Create(
            tier: 4,
            displayName: displayName,
            unlockCost: 6,
            requiresPreviousTier: true,
            requiredRank: 4,
            abilities: abilities,
            logger: logger);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // HELPER METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Checks if this tier can be unlocked given the current player state.
    /// </summary>
    /// <param name="currentRank">Player's current progression rank.</param>
    /// <param name="previousTierUnlocked">Whether the previous tier has been unlocked.</param>
    /// <param name="availablePp">Available Progression Points to spend.</param>
    /// <returns>
    /// A tuple where <c>CanUnlock</c> is <c>true</c> if all requirements are met,
    /// or <c>false</c> with a <c>Reason</c> string explaining which requirement failed.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Checks are performed in priority order:
    /// </para>
    /// <list type="number">
    ///   <item><description>Previous tier prerequisite (Tier 2+ only)</description></item>
    ///   <item><description>Rank requirement</description></item>
    ///   <item><description>PP availability</description></item>
    /// </list>
    /// <para>
    /// The first failing check determines the reason returned. If all checks
    /// pass, returns <c>(true, null)</c>.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var tier2 = SpecializationAbilityTier.CreateTier2("Advanced Mastery", abilities);
    ///
    /// // All requirements met
    /// var (canUnlock, reason) = tier2.CanUnlock(currentRank: 2, previousTierUnlocked: true, availablePp: 5);
    /// // canUnlock = true, reason = null
    ///
    /// // Previous tier not unlocked
    /// var (canUnlock2, reason2) = tier2.CanUnlock(currentRank: 2, previousTierUnlocked: false, availablePp: 5);
    /// // canUnlock2 = false, reason2 = "Tier 1 must be unlocked first"
    ///
    /// // Insufficient rank
    /// var (canUnlock3, reason3) = tier2.CanUnlock(currentRank: 1, previousTierUnlocked: true, availablePp: 5);
    /// // canUnlock3 = false, reason3 = "Requires Rank 2"
    ///
    /// // Insufficient PP
    /// var (canUnlock4, reason4) = tier2.CanUnlock(currentRank: 2, previousTierUnlocked: true, availablePp: 1);
    /// // canUnlock4 = false, reason4 = "Requires 2 PP (1 available)"
    /// </code>
    /// </example>
    public (bool CanUnlock, string? Reason) CanUnlock(
        int currentRank,
        bool previousTierUnlocked,
        int availablePp)
    {
        _logger?.LogDebug(
            "Checking unlock eligibility for Tier {Tier} '{DisplayName}'. " +
            "CurrentRank: {CurrentRank}, PreviousTierUnlocked: {PreviousTierUnlocked}, " +
            "AvailablePP: {AvailablePP}",
            Tier,
            DisplayName,
            currentRank,
            previousTierUnlocked,
            availablePp);

        // Check 1: Previous tier prerequisite
        if (RequiresPreviousTier && !previousTierUnlocked)
        {
            var reason = $"Tier {Tier - 1} must be unlocked first";

            _logger?.LogDebug(
                "Tier {Tier} unlock check failed: {Reason}",
                Tier,
                reason);

            return (false, reason);
        }

        // Check 2: Rank requirement
        if (currentRank < RequiredRank)
        {
            var reason = $"Requires Rank {RequiredRank}";

            _logger?.LogDebug(
                "Tier {Tier} unlock check failed: {Reason} (current: {CurrentRank})",
                Tier,
                reason,
                currentRank);

            return (false, reason);
        }

        // Check 3: PP availability
        if (availablePp < UnlockCost)
        {
            var reason = $"Requires {UnlockCost} PP ({availablePp} available)";

            _logger?.LogDebug(
                "Tier {Tier} unlock check failed: {Reason}",
                Tier,
                reason);

            return (false, reason);
        }

        _logger?.LogDebug(
            "Tier {Tier} '{DisplayName}' unlock check passed. " +
            "All requirements met (Rank {CurrentRank} >= {RequiredRank}, " +
            "PP {AvailablePP} >= {UnlockCost}, previous tier: {PreviousTierUnlocked})",
            Tier,
            DisplayName,
            currentRank,
            RequiredRank,
            availablePp,
            UnlockCost,
            previousTierUnlocked);

        return (true, null);
    }

    /// <summary>
    /// Gets a short summary string for UI display.
    /// </summary>
    /// <returns>
    /// A formatted string like "Tier 1: Core Techniques (3 abilities, 0 PP)".
    /// </returns>
    /// <example>
    /// <code>
    /// var tier = SpecializationAbilityTier.CreateTier1("Core Techniques", abilities);
    /// tier.GetShortDisplay(); // "Tier 1: Core Techniques — 3 abilities (0 PP)"
    /// </code>
    /// </example>
    public string GetShortDisplay() =>
        $"Tier {Tier}: {DisplayName} — {AbilityCount} abilities ({UnlockCost} PP)";

    /// <summary>
    /// Gets a detailed multi-line display string with tier and ability information.
    /// </summary>
    /// <returns>
    /// A formatted multi-line string with tier properties and all ability summaries.
    /// </returns>
    /// <example>
    /// <code>
    /// var tier = SpecializationAbilityTier.CreateTier1("Core Techniques", abilities);
    /// tier.GetDetailDisplay();
    /// // "Tier 1: Core Techniques
    /// //  Unlock Cost: 0 PP | Required Rank: 1 | Requires Previous Tier: No
    /// //  Abilities (3 total — 2 Active, 1 Passive):
    /// //    - Rage Strike [Active, 20 rage]
    /// //    - War Cry [Active, 15 rage]
    /// //    - Blood Frenzy [Passive]"
    /// </code>
    /// </example>
    public string GetDetailDisplay()
    {
        var lines = new List<string>
        {
            $"Tier {Tier}: {DisplayName}",
            $"  Unlock Cost: {UnlockCost} PP | Required Rank: {RequiredRank} | " +
            $"Requires Previous Tier: {(RequiresPreviousTier ? "Yes" : "No")}",
            $"  Abilities ({AbilityCount} total — {ActiveCount} Active, {PassiveCount} Passive):"
        };

        if (Abilities is not null)
        {
            foreach (var ability in Abilities)
            {
                lines.Add($"    - {ability}");
            }
        }

        return string.Join("\n", lines);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DEBUGGING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a compact string representation for debugging.
    /// </summary>
    /// <returns>
    /// A formatted string: "Tier {N}: {DisplayName} ({Count} abilities, {Cost} PP)".
    /// </returns>
    /// <example>
    /// <code>
    /// // "Tier 1: Core Techniques (3 abilities, 0 PP)"
    /// // "Tier 2: Advanced Mastery (3 abilities, 2 PP)"
    /// // "Tier 3: Ultimate Power (3 abilities, 3 PP)"
    /// </code>
    /// </example>
    public override string ToString()
    {
        return $"Tier {Tier}: {DisplayName} ({AbilityCount} abilities, {UnlockCost} PP)";
    }
}
