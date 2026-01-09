namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Possible states of interactive objects.
/// </summary>
public enum ObjectState
{
    // General states
    /// <summary>Default state.</summary>
    Normal,

    /// <summary>Object is broken or damaged.</summary>
    Broken,

    /// <summary>Object has been destroyed.</summary>
    Destroyed,

    // Door/Chest states
    /// <summary>Closed but not locked.</summary>
    Closed,

    /// <summary>Open.</summary>
    Open,

    /// <summary>Locked and requires a key.</summary>
    Locked,

    /// <summary>Barred from the other side.</summary>
    Barred,

    // Container states
    /// <summary>Container is empty.</summary>
    Empty,

    /// <summary>Container is trapped.</summary>
    Trapped,

    // Lever states
    /// <summary>Lever is in up position.</summary>
    Up,

    /// <summary>Lever is in down position.</summary>
    Down,

    /// <summary>Mechanism is stuck.</summary>
    Stuck,

    // Altar/Statue states
    /// <summary>Active or powered.</summary>
    Active,

    /// <summary>Inactive or dormant.</summary>
    Inactive,

    /// <summary>Corrupted or desecrated.</summary>
    Desecrated,

    /// <summary>Blessed or sanctified.</summary>
    Blessed,

    /// <summary>Damaged but not destroyed.</summary>
    Damaged,

    /// <summary>Defaced or vandalized.</summary>
    Defaced,

    // Light source states
    /// <summary>Lit and providing light.</summary>
    Lit,

    /// <summary>Unlit or extinguished.</summary>
    Unlit,

    /// <summary>Flickering unreliably.</summary>
    Flickering
}
