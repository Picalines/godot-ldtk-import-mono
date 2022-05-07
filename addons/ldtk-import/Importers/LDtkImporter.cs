#if TOOLS

using Godot;
using Picalines.Godot.LDtkImport.Json;
using Picalines.Godot.LDtkImport.Utils;

namespace Picalines.Godot.LDtkImport.Importers
{
    [Tool]
    public static class LDtkImporter
    {
        public static bool Import(string ldtkFilePath, string settingsFilePath, out string? outputDirectory)
        {
            var worldJson = JsonFile.TryParse<WorldJson>(ldtkFilePath);
            var settings = JsonFile.TryParse<LDtkImportSettings>(settingsFilePath);

            if (worldJson is null || settings is null)
            {
                outputDirectory = null;
                return false;
            }

            using var outputDir = new Directory();
            outputDir.MakeDirRecursive(settings.OutputDirectory);

            ImportTileSets(ldtkFilePath, settings.OutputDirectory, worldJson);

            // TODO: actual import.

            outputDirectory = settings.OutputDirectory;
            return true;
        }

        private static void ImportTileSets(string ldtkFilePath, string outputDir, WorldJson worldJson)
        {
            var tileSetsDir = outputDir + "tilesets";

            using var dir = new Directory();
            dir.MakeDir(tileSetsDir);

            foreach (var tileSetJson in worldJson.Definitions.TileSets)
            {
                var tileSet = TileSetImporter.Import(tileSetJson, ldtkFilePath);

                var savePath = $"{tileSetsDir}/{tileSetJson.Identifier}.tres";

                if (ResourceSaver.Save(savePath, tileSet) is not Error.Ok)
                {
                    GD.PushError(ErrorMessage.FailedToImportTileSet(ldtkFilePath, tileSetJson.Identifier));
                }
            }
        }
    }
}

#endif
