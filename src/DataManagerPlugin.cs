using BepInEx.Configuration;
using static System.Linq.Enumerable;
using Bep = BepInEx;
using CG = System.Collections.Generic;
using HL = HarmonyLib;
using IO = System.IO;
using TC = TeamCherry;
using UE = UnityEngine;

namespace Silksong.DataManager;

[Bep.BepInAutoPlugin(id: "org.silksong-modding.datamanager")]
[Bep.BepInDependency("org.silksong-modding.i18n")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public partial class DataManagerPlugin : Bep.BaseUnityPlugin, IOnceSaveDataMod<Data.DMOnceSaveData>
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
{
    // These properties will never be accessed before Start executes.
    internal static DataManagerPlugin Instance { get; private set; } = null!;
    internal static Bep.Logging.ManualLogSource InstanceLogger => Instance.Logger;

    internal CG.Dictionary<string, ManagedMod> ManagedMods = [];

    internal ManagedMod ManagedSelf => ManagedMods[this.Info.Metadata.GUID];

    /// <summary>
    /// The OnceSaveData for DataManager.
    /// </summary>
    internal Data.DMOnceSaveData? OnceSaveData { get; set; }

    // The interface implementation has to be explicit because OnceSaveData is nonpublic.
    Data.DMOnceSaveData? IOnceSaveDataMod<Data.DMOnceSaveData>.OnceSaveData
    {
        get => OnceSaveData;
        set => OnceSaveData = value;
    }

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
        foreach (var (guid, info) in Bep.Bootstrap.Chainloader.PluginInfos)
        {
            if (
                info?.Instance is not { } plugin
                || !ManagedMod.TryCreate(plugin, out var managedMod)
            )
                continue;

            managedMod.LoadProfileData();
            managedMod.LoadGlobalData();
            ManagedMods.Add(guid, managedMod);
        }
    }

    internal static void ClearModdedSaveSlot(int saveSlot)
    {
        var saveDir = DataPaths.SaveSlotDir(saveSlot);

        try
        {
            IO.Directory.Delete(saveDir, true);
            DataManagerPlugin.InstanceLogger.LogInfo(
                $"Cleared modded save slot for slot {saveSlot}"
            );
        }
        catch (IO.DirectoryNotFoundException)
        {
            DataManagerPlugin.InstanceLogger.LogInfo(
                $"No modded save slot to clear for slot {saveSlot}"
            );
        }
        catch (System.Exception err)
        {
            DataManagerPlugin.InstanceLogger.LogError(
                $"Error clearing modded save slot for slot {saveSlot}: {err}"
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
        try
        {
            ManagedSelf.LoadOnceSaveData(saveSlot);
        }
        catch (IO.IOException)
        {
            return [];
        }

        var requiredMods = this.OnceSaveData?.RequiredModGuids ?? [];
        var missingMods = requiredMods.Where(guid => !ManagedMods.ContainsKey(guid)).ToList();

        return missingMods;
    }
}
