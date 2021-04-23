using System;
using Godot;
using Newtonsoft.Json;

namespace LDtkImport.Json
{
    public abstract class BaseJson<Root> where Root : class
    {
        public static Root Load(string path)
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

            Root? result;

            try
            {
                result = JsonConvert.DeserializeObject(jsonText, typeof(Root)) as Root;
                if (result is null)
                {
                    throw new NullReferenceException();
                }
            }
            catch (Exception exception)
            {
                throw new Exception($"error on parsing json at '{path}' ({exception.GetType().FullName}): {exception.Message}", exception);
            }

            return result;
        }
    }
}
