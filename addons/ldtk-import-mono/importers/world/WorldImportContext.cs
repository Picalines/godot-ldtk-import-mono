#if TOOLS

using Picalines.Godot.LDtkImport.Json;

namespace Picalines.Godot.LDtkImport.Importers
{
    public record WorldImportContext
    {
        public WorldJson WorldJson { get; init; }
    }
}

#endif
