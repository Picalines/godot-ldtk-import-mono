#if TOOLS

using Godot;
using System;
using Picalines.Godot.LDtkImport.Json;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace Picalines.Godot.LDtkImport.Importers
{
    public sealed class EntityLayerImporter
    {
        public const string EntityScenePathReplaceTarget = "$";

        private record LDtkFieldInfo(string EditorName, MemberInfo TargetMember);

        private readonly Dictionary<Type, IEnumerable<LDtkFieldInfo>> _CachedLDtkFields = new();

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
            foreach (var entityInstance in layer.EntityInstances)
            {
                var scenePath = GetEntityScenePath(entityInstance);

                var entity = scenePath is not null
                    ? InstanceEntityScene(entityInstance, scenePath)
                    : CreateEntityMarker(entityInstance);

                entitiesLayer.AddChild(entity);
            }
        }

        private Node2D InstanceEntityScene(LevelJson.EntityInstance entityJson, string scenePath)
        {
            var sceneInstance = GD.Load<PackedScene>(scenePath).Instance<Node2D>();

            foreach (Node child in sceneInstance.GetChildren())
            {
                child.Free();
            }

            sceneInstance.Position = entityJson.PxCoords;

            AssignLDtkFields(entityJson, sceneInstance);

            return sceneInstance;
        }

        private void AssignLDtkFields(LevelJson.EntityInstance entityJson, Node2D sceneInstance)
        {
            var entityType = sceneInstance.GetType();

            GD.Print(entityType);

            if (!_CachedLDtkFields.TryGetValue(entityType, out var targetFields))
            {
                targetFields = GetEntityTargetMembers(entityType)
                    .Select(member => new LDtkFieldInfo(
                        member.GetCustomAttribute<LDtkFieldAttribute>().FieldEditorName ?? member.Name,
                        member
                    ));

                _CachedLDtkFields.Add(entityType, targetFields);
            }

            foreach (var field in targetFields)
            {
                sceneInstance.Set(field.TargetMember.Name, entityJson.FieldInstances.FindField(field.EditorName, null).Value);
            }
        }

        private static IEnumerable<MemberInfo> GetEntityTargetMembers(Type entityType) => entityType.FindMembers(
            MemberTypes.Field | MemberTypes.Property,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy,
            (member, _) => member.IsDefined(typeof(LDtkFieldAttribute)),
            null
        );

        private string GetEntityScenePath(LevelJson.EntityInstance entityInstance)
        {
            return _EntityScenePath.Replace(EntityScenePathReplaceTarget, entityInstance.Identifier);
        }

        private static Position2D CreateEntityMarker(LevelJson.EntityInstance entityInstance) => new()
        {
            Name = entityInstance.Identifier,
            Position = entityInstance.PxCoords,
        };
    }
}

#endif
