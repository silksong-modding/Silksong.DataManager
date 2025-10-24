namespace Silksong.DataManager;

using IO = System.IO;

public static class DataPaths
{
    private const string ModdedSubdir = "Modded";
    private const string OncePerSaveSubdir = "OncePerSave";

    private static string SaveDir(string subdir, int saveSlot)
    {
        // Other platforms are not relevant for modding.
        var platform = (DesktopPlatform)Platform.Current;
        return IO.Path.Combine(platform.saveDirPath, ModdedSubdir, subdir, saveSlot.ToString());
    }

    public static string OnceSaveDataDir(int saveSlot) => SaveDir(OncePerSaveSubdir, saveSlot);
}
