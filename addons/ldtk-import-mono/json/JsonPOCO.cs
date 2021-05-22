using System;
using System.Runtime.Serialization;
using Godot;
using Newtonsoft.Json;

namespace LDtkImport.Json
{
    public abstract class JsonPOCO<T> where T : JsonPOCO<T>
    {
        public static T Load(string path)
        {
            string jsonText;

            using (var file = new File())
            {
                var openError = file.Open(path, File.ModeFlags.Read);
                if (openError != Error.Ok)
                {
                    throw new Exception($"File not found at {path}");
                }

                jsonText = file.GetAsText();
            }

            T? result;

            try
            {
                result = JsonConvert.DeserializeObject<T>(jsonText);
                if (result is null)
                {
                    throw new NullReferenceException($"{nameof(JsonConvert.DeserializeObject)} returned null");
                }
            }
            catch (Exception exception)
            {
                throw new SerializationException($"Error on parsing json at '{path}' ({exception.GetType().FullName}): {exception.Message}", exception);
            }

            return result;
        }
    }
}
