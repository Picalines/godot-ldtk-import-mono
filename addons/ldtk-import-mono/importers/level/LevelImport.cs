#if TOOLS

using Godot;
using System;
using System.Linq;
using Picalines.Godot.LDtkImport.Json;
using GDArray = Godot.Collections.Array;

namespace Picalines.Godot.LDtkImport.Importers
{
    [Tool]
    public class LevelImport : SceneImport<Node2D, LevelImportContext, LevelImportExtension>
    {
        public override string GetImporterName() => "ldtk.level";
        public override string GetVisibleName() => "Level Importer";

        public override GDArray GetRecognizedExtensions() => new() { "ldtkl" };

        public override int GetPresetCount() => 1;
        public override string GetPresetName(int preset) => "default";

        protected override LevelImportContext GetContext() => new()
        {
            WorldJson = JsonLoader.Load<WorldJson>(ImportContext.SourceFile.GetBaseDir() + ".ldtk"),
            LevelJson = JsonLoader.Load<LevelJson>(ImportContext.SourceFile),
        };

        protected override Node2D BuildScene()
        {
            Node2D levelNode = new() { Name = SceneContext.LevelJson.Identifier };

            AddLayers(levelNode);

            return levelNode;
        }

        private void AddLayers(Node2D levelNode)
        {
            var tileMapImporter = new Lazy<TileMapLayerImporter>(() => new()
            {
                FileContext = ImportContext,
                SceneContext = SceneContext,
            });

            var entityLayerImporter = new Lazy<EntityLayerImporter>(() => new()
            {
                SceneContext = SceneContext,
                UsedExtension = UsedExtension,
            });

            foreach (var layer in SceneContext.LevelJson.LayerInstances!.Reverse())
            {
                Node layerNode = layer.Type == LayerType.Entities
                    ? entityLayerImporter.Value.Import(layer)
                    : tileMapImporter.Value.Import(layer);

                levelNode.AddChild(layerNode);
            }

            if (entityLayerImporter.IsValueCreated)
            {
                entityLayerImporter.Value.Dispose();
            }
        }
    }
}

#endif
