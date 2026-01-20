// ═══════════════════════════════════════════════════════════════════════════════
// IncomingAttackDto.cs
// Data transfer object for incoming attack information.
// Version: 0.13.0d
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Presentation.Tui.DTOs;

/// <summary>
/// Data transfer object for incoming attack information.
/// </summary>
/// <param name="AttackerId">ID of the attacking combatant.</param>
/// <param name="AttackerName">Display name of the attacker.</param>
/// <param name="AttackName">Name of the attack being used.</param>
/// <param name="Damage">Expected damage amount.</param>
/// <param name="DamageType">Type of damage (physical, fire, etc.).</param>
public record IncomingAttackDto(
    Guid AttackerId,
    string AttackerName,
    string AttackName,
    int Damage,
    string DamageType);
