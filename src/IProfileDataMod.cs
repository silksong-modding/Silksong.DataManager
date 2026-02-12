using IO = System.IO;

namespace Silksong.DataManager;

/// Interface for mods that need to store data scoped to a BepInEx profile.
public interface IProfileDataMod<T> : IRawProfileDataMod
    where T : class
{
    /// If there are errors while loading the data (eg. no such data exists) this property is set to
    /// null.
    T? ProfileData { get; set; }

    bool IRawProfileDataMod.HasProfileData => ProfileData != null;

    void IRawProfileDataMod.WriteProfileData(IO.Stream saveFile)
    {
        using var sw = new IO.StreamWriter(saveFile);
        // This is only called if ProfileData is not null.
        Json.JsonUtil.Serialize(sw, ProfileData!);
    }

    void IRawProfileDataMod.ReadProfileData(IO.Stream? saveFile)
    {
        if (saveFile == null)
        {
            ProfileData = null;
            return;
        }
        using var sr = new IO.StreamReader(saveFile);
        ProfileData = Json.JsonUtil.Deserialize<T>(sr);
    }
}

// Stub for binary compatibility with mods that reference earlier
// versions of DataManager.
internal interface IProfileDataMod { }
