namespace Monod.ModSystem;

/// <summary>
/// Represents status a <see cref="Mod"/> can have.
/// </summary>
public enum ModStatus
{
    /// <summary>
    /// Mod is disabled by user.
    /// </summary>
    Disabled,
    
    /// <summary>
    /// Mod is enabled and working successfully.
    /// </summary>
    Enabled,
    
    /// <summary>
    /// Mod failed to load.
    /// </summary>
    FailedToLoad,
}