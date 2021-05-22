#if TOOLS

using Godot;
using LDtkImport.Json;

namespace LDtkImport.Importers
{
    public abstract class EntityImporterExtension
    {
        public LevelImportContext SceneContext { get; init; }

        public abstract void PrepareInstance(LevelJson.EntityInstance json, Node2D scene);
    }
}

#endif
