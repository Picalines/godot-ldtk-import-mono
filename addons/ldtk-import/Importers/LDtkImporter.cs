#if TOOLS

using Godot;
using Picalines.Godot.LDtkImport.Json;
using System;

namespace Picalines.Godot.LDtkImport.Importers
{
    internal static class LDtkImporter
    {
        public static void Import(string ldtkFilePath, string settingsFilePath, out string outputDirectory)
        {
            var worldJson = JsonFile.Parse<WorldJson>(ldtkFilePath);
            var settings = JsonFile.Parse<LDtkImportSettings>(settingsFilePath);

            using var outputDir = new Directory();
            outputDir.MakeDirRecursive(settings.OutputDirectory + "/tilesets");

            ImportTileSets(ldtkFilePath, settings.OutputDirectory, worldJson);

            LDtkFieldAssigner.Initialize();

            ImportLevels(ldtkFilePath, settings, worldJson);

            if (settings.WorldSceneSettings is not null)
            {
                WorldImporter.Import(ldtkFilePath, settings, worldJson);
            }

            outputDirectory = settings.OutputDirectory;
        }

        private static void ImportLevels(string ldtkFilePath, LDtkImportSettings importSettings, WorldJson worldJson)
        {
            Func<LevelJson, LevelJson> getLevelJson = worldJson.ExternalLevels
                ? levelInfo => JsonFile.Parse<LevelJson>($"{ldtkFilePath.GetBaseDir()}/{levelInfo.ExternalRelPath}")
                : fullLevel => fullLevel;

            foreach (var levelInfo in worldJson.Levels)
            {
                var levelJson = getLevelJson(levelInfo);

                LevelImporter.Import(new LevelImportContext(ldtkFilePath, importSettings, worldJson, levelJson));
            }
        }

        private static void ImportTileSets(string ldtkFilePath, string outputDir, WorldJson worldJson)
        {
            foreach (var tileSetJson in worldJson.Definitions.TileSets)
            {
                var savePath = $"{outputDir}/tilesets/{tileSetJson.Identifier}.tres";

                TileSet tileSet;

                if (ResourceLoader.Exists(savePath))
                {
                    tileSet = GD.Load<TileSet>(savePath);
                    TileSetImporter.ApplyChanges(ldtkFilePath, tileSetJson, tileSet);
                }
                else
                {
                    tileSet = TileSetImporter.CreateNew(ldtkFilePath, tileSetJson);
                }

                if (ResourceSaver.Save(savePath, tileSet) is not Error.Ok)
                {
                    throw LDtkImportException.FailedToImportTileSet(ldtkFilePath, tileSetJson.Identifier);
                }
            }
        }
    }
}

#endif
