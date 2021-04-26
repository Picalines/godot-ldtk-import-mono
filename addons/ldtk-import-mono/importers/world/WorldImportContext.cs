using LDtkImport.Json;

namespace LDtkImport.Importers
{
    public record WorldImportContext
    {
        public WorldJson WorldJson { get; init; }
    }
}
