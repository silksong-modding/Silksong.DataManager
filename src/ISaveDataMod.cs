using IO = System.IO;

namespace Silksong.DataManager;

/// Interface for mods that need to store save data.
public interface ISaveDataMod<T> : IRawSaveDataMod
    where T : class
{
    /// If there are errors while loading the data (eg. no such data exists), or the game is at the
    /// title screen, this property is set to null.
    T? SaveData { get; set; }

    bool IRawSaveDataMod.HasSaveData => SaveData != null;

    /// Writes the save data for the current file.
    void IRawSaveDataMod.WriteSaveData(IO.Stream saveFile)
    {
        using var sw = new IO.StreamWriter(saveFile);
        // This is only called if SaveData is not null.
        Json.JsonUtil.Serialize(sw, SaveData!);
    }

    void IRawSaveDataMod.ReadSaveData(IO.Stream? saveFile)
    {
        if (saveFile == null)
        {
            SaveData = null;
            return;
        }
        using var sr = new IO.StreamReader(saveFile);
        SaveData = Json.JsonUtil.Deserialize<T>(sr);
    }
}

// Stub for binary compatibility with mods that reference earlier
// versions of DataManager.
internal interface ISaveDataMod { }
