namespace Silksong.DataManager;

/// <summary>
/// Interface indicating that this mod may be required for the save.
///
/// If a mod implements this interface with <see cref="IsRequired"/> true,
/// then save files associated with the mod will not be loadable if
/// that mod is uninstalled.
/// </summary>
public interface IRequiredMod
{
    /// <summary>
    /// If this returns true, this mod will be required for the save.
    /// </summary>
    bool IsRequired { get; }
}
