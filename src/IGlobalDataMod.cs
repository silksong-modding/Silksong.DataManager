namespace Silksong.DataManager;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public interface IGlobalDataMod<T> : IGlobalDataMod
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
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
public interface IGlobalDataMod
{
    /// The target type to use when deserializing save data for this mod.
    System.Type GlobalDataType { get; }

    /// The object (of type <see cref="GlobalDataType"/>) to be serialized.
    object? UntypedGlobalData { get; set; }
}
