#if TOOLS

using Newtonsoft.Json;
using System;

namespace Picalines.Godot.LDtkImport.Json.Converters
{
    internal class LevelNeighbourDirectionConverter : JsonConverter<LevelNeighbourDirection>
    {
        public override LevelNeighbourDirection ReadJson(JsonReader reader, Type objectType, LevelNeighbourDirection existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return (string)reader.Value! switch
            {
                "n" => LevelNeighbourDirection.North,
                "s" => LevelNeighbourDirection.South,
                "w" => LevelNeighbourDirection.West,
                "e" => LevelNeighbourDirection.East,
                _ => throw new JsonException(),
            };
        }

        public override void WriteJson(JsonWriter writer, LevelNeighbourDirection value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}

#endif