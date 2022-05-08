#if TOOLS

using Godot;
using Picalines.Godot.LDtkImport.Json;

namespace Picalines.Godot.LDtkImport.Importers
{
    internal static class LevelImporter
    {
        public static void Import(LevelImportContext context)
        {
            var levelNode = new Node2D()
            {
                Name = context.LevelJson.Identifier,
            };

            AddLayers(context, levelNode);

            SaveScene(context, levelNode);
        }

        private static void AddLayers(LevelImportContext context, Node2D levelNode)
        {
            foreach (var layer in context.LevelJson.LayerInstances!)
            {
                var layerContext = new LayerImportContext(
                    context.LDtkFilePath,
                    context.ImportSettings,
                    context.WorldJson,
                    context.LevelJson,
                    layer
                );

                var layerNode = layer.Type switch
                {
                    LayerType.Entities => EntityLayerImporter.Import(context, layer),
                    _ => TileMapImporter.Import(context, layer),
                };

                levelNode.AddChild(layerNode);

                layerNode.Owner = levelNode;
                foreach (Node child in layerNode.GetChildren())
                {
                    child.Owner = levelNode;
                }
            }
        }

        private static void SaveScene(LevelImportContext context, Node2D levelNode)
        {
            var savePath = $"{context.ImportSettings.OutputDirectory}/{context.LevelJson.Identifier}.tscn";

            var packedLevelScene = new PackedScene();
            packedLevelScene.Pack(levelNode);

            if (ResourceSaver.Save(savePath, packedLevelScene) is not Error.Ok)
            {
                throw LDtkImportException.FailedToImportLevel(context.LDtkFilePath, context.LevelJson.Identifier);
            }
        }
    }
}

#endif