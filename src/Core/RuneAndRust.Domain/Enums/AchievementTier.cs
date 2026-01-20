namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Tiers of achievements with associated point values.
/// </summary>
/// <remarks>
/// <para>
/// Each tier represents a level of difficulty or rarity:
/// <list type="bullet">
///   <item><description>Bronze (10 pts) — Entry-level, first-time accomplishments</description></item>
///   <item><description>Silver (25 pts) — Moderate challenge, requires some effort</description></item>
///   <item><description>Gold (50 pts) — Significant accomplishment, substantial effort</description></item>
///   <item><description>Platinum (100 pts) — Major achievement, rare or difficult feat</description></item>
/// </list>
/// </para>
/// <para>The enum values represent the point values directly.</para>
/// </remarks>
public enum AchievementTier
{
    /// <summary>Entry-level achievement worth 10 points. Typically first-time accomplishments.</summary>
    Bronze = 10,

    /// <summary>Moderate challenge achievement worth 25 points. Requires some dedication to unlock.</summary>
    Silver = 25,

    /// <summary>Significant achievement worth 50 points. Requires substantial effort to unlock.</summary>
    Gold = 50,

    /// <summary>Major achievement worth 100 points. Rare or very difficult to unlock.</summary>
    Platinum = 100
}
