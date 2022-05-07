using Picalines.Godot.LDtkImport.Json;

namespace Picalines.Godot.LDtkImport.Importers
{
    internal record LevelImportContext(
        string LDtkFilePath,
        LDtkImportSettings ImportSettings,
        WorldJson WorldJson,
        LevelJson LevelJson);

    internal record LayerImportContext(
        string LDtkFilePath,
        LDtkImportSettings ImportSettings,
        WorldJson WorldJson,
        LevelJson LevelJson,
        LevelJson.LayerInstance LayerJson);
}
