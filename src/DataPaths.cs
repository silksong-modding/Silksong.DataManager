using IO = System.IO;

namespace Silksong.DataManager;

/// <summary>
/// Static class exposing paths to files and folders used by DataManager.
/// </summary>
public static class DataPaths
{
    private static string ModdedDir(string subdir)
    {
        // Other platforms are not relevant for modding.
        var platform = (DesktopPlatform)Platform.Current;
        return IO.Path.Combine(platform.saveDirPath, "Modded", subdir);
    }

    internal static string SaveSlotDir(int saveSlot) => ModdedDir($"user{saveSlot}");

    /// <summary>
    /// The directory containing data for <see cref="IProfileDataMod{T}"/> mods.
    /// </summary>
    public static string ProfileDataDir => BepInEx.Paths.ConfigPath;

    /// <summary>
    /// The directory containing data for <see cref="IGlobalDataMod{T}"/> mods.
    /// </summary>
    public static string GlobalDataDir => ModdedDir("Global");

    /// <summary>
    /// The directory containing data for <see cref="ISaveDataMod{T}"/> mods.
    /// </summary>
    public static string SaveDataDir(int saveSlot) =>
        IO.Path.Combine(SaveSlotDir(saveSlot), "SaveData");

    /// <summary>
    /// The directory containing data for <see cref="IOnceSaveDataMod{T}"/> mods.
    /// </summary>
    public static string OnceSaveDataDir(int saveSlot) =>
        IO.Path.Combine(SaveSlotDir(saveSlot), "OncePerSave");
}
