// ------------------------------------------------------------------------------
// <copyright file="AlternativeMethod.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Represents an alternative approach to bypassing an obstacle when the
// primary skill check isn't available or has failed. Every obstacle in
// Aethelgard has multiple solutions - these alternatives ensure characters
// aren't completely blocked.
// Part of v0.15.4h Alternative Bypass Methods implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.ValueObjects;

// =============================================================================
// OBSTACLE TYPE ENUM
// =============================================================================

/// <summary>
/// Types of obstacles that have alternative bypass methods.
/// </summary>
/// <remarks>
/// <para>
/// Each obstacle type has its own set of alternative approaches:
/// <list type="bullet">
///   <item><description>Locked Door: Find key, brute force, runic bypass, alternate route</description></item>
///   <item><description>Secured Terminal: Find codes, hotwire, partial access</description></item>
///   <item><description>Active Trap: Trigger from distance, destroy mechanism, sacrifice</description></item>
///   <item><description>Physical Barrier: Requires future implementation</description></item>
///   <item><description>Energy Barrier: Requires future implementation</description></item>
/// </list>
/// </para>
/// </remarks>
public enum BypassObstacleType
{
    /// <summary>
    /// A locked door or gate requiring lockpicking.
    /// </summary>
    /// <remarks>
    /// Locked doors can be bypassed through finding the key, brute force,
    /// runic manipulation, or finding an alternate route around.
    /// </remarks>
    LockedDoor,

    /// <summary>
    /// A secured terminal requiring hacking.
    /// </summary>
    /// <remarks>
    /// Secured terminals can be bypassed through finding access codes,
    /// hotwiring to a less secure terminal, or accepting partial access.
    /// </remarks>
    SecuredTerminal,

    /// <summary>
    /// An active trap that must be dealt with.
    /// </summary>
    /// <remarks>
    /// Active traps can be bypassed through triggering from a safe distance,
    /// destroying the mechanism, or sacrificing a shield or item to absorb
    /// the effect.
    /// </remarks>
    ActiveTrap,

    /// <summary>
    /// A physical barrier such as a wall or rubble.
    /// </summary>
    /// <remarks>
    /// Physical barriers represent impassable terrain that may be cleared,
    /// climbed over, or bypassed through alternative paths. Detailed
    /// implementation reserved for future versions.
    /// </remarks>
    PhysicalBarrier,

    /// <summary>
    /// An energy field or force barrier.
    /// </summary>
    /// <remarks>
    /// Energy barriers represent magical or technological shields that
    /// require specific counters to bypass. Detailed implementation
    /// reserved for future versions.
    /// </remarks>
    EnergyBarrier
}

// =============================================================================
// ALTERNATIVE CHECK TYPE ENUM
// =============================================================================

/// <summary>
/// Types of checks required for alternative bypass methods.
/// </summary>
/// <remarks>
/// <para>
/// Alternative methods use various attributes and skills to provide diverse
/// options for different character builds. A strength-focused character
/// can use Might-based brute force, while a perceptive character might
/// find an alternate route.
/// </para>
/// </remarks>
public enum AlternativeCheckType
{
    /// <summary>
    /// No check required; automatic if prerequisites are met.
    /// </summary>
    /// <remarks>
    /// Used for methods like "Accept Partial Access" or "Sacrifice" where
    /// having the right resources is sufficient.
    /// </remarks>
    None,

    /// <summary>
    /// MIGHT attribute check.
    /// </summary>
    /// <remarks>
    /// Used for brute force attempts against physical obstacles.
    /// The DC varies based on the obstacle's construction.
    /// </remarks>
    Might,

    /// <summary>
    /// FINESSE attribute check.
    /// </summary>
    /// <remarks>
    /// Used for precision tasks like triggering traps from a distance
    /// with a thrown object or ranged weapon.
    /// </remarks>
    Finesse,

    /// <summary>
    /// WITS attribute check.
    /// </summary>
    /// <remarks>
    /// Used for clever technical solutions like hotwiring terminals
    /// or improvising bypass methods.
    /// </remarks>
    Wits,

    /// <summary>
    /// Investigation skill check.
    /// </summary>
    /// <remarks>
    /// Used for finding hidden keys, access codes, or other information
    /// through methodical searching.
    /// </remarks>
    Investigation,

    /// <summary>
    /// Perception skill check.
    /// </summary>
    /// <remarks>
    /// Used for spotting alternate routes like ventilation shafts,
    /// windows, or structural weaknesses.
    /// </remarks>
    Perception,

    /// <summary>
    /// Runecraft skill check.
    /// </summary>
    /// <remarks>
    /// Used for manipulating runic mechanisms in locks or barriers.
    /// Requires the [Rune Sight] ability or equivalent.
    /// </remarks>
    Runecraft,

    /// <summary>
    /// Standard attack roll.
    /// </summary>
    /// <remarks>
    /// Used for destroying mechanisms or barriers through direct attack.
    /// The target's AC determines the difficulty.
    /// </remarks>
    Attack,

    /// <summary>
    /// Social or Charisma-based check.
    /// </summary>
    /// <remarks>
    /// Used for convincing NPCs to provide keys, codes, or assistance
    /// in bypassing obstacles.
    /// </remarks>
    Social
}

// =============================================================================
// ALTERNATIVE METHOD VALUE OBJECT
// =============================================================================

/// <summary>
/// Represents an alternative approach to bypassing an obstacle when the
/// primary skill check isn't available or has failed.
/// </summary>
/// <remarks>
/// <para>
/// Every obstacle in Aethelgard has multiple solutions. These alternatives
/// ensure characters aren't completely blocked by any single obstacle,
/// though each approach comes with its own trade-offs:
/// <list type="bullet">
///   <item><description>Time cost: Some methods take longer than others</description></item>
///   <item><description>Resource cost: May require items, essence, or other resources</description></item>
///   <item><description>Consequences: Noise, damage, or other negative effects</description></item>
///   <item><description>Prerequisites: Some methods require specific abilities or items</description></item>
/// </list>
/// </para>
/// <para>
/// <b>Design Principle:</b> No character should be permanently stuck because
/// they lack a specific skill. Alternative methods provide different paths
/// forward, each suited to different character builds.
/// </para>
/// </remarks>
/// <param name="MethodId">Unique identifier for the method.</param>
/// <param name="ObstacleType">Type of obstacle this method applies to.</param>
/// <param name="MethodName">Display name of the alternative.</param>
/// <param name="Description">Narrative description of the approach.</param>
/// <param name="RequiredCheck">Skill or attribute check needed.</param>
/// <param name="CheckDc">Difficulty of the required check (0 if no check needed).</param>
/// <param name="Consequences">Trade-offs for using this method.</param>
/// <param name="TimeRequired">Time cost description.</param>
/// <param name="Prerequisites">Any requirements to attempt this method.</param>
public readonly record struct AlternativeMethod(
    string MethodId,
    BypassObstacleType ObstacleType,
    string MethodName,
    string Description,
    AlternativeCheckType RequiredCheck,
    int CheckDc,
    IReadOnlyList<string> Consequences,
    string TimeRequired,
    IReadOnlyList<string> Prerequisites)
{
    // =========================================================================
    // LOCKED DOOR ALTERNATIVES
    // =========================================================================

    /// <summary>
    /// Find the key by searching or social interaction.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Find Key Statistics:
    /// <list type="bullet">
    ///   <item><description>Check: Investigation DC 14</description></item>
    ///   <item><description>Time: 10-30 minutes</description></item>
    ///   <item><description>Prerequisites: None</description></item>
    ///   <item><description>Consequences: Time spent, random encounters, social costs</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// This method involves methodically searching nearby areas for the key,
    /// or negotiating with someone who might have it. Social approaches may
    /// require payment, favors, or persuasion checks.
    /// </para>
    /// </remarks>
    public static AlternativeMethod FindKey => new(
        MethodId: "alt_door_find_key",
        ObstacleType: BypassObstacleType.LockedDoor,
        MethodName: "Find Key",
        Description: "Search nearby areas or negotiate with someone who has the key.",
        RequiredCheck: AlternativeCheckType.Investigation,
        CheckDc: 14,
        Consequences: new List<string>
        {
            "Time spent searching",
            "May trigger random encounters",
            "Social interaction may require favors or payment"
        }.AsReadOnly(),
        TimeRequired: "10-30 minutes",
        Prerequisites: new List<string>().AsReadOnly());

    /// <summary>
    /// Apply brute force to destroy the door.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Brute Force Statistics:
    /// <list type="bullet">
    ///   <item><description>Check: Might DC 12 (varies by door type)</description></item>
    ///   <item><description>Time: 1-3 rounds</description></item>
    ///   <item><description>Prerequisites: None</description></item>
    ///   <item><description>Consequences: Loud noise, content damage, exhaustion</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// The DC shown here is for simple doors. See <see cref="BruteForceOption"/>
    /// for detailed DCs based on door construction (Simple 12, Reinforced 16, Vault 22).
    /// </para>
    /// </remarks>
    public static AlternativeMethod BruteForce => new(
        MethodId: "alt_door_brute_force",
        ObstacleType: BypassObstacleType.LockedDoor,
        MethodName: "Brute Force",
        Description: "Use raw strength to break down the door.",
        RequiredCheck: AlternativeCheckType.Might,
        CheckDc: 12, // Varies by door type - see BruteForceOption
        Consequences: new List<string>
        {
            "Loud noise alerts nearby",
            "Contents behind door may be damaged",
            "Multiple attempts cause exhaustion"
        }.AsReadOnly(),
        TimeRequired: "1-3 rounds",
        Prerequisites: new List<string>().AsReadOnly());

    /// <summary>
    /// Use Runecraft to manipulate lock runes.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Runic Bypass Statistics:
    /// <list type="bullet">
    ///   <item><description>Check: Runecraft DC 16</description></item>
    ///   <item><description>Time: 1 round</description></item>
    ///   <item><description>Prerequisites: [Rune Sight] ability, Runecraft trained</description></item>
    ///   <item><description>Consequences: Essence cost, corruption exposure, alarms</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// This advanced technique requires the ability to perceive runic patterns.
    /// The character manipulates the lock's binding runes directly, releasing
    /// the mechanism without physical force. Costs 1-3 essence points.
    /// </para>
    /// </remarks>
    public static AlternativeMethod RunicBypass => new(
        MethodId: "alt_door_runic_bypass",
        ObstacleType: BypassObstacleType.LockedDoor,
        MethodName: "Runic Bypass",
        Description: "Manipulate the lock's runic bindings to release the mechanism.",
        RequiredCheck: AlternativeCheckType.Runecraft,
        CheckDc: 16,
        Consequences: new List<string>
        {
            "Essence cost (1-3 points)",
            "Potential corruption exposure",
            "May trigger runic alarms"
        }.AsReadOnly(),
        TimeRequired: "1 round",
        Prerequisites: new List<string>
        {
            "[Rune Sight] ability or equivalent",
            "Runecraft skill trained"
        }.AsReadOnly());

    /// <summary>
    /// Find another way around the door.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Alternate Route Statistics:
    /// <list type="bullet">
    ///   <item><description>Check: Perception DC 14</description></item>
    ///   <item><description>Time: 5-20 minutes</description></item>
    ///   <item><description>Prerequisites: None</description></item>
    ///   <item><description>Consequences: Time spent, path hazards, accessibility</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Rather than bypassing the door itself, this method involves finding
    /// another path: a ventilation shaft, window, collapsed wall, or other
    /// structural weakness. Not all party members may fit through.
    /// </para>
    /// </remarks>
    public static AlternativeMethod AlternateRoute => new(
        MethodId: "alt_door_alternate_route",
        ObstacleType: BypassObstacleType.LockedDoor,
        MethodName: "Alternate Route",
        Description: "Find another path: ventilation shaft, window, or collapsed wall.",
        RequiredCheck: AlternativeCheckType.Perception,
        CheckDc: 14,
        Consequences: new List<string>
        {
            "Time spent exploring",
            "Alternate paths may have their own hazards",
            "May not be accessible for all party members"
        }.AsReadOnly(),
        TimeRequired: "5-20 minutes",
        Prerequisites: new List<string>().AsReadOnly());

    // =========================================================================
    // SECURED TERMINAL ALTERNATIVES
    // =========================================================================

    /// <summary>
    /// Find access codes through investigation.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Find Codes Statistics:
    /// <list type="bullet">
    ///   <item><description>Check: Investigation DC 16</description></item>
    ///   <item><description>Time: 5-15 minutes</description></item>
    ///   <item><description>Prerequisites: None</description></item>
    ///   <item><description>Consequences: Time spent, expired codes, partial codes</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Many operators write passwords on sticky notes or in notebooks.
    /// A thorough search of the area may reveal credentials, though they
    /// might be expired, single-use, or only partially complete.
    /// </para>
    /// </remarks>
    public static AlternativeMethod FindCodes => new(
        MethodId: "alt_terminal_find_codes",
        ObstacleType: BypassObstacleType.SecuredTerminal,
        MethodName: "Find Codes",
        Description: "Search for written passwords or credentials in the area.",
        RequiredCheck: AlternativeCheckType.Investigation,
        CheckDc: 16,
        Consequences: new List<string>
        {
            "Time spent searching",
            "Codes may be expired or single-use",
            "May find partial codes only"
        }.AsReadOnly(),
        TimeRequired: "5-15 minutes",
        Prerequisites: new List<string>().AsReadOnly());

    /// <summary>
    /// Hotwire connection to a less secured terminal.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Hotwire Terminal Statistics:
    /// <list type="bullet">
    ///   <item><description>Check: Wits DC 14</description></item>
    ///   <item><description>Time: 2-5 rounds</description></item>
    ///   <item><description>Prerequisites: Secondary terminal, wire/cable</description></item>
    ///   <item><description>Consequences: Reduced access, unstable connection, ICE</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Rather than breaking into the secured terminal directly, this method
    /// routes the connection through a less secure access point. The trade-off
    /// is reduced access level (User instead of Admin) and potential instability.
    /// </para>
    /// </remarks>
    public static AlternativeMethod HotwireTerminal => new(
        MethodId: "alt_terminal_hotwire",
        ObstacleType: BypassObstacleType.SecuredTerminal,
        MethodName: "Hotwire to Different Terminal",
        Description: "Bypass this terminal by connecting to a less secured access point.",
        RequiredCheck: AlternativeCheckType.Wits,
        CheckDc: 14,
        Consequences: new List<string>
        {
            "Reduced access level (User instead of Admin)",
            "Connection may be unstable",
            "May trigger ICE on secondary terminal"
        }.AsReadOnly(),
        TimeRequired: "2-5 rounds",
        Prerequisites: new List<string>
        {
            "Secondary terminal must exist in area",
            "Wire or cable available"
        }.AsReadOnly());

    /// <summary>
    /// Accept partial access instead of full.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Accept Partial Access Statistics:
    /// <list type="bullet">
    ///   <item><description>Check: None required</description></item>
    ///   <item><description>Time: Immediate</description></item>
    ///   <item><description>Prerequisites: Must have achieved User Level access</description></item>
    ///   <item><description>Consequences: Limited functionality, no classified files</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Sometimes User Level access is sufficient for the immediate objective.
    /// This method accepts the current access level rather than pursuing
    /// the higher-risk Admin Level access, but limits available functionality.
    /// </para>
    /// </remarks>
    public static AlternativeMethod AcceptPartialAccess => new(
        MethodId: "alt_terminal_partial",
        ObstacleType: BypassObstacleType.SecuredTerminal,
        MethodName: "Accept Partial Access",
        Description: "Stop at User Level access rather than pursuing Admin Level.",
        RequiredCheck: AlternativeCheckType.None,
        CheckDc: 0, // No check required
        Consequences: new List<string>
        {
            "Cannot access hidden or classified files",
            "Limited functionality available",
            "May need to return later with better access"
        }.AsReadOnly(),
        TimeRequired: "Immediate",
        Prerequisites: new List<string>
        {
            "Must have achieved at least User Level access"
        }.AsReadOnly());

    // =========================================================================
    // ACTIVE TRAP ALTERNATIVES
    // =========================================================================

    /// <summary>
    /// Trigger the trap from a safe distance.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Trigger from Distance Statistics:
    /// <list type="bullet">
    ///   <item><description>Check: Finesse DC 12</description></item>
    ///   <item><description>Time: 1 round</description></item>
    ///   <item><description>Prerequisites: Throwable object, line of sight</description></item>
    ///   <item><description>Consequences: Noise, area damage, consumes object</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Rather than carefully disarming the trap, this method intentionally
    /// triggers it from a safe distance using a thrown object or ranged attack.
    /// The trap activates but the party remains unharmed (though any loot in
    /// the area may be destroyed).
    /// </para>
    /// </remarks>
    public static AlternativeMethod TriggerFromDistance => new(
        MethodId: "alt_trap_trigger_distance",
        ObstacleType: BypassObstacleType.ActiveTrap,
        MethodName: "Trigger from Distance",
        Description: "Throw an object or use ranged attack to safely trigger the trap.",
        RequiredCheck: AlternativeCheckType.Finesse,
        CheckDc: 12,
        Consequences: new List<string>
        {
            "Noise from trap activation",
            "Trap effects apply to area (may destroy loot)",
            "Consumes thrown object"
        }.AsReadOnly(),
        TimeRequired: "1 round",
        Prerequisites: new List<string>
        {
            "Object to throw or ranged weapon",
            "Line of sight to trigger mechanism"
        }.AsReadOnly());

    /// <summary>
    /// Destroy the trap mechanism with direct attack.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Destroy Mechanism Statistics:
    /// <list type="bullet">
    ///   <item><description>Check: Attack roll vs trap's AC</description></item>
    ///   <item><description>Time: 1 round</description></item>
    ///   <item><description>Prerequisites: Weapon or damaging ability</description></item>
    ///   <item><description>Consequences: Noise, trigger risk, no salvage</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// A direct approach - attack the trap mechanism to destroy it before
    /// it can trigger. If the attack doesn't destroy the trap in one hit,
    /// it may trigger instead. No components can be salvaged from a
    /// destroyed trap.
    /// </para>
    /// </remarks>
    public static AlternativeMethod DestroyMechanism => new(
        MethodId: "alt_trap_destroy",
        ObstacleType: BypassObstacleType.ActiveTrap,
        MethodName: "Destroy Mechanism",
        Description: "Attack the trap mechanism directly to disable it.",
        RequiredCheck: AlternativeCheckType.Attack,
        CheckDc: 0, // Uses attack roll vs trap AC
        Consequences: new List<string>
        {
            "Noise from destruction",
            "Trap may trigger if not destroyed in one hit",
            "No salvageable components"
        }.AsReadOnly(),
        TimeRequired: "1 round",
        Prerequisites: new List<string>
        {
            "Weapon or damaging ability"
        }.AsReadOnly());

    /// <summary>
    /// Use a shield or item to absorb trap effect.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Sacrifice Statistics:
    /// <list type="bullet">
    ///   <item><description>Check: None required</description></item>
    ///   <item><description>Time: 1 round</description></item>
    ///   <item><description>Prerequisites: Shield or expendable item</description></item>
    ///   <item><description>Consequences: Item damage/destruction, alerts</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// When you can't disarm a trap but must proceed, blocking the effect
    /// with a shield or absorbing it with an expendable item is an option.
    /// The trap still triggers (alerting nearby enemies), but the party
    /// takes no damage. The sacrificed item may be destroyed.
    /// </para>
    /// </remarks>
    public static AlternativeMethod Sacrifice => new(
        MethodId: "alt_trap_sacrifice",
        ObstacleType: BypassObstacleType.ActiveTrap,
        MethodName: "Sacrifice (Shield/Item)",
        Description: "Block trap effect with shield or absorb with expendable item.",
        RequiredCheck: AlternativeCheckType.None,
        CheckDc: 0, // Automatic if resources available
        Consequences: new List<string>
        {
            "Shield takes damage (may break)",
            "Consumable item destroyed",
            "Trap still triggers alert systems"
        }.AsReadOnly(),
        TimeRequired: "1 round",
        Prerequisites: new List<string>
        {
            "Shield or expendable item available",
            "Item must be appropriate for trap type"
        }.AsReadOnly());

    // =========================================================================
    // STATIC UTILITY METHODS
    // =========================================================================

    /// <summary>
    /// Gets all alternative bypass methods for a given obstacle type.
    /// </summary>
    /// <param name="obstacleType">The type of obstacle to get alternatives for.</param>
    /// <returns>An enumerable of all applicable alternative methods.</returns>
    /// <remarks>
    /// <para>
    /// Returns the following alternatives by obstacle type:
    /// <list type="bullet">
    ///   <item><description>LockedDoor: FindKey, BruteForce, RunicBypass, AlternateRoute</description></item>
    ///   <item><description>SecuredTerminal: FindCodes, HotwireTerminal, AcceptPartialAccess</description></item>
    ///   <item><description>ActiveTrap: TriggerFromDistance, DestroyMechanism, Sacrifice</description></item>
    ///   <item><description>PhysicalBarrier: Empty (future implementation)</description></item>
    ///   <item><description>EnergyBarrier: Empty (future implementation)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var doorAlternatives = AlternativeMethod.GetAlternativesFor(ObstacleType.LockedDoor);
    /// foreach (var method in doorAlternatives)
    /// {
    ///     Console.WriteLine($"{method.MethodName}: {method.Description}");
    /// }
    /// // Output:
    /// // Find Key: Search nearby areas or negotiate with someone who has the key.
    /// // Brute Force: Use raw strength to break down the door.
    /// // Runic Bypass: Manipulate the lock's runic bindings to release the mechanism.
    /// // Alternate Route: Find another path: ventilation shaft, window, or collapsed wall.
    /// </code>
    /// </example>
    public static IEnumerable<AlternativeMethod> GetAlternativesFor(BypassObstacleType obstacleType) =>
        obstacleType switch
        {
            BypassObstacleType.LockedDoor => new[] { FindKey, BruteForce, RunicBypass, AlternateRoute },
            BypassObstacleType.SecuredTerminal => new[] { FindCodes, HotwireTerminal, AcceptPartialAccess },
            BypassObstacleType.ActiveTrap => new[] { TriggerFromDistance, DestroyMechanism, Sacrifice },
            _ => Enumerable.Empty<AlternativeMethod>()
        };

    /// <summary>
    /// Gets an alternative method by its unique identifier.
    /// </summary>
    /// <param name="methodId">The method ID to search for.</param>
    /// <returns>The matching alternative method, or null if not found.</returns>
    /// <example>
    /// <code>
    /// var method = AlternativeMethod.GetById("alt_door_brute_force");
    /// if (method.HasValue)
    /// {
    ///     Console.WriteLine($"Found: {method.Value.MethodName}");
    /// }
    /// </code>
    /// </example>
    public static AlternativeMethod? GetById(string methodId)
    {
        // Search all obstacle types
        foreach (BypassObstacleType obstacleType in Enum.GetValues<BypassObstacleType>())
        {
            var match = GetAlternativesFor(obstacleType)
                .FirstOrDefault(m => m.MethodId.Equals(methodId, StringComparison.Ordinal));

            if (!string.IsNullOrEmpty(match.MethodId))
            {
                return match;
            }
        }

        return null;
    }

    // =========================================================================
    // INSTANCE METHODS
    // =========================================================================

    /// <summary>
    /// Determines whether this method requires a skill or attribute check.
    /// </summary>
    /// <returns>True if a check is required, false if automatic.</returns>
    public bool RequiresCheck() => RequiredCheck != AlternativeCheckType.None;

    /// <summary>
    /// Determines whether this method has any prerequisites.
    /// </summary>
    /// <returns>True if prerequisites exist, false otherwise.</returns>
    public bool HasPrerequisites() => Prerequisites.Count > 0;

    /// <summary>
    /// Determines whether this method has any consequences.
    /// </summary>
    /// <returns>True if consequences exist, false otherwise.</returns>
    public bool HasConsequences() => Consequences.Count > 0;

    /// <summary>
    /// Gets a short summary of the check required for this method.
    /// </summary>
    /// <returns>A formatted string showing the check type and DC.</returns>
    /// <example>
    /// <code>
    /// var method = AlternativeMethod.BruteForce;
    /// Console.WriteLine(method.GetCheckSummary());
    /// // Output: "Might DC 12"
    ///
    /// var sacrifice = AlternativeMethod.Sacrifice;
    /// Console.WriteLine(sacrifice.GetCheckSummary());
    /// // Output: "None required"
    /// </code>
    /// </example>
    public string GetCheckSummary()
    {
        if (RequiredCheck == AlternativeCheckType.None)
        {
            return "None required";
        }

        if (RequiredCheck == AlternativeCheckType.Attack)
        {
            return "Attack vs trap AC";
        }

        return $"{RequiredCheck} DC {CheckDc}";
    }

    /// <summary>
    /// Creates a display string for this alternative method.
    /// </summary>
    /// <returns>A formatted multi-line string showing all method details.</returns>
    /// <example>
    /// <code>
    /// var method = AlternativeMethod.RunicBypass;
    /// Console.WriteLine(method.ToDisplayString());
    /// // Output:
    /// // [Runic Bypass]
    /// // Manipulate the lock's runic bindings to release the mechanism.
    /// //
    /// // Check: Runecraft DC 16
    /// // Time: 1 round
    /// //
    /// // Prerequisites:
    /// //   - [Rune Sight] ability or equivalent
    /// //   - Runecraft skill trained
    /// //
    /// // Consequences:
    /// //   - Essence cost (1-3 points)
    /// //   - Potential corruption exposure
    /// //   - May trigger runic alarms
    /// </code>
    /// </example>
    public string ToDisplayString()
    {
        var lines = new List<string>
        {
            $"[{MethodName}]",
            Description,
            string.Empty
        };

        lines.Add($"Check: {GetCheckSummary()}");
        lines.Add($"Time: {TimeRequired}");

        if (Prerequisites.Count > 0)
        {
            lines.Add(string.Empty);
            lines.Add("Prerequisites:");
            foreach (var prereq in Prerequisites)
            {
                lines.Add($"  - {prereq}");
            }
        }

        if (Consequences.Count > 0)
        {
            lines.Add(string.Empty);
            lines.Add("Consequences:");
            foreach (var consequence in Consequences)
            {
                lines.Add($"  - {consequence}");
            }
        }

        return string.Join(Environment.NewLine, lines);
    }
}
