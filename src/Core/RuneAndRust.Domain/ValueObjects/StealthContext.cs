namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Extensions;

/// <summary>
/// Encapsulates all context for a stealth check.
/// </summary>
/// <remarks>
/// <para>
/// The stealth context includes:
/// <list type="bullet">
///   <item><description>Surface type determining base DC</description></item>
///   <item><description>Lighting conditions affecting difficulty</description></item>
///   <item><description>Enemy alert status</description></item>
///   <item><description>Zone-specific modifiers</description></item>
///   <item><description>Party check coordination</description></item>
/// </list>
/// </para>
/// </remarks>
/// <param name="SurfaceType">The type of surface being traversed.</param>
/// <param name="BaseDc">Base DC from surface type (in successes).</param>
/// <param name="LightingModifier">DC modifier from lighting conditions.</param>
/// <param name="AlertModifier">DC modifier if enemies are alerted.</param>
/// <param name="ZoneModifier">DC modifier from zone effects (e.g., [Psychic Resonance]).</param>
/// <param name="IsPartyCheck">Whether this is a party stealth check.</param>
/// <param name="PartyMemberIds">IDs of party members if party check.</param>
/// <param name="IsInCombat">Whether stealth is attempted during combat.</param>
public readonly record struct StealthContext(
    StealthSurface SurfaceType,
    int BaseDc,
    int LightingModifier = 0,
    int AlertModifier = 0,
    int ZoneModifier = 0,
    bool IsPartyCheck = false,
    IReadOnlyList<string>? PartyMemberIds = null,
    bool IsInCombat = false)
{
    /// <summary>
    /// DC modifier for dim light / obscuring terrain.
    /// </summary>
    public const int DimLightBonus = -1;

    /// <summary>
    /// DC modifier for being illuminated.
    /// </summary>
    public const int IlluminatedPenalty = 2;

    /// <summary>
    /// DC modifier for enemies being alerted / in combat.
    /// </summary>
    public const int AlertedPenalty = 1;

    /// <summary>
    /// DC modifier for [Psychic Resonance] zones.
    /// </summary>
    public const int PsychicResonanceBonus = -2;

    /// <summary>
    /// Gets the effective DC after all modifiers.
    /// </summary>
    /// <remarks>
    /// Minimum DC is 1 (regardless of bonuses).
    /// </remarks>
    public int EffectiveDc => Math.Max(1, BaseDc + LightingModifier + AlertModifier + ZoneModifier);

    /// <summary>
    /// Gets a value indicating whether the context has favorable lighting.
    /// </summary>
    public bool HasFavorableLighting => LightingModifier < 0;

    /// <summary>
    /// Gets a value indicating whether enemies are on alert.
    /// </summary>
    public bool EnemiesAlerted => AlertModifier > 0;

    /// <summary>
    /// Gets a value indicating whether in a beneficial zone.
    /// </summary>
    public bool InBeneficialZone => ZoneModifier < 0;

    /// <summary>
    /// Creates a stealth context with standard conditions.
    /// </summary>
    /// <param name="surface">The surface type.</param>
    /// <returns>A new StealthContext with base conditions.</returns>
    public static StealthContext Create(StealthSurface surface)
    {
        return new StealthContext(
            SurfaceType: surface,
            BaseDc: surface.GetBaseDc());
    }

    /// <summary>
    /// Creates a stealth context with environmental modifiers.
    /// </summary>
    /// <param name="surface">The surface type.</param>
    /// <param name="isDimLight">Whether in dim lighting.</param>
    /// <param name="isIlluminated">Whether in bright light.</param>
    /// <param name="enemiesAlerted">Whether enemies are alerted.</param>
    /// <param name="isInCombat">Whether currently in combat.</param>
    /// <param name="inPsychicResonance">Whether in a [Psychic Resonance] zone.</param>
    /// <returns>A new StealthContext with all modifiers calculated.</returns>
    public static StealthContext CreateWithModifiers(
        StealthSurface surface,
        bool isDimLight = false,
        bool isIlluminated = false,
        bool enemiesAlerted = false,
        bool isInCombat = false,
        bool inPsychicResonance = false)
    {
        var lightingMod = 0;
        if (isDimLight && !isIlluminated)
        {
            lightingMod = DimLightBonus;
        }
        else if (isIlluminated)
        {
            lightingMod = IlluminatedPenalty;
        }

        var alertMod = (enemiesAlerted || isInCombat) ? AlertedPenalty : 0;
        var zoneMod = inPsychicResonance ? PsychicResonanceBonus : 0;

        return new StealthContext(
            SurfaceType: surface,
            BaseDc: surface.GetBaseDc(),
            LightingModifier: lightingMod,
            AlertModifier: alertMod,
            ZoneModifier: zoneMod,
            IsInCombat: isInCombat);
    }

    /// <summary>
    /// Creates a party stealth context.
    /// </summary>
    /// <param name="surface">The surface type.</param>
    /// <param name="partyMemberIds">IDs of all party members.</param>
    /// <returns>A new StealthContext for party checks.</returns>
    public static StealthContext CreatePartyCheck(
        StealthSurface surface,
        IReadOnlyList<string> partyMemberIds)
    {
        return new StealthContext(
            SurfaceType: surface,
            BaseDc: surface.GetBaseDc(),
            IsPartyCheck: true,
            PartyMemberIds: partyMemberIds);
    }

    /// <summary>
    /// Gets a detailed description of the stealth context for display.
    /// </summary>
    /// <returns>A multi-line string describing all stealth factors.</returns>
    public string ToDescription()
    {
        var lines = new List<string>
        {
            $"Surface: {SurfaceType.GetDescription()}",
            $"Base DC: {BaseDc} successes"
        };

        if (LightingModifier != 0)
        {
            var lightDesc = LightingModifier < 0 ? "Dim Light" : "Illuminated";
            lines.Add($"Lighting: {lightDesc} ({LightingModifier:+#;-#;+0})");
        }

        if (AlertModifier != 0)
        {
            lines.Add($"Alert Status: Enemies aware ({AlertModifier:+#;-#;+0})");
        }

        if (ZoneModifier != 0)
        {
            lines.Add($"Zone Effect: [Psychic Resonance] ({ZoneModifier:+#;-#;+0})");
        }

        lines.Add($"Final DC: {EffectiveDc} successes");

        if (IsPartyCheck)
        {
            lines.Add($"Party Check: {PartyMemberIds?.Count ?? 0} members (weakest link)");
        }

        return string.Join(Environment.NewLine, lines);
    }

    /// <inheritdoc/>
    public override string ToString() =>
        $"Stealth DC {EffectiveDc} ({SurfaceType}{(IsPartyCheck ? ", party" : "")})";
}
