using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Picalines.Godot.LDtkImport.Json.Converters
{
    internal class FieldInstanceValueConverter : JsonConverter
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var defaultValue = serializer.Deserialize(reader, objectType);

            if (defaultValue is JArray jArray)
            {
                var array = jArray.ToObject<object[]>();

                for (int i = 0; i < array.Length; i++)
                {
                    if (array[i] is JObject jObjectItem)
                    {
                        array[i] = jObjectItem.ToObject<Dictionary<string, object>>();
                    }
                }

                defaultValue = array;
            }

            if (defaultValue is JObject jObject)
            {
                defaultValue = jObject.ToObject<Dictionary<string, object>>();
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
