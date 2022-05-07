using Godot;
using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace Picalines.Godot.LDtkImport.Json;

public static class JsonLoader
{
    public static T Load<T>(string path)
    {
        using var file = new File();

        if (file.Open(path, File.ModeFlags.Read) != Error.Ok)
        {
            throw new System.IO.FileNotFoundException($"json file not found at {path}");
        }

        var jsonText = file.GetAsText();

        try
        {
            return JsonConvert.DeserializeObject<T>(jsonText) ?? throw new NullReferenceException();
        }
        catch (Exception exception)
        {
            throw new SerializationException($"invalid json file {path}", exception);
        }
    }
}
