#if TOOLS

using Picalines.Godot.LDtkImport.Json;

namespace Picalines.Godot.LDtkImport.Importers
{
    internal record LevelImportContext(
        string LDtkFilePath,
        LDtkImportSettings ImportSettings,
        WorldJson WorldJson,
        LevelJson LevelJson);
}

#endif