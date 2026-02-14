using IO = System.IO;

namespace Silksong.DataManager;

/// Interface for mods that need to store save data and require
/// precise control over their serialization process.
/// Typical mods should implement <see cref="IGlobalDataMod{T}"/>.
public interface IRawGlobalDataMod
{
    /// If true, a data file will be created for this mod when closing the game.
    bool HasGlobalData { get; }

    /// Writes the global data for this mod.
    void WriteGlobalData(IO.Stream saveFile);

    /// Called when the game starts up. If no data is present for this mod, saveFile is null.
    void ReadGlobalData(IO.Stream? saveFile);
}
