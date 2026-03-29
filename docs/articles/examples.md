---
uid: ExamplesArticle
---

# Examples

The following is the standard pattern for mods implementing ISaveDataMod.

```csharp
using System.Diagnostics.CodeAnalysis;
using Silksong.DataManager;
using BepInEx;

// The Save Data class can be any serializable type
public class SaveData
{
    public long TheSavedIntValue { get; set; }
}

[BepInAutoPlugin(id: "io.github.username.my-mod")]
[BepInDependency(Silksong.DataManager.DataManagerPlugin.Id)]
public partial class MyPlugin : BaseUnityPlugin, ISaveDataMod<SaveData>
{
    private SaveData _saveData = new SaveData();

    [AllowNull]
    public SaveData SaveData
    {
        get => _saveData;
        set => _saveData = value ?? new SaveData();
    }

    // rest of mod class
}
```

You can freely access members of the SaveData object; when the user enters an existing save file,
DataManager will set this to the value from the last time they used that save file,
and when the user returns to menu, it will be replaced with a new object.

