using Picalines.Godot.LDtkImport.Json;

namespace Picalines.Godot.LDtkImport.Importers
{
    internal sealed record LevelImportContext(
        string LDtkFilePath,
        LDtkImportSettings ImportSettings,
        WorldJson WorldJson,
        LevelJson LevelJson);
}
