using IO = System.IO;

namespace Silksong.DataManager;

/// Interface for mods that need to store some save data at the start of each new game,
/// but don't need to change it afterwards.
public interface IOnceSaveDataMod<T> : IRawOnceSaveDataMod
    where T : class
{
    /// The once-save data for the current file.
    /// DataManager reads and stores its value once at the start of each new game,
    /// after GameManager.StartNewGame completes.
    /// If this property is null at that time, no data are written for that file for the client mod.
    /// Upon loading any existing file, this property is set to the saved data for that file
    /// for the client mod, if any exists.
    /// If there are errors while loading the data (eg. no such data exists), or the game is at the
    /// title screen, this property is set to null.
    T? OnceSaveData { get; set; }

    bool IRawOnceSaveDataMod.HasOnceSaveData => OnceSaveData != null;

    void IRawOnceSaveDataMod.WriteOnceSaveData(IO.Stream saveFile)
    {
        using var sw = new IO.StreamWriter(saveFile);
        // This is only called if OnceSaveData is not null.
        Json.JsonUtil.Serialize(sw, OnceSaveData!);
    }

    void IRawOnceSaveDataMod.ReadOnceSaveData(IO.Stream? saveFile)
    {
        if (saveFile == null)
        {
            OnceSaveData = null;
            return;
        }
        using var sr = new IO.StreamReader(saveFile);
        OnceSaveData = Json.JsonUtil.Deserialize<T>(sr);
    }
}

// Stub for binary compatibility with mods that reference earlier
// versions of DataManager.
internal interface IOnceSaveDataMod { }
