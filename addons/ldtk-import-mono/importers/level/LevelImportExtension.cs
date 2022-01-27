#if TOOLS

using Picalines.Godot.LDtkImport.Json;
using Godot;

namespace Picalines.Godot.LDtkImport.Importers
{
    public abstract class LevelImportExtension : SceneImportExtension<Node2D, LevelImportContext>
    {
        private LevelImportExtension(FileImportContext importContext, LevelImportContext sceneContext) : base(importContext, sceneContext)
        {
        }

        public virtual bool UseYSortForEntityLayer(LevelJson.LayerInstance entityLayer) => false;

        public virtual string? GetEntityScenePath(LevelJson.EntityInstance entity) => null;
    }
}

#endif
