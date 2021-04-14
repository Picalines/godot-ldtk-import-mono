using System;
using LDtkImport.Json;
using Godot;

namespace LDtkImport.Importers
{
    public static class EntitiesLayerImporter
    {
        public static Node2D Import(LevelJson.LayerInstance layer, LevelImportExtension? extension)
        {
            var entitiesLayer = new Node2D
            {
                Name = layer.Identifier,
            };

            Func<LevelJson.EntityInstance, Node2D?> createEntityNode = extension is not null
                ? extension.CreateEntity
                : LevelImportExtension.CreateEntityMarker;

            foreach (var entityInstance in layer.EntityInstances)
            {
                Node2D? entity = createEntityNode(entityInstance);

                if (entity is null)
                {
                    GD.PushWarning($"Entity '{entityInstance.Identifier}' on layer '{layer.Identifier}' is ignored (level #{layer.LevelId})");
                    continue;
                }

                entitiesLayer.AddChild(entity);
            }

            return entitiesLayer;
        }
    }
}
