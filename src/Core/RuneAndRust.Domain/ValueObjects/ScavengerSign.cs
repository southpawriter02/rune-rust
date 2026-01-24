using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents a scavenger sign discovered in the environment.
/// </summary>
/// <remarks>
/// <para>
/// ScavengerSign captures all information about a wasteland marking:
/// <list type="bullet">
///   <item><description>SignType - The category of sign (determines base interpretation DC)</description></item>
///   <item><description>FactionId - Who left the sign (affects DC if unknown to player)</description></item>
///   <item><description>Meaning - The actual message once interpreted</description></item>
///   <item><description>Age - How old the sign is (adds DC modifier and affects reliability)</description></item>
///   <item><description>VisualDescription - How the sign appears before interpretation</description></item>
/// </list>
/// </para>
/// <para>
/// Signs are placed by factions to communicate with their own members. Players can attempt
/// to interpret them using the Wasteland Survival skill. The interpretation DC is calculated as:
/// <code>Final DC = Base DC (from SignType) + Age Modifier + Unknown Faction Modifier (+4 if unknown)</code>
/// </para>
/// <para>
/// This value object is immutable and represents the sign as it exists in the world,
/// independent of any interpretation attempt.
/// </para>
/// </remarks>
/// <param name="SignType">The category of scavenger sign.</param>
/// <param name="FactionId">The identifier of the faction that left this sign.</param>
/// <param name="Meaning">The actual meaning/message of the sign.</param>
/// <param name="Age">How old the sign is (affects DC and reliability).</param>
/// <param name="VisualDescription">How the sign appears before interpretation.</param>
public readonly record struct ScavengerSign(
    ScavengerSignType SignType,
    string FactionId,
    string Meaning,
    SignAge Age,
    string VisualDescription)
{
    // ═══════════════════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the base interpretation DC for this sign's type.
    /// </summary>
    /// <remarks>
    /// This does not include modifiers for faction familiarity or sign age.
    /// Use <see cref="CalculateInterpretationDc"/> for the full DC calculation.
    /// </remarks>
    public int BaseDc => SignType.GetBaseDc();

    /// <summary>
    /// Gets the DC modifier from this sign's age.
    /// </summary>
    /// <remarks>
    /// Age modifiers:
    /// <list type="bullet">
    ///   <item><description>Fresh/Recent: +0</description></item>
    ///   <item><description>Old: +1</description></item>
    ///   <item><description>Faded: +2</description></item>
    ///   <item><description>Ancient: +4</description></item>
    /// </list>
    /// </remarks>
    public int AgeModifier => Age.GetDcModifier();

    /// <summary>
    /// Gets whether this sign is from a major faction (whose signs are always recognizable).
    /// </summary>
    /// <remarks>
    /// Major factions have standardized sign systems that are known to all wasteland survivors:
    /// <list type="bullet">
    ///   <item><description>iron-covenant - Iron Covenant</description></item>
    ///   <item><description>rust-walkers - Rust Walkers</description></item>
    ///   <item><description>silent-ones - Silent Ones</description></item>
    ///   <item><description>verdant-circle - Verdant Circle</description></item>
    ///   <item><description>ash-born - Ash-Born</description></item>
    /// </list>
    /// </remarks>
    public bool IsMajorFaction => !string.IsNullOrEmpty(FactionId) && MajorFactionIds.Contains(FactionId);

    /// <summary>
    /// Gets whether the sign's information is likely still reliable based on age.
    /// </summary>
    /// <remarks>
    /// Fresh and Recent signs contain reliable information.
    /// Older signs may have outdated information (caches moved, dangers passed, etc.).
    /// </remarks>
    public bool IsReliable => Age.IsReliable();

    /// <summary>
    /// Gets whether this sign indicates recent faction activity in the area.
    /// </summary>
    public bool IndicatesRecentActivity => Age.IndicatesRecentActivity();

    // ═══════════════════════════════════════════════════════════════════════════
    // DC CALCULATION METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Calculates the full interpretation DC for this sign.
    /// </summary>
    /// <param name="factionKnown">Whether the faction is known to the interpreting player.</param>
    /// <returns>The total DC including base DC, age modifier, and faction modifier.</returns>
    /// <remarks>
    /// <para>
    /// The DC calculation is:
    /// <code>Final DC = Base DC + Age Modifier + (Unknown Faction ? +4 : 0)</code>
    /// </para>
    /// <para>
    /// Example calculations:
    /// <list type="bullet">
    ///   <item><description>Fresh TerritoryMarker from known faction: 10 + 0 + 0 = DC 10</description></item>
    ///   <item><description>Faded CacheIndicator from unknown faction: 14 + 2 + 4 = DC 20</description></item>
    ///   <item><description>Ancient TabooSign from major faction: 12 + 4 + 0 = DC 16</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public int CalculateInterpretationDc(bool factionKnown)
    {
        var unknownFactionModifier = factionKnown ? 0 : UnknownFactionDcModifier;
        return BaseDc + AgeModifier + unknownFactionModifier;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new scavenger sign with default visual description.
    /// </summary>
    /// <param name="signType">The type of sign.</param>
    /// <param name="factionId">The faction that left the sign.</param>
    /// <param name="meaning">The meaning of the sign.</param>
    /// <param name="age">The age of the sign.</param>
    /// <returns>A new ScavengerSign with auto-generated visual description.</returns>
    public static ScavengerSign Create(
        ScavengerSignType signType,
        string factionId,
        string meaning,
        SignAge age = SignAge.Recent)
    {
        var visualDescription = GetDefaultVisualDescription(signType);
        return new ScavengerSign(signType, factionId, meaning, age, visualDescription);
    }

    /// <summary>
    /// Creates a territory marker sign.
    /// </summary>
    /// <param name="factionId">The faction claiming the territory.</param>
    /// <param name="age">The age of the sign.</param>
    /// <param name="visualDescription">Optional custom visual description.</param>
    /// <returns>A new territory marker sign.</returns>
    public static ScavengerSign TerritoryMarker(
        string factionId,
        SignAge age = SignAge.Recent,
        string? visualDescription = null)
    {
        return new ScavengerSign(
            ScavengerSignType.TerritoryMarker,
            factionId,
            $"This area belongs to {factionId}. Trespassers may face hostility.",
            age,
            visualDescription ?? "a series of parallel scratches with a symbol at the center");
    }

    /// <summary>
    /// Creates a warning sign.
    /// </summary>
    /// <param name="factionId">The faction that left the warning.</param>
    /// <param name="dangerType">Description of the danger being warned about.</param>
    /// <param name="age">The age of the sign.</param>
    /// <param name="visualDescription">Optional custom visual description.</param>
    /// <returns>A new warning sign.</returns>
    public static ScavengerSign WarningSign(
        string factionId,
        string dangerType,
        SignAge age = SignAge.Recent,
        string? visualDescription = null)
    {
        return new ScavengerSign(
            ScavengerSignType.WarningSign,
            factionId,
            $"Danger lies ahead: {dangerType}. Proceed with caution.",
            age,
            visualDescription ?? "jagged lines radiating from a central point");
    }

    /// <summary>
    /// Creates a cache indicator sign.
    /// </summary>
    /// <param name="factionId">The faction that left the cache.</param>
    /// <param name="direction">Direction to the cache.</param>
    /// <param name="distance">Approximate distance to the cache.</param>
    /// <param name="age">The age of the sign.</param>
    /// <param name="visualDescription">Optional custom visual description.</param>
    /// <returns>A new cache indicator sign.</returns>
    public static ScavengerSign CacheIndicator(
        string factionId,
        string direction,
        string distance,
        SignAge age = SignAge.Recent,
        string? visualDescription = null)
    {
        return new ScavengerSign(
            ScavengerSignType.CacheIndicator,
            factionId,
            $"Hidden supplies are concealed {direction}, approximately {distance} away.",
            age,
            visualDescription ?? "a subtle arrow-like mark barely visible among debris");
    }

    /// <summary>
    /// Creates a trail blaze sign.
    /// </summary>
    /// <param name="factionId">The faction that marked the trail.</param>
    /// <param name="direction">Direction the safe path leads.</param>
    /// <param name="age">The age of the sign.</param>
    /// <param name="visualDescription">Optional custom visual description.</param>
    /// <returns>A new trail blaze sign.</returns>
    public static ScavengerSign TrailBlaze(
        string factionId,
        string direction,
        SignAge age = SignAge.Recent,
        string? visualDescription = null)
    {
        return new ScavengerSign(
            ScavengerSignType.TrailBlaze,
            factionId,
            $"This path leads {direction} and has been marked as safe.",
            age,
            visualDescription ?? "a simple arrow scratched at eye level");
    }

    /// <summary>
    /// Creates a hunt marker sign.
    /// </summary>
    /// <param name="factionId">The faction that marked the hunting ground.</param>
    /// <param name="preyType">Type of prey indicated.</param>
    /// <param name="direction">Direction where prey was spotted.</param>
    /// <param name="timeAgo">When the prey was spotted.</param>
    /// <param name="age">The age of the sign.</param>
    /// <param name="visualDescription">Optional custom visual description.</param>
    /// <returns>A new hunt marker sign.</returns>
    public static ScavengerSign HuntMarker(
        string factionId,
        string preyType,
        string direction,
        string timeAgo,
        SignAge age = SignAge.Recent,
        string? visualDescription = null)
    {
        return new ScavengerSign(
            ScavengerSignType.HuntMarker,
            factionId,
            $"{preyType} was spotted {direction}, {timeAgo} ago.",
            age,
            visualDescription ?? "stylized animal tracks leading in a direction");
    }

    /// <summary>
    /// Creates a taboo sign.
    /// </summary>
    /// <param name="factionId">The faction that marked the area as forbidden.</param>
    /// <param name="reason">Reason the area is forbidden.</param>
    /// <param name="age">The age of the sign.</param>
    /// <param name="visualDescription">Optional custom visual description.</param>
    /// <returns>A new taboo sign.</returns>
    public static ScavengerSign TabooSign(
        string factionId,
        string reason,
        SignAge age = SignAge.Recent,
        string? visualDescription = null)
    {
        return new ScavengerSign(
            ScavengerSignType.TabooSign,
            factionId,
            $"This area is forbidden. {reason}",
            age,
            visualDescription ?? "an X mark with additional warning symbols");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // STRING FORMATTING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a display string for when the sign is first discovered (before interpretation).
    /// </summary>
    /// <returns>A narrative description of discovering the sign.</returns>
    public string ToDiscoveryString()
    {
        return $"You notice {VisualDescription}. The markings appear to be {Age.ToDisplayString()}.";
    }

    /// <summary>
    /// Returns a detailed diagnostic string for logging and debugging.
    /// </summary>
    /// <returns>A multi-line string with complete sign details.</returns>
    public string ToDetailedString()
    {
        return $"ScavengerSign\n" +
               $"  Type: {SignType.GetDisplayName()}\n" +
               $"  Faction: {FactionId}\n" +
               $"  Age: {Age.GetDisplayName()} ({Age.ToDisplayString()})\n" +
               $"  Base DC: {BaseDc}\n" +
               $"  Age Modifier: +{AgeModifier}\n" +
               $"  Is Major Faction: {IsMajorFaction}\n" +
               $"  Is Reliable: {IsReliable}\n" +
               $"  Meaning: {Meaning}\n" +
               $"  Visual: {VisualDescription}";
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{SignType.GetDisplayName()} ({Age.GetDisplayName()}) from {FactionId}";
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTANTS AND HELPERS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// The DC modifier applied when the sign's faction is unknown to the player.
    /// </summary>
    public const int UnknownFactionDcModifier = 4;

    /// <summary>
    /// Set of major faction IDs that are known to all wasteland survivors.
    /// </summary>
    private static readonly HashSet<string> MajorFactionIds = new(StringComparer.OrdinalIgnoreCase)
    {
        "iron-covenant",
        "rust-walkers",
        "silent-ones",
        "verdant-circle",
        "ash-born"
    };

    /// <summary>
    /// Gets the list of major faction IDs.
    /// </summary>
    /// <returns>A read-only collection of major faction identifiers.</returns>
    public static IReadOnlyCollection<string> GetMajorFactionIds() => MajorFactionIds;

    /// <summary>
    /// Determines whether a faction ID represents a major faction.
    /// </summary>
    /// <param name="factionId">The faction ID to check.</param>
    /// <returns>True if the faction is a major faction.</returns>
    public static bool IsMajorFactionId(string factionId) =>
        !string.IsNullOrEmpty(factionId) && MajorFactionIds.Contains(factionId);

    /// <summary>
    /// Gets a default visual description for a sign type.
    /// </summary>
    /// <param name="signType">The type of sign.</param>
    /// <returns>A default visual description string.</returns>
    private static string GetDefaultVisualDescription(ScavengerSignType signType)
    {
        return signType switch
        {
            ScavengerSignType.TerritoryMarker => "a series of parallel scratches with a symbol at the center",
            ScavengerSignType.WarningSign => "jagged lines radiating from a central point",
            ScavengerSignType.CacheIndicator => "a subtle arrow-like mark barely visible among debris",
            ScavengerSignType.TrailBlaze => "a simple arrow scratched at eye level",
            ScavengerSignType.HuntMarker => "stylized animal tracks leading in a direction",
            ScavengerSignType.TabooSign => "an X mark with additional warning symbols",
            _ => "strange markings scratched into the surface"
        };
    }
}
