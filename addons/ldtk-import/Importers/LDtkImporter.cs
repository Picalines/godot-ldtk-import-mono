#if TOOLS

using Godot;
using Picalines.Godot.LDtkImport.Json;

namespace Picalines.Godot.LDtkImport.Importers
{
    [Tool]
    public static class LDtkImporter
    {
        public static void Import(string ldtkFilePath, string settingsFilePath)
        {
            var worldJson = JsonFile.TryParse<WorldJson>(ldtkFilePath);
            var settings = JsonFile.TryParse<LDtkImportSettings>(settingsFilePath);

            if (worldJson is null || settings is null)
            {
                return;
            }

            using var outputDir = new Directory();
            outputDir.MakeDirRecursive(settings.OutputDirectory);

            // TODO: actual import.
        }
    }
}

#endif
