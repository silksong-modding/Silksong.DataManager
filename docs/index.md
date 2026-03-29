# DataManager

DataManager is a mod that saves and loads data - global as well as save slot-specific -
on behalf of other mods.

## Usage

Add the following line to your .csproj:
```
<PackageReference Include="Silksong.DataManager" Version="1.2.1" />
```
The most up to date version number can be retrieved from [Nuget](https://www.nuget.org/packages/Silksong.DataManager).

You will also need to add a dependency to your thunderstore.toml:
```
silksong_modding-DataManager = "1.2.1"
```
The version number does not matter hugely, but the most up to date number can be retrieved from
[Thunderstore](https://thunderstore.io/c/hollow-knight-silksong/p/silksong_modding/DataManager/).
If manually uploading, instead copy the dependency string from the Thunderstore link.

DataManager should be added as a BepInEx dependency by putting the following attribute
onto your plugin class, below the BepInAutoPlugin attribute.
```
[BepInDependency(Silksong.DataManager.DataManagerPlugin.Id)]
```
