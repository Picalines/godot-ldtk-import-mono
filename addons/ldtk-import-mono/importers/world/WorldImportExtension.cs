using LDtkImport.Json;

namespace LDtkImport.Importers
{
    public class WorldImportExtension : ImportPluginExtension
    {
        public WorldJson.Root WorldJson { get; init; }

        public virtual void OnWorldImported() { }
    }
}
