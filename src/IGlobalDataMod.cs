using SC = System.ComponentModel;

namespace Silksong.DataManager;

/// Interface for mods that need to store data that would be shared across all BepInEx profiles.
public interface IGlobalDataMod<T> : IGlobalDataMod
    where T : class
{
    /// If there are errors while loading the data (eg. no such data exists) this property is set to
    /// null.
    T? GlobalData { get; set; }

    System.Type IGlobalDataMod.GlobalDataType => typeof(T);

    object? IGlobalDataMod.UntypedGlobalData
    {
        get => GlobalData;
        set => GlobalData = value == null ? null : (T)value;
    }
}

/// An implementation detail that must be made public due to accessibility rules.
/// Client mods should instead implement <see cref="IGlobalDataMod{T}"/>.
[SC.EditorBrowsable(SC.EditorBrowsableState.Never)]
public interface IGlobalDataMod
{
    /// The target type to use when deserializing save data for this mod.
    System.Type GlobalDataType { get; }

    /// The object (of type <see cref="GlobalDataType"/>) to be serialized.
    object? UntypedGlobalData { get; set; }
}
