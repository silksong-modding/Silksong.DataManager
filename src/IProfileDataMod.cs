namespace Silksong.DataManager;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public interface IProfileDataMod<T> : IProfileDataMod
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    where T : class
{
    /// If there are errors while loading the data (eg. no such data exists) this property is set to
    /// null.
    T? ProfileData { get; set; }

    System.Type IProfileDataMod.ProfileDataType => typeof(T);

    object? IProfileDataMod.UntypedProfileData
    {
        get => ProfileData;
        set => ProfileData = value == null ? null : (T)value;
    }
}

/// An implementation detail that must be made public due to accessibility rules.
/// Client mods should instead implement <see cref="IProfileDataMod{T}"/>.
public interface IProfileDataMod
{
    /// The target type to use when deserializing save data for this mod.
    System.Type ProfileDataType { get; }

    /// The object (of type <see cref="ProfileDataType"/>) to be serialized.
    object? UntypedProfileData { get; set; }
}
