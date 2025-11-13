# Silksong.DataManager

DataManager is a mod that saves and loads data - global as well as save slot-specific -
on behalf of other mods.

# Slot-specific data

## Once-save data

Once-save data is per-slot data that is set at the start of the game, and not changed thereafter.

To use this kind of data, your plugin class should implement `IOnceSaveData<T>`, with T being the
type that you want to store. As an example:

```csharp

using Silksong.DataManager;
using BepInEx;

public class SaveData
{
    public long rngSeedForSomePurpose;
}

[BepInAutoPlugin(id: "silk.song.example.mod")]
[BepInDependency("org.silksong-modding.datamanager")]
public partial class StartCrestSelectorPlugin : BaseUnityPlugin, IOnceSaveDataMod<SaveData>
{
    public SaveData? OnceSaveData { get; set; }

    // rest of mod class
}
```

When the player starts a new game - specifically at the end of GameManager.StartNewGame - DataManager
will read the value of the OnceSaveData property, and if non-null, serialize it in the game's saves
directory, under `Modded/OncePerSave/N/GUID.json.dat`, where N is the save slot's number and GUID the
GUID of the client mod.

When the player loads any save file, DataManager will look for a corresponding save file for your mod,
and if present, it will deserialize it and set the OnceSaveData property to the deserialized object.
If not present, it will instead set OnceSaveData to null.
