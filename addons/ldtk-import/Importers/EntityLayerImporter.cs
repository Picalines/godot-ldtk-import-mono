#if TOOLS

using Godot;
using Picalines.Godot.LDtkImport.Json;
using System.Linq;

namespace Picalines.Godot.LDtkImport.Importers
{
    internal static class EntityLayerImporter
    {
        public static Node Import(LevelImportContext context, LevelJson.LayerInstance layerJson)
        {
            var layerNode = CreateEntityLayer(context, layerJson);

            AddEntities(context, layerJson, layerNode);

            return layerNode;
        }

        private static void AddEntities(LevelImportContext context, LevelJson.LayerInstance layerJson, Node2D layerNode)
        {
            foreach (var entityInstance in layerJson.EntityInstances)
            {
                var scenePath = context.ImportSettings.GetEntityScenePath(entityInstance.Identifier);

                var entityNode = scenePath is not null ? TryInstantiateEntity(entityInstance, scenePath) : null;

                entityNode ??= new Position2D() { Name = entityInstance.Identifier };

                if (entityNode is Node2D entity2D)
                {
                    entity2D.Position = entityInstance.PxCoords;
                }

                layerNode.AddChild(entityNode);
            }
        }

        private static Node? TryInstantiateEntity(LevelJson.EntityInstance entityInstance, string scenePath)
        {
            if (!ResourceLoader.Exists(scenePath, nameof(PackedScene)))
            {
                return null;
            }

            var entityPackedScene = GD.Load<PackedScene>(scenePath);
            var entityNode = entityPackedScene.Instance<Node>();

            entityNode.SetScript(null!);

            // TODO: assign fields

            return entityNode;
        }

        private static Node2D CreateEntityLayer(LevelImportContext context, LevelJson.LayerInstance layerJson)
        {
            var layerDefinition = context.WorldJson.Definitions.Layers
                .First(layer => layer.Uid == layerJson.LayerDefUid);

            var layerNode = layerDefinition.RequiredEntityTags!.Contains("YSort")
                ? new YSort() : new Node2D();

            layerNode.Name = layerJson.Identifier;
            return layerNode;
        }
    }
}

#endif