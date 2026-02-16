namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the current intensity level of a Berserkr's Rage resource.
/// Each level corresponds to a specific Rage value range and gameplay effects.
/// </summary>
/// <remarks>
/// <para>Rage levels define escalating combat states:</para>
/// <list type="bullet">
/// <item>Calm (0–19): No bonus effects, baseline state</item>
/// <item>Irritated (20–39): Minor combat awareness, +1 Attack</item>
/// <item>Angry (40–59): Growing fury, +2 Attack</item>
/// <item>Furious (60–79): Dangerous intensity, +3 Attack, minor Corruption risk</item>
/// <item>Enraged (80–99): Overwhelming fury, +4 Attack, significant Corruption risk</item>
/// <item>Berserk (100): Maximum Rage, +5 Attack, highest Corruption risk</item>
/// </list>
/// <para>Higher Rage levels unlock stronger bonuses but increase the risk of
/// Corruption accumulation through the Heretical Path mechanics.</para>
/// </remarks>
public enum RageLevel
{
    /// <summary>Rage 0–19: Baseline state with no Rage-based bonuses.</summary>
    Calm = 0,

    /// <summary>Rage 20–39: Minor heightened awareness, +1 Attack bonus.</summary>
    Irritated = 1,

    /// <summary>Rage 40–59: Growing fury grants +2 Attack bonus.</summary>
    Angry = 2,

    /// <summary>Rage 60–79: Dangerous intensity grants +3 Attack bonus with minor Corruption risk.</summary>
    Furious = 3,

    /// <summary>Rage 80–99: Overwhelming fury grants +4 Attack bonus with significant Corruption risk.</summary>
    Enraged = 4,

    /// <summary>Rage 100: Maximum Rage grants +5 Attack bonus with highest Corruption risk.</summary>
    Berserk = 5
}
