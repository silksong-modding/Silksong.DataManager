using static System.Linq.Enumerable;
using Bep = BepInEx;
using CG = System.Collections.Generic;
using HL = HarmonyLib;
using IO = System.IO;
using Json = Newtonsoft.Json;
using UE = UnityEngine;

namespace Silksong.DataManager;

[Bep.BepInAutoPlugin(id: "org.silksong-modding.datamanager")]
public partial class DataManagerPlugin : Bep.BaseUnityPlugin
{
    // This property will never be accessed before Start executes.
    internal static DataManagerPlugin Instance { get; private set; } = null!;

    internal CG.Dictionary<string, IOnceSaveDataMod> onceSaveDataMods = new();

    // We must use Start instead of Awake here - PluginInfos does not contain any mod instances
    // when Awake runs.
    private void Start()
    {
        Instance = this;
        foreach (var (_, mod) in Bep.Bootstrap.Chainloader.PluginInfos)
        {
            if (mod.Instance is IOnceSaveDataMod modInstance)
            {
                Logger.LogInfo($"{mod.Metadata.GUID} uses once-save data");
                onceSaveDataMods.Add(mod.Metadata.GUID, modInstance);
            }
        }
        new HL.Harmony(Id).PatchAll();
    }

    private static Json.JsonSerializerSettings jsonSettings = new()
    {
        TypeNameHandling = Json.TypeNameHandling.Auto,
    };

    private const string SyncedFilenameSuffix = ".json.dat";

    [HL.HarmonyPatch(
        typeof(GameManager),
        nameof(GameManager.SetLoadedGameData),
        typeof(SaveGameData),
        typeof(int)
    )]
    private static class OnceLoadHook
    {
        private static void Postfix(int saveSlot)
        {
            var mods = Instance.onceSaveDataMods;

            if (saveSlot == 0)
            {
                foreach (var (_, mod) in mods)
                {
                    mod.UntypedOnceSaveData = null;
                }
                return;
            }

            var saveDir = DataPaths.OnceSaveDataDir(saveSlot);

            foreach (var (guid, mod) in mods)
            {
                var saveFileName = IO.Path.Combine(saveDir, guid + SyncedFilenameSuffix);
                try
                {
                    using var file = IO.File.OpenText(saveFileName);
                    using var reader = new Json.JsonTextReader(file);
                    var ser = Json.JsonSerializer.CreateDefault(jsonSettings);
                    var obj = ser.Deserialize(reader, mod.OnceSaveDataType);
                    mod.UntypedOnceSaveData = obj;
                    Instance.Logger.LogInfo($"Loaded save data for mod {guid}, slot {saveSlot}");
                }
                catch (IO.FileNotFoundException)
                {
                    mod.UntypedOnceSaveData = null;
                }
                catch (System.Exception err)
                {
                    mod.UntypedOnceSaveData = null;
                    Instance.Logger.LogError(
                        $"Error loading save data for mod {guid}, slot {saveSlot}: {err}"
                    );
                }
            }
        }
    }

    [HL.HarmonyPatch(typeof(GameManager), nameof(GameManager.StartNewGame))]
    private static class OnceSetupHook
    {
        private static void Postfix(GameManager __instance)
        {
            var saveSlot = __instance.profileID;
            if (saveSlot == 0)
            {
                return;
            }

            var mods = Instance.onceSaveDataMods;
            var saveDir = DataPaths.OnceSaveDataDir(saveSlot);

            IO.Directory.CreateDirectory(saveDir);

            foreach (var (guid, mod) in mods)
            {
                var data = mod.UntypedOnceSaveData;
                if (data == null)
                {
                    continue;
                }

                var saveFileName = IO.Path.Combine(saveDir, guid + SyncedFilenameSuffix);
                try
                {
                    using var file = IO.File.CreateText(saveFileName);
                    using var writer = new Json.JsonTextWriter(file);
                    var ser = Json.JsonSerializer.CreateDefault(jsonSettings);
                    ser.Serialize(writer, data);
                    Instance.Logger.LogInfo($"Saved save data for mod {guid}, slot {saveSlot}");
                }
                catch (System.Exception err)
                {
                    Instance.Logger.LogError(
                        $"Error saving data for mod {guid}, slot {saveSlot}: {err}"
                    );
                }
            }
        }
    }

    [HL.HarmonyPatch(typeof(GameManager), nameof(GameManager.ClearSaveFile))]
    private static class ClearHook
    {
        private static void Postfix(int saveSlot)
        {
            if (saveSlot == 0)
            {
                return;
            }

            var onceSaveDir = DataPaths.OnceSaveDataDir(saveSlot);
            try
            {
                IO.Directory.Delete(onceSaveDir, true);
                Instance.Logger.LogInfo($"Cleared modded data for slot {saveSlot}");
            }
            catch (IO.DirectoryNotFoundException)
            {
                Instance.Logger.LogInfo($"No modded data to clear for slot {saveSlot}");
            }
            catch (System.Exception err)
            {
                Instance.Logger.LogError($"Error clearing modded data for slot {saveSlot}: {err}");
            }
        }
    }

    [HL.HarmonyPatch(typeof(UE.UI.SaveSlotButton), nameof(UE.UI.SaveSlotButton.ProcessSaveStats))]
    private static class ValidationHook
    {
        private static bool Prefix(
            UE.UI.SaveSlotButton __instance,
            ref bool __result,
            bool doAnimate,
            string errorInfo
        )
        {
            var saveSlot = __instance.SaveSlotIndex;
            var missingMods = Instance.MissingMods(saveSlot);
            if (missingMods.Count == 0)
            {
                return true;
            }
            Instance.Logger.LogInfo($"save slot {saveSlot} has save data for missing mods:");
            foreach (var m in missingMods)
            {
                Instance.Logger.LogInfo(m);
            }
            // to match the behavior of the original method
            CheatManager.LastErrorText = errorInfo;
            __instance.ChangeSaveFileState(
                UE.UI.SaveSlotButton.SaveFileStates.Incompatible,
                doAnimate
            );
            __result = true;
            return false;
        }
    }

    private CG.List<string> MissingMods(int saveSlot)
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
