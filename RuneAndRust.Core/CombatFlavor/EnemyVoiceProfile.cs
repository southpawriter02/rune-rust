namespace RuneAndRust.Core.CombatFlavor;

/// <summary>
/// v0.38.6: Enemy archetype voice profile
/// Defines combat personality and descriptor sets for each enemy type
/// </summary>
public class EnemyVoiceProfile
{
    public int ProfileId { get; set; }
    public string EnemyArchetype { get; set; } = string.Empty;
    public string VoiceDescription { get; set; } = string.Empty;
    public string SettingContext { get; set; } = string.Empty;

    // JSON arrays of descriptor_ids
    public string AttackDescriptors { get; set; } = "[]";
    public string ReactionDamage { get; set; } = "[]";
    public string ReactionDeath { get; set; } = "[]";
    public string? SpecialAttacks { get; set; }
}

/// <summary>
/// Enemy archetypes with distinct combat voices
/// </summary>
public enum EnemyArchetype
{
    Servitor,              // Corrupted machines
    Forlorn,               // Undead
    Corrupted_Dvergr,      // Mad engineers
    Blight_Touched_Beast,  // Corrupted animals
    Aether_Wraith          // Paradox entities
}
