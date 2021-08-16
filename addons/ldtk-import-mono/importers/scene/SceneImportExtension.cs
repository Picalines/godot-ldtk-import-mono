#if TOOLS

using Godot;

namespace Picalines.Godot.LDtkImport.Importers
{
    public abstract class SceneImportExtension<SceneBase, Context> : ImportPluginExtension
        where SceneBase : Node
        where Context : class
    {
        public Context SceneContext { get; init; }

        public virtual void OnSceneBuilt(SceneBase node) { }
    }
}

#endif
