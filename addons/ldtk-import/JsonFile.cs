#if TOOLS

using Godot;
using Newtonsoft.Json;

namespace Picalines.Godot.LDtkImport
{
    internal static class JsonFile
    {
        public static T? TryParse<T>(string path) where T : class
        {
            using var file = new File();

            try
            {
                var openError = file.Open(path, File.ModeFlags.Read);

                if (openError is not Error.Ok)
                {
                    GD.PushError(ErrorMessage.FailedToOpenFile(openError));
                    return null;
                }

                return JsonConvert.DeserializeObject<T>(file.GetAsText());
            }
            catch (JsonSerializationException exception)
            {
                GD.PushError(ErrorMessage.FailedToParseJsonFile(path, exception.Message));
                return null;
            }
            finally
            {
                file.Close();
            }
        }
    }
}

#endif
