#if TOOLS

using Godot;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Picalines.Godot.LDtkImport.Json.Converters
{
    internal class Vector2Converter : JsonConverter<Vector2>
    {
        public override Vector2 ReadJson(JsonReader reader, Type objectType, Vector2 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var jArray = JArray.Load(reader);

            if (jArray.Count != 2)
            {
                throw new Exception($"{nameof(Vector2)} json array contains {jArray.Count} elements ({reader.Path})");
            }

            return new Vector2(
                jArray[0].ToObject<float>(),
                jArray[1].ToObject<float>()
            );
        }

        public override void WriteJson(JsonWriter writer, Vector2 value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}

#endif
