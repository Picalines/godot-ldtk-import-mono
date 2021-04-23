using Godot;
using LDtkImport.Json;

namespace LDtkImport.Importers
{
    public abstract class WorldImportExtension : SceneImportExtension<Node2D, WorldImportContext>
    {
        public virtual void PrepareTileSet(TileSet tileSet, WorldJson.TileSetDef json) { }

        public virtual void PrepareLevel(Node2D levelNode, LevelJson.Root json) { }
    }
}
