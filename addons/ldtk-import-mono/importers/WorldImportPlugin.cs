#if TOOLS

using Godot;
using Picalines.Godot.LDtkImport.Json;
using GDArray = Godot.Collections.Array;
using GDDictionary = Godot.Collections.Dictionary;

namespace Picalines.Godot.LDtkImport.Importers
{
    [Tool]
    public sealed class WorldImportPlugin : EditorSceneImportPlugin
    {
        public const string TileSetsDirectory = "tilesets";

        private const string BaseScenePath = "BaseScenePath";
        private const string LevelsParentName = "LevelsParentName";

        public override string GetImporterName() => "ldtk.world";
        public override string GetVisibleName() => "LDtk World";

        public override GDArray GetRecognizedExtensions() => new() { "ldtk" };

        public override GDArray GetImportOptions(int preset) => new()
        {
            new GDDictionary()
            {
                ["name"] = BaseScenePath,
                ["default_value"] = "",
            },
            new GDDictionary()
            {
                ["name"] = LevelsParentName,
                ["default_value"] = "Levels",
            }
        };

        protected override Node ImportScene(string sourceFile, string savePath, GDDictionary options, GDArray platformVariants, GDArray genFiles)
        {
            CheckRequiredDirs(sourceFile);

            var worldJson = JsonLoader.Load<WorldJson>(sourceFile);

            ImportTileSets(sourceFile, genFiles, worldJson);

            var worldNode = CreateWorldScene(options);

            worldNode.Name = sourceFile.BaseName().Substring(sourceFile.GetBaseDir().Length);

            PlaceLevels(sourceFile, options, worldJson, worldNode);

            return worldNode;
        }

        private void PlaceLevels(string sourceFile, GDDictionary options, WorldJson worldJson, Node2D worldNode)
        {
            if (!worldJson.ExternalLevels)
            {
                GD.PushError($"{sourceFile}: please turn on the 'Save levels separately' project option");
                return;
            }

            var levelsParent = new Node2D
            {
                Name = options[LevelsParentName].ToString()
            };

            worldNode.AddChild(levelsParent);
            levelsParent.Owner = worldNode;

            foreach (LevelJson levelJson in worldJson.Levels)
            {
                var path = $"{sourceFile.GetBaseDir()}/{levelJson.ExternalRelPath}";
                var scene = GD.Load<PackedScene>(path);
                var instance = scene.Instance();

                if (instance is not Node2D levelNode)
                {
                    GD.PushError($"scene of type {nameof(Node2D)} expected at {path}");
                    return;
                }

                foreach (Node child in instance.GetChildren())
                {
                    child.Free();
                }

                levelNode.Position = levelJson.WorldPos;

                levelsParent.AddChild(levelNode);
                levelNode.Owner = worldNode;
            }
        }

        private void ImportTileSets(string sourceFile, GDArray genFiles, WorldJson worldJson)
        {
            var tileSetsDir = $"{sourceFile.BaseName()}/{TileSetsDirectory}";

            foreach (var tileSetJson in worldJson.Definitions.TileSets)
            {
                var savePath = $"{tileSetsDir}/{tileSetJson.Identifier}.tres";

                TileSet tileSet;

                using (var file = new File())
                {
                    if (file.FileExists(savePath))
                    {
                        tileSet = GD.Load<TileSet>(savePath);
                        TileSetImporter.ApplyChanges(tileSet, tileSetJson, sourceFile);
                        continue;
                    }
                }

                tileSet = TileSetImporter.CreateNew(tileSetJson, sourceFile);

                if (ResourceSaver.Save(savePath, tileSet) != Error.Ok)
                {
                    GD.PushError($"failed to import tileset '{tileSetJson.Identifier}'");
                    continue;
                }

                genFiles.Add(savePath);
            }
        }

        private static Node2D CreateWorldScene(GDDictionary options)
        {
            var baseScenePath = options[BaseScenePath].ToString();

            if (baseScenePath != "")
            {
                var packedBaseScene = GD.Load<PackedScene>(baseScenePath);
                return packedBaseScene.Instance<Node2D>(PackedScene.GenEditState.Disabled);
            }

            return new Node2D();
        }

        private void CheckRequiredDirs(string sourceFile)
        {
            using var dir = new Directory();
            var baseDir = sourceFile.BaseName();
            dir.MakeDirRecursive($"{baseDir}/{TileSetsDirectory}");
        }
    }
}

#endif
