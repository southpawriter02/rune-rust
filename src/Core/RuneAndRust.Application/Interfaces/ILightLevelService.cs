// ═══════════════════════════════════════════════════════════════════════════════
// ILightLevelService.cs
// Interface for querying environmental light conditions at positions and
// for characters. Used by Myrk-gengr abilities for targeting and effects.
// Version: 0.20.4a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Service interface for querying light conditions at positions and
/// for characters.
/// </summary>
/// <remarks>
/// <para>
/// Light level data drives all Myrk-gengr mechanics: Shadow Essence generation,
/// Shadow Step targeting, Cloak of Night effectiveness, and Corruption risk.
/// This service abstracts the environment's light system for use by
/// specialization services.
/// </para>
/// </remarks>
/// <seealso cref="ShadowLightLevel"/>
public interface ILightLevelService
{
    /// <summary>
    /// Gets the light level at the specified grid position.
    /// </summary>
    /// <param name="x">X coordinate.</param>
    /// <param name="y">Y coordinate.</param>
    /// <returns>Light level information at the position.</returns>
    ShadowLightLevel GetLightLevelAtPosition(int x, int y);

    /// <summary>
    /// Gets the light level at the specified character's current position.
    /// </summary>
    /// <param name="characterId">Character to query.</param>
    /// <returns>Light level information at the character's position.</returns>
    ShadowLightLevel GetLightLevelForCharacter(Guid characterId);

    /// <summary>
    /// Checks whether the specified position is in shadow (Darkness or DimLight).
    /// </summary>
    /// <param name="x">X coordinate.</param>
    /// <param name="y">Y coordinate.</param>
    /// <returns><c>true</c> if the position is in shadow.</returns>
    bool IsInShadow(int x, int y);
}
