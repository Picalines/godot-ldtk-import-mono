#if TOOLS

using LDtkImport.Json;
using Godot;

namespace LDtkImport.Importers
{
    public abstract class LevelImportExtension : SceneImportExtension<Node2D, LevelImportContext>
    {
        public virtual string? GetEntityScenePath(LevelJson.EntityInstance entity) => null;
    }
}

#endif
