#if TOOLS

using Godot;
using Picalines.Godot.LDtkImport.Json;
using System.Collections.Generic;
using System.Linq;

namespace Picalines.Godot.LDtkImport.Importers
{
    internal static class EntityLayerImporter
    {
        private static PackedScene? _ReferenceAssignerScene = null;

        public static void Import(LevelImportContext context, LevelJson.LayerInstance layerJson, Node layerNode)
        {
            var referenceAssigner = CreateReferenceAssigner();

            foreach (var entityInstance in layerJson.EntityInstances)
            {
                var entityNode = TryInstantiate(context, entityInstance.Identifier);

                bool instantiated = entityNode is not null;

                entityNode ??= new Position2D() { Name = entityInstance.Identifier };

                entityNode.SetMeta(LDtkConstants.MetaKeys.InstanceId, entityInstance.Id);
                entityNode.AddToGroup(LDtkConstants.GroupNames.Entities, persistent: true);

                layerNode.AddChild(entityNode);

                if (instantiated)
                {
                    var entityFields = entityInstance.FieldInstances
                        .Select(field => new KeyValuePair<string, object>(field.Identifier, field.Value))
                        .Append(new(LDtkConstants.SpecialFieldNames.Size, entityInstance.Size))
                        .ToDictionary(pair => pair.Key, pair => pair.Value);

                    LDtkFieldAssigner.Assign(entityNode, entityFields, new()
                    {
                        GridSize = layerJson.GridSizeV,
                        ReferenceAssigner = referenceAssigner,
                    });
                }

                if (entityNode is Node2D entity2D)
                {
                    var pivotOffset = (Vector2.One / 2 - entityInstance.Pivot) * entityInstance.Size;
                    entity2D.Position = entityInstance.PxCoords + pivotOffset;
                }
            }

            if (referenceAssigner.IsUsed)
            {
                layerNode.AddChild(referenceAssigner);
                layerNode.MoveChild(referenceAssigner, 0);
            }
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

        private static LDtkEntityReferenceAssigner CreateReferenceAssigner()
        {
            _ReferenceAssignerScene ??= GD.Load<PackedScene>("res://addons/ldtk-import/LDtkEntityReferenceAssigner.tscn");
            return _ReferenceAssignerScene.Instance<LDtkEntityReferenceAssigner>();
        }
    }
}

#endif