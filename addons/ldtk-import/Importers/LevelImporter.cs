#if TOOLS

using Godot;
using Picalines.Godot.LDtkImport.Json;

namespace Picalines.Godot.LDtkImport.Importers
{
    internal static class LevelImporter
    {
        public static void Import(LevelImportContext context)
        {
            Node2D levelNode;

            if (context.ImportSettings.LevelSceneSettings?.BaseScenePath is string baseScenePath)
            {
                // NOTE: actual scene inheritance through code is impossible in Godot.
                // https://github.com/godotengine/godot-proposals/issues/3907
                var basePackedScene = GD.Load<PackedScene>(baseScenePath);
                levelNode = basePackedScene.Instance<Node2D>();
            }
            else
            {
                levelNode = new();
            }

            levelNode.Name = context.LevelJson.Identifier;

            AddLayers(context, levelNode);

            SaveScene(context, levelNode);
        }

        private static void AddLayers(LevelImportContext context, Node2D levelNode)
        {
            Node layersParent = levelNode;

            if (context.ImportSettings.LevelSceneSettings?.LayersParentNodeName is string layersParentNodeName)
            {
                layersParent = levelNode.GetNodeOrNull(layersParentNodeName)
                    ?? new Node2D() { Name = layersParentNodeName };

                layersParent.Owner = levelNode;
            }

            foreach (var layer in context.LevelJson.LayerInstances!)
            {
                var layerNode = layer.Type switch
                {
                    LayerType.Entities => EntityLayerImporter.Import(context, layer),
                    _ => TileMapImporter.Import(context, layer),
                };

                layersParent.AddChild(layerNode);

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