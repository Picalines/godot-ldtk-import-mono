using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Picalines.Godot.LDtkImport.Json.Converters
{
    internal class FieldInstanceValueConverter : JsonConverter
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var defaultValue = serializer.Deserialize(reader, objectType);

            if (defaultValue is JArray jArray)
            {
                defaultValue = jArray.ToObject<object[]>();
            }

            return defaultValue;
        }

        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
