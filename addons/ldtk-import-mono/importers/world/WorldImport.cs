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
            WorldJson = WorldJson.Load(ImportContext.SourceFile)
        };

        protected override Node2D BuildScene()
        {
            CreateDir();

            ImportTileSets();

            Node2D world = new()
            {
                Name = ImportContext.SourceFile.BaseName()[ImportContext.SourceFile.GetBaseDir().Length..]
            };

            PlaceLevels(world);

            return world;
        }

        private void PlaceLevels(Node2D world)
        {
            Func<Node2D, LevelJson.Root, Node2D> prepareLevel = UsedExtension is not null
                ? UsedExtension.PrepareLevel
                : (node, _) => node;

            foreach (LevelJson.Root levelJson in SceneContext.WorldJson.Levels)
            {
                if (levelJson.ExternalRelPath is null)
                {
                    throw new NotImplementedException($"please turn on the 'Save levels separately' project option ({ImportContext.SourceFile})");
                }

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

                node = prepareLevel(node, levelJson);

                world.AddChild(node);
                node.Owner = world;
            }
        }

        private void CreateDir()
        {
            using var dir = new Directory();

            string baseDir = ImportContext.SourceFile.BaseName();

            if (!dir.DirExists(baseDir))
            {
                dir.MakeDir(baseDir);
            }
        }

        private void ImportTileSets()
        {
            foreach (WorldJson.TileSetDef tileSet in SceneContext.WorldJson.Defs.Tilesets)
            {
                if (TileSetImport.Import(tileSet, ImportContext.SourceFile) != Error.Ok)
                {
                    throw new Exception($"failed to import tileset '{tileSet.Identifier}'");
                }
            }
        }
    }
}
