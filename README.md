# Silksong.DataManager

DataManager is a mod that saves and loads data - global as well as save slot-specific -
on behalf of other mods.

## Using the mod

To use DataManager as a developer, you will need to do the following:

1. Add a dependency on the Silksong.DataManager NuGet package
2. Add a `BepInDependency` attribute on your plugin class for the `org.silksong-modding.datamanager` plugin
3. Implement one of the data interfaces provided by DataManager (discussed in more depth below)

As a simple example:

```csharp

using Silksong.DataManager;
using BepInEx;

public class SaveData
{
    public long rngSeedForSomePurpose;
}

[BepInAutoPlugin(id: "io.github.username.my-mod")]
[BepInDependency("org.silksong-modding.datamanager")]
public partial class MyPlugin : BaseUnityPlugin, ISaveDataMod<SaveData>
{
    public SaveData? SaveData { get; set; }

    // rest of mod class
}
```

## Types of data

DataManager supports global data, which is an alternative to BepInEx configuration, and per-save data
which is effectively a modded extension to save data.

All data types are handled by generic interfaces, such as `IGlobalDataMod<T>` where T is the type of data you want to
store. DataManager will automatically serialize your data as JSON at appropriate times. DataManager uses the
Newtonsoft.Json library for serialization and the serialization behavior can be altered by using Newtonsoft.Json
attributes in advanced use cases.

In the case that a mod needs to add its own data manually instead of or in addition to these interfaces, the paths
used by DataManager can all be found in the `DataPaths` class.

### Global data

Global data is available for cases where data is needed across multiple saves, or outside of saves such as in the menu.
DataManager offers 2 types of global data which mod developers can choose between based on their use case. Save data
is automatically sychronized across devices by Steam Cloud.

#### `IProfileDataMod`

`IProfileDataMod<T>` is ideal for mods that want a more powerful alternative to BepInEx configuration. Data is stored
in the same location as BepInEx configuration files, which means that is scoped to your profile in the mod loader. It
is loaded when the game opens and saved when the game closes; you can access it in your plugin's `Start` method.

#### `IGlobalDataMod`

`IGlobalDataMod<T>` is an alternative to `IProfileDataMod<T>` which is saved in the save data directory, in
`<save directory>/Modded/Global/<plugin ID>.json.dat`. There are 2 benefits of this location:
1. It will be synced across devices by Steam Cloud
2. It will be available across multiple profiles

Therefore, if either of these use cases are relevant to your mod, you should use `IGlobalDataMod<T>`. Similar to
`IProfileDataMod<T>`, this data is loaded when the game opens and saved when the game closes; you can access it in
your plugin's `Start` method.

### Save-specific data

Save-specific data is a way for your mod to add its own data to a save file. DataManager supports both mutable
and immutable data.

#### `IOnceSaveDataMod`

`IOnceSaveDataMod<T>` provides immutable data which is set at the beginning of the file and never updated again.
It is stored in `<save directory>/Modded/userN/OncePerSave/<mod ID>.json.dat`. Data is saved when a game starts,
specifically at the end of `GameManager.StartNewGame` and loaded when loading into a file. Immutable data results
in fewer disk operations which makes the game more performant, so developers are encouraged to use it when possible.

#### `ISaveDataMod`

`ISaveDataMod<T>` is used for all mutable save data. It is stored in 
`<save directory>/Modded/userN/SaveData/<mod ID>.json.dat`. Data is saved when the game is saved and loaded when
loading into a file.
