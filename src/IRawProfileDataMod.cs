using IO = System.IO;

namespace Silksong.DataManager;

/// Interface for mods that need to store save data and require
/// precise control over their serialization process.
/// Typical mods should implement <see cref="IProfileDataMod{T}"/>.
public interface IRawProfileDataMod
{
    /// If true, a data file will be created for this mod when closing the game.
    bool HasProfileData { get; }

    /// Writes the profile data for this mod.
    void WriteProfileData(IO.Stream saveFile);

    /// Called when the game starts up. If no data is present for this mod, saveFile is null.
    void ReadProfileData(IO.Stream? saveFile);
}
