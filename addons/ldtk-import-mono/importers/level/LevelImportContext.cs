using LDtkImport.Json;

namespace LDtkImport.Importers
{
    public record LevelImportContext : FileImportContext
    {
        public WorldJson.Root WorldJson { get; init; }
        public LevelJson.Root LevelJson { get; init; }

        public LevelImportContext(FileImportContext importContext)
        {
            SourceFile = importContext.SourceFile;
            SavePath = importContext.SavePath;
            Options = importContext.Options;
            PlatformVariants = importContext.PlatformVariants;
            GenFiles = importContext.GenFiles;
        }
    }
}
