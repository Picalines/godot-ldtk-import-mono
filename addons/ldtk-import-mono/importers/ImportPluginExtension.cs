#if TOOLS

using System;

namespace Picalines.Godot.LDtkImport.Importers
{
    public abstract class ImportPluginExtension
    {
        public readonly FileImportContext ImportContext;

        public ImportPluginExtension(FileImportContext importContext)
        {
            ImportContext = importContext;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class LDtkImportExtensionAttribute : Attribute { }
}

#endif
