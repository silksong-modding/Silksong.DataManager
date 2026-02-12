using System.Diagnostics.CodeAnalysis;
using BepInEx;
using Bep = BepInEx;
using IO = System.IO;

namespace Silksong.DataManager;

internal record ManagedMod(
    string Guid,
    IRawProfileDataMod? ProfileData,
    IRawGlobalDataMod? GlobalData,
    IRawSaveDataMod? SaveData,
    IRawOnceSaveDataMod? OnceSaveData,
    IRequiredMod? RequiredMod
)
{
    internal static bool TryCreate(
        Bep.BaseUnityPlugin plugin,
        [NotNullWhen(true)] out ManagedMod? instance
    )
    {
        instance = null!;

        var guid = plugin.Info.Metadata.GUID;
        var profileData = plugin as IRawProfileDataMod;
        var globalData = plugin as IRawGlobalDataMod;
        var saveData = plugin as IRawSaveDataMod;
        var onceSaveData = plugin as IRawOnceSaveDataMod;
        var requiredMod = plugin as IRequiredMod;

        if (
            profileData is null
            && globalData is null
            && saveData is null
            && onceSaveData is null
            && requiredMod is null
        )
            return false;

        instance = new(guid, profileData, globalData, saveData, onceSaveData, requiredMod);

        // TODO(UserIsntAvailable): Display which interfaces the plugin implements.
        DataManagerPlugin.InstanceLogger.LogInfo($"{guid} uses data manager");

        return true;
    }

    public bool HasAnyGlobalData => ProfileData is not null || GlobalData is not null;
    public bool HasAnySaveData => SaveData is not null || OnceSaveData is not null;

    public bool IsRequired => RequiredMod?.IsRequired ?? false;

    internal void LoadProfileData()
    {
        if (ProfileData is not null)
        {
            LoadStreamData(ProfileDataPath, ProfileData.ReadProfileData);
        }
    }

    internal void SaveProfileData()
    {
        if (ProfileData is not null && ProfileData.HasProfileData)
        {
            SaveStreamData(ProfileDataPath, ProfileData.WriteProfileData);
        }
    }

    internal void LoadGlobalData()
    {
        if (GlobalData is not null)
        {
            // TODO(UserIsntAvailable): Overrides
            LoadStreamData(GlobalDataPath, GlobalData.ReadGlobalData);
        }
    }

    internal void SaveGlobalData()
    {
        if (GlobalData is not null && GlobalData.HasGlobalData)
        {
            SaveStreamData(GlobalDataPath, GlobalData.WriteGlobalData);
        }
    }

    internal void LoadSaveData(int saveSlot)
    {
        if (SaveData is not null)
        {
            LoadStreamData(SaveDataPath(saveSlot), SaveData.ReadSaveData);
        }
    }

    internal void SaveSaveData(int saveSlot)
    {
        if (SaveData is not null && SaveData.HasSaveData)
        {
            SaveStreamData(SaveDataPath(saveSlot), SaveData.WriteSaveData);
        }
    }

    internal void LoadOnceSaveData(int saveSlot)
    {
        if (OnceSaveData is not null)
        {
            LoadStreamData(OnceSaveDataPath(saveSlot), OnceSaveData.ReadOnceSaveData);
        }
    }

    internal void SaveOnceSaveData(int saveSlot)
    {
        if (OnceSaveData is not null && OnceSaveData.HasOnceSaveData)
        {
            SaveStreamData(OnceSaveDataPath(saveSlot), OnceSaveData.WriteOnceSaveData);
        }
    }

    private string ProfileDataPath => IO.Path.Combine(DataPaths.ProfileDataDir, $"{Guid}.json");

    private string GlobalDataPath => IO.Path.Combine(DataPaths.GlobalDataDir, $"{Guid}.json.dat");

    private string SaveDataPath(int slot) =>
        IO.Path.Combine(DataPaths.SaveDataDir(slot), $"{Guid}.json.dat");

    private string OnceSaveDataPath(int slot) =>
        IO.Path.Combine(DataPaths.OnceSaveDataDir(slot), $"{Guid}.json.dat");

    private void LoadStreamData(string path, System.Action<IO.Stream?> reader)
    {
        try
        {
            using var file = IO.File.OpenRead(path);
            reader(file);
        }
        catch (System.Exception err)
            when (err is IO.FileNotFoundException or IO.DirectoryNotFoundException)
        {
            reader(null);
        }
        catch (System.Exception err)
        {
            reader(null);
            DataManagerPlugin.InstanceLogger.LogError($"Error loading data for mod {Guid}: {err}");
        }
    }

    private void SaveStreamData(string path, System.Action<IO.Stream> writer)
    {
        try
        {
            using var file = IO.File.Create(path);
            writer(file);
        }
        catch (System.Exception err)
        {
            DataManagerPlugin.InstanceLogger.LogError($"Error saving data for mod {Guid}: {err}");
        }
    }
}
