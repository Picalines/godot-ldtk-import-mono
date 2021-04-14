using System.Linq;
using LDtkImport.Json;
using Godot.Collections;
using Godot;

namespace LDtkImport.Importers
{
    [Tool]
    public class LevelImport : SceneImport<Node2D, LevelImportContext>
    {
        public override string GetImporterName() => "ldtk.level";
        public override string GetVisibleName() => "Level Importer";

        public override Array GetRecognizedExtensions() => new() { "ldtkl" };

        public override int GetPresetCount() => 1;
        public override string GetPresetName(int preset) => "default";

        protected override LevelImportContext GetContext() => new(ImportContext)
        {
            WorldJson = WorldJson.Load(ImportContext.SourceFile.GetBaseDir() + ".ldtk"),
            LevelJson = LevelJson.Load(ImportContext.SourceFile),
        };

        protected override Node2D BuildScene(LevelImportContext context)
        {
            var levelNode = new Node2D()
            {
                Name = context.LevelJson.Identifier
            };

            foreach (var layer in context.LevelJson.LayerInstances.Reverse())
            {
                Node layerNode = layer.Type switch
                {
                    WorldJson.LayerType.IntGrid => TileMapLayerImporter.Import(context, layer),
                    WorldJson.LayerType.Entities => EntitiesLayerImporter.Import(layer, UsedExtension as LevelImportExtension),
                    _ => new Node { Name = layer.Identifier },
                };

                levelNode.AddChild(layerNode);
            }

            return levelNode;
        }
    }
}
