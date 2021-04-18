using Godot;
using LDtkImport.Json;

namespace LDtkImport.Importers
{
    public abstract class WorldImportExtension : SceneImportExtension<Node2D, WorldImportContext>
    {
        public virtual Node2D PrepareLevel(Node2D levelNode, LevelJson.Root levelJson) => levelNode;
    }
}
