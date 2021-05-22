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
            var entitiesLayer = new Node2D
            {
                Name = layer.Identifier,
            };

            foreach (var entityInstance in layer.EntityInstances)
            {
                var scenePath = UsedExtension?.GetEntityScenePath(entityInstance);

                Node2D entity = scenePath is not null
                    ? InstanceEntityScene(entityInstance, scenePath)
                    : CreateEntityMarker(entityInstance);

                entitiesLayer.AddChild(entity);
            }

            return entitiesLayer;
        }

        private Node2D InstanceEntityScene(LevelJson.EntityInstance entityJson, string scenePath)
        {
            var sceneInstance = GD.Load<PackedScene>(scenePath).Instance<Node2D>();

            foreach (Node child in sceneInstance.GetChildren())
            {
                child.Free();
            }

            sceneInstance.Position = entityJson.PxCoords;

            if (!CachedEntityImporters.TryGetValue(entityJson.Identifier, out var importers))
            {
                importers = GetEntityImporters(entityJson.Identifier);
                CachedEntityImporters.Add(entityJson.Identifier, importers);
            }

            foreach (var importer in importers)
            {
                importer.PrepareInstance(entityJson, sceneInstance);
            }

            return sceneInstance;
        }

        private IReadOnlyList<EntityImporterExtension> GetEntityImporters(string entityIdentifier) => GetEntityImporters()
            .Where(type => type.GetCustomAttribute<LDtkEntityImporterAttribute>().EntityIdentifier == entityIdentifier)
            .Select(type => CreateEntityImporter(type))
            .ToList();

        private static IEnumerable<Type> GetEntityImporters() => Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(type => Attribute.IsDefined(type, typeof(LDtkEntityImporterAttribute)))
            .Where(type => typeof(EntityImporterExtension).IsAssignableFrom(type))
            .Where(type => !type.IsAbstract && !type.IsGenericType);

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
