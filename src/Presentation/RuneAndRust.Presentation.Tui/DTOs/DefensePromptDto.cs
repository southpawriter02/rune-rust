// ═══════════════════════════════════════════════════════════════════════════════
// DefensePromptDto.cs
// Data transfer object for defense action prompt display.
// Version: 0.13.0d
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Presentation.Tui.DTOs;

/// <summary>
/// Data transfer object for defense action prompt display.
/// </summary>
/// <param name="ActionId">Unique identifier of the defense action.</param>
/// <param name="ActionName">Display name of the action (Block, Dodge, Parry).</param>
/// <param name="ActionKey">Keyboard key for this action (B, D, P).</param>
/// <param name="Description">Description of the action's effect.</param>
/// <param name="DamageReduction">Damage reduction percentage (0.0-1.0).</param>
/// <param name="SuccessChance">Success chance percentage (0.0-1.0).</param>
/// <param name="StaminaCost">Stamina cost to use this action.</param>
/// <param name="IsAvailable">Whether the action is currently available.</param>
/// <param name="RequiresShield">Whether the action requires a shield equipped.</param>
public record DefensePromptDto(
    string ActionId,
    string ActionName,
    char ActionKey,
    string Description,
    float DamageReduction,
    float SuccessChance,
    int StaminaCost,
    bool IsAvailable,
    bool RequiresShield);
