namespace RuneAndRust.Core.Enums;

/// <summary>
/// Determines which panel has focus in the Specialization UI.
/// </summary>
/// <remarks>See: v0.4.1c (The Tree of Runes) for implementation.</remarks>
public enum SpecializationViewMode
{
    /// <summary>
    /// Left panel focused: browsing available specializations.
    /// </summary>
    SpecList,

    /// <summary>
    /// Right panel focused: navigating nodes within selected tree.
    /// </summary>
    TreeDetail
}
