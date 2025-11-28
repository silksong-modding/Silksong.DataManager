namespace Silksong.DataManager;

using IO = System.IO;

/// <summary>
/// Static class exposing paths to files and folders used by DataManager.
/// </summary>
public static class DataPaths
{
    private const string ModdedSubdir = "Modded";
    private const string OncePerSaveSubdir = "OncePerSave";

    private static string SaveDir(string subdir, int saveSlot)
    {
        // Other platforms are not relevant for modding.
        var platform = (DesktopPlatform)Platform.Current;
        return IO.Path.Combine(platform.saveDirPath, ModdedSubdir, $"user{saveSlot}", subdir);
    }

    /// <summary>
    /// The directory containing data for <see cref="IOnceSaveDataMod{T}"/> mods.
    /// </summary>
    public static string OnceSaveDataDir(int saveSlot) => SaveDir(OncePerSaveSubdir, saveSlot);
}
