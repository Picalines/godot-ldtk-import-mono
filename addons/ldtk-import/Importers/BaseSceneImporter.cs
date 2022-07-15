#if TOOLS

using Godot;
using Picalines.Godot.LDtkImport.Json;
using System.Collections.Generic;
using System.Linq;

namespace Picalines.Godot.LDtkImport.Importers
{
    internal static class BaseSceneImporter
    {
        private record BaseSceneInfo(List<NodePath> IgnoredNodePaths, List<NodePath> BaseNodePaths);

        private static readonly Dictionary<string, BaseSceneInfo> _BaseSceneInfoCache = new();

        public static void Initialize()
        {
            _BaseSceneInfoCache.Clear();
        }

        public static Node ImportOrCreate<TDefaultNode>(SceneImportSettings? sceneImportSettings)
            where TDefaultNode : Node, new()
        {
            Node sceneNode;

            if (sceneImportSettings?.BaseScenePath is string baseScenePath)
            {
                var basePackedScene = GD.Load<PackedScene>(baseScenePath);
                sceneNode = basePackedScene.Instance<Node>();

                sceneNode.SetMeta(LDtkConstants.MetaKeys.BaseScenePath, baseScenePath);

                if (!_BaseSceneInfoCache.TryGetValue(baseScenePath, out var baseSceneInfo))
                {
                    baseSceneInfo = new BaseSceneInfo(new(), new());

                    FillBaseSceneInfo(sceneImportSettings, sceneNode, sceneNode, baseSceneInfo);

                    _BaseSceneInfoCache.Add(baseScenePath, baseSceneInfo);
                }

                foreach (var nodePath in baseSceneInfo.IgnoredNodePaths)
                {
                    sceneNode.GetNode(nodePath).Free();
                }
            }
            else
            {
                sceneNode = new TDefaultNode();
            }

            return sceneNode;
        }

        public static TNode FindOrCreateBaseNode<TNode>(Node parent)
            where TNode : Node, new()
        {
            TNode? baseNode = null;

            var baseScenePath = parent.Owner.HasMeta(LDtkConstants.MetaKeys.BaseScenePath)
                ? parent.Owner.GetMeta(LDtkConstants.MetaKeys.BaseScenePath) as string
                : null;

            if (baseScenePath is not null && _BaseSceneInfoCache.TryGetValue(baseScenePath, out var baseSceneInfo))
            {
                baseNode = baseSceneInfo.BaseNodePaths
                    .Select(nodePath => parent.Owner.GetNode(nodePath))
                    .OfType<TNode>()
                    .FirstOrDefault(node => node.GetParent() == parent);
            }

            return baseNode ?? new TNode();
        }

        private static void FillBaseSceneInfo(SceneImportSettings sceneImportSettings, Node sceneNode, Node currentNode, BaseSceneInfo baseSceneInfo)
        {
            var ignoreMask = sceneImportSettings.BaseIgnoreMask;
            var baseNodeMask = (sceneImportSettings as LevelSceneImportSettings)?.BaseNodeMask;

            foreach (Node child in currentNode.GetChildren())
            {
                if (ignoreMask is not null && child.Name.Match(ignoreMask, caseSensitive: true))
                {
                    baseSceneInfo.IgnoredNodePaths.Add(sceneNode.GetPathTo(child));
                    continue;
                }

                if (baseNodeMask is not null && child.Name.Match(baseNodeMask, caseSensitive: true))
                {
                    baseSceneInfo.BaseNodePaths.Add(sceneNode.GetPathTo(child));
                }

                FillBaseSceneInfo(sceneImportSettings, sceneNode, child, baseSceneInfo);
            }
        }
    }
}

#endif