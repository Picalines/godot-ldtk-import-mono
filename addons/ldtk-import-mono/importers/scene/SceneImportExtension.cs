#if TOOLS

using Godot;

namespace Picalines.Godot.LDtkImport.Importers
{
    public abstract class SceneImportExtension<SceneBase, Context> : ImportPluginExtension
        where SceneBase : Node
        where Context : class
    {
        public readonly Context SceneContext;

        public SceneImportExtension(FileImportContext importContext, Context sceneContext) : base(importContext)
        {
            SceneContext = sceneContext;
        }

        public virtual void OnSceneBuilt(SceneBase node) { }
    }
}

#endif
