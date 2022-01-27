#if TOOLS

using Godot;
using Picalines.Godot.LDtkImport.Json;

namespace Picalines.Godot.LDtkImport.Importers
{
    public abstract class WorldImportExtension : SceneImportExtension<Node2D, WorldImportContext>
    {
        public WorldImportExtension(FileImportContext importContext, WorldImportContext sceneContext) : base(importContext, sceneContext)
        {
        }

        public virtual void PrepareTileSet(TileSet tileSet, WorldJson.TileSetDefinition json) { }
    }
}

#endif
