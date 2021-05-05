#if TOOLS

using System;

namespace LDtkImport.Importers
{
    public abstract class ImportPluginExtension
    {
        public FileImportContext ImportContext { get; init; }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class LDtkImportExtensionAttribute : Attribute { }
}

#endif
