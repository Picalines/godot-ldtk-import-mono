using Godot.Collections;

namespace LDtkImport.Importers
{
    public record FileImportContext
    {
        public string SourceFile { get; init; }
        public string SavePath { get; init; }
        public Dictionary Options { get; init; }
        public Array PlatformVariants { get; init; }
        public Array GenFiles { get; init; }
    }
}
