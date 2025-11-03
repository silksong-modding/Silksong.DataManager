using System.Diagnostics.CodeAnalysis;
using Bep = BepInEx;
using IO = System.IO;

namespace Silksong.DataManager;

internal record ManagedMod(string Guid, IOnceSaveDataMod? OnceSaveData)
{
    internal static bool TryCreate(
        Bep.BaseUnityPlugin plugin,
        [NotNullWhen(true)] out ManagedMod? instance
    )
    {
        instance = null!;

        var guid = plugin.Info.Metadata.GUID;
        var onceSaveData = plugin as IOnceSaveDataMod;

        if (onceSaveData is null)
            return false;

        instance = new(guid, onceSaveData);
        DataManagerPlugin.InstanceLogger.LogInfo($"{guid} uses once-save data");

        return true;
    }

    internal void LoadOnceSaveData(int saveSlot)
    {
        if (OnceSaveData is null)
        {
            return;
        }

        var saveFileName = OnceSaveDataPath(saveSlot);
        try
        {
            var obj = Json.Utils.Deserialize(saveFileName, OnceSaveData.OnceSaveDataType);
            OnceSaveData.UntypedOnceSaveData = obj;
            DataManagerPlugin.InstanceLogger.LogInfo(
                $"Loaded save data for mod {Guid}, slot {saveSlot}"
            );
        }
        catch (IO.FileNotFoundException)
        {
            OnceSaveData.UntypedOnceSaveData = null;
        }
        catch (System.Exception err)
        {
            OnceSaveData.UntypedOnceSaveData = null;
            DataManagerPlugin.InstanceLogger.LogError(
                $"Error loading save data for mod {Guid}, slot {saveSlot}: {err}"
            );
        }
    }

    internal void SaveOnceSaveData(int saveSlot)
    {
        if (OnceSaveData?.UntypedOnceSaveData is not { } data)
        {
            return;
        }

        var saveFileName = OnceSaveDataPath(saveSlot);
        try
        {
            Json.Utils.Serialize(saveFileName, data);
            DataManagerPlugin.InstanceLogger.LogInfo(
                $"Saved save data for mod {Guid}, slot {saveSlot}"
            );
        }
        catch (System.Exception err)
        {
            DataManagerPlugin.InstanceLogger.LogError(
                $"Error saving data for mod {Guid}, slot {saveSlot}: {err}"
            );
        }
    }

    private string OnceSaveDataPath(int slot) =>
        IO.Path.Combine(DataPaths.OnceSaveDataDir(slot), $"{Guid}.json.dat");
}
