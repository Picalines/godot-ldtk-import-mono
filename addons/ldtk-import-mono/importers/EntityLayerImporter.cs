#if TOOLS

using Godot;
using Picalines.Godot.LDtkImport.Json;

namespace Picalines.Godot.LDtkImport.Importers
{
    public sealed class EntityLayerImporter
    {
        public const string EntityScenePathReplaceTarget = "$";

        private readonly string _EntityScenePath;

        public EntityLayerImporter(string entityScenePathOption)
        {
            _EntityScenePath = entityScenePathOption;
        }

        public Node2D Import(LevelJson.LayerInstance layer)
        {
            var entitiesLayer = CreateEntityLayerNode(layer);

            AddEntities(layer, entitiesLayer);

            return entitiesLayer;
        }

        private Node2D CreateEntityLayerNode(LevelJson.LayerInstance layer)
        {
            var useYSort = false; // TODO
            var entitiesLayer = useYSort ? new YSort() : new Node2D();

            entitiesLayer.Name = layer.Identifier;

            return entitiesLayer;
        }

        private void AddEntities(LevelJson.LayerInstance layer, Node2D entitiesLayer)
        {
            LDtkFieldAssigner.Initialize();

            foreach (var entityInstance in layer.EntityInstances)
            {
                var scenePath = GetEntityScenePath(entityInstance);

                var entity = TryInstanceEntityScene(entityInstance, scenePath);

                if (entity is not null)
                {
                    entitiesLayer.AddChild(entity);
                }
            }
        }

        private Node? TryInstanceEntityScene(LevelJson.EntityInstance entityJson, string scenePath)
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

            LDtkFieldAssigner.Assign(entityJson, sceneInstance);

            return sceneInstance;
        }

        private string GetEntityScenePath(LevelJson.EntityInstance entityInstance)
        {
            return _EntityScenePath.Replace(EntityScenePathReplaceTarget, entityInstance.Identifier);
        }
    }
}

#endif
