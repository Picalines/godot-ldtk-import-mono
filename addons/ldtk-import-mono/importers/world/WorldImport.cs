#if TOOLS

using System;
using Godot;
using Godot.Collections;
using LDtkImport.Json;
using GDArray = Godot.Collections.Array;

namespace LDtkImport.Importers
{
    [Tool]
    public class WorldImport : SceneImport<Node2D, WorldImportContext, WorldImportExtension>
    {
        public override string GetImporterName() => "ldtk.world";
        public override string GetVisibleName() => "World Importer";

        public override GDArray GetRecognizedExtensions() => new() { "ldtk" };

        public override int GetPresetCount() => 1;
        public override string GetPresetName(int preset) => "default";

        public override bool GetOptionVisibility(string option, Dictionary options) => true;

        protected override WorldImportContext GetContext() => new()
        {
            WorldJson = JsonLoader.Load<WorldJson>(ImportContext.SourceFile)
        };

        protected override Node2D BuildScene()
        {
            CheckRequiredDirs();

            ImportTileSets();

            Node2D world = new()
            {
                Name = ImportContext.SourceFile.BaseName().Substring(ImportContext.SourceFile.GetBaseDir().Length)
            };

            PlaceLevels(world);

            return world;
        }

        private void PlaceLevels(Node2D world)
        {
            if (!SceneContext.WorldJson.ExternalLevels)
            {
                throw new NotImplementedException($"please turn on the 'Save levels separately' project option ({ImportContext.SourceFile})");
            }

            foreach (LevelJson levelJson in SceneContext.WorldJson.Levels)
            {
                var path = $"{ImportContext.SourceFile.GetBaseDir()}/{levelJson.ExternalRelPath}";
                var scene = GD.Load<PackedScene>(path);
                var instance = scene.Instance();

                if (instance is not Node2D node)
                {
                    throw new Exception($"scene of type {nameof(Node2D)} expected at {path}");
                }

                foreach (Node child in instance.GetChildren())
                {
                    child.Free();
                }

                node.Position = levelJson.WorldPos;

                world.AddChild(node);
                node.Owner = world;
            }
        }

        private void CheckRequiredDirs()
        {
            using var dir = new Directory();
            var baseDir = ImportContext.SourceFile.BaseName();
            dir.MakeDirRecursive(baseDir + "/tilesets");
        }

        private void ImportTileSets()
        {
            string tileSetsDir = ImportContext.SourceFile.BaseName() + "/tilesets";

            foreach (var tileSetJson in SceneContext.WorldJson.Definitions.TileSets)
            {
                var tileSet = TileSetImporter.Import(tileSetJson, ImportContext.SourceFile);

                UsedExtension?.PrepareTileSet(tileSet, tileSetJson);

                var savePath = $"{tileSetsDir}/{tileSetJson.Identifier}.tres";

                var saveResult = ResourceSaver.Save(savePath, tileSet);
                if (saveResult != Error.Ok)
                {
                    throw new Exception($"failed to import tileset '{tileSetJson.Identifier}'");
                }

                ImportContext.GenFiles.Add(savePath);
            }
        }
    }
}

#endif
