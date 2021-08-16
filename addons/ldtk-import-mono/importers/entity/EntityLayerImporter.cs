#if TOOLS

using Godot;
using System;
using LDtkImport.Json;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace LDtkImport.Importers
{
    public class EntityLayerImporter : IDisposable
    {
        public LevelImportContext SceneContext { get; init; }
        public LevelImportExtension? UsedExtension { get; init; }

        private readonly Dictionary<string, IReadOnlyList<EntityImporterExtension>> CachedEntityImporters = new();

        public void Dispose()
        {
            CachedEntityImporters.Clear();
        }

        public Node2D Import(LevelJson.LayerInstance layer)
        {
            var entitiesLayer = CreateEntityLayerNode(layer);

            AddEntities(layer, entitiesLayer);

            return entitiesLayer;
        }

        private Node2D CreateEntityLayerNode(LevelJson.LayerInstance layer)
        {
            var useYSort = UsedExtension?.UseYSortForEntityLayer(layer) ?? false;
            var entitiesLayer = useYSort ? new YSort() : new Node2D();

            entitiesLayer.Name = layer.Identifier;

            return entitiesLayer;
        }

        private void AddEntities(LevelJson.LayerInstance layer, Node2D entitiesLayer)
        {
            foreach (var entityInstance in layer.EntityInstances)
            {
                var scenePath = UsedExtension?.GetEntityScenePath(entityInstance);

                Node2D entity = scenePath is not null
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

            var importers = GetEntityImporters(entityJson);

            foreach (var importer in importers)
            {
                importer.PrepareInstance(entityJson, sceneInstance);
            }

            return sceneInstance;
        }

        private IReadOnlyList<EntityImporterExtension> GetEntityImporters(LevelJson.EntityInstance entityJson)
        {
            if (!CachedEntityImporters.TryGetValue(entityJson.Identifier, out var importers))
            {
                importers = Assembly.GetExecutingAssembly().GetTypes()
                    .Where(type => Attribute.IsDefined(type, typeof(LDtkEntityImporterAttribute)))
                    .Where(type => typeof(EntityImporterExtension).IsAssignableFrom(type))
                    .Where(type => !type.IsAbstract && !type.IsGenericType)
                    .Where(type => type.GetCustomAttribute<LDtkEntityImporterAttribute>().EntityIdentifier == entityJson.Identifier)
                    .Select(type => CreateEntityImporter(type))
                    .ToList();

                CachedEntityImporters.Add(entityJson.Identifier, importers);
            }

            return importers;
        }

        private EntityImporterExtension CreateEntityImporter(Type type)
        {
            var instance = (Activator.CreateInstance(type) as EntityImporterExtension)!;

            type.GetProperty(nameof(EntityImporterExtension.SceneContext)).SetValue(instance, SceneContext);

            return instance;
        }

        private static Position2D CreateEntityMarker(LevelJson.EntityInstance entityInstance) => new()
        {
            Name = entityInstance.Identifier,
            Position = entityInstance.PxCoords,
        };
    }
}

#endif
