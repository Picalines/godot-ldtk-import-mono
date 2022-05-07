#if TOOLS

using System.Collections.Generic;

namespace Picalines.Godot.LDtkImport.Importers;

internal sealed record LDtkImportOptions(
    LDtkWorldImportOptions? WorldImportOptions,
    LDtkLevelImportOptions LevelImportOptions,
    IReadOnlyDictionary<string, string> EntityScenePaths,
    IReadOnlyDictionary<string, string> TileScenePaths
);

internal sealed record LDtkWorldImportOptions(
    string? BaseScenePath,
    string? LevelsParentName
);

internal sealed record LDtkLevelImportOptions(
    string? BaseScenePath,
    string? LayersParentName
);

#endif
