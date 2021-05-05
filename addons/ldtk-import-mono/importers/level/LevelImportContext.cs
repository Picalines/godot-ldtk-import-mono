#if TOOLS

using LDtkImport.Json;

namespace LDtkImport.Importers
{
    public record LevelImportContext
    {
        public WorldJson WorldJson { get; init; }
        public LevelJson LevelJson { get; init; }
    }
}

#endif
