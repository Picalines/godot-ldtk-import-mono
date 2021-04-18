using LDtkImport.Json;

namespace LDtkImport.Importers
{
    public record WorldImportContext
    {
        public WorldJson.Root WorldJson { get; init; }
    }
}
