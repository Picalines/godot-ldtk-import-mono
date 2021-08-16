#if TOOLS

using System;
using Godot;
using Godot.Collections;

namespace Picalines.Godot.LDtkImport.Importers
{
    public abstract class SceneImport<SceneBase, Context, Extension> : ExtendableImportPlugin<Extension, LDtkImportExtensionAttribute>
        where SceneBase : Node
        where Context : class
        where Extension : SceneImportExtension<SceneBase, Context>
    {
        public sealed override string GetResourceType() => nameof(PackedScene);
        public sealed override string GetSaveExtension() => "tscn";

        public override bool GetOptionVisibility(string option, Dictionary options) => true;

        protected Context SceneContext { get; private set; }

        protected abstract Context GetContext();
        protected abstract SceneBase BuildScene();

        protected sealed override Error Import()
        {
            SceneContext = GetContext();

            if (UsedExtension is not null)
            {
                typeof(SceneImportExtension<SceneBase, Context>)
                    .GetProperty(nameof(SceneImportExtension<SceneBase, Context>.SceneContext))!
                    .SetValue(UsedExtension, SceneContext);
            }

            ulong buildedSceneId;

            try
            {
                SceneBase sceneNode = BuildScene();
                buildedSceneId = sceneNode.GetInstanceId();
                UsedExtension?.OnSceneBuilt(sceneNode);
            }
            catch (Exception error)
            {
                GD.PushError(error.Message);
                return Error.Bug;
            }

            SceneBase buildedScene = (GD.InstanceFromId(buildedSceneId) as SceneBase)!;

            foreach (Node child in buildedScene.GetChildren())
            {
                SetOwnerRecursive(child, buildedScene);
            }

            var packedScene = new PackedScene();
            packedScene.TakeOverPath(ImportContext.SavePath);

            if (packedScene.Pack(buildedScene) != Error.Ok)
            {
                return Error.Failed;
            }

            return ResourceSaver.Save($"{ImportContext.SavePath}.{GetSaveExtension()}", packedScene);
        }

        private static void SetOwnerRecursive(Node node, Node owner)
        {
            node.Owner = owner;

            foreach (Node child in node.GetChildren())
            {
                SetOwnerRecursive(child, owner);
            }
        }
    }
}

#endif
