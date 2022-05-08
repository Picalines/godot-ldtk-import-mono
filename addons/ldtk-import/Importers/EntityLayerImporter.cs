#if TOOLS

using Godot;
using Picalines.Godot.LDtkImport.Json;
using System.Collections.Generic;
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

        public static Node? TryInstantiate(LevelImportContext context, string entityName, IEnumerable<KeyValuePair<string, object>> fieldValues)
        {
            var possiblePaths = context.ImportSettings.GetPossibleEntityPaths(entityName);

            var scenePath = possiblePaths.FirstOrDefault(path => ResourceLoader.Exists(path, nameof(PackedScene)));

            if (scenePath is null)
            {
                return null;
            }

            var entityPackedScene = GD.Load<PackedScene>(scenePath);
            var entityNode = entityPackedScene.Instance<Node>();

            foreach (var pair in fieldValues)
            {
                // TODO: LDtkFieldAttribute
                entityNode.Set(pair.Key, pair.Value);
            }

            return entityNode;
        }

        private static void AddEntities(LevelImportContext context, LevelJson.LayerInstance layerJson, Node2D layerNode)
        {
            foreach (var entityInstance in layerJson.EntityInstances)
            {
                var fieldValues = entityInstance.FieldInstances
                    .Select(fieldInstance => new KeyValuePair<string, object>(fieldInstance.Identifier, fieldInstance.Value));

                var entityNode = TryInstantiate(context, entityInstance.Identifier, fieldValues);

                entityNode ??= new Position2D() { Name = entityInstance.Identifier };

                if (entityNode is Node2D entity2D)
                {
                    entity2D.Position = entityInstance.PxCoords;
                }

                layerNode.AddChild(entityNode);
            }
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