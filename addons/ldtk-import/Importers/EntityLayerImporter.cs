#if TOOLS

using Godot;
using Picalines.Godot.LDtkImport.Json;

namespace Picalines.Godot.LDtkImport.Importers
{
    internal static class EntityLayerImporter
    {
        public static Node2D Import(LevelImportContext context, LevelJson.LayerInstance layerJson)
        {
            // TODO: actual import.

            return new Node2D() { Name = layerJson.Identifier };
        }
    }
}

#endif