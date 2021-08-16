#if TOOLS

using Picalines.Godot.LDtkImport.Json;

namespace Picalines.Godot.LDtkImport.Importers
{
    public record LevelImportContext
    {
        public WorldJson WorldJson { get; init; }
        public LevelJson LevelJson { get; init; }
    }
}

#endif
