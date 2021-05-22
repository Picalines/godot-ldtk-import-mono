#if TOOLS

using System.Linq;
using LDtkImport.Json;
using Godot.Collections;
using Godot;

namespace LDtkImport.Importers
{
    [Tool]
    public class LevelImport : SceneImport<Node2D, LevelImportContext, LevelImportExtension>
    {
        public override string GetImporterName() => "ldtk.level";
        public override string GetVisibleName() => "Level Importer";

        public override Array GetRecognizedExtensions() => new() { "ldtkl" };

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
            using var entityLayerImporter = new EntityLayerImporter()
            {
                SceneContext = SceneContext,
                UsedExtension = UsedExtension,
            };

            foreach (var layer in SceneContext.LevelJson.LayerInstances!.Reverse())
            {
                Node layerNode = layer.Type == LayerType.Entities
                    ? entityLayerImporter.Import(layer)
                    : TileMapLayerImporter.Import(ImportContext, SceneContext, layer);

                levelNode.AddChild(layerNode);
            }
        }
    }
}

#endif
