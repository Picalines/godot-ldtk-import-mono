#if TOOLS

using System;

namespace LDtkImport.Importers
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class LDtkEntityImporterAttribute : Attribute
    {
        public string EntityIdentifier { get; }

        public LDtkEntityImporterAttribute(string entityIdentifier)
        {
            EntityIdentifier = entityIdentifier;
        }
    }
}

#endif
