using System;
using Godot;
using Godot.Collections;
using LDtkImport.Utils;

namespace LDtkImport.Importers
{
    public abstract class SceneImport<SceneBase, Context> : ExtendableImportPlugin<SceneImportExtension<SceneBase, Context>, LDtkImportExtensionAttribute>
        where SceneBase : Node
        where Context : class
    {
        public sealed override string GetResourceType() => nameof(PackedScene);
        public sealed override string GetSaveExtension() => "tscn";

        public override bool GetOptionVisibility(string option, Dictionary options) => true;

        protected Context SceneContext { get; private set; }

        protected abstract Context GetContext();
        protected abstract SceneBase BuildScene(Context sceneContext);

        protected override Error Import()
        {
            SceneContext = GetContext();

            if (UsedExtension is not null)
            {
                typeof(SceneImportExtension<SceneBase, Context>)
                    .GetProperty(nameof(SceneImportExtension<SceneBase, Context>.SceneContext))
                    .SetValue(UsedExtension, SceneContext);
            }

            SceneBase sceneNode;

            try
            {
                sceneNode = BuildScene(SceneContext);
                UsedExtension?.OnSceneBuilt(sceneNode);
            }
            catch (Exception error)
            {
                GD.PushError(error.Message);
                return Error.Bug;
            }

            foreach (Node child in sceneNode.GetChildren())
            {
                child.SetOwnerRecursive(sceneNode);
            }

            var packedScene = new PackedScene();
            packedScene.TakeOverPath(ImportContext.SavePath);

            if (packedScene.Pack(sceneNode) != Error.Ok)
            {
                return Error.Failed;
            }

            return ResourceSaver.Save($"{ImportContext.SavePath}.{GetSaveExtension()}", packedScene);
        }
    }
}
