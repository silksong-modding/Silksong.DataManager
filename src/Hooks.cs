using HL = HarmonyLib;
using IO = System.IO;
using TC = TeamCherry;
using UE = UnityEngine;

namespace Silksong.DataManager;

[HL.HarmonyPatch(
    typeof(GameManager),
    nameof(GameManager.SetLoadedGameData),
    typeof(SaveGameData),
    typeof(int)
)]
internal static class OnceLoadHook
{
    private static void Postfix(int saveSlot)
    {
        var mods = DataManagerPlugin.Instance.onceSaveDataMods;

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
            var saveFileName = IO.Path.Combine(
                saveDir,
                guid + DataManagerPlugin.SyncedFilenameSuffix
            );
            try
            {
                var obj = Json.Utils.Deserialize(saveFileName, mod.OnceSaveDataType);
                mod.UntypedOnceSaveData = obj;
                DataManagerPlugin.InstanceLogger.LogInfo(
                    $"Loaded save data for mod {guid}, slot {saveSlot}"
                );
            }
            catch (IO.FileNotFoundException)
            {
                mod.UntypedOnceSaveData = null;
            }
            catch (System.Exception err)
            {
                mod.UntypedOnceSaveData = null;
                DataManagerPlugin.InstanceLogger.LogError(
                    $"Error loading save data for mod {guid}, slot {saveSlot}: {err}"
                );
            }
        }
    }
}

[HL.HarmonyPatch(typeof(GameManager), nameof(GameManager.StartNewGame))]
internal static class OnceSetupHook
{
    private static void Postfix(GameManager __instance)
    {
        var saveSlot = __instance.profileID;
        if (saveSlot == 0)
        {
            return;
        }

        var mods = DataManagerPlugin.Instance.onceSaveDataMods;
        var saveDir = DataPaths.OnceSaveDataDir(saveSlot);

        // Clear any existing modded data for this slot.
        // This can happen if a savefile is started with a mod active, and then the
        // game is closed before first saving that savefile.
        // Without clearing that data, it would be erroneously applied to the new savefile,
        // potentially even rendering it (spuriously) incompatible if said mod was uninstalled
        // in the meantime.
        DataManagerPlugin.ClearModdedData(saveSlot);
        IO.Directory.CreateDirectory(saveDir);

        foreach (var (guid, mod) in mods)
        {
            var data = mod.UntypedOnceSaveData;
            if (data == null)
            {
                continue;
            }

            var saveFileName = IO.Path.Combine(
                saveDir,
                guid + DataManagerPlugin.SyncedFilenameSuffix
            );
            try
            {
                Json.Utils.Serialize(saveFileName, data);
                DataManagerPlugin.InstanceLogger.LogInfo(
                    $"Saved save data for mod {guid}, slot {saveSlot}"
                );
            }
            catch (System.Exception err)
            {
                DataManagerPlugin.InstanceLogger.LogError(
                    $"Error saving data for mod {guid}, slot {saveSlot}: {err}"
                );
            }
        }
    }
}

[HL.HarmonyPatch(typeof(GameManager), nameof(GameManager.ClearSaveFile))]
internal static class ClearHook
{
    private static void Postfix(int saveSlot)
    {
        if (saveSlot != 0)
        {
            DataManagerPlugin.ClearModdedData(saveSlot);
        }
    }
}

[HL.HarmonyPatch(typeof(UE.UI.SaveSlotButton), nameof(UE.UI.SaveSlotButton.ProcessSaveStats))]
internal static class ValidationHook
{
    private static TC.Localization.LocalisedString? vanillaSaveIncompatibleText;

    private static bool Prefix(
        UE.UI.SaveSlotButton __instance,
        ref bool __result,
        bool doAnimate,
        string errorInfo
    )
    {
        var saveSlot = __instance.SaveSlotIndex;
        var missingMods = DataManagerPlugin.Instance.MissingMods(saveSlot);
        if (missingMods.Count == 0)
        {
            if (vanillaSaveIncompatibleText is { } text)
            {
                DataManagerPlugin.SetSaveIncompatibleText(__instance, text);
                // Since the vanilla text is restored, we don't need to do this again.
                vanillaSaveIncompatibleText = null;
            }
            return true;
        }
        DataManagerPlugin.InstanceLogger.LogInfo(
            $"save slot {saveSlot} has save data for missing mods:"
        );
        foreach (var m in missingMods)
        {
            DataManagerPlugin.InstanceLogger.LogInfo(m);
        }
        vanillaSaveIncompatibleText = DataManagerPlugin.SetSaveIncompatibleText(
            __instance,
            new($"Mods.{DataManagerPlugin.Id}", "REQUIRED_MODS_MISSING")
        );
        // to match the behavior of the original method
        CheatManager.LastErrorText = errorInfo;
        __instance.ChangeSaveFileState(UE.UI.SaveSlotButton.SaveFileStates.Incompatible, doAnimate);
        __result = true;
        return false;
    }
}
