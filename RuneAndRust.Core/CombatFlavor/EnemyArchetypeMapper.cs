namespace RuneAndRust.Core.CombatFlavor;

/// <summary>
/// v0.38.6: Maps EnemyType to EnemyArchetype for combat flavor text
/// </summary>
public static class EnemyArchetypeMapper
{
    /// <summary>
    /// Determines the combat flavor archetype for an enemy type
    /// </summary>
    public static EnemyArchetype GetArchetype(EnemyType enemyType)
    {
        return enemyType switch
        {
            // SERVITOR ARCHETYPE - Mechanical enemies
            EnemyType.CorruptedServitor => EnemyArchetype.Servitor,
            EnemyType.BlightDrone => EnemyArchetype.Servitor,
            EnemyType.RuinWarden => EnemyArchetype.Servitor,
            EnemyType.MaintenanceConstruct => EnemyArchetype.Servitor,
            EnemyType.CorrodedSentry => EnemyArchetype.Servitor,
            EnemyType.ArcWelderUnit => EnemyArchetype.Servitor,
            EnemyType.ServitorSwarm => EnemyArchetype.Servitor,
            EnemyType.WarFrame => EnemyArchetype.Servitor,
            EnemyType.VaultCustodian => EnemyArchetype.Servitor,
            EnemyType.FailureColossus => EnemyArchetype.Servitor,
            EnemyType.SentinelPrime => EnemyArchetype.Servitor,
            EnemyType.OmegaSentinel => EnemyArchetype.Servitor,

            // FORLORN ARCHETYPE - Undead/Corrupted humans
            EnemyType.ForlornScholar => EnemyArchetype.Forlorn,
            EnemyType.ForlornArchivist => EnemyArchetype.Forlorn,
            EnemyType.HuskEnforcer => EnemyArchetype.Forlorn,
            EnemyType.Shrieker => EnemyArchetype.Forlorn,
            EnemyType.BoneKeeper => EnemyArchetype.Forlorn,

            // CORRUPTED_DVERGR ARCHETYPE - Mad engineers/technicians
            EnemyType.CorruptedEngineer => EnemyArchetype.Corrupted_Dvergr,
            EnemyType.TestSubject => EnemyArchetype.Corrupted_Dvergr,

            // BLIGHT_TOUCHED_BEAST ARCHETYPE - Corrupted creatures
            EnemyType.ScrapHound => EnemyArchetype.Blight_Touched_Beast,
            EnemyType.SludgeCrawler => EnemyArchetype.Blight_Touched_Beast,

            // AETHER_WRAITH ARCHETYPE - Reality-warping entities
            EnemyType.AethericAberration => EnemyArchetype.Aether_Wraith,
            EnemyType.JotunReaderFragment => EnemyArchetype.Aether_Wraith,
            EnemyType.RustWitch => EnemyArchetype.Aether_Wraith,

            // Default to Servitor for unmapped types
            _ => EnemyArchetype.Servitor
        };
    }

    /// <summary>
    /// Gets the combat flavor archetype for an enemy instance
    /// </summary>
    public static EnemyArchetype GetArchetype(Enemy enemy)
    {
        return GetArchetype(enemy.Type);
    }
}
