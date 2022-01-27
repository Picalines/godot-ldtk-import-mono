#if TOOLS

using Godot.Collections;

namespace Picalines.Godot.LDtkImport.Importers
{
    public sealed record FileImportContext(
        string SourceFile,
        string SavePath,
        Dictionary Options,
        Array PlatformVariants,
        Array GenFiles
    );
}

#endif
