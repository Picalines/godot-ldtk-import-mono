#if TOOLS

using Godot;
using Picalines.Godot.LDtkImport.Importers;

namespace Picalines.Godot.LDtkImport
{
    [Tool]
    internal sealed class LDtkImportPlugin : EditorPlugin
    {
        private WorldImport? WorldImportPlugin;
        private LevelImport? LevelImportPlugin;

        public override void _EnterTree()
        {
            base._EnterTree();

            WorldImportPlugin = new WorldImport();
            LevelImportPlugin = new LevelImport();

            AddImportPlugin(WorldImportPlugin);
            AddImportPlugin(LevelImportPlugin);
        }

        public override void _ExitTree()
        {
            base._ExitTree();

            RemoveImportPlugin(WorldImportPlugin);
            RemoveImportPlugin(LevelImportPlugin);

            WorldImportPlugin = null;
            LevelImportPlugin = null;
        }
    }
}

#endif
