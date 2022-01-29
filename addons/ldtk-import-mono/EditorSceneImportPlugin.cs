#if TOOLS

using System;
using Godot;
using GDArray = Godot.Collections.Array;
using GDDictionary = Godot.Collections.Dictionary;

namespace Picalines.Godot.LDtkImport
{
    public abstract class EditorSceneImportPlugin : EditorImportPlugin
    {
        public sealed override string GetResourceType() => nameof(PackedScene);
        public sealed override string GetSaveExtension() => "tscn";

        public override int GetPresetCount() => 1;
        public override string GetPresetName(int preset) => "default";

        public override bool GetOptionVisibility(string option, GDDictionary options) => true;

        public sealed override int Import(string sourceFile, string savePath, GDDictionary options, GDArray platformVariants, GDArray genFiles)
        {
            Node sceneNode;
            try
            {
                sceneNode = ImportScene(sourceFile, savePath, options, platformVariants, genFiles);
            }
            catch (Exception exception)
            {
                GD.PushError(exception.Message);
                return (int)Error.Failed;
            }

            return (int)SaveScene(savePath, sceneNode);
        }

        protected abstract Node ImportScene(string sourceFile, string savePath, GDDictionary options, GDArray platformVariants, GDArray genFiles);

        private Error SaveScene(string savePath, Node sceneNode)
        {
            foreach (Node child in sceneNode.GetChildren())
            {
                SetOwnerRecursive(child, sceneNode);
            }

            var packedScene = new PackedScene();
            packedScene.TakeOverPath(savePath);

            if (packedScene.Pack(sceneNode) != Error.Ok)
            {
                return Error.Failed;
            }

            return ResourceSaver.Save($"{savePath}.{GetSaveExtension()}", packedScene);
        }

        protected static void SetOwnerRecursive(Node node, Node owner)
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
