using System.ComponentModel;

namespace Silksong.DataManager;

/// Interface for mods that need to store data scoped to a BepInEx profile.
public interface IProfileDataMod<T> : IProfileDataMod
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
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IProfileDataMod
{
    /// The target type to use when deserializing save data for this mod.
    System.Type ProfileDataType { get; }

    /// The object (of type <see cref="ProfileDataType"/>) to be serialized.
    object? UntypedProfileData { get; set; }
}
