using IO = System.IO;

namespace Silksong.DataManager;

/// Interface for mods that need to store save data and require
/// precise control over their serialization process.
/// Typical mods should implement <see cref="ISaveDataMod{T}"/>.
public interface IRawSaveDataMod
{
    /// If true, a data file will be created for this mod when saving the game.
    bool HasSaveData { get; }

    void WriteSaveData(IO.Stream saveFile);

    /// Called when a save file is loaded. If no data is present for this mod, saveFile is null.
    void ReadSaveData(IO.Stream? saveFile);
}
