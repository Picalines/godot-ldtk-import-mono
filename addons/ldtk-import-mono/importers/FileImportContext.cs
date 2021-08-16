#if TOOLS

using Godot.Collections;

namespace Picalines.Godot.LDtkImport.Importers
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

#endif
