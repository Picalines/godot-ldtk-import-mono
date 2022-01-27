#if TOOLS

using Godot;
using Picalines.Godot.LDtkImport.Json;

namespace Picalines.Godot.LDtkImport.Importers
{
    public abstract class EntityImporterExtension
    {
        public readonly LevelImportContext SceneContext;

        public EntityImporterExtension(LevelImportContext sceneContext)
        {
            SceneContext = sceneContext;
        }

        public abstract void PrepareInstance(LevelJson.EntityInstance json, Node2D scene);
    }
}

#endif
