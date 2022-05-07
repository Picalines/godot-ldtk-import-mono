#if TOOLS

using Godot;
using Picalines.Godot.LDtkImport.Json;

namespace Picalines.Godot.LDtkImport.Importers
{
    internal class LevelImporter
    {
        public static void Import(string ldtkFilePath, LDtkImportSettings importSettings, LevelJson levelJson)
        {
            var levelNode = new Node2D()
            {
                Name = levelJson.Identifier,
            };

            AddLayers(ldtkFilePath, importSettings, levelJson, levelNode);

            var savePath = $"{importSettings.OutputDirectory}{levelJson.Identifier}.tscn";

            var packedLevelScene = new PackedScene();
            packedLevelScene.Pack(levelNode);

            if (ResourceSaver.Save(savePath, packedLevelScene) is not Error.Ok)
            {
                throw LDtkImportException.FailedToImportLevel(ldtkFilePath, levelJson.Identifier);
            }
        }

        private static void AddLayers(string ldtkFilePath, LDtkImportSettings importSettings, LevelJson levelJson, Node2D levelNode)
        {
            foreach (var layer in levelJson.LayerInstances!)
            {
                var layerNode = layer.Type switch
                {
                    LayerType.Entities => EntityLayerImporter.Import(ldtkFilePath, importSettings, layer),
                    _ => TileMapImporter.Import(ldtkFilePath, importSettings, layer),
                };

                layerNode.Owner = levelNode;

                foreach (Node child in levelNode.GetChildren())
                {
                    child.Owner = levelNode;
                }

                levelNode.AddChild(layerNode);
            }
        }
    }
}

#endif