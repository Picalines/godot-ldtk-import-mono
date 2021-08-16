#if TOOLS

using Godot;
using Picalines.Godot.LDtkImport.Json;

namespace Picalines.Godot.LDtkImport.Importers
{
    public abstract class EntityImporterExtension
    {
        public LevelImportContext SceneContext { get; init; }

        public abstract void PrepareInstance(LevelJson.EntityInstance json, Node2D scene);
    }
}

#endif
