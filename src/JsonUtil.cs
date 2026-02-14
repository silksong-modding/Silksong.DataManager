using IO = System.IO;
using NJson = Newtonsoft.Json;

namespace Silksong.DataManager.Json;

// TODO(UserIsntAvailable): Make `public`?

internal static class JsonUtil
{
    private static readonly NJson.JsonSerializerSettings _settings = new()
    {
        TypeNameHandling = NJson.TypeNameHandling.Auto,
        ObjectCreationHandling = NJson.ObjectCreationHandling.Replace,
    };

    internal static T? Deserialize<T>(IO.StreamReader s)
    {
        using var reader = new NJson.JsonTextReader(s);
        var ser = NJson.JsonSerializer.CreateDefault(_settings);
        return ser.Deserialize<T>(reader);
    }

    internal static object? Deserialize(string path, System.Type type)
    {
        using var file = IO.File.OpenText(path);
        using var reader = new NJson.JsonTextReader(file);
        var ser = NJson.JsonSerializer.CreateDefault(_settings);
        return ser.Deserialize(reader, type);
    }

    internal static void Serialize(IO.StreamWriter s, object value)
    {
        using var writer = new NJson.JsonTextWriter(s);
        var ser = NJson.JsonSerializer.CreateDefault(_settings);
        ser.Serialize(writer, value);
    }

    internal static void Serialize(string path, object value)
    {
        using var file = IO.File.CreateText(path);
        Serialize(file, value);
    }
}
