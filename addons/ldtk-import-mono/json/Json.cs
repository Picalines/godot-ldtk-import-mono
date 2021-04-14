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
                    throw new System.Exception($"File not found at {path}");
                }

                jsonText = file.GetAsText();
            }

            return JsonConvert.DeserializeObject<Root>(jsonText);
        }
    }
}
