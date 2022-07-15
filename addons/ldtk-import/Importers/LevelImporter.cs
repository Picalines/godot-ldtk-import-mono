#if TOOLS

using Godot;
using System.Collections.Generic;
using System.Linq;

namespace Picalines.Godot.LDtkImport.Importers
{
    internal static class LevelImporter
    {
        public static void Import(LevelImportContext context)
        {
            Node levelNode = BaseSceneImporter.ImportOrCreate<Node2D>(context.ImportSettings.LevelSceneSettings);

            levelNode.Name = context.LevelJson.Identifier;
            levelNode.AddToGroup(LDtkConstants.GroupNames.Levels, persistent: true);
            levelNode.SetMeta(LDtkConstants.MetaKeys.ImportSettingsFilePath, context.ImportSettings.FilePath);

            if (levelNode is Node2D levelNode2D)
            {
                levelNode2D.ZIndex = context.LevelJson.WorldDepth;
            }

            BackgroundImporter.Import(context, levelNode);

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

            foreach (var layerJson in context.LevelJson.LayerInstances!.Reverse())
            {
                var layerNode = layersParent.GetNodeOrNull(layerJson.Identifier);

                bool newChild = false;

                if (layerNode is null)
                {
                    layerNode = new Node2D() { Name = layerJson.Identifier };
                    newChild = true;
                }

                if (layerNode is CanvasItem canvasItem)
                {
                    canvasItem.Visible = layerJson.Visible;
                    canvasItem.Modulate = Colors.White with { a = layerJson.Opacity };
                }

                if (layerNode is Node2D layer2D)
                {
                    layer2D.Position = layerJson.PxTotalOffset;
                }

                switch (layerJson.Type)
                {
                    case Json.LayerType.Entities:
                    {
                        EntityLayerImporter.Import(context, layerJson, layerNode);
                    }
                    break;

                    default:
                    {
                        TileMapImporter.Import(context, layerJson, layerNode);
                    }
                    break;
                }

                if (newChild)
                {
                    layersParent.AddChild(layerNode);
                }

                layerNode.Owner = levelNode;
                foreach (Node child in layerNode.GetChildren())
                {
                    child.Owner = levelNode;
                }
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