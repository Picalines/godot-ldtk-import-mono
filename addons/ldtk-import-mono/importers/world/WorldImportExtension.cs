#if TOOLS

using Godot;
using Picalines.Godot.LDtkImport.Json;

namespace Picalines.Godot.LDtkImport.Importers
{
    public abstract class WorldImportExtension : SceneImportExtension<Node2D, WorldImportContext>
    {
        public virtual void PrepareTileSet(TileSet tileSet, WorldJson.TileSetDefinition json) { }
    }
}

#endif
