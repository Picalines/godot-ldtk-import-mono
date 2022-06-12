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

        public static Node? TryInstantiate(LevelImportContext context, string entityName)
        {
            var possiblePaths = context.ImportSettings.GetPossibleEntityPaths(entityName);

            var scenePath = possiblePaths.FirstOrDefault(path => ResourceLoader.Exists(path, nameof(PackedScene)));

            if (scenePath is null)
            {
                return null;
            }

            var entityPackedScene = GD.Load<PackedScene>(scenePath);
            var entityNode = entityPackedScene.Instance<Node>();

            return entityNode;
        }

        private static void AddEntities(LevelImportContext context, LevelJson.LayerInstance layerJson, Node2D layerNode)
        {
            var referenceAssigner = CreateReferenceAssigner();

            layerNode.AddChild(referenceAssigner);

            foreach (var entityInstance in layerJson.EntityInstances)
            {
                var entityNode = TryInstantiate(context, entityInstance.Identifier);

                bool instantiated = entityNode is not null;

                entityNode ??= new Position2D() { Name = entityInstance.Identifier };

                entityNode.SetMeta(LDtkEntityReferenceAssigner.InstanceIdMetaKey, entityInstance.Id);
                entityNode.AddToGroup(LDtkEntityReferenceAssigner.EntitiesGroupName, persistent: true);

                if (instantiated)
                {
                    LDtkFieldAssigner.Assign(entityNode, entityInstance.FieldInstances, new()
                    {
                        GridSize = layerJson.GridSize,
                        ReferenceAssigner = referenceAssigner,
                    });
                }

                if (entityNode is Node2D entity2D)
                {
                    var pivotOffset = (Vector2.One / 2 - entityInstance.Pivot) * entityInstance.Size;
                    entity2D.Position = entityInstance.PxCoords + pivotOffset;
                }

                layerNode.AddChild(entityNode);
            }

            referenceAssigner.Serialize();
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

        private static LDtkEntityReferenceAssigner CreateReferenceAssigner()
        {
            var packedScene = GD.Load<PackedScene>("res://addons/ldtk-import/LDtkEntityReferenceAssigner.tscn");
            return packedScene.Instance<LDtkEntityReferenceAssigner>();
        }
    }
}

#endif