using System;
using BepInEx;

namespace Silksong.DataManager.Extensions;

/// <summary>
/// Utility methods to help with mod operations.
/// </summary>
public static class PluginExtensions
{
    /// <summary>
    /// Ensure that the profile data for a plugin is loaded if it implements <see cref="IProfileDataMod{T}"/>.
    /// If possible, also ensure that the global data is loaded if it implements <see cref="IGlobalDataMod{T}"/>.
    ///
    /// Note that global data cannot be loaded during any plugin's Awake method.
    ///
    /// If the plugin has had data loaded already, then this method will do nothing; if the plugin has
    /// data loaded by this method, then it will not be loaded again at the usual time.
    /// </summary>
    /// <param name="plugin">The plugin.</param>
    /// <exception cref="InvalidOperationException">If this method is called before <see cref="DataManagerPlugin"/> Awake.
    /// Plugins calling this method should declare DataManager as a <see cref="BepInDependency"/>.</exception>
    public static void EnsureDataLoaded(this BaseUnityPlugin plugin)
    {
        if (DataManagerPlugin.Instance == null)
        {
            throw new InvalidOperationException(
                $"Cannot invoke {nameof(EnsureDataLoaded)} until Data Manager has Awoken."
            );
        }
        string guid = plugin.Info.Metadata.GUID;

        if (!DataManagerPlugin.Instance.ManagedMods.TryGetValue(guid, out ManagedMod mod))
        {
            bool created = ManagedMod.TryCreate(plugin, out mod!);
            if (!created)
            {
                DataManagerPlugin.InstanceLogger.LogWarning(
                    $"Could not create managed mod for {guid}"
                );
                return;
            }

            DataManagerPlugin.Instance.ManagedMods[guid] = mod;
        }

        mod.LoadProfileData();

        if (Platform.Current != null)
        {
            mod.LoadGlobalData();
        }
    }
}
