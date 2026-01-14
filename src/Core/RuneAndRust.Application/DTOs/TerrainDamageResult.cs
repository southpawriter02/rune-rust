namespace RuneAndRust.Application.DTOs;

/// <summary>
/// Result of terrain hazard damage calculation or application.
/// </summary>
/// <param name="DamageDealt">Whether damage was actually dealt.</param>
/// <param name="Damage">The amount of damage (0 if no damage).</param>
/// <param name="DamageType">The type of damage (e.g., "fire", "acid").</param>
/// <param name="TerrainName">The name of the hazardous terrain.</param>
/// <param name="Message">A descriptive message for display.</param>
public readonly record struct TerrainDamageResult(
    bool DamageDealt,
    int Damage,
    string DamageType,
    string TerrainName,
    string Message)
{
    /// <summary>
    /// Creates an empty result indicating no damage was dealt.
    /// </summary>
    public static TerrainDamageResult None => new(false, 0, string.Empty, string.Empty, string.Empty);
}
