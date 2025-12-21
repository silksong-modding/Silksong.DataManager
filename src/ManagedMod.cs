using System.Diagnostics.CodeAnalysis;
using Bep = BepInEx;
using IO = System.IO;

namespace Silksong.DataManager;

internal record ManagedMod(
    string Guid,
    IProfileDataMod? ProfileData,
    IGlobalDataMod? GlobalData,
    ISaveDataMod? SaveData,
    IOnceSaveDataMod? OnceSaveData
)
{
    private bool _loadedProfileData = false;
    private bool _loadedGlobalData = false;

    internal static bool TryCreate(
        Bep.BaseUnityPlugin plugin,
        [NotNullWhen(true)] out ManagedMod? instance
    )
    {
        instance = null!;

        var guid = plugin.Info.Metadata.GUID;
        var profileData = plugin as IProfileDataMod;
        var globalData = plugin as IGlobalDataMod;
        var saveData = plugin as ISaveDataMod;
        var onceSaveData = plugin as IOnceSaveDataMod;

        if (profileData is null && globalData is null && saveData is null && onceSaveData is null)
            return false;

        instance = new(guid, profileData, globalData, saveData, onceSaveData);

        // TODO(UserIsntAvailable): Display which interfaces the plugin implements.
        DataManagerPlugin.InstanceLogger.LogInfo($"{guid} uses data manager");

        return true;
    }

    internal void LoadProfileData()
    {
        if (_loadedProfileData)
            return;

        if (ProfileData is not null)
        {
            LoadUntypedData(
                ProfileDataPath,
                ProfileData.ProfileDataType,
                obj => ProfileData.UntypedProfileData = obj
            );
        }

        _loadedProfileData = true;
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
        if (_loadedGlobalData)
            return;

        if (GlobalData is not null)
        {
            // TODO(UserIsntAvailable): Overrides
            LoadUntypedData(
                GlobalDataPath,
                GlobalData.GlobalDataType,
                obj => GlobalData.UntypedGlobalData = obj
            );
        }

        _loadedGlobalData = true;
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
            LoadUntypedData(
                SaveDataPath(saveSlot),
                SaveData.SaveDataType,
                obj => SaveData.UntypedSaveData = obj
            );
        }
    }

    internal void SaveSaveData(int saveSlot)
    {
        if (SaveData is not null)
        {
            SaveUntypedData(
                SaveDataPath(saveSlot),
                SaveData.SaveDataType,
                SaveData.UntypedSaveData
            );
        }
    }

    internal void LoadOnceSaveData(int saveSlot)
    {
        if (OnceSaveData is not null)
        {
            LoadUntypedData(
                OnceSaveDataPath(saveSlot),
                OnceSaveData.OnceSaveDataType,
                obj => OnceSaveData.UntypedOnceSaveData = obj
            );
        }
    }

    internal void SaveOnceSaveData(int saveSlot)
    {
        if (OnceSaveData is not null)
        {
            SaveUntypedData(
                OnceSaveDataPath(saveSlot),
                OnceSaveData.OnceSaveDataType,
                OnceSaveData.UntypedOnceSaveData
            );
        }
    }

    private string ProfileDataPath => IO.Path.Combine(DataPaths.ProfileDataDir, $"{Guid}.json");

    private string GlobalDataPath => IO.Path.Combine(DataPaths.GlobalDataDir, $"{Guid}.json.dat");

    private string SaveDataPath(int slot) =>
        IO.Path.Combine(DataPaths.SaveDataDir(slot), $"{Guid}.json.dat");

    private string OnceSaveDataPath(int slot) =>
        IO.Path.Combine(DataPaths.OnceSaveDataDir(slot), $"{Guid}.json.dat");

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
