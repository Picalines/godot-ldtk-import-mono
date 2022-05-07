#if TOOLS

using Godot;

namespace Picalines.Godot.LDtkImport.Importers
{
    internal static class EntityLayerImporter
    {
        public static Node2D Import(LayerImportContext context)
        {
            // TODO: actual import.

            return new Node2D() { Name = context.LayerJson.Identifier };
        }
    }
}

#endif