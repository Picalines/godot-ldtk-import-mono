#if TOOLS

using Godot;
using Newtonsoft.Json;
using System;

namespace Picalines.Godot.LDtkImport.Json.Converters
{
    internal class ColorConverter : JsonConverter<Color>
    {
        public override Color ReadJson(JsonReader reader, Type objectType, Color existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return new Color((string)reader.Value!);
        }

        public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}

#endif