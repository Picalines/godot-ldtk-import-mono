#if TOOLS

using Godot;
using Picalines.Godot.LDtkImport.Importers;

namespace Picalines.Godot.LDtkImport
{
    [Tool]
    internal sealed class LDtkImportPlugin : EditorPlugin
    {
        private WorldImportPlugin? WorldImportPlugin;
        private LevelImportPlugin? LevelImportPlugin;

        public override void _EnterTree()
        {
            base._EnterTree();

            WorldImportPlugin = new WorldImportPlugin();
            LevelImportPlugin = new LevelImportPlugin();

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
