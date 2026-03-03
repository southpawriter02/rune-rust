namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Identifies the specific source of deterministic self-Corruption for the Blót-Priest.
/// </summary>
/// <remarks>
/// <para>The Blót-Priest is the most Corruption-intensive specialization in the system.
/// Almost every action generates self-Corruption, and several abilities also transfer
/// Corruption to allies (Blight Transference).</para>
///
/// <para>Self-Corruption amounts by trigger:</para>
/// <list type="table">
///   <listheader><term>Trigger</term><description>Corruption Amount</description></listheader>
///   <item><term>Sacrificial Casting (HP→AP)</term><description>+1 per HP-cast (R3: +1, rounded from 0.5)</description></item>
///   <item><term>Blood Siphon cast</term><description>+1 per cast</description></item>
///   <item><term>Gift of Vitae cast</term><description>+1 per cast (+ transfers 1-2 to ally)</description></item>
///   <item><term>Blood Ward cast</term><description>+1 per cast</description></item>
///   <item><term>Exsanguinate tick</term><description>+1 per tick (3 ticks = +3 total)</description></item>
///   <item><term>Hemorrhaging Curse cast</term><description>+2 per cast (fixed)</description></item>
///   <item><term>Heartstopper: Crimson Deluge</term><description>+10 self (+ 5 to each ally)</description></item>
///   <item><term>Heartstopper: Final Anathema</term><description>+15 self (after target death transfer)</description></item>
/// </list>
/// </remarks>
public enum BlotPriestCorruptionTrigger
{
    /// <summary>
    /// Corruption from spending HP to cast via Sanguine Pact.
    /// +1 Corruption per sacrificial cast (R3: +1, rounded from 0.5).
    /// </summary>
    SacrificialCast = 1,

    /// <summary>
    /// Corruption from Blood Siphon's life force extraction.
    /// +1 Corruption per cast (consuming Blighted life force).
    /// </summary>
    BloodSiphonCast = 2,

    /// <summary>
    /// Corruption from Gift of Vitae (healing cast).
    /// +1 self-Corruption per cast. Also transfers 1-2 Corruption to the healed ally.
    /// </summary>
    GiftOfVitaeCast = 3,

    /// <summary>
    /// Corruption from Blood Ward (shield creation).
    /// +1 Corruption per cast.
    /// </summary>
    BloodWardCast = 4,

    /// <summary>
    /// Corruption from Exsanguinate DoT tick (per tick, not per cast).
    /// +1 Corruption per tick, 3 ticks total = +3 over full duration.
    /// </summary>
    ExsanguinateTick = 5,

    /// <summary>
    /// Corruption from Hemorrhaging Curse cast.
    /// +2 Corruption per cast (fixed, no rank reduction).
    /// </summary>
    HemorrhagingCurseCast = 6,

    /// <summary>
    /// Corruption from Heartstopper: Crimson Deluge mode (AoE heal).
    /// +10 self-Corruption, +5 Corruption to each healed ally.
    /// </summary>
    CrimsonDelugeCast = 7,

    /// <summary>
    /// Corruption from Heartstopper: Final Anathema mode (execute).
    /// +15 self-Corruption (after absorbing target's remaining Corruption).
    /// </summary>
    FinalAnathemaCast = 8
}
