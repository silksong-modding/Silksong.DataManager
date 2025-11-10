using static System.Linq.Enumerable;
using Bep = BepInEx;
using CG = System.Collections.Generic;
using HL = HarmonyLib;
using IO = System.IO;
using Json = Newtonsoft.Json;
using TC = TeamCherry;
using UE = UnityEngine;

namespace Silksong.DataManager;

[Bep.BepInAutoPlugin(id: "org.silksong-modding.datamanager")]
[Bep.BepInDependency("org.silksong-modding.i18n")]
public partial class DataManagerPlugin : Bep.BaseUnityPlugin
{
    // This property will never be accessed before Start executes.
    internal static DataManagerPlugin Instance { get; private set; } = null!;

    internal static Bep.Logging.ManualLogSource InstanceLogger => Instance.Logger;

    internal CG.Dictionary<string, IOnceSaveDataMod> onceSaveDataMods = new();

    private void Awake()
    {
        Instance = this;
        new HL.Harmony(Id).PatchAll();

        Logger.LogInfo("Mod Loaded");
    }

    // We must use Start instead of Awake here - PluginInfos does not contain
    // any mod instances when Awake runs.
    private void Start()
    {
        foreach (var (_, mod) in Bep.Bootstrap.Chainloader.PluginInfos)
        {
            if (mod.Instance is IOnceSaveDataMod modInstance)
            {
                Logger.LogInfo($"{mod.Metadata.GUID} uses once-save data");
                onceSaveDataMods.Add(mod.Metadata.GUID, modInstance);
            }
        }
    }

    internal static Json.JsonSerializerSettings jsonSettings = new()
    {
        TypeNameHandling = Json.TypeNameHandling.Auto,
    };

    internal const string SyncedFilenameSuffix = ".json.dat";

    internal static void ClearModdedData(int saveSlot)
    {
        var onceSaveDir = DataPaths.OnceSaveDataDir(saveSlot);
        try
        {
            IO.Directory.Delete(onceSaveDir, true);
            DataManagerPlugin.InstanceLogger.LogInfo($"Cleared modded data for slot {saveSlot}");
        }
        catch (IO.DirectoryNotFoundException)
        {
            DataManagerPlugin.InstanceLogger.LogInfo(
                $"No modded data to clear for slot {saveSlot}"
            );
        }
        catch (System.Exception err)
        {
            DataManagerPlugin.InstanceLogger.LogError(
                $"Error clearing modded data for slot {saveSlot}: {err}"
            );
        }
    }

    internal static TC.Localization.LocalisedString SetSaveIncompatibleText(
        UE.UI.SaveSlotButton button,
        TC.Localization.LocalisedString s
    )
    {
        var smallDescLocalizer = button
            .saveIncompatibleText.gameObject.transform.Find("Small Desc")
            .gameObject.GetComponent<AutoLocalizeTextUI>();
        var oldText = smallDescLocalizer.Text;
        smallDescLocalizer.Text = s;
        return oldText;
    }

    internal CG.List<string> MissingMods(int saveSlot)
    {
        var onceSaveDir = DataPaths.OnceSaveDataDir(saveSlot);
        try
        {
            // The ?* instead of just * is to work around a quirk of EnumerateFiles;
            // see https://learn.microsoft.com/en-us/dotnet/api/system.io.directory.enumeratefiles?view=netstandard-2.1#system-io-directory-enumeratefiles(system-string-system-string)
            return IO
                .Directory.EnumerateFiles(onceSaveDir, "?*" + SyncedFilenameSuffix)
                .Select(path =>
                {
                    var name = IO.Path.GetFileName(path);
                    return name.Substring(0, name.Length - SyncedFilenameSuffix.Length);
                })
                .Where(modGUID => !onceSaveDataMods.ContainsKey(modGUID))
                .ToList();
        }
        catch (IO.DirectoryNotFoundException)
        {
            return [];
        }
    }
}
