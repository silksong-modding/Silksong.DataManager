using IO = System.IO;

namespace Silksong.DataManager;

/// Interface for mods that need to store data that would be shared across all BepInEx profiles.
public interface IGlobalDataMod<T> : IRawGlobalDataMod
    where T : class
{
    /// If there are errors while loading the data (eg. no such data exists) this property is set to
    /// null.
    T? GlobalData { get; set; }

    bool IRawGlobalDataMod.HasGlobalData => GlobalData != null;

    void IRawGlobalDataMod.WriteGlobalData(IO.Stream saveFile)
    {
        using var sw = new IO.StreamWriter(saveFile);
        // This is only called if GlobalData is not null.
        Json.JsonUtil.Serialize(sw, GlobalData!);
    }

    void IRawGlobalDataMod.ReadGlobalData(IO.Stream? saveFile)
    {
        if (saveFile == null)
        {
            GlobalData = null;
            return;
        }
        using var sr = new IO.StreamReader(saveFile);
        GlobalData = Json.JsonUtil.Deserialize<T>(sr);
    }
}

// Stub for binary compatibility with mods that reference earlier
// versions of DataManager.
internal interface IGlobalDataMod { }
