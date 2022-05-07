#if TOOLS

using Godot;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Picalines.Godot.LDtkImport.Json.Converters
{
    internal class Rect2Converter : JsonConverter<Rect2>
    {
        public override Rect2 ReadJson(JsonReader reader, Type objectType, Rect2 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var jArray = JArray.Load(reader);

            if (jArray.Count != 4)
            {
                throw new Exception($"{nameof(Rect2)} json array contains {jArray.Count} elements ({reader.Path})");
            }

            return new Rect2(
                new Vector2(
                    jArray[0].ToObject<float>(),
                    jArray[1].ToObject<float>()
                ),
                new Vector2(
                    jArray[2].ToObject<float>(),
                    jArray[3].ToObject<float>()
                )
            );
        }

        public override void WriteJson(JsonWriter writer, Rect2 value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}

#endif