#if TOOLS

using Picalines.Godot.LDtkImport.Json;

namespace Picalines.Godot.LDtkImport.Importers
{
    public sealed record LevelImportContext(WorldJson WorldJson, LevelJson LevelJson);
}

#endif
