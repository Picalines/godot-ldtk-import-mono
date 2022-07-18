#if TOOLS

using Godot;

namespace Picalines.Godot.LDtkImport.Importers
{
    internal static class BackgroundImporter
    {
        public static void Import(LevelImportContext context, Node levelNode)
        {
            Node bgParent = levelNode;
            bool seperateParent = false;

            if (context.ImportSettings.LevelSceneSettings?.BackgroundParentNodeName is string backgroundParentNodeName)
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

            var bgSprite = BaseSceneImporter.FindOrCreateBaseNode<Sprite>(bgParent);
            var bgColorRect = BaseSceneImporter.FindOrCreateBaseNode<ColorRect>(bgParent);

            if (context.LevelJson is { BgRelPath: string bgRelPath, BgPosition: { } bgPosition })
            {
                var bgTexture = GD.Load<Texture>($"{context.LDtkFilePath.GetBaseDir()}/{bgRelPath}");

                bgSprite.Name = $"{(seperateParent ? "" : "Background")}{nameof(Sprite)}";
                bgSprite.Texture = bgTexture;
                bgSprite.Centered = false;
                bgSprite.RegionEnabled = true;
                bgSprite.RegionRect = bgPosition.CropRect;
                bgSprite.Position = bgPosition.TopLeftPxCoords;
                bgSprite.Scale = bgPosition.Scale;

                if (bgSprite.Owner is null)
                {
                    bgParent.AddChild(bgSprite);
                    bgSprite.Owner = levelNode;
                }

                bgParent.MoveChild(bgSprite, 0);
            }
            else
            {
                bgSprite.Free();
            }

            if (context.ImportSettings.LevelSceneSettings?.IgnoreBackgroundColor is false or null)
            {
                bgColorRect.Name = $"{(seperateParent ? "" : "Background")}{nameof(ColorRect)}";
                bgColorRect.Color = context.LevelJson.BgColor;
                bgColorRect.RectSize = context.LevelJson.PxSize;

                if (bgColorRect.Owner is null)
                {
                    bgParent.AddChild(bgColorRect);
                    bgColorRect.Owner = levelNode;
                }

                bgParent.MoveChild(bgColorRect, 0);
            }
            else
            {
                bgColorRect.Free();
            }
        }
    }
}

#endif