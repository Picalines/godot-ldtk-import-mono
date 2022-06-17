#if TOOLS

using Godot;
using Picalines.Godot.LDtkImport.Json;
using System.Collections.Generic;
using System.Linq;

namespace Picalines.Godot.LDtkImport.Importers
{
    internal static class LevelImporter
    {
        public static void Import(LevelImportContext context)
        {
            Node levelNode;

            if (context.ImportSettings.LevelSceneSettings?.BaseScenePath is string baseScenePath)
            {
                // NOTE: actual scene inheritance through code is impossible in Godot.
                // https://github.com/godotengine/godot-proposals/issues/3907
                var basePackedScene = GD.Load<PackedScene>(baseScenePath);
                levelNode = basePackedScene.Instance<Node>();
            }
            else
            {
                levelNode = new Node2D();
            }

            levelNode.Name = context.LevelJson.Identifier;
            levelNode.AddToGroup(LDtkConstants.GroupNames.Levels, persistent: true);
            levelNode.SetMeta(LDtkConstants.MetaKeys.ImportSettingsFilePath, context.ImportSettings.FilePath);

            if (levelNode is Node2D levelNode2D)
            {
                levelNode2D.ZIndex = context.LevelJson.WorldDepth;
            }

            AddBackground(context, levelNode);

            AddLayers(context, levelNode);

            var levelFields = context.LevelJson.FieldInstances
                .Select(field => new KeyValuePair<string, object>(field.Identifier, field.Value))
                .Append(new(LDtkConstants.SpecialFieldNames.Size, context.LevelJson.PxSize))
                .ToDictionary(pair => pair.Key, pair => pair.Value);

            LDtkFieldAssigner.Assign(levelNode, levelFields, new());

            SaveScene(context, levelNode);
        }

        private static void AddLayers(LevelImportContext context, Node levelNode)
        {
            Node layersParent = levelNode;

            if (context.ImportSettings.LevelSceneSettings?.LayersParentNodeName is { } layersParentNodeName)
            {
                layersParent = levelNode.GetNodeOrNull(layersParentNodeName)
                    ?? new Node2D() { Name = layersParentNodeName };

                if (layersParent.Owner is null)
                {
                    levelNode.AddChild(layersParent);
                    layersParent.Owner = levelNode;
                }
            }

            foreach (var layer in context.LevelJson.LayerInstances!.Reverse())
            {
                var layerNode = layer.Type switch
                {
                    LayerType.Entities => EntityLayerImporter.Import(context, layer),
                    _ => TileMapImporter.Import(context, layer),
                };

                layersParent.AddChild(layerNode);

                layerNode.Owner = levelNode;
                foreach (Node child in layerNode.GetChildren())
                {
                    child.Owner = levelNode;
                }
            }
        }

        private static void AddBackground(LevelImportContext context, Node levelNode)
        {
            Node bgParent = levelNode;
            bool seperateParent = false;

            if (context.ImportSettings.LevelSceneSettings?.BackgroundParentNodeName is { } backgroundParentNodeName)
            {
                seperateParent = true;

                bgParent = levelNode.GetNodeOrNull(backgroundParentNodeName)
                    ?? new Node2D() { Name = backgroundParentNodeName };

                if (bgParent.Owner is null)
                {
                    levelNode.AddChild(bgParent);
                    levelNode.MoveChild(bgParent, 0);
                    bgParent.Owner = levelNode;
                }
            }

            bool hasBgColor = false;

            if (context.ImportSettings.LevelSceneSettings?.IgnoreBackgroundColor is false)
            {
                hasBgColor = true;
                var colorRect = new ColorRect()
                {
                    Name = $"{(seperateParent ? "Background" : "")}{nameof(ColorRect)}",
                    Color = context.LevelJson.BgColor,
                    RectSize = context.LevelJson.PxSize
                };

                bgParent.AddChild(colorRect);
                bgParent.MoveChild(colorRect, 0);
                colorRect.Owner = levelNode;
            }

            if (context.LevelJson is { BgRelPath: { } bgRelPath, BgPosition: { } bgPosition })
            {
                var bgTexture = GD.Load<Texture>($"{context.LDtkFilePath.GetBaseDir()}/{bgRelPath}");

                var bgSprite = new Sprite()
                {
                    Name = $"{(seperateParent ? "Background" : "")}{nameof(Sprite)}",
                    Texture = bgTexture,
                    Centered = false,
                    RegionEnabled = true,
                    RegionRect = bgPosition.CropRect,
                    Position = bgPosition.TopLeftPxCoords,
                    Scale = bgPosition.Scale
                };

                bgParent.AddChild(bgSprite);
                bgParent.MoveChild(bgSprite, hasBgColor ? 1 : 0);
                bgSprite.Owner = levelNode;
            }
        }

        private static void SaveScene(LevelImportContext context, Node levelNode)
        {
            var savePath = $"{context.ImportSettings.OutputDirectory}/{context.LevelJson.Identifier}.tscn";

            var packedLevelScene = new PackedScene();
            packedLevelScene.Pack(levelNode);

            if (ResourceSaver.Save(savePath, packedLevelScene) is not Error.Ok)
            {
                throw new LDtkImportException(LDtkImportMessage.FailedToImportLevel(context.LDtkFilePath, context.LevelJson.Identifier));
            }
        }
    }
}

#endif