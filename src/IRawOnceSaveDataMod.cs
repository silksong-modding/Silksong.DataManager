using IO = System.IO;

namespace Silksong.DataManager;

/// Interface for mods that need to store some save data at the start of each new game,
/// but don't need to change it afterwards.
/// This should only be used by mods that require
/// precise control over their serialization process;
/// typical mods should instead implement <see cref="IOnceSaveDataMod{T}"/>.
public interface IRawOnceSaveDataMod
{
    /// If true, a data file will be created for this mod when creating a new save file.
    bool HasOnceSaveData { get; }

    /// Writes the once-save data for the current file.
    /// Called at the start of each new game, after GameManager.StartNewGame completes.
    void WriteOnceSaveData(IO.Stream saveFile);

    /// Called when a save file is loaded. If no data is present for this mod, saveFile is null.
    void ReadOnceSaveData(IO.Stream? saveFile);
}
