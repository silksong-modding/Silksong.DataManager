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
    internal static DataManagerPlugin? Instance;

    internal CG.List<(string GUID, IOnceSaveDataMod Mod)> onceSaveDataMods = new();

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
                onceSaveDataMods.Add((mod.Metadata.GUID, modInstance));
            }
        }
        new HL.Harmony("org.silksong-modding.datamanager").PatchAll();
    }

    private static Json.JsonSerializerSettings jsonSettings = new()
    {
        TypeNameHandling = Json.TypeNameHandling.Auto,
    };

    private static string SaveDir(string subdir, int saveSlot)
    {
        // Other platforms are not relevant for modding.
        var platform = (DesktopPlatform)Platform.Current;
        return IO.Path.Combine(platform.saveDirPath, "Modded", subdir, saveSlot.ToString());
    }

    private const string OncePerSaveSubdir = "OncePerSave";
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
            var mods = Instance!.onceSaveDataMods;

            if (saveSlot == 0)
            {
                foreach (var (_, mod) in mods)
                {
                    mod.UntypedOnceSaveData = null;
                }
                return;
            }

            var saveDir = SaveDir(OncePerSaveSubdir, saveSlot);

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
                    Instance!.Logger.LogInfo($"Loaded save data for mod {guid}, slot {saveSlot}");
                }
                catch (IO.FileNotFoundException)
                {
                    mod.UntypedOnceSaveData = null;
                }
                catch (System.Exception err)
                {
                    mod.UntypedOnceSaveData = null;
                    Instance!.Logger.LogError(
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

            var mods = Instance!.onceSaveDataMods;
            var saveDir = SaveDir(OncePerSaveSubdir, saveSlot);

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
                    Instance!.Logger.LogInfo($"Saved save data for mod {guid}, slot {saveSlot}");
                }
                catch (System.Exception err)
                {
                    Instance!.Logger.LogError(
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

            var onceSaveDir = SaveDir(OncePerSaveSubdir, saveSlot);
            try
            {
                IO.Directory.Delete(onceSaveDir, true);
                Instance!.Logger.LogInfo($"Cleared modded data for slot {saveSlot}");
            }
            catch (IO.DirectoryNotFoundException)
            {
                Instance!.Logger.LogInfo($"No modded data to clear for slot {saveSlot}");
            }
            catch (System.Exception err)
            {
                Instance!.Logger.LogError($"Error clearing modded data for slot {saveSlot}: {err}");
            }
        }
    }
}
