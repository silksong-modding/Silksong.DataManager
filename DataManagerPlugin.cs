using BepInEx;

namespace Silksong.DataManager;

[BepInAutoPlugin(id: "org.silksong-modding.datamanager")]
public partial class DataManagerPlugin : BaseUnityPlugin
{
    private void Awake()
    {
        // Put your initialization logic here
        Logger.LogInfo($"Plugin {Name} ({Id}) has loaded!");
    }
}
