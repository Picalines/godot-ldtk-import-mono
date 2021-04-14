using LDtkImport.Json;
using Godot;

namespace LDtkImport.Importers
{
    public abstract class LevelImportExtension : SceneImportExtension<Node2D, LevelImportContext>
    {
        public static Position2D CreateEntityMarker(LevelJson.EntityInstance entityJson) => new()
        {
            Name = entityJson.Identifier,
            Position = entityJson.PxCoords,
        };

        public virtual Node2D? CreateEntity(LevelJson.EntityInstance entityJson) => CreateEntityMarker(entityJson);
    }
}
