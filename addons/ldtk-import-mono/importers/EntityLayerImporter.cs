#if TOOLS

using System.Linq;
using Godot;
using Picalines.Godot.LDtkImport.Json;

namespace Picalines.Godot.LDtkImport.Importers
{
    internal static class EntityLayerImporter
    {
        public static Node2D Import(string entityPathTemplate, WorldJson worldJson, LevelJson.LayerInstance layer)
        {
            var entitiesLayer = CreateEntityLayerNode(worldJson, layer);

            AddEntities(entityPathTemplate, layer, entitiesLayer);

            return entitiesLayer;
        }

        private static Node2D CreateEntityLayerNode(WorldJson worldJson, LevelJson.LayerInstance layer)
        {
            var useYSort = worldJson.Definitions.Layers.First(l => l.Uid == layer.DefUid).RequiredTags.Contains(nameof(YSort));
            var entitiesLayer = useYSort ? new YSort() : new Node2D();

            entitiesLayer.Name = layer.Identifier;

            return entitiesLayer;
        }

        private static void AddEntities(string entityPathTemplate, LevelJson.LayerInstance layer, Node2D entitiesLayer)
        {
            LDtkFieldAssigner.Initialize();

            foreach (var entityInstance in layer.EntityInstances)
            {
                var scenePath = LevelImportPlugin.GetEntityScenePath(entityPathTemplate, entityInstance);

                var entity = TryInstanceEntityScene(entityInstance, scenePath);

                if (entity is not null)
                {
                    entitiesLayer.AddChild(entity);
                }
            }
        }

        private static Node? TryInstanceEntityScene(LevelJson.EntityInstance entityJson, string scenePath)
        {
            using (var file = new File())
            {
                if (!file.FileExists(scenePath))
                {
                    GD.PushWarning($"{scenePath}: file not found. Entity '{entityJson.Identifier}' is ignored");
                    return null;
                }
            }

            var sceneInstance = GD.Load<PackedScene>(scenePath).Instance<Node>();

            foreach (Node child in sceneInstance.GetChildren())
            {
                child.Free();
            }

            if (sceneInstance is Node2D node2D)
            {
                node2D.Position = entityJson.PxCoords;
            }

            LDtkFieldAssigner.Assign(sceneInstance, entityJson);

            return sceneInstance;
        }
    }
}

#endif
