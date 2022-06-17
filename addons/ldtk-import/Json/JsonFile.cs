#if TOOLS

using Godot;
using Newtonsoft.Json;

namespace Picalines.Godot.LDtkImport.Json
{
    internal static class JsonFile
    {
        public static T Parse<T>(string path) where T : class
        {
            using var file = new File();

            try
            {
                var openError = file.Open(path, File.ModeFlags.Read);

                if (openError is not Error.Ok)
                {
                    throw new LDtkImportException(LDtkImportMessage.FailedToOpenFile(openError));
                }

                return JsonConvert.DeserializeObject<T>(file.GetAsText());
            }
            catch (JsonSerializationException exception)
            {
                throw new LDtkImportException(LDtkImportMessage.FailedToParseJsonFile(path, exception.Message));
            }
            finally
            {
                file.Close();
            }
        }
    }
}

#endif