#if TOOLS

using Godot;
using LDtkImport.Json;

namespace LDtkImport.Importers
{
    public abstract class WorldImportExtension : SceneImportExtension<Node2D, WorldImportContext>
    {
        public virtual void PrepareTileSet(TileSet tileSet, WorldJson.TileSetDefinition json) { }
    }
}

#endif
