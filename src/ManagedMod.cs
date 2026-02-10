using System.Diagnostics.CodeAnalysis;
using BepInEx;
using Bep = BepInEx;
using IO = System.IO;

namespace Silksong.DataManager;

internal record ManagedMod(
    string Guid,
    IProfileDataMod? ProfileData,
    IGlobalDataMod? GlobalData,
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
        var profileData = plugin as IProfileDataMod;
        var globalData = plugin as IGlobalDataMod;
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
            LoadUntypedData(
                ProfileDataPath,
                ProfileData.ProfileDataType,
                obj => ProfileData.UntypedProfileData = obj
            );
        }
    }

    internal void SaveProfileData()
    {
        if (ProfileData is not null)
        {
            SaveUntypedData(
                ProfileDataPath,
                ProfileData.ProfileDataType,
                ProfileData.UntypedProfileData
            );
        }
    }

    internal void LoadGlobalData()
    {
        if (GlobalData is not null)
        {
            // TODO(UserIsntAvailable): Overrides
            LoadUntypedData(
                GlobalDataPath,
                GlobalData.GlobalDataType,
                obj => GlobalData.UntypedGlobalData = obj
            );
        }
    }

    internal void SaveGlobalData()
    {
        if (GlobalData is not null)
        {
            SaveUntypedData(
                GlobalDataPath,
                GlobalData.GlobalDataType,
                GlobalData.UntypedGlobalData
            );
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

    private void LoadUntypedData(string path, System.Type dataType, System.Action<object?> onLoad)
    {
        try
        {
            var obj = Json.JsonUtil.Deserialize(path, dataType);
            onLoad(obj);
            DataManagerPlugin.InstanceLogger.LogInfo($"Loaded {dataType.Name} for mod {Guid}");
        }
        catch (System.Exception err)
            when (err is IO.FileNotFoundException or IO.DirectoryNotFoundException)
        {
            onLoad(null);
        }
        catch (System.Exception err)
        {
            onLoad(null);
            DataManagerPlugin.InstanceLogger.LogError(
                $"Error loading {dataType.Name} for mod {Guid}: {err}"
            );
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

    private void SaveUntypedData(string path, System.Type dataType, object? untypedData)
    {
        if (untypedData is null)
        {
            return;
        }

        try
        {
            Json.JsonUtil.Serialize(path, untypedData);
            DataManagerPlugin.InstanceLogger.LogInfo($"Saved {dataType.Name} for mod {Guid}");
        }
        catch (System.Exception err)
        {
            DataManagerPlugin.InstanceLogger.LogError(
                $"Error saving {dataType.Name} for mod {Guid}: {err}"
            );
        }
    }
}
